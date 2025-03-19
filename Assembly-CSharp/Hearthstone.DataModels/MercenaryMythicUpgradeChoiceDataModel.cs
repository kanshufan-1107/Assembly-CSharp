using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryMythicUpgradeChoiceDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 766;

	private int m_MythicLevel;

	private int m_LevelCount;

	private int m_Cost;

	private CardDataModel m_UpgradeCard;

	private LettuceAbilityModifiedValuesDataModel m_NextTierAbilityChanges;

	private DataModelProperty[] m_properties = new DataModelProperty[5]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "mythic_level",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "Level_Count",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "cost",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "upgrade_card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "next_tier_ability_changes",
			Type = typeof(LettuceAbilityModifiedValuesDataModel)
		}
	};

	public int DataModelId => 766;

	public string DataModelDisplayName => "mercenary_mythic_upgrade_choice";

	public int MythicLevel
	{
		get
		{
			return m_MythicLevel;
		}
		set
		{
			if (m_MythicLevel != value)
			{
				m_MythicLevel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int LevelCount
	{
		get
		{
			return m_LevelCount;
		}
		set
		{
			if (m_LevelCount != value)
			{
				m_LevelCount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Cost
	{
		get
		{
			return m_Cost;
		}
		set
		{
			if (m_Cost != value)
			{
				m_Cost = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel UpgradeCard
	{
		get
		{
			return m_UpgradeCard;
		}
		set
		{
			if (m_UpgradeCard != value)
			{
				RemoveNestedDataModel(m_UpgradeCard);
				RegisterNestedDataModel(value);
				m_UpgradeCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceAbilityModifiedValuesDataModel NextTierAbilityChanges
	{
		get
		{
			return m_NextTierAbilityChanges;
		}
		set
		{
			if (m_NextTierAbilityChanges != value)
			{
				RemoveNestedDataModel(m_NextTierAbilityChanges);
				RegisterNestedDataModel(value);
				m_NextTierAbilityChanges = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MercenaryMythicUpgradeChoiceDataModel()
	{
		RegisterNestedDataModel(m_UpgradeCard);
		RegisterNestedDataModel(m_NextTierAbilityChanges);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_MythicLevel;
		int num2 = (num + m_MythicLevel.GetHashCode()) * 31;
		_ = m_LevelCount;
		int num3 = (num2 + m_LevelCount.GetHashCode()) * 31;
		_ = m_Cost;
		return ((num3 + m_Cost.GetHashCode()) * 31 + ((m_UpgradeCard != null) ? m_UpgradeCard.GetPropertiesHashCode() : 0)) * 31 + ((m_NextTierAbilityChanges != null) ? m_NextTierAbilityChanges.GetPropertiesHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_MythicLevel;
			return true;
		case 1:
			value = m_LevelCount;
			return true;
		case 2:
			value = m_Cost;
			return true;
		case 3:
			value = m_UpgradeCard;
			return true;
		case 4:
			value = m_NextTierAbilityChanges;
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
			MythicLevel = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			LevelCount = ((value != null) ? ((int)value) : 0);
			return true;
		case 2:
			Cost = ((value != null) ? ((int)value) : 0);
			return true;
		case 3:
			UpgradeCard = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 4:
			NextTierAbilityChanges = ((value != null) ? ((LettuceAbilityModifiedValuesDataModel)value) : null);
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
		case 3:
			info = Properties[3];
			return true;
		case 4:
			info = Properties[4];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
