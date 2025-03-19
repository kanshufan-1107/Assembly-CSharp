using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class ExternalUrlDbfRecord : DbfRecord
{
	[SerializeField]
	private string m_noteDesc;

	[SerializeField]
	private ExternalUrl.AssetFlags m_assetFlags;

	[SerializeField]
	private ExternalUrl.Endpoint m_endpoint;

	[SerializeField]
	private string m_globalUrl = "False";

	[DbfField("ASSET_FLAGS")]
	public ExternalUrl.AssetFlags AssetFlags => m_assetFlags;

	[DbfField("ENDPOINT")]
	public ExternalUrl.Endpoint Endpoint => m_endpoint;

	[DbfField("GLOBAL_URL")]
	public string GlobalUrl => m_globalUrl;

	public List<RegionOverridesDbfRecord> RegionOverrides
	{
		get
		{
			int id = base.ID;
			List<RegionOverridesDbfRecord> returnRecords = new List<RegionOverridesDbfRecord>();
			List<RegionOverridesDbfRecord> records = GameDbf.RegionOverrides.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				RegionOverridesDbfRecord record = records[i];
				if (record.ExternalUrlId == id)
				{
					returnRecords.Add(record);
				}
			}
			return returnRecords;
		}
	}

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"NOTE_DESC" => m_noteDesc, 
			"ASSET_FLAGS" => m_assetFlags, 
			"ENDPOINT" => m_endpoint, 
			"GLOBAL_URL" => m_globalUrl, 
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
		case "NOTE_DESC":
			m_noteDesc = (string)val;
			break;
		case "ASSET_FLAGS":
			if (val == null)
			{
				m_assetFlags = ExternalUrl.AssetFlags.NONE;
			}
			else if (val is ExternalUrl.AssetFlags || val is int)
			{
				m_assetFlags = (ExternalUrl.AssetFlags)val;
			}
			else if (val is string)
			{
				m_assetFlags = ExternalUrl.ParseAssetFlagsValue((string)val);
			}
			break;
		case "ENDPOINT":
			if (val == null)
			{
				m_endpoint = ExternalUrl.Endpoint.ACCOUNT;
			}
			else if (val is ExternalUrl.Endpoint || val is int)
			{
				m_endpoint = (ExternalUrl.Endpoint)val;
			}
			else if (val is string)
			{
				m_endpoint = ExternalUrl.ParseEndpointValue((string)val);
			}
			break;
		case "GLOBAL_URL":
			m_globalUrl = (string)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NOTE_DESC" => typeof(string), 
			"ASSET_FLAGS" => typeof(ExternalUrl.AssetFlags), 
			"ENDPOINT" => typeof(ExternalUrl.Endpoint), 
			"GLOBAL_URL" => typeof(string), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadExternalUrlDbfRecords loadRecords = new LoadExternalUrlDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		ExternalUrlDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(ExternalUrlDbfAsset)) as ExternalUrlDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"ExternalUrlDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
