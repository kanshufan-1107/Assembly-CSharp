using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadAccountLicenseDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<AccountLicenseDbfRecord> GetRecords()
	{
		AccountLicenseDbfAsset dbfAsset = assetBundleRequest.asset as AccountLicenseDbfAsset;
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

	public LoadAccountLicenseDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(AccountLicenseDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
