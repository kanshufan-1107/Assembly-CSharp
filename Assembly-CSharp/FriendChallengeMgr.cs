using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.GameService.Protocol.V2.Client;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Logging;
using Hearthstone;
using Hearthstone.Core;
using PegasusShared;
using PegasusUtil;
using SpectatorProto;
using UnityEngine;

public class FriendChallengeMgr
{
	public enum ChallengeMethod
	{
		INVALID,
		FROM_FRIEND_LIST
	}

	public enum DeclineReason
	{
		None,
		UserDeclined,
		NoValidDeck,
		StandardNoValidDeck,
		TavernBrawlNoValidDeck,
		TavernBrawlNotUnlocked,
		UserIsBusy,
		NotSeenWild,
		BattlegroundsNoEarlyAccess,
		ClassicNoValidDeck,
		BattlegroundsTutorialNotComplete,
		MercsTutorialNotComplete,
		TwistNoValidDeck
	}

	public delegate void ChangedCallback(FriendChallengeEvent challengeEvent, BnetPlayer player, FriendlyChallengeData challengeData, object userData);

	private class ChangedListener : EventListener<ChangedCallback>
	{
		public void Fire(FriendChallengeEvent challengeEvent, BnetPlayer player, FriendlyChallengeData challengeData)
		{
			m_callback(challengeEvent, player, challengeData, m_userData);
		}
	}

	private static FriendChallengeMgr s_instance;

	private bool m_netCacheReady;

	private bool m_myPlayerReady;

	private FriendlyChallengeData m_data = new FriendlyChallengeData();

	private bool m_hasPreSelectedDeckOrHero;

	private long m_preSelectedDeckId;

	private long m_preSelectedHeroId;

	private ChallengeMethod m_challengeMethod;

	private List<ChangedListener> m_changedListeners = new List<ChangedListener>();

	private DialogBase m_challengeDialog;

	private bool m_hasSeenDeclinedReason;

	private bool m_canBeInvitedToGame;

	private bool m_canBeInvitedToBattlegrounds;

	private bool m_canBeInvitedToMercenaries;

	private bool m_updateMyAvailabilityCallbackScheduledThisFrame;

	public bool IsChallengeFriendlyDuel
	{
		get
		{
			if (!IsChallengeStandardDuel() && !IsChallengeWildDuel())
			{
				return IsChallengeClassicDuel();
			}
			return true;
		}
	}

	public static FriendChallengeMgr Get()
	{
		if (s_instance == null)
		{
			s_instance = new FriendChallengeMgr();
			HearthstoneApplication.Get().WillReset += s_instance.WillReset;
			AchieveManager.Get().RegisterAchievesUpdatedListener(s_instance.AchieveManager_OnAchievesUpdated);
			BnetParty.OnJoined += s_instance.BnetParty_OnJoined;
			BnetParty.OnReceivedInvite += s_instance.BnetParty_OnReceivedInvite;
			BnetParty.OnPartyAttributeChanged += s_instance.BnetParty_OnPartyAttributeChanged;
			BnetParty.OnMemberEvent += s_instance.BnetParty_OnMemberEvent;
			BnetParty.OnSentInvite += s_instance.BnetParty_OnSentInvite;
		}
		return s_instance;
	}

	public void OnLoggedIn()
	{
		NetCache.Get().RegisterFriendChallenge(OnNetCacheReady);
		SceneMgr.Get().RegisterSceneUnloadedEvent(OnSceneUnloaded);
		SceneMgr.Get().RegisterSceneLoadedEvent(OnSceneLoaded);
		BnetPresenceMgr.Get().AddPlayersChangedListener(OnPlayersChanged);
		BnetFriendMgr.Get().AddChangeListener(OnFriendsChanged);
		BnetNearbyPlayerMgr.Get().AddChangeListener(OnNearbyPlayersChanged);
		GameMgr.Get().RegisterFindGameEvent(OnFindGameEvent);
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
		LoginManager.Get().OnInitialClientStateReceived += OnReconnectLoginComplete;
		AddChangedListener(OnChallengeChanged);
		Network.Get().OnDisconnectedFromBattleNet += OnDisconnectedFromBattleNet;
		BnetPresenceMgr.Get().SetGameField(19u, BattleNet.GetVersion());
		BnetPresenceMgr.Get().SetGameField(20u, BattleNet.GetEnvironment());
	}

	private void BnetParty_OnJoined(OnlineEventType evt, PartyInfo party, LeaveReason? reason)
	{
		if (party.Type != PartyType.FRIENDLY_CHALLENGE)
		{
			return;
		}
		switch (evt)
		{
		case OnlineEventType.ADDED:
		{
			if (DidSendChallenge() && !BnetParty.IsLeader(party.Id))
			{
				BnetParty.DissolveParty(party.Id);
				break;
			}
			if (m_data.m_partyId != null && m_data.m_partyId != party.Id)
			{
				BnetParty.DissolveParty(party.Id);
				break;
			}
			m_data.m_partyId = party.Id;
			if (BattleNet.GetPartyAttribute(party.Id, "WTCG.Game.ScenarioId", out long scenarioId))
			{
				m_data.m_scenarioId = (int)scenarioId;
			}
			if (BattleNet.GetPartyAttribute(party.Id, "WTCG.Format.Type", out long formatType))
			{
				m_data.m_challengeFormatType = (FormatType)formatType;
			}
			else
			{
				m_data.m_challengeFormatType = FormatType.FT_UNKNOWN;
			}
			if (BattleNet.GetPartyAttribute(party.Id, "WTCG.Brawl.Type", out long brawlType))
			{
				if (brawlType >= 1 && brawlType < 3)
				{
					m_data.m_challengeBrawlType = (BrawlType)brawlType;
				}
			}
			else
			{
				m_data.m_challengeBrawlType = BrawlType.BRAWL_TYPE_UNKNOWN;
			}
			if (BattleNet.GetPartyAttribute(party.Id, "WTCG.Season.Id", out int seasonId))
			{
				m_data.m_seasonId = seasonId;
			}
			else
			{
				m_data.m_seasonId = 0;
			}
			if (BattleNet.GetPartyAttribute(party.Id, "WTCG.Brawl.LibraryItemId", out int brawlLibraryItemId))
			{
				m_data.m_brawlLibraryItemId = brawlLibraryItemId;
			}
			else
			{
				m_data.m_brawlLibraryItemId = 0;
			}
			string stateAttr = (DidSendChallenge() ? "s1" : "s2");
			BattleNet.SetPartyAttributes(party.Id, BnetAttribute.CreateAttribute(stateAttr, "wait"));
			m_data.m_challengerDeckShareState = "none";
			m_data.m_challengeeDeckShareState = "none";
			m_data.m_sharedDecks = null;
			if (DidSendChallenge())
			{
				BnetParty.SendInvite(party.Id, m_data.m_challengee.GetHearthstoneGameAccountId(), isReservation: true);
			}
			else
			{
				BattleNet.GetAllPartyAttributes(party.Id, out var attributes);
				Attribute[] array = attributes;
				foreach (Attribute attribute in array)
				{
					BnetParty_OnPartyAttributeChanged(party, attribute);
				}
			}
			if (m_data.m_challengerDeckId != 0L)
			{
				SelectDeck(m_data.m_challengerDeckId);
			}
			if (m_data.m_challengerHeroId != 0L)
			{
				SelectHero(m_data.m_challengerHeroId);
			}
			break;
		}
		case OnlineEventType.REMOVED:
			if (!BnetParty.GetJoinedParties().Any((PartyInfo i) => i.Type == PartyType.FRIENDLY_CHALLENGE))
			{
				m_data.m_scenarioId = 2;
			}
			if (party.Id == m_data.m_partyId)
			{
				string strReason = (reason.HasValue ? ((int)reason.Value).ToString() : "NO_SUPPLIED_REASON");
				PushPartyEvent(party.Id, "left", strReason);
			}
			break;
		}
	}

	private void BnetParty_OnReceivedInvite(OnlineEventType evt, PartyInfo party, ulong inviteId, BnetGameAccountId inviter, string inviterBattletag, BnetGameAccountId invitee, InviteRemoveReason? reason)
	{
		if (party.Type == PartyType.FRIENDLY_CHALLENGE && evt == OnlineEventType.ADDED)
		{
			if (!PartyManager.IsPartyTypeEnabledInGuardian(party.Type))
			{
				BnetParty.DeclineReceivedInvite(inviteId);
			}
			else if (BnetParty.IsInParty(m_data.m_partyId) || DidSendChallenge())
			{
				BnetParty.DeclineReceivedInvite(inviteId);
			}
			else if (!GameUtils.IsTraditionalTutorialComplete())
			{
				BnetParty.DeclineReceivedInvite(inviteId);
			}
			else
			{
				BnetParty.AcceptReceivedInvite(inviteId);
			}
		}
	}

	private void BnetParty_OnPartyAttributeChanged(PartyInfo party, Attribute attribute)
	{
		if (party.Type != PartyType.FRIENDLY_CHALLENGE || m_data.m_partyId != party.Id)
		{
			return;
		}
		switch (attribute.Name)
		{
		case "WTCG.Friendly.DeclineReason":
			BnetParty_OnPartyAttributeChanged_DeclineReason(party, attribute);
			break;
		case "error":
			BnetParty_OnPartyAttributeChanged_Error(party, attribute);
			break;
		case "d1":
			m_data.m_challengerDeckId = (attribute.Value.HasIntValue ? attribute.Value.IntValue : 0);
			m_data.m_challengerDeckOrHeroSelected = m_data.m_challengerDeckId > 0;
			break;
		case "d2":
			m_data.m_challengeeDeckId = (attribute.Value.HasIntValue ? attribute.Value.IntValue : 0);
			m_data.m_challengeeDeckOrHeroSelected = m_data.m_challengeeDeckId > 0;
			break;
		case "hero1":
			m_data.m_challengerHeroId = (attribute.Value.HasIntValue ? attribute.Value.IntValue : 0);
			m_data.m_challengerDeckOrHeroSelected = m_data.m_challengerHeroId > 0;
			break;
		case "hero2":
			m_data.m_challengeeHeroId = (attribute.Value.HasIntValue ? attribute.Value.IntValue : 0);
			m_data.m_challengeeDeckOrHeroSelected = m_data.m_challengeeHeroId > 0;
			break;
		case "randomHero1":
			m_data.m_challengerRandomHeroCardId = (attribute.Value.HasIntValue ? attribute.Value.IntValue : 0);
			break;
		case "randomHero2":
			m_data.m_challengeeRandomHeroCardId = (attribute.Value.HasIntValue ? attribute.Value.IntValue : 0);
			break;
		case "p1CardBack":
			m_data.m_challengerCardBackId = null;
			if (attribute.Value.HasIntValue)
			{
				m_data.m_challengerCardBackId = attribute.Value.IntValue;
			}
			break;
		case "p2CardBack":
			m_data.m_challengeeCardBackId = null;
			if (attribute.Value.HasIntValue)
			{
				m_data.m_challengeeCardBackId = attribute.Value.IntValue;
			}
			break;
		}
		BnetGameAccountId otherPlayerGameAccountId = null;
		if (DidSendChallenge())
		{
			if (m_data.m_challengee != null)
			{
				otherPlayerGameAccountId = m_data.m_challengee.GetHearthstoneGameAccountId();
			}
		}
		else if (m_data.m_challenger != null)
		{
			otherPlayerGameAccountId = m_data.m_challenger.GetHearthstoneGameAccountId();
		}
		if (otherPlayerGameAccountId == null)
		{
			BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
			BnetParty.PartyMember[] members = BnetParty.GetMembers(party.Id);
			foreach (BnetParty.PartyMember member in members)
			{
				if (member.GameAccountId != myGameAccountId)
				{
					otherPlayerGameAccountId = member.GameAccountId;
					break;
				}
			}
		}
		string stringValue = (attribute.Value.HasStringValue ? attribute.Value.StringValue : string.Empty);
		PushPartyEvent(party.Id, attribute.Name, stringValue, otherPlayerGameAccountId);
	}

