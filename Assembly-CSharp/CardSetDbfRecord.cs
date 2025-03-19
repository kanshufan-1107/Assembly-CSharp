using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class CardSetDbfRecord : DbfRecord
{
	[SerializeField]
	private bool m_isCollectible = true;

	[SerializeField]
	private bool m_isCoreCardSet;

	[SerializeField]
	private EventTimingType m_legacyCardSetEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("never");

	[SerializeField]
	private EventTimingType m_contentLaunchEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("always");

	[SerializeField]
	private bool m_isFeaturedCardSet;

	[SerializeField]
	private EventTimingType m_standardEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("always");

	[SerializeField]
	private bool m_craftableWhenWild;

	[SerializeField]
	private string m_cardWatermarkTexture;

	[SerializeField]
	private EventTimingType m_setFilterEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("always");

	[SerializeField]
	private string m_filterIconTexture;

	[SerializeField]
	private double m_filterIconOffsetX;

	[SerializeField]
	private double m_filterIconOffsetY;

	[SerializeField]
	private int m_releaseOrder;

	[SerializeField]
	private bool m_isExpansionCardSet;

	[DbfField("IS_COLLECTIBLE")]
	public bool IsCollectible => m_isCollectible;

	[DbfField("IS_CORE_CARD_SET")]
	public bool IsCoreCardSet => m_isCoreCardSet;

	[DbfField("LEGACY_CARD_SET_EVENT")]
	public EventTimingType LegacyCardSetEvent => m_legacyCardSetEvent;

	[DbfField("CONTENT_LAUNCH_EVENT")]
	public EventTimingType ContentLaunchEvent => m_contentLaunchEvent;

	[DbfField("IS_FEATURED_CARD_SET")]
	public bool IsFeaturedCardSet => m_isFeaturedCardSet;

	[DbfField("STANDARD_EVENT")]
	public EventTimingType StandardEvent => m_standardEvent;

	[DbfField("CRAFTABLE_WHEN_WILD")]
	public bool CraftableWhenWild => m_craftableWhenWild;

	[DbfField("CARD_WATERMARK_TEXTURE")]
	public string CardWatermarkTexture => m_cardWatermarkTexture;

	[DbfField("SET_FILTER_EVENT")]
	public EventTimingType SetFilterEvent => m_setFilterEvent;

	[DbfField("FILTER_ICON_TEXTURE")]
	public string FilterIconTexture => m_filterIconTexture;

	[DbfField("FILTER_ICON_OFFSET_X")]
	public double FilterIconOffsetX => m_filterIconOffsetX;

	[DbfField("FILTER_ICON_OFFSET_Y")]
	public double FilterIconOffsetY => m_filterIconOffsetY;

	[DbfField("RELEASE_ORDER")]
	public int ReleaseOrder => m_releaseOrder;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"IS_COLLECTIBLE" => m_isCollectible, 
			"IS_CORE_CARD_SET" => m_isCoreCardSet, 
			"LEGACY_CARD_SET_EVENT" => m_legacyCardSetEvent, 
			"CONTENT_LAUNCH_EVENT" => m_contentLaunchEvent, 
			"IS_FEATURED_CARD_SET" => m_isFeaturedCardSet, 
			"STANDARD_EVENT" => m_standardEvent, 
			"CRAFTABLE_WHEN_WILD" => m_craftableWhenWild, 
			"CARD_WATERMARK_TEXTURE" => m_cardWatermarkTexture, 
			"SET_FILTER_EVENT" => m_setFilterEvent, 
			"FILTER_ICON_TEXTURE" => m_filterIconTexture, 
			"FILTER_ICON_OFFSET_X" => m_filterIconOffsetX, 
			"FILTER_ICON_OFFSET_Y" => m_filterIconOffsetY, 
			"RELEASE_ORDER" => m_releaseOrder, 
			"IS_EXPANSION_CARD_SET" => m_isExpansionCardSet, 
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
		case "IS_COLLECTIBLE":
			m_isCollectible = (bool)val;
			break;
		case "IS_CORE_CARD_SET":
			m_isCoreCardSet = (bool)val;
			break;
		case "LEGACY_CARD_SET_EVENT":
			m_legacyCardSetEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "CONTENT_LAUNCH_EVENT":
			m_contentLaunchEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "IS_FEATURED_CARD_SET":
			m_isFeaturedCardSet = (bool)val;
			break;
		case "STANDARD_EVENT":
			m_standardEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "CRAFTABLE_WHEN_WILD":
			m_craftableWhenWild = (bool)val;
			break;
		case "CARD_WATERMARK_TEXTURE":
			m_cardWatermarkTexture = (string)val;
			break;
		case "SET_FILTER_EVENT":
			m_setFilterEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "FILTER_ICON_TEXTURE":
			m_filterIconTexture = (string)val;
			break;
		case "FILTER_ICON_OFFSET_X":
			m_filterIconOffsetX = (double)val;
			break;
		case "FILTER_ICON_OFFSET_Y":
			m_filterIconOffsetY = (double)val;
			break;
		case "RELEASE_ORDER":
			m_releaseOrder = (int)val;
			break;
		case "IS_EXPANSION_CARD_SET":
			m_isExpansionCardSet = (bool)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"IS_COLLECTIBLE" => typeof(bool), 
			"IS_CORE_CARD_SET" => typeof(bool), 
			"LEGACY_CARD_SET_EVENT" => typeof(string), 
			"CONTENT_LAUNCH_EVENT" => typeof(string), 
			"IS_FEATURED_CARD_SET" => typeof(bool), 
			"STANDARD_EVENT" => typeof(string), 
			"CRAFTABLE_WHEN_WILD" => typeof(bool), 
			"CARD_WATERMARK_TEXTURE" => typeof(string), 
			"SET_FILTER_EVENT" => typeof(string), 
			"FILTER_ICON_TEXTURE" => typeof(string), 
			"FILTER_ICON_OFFSET_X" => typeof(double), 
			"FILTER_ICON_OFFSET_Y" => typeof(double), 
			"RELEASE_ORDER" => typeof(int), 
			"IS_EXPANSION_CARD_SET" => typeof(bool), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadCardSetDbfRecords loadRecords = new LoadCardSetDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		CardSetDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(CardSetDbfAsset)) as CardSetDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"CardSetDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
