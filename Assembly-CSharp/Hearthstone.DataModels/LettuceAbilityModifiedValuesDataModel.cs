using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceAbilityModifiedValuesDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 598;

	private bool m_IsAttackChanging;

	private bool m_IsHealthChanging;

	private bool m_IsSpeedChanging;

	private bool m_IsCooldownChanging;

	private bool m_IsDescriptionChanging;

	private DataModelProperty[] m_properties = new DataModelProperty[5]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "is_attack_changing",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "is_health_changing",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "is_speed_changing",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "is_cooldown_changing",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "is_description_changing",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 598;

	public string DataModelDisplayName => "lettuce_ability_modified_values";

	public bool IsAttackChanging
	{
		get
		{
			return m_IsAttackChanging;
		}
		set
		{
			if (m_IsAttackChanging != value)
			{
				m_IsAttackChanging = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsHealthChanging
	{
		get
		{
			return m_IsHealthChanging;
		}
		set
		{
			if (m_IsHealthChanging != value)
			{
				m_IsHealthChanging = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsSpeedChanging
	{
		get
		{
			return m_IsSpeedChanging;
		}
		set
		{
			if (m_IsSpeedChanging != value)
			{
				m_IsSpeedChanging = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsCooldownChanging
	{
		get
		{
			return m_IsCooldownChanging;
		}
		set
		{
			if (m_IsCooldownChanging != value)
			{
				m_IsCooldownChanging = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsDescriptionChanging
	{
		get
		{
			return m_IsDescriptionChanging;
		}
		set
		{
			if (m_IsDescriptionChanging != value)
			{
				m_IsDescriptionChanging = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int num = 17 * 31;
		_ = m_IsAttackChanging;
		int num2 = (num + m_IsAttackChanging.GetHashCode()) * 31;
		_ = m_IsHealthChanging;
		int num3 = (num2 + m_IsHealthChanging.GetHashCode()) * 31;
		_ = m_IsSpeedChanging;
		int num4 = (num3 + m_IsSpeedChanging.GetHashCode()) * 31;
		_ = m_IsCooldownChanging;
		int num5 = (num4 + m_IsCooldownChanging.GetHashCode()) * 31;
		_ = m_IsDescriptionChanging;
		return num5 + m_IsDescriptionChanging.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_IsAttackChanging;
			return true;
		case 1:
			value = m_IsHealthChanging;
			return true;
		case 2:
			value = m_IsSpeedChanging;
			return true;
		case 3:
			value = m_IsCooldownChanging;
			return true;
		case 4:
			value = m_IsDescriptionChanging;
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
			IsAttackChanging = value != null && (bool)value;
			return true;
		case 1:
			IsHealthChanging = value != null && (bool)value;
			return true;
		case 2:
			IsSpeedChanging = value != null && (bool)value;
			return true;
		case 3:
			IsCooldownChanging = value != null && (bool)value;
			return true;
		case 4:
			IsDescriptionChanging = value != null && (bool)value;
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
