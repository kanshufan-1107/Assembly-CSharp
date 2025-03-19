using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadSetRotationEventDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<SetRotationEventDbfRecord> GetRecords()
	{
		SetRotationEventDbfAsset dbfAsset = assetBundleRequest.asset as SetRotationEventDbfAsset;
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

	public LoadSetRotationEventDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(SetRotationEventDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
