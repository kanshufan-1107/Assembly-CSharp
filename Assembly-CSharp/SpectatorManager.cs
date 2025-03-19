using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.GameService.Protocol.V2.Client;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Time;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using PegasusGame;
using PegasusShared;
using SpectatorProto;
using UnityEngine;

public class SpectatorManager
{
	public delegate void InviteReceivedHandler(OnlineEventType evt, BnetPlayer inviter);

	public delegate void InviteSentHandler(OnlineEventType evt, BnetPlayer invitee);

	public delegate void SpectatorToMyGameHandler(OnlineEventType evt, BnetPlayer spectator);

	public delegate void SpectatorModeChangedHandler(OnlineEventType evt, BnetPlayer spectatee);

	private struct ReceivedInvite
	{
		public float m_timestamp;

		public JoinInfo m_joinInfo;
	}

	private class IntendedSpectateeParty
	{
		public BnetGameAccountId SpectateeId;

		public BnetPartyId PartyId;

		public IntendedSpectateeParty(BnetGameAccountId spectateeId, BnetPartyId partyId)
		{
			SpectateeId = spectateeId;
			PartyId = partyId;
		}
	}

	private class PendingSpectatePlayer
	{
		public BnetGameAccountId SpectateeId;

		public JoinInfo JoinInfo;

		public PendingSpectatePlayer(BnetGameAccountId spectateeId, JoinInfo joinInfo)
		{
			SpectateeId = spectateeId;
			JoinInfo = joinInfo;
		}
	}

	private static readonly PlatformDependentValue<float> WAITING_FOR_NEXT_GAME_AUTO_LEAVE_SECONDS = new PlatformDependentValue<float>(PlatformCategory.OS)
	{
		iOS = 300f,
		Android = 300f,
		PC = -1f,
		Mac = -1f
	};

	private static readonly PlatformDependentValue<bool> DISABLE_MENU_BUTTON_WHILE_WAITING = new PlatformDependentValue<bool>(PlatformCategory.OS)
	{
		iOS = true,
		Android = true,
		PC = false,
		Mac = false
	};

	private static SpectatorManager s_instance = null;

	private bool m_initialized;

	private BnetGameAccountId m_spectateeFriendlySide;

	private BnetGameAccountId m_spectateeOpposingSide;

	private BnetPartyId m_spectatorPartyIdMain;

	private BnetPartyId m_spectatorPartyIdOpposingSide;

	private Map<BnetPartyId, BnetGameAccountId> m_knownPartyCreatorIds = new Map<BnetPartyId, BnetGameAccountId>();

	private IntendedSpectateeParty m_requestedInvite;

	private AlertPopup m_waitingForNextGameDialog;

	private HashSet<BnetPartyId> m_leavePartyIdsRequested;

	private PendingSpectatePlayer m_pendingSpectatePlayerAfterLeave;

	private HashSet<BnetGameAccountId> m_userInitiatedOutgoingInvites;

	private HashSet<BnetGameAccountId> m_kickedPlayers;

	private Map<BnetGameAccountId, uint> m_kickedFromSpectatingList;

	private int? m_expectedDisconnectReason;

	private bool m_isExpectingArriveInGameplayAsSpectator;

	private bool m_isShowingRemovedAsSpectatorPopup;

	private HashSet<BnetGameAccountId> m_gameServerKnownSpectators = new HashSet<BnetGameAccountId>();

	private Map<BnetGameAccountId, ReceivedInvite> m_receivedSpectateMeInvites = new Map<BnetGameAccountId, ReceivedInvite>();

	private Map<BnetGameAccountId, float> m_sentSpectateMeInvites = new Map<BnetGameAccountId, float>();

	public bool IsSpectatingOrWatching
	{
		get
		{
			if (GameMgr.Get() != null && GameMgr.Get().IsSpectator())
			{
				return true;
			}
			if (IsInSpectatorMode())
			{
				return true;
			}
			return false;
		}
	}

	private static bool IsGameOver
	{
		get
		{
			if (GameState.Get() == null)
			{
				return false;
			}
			if (GameState.Get().IsGameOverNowOrPending())
			{
				return true;
			}
			return false;
		}
	}

	public event InviteReceivedHandler OnInviteReceived;

	public event InviteSentHandler OnInviteSent;

	public event Action OnSpectateRejected;

	public event SpectatorToMyGameHandler OnSpectatorToMyGame;

	public event SpectatorModeChangedHandler OnSpectatorModeChanged;

	public static SpectatorManager Get()
	{
		if (s_instance == null && SceneMgr.Get() != null)
		{
			CreateInstance();
		}
		return s_instance;
	}

	public static bool InstanceExists()
	{
		return s_instance != null;
	}

	public static JoinInfo GetSpectatorJoinInfo(BnetGameAccount gameAccount)
	{
		if (gameAccount == null)
		{
			return null;
		}
		byte[] blob = gameAccount.GetGameFieldBytes(21u);
		if (blob != null && blob.Length != 0)
		{
			return ProtobufUtil.ParseFrom<JoinInfo>(blob);
		}
		JoinInfo partySpectatorInfo = PartyManager.Get().GetSpectatorJoinInfoForPlayer(gameAccount.GetId());
		if (partySpectatorInfo != null)
		{
			return partySpectatorInfo;
		}
		return null;
	}

	public static int GetSpectatorGameHandleFromPlayer(BnetPlayer player)
	{
		return GetSpectatorJoinInfo(player.GetHearthstoneGameAccount())?.GameHandle ?? (-1);
	}

	public static bool IsSpectatorSlotAvailable(JoinInfo info)
	{
		if (info == null)
		{
			return false;
		}
		if (!info.HasPartyId)
		{
			if (!info.HasServerIpAddress || !info.HasSecretKey)
			{
				return false;
			}
			if (string.IsNullOrEmpty(info.SecretKey))
			{
				return false;
			}
		}
		if (info.HasIsJoinable && !info.IsJoinable)
		{
			return false;
		}
		if (info.HasMaxNumSpectators && info.HasCurrentNumSpectators && info.CurrentNumSpectators >= info.MaxNumSpectators)
		{
			return false;
		}
		return true;
	}

	public void InitializeConnectedToBnet()
	{
		if (!m_initialized)
		{
			m_initialized = true;
			PartyInfo[] joinedParties = BnetParty.GetJoinedParties();
			foreach (PartyInfo party in joinedParties)
			{
				BnetParty_OnJoined(OnlineEventType.ADDED, party, null);
			}
			PartyInvite[] receivedInvites = BnetParty.GetReceivedInvites();
			foreach (PartyInvite invite in receivedInvites)
			{
				BnetParty_OnReceivedInvite(OnlineEventType.ADDED, new PartyInfo(invite.PartyId, invite.PartyType), invite.InviteId, invite.InviterId, invite.InviterName, invite.InviteeId, null);
			}
		}
	}

	private bool IsInSpectableContextWithPlayer(BnetGameAccountId gameAccountId)
	{
		BnetPlayer player = BnetPresenceMgr.Get().GetPlayer(gameAccountId);
		return IsInSpectableContextWithPlayer(player);
	}

	private bool IsInSpectableContextWithPlayer(BnetPlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (BnetFriendMgr.Get().IsFriend(player))
		{
			return true;
		}
		if (PartyManager.Get().IsPlayerInCurrentPartyOrPending(player.GetBestGameAccountId()))
		{
			return true;
		}
		return false;
	}

	public bool CanSpectate(BnetPlayer player)
	{
		if (player == null)
		{
			return false;
		}
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (myself == null)
		{
			return false;
		}
		if (player == myself)
		{
			return false;
		}
		if (!IsInSpectableContextWithPlayer(player))
		{
			return false;
		}
		BnetGameAccount gameAccount = player.GetHearthstoneGameAccount();
		BnetGameAccount myHsAccount = myself.GetHearthstoneGameAccount();
		if (gameAccount == null || myHsAccount == null || !gameAccount.IsOnline() || !myHsAccount.IsOnline())
		{
			return false;
		}
		if (HearthstoneApplication.IsPublic() && (string.Compare(gameAccount.GetClientVersion(), myHsAccount.GetClientVersion()) != 0 || string.Compare(gameAccount.GetClientEnv(), myHsAccount.GetClientEnv()) != 0))
		{
			return false;
		}
		BnetGameAccountId gameAccountId = player.GetHearthstoneGameAccountId();
		JoinInfo joinInfo = GetSpectatorJoinInfo(gameAccount);
		return CanSpectate(gameAccountId, joinInfo);
	}

	private bool CanSpectateMultiplePlayersSimultaneously(GameType gameType)
	{
		if (GameUtils.IsMercenariesGameType(gameType))
		{
			return false;
		}
		return true;
	}

