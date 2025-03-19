using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class ScenarioOverrideDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_rankedPlaySeasonId;

	[SerializeField]
	private int m_scenarioId;

	[SerializeField]
	private EventTimingType m_eventTimingEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("always");

	[SerializeField]
	private string m_seasonDesc;

	[SerializeField]
	private string m_seasonTitle;

	[DbfField("RANKED_PLAY_SEASON_ID")]
	public int RankedPlaySeasonId => m_rankedPlaySeasonId;

	[DbfField("SCENARIO_ID")]
	public int ScenarioId => m_scenarioId;

	public ScenarioDbfRecord ScenarioRecord => GameDbf.Scenario.GetRecord(m_scenarioId);

	[DbfField("EVENT_TIMING_EVENT")]
	public EventTimingType EventTimingEvent => m_eventTimingEvent;

	[DbfField("SEASON_DESC")]
	public string SeasonDesc => m_seasonDesc;

	[DbfField("SEASON_TITLE")]
	public string SeasonTitle => m_seasonTitle;

	public override object GetVar(string name)
	{
		return name switch
		{
			"RANKED_PLAY_SEASON_ID" => m_rankedPlaySeasonId, 
			"SCENARIO_ID" => m_scenarioId, 
			"EVENT_TIMING_EVENT" => m_eventTimingEvent, 
			"SEASON_DESC" => m_seasonDesc, 
			"SEASON_TITLE" => m_seasonTitle, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "RANKED_PLAY_SEASON_ID":
			m_rankedPlaySeasonId = (int)val;
			break;
		case "SCENARIO_ID":
			m_scenarioId = (int)val;
			break;
		case "EVENT_TIMING_EVENT":
			m_eventTimingEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "SEASON_DESC":
			m_seasonDesc = (string)val;
			break;
		case "SEASON_TITLE":
			m_seasonTitle = (string)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"RANKED_PLAY_SEASON_ID" => typeof(int), 
			"SCENARIO_ID" => typeof(int), 
			"EVENT_TIMING_EVENT" => typeof(string), 
			"SEASON_DESC" => typeof(string), 
			"SEASON_TITLE" => typeof(string), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadScenarioOverrideDbfRecords loadRecords = new LoadScenarioOverrideDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		ScenarioOverrideDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(ScenarioOverrideDbfAsset)) as ScenarioOverrideDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"ScenarioOverrideDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
