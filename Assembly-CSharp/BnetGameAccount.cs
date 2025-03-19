using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using PegasusClient;
using PegasusFSG;

public class BnetGameAccount
{
	private BnetGameAccountId m_id;

	private BnetAccountId m_ownerId;

	private BnetProgramId m_programId;

	private BnetBattleTag m_battleTag;

	private bool m_away;

	private long m_awayTimeMicrosec;

	private bool m_busy;

	private bool m_online;

	private long m_lastOnlineMicrosec;

	private string m_richPresence;

	private Map<uint, object> m_gameFields = new Map<uint, object>();

	public string FullPresenceSummary => string.Format("GameAccount [id={0} battleTag={1} {2} {3} richPresence={4} away={5} busy={6} lastOnline={7} awayTime={8}]", m_id, m_battleTag, m_programId, m_online ? "online" : "offline", m_richPresence, m_away, m_busy, (m_lastOnlineMicrosec == 0L) ? "null" : TimeUtils.ConvertEpochMicrosecToDateTime(m_lastOnlineMicrosec).ToString("R"), (m_awayTimeMicrosec == 0L) ? "null" : TimeUtils.ConvertEpochMicrosecToDateTime(m_awayTimeMicrosec).ToString("R"));

	public BnetGameAccount Clone()
	{
		BnetGameAccount copy = (BnetGameAccount)MemberwiseClone();
		if (m_id != null)
		{
			copy.m_id = m_id.Clone();
		}
		if (m_ownerId != null)
		{
			copy.m_ownerId = m_ownerId.Clone();
		}
		if (m_programId != null)
		{
			copy.m_programId = m_programId.Clone();
		}
		if (m_battleTag != null)
		{
			copy.m_battleTag = m_battleTag.Clone();
		}
		copy.m_gameFields = new Map<uint, object>();
		foreach (KeyValuePair<uint, object> kvpair in m_gameFields)
		{
			copy.m_gameFields.Add(kvpair.Key, kvpair.Value);
		}
		return copy;
	}

	public BnetGameAccountId GetId()
	{
		return m_id;
	}

	public void SetId(BnetGameAccountId id)
	{
		m_id = id;
	}

	public BnetAccountId GetOwnerId()
	{
		return m_ownerId;
	}

	public void SetOwnerId(BnetAccountId id)
	{
		m_ownerId = id;
	}

	public BnetProgramId GetProgramId()
	{
		return m_programId;
	}

	public void SetProgramId(BnetProgramId programId)
	{
		m_programId = programId;
	}

	public BnetBattleTag GetBattleTag()
	{
		return m_battleTag;
	}

	public void SetBattleTag(BnetBattleTag battleTag)
	{
		m_battleTag = battleTag;
	}

	public bool IsAway()
	{
		return m_away;
	}

	public void SetAway(bool away)
	{
		m_away = away;
	}

	public long GetAwayTimeMicrosec()
	{
		return m_awayTimeMicrosec;
	}

	public void SetAwayTimeMicrosec(long awayTimeMicrosec)
	{
		m_awayTimeMicrosec = awayTimeMicrosec;
	}

	public bool IsBusy()
	{
		return m_busy;
	}

	public void SetBusy(bool busy)
	{
		m_busy = busy;
	}

	public bool IsOnline()
	{
		return m_online;
	}

	public void SetOnline(bool online)
	{
		m_online = online;
	}

	public long GetLastOnlineMicrosec()
	{
		return m_lastOnlineMicrosec;
	}

	public void SetLastOnlineMicrosec(long microsec)
	{
		m_lastOnlineMicrosec = microsec;
	}

	public string GetRichPresence()
	{
		return m_richPresence;
	}

	public void SetRichPresence(string richPresence)
	{
		m_richPresence = richPresence;
	}

	public Map<uint, object> GetGameFields()
	{
		return m_gameFields;
	}

	public bool HasGameField(uint fieldId)
	{
		return m_gameFields.ContainsKey(fieldId);
	}

	public void SetGameField(uint fieldId, object val)
	{
		m_gameFields[fieldId] = val;
	}

	public bool RemoveGameField(uint fieldId)
	{
		return m_gameFields.Remove(fieldId);
	}

	public bool TryGetGameField(uint fieldId, out object val)
	{
		return m_gameFields.TryGetValue(fieldId, out val);
	}

	public bool TryGetGameFieldBool(uint fieldId, out bool val)
	{
		val = false;
		object objVal = null;
		if (!m_gameFields.TryGetValue(fieldId, out objVal))
		{
			return false;
		}
		val = (bool)objVal;
		return true;
	}

	public bool TryGetGameFieldInt(uint fieldId, out int val)
	{
		val = 0;
		object objVal = null;
		if (!m_gameFields.TryGetValue(fieldId, out objVal))
		{
			return false;
		}
		val = (int)objVal;
		return true;
	}

	public bool TryGetGameFieldString(uint fieldId, out string val)
	{
		val = null;
		object objVal = null;
		if (!m_gameFields.TryGetValue(fieldId, out objVal))
		{
			return false;
		}
		val = (string)objVal;
		return true;
	}

