using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadFixedRewardDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<FixedRewardDbfRecord> GetRecords()
	{
		FixedRewardDbfAsset dbfAsset = assetBundleRequest.asset as FixedRewardDbfAsset;
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

	public LoadFixedRewardDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(FixedRewardDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
