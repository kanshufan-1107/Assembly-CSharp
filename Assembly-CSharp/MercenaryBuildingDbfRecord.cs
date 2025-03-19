using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class MercenaryBuildingDbfRecord : DbfRecord
{
	[SerializeField]
	private DbfLocValue m_name;

	[SerializeField]
	private DbfLocValue m_description;

	[SerializeField]
	private MercenaryBuilding.Mercenarybuildingtype m_mercenaryBuildingType = MercenaryBuilding.ParseMercenarybuildingtypeValue("Invalid");

	[SerializeField]
	private int m_defaultTierId;

	[DbfField("NAME")]
	public DbfLocValue Name => m_name;

	[DbfField("DESCRIPTION")]
	public DbfLocValue Description => m_description;

	[DbfField("MERCENARY_BUILDING_TYPE")]
	public MercenaryBuilding.Mercenarybuildingtype MercenaryBuildingType => m_mercenaryBuildingType;

	[DbfField("DEFAULT_TIER")]
	public int DefaultTier => m_defaultTierId;

	public List<BuildingTierDbfRecord> MercenaryBuildingTiers
	{
		get
		{
			int id = base.ID;
			List<BuildingTierDbfRecord> returnRecords = new List<BuildingTierDbfRecord>();
			List<BuildingTierDbfRecord> records = GameDbf.BuildingTier.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				BuildingTierDbfRecord record = records[i];
				if (record.MercenaryBuildingId == id)
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
			"NAME" => m_name, 
			"DESCRIPTION" => m_description, 
			"MERCENARY_BUILDING_TYPE" => m_mercenaryBuildingType, 
			"DEFAULT_TIER" => m_defaultTierId, 
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
		case "NAME":
			m_name = (DbfLocValue)val;
			break;
		case "DESCRIPTION":
			m_description = (DbfLocValue)val;
			break;
		case "MERCENARY_BUILDING_TYPE":
			if (val == null)
			{
				m_mercenaryBuildingType = MercenaryBuilding.Mercenarybuildingtype.VILLAGE;
			}
			else if (val is MercenaryBuilding.Mercenarybuildingtype || val is int)
			{
				m_mercenaryBuildingType = (MercenaryBuilding.Mercenarybuildingtype)val;
			}
			else if (val is string)
			{
				m_mercenaryBuildingType = MercenaryBuilding.ParseMercenarybuildingtypeValue((string)val);
			}
			break;
		case "DEFAULT_TIER":
			m_defaultTierId = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"NAME" => typeof(DbfLocValue), 
			"DESCRIPTION" => typeof(DbfLocValue), 
			"MERCENARY_BUILDING_TYPE" => typeof(MercenaryBuilding.Mercenarybuildingtype), 
			"DEFAULT_TIER" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadMercenaryBuildingDbfRecords loadRecords = new LoadMercenaryBuildingDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		MercenaryBuildingDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(MercenaryBuildingDbfAsset)) as MercenaryBuildingDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"MercenaryBuildingDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
		m_name.StripUnusedLocales();
		m_description.StripUnusedLocales();
	}
}
