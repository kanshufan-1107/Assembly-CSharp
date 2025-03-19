using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class FormulaDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	public List<FormulaChangePointDbfRecord> FormulaChangePoint
	{
		get
		{
			int id = base.ID;
			List<FormulaChangePointDbfRecord> returnRecords = new List<FormulaChangePointDbfRecord>();
			List<FormulaChangePointDbfRecord> records = GameDbf.FormulaChangePoint.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				FormulaChangePointDbfRecord record = records[i];
				if (record.FormulaId == id)
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
		LoadFormulaDbfRecords loadRecords = new LoadFormulaDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		FormulaDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(FormulaDbfAsset)) as FormulaDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"FormulaDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
