using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class TavernGuideQuestRecommendedClassesDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_tavernGuideQuestId;

	[SerializeField]
	private int m_classId;

	[DbfField("TAVERN_GUIDE_QUEST_ID")]
	public int TavernGuideQuestId => m_tavernGuideQuestId;

	[DbfField("CLASS")]
	public int Class => m_classId;

	public ClassDbfRecord ClassRecord => GameDbf.Class.GetRecord(m_classId);

	public void SetTavernGuideQuestId(int v)
	{
		m_tavernGuideQuestId = v;
	}

	public void SetClass(int v)
	{
		m_classId = v;
	}

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"TAVERN_GUIDE_QUEST_ID" => m_tavernGuideQuestId, 
			"CLASS" => m_classId, 
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
		case "TAVERN_GUIDE_QUEST_ID":
			m_tavernGuideQuestId = (int)val;
			break;
		case "CLASS":
			m_classId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"TAVERN_GUIDE_QUEST_ID" => typeof(int), 
			"CLASS" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadTavernGuideQuestRecommendedClassesDbfRecords loadRecords = new LoadTavernGuideQuestRecommendedClassesDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		TavernGuideQuestRecommendedClassesDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(TavernGuideQuestRecommendedClassesDbfAsset)) as TavernGuideQuestRecommendedClassesDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"TavernGuideQuestRecommendedClassesDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
