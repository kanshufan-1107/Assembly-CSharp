using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadRelatedCardsDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<RelatedCardsDbfRecord> GetRecords()
	{
		RelatedCardsDbfAsset dbfAsset = assetBundleRequest.asset as RelatedCardsDbfAsset;
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

	public LoadRelatedCardsDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(RelatedCardsDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
