using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class OverrideQuestPoolIdListDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_primaryRewardTrackId;

	[SerializeField]
	private int m_questPoolId;

	[DbfField("PRIMARY_REWARD_TRACK_ID")]
	public int PrimaryRewardTrackId => m_primaryRewardTrackId;

	[DbfField("QUEST_POOL_ID")]
	public int QuestPoolId => m_questPoolId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"PRIMARY_REWARD_TRACK_ID" => m_primaryRewardTrackId, 
			"QUEST_POOL_ID" => m_questPoolId, 
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
		case "PRIMARY_REWARD_TRACK_ID":
			m_primaryRewardTrackId = (int)val;
			break;
		case "QUEST_POOL_ID":
			m_questPoolId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"PRIMARY_REWARD_TRACK_ID" => typeof(int), 
			"QUEST_POOL_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadOverrideQuestPoolIdListDbfRecords loadRecords = new LoadOverrideQuestPoolIdListDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		OverrideQuestPoolIdListDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(OverrideQuestPoolIdListDbfAsset)) as OverrideQuestPoolIdListDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"OverrideQuestPoolIdListDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
