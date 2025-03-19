using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Assets;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Hearthstone.Util;
using PegasusShared;
using UnityEngine;

public class GameStrings
{
	public class PluralNumber
	{
		public int m_index;

		public int m_number;

		public bool m_useForOnlyThisIndex;
	}

	public const string s_UnknownName = "UNKNOWN";

	private static Map<Global.GameStringCategory, GameStringTable> s_tables = new Map<Global.GameStringCategory, GameStringTable>();

	private static readonly char[] LANGUAGE_RULE_ARG_DELIMITERS = new char[1] { ',' };

	private static List<Global.GameStringCategory> s_nativeGameStringCatetories = new List<Global.GameStringCategory>
	{
		Global.GameStringCategory.GLOBAL,
		Global.GameStringCategory.GLUE,
		Global.GameStringCategory.TUTORIAL,
		Global.GameStringCategory.GAMEPLAY,
		Global.GameStringCategory.PRESENCE
	};

	private const string NUMBER_PATTERN = "(?<!\\/)(?:[0-9]+,)*[0-9]+(?!\\/)";

	private const string NUMBER_PATTERN_ALT = "(?<!\\/)(?:[0-9]+,)*[0-9]+";

	private const string SKIPPABLE_KOREAN_CHARACTERS = ")}]:;?/*&^!~`/\\|_'\"";

	private const int KOREAN_NO_JONGSEONG = 51060;

	private const int KOREAN_JONGSEONG = 50689;

	private const int KOREAN_RIEUL_JONGSEONG = 51068;

	public static Map<TAG_CLASS, string> s_classNames = new Map<TAG_CLASS, string>
	{
		{
			TAG_CLASS.DEATHKNIGHT,
			"GLOBAL_CLASS_DEATHKNIGHT"
		},
		{
			TAG_CLASS.DRUID,
			"GLOBAL_CLASS_DRUID"
		},
		{
			TAG_CLASS.HUNTER,
			"GLOBAL_CLASS_HUNTER"
		},
		{
			TAG_CLASS.MAGE,
			"GLOBAL_CLASS_MAGE"
		},
		{
			TAG_CLASS.PALADIN,
			"GLOBAL_CLASS_PALADIN"
		},
		{
			TAG_CLASS.PRIEST,
			"GLOBAL_CLASS_PRIEST"
		},
		{
			TAG_CLASS.ROGUE,
			"GLOBAL_CLASS_ROGUE"
		},
		{
			TAG_CLASS.SHAMAN,
			"GLOBAL_CLASS_SHAMAN"
		},
		{
			TAG_CLASS.WARLOCK,
			"GLOBAL_CLASS_WARLOCK"
		},
		{
			TAG_CLASS.WARRIOR,
			"GLOBAL_CLASS_WARRIOR"
		},
		{
			TAG_CLASS.DEMONHUNTER,
			"GLOBAL_CLASS_DEMONHUNTER"
		},
		{
			TAG_CLASS.NEUTRAL,
			"GLOBAL_CLASS_NEUTRAL"
		}
	};

	public static Map<TAG_RACE, string> s_raceNames = new Map<TAG_RACE, string>
	{
		{
			TAG_RACE.BLOODELF,
			"GLOBAL_RACE_BLOODELF"
		},
		{
			TAG_RACE.DRAENEI,
			"GLOBAL_RACE_DRAENEI"
		},
		{
			TAG_RACE.DWARF,
			"GLOBAL_RACE_DWARF"
		},
		{
			TAG_RACE.GNOME,
			"GLOBAL_RACE_GNOME"
		},
		{
			TAG_RACE.GOBLIN,
			"GLOBAL_RACE_GOBLIN"
		},
		{
			TAG_RACE.HUMAN,
			"GLOBAL_RACE_HUMAN"
		},
		{
			TAG_RACE.NIGHTELF,
			"GLOBAL_RACE_NIGHTELF"
		},
		{
			TAG_RACE.ORC,
			"GLOBAL_RACE_ORC"
		},
		{
			TAG_RACE.TAUREN,
			"GLOBAL_RACE_TAUREN"
		},
		{
			TAG_RACE.TROLL,
			"GLOBAL_RACE_TROLL"
		},
		{
			TAG_RACE.UNDEAD,
			"GLOBAL_RACE_UNDEAD"
		},
		{
			TAG_RACE.WORGEN,
			"GLOBAL_RACE_WORGEN"
		},
		{
			TAG_RACE.MURLOC,
			"GLOBAL_RACE_MURLOC"
		},
		{
			TAG_RACE.DEMON,
			"GLOBAL_RACE_DEMON"
		},
		{
			TAG_RACE.SCOURGE,
			"GLOBAL_RACE_SCOURGE"
		},
		{
			TAG_RACE.MECHANICAL,
			"GLOBAL_RACE_MECHANICAL"
		},
		{
			TAG_RACE.ELEMENTAL,
			"GLOBAL_RACE_ELEMENTAL"
		},
		{
			TAG_RACE.OGRE,
			"GLOBAL_RACE_OGRE"
		},
		{
			TAG_RACE.PET,
			"GLOBAL_RACE_PET"
		},
		{
			TAG_RACE.TOTEM,
			"GLOBAL_RACE_TOTEM"
		},
		{
			TAG_RACE.NERUBIAN,
			"GLOBAL_RACE_NERUBIAN"
		},
		{
			TAG_RACE.PIRATE,
			"GLOBAL_RACE_PIRATE"
		},
		{
			TAG_RACE.DRAGON,
			"GLOBAL_RACE_DRAGON"
		},
		{
			TAG_RACE.ALL,
			"GLOBAL_RACE_ALL"
		},
		{
			TAG_RACE.EGG,
			"GLOBAL_RACE_EGG"
		},
		{
			TAG_RACE.QUILBOAR,
			"GLOBAL_RACE_QUILBOAR"
		},
		{
			TAG_RACE.CENTAUR,
			"GLOBAL_RACE_CENTAUR"
		},
		{
			TAG_RACE.FURBOLG,
			"GLOBAL_RACE_FURBOLG"
		},
		{
			TAG_RACE.HIGHELF,
			"GLOBAL_RACE_HIGHELF"
		},
		{
			TAG_RACE.TREANT,
			"GLOBAL_RACE_TREANT"
		},
		{
			TAG_RACE.OWLKIN,
			"GLOBAL_RACE_OWLKIN"
		},
		{
			TAG_RACE.HALFORC,
			"GLOBAL_RACE_HALFORC"
		},
		{
			TAG_RACE.LOCK,
			"GLOBAL_RACE_LOCK"
		},
		{
			TAG_RACE.NAGA,
			"GLOBAL_RACE_NAGA"
		},
		{
			TAG_RACE.OLDGOD,
			"GLOBAL_RACE_OLDGOD"
		},
		{
			TAG_RACE.PANDAREN,
			"GLOBAL_RACE_PANDAREN"
		},
		{
			TAG_RACE.GRONN,
			"GLOBAL_RACE_GRONN"
		},
		{
			TAG_RACE.CELESTIAL,
			"GLOBAL_RACE_CELESTIAL"
		},
		{
			TAG_RACE.GNOLL,
			"GLOBAL_RACE_GNOLL"
		},
		{
			TAG_RACE.GOLEM,
			"GLOBAL_RACE_GOLEM"
		},
		{
			TAG_RACE.HARPY,
			"GLOBAL_RACE_HARPY"
		},
		{
			TAG_RACE.VULPERA,
			"GLOBAL_RACE_VULPERA"
		}
	};

