using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadDeckTemplateChoicesDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<DeckTemplateChoicesDbfRecord> GetRecords()
	{
		DeckTemplateChoicesDbfAsset dbfAsset = assetBundleRequest.asset as DeckTemplateChoicesDbfAsset;
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

	public LoadDeckTemplateChoicesDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(DeckTemplateChoicesDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
