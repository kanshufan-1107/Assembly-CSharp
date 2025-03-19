using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class MercenaryArtVariationPremiumDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_mercenaryArtVariationId;

	[SerializeField]
	private MercenaryArtVariationPremium.MercenariesPremium m_premium;

	[SerializeField]
	private bool m_collectible;

	[SerializeField]
	private bool m_rewardTrack;

	[SerializeField]
	private DbfLocValue m_customAcquireText;

	[DbfField("MERCENARY_ART_VARIATION_ID")]
	public int MercenaryArtVariationId => m_mercenaryArtVariationId;

	[DbfField("PREMIUM")]
	public MercenaryArtVariationPremium.MercenariesPremium Premium => m_premium;

	[DbfField("COLLECTIBLE")]
	public bool Collectible => m_collectible;

	[DbfField("REWARD_TRACK")]
	public bool RewardTrack => m_rewardTrack;

	[DbfField("CUSTOM_ACQUIRE_TEXT")]
	public DbfLocValue CustomAcquireText => m_customAcquireText;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"MERCENARY_ART_VARIATION_ID" => m_mercenaryArtVariationId, 
			"PREMIUM" => m_premium, 
			"COLLECTIBLE" => m_collectible, 
			"REWARD_TRACK" => m_rewardTrack, 
			"CUSTOM_ACQUIRE_TEXT" => m_customAcquireText, 
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
		case "MERCENARY_ART_VARIATION_ID":
			m_mercenaryArtVariationId = (int)val;
			break;
		case "PREMIUM":
			if (val == null)
			{
				m_premium = MercenaryArtVariationPremium.MercenariesPremium.PREMIUM_NORMAL;
			}
			else if (val is MercenaryArtVariationPremium.MercenariesPremium || val is int)
			{
				m_premium = (MercenaryArtVariationPremium.MercenariesPremium)val;
			}
			else if (val is string)
			{
				m_premium = MercenaryArtVariationPremium.ParseMercenariesPremiumValue((string)val);
			}
			break;
		case "COLLECTIBLE":
			m_collectible = (bool)val;
			break;
		case "REWARD_TRACK":
			m_rewardTrack = (bool)val;
			break;
		case "CUSTOM_ACQUIRE_TEXT":
			m_customAcquireText = (DbfLocValue)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"MERCENARY_ART_VARIATION_ID" => typeof(int), 
			"PREMIUM" => typeof(MercenaryArtVariationPremium.MercenariesPremium), 
			"COLLECTIBLE" => typeof(bool), 
			"REWARD_TRACK" => typeof(bool), 
			"CUSTOM_ACQUIRE_TEXT" => typeof(DbfLocValue), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadMercenaryArtVariationPremiumDbfRecords loadRecords = new LoadMercenaryArtVariationPremiumDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		MercenaryArtVariationPremiumDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(MercenaryArtVariationPremiumDbfAsset)) as MercenaryArtVariationPremiumDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"MercenaryArtVariationPremiumDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_customAcquireText.StripUnusedLocales();
	}
}
