using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadCharacterDialogDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<CharacterDialogDbfRecord> GetRecords()
	{
		CharacterDialogDbfAsset dbfAsset = assetBundleRequest.asset as CharacterDialogDbfAsset;
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

	public LoadCharacterDialogDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(CharacterDialogDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
