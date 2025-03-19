using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class SubsetDbfRecord : DbfRecord
{
	[SerializeField]
	private bool m_includeAllCounterpartCards;

	[DbfField("INCLUDE_ALL_COUNTERPART_CARDS")]
	public bool IncludeAllCounterpartCards => m_includeAllCounterpartCards;

	public List<SubsetRuleDbfRecord> Rules
	{
		get
		{
			int id = base.ID;
			List<SubsetRuleDbfRecord> returnRecords = new List<SubsetRuleDbfRecord>();
			List<SubsetRuleDbfRecord> records = GameDbf.SubsetRule.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				SubsetRuleDbfRecord record = records[i];
				if (record.SubsetId == id)
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
			if (name == "INCLUDE_ALL_COUNTERPART_CARDS")
			{
				return m_includeAllCounterpartCards;
			}
			return null;
		}
		return base.ID;
	}

	public override void SetVar(string name, object val)
	{
		if (!(name == "ID"))
		{
			if (name == "INCLUDE_ALL_COUNTERPART_CARDS")
			{
				m_includeAllCounterpartCards = (bool)val;
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
			if (name == "INCLUDE_ALL_COUNTERPART_CARDS")
			{
				return typeof(bool);
			}
			return null;
		}
		return typeof(int);
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadSubsetDbfRecords loadRecords = new LoadSubsetDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		SubsetDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(SubsetDbfAsset)) as SubsetDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"SubsetDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
