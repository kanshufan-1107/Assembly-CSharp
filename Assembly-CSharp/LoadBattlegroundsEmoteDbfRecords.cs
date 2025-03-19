using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadBattlegroundsEmoteDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<BattlegroundsEmoteDbfRecord> GetRecords()
	{
		BattlegroundsEmoteDbfAsset dbfAsset = assetBundleRequest.asset as BattlegroundsEmoteDbfAsset;
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

	public LoadBattlegroundsEmoteDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(BattlegroundsEmoteDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
