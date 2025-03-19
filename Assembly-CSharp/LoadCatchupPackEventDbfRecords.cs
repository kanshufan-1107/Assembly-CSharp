using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadCatchupPackEventDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<CatchupPackEventDbfRecord> GetRecords()
	{
		CatchupPackEventDbfAsset dbfAsset = assetBundleRequest.asset as CatchupPackEventDbfAsset;
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

	public LoadCatchupPackEventDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(CatchupPackEventDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
