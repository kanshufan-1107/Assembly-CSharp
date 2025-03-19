using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ProfileGameModeStatDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 214;

	private string m_ModeName;

	private int m_ModeIcon;

	private string m_StatName;

	private DataModelList<int> m_StatValue = new DataModelList<int>();

	private string m_StatDesc;

	private DataModelList<string> m_StatValueDesc = new DataModelList<string>();

	private DataModelProperty[] m_properties = new DataModelProperty[6]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "mode_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "mode_icon",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "stat_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "stat_value",
			Type = typeof(DataModelList<int>)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "stat_desc",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "stat_value_desc",
			Type = typeof(DataModelList<string>)
		}
	};

	public int DataModelId => 214;

	public string DataModelDisplayName => "profile_game_mode_stat";

	public string ModeName
	{
		get
		{
			return m_ModeName;
		}
		set
		{
			if (!(m_ModeName == value))
			{
				m_ModeName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int ModeIcon
	{
		get
		{
			return m_ModeIcon;
		}
		set
		{
			if (m_ModeIcon != value)
			{
				m_ModeIcon = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string StatName
	{
		get
		{
			return m_StatName;
		}
		set
		{
			if (!(m_StatName == value))
			{
				m_StatName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<int> StatValue
	{
		get
		{
			return m_StatValue;
		}
		set
		{
			if (m_StatValue != value)
			{
				RemoveNestedDataModel(m_StatValue);
				RegisterNestedDataModel(value);
				m_StatValue = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string StatDesc
	{
		get
		{
			return m_StatDesc;
		}
		set
		{
			if (!(m_StatDesc == value))
			{
				m_StatDesc = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<string> StatValueDesc
	{
		get
		{
			return m_StatValueDesc;
		}
		set
		{
			if (m_StatValueDesc != value)
			{
				RemoveNestedDataModel(m_StatValueDesc);
				RegisterNestedDataModel(value);
				m_StatValueDesc = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public ProfileGameModeStatDataModel()
	{
		RegisterNestedDataModel(m_StatValue);
		RegisterNestedDataModel(m_StatValueDesc);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_ModeName != null) ? m_ModeName.GetHashCode() : 0);
		int num = hash * 31;
		_ = m_ModeIcon;
		hash = num + m_ModeIcon.GetHashCode();
		hash = hash * 31 + ((m_StatName != null) ? m_StatName.GetHashCode() : 0);
		if (m_StatValue != null && !inspectedDataModels.Contains(m_StatValue.GetHashCode()))
		{
			inspectedDataModels.Add(m_StatValue.GetHashCode());
			hash = hash * 31 + m_StatValue.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		hash = hash * 31 + ((m_StatDesc != null) ? m_StatDesc.GetHashCode() : 0);
		if (m_StatValueDesc != null && !inspectedDataModels.Contains(m_StatValueDesc.GetHashCode()))
		{
			inspectedDataModels.Add(m_StatValueDesc.GetHashCode());
			return hash * 31 + m_StatValueDesc.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_ModeName;
			return true;
		case 1:
			value = m_ModeIcon;
			return true;
		case 2:
			value = m_StatName;
			return true;
		case 3:
			value = m_StatValue;
			return true;
		case 4:
			value = m_StatDesc;
			return true;
		case 5:
			value = m_StatValueDesc;
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
			ModeName = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			ModeIcon = ((value != null) ? ((int)value) : 0);
			return true;
		case 2:
			StatName = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			StatValue = ((value != null) ? ((DataModelList<int>)value) : null);
			return true;
		case 4:
			StatDesc = ((value != null) ? ((string)value) : null);
			return true;
		case 5:
			StatValueDesc = ((value != null) ? ((DataModelList<string>)value) : null);
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
