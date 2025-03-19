using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadLuckyDrawRewardsDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<LuckyDrawRewardsDbfRecord> GetRecords()
	{
		LuckyDrawRewardsDbfAsset dbfAsset = assetBundleRequest.asset as LuckyDrawRewardsDbfAsset;
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

	public LoadLuckyDrawRewardsDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(LuckyDrawRewardsDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
