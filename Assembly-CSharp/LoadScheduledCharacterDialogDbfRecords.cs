using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadScheduledCharacterDialogDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<ScheduledCharacterDialogDbfRecord> GetRecords()
	{
		ScheduledCharacterDialogDbfAsset dbfAsset = assetBundleRequest.asset as ScheduledCharacterDialogDbfAsset;
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

	public LoadScheduledCharacterDialogDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(ScheduledCharacterDialogDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
