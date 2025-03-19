using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryMythicLevelSelectorDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 782;

	private int m_CurrentMythicLevel;

	private int m_MinMythicLevel;

	private int m_MaxMythicLevel;

	private DataModelProperty[] m_properties = new DataModelProperty[3]
	{
		new DataModelProperty
		{
			PropertyId = 783,
			PropertyDisplayName = "current_mythic_level",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 784,
			PropertyDisplayName = "min_mythic_level",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 785,
			PropertyDisplayName = "max_mythic_level",
			Type = typeof(int)
		}
	};

	public int DataModelId => 782;

	public string DataModelDisplayName => "mercenary_mythic_level_selector";

	public int CurrentMythicLevel
	{
		get
		{
			return m_CurrentMythicLevel;
		}
		set
		{
			if (m_CurrentMythicLevel != value)
			{
				m_CurrentMythicLevel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MinMythicLevel
	{
		get
		{
			return m_MinMythicLevel;
		}
		set
		{
			if (m_MinMythicLevel != value)
			{
				m_MinMythicLevel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MaxMythicLevel
	{
		get
		{
			return m_MaxMythicLevel;
		}
		set
		{
			if (m_MaxMythicLevel != value)
			{
				m_MaxMythicLevel = value;
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
		_ = m_CurrentMythicLevel;
		int num2 = (num + m_CurrentMythicLevel.GetHashCode()) * 31;
		_ = m_MinMythicLevel;
		int num3 = (num2 + m_MinMythicLevel.GetHashCode()) * 31;
		_ = m_MaxMythicLevel;
		return num3 + m_MaxMythicLevel.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 783:
			value = m_CurrentMythicLevel;
			return true;
		case 784:
			value = m_MinMythicLevel;
			return true;
		case 785:
			value = m_MaxMythicLevel;
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
		case 783:
			CurrentMythicLevel = ((value != null) ? ((int)value) : 0);
			return true;
		case 784:
			MinMythicLevel = ((value != null) ? ((int)value) : 0);
			return true;
		case 785:
			MaxMythicLevel = ((value != null) ? ((int)value) : 0);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 783:
			info = Properties[0];
			return true;
		case 784:
			info = Properties[1];
			return true;
		case 785:
			info = Properties[2];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
