using System;
using System.Collections.Generic;
using System.Text;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;

public class BnetPlayer
{
	private BnetPlayerSource m_source;

	private BnetAccountId m_accountId;

	private BnetAccount m_account;

	private Map<BnetGameAccountId, BnetGameAccount> m_gameAccounts = new Map<BnetGameAccountId, BnetGameAccount>();

	private BnetGameAccount m_hsGameAccount;

	private BnetGameAccount m_bestGameAccount;

	public BnetPlayerSource Source => m_source;

	public bool IsCheatPlayer { get; set; }

	public float TimeLastAddedToRecentPlayers { get; set; }

	public string ShortSummary
	{
		get
		{
			string realName = GetFullName();
			BnetBattleTag battleTag = GetBattleTag();
			string strBattleTag = ((battleTag == null) ? "null" : battleTag.ToString());
			if (!string.IsNullOrEmpty(realName) && battleTag != null)
			{
				strBattleTag = " " + strBattleTag;
			}
			string strIsOnline = (IsOnline() ? "online" : "offline");
			return $"{realName}{strBattleTag} {strIsOnline}";
		}
	}

	public string FullPresenceSummary
	{
		get
		{
			StringBuilder builder = new StringBuilder();
			if (m_account != null)
			{
				builder.Append(m_account.FullPresenceSummary);
			}
			else
			{
				builder.Append("null bnet account");
			}
			foreach (KeyValuePair<BnetGameAccountId, BnetGameAccount> gameAccount2 in m_gameAccounts)
			{
				BnetGameAccount gameAccount = gameAccount2.Value;
				if (!(gameAccount == null))
				{
					builder.Append("\n").Append(gameAccount.FullPresenceSummary);
				}
			}
			return builder.ToString();
		}
	}

	public BnetPlayer(BnetPlayerSource source)
	{
		m_source = source;
	}

	public BnetPlayer Clone()
	{
		BnetPlayer copy = (BnetPlayer)MemberwiseClone();
		if (m_accountId != null)
		{
			copy.m_accountId = m_accountId.Clone();
		}
		if (m_account != null)
		{
			copy.m_account = m_account.Clone();
		}
		if (m_hsGameAccount != null)
		{
			copy.m_hsGameAccount = m_hsGameAccount.Clone();
		}
		if (m_bestGameAccount != null)
		{
			copy.m_bestGameAccount = m_bestGameAccount.Clone();
		}
		copy.m_gameAccounts = new Map<BnetGameAccountId, BnetGameAccount>();
		foreach (KeyValuePair<BnetGameAccountId, BnetGameAccount> kvpair in m_gameAccounts)
		{
			copy.m_gameAccounts.Add(kvpair.Key.Clone(), kvpair.Value.Clone());
		}
		return copy;
	}

	public BnetAccountId GetAccountId()
	{
		if (m_accountId != null)
		{
			return m_accountId;
		}
		BnetGameAccount gameAccount = GetFirstGameAccount();
		if (gameAccount != null)
		{
			return gameAccount.GetOwnerId();
		}
		return null;
	}

	public void SetAccountId(BnetAccountId accountId)
	{
		m_accountId = accountId;
	}

	public BnetAccount GetAccount()
	{
		return m_account;
	}

	public void SetAccount(BnetAccount account)
	{
		m_account = account;
		m_accountId = account.GetId();
	}

	public string GetFullName()
	{
		if (!(m_account == null))
		{
			return m_account.GetFullName();
		}
		return null;
	}

	public BnetBattleTag GetBattleTag()
	{
		if (m_account != null && m_account.GetBattleTag() != null)
		{
			return m_account.GetBattleTag();
		}
		BnetGameAccount gameAccount = GetFirstGameAccount();
		if (gameAccount != null)
		{
			return gameAccount.GetBattleTag();
		}
		return null;
	}

	public Map<BnetGameAccountId, BnetGameAccount> GetGameAccounts()
	{
		return m_gameAccounts;
	}

