using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadDetailsVideoCueDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<DetailsVideoCueDbfRecord> GetRecords()
	{
		DetailsVideoCueDbfAsset dbfAsset = assetBundleRequest.asset as DetailsVideoCueDbfAsset;
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

	public LoadDetailsVideoCueDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(DetailsVideoCueDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
