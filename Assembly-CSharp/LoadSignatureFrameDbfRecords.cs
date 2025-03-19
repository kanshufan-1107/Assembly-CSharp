using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadSignatureFrameDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<SignatureFrameDbfRecord> GetRecords()
	{
		SignatureFrameDbfAsset dbfAsset = assetBundleRequest.asset as SignatureFrameDbfAsset;
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

	public LoadSignatureFrameDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(SignatureFrameDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
