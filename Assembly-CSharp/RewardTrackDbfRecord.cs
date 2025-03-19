using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class RewardTrackDbfRecord : DbfRecord
{
	[SerializeField]
	private DbfLocValue m_name;

	[SerializeField]
	private int m_version;

	[SerializeField]
	private int m_season;

	[SerializeField]
	private EventTimingType m_event = EventTimingType.UNKNOWN;

	[SerializeField]
	private int m_accountLicenseId;

	[SerializeField]
	private int m_seasonPassProductId;

	[SerializeField]
	private RewardTrack.RewardTrackType m_rewardTrackType;

	[SerializeField]
	private RewardTrack.RewardTrackType m_overrideQuestRewardTrackType;

	[SerializeField]
	private int m_overrideQuestPoolId;

	[SerializeField]
	private int m_levelCapSoft;

	[DbfField("NAME")]
	public DbfLocValue Name => m_name;

	[DbfField("VERSION")]
	public int Version => m_version;

	[DbfField("SEASON")]
	public int Season => m_season;

	[DbfField("EVENT")]
	public EventTimingType Event => m_event;

	public AccountLicenseDbfRecord AccountLicenseRecord => GameDbf.AccountLicense.GetRecord(m_accountLicenseId);

	[DbfField("SEASON_PASS_PRODUCT_ID")]
	public int SeasonPassProductId => m_seasonPassProductId;

	[DbfField("REWARD_TRACK_TYPE")]
	public RewardTrack.RewardTrackType RewardTrackType => m_rewardTrackType;

	[DbfField("OVERRIDE_QUEST_REWARD_TRACK_TYPE")]
	public RewardTrack.RewardTrackType OverrideQuestRewardTrackType => m_overrideQuestRewardTrackType;

	[DbfField("LEVEL_CAP_SOFT")]
	public int LevelCapSoft => m_levelCapSoft;

	public PaidPremiumTierDbfRecord PaidPremiumTier
	{
		get
		{
			int id = base.ID;
			List<PaidPremiumTierDbfRecord> records = GameDbf.PaidPremiumTier.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				PaidPremiumTierDbfRecord record = records[i];
				if (record.RewardTrackId == id)
				{
					return record;
				}
			}
			return null;
		}
	}

	public List<OverrideQuestPoolIdListDbfRecord> OverrideQuestPools
	{
		get
		{
			int id = base.ID;
			List<OverrideQuestPoolIdListDbfRecord> returnRecords = new List<OverrideQuestPoolIdListDbfRecord>();
			List<OverrideQuestPoolIdListDbfRecord> records = GameDbf.OverrideQuestPoolIdList.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				OverrideQuestPoolIdListDbfRecord record = records[i];
				if (record.PrimaryRewardTrackId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public List<RewardTrackLevelDbfRecord> Levels
	{
		get
		{
			int id = base.ID;
			List<RewardTrackLevelDbfRecord> returnRecords = new List<RewardTrackLevelDbfRecord>();
			List<RewardTrackLevelDbfRecord> records = GameDbf.RewardTrackLevel.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				RewardTrackLevelDbfRecord record = records[i];
				if (record.RewardTrackId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NAME" => m_name, 
			"VERSION" => m_version, 
			"SEASON" => m_season, 
			"EVENT" => m_event, 
			"ACCOUNT_LICENSE_ID" => m_accountLicenseId, 
			"SEASON_PASS_PRODUCT_ID" => m_seasonPassProductId, 
			"REWARD_TRACK_TYPE" => m_rewardTrackType, 
			"OVERRIDE_QUEST_REWARD_TRACK_TYPE" => m_overrideQuestRewardTrackType, 
			"OVERRIDE_QUEST_POOL_ID" => m_overrideQuestPoolId, 
			"LEVEL_CAP_SOFT" => m_levelCapSoft, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "ID":
			SetID((int)val);
			break;
		case "NAME":
			m_name = (DbfLocValue)val;
			break;
		case "VERSION":
			m_version = (int)val;
			break;
		case "SEASON":
			m_season = (int)val;
			break;
		case "EVENT":
			m_event = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "ACCOUNT_LICENSE_ID":
			m_accountLicenseId = (int)val;
			break;
		case "SEASON_PASS_PRODUCT_ID":
			m_seasonPassProductId = (int)val;
			break;
		case "REWARD_TRACK_TYPE":
			if (val == null)
			{
				m_rewardTrackType = RewardTrack.RewardTrackType.NONE;
			}
			else if (val is RewardTrack.RewardTrackType || val is int)
			{
				m_rewardTrackType = (RewardTrack.RewardTrackType)val;
			}
			else if (val is string)
			{
				m_rewardTrackType = RewardTrack.ParseRewardTrackTypeValue((string)val);
			}
			break;
		case "OVERRIDE_QUEST_REWARD_TRACK_TYPE":
			if (val == null)
			{
				m_overrideQuestRewardTrackType = RewardTrack.RewardTrackType.NONE;
			}
			else if (val is RewardTrack.RewardTrackType || val is int)
			{
				m_overrideQuestRewardTrackType = (RewardTrack.RewardTrackType)val;
			}
			else if (val is string)
			{
				m_overrideQuestRewardTrackType = RewardTrack.ParseRewardTrackTypeValue((string)val);
			}
			break;
		case "OVERRIDE_QUEST_POOL_ID":
			m_overrideQuestPoolId = (int)val;
			break;
		case "LEVEL_CAP_SOFT":
			m_levelCapSoft = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NAME" => typeof(DbfLocValue), 
			"VERSION" => typeof(int), 
			"SEASON" => typeof(int), 
			"EVENT" => typeof(string), 
			"ACCOUNT_LICENSE_ID" => typeof(int), 
			"SEASON_PASS_PRODUCT_ID" => typeof(int), 
			"REWARD_TRACK_TYPE" => typeof(RewardTrack.RewardTrackType), 
			"OVERRIDE_QUEST_REWARD_TRACK_TYPE" => typeof(RewardTrack.RewardTrackType), 
			"OVERRIDE_QUEST_POOL_ID" => typeof(int), 
			"LEVEL_CAP_SOFT" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadRewardTrackDbfRecords loadRecords = new LoadRewardTrackDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		RewardTrackDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(RewardTrackDbfAsset)) as RewardTrackDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"RewardTrackDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
			return false;
		}
		for (int i = 0; i < dbfAsset.Records.Count; i++)
		{
			dbfAsset.Records[i].StripUnusedLocales();
		}
		records = dbfAsset.Records as List<T>;
		return true;
	}

	public override bool SaveRecordsToAsset<T>(string assetPath, List<T> records)
	{
		return false;
	}

	public override void StripUnusedLocales()
	{
		m_name.StripUnusedLocales();
	}
}
