using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadLeagueRankDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<LeagueRankDbfRecord> GetRecords()
	{
		LeagueRankDbfAsset dbfAsset = assetBundleRequest.asset as LeagueRankDbfAsset;
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

	public LoadLeagueRankDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(LeagueRankDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
