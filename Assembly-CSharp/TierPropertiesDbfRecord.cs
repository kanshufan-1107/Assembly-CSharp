using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class TierPropertiesDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_buildingTierId;

	[SerializeField]
	private TierProperties.Buildingtierproperty m_tierPropertyType = TierProperties.ParseBuildingtierpropertyValue("Invalid");

	[SerializeField]
	private int m_tierPropertyValue;

	[DbfField("BUILDING_TIER_ID")]
	public int BuildingTierId => m_buildingTierId;

	[DbfField("TIER_PROPERTY_TYPE")]
	public TierProperties.Buildingtierproperty TierPropertyType => m_tierPropertyType;

	[DbfField("TIER_PROPERTY_VALUE")]
	public int TierPropertyValue => m_tierPropertyValue;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"BUILDING_TIER_ID" => m_buildingTierId, 
			"TIER_PROPERTY_TYPE" => m_tierPropertyType, 
			"TIER_PROPERTY_VALUE" => m_tierPropertyValue, 
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
		case "BUILDING_TIER_ID":
			m_buildingTierId = (int)val;
			break;
		case "TIER_PROPERTY_TYPE":
			if (val == null)
			{
				m_tierPropertyType = (TierProperties.Buildingtierproperty)0;
			}
			else if (val is TierProperties.Buildingtierproperty || val is int)
			{
				m_tierPropertyType = (TierProperties.Buildingtierproperty)val;
			}
			else if (val is string)
			{
				m_tierPropertyType = TierProperties.ParseBuildingtierpropertyValue((string)val);
			}
			break;
		case "TIER_PROPERTY_VALUE":
			m_tierPropertyValue = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"BUILDING_TIER_ID" => typeof(int), 
			"TIER_PROPERTY_TYPE" => typeof(TierProperties.Buildingtierproperty), 
			"TIER_PROPERTY_VALUE" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadTierPropertiesDbfRecords loadRecords = new LoadTierPropertiesDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		TierPropertiesDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(TierPropertiesDbfAsset)) as TierPropertiesDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"TierPropertiesDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
