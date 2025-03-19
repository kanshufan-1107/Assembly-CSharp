using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadLeagueBgPublicRatingEquivDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<LeagueBgPublicRatingEquivDbfRecord> GetRecords()
	{
		LeagueBgPublicRatingEquivDbfAsset dbfAsset = assetBundleRequest.asset as LeagueBgPublicRatingEquivDbfAsset;
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

	public LoadLeagueBgPublicRatingEquivDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(LeagueBgPublicRatingEquivDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
