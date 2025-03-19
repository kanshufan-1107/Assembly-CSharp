using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadBonusBountyDropChanceDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<BonusBountyDropChanceDbfRecord> GetRecords()
	{
		BonusBountyDropChanceDbfAsset dbfAsset = assetBundleRequest.asset as BonusBountyDropChanceDbfAsset;
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

	public LoadBonusBountyDropChanceDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(BonusBountyDropChanceDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