	public static Map<TAG_RACE, string> s_raceNamesBattlegrounds = new Map<TAG_RACE, string>
	{
		{
			TAG_RACE.BLOODELF,
			"GLOBAL_RACE_BLOODELF_BATTLEGROUNDS"
		},
		{
			TAG_RACE.DRAENEI,
			"GLOBAL_RACE_DRAENEI_BATTLEGROUNDS"
		},
		{
			TAG_RACE.DWARF,
			"GLOBAL_RACE_DWARF_BATTLEGROUNDS"
		},
		{
			TAG_RACE.GNOME,
			"GLOBAL_RACE_GNOME_BATTLEGROUNDS"
		},
		{
			TAG_RACE.GOBLIN,
			"GLOBAL_RACE_GOBLIN_BATTLEGROUNDS"
		},
		{
			TAG_RACE.HUMAN,
			"GLOBAL_RACE_HUMAN_BATTLEGROUNDS"
		},
		{
			TAG_RACE.NIGHTELF,
			"GLOBAL_RACE_NIGHTELF_BATTLEGROUNDS"
		},
		{
			TAG_RACE.ORC,
			"GLOBAL_RACE_ORC_BATTLEGROUNDS"
		},
		{
			TAG_RACE.TAUREN,
			"GLOBAL_RACE_TAUREN_BATTLEGROUNDS"
		},
		{
			TAG_RACE.TROLL,
			"GLOBAL_RACE_TROLL_BATTLEGROUNDS"
		},
		{
			TAG_RACE.UNDEAD,
			"GLOBAL_RACE_UNDEAD_BATTLEGROUNDS"
		},
		{
			TAG_RACE.WORGEN,
			"GLOBAL_RACE_WORGEN_BATTLEGROUNDS"
		},
		{
			TAG_RACE.MURLOC,
			"GLOBAL_RACE_MURLOC_BATTLEGROUNDS"
		},
		{
			TAG_RACE.DEMON,
			"GLOBAL_RACE_DEMON_BATTLEGROUNDS"
		},
		{
			TAG_RACE.SCOURGE,
			"GLOBAL_RACE_SCOURGE_BATTLEGROUNDS"
		},
		{
			TAG_RACE.MECHANICAL,
			"GLOBAL_RACE_MECHANICAL_BATTLEGROUNDS"
		},
		{
			TAG_RACE.ELEMENTAL,
			"GLOBAL_RACE_ELEMENTAL_BATTLEGROUNDS"
		},
		{
			TAG_RACE.OGRE,
			"GLOBAL_RACE_OGRE_BATTLEGROUNDS"
		},
		{
			TAG_RACE.PET,
			"GLOBAL_RACE_PET_BATTLEGROUNDS"
		},
		{
			TAG_RACE.TOTEM,
			"GLOBAL_RACE_TOTEM_BATTLEGROUNDS"
		},
		{
			TAG_RACE.NERUBIAN,
			"GLOBAL_RACE_NERUBIAN_BATTLEGROUNDS"
		},
		{
			TAG_RACE.PIRATE,
			"GLOBAL_RACE_PIRATE_BATTLEGROUNDS"
		},
		{
			TAG_RACE.DRAGON,
			"GLOBAL_RACE_DRAGON_BATTLEGROUNDS"
		},
		{
			TAG_RACE.ALL,
			"GLOBAL_RACE_ALL_BATTLEGROUNDS"
		},
		{
			TAG_RACE.EGG,
			"GLOBAL_RACE_EGG_BATTLEGROUNDS"
		},
		{
			TAG_RACE.NAGA,
			"GLOBAL_RACE_NAGA_BATTLEGROUNDS"
		},
		{
			TAG_RACE.QUILBOAR,
			"GLOBAL_RACE_QUILBOARS_BATTLEGROUNDS"
		}
	};

	public static Map<TAG_RARITY, string> s_rarityNames = new Map<TAG_RARITY, string>
	{
		{
			TAG_RARITY.COMMON,
			"GLOBAL_RARITY_COMMON"
		},
		{
			TAG_RARITY.EPIC,
			"GLOBAL_RARITY_EPIC"
		},
		{
			TAG_RARITY.LEGENDARY,
			"GLOBAL_RARITY_LEGENDARY"
		},
		{
			TAG_RARITY.RARE,
			"GLOBAL_RARITY_RARE"
		},
		{
			TAG_RARITY.FREE,
			"GLOBAL_RARITY_FREE"
		}
	};

	public static Map<TAG_PREMIUM, string> s_premiumNames = new Map<TAG_PREMIUM, string>
	{
		{
			TAG_PREMIUM.NORMAL,
			"GLOBAL_COLLECTION_NORMAL"
		},
		{
			TAG_PREMIUM.GOLDEN,
			"GLOBAL_COLLECTION_GOLDEN"
		},
		{
			TAG_PREMIUM.DIAMOND,
			"GLOBAL_COLLECTION_DIAMOND"
		},
		{
			TAG_PREMIUM.SIGNATURE,
			"GLOBAL_COLLECTION_SIGNATURE"
		}
	};

	public static Map<TAG_CARD_SET, string> s_cardSetNames = new Map<TAG_CARD_SET, string>
	{
		{
			TAG_CARD_SET.BASIC,
			"GLOBAL_CARD_SET_BASIC"
		},
		{
			TAG_CARD_SET.EXPERT1,
			"GLOBAL_CARD_SET_EXPERT1"
		},
		{
			TAG_CARD_SET.HOF,
			"GLOBAL_CARD_SET_HOF"
		},
		{
			TAG_CARD_SET.PROMO,
			"GLOBAL_CARD_SET_PROMO"
		},
		{
			TAG_CARD_SET.FP1,
			"GLOBAL_CARD_SET_NAXX"
		},
		{
			TAG_CARD_SET.PE1,
			"GLOBAL_CARD_SET_GVG"
		},
		{
			TAG_CARD_SET.BRM,
			"GLOBAL_CARD_SET_BRM"
		},
		{
			TAG_CARD_SET.TGT,
			"GLOBAL_CARD_SET_TGT"
		},
		{
			TAG_CARD_SET.LOE,
			"GLOBAL_CARD_SET_LOE"
		},
		{
			TAG_CARD_SET.OG,
			"GLOBAL_CARD_SET_OG"
		},
		{
			TAG_CARD_SET.OG_RESERVE,
			"GLOBAL_CARD_SET_OG_RESERVE"
		},
		{
			TAG_CARD_SET.SLUSH,
			"GLOBAL_CARD_SET_DEBUG"
		},
		{
			TAG_CARD_SET.KARA,
			"GLOBAL_CARD_SET_KARA"
		},
		{
			TAG_CARD_SET.KARA_RESERVE,
			"GLOBAL_CARD_SET_KARA_RESERVE"
		},
		{
			TAG_CARD_SET.GANGS,
			"GLOBAL_CARD_SET_GANGS"
		},
		{
			TAG_CARD_SET.GANGS_RESERVE,
			"GLOBAL_CARD_SET_GANGS_RESERVE"
		},
		{
			TAG_CARD_SET.UNGORO,
			"GLOBAL_CARD_SET_UNGORO"
		},
		{
			TAG_CARD_SET.ICECROWN,
			"GLOBAL_CARD_SET_ICECROWN"
		},
		{
			TAG_CARD_SET.LOOTAPALOOZA,
			"GLOBAL_CARD_SET_LOOTAPALOOZA"
		},
		{
			TAG_CARD_SET.GILNEAS,
			"GLOBAL_CARD_SET_GILNEAS"
		},
		{
			TAG_CARD_SET.BOOMSDAY,
			"GLOBAL_CARD_SET_BOOMSDAY"
		},
		{
			TAG_CARD_SET.TROLL,
			"GLOBAL_CARD_SET_TROLL"
		},
		{
			TAG_CARD_SET.DALARAN,
			"GLOBAL_CARD_SET_DALARAN"
		},
		{
			TAG_CARD_SET.ULDUM,
			"GLOBAL_CARD_SET_ULDUM"
		},
		{
			TAG_CARD_SET.WILD_EVENT,
			"GLOBAL_CARD_SET_WILD_EVENT"
		},
		{
			TAG_CARD_SET.DRAGONS,
			"GLOBAL_CARD_SET_DRG"
		},
		{
			TAG_CARD_SET.YEAR_OF_THE_DRAGON,
			"GLOBAL_CARD_SET_YOD"
		},
		{
			TAG_CARD_SET.BLACK_TEMPLE,
			"GLOBAL_CARD_SET_BT"
		},
		{
			TAG_CARD_SET.DEMON_HUNTER_INITIATE,
			"GLOBAL_CARD_SET_DHI"
		},
		{
			TAG_CARD_SET.SCHOLOMANCE,
			"GLOBAL_CARD_SET_SCH"
		},
		{
			TAG_CARD_SET.DARKMOON_FAIRE,
			"GLOBAL_CARD_SET_DMF"
		},
		{
			TAG_CARD_SET.THE_BARRENS,
			"GLOBAL_CARD_SET_BAR"
		},
		{
			TAG_CARD_SET.LEGACY,
			"GLOBAL_CARD_SET_LEGACY"
		},
		{
			TAG_CARD_SET.CORE,
			"GLOBAL_CARD_SET_CORE"
		},
		{
			TAG_CARD_SET.VANILLA,
			"GLOBAL_CARD_SET_VANILLA"
		},
		{
			TAG_CARD_SET.STORMWIND,
			"GLOBAL_CARD_SET_SW"
		},
		{
			TAG_CARD_SET.ALTERAC_VALLEY,
			"GLOBAL_CARD_SET_AV"
		},
		{
			TAG_CARD_SET.THE_SUNKEN_CITY,
			"GLOBAL_CARD_SET_TSC"
		},
		{
			TAG_CARD_SET.REVENDRETH,
			"GLOBAL_CARD_SET_REV"
		},
		{
			TAG_CARD_SET.RETURN_OF_THE_LICH_KING,
			"GLOBAL_CARD_SET_RLK"
		},
		{
			TAG_CARD_SET.PATH_OF_ARTHAS,
			"GLOBAL_CARD_SET_PA"
		},
		{
			TAG_CARD_SET.BATTLE_OF_THE_BANDS,
			"GLOBAL_CARD_SET_ETC"
		},
		{
			TAG_CARD_SET.TITANS,
			"GLOBAL_CARD_SET_TTN"
		},
		{
			TAG_CARD_SET.WONDERS,
			"GLOBAL_CARD_SET_WON"
		},
		{
			TAG_CARD_SET.WILD_WEST,
			"GLOBAL_CARD_SET_WST"
		},
		{
			TAG_CARD_SET.WHIZBANGS_WORKSHOP,
			"GLOBAL_CARD_SET_TOY"
		},
		{
			TAG_CARD_SET.TUTORIAL,
			"GLOBAL_CARD_SET_TUT"
		},
		{
			TAG_CARD_SET.EVENT,
			"GLOBAL_CARD_SET_EVE"
		},
		{
			TAG_CARD_SET.CORE_HIDDEN,
			"GLOBAL_CARD_SET_CORE_HIDDEN"
		},
		{
			TAG_CARD_SET.ISLAND_VACATION,
			"GLOBAL_CARD_SET_VAC"
		},
		{
			TAG_CARD_SET.SPACE,
			"GLOBAL_CARD_SET_GDB"
		}
	};

