using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadQuestDialogOnProgress1DbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<QuestDialogOnProgress1DbfRecord> GetRecords()
	{
		QuestDialogOnProgress1DbfAsset dbfAsset = assetBundleRequest.asset as QuestDialogOnProgress1DbfAsset;
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

	public LoadQuestDialogOnProgress1DbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(QuestDialogOnProgress1DbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
