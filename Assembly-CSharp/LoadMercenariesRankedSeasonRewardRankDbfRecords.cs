using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadMercenariesRankedSeasonRewardRankDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<MercenariesRankedSeasonRewardRankDbfRecord> GetRecords()
	{
		MercenariesRankedSeasonRewardRankDbfAsset dbfAsset = assetBundleRequest.asset as MercenariesRankedSeasonRewardRankDbfAsset;
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

	public LoadMercenariesRankedSeasonRewardRankDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(MercenariesRankedSeasonRewardRankDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
