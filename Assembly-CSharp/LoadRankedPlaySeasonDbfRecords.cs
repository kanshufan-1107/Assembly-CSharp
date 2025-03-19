using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadRankedPlaySeasonDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<RankedPlaySeasonDbfRecord> GetRecords()
	{
		RankedPlaySeasonDbfAsset dbfAsset = assetBundleRequest.asset as RankedPlaySeasonDbfAsset;
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

	public LoadRankedPlaySeasonDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(RankedPlaySeasonDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
