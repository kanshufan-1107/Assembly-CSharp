using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class CardValueDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_assetCardId;

	[SerializeField]
	private int m_premium;

	[SerializeField]
	private int m_buy;

	[SerializeField]
	private int m_sell;

	[SerializeField]
	private CardValue.SellState m_sellState;

	[SerializeField]
	private EventTimingType m_overrideEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent("never");

	[SerializeField]
	private CardValue.OverrideRegion m_overrideRegion;

	[DbfField("ASSET_CARD_ID")]
	public int AssetCardId => m_assetCardId;

	[DbfField("PREMIUM")]
	public int Premium => m_premium;

	[DbfField("BUY")]
	public int Buy => m_buy;

	[DbfField("SELL")]
	public int Sell => m_sell;

	[DbfField("SELL_STATE")]
	public CardValue.SellState SellState => m_sellState;

	[DbfField("OVERRIDE_EVENT")]
	public EventTimingType OverrideEvent => m_overrideEvent;

	[DbfField("OVERRIDE_REGION")]
	public CardValue.OverrideRegion OverrideRegion => m_overrideRegion;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"ASSET_CARD_ID" => m_assetCardId, 
			"PREMIUM" => m_premium, 
			"BUY" => m_buy, 
			"SELL" => m_sell, 
			"SELL_STATE" => m_sellState, 
			"OVERRIDE_EVENT" => m_overrideEvent, 
			"OVERRIDE_REGION" => m_overrideRegion, 
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
		case "ASSET_CARD_ID":
			m_assetCardId = (int)val;
			break;
		case "PREMIUM":
			m_premium = (int)val;
			break;
		case "BUY":
			m_buy = (int)val;
			break;
		case "SELL":
			m_sell = (int)val;
			break;
		case "SELL_STATE":
			if (val == null)
			{
				m_sellState = CardValue.SellState.NORMAL;
			}
			else if (val is CardValue.SellState || val is int)
			{
				m_sellState = (CardValue.SellState)val;
			}
			else if (val is string)
			{
				m_sellState = CardValue.ParseSellStateValue((string)val);
			}
			break;
		case "OVERRIDE_EVENT":
			m_overrideEvent = DbfShared.GetEventMap().ConvertStringToSpecialEvent((string)val);
			break;
		case "OVERRIDE_REGION":
			if (val == null)
			{
				m_overrideRegion = CardValue.OverrideRegion.NONE;
			}
			else if (val is CardValue.OverrideRegion || val is int)
			{
				m_overrideRegion = (CardValue.OverrideRegion)val;
			}
			else if (val is string)
			{
				m_overrideRegion = CardValue.ParseOverrideRegionValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"ASSET_CARD_ID" => typeof(int), 
			"PREMIUM" => typeof(int), 
			"BUY" => typeof(int), 
			"SELL" => typeof(int), 
			"SELL_STATE" => typeof(CardValue.SellState), 
			"OVERRIDE_EVENT" => typeof(string), 
			"OVERRIDE_REGION" => typeof(CardValue.OverrideRegion), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadCardValueDbfRecords loadRecords = new LoadCardValueDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		CardValueDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(CardValueDbfAsset)) as CardValueDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"CardValueDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
