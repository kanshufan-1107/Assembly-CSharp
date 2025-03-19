using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadQuestPoolDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<QuestPoolDbfRecord> GetRecords()
	{
		QuestPoolDbfAsset dbfAsset = assetBundleRequest.asset as QuestPoolDbfAsset;
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

	public LoadQuestPoolDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(QuestPoolDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
