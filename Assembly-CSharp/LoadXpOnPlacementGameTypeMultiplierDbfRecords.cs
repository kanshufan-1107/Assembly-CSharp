using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadXpOnPlacementGameTypeMultiplierDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<XpOnPlacementGameTypeMultiplierDbfRecord> GetRecords()
	{
		XpOnPlacementGameTypeMultiplierDbfAsset dbfAsset = assetBundleRequest.asset as XpOnPlacementGameTypeMultiplierDbfAsset;
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

	public LoadXpOnPlacementGameTypeMultiplierDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(XpOnPlacementGameTypeMultiplierDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
