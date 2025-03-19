using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadBattlegroundsGuideSkinDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<BattlegroundsGuideSkinDbfRecord> GetRecords()
	{
		BattlegroundsGuideSkinDbfAsset dbfAsset = assetBundleRequest.asset as BattlegroundsGuideSkinDbfAsset;
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

	public LoadBattlegroundsGuideSkinDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(BattlegroundsGuideSkinDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
