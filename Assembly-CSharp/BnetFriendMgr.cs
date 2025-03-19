using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone;
using PegasusClient;
using PegasusShared;
using UnityEngine;

public class BnetFriendMgr
{
	public delegate void ChangeCallback(BnetFriendChangelist changelist, object userData);

	private class ChangeListener : EventListener<ChangeCallback>
	{
		public void Fire(BnetFriendChangelist changelist)
		{
			m_callback(changelist, m_userData);
		}
	}

	private static BnetFriendMgr s_instance;

	private int m_maxFriends;

	private int m_maxReceivedInvites;

	private int m_maxSentInvites;

	private List<BnetPlayer> m_friends = new List<BnetPlayer>();

	private List<BnetInvitation> m_receivedInvites = new List<BnetInvitation>();

	private List<BnetInvitation> m_sentInvites = new List<BnetInvitation>();

	private List<ChangeListener> m_changeListeners = new List<ChangeListener>();

	private PendingBnetFriendChangelist m_pendingChangelist = new PendingBnetFriendChangelist();

	private bool m_isRegisteredToFriendHandler;

	private bool m_isFriendInviteFeatureEnabled;

	private static ulong nextIdToken;

	public bool IsFriendInviteFeatureEnabled => m_isFriendInviteFeatureEnabled;

	public static BnetFriendMgr Get()
	{
		if (s_instance == null)
		{
			s_instance = new BnetFriendMgr();
			HearthstoneApplication.Get().WillReset += s_instance.Clear;
		}
		return s_instance;
	}

	public void Initialize()
	{
		FriendMgr.Get().Initialize();
		Network.Get().OnDisconnectedFromBattleNet += OnDisconnectedFromBattleNet;
		RegisterFriendHandler();
		InitMaximums();
	}

	public void Shutdown()
	{
		UnregisterFriendHandler();
		Network.Get().OnDisconnectedFromBattleNet -= OnDisconnectedFromBattleNet;
	}

	public void SetFriendInviteFeatureStatus(bool isEnabled)
	{
		m_isFriendInviteFeatureEnabled = isEnabled;
		Log.Privacy.PrintDebug("BnetFriendMgr SetFriendInviteFeatureStatus m_isFriendInviteFeatureEnabled " + $" {m_isFriendInviteFeatureEnabled}, m_isRegisteredToFriendHandler {m_isRegisteredToFriendHandler}");
		if (!m_isRegisteredToFriendHandler)
		{
			RegisterFriendHandler();
		}
		if (m_isFriendInviteFeatureEnabled)
		{
			BnetFriendChangelist changelist = new BnetFriendChangelist();
			foreach (BnetInvitation receivedInvite in m_receivedInvites)
			{
				changelist.AddAddedReceivedInvite(receivedInvite);
			}
			if (!changelist.IsEmpty())
			{
				FireChangeEvent(changelist);
			}
			return;
		}
		BnetFriendChangelist changelist2 = new BnetFriendChangelist();
		foreach (BnetInvitation receivedInvite2 in m_receivedInvites)
		{
			changelist2.AddRemovedReceivedInvite(receivedInvite2);
		}
		if (!changelist2.IsEmpty())
		{
			FireChangeEvent(changelist2);
		}
	}

	private void RegisterFriendHandler()
	{
		if (!m_isRegisteredToFriendHandler)
		{
			Log.Privacy.PrintDebug("BnetFriendMgr RegisterFriendHandler");
			m_isRegisteredToFriendHandler = true;
			Network.Get().SetFriendsHandler(OnFriendsUpdate);
			Network.Get().AddBnetErrorListener(BnetFeature.Friends, OnBnetError);
		}
	}

	private void UnregisterFriendHandler()
	{
		Log.Privacy.PrintDebug("BnetFriendMgr UnregisterFriendHandler");
		m_isRegisteredToFriendHandler = false;
		Network.Get().SetFriendsHandler(null);
		Network.Get().RemoveBnetErrorListener(BnetFeature.Friends, OnBnetError);
	}

	public BnetPlayer FindFriend(BnetAccountId id)
	{
		BnetPlayer friend = FindNonPendingFriend(id);
		if (friend != null)
		{
			return friend;
		}
		friend = FindPendingFriend(id);
		if (friend != null)
		{
			return friend;
		}
		return null;
	}

	public bool IsFriend(BnetPlayer player)
	{
		if (IsNonPendingFriend(player))
		{
			return true;
		}
		if (IsPendingFriend(player))
		{
			return true;
		}
		return false;
	}

	public bool IsFriend(BnetAccountId id)
	{
		if (IsNonPendingFriend(id))
		{
			return true;
		}
		if (IsPendingFriend(id))
		{
			return true;
		}
		return false;
	}

