using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceEquipmentDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[DbfField("NOTE_DESC")]
	public string NoteDesc => m_noteDesc;

	public List<LettuceEquipmentTierDbfRecord> LettuceEquipmentTiers
	{
		get
		{
			int id = base.ID;
			List<LettuceEquipmentTierDbfRecord> returnRecords = new List<LettuceEquipmentTierDbfRecord>();
			List<LettuceEquipmentTierDbfRecord> records = GameDbf.LettuceEquipmentTier.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				LettuceEquipmentTierDbfRecord record = records[i];
				if (record.LettuceEquipmentId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public List<MythicEquipmentScalingCardTagDbfRecord> ScalingTags
	{
		get
		{
			int id = base.ID;
			List<MythicEquipmentScalingCardTagDbfRecord> returnRecords = new List<MythicEquipmentScalingCardTagDbfRecord>();
			List<MythicEquipmentScalingCardTagDbfRecord> records = GameDbf.MythicEquipmentScalingCardTag.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				MythicEquipmentScalingCardTagDbfRecord record = records[i];
				if (record.LettuceEquipmentId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public override object GetVar(string name)
	{
		if (!(name == "ID"))
		{
			if (name == "NOTE_DESC")
			{
				return m_noteDesc;
			}
			return null;
		}
		return base.ID;
	}

	public override void SetVar(string name, object val)
	{
		if (!(name == "ID"))
		{
			if (name == "NOTE_DESC")
			{
				m_noteDesc = (string)val;
			}
		}
		else
		{
			SetID((int)val);
		}
	}

	public override Type GetVarType(string name)
	{
		if (!(name == "ID"))
		{
			if (name == "NOTE_DESC")
			{
				return typeof(string);
			}
			return null;
		}
		return typeof(int);
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceEquipmentDbfRecords loadRecords = new LoadLettuceEquipmentDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceEquipmentDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceEquipmentDbfAsset)) as LettuceEquipmentDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceEquipmentDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
