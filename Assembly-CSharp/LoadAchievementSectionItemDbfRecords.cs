using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadAchievementSectionItemDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<AchievementSectionItemDbfRecord> GetRecords()
	{
		AchievementSectionItemDbfAsset dbfAsset = assetBundleRequest.asset as AchievementSectionItemDbfAsset;
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

	public LoadAchievementSectionItemDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(AchievementSectionItemDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
