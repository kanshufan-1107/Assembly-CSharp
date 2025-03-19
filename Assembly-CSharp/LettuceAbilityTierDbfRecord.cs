using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class LettuceAbilityTierDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_lettuceAbilityId;

	[SerializeField]
	private int m_tier = 1;

	[SerializeField]
	private int m_cardId;

	[SerializeField]
	private int m_coinCraftCost = 100;

	[DbfField("LETTUCE_ABILITY_ID")]
	public int LettuceAbilityId => m_lettuceAbilityId;

	[DbfField("TIER")]
	public int Tier => m_tier;

	[DbfField("CARD_ID")]
	public int CardId => m_cardId;

	public CardDbfRecord CardRecord => GameDbf.Card.GetRecord(m_cardId);

	[DbfField("COIN_CRAFT_COST")]
	public int CoinCraftCost => m_coinCraftCost;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"LETTUCE_ABILITY_ID" => m_lettuceAbilityId, 
			"TIER" => m_tier, 
			"CARD_ID" => m_cardId, 
			"COIN_CRAFT_COST" => m_coinCraftCost, 
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
		case "LETTUCE_ABILITY_ID":
			m_lettuceAbilityId = (int)val;
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
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"LETTUCE_ABILITY_ID" => typeof(int), 
			"TIER" => typeof(int), 
			"CARD_ID" => typeof(int), 
			"COIN_CRAFT_COST" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadLettuceAbilityTierDbfRecords loadRecords = new LoadLettuceAbilityTierDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		LettuceAbilityTierDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(LettuceAbilityTierDbfAsset)) as LettuceAbilityTierDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"LettuceAbilityTierDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
