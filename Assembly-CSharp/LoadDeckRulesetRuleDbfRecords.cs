using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadDeckRulesetRuleDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<DeckRulesetRuleDbfRecord> GetRecords()
	{
		DeckRulesetRuleDbfAsset dbfAsset = assetBundleRequest.asset as DeckRulesetRuleDbfAsset;
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

	public LoadDeckRulesetRuleDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(DeckRulesetRuleDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
