using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceMercenaryLevelUpDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 303;

	private int m_MercenaryId;

	private int m_AttackIncrease;

	private int m_NewAttackValue;

	private int m_HealthIncrease;

	private int m_NewHealthValue;

	private bool m_NewIsMaxLevel;

	private DataModelProperty[] m_properties = new DataModelProperty[6]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "mercenary_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "attack_increase",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "new_attack_value",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "health_increase",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "new_health_value",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "new_is_max_level",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 303;

	public string DataModelDisplayName => "lettuce_mercenary_level_up";

	public int MercenaryId
	{
		get
		{
			return m_MercenaryId;
		}
		set
		{
			if (m_MercenaryId != value)
			{
				m_MercenaryId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int AttackIncrease
	{
		get
		{
			return m_AttackIncrease;
		}
		set
		{
			if (m_AttackIncrease != value)
			{
				m_AttackIncrease = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NewAttackValue
	{
		get
		{
			return m_NewAttackValue;
		}
		set
		{
			if (m_NewAttackValue != value)
			{
				m_NewAttackValue = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int HealthIncrease
	{
		get
		{
			return m_HealthIncrease;
		}
		set
		{
			if (m_HealthIncrease != value)
			{
				m_HealthIncrease = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NewHealthValue
	{
		get
		{
			return m_NewHealthValue;
		}
		set
		{
			if (m_NewHealthValue != value)
			{
				m_NewHealthValue = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool NewIsMaxLevel
	{
		get
		{
			return m_NewIsMaxLevel;
		}
		set
		{
			if (m_NewIsMaxLevel != value)
			{
				m_NewIsMaxLevel = value;
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
		_ = m_MercenaryId;
		int num2 = (num + m_MercenaryId.GetHashCode()) * 31;
		_ = m_AttackIncrease;
		int num3 = (num2 + m_AttackIncrease.GetHashCode()) * 31;
		_ = m_NewAttackValue;
		int num4 = (num3 + m_NewAttackValue.GetHashCode()) * 31;
		_ = m_HealthIncrease;
		int num5 = (num4 + m_HealthIncrease.GetHashCode()) * 31;
		_ = m_NewHealthValue;
		int num6 = (num5 + m_NewHealthValue.GetHashCode()) * 31;
		_ = m_NewIsMaxLevel;
		return num6 + m_NewIsMaxLevel.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_MercenaryId;
			return true;
		case 1:
			value = m_AttackIncrease;
			return true;
		case 2:
			value = m_NewAttackValue;
			return true;
		case 3:
			value = m_HealthIncrease;
			return true;
		case 4:
			value = m_NewHealthValue;
			return true;
		case 5:
			value = m_NewIsMaxLevel;
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
			MercenaryId = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			AttackIncrease = ((value != null) ? ((int)value) : 0);
			return true;
		case 2:
			NewAttackValue = ((value != null) ? ((int)value) : 0);
			return true;
		case 3:
			HealthIncrease = ((value != null) ? ((int)value) : 0);
			return true;
		case 4:
			NewHealthValue = ((value != null) ? ((int)value) : 0);
			return true;
		case 5:
			NewIsMaxLevel = value != null && (bool)value;
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
		case 5:
			info = Properties[5];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
