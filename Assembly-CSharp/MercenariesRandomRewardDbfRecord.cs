using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class MercenariesRandomRewardDbfRecord : DbfRecord
{
	[SerializeField]
	private MercenariesRandomReward.RewardType m_rewardType;

	[SerializeField]
	private bool m_restrictRarity;

	[SerializeField]
	private int m_rarityId;

	[SerializeField]
	private MercenariesRandomReward.MercenariesPremium m_premium;

	[DbfField("REWARD_TYPE")]
	public MercenariesRandomReward.RewardType RewardType => m_rewardType;

	[DbfField("RESTRICT_RARITY")]
	public bool RestrictRarity => m_restrictRarity;

	[DbfField("RARITY")]
	public int Rarity => m_rarityId;

	[DbfField("PREMIUM")]
	public MercenariesRandomReward.MercenariesPremium Premium => m_premium;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"REWARD_TYPE" => m_rewardType, 
			"RESTRICT_RARITY" => m_restrictRarity, 
			"RARITY" => m_rarityId, 
			"PREMIUM" => m_premium, 
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
		case "REWARD_TYPE":
			if (val == null)
			{
				m_rewardType = MercenariesRandomReward.RewardType.REWARD_TYPE_MERCENARY;
			}
			else if (val is MercenariesRandomReward.RewardType || val is int)
			{
				m_rewardType = (MercenariesRandomReward.RewardType)val;
			}
			else if (val is string)
			{
				m_rewardType = MercenariesRandomReward.ParseRewardTypeValue((string)val);
			}
			break;
		case "RESTRICT_RARITY":
			m_restrictRarity = (bool)val;
			break;
		case "RARITY":
			m_rarityId = (int)val;
			break;
		case "PREMIUM":
			if (val == null)
			{
				m_premium = MercenariesRandomReward.MercenariesPremium.PREMIUM_NORMAL;
			}
			else if (val is MercenariesRandomReward.MercenariesPremium || val is int)
			{
				m_premium = (MercenariesRandomReward.MercenariesPremium)val;
			}
			else if (val is string)
			{
				m_premium = MercenariesRandomReward.ParseMercenariesPremiumValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"REWARD_TYPE" => typeof(MercenariesRandomReward.RewardType), 
			"RESTRICT_RARITY" => typeof(bool), 
			"RARITY" => typeof(int), 
			"PREMIUM" => typeof(MercenariesRandomReward.MercenariesPremium), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadMercenariesRandomRewardDbfRecords loadRecords = new LoadMercenariesRandomRewardDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		MercenariesRandomRewardDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(MercenariesRandomRewardDbfAsset)) as MercenariesRandomRewardDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"MercenariesRandomRewardDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
