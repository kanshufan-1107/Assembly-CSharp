using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadKeywordTextDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<KeywordTextDbfRecord> GetRecords()
	{
		KeywordTextDbfAsset dbfAsset = assetBundleRequest.asset as KeywordTextDbfAsset;
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

	public LoadKeywordTextDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(KeywordTextDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
