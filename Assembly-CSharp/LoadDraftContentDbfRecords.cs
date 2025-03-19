using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadDraftContentDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<DraftContentDbfRecord> GetRecords()
	{
		DraftContentDbfAsset dbfAsset = assetBundleRequest.asset as DraftContentDbfAsset;
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

	public LoadDraftContentDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(DraftContentDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
