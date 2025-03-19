using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadTavernGuideQuestRecommendedClassesDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<TavernGuideQuestRecommendedClassesDbfRecord> GetRecords()
	{
		TavernGuideQuestRecommendedClassesDbfAsset dbfAsset = assetBundleRequest.asset as TavernGuideQuestRecommendedClassesDbfAsset;
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

	public LoadTavernGuideQuestRecommendedClassesDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(TavernGuideQuestRecommendedClassesDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
