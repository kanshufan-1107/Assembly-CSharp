using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadBoosterCardSetDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<BoosterCardSetDbfRecord> GetRecords()
	{
		BoosterCardSetDbfAsset dbfAsset = assetBundleRequest.asset as BoosterCardSetDbfAsset;
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

	public LoadBoosterCardSetDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(BoosterCardSetDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
