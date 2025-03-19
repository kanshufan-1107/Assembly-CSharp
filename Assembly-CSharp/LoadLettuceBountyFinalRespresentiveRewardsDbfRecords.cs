using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadLettuceBountyFinalRespresentiveRewardsDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<LettuceBountyFinalRespresentiveRewardsDbfRecord> GetRecords()
	{
		LettuceBountyFinalRespresentiveRewardsDbfAsset dbfAsset = assetBundleRequest.asset as LettuceBountyFinalRespresentiveRewardsDbfAsset;
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

	public LoadLettuceBountyFinalRespresentiveRewardsDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(LettuceBountyFinalRespresentiveRewardsDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
