using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadAchievementSectionDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<AchievementSectionDbfRecord> GetRecords()
	{
		AchievementSectionDbfAsset dbfAsset = assetBundleRequest.asset as AchievementSectionDbfAsset;
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

	public LoadAchievementSectionDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(AchievementSectionDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
