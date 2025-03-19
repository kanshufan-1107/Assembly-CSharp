using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class AchievementSectionItemDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_achievementSubcategoryId;

	[SerializeField]
	private int m_achievementSectionId;

	[SerializeField]
	private int m_sortOrder;

	[DbfField("ACHIEVEMENT_SUBCATEGORY_ID")]
	public int AchievementSubcategoryId => m_achievementSubcategoryId;

	public AchievementSectionDbfRecord AchievementSectionRecord => GameDbf.AchievementSection.GetRecord(m_achievementSectionId);

	[DbfField("SORT_ORDER")]
	public int SortOrder => m_sortOrder;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"ACHIEVEMENT_SUBCATEGORY_ID" => m_achievementSubcategoryId, 
			"ACHIEVEMENT_SECTION" => m_achievementSectionId, 
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
		case "ACHIEVEMENT_SUBCATEGORY_ID":
			m_achievementSubcategoryId = (int)val;
			break;
		case "ACHIEVEMENT_SECTION":
			m_achievementSectionId = (int)val;
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
			"ACHIEVEMENT_SUBCATEGORY_ID" => typeof(int), 
			"ACHIEVEMENT_SECTION" => typeof(int), 
			"SORT_ORDER" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadAchievementSectionItemDbfRecords loadRecords = new LoadAchievementSectionItemDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		AchievementSectionItemDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(AchievementSectionItemDbfAsset)) as AchievementSectionItemDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"AchievementSectionItemDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
	}
}
