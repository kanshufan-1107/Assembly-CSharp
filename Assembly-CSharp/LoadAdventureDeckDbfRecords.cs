using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadAdventureDeckDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<AdventureDeckDbfRecord> GetRecords()
	{
		AdventureDeckDbfAsset dbfAsset = assetBundleRequest.asset as AdventureDeckDbfAsset;
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

	public LoadAdventureDeckDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(AdventureDeckDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
