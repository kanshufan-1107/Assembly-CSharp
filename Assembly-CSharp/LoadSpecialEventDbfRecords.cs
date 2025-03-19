using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadSpecialEventDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<SpecialEventDbfRecord> GetRecords()
	{
		SpecialEventDbfAsset dbfAsset = assetBundleRequest.asset as SpecialEventDbfAsset;
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

	public LoadSpecialEventDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(SpecialEventDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