	public static Map<TAG_CARD_SET, string> s_cardSetNamesShortened = new Map<TAG_CARD_SET, string>
	{
		{
			TAG_CARD_SET.BASIC,
			"GLOBAL_CARD_SET_BASIC"
		},
		{
			TAG_CARD_SET.EXPERT1,
			"GLOBAL_CARD_SET_EXPERT1"
		},
		{
			TAG_CARD_SET.HOF,
			"GLOBAL_CARD_SET_HOF"
		},
		{
			TAG_CARD_SET.PROMO,
			"GLOBAL_CARD_SET_PROMO"
		},
		{
			TAG_CARD_SET.FP1,
			"GLOBAL_CARD_SET_NAXX"
		},
		{
			TAG_CARD_SET.PE1,
			"GLOBAL_CARD_SET_GVG"
		},
		{
			TAG_CARD_SET.BRM,
			"GLOBAL_CARD_SET_BRM"
		},
		{
			TAG_CARD_SET.TGT,
			"GLOBAL_CARD_SET_TGT_SHORT"
		},
		{
			TAG_CARD_SET.LOE,
			"GLOBAL_CARD_SET_LOE_SHORT"
		},
		{
			TAG_CARD_SET.OG,
			"GLOBAL_CARD_SET_OG_SHORT"
		},
		{
			TAG_CARD_SET.OG_RESERVE,
			"GLOBAL_CARD_SET_OG_RESERVE"
		},
		{
			TAG_CARD_SET.SLUSH,
			"GLOBAL_CARD_SET_DEBUG"
		},
		{
			TAG_CARD_SET.KARA,
			"GLOBAL_CARD_SET_KARA_SHORT"
		},
		{
			TAG_CARD_SET.KARA_RESERVE,
			"GLOBAL_CARD_SET_KARA_RESERVE"
		},
		{
			TAG_CARD_SET.GANGS,
			"GLOBAL_CARD_SET_GANGS_SHORT"
		},
		{
			TAG_CARD_SET.GANGS_RESERVE,
			"GLOBAL_CARD_SET_GANGS_RESERVE"
		},
		{
			TAG_CARD_SET.UNGORO,
			"GLOBAL_CARD_SET_UNGORO_SHORT"
		},
		{
			TAG_CARD_SET.ICECROWN,
			"GLOBAL_CARD_SET_ICECROWN_SHORT"
		},
		{
			TAG_CARD_SET.LOOTAPALOOZA,
			"GLOBAL_CARD_SET_LOOTAPALOOZA_SHORT"
		},
		{
			TAG_CARD_SET.GILNEAS,
			"GLOBAL_CARD_SET_GILNEAS_SHORT"
		},
		{
			TAG_CARD_SET.BOOMSDAY,
			"GLOBAL_CARD_SET_BOOMSDAY_SHORT"
		},
		{
			TAG_CARD_SET.TROLL,
			"GLOBAL_CARD_SET_TROLL_SHORT"
		},
		{
			TAG_CARD_SET.DALARAN,
			"GLOBAL_CARD_SET_DALARAN_SHORT"
		},
		{
			TAG_CARD_SET.ULDUM,
			"GLOBAL_CARD_SET_ULDUM_SHORT"
		},
		{
			TAG_CARD_SET.WILD_EVENT,
			"GLOBAL_CARD_SET_WILD_EVENT_SHORT"
		},
		{
			TAG_CARD_SET.DRAGONS,
			"GLOBAL_CARD_SET_DRG_SHORT"
		},
		{
			TAG_CARD_SET.YEAR_OF_THE_DRAGON,
			"GLOBAL_CARD_SET_YOD_SHORT"
		},
		{
			TAG_CARD_SET.BLACK_TEMPLE,
			"GLOBAL_CARD_SET_BT_SHORT"
		},
		{
			TAG_CARD_SET.DEMON_HUNTER_INITIATE,
			"GLOBAL_CARD_SET_DHI_SHORT"
		},
		{
			TAG_CARD_SET.SCHOLOMANCE,
			"GLOBAL_CARD_SET_SCH_SHORT"
		},
		{
			TAG_CARD_SET.DARKMOON_FAIRE,
			"GLOBAL_CARD_SET_DMF_SHORT"
		},
		{
			TAG_CARD_SET.THE_BARRENS,
			"GLOBAL_CARD_SET_BAR_SHORT"
		},
		{
			TAG_CARD_SET.LEGACY,
			"GLOBAL_CARD_SET_LEGACY_SHORT"
		},
		{
			TAG_CARD_SET.CORE,
			"GLOBAL_CARD_SET_CORE_SHORT"
		},
		{
			TAG_CARD_SET.VANILLA,
			"GLOBAL_CARD_SET_VANILLA_FORMAT_SHORT"
		},
		{
			TAG_CARD_SET.STORMWIND,
			"GLOBAL_CARD_SET_SW_SHORT"
		},
		{
			TAG_CARD_SET.ALTERAC_VALLEY,
			"GLOBAL_CARD_SET_AV_SHORT"
		},
		{
			TAG_CARD_SET.THE_SUNKEN_CITY,
			"GLOBAL_CARD_SET_TSC_SHORT"
		},
		{
			TAG_CARD_SET.REVENDRETH,
			"GLOBAL_CARD_SET_REV_SHORT"
		},
		{
			TAG_CARD_SET.RETURN_OF_THE_LICH_KING,
			"GLOBAL_CARD_SET_RLK_SHORT"
		},
		{
			TAG_CARD_SET.PATH_OF_ARTHAS,
			"GLOBAL_CARD_SET_PA_SHORT"
		},
		{
			TAG_CARD_SET.BATTLE_OF_THE_BANDS,
			"GLOBAL_CARD_SET_ETC_SHORT"
		},
		{
			TAG_CARD_SET.TITANS,
			"GLOBAL_CARD_SET_TTN_SHORT"
		},
		{
			TAG_CARD_SET.WONDERS,
			"GLOBAL_CARD_SET_WON_SHORT"
		},
		{
			TAG_CARD_SET.WILD_WEST,
			"GLOBAL_CARD_SET_WST_SHORT"
		},
		{
			TAG_CARD_SET.WHIZBANGS_WORKSHOP,
			"GLOBAL_CARD_SET_TOY_SHORT"
		},
		{
			TAG_CARD_SET.TUTORIAL,
			"GLOBAL_CARD_SET_TUT_SHORT"
		},
		{
			TAG_CARD_SET.EVENT,
			"GLOBAL_CARD_SET_EVE_SHORT"
		},
		{
			TAG_CARD_SET.ISLAND_VACATION,
			"GLOBAL_CARD_SET_VAC_SHORT"
		},
		{
			TAG_CARD_SET.SPACE,
			"GLOBAL_CARD_SET_GDB_SHORT"
		}
	};

