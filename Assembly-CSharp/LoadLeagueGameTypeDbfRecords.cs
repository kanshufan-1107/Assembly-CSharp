using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadLeagueGameTypeDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<LeagueGameTypeDbfRecord> GetRecords()
	{
		LeagueGameTypeDbfAsset dbfAsset = assetBundleRequest.asset as LeagueGameTypeDbfAsset;
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

	public LoadLeagueGameTypeDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(LeagueGameTypeDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
