using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadLettuceTreasureDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<LettuceTreasureDbfRecord> GetRecords()
	{
		LettuceTreasureDbfAsset dbfAsset = assetBundleRequest.asset as LettuceTreasureDbfAsset;
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

	public LoadLettuceTreasureDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(LettuceTreasureDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
