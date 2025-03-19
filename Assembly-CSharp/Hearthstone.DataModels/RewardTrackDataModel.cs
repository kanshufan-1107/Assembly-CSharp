using Assets;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class RewardTrackDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 229;

	private int m_RewardTrackId;

	private int m_Level;

	private int m_Xp;

	private int m_XpNeeded;

	private int m_XpBonusPercent;

	private bool m_PaidRewardsUnlocked;

	private string m_XpProgress;

	private int m_Unclaimed;

	private int m_LevelSoftCap;

	private string m_XpBoostText;

	private int m_LevelHardCap;

	private int m_Season;

	private int m_SeasonLastSeen;

	private string m_Name;

	private Global.RewardTrackType m_RewardTrackType;

	private int m_TotalXp;

	private string m_TimeRemainingText;

	private string m_ShortDescription;

	private string m_LongDescription;

	private string m_ShortConclusion;

	private string m_LongConclusion;

	private bool m_Expired;

	private string m_ChoiceConfirmationText;

	private bool m_PaidPremiumRewardsUnlocked;

	private DataModelProperty[] m_properties = new DataModelProperty[24]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "reward_track_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "level",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "xp",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "xp_needed",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "xp_bonus_percent",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "paid_rewards_unlocked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "xp_progress",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "unclaimed",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "level_soft_cap",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "xp_boost_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "level_hard_cap",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "season",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 12,
			PropertyDisplayName = "season_last_seen",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 13,
			PropertyDisplayName = "name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 14,
			PropertyDisplayName = "reward_track_type",
			Type = typeof(Global.RewardTrackType)
		},
		new DataModelProperty
		{
			PropertyId = 15,
			PropertyDisplayName = "total_xp",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 16,
			PropertyDisplayName = "time_remaining_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 17,
			PropertyDisplayName = "short_description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 18,
			PropertyDisplayName = "long_description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 19,
			PropertyDisplayName = "short_conclusion",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 20,
			PropertyDisplayName = "long_conclusion",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 21,
			PropertyDisplayName = "expired",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 22,
			PropertyDisplayName = "choice_confirmation_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 23,
			PropertyDisplayName = "paid_premium_rewards_unlocked",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 229;

	public string DataModelDisplayName => "reward_track";

	public int RewardTrackId
	{
		get
		{
			return m_RewardTrackId;
		}
		set
		{
			if (m_RewardTrackId != value)
			{
				m_RewardTrackId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Level
	{
		get
		{
			return m_Level;
		}
		set
		{
			if (m_Level != value)
			{
				m_Level = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Xp
	{
		get
		{
			return m_Xp;
		}
		set
		{
			if (m_Xp != value)
			{
				m_Xp = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int XpNeeded
	{
		get
		{
			return m_XpNeeded;
		}
		set
		{
			if (m_XpNeeded != value)
			{
				m_XpNeeded = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int XpBonusPercent
	{
		get
		{
			return m_XpBonusPercent;
		}
		set
		{
			if (m_XpBonusPercent != value)
			{
				m_XpBonusPercent = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool PaidRewardsUnlocked
	{
		get
		{
			return m_PaidRewardsUnlocked;
		}
		set
		{
			if (m_PaidRewardsUnlocked != value)
			{
				m_PaidRewardsUnlocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string XpProgress
	{
		get
		{
			return m_XpProgress;
		}
		set
		{
			if (!(m_XpProgress == value))
			{
				m_XpProgress = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Unclaimed
	{
		get
		{
			return m_Unclaimed;
		}
		set
		{
			if (m_Unclaimed != value)
			{
				m_Unclaimed = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int LevelSoftCap
	{
		get
		{
			return m_LevelSoftCap;
		}
		set
		{
			if (m_LevelSoftCap != value)
			{
				m_LevelSoftCap = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string XpBoostText
	{
		get
		{
			return m_XpBoostText;
		}
		set
		{
			if (!(m_XpBoostText == value))
			{
				m_XpBoostText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int LevelHardCap
	{
		get
		{
			return m_LevelHardCap;
		}
		set
		{
			if (m_LevelHardCap != value)
			{
				m_LevelHardCap = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Season
	{
		get
		{
			return m_Season;
		}
		set
		{
			if (m_Season != value)
			{
				m_Season = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int SeasonLastSeen
	{
		get
		{
			return m_SeasonLastSeen;
		}
		set
		{
			if (m_SeasonLastSeen != value)
			{
				m_SeasonLastSeen = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

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

	public Global.RewardTrackType RewardTrackType
	{
		get
		{
			return m_RewardTrackType;
		}
		set
		{
			if (m_RewardTrackType != value)
			{
				m_RewardTrackType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TotalXp
	{
		get
		{
			return m_TotalXp;
		}
		set
		{
			if (m_TotalXp != value)
			{
				m_TotalXp = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string TimeRemainingText
	{
		get
		{
			return m_TimeRemainingText;
		}
		set
		{
			if (!(m_TimeRemainingText == value))
			{
				m_TimeRemainingText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ShortDescription
	{
		get
		{
			return m_ShortDescription;
		}
		set
		{
			if (!(m_ShortDescription == value))
			{
				m_ShortDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string LongDescription
	{
		get
		{
			return m_LongDescription;
		}
		set
		{
			if (!(m_LongDescription == value))
			{
				m_LongDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ShortConclusion
	{
		get
		{
			return m_ShortConclusion;
		}
		set
		{
			if (!(m_ShortConclusion == value))
			{
				m_ShortConclusion = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string LongConclusion
	{
		get
		{
			return m_LongConclusion;
		}
		set
		{
			if (!(m_LongConclusion == value))
			{
				m_LongConclusion = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Expired
	{
		get
		{
			return m_Expired;
		}
		set
		{
			if (m_Expired != value)
			{
				m_Expired = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ChoiceConfirmationText
	{
		get
		{
			return m_ChoiceConfirmationText;
		}
		set
		{
			if (!(m_ChoiceConfirmationText == value))
			{
				m_ChoiceConfirmationText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool PaidPremiumRewardsUnlocked
	{
		get
		{
			return m_PaidPremiumRewardsUnlocked;
		}
		set
		{
			if (m_PaidPremiumRewardsUnlocked != value)
			{
				m_PaidPremiumRewardsUnlocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_RewardTrackId;
		int num2 = (num + m_RewardTrackId.GetHashCode()) * 31;
		_ = m_Level;
		int num3 = (num2 + m_Level.GetHashCode()) * 31;
		_ = m_Xp;
		int num4 = (num3 + m_Xp.GetHashCode()) * 31;
		_ = m_XpNeeded;
		int num5 = (num4 + m_XpNeeded.GetHashCode()) * 31;
		_ = m_XpBonusPercent;
		int num6 = (num5 + m_XpBonusPercent.GetHashCode()) * 31;
		_ = m_PaidRewardsUnlocked;
		int num7 = ((num6 + m_PaidRewardsUnlocked.GetHashCode()) * 31 + ((m_XpProgress != null) ? m_XpProgress.GetHashCode() : 0)) * 31;
		_ = m_Unclaimed;
		int num8 = (num7 + m_Unclaimed.GetHashCode()) * 31;
		_ = m_LevelSoftCap;
		int num9 = ((num8 + m_LevelSoftCap.GetHashCode()) * 31 + ((m_XpBoostText != null) ? m_XpBoostText.GetHashCode() : 0)) * 31;
		_ = m_LevelHardCap;
		int num10 = (num9 + m_LevelHardCap.GetHashCode()) * 31;
		_ = m_Season;
		int num11 = (num10 + m_Season.GetHashCode()) * 31;
		_ = m_SeasonLastSeen;
		int num12 = ((num11 + m_SeasonLastSeen.GetHashCode()) * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0)) * 31;
		_ = m_RewardTrackType;
		int num13 = (num12 + m_RewardTrackType.GetHashCode()) * 31;
		_ = m_TotalXp;
		int num14 = ((((((num13 + m_TotalXp.GetHashCode()) * 31 + ((m_TimeRemainingText != null) ? m_TimeRemainingText.GetHashCode() : 0)) * 31 + ((m_ShortDescription != null) ? m_ShortDescription.GetHashCode() : 0)) * 31 + ((m_LongDescription != null) ? m_LongDescription.GetHashCode() : 0)) * 31 + ((m_ShortConclusion != null) ? m_ShortConclusion.GetHashCode() : 0)) * 31 + ((m_LongConclusion != null) ? m_LongConclusion.GetHashCode() : 0)) * 31;
		_ = m_Expired;
		int num15 = ((num14 + m_Expired.GetHashCode()) * 31 + ((m_ChoiceConfirmationText != null) ? m_ChoiceConfirmationText.GetHashCode() : 0)) * 31;
		_ = m_PaidPremiumRewardsUnlocked;
		return num15 + m_PaidPremiumRewardsUnlocked.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_RewardTrackId;
			return true;
		case 1:
			value = m_Level;
			return true;
		case 2:
			value = m_Xp;
			return true;
		case 3:
			value = m_XpNeeded;
			return true;
		case 4:
			value = m_XpBonusPercent;
			return true;
		case 5:
			value = m_PaidRewardsUnlocked;
			return true;
		case 6:
			value = m_XpProgress;
			return true;
		case 7:
			value = m_Unclaimed;
			return true;
		case 8:
			value = m_LevelSoftCap;
			return true;
		case 9:
			value = m_XpBoostText;
			return true;
		case 10:
			value = m_LevelHardCap;
			return true;
		case 11:
			value = m_Season;
			return true;
		case 12:
			value = m_SeasonLastSeen;
			return true;
		case 13:
			value = m_Name;
			return true;
		case 14:
			value = m_RewardTrackType;
			return true;
		case 15:
			value = m_TotalXp;
			return true;
		case 16:
			value = m_TimeRemainingText;
			return true;
		case 17:
			value = m_ShortDescription;
			return true;
		case 18:
			value = m_LongDescription;
			return true;
		case 19:
			value = m_ShortConclusion;
			return true;
		case 20:
			value = m_LongConclusion;
			return true;
		case 21:
			value = m_Expired;
			return true;
		case 22:
			value = m_ChoiceConfirmationText;
			return true;
		case 23:
			value = m_PaidPremiumRewardsUnlocked;
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
			RewardTrackId = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			Level = ((value != null) ? ((int)value) : 0);
			return true;
		case 2:
			Xp = ((value != null) ? ((int)value) : 0);
			return true;
		case 3:
			XpNeeded = ((value != null) ? ((int)value) : 0);
			return true;
		case 4:
			XpBonusPercent = ((value != null) ? ((int)value) : 0);
			return true;
		case 5:
			PaidRewardsUnlocked = value != null && (bool)value;
			return true;
		case 6:
			XpProgress = ((value != null) ? ((string)value) : null);
			return true;
		case 7:
			Unclaimed = ((value != null) ? ((int)value) : 0);
			return true;
		case 8:
			LevelSoftCap = ((value != null) ? ((int)value) : 0);
			return true;
		case 9:
			XpBoostText = ((value != null) ? ((string)value) : null);
			return true;
		case 10:
			LevelHardCap = ((value != null) ? ((int)value) : 0);
			return true;
		case 11:
			Season = ((value != null) ? ((int)value) : 0);
			return true;
		case 12:
			SeasonLastSeen = ((value != null) ? ((int)value) : 0);
			return true;
		case 13:
			Name = ((value != null) ? ((string)value) : null);
			return true;
		case 14:
			RewardTrackType = ((value != null) ? ((Global.RewardTrackType)value) : Global.RewardTrackType.NONE);
			return true;
		case 15:
			TotalXp = ((value != null) ? ((int)value) : 0);
			return true;
		case 16:
			TimeRemainingText = ((value != null) ? ((string)value) : null);
			return true;
		case 17:
			ShortDescription = ((value != null) ? ((string)value) : null);
			return true;
		case 18:
			LongDescription = ((value != null) ? ((string)value) : null);
			return true;
		case 19:
			ShortConclusion = ((value != null) ? ((string)value) : null);
			return true;
		case 20:
			LongConclusion = ((value != null) ? ((string)value) : null);
			return true;
		case 21:
			Expired = value != null && (bool)value;
			return true;
		case 22:
			ChoiceConfirmationText = ((value != null) ? ((string)value) : null);
			return true;
		case 23:
			PaidPremiumRewardsUnlocked = value != null && (bool)value;
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
		case 16:
			info = Properties[16];
			return true;
		case 17:
			info = Properties[17];
			return true;
		case 18:
			info = Properties[18];
			return true;
		case 19:
			info = Properties[19];
			return true;
		case 20:
			info = Properties[20];
			return true;
		case 21:
			info = Properties[21];
			return true;
		case 22:
			info = Properties[22];
			return true;
		case 23:
			info = Properties[23];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
