using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadEventRewardTrackDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<EventRewardTrackDbfRecord> GetRecords()
	{
		EventRewardTrackDbfAsset dbfAsset = assetBundleRequest.asset as EventRewardTrackDbfAsset;
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

	public LoadEventRewardTrackDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(EventRewardTrackDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
