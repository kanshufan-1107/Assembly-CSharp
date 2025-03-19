using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadOwnershipReqListDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<OwnershipReqListDbfRecord> GetRecords()
	{
		OwnershipReqListDbfAsset dbfAsset = assetBundleRequest.asset as OwnershipReqListDbfAsset;
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

	public LoadOwnershipReqListDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(OwnershipReqListDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
