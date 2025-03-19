using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadModifiedLettuceAbilityValueDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<ModifiedLettuceAbilityValueDbfRecord> GetRecords()
	{
		ModifiedLettuceAbilityValueDbfAsset dbfAsset = assetBundleRequest.asset as ModifiedLettuceAbilityValueDbfAsset;
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

	public LoadModifiedLettuceAbilityValueDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(ModifiedLettuceAbilityValueDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
