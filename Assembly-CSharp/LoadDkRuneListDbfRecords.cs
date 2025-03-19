using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadDkRuneListDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<DkRuneListDbfRecord> GetRecords()
	{
		DkRuneListDbfAsset dbfAsset = assetBundleRequest.asset as DkRuneListDbfAsset;
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

	public LoadDkRuneListDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(DkRuneListDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
