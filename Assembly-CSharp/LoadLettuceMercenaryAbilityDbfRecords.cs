using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadLettuceMercenaryAbilityDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<LettuceMercenaryAbilityDbfRecord> GetRecords()
	{
		LettuceMercenaryAbilityDbfAsset dbfAsset = assetBundleRequest.asset as LettuceMercenaryAbilityDbfAsset;
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

	public LoadLettuceMercenaryAbilityDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(LettuceMercenaryAbilityDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
