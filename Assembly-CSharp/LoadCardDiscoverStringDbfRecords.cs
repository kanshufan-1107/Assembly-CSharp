using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadCardDiscoverStringDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<CardDiscoverStringDbfRecord> GetRecords()
	{
		CardDiscoverStringDbfAsset dbfAsset = assetBundleRequest.asset as CardDiscoverStringDbfAsset;
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

	public LoadCardDiscoverStringDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(CardDiscoverStringDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
