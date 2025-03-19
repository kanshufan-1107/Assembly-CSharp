using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class BaconLobbyDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 43;

	private int m_Rating;

	private int m_FirstPlaceFinishes;

	private int m_Top4Finishes;

	private bool m_ShopOpen;

	private bool m_BattlegroundsSkinsEnabled;

	private bool m_HasNewProducts;

	private bool m_HasNewSkins;

	private bool m_BattlegroundsRewardTrackEnabled;

	private LuckyDrawDataModel m_LuckyDraw;

	private bool m_BattlegroundsInDuosMode;

	private bool m_FullyLoaded;

	private bool m_HasNewRules;

	private DataModelProperty[] m_properties = new DataModelProperty[12]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "rating",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "first_place_finishes",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "top_4_finishes",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "shop_open",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "battlegrounds_skins_enabled",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "has_new_products",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "has_new_skins",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "battlegrounds_rewardtrack_enabled",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "lucky_draw",
			Type = typeof(LuckyDrawDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 12,
			PropertyDisplayName = "battlegrounds_in_duos_mode",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 13,
			PropertyDisplayName = "fully_loaded",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 14,
			PropertyDisplayName = "has_new_rules",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 43;

	public string DataModelDisplayName => "baconlobby";

	public int Rating
	{
		get
		{
			return m_Rating;
		}
		set
		{
			if (m_Rating != value)
			{
				m_Rating = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int FirstPlaceFinishes
	{
		get
		{
			return m_FirstPlaceFinishes;
		}
		set
		{
			if (m_FirstPlaceFinishes != value)
			{
				m_FirstPlaceFinishes = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Top4Finishes
	{
		get
		{
			return m_Top4Finishes;
		}
		set
		{
			if (m_Top4Finishes != value)
			{
				m_Top4Finishes = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShopOpen
	{
		get
		{
			return m_ShopOpen;
		}
		set
		{
			if (m_ShopOpen != value)
			{
				m_ShopOpen = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool BattlegroundsSkinsEnabled
	{
		get
		{
			return m_BattlegroundsSkinsEnabled;
		}
		set
		{
			if (m_BattlegroundsSkinsEnabled != value)
			{
				m_BattlegroundsSkinsEnabled = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasNewProducts
	{
		get
		{
			return m_HasNewProducts;
		}
		set
		{
			if (m_HasNewProducts != value)
			{
				m_HasNewProducts = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasNewSkins
	{
		get
		{
			return m_HasNewSkins;
		}
		set
		{
			if (m_HasNewSkins != value)
			{
				m_HasNewSkins = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool BattlegroundsRewardTrackEnabled
	{
		get
		{
			return m_BattlegroundsRewardTrackEnabled;
		}
		set
		{
			if (m_BattlegroundsRewardTrackEnabled != value)
			{
				m_BattlegroundsRewardTrackEnabled = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LuckyDrawDataModel LuckyDraw
	{
		get
		{
			return m_LuckyDraw;
		}
		set
		{
			if (m_LuckyDraw != value)
			{
				RemoveNestedDataModel(m_LuckyDraw);
				RegisterNestedDataModel(value);
				m_LuckyDraw = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool BattlegroundsInDuosMode
	{
		get
		{
			return m_BattlegroundsInDuosMode;
		}
		set
		{
			if (m_BattlegroundsInDuosMode != value)
			{
				m_BattlegroundsInDuosMode = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool FullyLoaded
	{
		get
		{
			return m_FullyLoaded;
		}
		set
		{
			if (m_FullyLoaded != value)
			{
				m_FullyLoaded = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasNewRules
	{
		get
		{
			return m_HasNewRules;
		}
		set
		{
			if (m_HasNewRules != value)
			{
				m_HasNewRules = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public BaconLobbyDataModel()
	{
		RegisterNestedDataModel(m_LuckyDraw);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		int num = hash * 31;
		_ = m_Rating;
		hash = num + m_Rating.GetHashCode();
		int num2 = hash * 31;
		_ = m_FirstPlaceFinishes;
		hash = num2 + m_FirstPlaceFinishes.GetHashCode();
		int num3 = hash * 31;
		_ = m_Top4Finishes;
		hash = num3 + m_Top4Finishes.GetHashCode();
		int num4 = hash * 31;
		_ = m_ShopOpen;
		hash = num4 + m_ShopOpen.GetHashCode();
		int num5 = hash * 31;
		_ = m_BattlegroundsSkinsEnabled;
		hash = num5 + m_BattlegroundsSkinsEnabled.GetHashCode();
		int num6 = hash * 31;
		_ = m_HasNewProducts;
		hash = num6 + m_HasNewProducts.GetHashCode();
		int num7 = hash * 31;
		_ = m_HasNewSkins;
		hash = num7 + m_HasNewSkins.GetHashCode();
		int num8 = hash * 31;
		_ = m_BattlegroundsRewardTrackEnabled;
		hash = num8 + m_BattlegroundsRewardTrackEnabled.GetHashCode();
		if (m_LuckyDraw != null && !inspectedDataModels.Contains(m_LuckyDraw.GetHashCode()))
		{
			inspectedDataModels.Add(m_LuckyDraw.GetHashCode());
			hash = hash * 31 + m_LuckyDraw.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num9 = hash * 31;
		_ = m_BattlegroundsInDuosMode;
		hash = num9 + m_BattlegroundsInDuosMode.GetHashCode();
		int num10 = hash * 31;
		_ = m_FullyLoaded;
		hash = num10 + m_FullyLoaded.GetHashCode();
		int num11 = hash * 31;
		_ = m_HasNewRules;
		return num11 + m_HasNewRules.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Rating;
			return true;
		case 1:
			value = m_FirstPlaceFinishes;
			return true;
		case 2:
			value = m_Top4Finishes;
			return true;
		case 6:
			value = m_ShopOpen;
			return true;
		case 7:
			value = m_BattlegroundsSkinsEnabled;
			return true;
		case 8:
			value = m_HasNewProducts;
			return true;
		case 9:
			value = m_HasNewSkins;
			return true;
		case 10:
			value = m_BattlegroundsRewardTrackEnabled;
			return true;
		case 11:
			value = m_LuckyDraw;
			return true;
		case 12:
			value = m_BattlegroundsInDuosMode;
			return true;
		case 13:
			value = m_FullyLoaded;
			return true;
		case 14:
			value = m_HasNewRules;
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
			Rating = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			FirstPlaceFinishes = ((value != null) ? ((int)value) : 0);
			return true;
		case 2:
			Top4Finishes = ((value != null) ? ((int)value) : 0);
			return true;
		case 6:
			ShopOpen = value != null && (bool)value;
			return true;
		case 7:
			BattlegroundsSkinsEnabled = value != null && (bool)value;
			return true;
		case 8:
			HasNewProducts = value != null && (bool)value;
			return true;
		case 9:
			HasNewSkins = value != null && (bool)value;
			return true;
		case 10:
			BattlegroundsRewardTrackEnabled = value != null && (bool)value;
			return true;
		case 11:
			LuckyDraw = ((value != null) ? ((LuckyDrawDataModel)value) : null);
			return true;
		case 12:
			BattlegroundsInDuosMode = value != null && (bool)value;
			return true;
		case 13:
			FullyLoaded = value != null && (bool)value;
			return true;
		case 14:
			HasNewRules = value != null && (bool)value;
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
		case 6:
			info = Properties[3];
			return true;
		case 7:
			info = Properties[4];
			return true;
		case 8:
			info = Properties[5];
			return true;
		case 9:
			info = Properties[6];
			return true;
		case 10:
			info = Properties[7];
			return true;
		case 11:
			info = Properties[8];
			return true;
		case 12:
			info = Properties[9];
			return true;
		case 13:
			info = Properties[10];
			return true;
		case 14:
			info = Properties[11];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
