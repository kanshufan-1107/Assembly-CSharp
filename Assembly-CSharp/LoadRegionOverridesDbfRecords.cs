using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadRegionOverridesDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<RegionOverridesDbfRecord> GetRecords()
	{
		RegionOverridesDbfAsset dbfAsset = assetBundleRequest.asset as RegionOverridesDbfAsset;
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

	public LoadRegionOverridesDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(RegionOverridesDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
