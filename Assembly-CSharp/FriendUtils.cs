using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;

public class FriendUtils
{
	public static string GetUniqueName(BnetPlayer friend)
	{
		if (GetUniqueName(friend, out var battleTag, out var name))
		{
			return battleTag.ToString();
		}
		return name;
	}

	public static string GetUniqueNameWithColor(BnetPlayer friend)
	{
		string nameColorStr = ((friend != null && friend.IsOnline()) ? "5ecaf0ff" : "999999ff");
		if (GetUniqueName(friend, out var battleTag, out var name))
		{
			return GetBattleTagWithColor(battleTag, nameColorStr);
		}
		return $"<color=#{nameColorStr}>{name}</color>";
	}

	public static string GetBattleTagWithColor(BnetBattleTag battleTag, string nameColorStr)
	{
		return string.Format("<color=#{0}>{1}</color><color=#{2}>#{3}</color>", nameColorStr, battleTag.GetName(), "a1a1a1ff", battleTag.GetNumber());
	}

	public static string GetFriendListName(BnetPlayer friend, bool addColorTags)
	{
		string name = null;
		BnetAccount account = friend.GetAccount();
		if (account != null)
		{
			name = account.GetFullName();
			if (name == null && account.GetBattleTag() != null)
			{
				name = account.GetBattleTag().ToString();
			}
		}
		if (name == null)
		{
			foreach (KeyValuePair<BnetGameAccountId, BnetGameAccount> kv in friend.GetGameAccounts())
			{
				if (kv.Value.GetBattleTag() != null)
				{
					name = kv.Value.GetBattleTag().ToString();
					break;
				}
			}
		}
		if (addColorTags)
		{
			string nameColorStr = (friend.IsOnline() ? "5ecaf0ff" : "999999ff");
			return $"<color=#{nameColorStr}>{name}</color>";
		}
		return name;
	}

	public static string GetRequestElapsedTimeString(long epochMicrosec)
	{
		TimeUtils.ElapsedStringSet timeStringSet = new TimeUtils.ElapsedStringSet
		{
			m_seconds = "GLOBAL_DATETIME_FRIENDREQUEST_SECONDS",
			m_minutes = "GLOBAL_DATETIME_FRIENDREQUEST_MINUTES",
			m_hours = "GLOBAL_DATETIME_FRIENDREQUEST_HOURS",
			m_yesterday = "GLOBAL_DATETIME_FRIENDREQUEST_DAY",
			m_days = "GLOBAL_DATETIME_FRIENDREQUEST_DAYS",
			m_weeks = "GLOBAL_DATETIME_FRIENDREQUEST_WEEKS",
			m_monthAgo = "GLOBAL_DATETIME_FRIENDREQUEST_MONTH"
		};
		return TimeUtils.GetElapsedTimeStringFromEpochMicrosec(epochMicrosec, timeStringSet);
	}

	public static string GetLastOnlineElapsedTimeString(long epochMicrosec)
	{
		if (epochMicrosec == 0L)
		{
			return GameStrings.Get("GLOBAL_OFFLINE");
		}
		TimeUtils.ElapsedStringSet timeStringSet = new TimeUtils.ElapsedStringSet
		{
			m_seconds = "GLOBAL_DATETIME_LASTONLINE_SECONDS",
			m_minutes = "GLOBAL_DATETIME_LASTONLINE_MINUTES",
			m_hours = "GLOBAL_DATETIME_LASTONLINE_HOURS",
			m_yesterday = "GLOBAL_DATETIME_LASTONLINE_DAY",
			m_days = "GLOBAL_DATETIME_LASTONLINE_DAYS",
			m_weeks = "GLOBAL_DATETIME_LASTONLINE_WEEKS",
			m_monthAgo = "GLOBAL_DATETIME_LASTONLINE_MONTH"
		};
		return TimeUtils.GetElapsedTimeStringFromEpochMicrosec(epochMicrosec, timeStringSet);
	}

	public static string GetAwayTimeString(long epochMicrosec)
	{
		TimeUtils.ElapsedStringSet timeStringSet = new TimeUtils.ElapsedStringSet
		{
			m_seconds = "GLOBAL_DATETIME_AFK_SECONDS",
			m_minutes = "GLOBAL_DATETIME_AFK_MINUTES",
			m_hours = "GLOBAL_DATETIME_AFK_HOURS",
			m_yesterday = "GLOBAL_DATETIME_AFK_DAY",
			m_days = "GLOBAL_DATETIME_AFK_DAYS",
			m_weeks = "GLOBAL_DATETIME_AFK_WEEKS",
			m_monthAgo = "GLOBAL_DATETIME_AFK_MONTH"
		};
		return TimeUtils.GetElapsedTimeStringFromEpochMicrosec(epochMicrosec, timeStringSet);
	}

