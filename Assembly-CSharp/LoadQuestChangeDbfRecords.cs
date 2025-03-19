using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadQuestChangeDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<QuestChangeDbfRecord> GetRecords()
	{
		QuestChangeDbfAsset dbfAsset = assetBundleRequest.asset as QuestChangeDbfAsset;
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

	public LoadQuestChangeDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(QuestChangeDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
