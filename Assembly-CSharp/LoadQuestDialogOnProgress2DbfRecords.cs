using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadQuestDialogOnProgress2DbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<QuestDialogOnProgress2DbfRecord> GetRecords()
	{
		QuestDialogOnProgress2DbfAsset dbfAsset = assetBundleRequest.asset as QuestDialogOnProgress2DbfAsset;
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

	public LoadQuestDialogOnProgress2DbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(QuestDialogOnProgress2DbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
