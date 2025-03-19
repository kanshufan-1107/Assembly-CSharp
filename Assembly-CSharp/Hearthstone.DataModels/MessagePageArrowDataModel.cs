using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MessagePageArrowDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 453;

	private bool m_Active;

	private bool m_IsNew;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "active",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "is_new",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 453;

	public string DataModelDisplayName => "message_page_arrow";

	public bool Active
	{
		get
		{
			return m_Active;
		}
		set
		{
			if (m_Active != value)
			{
				m_Active = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsNew
	{
		get
		{
			return m_IsNew;
		}
		set
		{
			if (m_IsNew != value)
			{
				m_IsNew = value;
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
		_ = m_Active;
		int num2 = (num + m_Active.GetHashCode()) * 31;
		_ = m_IsNew;
		return num2 + m_IsNew.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Active;
			return true;
		case 1:
			value = m_IsNew;
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
			Active = value != null && (bool)value;
			return true;
		case 1:
			IsNew = value != null && (bool)value;
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
