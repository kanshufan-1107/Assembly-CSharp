using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class RatingsScreenDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 574;

	private string m_Url;

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "url",
			Type = typeof(string)
		}
	};

	public int DataModelId => 574;

	public string DataModelDisplayName => "ratings_screen";

	public string Url
	{
		get
		{
			return m_Url;
		}
		set
		{
			if (!(m_Url == value))
			{
				m_Url = value;
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
		return 17 * 31 + ((m_Url != null) ? m_Url.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 1)
		{
			value = m_Url;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 1)
		{
			Url = ((value != null) ? ((string)value) : null);
			return true;
		}
		return false;
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		if (id == 1)
		{
			info = Properties[0];
			return true;
		}
		info = default(DataModelProperty);
		return false;
	}
}
