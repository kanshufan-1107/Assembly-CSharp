using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadRewardBagDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<RewardBagDbfRecord> GetRecords()
	{
		RewardBagDbfAsset dbfAsset = assetBundleRequest.asset as RewardBagDbfAsset;
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

	public LoadRewardBagDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(RewardBagDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
