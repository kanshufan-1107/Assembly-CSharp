using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceBountyTeamSelectDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 518;

	private string m_HeaderText;

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "header_text",
			Type = typeof(string)
		}
	};

	public int DataModelId => 518;

	public string DataModelDisplayName => "lettuce_bounty_team_select";

	public string HeaderText
	{
		get
		{
			return m_HeaderText;
		}
		set
		{
			if (!(m_HeaderText == value))
			{
				m_HeaderText = value;
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
		return 17 * 31 + ((m_HeaderText != null) ? m_HeaderText.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_HeaderText;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			HeaderText = ((value != null) ? ((string)value) : null);
			return true;
		}
		return false;
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		if (id == 0)
		{
			info = Properties[0];
			return true;
		}
		info = default(DataModelProperty);
		return false;
	}
}
