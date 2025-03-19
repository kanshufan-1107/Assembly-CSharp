using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadSideboardCardDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<SideboardCardDbfRecord> GetRecords()
	{
		SideboardCardDbfAsset dbfAsset = assetBundleRequest.asset as SideboardCardDbfAsset;
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

	public LoadSideboardCardDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(SideboardCardDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
