using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadMercenaryVillageTriggerDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<MercenaryVillageTriggerDbfRecord> GetRecords()
	{
		MercenaryVillageTriggerDbfAsset dbfAsset = assetBundleRequest.asset as MercenaryVillageTriggerDbfAsset;
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

	public LoadMercenaryVillageTriggerDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(MercenaryVillageTriggerDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
