using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class InitCardValueDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_premium;

	[SerializeField]
	private int m_rarityId;

	[SerializeField]
	private int m_buy;

	[SerializeField]
	private int m_sell;

	[SerializeField]
	private int m_upgrade;

	[DbfField("PREMIUM")]
	public int Premium => m_premium;

	[DbfField("RARITY")]
	public int Rarity => m_rarityId;

	[DbfField("BUY")]
	public int Buy => m_buy;

	[DbfField("SELL")]
	public int Sell => m_sell;

	[DbfField("UPGRADE")]
	public int Upgrade => m_upgrade;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"PREMIUM" => m_premium, 
			"RARITY" => m_rarityId, 
			"BUY" => m_buy, 
			"SELL" => m_sell, 
			"UPGRADE" => m_upgrade, 
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
		case "PREMIUM":
			m_premium = (int)val;
			break;
		case "RARITY":
			m_rarityId = (int)val;
			break;
		case "BUY":
			m_buy = (int)val;
			break;
		case "SELL":
			m_sell = (int)val;
			break;
		case "UPGRADE":
			m_upgrade = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"PREMIUM" => typeof(int), 
			"RARITY" => typeof(int), 
			"BUY" => typeof(int), 
			"SELL" => typeof(int), 
			"UPGRADE" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadInitCardValueDbfRecords loadRecords = new LoadInitCardValueDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		InitCardValueDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(InitCardValueDbfAsset)) as InitCardValueDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"InitCardValueDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
