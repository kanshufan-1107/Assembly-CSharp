using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadLoginRewardDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<LoginRewardDbfRecord> GetRecords()
	{
		LoginRewardDbfAsset dbfAsset = assetBundleRequest.asset as LoginRewardDbfAsset;
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

	public LoadLoginRewardDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(LoginRewardDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
