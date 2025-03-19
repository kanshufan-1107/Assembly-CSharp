using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadRepeatableTaskListDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<RepeatableTaskListDbfRecord> GetRecords()
	{
		RepeatableTaskListDbfAsset dbfAsset = assetBundleRequest.asset as RepeatableTaskListDbfAsset;
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

	public LoadRepeatableTaskListDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(RepeatableTaskListDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
