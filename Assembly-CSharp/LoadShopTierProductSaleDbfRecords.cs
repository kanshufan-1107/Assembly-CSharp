using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadShopTierProductSaleDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<ShopTierProductSaleDbfRecord> GetRecords()
	{
		ShopTierProductSaleDbfAsset dbfAsset = assetBundleRequest.asset as ShopTierProductSaleDbfAsset;
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

	public LoadShopTierProductSaleDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(ShopTierProductSaleDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
