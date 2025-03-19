using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadInitCardValueDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<InitCardValueDbfRecord> GetRecords()
	{
		InitCardValueDbfAsset dbfAsset = assetBundleRequest.asset as InitCardValueDbfAsset;
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

	public LoadInitCardValueDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(InitCardValueDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
