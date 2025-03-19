using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadMythicEquipmentScalingCardTagDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<MythicEquipmentScalingCardTagDbfRecord> GetRecords()
	{
		MythicEquipmentScalingCardTagDbfAsset dbfAsset = assetBundleRequest.asset as MythicEquipmentScalingCardTagDbfAsset;
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

	public LoadMythicEquipmentScalingCardTagDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(MythicEquipmentScalingCardTagDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
