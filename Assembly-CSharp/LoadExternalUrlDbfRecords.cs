using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadExternalUrlDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<ExternalUrlDbfRecord> GetRecords()
	{
		ExternalUrlDbfAsset dbfAsset = assetBundleRequest.asset as ExternalUrlDbfAsset;
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

	public LoadExternalUrlDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(ExternalUrlDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
