using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class HiddenLicenseDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_accountLicenseId;

	[SerializeField]
	private bool m_isBlocking = true;

	[DbfField("ACCOUNT_LICENSE_ID")]
	public int AccountLicenseId => m_accountLicenseId;

	[DbfField("IS_BLOCKING")]
	public bool IsBlocking => m_isBlocking;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"ACCOUNT_LICENSE_ID" => m_accountLicenseId, 
			"IS_BLOCKING" => m_isBlocking, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "ID":
			SetID((int)val);
			break;
		case "ACCOUNT_LICENSE_ID":
			m_accountLicenseId = (int)val;
			break;
		case "IS_BLOCKING":
			m_isBlocking = (bool)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"ACCOUNT_LICENSE_ID" => typeof(int), 
			"IS_BLOCKING" => typeof(bool), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadHiddenLicenseDbfRecords loadRecords = new LoadHiddenLicenseDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		HiddenLicenseDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(HiddenLicenseDbfAsset)) as HiddenLicenseDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"HiddenLicenseDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
