using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadBattlegroundsSeasonDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<BattlegroundsSeasonDbfRecord> GetRecords()
	{
		BattlegroundsSeasonDbfAsset dbfAsset = assetBundleRequest.asset as BattlegroundsSeasonDbfAsset;
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

	public LoadBattlegroundsSeasonDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(BattlegroundsSeasonDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
