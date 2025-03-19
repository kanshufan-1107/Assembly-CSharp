using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ChangeMessageItemDisplayContentDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 357;

	private string m_ItemType;

	private string m_ItemId;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "item_type",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "item_id",
			Type = typeof(string)
		}
	};

	public int DataModelId => 357;

	public string DataModelDisplayName => "change_message_item_display_content";

	public string ItemType
	{
		get
		{
			return m_ItemType;
		}
		set
		{
			if (!(m_ItemType == value))
			{
				m_ItemType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ItemId
	{
		get
		{
			return m_ItemId;
		}
		set
		{
			if (!(m_ItemId == value))
			{
				m_ItemId = value;
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
		return (17 * 31 + ((m_ItemType != null) ? m_ItemType.GetHashCode() : 0)) * 31 + ((m_ItemId != null) ? m_ItemId.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_ItemType;
			return true;
		case 1:
			value = m_ItemId;
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
			ItemType = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			ItemId = ((value != null) ? ((string)value) : null);
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
