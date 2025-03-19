using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class CardBackDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 26;

	private int m_CardBackId;

	private string m_Name;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "name",
			Type = typeof(string)
		}
	};

	public int DataModelId => 26;

	public string DataModelDisplayName => "cardback";

	public int CardBackId
	{
		get
		{
			return m_CardBackId;
		}
		set
		{
			if (m_CardBackId != value)
			{
				m_CardBackId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			if (!(m_Name == value))
			{
				m_Name = value;
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
		_ = m_CardBackId;
		return (num + m_CardBackId.GetHashCode()) * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_CardBackId;
			return true;
		case 1:
			value = m_Name;
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
			CardBackId = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			Name = ((value != null) ? ((string)value) : null);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
