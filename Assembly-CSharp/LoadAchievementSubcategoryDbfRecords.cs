using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadAchievementSubcategoryDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<AchievementSubcategoryDbfRecord> GetRecords()
	{
		AchievementSubcategoryDbfAsset dbfAsset = assetBundleRequest.asset as AchievementSubcategoryDbfAsset;
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

	public LoadAchievementSubcategoryDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(AchievementSubcategoryDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
