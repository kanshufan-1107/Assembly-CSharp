using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceMercenarySpecializationDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_lettuceMercenaryId;

	[SerializeField]
	private DbfLocValue m_name;

	[DbfField("LETTUCE_MERCENARY_ID")]
	public int LettuceMercenaryId => m_lettuceMercenaryId;

	[DbfField("NAME")]
	public DbfLocValue Name => m_name;

	public List<LettuceMercenaryAbilityDbfRecord> LettuceMercenaryAbilities
	{
		get
		{
			int id = base.ID;
			List<LettuceMercenaryAbilityDbfRecord> returnRecords = new List<LettuceMercenaryAbilityDbfRecord>();
			List<LettuceMercenaryAbilityDbfRecord> records = GameDbf.LettuceMercenaryAbility.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				LettuceMercenaryAbilityDbfRecord record = records[i];
				if (record.LettuceMercenarySpecializationId == id)
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
			"LETTUCE_MERCENARY_ID" => m_lettuceMercenaryId, 
			"NAME" => m_name, 
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
		case "NAME":
			m_name = (DbfLocValue)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LETTUCE_MERCENARY_ID" => typeof(int), 
			"NAME" => typeof(DbfLocValue), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceMercenarySpecializationDbfRecords loadRecords = new LoadLettuceMercenarySpecializationDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceMercenarySpecializationDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceMercenarySpecializationDbfAsset)) as LettuceMercenarySpecializationDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceMercenarySpecializationDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_name.StripUnusedLocales();
	}
}
