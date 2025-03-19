using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadFixedRewardMapDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<FixedRewardMapDbfRecord> GetRecords()
	{
		FixedRewardMapDbfAsset dbfAsset = assetBundleRequest.asset as FixedRewardMapDbfAsset;
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

	public LoadFixedRewardMapDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(FixedRewardMapDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
