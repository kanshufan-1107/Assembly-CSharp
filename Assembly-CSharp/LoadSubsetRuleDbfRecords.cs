using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadSubsetRuleDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<SubsetRuleDbfRecord> GetRecords()
	{
		SubsetRuleDbfAsset dbfAsset = assetBundleRequest.asset as SubsetRuleDbfAsset;
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

	public LoadSubsetRuleDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(SubsetRuleDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
