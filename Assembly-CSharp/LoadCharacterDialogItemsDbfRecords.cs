using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadCharacterDialogItemsDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<CharacterDialogItemsDbfRecord> GetRecords()
	{
		CharacterDialogItemsDbfAsset dbfAsset = assetBundleRequest.asset as CharacterDialogItemsDbfAsset;
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

	public LoadCharacterDialogItemsDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(CharacterDialogItemsDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
