using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ProductTierDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 23;

	private string m_Header;

	private DataModelList<string> m_Tags = new DataModelList<string>();

	private int m_LayoutWidth;

	private int m_LayoutHeight;

	private DataModelList<string> m_LayoutMap = new DataModelList<string>();

	private int m_MaxLayoutCount;

	private DataModelList<ShopBrowserButtonDataModel> m_BrowserButtons = new DataModelList<ShopBrowserButtonDataModel>();

	private CurrencyListDataModel m_CurrenciesAvailable;

	private bool m_DisplayDivder;

	private bool m_DisplayTierData;

	private DataModelProperty[] m_properties = new DataModelProperty[10]
	{
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "header",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "tags",
			Type = typeof(DataModelList<string>)
		},
		new DataModelProperty
		{
			PropertyId = 872,
			PropertyDisplayName = "layou_width",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 873,
			PropertyDisplayName = "layout_height",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 861,
			PropertyDisplayName = "layout_map",
			Type = typeof(DataModelList<string>)
		},
		new DataModelProperty
		{
			PropertyId = 878,
			PropertyDisplayName = "max_layout_count",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "browser_buttons",
			Type = typeof(DataModelList<ShopBrowserButtonDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 826,
			PropertyDisplayName = "currencies_available",
			Type = typeof(CurrencyListDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 893,
			PropertyDisplayName = "display_divider",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 906,
			PropertyDisplayName = "display_tier_data",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 23;

	public string DataModelDisplayName => "product_tier";

	public string Header
	{
		get
		{
			return m_Header;
		}
		set
		{
			if (!(m_Header == value))
			{
				m_Header = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<string> Tags
	{
		get
		{
			return m_Tags;
		}
		set
		{
			if (m_Tags != value)
			{
				RemoveNestedDataModel(m_Tags);
				RegisterNestedDataModel(value);
				m_Tags = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int LayoutWidth
	{
		get
		{
			return m_LayoutWidth;
		}
		set
		{
			if (m_LayoutWidth != value)
			{
				m_LayoutWidth = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int LayoutHeight
	{
		get
		{
			return m_LayoutHeight;
		}
		set
		{
			if (m_LayoutHeight != value)
			{
				m_LayoutHeight = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<string> LayoutMap
	{
		get
		{
			return m_LayoutMap;
		}
		set
		{
			if (m_LayoutMap != value)
			{
				RemoveNestedDataModel(m_LayoutMap);
				RegisterNestedDataModel(value);
				m_LayoutMap = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MaxLayoutCount
	{
		get
		{
			return m_MaxLayoutCount;
		}
		set
		{
			if (m_MaxLayoutCount != value)
			{
				m_MaxLayoutCount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<ShopBrowserButtonDataModel> BrowserButtons
	{
		get
		{
			return m_BrowserButtons;
		}
		set
		{
			if (m_BrowserButtons != value)
			{
				RemoveNestedDataModel(m_BrowserButtons);
				RegisterNestedDataModel(value);
				m_BrowserButtons = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CurrencyListDataModel CurrenciesAvailable
	{
		get
		{
			return m_CurrenciesAvailable;
		}
		set
		{
			if (m_CurrenciesAvailable != value)
			{
				RemoveNestedDataModel(m_CurrenciesAvailable);
				RegisterNestedDataModel(value);
				m_CurrenciesAvailable = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool DisplayDivder
	{
		get
		{
			return m_DisplayDivder;
		}
		set
		{
			if (m_DisplayDivder != value)
			{
				m_DisplayDivder = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool DisplayTierData
	{
		get
		{
			return m_DisplayTierData;
		}
		set
		{
			if (m_DisplayTierData != value)
			{
				m_DisplayTierData = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public ProductTierDataModel()
	{
		RegisterNestedDataModel(m_Tags);
		RegisterNestedDataModel(m_LayoutMap);
		RegisterNestedDataModel(m_BrowserButtons);
		RegisterNestedDataModel(m_CurrenciesAvailable);
	}

	public int GetPropertiesHashCode()
	{
		int num = ((17 * 31 + ((m_Header != null) ? m_Header.GetHashCode() : 0)) * 31 + ((m_Tags != null) ? m_Tags.GetPropertiesHashCode() : 0)) * 31;
		_ = m_LayoutWidth;
		int num2 = (num + m_LayoutWidth.GetHashCode()) * 31;
		_ = m_LayoutHeight;
		int num3 = ((num2 + m_LayoutHeight.GetHashCode()) * 31 + ((m_LayoutMap != null) ? m_LayoutMap.GetPropertiesHashCode() : 0)) * 31;
		_ = m_MaxLayoutCount;
		int num4 = (((num3 + m_MaxLayoutCount.GetHashCode()) * 31 + ((m_BrowserButtons != null) ? m_BrowserButtons.GetPropertiesHashCode() : 0)) * 31 + ((m_CurrenciesAvailable != null) ? m_CurrenciesAvailable.GetPropertiesHashCode() : 0)) * 31;
		_ = m_DisplayDivder;
		int num5 = (num4 + m_DisplayDivder.GetHashCode()) * 31;
		_ = m_DisplayTierData;
		return num5 + m_DisplayTierData.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 1:
			value = m_Header;
			return true;
		case 2:
			value = m_Tags;
			return true;
		case 872:
			value = m_LayoutWidth;
			return true;
		case 873:
			value = m_LayoutHeight;
			return true;
		case 861:
			value = m_LayoutMap;
			return true;
		case 878:
			value = m_MaxLayoutCount;
			return true;
		case 4:
			value = m_BrowserButtons;
			return true;
		case 826:
			value = m_CurrenciesAvailable;
			return true;
		case 893:
			value = m_DisplayDivder;
			return true;
		case 906:
			value = m_DisplayTierData;
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
		case 1:
			Header = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			Tags = ((value != null) ? ((DataModelList<string>)value) : null);
			return true;
		case 872:
			LayoutWidth = ((value != null) ? ((int)value) : 0);
			return true;
		case 873:
			LayoutHeight = ((value != null) ? ((int)value) : 0);
			return true;
		case 861:
			LayoutMap = ((value != null) ? ((DataModelList<string>)value) : null);
			return true;
		case 878:
			MaxLayoutCount = ((value != null) ? ((int)value) : 0);
			return true;
		case 4:
			BrowserButtons = ((value != null) ? ((DataModelList<ShopBrowserButtonDataModel>)value) : null);
			return true;
		case 826:
			CurrenciesAvailable = ((value != null) ? ((CurrencyListDataModel)value) : null);
			return true;
		case 893:
			DisplayDivder = value != null && (bool)value;
			return true;
		case 906:
			DisplayTierData = value != null && (bool)value;
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 1:
			info = Properties[0];
			return true;
		case 2:
			info = Properties[1];
			return true;
		case 872:
			info = Properties[2];
			return true;
		case 873:
			info = Properties[3];
			return true;
		case 861:
			info = Properties[4];
			return true;
		case 878:
			info = Properties[5];
			return true;
		case 4:
			info = Properties[6];
			return true;
		case 826:
			info = Properties[7];
			return true;
		case 893:
			info = Properties[8];
			return true;
		case 906:
			info = Properties[9];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
