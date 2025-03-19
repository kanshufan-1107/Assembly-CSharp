using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadTavernBrawlTicketDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<TavernBrawlTicketDbfRecord> GetRecords()
	{
		TavernBrawlTicketDbfAsset dbfAsset = assetBundleRequest.asset as TavernBrawlTicketDbfAsset;
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

	public LoadTavernBrawlTicketDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(TavernBrawlTicketDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