	public static int FriendSortCompare(BnetPlayer friend1, BnetPlayer friend2)
	{
		int compareresult = 0;
		if (friend1 == null || friend2 == null)
		{
			if (friend1 != friend2)
			{
				if (friend1 != null)
				{
					return -1;
				}
				return 1;
			}
			return 0;
		}
		if (!friend1.IsOnline() && !friend2.IsOnline())
		{
			return FriendNameSortCompare(friend1, friend2);
		}
		if (friend1.IsOnline() && !friend2.IsOnline())
		{
			return -1;
		}
		if (!friend1.IsOnline() && friend2.IsOnline())
		{
			return 1;
		}
		BnetProgramId f1id = friend1.GetBestProgramId();
		BnetProgramId f2id = friend2.GetBestProgramId();
		if (FriendSortFlagCompare(friend1, friend2, f1id == BnetProgramId.HEARTHSTONE, f2id == BnetProgramId.HEARTHSTONE, out compareresult))
		{
			return compareresult;
		}
		bool f1IsGame = !(f1id == null) && f1id.IsGame();
		bool f2IsGame = !(f2id == null) && f2id.IsGame();
		if (FriendSortFlagCompare(friend1, friend2, f1IsGame, f2IsGame, out compareresult))
		{
			return compareresult;
		}
		bool f1IsPhoenix = !(f1id == null) && f1id.IsPhoenix();
		bool f2IsPhoenix = !(f2id == null) && f2id.IsPhoenix();
		if (FriendSortFlagCompare(friend1, friend2, f1IsPhoenix, f2IsPhoenix, out compareresult))
		{
			return compareresult;
		}
		bool f1IsFriend = BnetFriendMgr.Get().IsFriend(friend1);
		bool f2IsFriend = BnetFriendMgr.Get().IsFriend(friend2);
		if (f1IsFriend != f2IsFriend)
		{
			if (!f1IsFriend)
			{
				return 1;
			}
			return -1;
		}
		return FriendNameSortCompare(friend1, friend2);
	}

	public static int RecentFriendSortCompare(BnetPlayer friend1, BnetPlayer friend2)
	{
		int sortValue = friend2.TimeLastAddedToRecentPlayers.CompareTo(friend1.TimeLastAddedToRecentPlayers);
		if (sortValue == 0)
		{
			sortValue = FriendNameSortCompare(friend1, friend2);
		}
		return sortValue;
	}

	private static bool GetUniqueName(BnetPlayer friend, out BnetBattleTag battleTag, out string name)
	{
		if (friend != null)
		{
			battleTag = friend.GetBattleTag();
			name = friend.GetBestName();
		}
		else
		{
			battleTag = null;
			name = string.Empty;
		}
		if (battleTag == null)
		{
			return false;
		}
		if (BnetNearbyPlayerMgr.Get().IsNearbyStranger(friend))
		{
			return true;
		}
		foreach (BnetPlayer currFriend in BnetFriendMgr.Get().GetFriends())
		{
			if (currFriend != friend)
			{
				string currName = currFriend.GetBestName();
				if (string.Compare(name, currName, ignoreCase: true) == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool FriendSortFlagCompare(BnetPlayer lhs, BnetPlayer rhs, bool lhsflag, bool rhsflag, out int result)
	{
		if (lhsflag && !rhsflag)
		{
			result = -1;
			return true;
		}
		if (!lhsflag && rhsflag)
		{
			result = 1;
			return true;
		}
		result = 0;
		return false;
	}

	private static int FriendNameSortCompare(BnetPlayer friend1, BnetPlayer friend2)
	{
		int nameCompareResult = string.Compare(GetFriendListName(friend1, addColorTags: false), GetFriendListName(friend2, addColorTags: false), ignoreCase: true);
		if (nameCompareResult != 0)
		{
			return nameCompareResult;
		}
		ulong low = friend1.GetAccountId().Low;
		long friend2AccountIdLo = (long)friend2.GetAccountId().Low;
		return (int)((long)low - friend2AccountIdLo);
	}
}