	public bool IsFriend(BnetGameAccountId id)
	{
		if (IsNonPendingFriend(id))
		{
			return true;
		}
		if (IsPendingFriend(id))
		{
			return true;
		}
		return false;
	}

	public List<BnetPlayer> GetFriends()
	{
		return m_friends;
	}

	public int GetFriendCount()
	{
		return m_friends.Count;
	}

	public bool HasOnlineFriends()
	{
		foreach (BnetPlayer friend in m_friends)
		{
			if (friend.IsOnline())
			{
				return true;
			}
		}
		return false;
	}

	public int GetOnlineFriendCount()
	{
		int count = 0;
		foreach (BnetPlayer friend in m_friends)
		{
			if (friend.IsOnline() || friend.IsAway())
			{
				count++;
			}
		}
		return count;
	}

	public BnetPlayer FindNonPendingFriend(BnetAccountId id)
	{
		foreach (BnetPlayer friend in m_friends)
		{
			if (friend.GetAccountId() == id)
			{
				return friend;
			}
		}
		return null;
	}

	public BnetPlayer FindNonPendingFriend(BnetGameAccountId id)
	{
		foreach (BnetPlayer friend in m_friends)
		{
			if (friend.HasGameAccount(id))
			{
				return friend;
			}
		}
		return null;
	}

