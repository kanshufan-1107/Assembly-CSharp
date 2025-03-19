using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadAdventureModeDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<AdventureModeDbfRecord> GetRecords()
	{
		AdventureModeDbfAsset dbfAsset = assetBundleRequest.asset as AdventureModeDbfAsset;
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

	public LoadAdventureModeDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(AdventureModeDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
