using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadAdventureDataDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<AdventureDataDbfRecord> GetRecords()
	{
		AdventureDataDbfAsset dbfAsset = assetBundleRequest.asset as AdventureDataDbfAsset;
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

	public LoadAdventureDataDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(AdventureDataDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
