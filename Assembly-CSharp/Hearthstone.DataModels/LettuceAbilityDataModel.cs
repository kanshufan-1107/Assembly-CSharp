using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceAbilityDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 215;

	private int m_AbilityId;

	private string m_AbilityName;

	private int m_CurrentTier;

	private int m_MaxTier;

	private DataModelList<LettuceAbilityTierDataModel> m_AbilityTiers = new DataModelList<LettuceAbilityTierDataModel>();

	private int m_UnlockLevel;

	private int m_ParentMercId;

	private TAG_ROLE m_AbilityRole;

	private bool m_ReadyForUpgrade;

	private string m_LockPlateText;

	private bool m_Owned;

	private bool m_IsEquipment;

	private bool m_IsEquipped;

	private bool m_IsAffectedBySlottedEquipment;

	private string m_UnlockAchievementDescription;

	private bool m_IsNew;

	private bool m_CanMythicScale;

	private int m_MythicModifier;

	private DataModelProperty[] m_properties = new DataModelProperty[18]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "ability_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "ability_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "current_tier",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 14,
			PropertyDisplayName = "max_tier",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "ability_tiers",
			Type = typeof(DataModelList<LettuceAbilityTierDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "unlock_level",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "parent_merc_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "ability_role",
			Type = typeof(TAG_ROLE)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "ready_for_upgrade",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "lock_plate_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "owned",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "is_equipment",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "is_equipped",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 12,
			PropertyDisplayName = "is_affected_by_slotted_equipment",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 13,
			PropertyDisplayName = "unlock_achievement_description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 652,
			PropertyDisplayName = "is_new",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 754,
			PropertyDisplayName = "can_mythic_scale",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 755,
			PropertyDisplayName = "mythic_modifier",
			Type = typeof(int)
		}
	};

	public int DataModelId => 215;

	public string DataModelDisplayName => "lettuce_ability";

	public int AbilityId
	{
		get
		{
			return m_AbilityId;
		}
		set
		{
			if (m_AbilityId != value)
			{
				m_AbilityId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string AbilityName
	{
		get
		{
			return m_AbilityName;
		}
		set
		{
			if (!(m_AbilityName == value))
			{
				m_AbilityName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CurrentTier
	{
		get
		{
			return m_CurrentTier;
		}
		set
		{
			if (m_CurrentTier != value)
			{
				m_CurrentTier = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MaxTier
	{
		get
		{
			return m_MaxTier;
		}
		set
		{
			if (m_MaxTier != value)
			{
				m_MaxTier = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<LettuceAbilityTierDataModel> AbilityTiers
	{
		get
		{
			return m_AbilityTiers;
		}
		set
		{
			if (m_AbilityTiers != value)
			{
				RemoveNestedDataModel(m_AbilityTiers);
				RegisterNestedDataModel(value);
				m_AbilityTiers = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int UnlockLevel
	{
		get
		{
			return m_UnlockLevel;
		}
		set
		{
			if (m_UnlockLevel != value)
			{
				m_UnlockLevel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int ParentMercId
	{
		get
		{
			return m_ParentMercId;
		}
		set
		{
			if (m_ParentMercId != value)
			{
				m_ParentMercId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TAG_ROLE AbilityRole
	{
		get
		{
			return m_AbilityRole;
		}
		set
		{
			if (m_AbilityRole != value)
			{
				m_AbilityRole = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ReadyForUpgrade
	{
		get
		{
			return m_ReadyForUpgrade;
		}
		set
		{
			if (m_ReadyForUpgrade != value)
			{
				m_ReadyForUpgrade = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string LockPlateText
	{
		get
		{
			return m_LockPlateText;
		}
		set
		{
			if (!(m_LockPlateText == value))
			{
				m_LockPlateText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Owned
	{
		get
		{
			return m_Owned;
		}
		set
		{
			if (m_Owned != value)
			{
				m_Owned = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsEquipment
	{
		get
		{
			return m_IsEquipment;
		}
		set
		{
			if (m_IsEquipment != value)
			{
				m_IsEquipment = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsEquipped
	{
		get
		{
			return m_IsEquipped;
		}
		set
		{
			if (m_IsEquipped != value)
			{
				m_IsEquipped = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsAffectedBySlottedEquipment
	{
		get
		{
			return m_IsAffectedBySlottedEquipment;
		}
		set
		{
			if (m_IsAffectedBySlottedEquipment != value)
			{
				m_IsAffectedBySlottedEquipment = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string UnlockAchievementDescription
	{
		get
		{
			return m_UnlockAchievementDescription;
		}
		set
		{
			if (!(m_UnlockAchievementDescription == value))
			{
				m_UnlockAchievementDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsNew
	{
		get
		{
			return m_IsNew;
		}
		set
		{
			if (m_IsNew != value)
			{
				m_IsNew = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool CanMythicScale
	{
		get
		{
			return m_CanMythicScale;
		}
		set
		{
			if (m_CanMythicScale != value)
			{
				m_CanMythicScale = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MythicModifier
	{
		get
		{
			return m_MythicModifier;
		}
		set
		{
			if (m_MythicModifier != value)
			{
				m_MythicModifier = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceAbilityDataModel()
	{
		RegisterNestedDataModel(m_AbilityTiers);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_AbilityId;
		int num2 = ((num + m_AbilityId.GetHashCode()) * 31 + ((m_AbilityName != null) ? m_AbilityName.GetHashCode() : 0)) * 31;
		_ = m_CurrentTier;
		int num3 = (num2 + m_CurrentTier.GetHashCode()) * 31;
		_ = m_MaxTier;
		int num4 = ((num3 + m_MaxTier.GetHashCode()) * 31 + ((m_AbilityTiers != null) ? m_AbilityTiers.GetPropertiesHashCode() : 0)) * 31;
		_ = m_UnlockLevel;
		int num5 = (num4 + m_UnlockLevel.GetHashCode()) * 31;
		_ = m_ParentMercId;
		int num6 = (num5 + m_ParentMercId.GetHashCode()) * 31;
		_ = m_AbilityRole;
		int num7 = (num6 + m_AbilityRole.GetHashCode()) * 31;
		_ = m_ReadyForUpgrade;
		int num8 = ((num7 + m_ReadyForUpgrade.GetHashCode()) * 31 + ((m_LockPlateText != null) ? m_LockPlateText.GetHashCode() : 0)) * 31;
		_ = m_Owned;
		int num9 = (num8 + m_Owned.GetHashCode()) * 31;
		_ = m_IsEquipment;
		int num10 = (num9 + m_IsEquipment.GetHashCode()) * 31;
		_ = m_IsEquipped;
		int num11 = (num10 + m_IsEquipped.GetHashCode()) * 31;
		_ = m_IsAffectedBySlottedEquipment;
		int num12 = ((num11 + m_IsAffectedBySlottedEquipment.GetHashCode()) * 31 + ((m_UnlockAchievementDescription != null) ? m_UnlockAchievementDescription.GetHashCode() : 0)) * 31;
		_ = m_IsNew;
		int num13 = (num12 + m_IsNew.GetHashCode()) * 31;
		_ = m_CanMythicScale;
		int num14 = (num13 + m_CanMythicScale.GetHashCode()) * 31;
		_ = m_MythicModifier;
		return num14 + m_MythicModifier.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_AbilityId;
			return true;
		case 1:
			value = m_AbilityName;
			return true;
		case 2:
			value = m_CurrentTier;
			return true;
		case 14:
			value = m_MaxTier;
			return true;
		case 3:
			value = m_AbilityTiers;
			return true;
		case 4:
			value = m_UnlockLevel;
			return true;
		case 5:
			value = m_ParentMercId;
			return true;
		case 6:
			value = m_AbilityRole;
			return true;
		case 7:
			value = m_ReadyForUpgrade;
			return true;
		case 8:
			value = m_LockPlateText;
			return true;
		case 9:
			value = m_Owned;
			return true;
		case 10:
			value = m_IsEquipment;
			return true;
		case 11:
			value = m_IsEquipped;
			return true;
		case 12:
			value = m_IsAffectedBySlottedEquipment;
			return true;
		case 13:
			value = m_UnlockAchievementDescription;
			return true;
		case 652:
			value = m_IsNew;
			return true;
		case 754:
			value = m_CanMythicScale;
			return true;
		case 755:
			value = m_MythicModifier;
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
		case 0:
			AbilityId = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			AbilityName = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			CurrentTier = ((value != null) ? ((int)value) : 0);
			return true;
		case 14:
			MaxTier = ((value != null) ? ((int)value) : 0);
			return true;
		case 3:
			AbilityTiers = ((value != null) ? ((DataModelList<LettuceAbilityTierDataModel>)value) : null);
			return true;
		case 4:
			UnlockLevel = ((value != null) ? ((int)value) : 0);
			return true;
		case 5:
			ParentMercId = ((value != null) ? ((int)value) : 0);
			return true;
		case 6:
			AbilityRole = ((value != null) ? ((TAG_ROLE)value) : TAG_ROLE.INVALID);
			return true;
		case 7:
			ReadyForUpgrade = value != null && (bool)value;
			return true;
		case 8:
			LockPlateText = ((value != null) ? ((string)value) : null);
			return true;
		case 9:
			Owned = value != null && (bool)value;
			return true;
		case 10:
			IsEquipment = value != null && (bool)value;
			return true;
		case 11:
			IsEquipped = value != null && (bool)value;
			return true;
		case 12:
			IsAffectedBySlottedEquipment = value != null && (bool)value;
			return true;
		case 13:
			UnlockAchievementDescription = ((value != null) ? ((string)value) : null);
			return true;
		case 652:
			IsNew = value != null && (bool)value;
			return true;
		case 754:
			CanMythicScale = value != null && (bool)value;
			return true;
		case 755:
			MythicModifier = ((value != null) ? ((int)value) : 0);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 0:
			info = Properties[0];
			return true;
		case 1:
			info = Properties[1];
			return true;
		case 2:
			info = Properties[2];
			return true;
		case 14:
			info = Properties[3];
			return true;
		case 3:
			info = Properties[4];
			return true;
		case 4:
			info = Properties[5];
			return true;
		case 5:
			info = Properties[6];
			return true;
		case 6:
			info = Properties[7];
			return true;
		case 7:
			info = Properties[8];
			return true;
		case 8:
			info = Properties[9];
			return true;
		case 9:
			info = Properties[10];
			return true;
		case 10:
			info = Properties[11];
			return true;
		case 11:
			info = Properties[12];
			return true;
		case 12:
			info = Properties[13];
			return true;
		case 13:
			info = Properties[14];
			return true;
		case 652:
			info = Properties[15];
			return true;
		case 754:
			info = Properties[16];
			return true;
		case 755:
			info = Properties[17];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
