using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class DeckTemplateChoicesDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_scenarioId;

	[SerializeField]
	private int m_deckTemplateId;

	[DbfField("SCENARIO_ID")]
	public int ScenarioId => m_scenarioId;

	public DeckTemplateDbfRecord DeckTemplateRecord => GameDbf.DeckTemplate.GetRecord(m_deckTemplateId);

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"SCENARIO_ID" => m_scenarioId, 
			"DECK_TEMPLATE_ID" => m_deckTemplateId, 
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
		case "DECK_TEMPLATE_ID":
			m_deckTemplateId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"SCENARIO_ID" => typeof(int), 
			"DECK_TEMPLATE_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadDeckTemplateChoicesDbfRecords loadRecords = new LoadDeckTemplateChoicesDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		DeckTemplateChoicesDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(DeckTemplateChoicesDbfAsset)) as DeckTemplateChoicesDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"DeckTemplateChoicesDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
