using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LuckyDrawButtonDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 738;

	private int m_Hammers;

	private bool m_ClaimedFirstHammer;

	private bool m_LuckyDrawEnabled;

	private bool m_BattlePassPurchased;

	private bool m_HasNewHammers;

	private int m_HoursRemaining;

	private int m_NumHoursRemainingToShowTimer;

	private bool m_IsEventExpired;

	private bool m_ShowHighlight;

	private bool m_IsAllRewardsOwned;

	private DataModelProperty[] m_properties = new DataModelProperty[10]
	{
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "hammers",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "claimed_first_hammer",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "lucky_draw_enabled",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "battle_pass_purchased",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "has_new_hammers",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "hours_remaining",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "num_hours_remaining_to_show_timer",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "is_event_expired",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "show_highlight",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "is_all_rewards_owned",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 738;

	public string DataModelDisplayName => "lucky_draw_button";

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

	public bool ClaimedFirstHammer
	{
		get
		{
			return m_ClaimedFirstHammer;
		}
		set
		{
			if (m_ClaimedFirstHammer != value)
			{
				m_ClaimedFirstHammer = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool LuckyDrawEnabled
	{
		get
		{
			return m_LuckyDrawEnabled;
		}
		set
		{
			if (m_LuckyDrawEnabled != value)
			{
				m_LuckyDrawEnabled = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool BattlePassPurchased
	{
		get
		{
			return m_BattlePassPurchased;
		}
		set
		{
			if (m_BattlePassPurchased != value)
			{
				m_BattlePassPurchased = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasNewHammers
	{
		get
		{
			return m_HasNewHammers;
		}
		set
		{
			if (m_HasNewHammers != value)
			{
				m_HasNewHammers = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int HoursRemaining
	{
		get
		{
			return m_HoursRemaining;
		}
		set
		{
			if (m_HoursRemaining != value)
			{
				m_HoursRemaining = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NumHoursRemainingToShowTimer
	{
		get
		{
			return m_NumHoursRemainingToShowTimer;
		}
		set
		{
			if (m_NumHoursRemainingToShowTimer != value)
			{
				m_NumHoursRemainingToShowTimer = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsEventExpired
	{
		get
		{
			return m_IsEventExpired;
		}
		set
		{
			if (m_IsEventExpired != value)
			{
				m_IsEventExpired = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShowHighlight
	{
		get
		{
			return m_ShowHighlight;
		}
		set
		{
			if (m_ShowHighlight != value)
			{
				m_ShowHighlight = value;
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

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_Hammers;
		int num2 = (num + m_Hammers.GetHashCode()) * 31;
		_ = m_ClaimedFirstHammer;
		int num3 = (num2 + m_ClaimedFirstHammer.GetHashCode()) * 31;
		_ = m_LuckyDrawEnabled;
		int num4 = (num3 + m_LuckyDrawEnabled.GetHashCode()) * 31;
		_ = m_BattlePassPurchased;
		int num5 = (num4 + m_BattlePassPurchased.GetHashCode()) * 31;
		_ = m_HasNewHammers;
		int num6 = (num5 + m_HasNewHammers.GetHashCode()) * 31;
		_ = m_HoursRemaining;
		int num7 = (num6 + m_HoursRemaining.GetHashCode()) * 31;
		_ = m_NumHoursRemainingToShowTimer;
		int num8 = (num7 + m_NumHoursRemainingToShowTimer.GetHashCode()) * 31;
		_ = m_IsEventExpired;
		int num9 = (num8 + m_IsEventExpired.GetHashCode()) * 31;
		_ = m_ShowHighlight;
		int num10 = (num9 + m_ShowHighlight.GetHashCode()) * 31;
		_ = m_IsAllRewardsOwned;
		return num10 + m_IsAllRewardsOwned.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 1:
			value = m_Hammers;
			return true;
		case 2:
			value = m_ClaimedFirstHammer;
			return true;
		case 3:
			value = m_LuckyDrawEnabled;
			return true;
		case 4:
			value = m_BattlePassPurchased;
			return true;
		case 5:
			value = m_HasNewHammers;
			return true;
		case 6:
			value = m_HoursRemaining;
			return true;
		case 7:
			value = m_NumHoursRemainingToShowTimer;
			return true;
		case 8:
			value = m_IsEventExpired;
			return true;
		case 9:
			value = m_ShowHighlight;
			return true;
		case 10:
			value = m_IsAllRewardsOwned;
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
			Hammers = ((value != null) ? ((int)value) : 0);
			return true;
		case 2:
			ClaimedFirstHammer = value != null && (bool)value;
			return true;
		case 3:
			LuckyDrawEnabled = value != null && (bool)value;
			return true;
		case 4:
			BattlePassPurchased = value != null && (bool)value;
			return true;
		case 5:
			HasNewHammers = value != null && (bool)value;
			return true;
		case 6:
			HoursRemaining = ((value != null) ? ((int)value) : 0);
			return true;
		case 7:
			NumHoursRemainingToShowTimer = ((value != null) ? ((int)value) : 0);
			return true;
		case 8:
			IsEventExpired = value != null && (bool)value;
			return true;
		case 9:
			ShowHighlight = value != null && (bool)value;
			return true;
		case 10:
			IsAllRewardsOwned = value != null && (bool)value;
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
		case 3:
			info = Properties[2];
			return true;
		case 4:
			info = Properties[3];
			return true;
		case 5:
			info = Properties[4];
			return true;
		case 6:
			info = Properties[5];
			return true;
		case 7:
			info = Properties[6];
			return true;
		case 8:
			info = Properties[7];
			return true;
		case 9:
			info = Properties[8];
			return true;
		case 10:
			info = Properties[9];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
