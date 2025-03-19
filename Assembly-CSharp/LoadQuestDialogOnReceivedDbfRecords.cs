using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadQuestDialogOnReceivedDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<QuestDialogOnReceivedDbfRecord> GetRecords()
	{
		QuestDialogOnReceivedDbfAsset dbfAsset = assetBundleRequest.asset as QuestDialogOnReceivedDbfAsset;
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

	public LoadQuestDialogOnReceivedDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(QuestDialogOnReceivedDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
