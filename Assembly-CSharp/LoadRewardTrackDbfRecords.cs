using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadRewardTrackDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<RewardTrackDbfRecord> GetRecords()
	{
		RewardTrackDbfAsset dbfAsset = assetBundleRequest.asset as RewardTrackDbfAsset;
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

	public LoadRewardTrackDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(RewardTrackDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
