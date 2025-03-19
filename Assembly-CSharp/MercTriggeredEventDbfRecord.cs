using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class MercTriggeredEventDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_mercenaryVillageTriggerId;

	[SerializeField]
	private MercTriggeredEvent.EventType m_eventType;

	[SerializeField]
	private bool m_successRequired = true;

	[SerializeField]
	private int m_visitorId;

	[SerializeField]
	private int m_quantity = 1;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"MERCENARY_VILLAGE_TRIGGER_ID" => m_mercenaryVillageTriggerId, 
			"EVENT_TYPE" => m_eventType, 
			"SUCCESS_REQUIRED" => m_successRequired, 
			"VISITOR" => m_visitorId, 
			"QUANTITY" => m_quantity, 
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
				m_eventType = MercTriggeredEvent.EventType.NONE;
			}
			else if (val is MercTriggeredEvent.EventType || val is int)
			{
				m_eventType = (MercTriggeredEvent.EventType)val;
			}
			else if (val is string)
			{
				m_eventType = MercTriggeredEvent.ParseEventTypeValue((string)val);
			}
			break;
		case "SUCCESS_REQUIRED":
			m_successRequired = (bool)val;
			break;
		case "VISITOR":
			m_visitorId = (int)val;
			break;
		case "QUANTITY":
			m_quantity = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"MERCENARY_VILLAGE_TRIGGER_ID" => typeof(int), 
			"EVENT_TYPE" => typeof(MercTriggeredEvent.EventType), 
			"SUCCESS_REQUIRED" => typeof(bool), 
			"VISITOR" => typeof(int), 
			"QUANTITY" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadMercTriggeredEventDbfRecords loadRecords = new LoadMercTriggeredEventDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		MercTriggeredEventDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(MercTriggeredEventDbfAsset)) as MercTriggeredEventDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"MercTriggeredEventDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
