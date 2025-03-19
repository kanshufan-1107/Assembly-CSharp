using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class VisitorTaskChainDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_mercenaryVisitorId;

	[DbfField("MERCENARY_VISITOR_ID")]
	public int MercenaryVisitorId => m_mercenaryVisitorId;

	public List<TaskListDbfRecord> TaskList
	{
		get
		{
			int id = base.ID;
			List<TaskListDbfRecord> returnRecords = new List<TaskListDbfRecord>();
			List<TaskListDbfRecord> records = GameDbf.TaskList.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				TaskListDbfRecord record = records[i];
				if (record.VisitorTaskChainId == id)
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
			if (name == "MERCENARY_VISITOR_ID")
			{
				return m_mercenaryVisitorId;
			}
			return null;
		}
		return base.ID;
	}

	public override void SetVar(string name, object val)
	{
		if (!(name == "ID"))
		{
			if (name == "MERCENARY_VISITOR_ID")
			{
				m_mercenaryVisitorId = (int)val;
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
			if (name == "MERCENARY_VISITOR_ID")
			{
				return typeof(int);
			}
			return null;
		}
		return typeof(int);
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadVisitorTaskChainDbfRecords loadRecords = new LoadVisitorTaskChainDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		VisitorTaskChainDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(VisitorTaskChainDbfAsset)) as VisitorTaskChainDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"VisitorTaskChainDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
