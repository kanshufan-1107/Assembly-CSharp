using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadBoxProductBannerDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<BoxProductBannerDbfRecord> GetRecords()
	{
		BoxProductBannerDbfAsset dbfAsset = assetBundleRequest.asset as BoxProductBannerDbfAsset;
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

	public LoadBoxProductBannerDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(BoxProductBannerDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
