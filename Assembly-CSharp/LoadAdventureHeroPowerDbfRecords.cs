using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadAdventureHeroPowerDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<AdventureHeroPowerDbfRecord> GetRecords()
	{
		AdventureHeroPowerDbfAsset dbfAsset = assetBundleRequest.asset as AdventureHeroPowerDbfAsset;
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

	public LoadAdventureHeroPowerDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(AdventureHeroPowerDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
