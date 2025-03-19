using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class RankedPlaySeasonDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_scenarioId;

	[SerializeField]
	private int m_season;

	[SerializeField]
	private Assets.RankedPlaySeason.FormatType m_formatType;

	[SerializeField]
	private string m_seasonDesc;

	[SerializeField]
	private string m_seasonTitle;

	public ScenarioDbfRecord ScenarioRecord => GameDbf.Scenario.GetRecord(m_scenarioId);

	[DbfField("SEASON")]
	public int Season => m_season;

	[DbfField("SEASON_DESC")]
	public string SeasonDesc => m_seasonDesc;

	[DbfField("SEASON_TITLE")]
	public string SeasonTitle => m_seasonTitle;

	public List<HiddenCardSetsDbfRecord> HiddenCardSets
	{
		get
		{
			int id = base.ID;
			List<HiddenCardSetsDbfRecord> returnRecords = new List<HiddenCardSetsDbfRecord>();
			List<HiddenCardSetsDbfRecord> records = GameDbf.HiddenCardSets.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				HiddenCardSetsDbfRecord record = records[i];
				if (record.RankedPlaySeasonId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public List<ScenarioOverrideDbfRecord> ScenarioOverrides
	{
		get
		{
			int id = base.ID;
			List<ScenarioOverrideDbfRecord> returnRecords = new List<ScenarioOverrideDbfRecord>();
			List<ScenarioOverrideDbfRecord> records = GameDbf.ScenarioOverride.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				ScenarioOverrideDbfRecord record = records[i];
				if (record.RankedPlaySeasonId == id)
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
			"SCENARIO_ID" => m_scenarioId, 
			"SEASON" => m_season, 
			"FORMAT_TYPE" => m_formatType, 
			"SEASON_DESC" => m_seasonDesc, 
			"SEASON_TITLE" => m_seasonTitle, 
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
		case "SCENARIO_ID":
			m_scenarioId = (int)val;
			break;
		case "SEASON":
			m_season = (int)val;
			break;
		case "FORMAT_TYPE":
			if (val == null)
			{
				m_formatType = Assets.RankedPlaySeason.FormatType.FT_UNKNOWN;
			}
			else if (val is Assets.RankedPlaySeason.FormatType || val is int)
			{
				m_formatType = (Assets.RankedPlaySeason.FormatType)val;
			}
			else if (val is string)
			{
				m_formatType = Assets.RankedPlaySeason.ParseFormatTypeValue((string)val);
			}
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
			"ID" => typeof(int), 
			"SCENARIO_ID" => typeof(int), 
			"SEASON" => typeof(int), 
			"FORMAT_TYPE" => typeof(Assets.RankedPlaySeason.FormatType), 
			"SEASON_DESC" => typeof(string), 
			"SEASON_TITLE" => typeof(string), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadRankedPlaySeasonDbfRecords loadRecords = new LoadRankedPlaySeasonDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		RankedPlaySeasonDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(RankedPlaySeasonDbfAsset)) as RankedPlaySeasonDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"RankedPlaySeasonDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