	public bool IsNonPendingFriend(BnetPlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (m_friends.Contains(player))
		{
			return true;
		}
		BnetAccountId accountId = player.GetAccountId();
		if (accountId != null)
		{
			return IsFriend(accountId);
		}
		foreach (BnetGameAccountId gameAccountId in player.GetGameAccounts().Keys)
		{
			if (IsFriend(gameAccountId))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsNonPendingFriend(BnetAccountId id)
	{
		return FindNonPendingFriend(id) != null;
	}

	public bool IsNonPendingFriend(BnetGameAccountId id)
	{
		return FindNonPendingFriend(id) != null;
	}

	public BnetPlayer FindPendingFriend(BnetAccountId id)
	{
		return m_pendingChangelist.FindFriend(id);
	}

	public bool IsPendingFriend(BnetPlayer player)
	{
		return m_pendingChangelist.IsFriend(player);
	}

	public bool IsPendingFriend(BnetAccountId id)
	{
		return m_pendingChangelist.IsFriend(id);
	}

	public bool IsPendingFriend(BnetGameAccountId id)
	{
		return m_pendingChangelist.IsFriend(id);
	}

	public List<BnetInvitation> GetReceivedInvites()
	{
		if (m_isFriendInviteFeatureEnabled)
		{
			return m_receivedInvites;
		}
		return null;
	}

	public void AcceptInvite(BnetInvitation invite)
	{
		Network.AcceptFriendInvite(invite.GetId());
		BnetRecentPlayerMgr.Get().AddPendingFriend(invite.GetInviterId());
	}

	public void IgnoreInvite(BnetInvitationId inviteId)
	{
		Network.IgnoreFriendInvite(inviteId);
	}

	public bool SendInvite(string name)
	{
		Log.Privacy.PrintDebug($"BnetFriendMgr m_isFriendInviteFeatureEnabled {m_isFriendInviteFeatureEnabled}");
		if (!m_isFriendInviteFeatureEnabled)
		{
			return true;
		}
		if (name.Contains("@"))
		{
			return SendInviteByEmail(name);
		}
		if (name.Contains("#"))
		{
			return SendInviteByBattleTag(name);
		}
		return false;
	}

	public bool SendInviteByEmail(string email)
	{
		if (!new Regex("^\\S[^@]+@[A-Za-z0-9-]+(\\.[A-Za-z0-9-]+)+$").IsMatch(email))
		{
			return false;
		}
		Network.SendFriendInviteByEmail(BnetPresenceMgr.Get().GetMyPlayer().GetFullName(), email);
		return true;
	}

	public bool SendInviteByBattleTag(string battleTagString)
	{
		if (!new Regex("^[^\\W\\d_][^\\W_]{1,11}#\\d+$").IsMatch(battleTagString))
		{
			return false;
		}
		Network.SendFriendInviteByBattleTag(BnetPresenceMgr.Get().GetMyPlayer().GetBattleTag()
			.GetString(), battleTagString);
		BnetRecentPlayerMgr.Get().AddPendingFriend(battleTagString);
		return true;
	}

	public bool RemoveFriend(BnetPlayer friend)
	{
		bool foundFriend = false;
		for (int i = 0; i < m_friends.Count; i++)
		{
			if (m_friends[i].GetAccountId().Equals(friend.GetAccountId()))
			{
				foundFriend = true;
				break;
			}
		}
		if (!foundFriend)
		{
			return false;
		}
		Network.RemoveFriend(friend.GetAccountId());
		return true;
	}

	public bool AddChangeListener(ChangeCallback callback)
	{
		return AddChangeListener(callback, null);
	}

	public bool AddChangeListener(ChangeCallback callback, object userData)
	{
		ChangeListener listener = new ChangeListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_changeListeners.Contains(listener))
		{
			return false;
		}
		m_changeListeners.Add(listener);
		return true;
	}

	public bool RemoveChangeListener(ChangeCallback callback)
	{
		return RemoveChangeListener(callback, null);
	}

	private bool RemoveChangeListener(ChangeCallback callback, object userData)
	{
		ChangeListener listener = new ChangeListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_changeListeners.Remove(listener);
	}

	public static bool RemoveChangeListenerFromInstance(ChangeCallback callback, object userData = null)
	{
		if (s_instance == null)
		{
			return false;
		}
		return s_instance.RemoveChangeListener(callback, userData);
	}

	private void InitMaximums()
	{
		FriendsInfo info = default(FriendsInfo);
		BattleNet.GetFriendsInfo(ref info);
		m_maxFriends = info.maxFriends;
		m_maxReceivedInvites = info.maxRecvInvites;
		m_maxSentInvites = info.maxSentInvites;
	}

	private void ProcessPendingFriends()
	{
		bool anyDisplayable = false;
		foreach (BnetPlayer friend in m_pendingChangelist.GetFriends())
		{
			if (friend.IsDisplayable())
			{
				anyDisplayable = true;
				m_friends.Add(friend);
			}
		}
		if (anyDisplayable)
		{
			FirePendingFriendsChangedEvent();
		}
	}

	private void OnDisconnectedFromBattleNet(BattleNetErrors error)
	{
		Clear();
	}

	private void OnFriendsUpdate(FriendsUpdate[] updates)
	{
		BnetFriendChangelist changelist = new BnetFriendChangelist();
		for (int i = 0; i < updates.Length; i++)
		{
			FriendsUpdate update = updates[i];
			switch ((FriendsUpdate.Action)update.action)
			{
			case FriendsUpdate.Action.FRIEND_ADDED:
			{
				BnetAccountId friendId2 = BnetAccountId.CreateFromBnetEntityId(update.entity1);
				BnetPlayer newFriend = BnetPresenceMgr.Get().RegisterPlayer(BnetPlayerSource.FRIENDLIST, friendId2);
				if (newFriend.IsDisplayable())
				{
					m_friends.Add(newFriend);
					changelist.AddAddedFriend(newFriend);
				}
				else
				{
					AddPendingFriend(newFriend);
				}
				break;
			}
			case FriendsUpdate.Action.FRIEND_REMOVED:
			{
				BnetAccountId friendId = BnetAccountId.CreateFromBnetEntityId(update.entity1);
				BnetPlayer oldFriend = BnetPresenceMgr.Get().GetPlayer(friendId);
				m_friends.Remove(oldFriend);
				changelist.AddRemovedFriend(oldFriend);
				RemovePendingFriend(oldFriend);
				BnetPresenceMgr.Get().CheckSubscriptionsAndClearTransientStatus(friendId);
				break;
			}
			case FriendsUpdate.Action.FRIEND_INVITE:
			{
				BnetInvitation invite4 = BnetInvitation.CreateFromFriendsUpdate(update);
				m_receivedInvites.Add(invite4);
				changelist.AddAddedReceivedInvite(invite4);
				break;
			}
			case FriendsUpdate.Action.FRIEND_INVITE_REMOVED:
			{
				BnetInvitation invite3 = BnetInvitation.CreateFromFriendsUpdate(update);
				m_receivedInvites.Remove(invite3);
				changelist.AddRemovedReceivedInvite(invite3);
				break;
			}
			case FriendsUpdate.Action.FRIEND_SENT_INVITE:
			{
				BnetInvitation invite2 = BnetInvitation.CreateFromFriendsUpdate(update);
				m_sentInvites.Add(invite2);
				changelist.AddAddedSentInvite(invite2);
				break;
			}
			case FriendsUpdate.Action.FRIEND_SENT_INVITE_REMOVED:
			{
				BnetInvitation invite = BnetInvitation.CreateFromFriendsUpdate(update);
				m_sentInvites.Remove(invite);
				changelist.AddRemovedSentInvite(invite);
				break;
			}
			}
		}
		if (changelist.IsEmpty())
		{
			return;
		}
		if (m_isFriendInviteFeatureEnabled)
		{
			FireChangeEvent(changelist);
			return;
		}
		foreach (BnetInvitation invite5 in m_receivedInvites)
		{
			changelist.AddRemovedReceivedInvite(invite5);
			changelist.RemoveAddedReceivedInvite(invite5);
		}
		FireChangeEvent(changelist);
	}

	private void OnPendingPlayersChanged(BnetPlayerChangelist changelist, object userData)
	{
		ProcessPendingFriends();
	}

	private bool OnBnetError(BnetErrorInfo info, object userData)
	{
		return true;
	}

	private void Clear()
	{
		Shutdown();
		m_friends.Clear();
		m_receivedInvites.Clear();
		m_sentInvites.Clear();
		m_pendingChangelist.Clear();
		BnetPresenceMgr.Get().RemovePlayersChangedListener(OnPendingPlayersChanged);
	}

	private void FireChangeEvent(BnetFriendChangelist changelist)
	{
		ChangeListener[] listeners = m_changeListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(changelist);
		}
	}

