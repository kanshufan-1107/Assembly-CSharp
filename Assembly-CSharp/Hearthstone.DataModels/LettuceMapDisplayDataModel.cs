using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.DataModels;

public class LettuceMapDisplayDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 201;

	private DataModelList<CardDataModel> m_BossCard = new DataModelList<CardDataModel>();

	private string m_BossName;

	private RewardListDataModel m_FinalBossRewardList;

	private int m_MapSeed;

	private bool m_RunEnded;

	private DataModelList<LettuceTreasureSelectionDataModel> m_TreasureSelectionData = new DataModelList<LettuceTreasureSelectionDataModel>();

	private int m_SelectedTreasureChoices;

	private bool m_RunLost;

	private bool m_Heroic;

	private bool m_Tutorial;

	private Texture m_BountySetWatermark;

	private int m_CurrentTurns;

	private int m_FewestTurns;

	private string m_ZoneIdentifier;

	private LettuceVisitorSelectionDataModel m_VisitorSelectionData;

	private bool m_BonusRewardsActive;

	private DataModelProperty[] m_properties = new DataModelProperty[16]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "boss_card",
			Type = typeof(DataModelList<CardDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "boss_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "final_boss_reward_list",
			Type = typeof(RewardListDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "map_seed",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "run_ended",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "treasure_selection_data",
			Type = typeof(DataModelList<LettuceTreasureSelectionDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 768,
			PropertyDisplayName = "selected_treasure_choices",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "run_lost",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "heroic",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "tutorial",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "bounty_set_watermark",
			Type = typeof(Texture)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "current_turns",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "fewest_turns",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 12,
			PropertyDisplayName = "zone_identifier",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 14,
			PropertyDisplayName = "visitor_selection_data",
			Type = typeof(LettuceVisitorSelectionDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 644,
			PropertyDisplayName = "bonus_rewards_active",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 201;

	public string DataModelDisplayName => "lettuce_map_display";

	public DataModelList<CardDataModel> BossCard
	{
		get
		{
			return m_BossCard;
		}
		set
		{
			if (m_BossCard != value)
			{
				RemoveNestedDataModel(m_BossCard);
				RegisterNestedDataModel(value);
				m_BossCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string BossName
	{
		get
		{
			return m_BossName;
		}
		set
		{
			if (!(m_BossName == value))
			{
				m_BossName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public RewardListDataModel FinalBossRewardList
	{
		get
		{
			return m_FinalBossRewardList;
		}
		set
		{
			if (m_FinalBossRewardList != value)
			{
				RemoveNestedDataModel(m_FinalBossRewardList);
				RegisterNestedDataModel(value);
				m_FinalBossRewardList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MapSeed
	{
		get
		{
			return m_MapSeed;
		}
		set
		{
			if (m_MapSeed != value)
			{
				m_MapSeed = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool RunEnded
	{
		get
		{
			return m_RunEnded;
		}
		set
		{
			if (m_RunEnded != value)
			{
				m_RunEnded = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<LettuceTreasureSelectionDataModel> TreasureSelectionData
	{
		get
		{
			return m_TreasureSelectionData;
		}
		set
		{
			if (m_TreasureSelectionData != value)
			{
				RemoveNestedDataModel(m_TreasureSelectionData);
				RegisterNestedDataModel(value);
				m_TreasureSelectionData = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int SelectedTreasureChoices
	{
		get
		{
			return m_SelectedTreasureChoices;
		}
		set
		{
			if (m_SelectedTreasureChoices != value)
			{
				m_SelectedTreasureChoices = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool RunLost
	{
		get
		{
			return m_RunLost;
		}
		set
		{
			if (m_RunLost != value)
			{
				m_RunLost = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Heroic
	{
		get
		{
			return m_Heroic;
		}
		set
		{
			if (m_Heroic != value)
			{
				m_Heroic = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Tutorial
	{
		get
		{
			return m_Tutorial;
		}
		set
		{
			if (m_Tutorial != value)
			{
				m_Tutorial = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public Texture BountySetWatermark
	{
		get
		{
			return m_BountySetWatermark;
		}
		set
		{
			if (!(m_BountySetWatermark == value))
			{
				m_BountySetWatermark = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CurrentTurns
	{
		get
		{
			return m_CurrentTurns;
		}
		set
		{
			if (m_CurrentTurns != value)
			{
				m_CurrentTurns = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int FewestTurns
	{
		get
		{
			return m_FewestTurns;
		}
		set
		{
			if (m_FewestTurns != value)
			{
				m_FewestTurns = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ZoneIdentifier
	{
		get
		{
			return m_ZoneIdentifier;
		}
		set
		{
			if (!(m_ZoneIdentifier == value))
			{
				m_ZoneIdentifier = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceVisitorSelectionDataModel VisitorSelectionData
	{
		get
		{
			return m_VisitorSelectionData;
		}
		set
		{
			if (m_VisitorSelectionData != value)
			{
				RemoveNestedDataModel(m_VisitorSelectionData);
				RegisterNestedDataModel(value);
				m_VisitorSelectionData = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool BonusRewardsActive
	{
		get
		{
			return m_BonusRewardsActive;
		}
		set
		{
			if (m_BonusRewardsActive != value)
			{
				m_BonusRewardsActive = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceMapDisplayDataModel()
	{
		RegisterNestedDataModel(m_BossCard);
		RegisterNestedDataModel(m_FinalBossRewardList);
		RegisterNestedDataModel(m_TreasureSelectionData);
		RegisterNestedDataModel(m_VisitorSelectionData);
	}

	public int GetPropertiesHashCode()
	{
		int num = (((17 * 31 + ((m_BossCard != null) ? m_BossCard.GetPropertiesHashCode() : 0)) * 31 + ((m_BossName != null) ? m_BossName.GetHashCode() : 0)) * 31 + ((m_FinalBossRewardList != null) ? m_FinalBossRewardList.GetPropertiesHashCode() : 0)) * 31;
		_ = m_MapSeed;
		int num2 = (num + m_MapSeed.GetHashCode()) * 31;
		_ = m_RunEnded;
		int num3 = ((num2 + m_RunEnded.GetHashCode()) * 31 + ((m_TreasureSelectionData != null) ? m_TreasureSelectionData.GetPropertiesHashCode() : 0)) * 31;
		_ = m_SelectedTreasureChoices;
		int num4 = (num3 + m_SelectedTreasureChoices.GetHashCode()) * 31;
		_ = m_RunLost;
		int num5 = (num4 + m_RunLost.GetHashCode()) * 31;
		_ = m_Heroic;
		int num6 = (num5 + m_Heroic.GetHashCode()) * 31;
		_ = m_Tutorial;
		int num7 = ((num6 + m_Tutorial.GetHashCode()) * 31 + ((m_BountySetWatermark != null) ? m_BountySetWatermark.GetHashCode() : 0)) * 31;
		_ = m_CurrentTurns;
		int num8 = (num7 + m_CurrentTurns.GetHashCode()) * 31;
		_ = m_FewestTurns;
		int num9 = (((num8 + m_FewestTurns.GetHashCode()) * 31 + ((m_ZoneIdentifier != null) ? m_ZoneIdentifier.GetHashCode() : 0)) * 31 + ((m_VisitorSelectionData != null) ? m_VisitorSelectionData.GetPropertiesHashCode() : 0)) * 31;
		_ = m_BonusRewardsActive;
		return num9 + m_BonusRewardsActive.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_BossCard;
			return true;
		case 1:
			value = m_BossName;
			return true;
		case 2:
			value = m_FinalBossRewardList;
			return true;
		case 3:
			value = m_MapSeed;
			return true;
		case 4:
			value = m_RunEnded;
			return true;
		case 5:
			value = m_TreasureSelectionData;
			return true;
		case 768:
			value = m_SelectedTreasureChoices;
			return true;
		case 6:
			value = m_RunLost;
			return true;
		case 7:
			value = m_Heroic;
			return true;
		case 8:
			value = m_Tutorial;
			return true;
		case 9:
			value = m_BountySetWatermark;
			return true;
		case 10:
			value = m_CurrentTurns;
			return true;
		case 11:
			value = m_FewestTurns;
			return true;
		case 12:
			value = m_ZoneIdentifier;
			return true;
		case 14:
			value = m_VisitorSelectionData;
			return true;
		case 644:
			value = m_BonusRewardsActive;
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
			BossCard = ((value != null) ? ((DataModelList<CardDataModel>)value) : null);
			return true;
		case 1:
			BossName = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			FinalBossRewardList = ((value != null) ? ((RewardListDataModel)value) : null);
			return true;
		case 3:
			MapSeed = ((value != null) ? ((int)value) : 0);
			return true;
		case 4:
			RunEnded = value != null && (bool)value;
			return true;
		case 5:
			TreasureSelectionData = ((value != null) ? ((DataModelList<LettuceTreasureSelectionDataModel>)value) : null);
			return true;
		case 768:
			SelectedTreasureChoices = ((value != null) ? ((int)value) : 0);
			return true;
		case 6:
			RunLost = value != null && (bool)value;
			return true;
		case 7:
			Heroic = value != null && (bool)value;
			return true;
		case 8:
			Tutorial = value != null && (bool)value;
			return true;
		case 9:
			BountySetWatermark = ((value != null) ? ((Texture)value) : null);
			return true;
		case 10:
			CurrentTurns = ((value != null) ? ((int)value) : 0);
			return true;
		case 11:
			FewestTurns = ((value != null) ? ((int)value) : 0);
			return true;
		case 12:
			ZoneIdentifier = ((value != null) ? ((string)value) : null);
			return true;
		case 14:
			VisitorSelectionData = ((value != null) ? ((LettuceVisitorSelectionDataModel)value) : null);
			return true;
		case 644:
			BonusRewardsActive = value != null && (bool)value;
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
		case 768:
			info = Properties[6];
			return true;
		case 6:
			info = Properties[7];
			return true;
		case 7:
			info = Properties[8];
			return true;
		case 8:
			info = Properties[9];
			return true;
		case 9:
			info = Properties[10];
			return true;
		case 10:
			info = Properties[11];
			return true;
		case 11:
			info = Properties[12];
			return true;
		case 12:
			info = Properties[13];
			return true;
		case 14:
			info = Properties[14];
			return true;
		case 644:
			info = Properties[15];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
