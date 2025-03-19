using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadPvpdrSeasonDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<PvpdrSeasonDbfRecord> GetRecords()
	{
		PvpdrSeasonDbfAsset dbfAsset = assetBundleRequest.asset as PvpdrSeasonDbfAsset;
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

	public LoadPvpdrSeasonDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(PvpdrSeasonDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
