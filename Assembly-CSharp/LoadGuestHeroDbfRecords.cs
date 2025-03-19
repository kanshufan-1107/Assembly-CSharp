using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadGuestHeroDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<GuestHeroDbfRecord> GetRecords()
	{
		GuestHeroDbfAsset dbfAsset = assetBundleRequest.asset as GuestHeroDbfAsset;
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

	public LoadGuestHeroDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(GuestHeroDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
