using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class BoosterDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private int m_cardSetId;

	[SerializeField]
	private int m_latestExpansionOrder;

	[SerializeField]
	private int m_listDisplayOrder;

	[SerializeField]
	private int m_listDisplayOrderCategory;

	[SerializeField]
	private EventTimingType m_openPackEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("none");

	[SerializeField]
	private EventTimingType m_prereleaseOpenPackEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("never");

	[SerializeField]
	private EventTimingType m_buyWithGoldEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("never");

	[SerializeField]
	private EventTimingType m_rewardableEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("none");

	[SerializeField]
	private EventTimingType m_shownToClientEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("none");

	[SerializeField]
	private DbfLocValue m_name;

	[SerializeField]
	private DbfLocValue m_shortName;

	[SerializeField]
	private DbfLocValue m_descriptionOverride;

	[SerializeField]
	private string m_packOpeningPrefab;

	[SerializeField]
	private string m_packOpeningFxPrefab;

	[SerializeField]
	private string m_storePrefab;

	[SerializeField]
	private string m_arenaPrefab;

	[SerializeField]
	private bool m_leavingSoon;

	[SerializeField]
	private DbfLocValue m_leavingSoonText;

	[SerializeField]
	private EventTimingType m_standardEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("always");

	[SerializeField]
	private bool m_showInStore;

	[SerializeField]
	private int m_rankedRewardInitialSeason;

	[SerializeField]
	private int m_premium = -1;

	[SerializeField]
	private bool m_isCatchupPack;

	[SerializeField]
	private string m_questIconPath;

	[SerializeField]
	private double m_questIconOffsetX;

	[SerializeField]
	private double m_questIconOffsetY;

	[DbfField("CARD_SET_ID")]
	public int CardSetId => m_cardSetId;

	public CardSetDbfRecord CardSetRecord => GameDbf.CardSet.GetRecord(m_cardSetId);

	[DbfField("LATEST_EXPANSION_ORDER")]
	public int LatestExpansionOrder => m_latestExpansionOrder;

	[DbfField("LIST_DISPLAY_ORDER")]
	public int ListDisplayOrder => m_listDisplayOrder;

	[DbfField("LIST_DISPLAY_ORDER_CATEGORY")]
	public int ListDisplayOrderCategory => m_listDisplayOrderCategory;

	[DbfField("OPEN_PACK_EVENT")]
	public EventTimingType OpenPackEvent => m_openPackEvent;

	[DbfField("BUY_WITH_GOLD_EVENT")]
	public EventTimingType BuyWithGoldEvent => m_buyWithGoldEvent;

	[DbfField("REWARDABLE_EVENT")]
	public EventTimingType RewardableEvent => m_rewardableEvent;

	[DbfField("SHOWN_TO_CLIENT_EVENT")]
	public EventTimingType ShownToClientEvent => m_shownToClientEvent;

	[DbfField("NAME")]
	public DbfLocValue Name => m_name;

	[DbfField("SHORT_NAME")]
	public DbfLocValue ShortName => m_shortName;

	[DbfField("DESCRIPTION_OVERRIDE")]
	public DbfLocValue DescriptionOverride => m_descriptionOverride;

	[DbfField("PACK_OPENING_PREFAB")]
	public string PackOpeningPrefab => m_packOpeningPrefab;

	[DbfField("PACK_OPENING_FX_PREFAB")]
	public string PackOpeningFxPrefab => m_packOpeningFxPrefab;

	[DbfField("STORE_PREFAB")]
	public string StorePrefab => m_storePrefab;

	[DbfField("ARENA_PREFAB")]
	public string ArenaPrefab => m_arenaPrefab;

	[DbfField("STANDARD_EVENT")]
	public EventTimingType StandardEvent => m_standardEvent;

	[DbfField("RANKED_REWARD_INITIAL_SEASON")]
	public int RankedRewardInitialSeason => m_rankedRewardInitialSeason;

	[DbfField("PREMIUM")]
	public int Premium => m_premium;

	[DbfField("IS_CATCHUP_PACK")]
	public bool IsCatchupPack => m_isCatchupPack;

	[DbfField("QUEST_ICON_PATH")]
	public string QuestIconPath => m_questIconPath;

	[DbfField("QUEST_ICON_OFFSET_X")]
	public double QuestIconOffsetX => m_questIconOffsetX;

	[DbfField("QUEST_ICON_OFFSET_Y")]
	public double QuestIconOffsetY => m_questIconOffsetY;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NOTE_DESC" => m_noteDesc, 
			"CARD_SET_ID" => m_cardSetId, 
			"LATEST_EXPANSION_ORDER" => m_latestExpansionOrder, 
			"LIST_DISPLAY_ORDER" => m_listDisplayOrder, 
			"LIST_DISPLAY_ORDER_CATEGORY" => m_listDisplayOrderCategory, 
			"OPEN_PACK_EVENT" => m_openPackEvent, 
			"PRERELEASE_OPEN_PACK_EVENT" => m_prereleaseOpenPackEvent, 
			"BUY_WITH_GOLD_EVENT" => m_buyWithGoldEvent, 
			"REWARDABLE_EVENT" => m_rewardableEvent, 
			"SHOWN_TO_CLIENT_EVENT" => m_shownToClientEvent, 
			"NAME" => m_name, 
			"SHORT_NAME" => m_shortName, 
			"DESCRIPTION_OVERRIDE" => m_descriptionOverride, 
			"PACK_OPENING_PREFAB" => m_packOpeningPrefab, 
			"PACK_OPENING_FX_PREFAB" => m_packOpeningFxPrefab, 
			"STORE_PREFAB" => m_storePrefab, 
			"ARENA_PREFAB" => m_arenaPrefab, 
			"LEAVING_SOON" => m_leavingSoon, 
			"LEAVING_SOON_TEXT" => m_leavingSoonText, 
			"STANDARD_EVENT" => m_standardEvent, 
			"SHOW_IN_STORE" => m_showInStore, 
			"RANKED_REWARD_INITIAL_SEASON" => m_rankedRewardInitialSeason, 
			"PREMIUM" => m_premium, 
			"IS_CATCHUP_PACK" => m_isCatchupPack, 
			"QUEST_ICON_PATH" => m_questIconPath, 
			"QUEST_ICON_OFFSET_X" => m_questIconOffsetX, 
			"QUEST_ICON_OFFSET_Y" => m_questIconOffsetY, 
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
		case "CARD_SET_ID":
			m_cardSetId = (int)val;
			break;
		case "LATEST_EXPANSION_ORDER":
			m_latestExpansionOrder = (int)val;
			break;
		case "LIST_DISPLAY_ORDER":
			m_listDisplayOrder = (int)val;
			break;
		case "LIST_DISPLAY_ORDER_CATEGORY":
			m_listDisplayOrderCategory = (int)val;
			break;
		case "OPEN_PACK_EVENT":
			m_openPackEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "PRERELEASE_OPEN_PACK_EVENT":
			m_prereleaseOpenPackEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "BUY_WITH_GOLD_EVENT":
			m_buyWithGoldEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "REWARDABLE_EVENT":
			m_rewardableEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "SHOWN_TO_CLIENT_EVENT":
			m_shownToClientEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "NAME":
			m_name = (DbfLocValue)val;
			break;
		case "SHORT_NAME":
			m_shortName = (DbfLocValue)val;
			break;
		case "DESCRIPTION_OVERRIDE":
			m_descriptionOverride = (DbfLocValue)val;
			break;
		case "PACK_OPENING_PREFAB":
			m_packOpeningPrefab = (string)val;
			break;
		case "PACK_OPENING_FX_PREFAB":
			m_packOpeningFxPrefab = (string)val;
			break;
		case "STORE_PREFAB":
			m_storePrefab = (string)val;
			break;
		case "ARENA_PREFAB":
			m_arenaPrefab = (string)val;
			break;
		case "LEAVING_SOON":
			m_leavingSoon = (bool)val;
			break;
		case "LEAVING_SOON_TEXT":
			m_leavingSoonText = (DbfLocValue)val;
			break;
		case "STANDARD_EVENT":
			m_standardEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "SHOW_IN_STORE":
			m_showInStore = (bool)val;
			break;
		case "RANKED_REWARD_INITIAL_SEASON":
			m_rankedRewardInitialSeason = (int)val;
			break;
		case "PREMIUM":
			m_premium = (int)val;
			break;
		case "IS_CATCHUP_PACK":
			m_isCatchupPack = (bool)val;
			break;
		case "QUEST_ICON_PATH":
			m_questIconPath = (string)val;
			break;
		case "QUEST_ICON_OFFSET_X":
			m_questIconOffsetX = (double)val;
			break;
		case "QUEST_ICON_OFFSET_Y":
			m_questIconOffsetY = (double)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"CARD_SET_ID" => typeof(int), 
			"LATEST_EXPANSION_ORDER" => typeof(int), 
			"LIST_DISPLAY_ORDER" => typeof(int), 
			"LIST_DISPLAY_ORDER_CATEGORY" => typeof(int), 
			"OPEN_PACK_EVENT" => typeof(string), 
			"PRERELEASE_OPEN_PACK_EVENT" => typeof(string), 
			"BUY_WITH_GOLD_EVENT" => typeof(string), 
			"REWARDABLE_EVENT" => typeof(string), 
			"SHOWN_TO_CLIENT_EVENT" => typeof(string), 
			"NAME" => typeof(DbfLocValue), 
			"SHORT_NAME" => typeof(DbfLocValue), 
			"DESCRIPTION_OVERRIDE" => typeof(DbfLocValue), 
			"PACK_OPENING_PREFAB" => typeof(string), 
			"PACK_OPENING_FX_PREFAB" => typeof(string), 
			"STORE_PREFAB" => typeof(string), 
			"ARENA_PREFAB" => typeof(string), 
			"LEAVING_SOON" => typeof(bool), 
			"LEAVING_SOON_TEXT" => typeof(DbfLocValue), 
			"STANDARD_EVENT" => typeof(string), 
			"SHOW_IN_STORE" => typeof(bool), 
			"RANKED_REWARD_INITIAL_SEASON" => typeof(int), 
			"PREMIUM" => typeof(int), 
			"IS_CATCHUP_PACK" => typeof(bool), 
			"QUEST_ICON_PATH" => typeof(string), 
			"QUEST_ICON_OFFSET_X" => typeof(double), 
			"QUEST_ICON_OFFSET_Y" => typeof(double), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadBoosterDbfRecords loadRecords = new LoadBoosterDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		BoosterDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(BoosterDbfAsset)) as BoosterDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"BoosterDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_name.StripUnusedLocales();
		m_shortName.StripUnusedLocales();
		m_descriptionOverride.StripUnusedLocales();
		m_leavingSoonText.StripUnusedLocales();
	}
}
