using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadGameSaveSubkeyDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<GameSaveSubkeyDbfRecord> GetRecords()
	{
		GameSaveSubkeyDbfAsset dbfAsset = assetBundleRequest.asset as GameSaveSubkeyDbfAsset;
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

	public LoadGameSaveSubkeyDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(GameSaveSubkeyDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
