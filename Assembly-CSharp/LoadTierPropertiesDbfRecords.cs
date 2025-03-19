using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadTierPropertiesDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<TierPropertiesDbfRecord> GetRecords()
	{
		TierPropertiesDbfAsset dbfAsset = assetBundleRequest.asset as TierPropertiesDbfAsset;
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

	public LoadTierPropertiesDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(TierPropertiesDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
