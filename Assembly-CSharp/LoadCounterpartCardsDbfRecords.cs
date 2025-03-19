using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadCounterpartCardsDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<CounterpartCardsDbfRecord> GetRecords()
	{
		CounterpartCardsDbfAsset dbfAsset = assetBundleRequest.asset as CounterpartCardsDbfAsset;
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

	public LoadCounterpartCardsDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(CounterpartCardsDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
