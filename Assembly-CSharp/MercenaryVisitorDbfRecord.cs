using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class MercenaryVisitorDbfRecord : DbfRecord
{
	[SerializeField]
	private EventTimingType m_event = EventTimingType.UNKNOWN;

	[SerializeField]
	private int m_mercenaryId;

	[SerializeField]
	private MercenaryVisitor.VillageVisitorType m_visitorType;

	[SerializeField]
	private int m_maxEventTasksPerDay = 1;

	[DbfField("EVENT")]
	public EventTimingType Event => m_event;

	[DbfField("MERCENARY_ID")]
	public int MercenaryId => m_mercenaryId;

	[DbfField("VISITOR_TYPE")]
	public MercenaryVisitor.VillageVisitorType VisitorType => m_visitorType;

	public List<VisitorTaskChainDbfRecord> VisitorTaskChains
	{
		get
		{
			int id = base.ID;
			List<VisitorTaskChainDbfRecord> returnRecords = new List<VisitorTaskChainDbfRecord>();
			List<VisitorTaskChainDbfRecord> records = GameDbf.VisitorTaskChain.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				VisitorTaskChainDbfRecord record = records[i];
				if (record.MercenaryVisitorId == id)
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
			"EVENT" => m_event, 
			"MERCENARY_ID" => m_mercenaryId, 
			"VISITOR_TYPE" => m_visitorType, 
			"MAX_EVENT_TASKS_PER_DAY" => m_maxEventTasksPerDay, 
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
		case "EVENT":
			m_event = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "MERCENARY_ID":
			m_mercenaryId = (int)val;
			break;
		case "VISITOR_TYPE":
			if (val == null)
			{
				m_visitorType = MercenaryVisitor.VillageVisitorType.STANDARD;
			}
			else if (val is MercenaryVisitor.VillageVisitorType || val is int)
			{
				m_visitorType = (MercenaryVisitor.VillageVisitorType)val;
			}
			else if (val is string)
			{
				m_visitorType = MercenaryVisitor.ParseVillageVisitorTypeValue((string)val);
			}
			break;
		case "MAX_EVENT_TASKS_PER_DAY":
			m_maxEventTasksPerDay = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"EVENT" => typeof(string), 
			"MERCENARY_ID" => typeof(int), 
			"VISITOR_TYPE" => typeof(MercenaryVisitor.VillageVisitorType), 
			"MAX_EVENT_TASKS_PER_DAY" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadMercenaryVisitorDbfRecords loadRecords = new LoadMercenaryVisitorDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		MercenaryVisitorDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(MercenaryVisitorDbfAsset)) as MercenaryVisitorDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"MercenaryVisitorDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
