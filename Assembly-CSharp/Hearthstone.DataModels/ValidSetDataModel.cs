using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ValidSetDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 825;

	private string m_SetNameLeft;

	private string m_SetIconNameLeft;

	private string m_SetNameRight;

	private string m_SetIconNameRight;

	private DataModelProperty[] m_properties = new DataModelProperty[4]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "set_name_left",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "set_icon_name_Left",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "set_name_right",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "set_icon_name_right",
			Type = typeof(string)
		}
	};

	public int DataModelId => 825;

	public string DataModelDisplayName => "valid_set";

	public string SetNameLeft
	{
		get
		{
			return m_SetNameLeft;
		}
		set
		{
			if (!(m_SetNameLeft == value))
			{
				m_SetNameLeft = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string SetIconNameLeft
	{
		get
		{
			return m_SetIconNameLeft;
		}
		set
		{
			if (!(m_SetIconNameLeft == value))
			{
				m_SetIconNameLeft = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string SetNameRight
	{
		get
		{
			return m_SetNameRight;
		}
		set
		{
			if (!(m_SetNameRight == value))
			{
				m_SetNameRight = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string SetIconNameRight
	{
		get
		{
			return m_SetIconNameRight;
		}
		set
		{
			if (!(m_SetIconNameRight == value))
			{
				m_SetIconNameRight = value;
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
		return (((17 * 31 + ((m_SetNameLeft != null) ? m_SetNameLeft.GetHashCode() : 0)) * 31 + ((m_SetIconNameLeft != null) ? m_SetIconNameLeft.GetHashCode() : 0)) * 31 + ((m_SetNameRight != null) ? m_SetNameRight.GetHashCode() : 0)) * 31 + ((m_SetIconNameRight != null) ? m_SetIconNameRight.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_SetNameLeft;
			return true;
		case 1:
			value = m_SetIconNameLeft;
			return true;
		case 2:
			value = m_SetNameRight;
			return true;
		case 3:
			value = m_SetIconNameRight;
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
			SetNameLeft = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			SetIconNameLeft = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			SetNameRight = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			SetIconNameRight = ((value != null) ? ((string)value) : null);
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
