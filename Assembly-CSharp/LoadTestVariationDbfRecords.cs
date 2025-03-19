using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadTestVariationDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<TestVariationDbfRecord> GetRecords()
	{
		TestVariationDbfAsset dbfAsset = assetBundleRequest.asset as TestVariationDbfAsset;
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

	public LoadTestVariationDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(TestVariationDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
