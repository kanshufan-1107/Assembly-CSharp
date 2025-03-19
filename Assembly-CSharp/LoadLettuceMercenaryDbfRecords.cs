using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadLettuceMercenaryDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<LettuceMercenaryDbfRecord> GetRecords()
	{
		LettuceMercenaryDbfAsset dbfAsset = assetBundleRequest.asset as LettuceMercenaryDbfAsset;
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

	public LoadLettuceMercenaryDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(LettuceMercenaryDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
