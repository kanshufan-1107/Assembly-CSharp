using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class AdventureDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 7;

	private bool m_AnomalyActivated;

	private bool m_AllChaptersCompleted;

	private bool m_IsAdventureComplete;

	private bool m_AllChaptersOwned;

	private AdventureDbId m_SelectedAdventure;

	private AdventureModeDbId m_SelectedAdventureMode;

	private bool m_MapNewlyCompleted;

	private RewardListDataModel m_CompletionRewards;

	private Reward.Type m_CompletionRewardType;

	private int m_CompletionRewardId;

	private string m_StoreDescriptionTextTimelockedTrue;

	private string m_StoreDescriptionTextTimelockedFalse;

	private bool m_ShouldSeeFirstTimeFlow;

	private string m_SelectedHeroId;

	private bool m_IsSelectedModeHeroic;

	private bool m_IsDuelsAdventure;

	private DataModelProperty[] m_properties = new DataModelProperty[16]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "anomaly_activated",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "all_chapters_completed",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 12,
			PropertyDisplayName = "is_adventure_complete",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "all_chapters_owned",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "selected_adventure",
			Type = typeof(AdventureDbId)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "selected_adventure_mode",
			Type = typeof(AdventureModeDbId)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "map_newly_completed",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 170,
			PropertyDisplayName = "completion_rewards",
			Type = typeof(RewardListDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "DEPRECATED_completion_reward_type",
			Type = typeof(Reward.Type)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "DEPRECATED_completion_reward_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "store_description_timelocked_true",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "store_description_timelocked_false",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "should_see_first_time_flow",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "selected_hero_id",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 126,
			PropertyDisplayName = "is_selected_mode_heroic",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 576,
			PropertyDisplayName = "is_duels_adventure",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 7;

	public string DataModelDisplayName => "adventure";

	public bool AnomalyActivated
	{
		get
		{
			return m_AnomalyActivated;
		}
		set
		{
			if (m_AnomalyActivated != value)
			{
				m_AnomalyActivated = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool AllChaptersCompleted
	{
		get
		{
			return m_AllChaptersCompleted;
		}
		set
		{
			if (m_AllChaptersCompleted != value)
			{
				m_AllChaptersCompleted = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsAdventureComplete
	{
		get
		{
			return m_IsAdventureComplete;
		}
		set
		{
			if (m_IsAdventureComplete != value)
			{
				m_IsAdventureComplete = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool AllChaptersOwned
	{
		get
		{
			return m_AllChaptersOwned;
		}
		set
		{
			if (m_AllChaptersOwned != value)
			{
				m_AllChaptersOwned = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public AdventureDbId SelectedAdventure
	{
		get
		{
			return m_SelectedAdventure;
		}
		set
		{
			if (m_SelectedAdventure != value)
			{
				m_SelectedAdventure = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public AdventureModeDbId SelectedAdventureMode
	{
		get
		{
			return m_SelectedAdventureMode;
		}
		set
		{
			if (m_SelectedAdventureMode != value)
			{
				m_SelectedAdventureMode = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool MapNewlyCompleted
	{
		get
		{
			return m_MapNewlyCompleted;
		}
		set
		{
			if (m_MapNewlyCompleted != value)
			{
				m_MapNewlyCompleted = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public RewardListDataModel CompletionRewards
	{
		get
		{
			return m_CompletionRewards;
		}
		set
		{
			if (m_CompletionRewards != value)
			{
				RemoveNestedDataModel(m_CompletionRewards);
				RegisterNestedDataModel(value);
				m_CompletionRewards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public Reward.Type CompletionRewardType
	{
		get
		{
			return m_CompletionRewardType;
		}
		set
		{
			if (m_CompletionRewardType != value)
			{
				m_CompletionRewardType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CompletionRewardId
	{
		get
		{
			return m_CompletionRewardId;
		}
		set
		{
			if (m_CompletionRewardId != value)
			{
				m_CompletionRewardId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string StoreDescriptionTextTimelockedTrue
	{
		get
		{
			return m_StoreDescriptionTextTimelockedTrue;
		}
		set
		{
			if (!(m_StoreDescriptionTextTimelockedTrue == value))
			{
				m_StoreDescriptionTextTimelockedTrue = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string StoreDescriptionTextTimelockedFalse
	{
		get
		{
			return m_StoreDescriptionTextTimelockedFalse;
		}
		set
		{
			if (!(m_StoreDescriptionTextTimelockedFalse == value))
			{
				m_StoreDescriptionTextTimelockedFalse = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShouldSeeFirstTimeFlow
	{
		get
		{
			return m_ShouldSeeFirstTimeFlow;
		}
		set
		{
			if (m_ShouldSeeFirstTimeFlow != value)
			{
				m_ShouldSeeFirstTimeFlow = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string SelectedHeroId
	{
		get
		{
			return m_SelectedHeroId;
		}
		set
		{
			if (!(m_SelectedHeroId == value))
			{
				m_SelectedHeroId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsSelectedModeHeroic
	{
		get
		{
			return m_IsSelectedModeHeroic;
		}
		set
		{
			if (m_IsSelectedModeHeroic != value)
			{
				m_IsSelectedModeHeroic = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsDuelsAdventure
	{
		get
		{
			return m_IsDuelsAdventure;
		}
		set
		{
			if (m_IsDuelsAdventure != value)
			{
				m_IsDuelsAdventure = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public AdventureDataModel()
	{
		RegisterNestedDataModel(m_CompletionRewards);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_AnomalyActivated;
		int num2 = (num + m_AnomalyActivated.GetHashCode()) * 31;
		_ = m_AllChaptersCompleted;
		int num3 = (num2 + m_AllChaptersCompleted.GetHashCode()) * 31;
		_ = m_IsAdventureComplete;
		int num4 = (num3 + m_IsAdventureComplete.GetHashCode()) * 31;
		_ = m_AllChaptersOwned;
		int num5 = (num4 + m_AllChaptersOwned.GetHashCode()) * 31;
		_ = m_SelectedAdventure;
		int num6 = (num5 + m_SelectedAdventure.GetHashCode()) * 31;
		_ = m_SelectedAdventureMode;
		int num7 = (num6 + m_SelectedAdventureMode.GetHashCode()) * 31;
		_ = m_MapNewlyCompleted;
		int num8 = ((num7 + m_MapNewlyCompleted.GetHashCode()) * 31 + ((m_CompletionRewards != null) ? m_CompletionRewards.GetPropertiesHashCode() : 0)) * 31;
		_ = m_CompletionRewardType;
		int num9 = (num8 + m_CompletionRewardType.GetHashCode()) * 31;
		_ = m_CompletionRewardId;
		int num10 = (((num9 + m_CompletionRewardId.GetHashCode()) * 31 + ((m_StoreDescriptionTextTimelockedTrue != null) ? m_StoreDescriptionTextTimelockedTrue.GetHashCode() : 0)) * 31 + ((m_StoreDescriptionTextTimelockedFalse != null) ? m_StoreDescriptionTextTimelockedFalse.GetHashCode() : 0)) * 31;
		_ = m_ShouldSeeFirstTimeFlow;
		int num11 = ((num10 + m_ShouldSeeFirstTimeFlow.GetHashCode()) * 31 + ((m_SelectedHeroId != null) ? m_SelectedHeroId.GetHashCode() : 0)) * 31;
		_ = m_IsSelectedModeHeroic;
		int num12 = (num11 + m_IsSelectedModeHeroic.GetHashCode()) * 31;
		_ = m_IsDuelsAdventure;
		return num12 + m_IsDuelsAdventure.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_AnomalyActivated;
			return true;
		case 1:
			value = m_AllChaptersCompleted;
			return true;
		case 12:
			value = m_IsAdventureComplete;
			return true;
		case 2:
			value = m_AllChaptersOwned;
			return true;
		case 3:
			value = m_SelectedAdventure;
			return true;
		case 4:
			value = m_SelectedAdventureMode;
			return true;
		case 5:
			value = m_MapNewlyCompleted;
			return true;
		case 170:
			value = m_CompletionRewards;
			return true;
		case 6:
			value = m_CompletionRewardType;
			return true;
		case 7:
			value = m_CompletionRewardId;
			return true;
		case 8:
			value = m_StoreDescriptionTextTimelockedTrue;
			return true;
		case 9:
			value = m_StoreDescriptionTextTimelockedFalse;
			return true;
		case 10:
			value = m_ShouldSeeFirstTimeFlow;
			return true;
		case 11:
			value = m_SelectedHeroId;
			return true;
		case 126:
			value = m_IsSelectedModeHeroic;
			return true;
		case 576:
			value = m_IsDuelsAdventure;
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
			AnomalyActivated = value != null && (bool)value;
			return true;
		case 1:
			AllChaptersCompleted = value != null && (bool)value;
			return true;
		case 12:
			IsAdventureComplete = value != null && (bool)value;
			return true;
		case 2:
			AllChaptersOwned = value != null && (bool)value;
			return true;
		case 3:
			SelectedAdventure = ((value != null) ? ((AdventureDbId)value) : AdventureDbId.INVALID);
			return true;
		case 4:
			SelectedAdventureMode = ((value != null) ? ((AdventureModeDbId)value) : AdventureModeDbId.INVALID);
			return true;
		case 5:
			MapNewlyCompleted = value != null && (bool)value;
			return true;
		case 170:
			CompletionRewards = ((value != null) ? ((RewardListDataModel)value) : null);
			return true;
		case 6:
			CompletionRewardType = ((value != null) ? ((Reward.Type)value) : Reward.Type.ARCANE_DUST);
			return true;
		case 7:
			CompletionRewardId = ((value != null) ? ((int)value) : 0);
			return true;
		case 8:
			StoreDescriptionTextTimelockedTrue = ((value != null) ? ((string)value) : null);
			return true;
		case 9:
			StoreDescriptionTextTimelockedFalse = ((value != null) ? ((string)value) : null);
			return true;
		case 10:
			ShouldSeeFirstTimeFlow = value != null && (bool)value;
			return true;
		case 11:
			SelectedHeroId = ((value != null) ? ((string)value) : null);
			return true;
		case 126:
			IsSelectedModeHeroic = value != null && (bool)value;
			return true;
		case 576:
			IsDuelsAdventure = value != null && (bool)value;
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
		case 12:
			info = Properties[2];
			return true;
		case 2:
			info = Properties[3];
			return true;
		case 3:
			info = Properties[4];
			return true;
		case 4:
			info = Properties[5];
			return true;
		case 5:
			info = Properties[6];
			return true;
		case 170:
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
		case 10:
			info = Properties[12];
			return true;
		case 11:
			info = Properties[13];
			return true;
		case 126:
			info = Properties[14];
			return true;
		case 576:
			info = Properties[15];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
