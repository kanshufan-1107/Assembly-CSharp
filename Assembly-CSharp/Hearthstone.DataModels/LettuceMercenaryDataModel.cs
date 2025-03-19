using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceMercenaryDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 216;

	private int m_MercenaryId;

	private string m_MercenaryName;

	private string m_MercenaryShortName;

	private TAG_ROLE m_MercenaryRole;

	private int m_MercenaryLevel;

	private TAG_RARITY m_MercenaryRarity;

	private bool m_ReadyForCrafting;

	private LettuceMercenaryCoinDataModel m_MercenaryCoin;

	private DataModelList<LettuceAbilityDataModel> m_AbilityList = new DataModelList<LettuceAbilityDataModel>();

	private int m_EquipmentSlotIndex;

	private bool m_DeadInMapRun;

	private int m_AbilitySlotUnlockThreshold;

	private DataModelList<LettuceAbilityDataModel> m_EquipmentList = new DataModelList<LettuceAbilityDataModel>();

	private bool m_MercenarySelected;

	private bool m_Owned;

	private int m_ExperienceInitial;

	private int m_ExperienceFinal;

	private bool m_FullyUpgradedInitial;

	private bool m_FullyUpgradedFinal;

	private bool m_InCurrentTeam;

	private bool m_ChildUpgradeAvailable;

	private bool m_IsAffectedBySlottedEquipment;

	private bool m_IsDisabled;

	private CardDataModel m_Card;

	private DataModelList<LettuceMercenaryArtVariationDataModel> m_ArtVariationList = new DataModelList<LettuceMercenaryArtVariationDataModel>();

	private int m_ArtVariationPageIndex;

	private DataModelList<LettuceMercenaryArtVariationPageDataModel> m_ArtVariationPageList = new DataModelList<LettuceMercenaryArtVariationPageDataModel>();

	private int m_CraftingCost;

	private bool m_IsAcquiredByCrafting;

	private CardDataModel m_TreasureCard;

	private bool m_HideXp;

	private bool m_HideWatermark;

	private bool m_HideStats;

	private string m_Label;

	private TAG_ACQUIRE_TYPE m_AcquireType;

	private bool m_ShowLevelInList;

	private bool m_ShowAbilityText;

	private string m_AbilityText;

	private float m_XPBarPercentage;

	private string m_CustomAcquireText;

	private bool m_ShowCustomAcquireText;

	private bool m_IsMaxLevel;

	private bool m_ShowAsNew;

	private int m_NumNewPortraits;

	private bool m_IsRemote;

	private DataModelList<MercenaryMythicTreasureDataModel> m_MythicTreasureList = new DataModelList<MercenaryMythicTreasureDataModel>();

	private long m_MythicLevel;

	private long m_MythicModifier;

	private bool m_MythicView;

	private bool m_IsMythicFirstUnlock;

	private bool m_MythicToggleEnable;

	private bool m_MythicToggle;

	private DataModelProperty[] m_properties = new DataModelProperty[52]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "mercenary_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "mercenary_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 21,
			PropertyDisplayName = "mercenary_short_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "mercenary_role",
			Type = typeof(TAG_ROLE)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "mercenary_level",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 29,
			PropertyDisplayName = "mercenary_rarity",
			Type = typeof(TAG_RARITY)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "ready_for_crafting",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "mercenary_coin",
			Type = typeof(LettuceMercenaryCoinDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "ability_list",
			Type = typeof(DataModelList<LettuceAbilityDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "equipment_slot_index",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "dead_in_map_run",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "ability_slot_unlock_threshold",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 13,
			PropertyDisplayName = "equipment_list",
			Type = typeof(DataModelList<LettuceAbilityDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 14,
			PropertyDisplayName = "mercenary_selected",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 15,
			PropertyDisplayName = "owned",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 16,
			PropertyDisplayName = "experience_initial",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 17,
			PropertyDisplayName = "experience_final",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 32,
			PropertyDisplayName = "fully_upgraded_initial",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 31,
			PropertyDisplayName = "fully_upgraded_final",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 18,
			PropertyDisplayName = "in_current_team",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 19,
			PropertyDisplayName = "child_upgrade_available",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 20,
			PropertyDisplayName = "is_affected_by_slotted_equipment",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 22,
			PropertyDisplayName = "is_disabled",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 23,
			PropertyDisplayName = "card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 26,
			PropertyDisplayName = "art_variation_list",
			Type = typeof(DataModelList<LettuceMercenaryArtVariationDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 28,
			PropertyDisplayName = "art_variation_page_index",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 27,
			PropertyDisplayName = "art_variation_page_list",
			Type = typeof(DataModelList<LettuceMercenaryArtVariationPageDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 25,
			PropertyDisplayName = "crafting_cost",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 37,
			PropertyDisplayName = "is_acquired_by_crafting",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 30,
			PropertyDisplayName = "treasure_card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 33,
			PropertyDisplayName = "hide_xp",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 34,
			PropertyDisplayName = "hide_watermark",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 35,
			PropertyDisplayName = "hide_stats",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 36,
			PropertyDisplayName = "label",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 38,
			PropertyDisplayName = "acquire_type",
			Type = typeof(TAG_ACQUIRE_TYPE)
		},
		new DataModelProperty
		{
			PropertyId = 577,
			PropertyDisplayName = "show_level_in_list",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 617,
			PropertyDisplayName = "show_ability_text",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 618,
			PropertyDisplayName = "ability_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 635,
			PropertyDisplayName = "xp_bar_percentage",
			Type = typeof(float)
		},
		new DataModelProperty
		{
			PropertyId = 640,
			PropertyDisplayName = "custom_acquire_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 641,
			PropertyDisplayName = "show_custom_acquire_text",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 649,
			PropertyDisplayName = "is_max_level",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 653,
			PropertyDisplayName = "show_as_new",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 659,
			PropertyDisplayName = "num_new_portraits",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 660,
			PropertyDisplayName = "is_remote",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 751,
			PropertyDisplayName = "mythic_treasure_list",
			Type = typeof(DataModelList<MercenaryMythicTreasureDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 756,
			PropertyDisplayName = "mythic_level",
			Type = typeof(long)
		},
		new DataModelProperty
		{
			PropertyId = 757,
			PropertyDisplayName = "mythic_modifier",
			Type = typeof(long)
		},
		new DataModelProperty
		{
			PropertyId = 761,
			PropertyDisplayName = "mythic_view",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 787,
			PropertyDisplayName = "is_mythic_first_unlock",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 791,
			PropertyDisplayName = "mythic_toggle_enable",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 790,
			PropertyDisplayName = "mythic_toggle",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 216;

	public string DataModelDisplayName => "lettuce_mercenary";

	public int MercenaryId
	{
		get
		{
			return m_MercenaryId;
		}
		set
		{
			if (m_MercenaryId != value)
			{
				m_MercenaryId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string MercenaryName
	{
		get
		{
			return m_MercenaryName;
		}
		set
		{
			if (!(m_MercenaryName == value))
			{
				m_MercenaryName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string MercenaryShortName
	{
		get
		{
			return m_MercenaryShortName;
		}
		set
		{
			if (!(m_MercenaryShortName == value))
			{
				m_MercenaryShortName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TAG_ROLE MercenaryRole
	{
		get
		{
			return m_MercenaryRole;
		}
		set
		{
			if (m_MercenaryRole != value)
			{
				m_MercenaryRole = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MercenaryLevel
	{
		get
		{
			return m_MercenaryLevel;
		}
		set
		{
			if (m_MercenaryLevel != value)
			{
				m_MercenaryLevel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TAG_RARITY MercenaryRarity
	{
		get
		{
			return m_MercenaryRarity;
		}
		set
		{
			if (m_MercenaryRarity != value)
			{
				m_MercenaryRarity = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ReadyForCrafting
	{
		get
		{
			return m_ReadyForCrafting;
		}
		set
		{
			if (m_ReadyForCrafting != value)
			{
				m_ReadyForCrafting = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceMercenaryCoinDataModel MercenaryCoin
	{
		get
		{
			return m_MercenaryCoin;
		}
		set
		{
			if (m_MercenaryCoin != value)
			{
				RemoveNestedDataModel(m_MercenaryCoin);
				RegisterNestedDataModel(value);
				m_MercenaryCoin = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<LettuceAbilityDataModel> AbilityList
	{
		get
		{
			return m_AbilityList;
		}
		set
		{
			if (m_AbilityList != value)
			{
				RemoveNestedDataModel(m_AbilityList);
				RegisterNestedDataModel(value);
				m_AbilityList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int EquipmentSlotIndex
	{
		get
		{
			return m_EquipmentSlotIndex;
		}
		set
		{
			if (m_EquipmentSlotIndex != value)
			{
				m_EquipmentSlotIndex = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool DeadInMapRun
	{
		get
		{
			return m_DeadInMapRun;
		}
		set
		{
			if (m_DeadInMapRun != value)
			{
				m_DeadInMapRun = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int AbilitySlotUnlockThreshold
	{
		get
		{
			return m_AbilitySlotUnlockThreshold;
		}
		set
		{
			if (m_AbilitySlotUnlockThreshold != value)
			{
				m_AbilitySlotUnlockThreshold = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<LettuceAbilityDataModel> EquipmentList
	{
		get
		{
			return m_EquipmentList;
		}
		set
		{
			if (m_EquipmentList != value)
			{
				RemoveNestedDataModel(m_EquipmentList);
				RegisterNestedDataModel(value);
				m_EquipmentList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool MercenarySelected
	{
		get
		{
			return m_MercenarySelected;
		}
		set
		{
			if (m_MercenarySelected != value)
			{
				m_MercenarySelected = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Owned
	{
		get
		{
			return m_Owned;
		}
		set
		{
			if (m_Owned != value)
			{
				m_Owned = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int ExperienceInitial
	{
		get
		{
			return m_ExperienceInitial;
		}
		set
		{
			if (m_ExperienceInitial != value)
			{
				m_ExperienceInitial = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int ExperienceFinal
	{
		get
		{
			return m_ExperienceFinal;
		}
		set
		{
			if (m_ExperienceFinal != value)
			{
				m_ExperienceFinal = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool FullyUpgradedInitial
	{
		get
		{
			return m_FullyUpgradedInitial;
		}
		set
		{
			if (m_FullyUpgradedInitial != value)
			{
				m_FullyUpgradedInitial = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool FullyUpgradedFinal
	{
		get
		{
			return m_FullyUpgradedFinal;
		}
		set
		{
			if (m_FullyUpgradedFinal != value)
			{
				m_FullyUpgradedFinal = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool InCurrentTeam
	{
		get
		{
			return m_InCurrentTeam;
		}
		set
		{
			if (m_InCurrentTeam != value)
			{
				m_InCurrentTeam = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ChildUpgradeAvailable
	{
		get
		{
			return m_ChildUpgradeAvailable;
		}
		set
		{
			if (m_ChildUpgradeAvailable != value)
			{
				m_ChildUpgradeAvailable = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsAffectedBySlottedEquipment
	{
		get
		{
			return m_IsAffectedBySlottedEquipment;
		}
		set
		{
			if (m_IsAffectedBySlottedEquipment != value)
			{
				m_IsAffectedBySlottedEquipment = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsDisabled
	{
		get
		{
			return m_IsDisabled;
		}
		set
		{
			if (m_IsDisabled != value)
			{
				m_IsDisabled = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel Card
	{
		get
		{
			return m_Card;
		}
		set
		{
			if (m_Card != value)
			{
				RemoveNestedDataModel(m_Card);
				RegisterNestedDataModel(value);
				m_Card = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<LettuceMercenaryArtVariationDataModel> ArtVariationList
	{
		get
		{
			return m_ArtVariationList;
		}
		set
		{
			if (m_ArtVariationList != value)
			{
				RemoveNestedDataModel(m_ArtVariationList);
				RegisterNestedDataModel(value);
				m_ArtVariationList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int ArtVariationPageIndex
	{
		get
		{
			return m_ArtVariationPageIndex;
		}
		set
		{
			if (m_ArtVariationPageIndex != value)
			{
				m_ArtVariationPageIndex = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<LettuceMercenaryArtVariationPageDataModel> ArtVariationPageList
	{
		get
		{
			return m_ArtVariationPageList;
		}
		set
		{
			if (m_ArtVariationPageList != value)
			{
				RemoveNestedDataModel(m_ArtVariationPageList);
				RegisterNestedDataModel(value);
				m_ArtVariationPageList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CraftingCost
	{
		get
		{
			return m_CraftingCost;
		}
		set
		{
			if (m_CraftingCost != value)
			{
				m_CraftingCost = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsAcquiredByCrafting
	{
		get
		{
			return m_IsAcquiredByCrafting;
		}
		set
		{
			if (m_IsAcquiredByCrafting != value)
			{
				m_IsAcquiredByCrafting = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel TreasureCard
	{
		get
		{
			return m_TreasureCard;
		}
		set
		{
			if (m_TreasureCard != value)
			{
				RemoveNestedDataModel(m_TreasureCard);
				RegisterNestedDataModel(value);
				m_TreasureCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HideXp
	{
		get
		{
			return m_HideXp;
		}
		set
		{
			if (m_HideXp != value)
			{
				m_HideXp = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HideWatermark
	{
		get
		{
			return m_HideWatermark;
		}
		set
		{
			if (m_HideWatermark != value)
			{
				m_HideWatermark = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HideStats
	{
		get
		{
			return m_HideStats;
		}
		set
		{
			if (m_HideStats != value)
			{
				m_HideStats = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Label
	{
		get
		{
			return m_Label;
		}
		set
		{
			if (!(m_Label == value))
			{
				m_Label = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TAG_ACQUIRE_TYPE AcquireType
	{
		get
		{
			return m_AcquireType;
		}
		set
		{
			if (m_AcquireType != value)
			{
				m_AcquireType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShowLevelInList
	{
		get
		{
			return m_ShowLevelInList;
		}
		set
		{
			if (m_ShowLevelInList != value)
			{
				m_ShowLevelInList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShowAbilityText
	{
		get
		{
			return m_ShowAbilityText;
		}
		set
		{
			if (m_ShowAbilityText != value)
			{
				m_ShowAbilityText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string AbilityText
	{
		get
		{
			return m_AbilityText;
		}
		set
		{
			if (!(m_AbilityText == value))
			{
				m_AbilityText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public float XPBarPercentage
	{
		get
		{
			return m_XPBarPercentage;
		}
		set
		{
			if (m_XPBarPercentage != value)
			{
				m_XPBarPercentage = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string CustomAcquireText
	{
		get
		{
			return m_CustomAcquireText;
		}
		set
		{
			if (!(m_CustomAcquireText == value))
			{
				m_CustomAcquireText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShowCustomAcquireText
	{
		get
		{
			return m_ShowCustomAcquireText;
		}
		set
		{
			if (m_ShowCustomAcquireText != value)
			{
				m_ShowCustomAcquireText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsMaxLevel
	{
		get
		{
			return m_IsMaxLevel;
		}
		set
		{
			if (m_IsMaxLevel != value)
			{
				m_IsMaxLevel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShowAsNew
	{
		get
		{
			return m_ShowAsNew;
		}
		set
		{
			if (m_ShowAsNew != value)
			{
				m_ShowAsNew = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NumNewPortraits
	{
		get
		{
			return m_NumNewPortraits;
		}
		set
		{
			if (m_NumNewPortraits != value)
			{
				m_NumNewPortraits = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsRemote
	{
		get
		{
			return m_IsRemote;
		}
		set
		{
			if (m_IsRemote != value)
			{
				m_IsRemote = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<MercenaryMythicTreasureDataModel> MythicTreasureList
	{
		get
		{
			return m_MythicTreasureList;
		}
		set
		{
			if (m_MythicTreasureList != value)
			{
				RemoveNestedDataModel(m_MythicTreasureList);
				RegisterNestedDataModel(value);
				m_MythicTreasureList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public long MythicLevel
	{
		get
		{
			return m_MythicLevel;
		}
		set
		{
			if (m_MythicLevel != value)
			{
				m_MythicLevel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public long MythicModifier
	{
		get
		{
			return m_MythicModifier;
		}
		set
		{
			if (m_MythicModifier != value)
			{
				m_MythicModifier = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool MythicView
	{
		get
		{
			return m_MythicView;
		}
		set
		{
			if (m_MythicView != value)
			{
				m_MythicView = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsMythicFirstUnlock
	{
		get
		{
			return m_IsMythicFirstUnlock;
		}
		set
		{
			if (m_IsMythicFirstUnlock != value)
			{
				m_IsMythicFirstUnlock = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool MythicToggleEnable
	{
		get
		{
			return m_MythicToggleEnable;
		}
		set
		{
			if (m_MythicToggleEnable != value)
			{
				m_MythicToggleEnable = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool MythicToggle
	{
		get
		{
			return m_MythicToggle;
		}
		set
		{
			if (m_MythicToggle != value)
			{
				m_MythicToggle = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceMercenaryDataModel()
	{
		RegisterNestedDataModel(m_MercenaryCoin);
		RegisterNestedDataModel(m_AbilityList);
		RegisterNestedDataModel(m_EquipmentList);
		RegisterNestedDataModel(m_Card);
		RegisterNestedDataModel(m_ArtVariationList);
		RegisterNestedDataModel(m_ArtVariationPageList);
		RegisterNestedDataModel(m_TreasureCard);
		RegisterNestedDataModel(m_MythicTreasureList);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_MercenaryId;
		int num2 = (((num + m_MercenaryId.GetHashCode()) * 31 + ((m_MercenaryName != null) ? m_MercenaryName.GetHashCode() : 0)) * 31 + ((m_MercenaryShortName != null) ? m_MercenaryShortName.GetHashCode() : 0)) * 31;
		_ = m_MercenaryRole;
		int num3 = (num2 + m_MercenaryRole.GetHashCode()) * 31;
		_ = m_MercenaryLevel;
		int num4 = (num3 + m_MercenaryLevel.GetHashCode()) * 31;
		_ = m_MercenaryRarity;
		int num5 = (num4 + m_MercenaryRarity.GetHashCode()) * 31;
		_ = m_ReadyForCrafting;
		int num6 = (((num5 + m_ReadyForCrafting.GetHashCode()) * 31 + ((m_MercenaryCoin != null) ? m_MercenaryCoin.GetPropertiesHashCode() : 0)) * 31 + ((m_AbilityList != null) ? m_AbilityList.GetPropertiesHashCode() : 0)) * 31;
		_ = m_EquipmentSlotIndex;
		int num7 = (num6 + m_EquipmentSlotIndex.GetHashCode()) * 31;
		_ = m_DeadInMapRun;
		int num8 = (num7 + m_DeadInMapRun.GetHashCode()) * 31;
		_ = m_AbilitySlotUnlockThreshold;
		int num9 = ((num8 + m_AbilitySlotUnlockThreshold.GetHashCode()) * 31 + ((m_EquipmentList != null) ? m_EquipmentList.GetPropertiesHashCode() : 0)) * 31;
		_ = m_MercenarySelected;
		int num10 = (num9 + m_MercenarySelected.GetHashCode()) * 31;
		_ = m_Owned;
		int num11 = (num10 + m_Owned.GetHashCode()) * 31;
		_ = m_ExperienceInitial;
		int num12 = (num11 + m_ExperienceInitial.GetHashCode()) * 31;
		_ = m_ExperienceFinal;
		int num13 = (num12 + m_ExperienceFinal.GetHashCode()) * 31;
		_ = m_FullyUpgradedInitial;
		int num14 = (num13 + m_FullyUpgradedInitial.GetHashCode()) * 31;
		_ = m_FullyUpgradedFinal;
		int num15 = (num14 + m_FullyUpgradedFinal.GetHashCode()) * 31;
		_ = m_InCurrentTeam;
		int num16 = (num15 + m_InCurrentTeam.GetHashCode()) * 31;
		_ = m_ChildUpgradeAvailable;
		int num17 = (num16 + m_ChildUpgradeAvailable.GetHashCode()) * 31;
		_ = m_IsAffectedBySlottedEquipment;
		int num18 = (num17 + m_IsAffectedBySlottedEquipment.GetHashCode()) * 31;
		_ = m_IsDisabled;
		int num19 = (((num18 + m_IsDisabled.GetHashCode()) * 31 + ((m_Card != null) ? m_Card.GetPropertiesHashCode() : 0)) * 31 + ((m_ArtVariationList != null) ? m_ArtVariationList.GetPropertiesHashCode() : 0)) * 31;
		_ = m_ArtVariationPageIndex;
		int num20 = ((num19 + m_ArtVariationPageIndex.GetHashCode()) * 31 + ((m_ArtVariationPageList != null) ? m_ArtVariationPageList.GetPropertiesHashCode() : 0)) * 31;
		_ = m_CraftingCost;
		int num21 = (num20 + m_CraftingCost.GetHashCode()) * 31;
		_ = m_IsAcquiredByCrafting;
		int num22 = ((num21 + m_IsAcquiredByCrafting.GetHashCode()) * 31 + ((m_TreasureCard != null) ? m_TreasureCard.GetPropertiesHashCode() : 0)) * 31;
		_ = m_HideXp;
		int num23 = (num22 + m_HideXp.GetHashCode()) * 31;
		_ = m_HideWatermark;
		int num24 = (num23 + m_HideWatermark.GetHashCode()) * 31;
		_ = m_HideStats;
		int num25 = ((num24 + m_HideStats.GetHashCode()) * 31 + ((m_Label != null) ? m_Label.GetHashCode() : 0)) * 31;
		_ = m_AcquireType;
		int num26 = (num25 + m_AcquireType.GetHashCode()) * 31;
		_ = m_ShowLevelInList;
		int num27 = (num26 + m_ShowLevelInList.GetHashCode()) * 31;
		_ = m_ShowAbilityText;
		int num28 = ((num27 + m_ShowAbilityText.GetHashCode()) * 31 + ((m_AbilityText != null) ? m_AbilityText.GetHashCode() : 0)) * 31;
		_ = m_XPBarPercentage;
		int num29 = ((num28 + m_XPBarPercentage.GetHashCode()) * 31 + ((m_CustomAcquireText != null) ? m_CustomAcquireText.GetHashCode() : 0)) * 31;
		_ = m_ShowCustomAcquireText;
		int num30 = (num29 + m_ShowCustomAcquireText.GetHashCode()) * 31;
		_ = m_IsMaxLevel;
		int num31 = (num30 + m_IsMaxLevel.GetHashCode()) * 31;
		_ = m_ShowAsNew;
		int num32 = (num31 + m_ShowAsNew.GetHashCode()) * 31;
		_ = m_NumNewPortraits;
		int num33 = (num32 + m_NumNewPortraits.GetHashCode()) * 31;
		_ = m_IsRemote;
		int num34 = ((num33 + m_IsRemote.GetHashCode()) * 31 + ((m_MythicTreasureList != null) ? m_MythicTreasureList.GetPropertiesHashCode() : 0)) * 31;
		_ = m_MythicLevel;
		int num35 = (num34 + m_MythicLevel.GetHashCode()) * 31;
		_ = m_MythicModifier;
		int num36 = (num35 + m_MythicModifier.GetHashCode()) * 31;
		_ = m_MythicView;
		int num37 = (num36 + m_MythicView.GetHashCode()) * 31;
		_ = m_IsMythicFirstUnlock;
		int num38 = (num37 + m_IsMythicFirstUnlock.GetHashCode()) * 31;
		_ = m_MythicToggleEnable;
		int num39 = (num38 + m_MythicToggleEnable.GetHashCode()) * 31;
		_ = m_MythicToggle;
		return num39 + m_MythicToggle.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_MercenaryId;
			return true;
		case 1:
			value = m_MercenaryName;
			return true;
		case 21:
			value = m_MercenaryShortName;
			return true;
		case 2:
			value = m_MercenaryRole;
			return true;
		case 3:
			value = m_MercenaryLevel;
			return true;
		case 29:
			value = m_MercenaryRarity;
			return true;
		case 4:
			value = m_ReadyForCrafting;
			return true;
		case 7:
			value = m_MercenaryCoin;
			return true;
		case 8:
			value = m_AbilityList;
			return true;
		case 9:
			value = m_EquipmentSlotIndex;
			return true;
		case 10:
			value = m_DeadInMapRun;
			return true;
		case 11:
			value = m_AbilitySlotUnlockThreshold;
			return true;
		case 13:
			value = m_EquipmentList;
			return true;
		case 14:
			value = m_MercenarySelected;
			return true;
		case 15:
			value = m_Owned;
			return true;
		case 16:
			value = m_ExperienceInitial;
			return true;
		case 17:
			value = m_ExperienceFinal;
			return true;
		case 32:
			value = m_FullyUpgradedInitial;
			return true;
		case 31:
			value = m_FullyUpgradedFinal;
			return true;
		case 18:
			value = m_InCurrentTeam;
			return true;
		case 19:
			value = m_ChildUpgradeAvailable;
			return true;
		case 20:
			value = m_IsAffectedBySlottedEquipment;
			return true;
		case 22:
			value = m_IsDisabled;
			return true;
		case 23:
			value = m_Card;
			return true;
		case 26:
			value = m_ArtVariationList;
			return true;
		case 28:
			value = m_ArtVariationPageIndex;
			return true;
		case 27:
			value = m_ArtVariationPageList;
			return true;
		case 25:
			value = m_CraftingCost;
			return true;
		case 37:
			value = m_IsAcquiredByCrafting;
			return true;
		case 30:
			value = m_TreasureCard;
			return true;
		case 33:
			value = m_HideXp;
			return true;
		case 34:
			value = m_HideWatermark;
			return true;
		case 35:
			value = m_HideStats;
			return true;
		case 36:
			value = m_Label;
			return true;
		case 38:
			value = m_AcquireType;
			return true;
		case 577:
			value = m_ShowLevelInList;
			return true;
		case 617:
			value = m_ShowAbilityText;
			return true;
		case 618:
			value = m_AbilityText;
			return true;
		case 635:
			value = m_XPBarPercentage;
			return true;
		case 640:
			value = m_CustomAcquireText;
			return true;
		case 641:
			value = m_ShowCustomAcquireText;
			return true;
		case 649:
			value = m_IsMaxLevel;
			return true;
		case 653:
			value = m_ShowAsNew;
			return true;
		case 659:
			value = m_NumNewPortraits;
			return true;
		case 660:
			value = m_IsRemote;
			return true;
		case 751:
			value = m_MythicTreasureList;
			return true;
		case 756:
			value = m_MythicLevel;
			return true;
		case 757:
			value = m_MythicModifier;
			return true;
		case 761:
			value = m_MythicView;
			return true;
		case 787:
			value = m_IsMythicFirstUnlock;
			return true;
		case 791:
			value = m_MythicToggleEnable;
			return true;
		case 790:
			value = m_MythicToggle;
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
			MercenaryId = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			MercenaryName = ((value != null) ? ((string)value) : null);
			return true;
		case 21:
			MercenaryShortName = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			MercenaryRole = ((value != null) ? ((TAG_ROLE)value) : TAG_ROLE.INVALID);
			return true;
		case 3:
			MercenaryLevel = ((value != null) ? ((int)value) : 0);
			return true;
		case 29:
			MercenaryRarity = ((value != null) ? ((TAG_RARITY)value) : TAG_RARITY.INVALID);
			return true;
		case 4:
			ReadyForCrafting = value != null && (bool)value;
			return true;
		case 7:
			MercenaryCoin = ((value != null) ? ((LettuceMercenaryCoinDataModel)value) : null);
			return true;
		case 8:
			AbilityList = ((value != null) ? ((DataModelList<LettuceAbilityDataModel>)value) : null);
			return true;
		case 9:
			EquipmentSlotIndex = ((value != null) ? ((int)value) : 0);
			return true;
		case 10:
			DeadInMapRun = value != null && (bool)value;
			return true;
		case 11:
			AbilitySlotUnlockThreshold = ((value != null) ? ((int)value) : 0);
			return true;
		case 13:
			EquipmentList = ((value != null) ? ((DataModelList<LettuceAbilityDataModel>)value) : null);
			return true;
		case 14:
			MercenarySelected = value != null && (bool)value;
			return true;
		case 15:
			Owned = value != null && (bool)value;
			return true;
		case 16:
			ExperienceInitial = ((value != null) ? ((int)value) : 0);
			return true;
		case 17:
			ExperienceFinal = ((value != null) ? ((int)value) : 0);
			return true;
		case 32:
			FullyUpgradedInitial = value != null && (bool)value;
			return true;
		case 31:
			FullyUpgradedFinal = value != null && (bool)value;
			return true;
		case 18:
			InCurrentTeam = value != null && (bool)value;
			return true;
		case 19:
			ChildUpgradeAvailable = value != null && (bool)value;
			return true;
		case 20:
			IsAffectedBySlottedEquipment = value != null && (bool)value;
			return true;
		case 22:
			IsDisabled = value != null && (bool)value;
			return true;
		case 23:
			Card = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 26:
			ArtVariationList = ((value != null) ? ((DataModelList<LettuceMercenaryArtVariationDataModel>)value) : null);
			return true;
		case 28:
			ArtVariationPageIndex = ((value != null) ? ((int)value) : 0);
			return true;
		case 27:
			ArtVariationPageList = ((value != null) ? ((DataModelList<LettuceMercenaryArtVariationPageDataModel>)value) : null);
			return true;
		case 25:
			CraftingCost = ((value != null) ? ((int)value) : 0);
			return true;
		case 37:
			IsAcquiredByCrafting = value != null && (bool)value;
			return true;
		case 30:
			TreasureCard = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 33:
			HideXp = value != null && (bool)value;
			return true;
		case 34:
			HideWatermark = value != null && (bool)value;
			return true;
		case 35:
			HideStats = value != null && (bool)value;
			return true;
		case 36:
			Label = ((value != null) ? ((string)value) : null);
			return true;
		case 38:
			AcquireType = ((value != null) ? ((TAG_ACQUIRE_TYPE)value) : TAG_ACQUIRE_TYPE.NONE);
			return true;
		case 577:
			ShowLevelInList = value != null && (bool)value;
			return true;
		case 617:
			ShowAbilityText = value != null && (bool)value;
			return true;
		case 618:
			AbilityText = ((value != null) ? ((string)value) : null);
			return true;
		case 635:
			XPBarPercentage = ((value != null) ? ((float)value) : 0f);
			return true;
		case 640:
			CustomAcquireText = ((value != null) ? ((string)value) : null);
			return true;
		case 641:
			ShowCustomAcquireText = value != null && (bool)value;
			return true;
		case 649:
			IsMaxLevel = value != null && (bool)value;
			return true;
		case 653:
			ShowAsNew = value != null && (bool)value;
			return true;
		case 659:
			NumNewPortraits = ((value != null) ? ((int)value) : 0);
			return true;
		case 660:
			IsRemote = value != null && (bool)value;
			return true;
		case 751:
			MythicTreasureList = ((value != null) ? ((DataModelList<MercenaryMythicTreasureDataModel>)value) : null);
			return true;
		case 756:
			MythicLevel = ((value != null) ? ((long)value) : 0);
			return true;
		case 757:
			MythicModifier = ((value != null) ? ((long)value) : 0);
			return true;
		case 761:
			MythicView = value != null && (bool)value;
			return true;
		case 787:
			IsMythicFirstUnlock = value != null && (bool)value;
			return true;
		case 791:
			MythicToggleEnable = value != null && (bool)value;
			return true;
		case 790:
			MythicToggle = value != null && (bool)value;
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
		case 21:
			info = Properties[2];
			return true;
		case 2:
			info = Properties[3];
			return true;
		case 3:
			info = Properties[4];
			return true;
		case 29:
			info = Properties[5];
			return true;
		case 4:
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
		case 32:
			info = Properties[17];
			return true;
		case 31:
			info = Properties[18];
			return true;
		case 18:
			info = Properties[19];
			return true;
		case 19:
			info = Properties[20];
			return true;
		case 20:
			info = Properties[21];
			return true;
		case 22:
			info = Properties[22];
			return true;
		case 23:
			info = Properties[23];
			return true;
		case 26:
			info = Properties[24];
			return true;
		case 28:
			info = Properties[25];
			return true;
		case 27:
			info = Properties[26];
			return true;
		case 25:
			info = Properties[27];
			return true;
		case 37:
			info = Properties[28];
			return true;
		case 30:
			info = Properties[29];
			return true;
		case 33:
			info = Properties[30];
			return true;
		case 34:
			info = Properties[31];
			return true;
		case 35:
			info = Properties[32];
			return true;
		case 36:
			info = Properties[33];
			return true;
		case 38:
			info = Properties[34];
			return true;
		case 577:
			info = Properties[35];
			return true;
		case 617:
			info = Properties[36];
			return true;
		case 618:
			info = Properties[37];
			return true;
		case 635:
			info = Properties[38];
			return true;
		case 640:
			info = Properties[39];
			return true;
		case 641:
			info = Properties[40];
			return true;
		case 649:
			info = Properties[41];
			return true;
		case 653:
			info = Properties[42];
			return true;
		case 659:
			info = Properties[43];
			return true;
		case 660:
			info = Properties[44];
			return true;
		case 751:
			info = Properties[45];
			return true;
		case 756:
			info = Properties[46];
			return true;
		case 757:
			info = Properties[47];
			return true;
		case 761:
			info = Properties[48];
			return true;
		case 787:
			info = Properties[49];
			return true;
		case 791:
			info = Properties[50];
			return true;
		case 790:
			info = Properties[51];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
