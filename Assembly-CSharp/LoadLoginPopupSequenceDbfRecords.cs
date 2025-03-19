using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadLoginPopupSequenceDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<LoginPopupSequenceDbfRecord> GetRecords()
	{
		LoginPopupSequenceDbfAsset dbfAsset = assetBundleRequest.asset as LoginPopupSequenceDbfAsset;
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

	public LoadLoginPopupSequenceDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(LoginPopupSequenceDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
