using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadVisitorTaskChainDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<VisitorTaskChainDbfRecord> GetRecords()
	{
		VisitorTaskChainDbfAsset dbfAsset = assetBundleRequest.asset as VisitorTaskChainDbfAsset;
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

	public LoadVisitorTaskChainDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(VisitorTaskChainDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
