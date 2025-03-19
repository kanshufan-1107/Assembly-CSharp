using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.GameService.Protocol.V2.Client;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core.Streaming;
using Hearthstone.Streaming;
using PegasusLettuce;
using PegasusShared;
using SpectatorProto;
using UnityEngine;

public class PartyManager : IService
{
	public enum MercTeamShareState
	{
		NOT_SHARING,
		USING_LOCAL_TEAMS,
		USING_REMOTE_TEAMS
	}

	public enum MercTeamSharingButtonStatus
	{
		ENABLED,
		DISABLED
	}

	public enum MercTeamShareMSG
	{
		NO_MSG,
		REQUEST_SHARING,
		SHARING_REQUEST_CANCELLED,
		SHARING_REQUEST_DENIED
	}

	public enum PartyInviteEvent
	{
		I_CREATED_PARTY,
		I_SENT_INVITE,
		I_RESCINDED_INVITE,
		FRIEND_RECEIVED_INVITE,
		FRIEND_ACCEPTED_INVITE,
		FRIEND_DECLINED_INVITE,
		INVITE_EXPIRED,
		I_ACCEPTED_INVITE,
		I_DECLINED_INVITE,
		FRIEND_RESCINDED_INVITE,
		FRIEND_LEFT,
		LEADER_DISSOLVED_PARTY
	}

	public class PartyData
	{
		public PartyType m_type;

		public ulong m_inviteId;

		public BnetPartyId m_partyId;

		public ScenarioDbId m_scenarioId;

		public FormatType m_format;

		public int m_season;

		public bool m_private;
	}

	private class ChangedListener : EventListener<ChangedCallback>
	{
		public void Fire(PartyInviteEvent challengeEvent, BnetGameAccountId playerGameAccountId, PartyData challengeData)
		{
			m_callback(challengeEvent, playerGameAccountId, challengeData, m_userData);
		}
	}

	public delegate void ChangedCallback(PartyInviteEvent challengeEvent, BnetGameAccountId playerGameAccountId, PartyData challengeData, object userData);

	private class MemberAttributeChangedListener : EventListener<MemberAttributeChangedCallback>
	{
		public void Fire(BnetGameAccountId playerGameAccountId, Blizzard.GameService.Protocol.V2.Client.Attribute attribute)
		{
			m_callback(playerGameAccountId, attribute, m_userData);
		}
	}

	public delegate void MemberAttributeChangedCallback(BnetGameAccountId playerGameAccountId, Blizzard.GameService.Protocol.V2.Client.Attribute attribute, object userData);

	private class PartyAttributeChangedListener : EventListener<PartyAttributeChangedCallback>
	{
		public void Fire(Blizzard.GameService.Protocol.V2.Client.Attribute attribute)
		{
			m_callback(attribute, m_userData);
		}
	}

	public delegate void PartyAttributeChangedCallback(Blizzard.GameService.Protocol.V2.Client.Attribute attribute, object userData);

	private PartyData m_partyData = new PartyData();

	private DialogBase m_inviteDialog;

	private BnetPartyId m_pendingParty;

	public static int BATTLEGROUNDS_PARTY_LIMIT = 8;

	public static int BATTLEGROUNDS_MAX_RANKED_PARTY_SIZE_FALLBACK = 4;

	public static int MERCENARIES_PARTY_LIMIT = 2;

	private readonly Dictionary<int, int> BATTLEGROUNDS_DUO_SLOT_TO_TEAM = new Dictionary<int, int>
	{
		{ 1, 1 },
		{ 2, 1 },
		{ 3, 2 },
		{ 4, 2 },
		{ 5, 3 },
		{ 6, 3 },
		{ 7, 4 },
		{ 8, 4 }
	};

	private List<ChangedListener> m_changedListeners = new List<ChangedListener>();

	private List<MemberAttributeChangedListener> m_memberAttributeChangedListeners = new List<MemberAttributeChangedListener>();

