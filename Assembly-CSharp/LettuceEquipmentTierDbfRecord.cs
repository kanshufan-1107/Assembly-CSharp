using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceEquipmentTierDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_lettuceEquipmentId;

	[SerializeField]
	private int m_tier = 1;

	[SerializeField]
	private int m_cardId;

	[SerializeField]
	private int m_coinCraftCost = 100;

	[SerializeField]
	private bool m_showTextOnMerc;

	[DbfField("LETTUCE_EQUIPMENT_ID")]
	public int LettuceEquipmentId => m_lettuceEquipmentId;

	[DbfField("TIER")]
	public int Tier => m_tier;

	[DbfField("CARD_ID")]
	public int CardId => m_cardId;

	public CardDbfRecord CardRecord => GameDbf.Card.GetRecord(m_cardId);

	[DbfField("COIN_CRAFT_COST")]
	public int CoinCraftCost => m_coinCraftCost;

	[DbfField("SHOW_TEXT_ON_MERC")]
	public bool ShowTextOnMerc => m_showTextOnMerc;

	public LettuceEquipmentModifierDataDbfRecord EquipmentModifierData
	{
		get
		{
			int id = base.ID;
			List<LettuceEquipmentModifierDataDbfRecord> records = GameDbf.LettuceEquipmentModifierData.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				LettuceEquipmentModifierDataDbfRecord record = records[i];
				if (record.LettuceEquipmentTierId == id)
				{
					return record;
				}
			}
			return null;
		}
	}

	public List<BonusBountyDropChanceDbfRecord> BonusBountyDropChances
	{
		get
		{
			int id = base.ID;
			List<BonusBountyDropChanceDbfRecord> returnRecords = new List<BonusBountyDropChanceDbfRecord>();
			List<BonusBountyDropChanceDbfRecord> records = GameDbf.BonusBountyDropChance.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				BonusBountyDropChanceDbfRecord record = records[i];
				if (record.LettuceEquipmentTierId == id)
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
			"LETTUCE_EQUIPMENT_ID" => m_lettuceEquipmentId, 
			"TIER" => m_tier, 
			"CARD_ID" => m_cardId, 
			"COIN_CRAFT_COST" => m_coinCraftCost, 
			"SHOW_TEXT_ON_MERC" => m_showTextOnMerc, 
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
		case "LETTUCE_EQUIPMENT_ID":
			m_lettuceEquipmentId = (int)val;
			break;
		case "TIER":
			m_tier = (int)val;
			break;
		case "CARD_ID":
			m_cardId = (int)val;
			break;
		case "COIN_CRAFT_COST":
			m_coinCraftCost = (int)val;
			break;
		case "SHOW_TEXT_ON_MERC":
			m_showTextOnMerc = (bool)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LETTUCE_EQUIPMENT_ID" => typeof(int), 
			"TIER" => typeof(int), 
			"CARD_ID" => typeof(int), 
			"COIN_CRAFT_COST" => typeof(int), 
			"SHOW_TEXT_ON_MERC" => typeof(bool), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceEquipmentTierDbfRecords loadRecords = new LoadLettuceEquipmentTierDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceEquipmentTierDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceEquipmentTierDbfAsset)) as LettuceEquipmentTierDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceEquipmentTierDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
