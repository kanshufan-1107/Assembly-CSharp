using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadCosmeticCoinDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<CosmeticCoinDbfRecord> GetRecords()
	{
		CosmeticCoinDbfAsset dbfAsset = assetBundleRequest.asset as CosmeticCoinDbfAsset;
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

	public LoadCosmeticCoinDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(CosmeticCoinDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
