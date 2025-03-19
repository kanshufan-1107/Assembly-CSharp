using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadTavernGuideQuestSetDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<TavernGuideQuestSetDbfRecord> GetRecords()
	{
		TavernGuideQuestSetDbfAsset dbfAsset = assetBundleRequest.asset as TavernGuideQuestSetDbfAsset;
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

	public LoadTavernGuideQuestSetDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(TavernGuideQuestSetDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
