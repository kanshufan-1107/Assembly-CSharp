using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadCardAdditonalSearchTermsDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<CardAdditonalSearchTermsDbfRecord> GetRecords()
	{
		CardAdditonalSearchTermsDbfAsset dbfAsset = assetBundleRequest.asset as CardAdditonalSearchTermsDbfAsset;
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

	public LoadCardAdditonalSearchTermsDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(CardAdditonalSearchTermsDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
