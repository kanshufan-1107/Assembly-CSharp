using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LuckyDrawDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 666;

	private string m_Name;

	private string m_Theme;

	private string m_Layout;

	private bool m_HasPaid;

	private int m_Hammers;

	private ProductDataModel m_Product;

	private DataModelList<LuckyDrawRewardDataModel> m_Rewards = new DataModelList<LuckyDrawRewardDataModel>();

	private EventTimingType m_Event;

	private int m_NumUnacknowledgedHammers;

	private string m_TimeLeft;

	private bool m_IsAllRewardsOwned;

	private int m_NumUnacknowledgedBonusHammers;

	private int m_NumUnacknowledgedEarnedHammers;

	private int m_NumUnacknowledgedFreeHammers;

	private string m_TimeLeftStrPopup;

	private string m_TimeLeftToolTip;

	private DataModelProperty[] m_properties = new DataModelProperty[16]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "theme",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "layout",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "has_paid",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "hammers",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "product",
			Type = typeof(ProductDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "rewards",
			Type = typeof(DataModelList<LuckyDrawRewardDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "event",
			Type = typeof(EventTimingType)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "num_unacknowledged_hammers",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "time_left",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "is_all_rewards_owned",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "num_unacknowledged_bonus_hammers",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 12,
			PropertyDisplayName = "num_unacknowledged_earned_hammers",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 13,
			PropertyDisplayName = "num_unacknowledged_free_hammers",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 14,
			PropertyDisplayName = "time_left_str_popup",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 15,
			PropertyDisplayName = "time_left_tooltip",
			Type = typeof(string)
		}
	};

	public int DataModelId => 666;

	public string DataModelDisplayName => "lucky_draw";

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			if (!(m_Name == value))
			{
				m_Name = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Theme
	{
		get
		{
			return m_Theme;
		}
		set
		{
			if (!(m_Theme == value))
			{
				m_Theme = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Layout
	{
		get
		{
			return m_Layout;
		}
		set
		{
			if (!(m_Layout == value))
			{
				m_Layout = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasPaid
	{
		get
		{
			return m_HasPaid;
		}
		set
		{
			if (m_HasPaid != value)
			{
				m_HasPaid = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Hammers
	{
		get
		{
			return m_Hammers;
		}
		set
		{
			if (m_Hammers != value)
			{
				m_Hammers = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public ProductDataModel Product
	{
		get
		{
			return m_Product;
		}
		set
		{
			if (m_Product != value)
			{
				RemoveNestedDataModel(m_Product);
				RegisterNestedDataModel(value);
				m_Product = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<LuckyDrawRewardDataModel> Rewards
	{
		get
		{
			return m_Rewards;
		}
		set
		{
			if (m_Rewards != value)
			{
				RemoveNestedDataModel(m_Rewards);
				RegisterNestedDataModel(value);
				m_Rewards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public EventTimingType Event
	{
		get
		{
			return m_Event;
		}
		set
		{
			if (m_Event != value)
			{
				m_Event = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NumUnacknowledgedHammers
	{
		get
		{
			return m_NumUnacknowledgedHammers;
		}
		set
		{
			if (m_NumUnacknowledgedHammers != value)
			{
				m_NumUnacknowledgedHammers = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string TimeLeft
	{
		get
		{
			return m_TimeLeft;
		}
		set
		{
			if (!(m_TimeLeft == value))
			{
				m_TimeLeft = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsAllRewardsOwned
	{
		get
		{
			return m_IsAllRewardsOwned;
		}
		set
		{
			if (m_IsAllRewardsOwned != value)
			{
				m_IsAllRewardsOwned = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NumUnacknowledgedBonusHammers
	{
		get
		{
			return m_NumUnacknowledgedBonusHammers;
		}
		set
		{
			if (m_NumUnacknowledgedBonusHammers != value)
			{
				m_NumUnacknowledgedBonusHammers = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NumUnacknowledgedEarnedHammers
	{
		get
		{
			return m_NumUnacknowledgedEarnedHammers;
		}
		set
		{
			if (m_NumUnacknowledgedEarnedHammers != value)
			{
				m_NumUnacknowledgedEarnedHammers = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NumUnacknowledgedFreeHammers
	{
		get
		{
			return m_NumUnacknowledgedFreeHammers;
		}
		set
		{
			if (m_NumUnacknowledgedFreeHammers != value)
			{
				m_NumUnacknowledgedFreeHammers = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string TimeLeftStrPopup
	{
		get
		{
			return m_TimeLeftStrPopup;
		}
		set
		{
			if (!(m_TimeLeftStrPopup == value))
			{
				m_TimeLeftStrPopup = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string TimeLeftToolTip
	{
		get
		{
			return m_TimeLeftToolTip;
		}
		set
		{
			if (!(m_TimeLeftToolTip == value))
			{
				m_TimeLeftToolTip = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LuckyDrawDataModel()
	{
		RegisterNestedDataModel(m_Product);
		RegisterNestedDataModel(m_Rewards);
	}

	public int GetPropertiesHashCode()
	{
		int num = (((17 * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0)) * 31 + ((m_Theme != null) ? m_Theme.GetHashCode() : 0)) * 31 + ((m_Layout != null) ? m_Layout.GetHashCode() : 0)) * 31;
		_ = m_HasPaid;
		int num2 = (num + m_HasPaid.GetHashCode()) * 31;
		_ = m_Hammers;
		int num3 = (((num2 + m_Hammers.GetHashCode()) * 31 + ((m_Product != null) ? m_Product.GetPropertiesHashCode() : 0)) * 31 + ((m_Rewards != null) ? m_Rewards.GetPropertiesHashCode() : 0)) * 31;
		_ = m_Event;
		int num4 = (num3 + m_Event.GetHashCode()) * 31;
		_ = m_NumUnacknowledgedHammers;
		int num5 = ((num4 + m_NumUnacknowledgedHammers.GetHashCode()) * 31 + ((m_TimeLeft != null) ? m_TimeLeft.GetHashCode() : 0)) * 31;
		_ = m_IsAllRewardsOwned;
		int num6 = (num5 + m_IsAllRewardsOwned.GetHashCode()) * 31;
		_ = m_NumUnacknowledgedBonusHammers;
		int num7 = (num6 + m_NumUnacknowledgedBonusHammers.GetHashCode()) * 31;
		_ = m_NumUnacknowledgedEarnedHammers;
		int num8 = (num7 + m_NumUnacknowledgedEarnedHammers.GetHashCode()) * 31;
		_ = m_NumUnacknowledgedFreeHammers;
		return ((num8 + m_NumUnacknowledgedFreeHammers.GetHashCode()) * 31 + ((m_TimeLeftStrPopup != null) ? m_TimeLeftStrPopup.GetHashCode() : 0)) * 31 + ((m_TimeLeftToolTip != null) ? m_TimeLeftToolTip.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Name;
			return true;
		case 1:
			value = m_Theme;
			return true;
		case 2:
			value = m_Layout;
			return true;
		case 3:
			value = m_HasPaid;
			return true;
		case 4:
			value = m_Hammers;
			return true;
		case 5:
			value = m_Product;
			return true;
		case 6:
			value = m_Rewards;
			return true;
		case 7:
			value = m_Event;
			return true;
		case 8:
			value = m_NumUnacknowledgedHammers;
			return true;
		case 9:
			value = m_TimeLeft;
			return true;
		case 10:
			value = m_IsAllRewardsOwned;
			return true;
		case 11:
			value = m_NumUnacknowledgedBonusHammers;
			return true;
		case 12:
			value = m_NumUnacknowledgedEarnedHammers;
			return true;
		case 13:
			value = m_NumUnacknowledgedFreeHammers;
			return true;
		case 14:
			value = m_TimeLeftStrPopup;
			return true;
		case 15:
			value = m_TimeLeftToolTip;
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
			Name = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			Theme = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			Layout = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			HasPaid = value != null && (bool)value;
			return true;
		case 4:
			Hammers = ((value != null) ? ((int)value) : 0);
			return true;
		case 5:
			Product = ((value != null) ? ((ProductDataModel)value) : null);
			return true;
		case 6:
			Rewards = ((value != null) ? ((DataModelList<LuckyDrawRewardDataModel>)value) : null);
			return true;
		case 7:
			Event = ((value != null) ? ((EventTimingType)value) : EventTimingType.IGNORE);
			return true;
		case 8:
			NumUnacknowledgedHammers = ((value != null) ? ((int)value) : 0);
			return true;
		case 9:
			TimeLeft = ((value != null) ? ((string)value) : null);
			return true;
		case 10:
			IsAllRewardsOwned = value != null && (bool)value;
			return true;
		case 11:
			NumUnacknowledgedBonusHammers = ((value != null) ? ((int)value) : 0);
			return true;
		case 12:
			NumUnacknowledgedEarnedHammers = ((value != null) ? ((int)value) : 0);
			return true;
		case 13:
			NumUnacknowledgedFreeHammers = ((value != null) ? ((int)value) : 0);
			return true;
		case 14:
			TimeLeftStrPopup = ((value != null) ? ((string)value) : null);
			return true;
		case 15:
			TimeLeftToolTip = ((value != null) ? ((string)value) : null);
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
		case 3:
			info = Properties[3];
			return true;
		case 4:
			info = Properties[4];
			return true;
		case 5:
			info = Properties[5];
			return true;
		case 6:
			info = Properties[6];
			return true;
		case 7:
			info = Properties[7];
			return true;
		case 8:
			info = Properties[8];
			return true;
		case 9:
			info = Properties[9];
			return true;
		case 10:
			info = Properties[10];
			return true;
		case 11:
			info = Properties[11];
			return true;
		case 12:
			info = Properties[12];
			return true;
		case 13:
			info = Properties[13];
			return true;
		case 14:
			info = Properties[14];
			return true;
		case 15:
			info = Properties[15];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
