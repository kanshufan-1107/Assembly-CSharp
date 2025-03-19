using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class JournalMetaDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 279;

	private int m_TabIndex;

	private bool m_EventTabActive;

	private bool m_RewardTrackHasUnclaimed;

	private bool m_RewardTrackHasNewSeason;

	private bool m_EventHasUnclaimed;

	private bool m_EventIsNew;

	private string m_RewardTrackSeasonName;

	private bool m_EventActive;

	private bool m_EventCompleted;

	private bool m_DoneChangingTabs;

	private bool m_RewardTrackPaidUnlocked;

	private bool m_IsActivatingEventTrack;

	private bool m_EventIsEndingSoon;

	private bool m_HidingAllBadges;

	private bool m_IsApprenticeTrackActive;

	private bool m_IsTavernGuideActive;

	private bool m_HasPassedTavernGuideButtonIntro;

	private bool m_TavernGuideHasUnclaimed;

	private bool m_HasSeenWelcomeApprentice;

	private bool m_HasJustCompletedApprentice;

	private bool m_EventLocked;

	private string m_EventLockedReason;

	private bool m_RewardTrackPaidPremiumUnlocked;

	private DataModelProperty[] m_properties = new DataModelProperty[23]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "tab_index",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "event_tab_active",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "reward_track_has_unclaimed",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "reward_track_has_new_season",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "event_has_unclaimed",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "event_is_new",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "reward_track_season_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "event_active",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "event_completed",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "done_changing_tabs",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "reward_track_paid_unlocked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "is_activating_event_track",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 12,
			PropertyDisplayName = "event_is_ending_soon",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 13,
			PropertyDisplayName = "hiding_all_badges",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 14,
			PropertyDisplayName = "is_apprentice_track_active",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 15,
			PropertyDisplayName = "is_tavern_guide_active",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 16,
			PropertyDisplayName = "has_passed_tavern_guide_button_intro",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 17,
			PropertyDisplayName = "tavern_guide_has_unclaimed",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 18,
			PropertyDisplayName = "has_seen_welcome_apprentice",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 19,
			PropertyDisplayName = "has_just_completed_apprentice",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 20,
			PropertyDisplayName = "event_locked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 21,
			PropertyDisplayName = "event_locked_reason",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 22,
			PropertyDisplayName = "reward_track_paid_premium_unlocked",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 279;

	public string DataModelDisplayName => "journal_meta";

	public int TabIndex
	{
		get
		{
			return m_TabIndex;
		}
		set
		{
			if (m_TabIndex != value)
			{
				m_TabIndex = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool EventTabActive
	{
		get
		{
			return m_EventTabActive;
		}
		set
		{
			if (m_EventTabActive != value)
			{
				m_EventTabActive = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool RewardTrackHasUnclaimed
	{
		get
		{
			return m_RewardTrackHasUnclaimed;
		}
		set
		{
			if (m_RewardTrackHasUnclaimed != value)
			{
				m_RewardTrackHasUnclaimed = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool RewardTrackHasNewSeason
	{
		get
		{
			return m_RewardTrackHasNewSeason;
		}
		set
		{
			if (m_RewardTrackHasNewSeason != value)
			{
				m_RewardTrackHasNewSeason = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool EventHasUnclaimed
	{
		get
		{
			return m_EventHasUnclaimed;
		}
		set
		{
			if (m_EventHasUnclaimed != value)
			{
				m_EventHasUnclaimed = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool EventIsNew
	{
		get
		{
			return m_EventIsNew;
		}
		set
		{
			if (m_EventIsNew != value)
			{
				m_EventIsNew = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string RewardTrackSeasonName
	{
		get
		{
			return m_RewardTrackSeasonName;
		}
		set
		{
			if (!(m_RewardTrackSeasonName == value))
			{
				m_RewardTrackSeasonName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool EventActive
	{
		get
		{
			return m_EventActive;
		}
		set
		{
			if (m_EventActive != value)
			{
				m_EventActive = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool EventCompleted
	{
		get
		{
			return m_EventCompleted;
		}
		set
		{
			if (m_EventCompleted != value)
			{
				m_EventCompleted = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool DoneChangingTabs
	{
		get
		{
			return m_DoneChangingTabs;
		}
		set
		{
			if (m_DoneChangingTabs != value)
			{
				m_DoneChangingTabs = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool RewardTrackPaidUnlocked
	{
		get
		{
			return m_RewardTrackPaidUnlocked;
		}
		set
		{
			if (m_RewardTrackPaidUnlocked != value)
			{
				m_RewardTrackPaidUnlocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsActivatingEventTrack
	{
		get
		{
			return m_IsActivatingEventTrack;
		}
		set
		{
			if (m_IsActivatingEventTrack != value)
			{
				m_IsActivatingEventTrack = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool EventIsEndingSoon
	{
		get
		{
			return m_EventIsEndingSoon;
		}
		set
		{
			if (m_EventIsEndingSoon != value)
			{
				m_EventIsEndingSoon = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HidingAllBadges
	{
		get
		{
			return m_HidingAllBadges;
		}
		set
		{
			if (m_HidingAllBadges != value)
			{
				m_HidingAllBadges = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsApprenticeTrackActive
	{
		get
		{
			return m_IsApprenticeTrackActive;
		}
		set
		{
			if (m_IsApprenticeTrackActive != value)
			{
				m_IsApprenticeTrackActive = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsTavernGuideActive
	{
		get
		{
			return m_IsTavernGuideActive;
		}
		set
		{
			if (m_IsTavernGuideActive != value)
			{
				m_IsTavernGuideActive = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasPassedTavernGuideButtonIntro
	{
		get
		{
			return m_HasPassedTavernGuideButtonIntro;
		}
		set
		{
			if (m_HasPassedTavernGuideButtonIntro != value)
			{
				m_HasPassedTavernGuideButtonIntro = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool TavernGuideHasUnclaimed
	{
		get
		{
			return m_TavernGuideHasUnclaimed;
		}
		set
		{
			if (m_TavernGuideHasUnclaimed != value)
			{
				m_TavernGuideHasUnclaimed = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasSeenWelcomeApprentice
	{
		get
		{
			return m_HasSeenWelcomeApprentice;
		}
		set
		{
			if (m_HasSeenWelcomeApprentice != value)
			{
				m_HasSeenWelcomeApprentice = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasJustCompletedApprentice
	{
		get
		{
			return m_HasJustCompletedApprentice;
		}
		set
		{
			if (m_HasJustCompletedApprentice != value)
			{
				m_HasJustCompletedApprentice = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool EventLocked
	{
		get
		{
			return m_EventLocked;
		}
		set
		{
			if (m_EventLocked != value)
			{
				m_EventLocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string EventLockedReason
	{
		get
		{
			return m_EventLockedReason;
		}
		set
		{
			if (!(m_EventLockedReason == value))
			{
				m_EventLockedReason = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool RewardTrackPaidPremiumUnlocked
	{
		get
		{
			return m_RewardTrackPaidPremiumUnlocked;
		}
		set
		{
			if (m_RewardTrackPaidPremiumUnlocked != value)
			{
				m_RewardTrackPaidPremiumUnlocked = value;
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
		_ = m_TabIndex;
		int num2 = (num + m_TabIndex.GetHashCode()) * 31;
		_ = m_EventTabActive;
		int num3 = (num2 + m_EventTabActive.GetHashCode()) * 31;
		_ = m_RewardTrackHasUnclaimed;
		int num4 = (num3 + m_RewardTrackHasUnclaimed.GetHashCode()) * 31;
		_ = m_RewardTrackHasNewSeason;
		int num5 = (num4 + m_RewardTrackHasNewSeason.GetHashCode()) * 31;
		_ = m_EventHasUnclaimed;
		int num6 = (num5 + m_EventHasUnclaimed.GetHashCode()) * 31;
		_ = m_EventIsNew;
		int num7 = ((num6 + m_EventIsNew.GetHashCode()) * 31 + ((m_RewardTrackSeasonName != null) ? m_RewardTrackSeasonName.GetHashCode() : 0)) * 31;
		_ = m_EventActive;
		int num8 = (num7 + m_EventActive.GetHashCode()) * 31;
		_ = m_EventCompleted;
		int num9 = (num8 + m_EventCompleted.GetHashCode()) * 31;
		_ = m_DoneChangingTabs;
		int num10 = (num9 + m_DoneChangingTabs.GetHashCode()) * 31;
		_ = m_RewardTrackPaidUnlocked;
		int num11 = (num10 + m_RewardTrackPaidUnlocked.GetHashCode()) * 31;
		_ = m_IsActivatingEventTrack;
		int num12 = (num11 + m_IsActivatingEventTrack.GetHashCode()) * 31;
		_ = m_EventIsEndingSoon;
		int num13 = (num12 + m_EventIsEndingSoon.GetHashCode()) * 31;
		_ = m_HidingAllBadges;
		int num14 = (num13 + m_HidingAllBadges.GetHashCode()) * 31;
		_ = m_IsApprenticeTrackActive;
		int num15 = (num14 + m_IsApprenticeTrackActive.GetHashCode()) * 31;
		_ = m_IsTavernGuideActive;
		int num16 = (num15 + m_IsTavernGuideActive.GetHashCode()) * 31;
		_ = m_HasPassedTavernGuideButtonIntro;
		int num17 = (num16 + m_HasPassedTavernGuideButtonIntro.GetHashCode()) * 31;
		_ = m_TavernGuideHasUnclaimed;
		int num18 = (num17 + m_TavernGuideHasUnclaimed.GetHashCode()) * 31;
		_ = m_HasSeenWelcomeApprentice;
		int num19 = (num18 + m_HasSeenWelcomeApprentice.GetHashCode()) * 31;
		_ = m_HasJustCompletedApprentice;
		int num20 = (num19 + m_HasJustCompletedApprentice.GetHashCode()) * 31;
		_ = m_EventLocked;
		int num21 = ((num20 + m_EventLocked.GetHashCode()) * 31 + ((m_EventLockedReason != null) ? m_EventLockedReason.GetHashCode() : 0)) * 31;
		_ = m_RewardTrackPaidPremiumUnlocked;
		return num21 + m_RewardTrackPaidPremiumUnlocked.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_TabIndex;
			return true;
		case 1:
			value = m_EventTabActive;
			return true;
		case 2:
			value = m_RewardTrackHasUnclaimed;
			return true;
		case 3:
			value = m_RewardTrackHasNewSeason;
			return true;
		case 4:
			value = m_EventHasUnclaimed;
			return true;
		case 5:
			value = m_EventIsNew;
			return true;
		case 6:
			value = m_RewardTrackSeasonName;
			return true;
		case 7:
			value = m_EventActive;
			return true;
		case 8:
			value = m_EventCompleted;
			return true;
		case 9:
			value = m_DoneChangingTabs;
			return true;
		case 10:
			value = m_RewardTrackPaidUnlocked;
			return true;
		case 11:
			value = m_IsActivatingEventTrack;
			return true;
		case 12:
			value = m_EventIsEndingSoon;
			return true;
		case 13:
			value = m_HidingAllBadges;
			return true;
		case 14:
			value = m_IsApprenticeTrackActive;
			return true;
		case 15:
			value = m_IsTavernGuideActive;
			return true;
		case 16:
			value = m_HasPassedTavernGuideButtonIntro;
			return true;
		case 17:
			value = m_TavernGuideHasUnclaimed;
			return true;
		case 18:
			value = m_HasSeenWelcomeApprentice;
			return true;
		case 19:
			value = m_HasJustCompletedApprentice;
			return true;
		case 20:
			value = m_EventLocked;
			return true;
		case 21:
			value = m_EventLockedReason;
			return true;
		case 22:
			value = m_RewardTrackPaidPremiumUnlocked;
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
			TabIndex = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			EventTabActive = value != null && (bool)value;
			return true;
		case 2:
			RewardTrackHasUnclaimed = value != null && (bool)value;
			return true;
		case 3:
			RewardTrackHasNewSeason = value != null && (bool)value;
			return true;
		case 4:
			EventHasUnclaimed = value != null && (bool)value;
			return true;
		case 5:
			EventIsNew = value != null && (bool)value;
			return true;
		case 6:
			RewardTrackSeasonName = ((value != null) ? ((string)value) : null);
			return true;
		case 7:
			EventActive = value != null && (bool)value;
			return true;
		case 8:
			EventCompleted = value != null && (bool)value;
			return true;
		case 9:
			DoneChangingTabs = value != null && (bool)value;
			return true;
		case 10:
			RewardTrackPaidUnlocked = value != null && (bool)value;
			return true;
		case 11:
			IsActivatingEventTrack = value != null && (bool)value;
			return true;
		case 12:
			EventIsEndingSoon = value != null && (bool)value;
			return true;
		case 13:
			HidingAllBadges = value != null && (bool)value;
			return true;
		case 14:
			IsApprenticeTrackActive = value != null && (bool)value;
			return true;
		case 15:
			IsTavernGuideActive = value != null && (bool)value;
			return true;
		case 16:
			HasPassedTavernGuideButtonIntro = value != null && (bool)value;
			return true;
		case 17:
			TavernGuideHasUnclaimed = value != null && (bool)value;
			return true;
		case 18:
			HasSeenWelcomeApprentice = value != null && (bool)value;
			return true;
		case 19:
			HasJustCompletedApprentice = value != null && (bool)value;
			return true;
		case 20:
			EventLocked = value != null && (bool)value;
			return true;
		case 21:
			EventLockedReason = ((value != null) ? ((string)value) : null);
			return true;
		case 22:
			RewardTrackPaidPremiumUnlocked = value != null && (bool)value;
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
