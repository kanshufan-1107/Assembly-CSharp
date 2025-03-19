using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadHiddenCardSetsDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<HiddenCardSetsDbfRecord> GetRecords()
	{
		HiddenCardSetsDbfAsset dbfAsset = assetBundleRequest.asset as HiddenCardSetsDbfAsset;
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

	public LoadHiddenCardSetsDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(HiddenCardSetsDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
