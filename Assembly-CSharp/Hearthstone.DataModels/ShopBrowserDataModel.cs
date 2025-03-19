using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ShopBrowserDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 916;

	private bool m_ShowEmptyShopMessage;

	private string m_EmptyShopMessage;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 917,
			PropertyDisplayName = "show_empty_shop_message",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 918,
			PropertyDisplayName = "empty_shop_message",
			Type = typeof(string)
		}
	};

	public int DataModelId => 916;

	public string DataModelDisplayName => "shop_browser";

	public bool ShowEmptyShopMessage
	{
		get
		{
			return m_ShowEmptyShopMessage;
		}
		set
		{
			if (m_ShowEmptyShopMessage != value)
			{
				m_ShowEmptyShopMessage = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string EmptyShopMessage
	{
		get
		{
			return m_EmptyShopMessage;
		}
		set
		{
			if (!(m_EmptyShopMessage == value))
			{
				m_EmptyShopMessage = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_ShowEmptyShopMessage;
		return (num + m_ShowEmptyShopMessage.GetHashCode()) * 31 + ((m_EmptyShopMessage != null) ? m_EmptyShopMessage.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 917:
			value = m_ShowEmptyShopMessage;
			return true;
		case 918:
			value = m_EmptyShopMessage;
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
		case 917:
			ShowEmptyShopMessage = value != null && (bool)value;
			return true;
		case 918:
			EmptyShopMessage = ((value != null) ? ((string)value) : null);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 917:
			info = Properties[0];
			return true;
		case 918:
			info = Properties[1];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
