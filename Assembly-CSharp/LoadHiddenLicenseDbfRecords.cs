using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadHiddenLicenseDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<HiddenLicenseDbfRecord> GetRecords()
	{
		HiddenLicenseDbfAsset dbfAsset = assetBundleRequest.asset as HiddenLicenseDbfAsset;
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

	public LoadHiddenLicenseDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(HiddenLicenseDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
