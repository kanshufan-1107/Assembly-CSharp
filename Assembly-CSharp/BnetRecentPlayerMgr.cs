using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone;
using UnityEngine;

public class BnetRecentPlayerMgr
{
	public delegate void ChangeCallback(BnetRecentOrNearbyPlayerChangelist changelist, object userData);

	private class ChangeListener : EventListener<ChangeCallback>
	{
		public void Fire(BnetRecentOrNearbyPlayerChangelist changelist)
		{
			m_callback(changelist, m_userData);
		}
	}

	public enum RecentReason
	{
		INVALID,
		RECENT_OPPONENT,
		FORMER_FRIEND,
		NEW_FRIEND,
		RECENT_CHALLENGED,
		RECENT_CHATTED,
		LAST_OPPONENT,
		CURRENT_OPPONENT,
		CURRENTLY_SPECTATING,
		RECENT_SPECTATED,
		CURRENT_TEAMMATE,
		RECENT_TEAMMATE,
		CHEAT
	}

	private static readonly int MAX_NUMBER_ENTRIES_PC_TABLET = 5;

	private static readonly int MAX_NUMBER_ENTRIES_PHONE = 2;

	private static BnetRecentPlayerMgr s_instance;

	private int m_maxNumberOfEntries;

	private List<BnetPlayer> m_recentPlayers = new List<BnetPlayer>();

	private Dictionary<BnetPlayer, RecentReason> m_recentPlayerData = new Dictionary<BnetPlayer, RecentReason>();

	private List<BnetPlayer> m_recentFriends = new List<BnetPlayer>();

	private List<BnetPlayer> m_recentStrangers = new List<BnetPlayer>();

	private List<ChangeListener> m_changeListeners = new List<ChangeListener>();

	private BnetRecentOrNearbyPlayerChangelist m_changelist = new BnetRecentOrNearbyPlayerChangelist();

	private HashSet<BnetEntityId> m_pendingFriendsById = new HashSet<BnetEntityId>();

	private HashSet<string> m_pendingFriendsByBattleTag = new HashSet<string>();

	private BnetPlayer m_lastOpponent;

	private BnetRecentPlayerMgr()
	{
		m_maxNumberOfEntries = (UniversalInputManager.UsePhoneUI ? MAX_NUMBER_ENTRIES_PHONE : MAX_NUMBER_ENTRIES_PC_TABLET);
	}

	public static BnetRecentPlayerMgr Get()
	{
		if (s_instance == null)
		{
			s_instance = new BnetRecentPlayerMgr();
			HearthstoneApplication.Get().WillReset += s_instance.Shutdown;
		}
		return s_instance;
	}

	public void Initialize()
	{
		BnetFriendMgr.Get().AddChangeListener(OnFriendsChanged);
		FriendChallengeMgr.Get().AddChangedListener(OnChallengeChanged);
	}

	public void Shutdown()
	{
		BnetFriendMgr.Get().RemoveChangeListener(OnFriendsChanged);
		FriendChallengeMgr.Get().RemoveChangedListener(OnChallengeChanged);
		m_recentPlayers.Clear();
		m_recentPlayerData.Clear();
		m_recentFriends.Clear();
		m_recentStrangers.Clear();
		m_changeListeners.Clear();
		m_changelist.Clear();
		m_pendingFriendsById.Clear();
		m_pendingFriendsByBattleTag.Clear();
		m_lastOpponent = null;
	}

	public List<BnetPlayer> GetRecentPlayers()
	{
		return m_recentPlayers;
	}

	public string GetRecentReason(BnetPlayer player)
	{
		if (!m_recentPlayerData.ContainsKey(player))
		{
			return null;
		}
		return m_recentPlayerData[player] switch
		{
			RecentReason.RECENT_OPPONENT => GameStrings.Get("GLOBAL_FRIENDLIST_RECENT_OPPONENT_STATUS"), 
			RecentReason.FORMER_FRIEND => GameStrings.Get("GLOBAL_FRIENDLIST_FORMER_FRIEND_STATUS"), 
			RecentReason.NEW_FRIEND => GameStrings.Get("GLOBAL_FRIENDLIST_NEW_FRIEND_STATUS"), 
			RecentReason.RECENT_CHALLENGED => GameStrings.Get("GLOBAL_FRIENDLIST_RECENT_CHALLENGED_STATUS"), 
			RecentReason.RECENT_CHATTED => GameStrings.Get("GLOBAL_FRIENDLIST_RECENT_CHATTED_STATUS"), 
			RecentReason.LAST_OPPONENT => GameStrings.Get("GLOBAL_FRIENDLIST_LAST_OPPONENT_STATUS"), 
			RecentReason.CURRENT_OPPONENT => GameStrings.Get("GLOBAL_FRIENDLIST_CURRENT_OPPONENT_STATUS"), 
			RecentReason.CURRENTLY_SPECTATING => GameStrings.Get("GLOBAL_FRIENDLIST_CURRENTLY_SPECTATING_STATUS"), 
			RecentReason.RECENT_SPECTATED => GameStrings.Get("GLOBAL_FRIENDLIST_RECENTLY_SPECTATING_STATUS"), 
			RecentReason.CURRENT_TEAMMATE => GameStrings.Get("GLOBAL_FRIENDLIST_CURRENT_TEAMMATE_STATUS"), 
			RecentReason.RECENT_TEAMMATE => GameStrings.Get("GLOBAL_FRIENDLIST_RECENT_TEAMMATE_STATUS"), 
			_ => null, 
		};
	}

