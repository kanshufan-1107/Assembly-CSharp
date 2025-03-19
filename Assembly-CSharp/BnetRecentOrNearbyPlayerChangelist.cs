using System.Collections.Generic;

public class BnetRecentOrNearbyPlayerChangelist
{
	private List<BnetPlayer> m_playersAdded = new List<BnetPlayer>();

	private List<BnetPlayer> m_playersUpdated = new List<BnetPlayer>();

	private List<BnetPlayer> m_playersRemoved = new List<BnetPlayer>();

	private List<BnetPlayer> m_friendsAdded = new List<BnetPlayer>();

	private List<BnetPlayer> m_friendsUpdated = new List<BnetPlayer>();

	private List<BnetPlayer> m_friendsRemoved = new List<BnetPlayer>();

	private List<BnetPlayer> m_strangersAdded = new List<BnetPlayer>();

	private List<BnetPlayer> m_strangersUpdated = new List<BnetPlayer>();

	private List<BnetPlayer> m_strangersRemoved = new List<BnetPlayer>();

	public List<BnetPlayer> GetAddedPlayers()
	{
		return m_playersAdded;
	}

	public List<BnetPlayer> GetRemovedPlayers()
	{
		return m_playersRemoved;
	}

	public List<BnetPlayer> GetAddedFriends()
	{
		return m_friendsAdded;
	}

	public List<BnetPlayer> GetRemovedFriends()
	{
		return m_friendsRemoved;
	}

	public List<BnetPlayer> GetAddedStrangers()
	{
		return m_strangersAdded;
	}

	public List<BnetPlayer> GetUpdatedStrangers()
	{
		return m_strangersUpdated;
	}

	public List<BnetPlayer> GetRemovedStrangers()
	{
		return m_strangersRemoved;
	}

	public bool IsEmpty()
	{
		if (m_playersAdded != null && m_playersAdded.Count > 0)
		{
			return false;
		}
		if (m_playersUpdated != null && m_playersUpdated.Count > 0)
		{
			return false;
		}
		if (m_playersRemoved != null && m_playersRemoved.Count > 0)
		{
			return false;
		}
		if (m_friendsAdded != null && m_friendsAdded.Count > 0)
		{
			return false;
		}
		if (m_friendsUpdated != null && m_friendsUpdated.Count > 0)
		{
			return false;
		}
		if (m_friendsRemoved != null && m_friendsRemoved.Count > 0)
		{
			return false;
		}
		if (m_strangersAdded != null && m_strangersAdded.Count > 0)
		{
			return false;
		}
		if (m_strangersUpdated != null && m_strangersUpdated.Count > 0)
		{
			return false;
		}
		if (m_strangersRemoved != null && m_strangersRemoved.Count > 0)
		{
			return false;
		}
		return true;
	}

	public void Clear()
	{
		m_playersAdded.Clear();
		m_playersUpdated.Clear();
		m_playersRemoved.Clear();
		m_friendsAdded.Clear();
		m_friendsUpdated.Clear();
		m_friendsRemoved.Clear();
		m_strangersAdded.Clear();
		m_strangersUpdated.Clear();
		m_strangersRemoved.Clear();
	}

	public bool AddAddedPlayer(BnetPlayer player)
	{
		if (m_playersAdded.Contains(player))
		{
			return false;
		}
		m_playersAdded.Add(player);
		return true;
	}

	public bool AddUpdatedPlayer(BnetPlayer player)
	{
		if (m_playersUpdated.Contains(player))
		{
			return false;
		}
		m_playersUpdated.Add(player);
		return true;
	}

	public bool AddRemovedPlayer(BnetPlayer player)
	{
		if (m_playersRemoved.Contains(player))
		{
			return false;
		}
		m_playersRemoved.Add(player);
		return true;
	}

	public bool AddAddedFriend(BnetPlayer friend)
	{
		if (m_friendsAdded.Contains(friend))
		{
			return false;
		}
		m_friendsAdded.Add(friend);
		return true;
	}

	public bool AddUpdatedFriend(BnetPlayer friend)
	{
		if (m_friendsUpdated.Contains(friend))
		{
			return false;
		}
		m_friendsUpdated.Add(friend);
		return true;
	}

	public bool AddRemovedFriend(BnetPlayer friend)
	{
		if (m_friendsRemoved.Contains(friend))
		{
			return false;
		}
		m_friendsRemoved.Add(friend);
		return true;
	}

	public bool AddAddedStranger(BnetPlayer stranger)
	{
		if (m_strangersAdded.Contains(stranger))
		{
			return false;
		}
		m_strangersAdded.Add(stranger);
		return true;
	}

	public bool AddUpdatedStranger(BnetPlayer stranger)
	{
		if (m_strangersUpdated.Contains(stranger))
		{
			return false;
		}
		m_strangersUpdated.Add(stranger);
		return true;
	}

	public bool AddRemovedStranger(BnetPlayer stranger)
	{
		if (m_strangersRemoved.Contains(stranger))
		{
			return false;
		}
		m_strangersRemoved.Add(stranger);
		return true;
	}
}
