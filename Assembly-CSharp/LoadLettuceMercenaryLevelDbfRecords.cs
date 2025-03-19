using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadLettuceMercenaryLevelDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<LettuceMercenaryLevelDbfRecord> GetRecords()
	{
		LettuceMercenaryLevelDbfAsset dbfAsset = assetBundleRequest.asset as LettuceMercenaryLevelDbfAsset;
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

	public LoadLettuceMercenaryLevelDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(LettuceMercenaryLevelDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
