using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class MiniSetDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_deckId;

	[SerializeField]
	private int m_boosterId;

	[SerializeField]
	private DbfLocValue m_goldenName;

	[SerializeField]
	private bool m_hideOnClient;

	public DeckDbfRecord DeckRecord => GameDbf.Deck.GetRecord(m_deckId);

	[DbfField("BOOSTER_ID")]
	public int BoosterId => m_boosterId;

	public BoosterDbfRecord BoosterRecord => GameDbf.Booster.GetRecord(m_boosterId);

	[DbfField("GOLDEN_NAME")]
	public DbfLocValue GoldenName => m_goldenName;

	[DbfField("HIDE_ON_CLIENT")]
	public bool HideOnClient => m_hideOnClient;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"DECK_ID" => m_deckId, 
			"BOOSTER_ID" => m_boosterId, 
			"GOLDEN_NAME" => m_goldenName, 
			"HIDE_ON_CLIENT" => m_hideOnClient, 
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
		case "DECK_ID":
			m_deckId = (int)val;
			break;
		case "BOOSTER_ID":
			m_boosterId = (int)val;
			break;
		case "GOLDEN_NAME":
			m_goldenName = (DbfLocValue)val;
			break;
		case "HIDE_ON_CLIENT":
			m_hideOnClient = (bool)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"DECK_ID" => typeof(int), 
			"BOOSTER_ID" => typeof(int), 
			"GOLDEN_NAME" => typeof(DbfLocValue), 
			"HIDE_ON_CLIENT" => typeof(bool), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadMiniSetDbfRecords loadRecords = new LoadMiniSetDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		MiniSetDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(MiniSetDbfAsset)) as MiniSetDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"MiniSetDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_goldenName.StripUnusedLocales();
	}
}
