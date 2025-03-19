using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadMercenaryBuildingDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<MercenaryBuildingDbfRecord> GetRecords()
	{
		MercenaryBuildingDbfAsset dbfAsset = assetBundleRequest.asset as MercenaryBuildingDbfAsset;
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

	public LoadMercenaryBuildingDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(MercenaryBuildingDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
