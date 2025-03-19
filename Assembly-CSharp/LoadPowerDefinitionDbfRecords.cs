using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadPowerDefinitionDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<PowerDefinitionDbfRecord> GetRecords()
	{
		PowerDefinitionDbfAsset dbfAsset = assetBundleRequest.asset as PowerDefinitionDbfAsset;
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

	public LoadPowerDefinitionDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(PowerDefinitionDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
