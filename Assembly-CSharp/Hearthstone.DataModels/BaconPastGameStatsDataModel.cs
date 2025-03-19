using System.Collections.Generic;
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

	private CardDataModel m_DualHeroPower;

	private CardDataModel m_TeammateDualHeroPower;

	private DataModelProperty[] m_properties = new DataModelProperty[60]
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
		},
		new DataModelProperty
		{
			PropertyId = 59,
			PropertyDisplayName = "dual_hero_power",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 60,
			PropertyDisplayName = "teammate_dual_hero_power",
			Type = typeof(CardDataModel)
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

	public CardDataModel DualHeroPower
	{
		get
		{
			return m_DualHeroPower;
		}
		set
		{
			if (m_DualHeroPower != value)
			{
				RemoveNestedDataModel(m_DualHeroPower);
				RegisterNestedDataModel(value);
				m_DualHeroPower = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel TeammateDualHeroPower
	{
		get
		{
			return m_TeammateDualHeroPower;
		}
		set
		{
			if (m_TeammateDualHeroPower != value)
			{
				RemoveNestedDataModel(m_TeammateDualHeroPower);
				RegisterNestedDataModel(value);
				m_TeammateDualHeroPower = value;
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
		RegisterNestedDataModel(m_DualHeroPower);
		RegisterNestedDataModel(m_TeammateDualHeroPower);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_Hero != null && !inspectedDataModels.Contains(m_Hero.GetHashCode()))
		{
			inspectedDataModels.Add(m_Hero.GetHashCode());
			hash = hash * 31 + m_Hero.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_HeroPower != null && !inspectedDataModels.Contains(m_HeroPower.GetHashCode()))
		{
			inspectedDataModels.Add(m_HeroPower.GetHashCode());
			hash = hash * 31 + m_HeroPower.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_Place;
		hash = num + m_Place.GetHashCode();
		if (m_Minions != null && !inspectedDataModels.Contains(m_Minions.GetHashCode()))
		{
			inspectedDataModels.Add(m_Minions.GetHashCode());
			hash = hash * 31 + m_Minions.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		hash = hash * 31 + ((m_HeroName != null) ? m_HeroName.GetHashCode() : 0);
		if (m_Quest != null && !inspectedDataModels.Contains(m_Quest.GetHashCode()))
		{
			inspectedDataModels.Add(m_Quest.GetHashCode());
			hash = hash * 31 + m_Quest.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_Reward != null && !inspectedDataModels.Contains(m_Reward.GetHashCode()))
		{
			inspectedDataModels.Add(m_Reward.GetHashCode());
			hash = hash * 31 + m_Reward.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num2 = hash * 31;
		_ = m_RewardCompleted;
		hash = num2 + m_RewardCompleted.GetHashCode();
		int num3 = hash * 31;
		_ = m_HeroPowerRewardCardDatabaseID;
		hash = num3 + m_HeroPowerRewardCardDatabaseID.GetHashCode();
		int num4 = hash * 31;
		_ = m_HeroPowerRewardMinionType;
		hash = num4 + m_HeroPowerRewardMinionType.GetHashCode();
		int num5 = hash * 31;
		_ = m_RewardCardDatabaseID;
		hash = num5 + m_RewardCardDatabaseID.GetHashCode();
		int num6 = hash * 31;
		_ = m_RewardMinionType;
		hash = num6 + m_RewardMinionType.GetHashCode();
		int num7 = hash * 31;
		_ = m_QuestRace1;
		hash = num7 + m_QuestRace1.GetHashCode();
		int num8 = hash * 31;
		_ = m_QuestRace2;
		hash = num8 + m_QuestRace2.GetHashCode();
		int num9 = hash * 31;
		_ = m_QuestProgressTotal;
		hash = num9 + m_QuestProgressTotal.GetHashCode();
		int num10 = hash * 31;
		_ = m_HeroPowerQuestRace1;
		hash = num10 + m_HeroPowerQuestRace1.GetHashCode();
		int num11 = hash * 31;
		_ = m_HeroPowerQuestRace2;
		hash = num11 + m_HeroPowerQuestRace2.GetHashCode();
		int num12 = hash * 31;
		_ = m_HeroPowerQuestProgressTotal;
		hash = num12 + m_HeroPowerQuestProgressTotal.GetHashCode();
		if (m_HeroBuddyMeter != null && !inspectedDataModels.Contains(m_HeroBuddyMeter.GetHashCode()))
		{
			inspectedDataModels.Add(m_HeroBuddyMeter.GetHashCode());
			hash = hash * 31 + m_HeroBuddyMeter.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num13 = hash * 31;
		_ = m_NumHeroBuddiesGained;
		hash = num13 + m_NumHeroBuddiesGained.GetHashCode();
		if (m_HeroBuddy != null && !inspectedDataModels.Contains(m_HeroBuddy.GetHashCode()))
		{
			inspectedDataModels.Add(m_HeroBuddy.GetHashCode());
			hash = hash * 31 + m_HeroBuddy.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num14 = hash * 31;
		_ = m_HeroBuddyDatabaseID;
		hash = num14 + m_HeroBuddyDatabaseID.GetHashCode();
		int num15 = hash * 31;
		_ = m_HeroBuddyCost;
		hash = num15 + m_HeroBuddyCost.GetHashCode();
		if (m_Anomaly != null && !inspectedDataModels.Contains(m_Anomaly.GetHashCode()))
		{
			inspectedDataModels.Add(m_Anomaly.GetHashCode());
			hash = hash * 31 + m_Anomaly.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_TeammateHero != null && !inspectedDataModels.Contains(m_TeammateHero.GetHashCode()))
		{
			inspectedDataModels.Add(m_TeammateHero.GetHashCode());
			hash = hash * 31 + m_TeammateHero.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_TeammateHeroPower != null && !inspectedDataModels.Contains(m_TeammateHeroPower.GetHashCode()))
		{
			inspectedDataModels.Add(m_TeammateHeroPower.GetHashCode());
			hash = hash * 31 + m_TeammateHeroPower.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_TeammateMinions != null && !inspectedDataModels.Contains(m_TeammateMinions.GetHashCode()))
		{
			inspectedDataModels.Add(m_TeammateMinions.GetHashCode());
			hash = hash * 31 + m_TeammateMinions.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		hash = hash * 31 + ((m_TeammateHeroName != null) ? m_TeammateHeroName.GetHashCode() : 0);
		if (m_TeammateQuest != null && !inspectedDataModels.Contains(m_TeammateQuest.GetHashCode()))
		{
			inspectedDataModels.Add(m_TeammateQuest.GetHashCode());
			hash = hash * 31 + m_TeammateQuest.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_TeammateReward != null && !inspectedDataModels.Contains(m_TeammateReward.GetHashCode()))
		{
			inspectedDataModels.Add(m_TeammateReward.GetHashCode());
			hash = hash * 31 + m_TeammateReward.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num16 = hash * 31;
		_ = m_TeammateRewardCompleted;
		hash = num16 + m_TeammateRewardCompleted.GetHashCode();
		int num17 = hash * 31;
		_ = m_TeammateHeroPowerRewardCardDatabaseID;
		hash = num17 + m_TeammateHeroPowerRewardCardDatabaseID.GetHashCode();
		int num18 = hash * 31;
		_ = m_TeammateHeroPowerRewardMinionType;
		hash = num18 + m_TeammateHeroPowerRewardMinionType.GetHashCode();
		int num19 = hash * 31;
		_ = m_TeammateRewardCardDatabaseID;
		hash = num19 + m_TeammateRewardCardDatabaseID.GetHashCode();
		int num20 = hash * 31;
		_ = m_TeammateRewardMinionType;
		hash = num20 + m_TeammateRewardMinionType.GetHashCode();
		int num21 = hash * 31;
		_ = m_TeammateQuestRace1;
		hash = num21 + m_TeammateQuestRace1.GetHashCode();
		int num22 = hash * 31;
		_ = m_TeammateQuestRace2;
		hash = num22 + m_TeammateQuestRace2.GetHashCode();
		int num23 = hash * 31;
		_ = m_TeammateQuestProgressTotal;
		hash = num23 + m_TeammateQuestProgressTotal.GetHashCode();
		int num24 = hash * 31;
		_ = m_TeammateHeroPowerQuestRace1;
		hash = num24 + m_TeammateHeroPowerQuestRace1.GetHashCode();
		int num25 = hash * 31;
		_ = m_TeammateHeroPowerQuestRace2;
		hash = num25 + m_TeammateHeroPowerQuestRace2.GetHashCode();
		int num26 = hash * 31;
		_ = m_TeammateHeroPowerQuestProgressTotal;
		hash = num26 + m_TeammateHeroPowerQuestProgressTotal.GetHashCode();
		if (m_TeammateHeroBuddyMeter != null && !inspectedDataModels.Contains(m_TeammateHeroBuddyMeter.GetHashCode()))
		{
			inspectedDataModels.Add(m_TeammateHeroBuddyMeter.GetHashCode());
			hash = hash * 31 + m_TeammateHeroBuddyMeter.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num27 = hash * 31;
		_ = m_TeammateNumHeroBuddiesGained;
		hash = num27 + m_TeammateNumHeroBuddiesGained.GetHashCode();
		if (m_TeammateHeroBuddy != null && !inspectedDataModels.Contains(m_TeammateHeroBuddy.GetHashCode()))
		{
			inspectedDataModels.Add(m_TeammateHeroBuddy.GetHashCode());
			hash = hash * 31 + m_TeammateHeroBuddy.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num28 = hash * 31;
		_ = m_TeammateHeroBuddyDatabaseID;
		hash = num28 + m_TeammateHeroBuddyDatabaseID.GetHashCode();
		int num29 = hash * 31;
		_ = m_TeammateHeroBuddyCost;
		hash = num29 + m_TeammateHeroBuddyCost.GetHashCode();
		if (m_Trinket1 != null && !inspectedDataModels.Contains(m_Trinket1.GetHashCode()))
		{
			inspectedDataModels.Add(m_Trinket1.GetHashCode());
			hash = hash * 31 + m_Trinket1.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_Trinket2 != null && !inspectedDataModels.Contains(m_Trinket2.GetHashCode()))
		{
			inspectedDataModels.Add(m_Trinket2.GetHashCode());
			hash = hash * 31 + m_Trinket2.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_TrinketHeroPower != null && !inspectedDataModels.Contains(m_TrinketHeroPower.GetHashCode()))
		{
			inspectedDataModels.Add(m_TrinketHeroPower.GetHashCode());
			hash = hash * 31 + m_TrinketHeroPower.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_TeammateTrinket1 != null && !inspectedDataModels.Contains(m_TeammateTrinket1.GetHashCode()))
		{
			inspectedDataModels.Add(m_TeammateTrinket1.GetHashCode());
			hash = hash * 31 + m_TeammateTrinket1.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_TeammateTrinket2 != null && !inspectedDataModels.Contains(m_TeammateTrinket2.GetHashCode()))
		{
			inspectedDataModels.Add(m_TeammateTrinket2.GetHashCode());
			hash = hash * 31 + m_TeammateTrinket2.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_TeammateTrinketHeroPower != null && !inspectedDataModels.Contains(m_TeammateTrinketHeroPower.GetHashCode()))
		{
			inspectedDataModels.Add(m_TeammateTrinketHeroPower.GetHashCode());
			hash = hash * 31 + m_TeammateTrinketHeroPower.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num30 = hash * 31;
		_ = m_Trinket1MinionType;
		hash = num30 + m_Trinket1MinionType.GetHashCode();
		int num31 = hash * 31;
		_ = m_Trinket2MinionType;
		hash = num31 + m_Trinket2MinionType.GetHashCode();
		int num32 = hash * 31;
		_ = m_TrinketHeroPowerMinionType;
		hash = num32 + m_TrinketHeroPowerMinionType.GetHashCode();
		int num33 = hash * 31;
		_ = m_TeammateTrinket1MinionType;
		hash = num33 + m_TeammateTrinket1MinionType.GetHashCode();
		int num34 = hash * 31;
		_ = m_TeammateTrinket2MinionType;
		hash = num34 + m_TeammateTrinket2MinionType.GetHashCode();
		int num35 = hash * 31;
		_ = m_TeammateTrinketHeroPowerMinionType;
		hash = num35 + m_TeammateTrinketHeroPowerMinionType.GetHashCode();
		if (m_DualHeroPower != null && !inspectedDataModels.Contains(m_DualHeroPower.GetHashCode()))
		{
			inspectedDataModels.Add(m_DualHeroPower.GetHashCode());
			hash = hash * 31 + m_DualHeroPower.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_TeammateDualHeroPower != null && !inspectedDataModels.Contains(m_TeammateDualHeroPower.GetHashCode()))
		{
			inspectedDataModels.Add(m_TeammateDualHeroPower.GetHashCode());
			return hash * 31 + m_TeammateDualHeroPower.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
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
		case 59:
			value = m_DualHeroPower;
			return true;
		case 60:
			value = m_TeammateDualHeroPower;
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
		case 59:
			DualHeroPower = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 60:
			TeammateDualHeroPower = ((value != null) ? ((CardDataModel)value) : null);
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
		case 59:
			info = Properties[58];
			return true;
		case 60:
			info = Properties[59];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