	public bool IsCurrentOpponent(BnetPlayer player)
	{
		if (!m_recentPlayerData.ContainsKey(player))
		{
			return false;
		}
		return m_recentPlayerData[player] == RecentReason.CURRENT_OPPONENT;
	}

	public bool IsRecentStranger(BnetPlayer player)
	{
		return IsRecentInList(player, m_recentStrangers);
	}

	public bool IsRecentPlayer(BnetPlayer player)
	{
		return IsRecentInList(player, m_recentPlayers);
	}

	private static bool IsRecentInList(BnetPlayer player, List<BnetPlayer> bnetPlayers)
	{
		if (player == null)
		{
			return false;
		}
		BnetAccountId accountId = player.GetAccountId();
		if (accountId != null)
		{
			for (int i = 0; i < bnetPlayers.Count; i++)
			{
				if (accountId == bnetPlayers[i].GetAccountId())
				{
					return true;
				}
			}
			return false;
		}
		BnetGameAccountId gameAccountId = player.GetHearthstoneGameAccountId();
		if (gameAccountId != null)
		{
			for (int j = 0; j < bnetPlayers.Count; j++)
			{
				if (gameAccountId == bnetPlayers[j].GetHearthstoneGameAccountId())
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public bool AddChangeListener(ChangeCallback callback)
	{
		ChangeListener listener = new ChangeListener();
		listener.SetCallback(callback);
		if (m_changeListeners.Contains(listener))
		{
			return false;
		}
		m_changeListeners.Add(listener);
		return true;
	}

	public bool RemoveChangeListenerFromInstance(ChangeCallback callback)
	{
		ChangeListener listener = new ChangeListener();
		listener.SetCallback(callback);
		return m_changeListeners.Remove(listener);
	}

	public void AddRecentPlayer(BnetPlayer player, RecentReason recentReason)
	{
		if (player == null || !NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().RecentFriendListDisplayEnabled)
		{
			return;
		}
		if (recentReason == RecentReason.LAST_OPPONENT)
		{
			if (m_lastOpponent != null)
			{
				m_recentPlayerData[m_lastOpponent] = RecentReason.RECENT_OPPONENT;
			}
			m_lastOpponent = player;
		}
		player.TimeLastAddedToRecentPlayers = Time.time;
		if (m_recentPlayers.Contains(player))
		{
			m_recentPlayers.Remove(player);
			m_recentPlayers.Insert(0, player);
			m_recentPlayerData[player] = recentReason;
			return;
		}
		BnetRecentOrNearbyPlayerChangelist changelist = new BnetRecentOrNearbyPlayerChangelist();
		m_recentPlayers.Insert(0, player);
		changelist.AddAddedPlayer(player);
		m_recentPlayerData[player] = recentReason;
		if (BnetFriendMgr.Get().IsFriend(player))
		{
			m_recentFriends.Add(player);
			changelist.AddAddedFriend(player);
		}
		else
		{
			m_recentStrangers.Add(player);
			changelist.AddAddedStranger(player);
		}
		RemoveNoLongerRecentPlayers(changelist);
		FireChangeEvent(changelist);
	}

	public void ConvertAllCurrentOpponentsToRecentOpponents()
	{
		List<BnetPlayer> playersToUpdate = new List<BnetPlayer>(m_recentPlayerData.Count);
		List<BnetPlayer> teammatesToUpdate = new List<BnetPlayer>(m_recentPlayerData.Count);
		foreach (KeyValuePair<BnetPlayer, RecentReason> pair in m_recentPlayerData)
		{
			if (pair.Value == RecentReason.CURRENT_OPPONENT)
			{
				playersToUpdate.Add(pair.Key);
			}
			else if (pair.Value == RecentReason.CURRENT_TEAMMATE)
			{
				teammatesToUpdate.Add(pair.Key);
			}
		}
		foreach (BnetPlayer playerToUpdate in playersToUpdate)
		{
			m_recentPlayerData[playerToUpdate] = RecentReason.RECENT_OPPONENT;
		}
		foreach (BnetPlayer teammateToUpdate in teammatesToUpdate)
		{
			m_recentPlayerData[teammateToUpdate] = RecentReason.RECENT_TEAMMATE;
		}
	}

	public void AddPendingFriend(BnetEntityId playerId)
	{
		if (!m_pendingFriendsById.Contains(playerId))
		{
			m_pendingFriendsById.Add(playerId);
		}
	}

	public void AddPendingFriend(string playerBattleTag)
	{
		if (!m_pendingFriendsByBattleTag.Contains(playerBattleTag))
		{
			m_pendingFriendsByBattleTag.Add(playerBattleTag);
		}
	}

	public BnetPlayer GetCurrentOpponent()
	{
		foreach (KeyValuePair<BnetPlayer, RecentReason> pair in m_recentPlayerData)
		{
			if (pair.Value == RecentReason.CURRENT_OPPONENT)
			{
				return pair.Key;
			}
		}
		return null;
	}

	public void Update()
	{
		m_changelist.Clear();
		RemoveNoLongerRecentPlayers(m_changelist);
		FireChangeEvent(m_changelist);
	}

	private void RemoveNoLongerRecentPlayers(BnetRecentOrNearbyPlayerChangelist changelist)
	{
		int numberOfPlayersToRemove = m_recentPlayers.Count - m_maxNumberOfEntries;
		if (numberOfPlayersToRemove > 0)
		{
			for (int numPlayersRemoved = 0; numPlayersRemoved < numberOfPlayersToRemove; numPlayersRemoved++)
			{
				RemoveRecentPlayer(m_recentPlayers[m_recentPlayers.Count - 1], changelist);
			}
		}
	}

	private void RemoveRecentPlayer(BnetPlayer recentPlayer, BnetRecentOrNearbyPlayerChangelist changelist)
	{
		m_recentPlayers.Remove(recentPlayer);
		changelist.AddRemovedPlayer(recentPlayer);
		if (m_recentFriends.Remove(recentPlayer))
		{
			changelist.AddRemovedFriend(recentPlayer);
		}
		else if (m_recentStrangers.Remove(recentPlayer))
		{
			changelist.AddRemovedStranger(recentPlayer);
		}
	}

	private void FireChangeEvent(BnetRecentOrNearbyPlayerChangelist changelist)
	{
		if (!changelist.IsEmpty())
		{
			ChangeListener[] listeners = m_changeListeners.ToArray();
			for (int i = 0; i < listeners.Length; i++)
			{
				listeners[i].Fire(changelist);
			}
		}
	}

	private void OnFriendsChanged(BnetFriendChangelist changelist, object userData)
	{
		List<BnetPlayer> addedFriends = changelist.GetAddedFriends();
		if (addedFriends != null && (m_pendingFriendsById.Count > 0 || m_pendingFriendsByBattleTag.Count > 0))
		{
			foreach (BnetPlayer addedFriend in addedFriends)
			{
				BnetEntityId friendAccountId = addedFriend.GetAccountId();
				string friendBattleTag = addedFriend.GetBattleTag().ToString();
				if (m_pendingFriendsById.Contains(friendAccountId))
				{
					m_pendingFriendsById.Remove(friendAccountId);
					AddRecentPlayer(addedFriend, RecentReason.NEW_FRIEND);
				}
				else if (m_pendingFriendsByBattleTag.Contains(friendBattleTag))
				{
					m_pendingFriendsByBattleTag.Remove(friendBattleTag);
					AddRecentPlayer(addedFriend, RecentReason.NEW_FRIEND);
				}
			}
		}
		List<BnetPlayer> removedFriends = changelist.GetRemovedFriends();
		if (removedFriends == null)
		{
			return;
		}
		foreach (BnetPlayer removedFriend in removedFriends)
		{
			AddRecentPlayer(removedFriend, RecentReason.FORMER_FRIEND);
		}
	}

	private void OnChallengeChanged(FriendChallengeEvent challengeEvent, BnetPlayer player, FriendlyChallengeData challengeData, object userData)
	{
		if (challengeEvent == FriendChallengeEvent.I_SENT_CHALLENGE || challengeEvent == FriendChallengeEvent.I_RECEIVED_CHALLENGE)
		{
			AddRecentPlayer(player, RecentReason.RECENT_CHALLENGED);
		}
	}

	public BnetPlayer Cheat_CreateRecentPlayer(string fullName, int leagueId, int starLevel, BnetProgramId programId, bool isFriend, bool isOnline)
	{
		BnetPlayer player = BnetFriendMgr.Get().Cheat_CreatePlayer(fullName, leagueId, starLevel, programId, isFriend, isOnline);
		AddRecentPlayer(player, RecentReason.CHEAT);
		return player;
	}

	public int Cheat_RemoveCheatFriends()
	{
		int numRemoved = 0;
		BnetRecentOrNearbyPlayerChangelist changelist = new BnetRecentOrNearbyPlayerChangelist();
		for (int i = m_recentPlayers.Count - 1; i >= 0; i--)
		{
			BnetPlayer recentPlayer = m_recentPlayers[i];
			if (recentPlayer.IsCheatPlayer)
			{
				RemoveRecentPlayer(recentPlayer, changelist);
				numRemoved++;
			}
		}
		FireChangeEvent(changelist);
		return numRemoved;
	}
}
