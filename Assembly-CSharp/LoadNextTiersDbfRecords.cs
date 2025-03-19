using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadNextTiersDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<NextTiersDbfRecord> GetRecords()
	{
		NextTiersDbfAsset dbfAsset = assetBundleRequest.asset as NextTiersDbfAsset;
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

	public LoadNextTiersDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(NextTiersDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
