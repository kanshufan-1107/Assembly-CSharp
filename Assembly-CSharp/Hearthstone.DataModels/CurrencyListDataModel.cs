using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class CurrencyListDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 818;

	private DataModelList<CurrencyType> m_VCCurrency = new DataModelList<CurrencyType>();

	private string m_ISOCode;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 819,
			PropertyDisplayName = "vc_currency",
			Type = typeof(DataModelList<CurrencyType>)
		},
		new DataModelProperty
		{
			PropertyId = 820,
			PropertyDisplayName = "iso_code",
			Type = typeof(string)
		}
	};

	public int DataModelId => 818;

	public string DataModelDisplayName => "currency_list";

	public DataModelList<CurrencyType> VCCurrency
	{
		get
		{
			return m_VCCurrency;
		}
		set
		{
			if (m_VCCurrency != value)
			{
				RemoveNestedDataModel(m_VCCurrency);
				RegisterNestedDataModel(value);
				m_VCCurrency = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ISOCode
	{
		get
		{
			return m_ISOCode;
		}
		set
		{
			if (!(m_ISOCode == value))
			{
				m_ISOCode = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public CurrencyListDataModel()
	{
		RegisterNestedDataModel(m_VCCurrency);
	}

	public int GetPropertiesHashCode()
	{
		return (17 * 31 + ((m_VCCurrency != null) ? m_VCCurrency.GetPropertiesHashCode() : 0)) * 31 + ((m_ISOCode != null) ? m_ISOCode.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 819:
			value = m_VCCurrency;
			return true;
		case 820:
			value = m_ISOCode;
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
		case 819:
			VCCurrency = ((value != null) ? ((DataModelList<CurrencyType>)value) : null);
			return true;
		case 820:
			ISOCode = ((value != null) ? ((string)value) : null);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 819:
			info = Properties[0];
			return true;
		case 820:
			info = Properties[1];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
