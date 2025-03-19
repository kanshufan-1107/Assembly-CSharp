using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class BuildingTierDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_mercenaryBuildingId;

	[SerializeField]
	private DbfLocValue m_name;

	[SerializeField]
	private DbfLocValue m_description;

	[SerializeField]
	private int m_unlockAchievementId;

	[SerializeField]
	private int m_onUpgradedDialogId;

	[SerializeField]
	private int m_upgradeCost;

	[SerializeField]
	private BuildingTier.VillageTutorialServerEvent m_tutorialEventType;

	[SerializeField]
	private int m_tutorialEventValue;

	[DbfField("MERCENARY_BUILDING_ID")]
	public int MercenaryBuildingId => m_mercenaryBuildingId;

	[DbfField("DESCRIPTION")]
	public DbfLocValue Description => m_description;

	[DbfField("UNLOCK_ACHIEVEMENT")]
	public int UnlockAchievement => m_unlockAchievementId;

	[DbfField("ON_UPGRADED_DIALOG")]
	public int OnUpgradedDialog => m_onUpgradedDialogId;

	[DbfField("UPGRADE_COST")]
	public int UpgradeCost => m_upgradeCost;

	[DbfField("TUTORIAL_EVENT_TYPE")]
	public BuildingTier.VillageTutorialServerEvent TutorialEventType => m_tutorialEventType;

	[DbfField("TUTORIAL_EVENT_VALUE")]
	public int TutorialEventValue => m_tutorialEventValue;

	public List<TierPropertiesDbfRecord> MercenaryBuildingTierProperties
	{
		get
		{
			int id = base.ID;
			List<TierPropertiesDbfRecord> returnRecords = new List<TierPropertiesDbfRecord>();
			List<TierPropertiesDbfRecord> records = GameDbf.TierProperties.GetRecords();
			int i = 0;
			for (int iMax = records.Count; i < iMax; i++)
			{
				TierPropertiesDbfRecord record = records[i];
				if (record.BuildingTierId == id)
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
			"MERCENARY_BUILDING_ID" => m_mercenaryBuildingId, 
			"NAME" => m_name, 
			"DESCRIPTION" => m_description, 
			"UNLOCK_ACHIEVEMENT" => m_unlockAchievementId, 
			"ON_UPGRADED_DIALOG" => m_onUpgradedDialogId, 
			"UPGRADE_COST" => m_upgradeCost, 
			"TUTORIAL_EVENT_TYPE" => m_tutorialEventType, 
			"TUTORIAL_EVENT_VALUE" => m_tutorialEventValue, 
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
		case "MERCENARY_BUILDING_ID":
			m_mercenaryBuildingId = (int)val;
			break;
		case "NAME":
			m_name = (DbfLocValue)val;
			break;
		case "DESCRIPTION":
			m_description = (DbfLocValue)val;
			break;
		case "UNLOCK_ACHIEVEMENT":
			m_unlockAchievementId = (int)val;
			break;
		case "ON_UPGRADED_DIALOG":
			m_onUpgradedDialogId = (int)val;
			break;
		case "UPGRADE_COST":
			m_upgradeCost = (int)val;
			break;
		case "TUTORIAL_EVENT_TYPE":
			if (val == null)
			{
				m_tutorialEventType = BuildingTier.VillageTutorialServerEvent.NONE;
			}
			else if (val is BuildingTier.VillageTutorialServerEvent || val is int)
			{
				m_tutorialEventType = (BuildingTier.VillageTutorialServerEvent)val;
			}
			else if (val is string)
			{
				m_tutorialEventType = BuildingTier.ParseVillageTutorialServerEventValue((string)val);
			}
			break;
		case "TUTORIAL_EVENT_VALUE":
			m_tutorialEventValue = (int)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"MERCENARY_BUILDING_ID" => typeof(int), 
			"NAME" => typeof(DbfLocValue), 
			"DESCRIPTION" => typeof(DbfLocValue), 
			"UNLOCK_ACHIEVEMENT" => typeof(int), 
			"ON_UPGRADED_DIALOG" => typeof(int), 
			"UPGRADE_COST" => typeof(int), 
			"TUTORIAL_EVENT_TYPE" => typeof(BuildingTier.VillageTutorialServerEvent), 
			"TUTORIAL_EVENT_VALUE" => typeof(int), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadBuildingTierDbfRecords loadRecords = new LoadBuildingTierDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		BuildingTierDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(BuildingTierDbfAsset)) as BuildingTierDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"BuildingTierDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
