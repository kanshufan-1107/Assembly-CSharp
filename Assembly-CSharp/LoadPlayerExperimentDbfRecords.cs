using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadPlayerExperimentDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<PlayerExperimentDbfRecord> GetRecords()
	{
		PlayerExperimentDbfAsset dbfAsset = assetBundleRequest.asset as PlayerExperimentDbfAsset;
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

	public LoadPlayerExperimentDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(PlayerExperimentDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
