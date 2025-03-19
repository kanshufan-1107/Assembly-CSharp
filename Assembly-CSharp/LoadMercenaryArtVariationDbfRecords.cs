using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadMercenaryArtVariationDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<MercenaryArtVariationDbfRecord> GetRecords()
	{
		MercenaryArtVariationDbfAsset dbfAsset = assetBundleRequest.asset as MercenaryArtVariationDbfAsset;
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

	public LoadMercenaryArtVariationDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(MercenaryArtVariationDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
