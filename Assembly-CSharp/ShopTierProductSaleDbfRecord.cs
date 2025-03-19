using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class ShopTierProductSaleDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_tierId;

	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private int m_slotIndex = -1;

	[SerializeField]
	private int m_pmtProductId;

	[SerializeField]
	private string m_event = "always";

	public override object GetVar(string name)
	{
		return name switch
		{
			"TIER_ID" => m_tierId, 
			"NOTE_DESC" => m_noteDesc, 
			"SLOT_INDEX" => m_slotIndex, 
			"PMT_PRODUCT_ID" => m_pmtProductId, 
			"EVENT" => m_event, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "TIER_ID":
			m_tierId = (int)val;
			break;
		case "NOTE_DESC":
			m_noteDesc = (string)val;
			break;
		case "SLOT_INDEX":
			m_slotIndex = (int)val;
			break;
		case "PMT_PRODUCT_ID":
			m_pmtProductId = (int)val;
			break;
		case "EVENT":
			m_event = (string)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"TIER_ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"SLOT_INDEX" => typeof(int), 
			"PMT_PRODUCT_ID" => typeof(int), 
			"EVENT" => typeof(string), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadShopTierProductSaleDbfRecords loadRecords = new LoadShopTierProductSaleDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		ShopTierProductSaleDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(ShopTierProductSaleDbfAsset)) as ShopTierProductSaleDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"ShopTierProductSaleDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
