using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadUnlockedTavernGuideSetDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<UnlockedTavernGuideSetDbfRecord> GetRecords()
	{
		UnlockedTavernGuideSetDbfAsset dbfAsset = assetBundleRequest.asset as UnlockedTavernGuideSetDbfAsset;
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

	public LoadUnlockedTavernGuideSetDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(UnlockedTavernGuideSetDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
