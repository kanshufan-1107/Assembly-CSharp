using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadQuestDialogOnCompleteDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<QuestDialogOnCompleteDbfRecord> GetRecords()
	{
		QuestDialogOnCompleteDbfAsset dbfAsset = assetBundleRequest.asset as QuestDialogOnCompleteDbfAsset;
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

	public LoadQuestDialogOnCompleteDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(QuestDialogOnCompleteDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
