using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadScalingTreasureCardTagDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<ScalingTreasureCardTagDbfRecord> GetRecords()
	{
		ScalingTreasureCardTagDbfAsset dbfAsset = assetBundleRequest.asset as ScalingTreasureCardTagDbfAsset;
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

	public LoadScalingTreasureCardTagDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(ScalingTreasureCardTagDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
