using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadOverrideQuestPoolIdListDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<OverrideQuestPoolIdListDbfRecord> GetRecords()
	{
		OverrideQuestPoolIdListDbfAsset dbfAsset = assetBundleRequest.asset as OverrideQuestPoolIdListDbfAsset;
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

	public LoadOverrideQuestPoolIdListDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(OverrideQuestPoolIdListDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
