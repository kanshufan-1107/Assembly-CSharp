using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class PriceDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 29;

	private float m_Amount;

	private CurrencyType m_Currency;

	private string m_DisplayText;

	private float m_OriginalAmount;

	private string m_OriginalDisplayText;

	private bool m_OnSale;

	private string m_OverrideSkuDisplayText;

	private DataModelProperty[] m_properties = new DataModelProperty[7]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "amount",
			Type = typeof(float)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "currency",
			Type = typeof(CurrencyType)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "display_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 810,
			PropertyDisplayName = "original_amount",
			Type = typeof(float)
		},
		new DataModelProperty
		{
			PropertyId = 811,
			PropertyDisplayName = "original_display_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 812,
			PropertyDisplayName = "on_sale",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 827,
			PropertyDisplayName = "override_sku_display_text",
			Type = typeof(string)
		}
	};

	public int DataModelId => 29;

	public string DataModelDisplayName => "price";

	public float Amount
	{
		get
		{
			return m_Amount;
		}
		set
		{
			if (m_Amount != value)
			{
				m_Amount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CurrencyType Currency
	{
		get
		{
			return m_Currency;
		}
		set
		{
			if (m_Currency != value)
			{
				m_Currency = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DisplayText
	{
		get
		{
			return m_DisplayText;
		}
		set
		{
			if (!(m_DisplayText == value))
			{
				m_DisplayText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public float OriginalAmount
	{
		get
		{
			return m_OriginalAmount;
		}
		set
		{
			if (m_OriginalAmount != value)
			{
				m_OriginalAmount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string OriginalDisplayText
	{
		get
		{
			return m_OriginalDisplayText;
		}
		set
		{
			if (!(m_OriginalDisplayText == value))
			{
				m_OriginalDisplayText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool OnSale
	{
		get
		{
			return m_OnSale;
		}
		set
		{
			if (m_OnSale != value)
			{
				m_OnSale = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string OverrideSkuDisplayText
	{
		get
		{
			return m_OverrideSkuDisplayText;
		}
		set
		{
			if (!(m_OverrideSkuDisplayText == value))
			{
				m_OverrideSkuDisplayText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_Amount;
		int num2 = (num + m_Amount.GetHashCode()) * 31;
		_ = m_Currency;
		int num3 = ((num2 + m_Currency.GetHashCode()) * 31 + ((m_DisplayText != null) ? m_DisplayText.GetHashCode() : 0)) * 31;
		_ = m_OriginalAmount;
		int num4 = ((num3 + m_OriginalAmount.GetHashCode()) * 31 + ((m_OriginalDisplayText != null) ? m_OriginalDisplayText.GetHashCode() : 0)) * 31;
		_ = m_OnSale;
		return (num4 + m_OnSale.GetHashCode()) * 31 + ((m_OverrideSkuDisplayText != null) ? m_OverrideSkuDisplayText.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Amount;
			return true;
		case 1:
			value = m_Currency;
			return true;
		case 2:
			value = m_DisplayText;
			return true;
		case 810:
			value = m_OriginalAmount;
			return true;
		case 811:
			value = m_OriginalDisplayText;
			return true;
		case 812:
			value = m_OnSale;
			return true;
		case 827:
			value = m_OverrideSkuDisplayText;
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
			Amount = ((value != null) ? ((float)value) : 0f);
			return true;
		case 1:
			Currency = ((value != null) ? ((CurrencyType)value) : CurrencyType.NONE);
			return true;
		case 2:
			DisplayText = ((value != null) ? ((string)value) : null);
			return true;
		case 810:
			OriginalAmount = ((value != null) ? ((float)value) : 0f);
			return true;
		case 811:
			OriginalDisplayText = ((value != null) ? ((string)value) : null);
			return true;
		case 812:
			OnSale = value != null && (bool)value;
			return true;
		case 827:
			OverrideSkuDisplayText = ((value != null) ? ((string)value) : null);
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
		case 810:
			info = Properties[3];
			return true;
		case 811:
			info = Properties[4];
			return true;
		case 812:
			info = Properties[5];
			return true;
		case 827:
			info = Properties[6];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