	public bool CanSpectate(BnetGameAccountId gameAccountId, JoinInfo joinInfo)
	{
		if (IsSpectatingPlayer(gameAccountId))
		{
			return false;
		}
		if (m_spectateeOpposingSide != null)
		{
			return false;
		}
		if (HasPreviouslyKickedMeFromGame(gameAccountId, joinInfo?.GameHandle ?? (-1)) && !HasInvitedMeToSpectate(gameAccountId))
		{
			return false;
		}
		if (GameMgr.Get().IsFindingGame())
		{
			return false;
		}
		if (GameMgr.Get().IsNextSpectator())
		{
			return false;
		}
		if (FriendChallengeMgr.Get().HasChallenge())
		{
			return false;
		}
		if (!IsSpectatorSlotAvailable(joinInfo) && !HasInvitedMeToSpectate(gameAccountId))
		{
			return false;
		}
		if (GameMgr.Get().IsSpectator())
		{
			if (!IsPlayerInGame(gameAccountId))
			{
				return false;
			}
			if (joinInfo != null && !CanSpectateMultiplePlayersSimultaneously(joinInfo.GameType))
			{
				return false;
			}
		}
		else if (SceneMgr.Get().IsInGame())
		{
			return false;
		}
		if (!GameUtils.IsAnyTutorialComplete())
		{
			return false;
		}
		if (joinInfo != null && PartyManager.Get() != null && !PartyManager.Get().HasGameDataForGameType(joinInfo.GameType))
		{
			return false;
		}
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.LOGIN)
		{
			return false;
		}
		if (BnetPresenceMgr.Get().GetMyPlayer().IsAppearingOffline())
		{
			return false;
		}
		if (PartyManager.Get().IsInParty() && !PartyManager.Get().IsPlayerInCurrentPartyOrPending(gameAccountId))
		{
			return false;
		}
		return true;
	}

	public bool IsInSpectatorMode()
	{
		if (m_spectateeFriendlySide == null)
		{
			return false;
		}
		if (m_spectatorPartyIdMain == null)
		{
			return false;
		}
		if (!IsStillInParty(m_spectatorPartyIdMain))
		{
			return false;
		}
		if (!m_initialized)
		{
			return false;
		}
		if (GetPartyCreator(m_spectatorPartyIdMain) == null)
		{
			return false;
		}
		if (ShouldBePartyLeader(m_spectatorPartyIdMain))
		{
			return false;
		}
		return true;
	}

	public bool IsSpectatingPlayer(BnetGameAccountId gameAccountId)
	{
		if (m_spectateeFriendlySide != null && m_spectateeFriendlySide == gameAccountId)
		{
			return true;
		}
		if (m_spectateeOpposingSide != null && m_spectateeOpposingSide == gameAccountId)
		{
			return true;
		}
		return false;
	}

	public bool IsSpectatingMe(BnetGameAccountId gameAccountId)
	{
		if (IsInSpectatorMode())
		{
			return false;
		}
		if (m_gameServerKnownSpectators.Contains(gameAccountId))
		{
			return true;
		}
		if (gameAccountId != BnetPresenceMgr.Get().GetMyGameAccountId() && BnetParty.IsMember(m_spectatorPartyIdMain, gameAccountId))
		{
			return true;
		}
		return false;
	}

	public int GetCountSpectatingMe()
	{
		if (m_spectatorPartyIdMain != null && !ShouldBePartyLeader(m_spectatorPartyIdMain))
		{
			return 0;
		}
		int gameServerSpectators = m_gameServerKnownSpectators.Count;
		return Mathf.Max(BnetParty.CountMembers(m_spectatorPartyIdMain) - 1, gameServerSpectators);
	}

	public bool IsBeingSpectated()
	{
		return GetCountSpectatingMe() > 0;
	}

	public BnetGameAccountId[] GetSpectatorPartyMembers(bool friendlySide = true, bool includeSelf = false)
	{
		List<BnetGameAccountId> list = new List<BnetGameAccountId>(m_gameServerKnownSpectators);
		BnetParty.PartyMember[] members = BnetParty.GetMembers(friendlySide ? m_spectatorPartyIdMain : m_spectatorPartyIdOpposingSide);
		BnetGameAccountId myself = BnetPresenceMgr.Get().GetMyGameAccountId();
		BnetParty.PartyMember[] array = members;
		foreach (BnetParty.PartyMember member in array)
		{
			if ((includeSelf || member.GameAccountId != myself) && !list.Contains(member.GameAccountId))
			{
				list.Add(member.GameAccountId);
			}
		}
		return list.ToArray();
	}

	public bool IsInSpectatableGame()
	{
		if (!SceneMgr.Get().IsInGame())
		{
			return false;
		}
		if (GameMgr.Get().IsSpectator())
		{
			return false;
		}
		if (IsGameOver)
		{
			return false;
		}
		return true;
	}

	private bool IsInSpectatableScene(bool alsoCheckRequestedScene)
	{
		if (SceneMgr.Get().IsInGame())
		{
			return true;
		}
		if (IsSpectatableScene(SceneMgr.Get().GetMode()))
		{
			return true;
		}
		if (alsoCheckRequestedScene && IsSpectatableScene(SceneMgr.Get().GetNextMode()))
		{
			return true;
		}
		return false;
	}

	private static bool IsSpectatableScene(SceneMgr.Mode sceneMode)
	{
		if (sceneMode == SceneMgr.Mode.GAMEPLAY)
		{
			return true;
		}
		return false;
	}

	public bool CanAddSpectators()
	{
		if (GameMgr.Get() != null && GameMgr.Get().IsSpectator())
		{
			return false;
		}
		if (m_spectateeFriendlySide != null || m_spectateeOpposingSide != null)
		{
			return false;
		}
		int countCurrentSpectators = GetCountSpectatingMe();
		if (!IsInSpectatableGame())
		{
			if (m_spectatorPartyIdMain == null)
			{
				return false;
			}
			if (countCurrentSpectators <= 0)
			{
				return false;
			}
		}
		if (countCurrentSpectators >= 10)
		{
			return false;
		}
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (myself != null && myself.IsAppearingOffline())
		{
			return false;
		}
		return true;
	}

	public bool CanInviteToSpectateMyGame(BnetGameAccountId gameAccountId)
	{
		if (!CanAddSpectators())
		{
			return false;
		}
		BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
		if (gameAccountId == myGameAccountId)
		{
			return false;
		}
		if (!IsInSpectableContextWithPlayer(gameAccountId))
		{
			return false;
		}
		if (IsSpectatingMe(gameAccountId))
		{
			return false;
		}
		if (IsInvitedToSpectateMyGame(gameAccountId))
		{
			return false;
		}
		if (PartyManager.Get().IsPlayerInAnyParty(gameAccountId))
		{
			return false;
		}
		BnetGameAccount gameAccount = BnetPresenceMgr.Get().GetGameAccount(gameAccountId);
		if (gameAccount == null || !gameAccount.IsOnline())
		{
			return false;
		}
		if (!gameAccount.CanBeInvitedToGame())
		{
			if (!IsPlayerSpectatingMyGamesOpposingSide(gameAccountId))
			{
				return false;
			}
			JoinInfo joinInfo = GetSpectatorJoinInfo(gameAccount);
			if (joinInfo != null && CanSpectateMultiplePlayersSimultaneously(joinInfo.GameType))
			{
				return false;
			}
		}
		BnetGameAccount myHsAccount = BnetPresenceMgr.Get().GetMyPlayer().GetHearthstoneGameAccount();
		if (string.Compare(gameAccount.GetClientVersion(), myHsAccount.GetClientVersion()) != 0)
		{
			return false;
		}
		if (HearthstoneApplication.IsPublic() && string.Compare(gameAccount.GetClientEnv(), myHsAccount.GetClientEnv()) != 0)
		{
			return false;
		}
		if (!SceneMgr.Get().IsInGame())
		{
			return false;
		}
		return true;
	}

	public bool IsPlayerSpectatingMyGamesOpposingSide(BnetGameAccountId gameAccountId)
	{
		BnetGameAccount gameAccount = BnetPresenceMgr.Get().GetGameAccount(gameAccountId);
		if (gameAccount == null)
		{
			return false;
		}
		BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
		bool isSpectatableAndSpectatingOtherPlayer = false;
		if (IsInSpectableContextWithPlayer(gameAccountId))
		{
			JoinInfo joinInfo = GetSpectatorJoinInfo(gameAccount);
			Map<int, Player>.ValueCollection playersInMyGame = ((GameState.Get() == null) ? null : GameState.Get().GetPlayerMap().Values);
			if (joinInfo != null && joinInfo.SpectatedPlayers.Count > 0 && playersInMyGame != null && playersInMyGame.Count > 0)
			{
				for (int i = 0; i < joinInfo.SpectatedPlayers.Count; i++)
				{
					BnetGameAccountId spectatedPlayerId = BnetGameAccountId.CreateFromNet(joinInfo.SpectatedPlayers[i]);
					if (spectatedPlayerId != myGameAccountId && playersInMyGame.Any((Player p) => p.GetGameAccountId() == spectatedPlayerId))
					{
						isSpectatableAndSpectatingOtherPlayer = true;
						break;
					}
				}
			}
		}
		return isSpectatableAndSpectatingOtherPlayer;
	}

	public bool IsInvitedToSpectateMyGame(BnetGameAccountId gameAccountId)
	{
		if (BnetParty.GetSentInvites(m_spectatorPartyIdMain).FirstOrDefault((PartyInvite i) => i.InviteeId == gameAccountId) != null)
		{
			return true;
		}
		return false;
	}

	public bool CanKickSpectator(BnetGameAccountId gameAccountId)
	{
		if (!IsSpectatingMe(gameAccountId))
		{
			return false;
		}
		return true;
	}

	public bool HasInvitedMeToSpectate(BnetGameAccountId gameAccountId)
	{
		if (BnetParty.GetReceivedInviteFrom(gameAccountId, PartyType.SPECTATOR_PARTY) != null)
		{
			return true;
		}
		return false;
	}

	public bool HasAnyReceivedInvites()
	{
		return (from i in BnetParty.GetReceivedInvites()
			where i.PartyType == PartyType.SPECTATOR_PARTY
			select i).ToArray().Length != 0;
	}

	public bool MyGameHasSpectators()
	{
		if (!SceneMgr.Get().IsInGame())
		{
			return false;
		}
		return m_gameServerKnownSpectators.Count > 0;
	}

	public BnetGameAccountId GetSpectateeFriendlySide()
	{
		return m_spectateeFriendlySide;
	}

	public bool IsSpectatingOpposingSide()
	{
		return m_spectateeOpposingSide != null;
	}

	public bool HasPreviouslyKickedMeFromGame(BnetGameAccountId playerId, int currentGameHandle)
	{
		if (m_kickedFromSpectatingList == null)
		{
			return false;
		}
		uint kickedGameHandle = 0u;
		if (m_kickedFromSpectatingList.TryGetValue(playerId, out kickedGameHandle))
		{
			if (kickedGameHandle == currentGameHandle)
			{
				return true;
			}
			m_kickedFromSpectatingList.Remove(playerId);
			if (m_kickedFromSpectatingList.Count == 0)
			{
				m_kickedFromSpectatingList = null;
			}
		}
		return false;
	}

	public void SpectatePlayer(BnetPlayer player)
	{
		if (CanSpectate(player))
		{
			BnetGameAccountId gameAccountId = player.GetHearthstoneGameAccountId();
			JoinInfo joinInfo = GetSpectatorJoinInfo(player.GetHearthstoneGameAccount());
			SpectatePlayer(gameAccountId, joinInfo);
		}
	}

	public void SpectatePlayer(BnetGameAccountId gameAccountId, JoinInfo joinInfo)
	{
		if (m_pendingSpectatePlayerAfterLeave != null)
		{
			return;
		}
		PartyInvite receivedInvite = BnetParty.GetReceivedInviteFrom(gameAccountId, PartyType.SPECTATOR_PARTY);
		if (receivedInvite != null)
		{
			if (m_spectateeFriendlySide == null || (m_spectateeOpposingSide == null && IsPlayerInGame(gameAccountId)))
			{
				CloseWaitingForNextGameDialog();
				if (m_spectateeFriendlySide != null && gameAccountId != m_spectateeFriendlySide)
				{
					m_spectateeOpposingSide = gameAccountId;
				}
				BnetParty.AcceptReceivedInvite(receivedInvite.InviteId);
				HideShownUI();
			}
			else
			{
				LogInfoParty("SpectatePlayer: trying to accept an invite even though there is no room for another spectatee: player={0} spectatee1={1} spectatee2={2} isPlayerInGame={3} inviteId={4}", gameAccountId?.ToString() + " (" + BnetUtils.GetPlayerBestName(gameAccountId) + ")", m_spectateeFriendlySide, m_spectateeOpposingSide, IsPlayerInGame(gameAccountId), receivedInvite.InviteId);
				BnetParty.DeclineReceivedInvite(receivedInvite.InviteId);
			}
			return;
		}
		if (joinInfo == null)
		{
			Error.AddWarningLoc("Bad Spectator Key", "Spectator key is blank!");
			return;
		}
		if (!joinInfo.HasPartyId && string.IsNullOrEmpty(joinInfo.SecretKey))
		{
			Error.AddWarningLoc("No Party/Bad Spectator Key", "No party information and Spectator key is blank!");
			return;
		}
		if (joinInfo.HasPartyId && m_requestedInvite != null)
		{
			LogInfoParty("SpectatePlayer: already requesting invite from {0}:party={1}, cannot request another from {2}:party={3}", m_requestedInvite.SpectateeId, m_requestedInvite.PartyId, gameAccountId, BnetUtils.CreatePartyId(joinInfo.PartyId));
			return;
		}
		HideShownUI();
		if (!(m_spectateeFriendlySide != null) || !(m_spectateeOpposingSide == null) || GameMgr.Get() == null || !GameMgr.Get().IsSpectator())
		{
			if (m_spectatorPartyIdMain != null)
			{
				if (IsInSpectatorMode())
				{
					EndSpectatorMode(wasKnownSpectating: true);
				}
				else
				{
					LeaveParty(m_spectatorPartyIdMain, ShouldBePartyLeader(m_spectatorPartyIdMain));
				}
				m_pendingSpectatePlayerAfterLeave = new PendingSpectatePlayer(gameAccountId, joinInfo);
				return;
			}
			if (m_spectatorPartyIdOpposingSide != null)
			{
				m_pendingSpectatePlayerAfterLeave = new PendingSpectatePlayer(gameAccountId, joinInfo);
				LeaveParty(m_spectatorPartyIdOpposingSide, dissolve: false);
				return;
			}
		}
		SpectatePlayer_Internal(gameAccountId, joinInfo);
	}

	private void HideShownUI()
	{
		ShownUIMgr shownUI = ShownUIMgr.Get();
		if (shownUI == null)
		{
			return;
		}
		switch (shownUI.GetShownUI())
		{
		case ShownUIMgr.UI_WINDOW.QUEST_LOG:
			if (QuestLog.Get() != null)
			{
				QuestLog.Get().Hide();
			}
			break;
		case ShownUIMgr.UI_WINDOW.ARENA_STORE:
			if (ArenaStore.Get() != null)
			{
				ArenaStore.Get().Hide();
			}
			break;
		case ShownUIMgr.UI_WINDOW.TAVERN_BRAWL_STORE:
			if (TavernBrawlStore.Get() != null)
			{
				TavernBrawlStore.Get().Hide();
			}
			break;
		}
	}

	private void FireSpectatorModeChanged(OnlineEventType evt, BnetPlayer spectatee)
	{
		if (FriendChallengeMgr.Get() != null)
		{
			FriendChallengeMgr.Get().UpdateMyAvailability();
		}
		if (this.OnSpectatorModeChanged != null)
		{
			this.OnSpectatorModeChanged(evt, spectatee);
		}
	}

	private void SpectatePlayer_Internal(BnetGameAccountId gameAccountId, JoinInfo joinInfo)
	{
		if (!m_initialized)
		{
			LogInfoParty("ERROR: SpectatePlayer_Internal called before initialized; spectatee={0}", gameAccountId);
		}
		m_pendingSpectatePlayerAfterLeave = null;
		if (WelcomeQuests.Get() != null)
		{
			WelcomeQuests.Hide();
		}
		PartyInvite receivedInvite = BnetParty.GetReceivedInviteFrom(gameAccountId, PartyType.SPECTATOR_PARTY);
		if (m_spectateeFriendlySide == null)
		{
			LogInfoPower("================== Begin Spectating 1st player ==================");
			m_spectateeFriendlySide = gameAccountId;
			if (receivedInvite != null)
			{
				CloseWaitingForNextGameDialog();
				BnetParty.AcceptReceivedInvite(receivedInvite.InviteId);
			}
			else if (joinInfo.HasPartyId)
			{
				BnetPartyId partyId = BnetUtils.CreatePartyId(joinInfo.PartyId);
				m_requestedInvite = new IntendedSpectateeParty(gameAccountId, partyId);
				BnetGameAccountId myself = BnetPresenceMgr.Get().GetMyGameAccountId();
				BnetParty.RequestInvite(partyId, gameAccountId, myself, PartyType.SPECTATOR_PARTY);
				Processor.ScheduleCallback(5f, realTime: true, SpectatePlayer_RequestInvite_FriendlySide_Timeout);
			}
			else
			{
				CloseWaitingForNextGameDialog();
				m_isExpectingArriveInGameplayAsSpectator = true;
				GameMgr.Get().SpectateGame(joinInfo);
			}
		}
		else if (m_spectateeOpposingSide == null)
		{
			if (!IsPlayerInGame(gameAccountId))
			{
				Error.AddWarning(GameStrings.Get("GLOBAL_ERROR_GENERIC_HEADER"), GameStrings.Get("GLOBAL_SPECTATOR_ERROR_CANNOT_SPECTATE_2_GAMES"));
			}
			else
			{
				if (m_spectateeFriendlySide == gameAccountId)
				{
					LogInfoParty("SpectatePlayer: already spectating player {0}", gameAccountId);
					if (receivedInvite != null)
					{
						BnetParty.AcceptReceivedInvite(receivedInvite.InviteId);
					}
					return;
				}
				LogInfoPower("================== Begin Spectating 2nd player ==================");
				m_spectateeOpposingSide = gameAccountId;
				if (receivedInvite != null)
				{
					BnetParty.AcceptReceivedInvite(receivedInvite.InviteId);
				}
				else if (joinInfo.HasPartyId)
				{
					BnetPartyId partyId2 = BnetUtils.CreatePartyId(joinInfo.PartyId);
					m_requestedInvite = new IntendedSpectateeParty(gameAccountId, partyId2);
					BnetGameAccountId myself2 = BnetPresenceMgr.Get().GetMyGameAccountId();
					BnetParty.RequestInvite(partyId2, gameAccountId, myself2, PartyType.SPECTATOR_PARTY);
					Processor.ScheduleCallback(5f, realTime: true, SpectatePlayer_RequestInvite_OpposingSide_Timeout);
				}
				else
				{
					SpectateSecondPlayer_Network(joinInfo);
				}
			}
		}
		else
		{
			if (m_spectateeFriendlySide == gameAccountId || m_spectateeOpposingSide == gameAccountId)
			{
				LogInfoParty("SpectatePlayer: already spectating player {0}", gameAccountId);
				return;
			}
			Error.AddDevFatal("Cannot spectate more than 2 players.");
		}
		BnetRecentPlayerMgr.Get().AddRecentPlayer(BnetUtils.GetPlayer(gameAccountId), BnetRecentPlayerMgr.RecentReason.CURRENTLY_SPECTATING);
	}

	private void SpectatePlayer_RequestInvite_FriendlySide_Timeout(object userData)
	{
		if (m_requestedInvite != null)
		{
			m_spectateeFriendlySide = null;
			EndSpectatorMode(wasKnownSpectating: true);
			string header = GameStrings.Get("GLOBAL_SPECTATOR_SERVER_REJECTED_HEADER");
			string bodyText = GameStrings.Get("GLOBAL_SPECTATOR_SERVER_REJECTED_TEXT");
			DisplayErrorDialog(header, bodyText);
			if (this.OnSpectateRejected != null)
			{
				this.OnSpectateRejected();
			}
		}
	}

	private void SpectatePlayer_RequestInvite_OpposingSide_Timeout(object userData)
	{
		if (m_requestedInvite != null)
		{
			m_requestedInvite = null;
			m_spectateeOpposingSide = null;
			string header = GameStrings.Get("GLOBAL_SPECTATOR_SERVER_REJECTED_HEADER");
			string bodyText = GameStrings.Get("GLOBAL_SPECTATOR_SERVER_REJECTED_TEXT");
			DisplayErrorDialog(header, bodyText);
			if (this.OnSpectateRejected != null)
			{
				this.OnSpectateRejected();
			}
		}
	}

	private static JoinInfo CreateJoinInfo(PartyServerInfo serverInfo)
	{
		JoinInfo joinInfo = new JoinInfo();
		joinInfo.ServerIpAddress = serverInfo.ServerIpAddress;
		joinInfo.ServerPort = serverInfo.ServerPort;
		joinInfo.GameHandle = serverInfo.GameHandle;
		joinInfo.SecretKey = serverInfo.SecretKey;
		if (serverInfo.HasGameType)
		{
			joinInfo.GameType = serverInfo.GameType;
		}
		if (serverInfo.HasFormatType)
		{
			joinInfo.FormatType = serverInfo.FormatType;
		}
		if (serverInfo.HasMissionId)
		{
			joinInfo.MissionId = serverInfo.MissionId;
		}
		return joinInfo;
	}

	private static bool IsSameGameAndServer(PartyServerInfo a, GameServerInfo b)
	{
		if (a == null)
		{
			return b == null;
		}
		if (b == null)
		{
			return false;
		}
		if (a.ServerIpAddress == b.Address)
		{
			return a.GameHandle == b.GameHandle;
		}
		return false;
	}

	private void SpectateSecondPlayer_Network(JoinInfo joinInfo)
	{
		GameServerInfo serverInfo = new GameServerInfo();
		serverInfo.Address = joinInfo.ServerIpAddress;
		serverInfo.Port = joinInfo.ServerPort;
		serverInfo.GameHandle = (uint)joinInfo.GameHandle;
		serverInfo.SpectatorPassword = joinInfo.SecretKey;
		serverInfo.SpectatorMode = true;
		Network.Get().SpectateSecondPlayer(serverInfo);
	}

	private void JoinPartyGame(BnetPartyId partyId)
	{
		if (!(partyId == null))
		{
			PartyInfo party = BnetParty.GetJoinedParty(partyId);
			if (party != null)
			{
				BattleNet.GetPartyAttribute(partyId, "WTCG.Party.ServerInfo", out var attribute);
				BnetParty_OnPartyAttributeChanged_ServerInfo(party, attribute);
			}
		}
	}

	public void LeaveSpectatorMode()
	{
		GameMgr gameManagerInstance = GameMgr.Get();
		bool num = gameManagerInstance.IsSpectator();
		if (num)
		{
			if (Network.Get().IsConnectedToGameServer())
			{
				Network.Get().DisconnectFromGameServer(Network.DisconnectReason.LeaveSpectator);
			}
			else
			{
				LeaveGameScene();
			}
		}
		if (num || gameManagerInstance.WasSpectator())
		{
			if (m_spectatorPartyIdOpposingSide != null)
			{
				LeaveParty(m_spectatorPartyIdOpposingSide, dissolve: false);
			}
			if (m_spectatorPartyIdMain != null)
			{
				LeaveParty(m_spectatorPartyIdMain, dissolve: false);
			}
		}
		BnetRecentPlayerMgr.Get().AddRecentPlayer(BnetUtils.GetPlayer(m_spectateeFriendlySide), BnetRecentPlayerMgr.RecentReason.RECENT_SPECTATED);
	}

	private void OnDisconnect(BattleNetErrors error)
	{
		m_leavePartyIdsRequested?.Clear();
		m_spectateeFriendlySide = null;
		m_spectateeOpposingSide = null;
		m_spectatorPartyIdMain = null;
		m_spectatorPartyIdOpposingSide = null;
		m_requestedInvite = null;
		m_waitingForNextGameDialog = null;
		m_pendingSpectatePlayerAfterLeave = null;
		m_isExpectingArriveInGameplayAsSpectator = false;
		CloseWaitingForNextGameDialog();
	}

	public void InviteToSpectateMe(BnetPlayer player)
	{
		if (player == null)
		{
			return;
		}
		BnetGameAccountId gameAccountId = player.GetHearthstoneGameAccountId();
		if (m_kickedPlayers != null && m_kickedPlayers.Contains(gameAccountId))
		{
			m_kickedPlayers.Remove(gameAccountId);
		}
		if (CanInviteToSpectateMyGame(gameAccountId))
		{
			if (m_userInitiatedOutgoingInvites == null)
			{
				m_userInitiatedOutgoingInvites = new HashSet<BnetGameAccountId>();
			}
			m_userInitiatedOutgoingInvites.Add(gameAccountId);
			if (m_spectatorPartyIdMain == null)
			{
				byte[] creatorBlob = ProtobufUtil.ToByteArray(BnetUtils.CreatePegasusBnetId(BnetPresenceMgr.Get().GetMyGameAccountId()));
				BnetParty.CreateParty(PartyType.SPECTATOR_PARTY, ChannelApi.PartyPrivacyLevel.OpenInvitation, creatorBlob, null);
			}
			else
			{
				BnetParty.SendInvite(m_spectatorPartyIdMain, gameAccountId, isReservation: false);
			}
		}
	}

	public void KickSpectator(BnetPlayer player, bool regenerateSpectatorPassword)
	{
		KickSpectator_Internal(player, regenerateSpectatorPassword, addToKickList: true);
	}

	private void KickSpectator_Internal(BnetPlayer player, bool regenerateSpectatorPassword, bool addToKickList)
	{
		if (player == null)
		{
			return;
		}
		BnetGameAccountId gameAccountId = player.GetHearthstoneGameAccountId();
		if (!CanKickSpectator(gameAccountId))
		{
			return;
		}
		if (addToKickList)
		{
			if (m_kickedPlayers == null)
			{
				m_kickedPlayers = new HashSet<BnetGameAccountId>();
			}
			m_kickedPlayers.Add(gameAccountId);
		}
		if (Network.Get().IsConnectedToGameServer())
		{
			Network.Get().SendRemoveSpectators(regenerateSpectatorPassword, gameAccountId);
		}
		if (m_spectatorPartyIdMain != null && ShouldBePartyLeader(m_spectatorPartyIdMain) && BnetParty.IsMember(m_spectatorPartyIdMain, gameAccountId))
		{
			BnetParty.KickMember(m_spectatorPartyIdMain, gameAccountId);
		}
	}

	public void UpdateMySpectatorInfo()
	{
		UpdateSpectatorPresence();
		UpdateSpectatorPartyServerInfo();
	}

	private JoinInfo GetMyGameJoinInfo()
	{
		JoinInfo info = null;
		JoinInfo builder = new JoinInfo();
		if (IsInSpectatorMode())
		{
			if (m_spectateeFriendlySide != null)
			{
				BnetId bnetId = BnetUtils.CreatePegasusBnetId(m_spectateeFriendlySide);
				builder.SpectatedPlayers.Add(bnetId);
			}
			if (m_spectateeOpposingSide != null)
			{
				BnetId bnetId2 = BnetUtils.CreatePegasusBnetId(m_spectateeOpposingSide);
				builder.SpectatedPlayers.Add(bnetId2);
			}
			if (builder.SpectatedPlayers.Count > 0)
			{
				info = builder;
			}
		}
		else if (SceneMgr.Get().IsInGame())
		{
			int countCurrentSpectators = GetCountSpectatingMe();
			if (CanAddSpectators())
			{
				GameServerInfo serverInfo = Network.Get().GetLastGameServerJoined();
				if (m_spectatorPartyIdMain == null && serverInfo != null && SceneMgr.Get().IsInGame() && !IsGameOver)
				{
					builder.ServerIpAddress = serverInfo.Address;
					builder.ServerPort = serverInfo.Port;
					builder.GameHandle = (int)serverInfo.GameHandle;
					builder.SecretKey = serverInfo.SpectatorPassword ?? "";
				}
				if (m_spectatorPartyIdMain != null)
				{
					BnetId bnetId3 = BnetUtils.CreatePegasusBnetId(m_spectatorPartyIdMain);
					builder.PartyId = bnetId3;
					builder.GameHandle = (int)serverInfo.GameHandle;
				}
			}
			builder.CurrentNumSpectators = countCurrentSpectators;
			builder.MaxNumSpectators = 10;
			builder.IsJoinable = builder.CurrentNumSpectators < builder.MaxNumSpectators;
			builder.GameType = GameMgr.Get().GetGameType();
			builder.FormatType = GameMgr.Get().GetFormatType();
			builder.MissionId = GameMgr.Get().GetMissionId();
			info = builder;
		}
		return info;
	}

	private static PartyServerInfo GetPartyServerInfo(BnetPartyId partyId)
	{
		if (BattleNet.GetPartyAttribute(partyId, "WTCG.Party.ServerInfo", out byte[] serverInfo))
		{
			return ProtobufUtil.ParseFrom<PartyServerInfo>(serverInfo);
		}
		return null;
	}

	public bool HandleDisconnectFromGameplay()
	{
		bool hasValue = m_expectedDisconnectReason.HasValue;
		EndCurrentSpectatedGame(isLeavingGameplay: false);
		if (hasValue)
		{
			if (GameMgr.Get().IsTransitionPopupShown())
			{
				GameMgr.Get().GetTransitionPopup().Cancel();
				return hasValue;
			}
			LeaveGameScene();
		}
		return hasValue;
	}

	public void OnRealTimeGameOver()
	{
		UpdateMySpectatorInfo();
	}

	private void EndCurrentSpectatedGame(bool isLeavingGameplay)
	{
		if (isLeavingGameplay && IsInSpectatorMode())
		{
			SoundManager.Get().LoadAndPlay("SpectatorMode_Exit.prefab:f1d7dab96facdc64fb6648ff1dd22073");
		}
		m_expectedDisconnectReason = null;
		m_isExpectingArriveInGameplayAsSpectator = false;
		ClearAllGameServerKnownSpectators();
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null && !hearthstoneApplication.IsResetting())
		{
			UpdateSpectatorPresence();
		}
		if (GameMgr.Get() != null && GameMgr.Get().GetTransitionPopup() != null)
		{
			GameMgr.Get().GetTransitionPopup().OnHidden -= EnterSpectatorMode_OnTransitionPopupHide;
		}
	}

	private void EndSpectatorMode(bool wasKnownSpectating = false)
	{
		bool wasExpectingArriveInGameplayAsSpectator = m_isExpectingArriveInGameplayAsSpectator;
		bool num = wasKnownSpectating || m_spectateeFriendlySide != null || m_spectateeOpposingSide != null;
		LeaveSpectatorMode();
		EndCurrentSpectatedGame(isLeavingGameplay: false);
		m_spectateeFriendlySide = null;
		m_spectateeOpposingSide = null;
		m_requestedInvite = null;
		CloseWaitingForNextGameDialog();
		m_pendingSpectatePlayerAfterLeave = null;
		m_isExpectingArriveInGameplayAsSpectator = false;
		if (num)
		{
			LogInfoPower("================== End Spectator Mode ==================");
			BnetPlayer player = BnetUtils.GetPlayer(m_spectateeFriendlySide);
			FireSpectatorModeChanged(OnlineEventType.REMOVED, player);
		}
		SceneMgr.Mode postGameSceneMode = GameMgr.Get().GetPostGameSceneMode();
		if ((postGameSceneMode == SceneMgr.Mode.HUB || postGameSceneMode == SceneMgr.Mode.INVALID) && !PartyManager.Get().IsInParty() && !HearthstoneApplication.Get().IsResetting())
		{
			if (wasExpectingArriveInGameplayAsSpectator)
			{
				ReturnToHub(allowReloadHub: true);
			}
			else
			{
				ReturnToHub();
			}
		}
	}

	public void ReturnToHub(bool allowReloadHub = false)
	{
		SceneMgr.Mode nextMode = SceneMgr.Mode.HUB;
		bool isCurrentlyAtHub = SceneMgr.Get().GetMode() == nextMode;
		if (!GameUtils.IsAnyTutorialComplete() && Network.ShouldBeConnectedToAurora())
		{
			Network.Get().ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_LOST_GAME_CONNECTION");
		}
		else if (!SceneMgr.Get().IsModeRequested(nextMode))
		{
			SceneMgr.Get().SetNextMode(nextMode);
		}
		else if (isCurrentlyAtHub && allowReloadHub)
		{
			SceneMgr.Get().ReloadMode();
		}
		if (isCurrentlyAtHub && !allowReloadHub)
		{
			CheckShowWaitingForNextGameDialog();
		}
	}

	private void ClearAllCacheForReset()
	{
		EndSpectatorMode();
		m_initialized = false;
		m_spectatorPartyIdMain = null;
		m_spectatorPartyIdOpposingSide = null;
		m_requestedInvite = null;
		m_waitingForNextGameDialog = null;
		m_pendingSpectatePlayerAfterLeave = null;
		m_userInitiatedOutgoingInvites = null;
		m_kickedPlayers = null;
		m_kickedFromSpectatingList = null;
		m_expectedDisconnectReason = null;
		m_isExpectingArriveInGameplayAsSpectator = false;
		m_isShowingRemovedAsSpectatorPopup = false;
		m_gameServerKnownSpectators.Clear();
	}

	private void WillReset()
	{
		ClearAllCacheForReset();
		Processor.CancelScheduledCallback(SpectatorManager_UpdatePresenceNextFrame);
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		switch (eventData.m_state)
		{
		case FindGameState.CLIENT_CANCELED:
		case FindGameState.CLIENT_ERROR:
		case FindGameState.BNET_QUEUE_CANCELED:
		case FindGameState.BNET_ERROR:
			if (IsInSpectatorMode())
			{
				EndSpectatorMode(wasKnownSpectating: true);
			}
			break;
		case FindGameState.SERVER_GAME_CANCELED:
			if (IsInSpectatorMode())
			{
				string header = GameStrings.Get("GLOBAL_SPECTATOR_SERVER_REJECTED_HEADER");
				string bodyText = GameStrings.Get("GLOBAL_SPECTATOR_SERVER_REJECTED_TEXT");
				DisplayErrorDialog(header, bodyText);
				EndSpectatorMode(wasKnownSpectating: true);
				if (this.OnSpectateRejected != null)
				{
					this.OnSpectateRejected();
				}
			}
			break;
		}
		return false;
	}

	private void GameState_InitializedEvent(GameState instance, object userData)
	{
		if (m_spectatorPartyIdOpposingSide != null)
		{
			GameState.Get().RegisterCreateGameListener(GameState_CreateGameEvent, null);
		}
	}

	private void GameState_CreateGameEvent(GameState.CreateGamePhase createGamePhase, object userData)
	{
		if (createGamePhase >= GameState.CreateGamePhase.CREATED)
		{
			GameState.Get().UnregisterCreateGameListener(GameState_CreateGameEvent);
			if (m_spectatorPartyIdOpposingSide != null)
			{
				AutoSpectateOpposingSide();
			}
		}
	}

	private void AutoSpectateOpposingSide()
	{
		if (GameState.Get() == null)
		{
			return;
		}
		if (GameState.Get().GetCreateGamePhase() < GameState.CreateGamePhase.CREATED)
		{
			GameState.Get().RegisterCreateGameListener(GameState_CreateGameEvent, null);
		}
		else
		{
			if (SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY)
			{
				return;
			}
			if (GameMgr.Get().GetTransitionPopup() != null && GameMgr.Get().GetTransitionPopup().IsShown())
			{
				GameMgr.Get().GetTransitionPopup().OnHidden += EnterSpectatorMode_OnTransitionPopupHide;
			}
			else
			{
				if (!(m_spectatorPartyIdOpposingSide != null) || !(m_spectateeOpposingSide != null) || !IsStillInParty(m_spectatorPartyIdOpposingSide))
				{
					return;
				}
				if (IsPlayerInGame(m_spectateeOpposingSide))
				{
					PartyServerInfo serverInfo = GetPartyServerInfo(m_spectatorPartyIdOpposingSide);
					JoinInfo joinInfo = ((serverInfo == null) ? null : CreateJoinInfo(serverInfo));
					if (joinInfo != null)
					{
						SpectateSecondPlayer_Network(joinInfo);
					}
				}
				else
				{
					LogInfoPower("================== End Spectating 2nd player ==================");
					LeaveParty(m_spectatorPartyIdOpposingSide, dissolve: false);
				}
			}
		}
	}

	private void OnSceneUnloaded(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		SceneMgr.Mode newMode = SceneMgr.Get().GetMode();
		if (newMode == SceneMgr.Mode.GAMEPLAY)
		{
			m_gameServerKnownSpectators.Clear();
		}
		if (newMode == SceneMgr.Mode.GAMEPLAY && prevMode != SceneMgr.Mode.GAMEPLAY)
		{
			if (m_spectateeFriendlySide != null)
			{
				BnetBar.Get().HideFriendList();
			}
			if (GameMgr.Get().IsSpectator())
			{
				if (GameMgr.Get().GetTransitionPopup() != null)
				{
					GameMgr.Get().GetTransitionPopup().OnHidden += EnterSpectatorMode_OnTransitionPopupHide;
				}
				BnetPlayer player = BnetUtils.GetPlayer(m_spectateeOpposingSide ?? m_spectateeFriendlySide);
				FireSpectatorModeChanged(OnlineEventType.ADDED, player);
			}
			else
			{
				m_kickedPlayers = null;
			}
			CloseWaitingForNextGameDialog();
			DeclineAllReceivedInvitations();
			UpdateMySpectatorInfo();
		}
		else
		{
			if (prevMode != SceneMgr.Mode.GAMEPLAY || newMode == SceneMgr.Mode.GAMEPLAY)
			{
				return;
			}
			if (IsInSpectatorMode())
			{
				LogInfoPower("================== End Spectator Game ==================");
				TimeScaleMgr.Get().SetGameTimeScale(1f);
			}
			EndCurrentSpectatedGame(isLeavingGameplay: true);
			UpdateMySpectatorInfo();
			if (!IsInSpectatorMode())
			{
				return;
			}
			PartyServerInfo serverInfo = GetPartyServerInfo(m_spectatorPartyIdMain);
			if (serverInfo == null)
			{
				ShowWaitingForNextGameDialog();
				return;
			}
			GameServerInfo lastServerInfo = Network.Get().GetLastGameServerJoined();
			if (!IsSameGameAndServer(serverInfo, lastServerInfo))
			{
				LogInfoPower("================== OnSceneUnloaded: auto-spectating game after leaving game ==================");
				BattleNet.GetPartyAttribute(m_spectatorPartyIdMain, "WTCG.Party.ServerInfo", out var serverInfoAttribute);
				BnetParty_OnPartyAttributeChanged_ServerInfo(new PartyInfo(m_spectatorPartyIdMain, PartyType.SPECTATOR_PARTY), serverInfoAttribute);
			}
			else
			{
				ShowWaitingForNextGameDialog();
			}
		}
	}

	public void CheckShowWaitingForNextGameDialog()
	{
		bool canShow = true;
		if (!IsInSpectatorMode())
		{
			canShow = false;
		}
		else if (SceneMgr.Get().GetNextMode() != 0)
		{
			canShow = false;
		}
		else if (IsInSpectatableScene(alsoCheckRequestedScene: true))
		{
			canShow = false;
		}
		if (canShow)
		{
			ShowWaitingForNextGameDialog();
		}
		else
		{
			CloseWaitingForNextGameDialog();
		}
	}

	public void ShowWaitingForNextGameDialog()
	{
		if (Network.IsLoggedIn())
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_id = "SPECTATOR_WAITING_FOR_NEXT_GAME";
			info.m_layerToUse = GameLayer.UI;
			info.m_headerText = GameStrings.Get("GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_HEADER");
			info.m_text = GetWaitingForNextGameDialogText();
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CANCEL;
			info.m_cancelText = GameStrings.Get("GLOBAL_LEAVE_SPECTATOR_MODE");
			info.m_responseCallback = OnSceneUnloaded_AwaitingNextGame_LeaveSpectatorMode;
			info.m_keyboardEscIsCancel = false;
			DialogManager.Get().ShowUniquePopup(info, OnSceneUnloaded_AwaitingNextGame_DialogProcessCallback);
			Processor.CancelScheduledCallback(WaitingForNextGame_AutoLeaveSpectatorMode);
			if ((float)WAITING_FOR_NEXT_GAME_AUTO_LEAVE_SECONDS >= 0f)
			{
				Processor.ScheduleCallback(WAITING_FOR_NEXT_GAME_AUTO_LEAVE_SECONDS, realTime: true, WaitingForNextGame_AutoLeaveSpectatorMode);
			}
		}
	}

	private void CloseWaitingForNextGameDialog()
	{
		if ((bool)DISABLE_MENU_BUTTON_WHILE_WAITING)
		{
			BnetBar.Get().m_menuButton.SetEnabled(enabled: true);
		}
		if (DialogManager.Get() != null)
		{
			DialogManager.Get().RemoveUniquePopupRequestFromQueue("SPECTATOR_WAITING_FOR_NEXT_GAME");
		}
		if (m_waitingForNextGameDialog != null)
		{
			m_waitingForNextGameDialog.Hide();
			m_waitingForNextGameDialog = null;
		}
		Processor.CancelScheduledCallback(WaitingForNextGame_AutoLeaveSpectatorMode);
	}

	private void UpdateWaitingForNextGameDialog()
	{
		if (!(m_waitingForNextGameDialog == null))
		{
			string text = GetWaitingForNextGameDialogText();
			m_waitingForNextGameDialog.BodyText = text;
		}
	}

	private string GetWaitingForNextGameDialogText()
	{
		BnetPlayer player = BnetUtils.GetPlayer(m_spectateeFriendlySide);
		string playerName = BnetUtils.GetPlayerBestName(m_spectateeFriendlySide);
		string textKey = null;
		string statusText = null;
		if (player != null && player.IsOnline())
		{
			statusText = PresenceMgr.Get().GetStatusText(player) ?? "";
			if (!string.IsNullOrEmpty(statusText))
			{
				statusText = statusText.Trim();
				textKey = "GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_TEXT";
			}
			else
			{
				textKey = "GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_TEXT_ONLINE";
			}
			Enum[] enumVals = PresenceMgr.Get().GetStatusEnums(player);
			if (enumVals.Length != 0 && (Global.PresenceStatus)(object)enumVals[0] == Global.PresenceStatus.ADVENTURE_SCENARIO_SELECT)
			{
				if (enumVals.Length > 1 && (PresenceAdventureMode)(object)enumVals[1] < PresenceAdventureMode.RETURNING_PLAYER_CHALLENGE)
				{
					textKey = "GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_TEXT_ENTERING";
				}
			}
			else if (enumVals.Length != 0 && (Global.PresenceStatus)(object)enumVals[0] == Global.PresenceStatus.ADVENTURE_SCENARIO_PLAYING_GAME)
			{
				if (enumVals.Length > 1 && GameUtils.IsHeroicAdventureMission((int)(ScenarioDbId)(object)enumVals[1]))
				{
					textKey = "GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_TEXT_BATTLING";
				}
				else if (enumVals.Length > 1 && GameUtils.IsClassChallengeMission((int)(ScenarioDbId)(object)enumVals[1]))
				{
					textKey = "GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_TEXT_PLAYING";
				}
			}
		}
		else
		{
			textKey = "GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_TEXT_OFFLINE";
			statusText = GameStrings.Get("GLOBAL_OFFLINE");
		}
		return GameStrings.Format(textKey, playerName, statusText);
	}

	private bool OnSceneUnloaded_AwaitingNextGame_DialogProcessCallback(DialogBase dialog, object userData)
	{
		if (SceneMgr.Get().IsInGame() || (GameMgr.Get() != null && GameMgr.Get().IsFindingGame()))
		{
			return false;
		}
		m_waitingForNextGameDialog = (AlertPopup)dialog;
		UpdateWaitingForNextGameDialog();
		if ((bool)DISABLE_MENU_BUTTON_WHILE_WAITING)
		{
			BnetBar.Get().m_menuButton.SetEnabled(enabled: false);
		}
		return true;
	}

	private static void WaitingForNextGame_AutoLeaveSpectatorMode(object userData)
	{
		if (Get().IsInSpectatorMode() && !SceneMgr.Get().IsInGame())
		{
			Get().LeaveSpectatorMode();
			string header = GameStrings.Get("GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_HEADER");
			string bodyText = GameStrings.Format("GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_TIMEOUT");
			DisplayErrorDialog(header, bodyText);
		}
	}

	private void OnSceneUnloaded_AwaitingNextGame_LeaveSpectatorMode(AlertPopup.Response response, object userData)
	{
		LeaveSpectatorMode();
	}

	private void EnterSpectatorMode_OnTransitionPopupHide(TransitionPopup popup)
	{
		popup.OnHidden -= EnterSpectatorMode_OnTransitionPopupHide;
		if (SoundManager.Get() != null)
		{
			SoundManager.Get().LoadAndPlay("SpectatorMode_Enter.prefab:e0c11cb0f554e6c4cb9f24994bf13e1c");
		}
		if (m_spectateeOpposingSide != null)
		{
			AutoSpectateOpposingSide();
		}
	}

	private void OnSpectatorOpenJoinOptionChanged(Option option, object prevValue, bool existed, object userData)
	{
		bool openJoinEnabled = Options.Get().GetBool(Option.SPECTATOR_OPEN_JOIN);
		if ((!existed || (bool)prevValue != openJoinEnabled) && ServiceManager.IsAvailable<SceneMgr>() && SceneMgr.Get().IsInGame() && (GameMgr.Get() == null || !GameMgr.Get().IsSpectator()))
		{
			JoinInfo joinInfo = ((!openJoinEnabled) ? null : GetMyGameJoinInfo());
			if (Network.ShouldBeConnectedToAurora() && Network.IsLoggedIn())
			{
				BnetPresenceMgr.Get().SetPresenceSpectatorJoinInfo(joinInfo);
			}
		}
	}

	private void Network_OnSpectatorNotifyEvent()
	{
		SpectatorNotify notify = Network.Get().GetSpectatorNotify();
		if (notify == null)
		{
			TelemetryManager.Client().SendLiveIssue("Network_OnSpectatorNotifyEvent Exception", "'notify' is null.");
			return;
		}
		if (notify.HasSpectatorPasswordUpdate && !string.IsNullOrEmpty(notify.SpectatorPasswordUpdate))
		{
			GameServerInfo serverInfo = Network.Get().GetLastGameServerJoined();
			if (serverInfo == null)
			{
				TelemetryManager.Client().SendLiveIssue("Network_OnSpectatorNotifyEvent Exception", "'serverInfo' is null.");
			}
			else if (!notify.SpectatorPasswordUpdate.Equals(serverInfo.SpectatorPassword))
			{
				serverInfo.SpectatorPassword = notify.SpectatorPasswordUpdate;
				UpdateMySpectatorInfo();
				RevokeAllSentInvitations();
			}
		}
		if (notify.HasSpectatorRemoved)
		{
			m_expectedDisconnectReason = notify.SpectatorRemoved.ReasonCode;
			GameMgr gameMgr = GameMgr.Get();
			if (gameMgr == null)
			{
				TelemetryManager.Client().SendLiveIssue("Network_OnSpectatorNotifyEvent Exception", "GameMgr is null.");
			}
			bool gotoNextMode = gameMgr?.IsTransitionPopupShown() ?? false;
			if (notify.SpectatorRemoved.ReasonCode == 0)
			{
				if (notify.SpectatorRemoved.HasRemovedBy)
				{
					GameServerInfo serverInfo2 = Network.Get().GetLastGameServerJoined();
					if (serverInfo2 != null)
					{
						if (m_kickedFromSpectatingList == null)
						{
							m_kickedFromSpectatingList = new Map<BnetGameAccountId, uint>();
						}
						BnetGameAccountId removedBy = BnetGameAccountId.CreateFromNet(notify.SpectatorRemoved.RemovedBy);
						m_kickedFromSpectatingList[removedBy] = serverInfo2.GameHandle;
					}
				}
				if (!m_isShowingRemovedAsSpectatorPopup)
				{
					AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
					info.m_headerText = GameStrings.Get("GLOBAL_SPECTATOR_REMOVED_PROMPT_HEADER");
					info.m_text = GameStrings.Get("GLOBAL_SPECTATOR_REMOVED_PROMPT_TEXT");
					info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
					if (gotoNextMode)
					{
						info.m_responseCallback = Network_OnSpectatorNotifyEvent_Removed_GoToNextMode;
					}
					else
					{
						info.m_responseCallback = delegate
						{
							SpectatorManager spectatorManager = Get();
							if (spectatorManager != null)
							{
								spectatorManager.m_isShowingRemovedAsSpectatorPopup = false;
							}
							else
							{
								TelemetryManager.Client().SendLiveIssue("Network_OnSpectatorNotifyEvent Exception", "SpectatorManager is null in response callback.");
							}
						};
					}
					m_isShowingRemovedAsSpectatorPopup = true;
					DialogManager dialogManager = DialogManager.Get();
					if (dialogManager != null)
					{
						dialogManager.ShowPopup(info);
					}
					else
					{
						TelemetryManager.Client().SendLiveIssue("Network_OnSpectatorNotifyEvent Exception", "DialogManager is null.");
					}
				}
			}
			else if (gotoNextMode)
			{
				Network_OnSpectatorNotifyEvent_Removed_GoToNextMode(AlertPopup.Response.OK, null);
			}
			SoundManager soundManager = SoundManager.Get();
			if (soundManager != null)
			{
				soundManager.LoadAndPlay("SpectatorMode_Exit.prefab:f1d7dab96facdc64fb6648ff1dd22073");
			}
			else
			{
				TelemetryManager.Client().SendLiveIssue("Network_OnSpectatorNotifyEvent Exception", "SoundManager is null.");
			}
			EndSpectatorMode(wasKnownSpectating: true);
			m_expectedDisconnectReason = notify.SpectatorRemoved.ReasonCode;
		}
		if (notify == null || notify.SpectatorChange.Count == 0 || (GameMgr.Get() != null && GameMgr.Get().IsSpectator()))
		{
			return;
		}
		foreach (SpectatorChange item in notify.SpectatorChange)
		{
			BnetGameAccountId gameAccountId = BnetGameAccountId.CreateFromNet(item.GameAccountId);
			if (item.IsRemoved)
			{
				RemoveKnownSpectator(gameAccountId);
				continue;
			}
			AddKnownSpectator(gameAccountId);
			ReinviteKnownSpectatorsNotInParty();
		}
	}

	private void Network_OnSpectatorNotifyEvent_Removed_GoToNextMode(AlertPopup.Response response, object userData)
	{
		m_isShowingRemovedAsSpectatorPopup = false;
	}

	private void Presence_OnGameAccountPresenceChange(PresenceUpdate[] updates)
	{
		for (int j = 0; j < updates.Length; j++)
		{
			PresenceUpdate update = updates[j];
			BnetGameAccountId entityId = BnetGameAccountId.CreateFromBnetEntityId(update.entityId);
			bool isOnlinePresence = update.fieldId == 1 && update.programId == BnetProgramId.BNET;
			bool isStatusPresence = update.programId == BnetProgramId.HEARTHSTONE && update.fieldId == 17;
			if (m_waitingForNextGameDialog != null && m_spectateeFriendlySide != null && (isOnlinePresence || isStatusPresence) && entityId == m_spectateeFriendlySide)
			{
				UpdateWaitingForNextGameDialog();
			}
			if (!isOnlinePresence || !update.boolVal)
			{
				continue;
			}
			BnetPartyId[] joinedPartyIds = BnetParty.GetJoinedPartyIds();
			foreach (BnetPartyId partyId in joinedPartyIds)
			{
				if (BnetParty.IsLeader(partyId) && !BnetParty.IsMember(partyId, entityId))
				{
					BnetGameAccountId creatorId = GetPartyCreator(partyId);
					if (creatorId != null && creatorId == entityId && !BnetParty.GetSentInvites(partyId).Any((PartyInvite i) => i.InviteeId == entityId))
					{
						BnetParty.SendInvite(partyId, entityId, isReservation: false);
					}
				}
			}
		}
	}

	private void BnetFriendMgr_OnFriendsChanged(BnetFriendChangelist changelist, object userData)
	{
		if (changelist != null)
		{
			List<BnetPlayer> removed = changelist.GetRemovedFriends();
			CheckSpectatorsOnChangedContext(removed);
		}
	}

	private void CheckSpectatorsOnChangedContext(List<BnetPlayer> players)
	{
		if (!IsBeingSpectated() || players == null)
		{
			return;
		}
		foreach (BnetPlayer player in players)
		{
			BnetGameAccountId gameAccountId = player.GetHearthstoneGameAccountId();
			if (IsSpectatingMe(gameAccountId) && !IsInSpectableContextWithPlayer(gameAccountId))
			{
				KickSpectator_Internal(player, regenerateSpectatorPassword: true, addToKickList: false);
			}
		}
	}

	private void EndGameScreen_OnTwoScoopsShown(bool shown, EndGameTwoScoop twoScoops)
	{
		if (IsSpectatingOrWatching)
		{
			if (shown)
			{
				Processor.ScheduleCallback(5f, realTime: false, EndGameScreen_OnTwoScoopsShown_AutoClose);
			}
			else
			{
				Processor.CancelScheduledCallback(EndGameScreen_OnTwoScoopsShown_AutoClose);
			}
		}
	}

	private void EndGameScreen_OnTwoScoopsShown_AutoClose(object userData)
	{
		if (EndGameScreen.Get() == null)
		{
			return;
		}
		if ((float)WAITING_FOR_NEXT_GAME_AUTO_LEAVE_SECONDS >= 0f)
		{
			int attempt = 0;
			while (EndGameScreen.Get().ContinueEvents())
			{
				attempt++;
				if (attempt > 100)
				{
					break;
				}
			}
		}
		else
		{
			EndGameScreen.Get().ContinueEvents();
		}
	}

	private void EndGameScreen_OnBackOutOfGameplay()
	{
		if (PartyManager.Get().IsInParty())
		{
			LeaveSpectatorMode();
		}
	}

	private void BnetParty_OnError(PartyError error)
	{
		if (!error.IsOperationCallback)
		{
			return;
		}
		switch (error.FeatureEvent)
		{
		case BnetFeatureEvent.Party_Leave_Callback:
		case BnetFeatureEvent.Party_Dissolve_Callback:
			if (m_leavePartyIdsRequested != null)
			{
				m_leavePartyIdsRequested.Remove(error.PartyId);
			}
			if (m_pendingSpectatePlayerAfterLeave != null && error.ErrorCode != 0)
			{
				string playerName = BnetUtils.GetPlayerBestName(m_pendingSpectatePlayerAfterLeave.SpectateeId);
				string header2 = GameStrings.Get("GLOBAL_ERROR_GENERIC_HEADER");
				string body2 = GameStrings.Format("GLOBAL_SPECTATOR_ERROR_LEAVE_FOR_SPECTATE_PLAYER_TEXT", playerName);
				DisplayErrorDialog(header2, body2);
				m_pendingSpectatePlayerAfterLeave = null;
			}
			break;
		case BnetFeatureEvent.Party_Create_Callback:
			if (error.ErrorCode != 0)
			{
				m_userInitiatedOutgoingInvites = null;
				string header = GameStrings.Get("GLOBAL_ERROR_GENERIC_HEADER");
				string body = GameStrings.Format("GLOBAL_SPECTATOR_ERROR_CREATE_PARTY_TEXT");
				DisplayErrorDialog(header, body);
			}
			break;
		}
	}

	private static void DisplayErrorDialog(string header, string body)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = header;
		info.m_text = body;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.Get().ShowPopup(info);
	}

	private void BnetParty_OnJoined(OnlineEventType evt, PartyInfo party, LeaveReason? reason)
	{
		if (!m_initialized || party.Type != PartyType.SPECTATOR_PARTY)
		{
			return;
		}
		if (evt == OnlineEventType.REMOVED)
		{
			bool wasLeaveRequested = false;
			if (m_leavePartyIdsRequested != null)
			{
				wasLeaveRequested = m_leavePartyIdsRequested.Remove(party.Id);
			}
			LogInfoParty("SpectatorParty_OnLeft: left party={0} current={1} reason={2} wasRequested={3}", party, m_spectatorPartyIdMain, reason.HasValue ? reason.Value.ToString() : "null", wasLeaveRequested);
			bool leftAsSpectator = false;
			if (party.Id == m_spectatorPartyIdOpposingSide)
			{
				m_spectatorPartyIdOpposingSide = null;
				leftAsSpectator = true;
			}
			else if (m_spectateeFriendlySide != null)
			{
				if (party.Id == m_spectatorPartyIdMain)
				{
					m_spectatorPartyIdMain = null;
					leftAsSpectator = true;
				}
			}
			else if (m_spectateeFriendlySide == null && m_spectateeOpposingSide == null)
			{
				if (party.Id != m_spectatorPartyIdMain)
				{
					CreatePartyIfNecessary();
					return;
				}
				m_userInitiatedOutgoingInvites = null;
				m_spectatorPartyIdMain = null;
				UpdateSpectatorPresence();
				if (reason.HasValue && reason.Value != 0 && reason.Value != LeaveReason.DISSOLVED_BY_MEMBER)
				{
					Processor.ScheduleCallback(1f, realTime: true, delegate
					{
						CreatePartyIfNecessary();
					});
				}
			}
			if (m_pendingSpectatePlayerAfterLeave != null && m_spectatorPartyIdMain == null && m_spectatorPartyIdOpposingSide == null)
			{
				SpectatePlayer_Internal(m_pendingSpectatePlayerAfterLeave.SpectateeId, m_pendingSpectatePlayerAfterLeave.JoinInfo);
			}
			else if (leftAsSpectator && m_spectatorPartyIdMain == null)
			{
				if (wasLeaveRequested)
				{
					EndSpectatorMode(wasKnownSpectating: true);
				}
				else
				{
					bool intentialLeaderRemoval = reason.HasValue && reason.Value == LeaveReason.MEMBER_KICKED;
					bool receivedServerKickNotify = m_expectedDisconnectReason.HasValue && m_expectedDisconnectReason.Value == 0;
					EndSpectatorMode(wasKnownSpectating: true);
					if (intentialLeaderRemoval && !receivedServerKickNotify)
					{
						if (intentialLeaderRemoval)
						{
							BnetGameAccountId leaderId = GetPartyCreator(party.Id);
							if (leaderId == null)
							{
								leaderId = BnetParty.GetLeader(party.Id)?.GameAccountId;
							}
							if (leaderId != null)
							{
								GameServerInfo serverInfo = Network.Get().GetLastGameServerJoined();
								if (serverInfo != null)
								{
									if (m_kickedFromSpectatingList == null)
									{
										m_kickedFromSpectatingList = new Map<BnetGameAccountId, uint>();
									}
									m_kickedFromSpectatingList[leaderId] = serverInfo.GameHandle;
								}
							}
						}
						if (!m_isShowingRemovedAsSpectatorPopup)
						{
							bool num = GameMgr.Get().IsTransitionPopupShown();
							AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
							{
								m_headerText = GameStrings.Get("GLOBAL_SPECTATOR_REMOVED_PROMPT_HEADER"),
								m_text = (BnetPresenceMgr.Get().GetMyPlayer().IsAppearingOffline() ? GameStrings.Get("GLOBAL_SPECTATOR_REMOVED_PROMPT_APPEAR_OFFLINE_TEXT") : GameStrings.Get("GLOBAL_SPECTATOR_REMOVED_PROMPT_TEXT")),
								m_responseDisplay = AlertPopup.ResponseDisplay.OK
							};
							if (num)
							{
								info.m_responseCallback = Network_OnSpectatorNotifyEvent_Removed_GoToNextMode;
							}
							else
							{
								info.m_responseCallback = delegate
								{
									Get().m_isShowingRemovedAsSpectatorPopup = false;
								};
							}
							m_isShowingRemovedAsSpectatorPopup = true;
							DialogManager.Get().ShowPopup(info);
						}
					}
				}
			}
			Processor.ScheduleCallback(0.5f, realTime: false, BnetParty_OnLostPartyReference_RemoveKnownCreator, party.Id);
		}
		if (evt != 0)
		{
			return;
		}
		BnetGameAccountId creatorId = GetPartyCreator(party.Id);
		if (creatorId == null)
		{
			LogInfoParty("SpectatorParty_OnJoined: joined party={0} without creator.", party.Id);
			LeaveParty(party.Id, BnetParty.IsLeader(party.Id));
			return;
		}
		if (m_requestedInvite != null && m_requestedInvite.PartyId == party.Id)
		{
			m_requestedInvite = null;
			Processor.CancelScheduledCallback(SpectatePlayer_RequestInvite_FriendlySide_Timeout);
			Processor.CancelScheduledCallback(SpectatePlayer_RequestInvite_OpposingSide_Timeout);
		}
		bool shouldBePartyLeader = ShouldBePartyLeader(party.Id);
		bool wasNotInSpectatorParty = m_spectatorPartyIdMain == null;
		bool isDifferent = wasNotInSpectatorParty;
		if (m_spectatorPartyIdMain != null && m_spectatorPartyIdMain != party.Id && (shouldBePartyLeader || creatorId != m_spectateeOpposingSide))
		{
			isDifferent = true;
			LogInfoParty("SpectatorParty_OnJoined: joined party={0} when different current={1} (will be clobbered) joinedParties={2}", party.Id, m_spectatorPartyIdMain, string.Join(", ", (from i in BnetParty.GetJoinedParties()
				select i.ToString()).ToArray()));
		}
		if (shouldBePartyLeader)
		{
			m_spectatorPartyIdMain = party.Id;
			if (isDifferent)
			{
				UpdateSpectatorPresence();
			}
			UpdateSpectatorPartyServerInfo();
			ReinviteKnownSpectatorsNotInParty();
			if (m_userInitiatedOutgoingInvites != null)
			{
				foreach (BnetGameAccountId playerId in m_userInitiatedOutgoingInvites)
				{
					BnetParty.SendInvite(m_spectatorPartyIdMain, playerId, isReservation: false);
				}
			}
			if (!wasNotInSpectatorParty || this.OnSpectatorToMyGame == null)
			{
				return;
			}
			BnetParty.PartyMember[] members = BnetParty.GetMembers(m_spectatorPartyIdMain);
			foreach (BnetParty.PartyMember member in members)
			{
				if (!(member.GameAccountId == BnetPresenceMgr.Get().GetMyGameAccountId()))
				{
					Processor.RunCoroutine(WaitForPresenceThenToast(member.GameAccountId, SocialToastMgr.TOAST_TYPE.SPECTATOR_ADDED));
					BnetPlayer spectator = BnetUtils.GetPlayer(member.GameAccountId);
					this.OnSpectatorToMyGame(OnlineEventType.ADDED, spectator);
				}
			}
			return;
		}
		bool isExpectedPartyArrival = true;
		if (m_spectateeFriendlySide == null)
		{
			m_spectateeFriendlySide = creatorId;
			m_spectatorPartyIdMain = party.Id;
			isExpectedPartyArrival = false;
		}
		else if (creatorId == m_spectateeFriendlySide)
		{
			m_spectatorPartyIdMain = party.Id;
		}
		else if (creatorId == m_spectateeOpposingSide)
		{
			m_spectatorPartyIdOpposingSide = party.Id;
		}
		if (BattleNet.GetPartyAttribute(party.Id, "WTCG.Party.ServerInfo", out byte[] _))
		{
			LogInfoParty("SpectatorParty_OnJoined: joined party={0} as spectator, begin spectating game.", party.Id);
			if (!isExpectedPartyArrival)
			{
				if (creatorId == m_spectateeOpposingSide)
				{
					LogInfoPower("================== Begin Spectating 2nd player ==================");
				}
				else
				{
					LogInfoPower("================== Begin Spectating 1st player ==================");
				}
			}
			JoinPartyGame(party.Id);
			return;
		}
		if (PartyManager.Get().IsInParty())
		{
			string header = GameStrings.Get("GLOBAL_SPECTATOR_SERVER_REJECTED_HEADER");
			string bodyText = GameStrings.Get("GLOBAL_SPECTATOR_SERVER_REJECTED_TEXT");
			DisplayErrorDialog(header, bodyText);
			EndSpectatorMode(wasKnownSpectating: true);
			if (this.OnSpectateRejected != null)
			{
				this.OnSpectateRejected();
			}
		}
		else if (!SceneMgr.Get().IsInGame())
		{
			ShowWaitingForNextGameDialog();
		}
		BnetPlayer player = BnetUtils.GetPlayer(creatorId);
		FireSpectatorModeChanged(OnlineEventType.ADDED, player);
	}

	private void BnetParty_OnLostPartyReference_RemoveKnownCreator(object userData)
	{
		BnetPartyId partyId = userData as BnetPartyId;
		if (partyId != null && !BnetParty.IsInParty(partyId) && !BnetParty.GetReceivedInvites().Any((PartyInvite i) => i.PartyId == partyId))
		{
			Get().m_knownPartyCreatorIds.Remove(partyId);
		}
	}

	private void BnetParty_OnReceivedInvite(OnlineEventType evt, PartyInfo party, ulong inviteId, BnetGameAccountId inviterId, string inviterBattletag, BnetGameAccountId inviteeId, InviteRemoveReason? reason)
	{
		if (!m_initialized || party.Type != PartyType.SPECTATOR_PARTY)
		{
			return;
		}
		PartyInvite invite = BnetParty.GetReceivedInvite(inviteId);
		bool isRejoin = invite != null && (invite.IsRejoin || (invite.InviterId == invite.InviteeId && invite.InviteeId == BnetPresenceMgr.Get().GetMyGameAccountId()));
		BnetGameAccountId creatorId = ((invite == null) ? null : GetPartyCreator(invite.PartyId));
		BnetPlayer inviter = ((invite == null) ? null : BnetUtils.GetPlayer(invite.InviterId));
		bool acceptInvite = false;
		bool declineInvite = false;
		string acceptDeclineReason = string.Empty;
		switch (evt)
		{
		case OnlineEventType.ADDED:
			if (invite == null)
			{
				return;
			}
			if (isRejoin || ShouldBePartyLeader(invite.PartyId))
			{
				if (ShouldBePartyLeader(invite.PartyId))
				{
					acceptInvite = true;
					acceptDeclineReason = "should_be_leader";
					break;
				}
				if (m_spectatorPartyIdMain != null)
				{
					if (m_spectatorPartyIdMain == invite.PartyId)
					{
						acceptInvite = true;
						acceptDeclineReason = "spectating_this_party";
					}
					else
					{
						declineInvite = true;
						acceptDeclineReason = "spectating_other_party";
					}
					break;
				}
				declineInvite = true;
				acceptDeclineReason = "not_spectating";
				if (creatorId != null && m_spectateeFriendlySide == null)
				{
					m_spectateeFriendlySide = creatorId;
					acceptInvite = true;
					declineInvite = false;
					acceptDeclineReason = "rejoin_spectating";
				}
			}
			else if (invite.InviterId == m_spectateeFriendlySide || invite.InviterId == m_spectateeOpposingSide || (m_requestedInvite != null && m_requestedInvite.PartyId == invite.PartyId))
			{
				acceptInvite = true;
				acceptDeclineReason = "spectating_this_player";
				if (m_requestedInvite != null)
				{
					m_requestedInvite = null;
					Processor.CancelScheduledCallback(SpectatePlayer_RequestInvite_FriendlySide_Timeout);
					Processor.CancelScheduledCallback(SpectatePlayer_RequestInvite_OpposingSide_Timeout);
				}
			}
			else if (!UserAttentionManager.CanShowAttentionGrabber("SpectatorManager.BnetParty_OnReceivedInvite:" + evt))
			{
				declineInvite = true;
				acceptDeclineReason = "user_attention_blocked";
			}
			else
			{
				if (m_kickedFromSpectatingList != null)
				{
					m_kickedFromSpectatingList.Remove(invite.InviterId);
				}
				if (SocialToastMgr.Get() != null)
				{
					string inviterName = BnetUtils.GetInviterBestName(invite);
					SocialToastMgr.Get().AddToast(UserAttentionBlocker.NONE, inviterName, SocialToastMgr.TOAST_TYPE.SPECTATOR_INVITE_RECEIVED);
				}
			}
			break;
		case OnlineEventType.REMOVED:
			if (!reason.HasValue || reason.Value == InviteRemoveReason.ACCEPTED)
			{
				Processor.ScheduleCallback(0.5f, realTime: false, BnetParty_OnLostPartyReference_RemoveKnownCreator, party.Id);
			}
			break;
		}
		LogInfoParty("Spectator_OnReceivedInvite {0} rejoin={1} partyId={2} creatorId={3} accept={4} decline={5} acceptDeclineReason={6} removeReason={7}", evt, isRejoin, party.Id, creatorId, acceptInvite, declineInvite, acceptDeclineReason, reason);
		if (acceptInvite)
		{
			BnetParty.AcceptReceivedInvite(inviteId);
		}
		else if (declineInvite)
		{
			BnetParty.DeclineReceivedInvite(inviteId);
		}
		else if (this.OnInviteReceived != null)
		{
			this.OnInviteReceived(evt, inviter);
		}
	}

	private void BnetParty_OnSentInvite(OnlineEventType evt, PartyInfo party, ulong inviteId, BnetGameAccountId inviterId, BnetGameAccountId inviteeId, bool senderIsMyself, InviteRemoveReason? reason)
	{
		if (party.Type != PartyType.SPECTATOR_PARTY || !senderIsMyself)
		{
			return;
		}
		PartyInvite invite = BnetParty.GetSentInvite(party.Id, inviteId);
		BnetPlayer invitee = ((invite == null) ? null : BnetUtils.GetPlayer(invite.InviteeId));
		if (evt == OnlineEventType.ADDED)
		{
			bool isUserInitiated = false;
			if (m_userInitiatedOutgoingInvites != null && invite != null)
			{
				isUserInitiated = m_userInitiatedOutgoingInvites.Remove(invite.InviteeId);
			}
			if (isUserInitiated && invite != null && ShouldBePartyLeader(party.Id) && !m_gameServerKnownSpectators.Contains(invite.InviteeId) && SocialToastMgr.Get() != null)
			{
				string inviteeName = BnetUtils.GetPlayerBestName(invite.InviteeId);
				SocialToastMgr.Get().AddToast(UserAttentionBlocker.NONE, inviteeName, SocialToastMgr.TOAST_TYPE.SPECTATOR_INVITE_SENT);
			}
		}
		if (invite != null && !m_gameServerKnownSpectators.Contains(invite.InviteeId) && this.OnInviteSent != null)
		{
			this.OnInviteSent(evt, invitee);
		}
	}

	private void BnetParty_OnReceivedInviteRequest(OnlineEventType evt, PartyInfo party, InviteRequest request, InviteRequestRemovedReason? reason)
	{
		if (party.Type == PartyType.SPECTATOR_PARTY && evt == OnlineEventType.ADDED)
		{
			bool decline = false;
			if (party.Id != m_spectatorPartyIdMain)
			{
				decline = true;
			}
			if (request.RequesterId != null && request.RequesterId == request.TargetId && !Options.Get().GetBool(Option.SPECTATOR_OPEN_JOIN))
			{
				decline = true;
			}
			if (!IsInSpectableContextWithPlayer(request.RequesterId))
			{
				decline = true;
			}
			if (!IsInSpectableContextWithPlayer(request.TargetId))
			{
				decline = true;
			}
			if (m_kickedPlayers != null && (m_kickedPlayers.Contains(request.RequesterId) || m_kickedPlayers.Contains(request.TargetId)))
			{
				decline = true;
			}
			if (decline)
			{
				BnetParty.IgnoreInviteRequest(party.Id, request.TargetId);
			}
			else
			{
				BnetParty.AcceptInviteRequest(party.Id, request.TargetId, isReservation: false);
			}
		}
	}

	private void BnetParty_OnMemberEvent(OnlineEventType evt, PartyInfo party, BnetGameAccountId memberId, bool isRolesUpdate, LeaveReason? reason)
	{
		if (party.Id == null || (party.Id != m_spectatorPartyIdMain && party.Id != m_spectatorPartyIdOpposingSide))
		{
			return;
		}
		if (evt == OnlineEventType.ADDED && BnetParty.IsLeader(party.Id))
		{
			BnetGameAccountId creatorId = GetPartyCreator(party.Id);
			if (creatorId != null && creatorId == memberId)
			{
				BnetParty.SetLeader(party.Id, memberId);
			}
		}
		if (m_initialized && evt != OnlineEventType.UPDATED && memberId != BnetPresenceMgr.Get().GetMyGameAccountId() && ShouldBePartyLeader(party.Id) && (!SceneMgr.Get().IsInGame() || !Network.Get().IsConnectedToGameServer() || !m_gameServerKnownSpectators.Contains(memberId)))
		{
			SocialToastMgr.TOAST_TYPE toastType = ((evt == OnlineEventType.ADDED) ? SocialToastMgr.TOAST_TYPE.SPECTATOR_ADDED : SocialToastMgr.TOAST_TYPE.SPECTATOR_REMOVED);
			Processor.RunCoroutine(WaitForPresenceThenToast(memberId, toastType));
			if (this.OnSpectatorToMyGame != null)
			{
				BnetPlayer spectator = BnetUtils.GetPlayer(memberId);
				this.OnSpectatorToMyGame(evt, spectator);
			}
		}
	}

	private void BnetParty_OnChatMessage(PartyInfo party, BnetGameAccountId speakerId, string chatMessage)
	{
	}

	private void BnetParty_OnPartyAttributeChanged_ServerInfo(PartyInfo party, Blizzard.GameService.Protocol.V2.Client.Attribute attribute)
	{
		if (party.Type != PartyType.SPECTATOR_PARTY || !BnetAttribute.GetAttributeValue<byte[]>(attribute, out var blobVal))
		{
			return;
		}
		PartyServerInfo serverInfo = ProtobufUtil.ParseFrom<PartyServerInfo>(blobVal);
		if (serverInfo == null)
		{
			return;
		}
		if (!serverInfo.HasSecretKey || string.IsNullOrEmpty(serverInfo.SecretKey))
		{
			LogInfoParty("BnetParty_OnPartyAttributeChanged_ServerInfo: no secret key in serverInfo.");
			return;
		}
		GameServerInfo lastServerInfo = Network.Get().GetLastGameServerJoined();
		bool alreadyConnected = Network.Get().IsConnectedToGameServer() && IsSameGameAndServer(serverInfo, lastServerInfo);
		if (!alreadyConnected && SceneMgr.Get().IsInGame())
		{
			LogInfoParty("BnetParty_OnPartyAttributeChanged_ServerInfo: cannot join game while in gameplay new={0} curr={1}.", serverInfo.GameHandle, lastServerInfo.GameHandle);
			return;
		}
		JoinInfo joinInfo = CreateJoinInfo(serverInfo);
		if (party.Id == m_spectatorPartyIdOpposingSide)
		{
			if (GameMgr.Get().GetTransitionPopup() == null && GameMgr.Get().IsSpectator())
			{
				SpectateSecondPlayer_Network(joinInfo);
			}
		}
		else if (!alreadyConnected && party.Id == m_spectatorPartyIdMain)
		{
			LogInfoPower("================== Start Spectator Game ==================");
			m_isExpectingArriveInGameplayAsSpectator = true;
			GameMgr.Get().SpectateGame(joinInfo);
			CloseWaitingForNextGameDialog();
		}
	}

	private void LogInfoParty(string format, params object[] args)
	{
		Log.Party.Print(format, args);
	}

	private void LogInfoPower(string format, params object[] args)
	{
		Log.Party.Print(format, args);
		GameState gameState = GameState.Get();
		if (gameState == null || gameState.GameScenarioAllowsPowerPrinting())
		{
			Log.Power.Print(format, args);
		}
	}

	private bool IsPlayerInGame(BnetGameAccountId gameAccountId)
	{
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return false;
		}
		foreach (KeyValuePair<int, Player> item in gameState.GetPlayerMap())
		{
			BnetPlayer player = item.Value.GetBnetPlayer();
			if (player != null && player.GetHearthstoneGameAccountId() == gameAccountId)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsStillInParty(BnetPartyId partyId)
	{
		if (!BnetParty.IsInParty(partyId))
		{
			return false;
		}
		if (m_leavePartyIdsRequested != null && m_leavePartyIdsRequested.Contains(partyId))
		{
			return false;
		}
		return true;
	}

	private void BnetPresenceMgr_OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
	{
		BnetGameAccountId myselfId = BnetPresenceMgr.Get().GetMyGameAccountId();
		BnetPlayerChange myOwnChange = changelist.FindChange(myselfId);
		if (myOwnChange != null)
		{
			bool isNowAppearingOffline = myOwnChange.GetNewPlayer().IsAppearingOffline();
			bool wasPrevAppearingOffline = myOwnChange.GetOldPlayer().IsAppearingOffline();
			if (isNowAppearingOffline && !wasPrevAppearingOffline && MyGameHasSpectators())
			{
				BnetGameAccountId[] array = GetSpectatorPartyMembers().ToArray();
				foreach (BnetGameAccountId spectator in array)
				{
					KickSpectator_Internal(BnetPresenceMgr.Get().GetPlayer(spectator), regenerateSpectatorPassword: true, addToKickList: false);
				}
			}
			else if (wasPrevAppearingOffline && !isNowAppearingOffline)
			{
				UpdateMySpectatorInfo();
			}
		}
		if (!IsBeingSpectated())
		{
			return;
		}
		foreach (BnetPlayerChange change in from c in changelist.GetChanges()
			where c != myOwnChange && c.GetOldPlayer() != null && c.GetOldPlayer().IsOnline() && !c.GetNewPlayer().IsOnline()
			select c)
		{
			KickSpectator_Internal(BnetPresenceMgr.Get().GetPlayer(change.GetPlayer().GetAccountId()), regenerateSpectatorPassword: true, addToKickList: false);
		}
	}

	private void RemoveReceivedInvitation(BnetGameAccountId inviterId)
	{
		if (!(inviterId == null) && m_receivedSpectateMeInvites.Remove(inviterId))
		{
			BnetPlayer inviter = BnetUtils.GetPlayer(inviterId);
			if (this.OnInviteReceived != null)
			{
				this.OnInviteReceived(OnlineEventType.REMOVED, inviter);
			}
		}
	}

	private void RemoveSentInvitation(BnetGameAccountId inviteeId)
	{
		if (!(inviteeId == null) && m_sentSpectateMeInvites.Remove(inviteeId))
		{
			BnetPlayer invitee = BnetUtils.GetPlayer(inviteeId);
			if (this.OnInviteSent != null)
			{
				this.OnInviteSent(OnlineEventType.REMOVED, invitee);
			}
		}
	}

	private void DeclineAllReceivedInvitations()
	{
		PartyInvite[] receivedInvites = BnetParty.GetReceivedInvites();
		foreach (PartyInvite invite in receivedInvites)
		{
			if (invite.PartyType == PartyType.SPECTATOR_PARTY)
			{
				BnetParty.DeclineReceivedInvite(invite.InviteId);
			}
		}
	}

	private void RevokeAllSentInvitations()
	{
		ClearAllSentInvitations();
		BnetGameAccountId myselfId = BnetPresenceMgr.Get().GetMyGameAccountId();
		BnetPartyId[] array = new BnetPartyId[2] { m_spectatorPartyIdMain, m_spectatorPartyIdOpposingSide };
		foreach (BnetPartyId partyId in array)
		{
			if (partyId == null)
			{
				continue;
			}
			PartyInvite[] sentInvites = BnetParty.GetSentInvites(partyId);
			foreach (PartyInvite invite in sentInvites)
			{
				if (!(invite.InviterId != myselfId))
				{
					BnetParty.RevokeSentInvite(partyId, invite.InviteId);
				}
			}
		}
	}

	private void ClearAllSentInvitations()
	{
		BnetGameAccountId[] gameAccountIds = m_sentSpectateMeInvites.Keys.ToArray();
		m_sentSpectateMeInvites.Clear();
		if (this.OnInviteSent != null)
		{
			BnetGameAccountId[] array = gameAccountIds;
			for (int i = 0; i < array.Length; i++)
			{
				BnetPlayer invitee = BnetUtils.GetPlayer(array[i]);
				this.OnInviteSent(OnlineEventType.REMOVED, invitee);
			}
		}
	}

	private void AddKnownSpectator(BnetGameAccountId gameAccountId)
	{
		if (gameAccountId == null)
		{
			return;
		}
		bool num = m_gameServerKnownSpectators.Add(gameAccountId);
		CreatePartyIfNecessary();
		RemoveSentInvitation(gameAccountId);
		RemoveReceivedInvitation(gameAccountId);
		if (!num)
		{
			return;
		}
		if (SceneMgr.Get().IsInGame() && Network.Get().IsConnectedToGameServer())
		{
			bool num2 = BnetParty.IsMember(m_spectatorPartyIdMain, gameAccountId);
			BnetPlayer spectator = BnetUtils.GetPlayer(gameAccountId);
			if (!num2)
			{
				Processor.RunCoroutine(WaitForPresenceThenToast(gameAccountId, SocialToastMgr.TOAST_TYPE.SPECTATOR_ADDED));
			}
			if (this.OnSpectatorToMyGame != null)
			{
				this.OnSpectatorToMyGame(OnlineEventType.ADDED, spectator);
			}
		}
		UpdateSpectatorPresence();
	}

	private void RemoveKnownSpectator(BnetGameAccountId gameAccountId)
	{
		if (gameAccountId == null || !m_gameServerKnownSpectators.Remove(gameAccountId))
		{
			return;
		}
		if (SceneMgr.Get().IsInGame() && Network.Get().IsConnectedToGameServer())
		{
			bool num = BnetParty.IsMember(m_spectatorPartyIdMain, gameAccountId);
			BnetPlayer spectator = BnetUtils.GetPlayer(gameAccountId);
			if (!num)
			{
				Processor.RunCoroutine(WaitForPresenceThenToast(gameAccountId, SocialToastMgr.TOAST_TYPE.SPECTATOR_REMOVED));
			}
			if (this.OnSpectatorToMyGame != null)
			{
				this.OnSpectatorToMyGame(OnlineEventType.REMOVED, spectator);
			}
		}
		UpdateSpectatorPresence();
	}

	private void ClearAllGameServerKnownSpectators()
	{
		BnetGameAccountId[] gameAccountIds = m_gameServerKnownSpectators.ToArray();
		m_gameServerKnownSpectators.Clear();
		if (this.OnSpectatorToMyGame != null && SceneMgr.Get().IsInGame() && Network.Get().IsConnectedToGameServer())
		{
			BnetGameAccountId[] array = gameAccountIds;
			for (int i = 0; i < array.Length; i++)
			{
				BnetPlayer spectator = BnetUtils.GetPlayer(array[i]);
				this.OnSpectatorToMyGame(OnlineEventType.REMOVED, spectator);
			}
		}
		if (gameAccountIds.Length != 0)
		{
			UpdateSpectatorPresence();
		}
	}

	private void UpdateSpectatorPresence()
	{
		if (HearthstoneApplication.Get() != null)
		{
			Processor.CancelScheduledCallback(SpectatorManager_UpdatePresenceNextFrame);
			Processor.ScheduleCallback(0f, realTime: true, SpectatorManager_UpdatePresenceNextFrame);
		}
		else
		{
			SpectatorManager_UpdatePresenceNextFrame(null);
		}
	}

	private void SpectatorManager_UpdatePresenceNextFrame(object userData)
	{
		JoinInfo joinInfo = null;
		bool canSetJoinInfo = Options.Get().GetBool(Option.SPECTATOR_OPEN_JOIN) || IsInSpectatorMode();
		joinInfo = GetMyGameJoinInfo();
		if (Network.ShouldBeConnectedToAurora() && Network.IsLoggedIn())
		{
			BnetPresenceMgr.Get().SetPresenceSpectatorJoinInfo(canSetJoinInfo ? joinInfo : null);
		}
		PartyManager.Get().UpdateSpectatorJoinInfo(joinInfo);
	}

	private void UpdateSpectatorPartyServerInfo()
	{
		if (m_spectatorPartyIdMain == null)
		{
			return;
		}
		if (!ShouldBePartyLeader(m_spectatorPartyIdMain))
		{
			if (BnetParty.IsLeader(m_spectatorPartyIdMain))
			{
				BattleNet.ClearPartyAttribute(m_spectatorPartyIdMain, "WTCG.Party.ServerInfo");
			}
			return;
		}
		BattleNet.GetPartyAttribute(m_spectatorPartyIdMain, "WTCG.Party.ServerInfo", out byte[] currBlobValue);
		GameServerInfo serverInfo = Network.Get().GetLastGameServerJoined();
		if (IsGameOver || !SceneMgr.Get().IsInGame() || !Network.Get().IsConnectedToGameServer() || serverInfo == null || string.IsNullOrEmpty(serverInfo.Address))
		{
			if (currBlobValue != null)
			{
				BattleNet.ClearPartyAttribute(m_spectatorPartyIdMain, "WTCG.Party.ServerInfo");
			}
			return;
		}
		byte[] blobVal = ProtobufUtil.ToByteArray(new PartyServerInfo
		{
			ServerIpAddress = serverInfo.Address,
			ServerPort = serverInfo.Port,
			GameHandle = (int)serverInfo.GameHandle,
			SecretKey = (serverInfo.SpectatorPassword ?? ""),
			GameType = GameMgr.Get().GetGameType(),
			FormatType = GameMgr.Get().GetFormatType(),
			MissionId = GameMgr.Get().GetMissionId()
		});
		if (!GeneralUtils.AreArraysEqual(blobVal, currBlobValue))
		{
			BattleNet.SetPartyAttributes(m_spectatorPartyIdMain, BnetAttribute.CreateAttribute("WTCG.Party.ServerInfo", blobVal));
		}
	}

	private bool ShouldBePartyLeader(BnetPartyId partyId)
	{
		if (GameMgr.Get().IsSpectator())
		{
			return false;
		}
		if (m_spectateeFriendlySide != null || m_spectateeOpposingSide != null)
		{
			return false;
		}
		BnetGameAccountId partyCreator = GetPartyCreator(partyId);
		if (partyCreator == null)
		{
			return false;
		}
		if (partyCreator != BnetPresenceMgr.Get().GetMyGameAccountId())
		{
			return false;
		}
		return true;
	}

	private BnetGameAccountId GetPartyCreator(BnetPartyId partyId)
	{
		if (partyId == null)
		{
			return null;
		}
		BnetGameAccountId partyCreator = null;
		if (m_knownPartyCreatorIds.TryGetValue(partyId, out partyCreator) && partyCreator != null)
		{
			return partyCreator;
		}
		if (!BattleNet.GetPartyAttribute(partyId, "WTCG.Party.Creator", out byte[] blobVal))
		{
			return null;
		}
		partyCreator = BnetGameAccountId.CreateFromNet(ProtobufUtil.ParseFrom<BnetId>(blobVal));
		if (partyCreator.IsValid())
		{
			m_knownPartyCreatorIds[partyId] = partyCreator;
		}
		return partyCreator;
	}

	private bool CreatePartyIfNecessary()
	{
		if (!Network.ShouldBeConnectedToAurora())
		{
			return false;
		}
		if (m_spectatorPartyIdMain != null)
		{
			if (GetPartyCreator(m_spectatorPartyIdMain) != null && !ShouldBePartyLeader(m_spectatorPartyIdMain))
			{
				return false;
			}
			PartyInfo[] parties = BnetParty.GetJoinedParties();
			PartyInfo spectatorParty = parties.FirstOrDefault((PartyInfo i) => i.Id == m_spectatorPartyIdMain && i.Type == PartyType.SPECTATOR_PARTY);
			if (spectatorParty == null)
			{
				LogInfoParty("CreatePartyIfNecessary stored PartyId={0} is not in joined party list: {1}", m_spectatorPartyIdMain, string.Join(", ", parties.Select((PartyInfo i) => i.ToString()).ToArray()));
				m_spectatorPartyIdMain = null;
				UpdateSpectatorPresence();
			}
			spectatorParty = parties.FirstOrDefault((PartyInfo i) => i.Type == PartyType.SPECTATOR_PARTY);
			if (spectatorParty != null && m_spectatorPartyIdMain != spectatorParty.Id)
			{
				LogInfoParty("CreatePartyIfNecessary repairing mismatching PartyIds current={0} new={1}", m_spectatorPartyIdMain, spectatorParty.Id);
				m_spectatorPartyIdMain = spectatorParty.Id;
				UpdateSpectatorPresence();
			}
			if (m_spectatorPartyIdMain != null)
			{
				return false;
			}
		}
		if (GetCountSpectatingMe() <= 0)
		{
			return false;
		}
		byte[] creatorBlob = ProtobufUtil.ToByteArray(BnetUtils.CreatePegasusBnetId(BnetPresenceMgr.Get().GetMyGameAccountId()));
		BnetParty.CreateParty(PartyType.SPECTATOR_PARTY, ChannelApi.PartyPrivacyLevel.OpenInvitation, creatorBlob, null);
		return true;
	}

	private void ReinviteKnownSpectatorsNotInParty()
	{
		if (m_spectatorPartyIdMain == null || !ShouldBePartyLeader(m_spectatorPartyIdMain))
		{
			return;
		}
		BnetParty.PartyMember[] members = BnetParty.GetMembers(m_spectatorPartyIdMain);
		foreach (BnetGameAccountId knownSpectator in m_gameServerKnownSpectators)
		{
			if (members.FirstOrDefault((BnetParty.PartyMember m) => m.GameAccountId == knownSpectator) == null)
			{
				BnetParty.SendInvite(m_spectatorPartyIdMain, knownSpectator, isReservation: false);
			}
		}
	}

	private void LeaveParty(BnetPartyId partyId, bool dissolve)
	{
		if (!(partyId == null))
		{
			if (m_leavePartyIdsRequested == null)
			{
				m_leavePartyIdsRequested = new HashSet<BnetPartyId>();
			}
			m_leavePartyIdsRequested.Add(partyId);
			if (dissolve)
			{
				BnetParty.DissolveParty(partyId);
			}
			else
			{
				BnetParty.Leave(partyId);
			}
		}
	}

	public void LeaveGameScene()
	{
		if (EndGameScreen.Get() != null)
		{
			EndGameScreen.Get().m_hitbox.TriggerPress();
			EndGameScreen.Get().m_hitbox.TriggerRelease();
		}
		else if (!HearthstoneApplication.Get().IsResetting())
		{
			SceneMgr.Mode nextMode = GameMgr.Get().GetPostGameSceneMode();
			SceneMgr.Get().SetNextMode(nextMode);
		}
	}

	private IEnumerator WaitForPresenceThenToast(BnetGameAccountId gameAccountId, SocialToastMgr.TOAST_TYPE toastType)
	{
		float timeStarted = Time.time;
		float timeElapsed = Time.time - timeStarted;
		while (timeElapsed < 30f && !BnetUtils.HasPlayerBestNamePresence(gameAccountId))
		{
			yield return null;
			timeElapsed = Time.time - timeStarted;
		}
		if (SocialToastMgr.Get() != null)
		{
			string playerName = BnetUtils.GetPlayerBestName(gameAccountId);
			SocialToastMgr.Get().AddToast(UserAttentionBlocker.NONE, playerName, toastType);
		}
	}

	private SpectatorManager()
	{
	}

	private static SpectatorManager CreateInstance()
	{
		s_instance = new SpectatorManager();
		HearthstoneApplication.Get().WillReset += s_instance.WillReset;
		GameMgr.Get().RegisterFindGameEvent(s_instance.OnFindGameEvent);
		SceneMgr.Get().RegisterSceneUnloadedEvent(s_instance.OnSceneUnloaded);
		GameState.RegisterGameStateInitializedListener(s_instance.GameState_InitializedEvent);
		Options.Get().RegisterChangedListener(Option.SPECTATOR_OPEN_JOIN, s_instance.OnSpectatorOpenJoinOptionChanged);
		BnetPresenceMgr.Get().OnGameAccountPresenceChange += s_instance.Presence_OnGameAccountPresenceChange;
		BnetFriendMgr.Get().AddChangeListener(s_instance.BnetFriendMgr_OnFriendsChanged);
		EndGameScreen.OnTwoScoopsShown = (EndGameScreen.OnTwoScoopsShownHandler)Delegate.Combine(EndGameScreen.OnTwoScoopsShown, new EndGameScreen.OnTwoScoopsShownHandler(s_instance.EndGameScreen_OnTwoScoopsShown));
		EndGameScreen.OnBackOutOfGameplay = (Action)Delegate.Combine(EndGameScreen.OnBackOutOfGameplay, new Action(s_instance.EndGameScreen_OnBackOutOfGameplay));
		BnetPresenceMgr.Get().AddPlayersChangedListener(s_instance.BnetPresenceMgr_OnPlayersChanged);
		Network.Get().OnDisconnectedFromBattleNet += s_instance.OnDisconnect;
		Network.Get().RegisterNetHandler(SpectatorNotify.PacketID.ID, s_instance.Network_OnSpectatorNotifyEvent);
		BnetParty.OnError += s_instance.BnetParty_OnError;
		BnetParty.OnJoined += s_instance.BnetParty_OnJoined;
		BnetParty.OnReceivedInvite += s_instance.BnetParty_OnReceivedInvite;
		BnetParty.OnSentInvite += s_instance.BnetParty_OnSentInvite;
		BnetParty.OnReceivedInviteRequest += s_instance.BnetParty_OnReceivedInviteRequest;
		BnetParty.OnMemberEvent += s_instance.BnetParty_OnMemberEvent;
		BnetParty.OnChatMessage += s_instance.BnetParty_OnChatMessage;
		BnetParty.RegisterAttributeChangedHandler("WTCG.Party.ServerInfo", s_instance.BnetParty_OnPartyAttributeChanged_ServerInfo);
		return s_instance;
	}
}