	public static Map<TAG_CARD_SET, string> s_cardSetNamesInitials = new Map<TAG_CARD_SET, string>
	{
		{
			TAG_CARD_SET.FP1,
			"GLOBAL_CARD_SET_NAXX_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.PE1,
			"GLOBAL_CARD_SET_GVG_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.BRM,
			"GLOBAL_CARD_SET_BRM_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.TGT,
			"GLOBAL_CARD_SET_TGT_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.LOE,
			"GLOBAL_CARD_SET_LOE_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.OG,
			"GLOBAL_CARD_SET_OG_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.GANGS,
			"GLOBAL_CARD_SET_GANGS_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.LOOTAPALOOZA,
			"GLOBAL_CARD_SET_LOOTAPALOOZA_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.BOOMSDAY,
			"GLOBAL_CARD_SET_BOOMSDAY_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.TROLL,
			"GLOBAL_CARD_SET_TROLL_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.DALARAN,
			"GLOBAL_CARD_SET_DALARAN_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.ULDUM,
			"GLOBAL_CARD_SET_ULDUM_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.DRAGONS,
			"GLOBAL_CARD_SET_DRG_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.BLACK_TEMPLE,
			"GLOBAL_CARD_SET_BT_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.DEMON_HUNTER_INITIATE,
			"GLOBAL_CARD_SET_DHI_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.SCHOLOMANCE,
			"GLOBAL_CARD_SET_SCH_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.DARKMOON_FAIRE,
			"GLOBAL_CARD_SET_DMF_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.THE_BARRENS,
			"GLOBAL_CARD_SET_BAR_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.LEGACY,
			"GLOBAL_CARD_SET_LEGACY_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.CORE,
			"GLOBAL_CARD_SET_CORE_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.VANILLA,
			"GLOBAL_CARD_SET_VANILLA_FORMAT_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.STORMWIND,
			"GLOBAL_CARD_SET_SW_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.ALTERAC_VALLEY,
			"GLOBAL_CARD_SET_AV_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.THE_SUNKEN_CITY,
			"GLOBAL_CARD_SET_TSC_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.REVENDRETH,
			"GLOBAL_CARD_SET_REV_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.RETURN_OF_THE_LICH_KING,
			"GLOBAL_CARD_SET_RLK_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.PATH_OF_ARTHAS,
			"GLOBAL_CARD_SET_PA_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.BATTLE_OF_THE_BANDS,
			"GLOBAL_CARD_SET_ETC_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.TITANS,
			"GLOBAL_CARD_SET_TTN_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.WONDERS,
			"GLOBAL_CARD_SET_WON_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.WILD_WEST,
			"GLOBAL_CARD_SET_WST_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.WHIZBANGS_WORKSHOP,
			"GLOBAL_CARD_SET_TOY_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.TUTORIAL,
			"GLOBAL_CARD_SET_TUT_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.EVENT,
			"GLOBAL_CARD_SET_EVE_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.ISLAND_VACATION,
			"GLOBAL_CARD_SET_VAC_SEARCHABLE_SHORTHAND_NAMES"
		},
		{
			TAG_CARD_SET.SPACE,
			"GLOBAL_CARD_SET_GDB_SEARCHABLE_SHORTHAND_NAMES"
		}
	};

	public static Map<TAG_CARD_SET, string> s_miniSetNames = new Map<TAG_CARD_SET, string>
	{
		{
			TAG_CARD_SET.DARKMOON_FAIRE,
			"GLOBAL_MINI_SET_DMF"
		},
		{
			TAG_CARD_SET.THE_BARRENS,
			"GLOBAL_MINI_SET_BAR"
		},
		{
			TAG_CARD_SET.ALTERAC_VALLEY,
			"GLOBAL_MINI_SET_ONY"
		}
	};

	public static Map<TAG_CARDTYPE, string> s_cardTypeNames = new Map<TAG_CARDTYPE, string>
	{
		{
			TAG_CARDTYPE.HERO,
			"GLOBAL_CARDTYPE_HERO"
		},
		{
			TAG_CARDTYPE.MINION,
			"GLOBAL_CARDTYPE_MINION"
		},
		{
			TAG_CARDTYPE.SPELL,
			"GLOBAL_CARDTYPE_SPELL"
		},
		{
			TAG_CARDTYPE.ENCHANTMENT,
			"GLOBAL_CARDTYPE_ENCHANTMENT"
		},
		{
			TAG_CARDTYPE.WEAPON,
			"GLOBAL_CARDTYPE_WEAPON"
		},
		{
			TAG_CARDTYPE.ITEM,
			"GLOBAL_CARDTYPE_ITEM"
		},
		{
			TAG_CARDTYPE.TOKEN,
			"GLOBAL_CARDTYPE_TOKEN"
		},
		{
			TAG_CARDTYPE.HERO_POWER,
			"GLOBAL_CARDTYPE_HEROPOWER"
		},
		{
			TAG_CARDTYPE.LOCATION,
			"GLOBAL_CARDTYPE_LOCATION"
		},
		{
			TAG_CARDTYPE.BATTLEGROUND_HERO_BUDDY,
			"GLOBAL_CARDTYPE_BACONHEROBUDDY"
		},
		{
			TAG_CARDTYPE.BATTLEGROUND_QUEST_REWARD,
			"GLOBAL_CARDTYPE_BACONQUESTREWARD"
		},
		{
			TAG_CARDTYPE.BATTLEGROUND_ANOMALY,
			"GLOBAL_CARDTYPE_ANOMALY"
		},
		{
			TAG_CARDTYPE.BATTLEGROUND_SPELL,
			"GLOBAL_CARDTYPE_SPELL"
		},
		{
			TAG_CARDTYPE.BATTLEGROUND_TRINKET,
			"GLOBAL_CARDTYPE_TRINKET"
		}
	};

	public static Map<TAG_SPELL_SCHOOL, string> s_spellSchoolNames = new Map<TAG_SPELL_SCHOOL, string>
	{
		{
			TAG_SPELL_SCHOOL.ARCANE,
			"GLOBAL_SPELL_SCHOOL_ARCANE"
		},
		{
			TAG_SPELL_SCHOOL.FIRE,
			"GLOBAL_SPELL_SCHOOL_FIRE"
		},
		{
			TAG_SPELL_SCHOOL.FROST,
			"GLOBAL_SPELL_SCHOOL_FROST"
		},
		{
			TAG_SPELL_SCHOOL.NATURE,
			"GLOBAL_SPELL_SCHOOL_NATURE"
		},
		{
			TAG_SPELL_SCHOOL.HOLY,
			"GLOBAL_SPELL_SCHOOL_HOLY"
		},
		{
			TAG_SPELL_SCHOOL.SHADOW,
			"GLOBAL_SPELL_SCHOOL_SHADOW"
		},
		{
			TAG_SPELL_SCHOOL.FEL,
			"GLOBAL_SPELL_SCHOOL_FEL"
		},
		{
			TAG_SPELL_SCHOOL.PHYSICAL_COMBAT,
			"GLOBAL_SPELL_SCHOOL_PHYSICAL_COMBAT"
		},
		{
			TAG_SPELL_SCHOOL.TAVERN,
			"GLOBAL_SPELL_SCHOOL_TAVERN"
		},
		{
			TAG_SPELL_SCHOOL.SPELLCRAFT,
			"GLOBAL_SPELL_SCHOOL_SPELLCRAFT"
		},
		{
			TAG_SPELL_SCHOOL.LESSER_TRINKET,
			"GLOBAL_SPELL_SCHOOL_LESSER_TRINKET"
		},
		{
			TAG_SPELL_SCHOOL.GREATER_TRINKET,
			"GLOBAL_SPELL_SCHOOL_GREATER_TRINKET"
		}
	};

	public static Map<FormatType, string> s_formatNames = new Map<FormatType, string>
	{
		{
			FormatType.FT_STANDARD,
			"GLOBAL_STANDARD"
		},
		{
			FormatType.FT_WILD,
			"GLOBAL_WILD"
		},
		{
			FormatType.FT_CLASSIC,
			"GLOBAL_CLASSIC"
		},
		{
			FormatType.FT_TWIST,
			"GLOBAL_TWIST"
		}
	};

	public static Map<TAG_ROLE, string> s_roleNames = new Map<TAG_ROLE, string>
	{
		{
			TAG_ROLE.FIGHTER,
			"GLOBAL_ROLE_FIGHTER"
		},
		{
			TAG_ROLE.TANK,
			"GLOBAL_ROLE_TANK"
		},
		{
			TAG_ROLE.CASTER,
			"GLOBAL_ROLE_CASTER"
		}
	};

	public static Map<TAG_LETTUCE_FACTION, string> s_mercenaryFactionNames = new Map<TAG_LETTUCE_FACTION, string>
	{
		{
			TAG_LETTUCE_FACTION.ALLIANCE,
			"GLOBAL_MERCENARY_FACTION_ALLIANCE"
		},
		{
			TAG_LETTUCE_FACTION.HORDE,
			"GLOBAL_MERCENARY_FACTION_HORDE"
		},
		{
			TAG_LETTUCE_FACTION.EMPIRE,
			"GLOBAL_MERCENARY_FACTION_EMPIRE"
		},
		{
			TAG_LETTUCE_FACTION.EXPLORER,
			"GLOBAL_MERCENARY_FACTION_EXPLORER"
		},
		{
			TAG_LETTUCE_FACTION.LEGION,
			"GLOBAL_MERCENARY_FACTION_LEGION"
		},
		{
			TAG_LETTUCE_FACTION.PIRATE,
			"GLOBAL_MERCENARY_FACTION_PIRATE"
		},
		{
			TAG_LETTUCE_FACTION.SCOURGE,
			"GLOBAL_MERCENARY_FACTION_SCOURGE"
		}
	};

