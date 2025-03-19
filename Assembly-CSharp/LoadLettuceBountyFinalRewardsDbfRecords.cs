using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadLettuceBountyFinalRewardsDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<LettuceBountyFinalRewardsDbfRecord> GetRecords()
	{
		LettuceBountyFinalRewardsDbfAsset dbfAsset = assetBundleRequest.asset as LettuceBountyFinalRewardsDbfAsset;
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

	public LoadLettuceBountyFinalRewardsDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(LettuceBountyFinalRewardsDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
