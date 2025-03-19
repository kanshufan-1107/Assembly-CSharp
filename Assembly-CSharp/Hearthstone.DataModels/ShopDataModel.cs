using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ShopDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 24;

	private bool m_IsWild;

	private DataModelList<ShopPageDataModel> m_Pages = new DataModelList<ShopPageDataModel>();

	private ShopTabDataModel m_CurrentTab;

	private ShopTabDataModel m_CurrentSubTab;

	private bool m_ShowSubTabBar;

	private ProductDataModel m_VirtualCurrency;

	private ProductDataModel m_BoosterCurrency;

	private bool m_AutoconvertCurrency;

	private PriceDataModel m_VirtualCurrencyBalance;

	private PriceDataModel m_BoosterCurrencyBalance;

	private PriceDataModel m_GoldBalance;

	private PriceDataModel m_DustBalance;

	private int m_TavernTicketBalance;

	private bool m_DebugShowProductIds;

	private DataModelList<string> m_TabsWithUndisplayedProducts = new DataModelList<string>();

	private PriceDataModel m_RenownBalance;

	private PriceDataModel m_BattlegroundsTokenBalance;

	private bool m_AllowRealMoneyPurchases;

	private string m_DisableRealMoneyPurchaseMessage;

	private PriceDataModel m_TavernTicketCurrencyBalance;

	private DataModelProperty[] m_properties = new DataModelProperty[20]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "is_wild",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 846,
			PropertyDisplayName = "pages",
			Type = typeof(DataModelList<ShopPageDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 855,
			PropertyDisplayName = "current_tab",
			Type = typeof(ShopTabDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 856,
			PropertyDisplayName = "current_sub_tab",
			Type = typeof(ShopTabDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 886,
			PropertyDisplayName = "show_sub_tab_bar",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "virtual_currency",
			Type = typeof(ProductDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "booster_currency",
			Type = typeof(ProductDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "autoconvert_currency",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "virtual_currency_balance",
			Type = typeof(PriceDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "booster_currency_balance",
			Type = typeof(PriceDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "gold_balance",
			Type = typeof(PriceDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "dust_balance",
			Type = typeof(PriceDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "tavern_ticket_balance",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 12,
			PropertyDisplayName = "debug_show_product_ids",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 13,
			PropertyDisplayName = "tabs_with_undisplayed_products",
			Type = typeof(DataModelList<string>)
		},
		new DataModelProperty
		{
			PropertyId = 685,
			PropertyDisplayName = "renown_balance",
			Type = typeof(PriceDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 686,
			PropertyDisplayName = "bg_token_balance",
			Type = typeof(PriceDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 910,
			PropertyDisplayName = "allow_real_money_purchases",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 911,
			PropertyDisplayName = "disable_real_money_purchase_message",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1143,
			PropertyDisplayName = "tavern_ticket_currency_balance",
			Type = typeof(PriceDataModel)
		}
	};

	public int DataModelId => 24;

	public string DataModelDisplayName => "shop";

	public bool IsWild
	{
		get
		{
			return m_IsWild;
		}
		set
		{
			if (m_IsWild != value)
			{
				m_IsWild = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<ShopPageDataModel> Pages
	{
		get
		{
			return m_Pages;
		}
		set
		{
			if (m_Pages != value)
			{
				RemoveNestedDataModel(m_Pages);
				RegisterNestedDataModel(value);
				m_Pages = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public ShopTabDataModel CurrentTab
	{
		get
		{
			return m_CurrentTab;
		}
		set
		{
			if (m_CurrentTab != value)
			{
				RemoveNestedDataModel(m_CurrentTab);
				RegisterNestedDataModel(value);
				m_CurrentTab = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public ShopTabDataModel CurrentSubTab
	{
		get
		{
			return m_CurrentSubTab;
		}
		set
		{
			if (m_CurrentSubTab != value)
			{
				RemoveNestedDataModel(m_CurrentSubTab);
				RegisterNestedDataModel(value);
				m_CurrentSubTab = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShowSubTabBar
	{
		get
		{
			return m_ShowSubTabBar;
		}
		set
		{
			if (m_ShowSubTabBar != value)
			{
				m_ShowSubTabBar = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public ProductDataModel VirtualCurrency
	{
		get
		{
			return m_VirtualCurrency;
		}
		set
		{
			if (m_VirtualCurrency != value)
			{
				RemoveNestedDataModel(m_VirtualCurrency);
				RegisterNestedDataModel(value);
				m_VirtualCurrency = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public ProductDataModel BoosterCurrency
	{
		get
		{
			return m_BoosterCurrency;
		}
		set
		{
			if (m_BoosterCurrency != value)
			{
				RemoveNestedDataModel(m_BoosterCurrency);
				RegisterNestedDataModel(value);
				m_BoosterCurrency = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool AutoconvertCurrency
	{
		get
		{
			return m_AutoconvertCurrency;
		}
		set
		{
			if (m_AutoconvertCurrency != value)
			{
				m_AutoconvertCurrency = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public PriceDataModel VirtualCurrencyBalance
	{
		get
		{
			return m_VirtualCurrencyBalance;
		}
		set
		{
			if (m_VirtualCurrencyBalance != value)
			{
				RemoveNestedDataModel(m_VirtualCurrencyBalance);
				RegisterNestedDataModel(value);
				m_VirtualCurrencyBalance = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public PriceDataModel BoosterCurrencyBalance
	{
		get
		{
			return m_BoosterCurrencyBalance;
		}
		set
		{
			if (m_BoosterCurrencyBalance != value)
			{
				RemoveNestedDataModel(m_BoosterCurrencyBalance);
				RegisterNestedDataModel(value);
				m_BoosterCurrencyBalance = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public PriceDataModel GoldBalance
	{
		get
		{
			return m_GoldBalance;
		}
		set
		{
			if (m_GoldBalance != value)
			{
				RemoveNestedDataModel(m_GoldBalance);
				RegisterNestedDataModel(value);
				m_GoldBalance = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public PriceDataModel DustBalance
	{
		get
		{
			return m_DustBalance;
		}
		set
		{
			if (m_DustBalance != value)
			{
				RemoveNestedDataModel(m_DustBalance);
				RegisterNestedDataModel(value);
				m_DustBalance = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TavernTicketBalance
	{
		get
		{
			return m_TavernTicketBalance;
		}
		set
		{
			if (m_TavernTicketBalance != value)
			{
				m_TavernTicketBalance = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool DebugShowProductIds
	{
		get
		{
			return m_DebugShowProductIds;
		}
		set
		{
			if (m_DebugShowProductIds != value)
			{
				m_DebugShowProductIds = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<string> TabsWithUndisplayedProducts
	{
		get
		{
			return m_TabsWithUndisplayedProducts;
		}
		set
		{
			if (m_TabsWithUndisplayedProducts != value)
			{
				RemoveNestedDataModel(m_TabsWithUndisplayedProducts);
				RegisterNestedDataModel(value);
				m_TabsWithUndisplayedProducts = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public PriceDataModel RenownBalance
	{
		get
		{
			return m_RenownBalance;
		}
		set
		{
			if (m_RenownBalance != value)
			{
				RemoveNestedDataModel(m_RenownBalance);
				RegisterNestedDataModel(value);
				m_RenownBalance = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public PriceDataModel BattlegroundsTokenBalance
	{
		get
		{
			return m_BattlegroundsTokenBalance;
		}
		set
		{
			if (m_BattlegroundsTokenBalance != value)
			{
				RemoveNestedDataModel(m_BattlegroundsTokenBalance);
				RegisterNestedDataModel(value);
				m_BattlegroundsTokenBalance = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool AllowRealMoneyPurchases
	{
		get
		{
			return m_AllowRealMoneyPurchases;
		}
		set
		{
			if (m_AllowRealMoneyPurchases != value)
			{
				m_AllowRealMoneyPurchases = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DisableRealMoneyPurchaseMessage
	{
		get
		{
			return m_DisableRealMoneyPurchaseMessage;
		}
		set
		{
			if (!(m_DisableRealMoneyPurchaseMessage == value))
			{
				m_DisableRealMoneyPurchaseMessage = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public PriceDataModel TavernTicketCurrencyBalance
	{
		get
		{
			return m_TavernTicketCurrencyBalance;
		}
		set
		{
			if (m_TavernTicketCurrencyBalance != value)
			{
				RemoveNestedDataModel(m_TavernTicketCurrencyBalance);
				RegisterNestedDataModel(value);
				m_TavernTicketCurrencyBalance = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public ShopDataModel()
	{
		RegisterNestedDataModel(m_Pages);
		RegisterNestedDataModel(m_CurrentTab);
		RegisterNestedDataModel(m_CurrentSubTab);
		RegisterNestedDataModel(m_VirtualCurrency);
		RegisterNestedDataModel(m_BoosterCurrency);
		RegisterNestedDataModel(m_VirtualCurrencyBalance);
		RegisterNestedDataModel(m_BoosterCurrencyBalance);
		RegisterNestedDataModel(m_GoldBalance);
		RegisterNestedDataModel(m_DustBalance);
		RegisterNestedDataModel(m_TabsWithUndisplayedProducts);
		RegisterNestedDataModel(m_RenownBalance);
		RegisterNestedDataModel(m_BattlegroundsTokenBalance);
		RegisterNestedDataModel(m_TavernTicketCurrencyBalance);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		int num = hash * 31;
		_ = m_IsWild;
		hash = num + m_IsWild.GetHashCode();
		if (m_Pages != null && !inspectedDataModels.Contains(m_Pages.GetHashCode()))
		{
			inspectedDataModels.Add(m_Pages.GetHashCode());
			hash = hash * 31 + m_Pages.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_CurrentTab != null && !inspectedDataModels.Contains(m_CurrentTab.GetHashCode()))
		{
			inspectedDataModels.Add(m_CurrentTab.GetHashCode());
			hash = hash * 31 + m_CurrentTab.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_CurrentSubTab != null && !inspectedDataModels.Contains(m_CurrentSubTab.GetHashCode()))
		{
			inspectedDataModels.Add(m_CurrentSubTab.GetHashCode());
			hash = hash * 31 + m_CurrentSubTab.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num2 = hash * 31;
		_ = m_ShowSubTabBar;
		hash = num2 + m_ShowSubTabBar.GetHashCode();
		if (m_VirtualCurrency != null && !inspectedDataModels.Contains(m_VirtualCurrency.GetHashCode()))
		{
			inspectedDataModels.Add(m_VirtualCurrency.GetHashCode());
			hash = hash * 31 + m_VirtualCurrency.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_BoosterCurrency != null && !inspectedDataModels.Contains(m_BoosterCurrency.GetHashCode()))
		{
			inspectedDataModels.Add(m_BoosterCurrency.GetHashCode());
			hash = hash * 31 + m_BoosterCurrency.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num3 = hash * 31;
		_ = m_AutoconvertCurrency;
		hash = num3 + m_AutoconvertCurrency.GetHashCode();
		if (m_VirtualCurrencyBalance != null && !inspectedDataModels.Contains(m_VirtualCurrencyBalance.GetHashCode()))
		{
			inspectedDataModels.Add(m_VirtualCurrencyBalance.GetHashCode());
			hash = hash * 31 + m_VirtualCurrencyBalance.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_BoosterCurrencyBalance != null && !inspectedDataModels.Contains(m_BoosterCurrencyBalance.GetHashCode()))
		{
			inspectedDataModels.Add(m_BoosterCurrencyBalance.GetHashCode());
			hash = hash * 31 + m_BoosterCurrencyBalance.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_GoldBalance != null && !inspectedDataModels.Contains(m_GoldBalance.GetHashCode()))
		{
			inspectedDataModels.Add(m_GoldBalance.GetHashCode());
			hash = hash * 31 + m_GoldBalance.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_DustBalance != null && !inspectedDataModels.Contains(m_DustBalance.GetHashCode()))
		{
			inspectedDataModels.Add(m_DustBalance.GetHashCode());
			hash = hash * 31 + m_DustBalance.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num4 = hash * 31;
		_ = m_TavernTicketBalance;
		hash = num4 + m_TavernTicketBalance.GetHashCode();
		int num5 = hash * 31;
		_ = m_DebugShowProductIds;
		hash = num5 + m_DebugShowProductIds.GetHashCode();
		if (m_TabsWithUndisplayedProducts != null && !inspectedDataModels.Contains(m_TabsWithUndisplayedProducts.GetHashCode()))
		{
			inspectedDataModels.Add(m_TabsWithUndisplayedProducts.GetHashCode());
			hash = hash * 31 + m_TabsWithUndisplayedProducts.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_RenownBalance != null && !inspectedDataModels.Contains(m_RenownBalance.GetHashCode()))
		{
			inspectedDataModels.Add(m_RenownBalance.GetHashCode());
			hash = hash * 31 + m_RenownBalance.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_BattlegroundsTokenBalance != null && !inspectedDataModels.Contains(m_BattlegroundsTokenBalance.GetHashCode()))
		{
			inspectedDataModels.Add(m_BattlegroundsTokenBalance.GetHashCode());
			hash = hash * 31 + m_BattlegroundsTokenBalance.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num6 = hash * 31;
		_ = m_AllowRealMoneyPurchases;
		hash = num6 + m_AllowRealMoneyPurchases.GetHashCode();
		hash = hash * 31 + ((m_DisableRealMoneyPurchaseMessage != null) ? m_DisableRealMoneyPurchaseMessage.GetHashCode() : 0);
		if (m_TavernTicketCurrencyBalance != null && !inspectedDataModels.Contains(m_TavernTicketCurrencyBalance.GetHashCode()))
		{
			inspectedDataModels.Add(m_TavernTicketCurrencyBalance.GetHashCode());
			return hash * 31 + m_TavernTicketCurrencyBalance.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_IsWild;
			return true;
		case 846:
			value = m_Pages;
			return true;
		case 855:
			value = m_CurrentTab;
			return true;
		case 856:
			value = m_CurrentSubTab;
			return true;
		case 886:
			value = m_ShowSubTabBar;
			return true;
		case 3:
			value = m_VirtualCurrency;
			return true;
		case 4:
			value = m_BoosterCurrency;
			return true;
		case 5:
			value = m_AutoconvertCurrency;
			return true;
		case 6:
			value = m_VirtualCurrencyBalance;
			return true;
		case 7:
			value = m_BoosterCurrencyBalance;
			return true;
		case 8:
			value = m_GoldBalance;
			return true;
		case 9:
			value = m_DustBalance;
			return true;
		case 11:
			value = m_TavernTicketBalance;
			return true;
		case 12:
			value = m_DebugShowProductIds;
			return true;
		case 13:
			value = m_TabsWithUndisplayedProducts;
			return true;
		case 685:
			value = m_RenownBalance;
			return true;
		case 686:
			value = m_BattlegroundsTokenBalance;
			return true;
		case 910:
			value = m_AllowRealMoneyPurchases;
			return true;
		case 911:
			value = m_DisableRealMoneyPurchaseMessage;
			return true;
		case 1143:
			value = m_TavernTicketCurrencyBalance;
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
			IsWild = value != null && (bool)value;
			return true;
		case 846:
			Pages = ((value != null) ? ((DataModelList<ShopPageDataModel>)value) : null);
			return true;
		case 855:
			CurrentTab = ((value != null) ? ((ShopTabDataModel)value) : null);
			return true;
		case 856:
			CurrentSubTab = ((value != null) ? ((ShopTabDataModel)value) : null);
			return true;
		case 886:
			ShowSubTabBar = value != null && (bool)value;
			return true;
		case 3:
			VirtualCurrency = ((value != null) ? ((ProductDataModel)value) : null);
			return true;
		case 4:
			BoosterCurrency = ((value != null) ? ((ProductDataModel)value) : null);
			return true;
		case 5:
			AutoconvertCurrency = value != null && (bool)value;
			return true;
		case 6:
			VirtualCurrencyBalance = ((value != null) ? ((PriceDataModel)value) : null);
			return true;
		case 7:
			BoosterCurrencyBalance = ((value != null) ? ((PriceDataModel)value) : null);
			return true;
		case 8:
			GoldBalance = ((value != null) ? ((PriceDataModel)value) : null);
			return true;
		case 9:
			DustBalance = ((value != null) ? ((PriceDataModel)value) : null);
			return true;
		case 11:
			TavernTicketBalance = ((value != null) ? ((int)value) : 0);
			return true;
		case 12:
			DebugShowProductIds = value != null && (bool)value;
			return true;
		case 13:
			TabsWithUndisplayedProducts = ((value != null) ? ((DataModelList<string>)value) : null);
			return true;
		case 685:
			RenownBalance = ((value != null) ? ((PriceDataModel)value) : null);
			return true;
		case 686:
			BattlegroundsTokenBalance = ((value != null) ? ((PriceDataModel)value) : null);
			return true;
		case 910:
			AllowRealMoneyPurchases = value != null && (bool)value;
			return true;
		case 911:
			DisableRealMoneyPurchaseMessage = ((value != null) ? ((string)value) : null);
			return true;
		case 1143:
			TavernTicketCurrencyBalance = ((value != null) ? ((PriceDataModel)value) : null);
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
		case 846:
			info = Properties[1];
			return true;
		case 855:
			info = Properties[2];
			return true;
		case 856:
			info = Properties[3];
			return true;
		case 886:
			info = Properties[4];
			return true;
		case 3:
			info = Properties[5];
			return true;
		case 4:
			info = Properties[6];
			return true;
		case 5:
			info = Properties[7];
			return true;
		case 6:
			info = Properties[8];
			return true;
		case 7:
			info = Properties[9];
			return true;
		case 8:
			info = Properties[10];
			return true;
		case 9:
			info = Properties[11];
			return true;
		case 11:
			info = Properties[12];
			return true;
		case 12:
			info = Properties[13];
			return true;
		case 13:
			info = Properties[14];
			return true;
		case 685:
			info = Properties[15];
			return true;
		case 686:
			info = Properties[16];
			return true;
		case 910:
			info = Properties[17];
			return true;
		case 911:
			info = Properties[18];
			return true;
		case 1143:
			info = Properties[19];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