	public static Map<RewardItem.UnlockableGameMode, string> s_gameModeNames = new Map<RewardItem.UnlockableGameMode, string>
	{
		{
			RewardItem.UnlockableGameMode.TAVERN_BRAWL,
			"GLOBAL_TAVERN_BRAWL"
		},
		{
			RewardItem.UnlockableGameMode.ARENA,
			"GLOBAL_ARENA"
		},
		{
			RewardItem.UnlockableGameMode.BATTLEGROUNDS,
			"GLOBAL_BATTLEGROUNDS"
		},
		{
			RewardItem.UnlockableGameMode.SOLO_ADVENTURES,
			"GLUE_ADVENTURE"
		},
		{
			RewardItem.UnlockableGameMode.MERCENARIES,
			"GLOBAL_MERCENARIES"
		}
	};

	public static Global.GameStringCategory[] NativeGameStringCatetories => s_nativeGameStringCatetories.ToArray();

	public static void LoadAll()
	{
		float start = Time.realtimeSinceStartup;
		foreach (Global.GameStringCategory category in Enum.GetValues(typeof(Global.GameStringCategory)))
		{
			if (category != 0)
			{
				LoadCategory(category, native: false);
			}
		}
		float end = Time.realtimeSinceStartup;
		Log.Performance.Print($"Loading All GameStrings took {end - start}s)");
	}

	public static IEnumerator<IAsyncJobResult> Job_LoadAll()
	{
		JobResultCollection jobs = new JobResultCollection();
		foreach (Global.GameStringCategory category in Enum.GetValues(typeof(Global.GameStringCategory)))
		{
			if (category != 0)
			{
				jobs.Add(CreateLoadCategoryJob(category, native: false));
			}
		}
		yield return jobs;
	}

	private static IAsyncJobResult CreateLoadCategoryJob(Global.GameStringCategory category, bool native)
	{
		return new JobDefinition($"GameStrings.LoadCategory[{category}]", Job_LoadCategory(category, native));
	}

	private static IEnumerator<IAsyncJobResult> Job_LoadCategory(Global.GameStringCategory category, bool native)
	{
		if (s_tables.ContainsKey(category))
		{
			UnloadCategory(category);
		}
		LoadCategory(category, native);
		yield break;
	}

	private static void ReloadAllInternal(bool native)
	{
		float start = Time.realtimeSinceStartup;
		foreach (Global.GameStringCategory category in Enum.GetValues(typeof(Global.GameStringCategory)))
		{
			if (category != 0 && (!native || s_nativeGameStringCatetories.Contains(category)))
			{
				if (s_tables.ContainsKey(category))
				{
					UnloadCategory(category);
				}
				LoadCategory(category, native);
			}
		}
		float end = Time.realtimeSinceStartup;
		Log.Performance.Print(string.Format("Reloading {0} GameStrings took {1}s)", native ? "Native" : "All", end - start));
	}

	public static void ReloadAll()
	{
		ReloadAllInternal(native: false);
	}

	public static void LoadNative()
	{
		ReloadAllInternal(native: true);
	}

	public static string GetAssetPath(Locale locale, string fileName, bool native = false)
	{
		if (native)
		{
			return PlatformFilePaths.GetAssetPath(string.Format("{0}/{1}/{2}", "NativeStrings", locale, fileName), useAssetBundleFolder: false);
		}
		return PlatformFilePaths.GetAssetPath(string.Format("{0}/{1}/{2}", "Strings", locale, fileName));
	}

	public static bool HasKey(string key)
	{
		return Find(key) != null;
	}

	public static string Get(string key)
	{
		if (!FindSafe(key, out var text))
		{
			return text;
		}
		return ParseLanguageRules(text);
	}

	public static string Format(string key, params object[] args)
	{
		if (!FindSafe(key, out var text))
		{
			return text;
		}
		return FormatLocalizedString(text, args);
	}

	public static string FormatLocalizedString(string text, params object[] args)
	{
		text = string.Format(Localization.GetCultureInfo(), text, args);
		text = ParseLanguageRules(text);
		return text;
	}

	public static string FormatLocalizedStringWithPlurals(string text, PluralNumber[] pluralNumbers, params object[] args)
	{
		text = string.Format(Localization.GetCultureInfo(), text, args);
		text = ParseLanguageRules(text, pluralNumbers);
		return text;
	}

	public static string FormatPlurals(string key, PluralNumber[] pluralNumbers, params object[] args)
	{
		if (!FindSafe(key, out var text))
		{
			return text;
		}
		text = string.Format(Localization.GetCultureInfo(), text, args);
		return ParseLanguageRules(text, pluralNumbers);
	}

	public static string FormatStringWithPlurals(List<LocalizedString> protoLocalized, string stringKey, params object[] optionalFormatArgs)
	{
		Locale locale = Localization.GetActualLocale();
		LocalizedString localizedStr = protoLocalized.FirstOrDefault((LocalizedString s) => s.Key == stringKey);
		LocalizedStringValue localizedVal = null;
		if (localizedStr == null)
		{
			Debug.LogWarning($"GameStrings.FormatStringWithPlurals() - localizedStr was null for string key {stringKey}");
			return null;
		}
		localizedVal = localizedStr.Values.FirstOrDefault((LocalizedStringValue v) => v.Locale == (int)locale);
		if (localizedVal.Value == null)
		{
			Debug.LogWarning($"GameStrings.FormatStringWithPlurals() - localizedVal was null");
			return null;
		}
		return ParseLanguageRules(string.Format(localizedVal.Value, optionalFormatArgs));
	}

	public static PluralNumber[] MakePlurals(params int[] quantities)
	{
		List<PluralNumber> pluralNumberList = new List<PluralNumber>();
		for (int i = 0; i < quantities.Length; i++)
		{
			PluralNumber pluralNumber = new PluralNumber
			{
				m_index = i,
				m_number = quantities[i]
			};
			pluralNumberList.Add(pluralNumber);
		}
		return pluralNumberList.ToArray();
	}

	public static string ParseLanguageRules(string str)
	{
		str = ParseLanguageRule1(str);
		str = ParseLanguageRule4(str);
		return str;
	}

	public static string ParseLanguageRules(string str, PluralNumber[] pluralNumbers)
	{
		str = ParseLanguageRule1(str);
		str = ParseLanguageRule4(str, pluralNumbers);
		return str;
	}

	public static bool HasClassName(TAG_CLASS tag)
	{
		return s_classNames.ContainsKey(tag);
	}

	public static string GetClassName(TAG_CLASS tag)
	{
		string key = null;
		if (!s_classNames.TryGetValue(tag, out key))
		{
			return "UNKNOWN";
		}
		return Get(key);
	}

	public static string GetClassesName(IList<TAG_CLASS> tags)
	{
		string key = null;
		for (int i = 0; i < tags.Count; i++)
		{
			TAG_CLASS tag = tags[i];
			key += (s_classNames.TryGetValue(tag, out key) ? Get(key) : "UNKNOWN");
			if (i != tags.Count - 1)
			{
				key += "/";
			}
		}
		return key;
	}

	public static string GetClassesNameComma(IList<TAG_CLASS> tags)
	{
		string key = null;
		for (int i = 0; i < tags.Count; i++)
		{
			TAG_CLASS tag = tags[i];
			key += (s_classNames.TryGetValue(tag, out key) ? Get(key) : "UNKNOWN");
			if (i != tags.Count - 1)
			{
				key += ", ";
			}
		}
		return key;
	}

	public static string GetClassNameKey(TAG_CLASS tag)
	{
		string key = null;
		if (!s_classNames.TryGetValue(tag, out key))
		{
			return null;
		}
		return key;
	}

	public static string GetRoleName(TAG_ROLE tag)
	{
		string key = null;
		if (!s_roleNames.TryGetValue(tag, out key))
		{
			return "UNKNOWN";
		}
		return Get(key);
	}

	public static string GetRoleNameKey(TAG_ROLE tag)
	{
		string key = null;
		if (!s_roleNames.TryGetValue(tag, out key))
		{
			return null;
		}
		return key;
	}

	private static KeywordTextDbfRecord GetKeywordTextRecord(GAME_TAG tag)
	{
		return GameDbf.KeywordText.GetRecord((KeywordTextDbfRecord r) => r.Tag == (int)tag);
	}

	public static bool HasKeywordName(GAME_TAG tag)
	{
		KeywordTextDbfRecord record = GetKeywordTextRecord(tag);
		if (record == null || string.IsNullOrEmpty(record.Name))
		{
			return false;
		}
		return true;
	}

