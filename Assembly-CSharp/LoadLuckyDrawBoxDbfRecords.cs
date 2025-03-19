using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadLuckyDrawBoxDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<LuckyDrawBoxDbfRecord> GetRecords()
	{
		LuckyDrawBoxDbfAsset dbfAsset = assetBundleRequest.asset as LuckyDrawBoxDbfAsset;
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

	public LoadLuckyDrawBoxDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(LuckyDrawBoxDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
