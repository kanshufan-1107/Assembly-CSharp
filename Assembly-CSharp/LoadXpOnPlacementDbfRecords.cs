using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadXpOnPlacementDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<XpOnPlacementDbfRecord> GetRecords()
	{
		XpOnPlacementDbfAsset dbfAsset = assetBundleRequest.asset as XpOnPlacementDbfAsset;
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

	public LoadXpOnPlacementDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(XpOnPlacementDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
