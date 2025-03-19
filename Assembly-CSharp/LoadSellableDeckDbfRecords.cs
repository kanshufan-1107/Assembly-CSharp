using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadSellableDeckDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<SellableDeckDbfRecord> GetRecords()
	{
		SellableDeckDbfAsset dbfAsset = assetBundleRequest.asset as SellableDeckDbfAsset;
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

	public LoadSellableDeckDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(SellableDeckDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