	private List<PartyAttributeChangedListener> m_partyAttributeChangedListeners = new List<PartyAttributeChangedListener>();

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		m_partyData = new PartyData();
		BnetParty.OnJoined += BnetParty_OnJoined;
		BnetParty.OnReceivedInvite += BnetParty_OnReceivedInvite;
		BnetParty.OnPartyAttributeChanged += BnetParty_OnPartyAttributeChanged;
		BnetParty.OnMemberAttributeChanged += BnetParty_OnMemberAttributeChanged;
		BnetParty.OnMemberEvent += BnetParty_OnMemberEvent;
		BnetParty.OnSentInvite += BnetParty_OnSentInvite;
		BnetParty.OnReceivedInviteRequest += BnetParty_OnReceivedInviteRequest;
		BnetPresenceMgr.Get().AddPlayersChangedListener(OnPresenceUpdated);
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
		LoginManager.Get().OnInitialClientStateReceived += OnLoginComplete;
		HearthstoneApplication.Get().WillReset += WillReset;
		yield break;
	}

	public void Shutdown()
	{
		BnetParty.OnJoined -= BnetParty_OnJoined;
		BnetParty.OnReceivedInvite -= BnetParty_OnReceivedInvite;
		BnetParty.OnPartyAttributeChanged -= BnetParty_OnPartyAttributeChanged;
		BnetParty.OnMemberAttributeChanged -= BnetParty_OnMemberAttributeChanged;
		BnetParty.OnMemberEvent -= BnetParty_OnMemberEvent;
		BnetParty.OnSentInvite -= BnetParty_OnSentInvite;
		BnetParty.OnReceivedInviteRequest -= BnetParty_OnReceivedInviteRequest;
		BnetPresenceMgr.Get().RemovePlayersChangedListener(OnPresenceUpdated);
		FatalErrorMgr.Get().RemoveErrorListener(OnFatalError);
		LoginManager.Get().OnInitialClientStateReceived -= OnLoginComplete;
		HearthstoneApplication.Get().WillReset -= WillReset;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(LoginManager),
			typeof(Network)
		};
	}

	public static PartyManager Get()
	{
		return ServiceManager.Get<PartyManager>();
	}

	private void WillReset()
	{
		ClearPartyData();
	}

	public static bool IsPartyTypeEnabledInGuardian(PartyType partyType)
	{
		NetCache.NetCacheFeatures.CacheGames cacheGames = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games;
		if (cacheGames == null)
		{
			return false;
		}
		return partyType switch
		{
			PartyType.BATTLEGROUNDS_PARTY => cacheGames.BattlegroundsFriendlyChallenge, 
			PartyType.FRIENDLY_CHALLENGE => cacheGames.Friendly, 
			PartyType.MERCENARIES_FRIENDLY_CHALLENGE => cacheGames.MercenariesFriendly, 
			PartyType.MERCENARIES_COOP_PARTY => cacheGames.MercenariesCoOp, 
			_ => true, 
		};
	}

	public bool IsInParty()
	{
		return m_partyData.m_partyId != null;
	}

	public BnetPartyId GetCurrentPartyId()
	{
		return m_partyData.m_partyId;
	}

	public bool IsInPrivateBattlegroundsParty()
	{
		if (IsInBattlegroundsParty())
		{
			return m_partyData.m_private;
		}
		return false;
	}

	public bool IsInBattlegroundsParty()
	{
		if (IsInParty())
		{
			return m_partyData.m_type == PartyType.BATTLEGROUNDS_PARTY;
		}
		return false;
	}

	public bool IsInMercenariesFriendlyChallenge()
	{
		if (IsInParty())
		{
			return m_partyData.m_type == PartyType.MERCENARIES_FRIENDLY_CHALLENGE;
		}
		return false;
	}

	public bool IsInMercenariesCoOpParty()
	{
		if (IsInParty())
		{
			return m_partyData.m_type == PartyType.MERCENARIES_COOP_PARTY;
		}
		return false;
	}

	public bool IsPlayerInCurrentPartyOrPending(BnetGameAccountId playerGameAccountId)
	{
		if (IsPlayerInCurrentParty(playerGameAccountId))
		{
			return true;
		}
		if (IsPlayerPendingInCurrentParty(playerGameAccountId))
		{
			return true;
		}
		return false;
	}

	public bool IsPlayerInCurrentParty(BnetGameAccountId playerGameAccountId)
	{
		if (BnetParty.IsMember(m_partyData.m_partyId, playerGameAccountId))
		{
			return true;
		}
		return false;
	}

	public bool IsPlayerPendingInCurrentParty(BnetGameAccountId playerGameAccountId)
	{
		PartyInvite[] pendingInvites = GetPendingInvites();
		for (int i = 0; i < pendingInvites.Length; i++)
		{
			if (pendingInvites[i].InviteeId == playerGameAccountId)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsPlayerInAnyParty(BnetGameAccountId playerGameAccountId)
	{
		BnetPlayer player = BnetUtils.GetPlayer(playerGameAccountId);
		if (player == null)
		{
			return false;
		}
		BnetGameAccount playerGameAccount = player.GetHearthstoneGameAccount();
		if (playerGameAccount == null || playerGameAccount.GetGameFields() == null)
		{
			return false;
		}
		return playerGameAccount.GetPartyId() != BnetPartyId.Empty;
	}

	public bool IsPartyLeader()
	{
		return BnetParty.IsLeader(m_partyData.m_partyId);
	}

	public BnetParty.PartyMember GetPartyLeader()
	{
		BnetParty.PartyMember[] members = BnetParty.GetMembers(m_partyData.m_partyId);
		foreach (BnetParty.PartyMember partyMember in members)
		{
			if (partyMember.IsLeader(m_partyData.m_type))
			{
				return partyMember;
			}
		}
		return null;
	}

	public BnetGameAccountId GetPartyLeaderGameAccountId()
	{
		BnetParty.PartyMember leader = GetPartyLeader();
		if (leader == null)
		{
			Log.Party.PrintError("PartyManager - No party leader.");
			return null;
		}
		return leader.GameAccountId;
	}

	public bool CanInvite(BnetGameAccountId playerGameAccountId)
	{
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (!myself.IsOnline() || myself.IsAppearingOffline())
		{
			return false;
		}
		if (IsPlayerInAnyParty(playerGameAccountId))
		{
			return false;
		}
		if (IsPlayerPendingInCurrentParty(playerGameAccountId))
		{
			return false;
		}
		if (IsInParty() && GetCurrentPartySize() >= GetMaxPartySizeByPartyType(m_partyData.m_type))
		{
			return false;
		}
		BnetPlayer player = BnetUtils.GetPlayer(playerGameAccountId);
		if (player == null || !FriendChallengeMgr.Get().IsOpponentAvailable(player))
		{
			return false;
		}
		return true;
	}

	public bool CanKick(BnetGameAccountId playerGameAccountId)
	{
		if (IsInParty() && !IsPartyLeader())
		{
			return false;
		}
		if (!IsPlayerInCurrentPartyOrPending(playerGameAccountId))
		{
			return false;
		}
		return true;
	}

	public bool CanSpectatePartyMember(BnetGameAccountId gameAccountId)
	{
		JoinInfo joinInfo = GetSpectatorJoinInfoForPlayer(gameAccountId);
		if (joinInfo == null)
		{
			return false;
		}
		return SpectatorManager.Get().CanSpectate(gameAccountId, joinInfo);
	}

	public bool SpectatePartyMember(BnetGameAccountId gameAccountId)
	{
		JoinInfo joinInfo = GetSpectatorJoinInfoForPlayer(gameAccountId);
		if (joinInfo == null)
		{
			return false;
		}
		if (!CanSpectatePartyMember(gameAccountId))
		{
			return false;
		}
		SpectatorManager.Get().SpectatePlayer(gameAccountId, joinInfo);
		return true;
	}

	public void SendInvite(PartyType partyType, BnetGameAccountId playerGameAccountId)
	{
		if (CanInvite(playerGameAccountId) && !IsPlayerInCurrentPartyOrPending(playerGameAccountId))
		{
			if (!IsInParty() && ShouldSupportPartyType(partyType))
			{
				CreateParty(partyType, playerGameAccountId);
			}
			else if (partyType == PartyType.BATTLEGROUNDS_PARTY)
			{
				InvitePlayerToBattlegroundsParty(playerGameAccountId);
			}
			else
			{
				SendInvite_Internal(playerGameAccountId);
			}
		}
	}

	public void KickPlayerFromParty(BnetGameAccountId playerGameAccountId)
	{
		if (!IsInParty())
		{
			return;
		}
		if (BnetParty.IsMember(m_partyData.m_partyId, playerGameAccountId))
		{
			BnetParty.KickMember(m_partyData.m_partyId, playerGameAccountId);
			return;
		}
		ulong? inviteId = GetPendingInviteIdFromGameAccount(playerGameAccountId);
		if (inviteId.HasValue)
		{
			BnetNearbyPlayerMgr.Get().FindNearbyStranger(playerGameAccountId)?.GetHearthstoneGameAccount().SetGameField(1u, false);
			BnetParty.RevokeSentInvite(m_partyData.m_partyId, inviteId.Value);
			FireChangedEvent(PartyInviteEvent.I_RESCINDED_INVITE, playerGameAccountId);
		}
		else
		{
			Log.Party.PrintError("Unable to kick player {0} from party. Player not found in party.", playerGameAccountId.ToString());
		}
	}

	public void SendInviteSuggestion(PartyType partyType, BnetGameAccountId playerGameAccountId)
	{
		if (!CanInvite(playerGameAccountId) || IsPlayerInCurrentPartyOrPending(playerGameAccountId))
		{
			return;
		}
		if (!IsInParty() && ShouldSupportPartyType(partyType))
		{
			CreateParty(partyType, playerGameAccountId);
			return;
		}
		BnetGameAccountId partyLeaderGameAccountId = GetPartyLeaderGameAccountId();
		if (!(partyLeaderGameAccountId == null) && partyType == PartyType.BATTLEGROUNDS_PARTY)
		{
			BnetParty.RequestInvite(m_partyData.m_partyId, partyLeaderGameAccountId, playerGameAccountId, partyType);
		}
	}

	public void SetMyPlayerTagsAttribute()
	{
		if (Cheats.Get() != null && !string.IsNullOrEmpty(Cheats.Get().GetPlayerTags()))
		{
			BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyPlayer().GetBestGameAccountId();
			BattleNet.SetMemberAttributes(m_partyData.m_partyId, myGameAccountId, BnetAttribute.CreateAttribute("cheat_player_tags", Cheats.Get().GetPlayerTags()));
		}
	}

	public BnetParty.PartyMember[] GetMembers()
	{
		if (m_partyData.m_partyId == null)
		{
			return new BnetParty.PartyMember[0];
		}
		return BnetParty.GetMembers(m_partyData.m_partyId);
	}

	public PartyInvite[] GetPendingInvites()
	{
		if (m_partyData.m_partyId == null)
		{
			return new PartyInvite[0];
		}
		return BnetParty.GetSentInvites(m_partyData.m_partyId);
	}

	public void FindGame()
	{
		if (!IsInParty() || !ShouldSupportPartyType(m_partyData.m_type))
		{
			Log.Party.PrintError("FindGame - Unable to enter game unless you are in a supported party.");
			return;
		}
		BattleNet.SetPartyAttributes(m_partyData.m_partyId, BnetAttribute.CreateAttribute("queue", "in_queue"));
		switch (m_partyData.m_type)
		{
		case PartyType.BATTLEGROUNDS_PARTY:
		{
			string battlegroundsMode = "solo";
			int selectedBattlegroundsModeScenarioId = 3459;
			if (BattleNet.GetPartyAttribute(GetCurrentPartyId(), "battlegrounds_mode", out battlegroundsMode))
			{
				selectedBattlegroundsModeScenarioId = ((battlegroundsMode == "duos") ? 5173 : 3459);
			}
			Network.Get().EnterBattlegroundsWithParty(GetMembers(), selectedBattlegroundsModeScenarioId);
			break;
		}
		case PartyType.MERCENARIES_COOP_PARTY:
		{
			BnetParty.PartyMember mercenariesCoOpPlayer2 = GetOtherPartyMember();
			if (mercenariesCoOpPlayer2 == null)
			{
				Log.Lettuce.PrintError("PartyManager.FindGame() - Not enough party members.");
				return;
			}
			if (!BattleNet.GetPartyAttribute(m_partyData.m_partyId, "node_id", out int mapNodeId))
			{
				Log.Lettuce.PrintError("PartyManager.FindGame() - No map node selected.");
				return;
			}
			Network.Get().EnterMercenariesCoOpWithFriend(mercenariesCoOpPlayer2.GameAccountId, 3899, mapNodeId);
			break;
		}
		case PartyType.MERCENARIES_FRIENDLY_CHALLENGE:
		{
			BnetParty.PartyMember mercenariesFriendlyPlayer2 = GetOtherPartyMember();
			if (mercenariesFriendlyPlayer2 == null)
			{
				Log.Lettuce.PrintError("PartyManager.FindGame() - Not enough party members.");
				return;
			}
			BattleNet.GetMemberAttribute(m_partyData.m_partyId, BnetPresenceMgr.Get().GetMyGameAccountId(), "team_id", out long teamId1);
			BattleNet.GetMemberAttribute(m_partyData.m_partyId, mercenariesFriendlyPlayer2.GameAccountId, "team_id", out long teamId2);
			BattleNet.GetMemberAttribute(m_partyData.m_partyId, BnetPresenceMgr.Get().GetMyGameAccountId(), "ts_state", out long sharing1);
			BattleNet.GetMemberAttribute(m_partyData.m_partyId, mercenariesFriendlyPlayer2.GameAccountId, "ts_state", out long sharing2);
			if (teamId1 == 0L || teamId2 == 0L)
			{
				Log.Lettuce.PrintError($"PartyManager.FindGame() - Team not selected. Team1={teamId1}, Team2={teamId2}");
				return;
			}
			Network.Get().EnterMercenariesFriendlyChallenge(3743, teamId1, sharing1 == 2, teamId2, sharing2 == 2, mercenariesFriendlyPlayer2.GameAccountId);
			break;
		}
		}
		WaitForGame();
	}

	public BnetGameAccountId GetLeader()
	{
		if (!IsInParty())
		{
			return null;
		}
		BnetParty.PartyMember leader = BnetParty.GetLeader(m_partyData.m_partyId);
		if (leader != null)
		{
			return leader.GameAccountId;
		}
		Log.Party.PrintError("PartyManager.GetLeader() - Unable to get party leader!");
		return null;
	}

	public BnetParty.PartyMember GetOtherPartyMember()
	{
		BnetParty.PartyMember[] partyMembers = GetMembers();
		if (partyMembers.Length != 2)
		{
			Log.Lettuce.PrintWarning("PartyManager.GetOtherPartyMember() - This function only works with a party size of 2.");
			return null;
		}
		return partyMembers[1];
	}

	public string GetOpponentBestName()
	{
		BnetParty.PartyMember opponent = GetOtherPartyMember();
		if (opponent == null)
		{
			return string.Empty;
		}
		return GetPartyMemberName(opponent.GameAccountId);
	}

	public bool IsBaconParty()
	{
		if (m_partyData.m_partyId != null)
		{
			if (m_partyData.m_scenarioId != ScenarioDbId.TB_BACONSHOP_8P)
			{
				return m_partyData.m_scenarioId == ScenarioDbId.TB_BACONSHOP_DUOS;
			}
			return true;
		}
		return false;
	}

	public void LeaveParty()
	{
		if (IsInParty())
		{
			if (IsPartyLeader())
			{
				BnetParty.DissolveParty(m_partyData.m_partyId);
			}
			else
			{
				BnetParty.Leave(m_partyData.m_partyId);
			}
			ClearPartyData();
		}
	}

	public void CancelQueue()
	{
		BnetGameAccountId myAccountId = BnetPresenceMgr.Get().GetMyPlayer().GetHearthstoneGameAccountId();
		byte[] accountIdBlob = ProtobufUtil.ToByteArray(new BnetId
		{
			Hi = myAccountId.High,
			Lo = myAccountId.Low
		});
		BattleNet.SetPartyAttributes(m_partyData.m_partyId, BnetAttribute.CreateAttribute("canceled_by", accountIdBlob));
		BattleNet.SetPartyAttributes(m_partyData.m_partyId, BnetAttribute.CreateAttribute("queue", "cancel_queue"));
	}

	public int GetCurrentPartySize()
	{
		return GetCurrentAndPendingPartyMembers().Count();
	}

	public int GetReadyPartyMemberCount()
	{
		int readyCount = 0;
		BnetPresenceMgr.Get().GetMyPlayer();
		BnetParty.PartyMember[] members = BnetParty.GetMembers(m_partyData.m_partyId);
		for (int i = 0; i < members.Length; i++)
		{
			BaconParty.Status memberStatus = BaconParty.GetReadyStatusForPartyMember(members[i].GameAccountId);
			if ((uint)(memberStatus - 2) <= 1u)
			{
				readyCount++;
			}
		}
		return readyCount;
	}

	public List<BnetGameAccountId> GetCurrentAndPendingPartyMembers()
	{
		List<BnetGameAccountId> totalPartyMemberList = new List<BnetGameAccountId>();
		BnetParty.PartyMember[] members = BnetParty.GetMembers(m_partyData.m_partyId);
		foreach (BnetParty.PartyMember partyMember in members)
		{
			totalPartyMemberList.Add(partyMember.GameAccountId);
		}
		PartyInvite[] pendingInvites = GetPendingInvites();
		for (int i = 0; i < pendingInvites.Length; i++)
		{
			BnetGameAccountId pendingPlayer = pendingInvites[i].InviteeId;
			if (!totalPartyMemberList.Contains(pendingPlayer))
			{
				totalPartyMemberList.Add(pendingPlayer);
			}
		}
		return totalPartyMemberList;
	}

	public int GetMaxPartySizeByPartyType(PartyType type)
	{
		switch (type)
		{
		case PartyType.BATTLEGROUNDS_PARTY:
			return BATTLEGROUNDS_PARTY_LIMIT;
		case PartyType.MERCENARIES_FRIENDLY_CHALLENGE:
		case PartyType.MERCENARIES_COOP_PARTY:
			return MERCENARIES_PARTY_LIMIT;
		default:
			Log.Party.PrintError("GetMaxPartySizeByPartyType() - Unsupported party type {0}.", type.ToString());
			return 2;
		}
	}

	public void UpdateSpectatorJoinInfo(JoinInfo joinInfo)
	{
		if (IsInParty())
		{
			BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
			byte[] joinInfoBytes = ((joinInfo == null) ? null : ProtobufUtil.ToByteArray(joinInfo));
			BattleNet.SetMemberAttributes(m_partyData.m_partyId, myGameAccountId, BnetAttribute.CreateAttribute("spectator_info", joinInfoBytes));
		}
	}

	public JoinInfo GetSpectatorJoinInfoForPlayer(BnetGameAccountId gameAccountId)
	{
		if (!IsInParty())
		{
			return null;
		}
		BattleNet.GetMemberAttribute(m_partyData.m_partyId, gameAccountId, "spectator_info", out byte[] blob);
		if (blob != null && blob.Length != 0)
		{
			return ProtobufUtil.ParseFrom<JoinInfo>(blob);
		}
		return null;
	}

	public int GetBattlegroundsPartyMemberRating(BnetGameAccountId playerGameAccountId)
	{
		int rating = 0;
		if (!IsInBattlegroundsParty())
		{
			return rating;
		}
		if (BattleNet.GetMemberAttribute(GetCurrentPartyId(), playerGameAccountId, "battlegrounds_mode_rating", out int playerRating))
		{
			rating = playerRating;
		}
		return rating;
	}

	public string GetPartyMemberName(BnetGameAccountId playerGameAccountId)
	{
		BnetPlayer player = BnetUtils.GetPlayer(playerGameAccountId);
		if (player != null)
		{
			return player.GetBestName();
		}
		BnetParty.PartyMember[] members = GetMembers();
		foreach (BnetParty.PartyMember member in members)
		{
			if (!(member.GameAccountId == playerGameAccountId))
			{
				continue;
			}
			if (!string.IsNullOrEmpty(member.BattleTag))
			{
				BnetBattleTag battleTag = BnetBattleTag.CreateFromString(member.BattleTag);
				if (!(battleTag == null))
				{
					return battleTag.GetName();
				}
				return member.BattleTag;
			}
			Log.Party.PrintError("GetPartyMemberName() - No name for party member {0}.", playerGameAccountId.ToString());
		}
		PartyInvite[] pendingInvites = GetPendingInvites();
		foreach (PartyInvite invite in pendingInvites)
		{
			if (!(invite.InviteeId == playerGameAccountId))
			{
				continue;
			}
			if (!string.IsNullOrEmpty(invite.InviteeName))
			{
				BnetBattleTag battleTag2 = BnetBattleTag.CreateFromString(invite.InviteeName);
				if (!(battleTag2 == null))
				{
					return battleTag2.GetName();
				}
				return invite.InviteeName;
			}
			Log.Party.PrintError("GetPartyMemberName() - No name for pending invitee {0}.", playerGameAccountId.ToString());
		}
		return GameStrings.Get("GLUE_PARTY_MEMBER_NO_NAME");
	}

	public bool HasPendingPartyInviteOrDialog()
	{
		if (!(m_pendingParty != null))
		{
			return m_inviteDialog != null;
		}
		return true;
	}

	public void SetReadyStatus(bool ready)
	{
		if (IsInParty())
		{
			BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyPlayer().GetBestGameAccountId();
			BattleNet.SetMemberAttributes(m_partyData.m_partyId, myGameAccountId, BnetAttribute.CreateAttribute("ready", ready ? "ready" : "not_ready"));
		}
	}

	public void SetSceneAttribute(string scene)
	{
		if (IsInParty())
		{
			BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyPlayer().GetBestGameAccountId();
			BattleNet.SetMemberAttributes(m_partyData.m_partyId, myGameAccountId, BnetAttribute.CreateAttribute("scene", scene));
		}
	}

	public bool AreAllPartyMembersReady()
	{
		BnetParty.PartyMember[] members = GetMembers();
		foreach (BnetParty.PartyMember partyMember in members)
		{
			if (!BattleNet.GetMemberAttribute(m_partyData.m_partyId, partyMember.GameAccountId, "ready", out string readyStatus) || string.IsNullOrEmpty(readyStatus) || readyStatus == "not_ready")
			{
				return false;
			}
		}
		return true;
	}

	private string GetGameStringPartyTitleKey()
	{
		if (IsBaconParty())
		{
			return "GLUE_BACON_PRIVATE_PARTY_TITLE";
		}
		return "GLOBAL_FRIEND_PARTY_INVITATION_TITLE";
	}

	public void UpdateBattlegroundsStrikeMemberAttribute()
	{
		if (IsInBattlegroundsParty())
		{
			BnetPartyId bnetPartyId = GetCurrentPartyId();
			BnetGameAccountId bnetGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
			SetMemberAttributeBattlegroundsStrikeId(bnetPartyId, bnetGameAccountId);
		}
	}

	public void SetBattlegroundsPrivateParty(bool isPrivate)
	{
		m_partyData.m_private = true;
		Blizzard.GameService.Protocol.V2.Client.Attribute privatePartyAttr = BnetAttribute.CreateAttribute("battlegrounds_private", isPrivate);
		BattleNet.SetPartyAttributes(m_partyData.m_partyId, privatePartyAttr);
		FirePartyAttributeChangedEvent(privatePartyAttr);
	}

	public int GetBattlegroundsMaxRankedPartySize()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>()?.BattlegroundsMaxRankedPartySize ?? BATTLEGROUNDS_MAX_RANKED_PARTY_SIZE_FALLBACK;
	}

	public bool IsPartySizeValidForCurrentGameMode()
	{
		if (!IsInBattlegroundsParty() || !BaconLobbyMgr.Get().IsInDuosMode())
		{
			return true;
		}
		int currentMemberCount = GetCurrentAndPendingPartyMembers().Count;
		bool isPrivate = currentMemberCount > GetBattlegroundsMaxRankedPartySize();
		Map<BnetGameAccountId, int> desiredDuosTeams = GetDesiredDuosTeams(isDuos: true, GetMembers());
		int uniqueTeams = 0;
		Dictionary<int, bool> seenTeam = new Dictionary<int, bool>
		{
			{ 1, false },
			{ 2, false },
			{ 3, false },
			{ 4, false }
		};
		foreach (int team in desiredDuosTeams.Values)
		{
			if (seenTeam.ContainsKey(team) && !seenTeam[team])
			{
				uniqueTeams++;
				seenTeam[team] = true;
			}
		}
		bool uniqueTeamCountIsValid = (double)uniqueTeams <= Math.Ceiling((float)currentMemberCount * 0.5f);
		if (isPrivate)
		{
			return currentMemberCount % 2 == 0 && uniqueTeamCountIsValid;
		}
		return uniqueTeamCountIsValid;
	}

	private void InvitePlayerToBattlegroundsParty(BnetGameAccountId playerGameAccountId)
	{
		if (GetCurrentPartySize() < BATTLEGROUNDS_PARTY_LIMIT)
		{
			SendInvite_Internal(playerGameAccountId);
		}
	}

	private void ShowDeclinedInvitationPopup(BnetGameAccountId gameAccountId)
	{
		BnetPlayer player = BnetUtils.GetPlayer(gameAccountId);
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get(GetGameStringPartyTitleKey());
		string uniqueName = ((player != null) ? FriendUtils.GetUniqueName(player) : GameStrings.Format("GLOBAL_PLAYER_PLAYER"));
		info.m_text = GameStrings.Format("GLOBAL_FRIEND_PARTY_INVITATION_BODY_DECLINED", uniqueName);
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		info.m_showAlertIcon = false;
		info.m_okText = GameStrings.Get("GLOBAL_OKAY");
		DialogManager.Get().ShowPopup(info);
	}

	public void SetMySelectedBattlegroundsDuosTeamSlotId(long slotId)
	{
		BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get()?.GetMyPlayer()?.GetBestGameAccountId();
		SetSelectedBattlegroundsDuosTeamSlotId(slotId, myGameAccountId);
	}

	public void UpdateMyBattlegroundsModeRating()
	{
		if (IsInBattlegroundsParty())
		{
			BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get()?.GetMyPlayer()?.GetBestGameAccountId();
			int rating = BaconLobbyMgr.Get().GetBattlegroundsActiveGameModeRating();
			BattleNet.SetMemberAttributes(m_partyData.m_partyId, myGameAccountId, BnetAttribute.CreateAttribute("battlegrounds_mode_rating", rating));
		}
	}

	public void SetSelectedBattlegroundsDuosTeamSlotId(long slotId, BnetGameAccountId memberId)
	{
		if (IsPartyLeader() && BnetParty.GetMember(GetCurrentPartyId(), memberId) != null)
		{
			long selectedSlotId = 0L;
			if (slotId >= 0 && slotId <= BATTLEGROUNDS_PARTY_LIMIT)
			{
				selectedSlotId = slotId;
			}
			BattleNet.SetMemberAttributes(m_partyData.m_partyId, memberId, BnetAttribute.CreateAttribute("battlegrounds_duos_team_slot", selectedSlotId));
		}
	}

	public int GetPlayerDuosSlot(BnetGameAccountId gameAccountId)
	{
		int returnSlot = 0;
		if (!IsInBattlegroundsParty())
		{
			return returnSlot;
		}
		if (BattleNet.GetMemberAttribute(GetCurrentPartyId(), gameAccountId, "battlegrounds_duos_team_slot", out long slot))
		{
			returnSlot = (int)slot;
		}
		return returnSlot;
	}

	public void SetSelectedBattlegroundsGameMode(string gameMode)
	{
		if (IsPartyLeader())
		{
			string selectedGameMode = "solo";
			if (gameMode == "solo" || gameMode == "duos")
			{
				selectedGameMode = gameMode;
				m_partyData.m_scenarioId = (BaconLobbyMgr.Get().IsInDuosMode() ? ScenarioDbId.TB_BACONSHOP_DUOS : ScenarioDbId.TB_BACONSHOP_8P);
			}
			BattleNet.SetPartyAttributes(m_partyData.m_partyId, BnetAttribute.CreateAttribute("battlegrounds_mode", selectedGameMode));
			BattleNet.SetPartyAttributes(m_partyData.m_partyId, BnetAttribute.CreateAttribute("WTCG.Game.ScenarioId", (long)m_partyData.m_scenarioId));
		}
	}

	public string GetSelectedBattlegroundsGameMode()
	{
		if (!IsInBattlegroundsParty())
		{
			return null;
		}
		string selectedGameMode = "solo";
		if (BattleNet.GetPartyAttribute(GetCurrentPartyId(), "battlegrounds_mode", out string realGameMode))
		{
			selectedGameMode = realGameMode;
		}
		return selectedGameMode;
	}

	private void SetMemberAttributeBattlegroundsStrikeId(BnetPartyId bnetPartyId, BnetGameAccountId bnetGameAccountId)
	{
		NetCache.NetCacheBattlegroundsFinishers netCacheBGFinishers = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsFinishers>();
		if (netCacheBGFinishers == null || netCacheBGFinishers.BattlegroundsFavoriteFinishers == null || netCacheBGFinishers.BattlegroundsFavoriteFinishers.Count == 0)
		{
			Blizzard.GameService.Protocol.V2.Client.Attribute defaultStrikeIdAttribute = BnetAttribute.CreateAttribute("battlegrounds_strike_id", 1);
			BattleNet.SetMemberAttributes(bnetPartyId, bnetGameAccountId, defaultStrikeIdAttribute);
		}
		else
		{
			int randomStrikeIndex = UnityEngine.Random.Range(0, netCacheBGFinishers.BattlegroundsFavoriteFinishers.Count);
			Blizzard.GameService.Protocol.V2.Client.Attribute strikeIdAttribute = BnetAttribute.CreateAttribute("battlegrounds_strike_id", netCacheBGFinishers.BattlegroundsFavoriteFinishers.ToList()[randomStrikeIndex].ToValue());
			BattleNet.SetMemberAttributes(bnetPartyId, bnetGameAccountId, strikeIdAttribute);
		}
	}

	private void OnBattlegroundsSuggestionReceivedResponse(bool accept, BnetGameAccountId playerToInvite)
	{
		if (accept)
		{
			InvitePlayerToBattlegroundsParty(playerToInvite);
		}
	}

	public void UpdateBattlegroundsBoardSkinMemberAttribute()
	{
		if (IsInBattlegroundsParty())
		{
			BnetPartyId bnetPartyId = GetCurrentPartyId();
			BnetGameAccountId bnetGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
			SetMemberAttributeBattlegroundsBoardSkinId(bnetPartyId, bnetGameAccountId);
		}
	}

	private void SetMemberAttributeBattlegroundsBoardSkinId(BnetPartyId bnetPartyId, BnetGameAccountId bnetGameAccountId)
	{
		NetCache.NetCacheBattlegroundsBoardSkins netCacheBattlegroundsBoardSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsBoardSkins>();
		if (netCacheBattlegroundsBoardSkins == null || netCacheBattlegroundsBoardSkins.BattlegroundsFavoriteBoardSkins == null || netCacheBattlegroundsBoardSkins.BattlegroundsFavoriteBoardSkins.Count == 0)
		{
			Blizzard.GameService.Protocol.V2.Client.Attribute defaultBoardSkinAttribute = BnetAttribute.CreateAttribute("battlegrounds_board_skin_id", 1);
			BattleNet.SetMemberAttributes(bnetPartyId, bnetGameAccountId, defaultBoardSkinAttribute);
		}
		else
		{
			int randomBoardSkinIndex = UnityEngine.Random.Range(0, netCacheBattlegroundsBoardSkins.BattlegroundsFavoriteBoardSkins.Count);
			Blizzard.GameService.Protocol.V2.Client.Attribute boardSkinIdAttribute = BnetAttribute.CreateAttribute("battlegrounds_board_skin_id", netCacheBattlegroundsBoardSkins.BattlegroundsFavoriteBoardSkins.ToList()[randomBoardSkinIndex].ToValue());
			BattleNet.SetMemberAttributes(bnetPartyId, bnetGameAccountId, boardSkinIdAttribute);
		}
	}

	public void SetSelectedMercenariesCoOpMapNodeId(int mapNodeId)
	{
		BattleNet.SetPartyAttributes(m_partyData.m_partyId, BnetAttribute.CreateAttribute("node_id", (long)mapNodeId));
	}

	public void SetSelectedMercenariesTeamId(long teamId)
	{
		BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get()?.GetMyPlayer()?.GetBestGameAccountId();
		BattleNet.SetMemberAttributes(m_partyData.m_partyId, myGameAccountId, BnetAttribute.CreateAttribute("team_id", teamId));
	}

	public void SetTeamSharingMsg(MercTeamShareMSG msg)
	{
		BnetGameAccountId myGameAccountHandle = BnetPresenceMgr.Get()?.GetMyPlayer()?.GetBestGameAccountId();
		BattleNet.SetMemberAttributes(m_partyData.m_partyId, myGameAccountHandle, BnetAttribute.CreateAttribute("ts_MSG", (long)msg));
	}

	public void SetOpponentTeamSharingButtonStatus(MercTeamSharingButtonStatus shareButtonStatus)
	{
		BnetGameAccountId myGameAccountHandle = BnetPresenceMgr.Get()?.GetMyPlayer()?.GetBestGameAccountId();
		BattleNet.SetMemberAttributes(m_partyData.m_partyId, myGameAccountHandle, BnetAttribute.CreateAttribute("ts_status", (long)shareButtonStatus));
	}

	public MercTeamSharingButtonStatus GetMyTeamSharingButtonStatus()
	{
		BnetGameAccountId accountId = GetOtherPartyMember()?.GameAccountId;
		BattleNet.GetMemberAttribute(m_partyData.m_partyId, accountId, "ts_status", out long shareButtonStatus);
		return (MercTeamSharingButtonStatus)shareButtonStatus;
	}

	public void SetTeamSharingState(MercTeamShareState shareState)
	{
		BnetGameAccountId myGameAccountHandle = BnetPresenceMgr.Get()?.GetMyPlayer()?.GetBestGameAccountId();
		BattleNet.SetMemberAttributes(m_partyData.m_partyId, myGameAccountHandle, BnetAttribute.CreateAttribute("ts_state", (long)shareState));
	}

	public MercTeamShareState GetTeamSharingState(bool getOpponentState = false)
	{
		BattleNet.GetMemberAttribute(partyMember: (!getOpponentState) ? BnetPresenceMgr.Get()?.GetMyPlayer()?.GetBestGameAccountId() : GetOtherPartyMember()?.GameAccountId, partyId: m_partyData.m_partyId, attributeKey: "ts_state", value: out long shareState);
		return (MercTeamShareState)shareState;
	}

	public void SetSharedTeams(LettuceTeamList teamList)
	{
		BnetGameAccountId myGameAccountHandle = BnetPresenceMgr.Get()?.GetMyPlayer()?.GetBestGameAccountId();
		byte[] teamBlob = ProtobufUtil.ToByteArray(teamList);
		BattleNet.SetMemberAttributes(m_partyData.m_partyId, myGameAccountHandle, BnetAttribute.CreateAttribute("ts_teams", teamBlob));
	}

	public LettuceTeamList GetSharedTeams()
	{
		BnetGameAccountId accountId = GetOtherPartyMember()?.GameAccountId;
		if (BattleNet.GetMemberAttribute(m_partyData.m_partyId, accountId, "ts_teams", out byte[] teamBlob) && teamBlob != null)
		{
			return ProtobufUtil.ParseFrom<LettuceTeamList>(teamBlob);
		}
		return null;
	}

	public long GetOpponentSelectedTeam()
	{
		BnetParty.PartyMember opponent = GetOtherPartyMember();
		if (opponent != null && BattleNet.GetMemberAttribute(m_partyData.m_partyId, opponent.GameAccountId, "team_id", out long opponentTeam))
		{
			return opponentTeam;
		}
		return 0L;
	}

	public void StartMercenariesFriendlyChallengeEntry(BnetPlayer opponent)
	{
		AddChangedListener(HandleMercenaryFriendlyChallengeNotifications, opponent);
		SendInvite(PartyType.MERCENARIES_FRIENDLY_CHALLENGE, opponent.GetBestGameAccountId());
	}

	private void ClearPartyData()
	{
		m_partyData = new PartyData();
		UpdateMyAvailability();
	}

	private bool ShouldSupportPartyType(PartyType partyType)
	{
		if ((uint)(partyType - 3) <= 2u)
		{
			return true;
		}
		return false;
	}

	private void WaitForGame()
	{
		GameMgr.Get().WaitForFriendChallengeToStart(m_partyData.m_format, BrawlType.BRAWL_TYPE_UNKNOWN, (int)m_partyData.m_scenarioId, 0, m_partyData.m_type);
	}

	private ScenarioDbId GetScenario(PartyType type, bool isDuos)
	{
		switch (type)
		{
		case PartyType.BATTLEGROUNDS_PARTY:
			if (!isDuos)
			{
				return ScenarioDbId.TB_BACONSHOP_8P;
			}
			return ScenarioDbId.TB_BACONSHOP_DUOS;
		case PartyType.MERCENARIES_COOP_PARTY:
			return ScenarioDbId.LETTUCE_MAP_COOP;
		case PartyType.MERCENARIES_FRIENDLY_CHALLENGE:
			return ScenarioDbId.LETTUCE_1v1;
		default:
			Log.Party.PrintError("PartyManager.GetScenario() received an unsupported party type: {0}", type);
			return ScenarioDbId.INVALID;
		}
	}

	private FormatType GetFormat(PartyType type)
	{
		return FormatType.FT_UNKNOWN;
	}

	private int GetSeason(PartyType type)
	{
		return 0;
	}

	private SceneMgr.Mode GetMode(PartyType type)
	{
		switch (type)
		{
		case PartyType.BATTLEGROUNDS_PARTY:
			return SceneMgr.Mode.BACON;
		case PartyType.MERCENARIES_COOP_PARTY:
			return SceneMgr.Mode.LETTUCE_COOP;
		case PartyType.MERCENARIES_FRIENDLY_CHALLENGE:
			return SceneMgr.Mode.LETTUCE_FRIENDLY;
		default:
			Log.Party.PrintError("PartyManager.GetMode() received an unsupported party type: {0}", type);
			return SceneMgr.Mode.HUB;
		}
	}

	private bool HasCompletedRequiredTutorialForPartyType(PartyType type)
	{
		switch (type)
		{
		case PartyType.BATTLEGROUNDS_PARTY:
			return GameUtils.IsBattleGroundsTutorialComplete();
		case PartyType.MERCENARIES_FRIENDLY_CHALLENGE:
		case PartyType.MERCENARIES_COOP_PARTY:
			return GameUtils.IsMercenariesVillageTutorialComplete();
		default:
			Log.Party.PrintError("PartyManager.HasCompletedRequiredTutorialForPartyType() received an unsupported party type: {0}", type);
			return true;
		}
	}

	private void CreateParty(PartyType type, BnetGameAccountId playerToInvite)
	{
		if (IsInParty())
		{
			return;
		}
		m_partyData.m_type = type;
		m_partyData.m_scenarioId = GetScenario(type, BaconLobbyMgr.Get().IsInDuosMode());
		m_partyData.m_format = GetFormat(type);
		m_partyData.m_season = GetSeason(type);
		List<Blizzard.GameService.Protocol.V2.Client.Attribute> partyAttributes = BnetAttribute.CreateAttributeCollection(BnetAttribute.CreateAttribute("WTCG.Game.ScenarioId", (long)m_partyData.m_scenarioId), BnetAttribute.CreateAttribute("WTCG.Format.Type", (long)m_partyData.m_format), BnetAttribute.CreateAttribute("WTCG.Season.Id", (long)m_partyData.m_season));
		if (type == PartyType.BATTLEGROUNDS_PARTY)
		{
			partyAttributes.Add(BnetAttribute.CreateAttribute("battlegrounds_mode", BaconLobbyMgr.Get().GetBattlegroundsGameMode()));
			partyAttributes.Add(BnetAttribute.CreateAttribute("battlegrounds_private", val: false));
		}
		if (GetPartyQuestInfoBlob(out var questInfoBlob))
		{
			partyAttributes.Add(BnetAttribute.CreateAttribute("WTCG.Party.QuestInfo", questInfoBlob));
		}
		BnetParty.CreateParty(type, ChannelApi.PartyPrivacyLevel.OpenInvitation, delegate(PartyType pType, BnetPartyId newlyCreatedPartyId)
		{
			m_partyData.m_partyId = newlyCreatedPartyId;
			UpdateMyAvailability();
			BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
			InitializePersonalPartyMemberAttributes(pType);
			FireChangedEvent(PartyInviteEvent.I_CREATED_PARTY, myGameAccountId);
			if (playerToInvite != null)
			{
				SendInvite(type, playerToInvite);
			}
		}, partyAttributes);
	}

	private bool GetPartyQuestInfoBlob(out byte[] questInfoBlob)
	{
		questInfoBlob = null;
		IEnumerable<Achievement> friendlyQuests = from q in AchieveManager.Get().GetActiveQuests()
			where q.IsFriendlyChallengeQuest
			select q;
		if (friendlyQuests.Any())
		{
			PartyQuestInfo info = new PartyQuestInfo();
			info.QuestIds.AddRange(friendlyQuests.Select((Achievement q) => q.ID));
			questInfoBlob = ProtobufUtil.ToByteArray(info);
			return true;
		}
		return false;
	}

	private void InitializePersonalPartyMemberAttributes(PartyType partyType)
	{
		if (partyType == PartyType.BATTLEGROUNDS_PARTY)
		{
			SetReadyStatus(ready: false);
			SetMyPlayerTagsAttribute();
			UpdateBattlegroundsStrikeMemberAttribute();
			UpdateBattlegroundsBoardSkinMemberAttribute();
			UpdateMyBattlegroundsModeRating();
			if (BaconLobbyMgr.Get().GetBattlegroundsGameMode() != GetSelectedBattlegroundsGameMode())
			{
				BaconLobbyMgr.Get().SetBattlegroundsGameMode(GetSelectedBattlegroundsGameMode());
			}
		}
	}

	private void SendInvite_Internal(BnetGameAccountId bnetGameAccountId)
	{
		BnetParty.SendInvite(m_partyData.m_partyId, bnetGameAccountId, isReservation: true);
	}

	private void UpdateMyAvailability()
	{
		if (Network.ShouldBeConnectedToAurora() && Network.IsLoggedIn())
		{
			BnetPartyId partyId = m_partyData.m_partyId;
			BnetPresenceMgr.Get().SetGameField(26u, (partyId != null) ? partyId.ToBnetEntityId() : BnetPartyId.Empty.ToBnetEntityId());
			BnetNearbyPlayerMgr.Get().SetPartyId(partyId ?? BnetPartyId.Empty);
		}
	}

	private void ShowInviteDialog(BnetGameAccountId leaderGameAccountId, string inviterBattleTag, PartyType partyType)
	{
		BnetPlayer leader = BnetUtils.GetPlayer(leaderGameAccountId);
		if (leader == null)
		{
			Log.Party.PrintDebug("PartyManager.ShowInviteDialog() - Received invite from player {0} with no presence!", leaderGameAccountId);
			BnetAccount bnetAccount = new BnetAccount();
			BnetBattleTag bnetBattleTag = new BnetBattleTag();
			bnetBattleTag.SetString(inviterBattleTag);
			bnetAccount.SetBattleTag(bnetBattleTag);
			leader = new BnetPlayer(BnetPlayerSource.UNASSIGNED);
			leader.SetAccount(bnetAccount);
		}
		if (partyType == PartyType.BATTLEGROUNDS_PARTY && !HasGameDataForPartyType(partyType))
		{
			DialogManager.Get().ShowFriendlyChallenge(FormatType.FT_UNKNOWN, leader, challengeIsTavernBrawl: false, partyType, FriendChallengeMgr.Get().GetPartyQuestInfo(m_pendingParty, "WTCG.Party.QuestInfo"), OnBGInviteReceivedNoDataDialogResponse, OnInviteReceivedDialogProcessed);
		}
		else
		{
			DialogManager.Get().ShowFriendlyChallenge(FormatType.FT_UNKNOWN, leader, challengeIsTavernBrawl: false, partyType, FriendChallengeMgr.Get().GetPartyQuestInfo(m_pendingParty, "WTCG.Party.QuestInfo"), OnInviteReceivedDialogResponse, OnInviteReceivedDialogProcessed);
		}
	}

	private void HandleMercenaryFriendlyChallengeNotifications(PartyInviteEvent challengeEvent, BnetGameAccountId playerGameAccountId, PartyData challengeData, object userData)
	{
		BnetPlayer opponent = (BnetPlayer)userData;
		string challengeCanceledMessage = null;
		switch (challengeEvent)
		{
		case PartyInviteEvent.I_CREATED_PARTY:
			ShowMercenaryFriendlyChallengeWaitForOpponent(opponent);
			break;
		case PartyInviteEvent.FRIEND_DECLINED_INVITE:
			challengeCanceledMessage = GameStrings.Format("GLOBAL_FRIEND_CHALLENGE_OPPONENT_DECLINED", opponent.GetBestName());
			break;
		case PartyInviteEvent.I_RESCINDED_INVITE:
		case PartyInviteEvent.INVITE_EXPIRED:
		case PartyInviteEvent.FRIEND_LEFT:
		case PartyInviteEvent.LEADER_DISSOLVED_PARTY:
			if (playerGameAccountId == opponent.GetBestGameAccountId())
			{
				challengeCanceledMessage = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_QUEUE_CANCELED");
			}
			break;
		case PartyInviteEvent.FRIEND_ACCEPTED_INVITE:
			DialogManager.Get().ClearAllImmediately();
			RemoveChangedListener(HandleMercenaryFriendlyChallengeNotifications, userData);
			NavigateToMercenaryFriendlyChallengeScreen();
			break;
		}
		if (challengeCanceledMessage != null)
		{
			ShowMercenaryFriendlyChallengeCanceled(challengeCanceledMessage, userData);
		}
	}

	private void ShowMercenaryFriendlyChallengeWaitForOpponent(BnetPlayer opponent)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER");
		info.m_text = GameStrings.Format("GLOBAL_FRIEND_CHALLENGE_OPPONENT_WAITING_RESPONSE", opponent.GetBestName());
		info.m_showAlertIcon = false;
		info.m_responseCallback = OnMercenaryFriendlyChallengeCancelPressed;
		info.m_responseUserData = opponent;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CANCEL;
		info.m_layerToUse = GameLayer.UI;
		DialogManager.Get().ShowPopup(info);
	}

	private void OnMercenaryFriendlyChallengeCancelPressed(AlertPopup.Response response, object userData)
	{
		RemoveChangedListener(HandleMercenaryFriendlyChallengeNotifications, userData);
		LeaveParty();
	}

	private void ShowMercenaryFriendlyChallengeCanceled(string message, object userData)
	{
		DialogManager.Get().ClearAllImmediately();
		RemoveChangedListener(HandleMercenaryFriendlyChallengeNotifications, userData);
		ShowSimpleAlertDialog(GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER"), message);
		LeaveParty();
	}

	private void NavigateToMercenaryFriendlyChallengeScreen()
	{
		GameMgr.Get().SetPendingAutoConcede(pendingAutoConcede: true);
		CollectionManager cm = CollectionManager.Get();
		if (cm.IsInEditMode())
		{
			cm.GetEditedDeck()?.SendChanges(CollectionDeck.ChangeSource.NavigateToSceneForPartyChallenge);
		}
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.LETTUCE_FRIENDLY);
	}

	private void ShowSimpleAlertDialog(string header, string body, bool showAlertIcon = false, UberText.AlignmentOptions textAlignment = UberText.AlignmentOptions.Left)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get(header);
		info.m_text = GameStrings.Get(body);
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		info.m_showAlertIcon = showAlertIcon;
		info.m_okText = GameStrings.Get("GLOBAL_OKAY");
		info.m_alertTextAlignment = textAlignment;
		DialogManager.Get().ShowPopup(info);
	}

	private bool OnInviteReceivedDialogProcessed(DialogBase dialog, object userData)
	{
		m_inviteDialog = dialog;
		return true;
	}

	private void UnlockBattlegrounds()
	{
		Network.Get().UnlockBattlegroundsDuringApprentice();
	}

	private void OnBGInviteReceivedNoDataDialogResponse(bool accept)
	{
		if (accept)
		{
			Box.Get().HandleBattleGroundDownloadRequired("GLUE_BACON_INVITE_NEW_PLAYER_DOWNLOAD");
			DeclinePartyInvite(m_partyData.m_inviteId);
		}
		else
		{
			DeclinePartyInvite(m_partyData.m_inviteId);
			if (!GameModeUtils.HasUnlockedMode(Global.UnlockableGameMode.BATTLEGROUNDS))
			{
				ShowSimpleAlertDialog("GLUE_BACON_INVITE_NEW_PLAYER_DECLINED_TITLE", "GLUE_BACON_INVITE_NEW_PLAYER_DECLINED_BODY_PLAYER", showAlertIcon: false, UberText.AlignmentOptions.Center);
			}
		}
		m_inviteDialog.Hide();
		m_inviteDialog = null;
		m_pendingParty = null;
		FriendChallengeMgr.Get().UpdateMyAvailability();
	}

	private void OnInviteReceivedDialogResponse(bool accept)
	{
		BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
		if (accept)
		{
			if (BnetPresenceMgr.Get().GetMyPlayer().IsAppearingOffline())
			{
				DeclinePartyInvite(m_partyData.m_inviteId);
				ShowSimpleAlertDialog("GLUE_BACON_INVITE_WHILE_APPEARING_OFFLINE_HEADER", "GLUE_BACON_INVITE_WHILE_APPEARING_OFFLINE", showAlertIcon: true);
			}
			else if (m_pendingParty != null && !IsInParty())
			{
				m_partyData.m_partyId = m_pendingParty;
				BnetParty.AcceptReceivedInvite(m_partyData.m_inviteId);
				UpdateMyAvailability();
				FireChangedEvent(PartyInviteEvent.I_ACCEPTED_INVITE, myGameAccountId);
				TransitionModeIfNeeded();
				UnlockBattlegrounds();
			}
			else if (IsInParty())
			{
				ShowSimpleAlertDialog("GLUE_BACON_EXPIRED_INVITE_HEADER", "GLUE_BACON_PARTY_INVITE_WHILE_IN_PARTY");
			}
			else
			{
				ShowSimpleAlertDialog("GLUE_BACON_EXPIRED_INVITE_HEADER", "GLUE_BACON_EXPIRD_INVITE_BODY");
			}
		}
		else
		{
			DeclinePartyInvite(m_partyData.m_inviteId);
			if (m_partyData.m_type == PartyType.BATTLEGROUNDS_PARTY && !GameModeUtils.HasUnlockedMode(Global.UnlockableGameMode.BATTLEGROUNDS))
			{
				ShowSimpleAlertDialog("GLUE_BACON_INVITE_NEW_PLAYER_DECLINED_TITLE", "GLUE_BACON_INVITE_NEW_PLAYER_DECLINED_BODY_PLAYER", showAlertIcon: false, UberText.AlignmentOptions.Center);
			}
		}
		m_inviteDialog = null;
		m_pendingParty = null;
		FriendChallengeMgr.Get().UpdateMyAvailability();
	}

	private void DeclinePartyInvite(ulong inviteId)
	{
		BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
		BnetParty.DeclineReceivedInvite(inviteId);
		FireChangedEvent(PartyInviteEvent.I_DECLINED_INVITE, myGameAccountId);
		m_pendingParty = null;
	}

	private void TransitionModeIfNeeded()
	{
		SceneMgr.Mode desiredMode = GetMode(m_partyData.m_type);
		SceneMgr.Mode currentMode = SceneMgr.Get().GetMode();
		if (desiredMode != currentMode)
		{
			SceneMgr.Get().SetNextMode(desiredMode);
		}
	}

	private void OnPresenceUpdated(BnetPlayerChangelist changelist, object userData)
	{
		foreach (BnetPlayerChange change in changelist.GetChanges())
		{
			BnetPlayer changedPlayer = change.GetPlayer();
			BnetGameAccountId changedGameAccountId = changedPlayer.GetBestGameAccountId();
			if (IsPlayerInCurrentPartyOrPending(changedGameAccountId) && !changedPlayer.IsOnline())
			{
				KickPlayerFromParty(changedGameAccountId);
			}
		}
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		ClearPartyData();
	}

	private void OnLoginComplete()
	{
		UpdateMyAvailability();
	}

	private ulong? GetPendingInviteIdFromGameAccount(BnetGameAccountId gameAccountId)
	{
		PartyInvite[] pendingInvites = GetPendingInvites();
		foreach (PartyInvite invite in pendingInvites)
		{
			if (invite.InviteeId == gameAccountId)
			{
				return invite.InviteId;
			}
		}
		return null;
	}

	public Map<BnetGameAccountId, int> GetDesiredDuosTeams(bool isDuos, BnetParty.PartyMember[] members)
	{
		Map<BnetGameAccountId, int> duosTeamIds = new Map<BnetGameAccountId, int>();
		if (isDuos)
		{
			for (int i = 0; i < members.Length; i++)
			{
				BnetGameAccountId bnetGameAccountId = members[i].GameAccountId;
				int desiredSlot = GetPlayerDuosSlot(bnetGameAccountId);
				int desiredTeam = 0;
				if (BATTLEGROUNDS_DUO_SLOT_TO_TEAM.ContainsKey(desiredSlot))
				{
					desiredTeam = BATTLEGROUNDS_DUO_SLOT_TO_TEAM[desiredSlot];
				}
				duosTeamIds.Add(bnetGameAccountId, desiredTeam);
			}
		}
		return duosTeamIds;
	}

	public Map<BnetGameAccountId, int> GetDuosTeams(bool isDuos, bool isPrivateParty, BnetParty.PartyMember[] members)
	{
		Map<BnetGameAccountId, int> duosTeamIds = GetDesiredDuosTeams(isDuos, members);
		if (isDuos)
		{
			Dictionary<int, List<BnetGameAccountId>> finalTeams = new Dictionary<int, List<BnetGameAccountId>>
			{
				{
					1,
					new List<BnetGameAccountId>()
				},
				{
					2,
					new List<BnetGameAccountId>()
				},
				{
					3,
					new List<BnetGameAccountId>()
				},
				{
					4,
					new List<BnetGameAccountId>()
				}
			};
			List<BnetGameAccountId> unpairedPlayers = new List<BnetGameAccountId>();
			foreach (KeyValuePair<BnetGameAccountId, int> playerDesiredTeam in duosTeamIds)
			{
				BnetGameAccountId player = playerDesiredTeam.Key;
				int desiredTeam = playerDesiredTeam.Value;
				if (desiredTeam == 0)
				{
					unpairedPlayers.Add(player);
				}
				else if (finalTeams[desiredTeam].Count < 2)
				{
					finalTeams[desiredTeam].Add(player);
				}
				else
				{
					unpairedPlayers.Add(player);
				}
			}
			List<int> emptyTeams = new List<int>();
			List<int> singlePlayerTeams = new List<int>();
			foreach (KeyValuePair<int, List<BnetGameAccountId>> team in finalTeams)
			{
				if (team.Value.Count == 0)
				{
					emptyTeams.Add(team.Key);
				}
				else if (team.Value.Count == 1)
				{
					singlePlayerTeams.Add(team.Key);
				}
			}
			foreach (BnetGameAccountId player2 in unpairedPlayers)
			{
				if (singlePlayerTeams.Count > 0)
				{
					int teamId = singlePlayerTeams[0];
					finalTeams[teamId].Add(player2);
					duosTeamIds[player2] = teamId;
					singlePlayerTeams.RemoveAt(0);
				}
				else if (emptyTeams.Count > 0)
				{
					int teamId2 = emptyTeams[0];
					finalTeams[teamId2].Add(player2);
					duosTeamIds[player2] = teamId2;
					emptyTeams.RemoveAt(0);
					singlePlayerTeams.Add(teamId2);
				}
			}
			if (isPrivateParty)
			{
				foreach (KeyValuePair<int, List<BnetGameAccountId>> team2 in finalTeams)
				{
					if (team2.Value.Count == 1)
					{
						BnetGameAccountId soloPlayer = team2.Value[0];
						unpairedPlayers.Add(soloPlayer);
						duosTeamIds[soloPlayer] = 0;
						emptyTeams.Add(team2.Key);
						team2.Value.Clear();
					}
				}
				emptyTeams.Clear();
				singlePlayerTeams.Clear();
				foreach (BnetGameAccountId player3 in unpairedPlayers)
				{
					if (singlePlayerTeams.Count > 0)
					{
						int teamId3 = singlePlayerTeams[0];
						finalTeams[teamId3].Add(player3);
						duosTeamIds[player3] = teamId3;
						singlePlayerTeams.RemoveAt(0);
					}
					else if (emptyTeams.Count > 0)
					{
						int teamId4 = emptyTeams[0];
						finalTeams[teamId4].Add(player3);
						duosTeamIds[player3] = teamId4;
						emptyTeams.RemoveAt(0);
						singlePlayerTeams.Add(teamId4);
					}
				}
			}
		}
		return duosTeamIds;
	}

	private void BnetParty_OnJoined(OnlineEventType evt, PartyInfo party, LeaveReason? reason)
	{
		if (!ShouldSupportPartyType(party.Type) || party.Id != m_partyData.m_partyId)
		{
			return;
		}
		if (evt == OnlineEventType.ADDED)
		{
			m_partyData.m_partyId = party.Id;
			UpdateMyAvailability();
			InitializePersonalPartyMemberAttributes(party.Type);
			if (BattleNet.GetPartyAttribute(party.Id, "WTCG.Game.ScenarioId", out long scenarioId))
			{
				m_partyData.m_scenarioId = (ScenarioDbId)scenarioId;
			}
			if (BattleNet.GetPartyAttribute(party.Id, "WTCG.Format.Type", out long formatType))
			{
				m_partyData.m_format = (FormatType)formatType;
			}
			if (BattleNet.GetPartyAttribute(party.Id, "WTCG.Season.Id", out int seasonId))
			{
				m_partyData.m_season = seasonId;
			}
			if (BattleNet.GetPartyAttribute(party.Id, "battlegrounds_private", out bool isPrivate))
			{
				m_partyData.m_private = isPrivate;
			}
		}
		if (evt == OnlineEventType.REMOVED)
		{
			ClearPartyData();
			UpdateMyAvailability();
			switch (reason)
			{
			case LeaveReason.DISSOLVED_BY_MEMBER:
			case LeaveReason.DISSOLVED_BY_SERVICE:
				ShowSimpleAlertDialog("GLUE_BACON_PARTY_DISBANDED_HEADER", "GLUE_BACON_PARTY_DISBANDED_BODY");
				break;
			case LeaveReason.MEMBER_KICKED:
				ShowSimpleAlertDialog("GLUE_BACON_PARTY_KICKED_HEADER", "GLUE_BACON_PARTY_KICKED_BODY");
				break;
			}
			FireChangedEvent(PartyInviteEvent.LEADER_DISSOLVED_PARTY, null);
		}
	}

	private void BnetParty_OnReceivedInvite(OnlineEventType evt, PartyInfo party, ulong inviteId, BnetGameAccountId inviter, string inviterBattleTag, BnetGameAccountId invitee, InviteRemoveReason? reason)
	{
		if (!ShouldSupportPartyType(party.Type))
		{
			return;
		}
		switch (evt)
		{
		case OnlineEventType.ADDED:
			if (!IsPartyTypeEnabledInGuardian(party.Type))
			{
				DeclinePartyInvite(inviteId);
				return;
			}
			if (!FriendChallengeMgr.Get().AmIAvailable())
			{
				DeclinePartyInvite(inviteId);
				return;
			}
			if (party.Type != PartyType.BATTLEGROUNDS_PARTY && !HasCompletedRequiredTutorialForPartyType(party.Type))
			{
				DeclinePartyInvite(inviteId);
				return;
			}
			if (party.Type != PartyType.BATTLEGROUNDS_PARTY && !HasGameDataForPartyType(party.Type))
			{
				DeclinePartyInvite(inviteId);
				return;
			}
			m_partyData.m_inviteId = inviteId;
			m_partyData.m_type = party.Type;
			m_pendingParty = party.Id;
			ShowInviteDialog(inviter, inviterBattleTag, party.Type);
			break;
		case OnlineEventType.REMOVED:
		{
			m_pendingParty = null;
			if (!(m_inviteDialog != null))
			{
				break;
			}
			PartyType partyType = party.Type;
			m_inviteDialog.AddHiddenOrDestroyedListener(delegate
			{
				m_inviteDialog = null;
				FriendChallengeMgr.Get().UpdateMyAvailability();
				if (partyType == PartyType.MERCENARIES_FRIENDLY_CHALLENGE)
				{
					m_pendingParty = null;
					ShowSimpleAlertDialog(GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER"), GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_QUEUE_CANCELED"));
				}
			});
			if (partyType == PartyType.MERCENARIES_FRIENDLY_CHALLENGE)
			{
				m_inviteDialog.Hide();
			}
			break;
		}
		}
		FriendChallengeMgr.Get().UpdateMyAvailability();
	}

	private static bool HasGameDataForPartyType(PartyType partyType)
	{
		switch (partyType)
		{
		case PartyType.BATTLEGROUNDS_PARTY:
			return IsModuleReady(DownloadTags.Content.Bgs);
		case PartyType.MERCENARIES_FRIENDLY_CHALLENGE:
		case PartyType.MERCENARIES_COOP_PARTY:
			return IsModuleReady(DownloadTags.Content.Merc);
		default:
			return true;
		}
		static bool IsModuleReady(DownloadTags.Content type)
		{
			return GameDownloadManagerProvider.Get()?.IsModuleReadyToPlay(type) ?? false;
		}
	}

	public bool HasGameDataForGameType(GameType gameType)
	{
		if (GameTypeIsMercenaries(gameType))
		{
			return IsModuleReady(DownloadTags.Content.Merc);
		}
		if (GameTypeIsBattlegrounds(gameType))
		{
			return IsModuleReady(DownloadTags.Content.Bgs);
		}
		return true;
		static bool IsModuleReady(DownloadTags.Content type)
		{
			return GameDownloadManagerProvider.Get()?.IsModuleReadyToPlay(type) ?? false;
		}
	}

	private static bool GameTypeIsMercenaries(GameType gameType)
	{
		if (gameType != GameType.GT_MERCENARIES_AI_VS_AI && gameType != GameType.GT_MERCENARIES_FRIENDLY && gameType != GameType.GT_MERCENARIES_PVE && gameType != GameType.GT_MERCENARIES_PVE_COOP)
		{
			return gameType == GameType.GT_MERCENARIES_PVP;
		}
		return true;
	}

	private static bool GameTypeIsBattlegrounds(GameType gameType)
	{
		if (gameType != GameType.GT_BATTLEGROUNDS && gameType != GameType.GT_BATTLEGROUNDS_AI_VS_AI && gameType != GameType.GT_BATTLEGROUNDS_DUO && gameType != GameType.GT_BATTLEGROUNDS_DUO_AI_VS_AI && gameType != GameType.GT_BATTLEGROUNDS_DUO_FRIENDLY)
		{
			return gameType == GameType.GT_BATTLEGROUNDS_PLAYER_VS_AI;
		}
		return true;
	}

	private void BnetParty_OnMemberEvent(OnlineEventType evt, PartyInfo party, BnetGameAccountId memberId, bool isRolesUpdate, LeaveReason? reason)
	{
		if (party == null)
		{
			Log.Party.PrintError("PartyManager.BnetParty_OnMemberEvent() received empty party info.");
			TelemetryManager.Client().SendLiveIssue("PartyManager.BnetParty_OnMemberEvent", "Party info is null.");
		}
		else
		{
			if (!ShouldSupportPartyType(party.Type) || party.Id != m_partyData.m_partyId)
			{
				return;
			}
			BnetGameAccountId myPlayerGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
			Log.Party.PrintDebug("PartyManager.BnetParty_OnMemberEvent() received event {0} for member {1}", evt.ToString(), memberId.ToString());
			if (evt == OnlineEventType.REMOVED && BnetParty.IsInParty(party.Id) && memberId != myPlayerGameAccountId)
			{
				FireChangedEvent(PartyInviteEvent.FRIEND_LEFT, memberId);
				BnetGameAccountId leaderGameAccountId = GetLeader();
				if (leaderGameAccountId == null || leaderGameAccountId == memberId)
				{
					LeaveParty();
					FireChangedEvent(PartyInviteEvent.LEADER_DISSOLVED_PARTY, memberId);
				}
			}
			else if (evt == OnlineEventType.ADDED && BnetParty.IsInParty(party.Id) && memberId != myPlayerGameAccountId)
			{
				FireChangedEvent(PartyInviteEvent.FRIEND_ACCEPTED_INVITE, memberId);
			}
		}
	}

	private void BnetParty_OnPartyAttributeChanged(PartyInfo party, Blizzard.GameService.Protocol.V2.Client.Attribute attribute)
	{
		if (party == null)
		{
			Log.Party.PrintError("PartyManager.BnetParty_OnPartyAttributeChanged() received empty party info.");
			TelemetryManager.Client().SendLiveIssue("PartyManager.BnetParty_OnPartyAttributeChanged", "Party info is null.");
		}
		else
		{
			if (!ShouldSupportPartyType(party.Type) || m_partyData.m_partyId != party.Id)
			{
				return;
			}
			switch (attribute.Name)
			{
			case "battlegrounds_private":
				m_partyData.m_private = attribute.Value.BoolValue;
				break;
			case "finding_bnet_game_type":
				Network.Get().BattlegroundsPartyLeaderChangedFindingGameState((int)attribute.Value.IntValue);
				break;
			case "queue":
				if (attribute.Value.HasStringValue && attribute.Value.StringValue.Equals("in_queue"))
				{
					if (Shop.Get() != null)
					{
						Shop.Get().Close(forceClose: true);
					}
					WaitForGame();
				}
				if (attribute.Value.HasStringValue && attribute.Value.StringValue.Equals("cancel_queue"))
				{
					GameMgr.Get().CancelFindGame();
				}
				break;
			case "canceled_by":
				if (attribute.Value.HasBlobValue)
				{
					BnetId canceledByBnetId = ProtobufUtil.ParseFrom<BnetId>(attribute.Value.BlobValue.ToArray());
					BnetGameAccountId canceledByGameAccountId = new BnetGameAccountId(canceledByBnetId.Hi, canceledByBnetId.Lo);
					BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
					if (!(canceledByGameAccountId == myGameAccountId))
					{
						string canceledByName = GetPartyMemberName(canceledByGameAccountId);
						AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
						info.m_headerText = GameStrings.Get("GLUE_BACON_PRIVATE_PARTY_TITLE");
						info.m_text = GameStrings.Format("GLUE_BACON_QUEUE_CANCELED", "5ecaf0ff", canceledByName);
						info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
						info.m_showAlertIcon = false;
						info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
						info.m_okText = GameStrings.Get("GLOBAL_OKAY");
						DialogManager.Get().ShowPopup(info);
					}
				}
				break;
			}
			FirePartyAttributeChangedEvent(attribute);
		}
	}

	private void BnetParty_OnMemberAttributeChanged(PartyInfo party, BnetGameAccountId partyMember, Blizzard.GameService.Protocol.V2.Client.Attribute attribute)
	{
		if (party == null)
		{
			Log.Party.PrintError("PartyManager.BnetParty_OnMemberAttributeChanged() received empty party info.");
			TelemetryManager.Client().SendLiveIssue("PartyManager.BnetParty_OnMemberAttributeChanged", "Party info is null.");
		}
		else if (ShouldSupportPartyType(party.Type) && !(m_partyData.m_partyId != party.Id))
		{
			Log.Party.PrintDebug("PartyManager.BnetParty_OnMemberAttributeChanged() - " + attribute.ToString());
			FireMemberAttributeChangedEvent(partyMember, attribute);
		}
	}

	private void BnetParty_OnSentInvite(OnlineEventType evt, PartyInfo party, ulong inviteId, BnetGameAccountId inviter, BnetGameAccountId invitee, bool senderIsMyself, InviteRemoveReason? reason)
	{
		if (!ShouldSupportPartyType(party.Type) || m_partyData.m_partyId != party.Id)
		{
			return;
		}
		if (evt == OnlineEventType.ADDED)
		{
			if (inviter != BnetPresenceMgr.Get().GetMyGameAccountId())
			{
				FireChangedEvent(PartyInviteEvent.I_SENT_INVITE, invitee);
			}
			else
			{
				FireChangedEvent(PartyInviteEvent.FRIEND_RECEIVED_INVITE, invitee);
			}
		}
		if (evt != OnlineEventType.REMOVED || !reason.HasValue)
		{
			return;
		}
		switch (reason.GetValueOrDefault())
		{
		case InviteRemoveReason.DECLINED:
			FireChangedEvent(PartyInviteEvent.FRIEND_DECLINED_INVITE, invitee);
			if (BnetParty.IsLeader(party.Id))
			{
				ShowDeclinedInvitationPopup(invitee);
			}
			break;
		case InviteRemoveReason.REVOKED:
		case InviteRemoveReason.EXPIRED:
		case InviteRemoveReason.CANCELED:
			FireChangedEvent(PartyInviteEvent.INVITE_EXPIRED, invitee);
			break;
		case InviteRemoveReason.IGNORED:
			break;
		}
	}

	private void BnetParty_OnReceivedInviteRequest(OnlineEventType evt, PartyInfo party, InviteRequest request, InviteRequestRemovedReason? reason)
	{
		if (ShouldSupportPartyType(party.Type) && !(m_partyData.m_partyId != party.Id) && BnetParty.IsLeader(party.Id) && !BnetParty.IsMember(party.Id, request.TargetId))
		{
			DialogManager.Get().ShowBattlegroundsSuggestion(request.TargetId, request.TargetName, request.RequesterId, request.RequesterName, OnBattlegroundsSuggestionReceivedResponse);
		}
	}

	private void FireChangedEvent(PartyInviteEvent challengeEvent, BnetGameAccountId playerGameAccountId)
	{
		ChangedListener[] listeners = m_changedListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(challengeEvent, playerGameAccountId, m_partyData);
		}
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

	public bool RemoveChangedListener(ChangedCallback callback, object userData)
	{
		ChangedListener listener = new ChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_changedListeners.Remove(listener);
	}

	private void FireMemberAttributeChangedEvent(BnetGameAccountId playerGameAccountId, Blizzard.GameService.Protocol.V2.Client.Attribute attribute)
	{
		MemberAttributeChangedListener[] listeners = m_memberAttributeChangedListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(playerGameAccountId, attribute);
		}
	}

	public bool AddMemberAttributeChangedListener(MemberAttributeChangedCallback callback)
	{
		return AddMemberAttributeChangedListener(callback, null);
	}

	public bool AddMemberAttributeChangedListener(MemberAttributeChangedCallback callback, object userData)
	{
		MemberAttributeChangedListener listener = new MemberAttributeChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_memberAttributeChangedListeners.Contains(listener))
		{
			return false;
		}
		m_memberAttributeChangedListeners.Add(listener);
		return true;
	}

	public bool RemoveMemberAttributeChangedListener(MemberAttributeChangedCallback callback)
	{
		return RemoveMemberAttributeChangedListener(callback, null);
	}

	public bool RemoveMemberAttributeChangedListener(MemberAttributeChangedCallback callback, object userData)
	{
		MemberAttributeChangedListener listener = new MemberAttributeChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_memberAttributeChangedListeners.Remove(listener);
	}

	private void FirePartyAttributeChangedEvent(Blizzard.GameService.Protocol.V2.Client.Attribute attribute)
	{
		PartyAttributeChangedListener[] listeners = m_partyAttributeChangedListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(attribute);
		}
	}

	public bool AddPartyAttributeChangedListener(PartyAttributeChangedCallback callback)
	{
		return AddPartyAttributeChangedListener(callback, null);
	}

	public bool AddPartyAttributeChangedListener(PartyAttributeChangedCallback callback, object userData)
	{
		PartyAttributeChangedListener listener = new PartyAttributeChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_partyAttributeChangedListeners.Contains(listener))
		{
			return false;
		}
		m_partyAttributeChangedListeners.Add(listener);
		return true;
	}

	public bool RemovePartyAttributeChangedListener(PartyAttributeChangedCallback callback)
	{
		return RemovePartyAttributeChangedListener(callback, null);
	}

	public bool RemovePartyAttributeChangedListener(PartyAttributeChangedCallback callback, object userData)
	{
		PartyAttributeChangedListener listener = new PartyAttributeChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_partyAttributeChangedListeners.Remove(listener);
	}
}
