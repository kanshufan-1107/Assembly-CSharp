using System;
using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class Global
{
	[Flags]
	public enum AssetFlags
	{
		[Description("none")]
		NONE = 0,
		[Description("dev_only")]
		DEV_ONLY = 1,
		[Description("not_packaged_in_client")]
		NOT_PACKAGED_IN_CLIENT = 2,
		[Description("force_do_not_localize")]
		FORCE_DO_NOT_LOCALIZE = 4
	}

	public enum Cardalternatecost
	{
		MANA,
		HEALTH,
		ARMOR,
		CORPSES
	}

	public enum Costcolor
	{
		DEFAULT,
		GREEN,
		RED
	}

	public enum PresenceStatus
	{
		UNKNOWN = -1,
		LOGIN,
		TUTORIAL_PREGAME,
		TUTORIAL_GAME,
		WELCOMEQUESTS,
		HUB,
		STORE,
		QUESTLOG,
		PACKOPENING,
		COLLECTION,
		DECKEDITOR,
		CRAFTING,
		PLAY_DECKPICKER,
		PLAY_QUEUE,
		PLAY_GAME,
		PRACTICE_DECKPICKER,
		PRACTICE_GAME,
		ARENA_PURCHASE,
		ARENA_FORGE,
		ARENA_IDLE,
		ARENA_QUEUE,
		ARENA_GAME,
		ARENA_REWARD,
		FRIENDLY_DECKPICKER,
		FRIENDLY_GAME,
		ADVENTURE_CHOOSING_MODE,
		ADVENTURE_SCENARIO_SELECT,
		ADVENTURE_SCENARIO_PLAYING_GAME,
		SPECTATING_GAME_TUTORIAL,
		SPECTATING_GAME_PRACTICE,
		SPECTATING_GAME_PLAY,
		SPECTATING_GAME_ARENA,
		SPECTATING_GAME_FRIENDLY,
		SPECTATING_GAME_ADVENTURE_NAXX_NORMAL,
		SPECTATING_GAME_ADVENTURE_NAXX_HEROIC,
		SPECTATING_GAME_ADVENTURE_NAXX_CLASS_CHALLENGE,
		SPECTATING_GAME_ADVENTURE_BRM_NORMAL,
		SPECTATING_GAME_ADVENTURE_BRM_HEROIC,
		SPECTATING_GAME_ADVENTURE_BRM_CLASS_CHALLENGE,
		TAVERN_BRAWL_SCREEN,
		TAVERN_BRAWL_DECKEDITOR,
		TAVERN_BRAWL_QUEUE,
		TAVERN_BRAWL_GAME,
		TAVERN_BRAWL_FRIENDLY_WAITING,
		TAVERN_BRAWL_FRIENDLY_GAME,
		SPECTATING_GAME_TAVERN_BRAWL,
		SPECTATING_GAME_ADVENTURE_LOE_NORMAL,
		SPECTATING_GAME_ADVENTURE_LOE_HEROIC,
		SPECTATING_GAME_ADVENTURE_LOE_CLASS_CHALLENGE,
		SPECTATING_GAME_ADVENTURE_KAR_NORMAL,
		SPECTATING_GAME_ADVENTURE_KAR_HEROIC,
		SPECTATING_GAME_ADVENTURE_KAR_CLASS_CHALLENGE,
		SPECTATING_GAME_RETURNING_PLAYER_CHALLENGE,
		FIRESIDE_BRAWL_SCREEN,
		SPECTATING_GAME_ADVENTURE_ICC_NORMAL,
		SPECTATING_GAME_ADVENTURE_LOOT,
		WAIT_FOR_OPPONENT_RECONNECT,
		SPECTATING_GAME_ADVENTURE_GIL,
		SPECTATING_GAME_ADVENTURE_GIL_BONUS_CHALLENGE,
		SPECTATING_GAME_ADVENTURE_BOT,
		SPECTATING_GAME_ADVENTURE_TRL,
		SPECTATING_GAME_ADVENTURE_DAL,
		SPECTATING_GAME_ADVENTURE_DAL_HEROIC,
		SPECTATING_GAME_ADVENTURE_ULD,
		SPECTATING_GAME_ADVENTURE_ULD_HEROIC,
		BATTLEGROUNDS_QUEUE,
		BATTLEGROUNDS_GAME,
		SPECTATING_GAME_BATTLEGROUNDS,
		BATTLEGROUNDS_SCREEN,
		SPECTATING_GAME_ADVENTURE_DRG,
		SPECTATING_GAME_ADVENTURE_DRG_HEROIC,
		SPECTATING_GAME_ADVENTURE_BTP,
		SPECTATING_GAME_ADVENTURE_BTA,
		SPECTATING_GAME_ADVENTURE_BTA_HEROIC,
		SPECTATING_GAME_ADVENTURE_BOH,
		DUELS_QUEUE,
		DUELS_GAME,
		DUELS_BUILDING_DECK,
		DUELS_IDLE,
		SPECTATING_GAME_DUELS,
		DUELS_REWARD,
		DUELS_PURCHASE,
		VIEWING_JOURNAL,
		SPECTATING_GAME_ADVENTURE_BOM,
		PLAY_RANKED_STANDARD,
		PLAY_RANKED_WILD,
		PLAY_RANKED_CLASSIC,
		PLAY_CASUAL_STANDARD,
		PLAY_CASUAL_WILD,
		PLAY_CASUAL_CLASSIC,
		MERCENARIES_QUEUE,
		MERCENARIES_PLAY_SCREEN,
		MERCENARIES_GAME,
		SPECTATING_GAME_MERCENARIES,
		MERCENARIES_COLLECTION,
		MERCENARIES_TEAM_EDITOR,
		MERCENARIES_VILLAGE,
		MERCENARIES_VILLAGE_TASKBOARD,
		MERCENARIES_VILLAGE_BUILDING_MANAGER,
		MERCENARIES_VILLAGE_PVE_ZONES,
		MERCENARIES_VILLAGE_PVE_BOUNTIES,
		MERCENARIES_VILLAGE_PVP,
		MERCENARIES_VILLAGE_MAILBOX,
		MERCENARIES_MAP,
		MERCENARIES_FRIENDLY_LOBBY,
		MERCENARIES_FRIENDLY_GAME,
		SPECTATING_GAME_PLAY_RANKED_STANDARD,
		SPECTATING_GAME_PLAY_RANKED_WILD,
		SPECTATING_GAME_PLAY_RANKED_CLASSIC,
		SPECTATING_GAME_PLAY_CASUAL_STANDARD,
		SPECTATING_GAME_PLAY_CASUAL_WILD,
		SPECTATING_GAME_PLAY_CASUAL_CLASSIC,
		MERCENARIES_VILLAGE_RENOWN_CONVERSION,
		SPECTATING_GAME_ADVENTURE_RLK,
		PLAY_RANKED_TWIST,
		PLAY_CASUAL_TWIST,
		SPECTATING_GAME_PLAY_RANKED_TWIST,
		SPECTATING_GAME_PLAY_CASUAL_TWIST,
		PLAY_BATTLEGROUNDS_DUOS
	}

	public enum Region
	{
		[Description("region_unknown")]
		REGION_UNKNOWN = 0,
		[Description("region_us")]
		REGION_US = 1,
		[Description("region_eu")]
		REGION_EU = 2,
		[Description("region_kr")]
		REGION_KR = 3,
		[Description("region_tw")]
		REGION_TW = 4,
		[Description("region_cn")]
		REGION_CN = 5,
		[Description("region_sg")]
		REGION_SG = 6,
		[Description("region_ptr")]
		REGION_PTR = 98
	}

	public enum FormatType
	{
		[Description("ft_unknown")]
		FT_UNKNOWN,
		[Description("ft_wild")]
		FT_WILD,
		[Description("ft_standard")]
		FT_STANDARD,
		[Description("ft_classic")]
		FT_CLASSIC,
		[Description("ft_twist")]
		FT_TWIST
	}

	public enum RewardType
	{
		NONE = 0,
		GOLD = 1,
		DUST = 2,
		ARCANE_ORBS = 3,
		BOOSTER = 4,
		CARD = 6,
		RANDOM_CARD = 7,
		TAVERN_TICKET = 8,
		CARD_BACK = 9,
		HERO_SKIN = 10,
		CUSTOM_COIN = 11,
		REWARD_TRACK_XP_BOOST = 12,
		CARD_SUBSET = 13,
		MERCENARY_CURRENCY = 14,
		MERCENARY_EQUIPMENT = 15,
		MERCENARY_XP = 16,
		MERCENARY = 18,
		BATTLEGROUNDS_HERO_SKIN = 19,
		BATTLEGROUNDS_GUIDE_SKIN = 20,
		BATTLEGROUNDS_BOARD_SKIN = 21,
		BATTLEGROUNDS_FINISHER = 22,
		BATTLEGROUNDS_EMOTE = 23,
		RENOWN = 24,
		BATTLEGROUNDS_SEASON_BONUS = 25,
		HERO_CLASS = 26,
		GAME_MODE = 27,
		DECK = 28,
		LOANER_DECKS = 29,
		BATTLEGROUNDS_TOKEN = 30
	}

	public enum CardPremiumLevel
	{
		NORMAL,
		GOLDEN,
		DIAMOND,
		SIGNATURE
	}

	public enum MissionEventType
	{
		INVALID = 0,
		[Description("InGame_BossAttacks")]
		INGAME_BOSSATTACKS = 500,
		[Description("InGame_BossAttacksSpecial")]
		INGAME_BOSSATTACKSSPECIAL = 501,
		[Description("InGame_PlayerAttacks")]
		INGAME_PLAYERATTACKS = 502,
		[Description("InGame_PlayerAttacksSpecial")]
		INGAME_PLAYERATTACKSSPECIAL = 503,
		[Description("InGame_VictoryPreExplosion")]
		INGAME_VICTORYPREEXPLOSION = 504,
		[Description("InGame_VictoryPostExplosion")]
		INGAME_VICTORYPOSTEXPLOSION = 505,
		[Description("InGame_LossPreExplosion")]
		INGAME_LOSSPREEXPLOSION = 506,
		[Description("InGame_LossPostExplosion")]
		INGAME_LOSSPOSTEXPLOSION = 507,
		[Description("InGame_PlayerUsesHeroPower")]
		INGAME_PLAYERUSESHEROPOWER = 508,
		[Description("InGame_PlayerUsesHeroPowerSpecial")]
		INGAME_PLAYERUSESHEROPOWERSPECIAL = 509,
		[Description("InGame_BossUsesHeroPower")]
		INGAME_BOSSUSESHEROPOWER = 510,
		[Description("InGame_BossUsesHeroPowerSpecial")]
		INGAME_BOSSUSESHEROPOWERSPECIAL = 511,
		[Description("InGame_PlayerEquipWeapon")]
		INGAME_PLAYEREQUIPWEAPON = 512,
		[Description("InGame_BossEquipWeapon")]
		INGAME_BOSSEQUIPWEAPON = 513,
		[Description("InGame_Introduction")]
		INGAME_INTRODUCTION = 514,
		[Description("InGame_EmoteResponse")]
		INGAME_EMOTERESPONSE = 515,
		[Description("InGame_BossDeath")]
		INGAME_BOSSDEATH = 516,
		[Description("InGame_PlayerIdle")]
		INGAME_PLAYERIDLE = 517,
		[Description("InGame_BossIdle")]
		INGAME_BOSSIDLE = 518
	}

	public enum BnetGameType
	{
		[Description("bgt_unknown")]
		BGT_UNKNOWN = 0,
		[Description("bgt_friends")]
		BGT_FRIENDS = 1,
		[Description("bgt_ranked_standard")]
		BGT_RANKED_STANDARD = 2,
		[Description("bgt_arena")]
		BGT_ARENA = 3,
		[Description("bgt_vs_ai")]
		BGT_VS_AI = 4,
		[Description("bgt_tutorial")]
		BGT_TUTORIAL = 5,
		[Description("bgt_async")]
		BGT_ASYNC = 6,
		[Description("bgt_casual_standard")]
		BGT_CASUAL_STANDARD = 10,
		[Description("bgt_test1")]
		BGT_TEST1 = 11,
		[Description("bgt_test2")]
		BGT_TEST2 = 12,
		[Description("bgt_test3")]
		BGT_TEST3 = 13,
		[Description("bgt_tavernbrawl_pvp")]
		BGT_TAVERNBRAWL_PVP = 16,
		[Description("bgt_tavernbrawl_1p_versus_ai")]
		BGT_TAVERNBRAWL_1P_VERSUS_AI = 17,
		[Description("bgt_tavernbrawl_2p_coop")]
		BGT_TAVERNBRAWL_2P_COOP = 18,
		[Description("bgt_ranked_wild")]
		BGT_RANKED_WILD = 30,
		[Description("bgt_casual_wild")]
		BGT_CASUAL_WILD = 31,
		[Description("bgt_fsg_brawl_vs_friend")]
		BGT_FSG_BRAWL_VS_FRIEND = 40,
		[Description("bgt_fsg_brawl_pvp")]
		BGT_FSG_BRAWL_PVP = 41,
		[Description("bgt_fsg_brawl_1p_versus_ai")]
		BGT_FSG_BRAWL_1P_VERSUS_AI = 42,
		[Description("bgt_fsg_brawl_2p_coop")]
		BGT_FSG_BRAWL_2P_COOP = 43,
		[Description("bgt_ranked_standard_new_player")]
		BGT_RANKED_STANDARD_NEW_PLAYER = 45,
		[Description("bgt_battlegrounds")]
		BGT_BATTLEGROUNDS = 50,
		[Description("bgt_battlegrounds_friendly")]
		BGT_BATTLEGROUNDS_FRIENDLY = 51,
		[Description("bgt_pvpdr_paid")]
		BGT_PVPDR_PAID = 54,
		[Description("bgt_pvpdr")]
		BGT_PVPDR = 55,
		[Description("bgt_mercenaries_pvp")]
		BGT_MERCENARIES_PVP = 56,
		[Description("bgt_mercenaries_pve")]
		BGT_MERCENARIES_PVE = 57,
		[Description("bgt_ranked_classic")]
		BGT_RANKED_CLASSIC = 58,
		[Description("bgt_casual_classic")]
		BGT_CASUAL_CLASSIC = 59,
		[Description("bgt_mercenaries_pve_coop")]
		BGT_MERCENARIES_PVE_COOP = 60,
		[Description("bgt_mercenaries_friendly")]
		BGT_MERCENARIES_FRIENDLY = 61,
		[Description("bgt_battlegrounds_player_vs_ai")]
		BGT_BATTLEGROUNDS_PLAYER_VS_AI = 62,
		[Description("bgt_ranked_twist")]
		BGT_RANKED_TWIST = 63,
		[Description("bgt_casual_twist")]
		BGT_CASUAL_TWIST = 64,
		[Description("bgt_casual_standard_apprentice")]
		BGT_CASUAL_STANDARD_APPRENTICE = 68,
		[Description("bgt_battlegrounds_duo")]
		BGT_BATTLEGROUNDS_DUO = 69,
		[Description("bgt_battlegrounds_duo_vs_ai")]
		BGT_BATTLEGROUNDS_DUO_VS_AI = 66,
		[Description("bgt_battlegrounds_duo_friendly")]
		BGT_BATTLEGROUNDS_DUO_FRIENDLY = 67,
		[Description("bgt_last")]
		BGT_LAST = 68
	}

	public enum SoundCategory
	{
		NONE,
		FX,
		MUSIC,
		VO,
		SPECIAL_VO,
		SPECIAL_CARD,
		AMBIENCE,
		SPECIAL_MUSIC,
		TRIGGER_VO,
		HERO_MUSIC,
		BOSS_VO,
		RESET_GAME,
		TRIGGER_SFX,
		SPECIAL_SFX
	}

	public enum GameStringCategory
	{
		INVALID,
		GLOBAL,
		GLUE,
		GAMEPLAY,
		TUTORIAL,
		PRESENCE,
		MISSION,
		ZILLIAX_DELUXE_3000
	}

	public enum MercenariesPremium
	{
		PREMIUM_NORMAL,
		PREMIUM_GOLDEN,
		PREMIUM_DIAMOND
	}

	[Flags]
	public enum MercenaryRewardRuleFlag
	{
		NONE = 0,
		MERCENARY_PROTECTION_UNOWNED = 1,
		PORTRAIT_PROTECTION = 2,
		MERCENARY_PROTECTION_OWNED = 4,
		PORTRAIT_REQUIRE_PREMIUM_LEVEL = 8,
		MERCENARY_PROTECTION_FALLBACK = 0x10
	}

	[Flags]
	public enum MercenaryRewardSourceFlag
	{
		NONE = 0,
		ANY = 1,
		PACK = 2,
		LICENSE = 4,
		ACHIEVEMENT = 8,
		QUEST = 0x10,
		REWARD_TRACK = 0x20,
		VISITOR_TASK = 0x40,
		SEASON_ROLL = 0x80,
		GAME_PVE = 0x100,
		GAME_PVP = 0x200,
		RENOWN_VENDOR = 0x400
	}

	public enum MercenariesBountyDifficulty
	{
		NONE,
		NORMAL,
		HEROIC,
		MYTHIC
	}

	public enum CardEmoteEvent
	{
		INVALID,
		START,
		THREATEN,
		WELL_PLAYED
	}

	public enum RewardTrackType
	{
		NONE = 0,
		GLOBAL = 1,
		BATTLEGROUNDS = 2,
		EVENT = 7,
		APPRENTICE = 8
	}

	public enum Boardvisualstate
	{
		NONE,
		SHOP,
		COMBAT
	}

	public enum Baconcombatstep
	{
		INVALID,
		COPY_OPPONENT_BOARD,
		END_OF_MINION_COMBAT,
		END_OF_COMBAT,
		DETERMINE_WINNER,
		PREPARE_COPY_BOARD,
		START_COMBAT_TRACKER
	}

	public enum LeagueType
	{
		[Description("unknown")]
		UNKNOWN,
		[Description("normal")]
		NORMAL,
		[Description("new_player")]
		NEW_PLAYER
	}

	public enum LettuceFaction
	{
		[Description("None")]
		NONE,
		[Description("Alliance")]
		ALLIANCE,
		[Description("Horde")]
		HORDE,
		[Description("Empire")]
		EMPIRE,
		[Description("Explorer")]
		EXPLORER,
		[Description("Legion")]
		LEGION,
		[Description("Pirate")]
		PIRATE,
		[Description("Scourge")]
		SCOURGE
	}

	public enum TestFeature
	{
		INVALID,
		BOTS_ON_LADDER,
		[Description("26_6_FTUE_ENGAGEMENT")]
		_26_6_FTUE_ENGAGEMENT
	}

	public enum TestGroup
	{
		CONTROL_GROUP,
		GROUP_A,
		GROUP_B,
		GROUP_C,
		GROUP_D,
		GROUP_E,
		INVALID
	}

	public enum UnlockableGameMode
	{
		INVALID,
		TAVERN_BRAWL,
		ARENA,
		BATTLEGROUNDS,
		SOLO_ADVENTURES,
		DUELS,
		MERCENARIES
	}

	public static AssetFlags ParseAssetFlagsValue(string value)
	{
		EnumUtils.TryGetEnum<AssetFlags>(value, out var e);
		return e;
	}

	public static Cardalternatecost ParseCardalternatecostValue(string value)
	{
		EnumUtils.TryGetEnum<Cardalternatecost>(value, out var e);
		return e;
	}

	public static Costcolor ParseCostcolorValue(string value)
	{
		EnumUtils.TryGetEnum<Costcolor>(value, out var e);
		return e;
	}

	public static PresenceStatus ParsePresenceStatusValue(string value)
	{
		EnumUtils.TryGetEnum<PresenceStatus>(value, out var e);
		return e;
	}

	public static Region ParseRegionValue(string value)
	{
		EnumUtils.TryGetEnum<Region>(value, out var e);
		return e;
	}

	public static FormatType ParseFormatTypeValue(string value)
	{
		EnumUtils.TryGetEnum<FormatType>(value, out var e);
		return e;
	}

	public static RewardType ParseRewardTypeValue(string value)
	{
		EnumUtils.TryGetEnum<RewardType>(value, out var e);
		return e;
	}

	public static CardPremiumLevel ParseCardPremiumLevelValue(string value)
	{
		EnumUtils.TryGetEnum<CardPremiumLevel>(value, out var e);
		return e;
	}

	public static MissionEventType ParseMissionEventTypeValue(string value)
	{
		EnumUtils.TryGetEnum<MissionEventType>(value, out var e);
		return e;
	}

	public static BnetGameType ParseBnetGameTypeValue(string value)
	{
		EnumUtils.TryGetEnum<BnetGameType>(value, out var e);
		return e;
	}

	public static SoundCategory ParseSoundCategoryValue(string value)
	{
		EnumUtils.TryGetEnum<SoundCategory>(value, out var e);
		return e;
	}

	public static GameStringCategory ParseGameStringCategoryValue(string value)
	{
		EnumUtils.TryGetEnum<GameStringCategory>(value, out var e);
		return e;
	}

	public static MercenariesPremium ParseMercenariesPremiumValue(string value)
	{
		EnumUtils.TryGetEnum<MercenariesPremium>(value, out var e);
		return e;
	}

	public static MercenaryRewardRuleFlag ParseMercenaryRewardRuleFlagValue(string value)
	{
		EnumUtils.TryGetEnum<MercenaryRewardRuleFlag>(value, out var e);
		return e;
	}

	public static MercenaryRewardSourceFlag ParseMercenaryRewardSourceFlagValue(string value)
	{
		EnumUtils.TryGetEnum<MercenaryRewardSourceFlag>(value, out var e);
		return e;
	}

	public static MercenariesBountyDifficulty ParseMercenariesBountyDifficultyValue(string value)
	{
		EnumUtils.TryGetEnum<MercenariesBountyDifficulty>(value, out var e);
		return e;
	}

	public static CardEmoteEvent ParseCardEmoteEventValue(string value)
	{
		EnumUtils.TryGetEnum<CardEmoteEvent>(value, out var e);
		return e;
	}

	public static RewardTrackType ParseRewardTrackTypeValue(string value)
	{
		EnumUtils.TryGetEnum<RewardTrackType>(value, out var e);
		return e;
	}

	public static Boardvisualstate ParseBoardvisualstateValue(string value)
	{
		EnumUtils.TryGetEnum<Boardvisualstate>(value, out var e);
		return e;
	}

	public static Baconcombatstep ParseBaconcombatstepValue(string value)
	{
		EnumUtils.TryGetEnum<Baconcombatstep>(value, out var e);
		return e;
	}

	public static LeagueType ParseLeagueTypeValue(string value)
	{
		EnumUtils.TryGetEnum<LeagueType>(value, out var e);
		return e;
	}

	public static LettuceFaction ParseLettuceFactionValue(string value)
	{
		EnumUtils.TryGetEnum<LettuceFaction>(value, out var e);
		return e;
	}

	public static TestFeature ParseTestFeatureValue(string value)
	{
		EnumUtils.TryGetEnum<TestFeature>(value, out var e);
		return e;
	}

	public static TestGroup ParseTestGroupValue(string value)
	{
		EnumUtils.TryGetEnum<TestGroup>(value, out var e);
		return e;
	}

	public static UnlockableGameMode ParseUnlockableGameModeValue(string value)
	{
		EnumUtils.TryGetEnum<UnlockableGameMode>(value, out var e);
		return e;
	}
}
