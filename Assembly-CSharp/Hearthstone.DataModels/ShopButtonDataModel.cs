using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ShopButtonDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 776;

	private bool m_IsShopOpen;

	private bool m_CanDisplayNewItemsBadge;

	private bool m_HasNewItems;

	private DataModelProperty[] m_properties = new DataModelProperty[3]
	{
		new DataModelProperty
		{
			PropertyId = 777,
			PropertyDisplayName = "is_shop_open",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 778,
			PropertyDisplayName = "can_display_new_items_badge",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 779,
			PropertyDisplayName = "has_new_items",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 776;

	public string DataModelDisplayName => "shop_button";

	public bool IsShopOpen
	{
		get
		{
			return m_IsShopOpen;
		}
		set
		{
			if (m_IsShopOpen != value)
			{
				m_IsShopOpen = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool CanDisplayNewItemsBadge
	{
		get
		{
			return m_CanDisplayNewItemsBadge;
		}
		set
		{
			if (m_CanDisplayNewItemsBadge != value)
			{
				m_CanDisplayNewItemsBadge = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasNewItems
	{
		get
		{
			return m_HasNewItems;
		}
		set
		{
			if (m_HasNewItems != value)
			{
				m_HasNewItems = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_IsShopOpen;
		int num2 = (num + m_IsShopOpen.GetHashCode()) * 31;
		_ = m_CanDisplayNewItemsBadge;
		int num3 = (num2 + m_CanDisplayNewItemsBadge.GetHashCode()) * 31;
		_ = m_HasNewItems;
		return num3 + m_HasNewItems.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 777:
			value = m_IsShopOpen;
			return true;
		case 778:
			value = m_CanDisplayNewItemsBadge;
			return true;
		case 779:
			value = m_HasNewItems;
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
		case 777:
			IsShopOpen = value != null && (bool)value;
			return true;
		case 778:
			CanDisplayNewItemsBadge = value != null && (bool)value;
			return true;
		case 779:
			HasNewItems = value != null && (bool)value;
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 777:
			info = Properties[0];
			return true;
		case 778:
			info = Properties[1];
			return true;
		case 779:
			info = Properties[2];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
