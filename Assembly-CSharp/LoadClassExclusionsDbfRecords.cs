using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadClassExclusionsDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<ClassExclusionsDbfRecord> GetRecords()
	{
		ClassExclusionsDbfAsset dbfAsset = assetBundleRequest.asset as ClassExclusionsDbfAsset;
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

	public LoadClassExclusionsDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(ClassExclusionsDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
