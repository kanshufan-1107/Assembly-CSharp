using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;

public class PendingBnetFriendChangelist
{
	private List<BnetPlayer> m_friends = new List<BnetPlayer>();

	public List<BnetPlayer> GetFriends()
	{
		return m_friends;
	}

	public bool Add(BnetPlayer friend)
	{
		if (m_friends.Contains(friend))
		{
			return false;
		}
		m_friends.Add(friend);
		return true;
	}

	public bool Remove(BnetPlayer friend)
	{
		return m_friends.Remove(friend);
	}

	public void Clear()
	{
		m_friends.Clear();
	}

	public int GetCount()
	{
		return m_friends.Count;
	}

	public BnetPlayer FindFriend(BnetAccountId id)
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

	public BnetPlayer FindFriend(BnetGameAccountId id)
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

	public bool IsFriend(BnetPlayer player)
	{
		if (m_friends.Contains(player))
		{
			return true;
		}
		if (player == null)
		{
			return false;
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

	public bool IsFriend(BnetAccountId id)
	{
		return FindFriend(id) != null;
	}

	public bool IsFriend(BnetGameAccountId id)
	{
		return FindFriend(id) != null;
	}

	public BnetFriendChangelist CreateChangelist()
	{
		BnetFriendChangelist changelist = new BnetFriendChangelist();
		for (int i = m_friends.Count - 1; i >= 0; i--)
		{
			BnetPlayer friend = m_friends[i];
			if (friend.IsDisplayable())
			{
				changelist.AddAddedFriend(friend);
				m_friends.RemoveAt(i);
			}
		}
		return changelist;
	}
}
