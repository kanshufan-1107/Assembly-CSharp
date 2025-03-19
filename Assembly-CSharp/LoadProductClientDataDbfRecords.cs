using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadProductClientDataDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<ProductClientDataDbfRecord> GetRecords()
	{
		ProductClientDataDbfAsset dbfAsset = assetBundleRequest.asset as ProductClientDataDbfAsset;
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

	public LoadProductClientDataDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(ProductClientDataDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
