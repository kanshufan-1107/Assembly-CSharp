using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryVillageConvertValueDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 709;

	private int m_CoinsToConvert;

	private PriceDataModel m_RenownCurrency;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 710,
			PropertyDisplayName = "coins_to_convert",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 711,
			PropertyDisplayName = "renown_currency",
			Type = typeof(PriceDataModel)
		}
	};

	public int DataModelId => 709;

	public string DataModelDisplayName => "mercenary_village_convert_value";

	public int CoinsToConvert
	{
		get
		{
			return m_CoinsToConvert;
		}
		set
		{
			if (m_CoinsToConvert != value)
			{
				m_CoinsToConvert = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public PriceDataModel RenownCurrency
	{
		get
		{
			return m_RenownCurrency;
		}
		set
		{
			if (m_RenownCurrency != value)
			{
				RemoveNestedDataModel(m_RenownCurrency);
				RegisterNestedDataModel(value);
				m_RenownCurrency = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MercenaryVillageConvertValueDataModel()
	{
		RegisterNestedDataModel(m_RenownCurrency);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_CoinsToConvert;
		return (num + m_CoinsToConvert.GetHashCode()) * 31 + ((m_RenownCurrency != null) ? m_RenownCurrency.GetPropertiesHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 710:
			value = m_CoinsToConvert;
			return true;
		case 711:
			value = m_RenownCurrency;
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
		case 710:
			CoinsToConvert = ((value != null) ? ((int)value) : 0);
			return true;
		case 711:
			RenownCurrency = ((value != null) ? ((PriceDataModel)value) : null);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 710:
			info = Properties[0];
			return true;
		case 711:
			info = Properties[1];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
