using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class ProductDbfRecord : DbfRecord
{
	public override object GetVar(string name)
	{
		if (name == "ID")
		{
			return base.ID;
		}
		return null;
	}

	public override void SetVar(string name, object val)
	{
		if (name == "ID")
		{
			SetID((int)val);
		}
	}

	public override Type GetVarType(string name)
	{
		if (name == "ID")
		{
			return typeof(int);
		}
		return null;
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadProductDbfRecords loadRecords = new LoadProductDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		ProductDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(ProductDbfAsset)) as ProductDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"ProductDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
			return false;
		}
		for (int i = 0; i < dbfAsset.Records.Count; i++)
		{
			dbfAsset.Records[i].StripUnusedLocales();
		}
		records = dbfAsset.Records as List<T>;
		return true;
	}

	public override bool SaveRecordsToAsset<T>(string assetPath, List<T> records)
	{
		return false;
	}

	public override void StripUnusedLocales()
	{
	}
}
