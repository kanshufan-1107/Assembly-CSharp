using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceMercenaryLevelStatsDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_lettuceMercenaryId;

	[SerializeField]
	private int m_lettuceMercenaryLevelId;

	[SerializeField]
	private int m_attack = 1;

	[SerializeField]
	private int m_health = 1;

	[DbfField("LETTUCE_MERCENARY_ID")]
	public int LettuceMercenaryId => m_lettuceMercenaryId;

	[DbfField("LETTUCE_MERCENARY_LEVEL_ID")]
	public int LettuceMercenaryLevelId => m_lettuceMercenaryLevelId;

	[DbfField("ATTACK")]
	public int Attack => m_attack;

	[DbfField("HEALTH")]
	public int Health => m_health;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"LETTUCE_MERCENARY_ID" => m_lettuceMercenaryId, 
			"LETTUCE_MERCENARY_LEVEL_ID" => m_lettuceMercenaryLevelId, 
			"ATTACK" => m_attack, 
			"HEALTH" => m_health, 
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
		case "LETTUCE_MERCENARY_ID":
			m_lettuceMercenaryId = (int)val;
			break;
		case "LETTUCE_MERCENARY_LEVEL_ID":
			m_lettuceMercenaryLevelId = (int)val;
			break;
		case "ATTACK":
			m_attack = (int)val;
			break;
		case "HEALTH":
			m_health = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LETTUCE_MERCENARY_ID" => typeof(int), 
			"LETTUCE_MERCENARY_LEVEL_ID" => typeof(int), 
			"ATTACK" => typeof(int), 
			"HEALTH" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceMercenaryLevelStatsDbfRecords loadRecords = new LoadLettuceMercenaryLevelStatsDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceMercenaryLevelStatsDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceMercenaryLevelStatsDbfAsset)) as LettuceMercenaryLevelStatsDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceMercenaryLevelStatsDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
