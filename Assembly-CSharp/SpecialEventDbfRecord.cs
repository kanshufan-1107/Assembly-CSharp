using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class SpecialEventDbfRecord : DbfRecord
{
	[SerializeField]
	private SpecialEvent.SpecialEventType m_specialEventType;

	[SerializeField]
	private EventTimingType m_eventTiming = EventTimingType.UNKNOWN;

	[SerializeField]
	private DbfLocValue m_displayName;

	[SerializeField]
	private DbfLocValue m_shortDescription;

	[SerializeField]
	private DbfLocValue m_longDescription;

	[SerializeField]
	private DbfLocValue m_chooseATrackPrompt;

	[DbfField("SPECIAL_EVENT_TYPE")]
	public SpecialEvent.SpecialEventType SpecialEventType => m_specialEventType;

	[DbfField("EVENT_TIMING")]
	public EventTimingType EventTiming => m_eventTiming;

	[DbfField("DISPLAY_NAME")]
	public DbfLocValue DisplayName => m_displayName;

	[DbfField("SHORT_DESCRIPTION")]
	public DbfLocValue ShortDescription => m_shortDescription;

	[DbfField("LONG_DESCRIPTION")]
	public DbfLocValue LongDescription => m_longDescription;

	[DbfField("CHOOSE_A_TRACK_PROMPT")]
	public DbfLocValue ChooseATrackPrompt => m_chooseATrackPrompt;

	public List<EventRewardTrackDbfRecord> RewardTracks
	{
		get
		{
			int id = base.ID;
			List<EventRewardTrackDbfRecord> returnRecords = new List<EventRewardTrackDbfRecord>();
			List<EventRewardTrackDbfRecord> records = GameDbf.EventRewardTrack.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				EventRewardTrackDbfRecord record = records[i];
				if (record.SpecialEventId == id)
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
			"SPECIAL_EVENT_TYPE" => m_specialEventType, 
			"EVENT_TIMING" => m_eventTiming, 
			"DISPLAY_NAME" => m_displayName, 
			"SHORT_DESCRIPTION" => m_shortDescription, 
			"LONG_DESCRIPTION" => m_longDescription, 
			"CHOOSE_A_TRACK_PROMPT" => m_chooseATrackPrompt, 
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
		case "SPECIAL_EVENT_TYPE":
			if (val == null)
			{
				m_specialEventType = SpecialEvent.SpecialEventType.INVALID;
			}
			else if (val is SpecialEvent.SpecialEventType || val is int)
			{
				m_specialEventType = (SpecialEvent.SpecialEventType)val;
			}
			else if (val is string)
			{
				m_specialEventType = SpecialEvent.ParseSpecialEventTypeValue((string)val);
			}
			break;
		case "EVENT_TIMING":
			m_eventTiming = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "DISPLAY_NAME":
			m_displayName = (DbfLocValue)val;
			break;
		case "SHORT_DESCRIPTION":
			m_shortDescription = (DbfLocValue)val;
			break;
		case "LONG_DESCRIPTION":
			m_longDescription = (DbfLocValue)val;
			break;
		case "CHOOSE_A_TRACK_PROMPT":
			m_chooseATrackPrompt = (DbfLocValue)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"SPECIAL_EVENT_TYPE" => typeof(SpecialEvent.SpecialEventType), 
			"EVENT_TIMING" => typeof(string), 
			"DISPLAY_NAME" => typeof(DbfLocValue), 
			"SHORT_DESCRIPTION" => typeof(DbfLocValue), 
			"LONG_DESCRIPTION" => typeof(DbfLocValue), 
			"CHOOSE_A_TRACK_PROMPT" => typeof(DbfLocValue), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadSpecialEventDbfRecords loadRecords = new LoadSpecialEventDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		SpecialEventDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(SpecialEventDbfAsset)) as SpecialEventDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"SpecialEventDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_displayName.StripUnusedLocales();
		m_shortDescription.StripUnusedLocales();
		m_longDescription.StripUnusedLocales();
		m_chooseATrackPrompt.StripUnusedLocales();
	}
}
