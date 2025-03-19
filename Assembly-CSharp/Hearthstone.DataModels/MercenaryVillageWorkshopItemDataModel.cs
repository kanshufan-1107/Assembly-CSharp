using Assets;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryVillageWorkshopItemDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 432;

	private int m_BuildingID;

	private string m_Title;

	private string m_Description;

	private MercenaryBuilding.Mercenarybuildingtype m_BuildingType;

	private bool m_IsFullyUpgraded;

	private bool m_IsAchievementCompleted;

	private bool m_CanAffordUpgrade;

	private bool m_IsNewBuilding;

	private string m_TierDescription;

	private int m_CurrentTierId;

	private int m_NextTierId;

	private PriceDataModel m_Price;

	private string m_AchievementDescription;

	private int m_Progress;

	private int m_Quota;

	private bool m_Prewarm;

	private bool m_ShowEmptySlot;

	private DataModelProperty[] m_properties = new DataModelProperty[17]
	{
		new DataModelProperty
		{
			PropertyId = 433,
			PropertyDisplayName = "building_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 434,
			PropertyDisplayName = "title",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 435,
			PropertyDisplayName = "description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 436,
			PropertyDisplayName = "building_type",
			Type = typeof(MercenaryBuilding.Mercenarybuildingtype)
		},
		new DataModelProperty
		{
			PropertyId = 437,
			PropertyDisplayName = "is_fully_upgraded",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 438,
			PropertyDisplayName = "is_achievement_completed",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 439,
			PropertyDisplayName = "can_afford_upgrade",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 440,
			PropertyDisplayName = "is_new_building",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 441,
			PropertyDisplayName = "tier_description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 475,
			PropertyDisplayName = "current_tier_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 474,
			PropertyDisplayName = "next_tier_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 444,
			PropertyDisplayName = "price",
			Type = typeof(PriceDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 445,
			PropertyDisplayName = "achievement_description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 446,
			PropertyDisplayName = "progress",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 447,
			PropertyDisplayName = "quota",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 547,
			PropertyDisplayName = "prewarm",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 548,
			PropertyDisplayName = "show_empty_slot",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 432;

	public string DataModelDisplayName => "mercenary_village_workshop_item";

	public int BuildingID
	{
		get
		{
			return m_BuildingID;
		}
		set
		{
			if (m_BuildingID != value)
			{
				m_BuildingID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Title
	{
		get
		{
			return m_Title;
		}
		set
		{
			if (!(m_Title == value))
			{
				m_Title = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Description
	{
		get
		{
			return m_Description;
		}
		set
		{
			if (!(m_Description == value))
			{
				m_Description = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public MercenaryBuilding.Mercenarybuildingtype BuildingType
	{
		get
		{
			return m_BuildingType;
		}
		set
		{
			if (m_BuildingType != value)
			{
				m_BuildingType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsFullyUpgraded
	{
		get
		{
			return m_IsFullyUpgraded;
		}
		set
		{
			if (m_IsFullyUpgraded != value)
			{
				m_IsFullyUpgraded = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsAchievementCompleted
	{
		get
		{
			return m_IsAchievementCompleted;
		}
		set
		{
			if (m_IsAchievementCompleted != value)
			{
				m_IsAchievementCompleted = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool CanAffordUpgrade
	{
		get
		{
			return m_CanAffordUpgrade;
		}
		set
		{
			if (m_CanAffordUpgrade != value)
			{
				m_CanAffordUpgrade = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsNewBuilding
	{
		get
		{
			return m_IsNewBuilding;
		}
		set
		{
			if (m_IsNewBuilding != value)
			{
				m_IsNewBuilding = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string TierDescription
	{
		get
		{
			return m_TierDescription;
		}
		set
		{
			if (!(m_TierDescription == value))
			{
				m_TierDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CurrentTierId
	{
		get
		{
			return m_CurrentTierId;
		}
		set
		{
			if (m_CurrentTierId != value)
			{
				m_CurrentTierId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NextTierId
	{
		get
		{
			return m_NextTierId;
		}
		set
		{
			if (m_NextTierId != value)
			{
				m_NextTierId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public PriceDataModel Price
	{
		get
		{
			return m_Price;
		}
		set
		{
			if (m_Price != value)
			{
				RemoveNestedDataModel(m_Price);
				RegisterNestedDataModel(value);
				m_Price = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string AchievementDescription
	{
		get
		{
			return m_AchievementDescription;
		}
		set
		{
			if (!(m_AchievementDescription == value))
			{
				m_AchievementDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Progress
	{
		get
		{
			return m_Progress;
		}
		set
		{
			if (m_Progress != value)
			{
				m_Progress = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Quota
	{
		get
		{
			return m_Quota;
		}
		set
		{
			if (m_Quota != value)
			{
				m_Quota = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Prewarm
	{
		get
		{
			return m_Prewarm;
		}
		set
		{
			if (m_Prewarm != value)
			{
				m_Prewarm = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShowEmptySlot
	{
		get
		{
			return m_ShowEmptySlot;
		}
		set
		{
			if (m_ShowEmptySlot != value)
			{
				m_ShowEmptySlot = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MercenaryVillageWorkshopItemDataModel()
	{
		RegisterNestedDataModel(m_Price);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_BuildingID;
		int num2 = (((num + m_BuildingID.GetHashCode()) * 31 + ((m_Title != null) ? m_Title.GetHashCode() : 0)) * 31 + ((m_Description != null) ? m_Description.GetHashCode() : 0)) * 31;
		_ = m_BuildingType;
		int num3 = (num2 + m_BuildingType.GetHashCode()) * 31;
		_ = m_IsFullyUpgraded;
		int num4 = (num3 + m_IsFullyUpgraded.GetHashCode()) * 31;
		_ = m_IsAchievementCompleted;
		int num5 = (num4 + m_IsAchievementCompleted.GetHashCode()) * 31;
		_ = m_CanAffordUpgrade;
		int num6 = (num5 + m_CanAffordUpgrade.GetHashCode()) * 31;
		_ = m_IsNewBuilding;
		int num7 = ((num6 + m_IsNewBuilding.GetHashCode()) * 31 + ((m_TierDescription != null) ? m_TierDescription.GetHashCode() : 0)) * 31;
		_ = m_CurrentTierId;
		int num8 = (num7 + m_CurrentTierId.GetHashCode()) * 31;
		_ = m_NextTierId;
		int num9 = (((num8 + m_NextTierId.GetHashCode()) * 31 + ((m_Price != null) ? m_Price.GetPropertiesHashCode() : 0)) * 31 + ((m_AchievementDescription != null) ? m_AchievementDescription.GetHashCode() : 0)) * 31;
		_ = m_Progress;
		int num10 = (num9 + m_Progress.GetHashCode()) * 31;
		_ = m_Quota;
		int num11 = (num10 + m_Quota.GetHashCode()) * 31;
		_ = m_Prewarm;
		int num12 = (num11 + m_Prewarm.GetHashCode()) * 31;
		_ = m_ShowEmptySlot;
		return num12 + m_ShowEmptySlot.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 433:
			value = m_BuildingID;
			return true;
		case 434:
			value = m_Title;
			return true;
		case 435:
			value = m_Description;
			return true;
		case 436:
			value = m_BuildingType;
			return true;
		case 437:
			value = m_IsFullyUpgraded;
			return true;
		case 438:
			value = m_IsAchievementCompleted;
			return true;
		case 439:
			value = m_CanAffordUpgrade;
			return true;
		case 440:
			value = m_IsNewBuilding;
			return true;
		case 441:
			value = m_TierDescription;
			return true;
		case 475:
			value = m_CurrentTierId;
			return true;
		case 474:
			value = m_NextTierId;
			return true;
		case 444:
			value = m_Price;
			return true;
		case 445:
			value = m_AchievementDescription;
			return true;
		case 446:
			value = m_Progress;
			return true;
		case 447:
			value = m_Quota;
			return true;
		case 547:
			value = m_Prewarm;
			return true;
		case 548:
			value = m_ShowEmptySlot;
			return true;
		default:
			value = null;
			return false;
		}
	}

	public bool SetPropertyValue(int id, object value)
	{
		switch (id)
		{
		case 433:
			BuildingID = ((value != null) ? ((int)value) : 0);
			return true;
		case 434:
			Title = ((value != null) ? ((string)value) : null);
			return true;
		case 435:
			Description = ((value != null) ? ((string)value) : null);
			return true;
		case 436:
			BuildingType = ((value != null) ? ((MercenaryBuilding.Mercenarybuildingtype)value) : MercenaryBuilding.Mercenarybuildingtype.VILLAGE);
			return true;
		case 437:
			IsFullyUpgraded = value != null && (bool)value;
			return true;
		case 438:
			IsAchievementCompleted = value != null && (bool)value;
			return true;
		case 439:
			CanAffordUpgrade = value != null && (bool)value;
			return true;
		case 440:
			IsNewBuilding = value != null && (bool)value;
			return true;
		case 441:
			TierDescription = ((value != null) ? ((string)value) : null);
			return true;
		case 475:
			CurrentTierId = ((value != null) ? ((int)value) : 0);
			return true;
		case 474:
			NextTierId = ((value != null) ? ((int)value) : 0);
			return true;
		case 444:
			Price = ((value != null) ? ((PriceDataModel)value) : null);
			return true;
		case 445:
			AchievementDescription = ((value != null) ? ((string)value) : null);
			return true;
		case 446:
			Progress = ((value != null) ? ((int)value) : 0);
			return true;
		case 447:
			Quota = ((value != null) ? ((int)value) : 0);
			return true;
		case 547:
			Prewarm = value != null && (bool)value;
			return true;
		case 548:
			ShowEmptySlot = value != null && (bool)value;
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 433:
			info = Properties[0];
			return true;
		case 434:
			info = Properties[1];
			return true;
		case 435:
			info = Properties[2];
			return true;
		case 436:
			info = Properties[3];
			return true;
		case 437:
			info = Properties[4];
			return true;
		case 438:
			info = Properties[5];
			return true;
		case 439:
			info = Properties[6];
			return true;
		case 440:
			info = Properties[7];
			return true;
		case 441:
			info = Properties[8];
			return true;
		case 475:
			info = Properties[9];
			return true;
		case 474:
			info = Properties[10];
			return true;
		case 444:
			info = Properties[11];
			return true;
		case 445:
			info = Properties[12];
			return true;
		case 446:
			info = Properties[13];
			return true;
		case 447:
			info = Properties[14];
			return true;
		case 547:
			info = Properties[15];
			return true;
		case 548:
			info = Properties[16];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
