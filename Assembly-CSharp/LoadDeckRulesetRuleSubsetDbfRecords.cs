using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadDeckRulesetRuleSubsetDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<DeckRulesetRuleSubsetDbfRecord> GetRecords()
	{
		DeckRulesetRuleSubsetDbfAsset dbfAsset = assetBundleRequest.asset as DeckRulesetRuleSubsetDbfAsset;
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

	public LoadDeckRulesetRuleSubsetDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(DeckRulesetRuleSubsetDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
