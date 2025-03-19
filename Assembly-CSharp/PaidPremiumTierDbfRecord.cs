using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class PaidPremiumTierDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_rewardTrackId;

	[SerializeField]
	private int m_accountLicenseId;

	[SerializeField]
	private int m_productId;

	[SerializeField]
	private int m_upgradeProductId;

	[DbfField("REWARD_TRACK_ID")]
	public int RewardTrackId => m_rewardTrackId;

	public AccountLicenseDbfRecord AccountLicenseRecord => GameDbf.AccountLicense.GetRecord(m_accountLicenseId);

	[DbfField("PRODUCT_ID")]
	public int ProductId => m_productId;

	[DbfField("UPGRADE_PRODUCT_ID")]
	public int UpgradeProductId => m_upgradeProductId;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"REWARD_TRACK_ID" => m_rewardTrackId, 
			"ACCOUNT_LICENSE_ID" => m_accountLicenseId, 
			"PRODUCT_ID" => m_productId, 
			"UPGRADE_PRODUCT_ID" => m_upgradeProductId, 
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
		case "REWARD_TRACK_ID":
			m_rewardTrackId = (int)val;
			break;
		case "ACCOUNT_LICENSE_ID":
			m_accountLicenseId = (int)val;
			break;
		case "PRODUCT_ID":
			m_productId = (int)val;
			break;
		case "UPGRADE_PRODUCT_ID":
			m_upgradeProductId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"REWARD_TRACK_ID" => typeof(int), 
			"ACCOUNT_LICENSE_ID" => typeof(int), 
			"PRODUCT_ID" => typeof(int), 
			"UPGRADE_PRODUCT_ID" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadPaidPremiumTierDbfRecords loadRecords = new LoadPaidPremiumTierDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		PaidPremiumTierDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(PaidPremiumTierDbfAsset)) as PaidPremiumTierDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"PaidPremiumTierDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
