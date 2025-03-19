using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class ReplacementsWhenPlayedDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_primaryCardId;

	[SerializeField]
	private int m_spawnWhenPlayedCardId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"PRIMARY_CARD_ID" => m_primaryCardId, 
			"SPAWN_WHEN_PLAYED_CARD_ID" => m_spawnWhenPlayedCardId, 
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
		case "PRIMARY_CARD_ID":
			m_primaryCardId = (int)val;
			break;
		case "SPAWN_WHEN_PLAYED_CARD_ID":
			m_spawnWhenPlayedCardId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"PRIMARY_CARD_ID" => typeof(int), 
			"SPAWN_WHEN_PLAYED_CARD_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadReplacementsWhenPlayedDbfRecords loadRecords = new LoadReplacementsWhenPlayedDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		ReplacementsWhenPlayedDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(ReplacementsWhenPlayedDbfAsset)) as ReplacementsWhenPlayedDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"ReplacementsWhenPlayedDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
