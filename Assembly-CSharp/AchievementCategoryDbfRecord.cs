using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class AchievementCategoryDbfRecord : DbfRecord
{
	[SerializeField]
	private DbfLocValue m_name;

	[SerializeField]
	private string m_icon;

	[SerializeField]
	private int m_sortOrder;

	[DbfField("NAME")]
	public DbfLocValue Name => m_name;

	[DbfField("ICON")]
	public string Icon => m_icon;

	[DbfField("SORT_ORDER")]
	public int SortOrder => m_sortOrder;

	public List<AchievementSubcategoryDbfRecord> Subcategories
	{
		get
		{
			int id = base.ID;
			List<AchievementSubcategoryDbfRecord> returnRecords = new List<AchievementSubcategoryDbfRecord>();
			List<AchievementSubcategoryDbfRecord> records = GameDbf.AchievementSubcategory.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				AchievementSubcategoryDbfRecord record = records[i];
				if (record.AchievementCategoryId == id)
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
			"NAME" => typeof(DbfLocValue), 
			"ICON" => typeof(string), 
			"SORT_ORDER" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadAchievementCategoryDbfRecords loadRecords = new LoadAchievementCategoryDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		AchievementCategoryDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(AchievementCategoryDbfAsset)) as AchievementCategoryDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"AchievementCategoryDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
