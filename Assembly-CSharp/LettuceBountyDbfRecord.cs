using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceBountyDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private int m_bountyLevel;

	[SerializeField]
	private bool m_enabled = true;

	[SerializeField]
	private EventTimingType m_event = DbfShared.GetEventMap().ConvertStringToSpecialEvent("always");

	[SerializeField]
	private EventTimingType m_availableAfterEvent = EventTimingType.UNKNOWN;

	[SerializeField]
	private DbfLocValue m_bossNameOverride;

	[SerializeField]
	private DbfLocValue m_bountyNameOverride;

	[SerializeField]
	private DbfLocValue m_posterNameOverride;

	[SerializeField]
	private DbfLocValue m_comingSoonText;

	[SerializeField]
	private int m_bountySetId;

	[SerializeField]
	private LettuceBounty.MercenariesBountyDifficulty m_difficultyMode = LettuceBounty.MercenariesBountyDifficulty.NORMAL;

	[SerializeField]
	private bool m_heroic;

	[SerializeField]
	private int m_finalBossCardId;

	[SerializeField]
	private int m_sortOrder;

	[SerializeField]
	private int m_requiredCompletedBountyId;

	[SerializeField]
	private bool m_generateFinalBossesOnServer;

	[SerializeField]
	private int m_weeklyUnlockDayOffset;

	[DbfField("NOTE_DESC")]
	public string NoteDesc => m_noteDesc;

	[DbfField("BOUNTY_LEVEL")]
	public int BountyLevel => m_bountyLevel;

	[DbfField("ENABLED")]
	public bool Enabled => m_enabled;

	[DbfField("EVENT")]
	public EventTimingType Event => m_event;

	[DbfField("AVAILABLE_AFTER_EVENT")]
	public EventTimingType AvailableAfterEvent => m_availableAfterEvent;

	[DbfField("BOSS_NAME_OVERRIDE")]
	public DbfLocValue BossNameOverride => m_bossNameOverride;

	[DbfField("BOUNTY_NAME_OVERRIDE")]
	public DbfLocValue BountyNameOverride => m_bountyNameOverride;

	[DbfField("POSTER_NAME_OVERRIDE")]
	public DbfLocValue PosterNameOverride => m_posterNameOverride;

	[DbfField("COMING_SOON_TEXT")]
	public DbfLocValue ComingSoonText => m_comingSoonText;

	[DbfField("BOUNTY_SET_ID")]
	public int BountySetId => m_bountySetId;

	public LettuceBountySetDbfRecord BountySetRecord => GameDbf.LettuceBountySet.GetRecord(m_bountySetId);

	[DbfField("DIFFICULTY_MODE")]
	public LettuceBounty.MercenariesBountyDifficulty DifficultyMode => m_difficultyMode;

	[DbfField("HEROIC")]
	public bool Heroic => m_heroic;

	[DbfField("FINAL_BOSS_CARD_ID")]
	public int FinalBossCardId => m_finalBossCardId;

	[DbfField("SORT_ORDER")]
	public int SortOrder => m_sortOrder;

	[DbfField("REQUIRED_COMPLETED_BOUNTY")]
	public int RequiredCompletedBounty => m_requiredCompletedBountyId;

	public List<LettuceBountyFinalRespresentiveRewardsDbfRecord> FinalBossRepresentiveRewards
	{
		get
		{
			int id = base.ID;
			List<LettuceBountyFinalRespresentiveRewardsDbfRecord> returnRecords = new List<LettuceBountyFinalRespresentiveRewardsDbfRecord>();
			List<LettuceBountyFinalRespresentiveRewardsDbfRecord> records = GameDbf.LettuceBountyFinalRespresentiveRewards.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				LettuceBountyFinalRespresentiveRewardsDbfRecord record = records[i];
				if (record.LettuceBountyId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public List<LettuceBountyFinalRewardsDbfRecord> FinalBossRewards
	{
		get
		{
			int id = base.ID;
			List<LettuceBountyFinalRewardsDbfRecord> returnRecords = new List<LettuceBountyFinalRewardsDbfRecord>();
			List<LettuceBountyFinalRewardsDbfRecord> records = GameDbf.LettuceBountyFinalRewards.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				LettuceBountyFinalRewardsDbfRecord record = records[i];
				if (record.LettuceBountyId == id)
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
			"NOTE_DESC" => m_noteDesc, 
			"BOUNTY_LEVEL" => m_bountyLevel, 
			"ENABLED" => m_enabled, 
			"EVENT" => m_event, 
			"AVAILABLE_AFTER_EVENT" => m_availableAfterEvent, 
			"BOSS_NAME_OVERRIDE" => m_bossNameOverride, 
			"BOUNTY_NAME_OVERRIDE" => m_bountyNameOverride, 
			"POSTER_NAME_OVERRIDE" => m_posterNameOverride, 
			"COMING_SOON_TEXT" => m_comingSoonText, 
			"BOUNTY_SET_ID" => m_bountySetId, 
			"DIFFICULTY_MODE" => m_difficultyMode, 
			"HEROIC" => m_heroic, 
			"FINAL_BOSS_CARD_ID" => m_finalBossCardId, 
			"SORT_ORDER" => m_sortOrder, 
			"REQUIRED_COMPLETED_BOUNTY" => m_requiredCompletedBountyId, 
			"GENERATE_FINAL_BOSSES_ON_SERVER" => m_generateFinalBossesOnServer, 
			"WEEKLY_UNLOCK_DAY_OFFSET" => m_weeklyUnlockDayOffset, 
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
		case "NOTE_DESC":
			m_noteDesc = (string)val;
			break;
		case "BOUNTY_LEVEL":
			m_bountyLevel = (int)val;
			break;
		case "ENABLED":
			m_enabled = (bool)val;
			break;
		case "EVENT":
			m_event = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "AVAILABLE_AFTER_EVENT":
			m_availableAfterEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "BOSS_NAME_OVERRIDE":
			m_bossNameOverride = (DbfLocValue)val;
			break;
		case "BOUNTY_NAME_OVERRIDE":
			m_bountyNameOverride = (DbfLocValue)val;
			break;
		case "POSTER_NAME_OVERRIDE":
			m_posterNameOverride = (DbfLocValue)val;
			break;
		case "COMING_SOON_TEXT":
			m_comingSoonText = (DbfLocValue)val;
			break;
		case "BOUNTY_SET_ID":
			m_bountySetId = (int)val;
			break;
		case "DIFFICULTY_MODE":
			if (val == null)
			{
				m_difficultyMode = LettuceBounty.MercenariesBountyDifficulty.NONE;
			}
			else if (val is LettuceBounty.MercenariesBountyDifficulty || val is int)
			{
				m_difficultyMode = (LettuceBounty.MercenariesBountyDifficulty)val;
			}
			else if (val is string)
			{
				m_difficultyMode = LettuceBounty.ParseMercenariesBountyDifficultyValue((string)val);
			}
			break;
		case "HEROIC":
			m_heroic = (bool)val;
			break;
		case "FINAL_BOSS_CARD_ID":
			m_finalBossCardId = (int)val;
			break;
		case "SORT_ORDER":
			m_sortOrder = (int)val;
			break;
		case "REQUIRED_COMPLETED_BOUNTY":
			m_requiredCompletedBountyId = (int)val;
			break;
		case "GENERATE_FINAL_BOSSES_ON_SERVER":
			m_generateFinalBossesOnServer = (bool)val;
			break;
		case "WEEKLY_UNLOCK_DAY_OFFSET":
			m_weeklyUnlockDayOffset = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"BOUNTY_LEVEL" => typeof(int), 
			"ENABLED" => typeof(bool), 
			"EVENT" => typeof(string), 
			"AVAILABLE_AFTER_EVENT" => typeof(string), 
			"BOSS_NAME_OVERRIDE" => typeof(DbfLocValue), 
			"BOUNTY_NAME_OVERRIDE" => typeof(DbfLocValue), 
			"POSTER_NAME_OVERRIDE" => typeof(DbfLocValue), 
			"COMING_SOON_TEXT" => typeof(DbfLocValue), 
			"BOUNTY_SET_ID" => typeof(int), 
			"DIFFICULTY_MODE" => typeof(LettuceBounty.MercenariesBountyDifficulty), 
			"HEROIC" => typeof(bool), 
			"FINAL_BOSS_CARD_ID" => typeof(int), 
			"SORT_ORDER" => typeof(int), 
			"REQUIRED_COMPLETED_BOUNTY" => typeof(int), 
			"GENERATE_FINAL_BOSSES_ON_SERVER" => typeof(bool), 
			"WEEKLY_UNLOCK_DAY_OFFSET" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceBountyDbfRecords loadRecords = new LoadLettuceBountyDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceBountyDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceBountyDbfAsset)) as LettuceBountyDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceBountyDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_bossNameOverride.StripUnusedLocales();
		m_bountyNameOverride.StripUnusedLocales();
		m_posterNameOverride.StripUnusedLocales();
		m_comingSoonText.StripUnusedLocales();
	}
}
