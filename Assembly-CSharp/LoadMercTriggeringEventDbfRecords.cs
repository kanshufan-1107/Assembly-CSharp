using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadMercTriggeringEventDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<MercTriggeringEventDbfRecord> GetRecords()
	{
		MercTriggeringEventDbfAsset dbfAsset = assetBundleRequest.asset as MercTriggeringEventDbfAsset;
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

	public LoadMercTriggeringEventDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(MercTriggeringEventDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
