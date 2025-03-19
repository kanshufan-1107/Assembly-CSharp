using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadTavernGuideQuestDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<TavernGuideQuestDbfRecord> GetRecords()
	{
		TavernGuideQuestDbfAsset dbfAsset = assetBundleRequest.asset as TavernGuideQuestDbfAsset;
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

	public LoadTavernGuideQuestDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(TavernGuideQuestDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
