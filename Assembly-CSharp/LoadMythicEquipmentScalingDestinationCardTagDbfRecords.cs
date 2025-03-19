using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadMythicEquipmentScalingDestinationCardTagDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<MythicEquipmentScalingDestinationCardTagDbfRecord> GetRecords()
	{
		MythicEquipmentScalingDestinationCardTagDbfAsset dbfAsset = assetBundleRequest.asset as MythicEquipmentScalingDestinationCardTagDbfAsset;
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

	public LoadMythicEquipmentScalingDestinationCardTagDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(MythicEquipmentScalingDestinationCardTagDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
