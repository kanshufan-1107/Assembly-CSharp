using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadAchievementCategoryDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<AchievementCategoryDbfRecord> GetRecords()
	{
		AchievementCategoryDbfAsset dbfAsset = assetBundleRequest.asset as AchievementCategoryDbfAsset;
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

	public LoadAchievementCategoryDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(AchievementCategoryDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
