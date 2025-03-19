using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryVillageRenownTradeDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 704;

	private int m_CurrentRenownBalance;

	private int m_CurrentConvertableRenownBalance;

	private int m_RareCoinRenownConversionRate;

	private int m_EpicCoinRenownConversionRate;

	private int m_LegendaryCoinRenownConversionRate;

	private int m_CoinConversionCount;

	private DataModelProperty[] m_properties = new DataModelProperty[6]
	{
		new DataModelProperty
		{
			PropertyId = 705,
			PropertyDisplayName = "current_renown_balance",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 712,
			PropertyDisplayName = "current_convertable_renown_balance",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 746,
			PropertyDisplayName = "rare_coin_conversion_rate",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 747,
			PropertyDisplayName = "epic_coin_conversion_rate",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 748,
			PropertyDisplayName = "legendary_coin_conversion_rate",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 745,
			PropertyDisplayName = "coin_conversion_count",
			Type = typeof(int)
		}
	};

	public int DataModelId => 704;

	public string DataModelDisplayName => "mercenary_village_renown_trade";

	public int CurrentRenownBalance
	{
		get
		{
			return m_CurrentRenownBalance;
		}
		set
		{
			if (m_CurrentRenownBalance != value)
			{
				m_CurrentRenownBalance = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CurrentConvertableRenownBalance
	{
		get
		{
			return m_CurrentConvertableRenownBalance;
		}
		set
		{
			if (m_CurrentConvertableRenownBalance != value)
			{
				m_CurrentConvertableRenownBalance = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int RareCoinRenownConversionRate
	{
		get
		{
			return m_RareCoinRenownConversionRate;
		}
		set
		{
			if (m_RareCoinRenownConversionRate != value)
			{
				m_RareCoinRenownConversionRate = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int EpicCoinRenownConversionRate
	{
		get
		{
			return m_EpicCoinRenownConversionRate;
		}
		set
		{
			if (m_EpicCoinRenownConversionRate != value)
			{
				m_EpicCoinRenownConversionRate = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int LegendaryCoinRenownConversionRate
	{
		get
		{
			return m_LegendaryCoinRenownConversionRate;
		}
		set
		{
			if (m_LegendaryCoinRenownConversionRate != value)
			{
				m_LegendaryCoinRenownConversionRate = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CoinConversionCount
	{
		get
		{
			return m_CoinConversionCount;
		}
		set
		{
			if (m_CoinConversionCount != value)
			{
				m_CoinConversionCount = value;
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
		_ = m_CurrentRenownBalance;
		int num2 = (num + m_CurrentRenownBalance.GetHashCode()) * 31;
		_ = m_CurrentConvertableRenownBalance;
		int num3 = (num2 + m_CurrentConvertableRenownBalance.GetHashCode()) * 31;
		_ = m_RareCoinRenownConversionRate;
		int num4 = (num3 + m_RareCoinRenownConversionRate.GetHashCode()) * 31;
		_ = m_EpicCoinRenownConversionRate;
		int num5 = (num4 + m_EpicCoinRenownConversionRate.GetHashCode()) * 31;
		_ = m_LegendaryCoinRenownConversionRate;
		int num6 = (num5 + m_LegendaryCoinRenownConversionRate.GetHashCode()) * 31;
		_ = m_CoinConversionCount;
		return num6 + m_CoinConversionCount.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 705:
			value = m_CurrentRenownBalance;
			return true;
		case 712:
			value = m_CurrentConvertableRenownBalance;
			return true;
		case 746:
			value = m_RareCoinRenownConversionRate;
			return true;
		case 747:
			value = m_EpicCoinRenownConversionRate;
			return true;
		case 748:
			value = m_LegendaryCoinRenownConversionRate;
			return true;
		case 745:
			value = m_CoinConversionCount;
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
		case 705:
			CurrentRenownBalance = ((value != null) ? ((int)value) : 0);
			return true;
		case 712:
			CurrentConvertableRenownBalance = ((value != null) ? ((int)value) : 0);
			return true;
		case 746:
			RareCoinRenownConversionRate = ((value != null) ? ((int)value) : 0);
			return true;
		case 747:
			EpicCoinRenownConversionRate = ((value != null) ? ((int)value) : 0);
			return true;
		case 748:
			LegendaryCoinRenownConversionRate = ((value != null) ? ((int)value) : 0);
			return true;
		case 745:
			CoinConversionCount = ((value != null) ? ((int)value) : 0);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 705:
			info = Properties[0];
			return true;
		case 712:
			info = Properties[1];
			return true;
		case 746:
			info = Properties[2];
			return true;
		case 747:
			info = Properties[3];
			return true;
		case 748:
			info = Properties[4];
			return true;
		case 745:
			info = Properties[5];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
