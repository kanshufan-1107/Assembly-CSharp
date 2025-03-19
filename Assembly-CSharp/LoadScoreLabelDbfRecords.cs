using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadScoreLabelDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<ScoreLabelDbfRecord> GetRecords()
	{
		ScoreLabelDbfAsset dbfAsset = assetBundleRequest.asset as ScoreLabelDbfAsset;
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

	public LoadScoreLabelDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(ScoreLabelDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
