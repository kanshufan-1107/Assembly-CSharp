using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadSignatureCardDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<SignatureCardDbfRecord> GetRecords()
	{
		SignatureCardDbfAsset dbfAsset = assetBundleRequest.asset as SignatureCardDbfAsset;
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

	public LoadSignatureCardDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(SignatureCardDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
