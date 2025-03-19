using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class BoxProductBannerDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_campaignName;

	[SerializeField]
	private EventTimingType m_eventTiming = EventTimingType.UNKNOWN;

	[SerializeField]
	private string m_texture;

	[SerializeField]
	private string m_secondaryTexture;

	[SerializeField]
	private DbfLocValue m_text;

	[SerializeField]
	private DbfLocValue m_buttonText;

	[SerializeField]
	private string m_bannerColorHex;

	[SerializeField]
	private long m_pmtProductId;

	[SerializeField]
	private Assets.BoxProductBanner.ProductActionType m_productActionType;

	[DbfField("CAMPAIGN_NAME")]
	public string CampaignName => m_campaignName;

	[DbfField("EVENT_TIMING")]
	public EventTimingType EventTiming => m_eventTiming;

	[DbfField("TEXTURE")]
	public string Texture => m_texture;

	[DbfField("SECONDARY_TEXTURE")]
	public string SecondaryTexture => m_secondaryTexture;

	[DbfField("TEXT")]
	public DbfLocValue Text => m_text;

	[DbfField("BUTTON_TEXT")]
	public DbfLocValue ButtonText => m_buttonText;

	[DbfField("BANNER_COLOR_HEX")]
	public string BannerColorHex => m_bannerColorHex;

	[DbfField("PMT_PRODUCT_ID")]
	public long PmtProductId => m_pmtProductId;

	[DbfField("PRODUCT_ACTION_TYPE")]
	public Assets.BoxProductBanner.ProductActionType ProductActionType => m_productActionType;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"CAMPAIGN_NAME" => m_campaignName, 
			"EVENT_TIMING" => m_eventTiming, 
			"TEXTURE" => m_texture, 
			"SECONDARY_TEXTURE" => m_secondaryTexture, 
			"TEXT" => m_text, 
			"BUTTON_TEXT" => m_buttonText, 
			"BANNER_COLOR_HEX" => m_bannerColorHex, 
			"PMT_PRODUCT_ID" => m_pmtProductId, 
			"PRODUCT_ACTION_TYPE" => m_productActionType, 
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
		case "CAMPAIGN_NAME":
			m_campaignName = (string)val;
			break;
		case "EVENT_TIMING":
			m_eventTiming = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "TEXTURE":
			m_texture = (string)val;
			break;
		case "SECONDARY_TEXTURE":
			m_secondaryTexture = (string)val;
			break;
		case "TEXT":
			m_text = (DbfLocValue)val;
			break;
		case "BUTTON_TEXT":
			m_buttonText = (DbfLocValue)val;
			break;
		case "BANNER_COLOR_HEX":
			m_bannerColorHex = (string)val;
			break;
		case "PMT_PRODUCT_ID":
			m_pmtProductId = (long)val;
			break;
		case "PRODUCT_ACTION_TYPE":
			if (val == null)
			{
				m_productActionType = Assets.BoxProductBanner.ProductActionType.OPEN_SHOP;
			}
			else if (val is Assets.BoxProductBanner.ProductActionType || val is int)
			{
				m_productActionType = (Assets.BoxProductBanner.ProductActionType)val;
			}
			else if (val is string)
			{
				m_productActionType = Assets.BoxProductBanner.ParseProductActionTypeValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"CAMPAIGN_NAME" => typeof(string), 
			"EVENT_TIMING" => typeof(string), 
			"TEXTURE" => typeof(string), 
			"SECONDARY_TEXTURE" => typeof(string), 
			"TEXT" => typeof(DbfLocValue), 
			"BUTTON_TEXT" => typeof(DbfLocValue), 
			"BANNER_COLOR_HEX" => typeof(string), 
			"PMT_PRODUCT_ID" => typeof(long), 
			"PRODUCT_ACTION_TYPE" => typeof(Assets.BoxProductBanner.ProductActionType), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadBoxProductBannerDbfRecords loadRecords = new LoadBoxProductBannerDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		BoxProductBannerDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(BoxProductBannerDbfAsset)) as BoxProductBannerDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"BoxProductBannerDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_text.StripUnusedLocales();
		m_buttonText.StripUnusedLocales();
	}
}
