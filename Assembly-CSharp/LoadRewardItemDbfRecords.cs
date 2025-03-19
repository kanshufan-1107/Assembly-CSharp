using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadRewardItemDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<RewardItemDbfRecord> GetRecords()
	{
		RewardItemDbfAsset dbfAsset = assetBundleRequest.asset as RewardItemDbfAsset;
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

	public LoadRewardItemDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(RewardItemDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
