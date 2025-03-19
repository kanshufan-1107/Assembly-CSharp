using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceAbilityUpgradeDisplayDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 280;

	private LettuceAbilityDataModel m_CurrentTierAbility;

	private LettuceAbilityDataModel m_NextTierAbility;

	private bool m_IsMinion;

	private LettuceAbilityModifiedValuesDataModel m_NextTierAbilityChanges;

	private DataModelProperty[] m_properties = new DataModelProperty[4]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "current_tier_ability",
			Type = typeof(LettuceAbilityDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "next_tier_ability",
			Type = typeof(LettuceAbilityDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "is_minion",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "next_tier_ability_changes",
			Type = typeof(LettuceAbilityModifiedValuesDataModel)
		}
	};

	public int DataModelId => 280;

	public string DataModelDisplayName => "lettuce_ability_upgrade_display";

	public LettuceAbilityDataModel CurrentTierAbility
	{
		get
		{
			return m_CurrentTierAbility;
		}
		set
		{
			if (m_CurrentTierAbility != value)
			{
				RemoveNestedDataModel(m_CurrentTierAbility);
				RegisterNestedDataModel(value);
				m_CurrentTierAbility = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceAbilityDataModel NextTierAbility
	{
		get
		{
			return m_NextTierAbility;
		}
		set
		{
			if (m_NextTierAbility != value)
			{
				RemoveNestedDataModel(m_NextTierAbility);
				RegisterNestedDataModel(value);
				m_NextTierAbility = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsMinion
	{
		get
		{
			return m_IsMinion;
		}
		set
		{
			if (m_IsMinion != value)
			{
				m_IsMinion = value;
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

	public LettuceAbilityUpgradeDisplayDataModel()
	{
		RegisterNestedDataModel(m_CurrentTierAbility);
		RegisterNestedDataModel(m_NextTierAbility);
		RegisterNestedDataModel(m_NextTierAbilityChanges);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_CurrentTierAbility != null && !inspectedDataModels.Contains(m_CurrentTierAbility.GetHashCode()))
		{
			inspectedDataModels.Add(m_CurrentTierAbility.GetHashCode());
			hash = hash * 31 + m_CurrentTierAbility.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_NextTierAbility != null && !inspectedDataModels.Contains(m_NextTierAbility.GetHashCode()))
		{
			inspectedDataModels.Add(m_NextTierAbility.GetHashCode());
			hash = hash * 31 + m_NextTierAbility.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_IsMinion;
		hash = num + m_IsMinion.GetHashCode();
		if (m_NextTierAbilityChanges != null && !inspectedDataModels.Contains(m_NextTierAbilityChanges.GetHashCode()))
		{
			inspectedDataModels.Add(m_NextTierAbilityChanges.GetHashCode());
			return hash * 31 + m_NextTierAbilityChanges.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_CurrentTierAbility;
			return true;
		case 1:
			value = m_NextTierAbility;
			return true;
		case 2:
			value = m_IsMinion;
			return true;
		case 3:
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
			CurrentTierAbility = ((value != null) ? ((LettuceAbilityDataModel)value) : null);
			return true;
		case 1:
			NextTierAbility = ((value != null) ? ((LettuceAbilityDataModel)value) : null);
			return true;
		case 2:
			IsMinion = value != null && (bool)value;
			return true;
		case 3:
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