	public bool HasGameAccount(BnetGameAccountId id)
	{
		return m_gameAccounts.ContainsKey(id);
	}

	public void AddGameAccount(BnetGameAccount gameAccount)
	{
		BnetGameAccountId id = gameAccount.GetId();
		if (!m_gameAccounts.ContainsKey(id))
		{
			m_gameAccounts.Add(id, gameAccount);
			CacheSpecialGameAccounts();
		}
	}

	public bool RemoveGameAccount(BnetGameAccountId id)
	{
		if (!m_gameAccounts.Remove(id))
		{
			return false;
		}
		CacheSpecialGameAccounts();
		return true;
	}

	public BnetGameAccount GetHearthstoneGameAccount()
	{
		return m_hsGameAccount;
	}

	public BnetGameAccountId GetHearthstoneGameAccountId()
	{
		if (m_hsGameAccount == null)
		{
			return null;
		}
		return m_hsGameAccount.GetId();
	}

	public BnetGameAccount GetBestGameAccount()
	{
		return m_bestGameAccount;
	}

	public BnetGameAccountId GetBestGameAccountId()
	{
		if (m_bestGameAccount == null)
		{
			return null;
		}
		return m_bestGameAccount.GetId();
	}

	public bool IsDisplayable()
	{
		return GetBestName() != null;
	}

