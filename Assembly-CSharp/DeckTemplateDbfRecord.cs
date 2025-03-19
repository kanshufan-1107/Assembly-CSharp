using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class DeckTemplateDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_classId;

	[SerializeField]
	private int m_heroCardId;

	[SerializeField]
	private EventTimingType m_event = EventTimingType.UNKNOWN;

	[SerializeField]
	private int m_sortOrder;

	[SerializeField]
	private int m_deckId;

	[SerializeField]
	private string m_displayTexture;

	[SerializeField]
	private bool m_isFreeReward;

	[SerializeField]
	private bool m_isStarterDeck;

	[SerializeField]
	private DeckTemplate.FormatType m_formatType = DeckTemplate.FormatType.FT_STANDARD;

	[SerializeField]
	private DeckTemplate.SourceType m_sourceType;

	[SerializeField]
	private int m_displayCardId;

	[DbfField("CLASS_ID")]
	public int ClassId => m_classId;

	[DbfField("HERO_CARD_ID")]
	public int HeroCardId => m_heroCardId;

	[DbfField("EVENT")]
	public EventTimingType Event => m_event;

	[DbfField("SORT_ORDER")]
	public int SortOrder => m_sortOrder;

	[DbfField("DECK_ID")]
	public int DeckId => m_deckId;

	public DeckDbfRecord DeckRecord => GameDbf.Deck.GetRecord(m_deckId);

	[DbfField("DISPLAY_TEXTURE")]
	public string DisplayTexture => m_displayTexture;

	[DbfField("IS_FREE_REWARD")]
	public bool IsFreeReward => m_isFreeReward;

	[DbfField("IS_STARTER_DECK")]
	public bool IsStarterDeck => m_isStarterDeck;

	[DbfField("FORMAT_TYPE")]
	public DeckTemplate.FormatType FormatType => m_formatType;

	[DbfField("DISPLAY_CARD_ID")]
	public int DisplayCardId => m_displayCardId;

	public List<DkRuneListDbfRecord> DKRunes
	{
		get
		{
			int id = base.ID;
			List<DkRuneListDbfRecord> returnRecords = new List<DkRuneListDbfRecord>();
			List<DkRuneListDbfRecord> records = GameDbf.DkRuneList.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				DkRuneListDbfRecord record = records[i];
				if (record.DeckTemplateId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public List<OwnershipReqListDbfRecord> OwnershipReqList
	{
		get
		{
			int id = base.ID;
			List<OwnershipReqListDbfRecord> returnRecords = new List<OwnershipReqListDbfRecord>();
			List<OwnershipReqListDbfRecord> records = GameDbf.OwnershipReqList.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				OwnershipReqListDbfRecord record = records[i];
				if (record.DeckTemplateId == id)
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
			"CLASS_ID" => m_classId, 
			"HERO_CARD_ID" => m_heroCardId, 
			"EVENT" => m_event, 
			"SORT_ORDER" => m_sortOrder, 
			"DECK_ID" => m_deckId, 
			"DISPLAY_TEXTURE" => m_displayTexture, 
			"IS_FREE_REWARD" => m_isFreeReward, 
			"IS_STARTER_DECK" => m_isStarterDeck, 
			"FORMAT_TYPE" => m_formatType, 
			"SOURCE_TYPE" => m_sourceType, 
			"DISPLAY_CARD_ID" => m_displayCardId, 
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
		case "CLASS_ID":
			m_classId = (int)val;
			break;
		case "HERO_CARD_ID":
			m_heroCardId = (int)val;
			break;
		case "EVENT":
			m_event = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "SORT_ORDER":
			m_sortOrder = (int)val;
			break;
		case "DECK_ID":
			m_deckId = (int)val;
			break;
		case "DISPLAY_TEXTURE":
			m_displayTexture = (string)val;
			break;
		case "IS_FREE_REWARD":
			m_isFreeReward = (bool)val;
			break;
		case "IS_STARTER_DECK":
			m_isStarterDeck = (bool)val;
			break;
		case "FORMAT_TYPE":
			if (val == null)
			{
				m_formatType = DeckTemplate.FormatType.FT_UNKNOWN;
			}
			else if (val is DeckTemplate.FormatType || val is int)
			{
				m_formatType = (DeckTemplate.FormatType)val;
			}
			else if (val is string)
			{
				m_formatType = DeckTemplate.ParseFormatTypeValue((string)val);
			}
			break;
		case "SOURCE_TYPE":
			if (val == null)
			{
				m_sourceType = DeckTemplate.SourceType.NONE;
			}
			else if (val is DeckTemplate.SourceType || val is int)
			{
				m_sourceType = (DeckTemplate.SourceType)val;
			}
			else if (val is string)
			{
				m_sourceType = DeckTemplate.ParseSourceTypeValue((string)val);
			}
			break;
		case "DISPLAY_CARD_ID":
			m_displayCardId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"CLASS_ID" => typeof(int), 
			"HERO_CARD_ID" => typeof(int), 
			"EVENT" => typeof(string), 
			"SORT_ORDER" => typeof(int), 
			"DECK_ID" => typeof(int), 
			"DISPLAY_TEXTURE" => typeof(string), 
			"IS_FREE_REWARD" => typeof(bool), 
			"IS_STARTER_DECK" => typeof(bool), 
			"FORMAT_TYPE" => typeof(DeckTemplate.FormatType), 
			"SOURCE_TYPE" => typeof(DeckTemplate.SourceType), 
			"DISPLAY_CARD_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadDeckTemplateDbfRecords loadRecords = new LoadDeckTemplateDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		DeckTemplateDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(DeckTemplateDbfAsset)) as DeckTemplateDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"DeckTemplateDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