	public bool TryGetGameFieldBytes(uint fieldId, out byte[] val)
	{
		val = null;
		object objVal = null;
		if (!m_gameFields.TryGetValue(fieldId, out objVal))
		{
			return false;
		}
		val = (byte[])objVal;
		return true;
	}

	public object GetGameField(uint fieldId)
	{
		object val = null;
		m_gameFields.TryGetValue(fieldId, out val);
		return val;
	}

	public bool GetGameFieldBool(uint fieldId)
	{
		object val = null;
		if (m_gameFields.TryGetValue(fieldId, out val) && val != null)
		{
			return (bool)val;
		}
		return false;
	}

	public int GetGameFieldInt(uint fieldId)
	{
		object val = null;
		if (m_gameFields.TryGetValue(fieldId, out val) && val != null)
		{
			return (int)val;
		}
		return 0;
	}

	public string GetGameFieldString(uint fieldId)
	{
		object val = null;
		if (m_gameFields.TryGetValue(fieldId, out val) && val != null)
		{
			return (string)val;
		}
		return null;
	}

	public byte[] GetGameFieldBytes(uint fieldId)
	{
		object val = null;
		if (m_gameFields.TryGetValue(fieldId, out val) && val != null)
		{
			return (byte[])val;
		}
		return null;
	}

	public BnetEntityId GetGameFieldEntityId(uint fieldId)
	{
		object val = null;
		if (m_gameFields.TryGetValue(fieldId, out val) && val != null)
		{
			return (BnetEntityId)val;
		}
		return new BnetEntityId(0uL, 0uL);
	}

	public bool CanBeInvitedToGame()
	{
		return GetGameFieldBool(1u);
	}

	public string GetClientVersion()
	{
		return GetGameFieldString(19u);
	}

	public string GetClientEnv()
	{
		return GetGameFieldString(20u);
	}

	public string GetDebugString()
	{
		return GetGameFieldString(2u);
	}

	public BnetPartyId GetPartyId()
	{
		BnetEntityId playerPartyEntityId = GetGameFieldEntityId(26u);
		return new BnetPartyId(playerPartyEntityId.High, playerPartyEntityId.Low);
	}

	public SessionRecord GetSessionRecord()
	{
		byte[] blob = GetGameFieldBytes(22u);
		if (blob != null && blob.Length != 0)
		{
			return ProtobufUtil.ParseFrom<SessionRecord>(blob);
		}
		return null;
	}

	public string GetCardsOpened()
	{
		return GetGameFieldString(4u);
	}

	public int GetLastAchievement()
	{
		return GetGameFieldInt(27u);
	}

	public int GetDruidLevel()
	{
		return GetGameFieldInt(5u);
	}

	public int GetHunterLevel()
	{
		return GetGameFieldInt(6u);
	}

	public int GetMageLevel()
	{
		return GetGameFieldInt(7u);
	}

	public int GetPaladinLevel()
	{
		return GetGameFieldInt(8u);
	}

	public int GetPriestLevel()
	{
		return GetGameFieldInt(9u);
	}

	public int GetRogueLevel()
	{
		return GetGameFieldInt(10u);
	}

	public int GetShamanLevel()
	{
		return GetGameFieldInt(11u);
	}

	public int GetWarlockLevel()
	{
		return GetGameFieldInt(12u);
	}

	public int GetWarriorLevel()
	{
		return GetGameFieldInt(13u);
	}

	public int GetDemonHunterLevel()
	{
		return GetGameFieldInt(30u);
	}

	public int GetDeathKnightLevel()
	{
		return GetGameFieldInt(31u);
	}

	public int GetGainMedal()
	{
		return GetGameFieldInt(14u);
	}

	public int GetTutorialBeaten()
	{
		return GetGameFieldInt(15u);
	}

	public bool GetBattlegroundsTutorialComplete()
	{
		return GetGameFieldInt(28u) > 0;
	}

	public bool GetMercenariesTutorialComplete()
	{
		return GetGameFieldInt(29u) > 0;
	}

	public int GetCollectionEvent()
	{
		return GetGameFieldInt(16u);
	}

	public DeckValidity GetDeckValidity()
	{
		byte[] blob = GetGameFieldBytes(24u);
		if (blob != null && blob.Length != 0)
		{
			return ProtobufUtil.ParseFrom<DeckValidity>(blob);
		}
		return null;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is BnetGameAccount other))
		{
			return false;
		}
		return m_id.Equals(other.m_id);
	}

	public bool Equals(BnetGameAccountId other)
	{
		if ((object)other == null)
		{
			return false;
		}
		return m_id.Equals(other);
	}

	public override int GetHashCode()
	{
		return m_id.GetHashCode();
	}

	public static bool operator ==(BnetGameAccount a, BnetGameAccount b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		return a.m_id == b.m_id;
	}

	public static bool operator !=(BnetGameAccount a, BnetGameAccount b)
	{
		return !(a == b);
	}

	public override string ToString()
	{
		if (m_id == null)
		{
			return "UNKNOWN GAME ACCOUNT";
		}
		return $"[id={m_id} programId={m_programId} battleTag={m_battleTag} online={m_online}]";
	}
}
