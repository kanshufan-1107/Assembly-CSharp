using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class CardDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteMiniGuid;

	[SerializeField]
	private DbfLocValue m_textInHand;

	[SerializeField]
	private EventTimingType m_gameplayEvent = EventTimingType.UNKNOWN;

	[SerializeField]
	private EventTimingType m_craftingEvent = EventTimingType.UNKNOWN;

	[SerializeField]
	private EventTimingType m_goldenCraftingEvent = EventTimingType.UNKNOWN;

	[SerializeField]
	private EventTimingType m_signatureCraftingEvent = EventTimingType.UNKNOWN;

	[SerializeField]
	private EventTimingType m_diamondCraftingEvent = EventTimingType.UNKNOWN;

	[SerializeField]
	private int m_suggestionWeight;

	[SerializeField]
	private int m_changeVersion;

	[SerializeField]
	private DbfLocValue m_name;

	[SerializeField]
	private DbfLocValue m_flavorText;

	[SerializeField]
	private DbfLocValue m_howToGetCard;

	[SerializeField]
	private DbfLocValue m_howToGetGoldCard;

	[SerializeField]
	private DbfLocValue m_howToGetSignatureCard;

	[SerializeField]
	private DbfLocValue m_howToGetDiamondCard;

	[SerializeField]
	private DbfLocValue m_targetArrowText;

	[SerializeField]
	private string m_artistName;

	[SerializeField]
	private string m_signatureArtistName;

	[SerializeField]
	private DbfLocValue m_shortName;

	[SerializeField]
	private string m_creditsCardName;

	[SerializeField]
	private EventTimingType m_featuredCardsEvent = EventTimingType.UNKNOWN;

	[SerializeField]
	private EventTimingType m_battlegroundsActiveEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("always");

	[SerializeField]
	private EventTimingType m_battlegroundsEarlyAccessEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("never");

	[SerializeField]
	private EventTimingType m_battlegroundsEveryGameEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("never");

	[SerializeField]
	private Assets.Card.CardTextBuilderType m_cardTextBuilderType;

	[SerializeField]
	private string m_watermarkTextureOverride;

	[DbfField("NOTE_MINI_GUID")]
	public string NoteMiniGuid => m_noteMiniGuid;

	[DbfField("TEXT_IN_HAND")]
	public DbfLocValue TextInHand => m_textInHand;

	[DbfField("GAMEPLAY_EVENT")]
	public EventTimingType GameplayEvent => m_gameplayEvent;

	[DbfField("CRAFTING_EVENT")]
	public EventTimingType CraftingEvent => m_craftingEvent;

	[DbfField("GOLDEN_CRAFTING_EVENT")]
	public EventTimingType GoldenCraftingEvent => m_goldenCraftingEvent;

	[DbfField("SIGNATURE_CRAFTING_EVENT")]
	public EventTimingType SignatureCraftingEvent => m_signatureCraftingEvent;

	[DbfField("DIAMOND_CRAFTING_EVENT")]
	public EventTimingType DiamondCraftingEvent => m_diamondCraftingEvent;

	[DbfField("SUGGESTION_WEIGHT")]
	public int SuggestionWeight => m_suggestionWeight;

	[DbfField("NAME")]
	public DbfLocValue Name => m_name;

	[DbfField("FLAVOR_TEXT")]
	public DbfLocValue FlavorText => m_flavorText;

	[DbfField("HOW_TO_GET_CARD")]
	public DbfLocValue HowToGetCard => m_howToGetCard;

	[DbfField("HOW_TO_GET_GOLD_CARD")]
	public DbfLocValue HowToGetGoldCard => m_howToGetGoldCard;

	[DbfField("HOW_TO_GET_SIGNATURE_CARD")]
	public DbfLocValue HowToGetSignatureCard => m_howToGetSignatureCard;

	[DbfField("HOW_TO_GET_DIAMOND_CARD")]
	public DbfLocValue HowToGetDiamondCard => m_howToGetDiamondCard;

	[DbfField("TARGET_ARROW_TEXT")]
	public DbfLocValue TargetArrowText => m_targetArrowText;

	[DbfField("ARTIST_NAME")]
	public string ArtistName => m_artistName;

	[DbfField("SIGNATURE_ARTIST_NAME")]
	public string SignatureArtistName => m_signatureArtistName;

	[DbfField("SHORT_NAME")]
	public DbfLocValue ShortName => m_shortName;

	[DbfField("CREDITS_CARD_NAME")]
	public string CreditsCardName => m_creditsCardName;

	[DbfField("FEATURED_CARDS_EVENT")]
	public EventTimingType FeaturedCardsEvent => m_featuredCardsEvent;

	[DbfField("BATTLEGROUNDS_ACTIVE_EVENT")]
	public EventTimingType BattlegroundsActiveEvent => m_battlegroundsActiveEvent;

	[DbfField("BATTLEGROUNDS_EARLY_ACCESS_EVENT")]
	public EventTimingType BattlegroundsEarlyAccessEvent => m_battlegroundsEarlyAccessEvent;

	[DbfField("CARD_TEXT_BUILDER_TYPE")]
	public Assets.Card.CardTextBuilderType CardTextBuilderType => m_cardTextBuilderType;

	[DbfField("WATERMARK_TEXTURE_OVERRIDE")]
	public string WatermarkTextureOverride => m_watermarkTextureOverride;

	public List<CardAdditonalSearchTermsDbfRecord> SearchTerms
	{
		get
		{
			int id = base.ID;
			List<CardAdditonalSearchTermsDbfRecord> returnRecords = new List<CardAdditonalSearchTermsDbfRecord>();
			List<CardAdditonalSearchTermsDbfRecord> records = GameDbf.CardAdditonalSearchTerms.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				CardAdditonalSearchTermsDbfRecord record = records[i];
				if (record.CardId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public List<CardEquipmentAltTextDbfRecord> EquipmentAltText
	{
		get
		{
			int id = base.ID;
			List<CardEquipmentAltTextDbfRecord> returnRecords = new List<CardEquipmentAltTextDbfRecord>();
			List<CardEquipmentAltTextDbfRecord> records = GameDbf.CardEquipmentAltText.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				CardEquipmentAltTextDbfRecord record = records[i];
				if (record.CardId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public List<CounterpartCardsDbfRecord> CounterpartCards
	{
		get
		{
			int id = base.ID;
			List<CounterpartCardsDbfRecord> returnRecords = new List<CounterpartCardsDbfRecord>();
			List<CounterpartCardsDbfRecord> records = GameDbf.CounterpartCards.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				CounterpartCardsDbfRecord record = records[i];
				if (record.PrimaryCardId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public List<RelatedCardsDbfRecord> RelatedCards
	{
		get
		{
			int id = base.ID;
			List<RelatedCardsDbfRecord> returnRecords = new List<RelatedCardsDbfRecord>();
			List<RelatedCardsDbfRecord> records = GameDbf.RelatedCards.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				RelatedCardsDbfRecord record = records[i];
				if (record.CardId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public void SetNoteMiniGuid(string v)
	{
		m_noteMiniGuid = v;
	}

	public void SetTextInHand(DbfLocValue v)
	{
		m_textInHand = v;
		v.SetDebugInfo(base.ID, "TEXT_IN_HAND");
	}

	public void SetName(DbfLocValue v)
	{
		m_name = v;
		v.SetDebugInfo(base.ID, "NAME");
	}

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NOTE_MINI_GUID" => m_noteMiniGuid, 
			"TEXT_IN_HAND" => m_textInHand, 
			"GAMEPLAY_EVENT" => m_gameplayEvent, 
			"CRAFTING_EVENT" => m_craftingEvent, 
			"GOLDEN_CRAFTING_EVENT" => m_goldenCraftingEvent, 
			"SIGNATURE_CRAFTING_EVENT" => m_signatureCraftingEvent, 
			"DIAMOND_CRAFTING_EVENT" => m_diamondCraftingEvent, 
			"SUGGESTION_WEIGHT" => m_suggestionWeight, 
			"CHANGE_VERSION" => m_changeVersion, 
			"NAME" => m_name, 
			"FLAVOR_TEXT" => m_flavorText, 
			"HOW_TO_GET_CARD" => m_howToGetCard, 
			"HOW_TO_GET_GOLD_CARD" => m_howToGetGoldCard, 
			"HOW_TO_GET_SIGNATURE_CARD" => m_howToGetSignatureCard, 
			"HOW_TO_GET_DIAMOND_CARD" => m_howToGetDiamondCard, 
			"TARGET_ARROW_TEXT" => m_targetArrowText, 
			"ARTIST_NAME" => m_artistName, 
			"SIGNATURE_ARTIST_NAME" => m_signatureArtistName, 
			"SHORT_NAME" => m_shortName, 
			"CREDITS_CARD_NAME" => m_creditsCardName, 
			"FEATURED_CARDS_EVENT" => m_featuredCardsEvent, 
			"BATTLEGROUNDS_ACTIVE_EVENT" => m_battlegroundsActiveEvent, 
			"BATTLEGROUNDS_EARLY_ACCESS_EVENT" => m_battlegroundsEarlyAccessEvent, 
			"BATTLEGROUNDS_EVERY_GAME_EVENT" => m_battlegroundsEveryGameEvent, 
			"CARD_TEXT_BUILDER_TYPE" => m_cardTextBuilderType, 
			"WATERMARK_TEXTURE_OVERRIDE" => m_watermarkTextureOverride, 
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
		case "NOTE_MINI_GUID":
			m_noteMiniGuid = (string)val;
			break;
		case "TEXT_IN_HAND":
			m_textInHand = (DbfLocValue)val;
			break;
		case "GAMEPLAY_EVENT":
			m_gameplayEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "CRAFTING_EVENT":
			m_craftingEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "GOLDEN_CRAFTING_EVENT":
			m_goldenCraftingEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "SIGNATURE_CRAFTING_EVENT":
			m_signatureCraftingEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "DIAMOND_CRAFTING_EVENT":
			m_diamondCraftingEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "SUGGESTION_WEIGHT":
			m_suggestionWeight = (int)val;
			break;
		case "CHANGE_VERSION":
			m_changeVersion = (int)val;
			break;
		case "NAME":
			m_name = (DbfLocValue)val;
			break;
		case "FLAVOR_TEXT":
			m_flavorText = (DbfLocValue)val;
			break;
		case "HOW_TO_GET_CARD":
			m_howToGetCard = (DbfLocValue)val;
			break;
		case "HOW_TO_GET_GOLD_CARD":
			m_howToGetGoldCard = (DbfLocValue)val;
			break;
		case "HOW_TO_GET_SIGNATURE_CARD":
			m_howToGetSignatureCard = (DbfLocValue)val;
			break;
		case "HOW_TO_GET_DIAMOND_CARD":
			m_howToGetDiamondCard = (DbfLocValue)val;
			break;
		case "TARGET_ARROW_TEXT":
			m_targetArrowText = (DbfLocValue)val;
			break;
		case "ARTIST_NAME":
			m_artistName = (string)val;
			break;
		case "SIGNATURE_ARTIST_NAME":
			m_signatureArtistName = (string)val;
			break;
		case "SHORT_NAME":
			m_shortName = (DbfLocValue)val;
			break;
		case "CREDITS_CARD_NAME":
			m_creditsCardName = (string)val;
			break;
		case "FEATURED_CARDS_EVENT":
			m_featuredCardsEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "BATTLEGROUNDS_ACTIVE_EVENT":
			m_battlegroundsActiveEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "BATTLEGROUNDS_EARLY_ACCESS_EVENT":
			m_battlegroundsEarlyAccessEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "BATTLEGROUNDS_EVERY_GAME_EVENT":
			m_battlegroundsEveryGameEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "CARD_TEXT_BUILDER_TYPE":
			if (val == null)
			{
				m_cardTextBuilderType = Assets.Card.CardTextBuilderType.DEFAULT;
			}
			else if (val is Assets.Card.CardTextBuilderType || val is int)
			{
				m_cardTextBuilderType = (Assets.Card.CardTextBuilderType)val;
			}
			else if (val is string)
			{
				m_cardTextBuilderType = Assets.Card.ParseCardTextBuilderTypeValue((string)val);
			}
			break;
		case "WATERMARK_TEXTURE_OVERRIDE":
			m_watermarkTextureOverride = (string)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NOTE_MINI_GUID" => typeof(string), 
			"TEXT_IN_HAND" => typeof(DbfLocValue), 
			"GAMEPLAY_EVENT" => typeof(string), 
			"CRAFTING_EVENT" => typeof(string), 
			"GOLDEN_CRAFTING_EVENT" => typeof(string), 
			"SIGNATURE_CRAFTING_EVENT" => typeof(string), 
			"DIAMOND_CRAFTING_EVENT" => typeof(string), 
			"SUGGESTION_WEIGHT" => typeof(int), 
			"CHANGE_VERSION" => typeof(int), 
			"NAME" => typeof(DbfLocValue), 
			"FLAVOR_TEXT" => typeof(DbfLocValue), 
			"HOW_TO_GET_CARD" => typeof(DbfLocValue), 
			"HOW_TO_GET_GOLD_CARD" => typeof(DbfLocValue), 
			"HOW_TO_GET_SIGNATURE_CARD" => typeof(DbfLocValue), 
			"HOW_TO_GET_DIAMOND_CARD" => typeof(DbfLocValue), 
			"TARGET_ARROW_TEXT" => typeof(DbfLocValue), 
			"ARTIST_NAME" => typeof(string), 
			"SIGNATURE_ARTIST_NAME" => typeof(string), 
			"SHORT_NAME" => typeof(DbfLocValue), 
			"CREDITS_CARD_NAME" => typeof(string), 
			"FEATURED_CARDS_EVENT" => typeof(string), 
			"BATTLEGROUNDS_ACTIVE_EVENT" => typeof(string), 
			"BATTLEGROUNDS_EARLY_ACCESS_EVENT" => typeof(string), 
			"BATTLEGROUNDS_EVERY_GAME_EVENT" => typeof(string), 
			"CARD_TEXT_BUILDER_TYPE" => typeof(Assets.Card.CardTextBuilderType), 
			"WATERMARK_TEXTURE_OVERRIDE" => typeof(string), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadCardDbfRecords loadRecords = new LoadCardDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		CardDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(CardDbfAsset)) as CardDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"CardDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_textInHand.StripUnusedLocales();
		m_name.StripUnusedLocales();
		m_flavorText.StripUnusedLocales();
		m_howToGetCard.StripUnusedLocales();
		m_howToGetGoldCard.StripUnusedLocales();
		m_howToGetSignatureCard.StripUnusedLocales();
		m_howToGetDiamondCard.StripUnusedLocales();
		m_targetArrowText.StripUnusedLocales();
		m_shortName.StripUnusedLocales();
	}
}
