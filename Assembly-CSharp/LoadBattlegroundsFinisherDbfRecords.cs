using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadBattlegroundsFinisherDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<BattlegroundsFinisherDbfRecord> GetRecords()
	{
		BattlegroundsFinisherDbfAsset dbfAsset = assetBundleRequest.asset as BattlegroundsFinisherDbfAsset;
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

	public LoadBattlegroundsFinisherDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(BattlegroundsFinisherDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