	private void AddPendingFriend(BnetPlayer friend)
	{
		if (m_pendingChangelist.Add(friend) && m_pendingChangelist.GetCount() == 1)
		{
			BnetPresenceMgr.Get().AddPlayersChangedListener(OnPendingPlayersChanged);
		}
	}

	private void RemovePendingFriend(BnetPlayer friend)
	{
		if (m_pendingChangelist.Remove(friend))
		{
			if (m_pendingChangelist.GetCount() == 0)
			{
				BnetPresenceMgr.Get().RemovePlayersChangedListener(OnPendingPlayersChanged);
			}
			else
			{
				ProcessPendingFriends();
			}
		}
	}

	private void FirePendingFriendsChangedEvent()
	{
		BnetFriendChangelist changelist = m_pendingChangelist.CreateChangelist();
		if (m_pendingChangelist.GetCount() == 0)
		{
			BnetPresenceMgr.Get().RemovePlayersChangedListener(OnPendingPlayersChanged);
		}
		FireChangeEvent(changelist);
	}

	public BnetPlayer Cheat_CreatePlayer(string fullName, int leagueId, int starLevel, BnetProgramId programId, bool isFriend, bool isOnline, bool isAway = false)
	{
		BnetBattleTag battleTag = new BnetBattleTag();
		battleTag.SetString($"friend#{nextIdToken}");
		BnetAccountId accountId = new BnetAccountId(nextIdToken++, nextIdToken++);
		BnetAccount account = new BnetAccount();
		account.SetId(accountId);
		account.SetFullName(fullName);
		account.SetBattleTag(battleTag);
		BnetGameAccountId gameAccountId = new BnetGameAccountId(nextIdToken++, nextIdToken++);
		BnetGameAccount gameAccount = new BnetGameAccount();
		gameAccount.SetId(gameAccountId);
		gameAccount.SetBattleTag(battleTag);
		gameAccount.SetOnline(isOnline);
		gameAccount.SetAway(isAway);
		gameAccount.SetProgramId(programId);
		GamePresenceRank gamePresenceRank = new GamePresenceRank();
		foreach (FormatType enumValue in Enum.GetValues(typeof(FormatType)))
		{
			if (enumValue != 0)
			{
				GamePresenceRankData rankData = new GamePresenceRankData
				{
					FormatType = enumValue,
					LeagueId = leagueId,
					StarLevel = starLevel,
					LegendRank = UnityEngine.Random.Range(1, 99999)
				};
				gamePresenceRank.Values.Add(rankData);
			}
		}
		byte[] blob = ProtobufUtil.ToByteArray(gamePresenceRank);
		gameAccount.SetGameField(18u, blob);
		BnetPlayer player = new BnetPlayer(BnetPlayerSource.CREATED_BY_CHEAT);
		player.SetAccount(account);
		player.AddGameAccount(gameAccount);
		player.IsCheatPlayer = true;
		if (isFriend)
		{
			m_friends.Add(player);
		}
		return player;
	}

	public BnetPlayer Cheat_CreateFriend(string fullName, int leagueId, int starLevel, BnetProgramId programId, bool isOnline, bool isAway)
	{
		return Cheat_CreatePlayer(fullName, leagueId, starLevel, programId, isFriend: true, isOnline, isAway);
	}

	public int Cheat_RemoveCheatFriends()
	{
		int numRemoved = 0;
		for (int i = m_friends.Count - 1; i >= 0; i--)
		{
			if (m_friends[i].IsCheatPlayer)
			{
				m_friends.RemoveAt(i);
				numRemoved++;
			}
		}
		return numRemoved;
	}
}
