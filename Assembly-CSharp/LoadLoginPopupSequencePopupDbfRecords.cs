using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadLoginPopupSequencePopupDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<LoginPopupSequencePopupDbfRecord> GetRecords()
	{
		LoginPopupSequencePopupDbfAsset dbfAsset = assetBundleRequest.asset as LoginPopupSequencePopupDbfAsset;
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

	public LoadLoginPopupSequencePopupDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(LoginPopupSequencePopupDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
