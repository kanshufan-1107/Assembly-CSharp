using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadCreditsYearDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<CreditsYearDbfRecord> GetRecords()
	{
		CreditsYearDbfAsset dbfAsset = assetBundleRequest.asset as CreditsYearDbfAsset;
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

	public LoadCreditsYearDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(CreditsYearDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
