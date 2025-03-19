using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadGuestHeroSelectionRatioDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<GuestHeroSelectionRatioDbfRecord> GetRecords()
	{
		GuestHeroSelectionRatioDbfAsset dbfAsset = assetBundleRequest.asset as GuestHeroSelectionRatioDbfAsset;
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

	public LoadGuestHeroSelectionRatioDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(GuestHeroSelectionRatioDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
