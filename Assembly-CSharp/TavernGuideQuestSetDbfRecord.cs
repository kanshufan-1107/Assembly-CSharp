using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class TavernGuideQuestSetDbfRecord : DbfRecord
{
	[SerializeField]
	private DbfLocValue m_title;

	[SerializeField]
	private DbfLocValue m_description;

	[SerializeField]
	private TavernGuideQuestSet.TavernGuideQuestDisplayType m_questDisplayType = TavernGuideQuestSet.ParseTavernGuideQuestDisplayTypeValue("QUEST_BOARD");

	[SerializeField]
	private TavernGuideQuestSet.TavernGuideCategory m_category = TavernGuideQuestSet.ParseTavernGuideCategoryValue("unknown");

	[SerializeField]
	private int m_categoryPosition;

	[SerializeField]
	private int m_completionAchievementId;

	[DbfField("TITLE")]
	public DbfLocValue Title => m_title;

	[DbfField("DESCRIPTION")]
	public DbfLocValue Description => m_description;

	[DbfField("QUEST_DISPLAY_TYPE")]
	public TavernGuideQuestSet.TavernGuideQuestDisplayType QuestDisplayType => m_questDisplayType;

	[DbfField("CATEGORY")]
	public TavernGuideQuestSet.TavernGuideCategory Category => m_category;

	[DbfField("CATEGORY_POSITION")]
	public int CategoryPosition => m_categoryPosition;

	[DbfField("COMPLETION_ACHIEVEMENT")]
	public int CompletionAchievement => m_completionAchievementId;

	public AchievementDbfRecord CompletionAchievementRecord => GameDbf.Achievement.GetRecord(m_completionAchievementId);

	public List<TavernGuideQuestDbfRecord> TavernGuideQuests
	{
		get
		{
			int id = base.ID;
			List<TavernGuideQuestDbfRecord> returnRecords = new List<TavernGuideQuestDbfRecord>();
			List<TavernGuideQuestDbfRecord> records = GameDbf.TavernGuideQuest.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				TavernGuideQuestDbfRecord record = records[i];
				if (record.TavernGuideQuestSetId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public List<UnlockedTavernGuideSetDbfRecord> NextUnlockedTavernGuideSets
	{
		get
		{
			int id = base.ID;
			List<UnlockedTavernGuideSetDbfRecord> returnRecords = new List<UnlockedTavernGuideSetDbfRecord>();
			List<UnlockedTavernGuideSetDbfRecord> records = GameDbf.UnlockedTavernGuideSet.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				UnlockedTavernGuideSetDbfRecord record = records[i];
				if (record.TavernGuideQuestSetId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public void SetTitle(DbfLocValue v)
	{
		m_title = v;
		v.SetDebugInfo(base.ID, "TITLE");
	}

	public void SetDescription(DbfLocValue v)
	{
		m_description = v;
		v.SetDebugInfo(base.ID, "DESCRIPTION");
	}

	public void SetQuestDisplayType(TavernGuideQuestSet.TavernGuideQuestDisplayType v)
	{
		m_questDisplayType = v;
	}

	public void SetCategory(TavernGuideQuestSet.TavernGuideCategory v)
	{
		m_category = v;
	}

	public void SetCategoryPosition(int v)
	{
		m_categoryPosition = v;
	}

	public void SetCompletionAchievement(int v)
	{
		m_completionAchievementId = v;
	}

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"TITLE" => m_title, 
			"DESCRIPTION" => m_description, 
			"QUEST_DISPLAY_TYPE" => m_questDisplayType, 
			"CATEGORY" => m_category, 
			"CATEGORY_POSITION" => m_categoryPosition, 
			"COMPLETION_ACHIEVEMENT" => m_completionAchievementId, 
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
		case "TITLE":
			m_title = (DbfLocValue)val;
			break;
		case "DESCRIPTION":
			m_description = (DbfLocValue)val;
			break;
		case "QUEST_DISPLAY_TYPE":
			if (val == null)
			{
				m_questDisplayType = TavernGuideQuestSet.TavernGuideQuestDisplayType.UNKNOWN;
			}
			else if (val is TavernGuideQuestSet.TavernGuideQuestDisplayType || val is int)
			{
				m_questDisplayType = (TavernGuideQuestSet.TavernGuideQuestDisplayType)val;
			}
			else if (val is string)
			{
				m_questDisplayType = TavernGuideQuestSet.ParseTavernGuideQuestDisplayTypeValue((string)val);
			}
			break;
		case "CATEGORY":
			if (val == null)
			{
				m_category = TavernGuideQuestSet.TavernGuideCategory.UNKNOWN;
			}
			else if (val is TavernGuideQuestSet.TavernGuideCategory || val is int)
			{
				m_category = (TavernGuideQuestSet.TavernGuideCategory)val;
			}
			else if (val is string)
			{
				m_category = TavernGuideQuestSet.ParseTavernGuideCategoryValue((string)val);
			}
			break;
		case "CATEGORY_POSITION":
			m_categoryPosition = (int)val;
			break;
		case "COMPLETION_ACHIEVEMENT":
			m_completionAchievementId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"TITLE" => typeof(DbfLocValue), 
			"DESCRIPTION" => typeof(DbfLocValue), 
			"QUEST_DISPLAY_TYPE" => typeof(TavernGuideQuestSet.TavernGuideQuestDisplayType), 
			"CATEGORY" => typeof(TavernGuideQuestSet.TavernGuideCategory), 
			"CATEGORY_POSITION" => typeof(int), 
			"COMPLETION_ACHIEVEMENT" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadTavernGuideQuestSetDbfRecords loadRecords = new LoadTavernGuideQuestSetDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		TavernGuideQuestSetDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(TavernGuideQuestSetDbfAsset)) as TavernGuideQuestSetDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"TavernGuideQuestSetDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_title.StripUnusedLocales();
		m_description.StripUnusedLocales();
	}
}
