using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadScenarioGuestHeroesDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<ScenarioGuestHeroesDbfRecord> GetRecords()
	{
		ScenarioGuestHeroesDbfAsset dbfAsset = assetBundleRequest.asset as ScenarioGuestHeroesDbfAsset;
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

	public LoadScenarioGuestHeroesDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(ScenarioGuestHeroesDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
