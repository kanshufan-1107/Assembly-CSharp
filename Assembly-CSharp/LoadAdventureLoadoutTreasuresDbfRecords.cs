using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadAdventureLoadoutTreasuresDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<AdventureLoadoutTreasuresDbfRecord> GetRecords()
	{
		AdventureLoadoutTreasuresDbfAsset dbfAsset = assetBundleRequest.asset as AdventureLoadoutTreasuresDbfAsset;
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

	public LoadAdventureLoadoutTreasuresDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(AdventureLoadoutTreasuresDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
