using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadDeckTemplateDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<DeckTemplateDbfRecord> GetRecords()
	{
		DeckTemplateDbfAsset dbfAsset = assetBundleRequest.asset as DeckTemplateDbfAsset;
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

	public LoadDeckTemplateDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(DeckTemplateDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
