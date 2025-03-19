using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class BaconPastGameStatsDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 140;

	private CardDataModel m_Hero;

	private CardDataModel m_HeroPower;

	private int m_Place;

	private DataModelList<CardDataModel> m_Minions = new DataModelList<CardDataModel>();

	private string m_HeroName;

	private CardDataModel m_Quest;

	private CardDataModel m_Reward;

	private bool m_RewardCompleted;

	private int m_HeroPowerRewardCardDatabaseID;

	private int m_HeroPowerRewardMinionType;

	private int m_RewardCardDatabaseID;

	private int m_RewardMinionType;

	private int m_QuestRace1;

	private int m_QuestRace2;

	private int m_QuestProgressTotal;

	private int m_HeroPowerQuestRace1;

	private int m_HeroPowerQuestRace2;

	private int m_HeroPowerQuestProgressTotal;

	private CardDataModel m_HeroBuddyMeter;

	private int m_NumHeroBuddiesGained;

	private CardDataModel m_HeroBuddy;

	private int m_HeroBuddyDatabaseID;

	private int m_HeroBuddyCost;

	private CardDataModel m_Anomaly;

	private CardDataModel m_TeammateHero;

	private CardDataModel m_TeammateHeroPower;

	private DataModelList<CardDataModel> m_TeammateMinions = new DataModelList<CardDataModel>();

	private string m_TeammateHeroName;

	private CardDataModel m_TeammateQuest;

	private CardDataModel m_TeammateReward;

	private bool m_TeammateRewardCompleted;

	private int m_TeammateHeroPowerRewardCardDatabaseID;

	private int m_TeammateHeroPowerRewardMinionType;

	private int m_TeammateRewardCardDatabaseID;

	private int m_TeammateRewardMinionType;

	private int m_TeammateQuestRace1;

	private int m_TeammateQuestRace2;

	private int m_TeammateQuestProgressTotal;

	private int m_TeammateHeroPowerQuestRace1;

	private int m_TeammateHeroPowerQuestRace2;

	private int m_TeammateHeroPowerQuestProgressTotal;

	private CardDataModel m_TeammateHeroBuddyMeter;

	private int m_TeammateNumHeroBuddiesGained;

	private CardDataModel m_TeammateHeroBuddy;

	private int m_TeammateHeroBuddyDatabaseID;

	private int m_TeammateHeroBuddyCost;

	private CardDataModel m_Trinket1;

	private CardDataModel m_Trinket2;

	private CardDataModel m_TrinketHeroPower;

	private CardDataModel m_TeammateTrinket1;

	private CardDataModel m_TeammateTrinket2;

	private CardDataModel m_TeammateTrinketHeroPower;

	private int m_Trinket1MinionType;

	private int m_Trinket2MinionType;

	private int m_TrinketHeroPowerMinionType;

	private int m_TeammateTrinket1MinionType;

	private int m_TeammateTrinket2MinionType;

	private int m_TeammateTrinketHeroPowerMinionType;

	private DataModelProperty[] m_properties = new DataModelProperty[58]
	{
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "hero",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "hero_power",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "place",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "minions",
			Type = typeof(DataModelList<CardDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "hero_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "quest",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "reward",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "reward_completed",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "hero_power_reward_card_database_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "hero_power_reward_minion_type",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "reward_card_database_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 12,
			PropertyDisplayName = "reward_minion_type",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 13,
			PropertyDisplayName = "quest_race_1",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 14,
			PropertyDisplayName = "quest_race_2",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 15,
			PropertyDisplayName = "quest_progress_total",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 16,
			PropertyDisplayName = "hero_power_quest_race_1",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 17,
			PropertyDisplayName = "hero_power_quest_race_2",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 18,
			PropertyDisplayName = "hero_power_quest_progress_total",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 19,
			PropertyDisplayName = "hero_buddy_meter",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 20,
			PropertyDisplayName = "num_hero_buddies_gained",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 21,
			PropertyDisplayName = "hero_buddy",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 22,
			PropertyDisplayName = "hero_buddy_database_ID",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 23,
			PropertyDisplayName = "hero_buddy_cost",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 24,
			PropertyDisplayName = "anomaly",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 25,
			PropertyDisplayName = "teammate_hero",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 26,
			PropertyDisplayName = "teammate_hero_power",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 27,
			PropertyDisplayName = "teammate_minions",
			Type = typeof(DataModelList<CardDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 28,
			PropertyDisplayName = "teammate_hero_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 29,
			PropertyDisplayName = "teammate_quest",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 30,
			PropertyDisplayName = "teammate_reward",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 31,
			PropertyDisplayName = "teammate_reward_completed",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 32,
			PropertyDisplayName = "teammate_hero_power_reward_card_database_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 33,
			PropertyDisplayName = "teammate_hero_power_reward_minion_type",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 34,
			PropertyDisplayName = "teamamte_reward_card_database_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 35,
			PropertyDisplayName = "teammate_reward_minion_type",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 36,
			PropertyDisplayName = "teammate_quest_race_1",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 37,
			PropertyDisplayName = "teammate_quest_race_2",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 38,
			PropertyDisplayName = "teammate_quest_progress_total",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 39,
			PropertyDisplayName = "teammate_hero_power_quest_race_1",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 40,
			PropertyDisplayName = "teammate_hero_power_quest_race_2",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 41,
			PropertyDisplayName = "teammate_hero_power_quest_progress_total",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 42,
			PropertyDisplayName = "teammate_hero_buddy_meter",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 43,
			PropertyDisplayName = "teammate_num_hero_buddies_gained",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 44,
			PropertyDisplayName = "teammate_hero_buddy",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 45,
			PropertyDisplayName = "teammate_hero_buddy_database_ID",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 46,
			PropertyDisplayName = "teammate_hero_buddy_cost",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 47,
			PropertyDisplayName = "trinket1",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 48,
			PropertyDisplayName = "trinket2",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 49,
			PropertyDisplayName = "trinket_hero_power",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 50,
			PropertyDisplayName = "teammate_trinket1",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 51,
			PropertyDisplayName = "teammate_trinket2",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 52,
			PropertyDisplayName = "teammate_trinket_hero_power",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 53,
			PropertyDisplayName = "trinket1_minion_type",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 54,
			PropertyDisplayName = "trinket2_minion_type",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 55,
			PropertyDisplayName = "trinket_hero_power_minion_type",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 56,
			PropertyDisplayName = "teammate_trinket1_minion_type",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 57,
			PropertyDisplayName = "teammate_trinket2_minion_type",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 58,
			PropertyDisplayName = "teammate_trinket_hero_power_minion_type",
			Type = typeof(int)
		}
	};

	public int DataModelId => 140;

	public string DataModelDisplayName => "baconpastgamestats";

	public CardDataModel Hero
	{
		get
		{
			return m_Hero;
		}
		set
		{
			if (m_Hero != value)
			{
				RemoveNestedDataModel(m_Hero);
				RegisterNestedDataModel(value);
				m_Hero = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel HeroPower
	{
		get
		{
			return m_HeroPower;
		}
		set
		{
			if (m_HeroPower != value)
			{
				RemoveNestedDataModel(m_HeroPower);
				RegisterNestedDataModel(value);
				m_HeroPower = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Place
	{
		get
		{
			return m_Place;
		}
		set
		{
			if (m_Place != value)
			{
				m_Place = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<CardDataModel> Minions
	{
		get
		{
			return m_Minions;
		}
		set
		{
			if (m_Minions != value)
			{
				RemoveNestedDataModel(m_Minions);
				RegisterNestedDataModel(value);
				m_Minions = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string HeroName
	{
		get
		{
			return m_HeroName;
		}
		set
		{
			if (!(m_HeroName == value))
			{
				m_HeroName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel Quest
	{
		get
		{
			return m_Quest;
		}
		set
		{
			if (m_Quest != value)
			{
				RemoveNestedDataModel(m_Quest);
				RegisterNestedDataModel(value);
				m_Quest = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel Reward
	{
		get
		{
			return m_Reward;
		}
		set
		{
			if (m_Reward != value)
			{
				RemoveNestedDataModel(m_Reward);
				RegisterNestedDataModel(value);
				m_Reward = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool RewardCompleted
	{
		get
		{
			return m_RewardCompleted;
		}
		set
		{
			if (m_RewardCompleted != value)
			{
				m_RewardCompleted = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int HeroPowerRewardCardDatabaseID
	{
		get
		{
			return m_HeroPowerRewardCardDatabaseID;
		}
		set
		{
			if (m_HeroPowerRewardCardDatabaseID != value)
			{
				m_HeroPowerRewardCardDatabaseID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int HeroPowerRewardMinionType
	{
		get
		{
			return m_HeroPowerRewardMinionType;
		}
		set
		{
			if (m_HeroPowerRewardMinionType != value)
			{
				m_HeroPowerRewardMinionType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int RewardCardDatabaseID
	{
		get
		{
			return m_RewardCardDatabaseID;
		}
		set
		{
			if (m_RewardCardDatabaseID != value)
			{
				m_RewardCardDatabaseID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int RewardMinionType
	{
		get
		{
			return m_RewardMinionType;
		}
		set
		{
			if (m_RewardMinionType != value)
			{
				m_RewardMinionType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int QuestRace1
	{
		get
		{
			return m_QuestRace1;
		}
		set
		{
			if (m_QuestRace1 != value)
			{
				m_QuestRace1 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int QuestRace2
	{
		get
		{
			return m_QuestRace2;
		}
		set
		{
			if (m_QuestRace2 != value)
			{
				m_QuestRace2 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int QuestProgressTotal
	{
		get
		{
			return m_QuestProgressTotal;
		}
		set
		{
			if (m_QuestProgressTotal != value)
			{
				m_QuestProgressTotal = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int HeroPowerQuestRace1
	{
		get
		{
			return m_HeroPowerQuestRace1;
		}
		set
		{
			if (m_HeroPowerQuestRace1 != value)
			{
				m_HeroPowerQuestRace1 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int HeroPowerQuestRace2
	{
		get
		{
			return m_HeroPowerQuestRace2;
		}
		set
		{
			if (m_HeroPowerQuestRace2 != value)
			{
				m_HeroPowerQuestRace2 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int HeroPowerQuestProgressTotal
	{
		get
		{
			return m_HeroPowerQuestProgressTotal;
		}
		set
		{
			if (m_HeroPowerQuestProgressTotal != value)
			{
				m_HeroPowerQuestProgressTotal = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel HeroBuddyMeter
	{
		get
		{
			return m_HeroBuddyMeter;
		}
		set
		{
			if (m_HeroBuddyMeter != value)
			{
				RemoveNestedDataModel(m_HeroBuddyMeter);
				RegisterNestedDataModel(value);
				m_HeroBuddyMeter = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NumHeroBuddiesGained
	{
		get
		{
			return m_NumHeroBuddiesGained;
		}
		set
		{
			if (m_NumHeroBuddiesGained != value)
			{
				m_NumHeroBuddiesGained = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel HeroBuddy
	{
		get
		{
			return m_HeroBuddy;
		}
		set
		{
			if (m_HeroBuddy != value)
			{
				RemoveNestedDataModel(m_HeroBuddy);
				RegisterNestedDataModel(value);
				m_HeroBuddy = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int HeroBuddyDatabaseID
	{
		get
		{
			return m_HeroBuddyDatabaseID;
		}
		set
		{
			if (m_HeroBuddyDatabaseID != value)
			{
				m_HeroBuddyDatabaseID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int HeroBuddyCost
	{
		get
		{
			return m_HeroBuddyCost;
		}
		set
		{
			if (m_HeroBuddyCost != value)
			{
				m_HeroBuddyCost = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel Anomaly
	{
		get
		{
			return m_Anomaly;
		}
		set
		{
			if (m_Anomaly != value)
			{
				RemoveNestedDataModel(m_Anomaly);
				RegisterNestedDataModel(value);
				m_Anomaly = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel TeammateHero
	{
		get
		{
			return m_TeammateHero;
		}
		set
		{
			if (m_TeammateHero != value)
			{
				RemoveNestedDataModel(m_TeammateHero);
				RegisterNestedDataModel(value);
				m_TeammateHero = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel TeammateHeroPower
	{
		get
		{
			return m_TeammateHeroPower;
		}
		set
		{
			if (m_TeammateHeroPower != value)
			{
				RemoveNestedDataModel(m_TeammateHeroPower);
				RegisterNestedDataModel(value);
				m_TeammateHeroPower = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<CardDataModel> TeammateMinions
	{
		get
		{
			return m_TeammateMinions;
		}
		set
		{
			if (m_TeammateMinions != value)
			{
				RemoveNestedDataModel(m_TeammateMinions);
				RegisterNestedDataModel(value);
				m_TeammateMinions = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string TeammateHeroName
	{
		get
		{
			return m_TeammateHeroName;
		}
		set
		{
			if (!(m_TeammateHeroName == value))
			{
				m_TeammateHeroName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel TeammateQuest
	{
		get
		{
			return m_TeammateQuest;
		}
		set
		{
			if (m_TeammateQuest != value)
			{
				RemoveNestedDataModel(m_TeammateQuest);
				RegisterNestedDataModel(value);
				m_TeammateQuest = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel TeammateReward
	{
		get
		{
			return m_TeammateReward;
		}
		set
		{
			if (m_TeammateReward != value)
			{
				RemoveNestedDataModel(m_TeammateReward);
				RegisterNestedDataModel(value);
				m_TeammateReward = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool TeammateRewardCompleted
	{
		get
		{
			return m_TeammateRewardCompleted;
		}
		set
		{
			if (m_TeammateRewardCompleted != value)
			{
				m_TeammateRewardCompleted = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateHeroPowerRewardCardDatabaseID
	{
		get
		{
			return m_TeammateHeroPowerRewardCardDatabaseID;
		}
		set
		{
			if (m_TeammateHeroPowerRewardCardDatabaseID != value)
			{
				m_TeammateHeroPowerRewardCardDatabaseID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateHeroPowerRewardMinionType
	{
		get
		{
			return m_TeammateHeroPowerRewardMinionType;
		}
		set
		{
			if (m_TeammateHeroPowerRewardMinionType != value)
			{
				m_TeammateHeroPowerRewardMinionType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateRewardCardDatabaseID
	{
		get
		{
			return m_TeammateRewardCardDatabaseID;
		}
		set
		{
			if (m_TeammateRewardCardDatabaseID != value)
			{
				m_TeammateRewardCardDatabaseID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateRewardMinionType
	{
		get
		{
			return m_TeammateRewardMinionType;
		}
		set
		{
			if (m_TeammateRewardMinionType != value)
			{
				m_TeammateRewardMinionType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateQuestRace1
	{
		get
		{
			return m_TeammateQuestRace1;
		}
		set
		{
			if (m_TeammateQuestRace1 != value)
			{
				m_TeammateQuestRace1 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateQuestRace2
	{
		get
		{
			return m_TeammateQuestRace2;
		}
		set
		{
			if (m_TeammateQuestRace2 != value)
			{
				m_TeammateQuestRace2 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateQuestProgressTotal
	{
		get
		{
			return m_TeammateQuestProgressTotal;
		}
		set
		{
			if (m_TeammateQuestProgressTotal != value)
			{
				m_TeammateQuestProgressTotal = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateHeroPowerQuestRace1
	{
		get
		{
			return m_TeammateHeroPowerQuestRace1;
		}
		set
		{
			if (m_TeammateHeroPowerQuestRace1 != value)
			{
				m_TeammateHeroPowerQuestRace1 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateHeroPowerQuestRace2
	{
		get
		{
			return m_TeammateHeroPowerQuestRace2;
		}
		set
		{
			if (m_TeammateHeroPowerQuestRace2 != value)
			{
				m_TeammateHeroPowerQuestRace2 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateHeroPowerQuestProgressTotal
	{
		get
		{
			return m_TeammateHeroPowerQuestProgressTotal;
		}
		set
		{
			if (m_TeammateHeroPowerQuestProgressTotal != value)
			{
				m_TeammateHeroPowerQuestProgressTotal = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel TeammateHeroBuddyMeter
	{
		get
		{
			return m_TeammateHeroBuddyMeter;
		}
		set
		{
			if (m_TeammateHeroBuddyMeter != value)
			{
				RemoveNestedDataModel(m_TeammateHeroBuddyMeter);
				RegisterNestedDataModel(value);
				m_TeammateHeroBuddyMeter = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateNumHeroBuddiesGained
	{
		get
		{
			return m_TeammateNumHeroBuddiesGained;
		}
		set
		{
			if (m_TeammateNumHeroBuddiesGained != value)
			{
				m_TeammateNumHeroBuddiesGained = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel TeammateHeroBuddy
	{
		get
		{
			return m_TeammateHeroBuddy;
		}
		set
		{
			if (m_TeammateHeroBuddy != value)
			{
				RemoveNestedDataModel(m_TeammateHeroBuddy);
				RegisterNestedDataModel(value);
				m_TeammateHeroBuddy = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateHeroBuddyDatabaseID
	{
		get
		{
			return m_TeammateHeroBuddyDatabaseID;
		}
		set
		{
			if (m_TeammateHeroBuddyDatabaseID != value)
			{
				m_TeammateHeroBuddyDatabaseID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateHeroBuddyCost
	{
		get
		{
			return m_TeammateHeroBuddyCost;
		}
		set
		{
			if (m_TeammateHeroBuddyCost != value)
			{
				m_TeammateHeroBuddyCost = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel Trinket1
	{
		get
		{
			return m_Trinket1;
		}
		set
		{
			if (m_Trinket1 != value)
			{
				RemoveNestedDataModel(m_Trinket1);
				RegisterNestedDataModel(value);
				m_Trinket1 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel Trinket2
	{
		get
		{
			return m_Trinket2;
		}
		set
		{
			if (m_Trinket2 != value)
			{
				RemoveNestedDataModel(m_Trinket2);
				RegisterNestedDataModel(value);
				m_Trinket2 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel TrinketHeroPower
	{
		get
		{
			return m_TrinketHeroPower;
		}
		set
		{
			if (m_TrinketHeroPower != value)
			{
				RemoveNestedDataModel(m_TrinketHeroPower);
				RegisterNestedDataModel(value);
				m_TrinketHeroPower = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel TeammateTrinket1
	{
		get
		{
			return m_TeammateTrinket1;
		}
		set
		{
			if (m_TeammateTrinket1 != value)
			{
				RemoveNestedDataModel(m_TeammateTrinket1);
				RegisterNestedDataModel(value);
				m_TeammateTrinket1 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel TeammateTrinket2
	{
		get
		{
			return m_TeammateTrinket2;
		}
		set
		{
			if (m_TeammateTrinket2 != value)
			{
				RemoveNestedDataModel(m_TeammateTrinket2);
				RegisterNestedDataModel(value);
				m_TeammateTrinket2 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel TeammateTrinketHeroPower
	{
		get
		{
			return m_TeammateTrinketHeroPower;
		}
		set
		{
			if (m_TeammateTrinketHeroPower != value)
			{
				RemoveNestedDataModel(m_TeammateTrinketHeroPower);
				RegisterNestedDataModel(value);
				m_TeammateTrinketHeroPower = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Trinket1MinionType
	{
		get
		{
			return m_Trinket1MinionType;
		}
		set
		{
			if (m_Trinket1MinionType != value)
			{
				m_Trinket1MinionType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Trinket2MinionType
	{
		get
		{
			return m_Trinket2MinionType;
		}
		set
		{
			if (m_Trinket2MinionType != value)
			{
				m_Trinket2MinionType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TrinketHeroPowerMinionType
	{
		get
		{
			return m_TrinketHeroPowerMinionType;
		}
		set
		{
			if (m_TrinketHeroPowerMinionType != value)
			{
				m_TrinketHeroPowerMinionType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateTrinket1MinionType
	{
		get
		{
			return m_TeammateTrinket1MinionType;
		}
		set
		{
			if (m_TeammateTrinket1MinionType != value)
			{
				m_TeammateTrinket1MinionType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateTrinket2MinionType
	{
		get
		{
			return m_TeammateTrinket2MinionType;
		}
		set
		{
			if (m_TeammateTrinket2MinionType != value)
			{
				m_TeammateTrinket2MinionType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TeammateTrinketHeroPowerMinionType
	{
		get
		{
			return m_TeammateTrinketHeroPowerMinionType;
		}
		set
		{
			if (m_TeammateTrinketHeroPowerMinionType != value)
			{
				m_TeammateTrinketHeroPowerMinionType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public BaconPastGameStatsDataModel()
	{
		RegisterNestedDataModel(m_Hero);
		RegisterNestedDataModel(m_HeroPower);
		RegisterNestedDataModel(m_Minions);
		RegisterNestedDataModel(m_Quest);
		RegisterNestedDataModel(m_Reward);
		RegisterNestedDataModel(m_HeroBuddyMeter);
		RegisterNestedDataModel(m_HeroBuddy);
		RegisterNestedDataModel(m_Anomaly);
		RegisterNestedDataModel(m_TeammateHero);
		RegisterNestedDataModel(m_TeammateHeroPower);
		RegisterNestedDataModel(m_TeammateMinions);
		RegisterNestedDataModel(m_TeammateQuest);
		RegisterNestedDataModel(m_TeammateReward);
		RegisterNestedDataModel(m_TeammateHeroBuddyMeter);
		RegisterNestedDataModel(m_TeammateHeroBuddy);
		RegisterNestedDataModel(m_Trinket1);
		RegisterNestedDataModel(m_Trinket2);
		RegisterNestedDataModel(m_TrinketHeroPower);
		RegisterNestedDataModel(m_TeammateTrinket1);
		RegisterNestedDataModel(m_TeammateTrinket2);
		RegisterNestedDataModel(m_TeammateTrinketHeroPower);
	}

	public int GetPropertiesHashCode()
	{
		int num = ((17 * 31 + ((m_Hero != null) ? m_Hero.GetPropertiesHashCode() : 0)) * 31 + ((m_HeroPower != null) ? m_HeroPower.GetPropertiesHashCode() : 0)) * 31;
		_ = m_Place;
		int num2 = (((((num + m_Place.GetHashCode()) * 31 + ((m_Minions != null) ? m_Minions.GetPropertiesHashCode() : 0)) * 31 + ((m_HeroName != null) ? m_HeroName.GetHashCode() : 0)) * 31 + ((m_Quest != null) ? m_Quest.GetPropertiesHashCode() : 0)) * 31 + ((m_Reward != null) ? m_Reward.GetPropertiesHashCode() : 0)) * 31;
		_ = m_RewardCompleted;
		int num3 = (num2 + m_RewardCompleted.GetHashCode()) * 31;
		_ = m_HeroPowerRewardCardDatabaseID;
		int num4 = (num3 + m_HeroPowerRewardCardDatabaseID.GetHashCode()) * 31;
		_ = m_HeroPowerRewardMinionType;
		int num5 = (num4 + m_HeroPowerRewardMinionType.GetHashCode()) * 31;
		_ = m_RewardCardDatabaseID;
		int num6 = (num5 + m_RewardCardDatabaseID.GetHashCode()) * 31;
		_ = m_RewardMinionType;
		int num7 = (num6 + m_RewardMinionType.GetHashCode()) * 31;
		_ = m_QuestRace1;
		int num8 = (num7 + m_QuestRace1.GetHashCode()) * 31;
		_ = m_QuestRace2;
		int num9 = (num8 + m_QuestRace2.GetHashCode()) * 31;
		_ = m_QuestProgressTotal;
		int num10 = (num9 + m_QuestProgressTotal.GetHashCode()) * 31;
		_ = m_HeroPowerQuestRace1;
		int num11 = (num10 + m_HeroPowerQuestRace1.GetHashCode()) * 31;
		_ = m_HeroPowerQuestRace2;
		int num12 = (num11 + m_HeroPowerQuestRace2.GetHashCode()) * 31;
		_ = m_HeroPowerQuestProgressTotal;
		int num13 = ((num12 + m_HeroPowerQuestProgressTotal.GetHashCode()) * 31 + ((m_HeroBuddyMeter != null) ? m_HeroBuddyMeter.GetPropertiesHashCode() : 0)) * 31;
		_ = m_NumHeroBuddiesGained;
		int num14 = ((num13 + m_NumHeroBuddiesGained.GetHashCode()) * 31 + ((m_HeroBuddy != null) ? m_HeroBuddy.GetPropertiesHashCode() : 0)) * 31;
		_ = m_HeroBuddyDatabaseID;
		int num15 = (num14 + m_HeroBuddyDatabaseID.GetHashCode()) * 31;
		_ = m_HeroBuddyCost;
		int num16 = ((((((((num15 + m_HeroBuddyCost.GetHashCode()) * 31 + ((m_Anomaly != null) ? m_Anomaly.GetPropertiesHashCode() : 0)) * 31 + ((m_TeammateHero != null) ? m_TeammateHero.GetPropertiesHashCode() : 0)) * 31 + ((m_TeammateHeroPower != null) ? m_TeammateHeroPower.GetPropertiesHashCode() : 0)) * 31 + ((m_TeammateMinions != null) ? m_TeammateMinions.GetPropertiesHashCode() : 0)) * 31 + ((m_TeammateHeroName != null) ? m_TeammateHeroName.GetHashCode() : 0)) * 31 + ((m_TeammateQuest != null) ? m_TeammateQuest.GetPropertiesHashCode() : 0)) * 31 + ((m_TeammateReward != null) ? m_TeammateReward.GetPropertiesHashCode() : 0)) * 31;
		_ = m_TeammateRewardCompleted;
		int num17 = (num16 + m_TeammateRewardCompleted.GetHashCode()) * 31;
		_ = m_TeammateHeroPowerRewardCardDatabaseID;
		int num18 = (num17 + m_TeammateHeroPowerRewardCardDatabaseID.GetHashCode()) * 31;
		_ = m_TeammateHeroPowerRewardMinionType;
		int num19 = (num18 + m_TeammateHeroPowerRewardMinionType.GetHashCode()) * 31;
		_ = m_TeammateRewardCardDatabaseID;
		int num20 = (num19 + m_TeammateRewardCardDatabaseID.GetHashCode()) * 31;
		_ = m_TeammateRewardMinionType;
		int num21 = (num20 + m_TeammateRewardMinionType.GetHashCode()) * 31;
		_ = m_TeammateQuestRace1;
		int num22 = (num21 + m_TeammateQuestRace1.GetHashCode()) * 31;
		_ = m_TeammateQuestRace2;
		int num23 = (num22 + m_TeammateQuestRace2.GetHashCode()) * 31;
		_ = m_TeammateQuestProgressTotal;
		int num24 = (num23 + m_TeammateQuestProgressTotal.GetHashCode()) * 31;
		_ = m_TeammateHeroPowerQuestRace1;
		int num25 = (num24 + m_TeammateHeroPowerQuestRace1.GetHashCode()) * 31;
		_ = m_TeammateHeroPowerQuestRace2;
		int num26 = (num25 + m_TeammateHeroPowerQuestRace2.GetHashCode()) * 31;
		_ = m_TeammateHeroPowerQuestProgressTotal;
		int num27 = ((num26 + m_TeammateHeroPowerQuestProgressTotal.GetHashCode()) * 31 + ((m_TeammateHeroBuddyMeter != null) ? m_TeammateHeroBuddyMeter.GetPropertiesHashCode() : 0)) * 31;
		_ = m_TeammateNumHeroBuddiesGained;
		int num28 = ((num27 + m_TeammateNumHeroBuddiesGained.GetHashCode()) * 31 + ((m_TeammateHeroBuddy != null) ? m_TeammateHeroBuddy.GetPropertiesHashCode() : 0)) * 31;
		_ = m_TeammateHeroBuddyDatabaseID;
		int num29 = (num28 + m_TeammateHeroBuddyDatabaseID.GetHashCode()) * 31;
		_ = m_TeammateHeroBuddyCost;
		int num30 = (((((((num29 + m_TeammateHeroBuddyCost.GetHashCode()) * 31 + ((m_Trinket1 != null) ? m_Trinket1.GetPropertiesHashCode() : 0)) * 31 + ((m_Trinket2 != null) ? m_Trinket2.GetPropertiesHashCode() : 0)) * 31 + ((m_TrinketHeroPower != null) ? m_TrinketHeroPower.GetPropertiesHashCode() : 0)) * 31 + ((m_TeammateTrinket1 != null) ? m_TeammateTrinket1.GetPropertiesHashCode() : 0)) * 31 + ((m_TeammateTrinket2 != null) ? m_TeammateTrinket2.GetPropertiesHashCode() : 0)) * 31 + ((m_TeammateTrinketHeroPower != null) ? m_TeammateTrinketHeroPower.GetPropertiesHashCode() : 0)) * 31;
		_ = m_Trinket1MinionType;
		int num31 = (num30 + m_Trinket1MinionType.GetHashCode()) * 31;
		_ = m_Trinket2MinionType;
		int num32 = (num31 + m_Trinket2MinionType.GetHashCode()) * 31;
		_ = m_TrinketHeroPowerMinionType;
		int num33 = (num32 + m_TrinketHeroPowerMinionType.GetHashCode()) * 31;
		_ = m_TeammateTrinket1MinionType;
		int num34 = (num33 + m_TeammateTrinket1MinionType.GetHashCode()) * 31;
		_ = m_TeammateTrinket2MinionType;
		int num35 = (num34 + m_TeammateTrinket2MinionType.GetHashCode()) * 31;
		_ = m_TeammateTrinketHeroPowerMinionType;
		return num35 + m_TeammateTrinketHeroPowerMinionType.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 1:
			value = m_Hero;
			return true;
		case 2:
			value = m_HeroPower;
			return true;
		case 3:
			value = m_Place;
			return true;
		case 4:
			value = m_Minions;
			return true;
		case 5:
			value = m_HeroName;
			return true;
		case 6:
			value = m_Quest;
			return true;
		case 7:
			value = m_Reward;
			return true;
		case 8:
			value = m_RewardCompleted;
			return true;
		case 9:
			value = m_HeroPowerRewardCardDatabaseID;
			return true;
		case 10:
			value = m_HeroPowerRewardMinionType;
			return true;
		case 11:
			value = m_RewardCardDatabaseID;
			return true;
		case 12:
			value = m_RewardMinionType;
			return true;
		case 13:
			value = m_QuestRace1;
			return true;
		case 14:
			value = m_QuestRace2;
			return true;
		case 15:
			value = m_QuestProgressTotal;
			return true;
		case 16:
			value = m_HeroPowerQuestRace1;
			return true;
		case 17:
			value = m_HeroPowerQuestRace2;
			return true;
		case 18:
			value = m_HeroPowerQuestProgressTotal;
			return true;
		case 19:
			value = m_HeroBuddyMeter;
			return true;
		case 20:
			value = m_NumHeroBuddiesGained;
			return true;
		case 21:
			value = m_HeroBuddy;
			return true;
		case 22:
			value = m_HeroBuddyDatabaseID;
			return true;
		case 23:
			value = m_HeroBuddyCost;
			return true;
		case 24:
			value = m_Anomaly;
			return true;
		case 25:
			value = m_TeammateHero;
			return true;
		case 26:
			value = m_TeammateHeroPower;
			return true;
		case 27:
			value = m_TeammateMinions;
			return true;
		case 28:
			value = m_TeammateHeroName;
			return true;
		case 29:
			value = m_TeammateQuest;
			return true;
		case 30:
			value = m_TeammateReward;
			return true;
		case 31:
			value = m_TeammateRewardCompleted;
			return true;
		case 32:
			value = m_TeammateHeroPowerRewardCardDatabaseID;
			return true;
		case 33:
			value = m_TeammateHeroPowerRewardMinionType;
			return true;
		case 34:
			value = m_TeammateRewardCardDatabaseID;
			return true;
		case 35:
			value = m_TeammateRewardMinionType;
			return true;
		case 36:
			value = m_TeammateQuestRace1;
			return true;
		case 37:
			value = m_TeammateQuestRace2;
			return true;
		case 38:
			value = m_TeammateQuestProgressTotal;
			return true;
		case 39:
			value = m_TeammateHeroPowerQuestRace1;
			return true;
		case 40:
			value = m_TeammateHeroPowerQuestRace2;
			return true;
		case 41:
			value = m_TeammateHeroPowerQuestProgressTotal;
			return true;
		case 42:
			value = m_TeammateHeroBuddyMeter;
			return true;
		case 43:
			value = m_TeammateNumHeroBuddiesGained;
			return true;
		case 44:
			value = m_TeammateHeroBuddy;
			return true;
		case 45:
			value = m_TeammateHeroBuddyDatabaseID;
			return true;
		case 46:
			value = m_TeammateHeroBuddyCost;
			return true;
		case 47:
			value = m_Trinket1;
			return true;
		case 48:
			value = m_Trinket2;
			return true;
		case 49:
			value = m_TrinketHeroPower;
			return true;
		case 50:
			value = m_TeammateTrinket1;
			return true;
		case 51:
			value = m_TeammateTrinket2;
			return true;
		case 52:
			value = m_TeammateTrinketHeroPower;
			return true;
		case 53:
			value = m_Trinket1MinionType;
			return true;
		case 54:
			value = m_Trinket2MinionType;
			return true;
		case 55:
			value = m_TrinketHeroPowerMinionType;
			return true;
		case 56:
			value = m_TeammateTrinket1MinionType;
			return true;
		case 57:
			value = m_TeammateTrinket2MinionType;
			return true;
		case 58:
			value = m_TeammateTrinketHeroPowerMinionType;
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
			Hero = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 2:
			HeroPower = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 3:
			Place = ((value != null) ? ((int)value) : 0);
			return true;
		case 4:
			Minions = ((value != null) ? ((DataModelList<CardDataModel>)value) : null);
			return true;
		case 5:
			HeroName = ((value != null) ? ((string)value) : null);
			return true;
		case 6:
			Quest = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 7:
			Reward = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 8:
			RewardCompleted = value != null && (bool)value;
			return true;
		case 9:
			HeroPowerRewardCardDatabaseID = ((value != null) ? ((int)value) : 0);
			return true;
		case 10:
			HeroPowerRewardMinionType = ((value != null) ? ((int)value) : 0);
			return true;
		case 11:
			RewardCardDatabaseID = ((value != null) ? ((int)value) : 0);
			return true;
		case 12:
			RewardMinionType = ((value != null) ? ((int)value) : 0);
			return true;
		case 13:
			QuestRace1 = ((value != null) ? ((int)value) : 0);
			return true;
		case 14:
			QuestRace2 = ((value != null) ? ((int)value) : 0);
			return true;
		case 15:
			QuestProgressTotal = ((value != null) ? ((int)value) : 0);
			return true;
		case 16:
			HeroPowerQuestRace1 = ((value != null) ? ((int)value) : 0);
			return true;
		case 17:
			HeroPowerQuestRace2 = ((value != null) ? ((int)value) : 0);
			return true;
		case 18:
			HeroPowerQuestProgressTotal = ((value != null) ? ((int)value) : 0);
			return true;
		case 19:
			HeroBuddyMeter = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 20:
			NumHeroBuddiesGained = ((value != null) ? ((int)value) : 0);
			return true;
		case 21:
			HeroBuddy = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 22:
			HeroBuddyDatabaseID = ((value != null) ? ((int)value) : 0);
			return true;
		case 23:
			HeroBuddyCost = ((value != null) ? ((int)value) : 0);
			return true;
		case 24:
			Anomaly = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 25:
			TeammateHero = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 26:
			TeammateHeroPower = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 27:
			TeammateMinions = ((value != null) ? ((DataModelList<CardDataModel>)value) : null);
			return true;
		case 28:
			TeammateHeroName = ((value != null) ? ((string)value) : null);
			return true;
		case 29:
			TeammateQuest = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 30:
			TeammateReward = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 31:
			TeammateRewardCompleted = value != null && (bool)value;
			return true;
		case 32:
			TeammateHeroPowerRewardCardDatabaseID = ((value != null) ? ((int)value) : 0);
			return true;
		case 33:
			TeammateHeroPowerRewardMinionType = ((value != null) ? ((int)value) : 0);
			return true;
		case 34:
			TeammateRewardCardDatabaseID = ((value != null) ? ((int)value) : 0);
			return true;
		case 35:
			TeammateRewardMinionType = ((value != null) ? ((int)value) : 0);
			return true;
		case 36:
			TeammateQuestRace1 = ((value != null) ? ((int)value) : 0);
			return true;
		case 37:
			TeammateQuestRace2 = ((value != null) ? ((int)value) : 0);
			return true;
		case 38:
			TeammateQuestProgressTotal = ((value != null) ? ((int)value) : 0);
			return true;
		case 39:
			TeammateHeroPowerQuestRace1 = ((value != null) ? ((int)value) : 0);
			return true;
		case 40:
			TeammateHeroPowerQuestRace2 = ((value != null) ? ((int)value) : 0);
			return true;
		case 41:
			TeammateHeroPowerQuestProgressTotal = ((value != null) ? ((int)value) : 0);
			return true;
		case 42:
			TeammateHeroBuddyMeter = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 43:
			TeammateNumHeroBuddiesGained = ((value != null) ? ((int)value) : 0);
			return true;
		case 44:
			TeammateHeroBuddy = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 45:
			TeammateHeroBuddyDatabaseID = ((value != null) ? ((int)value) : 0);
			return true;
		case 46:
			TeammateHeroBuddyCost = ((value != null) ? ((int)value) : 0);
			return true;
		case 47:
			Trinket1 = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 48:
			Trinket2 = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 49:
			TrinketHeroPower = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 50:
			TeammateTrinket1 = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 51:
			TeammateTrinket2 = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 52:
			TeammateTrinketHeroPower = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 53:
			Trinket1MinionType = ((value != null) ? ((int)value) : 0);
			return true;
		case 54:
			Trinket2MinionType = ((value != null) ? ((int)value) : 0);
			return true;
		case 55:
			TrinketHeroPowerMinionType = ((value != null) ? ((int)value) : 0);
			return true;
		case 56:
			TeammateTrinket1MinionType = ((value != null) ? ((int)value) : 0);
			return true;
		case 57:
			TeammateTrinket2MinionType = ((value != null) ? ((int)value) : 0);
			return true;
		case 58:
			TeammateTrinketHeroPowerMinionType = ((value != null) ? ((int)value) : 0);
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
		case 11:
			info = Properties[10];
			return true;
		case 12:
			info = Properties[11];
			return true;
		case 13:
			info = Properties[12];
			return true;
		case 14:
			info = Properties[13];
			return true;
		case 15:
			info = Properties[14];
			return true;
		case 16:
			info = Properties[15];
			return true;
		case 17:
			info = Properties[16];
			return true;
		case 18:
			info = Properties[17];
			return true;
		case 19:
			info = Properties[18];
			return true;
		case 20:
			info = Properties[19];
			return true;
		case 21:
			info = Properties[20];
			return true;
		case 22:
			info = Properties[21];
			return true;
		case 23:
			info = Properties[22];
			return true;
		case 24:
			info = Properties[23];
			return true;
		case 25:
			info = Properties[24];
			return true;
		case 26:
			info = Properties[25];
			return true;
		case 27:
			info = Properties[26];
			return true;
		case 28:
			info = Properties[27];
			return true;
		case 29:
			info = Properties[28];
			return true;
		case 30:
			info = Properties[29];
			return true;
		case 31:
			info = Properties[30];
			return true;
		case 32:
			info = Properties[31];
			return true;
		case 33:
			info = Properties[32];
			return true;
		case 34:
			info = Properties[33];
			return true;
		case 35:
			info = Properties[34];
			return true;
		case 36:
			info = Properties[35];
			return true;
		case 37:
			info = Properties[36];
			return true;
		case 38:
			info = Properties[37];
			return true;
		case 39:
			info = Properties[38];
			return true;
		case 40:
			info = Properties[39];
			return true;
		case 41:
			info = Properties[40];
			return true;
		case 42:
			info = Properties[41];
			return true;
		case 43:
			info = Properties[42];
			return true;
		case 44:
			info = Properties[43];
			return true;
		case 45:
			info = Properties[44];
			return true;
		case 46:
			info = Properties[45];
			return true;
		case 47:
			info = Properties[46];
			return true;
		case 48:
			info = Properties[47];
			return true;
		case 49:
			info = Properties[48];
			return true;
		case 50:
			info = Properties[49];
			return true;
		case 51:
			info = Properties[50];
			return true;
		case 52:
			info = Properties[51];
			return true;
		case 53:
			info = Properties[52];
			return true;
		case 54:
			info = Properties[53];
			return true;
		case 55:
			info = Properties[54];
			return true;
		case 56:
			info = Properties[55];
			return true;
		case 57:
			info = Properties[56];
			return true;
		case 58:
			info = Properties[57];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