	public BnetGameAccount GetFirstGameAccount()
	{
		using (Map<BnetGameAccountId, BnetGameAccount>.ValueCollection.Enumerator enumerator = m_gameAccounts.Values.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return null;
	}

	public long GetPersistentGameId()
	{
		return 0L;
	}

	public string GetBestName()
	{
		if (this == BnetPresenceMgr.Get().GetMyPlayer())
		{
			if (m_hsGameAccount == null)
			{
				return null;
			}
			if (!(m_hsGameAccount.GetBattleTag() == null))
			{
				return m_hsGameAccount.GetBattleTag().GetName();
			}
			return null;
		}
		if (m_account != null)
		{
			string name = m_account.GetFullName();
			if (name != null)
			{
				return name;
			}
			if (m_account.GetBattleTag() != null)
			{
				return m_account.GetBattleTag().GetName();
			}
		}
		foreach (KeyValuePair<BnetGameAccountId, BnetGameAccount> kv in m_gameAccounts)
		{
			if (kv.Value.GetBattleTag() != null)
			{
				return kv.Value.GetBattleTag().GetName();
			}
		}
		return null;
	}

	public BnetProgramId GetBestProgramId()
	{
		if (m_bestGameAccount == null)
		{
			return null;
		}
		return m_bestGameAccount.GetProgramId();
	}

	public bool IsOnline()
	{
		foreach (KeyValuePair<BnetGameAccountId, BnetGameAccount> gameAccount2 in m_gameAccounts)
		{
			if (gameAccount2.Value.IsOnline())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAway()
	{
		if (m_account != null && m_account.IsAway())
		{
			return true;
		}
		if (m_bestGameAccount != null && m_bestGameAccount.IsAway())
		{
			return true;
		}
		return false;
	}

	public bool IsBusy()
	{
		if (m_account != null && m_account.IsBusy())
		{
			return true;
		}
		if (m_bestGameAccount != null && m_bestGameAccount.IsBusy())
		{
			return true;
		}
		return false;
	}

	public bool IsAppearingOffline()
	{
		return m_account.IsAppearingOffline();
	}

	public long GetBestAwayTimeMicrosec()
	{
		long awayTimeMicrosec = 0L;
		if (m_account != null && m_account.IsAway())
		{
			awayTimeMicrosec = Math.Max(m_account.GetAwayTimeMicrosec(), m_account.GetLastOnlineMicrosec());
			if (awayTimeMicrosec != 0L)
			{
				return awayTimeMicrosec;
			}
		}
		if (m_bestGameAccount != null && m_bestGameAccount.IsAway())
		{
			return Math.Max(m_bestGameAccount.GetAwayTimeMicrosec(), m_bestGameAccount.GetLastOnlineMicrosec());
		}
		return awayTimeMicrosec;
	}

	public long GetBestLastOnlineMicrosec()
	{
		long lastOnlineMicrosec = 0L;
		if (m_account != null)
		{
			lastOnlineMicrosec = m_account.GetLastOnlineMicrosec();
			if (lastOnlineMicrosec != 0L)
			{
				return lastOnlineMicrosec;
			}
		}
		if (m_bestGameAccount != null)
		{
			return m_bestGameAccount.GetLastOnlineMicrosec();
		}
		return lastOnlineMicrosec;
	}

	public bool HasAccount(BnetEntityId id)
	{
		if (id == null)
		{
			return false;
		}
		if (m_accountId == id)
		{
			return true;
		}
		foreach (BnetGameAccountId key in m_gameAccounts.Keys)
		{
			if (key == id)
			{
				return true;
			}
		}
		return false;
	}

	public void OnGameAccountChanged(uint fieldId)
	{
		if (fieldId == 3 || fieldId == 1 || fieldId == 4)
		{
			CacheSpecialGameAccounts();
		}
	}

	public override string ToString()
	{
		BnetAccountId accountId = GetAccountId();
		BnetBattleTag battleTag = GetBattleTag();
		if (accountId == null && battleTag == null)
		{
			return "UNKNOWN PLAYER";
		}
		return $"[account={accountId} battleTag={battleTag} numGameAccounts={m_gameAccounts.Count}]";
	}

	private void CacheSpecialGameAccounts()
	{
		m_hsGameAccount = null;
		m_bestGameAccount = null;
		long bestGameAccountLastOnlineMicrosec = 0L;
		foreach (BnetGameAccount gameAccount in m_gameAccounts.Values)
		{
			BnetProgramId programId = gameAccount.GetProgramId();
			if (programId == null)
			{
				continue;
			}
			if (programId == BnetProgramId.HEARTHSTONE)
			{
				m_hsGameAccount = gameAccount;
				if (gameAccount.IsOnline() || !BnetFriendMgr.Get().IsFriend(gameAccount.GetId()))
				{
					m_bestGameAccount = gameAccount;
				}
				break;
			}
			if (m_bestGameAccount == null)
			{
				m_bestGameAccount = gameAccount;
				bestGameAccountLastOnlineMicrosec = m_bestGameAccount.GetLastOnlineMicrosec();
				continue;
			}
			if (!m_bestGameAccount.IsOnline() && gameAccount.IsOnline())
			{
				m_bestGameAccount = gameAccount;
				bestGameAccountLastOnlineMicrosec = m_bestGameAccount.GetLastOnlineMicrosec();
				continue;
			}
			BnetProgramId bestProgramId = m_bestGameAccount.GetProgramId();
			if (gameAccount.IsOnline())
			{
				if (programId.IsGame() && !bestProgramId.IsGame())
				{
					m_bestGameAccount = gameAccount;
					bestGameAccountLastOnlineMicrosec = m_bestGameAccount.GetLastOnlineMicrosec();
				}
				else if (programId.IsGame() && bestProgramId.IsGame())
				{
					long lastOnlineMicrosec = gameAccount.GetLastOnlineMicrosec();
					if (lastOnlineMicrosec > bestGameAccountLastOnlineMicrosec)
					{
						m_bestGameAccount = gameAccount;
						bestGameAccountLastOnlineMicrosec = lastOnlineMicrosec;
					}
				}
			}
			else if (!m_bestGameAccount.IsOnline())
			{
				long lastOnlineMicrosec2 = gameAccount.GetLastOnlineMicrosec();
				if (lastOnlineMicrosec2 > bestGameAccountLastOnlineMicrosec)
				{
					m_bestGameAccount = gameAccount;
					bestGameAccountLastOnlineMicrosec = lastOnlineMicrosec2;
				}
			}
		}
	}
}