	public static string GetKeywordName(GAME_TAG tag)
	{
		KeywordTextDbfRecord record = GetKeywordTextRecord(tag);
		if (record == null || record.Name == null)
		{
			return "UNKNOWN";
		}
		return Get(GetModeSpecificKey(record.Name));
	}

	public static bool HasKeywordText(GAME_TAG tag)
	{
		KeywordTextDbfRecord record = GetKeywordTextRecord(tag);
		if (record == null || string.IsNullOrEmpty(record.Text))
		{
			return false;
		}
		return true;
	}

	public static string GetKeywordText(GAME_TAG tag)
	{
		KeywordTextDbfRecord record = GetKeywordTextRecord(tag);
		if (record == null || record.Text == null)
		{
			return "UNKNOWN";
		}
		return Get(record.Text);
	}

	public static string GetKeywordTextKey(GAME_TAG tag)
	{
		KeywordTextDbfRecord record = GetKeywordTextRecord(tag);
		if (record == null || record.Text == null)
		{
			return "UNKNOWN";
		}
		return GetModeSpecificKey(record.Text);
	}

	public static bool HasRefKeywordText(GAME_TAG tag)
	{
		KeywordTextDbfRecord record = GetKeywordTextRecord(tag);
		if (record == null || string.IsNullOrEmpty(record.RefText))
		{
			return false;
		}
		return true;
	}

	public static string GetRefKeywordText(GAME_TAG tag)
	{
		KeywordTextDbfRecord record = GetKeywordTextRecord(tag);
		if (record == null || record.RefText == null)
		{
			return "UNKNOWN";
		}
		return Get(record.RefText);
	}

	public static string GetRefKeywordTextKey(GAME_TAG tag)
	{
		KeywordTextDbfRecord record = GetKeywordTextRecord(tag);
		if (record == null || record.RefText == null)
		{
			return "UNKNOWN";
		}
		return GetModeSpecificKey(record.RefText);
	}

	public static bool HasCollectionKeywordText(GAME_TAG tag)
	{
		KeywordTextDbfRecord record = GetKeywordTextRecord(tag);
		if (record == null || string.IsNullOrEmpty(record.CollectionText))
		{
			return false;
		}
		return true;
	}

	public static string GetCollectionKeywordText(GAME_TAG tag)
	{
		KeywordTextDbfRecord record = GetKeywordTextRecord(tag);
		if (record == null || record.CollectionText == null)
		{
			return "UNKNOWN";
		}
		return Get(record.CollectionText);
	}

	public static string GetCollectionKeywordTextKey(GAME_TAG tag)
	{
		KeywordTextDbfRecord record = GetKeywordTextRecord(tag);
		if (record == null || record.CollectionText == null)
		{
			return "UNKNOWN";
		}
		return GetModeSpecificKey(record.CollectionText);
	}

	private static string GetModeSpecificKey(string key)
	{
		if (!(GameState.Get()?.GetGameEntity() is LettuceMissionEntity))
		{
			SceneMgr sceneMgr = SceneMgr.Get();
			if (sceneMgr == null || sceneMgr.GetMode() != SceneMgr.Mode.LETTUCE_COLLECTION)
			{
				SceneMgr sceneMgr2 = SceneMgr.Get();
				if (sceneMgr2 == null || sceneMgr2.GetMode() != SceneMgr.Mode.LETTUCE_MAP)
				{
					goto IL_005f;
				}
			}
		}
		string mercenariesModeKey = key + "_MERC";
		if (HasKey(mercenariesModeKey))
		{
			return mercenariesModeKey;
		}
		goto IL_005f;
		IL_005f:
		return key;
	}

	public static bool HasRarityText(TAG_RARITY tag)
	{
		return s_rarityNames.ContainsKey(tag);
	}

	public static string GetRarityText(TAG_RARITY tag)
	{
		string key = null;
		if (!s_rarityNames.TryGetValue(tag, out key))
		{
			return "UNKNOWN";
		}
		return Get(key);
	}

	public static string GetRarityTextKey(TAG_RARITY tag)
	{
		string key = null;
		if (!s_rarityNames.TryGetValue(tag, out key))
		{
			return null;
		}
		return key;
	}

	public static bool HasPremiumText(TAG_PREMIUM tag)
	{
		return s_premiumNames.ContainsKey(tag);
	}

	public static string GetPremiumText(TAG_PREMIUM tag)
	{
		string key = null;
		if (!s_premiumNames.TryGetValue(tag, out key))
		{
			return "UNKNOWN";
		}
		return Get(key);
	}

	public static bool HasRaceName(TAG_RACE tag)
	{
		return s_raceNames.ContainsKey(tag);
	}

	public static string GetRaceName(TAG_RACE tag)
	{
		string key = null;
		if (!s_raceNames.TryGetValue(tag, out key))
		{
			return "UNKNOWN";
		}
		return Get(key);
	}

	public static string GetRaceNameKey(TAG_RACE tag)
	{
		string key = null;
		if (!s_raceNames.TryGetValue(tag, out key))
		{
			return null;
		}
		return key;
	}

	public static bool HasRaceNameBattlegrounds(TAG_RACE tag)
	{
		return s_raceNamesBattlegrounds.ContainsKey(tag);
	}

	public static string GetRaceNameBattlegrounds(TAG_RACE tag)
	{
		string key = null;
		if (!s_raceNamesBattlegrounds.TryGetValue(tag, out key))
		{
			return "UNKNOWN";
		}
		return Get(key);
	}

	public static string GetRaceNameKeyBattlegrounds(TAG_RACE tag)
	{
		string key = null;
		if (!s_raceNamesBattlegrounds.TryGetValue(tag, out key))
		{
			return null;
		}
		return key;
	}

	public static bool HasCardTypeName(TAG_CARDTYPE tag)
	{
		return s_cardTypeNames.ContainsKey(tag);
	}

	public static string GetCardTypeName(TAG_CARDTYPE tag)
	{
		string key = null;
		if (!s_cardTypeNames.TryGetValue(tag, out key))
		{
			return "UNKNOWN";
		}
		return Get(key);
	}

	public static string GetCardTypeNameKey(TAG_CARDTYPE tag)
	{
		string key = null;
		if (!s_cardTypeNames.TryGetValue(tag, out key))
		{
			return null;
		}
		return key;
	}

	public static bool HasCardSetName(TAG_CARD_SET tag)
	{
		return s_cardSetNames.ContainsKey(tag);
	}

	public static string GetCardSetName(TAG_CARD_SET tag)
	{
		string key = null;
		if (!s_cardSetNames.TryGetValue(tag, out key))
		{
			return "UNKNOWN";
		}
		return Get(key);
	}

	public static string GetCardSetNameKey(TAG_CARD_SET tag)
	{
		string key = null;
		if (!s_cardSetNames.TryGetValue(tag, out key))
		{
			return null;
		}
		return key;
	}

	public static bool HasCardSetNameShortened(TAG_CARD_SET tag)
	{
		return s_cardSetNamesShortened.ContainsKey(tag);
	}

	public static string GetCardSetNameShortened(TAG_CARD_SET tag)
	{
		string key = null;
		if (s_cardSetNamesShortened.TryGetValue(tag, out key))
		{
			return Get(key);
		}
		Log.All.PrintWarning("GetCardSetNameShortened - Could not find a Card Set name for tag {0}; returning {1}", tag, "UNKNOWN");
		return "UNKNOWN";
	}

	public static string GetCardSetNameKeyShortened(TAG_CARD_SET tag)
	{
		string key = null;
		if (!s_cardSetNamesShortened.TryGetValue(tag, out key))
		{
			return null;
		}
		return key;
	}

	public static bool HasCardSetNameInitials(TAG_CARD_SET tag)
	{
		return s_cardSetNamesInitials.ContainsKey(tag);
	}

	public static string GetCardSetNameInitials(TAG_CARD_SET tag)
	{
		string key = null;
		if (!s_cardSetNamesInitials.TryGetValue(tag, out key))
		{
			return "UNKNOWN";
		}
		return Get(key);
	}

	public static bool HasMiniSetName(TAG_CARD_SET tag)
	{
		return s_miniSetNames.ContainsKey(tag);
	}

	public static string GetMiniSetName(TAG_CARD_SET tag)
	{
		if (!s_miniSetNames.TryGetValue(tag, out var key))
		{
			return null;
		}
		return Get(key);
	}

	public static bool HasSpellSchoolName(TAG_SPELL_SCHOOL tag)
	{
		return s_spellSchoolNames.ContainsKey(tag);
	}

	public static string GetSpellSchoolName(TAG_SPELL_SCHOOL tag)
	{
		string key = null;
		if (!s_spellSchoolNames.TryGetValue(tag, out key))
		{
			return "UNKNOWN";
		}
		return Get(key);
	}

	public static bool HasMercenaryFactionName(TAG_LETTUCE_FACTION tag)
	{
		return s_mercenaryFactionNames.ContainsKey(tag);
	}

