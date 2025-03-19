using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadMythicAbilityScalingCardTagDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<MythicAbilityScalingCardTagDbfRecord> GetRecords()
	{
		MythicAbilityScalingCardTagDbfAsset dbfAsset = assetBundleRequest.asset as MythicAbilityScalingCardTagDbfAsset;
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

	public LoadMythicAbilityScalingCardTagDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(MythicAbilityScalingCardTagDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
