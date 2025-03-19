using Blizzard.T5.Core.Utils;

namespace Assets;

public static class RewardItem
{
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

	public enum BoosterSelector
	{
		NONE,
		LATEST,
		LATEST_OFFSET_BY_1,
		LATEST_OFFSET_BY_2,
		LATEST_OFFSET_BY_3,
		LATEST_CATCHUP_PACK
	}

	public enum BattlegroundsBonusType
	{
		DISCOVER_HERO,
		EARLY_ACCESS_HEROES,
		FREE_REROLLS
	}

	public enum MercenarySelector
	{
		SPECIFIC,
		CONTEXT,
		RANDOM
	}

	public enum MercenariesPremium
	{
		PREMIUM_NORMAL,
		PREMIUM_GOLDEN,
		PREMIUM_DIAMOND
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

	public static BoosterSelector ParseBoosterSelectorValue(string value)
	{
		EnumUtils.TryGetEnum<BoosterSelector>(value, out var e);
		return e;
	}

	public static BattlegroundsBonusType ParseBattlegroundsBonusTypeValue(string value)
	{
		EnumUtils.TryGetEnum<BattlegroundsBonusType>(value, out var e);
		return e;
	}

	public static MercenarySelector ParseMercenarySelectorValue(string value)
	{
		EnumUtils.TryGetEnum<MercenarySelector>(value, out var e);
		return e;
	}

	public static MercenariesPremium ParseMercenariesPremiumValue(string value)
	{
		EnumUtils.TryGetEnum<MercenariesPremium>(value, out var e);
		return e;
	}

	public static UnlockableGameMode ParseUnlockableGameModeValue(string value)
	{
		EnumUtils.TryGetEnum<UnlockableGameMode>(value, out var e);
		return e;
	}
}
