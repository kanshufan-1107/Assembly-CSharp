using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadBattlegroundsHeroSkinDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<BattlegroundsHeroSkinDbfRecord> GetRecords()
	{
		BattlegroundsHeroSkinDbfAsset dbfAsset = assetBundleRequest.asset as BattlegroundsHeroSkinDbfAsset;
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

	public LoadBattlegroundsHeroSkinDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(BattlegroundsHeroSkinDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
