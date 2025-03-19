using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceMercenaryAbilityDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_lettuceMercenarySpecializationId;

	[SerializeField]
	private int m_lettuceAbilityId;

	[SerializeField]
	private int m_lettuceMercenaryLevelIdRequiredId;

	[DbfField("LETTUCE_MERCENARY_SPECIALIZATION_ID")]
	public int LettuceMercenarySpecializationId => m_lettuceMercenarySpecializationId;

	[DbfField("LETTUCE_ABILITY_ID")]
	public int LettuceAbilityId => m_lettuceAbilityId;

	public LettuceAbilityDbfRecord LettuceAbilityRecord => GameDbf.LettuceAbility.GetRecord(m_lettuceAbilityId);

	[DbfField("LETTUCE_MERCENARY_LEVEL_ID_REQUIRED")]
	public int LettuceMercenaryLevelIdRequired => m_lettuceMercenaryLevelIdRequiredId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"LETTUCE_MERCENARY_SPECIALIZATION_ID" => m_lettuceMercenarySpecializationId, 
			"LETTUCE_ABILITY_ID" => m_lettuceAbilityId, 
			"LETTUCE_MERCENARY_LEVEL_ID_REQUIRED" => m_lettuceMercenaryLevelIdRequiredId, 
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
		case "LETTUCE_MERCENARY_SPECIALIZATION_ID":
			m_lettuceMercenarySpecializationId = (int)val;
			break;
		case "LETTUCE_ABILITY_ID":
			m_lettuceAbilityId = (int)val;
			break;
		case "LETTUCE_MERCENARY_LEVEL_ID_REQUIRED":
			m_lettuceMercenaryLevelIdRequiredId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LETTUCE_MERCENARY_SPECIALIZATION_ID" => typeof(int), 
			"LETTUCE_ABILITY_ID" => typeof(int), 
			"LETTUCE_MERCENARY_LEVEL_ID_REQUIRED" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceMercenaryAbilityDbfRecords loadRecords = new LoadLettuceMercenaryAbilityDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceMercenaryAbilityDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceMercenaryAbilityDbfAsset)) as LettuceMercenaryAbilityDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceMercenaryAbilityDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
