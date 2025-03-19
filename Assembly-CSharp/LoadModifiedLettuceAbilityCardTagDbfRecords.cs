using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadModifiedLettuceAbilityCardTagDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<ModifiedLettuceAbilityCardTagDbfRecord> GetRecords()
	{
		ModifiedLettuceAbilityCardTagDbfAsset dbfAsset = assetBundleRequest.asset as ModifiedLettuceAbilityCardTagDbfAsset;
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

	public LoadModifiedLettuceAbilityCardTagDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(ModifiedLettuceAbilityCardTagDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