	public static string GetMercenaryFactionName(TAG_LETTUCE_FACTION tag)
	{
		string key = null;
		if (!s_mercenaryFactionNames.TryGetValue(tag, out key))
		{
			return "UNKNOWN";
		}
		return Get(key);
	}

	public static string GetGameModeName(RewardItem.UnlockableGameMode gameMode)
	{
		string key = null;
		if (!s_gameModeNames.TryGetValue(gameMode, out key))
		{
			return "UNKNOWN";
		}
		return Get(key);
	}

	public static bool HasFormatName(FormatType format)
	{
		return s_formatNames.ContainsKey(format);
	}

	public static string GetFormatName(FormatType format)
	{
		string key = null;
		if (!s_formatNames.TryGetValue(format, out key))
		{
			return "UNKNOWN";
		}
		return Get(key);
	}

	public static string GetRandomTip(TipCategory tipCategory)
	{
		List<string> tips = GetListOfTips(tipCategory);
		if (tips.Count == 0)
		{
			Debug.LogError($"GameStrings.GetRandomTip() - no tips in category {tipCategory}");
			return "UNKNOWN";
		}
		int randomIndex = UnityEngine.Random.Range(0, tips.Count);
		return tips[randomIndex];
	}

	public static string GetRandomTip(List<TipCategory> tipCategories)
	{
		List<string> tips = new List<string>();
		foreach (TipCategory tip in tipCategories)
		{
			tips.AddRange(GetListOfTips(tip));
		}
		if (tips.Count == 0)
		{
			Debug.LogError($"GameStrings.GetRandomTip() - no tips in list of categories");
			return "UNKNOWN";
		}
		int randomIndex = UnityEngine.Random.Range(0, tips.Count);
		return tips[randomIndex];
	}

	public static string GetTip(TipCategory tipCategory, int? tipIndex)
	{
		List<string> tips = GetListOfTips(tipCategory);
		if (tipIndex.HasValue && tipIndex.Value < tips.Count)
		{
			return tips[tipIndex.Value];
		}
		return GetRandomTip(tipCategory);
	}

