using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class TavernGuideQuestDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_tavernGuideQuestSetId;

	[SerializeField]
	private DbfLocValue m_selectedDescription;

	[SerializeField]
	private DbfLocValue m_unlockRequirementDescription;

	[SerializeField]
	private TavernGuideQuest.UnlockableGameMode m_unlockRequirementMode = TavernGuideQuest.ParseUnlockableGameModeValue("INVALID");

	[SerializeField]
	private int m_unlockRequirementLevel;

	[SerializeField]
	private bool m_showQuota;

	[SerializeField]
	private int m_questId;

	[DbfField("TAVERN_GUIDE_QUEST_SET_ID")]
	public int TavernGuideQuestSetId => m_tavernGuideQuestSetId;

	[DbfField("SELECTED_DESCRIPTION")]
	public DbfLocValue SelectedDescription => m_selectedDescription;

	[DbfField("UNLOCK_REQUIREMENT_DESCRIPTION")]
	public DbfLocValue UnlockRequirementDescription => m_unlockRequirementDescription;

	[DbfField("UNLOCK_REQUIREMENT_MODE")]
	public TavernGuideQuest.UnlockableGameMode UnlockRequirementMode => m_unlockRequirementMode;

	[DbfField("UNLOCK_REQUIREMENT_LEVEL")]
	public int UnlockRequirementLevel => m_unlockRequirementLevel;

	[DbfField("SHOW_QUOTA")]
	public bool ShowQuota => m_showQuota;

	[DbfField("QUEST")]
	public int Quest => m_questId;

	public QuestDbfRecord QuestRecord => GameDbf.Quest.GetRecord(m_questId);

	public List<TavernGuideQuestRecommendedClassesDbfRecord> RecommendedClasses
	{
		get
		{
			int id = base.ID;
			List<TavernGuideQuestRecommendedClassesDbfRecord> returnRecords = new List<TavernGuideQuestRecommendedClassesDbfRecord>();
			List<TavernGuideQuestRecommendedClassesDbfRecord> records = GameDbf.TavernGuideQuestRecommendedClasses.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				TavernGuideQuestRecommendedClassesDbfRecord record = records[i];
				if (record.TavernGuideQuestId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public void SetTavernGuideQuestSetId(int v)
	{
		m_tavernGuideQuestSetId = v;
	}

	public void SetSelectedDescription(DbfLocValue v)
	{
		m_selectedDescription = v;
		v.SetDebugInfo(base.ID, "SELECTED_DESCRIPTION");
	}

	public void SetUnlockRequirementDescription(DbfLocValue v)
	{
		m_unlockRequirementDescription = v;
		v.SetDebugInfo(base.ID, "UNLOCK_REQUIREMENT_DESCRIPTION");
	}

	public void SetUnlockRequirementMode(TavernGuideQuest.UnlockableGameMode v)
	{
		m_unlockRequirementMode = v;
	}

	public void SetUnlockRequirementLevel(int v)
	{
		m_unlockRequirementLevel = v;
	}

	public void SetShowQuota(bool v)
	{
		m_showQuota = v;
	}

	public void SetQuest(int v)
	{
		m_questId = v;
	}

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"TAVERN_GUIDE_QUEST_SET_ID" => m_tavernGuideQuestSetId, 
			"SELECTED_DESCRIPTION" => m_selectedDescription, 
			"UNLOCK_REQUIREMENT_DESCRIPTION" => m_unlockRequirementDescription, 
			"UNLOCK_REQUIREMENT_MODE" => m_unlockRequirementMode, 
			"UNLOCK_REQUIREMENT_LEVEL" => m_unlockRequirementLevel, 
			"SHOW_QUOTA" => m_showQuota, 
			"QUEST" => m_questId, 
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
		case "TAVERN_GUIDE_QUEST_SET_ID":
			m_tavernGuideQuestSetId = (int)val;
			break;
		case "SELECTED_DESCRIPTION":
			m_selectedDescription = (DbfLocValue)val;
			break;
		case "UNLOCK_REQUIREMENT_DESCRIPTION":
			m_unlockRequirementDescription = (DbfLocValue)val;
			break;
		case "UNLOCK_REQUIREMENT_MODE":
			if (val == null)
			{
				m_unlockRequirementMode = TavernGuideQuest.UnlockableGameMode.INVALID;
			}
			else if (val is TavernGuideQuest.UnlockableGameMode || val is int)
			{
				m_unlockRequirementMode = (TavernGuideQuest.UnlockableGameMode)val;
			}
			else if (val is string)
			{
				m_unlockRequirementMode = TavernGuideQuest.ParseUnlockableGameModeValue((string)val);
			}
			break;
		case "UNLOCK_REQUIREMENT_LEVEL":
			m_unlockRequirementLevel = (int)val;
			break;
		case "SHOW_QUOTA":
			m_showQuota = (bool)val;
			break;
		case "QUEST":
			m_questId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"TAVERN_GUIDE_QUEST_SET_ID" => typeof(int), 
			"SELECTED_DESCRIPTION" => typeof(DbfLocValue), 
			"UNLOCK_REQUIREMENT_DESCRIPTION" => typeof(DbfLocValue), 
			"UNLOCK_REQUIREMENT_MODE" => typeof(TavernGuideQuest.UnlockableGameMode), 
			"UNLOCK_REQUIREMENT_LEVEL" => typeof(int), 
			"SHOW_QUOTA" => typeof(bool), 
			"QUEST" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadTavernGuideQuestDbfRecords loadRecords = new LoadTavernGuideQuestDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		TavernGuideQuestDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(TavernGuideQuestDbfAsset)) as TavernGuideQuestDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"TavernGuideQuestDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_selectedDescription.StripUnusedLocales();
		m_unlockRequirementDescription.StripUnusedLocales();
	}
}
