using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadVisitorTaskDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<VisitorTaskDbfRecord> GetRecords()
	{
		VisitorTaskDbfAsset dbfAsset = assetBundleRequest.asset as VisitorTaskDbfAsset;
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

	public LoadVisitorTaskDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(VisitorTaskDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
