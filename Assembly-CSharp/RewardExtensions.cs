using System.Collections.Generic;
using System.Linq;
using Assets;
using Hearthstone.DataModels;

public static class RewardExtensions
{
	public static BattlegroundsBonusType ToBattlegroundsBonusType(this RewardItem.BattlegroundsBonusType source)
	{
		switch (source)
		{
		case RewardItem.BattlegroundsBonusType.DISCOVER_HERO:
			return BattlegroundsBonusType.DISCOVER_HERO;
		case RewardItem.BattlegroundsBonusType.EARLY_ACCESS_HEROES:
			return BattlegroundsBonusType.EARLY_ACCESS_HEROES;
		case RewardItem.BattlegroundsBonusType.FREE_REROLLS:
			return BattlegroundsBonusType.FREE_REROLLS;
		default:
			Log.All.PrintWarning($"Battlegrounds Bonus Type has unsupported type [{source}]");
			return BattlegroundsBonusType.UNDEFINED;
		}
	}

	public static RewardItemType ToRewardItemType(this RewardItem.RewardType source)
	{
		switch (source)
		{
		case RewardItem.RewardType.NONE:
			return RewardItemType.UNDEFINED;
		case RewardItem.RewardType.GOLD:
			return RewardItemType.GOLD;
		case RewardItem.RewardType.DUST:
			return RewardItemType.DUST;
		case RewardItem.RewardType.ARCANE_ORBS:
			return RewardItemType.CN_ARCANE_ORBS;
		case RewardItem.RewardType.BOOSTER:
			return RewardItemType.BOOSTER;
		case RewardItem.RewardType.CARD:
			return RewardItemType.CARD;
		case RewardItem.RewardType.RANDOM_CARD:
			return RewardItemType.RANDOM_CARD;
		case RewardItem.RewardType.TAVERN_TICKET:
			return RewardItemType.ARENA_TICKET;
		case RewardItem.RewardType.CARD_BACK:
			return RewardItemType.CARD_BACK;
		case RewardItem.RewardType.HERO_SKIN:
			return RewardItemType.HERO_SKIN;
		case RewardItem.RewardType.CUSTOM_COIN:
			return RewardItemType.CUSTOM_COIN;
		case RewardItem.RewardType.REWARD_TRACK_XP_BOOST:
			return RewardItemType.REWARD_TRACK_XP_BOOST;
		case RewardItem.RewardType.CARD_SUBSET:
			return RewardItemType.CARD_SUBSET;
		case RewardItem.RewardType.BATTLEGROUNDS_HERO_SKIN:
			return RewardItemType.BATTLEGROUNDS_HERO_SKIN;
		case RewardItem.RewardType.BATTLEGROUNDS_GUIDE_SKIN:
			return RewardItemType.BATTLEGROUNDS_GUIDE_SKIN;
		case RewardItem.RewardType.BATTLEGROUNDS_SEASON_BONUS:
			return RewardItemType.BATTLEGROUNDS_BONUS;
		case RewardItem.RewardType.BATTLEGROUNDS_BOARD_SKIN:
			return RewardItemType.BATTLEGROUNDS_BOARD_SKIN;
		case RewardItem.RewardType.BATTLEGROUNDS_EMOTE:
			return RewardItemType.BATTLEGROUNDS_EMOTE;
		case RewardItem.RewardType.BATTLEGROUNDS_FINISHER:
			return RewardItemType.BATTLEGROUNDS_FINISHER;
		case RewardItem.RewardType.HERO_CLASS:
			return RewardItemType.HERO_CLASS;
		case RewardItem.RewardType.GAME_MODE:
			return RewardItemType.GAME_MODE;
		case RewardItem.RewardType.DECK:
			return RewardItemType.DECK;
		case RewardItem.RewardType.LOANER_DECKS:
			return RewardItemType.LOANER_DECKS;
		case RewardItem.RewardType.BATTLEGROUNDS_TOKEN:
			return RewardItemType.BATTLEGROUNDS_TOKEN;
		default:
			Log.All.PrintWarning($"RewardItem has unsupported item type [{source}]");
			return RewardItemType.UNDEFINED;
		}
	}

	public static IEnumerable<RewardItemDataModel> Consolidate(this IEnumerable<RewardItemDataModel> rewards)
	{
		return (from element in rewards
			group element by element.ItemType).SelectMany(RewardFactory.ConsolidateGroup);
	}
}
