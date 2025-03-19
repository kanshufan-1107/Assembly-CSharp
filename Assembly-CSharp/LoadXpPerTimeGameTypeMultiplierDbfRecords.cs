using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadXpPerTimeGameTypeMultiplierDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<XpPerTimeGameTypeMultiplierDbfRecord> GetRecords()
	{
		XpPerTimeGameTypeMultiplierDbfAsset dbfAsset = assetBundleRequest.asset as XpPerTimeGameTypeMultiplierDbfAsset;
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

	public LoadXpPerTimeGameTypeMultiplierDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(XpPerTimeGameTypeMultiplierDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
