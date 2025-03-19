using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class AccountLicenseDbfRecord : DbfRecord
{
	[SerializeField]
	private long m_licenseId;

	[DbfField("LICENSE_ID")]
	public long LicenseId => m_licenseId;

	public override object GetVar(string name)
	{
		if (!(name == "ID"))
		{
			if (name == "LICENSE_ID")
			{
				return m_licenseId;
			}
			return null;
		}
		return base.ID;
	}

	public override void SetVar(string name, object val)
	{
		if (!(name == "ID"))
		{
			if (name == "LICENSE_ID")
			{
				m_licenseId = (long)val;
			}
		}
		else
		{
			SetID((int)val);
		}
	}

	public override Type GetVarType(string name)
	{
		if (!(name == "ID"))
		{
			if (name == "LICENSE_ID")
			{
				return typeof(long);
			}
			return null;
		}
		return typeof(int);
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadAccountLicenseDbfRecords loadRecords = new LoadAccountLicenseDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		AccountLicenseDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(AccountLicenseDbfAsset)) as AccountLicenseDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"AccountLicenseDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
