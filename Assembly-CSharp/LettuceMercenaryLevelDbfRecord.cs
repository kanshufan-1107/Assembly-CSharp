using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceMercenaryLevelDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_level;

	[SerializeField]
	private int m_totalXpRequired;

	[DbfField("LEVEL")]
	public int Level => m_level;

	[DbfField("TOTAL_XP_REQUIRED")]
	public int TotalXpRequired => m_totalXpRequired;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"LEVEL" => m_level, 
			"TOTAL_XP_REQUIRED" => m_totalXpRequired, 
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
		case "LEVEL":
			m_level = (int)val;
			break;
		case "TOTAL_XP_REQUIRED":
			m_totalXpRequired = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LEVEL" => typeof(int), 
			"TOTAL_XP_REQUIRED" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceMercenaryLevelDbfRecords loadRecords = new LoadLettuceMercenaryLevelDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceMercenaryLevelDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceMercenaryLevelDbfAsset)) as LettuceMercenaryLevelDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceMercenaryLevelDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
