using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class AchievementSubcategoryDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_achievementCategoryId;

	[SerializeField]
	private DbfLocValue m_name;

	[SerializeField]
	private string m_icon;

	[SerializeField]
	private int m_sortOrder;

	[DbfField("ACHIEVEMENT_CATEGORY_ID")]
	public int AchievementCategoryId => m_achievementCategoryId;

	[DbfField("NAME")]
	public DbfLocValue Name => m_name;

	[DbfField("ICON")]
	public string Icon => m_icon;

	[DbfField("SORT_ORDER")]
	public int SortOrder => m_sortOrder;

	public List<AchievementSectionItemDbfRecord> Sections
	{
		get
		{
			int id = base.ID;
			List<AchievementSectionItemDbfRecord> returnRecords = new List<AchievementSectionItemDbfRecord>();
			List<AchievementSectionItemDbfRecord> records = GameDbf.AchievementSectionItem.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				AchievementSectionItemDbfRecord record = records[i];
				if (record.AchievementSubcategoryId == id)
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
			"ACHIEVEMENT_CATEGORY_ID" => m_achievementCategoryId, 
			"NAME" => m_name, 
			"ICON" => m_icon, 
			"SORT_ORDER" => m_sortOrder, 
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
		case "ACHIEVEMENT_CATEGORY_ID":
			m_achievementCategoryId = (int)val;
			break;
		case "NAME":
			m_name = (DbfLocValue)val;
			break;
		case "ICON":
			m_icon = (string)val;
			break;
		case "SORT_ORDER":
			m_sortOrder = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"ACHIEVEMENT_CATEGORY_ID" => typeof(int), 
			"NAME" => typeof(DbfLocValue), 
			"ICON" => typeof(string), 
			"SORT_ORDER" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadAchievementSubcategoryDbfRecords loadRecords = new LoadAchievementSubcategoryDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		AchievementSubcategoryDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(AchievementSubcategoryDbfAsset)) as AchievementSubcategoryDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"AchievementSubcategoryDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
