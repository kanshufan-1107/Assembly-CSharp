using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceAbilityDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private DbfLocValue m_abilityName;

	[DbfField("NOTE_DESC")]
	public string NoteDesc => m_noteDesc;

	[DbfField("ABILITY_NAME")]
	public DbfLocValue AbilityName => m_abilityName;

	public List<LettuceAbilityTierDbfRecord> LettuceAbilityTiers
	{
		get
		{
			int id = base.ID;
			List<LettuceAbilityTierDbfRecord> returnRecords = new List<LettuceAbilityTierDbfRecord>();
			List<LettuceAbilityTierDbfRecord> records = GameDbf.LettuceAbilityTier.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				LettuceAbilityTierDbfRecord record = records[i];
				if (record.LettuceAbilityId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public List<MythicAbilityScalingCardTagDbfRecord> ScalingTags
	{
		get
		{
			int id = base.ID;
			List<MythicAbilityScalingCardTagDbfRecord> returnRecords = new List<MythicAbilityScalingCardTagDbfRecord>();
			List<MythicAbilityScalingCardTagDbfRecord> records = GameDbf.MythicAbilityScalingCardTag.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				MythicAbilityScalingCardTagDbfRecord record = records[i];
				if (record.LettuceAbilityId == id)
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
			"NOTE_DESC" => m_noteDesc, 
			"ABILITY_NAME" => m_abilityName, 
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
		case "NOTE_DESC":
			m_noteDesc = (string)val;
			break;
		case "ABILITY_NAME":
			m_abilityName = (DbfLocValue)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"ABILITY_NAME" => typeof(DbfLocValue), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceAbilityDbfRecords loadRecords = new LoadLettuceAbilityDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceAbilityDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceAbilityDbfAsset)) as LettuceAbilityDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceAbilityDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_abilityName.StripUnusedLocales();
	}
}