	private void BnetParty_OnPartyAttributeChanged_DeclineReason(PartyInfo party, Attribute attribute)
	{
		if (party.Type != PartyType.FRIENDLY_CHALLENGE || !DidSendChallenge() || !attribute.Value.HasIntValue)
		{
			return;
		}
		DeclineReason reason = (DeclineReason)attribute.Value.IntValue;
		string msgKey = null;
		switch (reason)
		{
		case DeclineReason.UserIsBusy:
			msgKey = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_USER_IS_BUSY";
			break;
		case DeclineReason.NoValidDeck:
			msgKey = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_NO_DECK";
			break;
		case DeclineReason.StandardNoValidDeck:
			msgKey = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_NO_STANDARD_DECK";
			break;
		case DeclineReason.TavernBrawlNoValidDeck:
			msgKey = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_NO_TAVERN_BRAWL_DECK";
			break;
		case DeclineReason.TavernBrawlNotUnlocked:
			msgKey = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_TAVERN_BRAWL_LOCKED";
			break;
		case DeclineReason.NotSeenWild:
			msgKey = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_NOT_SEEN_WILD";
			break;
		case DeclineReason.BattlegroundsNoEarlyAccess:
			msgKey = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_NO_BATTLEGROUNDS_EARLY_ACCESS";
			break;
		case DeclineReason.ClassicNoValidDeck:
			msgKey = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_NO_CLASSIC_DECK";
			break;
		case DeclineReason.TwistNoValidDeck:
			msgKey = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_NO_TWIST_DECK";
			break;
		case DeclineReason.BattlegroundsTutorialNotComplete:
			msgKey = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_NO_BATTLEGROUNDS_TUTORIAL_COMPLETE";
			break;
		case DeclineReason.MercsTutorialNotComplete:
			msgKey = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_NO_MERCS_TUTORIAL_COMPLETE";
			break;
		}
		if (msgKey != null)
		{
			if (m_challengeDialog != null)
			{
				m_challengeDialog.Hide();
				m_challengeDialog = null;
			}
			m_hasSeenDeclinedReason = true;
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER"),
				m_text = GameStrings.Get(msgKey),
				m_responseDisplay = AlertPopup.ResponseDisplay.OK
			};
			DialogManager.Get().ShowPopup(info);
		}
	}

	private void BnetParty_OnPartyAttributeChanged_Error(PartyInfo party, Attribute attribute)
	{
		if (party.Type == PartyType.FRIENDLY_CHALLENGE)
		{
			if (DidReceiveChallenge() && attribute.Value.HasIntValue)
			{
				Log.Party.Print(Blizzard.T5.Logging.LogLevel.Error, "BnetParty_OnPartyAttributeChanged_Error - code={0}", attribute.Value.IntValue);
				BnetErrorInfo info = new BnetErrorInfo(BnetFeature.Games, BnetFeatureEvent.Games_OnCreated, (BattleNetErrors)attribute.Value.IntValue);
				GameMgr.Get().OnBnetError(info, null);
			}
			if (BnetParty.IsLeader(party.Id) && !BnetAttribute.IsNone(attribute))
			{
				BattleNet.ClearPartyAttribute(party.Id, attribute.Name);
			}
		}
	}

	private void BnetParty_OnMemberEvent(OnlineEventType evt, PartyInfo party, BnetGameAccountId memberId, bool isRolesUpdate, LeaveReason? reason)
	{
		if (party.Type == PartyType.FRIENDLY_CHALLENGE && evt == OnlineEventType.REMOVED && BnetParty.IsInParty(party.Id))
		{
			BnetParty.DissolveParty(party.Id);
		}
	}

	private void BnetParty_OnSentInvite(OnlineEventType evt, PartyInfo party, ulong inviteId, BnetGameAccountId inviter, BnetGameAccountId invitee, bool senderIsMyself, InviteRemoveReason? reason)
	{
		if (party.Type == PartyType.FRIENDLY_CHALLENGE && evt == OnlineEventType.REMOVED && reason == InviteRemoveReason.DECLINED)
		{
			DeclineFriendChallenge_Internal(party.Id);
			if (party.Id == m_data.m_partyId)
			{
				BnetPlayer challengee = m_data.m_challengee;
				FriendlyChallengeData challengeData = CleanUpChallengeData();
				FireChangedEvent(FriendChallengeEvent.OPPONENT_DECLINED_CHALLENGE, challengee, challengeData);
			}
		}
	}

	private void AchieveManager_OnAchievesUpdated(List<Achievement> updatedAchieves, List<Achievement> completedAchievements, object userData)
	{
		if (completedAchievements.Any((Achievement a) => a.IsFriendlyChallengeQuest))
		{
			if (SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
			{
				m_data.m_updatePartyQuestInfoOnGameplaySceneUnload = true;
			}
			else
			{
				UpdatePartyQuestInfo();
			}
		}
	}

	private void UpdatePartyQuestInfo()
	{
		if (!DidSendChallenge() || !BnetParty.IsInParty(m_data.m_partyId))
		{
			return;
		}
		byte[] blobValue = null;
		IEnumerable<Achievement> friendlyQuests = from q in AchieveManager.Get().GetActiveQuests()
			where q.IsFriendlyChallengeQuest
			select q;
		if (friendlyQuests.Any())
		{
			PartyQuestInfo partyQuestInfo = new PartyQuestInfo();
			partyQuestInfo.QuestIds.AddRange(friendlyQuests.Select((Achievement q) => q.ID));
			blobValue = ProtobufUtil.ToByteArray(partyQuestInfo);
		}
		BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("quests", blobValue));
	}

	public void OnStoreOpened()
	{
		UpdateMyAvailability();
	}

	public void OnStoreClosed()
	{
		UpdateMyAvailability();
	}

	public bool DidReceiveChallenge()
	{
		return m_data.DidReceiveChallenge;
	}

	public bool DidSendChallenge()
	{
		return m_data.DidSendChallenge;
	}

	public bool HasChallenge()
	{
		if (!DidSendChallenge())
		{
			return DidReceiveChallenge();
		}
		return true;
	}

	public bool DidChallengeeAccept()
	{
		return m_data.m_challengeeAccepted;
	}

	public bool AmIInGameState()
	{
		if (DidSendChallenge())
		{
			return m_data.m_challengerInGameState;
		}
		return m_data.m_challengeeInGameState;
	}

	public BnetPlayer GetOpponent(BnetPlayer player)
	{
		if (player == m_data.m_challenger)
		{
			return m_data.m_challengee;
		}
		if (player == m_data.m_challengee)
		{
			return m_data.m_challenger;
		}
		return null;
	}

	public BnetPlayer GetMyOpponent()
	{
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		return GetOpponent(myself);
	}

	public bool CanChallenge(BnetPlayer player)
	{
		if (player == null)
		{
			return false;
		}
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (player == myself)
		{
			return false;
		}
		if (!AmIAvailable())
		{
			return false;
		}
		if (!IsOpponentAvailable(player))
		{
			return false;
		}
		if (PartyManager.Get().IsPlayerInAnyParty(player.GetBestGameAccountId()))
		{
			return false;
		}
		if (!BnetFriendMgr.Get().IsFriend(player) && !BnetNearbyPlayerMgr.Get().IsNearbyStranger(player))
		{
			return false;
		}
		return true;
	}

	public bool CanShowFriendlyChallenge(BnetPlayer player)
	{
		if (player == null)
		{
			return false;
		}
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (player == myself)
		{
			return false;
		}
		if (PopupDisplayManager.Get().IsShowing)
		{
			return false;
		}
		if (SpectatorManager.Get().IsSpectatingOrWatching)
		{
			return false;
		}
		if (PartyManager.Get().IsInParty())
		{
			return false;
		}
		if (!myself.GetHearthstoneGameAccount().CanBeInvitedToGame())
		{
			return false;
		}
		if (!IsOpponentAvailable(player))
		{
			return false;
		}
		if (PartyManager.Get().IsPlayerInAnyParty(player.GetBestGameAccountId()))
		{
			return false;
		}
		if (!BnetFriendMgr.Get().IsFriend(player) && !BnetNearbyPlayerMgr.Get().IsNearbyStranger(player))
		{
			return false;
		}
		return true;
	}

	public bool IsHearthstoneFriendlyChallengeAvailable(BnetPlayer player)
	{
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (player == myself)
		{
			return false;
		}
		if (myself.IsAppearingOffline())
		{
			return false;
		}
		if (!IsOpponentAvailable(player))
		{
			return false;
		}
		if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.Friendly)
		{
			return false;
		}
		if (PartyManager.Get().IsPlayerInAnyParty(player.GetBestGameAccountId()))
		{
			return false;
		}
		if (!GameUtils.IsTraditionalTutorialComplete())
		{
			return false;
		}
		if (player.GetHearthstoneGameAccount().GetTutorialBeaten() < 1)
		{
			return false;
		}
		return true;
	}

	public bool IsBattlegroundsFriendlyChallengeAvailable(BnetPlayer player)
	{
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (player == myself)
		{
			return false;
		}
		if (myself.IsAppearingOffline())
		{
			return false;
		}
		if (!IsOpponentAvailable(player))
		{
			return false;
		}
		if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.BattlegroundsFriendlyChallenge)
		{
			return false;
		}
		if (PartyManager.Get().IsPlayerInAnyParty(player.GetBestGameAccountId()))
		{
			return false;
		}
		if (!GameUtils.IsBattleGroundsTutorialComplete())
		{
			return false;
		}
		if (!AllowBGInviteWhileInNPPGEnabled() && !player.GetHearthstoneGameAccount().GetBattlegroundsTutorialComplete())
		{
			return false;
		}
		return true;
	}

	public bool AllowBGInviteWhileInNPPGEnabled()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().AllowBGInviteWhileInNPPG;
	}

	public bool IsMercenariesFriendlyChallengeAvailable(BnetPlayer player)
	{
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (player == myself)
		{
			return false;
		}
		if (myself.IsAppearingOffline())
		{
			return false;
		}
		if (!IsOpponentAvailable(player))
		{
			return false;
		}
		if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.MercenariesFriendly)
		{
			return false;
		}
		if (PartyManager.Get().IsPlayerInAnyParty(player.GetBestGameAccountId()))
		{
			return false;
		}
		if (!GameUtils.IsMercenariesPrologueBountyComplete(NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>()) || !GameUtils.IsMercenariesVillageTutorialComplete())
		{
			return false;
		}
		if (!player.GetHearthstoneGameAccount().GetMercenariesTutorialComplete())
		{
			return false;
		}
		return true;
	}

	public bool AmIAvailable()
	{
		if (!m_netCacheReady)
		{
			return false;
		}
		if (!m_myPlayerReady)
		{
			return false;
		}
		if (SpectatorManager.Get().IsSpectatingOrWatching)
		{
			return false;
		}
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (myself == null)
		{
			return false;
		}
		BnetGameAccount myHsAccount = myself.GetHearthstoneGameAccount();
		if (myHsAccount == null)
		{
			return false;
		}
		if (!myself.IsOnline() || myself.IsAppearingOffline())
		{
			return false;
		}
		if (!Network.IsLoggedIn())
		{
			return false;
		}
		if (PopupDisplayManager.Get().IsShowing)
		{
			return false;
		}
		if (PartyManager.Get().IsInParty())
		{
			return false;
		}
		return myHsAccount.CanBeInvitedToGame();
	}

	public bool IsOpponentAvailable(BnetPlayer player)
	{
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		BnetGameAccount hsAccount = player.GetHearthstoneGameAccount();
		if (hsAccount == null)
		{
			return false;
		}
		if (!hsAccount.IsOnline())
		{
			return false;
		}
		if (!hsAccount.CanBeInvitedToGame())
		{
			return false;
		}
		if (HearthstoneApplication.IsPublic())
		{
			BnetGameAccount myHsAccount = myself.GetHearthstoneGameAccount();
			if (string.Compare(hsAccount.GetClientVersion(), myHsAccount.GetClientVersion()) != 0)
			{
				return false;
			}
			if (string.Compare(hsAccount.GetClientEnv(), myHsAccount.GetClientEnv()) != 0)
			{
				return false;
			}
		}
		return true;
	}

	public bool DidISelectDeckOrHero()
	{
		if (DidSendChallenge())
		{
			return m_data.m_challengerDeckOrHeroSelected;
		}
		if (DidReceiveChallenge())
		{
			return m_data.m_challengeeDeckOrHeroSelected;
		}
		return true;
	}

	public bool DidOpponentSelectDeckOrHero()
	{
		if (DidSendChallenge())
		{
			return m_data.m_challengeeDeckOrHeroSelected;
		}
		if (DidReceiveChallenge())
		{
			return m_data.m_challengerDeckOrHeroSelected;
		}
		return true;
	}

	public static void ShowChallengerNeedsToCreateTavernBrawlDeckAlert()
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER"),
			m_text = GameStrings.Format("GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGER_NO_TAVERN_BRAWL_DECK"),
			m_showAlertIcon = true,
			m_responseDisplay = AlertPopup.ResponseDisplay.OK
		};
		DialogManager.Get().ShowPopup(info);
	}

	public void SendChallenge(BnetPlayer player, FormatType formatType, bool enableDeckShare)
	{
		if (CanChallenge(player))
		{
			SendChallenge_Internal(player, formatType, BrawlType.BRAWL_TYPE_UNKNOWN, enableDeckShare, 0, 0, isBaconGame: false);
		}
	}

	public void SendTavernBrawlChallenge(BnetPlayer player, BrawlType brawlType, int seasonId, int brawlLibraryItemId)
	{
		if (CanChallenge(player))
		{
			TavernBrawlManager.Get().EnsureAllDataReady(brawlType, delegate
			{
				TavernBrawl_SendChallenge_OnEnsureServerDataReady(player, brawlType, seasonId, brawlLibraryItemId);
			});
		}
	}

	private void TavernBrawl_SendChallenge_OnEnsureServerDataReady(BnetPlayer player, BrawlType brawlType, int seasonId, int brawlLibraryItemId)
	{
		TavernBrawlManager tb = TavernBrawlManager.Get();
		if (CanChallenge(player) && tb.IsTavernBrawlActive(brawlType) && !HasChallenge())
		{
			if (!tb.CanChallengeToTavernBrawl(brawlType))
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
				{
					m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER"),
					m_text = GameStrings.Format("GLOBAL_FRIENDLIST_CHALLENGE_TOOLTIP_TAVERN_BRAWL_NOT_CHALLENGEABLE"),
					m_showAlertIcon = true,
					m_responseDisplay = AlertPopup.ResponseDisplay.OK
				};
				DialogManager.Get().ShowPopup(info);
			}
			else if (tb.GetMission(brawlType).canCreateDeck && !tb.HasValidDeck(brawlType))
			{
				ShowChallengerNeedsToCreateTavernBrawlDeckAlert();
			}
			else
			{
				SendChallenge_Internal(player, FormatType.FT_UNKNOWN, brawlType, enableDeckShare: false, seasonId, brawlLibraryItemId, isBaconGame: false);
			}
		}
	}

	private void SendChallenge_Internal(BnetPlayer player, FormatType formatType, BrawlType brawlType, bool enableDeckShare, int seasonId, int brawlLibraryItemId, bool isBaconGame)
	{
		if (m_data.m_partyId != null)
		{
			BnetParty.DissolveParty(m_data.m_partyId);
		}
		CleanUpChallengeData();
		if (m_hasPreSelectedDeckOrHero)
		{
			m_data.m_challengerDeckId = m_preSelectedDeckId;
			m_data.m_challengerHeroId = m_preSelectedHeroId;
			m_data.m_challengerDeckOrHeroSelected = m_hasPreSelectedDeckOrHero;
		}
		m_data.m_challenger = BnetPresenceMgr.Get().GetMyPlayer();
		m_data.m_challengerId = m_data.m_challenger.GetHearthstoneGameAccount().GetId();
		m_data.m_challengee = player;
		m_hasSeenDeclinedReason = false;
		m_data.m_scenarioId = 2;
		m_data.m_seasonId = seasonId;
		m_data.m_brawlLibraryItemId = brawlLibraryItemId;
		m_data.m_challengeBrawlType = brawlType;
		m_data.m_challengeFormatType = formatType;
		if (isBaconGame)
		{
			m_data.m_scenarioId = 3459;
		}
		else if (IsChallengeTavernBrawl())
		{
			TavernBrawlManager.Get().CurrentBrawlType = m_data.m_challengeBrawlType;
			TavernBrawlMission mission = TavernBrawlManager.Get().GetMission(brawlType);
			mission.SetSelectedBrawlLibraryItemId(brawlLibraryItemId);
			m_data.m_scenarioId = mission.missionId;
			m_data.m_challengeFormatType = mission.formatType;
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.TAVERN_BRAWL_FRIENDLY_WAITING);
		}
		else if (formatType == FormatType.FT_TWIST)
		{
			RankedPlaySeason twistSeason = RankMgr.Get()?.GetCurrentTwistSeason();
			if (twistSeason != null)
			{
				ScenarioDbfRecord scenarioRecord = twistSeason.GetScenario();
				if (scenarioRecord != null)
				{
					m_data.m_scenarioId = scenarioRecord.ID;
				}
			}
		}
		List<Attribute> partyAttributes = BnetAttribute.CreateAttributeCollection(BnetAttribute.CreateAttribute("WTCG.Game.ScenarioId", m_data.m_scenarioId), BnetAttribute.CreateAttribute("WTCG.Format.Type", (long)m_data.m_challengeFormatType), BnetAttribute.CreateAttribute("WTCG.Season.Id", (long)m_data.m_seasonId));
		if (IsChallengeTavernBrawl())
		{
			partyAttributes.Add(BnetAttribute.CreateAttribute("WTCG.Brawl.Type", (long)m_data.m_challengeBrawlType));
			partyAttributes.Add(BnetAttribute.CreateAttribute("WTCG.Brawl.LibraryItemId", (long)m_data.m_brawlLibraryItemId));
		}
		IEnumerable<Achievement> friendlyQuests = from q in AchieveManager.Get().GetActiveQuests()
			where q.IsFriendlyChallengeQuest
			select q;
		if (friendlyQuests.Any())
		{
			PartyQuestInfo partyQuestInfo = new PartyQuestInfo();
			partyQuestInfo.QuestIds.AddRange(friendlyQuests.Select((Achievement q) => q.ID));
			byte[] buffer = ProtobufUtil.ToByteArray(partyQuestInfo);
			partyAttributes.Add(BnetAttribute.CreateAttribute("quests", buffer));
		}
		if (m_data.m_challengerDeckId != 0L)
		{
			partyAttributes.Add(BnetAttribute.CreateAttribute("d1", m_data.m_challengerDeckId));
			partyAttributes.Add(BnetAttribute.CreateAttribute("hero1", m_data.m_challengerHeroId));
		}
		if (m_data.m_challengerDeckOrHeroSelected)
		{
			partyAttributes.Add(BnetAttribute.CreateAttribute("s1", "ready"));
		}
		string deckShareEnabledAttrValue = (enableDeckShare ? "deckShareEnabled" : "deckShareDisabled");
		partyAttributes.Add(BnetAttribute.CreateAttribute("isDeckShareEnabled", deckShareEnabledAttrValue));
		partyAttributes.Add(BnetAttribute.CreateAttribute("p1DeckShareState", "none"));
		partyAttributes.Add(BnetAttribute.CreateAttribute("p2DeckShareState", "none"));
		BnetParty.CreateParty(PartyType.FRIENDLY_CHALLENGE, ChannelApi.PartyPrivacyLevel.OpenInvitation, null, partyAttributes);
		UpdateMyAvailability();
		FireChangedEvent(FriendChallengeEvent.I_SENT_CHALLENGE, player);
	}

	public void CancelChallenge()
	{
		if (HasChallenge())
		{
			if (DidSendChallenge())
			{
				RescindChallenge();
			}
			else if (DidReceiveChallenge())
			{
				DeclineChallenge();
			}
		}
	}

	public void AcceptChallenge()
	{
		if (DidReceiveChallenge())
		{
			m_data.m_challengeeAccepted = true;
			string stateAttr = (DidSendChallenge() ? "s1" : "s2");
			BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute(stateAttr, "deck"));
			FireChangedEvent(FriendChallengeEvent.I_ACCEPTED_CHALLENGE, m_data.m_challenger);
		}
	}

	public void DeclineChallenge()
	{
		if (DidReceiveChallenge())
		{
			RevertTavernBrawlPresenceStatus();
			DeclineFriendChallenge_Internal(m_data.m_partyId);
			BnetPlayer challenger = m_data.m_challenger;
			FriendlyChallengeData challengeData = CleanUpChallengeData();
			FireChangedEvent(FriendChallengeEvent.I_DECLINED_CHALLENGE, challenger, challengeData);
		}
	}

	private void DeclineFriendChallenge_Internal(BnetPartyId partyId)
	{
		if (BnetParty.IsInParty(partyId))
		{
			BnetParty.DissolveParty(partyId);
		}
	}

	public void QueueCanceled()
	{
		BnetPlayer otherPlayer;
		if (DidReceiveChallenge())
		{
			otherPlayer = m_data.m_challenger;
		}
		else
		{
			if (!DidSendChallenge())
			{
				return;
			}
			otherPlayer = m_data.m_challengee;
		}
		FriendlyChallengeData challengeData = CleanUpChallengeData();
		FireChangedEvent(FriendChallengeEvent.QUEUE_CANCELED, otherPlayer, challengeData);
	}

	private void PushPartyEvent(BnetPartyId partyId, string type, string data, BnetGameAccountId otherPlayerGameAccountId = null)
	{
		if (otherPlayerGameAccountId == null)
		{
			otherPlayerGameAccountId = (DidSendChallenge() ? m_data.m_challenger : m_data.m_challengee)?.GetHearthstoneGameAccountId();
		}
		PartyEvent partyEvent = default(PartyEvent);
		partyEvent.partyId = partyId;
		partyEvent.eventName = type;
		partyEvent.eventData = data;
		partyEvent.otherMemberId = otherPlayerGameAccountId;
		OnPartyUpdate(new PartyEvent[1] { partyEvent });
	}

	public void RescindChallenge()
	{
		if (DidSendChallenge())
		{
			RevertTavernBrawlPresenceStatus();
			if (BnetParty.IsInParty(m_data.m_partyId))
			{
				BnetParty.DissolveParty(m_data.m_partyId);
			}
			BnetPlayer challengee = m_data.m_challengee;
			FriendlyChallengeData challengeData = CleanUpChallengeData();
			FireChangedEvent(FriendChallengeEvent.I_RESCINDED_CHALLENGE, challengee, challengeData);
		}
	}

	public bool IsChallengeStandardDuel()
	{
		if (!HasChallenge())
		{
			return false;
		}
		if (!IsChallengeTavernBrawl())
		{
			return m_data.m_challengeFormatType == FormatType.FT_STANDARD;
		}
		return false;
	}

	public bool IsChallengeWildDuel()
	{
		if (!HasChallenge())
		{
			return false;
		}
		if (!IsChallengeTavernBrawl())
		{
			return m_data.m_challengeFormatType == FormatType.FT_WILD;
		}
		return false;
	}

	public bool IsChallengeClassicDuel()
	{
		if (!HasChallenge())
		{
			return false;
		}
		if (!IsChallengeTavernBrawl())
		{
			return m_data.m_challengeFormatType == FormatType.FT_CLASSIC;
		}
		return false;
	}

	public bool IsChallengeTwistDuel()
	{
		if (!HasChallenge())
		{
			return false;
		}
		if (!IsChallengeTavernBrawl())
		{
			return m_data.m_challengeFormatType == FormatType.FT_TWIST;
		}
		return false;
	}

	public bool IsChallengeTavernBrawl()
	{
		if (!HasChallenge())
		{
			return false;
		}
		return m_data.m_challengeBrawlType != BrawlType.BRAWL_TYPE_UNKNOWN;
	}

	public bool IsChallengeBacon()
	{
		if (!HasChallenge())
		{
			return false;
		}
		return m_data.m_scenarioId == 3459;
	}

	public bool IsChallengeMercenaries()
	{
		if (!HasChallenge())
		{
			return false;
		}
		return m_data.m_scenarioId == 3743;
	}

	public BrawlType GetChallengeBrawlType()
	{
		if (!HasChallenge())
		{
			return BrawlType.BRAWL_TYPE_UNKNOWN;
		}
		return m_data.m_challengeBrawlType;
	}

	public bool IsDeckShareEnabled()
	{
		if (!HasChallenge())
		{
			return false;
		}
		if (BattleNet.GetPartyAttribute(m_data.m_partyId, "isDeckShareEnabled", out string attribute))
		{
			return attribute == "deckShareEnabled";
		}
		return false;
	}

	public void RequestDeckShare()
	{
		string player2DeckShare;
		if (DidSendChallenge())
		{
			if (BattleNet.GetPartyAttribute(m_data.m_partyId, "p1DeckShareState", out string player1DeckShare))
			{
				if (player1DeckShare == "sharingUnused")
				{
					BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p1DeckShareState", "sharing"));
				}
				else if (player1DeckShare == "none")
				{
					BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p1DeckShareState", "requested"));
				}
			}
		}
		else if (DidReceiveChallenge() && BattleNet.GetPartyAttribute(m_data.m_partyId, "p2DeckShareState", out player2DeckShare))
		{
			if (player2DeckShare == "sharingUnused")
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p2DeckShareState", "sharing"));
			}
			else if (player2DeckShare == "none")
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p2DeckShareState", "requested"));
			}
		}
	}

	public void EndDeckShare()
	{
		string player2DeckShare;
		if (DidSendChallenge())
		{
			if (BattleNet.GetPartyAttribute(m_data.m_partyId, "p1DeckShareState", out string player1DeckShare) && player1DeckShare == "sharing")
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p1DeckShareState", "sharingUnused"));
			}
		}
		else if (DidReceiveChallenge() && BattleNet.GetPartyAttribute(m_data.m_partyId, "p2DeckShareState", out player2DeckShare) && player2DeckShare == "sharing")
		{
			BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p2DeckShareState", "sharingUnused"));
		}
	}

	private void ShareDecks_InternalParty()
	{
		List<CollectionDeck> collectionDecks = CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK);
		byte[] blobValue = SerializeSharedDecks(collectionDecks);
		if (blobValue == null)
		{
			Log.Party.PrintError("{0}.ShareDecks_InternalParty(): Unable to Serialize decks!.", this);
			if (DidSendChallenge())
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p2DeckShareState", "error"));
			}
			else if (DidReceiveChallenge())
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p1DeckShareState", "error"));
			}
		}
		else if (DidSendChallenge())
		{
			BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p1DeckShareDecks", blobValue));
		}
		else if (DidReceiveChallenge())
		{
			BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p2DeckShareDecks", blobValue));
		}
	}

	public List<CollectionDeck> GetSharedDecks()
	{
		if (m_data.m_sharedDecks != null)
		{
			return new List<CollectionDeck>(m_data.m_sharedDecks);
		}
		byte[] blob = null;
		if (DidSendChallenge() && (m_data.m_challengerDeckShareState == "sharing" || m_data.m_challengerDeckShareState == "sharingUnused"))
		{
			BattleNet.GetPartyAttribute(m_data.m_partyId, "p2DeckShareDecks", out blob);
		}
		else if (DidReceiveChallenge() && (m_data.m_challengeeDeckShareState == "sharing" || m_data.m_challengeeDeckShareState == "sharingUnused"))
		{
			BattleNet.GetPartyAttribute(m_data.m_partyId, "p1DeckShareDecks", out blob);
		}
		if (blob == null)
		{
			return null;
		}
		return DeserializeSharedDecks(blob);
	}

	private byte[] SerializeSharedDecks(List<CollectionDeck> collectionDecks)
	{
		if (collectionDecks == null || collectionDecks.Count <= 0)
		{
			return null;
		}
		DeckList deckList = new DeckList();
		FormatType currentFormatType = Options.GetFormatType();
		foreach (CollectionDeck collectionDeck in collectionDecks)
		{
			if (collectionDeck.IsValidForRuleset && collectionDeck.IsValidForFormat(currentFormatType))
			{
				ulong validity = 0uL;
				if (collectionDeck.NeedsName)
				{
					validity |= 0x200;
				}
				if (currentFormatType == FormatType.FT_STANDARD)
				{
					validity |= 0x80;
				}
				if (collectionDeck.Locked)
				{
					validity |= 0x400;
				}
				DeckInfo deckInfo = new DeckInfo
				{
					Id = collectionDeck.ID,
					Name = collectionDeck.Name,
					Hero = GameUtils.TranslateCardIdToDbId(collectionDeck.HeroCardID),
					DeckType = collectionDeck.Type,
					CardBack = collectionDeck.CardBackID.GetValueOrDefault(),
					HeroOverride = collectionDeck.HeroOverridden,
					SeasonId = collectionDeck.SeasonId,
					BrawlLibraryItemId = collectionDeck.BrawlLibraryItemId,
					SortOrder = collectionDeck.SortOrder,
					FormatType = collectionDeck.FormatType,
					SourceType = collectionDeck.SourceType,
					Validity = validity,
					Rune1 = collectionDeck.GetRuneAtIndex(0),
					Rune2 = collectionDeck.GetRuneAtIndex(1),
					Rune3 = collectionDeck.GetRuneAtIndex(2)
				};
				if (collectionDeck.HasUIHeroOverride())
				{
					deckInfo.UiHeroOverride = GameUtils.TranslateCardIdToDbId(collectionDeck.UIHeroOverrideCardID);
					deckInfo.UiHeroOverridePremium = (int)collectionDeck.UIHeroOverridePremium;
				}
				deckList.Decks.Add(deckInfo);
			}
		}
		return ProtobufUtil.ToByteArray(deckList);
	}

	private List<CollectionDeck> DeserializeSharedDecks(byte[] blob)
	{
		if (blob == null)
		{
			return null;
		}
		try
		{
			DeckList deckList = ProtobufUtil.ParseFrom<DeckList>(blob);
			m_data.m_sharedDecks = new List<CollectionDeck>();
			foreach (DeckInfo deckInfo in deckList.Decks)
			{
				CollectionDeck collectionDeck = new CollectionDeck
				{
					ID = deckInfo.Id,
					Name = deckInfo.Name,
					HeroCardID = GameUtils.TranslateDbIdToCardId(deckInfo.Hero),
					Type = deckInfo.DeckType,
					CardBackID = deckInfo.CardBack,
					HeroOverridden = false,
					RandomHeroUseFavorite = true,
					SeasonId = deckInfo.SeasonId,
					BrawlLibraryItemId = deckInfo.BrawlLibraryItemId,
					NeedsName = Network.DeckNeedsName(deckInfo.Validity),
					SortOrder = (deckInfo.HasSortOrder ? deckInfo.SortOrder : deckInfo.Id),
					FormatType = deckInfo.FormatType,
					SourceType = (deckInfo.HasSourceType ? deckInfo.SourceType : DeckSourceType.DECK_SOURCE_TYPE_UNKNOWN),
					Locked = Network.AreDeckFlagsLocked(deckInfo.Validity),
					IsShared = true
				};
				collectionDeck.SetRuneOrder(deckInfo.Rune1, deckInfo.Rune2, deckInfo.Rune3);
				if (deckInfo.HasUiHeroOverride)
				{
					collectionDeck.UIHeroOverrideCardID = GameUtils.TranslateDbIdToCardId(deckInfo.UiHeroOverride);
					collectionDeck.UIHeroOverridePremium = (TAG_PREMIUM)deckInfo.UiHeroOverridePremium;
				}
				m_data.m_sharedDecks.Add(collectionDeck);
			}
		}
		catch
		{
			Log.Party.PrintError("{0}.ShareDecks_InternalParty(): Unable to Deserialize decks!.", this);
			m_data.m_sharedDecks = null;
		}
		return m_data.m_sharedDecks;
	}

	public bool HasOpponentSharedDecks()
	{
		return GetSharedDecks() != null;
	}

	public bool ShouldUseSharedDecks()
	{
		if (!HasOpponentSharedDecks())
		{
			return false;
		}
		if (DidSendChallenge() && m_data.m_challengerDeckShareState != "sharing")
		{
			return false;
		}
		if (DidReceiveChallenge() && m_data.m_challengeeDeckShareState != "sharing")
		{
			return false;
		}
		return true;
	}

	private void OnFriendChallengeDeckShareRequestDialogWaitingResponse(AlertPopup.Response response, object userData)
	{
		if (response != AlertPopup.Response.CANCEL)
		{
			return;
		}
		string player2DeckShareState;
		if (DidSendChallenge())
		{
			if (BattleNet.GetPartyAttribute(m_data.m_partyId, "p1DeckShareState", out string player1DeckShareState) && player1DeckShareState == "requested")
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p1DeckShareState", "none"));
			}
		}
		else if (DidReceiveChallenge() && BattleNet.GetPartyAttribute(m_data.m_partyId, "p2DeckShareState", out player2DeckShareState) && player2DeckShareState == "requested")
		{
			BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p2DeckShareState", "none"));
		}
	}

	private void OnFriendChallengeDeckShareRequestDialogResponse(AlertPopup.Response response, object userData)
	{
		string value = ((response == AlertPopup.Response.CANCEL) ? "declined" : "sharing");
		if (DidSendChallenge())
		{
			if (BattleNet.GetPartyAttribute(m_data.m_partyId, "p2DeckShareState", out string player2DeckShareState) && player2DeckShareState == "requested")
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p2DeckShareState", value));
			}
			if (BattleNet.GetPartyAttribute(m_data.m_partyId, "p1DeckShareState", out string player1DeckShareState) && player1DeckShareState == "requested")
			{
				FriendlyChallengeHelper.Get().ShowDeckShareRequestWaitingDialog(OnFriendChallengeDeckShareRequestDialogWaitingResponse);
			}
		}
		else if (DidReceiveChallenge())
		{
			if (BattleNet.GetPartyAttribute(m_data.m_partyId, "p1DeckShareState", out string player1DeckShareState2) && player1DeckShareState2 == "requested")
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p1DeckShareState", value));
			}
			if (BattleNet.GetPartyAttribute(m_data.m_partyId, "p2DeckShareState", out string player2DeckShareState2) && player2DeckShareState2 == "requested")
			{
				FriendlyChallengeHelper.Get().ShowDeckShareRequestWaitingDialog(OnFriendChallengeDeckShareRequestDialogWaitingResponse);
			}
		}
	}

	private DeckShareState GetDeckShareStateEnumFromAttribute(string deckShareStateAttribute)
	{
		DeckShareState deckShareStateEnum = DeckShareState.NO_DECK_SHARE;
		if (deckShareStateAttribute == "sharingUnused")
		{
			deckShareStateEnum = DeckShareState.DECK_SHARED_UNUSED;
		}
		else if (deckShareStateAttribute == "sharing")
		{
			deckShareStateEnum = DeckShareState.USING_SHARED_DECK;
		}
		return deckShareStateEnum;
	}

	public void SkipDeckSelection()
	{
		SelectDeck(1L);
	}

	public void SelectDeck(long deckId)
	{
		if (DidSendChallenge())
		{
			m_data.m_challengerDeckOrHeroSelected = true;
		}
		else
		{
			if (!DidReceiveChallenge())
			{
				return;
			}
			m_data.m_challengeeDeckOrHeroSelected = true;
		}
		SelectMyDeck_InternalParty(deckId);
		FireChangedEvent(FriendChallengeEvent.SELECTED_DECK_OR_HERO, BnetPresenceMgr.Get().GetMyPlayer());
	}

	public void SelectHero(long heroCardDbId)
	{
		if (DidSendChallenge())
		{
			m_data.m_challengerDeckOrHeroSelected = true;
		}
		else
		{
			if (!DidReceiveChallenge())
			{
				return;
			}
			m_data.m_challengeeDeckOrHeroSelected = true;
		}
		SelectMyHero_InternalParty(heroCardDbId);
		FireChangedEvent(FriendChallengeEvent.SELECTED_DECK_OR_HERO, BnetPresenceMgr.Get().GetMyPlayer());
	}

	public void DeselectDeckOrHero()
	{
		if (m_hasPreSelectedDeckOrHero)
		{
			m_hasPreSelectedDeckOrHero = false;
			m_preSelectedDeckId = 0L;
			m_preSelectedHeroId = 0L;
		}
		if (DidSendChallenge() && m_data.m_challengerDeckOrHeroSelected)
		{
			m_data.m_challengerDeckOrHeroSelected = false;
			m_data.m_challengerDeckId = 0L;
			m_data.m_challengerHeroId = 0L;
			m_data.m_challengerInGameState = false;
		}
		else
		{
			if (!DidReceiveChallenge() || !m_data.m_challengeeDeckOrHeroSelected)
			{
				return;
			}
			m_data.m_challengeeDeckOrHeroSelected = false;
			m_data.m_challengeeDeckId = 0L;
			m_data.m_challengeeHeroId = 0L;
			m_data.m_challengeeInGameState = false;
		}
		SelectMyDeck_InternalParty(0L);
		SelectMyHero_InternalParty(0L);
		FireChangedEvent(FriendChallengeEvent.DESELECTED_DECK_OR_HERO, BnetPresenceMgr.Get().GetMyPlayer());
	}

	public void SetChallengeMethod(ChallengeMethod challengeMethod)
	{
		m_challengeMethod = challengeMethod;
	}

	private void SelectMyDeck_InternalParty(long deckId)
	{
		string stateValue = ((deckId == 0L) ? "deck" : "ready");
		CardBackManager.Get().FindCardBackToUse(deckId, out var cardBackToUse, out var deckCardBack);
		long? randomHeroCardId = null;
		if (deckId != 0L)
		{
			CollectionDeck collectionDeck = CollectionManager.Get().GetDeck(deckId);
			if (collectionDeck == null && m_data.m_sharedDecks != null)
			{
				CollectionDeck borrowedDeck = m_data.m_sharedDecks.Find((CollectionDeck deck) => deck.ID == deckId);
				if (borrowedDeck != null)
				{
					collectionDeck = borrowedDeck;
					collectionDeck.HeroOverridden = false;
					collectionDeck.RandomHeroUseFavorite = true;
				}
			}
			if (collectionDeck != null && !collectionDeck.HeroOverridden)
			{
				randomHeroCardId = CollectionManager.Get().GetRandomHeroIdOwnedByPlayer(collectionDeck.GetClass(), collectionDeck.RandomHeroUseFavorite, null);
			}
		}
		if (DidSendChallenge())
		{
			m_data.m_challengerDeckId = deckId;
			BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("s1", stateValue), BnetAttribute.CreateAttribute("d1", deckId));
			if (cardBackToUse != deckCardBack)
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p1CardBack", cardBackToUse));
			}
			if (randomHeroCardId.HasValue)
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("randomHero1", randomHeroCardId.Value));
			}
		}
		else
		{
			m_data.m_challengeeDeckId = deckId;
			BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("s2", stateValue), BnetAttribute.CreateAttribute("d2", deckId));
			if (cardBackToUse != deckCardBack)
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p2CardBack", cardBackToUse));
			}
			if (randomHeroCardId.HasValue)
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("randomHero2", randomHeroCardId.Value));
			}
		}
	}

	private void SelectMyHero_InternalParty(long heroCardDbId)
	{
		string stateValue = ((heroCardDbId == 0L) ? "deck" : "ready");
		if (DidSendChallenge())
		{
			m_data.m_challengerHeroId = heroCardDbId;
			BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("s1", stateValue), BnetAttribute.CreateAttribute("hero1", heroCardDbId));
		}
		else
		{
			m_data.m_challengeeHeroId = heroCardDbId;
			BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("s2", stateValue), BnetAttribute.CreateAttribute("hero2", heroCardDbId));
		}
	}

	public int GetScenarioId()
	{
		return m_data.m_scenarioId;
	}

	public FormatType GetFormatType()
	{
		return m_data.m_challengeFormatType;
	}

	public PartyQuestInfo GetPartyQuestInfo()
	{
		return GetPartyQuestInfo(m_data.m_partyId, "quests");
	}

	public PartyQuestInfo GetPartyQuestInfo(BnetPartyId partyId, string attributeKey)
	{
		PartyQuestInfo info = null;
		if (BattleNet.GetPartyAttribute(partyId, attributeKey, out byte[] questInfoBlob))
		{
			info = ProtobufUtil.ParseFrom<PartyQuestInfo>(questInfoBlob);
		}
		return info;
	}

	public bool AddChangedListener(ChangedCallback callback)
	{
		return AddChangedListener(callback, null);
	}

	public bool AddChangedListener(ChangedCallback callback, object userData)
	{
		ChangedListener listener = new ChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_changedListeners.Contains(listener))
		{
			return false;
		}
		m_changedListeners.Add(listener);
		return true;
	}

	public bool RemoveChangedListener(ChangedCallback callback)
	{
		return RemoveChangedListener(callback, null);
	}

	private bool RemoveChangedListener(ChangedCallback callback, object userData)
	{
		ChangedListener listener = new ChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_changedListeners.Remove(listener);
	}

	public static bool RemoveChangedListenerFromInstance(ChangedCallback callback, object userData = null)
	{
		if (s_instance == null)
		{
			return false;
		}
		return s_instance.RemoveChangedListener(callback, userData);
	}

	private void OnPartyUpdate(PartyEvent[] updates)
	{
		for (int i = 0; i < updates.Length; i++)
		{
			PartyEvent update = updates[i];
			BnetPartyId partyId = update.partyId;
			BnetGameAccountId otherMemberId = update.otherMemberId;
			if (update.eventName == "s1")
			{
				if (update.eventData == "wait")
				{
					OnPartyUpdate_CreatedParty(partyId, otherMemberId);
				}
				else if (update.eventData == "deck")
				{
					if (DidReceiveChallenge() && m_data.m_challengerDeckOrHeroSelected)
					{
						m_data.m_challengerDeckOrHeroSelected = false;
						m_data.m_challengerInGameState = false;
						FireChangedEvent(FriendChallengeEvent.DESELECTED_DECK_OR_HERO, m_data.m_challenger);
					}
				}
				else if (update.eventData == "ready")
				{
					if (DidReceiveChallenge())
					{
						m_data.m_challengerDeckOrHeroSelected = true;
						FireChangedEvent(FriendChallengeEvent.SELECTED_DECK_OR_HERO, m_data.m_challenger);
						SetIAmInGameState();
					}
				}
				else if (update.eventData == "game")
				{
					if (DidReceiveChallenge())
					{
						m_data.m_challengerInGameState = true;
						SetIAmInGameState();
						StartFriendlyChallengeGameIfReady();
						FriendlyChallengeHelper.Get().WaitForFriendChallengeToStart();
						m_data.m_findGameErrorOccurred = false;
					}
				}
				else if (update.eventData == "goto")
				{
					m_data.m_challengerDeckOrHeroSelected = false;
					m_data.m_challengerInGameState = false;
				}
			}
			else if (update.eventName == "s2")
			{
				if (update.eventData == "wait")
				{
					OnPartyUpdate_JoinedParty(partyId, otherMemberId);
				}
				else if (update.eventData == "deck")
				{
					if (DidSendChallenge())
					{
						if (m_data.m_challengeeAccepted)
						{
							m_data.m_challengeeDeckOrHeroSelected = false;
							m_data.m_challengeeInGameState = false;
							FireChangedEvent(FriendChallengeEvent.DESELECTED_DECK_OR_HERO, m_data.m_challengee);
						}
						else
						{
							m_data.m_challengeeAccepted = true;
							FireChangedEvent(FriendChallengeEvent.OPPONENT_ACCEPTED_CHALLENGE, m_data.m_challengee);
						}
					}
				}
				else if (update.eventData == "ready")
				{
					if (DidSendChallenge())
					{
						m_data.m_challengeeDeckOrHeroSelected = true;
						FireChangedEvent(FriendChallengeEvent.SELECTED_DECK_OR_HERO, m_data.m_challengee);
						SetIAmInGameState();
					}
				}
				else if (update.eventData == "game")
				{
					if (DidSendChallenge())
					{
						m_data.m_challengeeInGameState = true;
						SetIAmInGameState();
						if (StartFriendlyChallengeGameIfReady())
						{
							FriendlyChallengeHelper.Get().WaitForFriendChallengeToStart();
						}
					}
				}
				else if (update.eventData == "goto")
				{
					m_data.m_challengeeDeckOrHeroSelected = false;
					m_data.m_challengeeInGameState = false;
				}
			}
			else if (update.eventName == "left")
			{
				if (DidSendChallenge())
				{
					BnetPlayer challengee = m_data.m_challengee;
					bool challengeeAccepted = m_data.m_challengeeAccepted;
					RevertTavernBrawlPresenceStatus();
					FriendlyChallengeData challengeData = CleanUpChallengeData();
					if (challengeeAccepted)
					{
						FireChangedEvent(FriendChallengeEvent.OPPONENT_CANCELED_CHALLENGE, challengee, challengeData);
					}
					else
					{
						FireChangedEvent(FriendChallengeEvent.OPPONENT_DECLINED_CHALLENGE, challengee, challengeData);
					}
				}
				else if (DidReceiveChallenge())
				{
					BnetPlayer challenger = m_data.m_challenger;
					bool challengeeAccepted2 = m_data.m_challengeeAccepted;
					RevertTavernBrawlPresenceStatus();
					FriendlyChallengeData challengeData2 = CleanUpChallengeData();
					if (challenger != null)
					{
						if (challengeeAccepted2)
						{
							FireChangedEvent(FriendChallengeEvent.OPPONENT_CANCELED_CHALLENGE, challenger, challengeData2);
						}
						else
						{
							FireChangedEvent(FriendChallengeEvent.OPPONENT_RESCINDED_CHALLENGE, challenger, challengeData2);
						}
					}
				}
				else
				{
					CleanUpChallengeData();
				}
			}
			else if (update.eventName == "p1DeckShareState")
			{
				if (m_data.m_challenger == null)
				{
					continue;
				}
				string oldState = m_data.m_challengerDeckShareState;
				m_data.m_challengerDeckShareState = update.eventData;
				if (oldState == "none" && m_data.m_challengerDeckShareState == "requested")
				{
					if (DidReceiveChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.OPPONENT_REQUESTED_DECK_SHARE, m_data.m_challenger);
					}
					else if (DidSendChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.I_REQUESTED_DECK_SHARE, m_data.m_challenger);
					}
				}
				else if (oldState == "requested" && m_data.m_challengerDeckShareState == "none")
				{
					if (DidSendChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.I_CANCELED_DECK_SHARE_REQUEST, m_data.m_challenger);
					}
					if (DidReceiveChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.OPPONENT_CANCELED_DECK_SHARE_REQUEST, m_data.m_challenger);
					}
				}
				else if (oldState == "requested" && m_data.m_challengerDeckShareState == "declined")
				{
					if (DidReceiveChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.I_DECLINED_DECK_SHARE_REQUEST, m_data.m_challenger);
					}
					else if (DidSendChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.OPPONENT_DECLINED_DECK_SHARE_REQUEST, m_data.m_challenger);
					}
				}
				else if (oldState == "requested" && m_data.m_challengerDeckShareState == "sharing")
				{
					if (DidReceiveChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.I_ACCEPTED_DECK_SHARE_REQUEST, m_data.m_challenger);
					}
					else if (DidSendChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.OPPONENT_ACCEPTED_DECK_SHARE_REQUEST, m_data.m_challenger);
					}
				}
				else if (oldState == "sharing" && m_data.m_challengerDeckShareState == "sharingUnused")
				{
					if (DidSendChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.I_ENDED_DECK_SHARE, m_data.m_challenger);
					}
				}
				else if (oldState == "sharingUnused" && m_data.m_challengerDeckShareState == "sharing")
				{
					if (DidSendChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.I_RECEIVED_SHARED_DECKS, m_data.m_challenger);
					}
				}
				else if (m_data.m_challengerDeckShareState == "error" && DidSendChallenge())
				{
					FireChangedEvent(FriendChallengeEvent.DECK_SHARE_ERROR_OCCURED, m_data.m_challenger);
				}
			}
			else if (update.eventName == "p2DeckShareState")
			{
				if (m_data.m_challengee == null)
				{
					continue;
				}
				string oldState2 = m_data.m_challengeeDeckShareState;
				m_data.m_challengeeDeckShareState = update.eventData;
				if (oldState2 == "none" && m_data.m_challengeeDeckShareState == "requested")
				{
					if (DidReceiveChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.I_REQUESTED_DECK_SHARE, m_data.m_challengee);
					}
					else if (DidSendChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.OPPONENT_REQUESTED_DECK_SHARE, m_data.m_challengee);
					}
				}
				else if (oldState2 == "requested" && m_data.m_challengeeDeckShareState == "none")
				{
					if (DidSendChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.OPPONENT_CANCELED_DECK_SHARE_REQUEST, m_data.m_challengee);
					}
					else if (DidReceiveChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.I_CANCELED_DECK_SHARE_REQUEST, m_data.m_challengee);
					}
				}
				else if (oldState2 == "requested" && m_data.m_challengeeDeckShareState == "declined")
				{
					if (DidReceiveChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.OPPONENT_DECLINED_DECK_SHARE_REQUEST, m_data.m_challengee);
					}
					else if (DidSendChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.I_DECLINED_DECK_SHARE_REQUEST, m_data.m_challengee);
					}
				}
				else if (oldState2 == "requested" && m_data.m_challengeeDeckShareState == "sharing")
				{
					if (DidReceiveChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.OPPONENT_ACCEPTED_DECK_SHARE_REQUEST, m_data.m_challengee);
					}
					else if (DidSendChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.I_ACCEPTED_DECK_SHARE_REQUEST, m_data.m_challengee);
					}
				}
				else if (oldState2 == "sharing" && m_data.m_challengeeDeckShareState == "sharingUnused")
				{
					if (DidReceiveChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.I_ENDED_DECK_SHARE, m_data.m_challengee);
					}
				}
				else if (oldState2 == "sharingUnused" && m_data.m_challengeeDeckShareState == "sharing")
				{
					if (DidReceiveChallenge())
					{
						FireChangedEvent(FriendChallengeEvent.I_RECEIVED_SHARED_DECKS, m_data.m_challengee);
					}
				}
				else if (m_data.m_challengeeDeckShareState == "error" && DidReceiveChallenge())
				{
					FireChangedEvent(FriendChallengeEvent.DECK_SHARE_ERROR_OCCURED, m_data.m_challengee);
				}
			}
			else if (update.eventName == "p1DeckShareDecks")
			{
				if (DidReceiveChallenge() && m_data.m_challengeeDeckShareState == "sharing")
				{
					if (HasOpponentSharedDecks())
					{
						FireChangedEvent(FriendChallengeEvent.I_RECEIVED_SHARED_DECKS, m_data.m_challengee);
						continue;
					}
					BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p2DeckShareState", "error"));
				}
			}
			else if (update.eventName == "p2DeckShareDecks" && DidSendChallenge() && m_data.m_challengerDeckShareState == "sharing")
			{
				if (HasOpponentSharedDecks())
				{
					FireChangedEvent(FriendChallengeEvent.I_RECEIVED_SHARED_DECKS, m_data.m_challenger);
					continue;
				}
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p1DeckShareState", "error"));
			}
		}
	}

	private void OnPartyUpdate_CreatedParty(BnetPartyId partyId, BnetGameAccountId otherMemberId)
	{
		UpdateChallengeSentDialog();
	}

	private void OnPartyUpdate_JoinedParty(BnetPartyId partyId, BnetGameAccountId otherMemberId)
	{
		if (!DidSendChallenge())
		{
			if (!CanReceiveChallengeFrom(otherMemberId, partyId))
			{
				DeclineFriendChallenge_Internal(partyId);
			}
			else if (!AmIAvailable())
			{
				DeclineFriendChallenge_Internal(partyId);
			}
			else
			{
				HandleJoinedParty(partyId, otherMemberId);
			}
		}
	}

	private static bool CanReceiveChallengeFrom(BnetGameAccountId challengerPlayer, BnetPartyId challengerPartyId)
	{
		if (BnetFriendMgr.Get().IsFriend(challengerPlayer))
		{
			return true;
		}
		if (BnetNearbyPlayerMgr.Get().IsNearbyStranger(challengerPlayer))
		{
			return true;
		}
		return false;
	}

	private bool StartFriendlyChallengeGameIfReady()
	{
		if (!DidSendChallenge())
		{
			return false;
		}
		if (!BnetParty.IsInParty(m_data.m_partyId))
		{
			return false;
		}
		bool bothDecksSelected = m_data.m_challengerDeckId != 0L && m_data.m_challengeeDeckId != 0;
		bool bothHeroesSelected = m_data.m_challengerHeroId != 0L && m_data.m_challengeeHeroId != 0;
		if (!bothDecksSelected && !bothHeroesSelected)
		{
			return false;
		}
		if (!m_data.m_challengerInGameState || !m_data.m_challengeeInGameState)
		{
			return false;
		}
		m_data.m_findGameErrorOccurred = false;
		BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("s1", "goto"), BnetAttribute.CreateAttribute("s2", "goto"));
		FormatType formatType = GetFormatType();
		if (IsChallengeBacon())
		{
			Network.Get().EnterBattlegroundsWithFriend(m_data.m_challengee.GetHearthstoneGameAccountId(), m_data.m_scenarioId);
		}
		else if (bothDecksSelected)
		{
			BattleNet.GetPartyAttribute(m_data.m_partyId, "p1DeckShareState", out string challengerDeckShareAttr);
			DeckShareState challengerDeckShareState = GetDeckShareStateEnumFromAttribute(challengerDeckShareAttr);
			BattleNet.GetPartyAttribute(m_data.m_partyId, "p2DeckShareState", out string challengeeDeckShareAttr);
			DeckShareState challengeeDeckShareState = GetDeckShareStateEnumFromAttribute(challengeeDeckShareAttr);
			GameMgr.Get().EnterFriendlyChallengeGameWithDecks(formatType, m_data.m_challengeBrawlType, m_data.m_scenarioId, m_data.m_seasonId, m_data.m_brawlLibraryItemId, m_data.m_challengee.GetHearthstoneGameAccountId(), challengerDeckShareState, m_data.m_challengerDeckId, challengeeDeckShareState, m_data.m_challengeeDeckId, m_data.m_challengerRandomHeroCardId, m_data.m_challengeeRandomHeroCardId, m_data.m_challengerCardBackId, m_data.m_challengeeCardBackId);
		}
		else
		{
			GameMgr.Get().EnterFriendlyChallengeGameWithHeroes(formatType, m_data.m_challengeBrawlType, m_data.m_scenarioId, m_data.m_seasonId, m_data.m_brawlLibraryItemId, m_data.m_challengee.GetHearthstoneGameAccountId(), m_data.m_challengerHeroId, m_data.m_challengeeHeroId, m_data.m_challengerCardBackId, m_data.m_challengeeCardBackId);
		}
		if (m_challengeDialog != null)
		{
			m_challengeDialog.Hide();
			m_challengeDialog = null;
		}
		return true;
	}

	private void SetIAmInGameState()
	{
		if (BnetParty.IsInParty(m_data.m_partyId) && m_data.m_challengerDeckOrHeroSelected && m_data.m_challengeeDeckOrHeroSelected && !AmIInGameState())
		{
			if (DidSendChallenge())
			{
				m_data.m_challengerInGameState = true;
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("s1", "game"));
			}
			else
			{
				m_data.m_challengeeInGameState = true;
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("s2", "game"));
			}
		}
	}

	private void OnNetCacheReady()
	{
		NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady);
		m_netCacheReady = true;
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.FATAL_ERROR)
		{
			UpdateMyAvailability();
		}
	}

	private void OnSceneUnloaded(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		if (prevMode != SceneMgr.Mode.GAMEPLAY)
		{
			UpdateMyAvailability();
		}
		if (m_data.m_updatePartyQuestInfoOnGameplaySceneUnload && prevMode == SceneMgr.Mode.GAMEPLAY)
		{
			m_data.m_updatePartyQuestInfoOnGameplaySceneUnload = false;
			UpdatePartyQuestInfo();
		}
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY && mode != SceneMgr.Mode.GAMEPLAY && mode != SceneMgr.Mode.FATAL_ERROR)
		{
			m_netCacheReady = false;
			if (mode == SceneMgr.Mode.FRIENDLY || (mode == SceneMgr.Mode.TAVERN_BRAWL && Get().IsChallengeTavernBrawl()))
			{
				UpdateMyAvailability();
			}
			else
			{
				CancelChallenge();
			}
			NetCache.Get().RegisterFriendChallenge(OnNetCacheReady);
		}
	}

	private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
	{
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		BnetPlayerChange change = changelist.FindChange(myself);
		if (change != null)
		{
			bool amIAvailable = AmIAvailable();
			BnetGameAccount myHsAccount = myself.GetHearthstoneGameAccount();
			if (myHsAccount != null && !m_myPlayerReady && myHsAccount.HasGameField(20u) && myHsAccount.HasGameField(19u))
			{
				m_myPlayerReady = true;
				if (!UpdateMyAvailability())
				{
					amIAvailable = false;
				}
			}
			if (!amIAvailable && m_data.m_challengerPending)
			{
				DeclineFriendChallenge_Internal(m_data.m_partyId);
				CleanUpChallengeData();
			}
		}
		if (!m_data.m_challengerPending)
		{
			return;
		}
		change = changelist.FindChange(m_data.m_challengerId);
		if (change != null)
		{
			BnetPlayer challenger = change.GetPlayer();
			if (challenger.IsDisplayable())
			{
				m_data.m_challenger = challenger;
				m_data.m_challengerPending = false;
				FireChangedEvent(FriendChallengeEvent.I_RECEIVED_CHALLENGE, m_data.m_challenger);
			}
		}
	}

	private void OnFriendsChanged(BnetFriendChangelist changelist, object userData)
	{
		if (!HasChallenge())
		{
			return;
		}
		List<BnetPlayer> removedFriends = changelist.GetRemovedFriends();
		if (removedFriends == null)
		{
			return;
		}
		BnetPlayer opponent = GetOpponent(BnetPresenceMgr.Get().GetMyPlayer());
		if (opponent == null)
		{
			return;
		}
		foreach (BnetPlayer item in removedFriends)
		{
			if (item != opponent)
			{
				continue;
			}
			PartyInfo[] joinedParties = BnetParty.GetJoinedParties();
			BnetGameAccountId opponentAccountId = opponent.GetHearthstoneGameAccountId();
			PartyInfo[] array = joinedParties;
			foreach (PartyInfo party in array)
			{
				if (BnetParty.IsMember(party.Id, opponentAccountId))
				{
					BnetParty.Leave(party.Id);
				}
			}
			RevertTavernBrawlPresenceStatus();
			FriendlyChallengeData challengeData = CleanUpChallengeData();
			FireChangedEvent(FriendChallengeEvent.OPPONENT_REMOVED_FROM_FRIENDS, opponent, challengeData);
			break;
		}
	}

	private void OnNearbyPlayersChanged(BnetRecentOrNearbyPlayerChangelist changelist, object userData)
	{
		if (!HasChallenge())
		{
			return;
		}
		List<BnetPlayer> removedPlayers = changelist.GetRemovedPlayers();
		if (removedPlayers == null)
		{
			return;
		}
		BnetPlayer opponent = GetOpponent(BnetPresenceMgr.Get().GetMyPlayer());
		if (opponent == null)
		{
			return;
		}
		foreach (BnetPlayer item in removedPlayers)
		{
			if (item == opponent)
			{
				FriendlyChallengeData challengeData = CleanUpChallengeData();
				FireChangedEvent(FriendChallengeEvent.OPPONENT_CANCELED_CHALLENGE, opponent, challengeData);
				break;
			}
		}
	}

	private void OnDisconnectedFromBattleNet(BattleNetErrors error)
	{
		OnDisconnect();
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		OnDisconnect();
	}

	private void OnDisconnect()
	{
		if (m_challengeDialog != null)
		{
			m_challengeDialog.Hide();
			m_challengeDialog = null;
		}
		CleanUpChallengeData();
	}

	private void OnReconnectLoginComplete()
	{
		UpdateMyAvailability();
	}

	private void OnChallengeChanged(FriendChallengeEvent challengeEvent, BnetPlayer player, FriendlyChallengeData challengeData, object userData)
	{
		switch (challengeEvent)
		{
		case FriendChallengeEvent.I_SENT_CHALLENGE:
			ShowISentChallengeDialog(player);
			break;
		case FriendChallengeEvent.OPPONENT_ACCEPTED_CHALLENGE:
			StartChallengeProcess();
			break;
		case FriendChallengeEvent.OPPONENT_DECLINED_CHALLENGE:
			ShowOpponentDeclinedChallengeDialog(player, challengeData);
			break;
		case FriendChallengeEvent.I_RECEIVED_CHALLENGE:
			if (CanPromptReceivedChallenge())
			{
				if (IsChallengeTavernBrawl())
				{
					PresenceMgr.Get().SetStatus(Global.PresenceStatus.TAVERN_BRAWL_FRIENDLY_WAITING);
				}
				ShowIReceivedChallengeDialog(player);
			}
			break;
		case FriendChallengeEvent.I_ACCEPTED_CHALLENGE:
			StartChallengeProcess();
			break;
		case FriendChallengeEvent.I_RESCINDED_CHALLENGE:
		case FriendChallengeEvent.I_DECLINED_CHALLENGE:
			OnChallengeCanceled();
			break;
		case FriendChallengeEvent.OPPONENT_RESCINDED_CHALLENGE:
			OnChallengeCanceled();
			ShowOpponentCanceledChallengeDialog(player, challengeData);
			break;
		case FriendChallengeEvent.OPPONENT_CANCELED_CHALLENGE:
			FriendlyChallengeHelper.Get().HideAllDeckShareDialogs();
			OnChallengeCanceled();
			ShowOpponentCanceledChallengeDialog(player, challengeData);
			break;
		case FriendChallengeEvent.OPPONENT_REMOVED_FROM_FRIENDS:
			FriendlyChallengeHelper.Get().HideAllDeckShareDialogs();
			ShowOpponentRemovedFromFriendsDialog(player, challengeData);
			break;
		case FriendChallengeEvent.QUEUE_CANCELED:
			OnChallengeCanceled();
			ShowQueueCanceledDialog(player, challengeData);
			break;
		case FriendChallengeEvent.I_REQUESTED_DECK_SHARE:
			if (!FriendlyChallengeHelper.Get().IsShowingDeckShareRequestDialog())
			{
				FriendlyChallengeHelper.Get().ShowDeckShareRequestWaitingDialog(OnFriendChallengeDeckShareRequestDialogWaitingResponse);
			}
			break;
		case FriendChallengeEvent.I_ACCEPTED_DECK_SHARE_REQUEST:
			FriendlyChallengeHelper.Get().HideDeckShareRequestDialog();
			ShareDecks_InternalParty();
			break;
		case FriendChallengeEvent.I_DECLINED_DECK_SHARE_REQUEST:
			FriendlyChallengeHelper.Get().HideDeckShareRequestDialog();
			break;
		case FriendChallengeEvent.I_CANCELED_DECK_SHARE_REQUEST:
			FriendlyChallengeHelper.Get().HideDeckShareRequestWaitingDialog();
			break;
		case FriendChallengeEvent.DECK_SHARE_ERROR_OCCURED:
			if (DidSendChallenge())
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p1DeckShareState", "none"));
			}
			else if (DidReceiveChallenge())
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p2DeckShareState", "none"));
			}
			FriendlyChallengeHelper.Get().HideAllDeckShareDialogs();
			FriendlyChallengeHelper.Get().ShowDeckShareErrorDialog();
			break;
		case FriendChallengeEvent.OPPONENT_REQUESTED_DECK_SHARE:
			FriendlyChallengeHelper.Get().HideAllDeckShareDialogs();
			FriendlyChallengeHelper.Get().HideFriendChallengeWaitingForOpponentDialog();
			FriendlyChallengeHelper.Get().ShowDeckShareRequestDialog(OnFriendChallengeDeckShareRequestDialogResponse);
			break;
		case FriendChallengeEvent.OPPONENT_ACCEPTED_DECK_SHARE_REQUEST:
			FriendlyChallengeHelper.Get().HideDeckShareRequestWaitingDialog();
			break;
		case FriendChallengeEvent.OPPONENT_DECLINED_DECK_SHARE_REQUEST:
			if (DidSendChallenge())
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p1DeckShareState", "none"));
			}
			else if (DidReceiveChallenge())
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("p2DeckShareState", "none"));
			}
			FriendlyChallengeHelper.Get().ShowDeckShareRequestDeclinedDialog();
			FriendlyChallengeHelper.Get().HideDeckShareRequestWaitingDialog();
			break;
		case FriendChallengeEvent.OPPONENT_CANCELED_DECK_SHARE_REQUEST:
			FriendlyChallengeHelper.Get().ShowDeckShareRequestCanceledDialog();
			FriendlyChallengeHelper.Get().HideDeckShareRequestDialog();
			break;
		case FriendChallengeEvent.SELECTED_DECK_OR_HERO:
		case FriendChallengeEvent.DESELECTED_DECK_OR_HERO:
		case FriendChallengeEvent.I_ENDED_DECK_SHARE:
		case FriendChallengeEvent.I_RECEIVED_SHARED_DECKS:
			break;
		}
	}

	private void OnChallengeCanceled()
	{
		GameMgr.Get().CancelFindGame();
		GameMgr.Get().HideTransitionPopup();
	}

	private bool CanPromptReceivedChallenge()
	{
		bool isBusy = !UserAttentionManager.CanShowAttentionGrabber("FriendlyChallengeMgr.CanPromptReceivedChallenge");
		if (!isBusy)
		{
			if (GameMgr.Get().IsFindingGame())
			{
				isBusy = true;
			}
			else if (RankMgr.Get().IsLegendRankInAnyFormat)
			{
				isBusy = SceneMgr.Get().IsModeRequested(SceneMgr.Mode.TOURNAMENT);
			}
		}
		if (isBusy)
		{
			BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("WTCG.Friendly.DeclineReason", 6L));
			DeclineChallenge();
			return false;
		}
		if (IsChallengeTavernBrawl())
		{
			if (!TavernBrawlManager.Get().HasUnlockedTavernBrawl(m_data.m_challengeBrawlType))
			{
				DeclineReason reason = DeclineReason.TavernBrawlNotUnlocked;
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("WTCG.Friendly.DeclineReason", (long)reason));
				DeclineChallenge();
				return false;
			}
			TavernBrawlManager.Get().EnsureAllDataReady(m_data.m_challengeBrawlType, TavernBrawl_ReceivedChallenge_OnEnsureServerDataReady);
			return false;
		}
		if (!CollectionManager.Get().AreAllDeckContentsReady())
		{
			CollectionManager.Get().RequestDeckContentsForDecksWithoutContentsLoaded(CanPromptReceivedChallenge_OnDeckContentsLoaded);
			return false;
		}
		if (IsChallengeStandardDuel() && !CollectionManager.Get().AccountHasValidDeck(FormatType.FT_STANDARD))
		{
			DeclineReason reason2 = DeclineReason.StandardNoValidDeck;
			BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("WTCG.Friendly.DeclineReason", (long)reason2));
			DeclineChallenge();
			return false;
		}
		if (IsChallengeWildDuel())
		{
			if (!CollectionManager.Get().ShouldAccountSeeStandardWild())
			{
				DeclineReason reason3 = DeclineReason.NotSeenWild;
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("WTCG.Friendly.DeclineReason", (long)reason3));
				DeclineChallenge();
				return false;
			}
			if (!CollectionManager.Get().AccountHasValidDeck(FormatType.FT_WILD))
			{
				DeclineReason reason4 = DeclineReason.NoValidDeck;
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("WTCG.Friendly.DeclineReason", (long)reason4));
				DeclineChallenge();
				return false;
			}
		}
		else
		{
			if (IsChallengeClassicDuel() && !CollectionManager.Get().AccountHasValidDeck(FormatType.FT_CLASSIC))
			{
				DeclineReason reason5 = DeclineReason.ClassicNoValidDeck;
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("WTCG.Friendly.DeclineReason", (long)reason5));
				DeclineChallenge();
				return false;
			}
			if (IsChallengeTwistDuel() && !CollectionManager.Get().AccountHasValidDeck(FormatType.FT_TWIST) && !RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
			{
				DeclineReason reason6 = DeclineReason.TwistNoValidDeck;
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("WTCG.Friendly.DeclineReason", (long)reason6));
				DeclineChallenge();
				return false;
			}
			if (IsChallengeBacon() && !AllowBGInviteWhileInNPPGEnabled() && !GameUtils.IsBattleGroundsTutorialComplete())
			{
				DeclineReason reason7 = DeclineReason.BattlegroundsTutorialNotComplete;
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("WTCG.Friendly.DeclineReason", (long)reason7));
				DeclineChallenge();
				return false;
			}
			if (IsChallengeMercenaries() && !GameUtils.IsMercenariesVillageTutorialComplete())
			{
				DeclineReason reason8 = DeclineReason.MercsTutorialNotComplete;
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("WTCG.Friendly.DeclineReason", (long)reason8));
				DeclineChallenge();
				return false;
			}
		}
		return true;
	}

	private void CanPromptReceivedChallenge_OnDeckContentsLoaded()
	{
		if (DidReceiveChallenge() && CanPromptReceivedChallenge())
		{
			ShowIReceivedChallengeDialog(m_data.m_challenger);
		}
	}

	private void TavernBrawl_ReceivedChallenge_OnEnsureServerDataReady()
	{
		TavernBrawlMission mission = TavernBrawlManager.Get().GetMission(m_data.m_challengeBrawlType);
		DeclineReason? declineReason = null;
		if (mission == null)
		{
			declineReason = DeclineReason.None;
		}
		if (mission != null && mission.CanCreateDeck(m_data.m_brawlLibraryItemId) && !TavernBrawlManager.Get().HasValidDeck(m_data.m_challengeBrawlType))
		{
			declineReason = DeclineReason.TavernBrawlNoValidDeck;
		}
		if (declineReason.HasValue)
		{
			BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("WTCG.Friendly.DeclineReason", (long)declineReason.Value));
			DeclineChallenge();
			return;
		}
		if (IsChallengeTavernBrawl())
		{
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.TAVERN_BRAWL_FRIENDLY_WAITING);
		}
		ShowIReceivedChallengeDialog(m_data.m_challenger);
	}

	private bool RevertTavernBrawlPresenceStatus()
	{
		if (IsChallengeTavernBrawl() && PresenceMgr.Get().CurrentStatus == Global.PresenceStatus.TAVERN_BRAWL_FRIENDLY_WAITING)
		{
			PresenceMgr.Get().SetPrevStatus();
			return true;
		}
		return false;
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		UpdateMyAvailability();
		switch (eventData.m_state)
		{
		case FindGameState.BNET_QUEUE_CANCELED:
		case FindGameState.BNET_ERROR:
		{
			m_data.m_findGameErrorOccurred = true;
			if (DidSendChallenge())
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("error", (long)GameMgr.Get().GetLastEnterGameError()));
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("s1", "deck"));
			}
			else if (DidReceiveChallenge())
			{
				BattleNet.SetPartyAttributes(m_data.m_partyId, BnetAttribute.CreateAttribute("s2", "deck"));
			}
			SceneMgr.Mode mode = SceneMgr.Get().GetMode();
			if (mode != SceneMgr.Mode.FRIENDLY && mode != SceneMgr.Mode.TAVERN_BRAWL)
			{
				QueueCanceled();
			}
			break;
		}
		case FindGameState.BNET_QUEUE_ENTERED:
		case FindGameState.SERVER_GAME_CONNECTING:
			if (HasChallenge())
			{
				DeselectDeckOrHero();
			}
			break;
		}
		return false;
	}

	private void WillReset()
	{
		CleanUpChallengeData(updateAvailability: false);
		if (m_challengeDialog != null)
		{
			m_challengeDialog.Hide();
			m_challengeDialog = null;
		}
		FriendlyChallengeHelper.Get().HideAllDeckShareDialogs();
	}

	private void ShowISentChallengeDialog(BnetPlayer challengee)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER");
		info.m_text = GameStrings.Format("GLOBAL_FRIEND_CHALLENGE_OPPONENT_WAITING_RESPONSE", FriendUtils.GetUniqueName(challengee));
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.NONE;
		info.m_responseCallback = OnChallengeSentDialogResponse;
		info.m_layerToUse = GameLayer.UI;
		DialogManager.Get().ShowPopup(info, OnChallengeSentDialogProcessed);
	}

	private void ShowOpponentDeclinedChallengeDialog(BnetPlayer challengee, FriendlyChallengeData challengeData)
	{
		if (m_challengeDialog != null)
		{
			m_challengeDialog.Hide();
			m_challengeDialog = null;
		}
		if (!m_hasSeenDeclinedReason)
		{
			AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
			popupInfo.m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER");
			popupInfo.m_text = GameStrings.Format("GLOBAL_FRIEND_CHALLENGE_OPPONENT_DECLINED", FriendUtils.GetUniqueName(challengee));
			popupInfo.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			popupInfo.m_showAlertIcon = false;
			popupInfo.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			popupInfo.m_responseCallback = OnOpponentDeclinedChallengeDialogDismissed;
			AlertPopup.PopupInfo info = popupInfo;
			DialogManager.Get().ShowPopup(info);
		}
	}

	private void OnOpponentDeclinedChallengeDialogDismissed(AlertPopup.Response response, object userData)
	{
		ChatMgr.Get().UpdateFriendItemsWhenAvailable();
	}

	private void ShowIReceivedChallengeDialog(BnetPlayer challenger)
	{
		if (m_challengeDialog != null)
		{
			m_challengeDialog.Hide();
			m_challengeDialog = null;
		}
		DialogManager.Get().ShowFriendlyChallenge(m_data.m_challengeFormatType, challenger, IsChallengeTavernBrawl(), PartyType.FRIENDLY_CHALLENGE, null, OnChallengeReceivedDialogResponse, OnChallengeReceivedDialogProcessed);
	}

	private void ShowOpponentCanceledChallengeDialog(BnetPlayer otherPlayer, FriendlyChallengeData challengeData)
	{
		if (m_challengeDialog != null)
		{
			m_challengeDialog.Hide();
			m_challengeDialog = null;
		}
		if ((GameMgr.Get() == null || !SuppressChallengeCanceledDialogByMissionId(GameMgr.Get().GetMissionId())) && (SceneMgr.Get() == null || !SceneMgr.Get().IsInGame() || GameState.Get() == null || GameState.Get().IsGameOverNowOrPending()))
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER");
			info.m_text = GameStrings.Format("GLOBAL_FRIEND_CHALLENGE_OPPONENT_CANCELED", FriendUtils.GetUniqueName(otherPlayer));
			info.m_showAlertIcon = false;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info.m_responseCallback = OnOpponentCanceledChallengeDialogClosed;
			DialogManager.Get().ShowPopup(info);
		}
	}

	public void OnOpponentCanceledChallengeDialogClosed(AlertPopup.Response response, object userData)
	{
		if (SceneMgr.Get().IsTransitionNowOrPending() && SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.FRIENDLY)
		{
			SceneMgr.Get().ReturnToPreviousMode();
		}
	}

	private void ShowOpponentRemovedFromFriendsDialog(BnetPlayer otherPlayer, FriendlyChallengeData challengeData)
	{
		if (m_challengeDialog != null)
		{
			m_challengeDialog.Hide();
			m_challengeDialog = null;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER");
		info.m_text = GameStrings.Format("GLOBAL_FRIEND_CHALLENGE_OPPONENT_FRIEND_REMOVED", FriendUtils.GetUniqueName(otherPlayer));
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.Get().ShowPopup(info);
	}

	private void ShowQueueCanceledDialog(BnetPlayer otherPlayer, FriendlyChallengeData challengeData)
	{
		if (m_challengeDialog != null)
		{
			m_challengeDialog.Hide();
			m_challengeDialog = null;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER");
		info.m_text = GameStrings.Format("GLOBAL_FRIEND_CHALLENGE_QUEUE_CANCELED");
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.Get().ShowPopup(info);
	}

	private bool OnChallengeSentDialogProcessed(DialogBase dialog, object userData)
	{
		if (!DidSendChallenge())
		{
			return false;
		}
		if (m_data.m_challengeeAccepted)
		{
			return false;
		}
		m_challengeDialog = dialog;
		UpdateChallengeSentDialog();
		return true;
	}

	private void UpdateChallengeSentDialog()
	{
		if (!(m_data.m_partyId == null) && !(m_challengeDialog == null))
		{
			AlertPopup challengeDialog = (AlertPopup)m_challengeDialog;
			AlertPopup.PopupInfo info = challengeDialog.GetInfo();
			if (info != null)
			{
				info.m_responseDisplay = AlertPopup.ResponseDisplay.CANCEL;
				challengeDialog.UpdateInfo(info);
			}
		}
	}

	private void OnChallengeSentDialogResponse(AlertPopup.Response response, object userData)
	{
		m_challengeDialog = null;
		RescindChallenge();
	}

	private bool OnChallengeReceivedDialogProcessed(DialogBase dialog, object userData)
	{
		if (!DidReceiveChallenge())
		{
			return false;
		}
		m_challengeDialog = dialog;
		PartyQuestInfo info = GetPartyQuestInfo();
		if (info != null)
		{
			((FriendlyChallengeDialog)dialog).SetQuestInfo(info);
		}
		return true;
	}

	private void OnChallengeReceivedDialogResponse(bool accept)
	{
		m_challengeDialog = null;
		if (accept)
		{
			AcceptChallenge();
		}
		else
		{
			DeclineChallenge();
		}
	}

	private void HandleJoinedParty(BnetPartyId partyId, BnetGameAccountId otherMemberId)
	{
		m_data.m_partyId = partyId;
		m_data.m_challengerId = otherMemberId;
		m_data.m_challenger = BnetUtils.GetPlayer(m_data.m_challengerId);
		m_data.m_challengee = BnetPresenceMgr.Get().GetMyPlayer();
		m_hasSeenDeclinedReason = false;
		if (m_data.m_challenger == null || !m_data.m_challenger.IsDisplayable())
		{
			m_data.m_challengerPending = true;
			UpdateMyAvailability();
		}
		else
		{
			UpdateMyAvailability();
			FireChangedEvent(FriendChallengeEvent.I_RECEIVED_CHALLENGE, m_data.m_challenger);
		}
	}

	public bool UpdateMyAvailability()
	{
		if (!Network.ShouldBeConnectedToAurora() || !Network.IsLoggedIn())
		{
			return false;
		}
		bool available = !HasAvailabilityBlocker();
		bool battlegroundsAvailable = GameUtils.CanCheckTutorialCompletion() && GameUtils.IsBattleGroundsTutorialComplete();
		bool mercenariesAvailable = GameUtils.CanCheckTutorialCompletion() && GameUtils.IsMercenariesVillageTutorialComplete();
		Log.Presence.PrintDebug("UpdateMyAvailability: Available=" + available);
		m_canBeInvitedToGame = available;
		m_canBeInvitedToBattlegrounds = battlegroundsAvailable;
		m_canBeInvitedToMercenaries = mercenariesAvailable;
		if (!m_updateMyAvailabilityCallbackScheduledThisFrame)
		{
			Processor.ScheduleCallback(0f, realTime: false, UpdateMyAvailabilityScheduledCallback);
		}
		m_updateMyAvailabilityCallbackScheduledThisFrame = true;
		return available;
	}

	private void UpdateMyAvailabilityScheduledCallback(object userData)
	{
		if (m_updateMyAvailabilityCallbackScheduledThisFrame)
		{
			m_updateMyAvailabilityCallbackScheduledThisFrame = false;
			Log.Presence.PrintDebug("UpdateMyAvailabilityScheduledCallback: Available=" + m_canBeInvitedToGame);
			BnetPresenceMgr.Get().SetGameField(1u, m_canBeInvitedToGame);
			BnetNearbyPlayerMgr.Get().SetAvailability(m_canBeInvitedToGame);
			BnetNearbyPlayerMgr.Get().SetBattlegroundsAvailability(m_canBeInvitedToBattlegrounds);
			BnetNearbyPlayerMgr.Get().SetMercenariesAvailability(m_canBeInvitedToMercenaries);
		}
	}

	private bool HasAvailabilityBlocker()
	{
		if (GetAvailabilityBlockerReason() != 0)
		{
			return true;
		}
		return false;
	}

	private AvailabilityBlockerReasons GetAvailabilityBlockerReason()
	{
		AvailabilityBlockerReasons reason = AvailabilityBlockerReasons.NONE;
		if (!m_netCacheReady)
		{
			reason = AvailabilityBlockerReasons.NETCACHE_NOT_READY;
		}
		if (!m_myPlayerReady)
		{
			reason = AvailabilityBlockerReasons.MY_PLAYER_NOT_READY;
		}
		if (HasChallenge())
		{
			reason = AvailabilityBlockerReasons.HAS_EXISTING_CHALLENGE;
		}
		if (PartyManager.Get().HasPendingPartyInviteOrDialog())
		{
			reason = AvailabilityBlockerReasons.HAS_PENDING_PARTY_INVITE;
		}
		if (reason == AvailabilityBlockerReasons.NONE)
		{
			reason = UserAttentionManager.GetAvailabilityBlockerReason(isFriendlyChallenge: true);
		}
		if (reason != 0)
		{
			Log.Presence.PrintDebug("GetAvailabilityBlockerReason: " + reason);
		}
		return reason;
	}

	private void FireChangedEvent(FriendChallengeEvent challengeEvent, BnetPlayer player, FriendlyChallengeData challengeData = null)
	{
		if (challengeData == null)
		{
			challengeData = m_data;
		}
		ChangedListener[] listeners = m_changedListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(challengeEvent, player, challengeData);
		}
	}

	private FriendlyChallengeData CleanUpChallengeData(bool updateAvailability = true)
	{
		FriendlyChallengeData data = m_data;
		m_data = new FriendlyChallengeData();
		if (updateAvailability)
		{
			UpdateMyAvailability();
		}
		return data;
	}

	private void StartChallengeProcess()
	{
		bool alreadySelectedDeck = (!DidSendChallenge() && m_data.m_challengeeDeckOrHeroSelected) || (DidSendChallenge() && m_data.m_challengerDeckOrHeroSelected);
		if (m_challengeDialog != null && !alreadySelectedDeck)
		{
			m_challengeDialog.Hide();
			m_challengeDialog = null;
		}
		GameMgr.Get().SetPendingAutoConcede(pendingAutoConcede: true);
		if (CollectionManager.Get().IsInEditMode())
		{
			CollectionManager.Get().GetEditedDeck()?.SendChanges(CollectionDeck.ChangeSource.StartChallengeProcess);
		}
		if (IsChallengeTavernBrawl())
		{
			TavernBrawlManager.Get().CurrentBrawlType = m_data.m_challengeBrawlType;
			TavernBrawlManager.Get().CurrentMission()?.SetSelectedBrawlLibraryItemId(m_data.m_brawlLibraryItemId);
		}
		if (IsChallengeBacon())
		{
			SkipDeckSelection();
			return;
		}
		if (IsChallengeTavernBrawl() && !TavernBrawlManager.Get().SelectHeroBeforeMission(m_data.m_challengeBrawlType))
		{
			if (TavernBrawlManager.Get().GetMission(m_data.m_challengeBrawlType).canCreateDeck)
			{
				if (TavernBrawlManager.Get().HasValidDeck(m_data.m_challengeBrawlType))
				{
					SelectDeck(TavernBrawlManager.Get().GetDeck(m_data.m_challengeBrawlType).ID);
				}
				else
				{
					Debug.LogError("Attempting to start a Tavern Brawl challenge without a valid deck!  How did this happen?");
				}
			}
			else
			{
				SkipDeckSelection();
			}
			return;
		}
		if (!IsChallengeTavernBrawl())
		{
			if (m_data.m_challengeFormatType == FormatType.FT_UNKNOWN)
			{
				RankMgr.LogMessage("m_data.m_challengeFormatType = FT_UNKOWN", "StartChallengeProcess", "D:\\p4Workspace\\32.0.0\\Pegasus\\Client\\Assets\\Game\\Bnet\\Scripts\\FriendChallengeMgr.cs", 3628);
				return;
			}
			Options.SetFormatType(m_data.m_challengeFormatType);
		}
		if (!DidSendChallenge() || !m_data.m_challengerDeckOrHeroSelected)
		{
			if (m_challengeDialog != null)
			{
				m_challengeDialog.Hide();
				m_challengeDialog = null;
			}
			Navigation.Clear();
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.FRIENDLY);
		}
	}

	private bool SuppressChallengeCanceledDialogByMissionId(int missionId)
	{
		if (missionId == 3459)
		{
			return true;
		}
		return false;
	}
}
