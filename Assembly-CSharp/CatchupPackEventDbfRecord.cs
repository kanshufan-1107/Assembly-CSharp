using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class CatchupPackEventDbfRecord : DbfRecord
{
	[SerializeField]
	private EventTimingType m_eventTiming = EventTimingType.UNKNOWN;

	[SerializeField]
	private EventTimingType m_boxDressingEventTiming = EventTimingType.UNKNOWN;

	[SerializeField]
	private EventTimingType m_introLetterEventTiming = EventTimingType.UNKNOWN;

	[SerializeField]
	private bool m_hasBoxDressing;

	[SerializeField]
	private CatchupPackEvent.CatchupEventType m_catchupEventType;

	[SerializeField]
	private DbfLocValue m_letterText;

	[DbfField("BOX_DRESSING_EVENT_TIMING")]
	public EventTimingType BoxDressingEventTiming => m_boxDressingEventTiming;

	[DbfField("CATCHUP_EVENT_TYPE")]
	public CatchupPackEvent.CatchupEventType CatchupEventType => m_catchupEventType;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"EVENT_TIMING" => m_eventTiming, 
			"BOX_DRESSING_EVENT_TIMING" => m_boxDressingEventTiming, 
			"INTRO_LETTER_EVENT_TIMING" => m_introLetterEventTiming, 
			"HAS_BOX_DRESSING" => m_hasBoxDressing, 
			"CATCHUP_EVENT_TYPE" => m_catchupEventType, 
			"LETTER_TEXT" => m_letterText, 
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
		case "EVENT_TIMING":
			m_eventTiming = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "BOX_DRESSING_EVENT_TIMING":
			m_boxDressingEventTiming = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "INTRO_LETTER_EVENT_TIMING":
			m_introLetterEventTiming = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "HAS_BOX_DRESSING":
			m_hasBoxDressing = (bool)val;
			break;
		case "CATCHUP_EVENT_TYPE":
			if (val == null)
			{
				m_catchupEventType = CatchupPackEvent.CatchupEventType.CATCH_UP_WILD_WEST;
			}
			else if (val is CatchupPackEvent.CatchupEventType || val is int)
			{
				m_catchupEventType = (CatchupPackEvent.CatchupEventType)val;
			}
			else if (val is string)
			{
				m_catchupEventType = CatchupPackEvent.ParseCatchupEventTypeValue((string)val);
			}
			break;
		case "LETTER_TEXT":
			m_letterText = (DbfLocValue)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"EVENT_TIMING" => typeof(string), 
			"BOX_DRESSING_EVENT_TIMING" => typeof(string), 
			"INTRO_LETTER_EVENT_TIMING" => typeof(string), 
			"HAS_BOX_DRESSING" => typeof(bool), 
			"CATCHUP_EVENT_TYPE" => typeof(CatchupPackEvent.CatchupEventType), 
			"LETTER_TEXT" => typeof(DbfLocValue), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadCatchupPackEventDbfRecords loadRecords = new LoadCatchupPackEventDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		CatchupPackEventDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(CatchupPackEventDbfAsset)) as CatchupPackEventDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"CatchupPackEventDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_letterText.StripUnusedLocales();
	}
}
