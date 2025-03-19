using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class UnlockedTavernGuideSetDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_tavernGuideQuestSetId;

	[SerializeField]
	private int m_unlockedTavernGuideQuestSetId;

	[DbfField("TAVERN_GUIDE_QUEST_SET_ID")]
	public int TavernGuideQuestSetId => m_tavernGuideQuestSetId;

	[DbfField("UNLOCKED_TAVERN_GUIDE_QUEST_SET")]
	public int UnlockedTavernGuideQuestSet => m_unlockedTavernGuideQuestSetId;

	public TavernGuideQuestSetDbfRecord UnlockedTavernGuideQuestSetRecord => GameDbf.TavernGuideQuestSet.GetRecord(m_unlockedTavernGuideQuestSetId);

	public void SetTavernGuideQuestSetId(int v)
	{
		m_tavernGuideQuestSetId = v;
	}

	public void SetUnlockedTavernGuideQuestSet(int v)
	{
		m_unlockedTavernGuideQuestSetId = v;
	}

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"TAVERN_GUIDE_QUEST_SET_ID" => m_tavernGuideQuestSetId, 
			"UNLOCKED_TAVERN_GUIDE_QUEST_SET" => m_unlockedTavernGuideQuestSetId, 
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
		case "UNLOCKED_TAVERN_GUIDE_QUEST_SET":
			m_unlockedTavernGuideQuestSetId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"TAVERN_GUIDE_QUEST_SET_ID" => typeof(int), 
			"UNLOCKED_TAVERN_GUIDE_QUEST_SET" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadUnlockedTavernGuideSetDbfRecords loadRecords = new LoadUnlockedTavernGuideSetDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		UnlockedTavernGuideSetDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(UnlockedTavernGuideSetDbfAsset)) as UnlockedTavernGuideSetDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"UnlockedTavernGuideSetDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
