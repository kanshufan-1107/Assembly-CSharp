using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadReplacementsWhenPlayedDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<ReplacementsWhenPlayedDbfRecord> GetRecords()
	{
		ReplacementsWhenPlayedDbfAsset dbfAsset = assetBundleRequest.asset as ReplacementsWhenPlayedDbfAsset;
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

	public LoadReplacementsWhenPlayedDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(ReplacementsWhenPlayedDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
