using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class DeckCardDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_nextCardId;

	[SerializeField]
	private int m_cardId;

	[SerializeField]
	private int m_deckId;

	[SerializeField]
	private DbfLocValue m_description;

	[DbfField("NEXT_CARD")]
	public int NextCard => m_nextCardId;

	[DbfField("CARD_ID")]
	public int CardId => m_cardId;

	public CardDbfRecord CardRecord => GameDbf.Card.GetRecord(m_cardId);

	[DbfField("DECK_ID")]
	public int DeckId => m_deckId;

	public List<SideboardCardDbfRecord> SideboardCards
	{
		get
		{
			int id = base.ID;
			List<SideboardCardDbfRecord> returnRecords = new List<SideboardCardDbfRecord>();
			List<SideboardCardDbfRecord> records = GameDbf.SideboardCard.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				SideboardCardDbfRecord record = records[i];
				if (record.DeckCardId == id)
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
			"NEXT_CARD" => m_nextCardId, 
			"CARD_ID" => m_cardId, 
			"DECK_ID" => m_deckId, 
			"DESCRIPTION" => m_description, 
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
		case "NEXT_CARD":
			m_nextCardId = (int)val;
			break;
		case "CARD_ID":
			m_cardId = (int)val;
			break;
		case "DECK_ID":
			m_deckId = (int)val;
			break;
		case "DESCRIPTION":
			m_description = (DbfLocValue)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NEXT_CARD" => typeof(int), 
			"CARD_ID" => typeof(int), 
			"DECK_ID" => typeof(int), 
			"DESCRIPTION" => typeof(DbfLocValue), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadDeckCardDbfRecords loadRecords = new LoadDeckCardDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		DeckCardDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(DeckCardDbfAsset)) as DeckCardDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"DeckCardDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_description.StripUnusedLocales();
	}
}
