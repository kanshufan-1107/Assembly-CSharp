using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class PvpdrSeasonDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private EventTimingType m_event = EventTimingType.UNKNOWN;

	[SerializeField]
	private int m_adventureId;

	[SerializeField]
	private int m_scenarioId;

	[SerializeField]
	private int m_maxWins;

	[SerializeField]
	private int m_maxLosses;

	[SerializeField]
	private int m_deckDisplayRulesetId;

	[SerializeField]
	private int m_maxHeroesDrafted;

	[SerializeField]
	private int m_rewardChestId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NOTE_DESC" => m_noteDesc, 
			"EVENT" => m_event, 
			"ADVENTURE_ID" => m_adventureId, 
			"SCENARIO_ID" => m_scenarioId, 
			"MAX_WINS" => m_maxWins, 
			"MAX_LOSSES" => m_maxLosses, 
			"DECK_DISPLAY_RULESET_ID" => m_deckDisplayRulesetId, 
			"MAX_HEROES_DRAFTED" => m_maxHeroesDrafted, 
			"REWARD_CHEST_ID" => m_rewardChestId, 
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
		case "NOTE_DESC":
			m_noteDesc = (string)val;
			break;
		case "EVENT":
			m_event = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "ADVENTURE_ID":
			m_adventureId = (int)val;
			break;
		case "SCENARIO_ID":
			m_scenarioId = (int)val;
			break;
		case "MAX_WINS":
			m_maxWins = (int)val;
			break;
		case "MAX_LOSSES":
			m_maxLosses = (int)val;
			break;
		case "DECK_DISPLAY_RULESET_ID":
			m_deckDisplayRulesetId = (int)val;
			break;
		case "MAX_HEROES_DRAFTED":
			m_maxHeroesDrafted = (int)val;
			break;
		case "REWARD_CHEST_ID":
			m_rewardChestId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"EVENT" => typeof(string), 
			"ADVENTURE_ID" => typeof(int), 
			"SCENARIO_ID" => typeof(int), 
			"MAX_WINS" => typeof(int), 
			"MAX_LOSSES" => typeof(int), 
			"DECK_DISPLAY_RULESET_ID" => typeof(int), 
			"MAX_HEROES_DRAFTED" => typeof(int), 
			"REWARD_CHEST_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadPvpdrSeasonDbfRecords loadRecords = new LoadPvpdrSeasonDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		PvpdrSeasonDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(PvpdrSeasonDbfAsset)) as PvpdrSeasonDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"PvpdrSeasonDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