	private static List<string> GetListOfTips(TipCategory tipCategory)
	{
		int index = 0;
		List<string> tips = new List<string>();
		while (true)
		{
			string tipKeyName = $"GLUE_TIP_{tipCategory}_{index}";
			string tipText = Get(tipKeyName);
			if (tipText.Equals(tipKeyName))
			{
				break;
			}
			if (tipCategory == TipCategory.DEFAULT && index == 25)
			{
				tipText = Format(tipKeyName, SetRotationManager.Get().GetActiveSetRotationYearLocalizedString());
			}
			if (UniversalInputManager.Get().IsTouchMode())
			{
				string touchTipKeyName = tipKeyName + "_TOUCH";
				string touchVersion = Get(touchTipKeyName);
				if (!touchVersion.Equals(touchTipKeyName))
				{
					tipText = touchVersion;
				}
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					string phoneTipKeyName = tipKeyName + "_PHONE";
					string phoneVersion = Get(phoneTipKeyName);
					if (!phoneVersion.Equals(phoneTipKeyName))
					{
						tipText = phoneVersion;
					}
				}
			}
			if (!string.IsNullOrEmpty(tipText))
			{
				tips.Add(tipText);
			}
			index++;
		}
		return tips;
	}

	public static string GetMonthFromDigits(int monthDigits)
	{
		if (Localization.GetLocale() == Locale.thTH)
		{
			return monthDigits switch
			{
				1 => "มกราคม", 
				2 => "ก\u0e38มภาพ\u0e31นธ\u0e4c", 
				3 => "ม\u0e35นาคม", 
				4 => "เมษายน", 
				5 => "พฤษภาคม", 
				6 => "ม\u0e34ถ\u0e38นายน", 
				7 => "กรกฎาคม", 
				8 => "ส\u0e34งหาคม", 
				9 => "ก\u0e31นยายน", 
				10 => "ต\u0e38ลาคม", 
				11 => "พฤศจ\u0e34กายน", 
				12 => "ธ\u0e31นวาคม", 
				_ => string.Empty, 
			};
		}
		return Localization.GetCultureInfo().DateTimeFormat.GetMonthName(monthDigits);
	}

	public static string GetOrdinalNumber(int number)
	{
		string ordinalGameString = "GLUE_ORDINAL_" + number;
		string finalString = Get(ordinalGameString);
		if (finalString == ordinalGameString)
		{
			Debug.LogError($"GameStrings.GetOrdinalNumber() - Unable to find ordinal string for number={number}");
			return number.ToString();
		}
		return finalString;
	}

	private static bool LoadCategory(Global.GameStringCategory cat, bool native)
	{
		if (s_tables.ContainsKey(cat))
		{
			Debug.LogWarning($"GameStrings.LoadCategory() - {cat} is already loaded");
			return false;
		}
		GameStringTable table = new GameStringTable();
		if (!table.Load(cat, native))
		{
			Debug.LogError($"GameStrings.LoadCategory() - {cat} failed to load");
			return false;
		}
		s_tables.Add(cat, table);
		return true;
	}

	private static bool UnloadCategory(Global.GameStringCategory cat)
	{
		if (!s_tables.Remove(cat))
		{
			Debug.LogWarning($"GameStrings.UnloadCategory() - {cat} was never loaded");
			return false;
		}
		return true;
	}

	private static string Find(string key)
	{
		if (key == null)
		{
			return null;
		}
		foreach (KeyValuePair<Global.GameStringCategory, GameStringTable> s_table in s_tables)
		{
			string val = s_table.Value.Get(key);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	private static bool FindSafe(string key, out string value)
	{
		if (key == null)
		{
			Debug.LogError("Attempting to retrieve null key from GameStrings.");
			value = string.Empty;
			return false;
		}
		if (key.Equals(string.Empty))
		{
			value = string.Empty;
			return false;
		}
		value = Find(key);
		if (value != null)
		{
			return true;
		}
		value = key;
		return false;
	}

	private static string[] ParseLanguageRuleArgs(string str, int ruleIndex, out int argStartIndex, out int argEndIndex)
	{
		argStartIndex = -1;
		argEndIndex = -1;
		argStartIndex = str.IndexOf('(', ruleIndex + 2);
		if (argStartIndex < 0)
		{
			Debug.LogWarning($"GameStrings.ParseLanguageRuleArgs() - failed to parse '(' for rule at index {ruleIndex} in string {str}");
			return null;
		}
		argEndIndex = str.IndexOf(')', argStartIndex + 1);
		if (argEndIndex < 0)
		{
			Debug.LogWarning($"GameStrings.ParseLanguageRuleArgs() - failed to parse ')' for rule at index {ruleIndex} in string {str}");
			return null;
		}
		StringBuilder builder = new StringBuilder();
		builder.Append(str, argStartIndex + 1, argEndIndex - argStartIndex - 1);
		string argsString = builder.ToString();
		MatchCollection numberMatches = Regex.Matches(argsString, "(?<!\\/)(?:[0-9]+,)*[0-9]+(?!\\/)");
		if (numberMatches.Count == 0)
		{
			numberMatches = Regex.Matches(argsString, "(?<!\\/)(?:[0-9]+,)*[0-9]+");
		}
		if (numberMatches.Count > 0)
		{
			builder.Remove(0, builder.Length);
			int startIndex = 0;
			foreach (Match numberMatch in numberMatches)
			{
				builder.Append(argsString, startIndex, numberMatch.Index - startIndex);
				builder.Append('0', numberMatch.Length);
				startIndex = numberMatch.Index + numberMatch.Length;
			}
			builder.Append(argsString, startIndex, argsString.Length - startIndex);
			argsString = builder.ToString();
		}
		string[] args = argsString.Split(LANGUAGE_RULE_ARG_DELIMITERS);
		int argsStringIndex = 0;
		for (int i = 0; i < args.Length; i++)
		{
			string arg = args[i];
			if (numberMatches.Count > 0)
			{
				builder.Remove(0, builder.Length);
				int startIndex2 = 0;
				foreach (Match numberMatch2 in numberMatches)
				{
					if (numberMatch2.Index >= argsStringIndex && numberMatch2.Index < argsStringIndex + arg.Length)
					{
						int argNumberIndex = numberMatch2.Index - argsStringIndex;
						builder.Append(arg, startIndex2, argNumberIndex - startIndex2);
						builder.Append(numberMatch2.Value);
						startIndex2 = argNumberIndex + numberMatch2.Length;
					}
				}
				builder.Append(arg, startIndex2, arg.Length - startIndex2);
				arg = builder.ToString();
				argsStringIndex += arg.Length + 1;
			}
			arg = arg.Trim();
			args[i] = arg;
		}
		return args;
	}

	private static bool FindPrecedingChar(string preStr, out int precedingChar)
	{
		int i = preStr.Length - 1;
		precedingChar = preStr[i];
		while (i >= 0)
		{
			precedingChar = preStr[i];
			if (precedingChar >= 44032 && precedingChar <= 55203)
			{
				break;
			}
			if ((precedingChar >= 65 && precedingChar <= 90) || (precedingChar >= 97 && precedingChar <= 122))
			{
				if (precedingChar == 76 || precedingChar == 108 || precedingChar == 82 || precedingChar == 114)
				{
					precedingChar = 51068;
				}
				else if (precedingChar == 77 || precedingChar == 109 || precedingChar == 110 || precedingChar == 110)
				{
					precedingChar = 50689;
				}
				else
				{
					precedingChar = 51060;
				}
				break;
			}
			if (!")}]:;?/*&^!~`/\\|_'\"".Contains((char)precedingChar))
			{
				if (precedingChar == 62 && preStr[i - 3] == '<')
				{
					i -= 3;
				}
				if (precedingChar >= 48 && precedingChar <= 57)
				{
					if (precedingChar == 48 || precedingChar == 51 || precedingChar == 54)
					{
						precedingChar = 50689;
					}
					else if (precedingChar == 49 || precedingChar == 55 || precedingChar == 56)
					{
						precedingChar = 51068;
					}
					else
					{
						precedingChar = 51060;
					}
					break;
				}
			}
			i--;
		}
		if (i < 0)
		{
			precedingChar = 51060;
		}
		return true;
	}

	private static string ParseLanguageRule1(string str)
	{
		int ruleIndex = str.IndexOf("|1");
		if (ruleIndex < 0)
		{
			return str;
		}
		StringBuilder builder = new StringBuilder();
		while (ruleIndex >= 0)
		{
			string preStr = str.Substring(0, ruleIndex);
			if (preStr.Length == 0)
			{
				Debug.LogWarningFormat("GameStrings.ParseLanguageRule1() - invalid preStr, str:{0}, ruleIndex:{1}", str, ruleIndex);
				break;
			}
			int openIndex = str.IndexOf('(', ruleIndex);
			if (openIndex < 0)
			{
				Debug.LogWarningFormat("GameStrings.ParseLanguageRule1() - invalid openIndex, str:{0}, ruleIndex:{1}", str, ruleIndex);
				break;
			}
			int closeIndex = str.IndexOf(')', openIndex);
			if (closeIndex < 0)
			{
				Debug.LogWarningFormat("GameStrings.ParseLanguageRule1() - invalid closeIndex, str:{0}, ruleIndex:{1}, openIndex:{2}", str, ruleIndex);
				break;
			}
			string argStr = str.Substring(openIndex + 1, closeIndex - openIndex - 1);
			string[] args = argStr.Split(',');
			if (args.Length != 2)
			{
				Debug.LogWarningFormat("GameStrings.ParseLanguageRule1() - invalid args, str:{0}, argStr:{1}", str, argStr);
				break;
			}
			if (!FindPrecedingChar(preStr, out var precedingChar))
			{
				Debug.LogWarningFormat("GameStrings.ParseLanguageRule1() - failed to find the preceding character, str:{0}, preStr{1}", str, preStr);
			}
			int index = 0;
			if (precedingChar < 44032 || precedingChar > 55203)
			{
				Debug.LogWarningFormat("GameStrings.ParseLanguageRule1() - invalid precedingChar, str:{0}, precedingChar:{1}", str, precedingChar);
				break;
			}
			int c_index = (precedingChar - 44032) % 28;
			index = ((c_index == 0 || (args[1][0] == '로' && c_index == 8)) ? 1 : 0);
			builder.Append(preStr);
			builder.Append(args[index]);
			str = str.Substring(closeIndex + 1);
			ruleIndex = str.IndexOf("|1");
		}
		builder.Append(str);
		return builder.ToString();
	}

	private static string ParseLanguageRule4(string str, PluralNumber[] pluralNumbers = null)
	{
		StringBuilder builder = null;
		int? number = null;
		int prevRuleEndIndex = 0;
		int ruleCount = 0;
		for (int ruleIndex = str.IndexOf("|4"); ruleIndex >= 0; ruleIndex = str.IndexOf("|4", ruleIndex + 2))
		{
			ruleCount++;
			int argStartIndex;
			int argEndIndex;
			string[] args = ParseLanguageRuleArgs(str, ruleIndex, out argStartIndex, out argEndIndex);
			if (args == null)
			{
				continue;
			}
			int betweenRulesStartIndex = prevRuleEndIndex;
			int betweenRulesEndIndex = ruleIndex - prevRuleEndIndex;
			string betweenRulesStr = str.Substring(betweenRulesStartIndex, betweenRulesEndIndex);
			PluralNumber pluralNumber = null;
			if (pluralNumbers != null)
			{
				int pluralArgIndex = ruleCount - 1;
				pluralNumber = Array.Find(pluralNumbers, (PluralNumber currPluralNumber) => currPluralNumber.m_index == pluralArgIndex);
			}
			int precedingNumber;
			if (pluralNumber != null)
			{
				number = pluralNumber.m_number;
			}
			else if (ParseLanguageRule4Number(args, betweenRulesStr, out precedingNumber))
			{
				number = precedingNumber;
			}
			else if (!number.HasValue)
			{
				Debug.LogWarning($"GameStrings.ParseLanguageRule4() - failed to parse a number in substring \"{betweenRulesStr}\" (indexes {betweenRulesStartIndex}-{betweenRulesEndIndex}) for rule {ruleCount} in string \"{str}\"");
				continue;
			}
			int pluralIndex = GetPluralIndex(number.Value);
			if (pluralIndex >= args.Length)
			{
				Debug.LogWarning($"GameStrings.ParseLanguageRule4() - not enough arguments for rule {ruleCount} in string \"{str}\"");
			}
			else
			{
				string plural = args[pluralIndex];
				if (builder == null)
				{
					builder = new StringBuilder();
				}
				builder.Append(betweenRulesStr);
				builder.Append(plural);
				prevRuleEndIndex = argEndIndex + 1;
			}
			if (pluralNumber != null && pluralNumber.m_useForOnlyThisIndex)
			{
				number = null;
			}
		}
		if (builder == null)
		{
			return str;
		}
		builder.Append(str, prevRuleEndIndex, str.Length - prevRuleEndIndex);
		return builder.ToString();
	}

	private static bool ParseLanguageRule4Number(string[] args, string betweenRulesStr, out int number)
	{
		if (ParseLanguageRule4Number_Foreward(args[0], out number))
		{
			return true;
		}
		if (ParseLanguageRule4Number_Backward(betweenRulesStr, out number))
		{
			return true;
		}
		number = 0;
		return false;
	}

	private static bool ParseLanguageRule4Number_Foreward(string str, out int number)
	{
		number = 0;
		Match match = Regex.Match(str, "(?<!\\/)(?:[0-9]+,)*[0-9]+(?!\\/)");
		if (!match.Success)
		{
			match = Regex.Match(str, "(?<!\\/)(?:[0-9]+,)*[0-9]+");
		}
		if (!match.Success)
		{
			return false;
		}
		if (!GeneralUtils.TryParseInt(match.Value, out number))
		{
			return false;
		}
		return true;
	}

	private static bool ParseLanguageRule4Number_Backward(string str, out int number)
	{
		number = 0;
		MatchCollection matches = Regex.Matches(str, "(?<!\\/)(?:[0-9]+,)*[0-9]+(?!\\/)");
		if (matches.Count == 0)
		{
			matches = Regex.Matches(str, "(?<!\\/)(?:[0-9]+,)*[0-9]+");
		}
		if (matches.Count == 0)
		{
			return false;
		}
		if (!GeneralUtils.TryParseInt(matches[matches.Count - 1].Value, out number))
		{
			return false;
		}
		return true;
	}

	private static int GetPluralIndex(int number)
	{
		switch (Localization.GetLocale())
		{
		case Locale.frFR:
		case Locale.koKR:
		case Locale.zhTW:
		case Locale.zhCN:
			if (number <= 1)
			{
				return 0;
			}
			return 1;
		case Locale.ruRU:
		{
			int num = number % 100;
			if ((uint)(num - 11) <= 3u)
			{
				return 2;
			}
			switch (number % 10)
			{
			case 1:
				return 0;
			case 2:
			case 3:
			case 4:
				return 1;
			default:
				return 2;
			}
		}
		case Locale.plPL:
			switch (number)
			{
			case 1:
				return 0;
			case 0:
				return 2;
			default:
			{
				int num = number % 100;
				if ((uint)(num - 11) <= 3u)
				{
					return 2;
				}
				num = number % 10;
				if ((uint)(num - 2) <= 2u)
				{
					return 1;
				}
				return 2;
			}
			}
		default:
			if (number == 1)
			{
				return 0;
			}
			return 1;
		}
	}
}
