using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadFixedRewardActionDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<FixedRewardActionDbfRecord> GetRecords()
	{
		FixedRewardActionDbfAsset dbfAsset = assetBundleRequest.asset as FixedRewardActionDbfAsset;
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

	public LoadFixedRewardActionDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(FixedRewardActionDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
