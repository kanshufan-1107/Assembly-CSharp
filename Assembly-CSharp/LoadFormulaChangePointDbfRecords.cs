using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

public class LoadFormulaChangePointDbfRecords : IJobDependency, IAsyncJobResult
{
	private AssetBundleRequest assetBundleRequest;

	public List<FormulaChangePointDbfRecord> GetRecords()
	{
		FormulaChangePointDbfAsset dbfAsset = assetBundleRequest.asset as FormulaChangePointDbfAsset;
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

	public LoadFormulaChangePointDbfRecords(string resourcePath)
	{
		assetBundleRequest = DbfShared.GetAssetBundle().LoadAssetAsync(resourcePath, typeof(FormulaChangePointDbfAsset));
	}

	public bool IsReady()
	{
		return assetBundleRequest.isDone;
	}
}
