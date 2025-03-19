using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadAchieveRegionDataDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<AchieveRegionDataDbfRecord> GetRecords()
	{
		AchieveRegionDataDbfAsset dbfAsset = assetBundleRequest.asset as AchieveRegionDataDbfAsset;
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

	public LoadAchieveRegionDataDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(AchieveRegionDataDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
