using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class MercTriggeringEventDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_mercenaryVillageTriggerId;

	[SerializeField]
	private MercTriggeringEvent.EventType m_eventType;

	[SerializeField]
	private int m_visitorId;

	[SerializeField]
	private int m_visitorTaskId;

	[SerializeField]
	private int m_buildingTierId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"MERCENARY_VILLAGE_TRIGGER_ID" => m_mercenaryVillageTriggerId, 
			"EVENT_TYPE" => m_eventType, 
			"VISITOR" => m_visitorId, 
			"VISITOR_TASK" => m_visitorTaskId, 
			"BUILDING_TIER" => m_buildingTierId, 
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
		case "MERCENARY_VILLAGE_TRIGGER_ID":
			m_mercenaryVillageTriggerId = (int)val;
			break;
		case "EVENT_TYPE":
			if (val == null)
			{
				m_eventType = MercTriggeringEvent.EventType.NONE;
			}
			else if (val is MercTriggeringEvent.EventType || val is int)
			{
				m_eventType = (MercTriggeringEvent.EventType)val;
			}
			else if (val is string)
			{
				m_eventType = MercTriggeringEvent.ParseEventTypeValue((string)val);
			}
			break;
		case "VISITOR":
			m_visitorId = (int)val;
			break;
		case "VISITOR_TASK":
			m_visitorTaskId = (int)val;
			break;
		case "BUILDING_TIER":
			m_buildingTierId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"MERCENARY_VILLAGE_TRIGGER_ID" => typeof(int), 
			"EVENT_TYPE" => typeof(MercTriggeringEvent.EventType), 
			"VISITOR" => typeof(int), 
			"VISITOR_TASK" => typeof(int), 
			"BUILDING_TIER" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadMercTriggeringEventDbfRecords loadRecords = new LoadMercTriggeringEventDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		MercTriggeringEventDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(MercTriggeringEventDbfAsset)) as MercTriggeringEventDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"MercTriggeringEventDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
