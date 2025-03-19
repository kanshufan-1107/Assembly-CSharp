using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class CounterpartCardsDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_primaryCardId;

	[SerializeField]
	private int m_deckEquivalentCardId;

	[DbfField("PRIMARY_CARD_ID")]
	public int PrimaryCardId => m_primaryCardId;

	[DbfField("DECK_EQUIVALENT_CARD_ID")]
	public int DeckEquivalentCardId => m_deckEquivalentCardId;

	public CardDbfRecord DeckEquivalentCardRecord => GameDbf.Card.GetRecord(m_deckEquivalentCardId);

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"PRIMARY_CARD_ID" => m_primaryCardId, 
			"DECK_EQUIVALENT_CARD_ID" => m_deckEquivalentCardId, 
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
		case "PRIMARY_CARD_ID":
			m_primaryCardId = (int)val;
			break;
		case "DECK_EQUIVALENT_CARD_ID":
			m_deckEquivalentCardId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"PRIMARY_CARD_ID" => typeof(int), 
			"DECK_EQUIVALENT_CARD_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadCounterpartCardsDbfRecords loadRecords = new LoadCounterpartCardsDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		CounterpartCardsDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(CounterpartCardsDbfAsset)) as CounterpartCardsDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"CounterpartCardsDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
