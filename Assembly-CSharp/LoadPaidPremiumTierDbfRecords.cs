using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadPaidPremiumTierDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<PaidPremiumTierDbfRecord> GetRecords()
	{
		PaidPremiumTierDbfAsset dbfAsset = assetBundleRequest.asset as PaidPremiumTierDbfAsset;
		if (dbfAsset != null)
		{
			for (int i = 0; i < dbfAsset.Records.Count; i++)
			{
				dbfAsset.Records[i].StripUnusedLocales();
			}
			return dbfAsset.Records;
		}
		return null;
	}

	public LoadPaidPremiumTierDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(PaidPremiumTierDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
