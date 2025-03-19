using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class RewardItemDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 17;

	private long m_PmtLicenseId;

	private RewardItemType m_ItemType;

	private int m_ItemId;

	private int m_Quantity;

	private PackDataModel m_Booster;

	private CardBackDataModel m_CardBack;

	private PriceDataModel m_Currency;

	private int m_AssetId;

	private CardDataModel m_Card;

	private RandomCardDataModel m_RandomCard;

	private LettuceMercenaryCoinDataModel m_MercenaryCoin;

	private LettuceMercenaryDataModel m_Mercenary;

	private LettuceAbilityDataModel m_MercenaryEquip;

	private LettuceRandomMercenaryDataModel m_RandomMercenary;

	private BattlegroundsBoardSkinDataModel m_BGBoardSkin;

	private BattlegroundsFinisherDataModel m_BGFinisher;

	private BattlegroundsEmoteDataModel m_BGEmote;

	private DataModelList<BattlegroundsEmoteDataModel> m_BGEmotePile = new DataModelList<BattlegroundsEmoteDataModel>();

	private BattlegroundsBonusType m_BattlegroundsBonusType;

	private bool m_IsMercenaryPortrait;

	private bool m_IsClaimed;

	private BattlegroundsBattleBashHammerDataModel m_BattlegroundsBattleBashHammer;

	private string m_Season;

	private int m_HeroClassId;

	private int m_GameModeId;

	private DeckPouchDataModel m_Deck;

	private int m_DeckTemplateId;

	private string m_StandaloneDescription;

	private int m_HeroClassCount;

	private DataModelProperty[] m_properties = new DataModelProperty[29]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "pmt_license_id",
			Type = typeof(long)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "item_type",
			Type = typeof(RewardItemType)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "item_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "quantity",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "booster",
			Type = typeof(PackDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "card_back",
			Type = typeof(CardBackDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "currency",
			Type = typeof(PriceDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "asset_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 110,
			PropertyDisplayName = "card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 111,
			PropertyDisplayName = "random_card",
			Type = typeof(RandomCardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 112,
			PropertyDisplayName = "mercenary_coin",
			Type = typeof(LettuceMercenaryCoinDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 113,
			PropertyDisplayName = "mercenary",
			Type = typeof(LettuceMercenaryDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 451,
			PropertyDisplayName = "mercenary_equip",
			Type = typeof(LettuceAbilityDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 453,
			PropertyDisplayName = "lettuce_random_mercenary",
			Type = typeof(LettuceRandomMercenaryDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 454,
			PropertyDisplayName = "battlegrounds_board_skin",
			Type = typeof(BattlegroundsBoardSkinDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 455,
			PropertyDisplayName = "battlegrounds_finisher",
			Type = typeof(BattlegroundsFinisherDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 456,
			PropertyDisplayName = "battlegrounds_emote",
			Type = typeof(BattlegroundsEmoteDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 457,
			PropertyDisplayName = "battlegrounds_emote_pile",
			Type = typeof(DataModelList<BattlegroundsEmoteDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 458,
			PropertyDisplayName = "battlegrounds_bonus_type",
			Type = typeof(BattlegroundsBonusType)
		},
		new DataModelProperty
		{
			PropertyId = 696,
			PropertyDisplayName = "is_mercenary_portrait",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 703,
			PropertyDisplayName = "is_claimed",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 706,
			PropertyDisplayName = "battlegrounds_battle_bash_hammer",
			Type = typeof(BattlegroundsBattleBashHammerDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 707,
			PropertyDisplayName = "season",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 866,
			PropertyDisplayName = "hero_class_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 867,
			PropertyDisplayName = "game_mode_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 868,
			PropertyDisplayName = "deck",
			Type = typeof(DeckPouchDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 877,
			PropertyDisplayName = "deck_template_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 876,
			PropertyDisplayName = "standalone_description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 878,
			PropertyDisplayName = "hero_class_count",
			Type = typeof(int)
		}
	};

	public int DataModelId => 17;

	public string DataModelDisplayName => "reward_item";

	public long PmtLicenseId
	{
		get
		{
			return m_PmtLicenseId;
		}
		set
		{
			if (m_PmtLicenseId != value)
			{
				m_PmtLicenseId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public RewardItemType ItemType
	{
		get
		{
			return m_ItemType;
		}
		set
		{
			if (m_ItemType != value)
			{
				m_ItemType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int ItemId
	{
		get
		{
			return m_ItemId;
		}
		set
		{
			if (m_ItemId != value)
			{
				m_ItemId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Quantity
	{
		get
		{
			return m_Quantity;
		}
		set
		{
			if (m_Quantity != value)
			{
				m_Quantity = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public PackDataModel Booster
	{
		get
		{
			return m_Booster;
		}
		set
		{
			if (m_Booster != value)
			{
				RemoveNestedDataModel(m_Booster);
				RegisterNestedDataModel(value);
				m_Booster = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardBackDataModel CardBack
	{
		get
		{
			return m_CardBack;
		}
		set
		{
			if (m_CardBack != value)
			{
				RemoveNestedDataModel(m_CardBack);
				RegisterNestedDataModel(value);
				m_CardBack = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public PriceDataModel Currency
	{
		get
		{
			return m_Currency;
		}
		set
		{
			if (m_Currency != value)
			{
				RemoveNestedDataModel(m_Currency);
				RegisterNestedDataModel(value);
				m_Currency = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int AssetId
	{
		get
		{
			return m_AssetId;
		}
		set
		{
			if (m_AssetId != value)
			{
				m_AssetId = value;
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

	public RandomCardDataModel RandomCard
	{
		get
		{
			return m_RandomCard;
		}
		set
		{
			if (m_RandomCard != value)
			{
				RemoveNestedDataModel(m_RandomCard);
				RegisterNestedDataModel(value);
				m_RandomCard = value;
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

	public LettuceMercenaryDataModel Mercenary
	{
		get
		{
			return m_Mercenary;
		}
		set
		{
			if (m_Mercenary != value)
			{
				RemoveNestedDataModel(m_Mercenary);
				RegisterNestedDataModel(value);
				m_Mercenary = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceAbilityDataModel MercenaryEquip
	{
		get
		{
			return m_MercenaryEquip;
		}
		set
		{
			if (m_MercenaryEquip != value)
			{
				RemoveNestedDataModel(m_MercenaryEquip);
				RegisterNestedDataModel(value);
				m_MercenaryEquip = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceRandomMercenaryDataModel RandomMercenary
	{
		get
		{
			return m_RandomMercenary;
		}
		set
		{
			if (m_RandomMercenary != value)
			{
				RemoveNestedDataModel(m_RandomMercenary);
				RegisterNestedDataModel(value);
				m_RandomMercenary = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public BattlegroundsBoardSkinDataModel BGBoardSkin
	{
		get
		{
			return m_BGBoardSkin;
		}
		set
		{
			if (m_BGBoardSkin != value)
			{
				RemoveNestedDataModel(m_BGBoardSkin);
				RegisterNestedDataModel(value);
				m_BGBoardSkin = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public BattlegroundsFinisherDataModel BGFinisher
	{
		get
		{
			return m_BGFinisher;
		}
		set
		{
			if (m_BGFinisher != value)
			{
				RemoveNestedDataModel(m_BGFinisher);
				RegisterNestedDataModel(value);
				m_BGFinisher = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public BattlegroundsEmoteDataModel BGEmote
	{
		get
		{
			return m_BGEmote;
		}
		set
		{
			if (m_BGEmote != value)
			{
				RemoveNestedDataModel(m_BGEmote);
				RegisterNestedDataModel(value);
				m_BGEmote = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<BattlegroundsEmoteDataModel> BGEmotePile
	{
		get
		{
			return m_BGEmotePile;
		}
		set
		{
			if (m_BGEmotePile != value)
			{
				RemoveNestedDataModel(m_BGEmotePile);
				RegisterNestedDataModel(value);
				m_BGEmotePile = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public BattlegroundsBonusType BattlegroundsBonusType
	{
		get
		{
			return m_BattlegroundsBonusType;
		}
		set
		{
			if (m_BattlegroundsBonusType != value)
			{
				m_BattlegroundsBonusType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsMercenaryPortrait
	{
		get
		{
			return m_IsMercenaryPortrait;
		}
		set
		{
			if (m_IsMercenaryPortrait != value)
			{
				m_IsMercenaryPortrait = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsClaimed
	{
		get
		{
			return m_IsClaimed;
		}
		set
		{
			if (m_IsClaimed != value)
			{
				m_IsClaimed = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public BattlegroundsBattleBashHammerDataModel BattlegroundsBattleBashHammer
	{
		get
		{
			return m_BattlegroundsBattleBashHammer;
		}
		set
		{
			if (m_BattlegroundsBattleBashHammer != value)
			{
				RemoveNestedDataModel(m_BattlegroundsBattleBashHammer);
				RegisterNestedDataModel(value);
				m_BattlegroundsBattleBashHammer = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Season
	{
		get
		{
			return m_Season;
		}
		set
		{
			if (!(m_Season == value))
			{
				m_Season = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int HeroClassId
	{
		get
		{
			return m_HeroClassId;
		}
		set
		{
			if (m_HeroClassId != value)
			{
				m_HeroClassId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int GameModeId
	{
		get
		{
			return m_GameModeId;
		}
		set
		{
			if (m_GameModeId != value)
			{
				m_GameModeId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DeckPouchDataModel Deck
	{
		get
		{
			return m_Deck;
		}
		set
		{
			if (m_Deck != value)
			{
				RemoveNestedDataModel(m_Deck);
				RegisterNestedDataModel(value);
				m_Deck = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int DeckTemplateId
	{
		get
		{
			return m_DeckTemplateId;
		}
		set
		{
			if (m_DeckTemplateId != value)
			{
				m_DeckTemplateId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string StandaloneDescription
	{
		get
		{
			return m_StandaloneDescription;
		}
		set
		{
			if (!(m_StandaloneDescription == value))
			{
				m_StandaloneDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int HeroClassCount
	{
		get
		{
			return m_HeroClassCount;
		}
		set
		{
			if (m_HeroClassCount != value)
			{
				m_HeroClassCount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public RewardItemDataModel()
	{
		RegisterNestedDataModel(m_Booster);
		RegisterNestedDataModel(m_CardBack);
		RegisterNestedDataModel(m_Currency);
		RegisterNestedDataModel(m_Card);
		RegisterNestedDataModel(m_RandomCard);
		RegisterNestedDataModel(m_MercenaryCoin);
		RegisterNestedDataModel(m_Mercenary);
		RegisterNestedDataModel(m_MercenaryEquip);
		RegisterNestedDataModel(m_RandomMercenary);
		RegisterNestedDataModel(m_BGBoardSkin);
		RegisterNestedDataModel(m_BGFinisher);
		RegisterNestedDataModel(m_BGEmote);
		RegisterNestedDataModel(m_BGEmotePile);
		RegisterNestedDataModel(m_BattlegroundsBattleBashHammer);
		RegisterNestedDataModel(m_Deck);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_PmtLicenseId;
		int num2 = (num + m_PmtLicenseId.GetHashCode()) * 31;
		_ = m_ItemType;
		int num3 = (num2 + m_ItemType.GetHashCode()) * 31;
		_ = m_ItemId;
		int num4 = (num3 + m_ItemId.GetHashCode()) * 31;
		_ = m_Quantity;
		int num5 = ((((num4 + m_Quantity.GetHashCode()) * 31 + ((m_Booster != null) ? m_Booster.GetPropertiesHashCode() : 0)) * 31 + ((m_CardBack != null) ? m_CardBack.GetPropertiesHashCode() : 0)) * 31 + ((m_Currency != null) ? m_Currency.GetPropertiesHashCode() : 0)) * 31;
		_ = m_AssetId;
		int num6 = (((((((((((num5 + m_AssetId.GetHashCode()) * 31 + ((m_Card != null) ? m_Card.GetPropertiesHashCode() : 0)) * 31 + ((m_RandomCard != null) ? m_RandomCard.GetPropertiesHashCode() : 0)) * 31 + ((m_MercenaryCoin != null) ? m_MercenaryCoin.GetPropertiesHashCode() : 0)) * 31 + ((m_Mercenary != null) ? m_Mercenary.GetPropertiesHashCode() : 0)) * 31 + ((m_MercenaryEquip != null) ? m_MercenaryEquip.GetPropertiesHashCode() : 0)) * 31 + ((m_RandomMercenary != null) ? m_RandomMercenary.GetPropertiesHashCode() : 0)) * 31 + ((m_BGBoardSkin != null) ? m_BGBoardSkin.GetPropertiesHashCode() : 0)) * 31 + ((m_BGFinisher != null) ? m_BGFinisher.GetPropertiesHashCode() : 0)) * 31 + ((m_BGEmote != null) ? m_BGEmote.GetPropertiesHashCode() : 0)) * 31 + ((m_BGEmotePile != null) ? m_BGEmotePile.GetPropertiesHashCode() : 0)) * 31;
		_ = m_BattlegroundsBonusType;
		int num7 = (num6 + m_BattlegroundsBonusType.GetHashCode()) * 31;
		_ = m_IsMercenaryPortrait;
		int num8 = (num7 + m_IsMercenaryPortrait.GetHashCode()) * 31;
		_ = m_IsClaimed;
		int num9 = (((num8 + m_IsClaimed.GetHashCode()) * 31 + ((m_BattlegroundsBattleBashHammer != null) ? m_BattlegroundsBattleBashHammer.GetPropertiesHashCode() : 0)) * 31 + ((m_Season != null) ? m_Season.GetHashCode() : 0)) * 31;
		_ = m_HeroClassId;
		int num10 = (num9 + m_HeroClassId.GetHashCode()) * 31;
		_ = m_GameModeId;
		int num11 = ((num10 + m_GameModeId.GetHashCode()) * 31 + ((m_Deck != null) ? m_Deck.GetPropertiesHashCode() : 0)) * 31;
		_ = m_DeckTemplateId;
		int num12 = ((num11 + m_DeckTemplateId.GetHashCode()) * 31 + ((m_StandaloneDescription != null) ? m_StandaloneDescription.GetHashCode() : 0)) * 31;
		_ = m_HeroClassCount;
		return num12 + m_HeroClassCount.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_PmtLicenseId;
			return true;
		case 1:
			value = m_ItemType;
			return true;
		case 2:
			value = m_ItemId;
			return true;
		case 3:
			value = m_Quantity;
			return true;
		case 4:
			value = m_Booster;
			return true;
		case 5:
			value = m_CardBack;
			return true;
		case 6:
			value = m_Currency;
			return true;
		case 7:
			value = m_AssetId;
			return true;
		case 110:
			value = m_Card;
			return true;
		case 111:
			value = m_RandomCard;
			return true;
		case 112:
			value = m_MercenaryCoin;
			return true;
		case 113:
			value = m_Mercenary;
			return true;
		case 451:
			value = m_MercenaryEquip;
			return true;
		case 453:
			value = m_RandomMercenary;
			return true;
		case 454:
			value = m_BGBoardSkin;
			return true;
		case 455:
			value = m_BGFinisher;
			return true;
		case 456:
			value = m_BGEmote;
			return true;
		case 457:
			value = m_BGEmotePile;
			return true;
		case 458:
			value = m_BattlegroundsBonusType;
			return true;
		case 696:
			value = m_IsMercenaryPortrait;
			return true;
		case 703:
			value = m_IsClaimed;
			return true;
		case 706:
			value = m_BattlegroundsBattleBashHammer;
			return true;
		case 707:
			value = m_Season;
			return true;
		case 866:
			value = m_HeroClassId;
			return true;
		case 867:
			value = m_GameModeId;
			return true;
		case 868:
			value = m_Deck;
			return true;
		case 877:
			value = m_DeckTemplateId;
			return true;
		case 876:
			value = m_StandaloneDescription;
			return true;
		case 878:
			value = m_HeroClassCount;
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
			PmtLicenseId = ((value != null) ? ((long)value) : 0);
			return true;
		case 1:
			ItemType = ((value != null) ? ((RewardItemType)value) : RewardItemType.UNDEFINED);
			return true;
		case 2:
			ItemId = ((value != null) ? ((int)value) : 0);
			return true;
		case 3:
			Quantity = ((value != null) ? ((int)value) : 0);
			return true;
		case 4:
			Booster = ((value != null) ? ((PackDataModel)value) : null);
			return true;
		case 5:
			CardBack = ((value != null) ? ((CardBackDataModel)value) : null);
			return true;
		case 6:
			Currency = ((value != null) ? ((PriceDataModel)value) : null);
			return true;
		case 7:
			AssetId = ((value != null) ? ((int)value) : 0);
			return true;
		case 110:
			Card = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 111:
			RandomCard = ((value != null) ? ((RandomCardDataModel)value) : null);
			return true;
		case 112:
			MercenaryCoin = ((value != null) ? ((LettuceMercenaryCoinDataModel)value) : null);
			return true;
		case 113:
			Mercenary = ((value != null) ? ((LettuceMercenaryDataModel)value) : null);
			return true;
		case 451:
			MercenaryEquip = ((value != null) ? ((LettuceAbilityDataModel)value) : null);
			return true;
		case 453:
			RandomMercenary = ((value != null) ? ((LettuceRandomMercenaryDataModel)value) : null);
			return true;
		case 454:
			BGBoardSkin = ((value != null) ? ((BattlegroundsBoardSkinDataModel)value) : null);
			return true;
		case 455:
			BGFinisher = ((value != null) ? ((BattlegroundsFinisherDataModel)value) : null);
			return true;
		case 456:
			BGEmote = ((value != null) ? ((BattlegroundsEmoteDataModel)value) : null);
			return true;
		case 457:
			BGEmotePile = ((value != null) ? ((DataModelList<BattlegroundsEmoteDataModel>)value) : null);
			return true;
		case 458:
			BattlegroundsBonusType = ((value != null) ? ((BattlegroundsBonusType)value) : BattlegroundsBonusType.UNDEFINED);
			return true;
		case 696:
			IsMercenaryPortrait = value != null && (bool)value;
			return true;
		case 703:
			IsClaimed = value != null && (bool)value;
			return true;
		case 706:
			BattlegroundsBattleBashHammer = ((value != null) ? ((BattlegroundsBattleBashHammerDataModel)value) : null);
			return true;
		case 707:
			Season = ((value != null) ? ((string)value) : null);
			return true;
		case 866:
			HeroClassId = ((value != null) ? ((int)value) : 0);
			return true;
		case 867:
			GameModeId = ((value != null) ? ((int)value) : 0);
			return true;
		case 868:
			Deck = ((value != null) ? ((DeckPouchDataModel)value) : null);
			return true;
		case 877:
			DeckTemplateId = ((value != null) ? ((int)value) : 0);
			return true;
		case 876:
			StandaloneDescription = ((value != null) ? ((string)value) : null);
			return true;
		case 878:
			HeroClassCount = ((value != null) ? ((int)value) : 0);
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
		case 110:
			info = Properties[8];
			return true;
		case 111:
			info = Properties[9];
			return true;
		case 112:
			info = Properties[10];
			return true;
		case 113:
			info = Properties[11];
			return true;
		case 451:
			info = Properties[12];
			return true;
		case 453:
			info = Properties[13];
			return true;
		case 454:
			info = Properties[14];
			return true;
		case 455:
			info = Properties[15];
			return true;
		case 456:
			info = Properties[16];
			return true;
		case 457:
			info = Properties[17];
			return true;
		case 458:
			info = Properties[18];
			return true;
		case 696:
			info = Properties[19];
			return true;
		case 703:
			info = Properties[20];
			return true;
		case 706:
			info = Properties[21];
			return true;
		case 707:
			info = Properties[22];
			return true;
		case 866:
			info = Properties[23];
			return true;
		case 867:
			info = Properties[24];
			return true;
		case 868:
			info = Properties[25];
			return true;
		case 877:
			info = Properties[26];
			return true;
		case 876:
			info = Properties[27];
			return true;
		case 878:
			info = Properties[28];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
