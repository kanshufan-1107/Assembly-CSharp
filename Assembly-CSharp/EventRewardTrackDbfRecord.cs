using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class EventRewardTrackDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_specialEventId;

	[SerializeField]
	private int m_eventRewardTrackId;

	[SerializeField]
	private DbfLocValue m_choiceConfirmationText;

	[SerializeField]
	private DbfLocValue m_shortConclusion;

	[SerializeField]
	private DbfLocValue m_longConclusion;

	[DbfField("SPECIAL_EVENT_ID")]
	public int SpecialEventId => m_specialEventId;

	[DbfField("EVENT_REWARD_TRACK_ID")]
	public int EventRewardTrackId => m_eventRewardTrackId;

	[DbfField("CHOICE_CONFIRMATION_TEXT")]
	public DbfLocValue ChoiceConfirmationText => m_choiceConfirmationText;

	[DbfField("SHORT_CONCLUSION")]
	public DbfLocValue ShortConclusion => m_shortConclusion;

	[DbfField("LONG_CONCLUSION")]
	public DbfLocValue LongConclusion => m_longConclusion;

	public override object GetVar(string name)
	{
		return name switch
		{
			"SPECIAL_EVENT_ID" => m_specialEventId, 
			"EVENT_REWARD_TRACK_ID" => m_eventRewardTrackId, 
			"CHOICE_CONFIRMATION_TEXT" => m_choiceConfirmationText, 
			"SHORT_CONCLUSION" => m_shortConclusion, 
			"LONG_CONCLUSION" => m_longConclusion, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "SPECIAL_EVENT_ID":
			m_specialEventId = (int)val;
			break;
		case "EVENT_REWARD_TRACK_ID":
			m_eventRewardTrackId = (int)val;
			break;
		case "CHOICE_CONFIRMATION_TEXT":
			m_choiceConfirmationText = (DbfLocValue)val;
			break;
		case "SHORT_CONCLUSION":
			m_shortConclusion = (DbfLocValue)val;
			break;
		case "LONG_CONCLUSION":
			m_longConclusion = (DbfLocValue)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"SPECIAL_EVENT_ID" => typeof(int), 
			"EVENT_REWARD_TRACK_ID" => typeof(int), 
			"CHOICE_CONFIRMATION_TEXT" => typeof(DbfLocValue), 
			"SHORT_CONCLUSION" => typeof(DbfLocValue), 
			"LONG_CONCLUSION" => typeof(DbfLocValue), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadEventRewardTrackDbfRecords loadRecords = new LoadEventRewardTrackDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		EventRewardTrackDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(EventRewardTrackDbfAsset)) as EventRewardTrackDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"EventRewardTrackDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_choiceConfirmationText.StripUnusedLocales();
		m_shortConclusion.StripUnusedLocales();
		m_longConclusion.StripUnusedLocales();
	}
}
