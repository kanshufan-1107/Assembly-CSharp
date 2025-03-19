using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class SetRotationEventDbfRecord : DbfRecord
{
	[SerializeField]
	private EventTimingType m_eventTiming = EventTimingType.UNKNOWN;

	[DbfField("EVENT_TIMING")]
	public EventTimingType EventTiming => m_eventTiming;

	public override object GetVar(string name)
	{
		if (!(name == "ID"))
		{
			if (name == "EVENT_TIMING")
			{
				return m_eventTiming;
			}
			return null;
		}
		return base.ID;
	}

	public override void SetVar(string name, object val)
	{
		if (!(name == "ID"))
		{
			if (name == "EVENT_TIMING")
			{
				m_eventTiming = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
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
			if (name == "EVENT_TIMING")
			{
				return typeof(string);
			}
			return null;
		}
		return typeof(int);
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadSetRotationEventDbfRecords loadRecords = new LoadSetRotationEventDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		SetRotationEventDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(SetRotationEventDbfAsset)) as SetRotationEventDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"SetRotationEventDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
