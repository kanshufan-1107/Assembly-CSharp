using Blizzard.T5.Core;
using UnityEngine;

public class PlayErrors
{
	public enum ErrorType
	{
		INVALID = -1,
		NONE = 0,
		REQ_MINION_TARGET = 1,
		REQ_FRIENDLY_TARGET = 2,
		REQ_ENEMY_TARGET = 3,
		REQ_DAMAGED_TARGET = 4,
		REQ_MAX_SECRETS = 5,
		REQ_FROZEN_TARGET = 6,
		REQ_CHARGE_TARGET = 7,
		REQ_TARGET_MAX_ATTACK = 8,
		REQ_NONSELF_TARGET = 9,
		REQ_TARGET_WITH_RACE = 10,
		REQ_TARGET_TO_PLAY = 11,
		REQ_NUM_MINION_SLOTS = 12,
		REQ_WEAPON_EQUIPPED = 13,
		REQ_ENOUGH_MANA = 14,
		REQ_YOUR_TURN = 15,
		REQ_NONSTEALTH_ENEMY_TARGET = 16,
		REQ_HERO_TARGET = 17,
		REQ_SECRET_ZONE_CAP = 18,
		REQ_MINION_CAP_IF_TARGET_AVAILABLE = 19,
		REQ_MINION_CAP = 20,
		REQ_TARGET_ATTACKED_THIS_TURN = 21,
		REQ_TARGET_IF_AVAILABLE = 22,
		REQ_MINIMUM_ENEMY_MINIONS = 23,
		REQ_TARGET_FOR_COMBO = 24,
		REQ_NOT_EXHAUSTED_ACTIVATE = 25,
		REQ_UNIQUE_SECRET_OR_QUEST = 26,
		REQ_TARGET_TAUNTER = 27,
		REQ_CAN_BE_ATTACKED = 28,
		REQ_ACTION_PWR_IS_MASTER_PWR = 29,
		REQ_TARGET_MAGNET = 30,
		REQ_ATTACK_GREATER_THAN_0 = 31,
		REQ_ATTACKER_NOT_FROZEN = 32,
		REQ_HERO_OR_MINION_TARGET = 33,
		REQ_CAN_BE_TARGETED_BY_SPELLS = 34,
		REQ_SUBCARD_IS_PLAYABLE = 35,
		REQ_TARGET_FOR_NO_COMBO = 36,
		REQ_NOT_MINION_JUST_PLAYED = 37,
		REQ_NOT_EXHAUSTED_HERO_POWER = 38,
		REQ_CAN_BE_TARGETED_BY_OPPONENTS = 39,
		REQ_ATTACKER_CAN_ATTACK = 40,
		REQ_TARGET_MIN_ATTACK = 41,
		REQ_CAN_BE_TARGETED_BY_HERO_POWERS = 42,
		REQ_ENEMY_TARGET_NOT_IMMUNE = 43,
		REQ_ALL_BASIC_TOTEMS_NOT_IN_PLAY = 44,
		REQ_MINIMUM_TOTAL_MINIONS = 45,
		REQ_MUST_TARGET_TAUNTER = 46,
		REQ_UNDAMAGED_TARGET = 47,
		REQ_CAN_BE_TARGETED_BY_BATTLECRIES = 48,
		REQ_STEADY_SHOT = 49,
		REQ_MINION_OR_ENEMY_HERO = 50,
		REQ_TARGET_IF_AVAILABLE_AND_DRAGON_IN_HAND = 51,
		REQ_LEGENDARY_TARGET = 52,
		REQ_FRIENDLY_MINION_DIED_THIS_TURN = 53,
		REQ_FRIENDLY_MINION_DIED_THIS_GAME = 54,
		REQ_ENEMY_WEAPON_EQUIPPED = 55,
		REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_MINIONS = 56,
		REQ_TARGET_WITH_BATTLECRY = 57,
		REQ_TARGET_WITH_DEATHRATTLE = 58,
		REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_SECRETS = 59,
		REQ_SECRET_ZONE_CAP_FOR_NON_SECRET = 60,
		REQ_TARGET_EXACT_COST = 61,
		REQ_STEALTHED_TARGET = 62,
		REQ_MINION_SLOT_OR_MANA_CRYSTAL_SLOT = 63,
		REQ_MAX_QUESTS = 64,
		REQ_TARGET_IF_AVAILABE_AND_ELEMENTAL_PLAYED_LAST_TURN = 65,
		REQ_TARGET_NOT_VAMPIRE = 66,
		REQ_TARGET_NOT_DAMAGEABLE_ONLY_BY_WEAPONS = 67,
		REQ_NOT_DISABLED_HERO_POWER = 68,
		REQ_MUST_PLAY_OTHER_CARD_FIRST = 69,
		REQ_HAND_NOT_FULL = 70,
		REQ_TARGET_IF_AVAILABLE_AND_NO_3_COST_CARD_IN_DECK = 71,
		REQ_CAN_BE_TARGETED_BY_COMBOS = 72,
		REQ_CANNOT_PLAY_THIS = 73,
		REQ_FRIENDLY_MINIONS_OF_RACE_DIED_THIS_GAME = 74,
		REQ_OPPONENT_PLAYED_CARDS_THIS_GAME = 77,
		REQ_LITERALLY_UNPLAYABLE = 78,
		REQ_TARGET_IF_AVAILABLE_AND_HERO_HAS_ATTACK = 79,
		REQ_FRIENDLY_MINION_OF_RACE_DIED_THIS_TURN = 80,
		REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_SPELLS_PLAYED_THIS_TURN = 81,
		REQ_FRIENDLY_MINION_OF_RACE_IN_HAND = 82,
		REQ_FRIENDLY_DEATHRATTLE_MINION_DIED_THIS_GAME = 86,
		REQ_FRIENDLY_REBORN_MINION_DIED_THIS_GAME = 89,
		REQ_MINION_DIED_THIS_GAME = 90,
		REQ_BOARD_NOT_COMPLETELY_FULL = 92,
		REQ_TARGET_IF_AVAILABLE_AND_HAS_OVERLOADED_MANA = 93,
		REQ_TARGET_IF_AVAILABLE_AND_HERO_ATTACKED_THIS_TURN = 94,
		REQ_TARGET_IF_AVAILABLE_AND_DRAWN_THIS_TURN = 95,
		REQ_TARGET_IF_AVAILABLE_AND_NOT_DRAWN_THIS_TURN = 96,
		REQ_TARGET_NON_TRIPLED_MINION = 97,
		REQ_BOUGHT_MINION_THIS_TURN = 98,
		REQ_SOLD_MINION_THIS_TURN = 99,
		REQ_TARGET_IF_AVAILABLE_AND_PLAYER_HEALTH_CHANGED_THIS_TURN = 100,
		REQ_TARGET_IF_AVAILABLE_AND_SOUL_FRAGMENT_IN_DECK = 101,
		REQ_DAMAGED_TARGET_UNLESS_COMBO = 102,
		REQ_NOT_MINION_DORMANT = 103,
		REQ_TARGET_NOT_UNTOUCHABLE = 104,
		REQ_TARGET_IF_AVAILABLE_AND_BOUGHT_RACE_THIS_TURN = 105,
		REQ_TARGET_IF_AVAILABLE_AND_SOLD_RACE_THIS_TURN = 106,
		REQ_NOT_IN_COOLDOWN = 107,
		REQ_TARGET_IS_MERC = 108,
		REQ_TARGET_IS_NON_MERC = 109,
		REQ_TWO_OF_A_KIND = 110,
		REQ_HAS_OVERLOADED_MANA = 111,
		REQ_LETTUCE_ABILITY_CANNOT_TARGET_OWNER = 112,
		REQ_TARGET_NOT_HAVE_TAG = 116,
		REQ_TARGET_MUST_HAVE_TAG = 117,
		REQ_TRADEABLE = 119,
		REQ_NOT_LEGENDARY_TARGET = 123,
		REQ_MINIMUM_TAVERN_TIER_LEVEL_TO_PLAY = 128,
		REQ_CARD_TAVERN_TIER_LEVEL_TO_PLAY = 129,
		REQ_NOT_EXHAUSTED_LOCATION = 130,
		REQ_LOCATION_TARGET = 131,
		REQ_TARGET_SILVER_HAND_RECRUIT = 132,
		REQ_MINIMUM_CORPSES = 133,
		REQ_LOCATION_OR_MINION_TARGET = 134,
		REQ_CAN_BE_TARGETED_BY_LOCATIONS = 135,
		REQ_FORGE = 136,
		REQ_TARGET_MAX_COST = 137,
		REQ_HAS_PLAYED_SPELL_THIS_GAME = 138,
		REQ_TARGET_IS_NON_TITAN = 141,
		REQ_BACON_DUO_PASSABLE = 142,
		REQ_TARGET_EXACT_ATTACK = 143,
		REQ_MINIMUM_NON_GOLDEN_ENEMY_MINIONS = 146,
		REQ_FRIENDLY_TAUNT_MINION_DIED_THIS_GAME = 148,
		REQ_TARGET_HAS_END_OF_TURN_POWER_TO_TRIGGER_THIS_TURN = 152,
		REQ_MINIMUM_GAME_TURN = 153,
		REQ_ENEMY_MINION_OF_RACE_IN_PLAY = 154,
		REQ_DRAG_TO_PLAY = 999
	}

	private static Map<ErrorType, string> s_playErrorsMessages = new Map<ErrorType, string>
	{
		{
			ErrorType.REQ_MINION_TARGET,
			"GAMEPLAY_PlayErrors_REQ_MINION_TARGET"
		},
		{
			ErrorType.REQ_FRIENDLY_TARGET,
			"GAMEPLAY_PlayErrors_REQ_FRIENDLY_TARGET"
		},
		{
			ErrorType.REQ_ENEMY_TARGET,
			"GAMEPLAY_PlayErrors_REQ_ENEMY_TARGET"
		},
		{
			ErrorType.REQ_DAMAGED_TARGET,
			"GAMEPLAY_PlayErrors_REQ_DAMAGED_TARGET"
		},
		{
			ErrorType.REQ_MAX_SECRETS,
			"GAMEPLAY_PlayErrors_REQ_MAX_SECRETS"
		},
		{
			ErrorType.REQ_FROZEN_TARGET,
			"GAMEPLAY_PlayErrors_REQ_FROZEN_TARGET"
		},
		{
			ErrorType.REQ_CHARGE_TARGET,
			"GAMEPLAY_PlayErrors_REQ_CHARGE_TARGET"
		},
		{
			ErrorType.REQ_TARGET_MAX_ATTACK,
			"GAMEPLAY_PlayErrors_REQ_TARGET_MAX_ATTACK"
		},
		{
			ErrorType.REQ_NONSELF_TARGET,
			"GAMEPLAY_PlayErrors_REQ_NONSELF_TARGET"
		},
		{
			ErrorType.REQ_TARGET_WITH_RACE,
			"GAMEPLAY_PlayErrors_REQ_TARGET_WITH_RACE"
		},
		{
			ErrorType.REQ_TARGET_TO_PLAY,
			"GAMEPLAY_PlayErrors_REQ_TARGET_TO_PLAY"
		},
		{
			ErrorType.REQ_NUM_MINION_SLOTS,
			"GAMEPLAY_PlayErrors_REQ_NUM_MINION_SLOTS"
		},
		{
			ErrorType.REQ_WEAPON_EQUIPPED,
			"GAMEPLAY_PlayErrors_REQ_WEAPON_EQUIPPED"
		},
		{
			ErrorType.REQ_YOUR_TURN,
			"GAMEPLAY_PlayErrors_REQ_YOUR_TURN"
		},
		{
			ErrorType.REQ_NONSTEALTH_ENEMY_TARGET,
			"GAMEPLAY_PlayErrors_REQ_NONSTEALTH_ENEMY_TARGET"
		},
		{
			ErrorType.REQ_HERO_TARGET,
			"GAMEPLAY_PlayErrors_REQ_HERO_TARGET"
		},
		{
			ErrorType.REQ_SECRET_ZONE_CAP,
			"GAMEPLAY_PlayErrors_REQ_SECRET_ZONE_CAP"
		},
		{
			ErrorType.REQ_MINION_CAP_IF_TARGET_AVAILABLE,
			"GAMEPLAY_PlayErrors_REQ_MINION_CAP_IF_TARGET_AVAILABLE"
		},
		{
			ErrorType.REQ_MINION_CAP,
			"GAMEPLAY_PlayErrors_REQ_MINION_CAP"
		},
		{
			ErrorType.REQ_TARGET_ATTACKED_THIS_TURN,
			"GAMEPLAY_PlayErrors_REQ_TARGET_ATTACKED_THIS_TURN"
		},
		{
			ErrorType.REQ_TARGET_IF_AVAILABLE,
			"GAMEPLAY_PlayErrors_REQ_TARGET_IF_AVAILABLE"
		},
		{
			ErrorType.REQ_MINIMUM_ENEMY_MINIONS,
			"GAMEPLAY_PlayErrors_REQ_MINIMUM_ENEMY_MINIONS"
		},
		{
			ErrorType.REQ_TARGET_FOR_COMBO,
			"GAMEPLAY_PlayErrors_REQ_TARGET_FOR_COMBO"
		},
		{
			ErrorType.REQ_NOT_EXHAUSTED_ACTIVATE,
			"GAMEPLAY_PlayErrors_REQ_NOT_EXHAUSTED_ACTIVATE"
		},
		{
			ErrorType.REQ_UNIQUE_SECRET_OR_QUEST,
			"GAMEPLAY_PlayErrors_REQ_UNIQUE_SECRET"
		},
		{
			ErrorType.REQ_CAN_BE_ATTACKED,
			"GAMEPLAY_PlayErrors_REQ_CAN_BE_ATTACKED"
		},
		{
			ErrorType.REQ_ACTION_PWR_IS_MASTER_PWR,
			"GAMEPLAY_PlayErrors_REQ_ACTION_PWR_IS_MASTER_PWR"
		},
		{
			ErrorType.REQ_TARGET_MAGNET,
			"GAMEPLAY_PlayErrors_REQ_TARGET_MAGNET"
		},
		{
			ErrorType.REQ_ATTACK_GREATER_THAN_0,
			"GAMEPLAY_PlayErrors_REQ_ATTACK_GREATER_THAN_0"
		},
		{
			ErrorType.REQ_ATTACKER_NOT_FROZEN,
			"GAMEPLAY_PlayErrors_REQ_ATTACKER_NOT_FROZEN"
		},
		{
			ErrorType.REQ_HERO_OR_MINION_TARGET,
			"GAMEPLAY_PlayErrors_REQ_HERO_OR_MINION_TARGET"
		},
		{
			ErrorType.REQ_CAN_BE_TARGETED_BY_SPELLS,
			"GAMEPLAY_PlayErrors_REQ_CAN_BE_TARGETED_BY_SPELLS"
		},
		{
			ErrorType.REQ_SUBCARD_IS_PLAYABLE,
			"GAMEPLAY_PlayErrors_REQ_SUBCARD_IS_PLAYABLE"
		},
		{
			ErrorType.REQ_TARGET_FOR_NO_COMBO,
			"GAMEPLAY_PlayErrors_REQ_TARGET_FOR_NO_COMBO"
		},
		{
			ErrorType.REQ_NOT_MINION_JUST_PLAYED,
			"GAMEPLAY_PlayErrors_REQ_NOT_MINION_JUST_PLAYED"
		},
		{
			ErrorType.REQ_NOT_EXHAUSTED_HERO_POWER,
			"GAMEPLAY_PlayErrors_REQ_NOT_EXHAUSTED_HERO_POWER"
		},
		{
			ErrorType.REQ_CAN_BE_TARGETED_BY_OPPONENTS,
			"GAMEPLAY_PlayErrors_REQ_CAN_BE_TARGETED_BY_OPPONENTS"
		},
		{
			ErrorType.REQ_ATTACKER_CAN_ATTACK,
			"GAMEPLAY_PlayErrors_REQ_ATTACKER_CAN_ATTACK"
		},
		{
			ErrorType.REQ_TARGET_MIN_ATTACK,
			"GAMEPLAY_PlayErrors_REQ_TARGET_MIN_ATTACK"
		},
		{
			ErrorType.REQ_CAN_BE_TARGETED_BY_HERO_POWERS,
			"GAMEPLAY_PlayErrors_REQ_CAN_BE_TARGETED_BY_HERO_POWERS"
		},
		{
			ErrorType.REQ_ENEMY_TARGET_NOT_IMMUNE,
			"GAMEPLAY_PlayErrors_REQ_ENEMY_TARGET_NOT_IMMUNE"
		},
		{
			ErrorType.REQ_ALL_BASIC_TOTEMS_NOT_IN_PLAY,
			"GAMEPLAY_PlayErrors_REQ_ENTIRE_ENTOURAGE_NOT_IN_PLAY"
		},
		{
			ErrorType.REQ_MINIMUM_TOTAL_MINIONS,
			"GAMEPLAY_PlayErrors_REQ_MINIMUM_TOTAL_MINIONS"
		},
		{
			ErrorType.REQ_MUST_TARGET_TAUNTER,
			"GAMEPLAY_PlayErrors_REQ_MUST_TARGET_TAUNTER"
		},
		{
			ErrorType.REQ_UNDAMAGED_TARGET,
			"GAMEPLAY_PlayErrors_REQ_UNDAMAGED_TARGET"
		},
		{
			ErrorType.REQ_CAN_BE_TARGETED_BY_BATTLECRIES,
			"GAMEPLAY_PlayErrors_REQ_CAN_BE_TARGETED_BY_BATTLECRIES"
		},
		{
			ErrorType.REQ_STEADY_SHOT,
			"GAMEPLAY_PlayErrors_REQ_STEADY_SHOT"
		},
		{
			ErrorType.REQ_MINION_OR_ENEMY_HERO,
			"GAMEPLAY_PlayErrors_REQ_MINION_OR_ENEMY_HERO"
		},
		{
			ErrorType.REQ_TARGET_IF_AVAILABLE_AND_DRAGON_IN_HAND,
			"GAMEPLAY_PlayErrors_REQ_TARGET_IF_AVAILABLE_AND_DRAGON_IN_HAND"
		},
		{
			ErrorType.REQ_TARGET_IF_AVAILABLE_AND_PLAYER_HEALTH_CHANGED_THIS_TURN,
			"GAMEPLAY_PlayErrors_REQ_TARGET_IF_AVAILABLE_AND_PLAYER_HEALTH_CHANGED_THIS_TURN"
		},
		{
			ErrorType.REQ_LEGENDARY_TARGET,
			"GAMEPLAY_PlayErrors_REQ_LEGENDARY_TARGET"
		},
		{
			ErrorType.REQ_NOT_LEGENDARY_TARGET,
			"GAMEPLAY_PlayErrors_REQ_NOT_LEGENDARY_TARGET"
		},
		{
			ErrorType.REQ_FRIENDLY_MINION_DIED_THIS_TURN,
			"GAMEPLAY_PlayErrors_REQ_FRIENDLY_MINION_DIED_THIS_TURN"
		},
		{
			ErrorType.REQ_FRIENDLY_MINION_DIED_THIS_GAME,
			"GAMEPLAY_PlayErrors_REQ_FRIENDLY_MINION_DIED_THIS_GAME"
		},
		{
			ErrorType.REQ_MINION_DIED_THIS_GAME,
			"GAMEPLAY_PlayErrors_REQ_MINION_DIED_THIS_GAME"
		},
		{
			ErrorType.REQ_ENEMY_WEAPON_EQUIPPED,
			"GAMEPLAY_PlayErrors_REQ_ENEMY_WEAPON_EQUIPPED"
		},
		{
			ErrorType.REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_MINIONS,
			"GAMEPLAY_PlayErrors_REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_MINIONS"
		},
		{
			ErrorType.REQ_TARGET_WITH_BATTLECRY,
			"GAMEPLAY_PlayErrors_REQ_TARGET_WITH_BATTLECRY"
		},
		{
			ErrorType.REQ_TARGET_WITH_DEATHRATTLE,
			"GAMEPLAY_PlayErrors_REQ_TARGET_WITH_DEATHRATTLE"
		},
		{
			ErrorType.REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_SECRETS,
			"GAMEPLAY_PlayErrors_REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_SECRETS"
		},
		{
			ErrorType.REQ_STEALTHED_TARGET,
			"GAMEPLAY_PlayErrors_REQ_STEALTHED_TARGET"
		},
		{
			ErrorType.REQ_MINION_SLOT_OR_MANA_CRYSTAL_SLOT,
			"GAMEPLAY_PlayErrors_REQ_MINION_SLOT_OR_MANA_CRYSTAL_SLOT"
		},
		{
			ErrorType.REQ_MAX_QUESTS,
			"GAMEPLAY_PlayErrors_REQ_MAX_QUESTS"
		},
		{
			ErrorType.REQ_TARGET_IF_AVAILABE_AND_ELEMENTAL_PLAYED_LAST_TURN,
			"GAMEPLAY_PlayErrors_REQ_TARGET_IF_AVAILABE_AND_ELEMENTAL_PLAYED_LAST_TURN"
		},
		{
			ErrorType.REQ_TARGET_NOT_VAMPIRE,
			"GAMEPLAY_PlayErrors_REQ_TARGET_NOT_VAMPIRE"
		},
		{
			ErrorType.REQ_TARGET_NOT_DAMAGEABLE_ONLY_BY_WEAPONS,
			"GAMEPLAY_PlayErrors_REQ_TARGET_NOT_DAMAGEABLE_ONLY_BY_WEAPONS"
		},
		{
			ErrorType.REQ_NOT_DISABLED_HERO_POWER,
			"GAMEPLAY_PlayErrors_REQ_NOT_DISABLED_HERO_POWER"
		},
		{
			ErrorType.REQ_MUST_PLAY_OTHER_CARD_FIRST,
			"GAMEPLAY_PlayErrors_REQ_MUST_PLAY_OTHER_CARD_FIRST"
		},
		{
			ErrorType.REQ_HAND_NOT_FULL,
			"GAMEPLAY_PlayErrors_REQ_HAND_NOT_FULL"
		},
		{
			ErrorType.REQ_CAN_BE_TARGETED_BY_COMBOS,
			"GAMEPLAY_PlayErrors_REQ_CAN_BE_TARGETED_BY_COMBOS"
		},
		{
			ErrorType.REQ_CANNOT_PLAY_THIS,
			"GAMEPLAY_PlayErrors_REQ_CANNOT_PLAY_THIS"
		},
		{
			ErrorType.REQ_FRIENDLY_MINIONS_OF_RACE_DIED_THIS_GAME,
			"GAMEPLAY_PlayErrors_REQ_FRIENDLY_MINIONS_OF_RACE_DIED_THIS_GAME"
		},
		{
			ErrorType.REQ_OPPONENT_PLAYED_CARDS_THIS_GAME,
			"GAMEPLAY_PlayErrors_REQ_OPPONENT_PLAYED_CARDS_THIS_GAME"
		},
		{
			ErrorType.REQ_FRIENDLY_MINION_OF_RACE_DIED_THIS_TURN,
			"GAMEPLAY_PlayErrors_REQ_FRIENDLY_MINION_OF_RACE_DIED_THIS_TURN"
		},
		{
			ErrorType.REQ_FRIENDLY_MINION_OF_RACE_IN_HAND,
			"GAMEPLAY_PlayErrors_REQ_FRIENDLY_MINION_OF_RACE_IN_HAND"
		},
		{
			ErrorType.REQ_FRIENDLY_DEATHRATTLE_MINION_DIED_THIS_GAME,
			"GAMEPLAY_PlayErrors_REQ_FRIENDLY_DEATHRATTLE_MINION_DIED_THIS_GAME"
		},
		{
			ErrorType.REQ_FRIENDLY_REBORN_MINION_DIED_THIS_GAME,
			"GAMEPLAY_PlayErrors_REQ_FRIENDLY_REBORN_MINION_DIED_THIS_GAME"
		},
		{
			ErrorType.REQ_LITERALLY_UNPLAYABLE,
			"GAMEPLAY_PlayErrors_REQ_CANNOT_PLAY_THIS"
		},
		{
			ErrorType.REQ_BOARD_NOT_COMPLETELY_FULL,
			"GAMEPLAY_PlayErrors_REQ_CANNOT_PLAY_THIS"
		},
		{
			ErrorType.REQ_NOT_MINION_DORMANT,
			"GAMEPLAY_PlayErrors_REQ_NOT_MINION_DORMANT"
		},
		{
			ErrorType.REQ_TARGET_NOT_UNTOUCHABLE,
			"GAMEPLAY_PlayErrors_REQ_TARGET_NOT_UNTOUCHABLE"
		},
		{
			ErrorType.REQ_TWO_OF_A_KIND,
			"GAMEPLAY_PlayErrors_REQ_TWO_OF_A_KIND"
		},
		{
			ErrorType.REQ_TARGET_NOT_HAVE_TAG,
			"GAMEPLAY_PlayErrors_REQ_TARGET_NOT_HAVE_TAG"
		},
		{
			ErrorType.REQ_TARGET_MUST_HAVE_TAG,
			"GAMEPLAY_PlayErrors_REQ_TARGET_MUST_HAVE_TAG"
		},
		{
			ErrorType.REQ_HAS_OVERLOADED_MANA,
			"GAMEPLAY_PlayErrors_REQ_HAS_OVERLOADED_MANA"
		},
		{
			ErrorType.REQ_TRADEABLE,
			"GAMEPLAY_PlayErrors_REQ_TRADEABLE"
		},
		{
			ErrorType.REQ_MINIMUM_TAVERN_TIER_LEVEL_TO_PLAY,
			"GAMEPLAY_PlayErrors_REQ_MINIMUM_TAVERN_TIER_LEVEL_TO_PLAY"
		},
		{
			ErrorType.REQ_CARD_TAVERN_TIER_LEVEL_TO_PLAY,
			"GAMEPLAY_PlayErrors_REQ_CARD_TAVERN_TIER_LEVEL_TO_PLAY"
		},
		{
			ErrorType.REQ_NOT_EXHAUSTED_LOCATION,
			"GAMEPLAY_PlayErrors_REQ_NOT_EXHAUSTED_LOCATION"
		},
		{
			ErrorType.REQ_LOCATION_TARGET,
			"GAMEPLAY_PlayErrors_REQ_LOCATION_TARGET"
		},
		{
			ErrorType.REQ_TARGET_SILVER_HAND_RECRUIT,
			"GAMEPLAY_PlayErrors_REQ_TARGET_SILVER_HAND_RECRUIT"
		},
		{
			ErrorType.REQ_LOCATION_OR_MINION_TARGET,
			"GAMEPLAY_PlayErrors_REQ_LOCATION_OR_MINION_TARGET"
		},
		{
			ErrorType.REQ_CAN_BE_TARGETED_BY_LOCATIONS,
			"GAMEPLAY_PlayErrors_REQ_CAN_BE_TARGETED_BY_LOCATIONS"
		},
		{
			ErrorType.REQ_HAS_PLAYED_SPELL_THIS_GAME,
			"GAMEPLAY_PlayErrors_REQ_HAS_PLAYED_SPELL_THIS_GAME"
		},
		{
			ErrorType.REQ_TARGET_IS_NON_TITAN,
			"GAMEPLAY_PlayErrors_REQ_TARGET_IS_NON_TITAN"
		},
		{
			ErrorType.REQ_TARGET_EXACT_ATTACK,
			"GAMEPLAY_PlayErrors_REQ_TARGET_EXACT_ATTACK"
		},
		{
			ErrorType.REQ_FRIENDLY_TAUNT_MINION_DIED_THIS_GAME,
			"GAMEPLAY_PlayErrors_REQ_FRIENDLY_TAUNT_MINION_DIED_THIS_GAME"
		},
		{
			ErrorType.REQ_DRAG_TO_PLAY,
			"GAMEPLAY_PlayErrors_REQ_DRAG_TO_PLAY"
		}
	};

	public static void DisplayPlayError(ErrorType error, int? errorParam, Entity errorSource, Entity errorTarget = null, string messageOverride = null)
	{
		Logger playErrors = Log.PlayErrors;
		string[] obj = new string[6]
		{
			"DisplayPlayError: ErrorType = ",
			error.ToString(),
			", ErrorParam = ",
			null,
			null,
			null
		};
		int? num = errorParam;
		obj[3] = num.ToString();
		obj[4] = ", ErrorSource = ";
		obj[5] = errorSource?.ToString();
		playErrors.Print(string.Concat(obj));
		if (GameState.Get().GetGameEntity().NotifyOfPlayError(error, errorParam, errorSource))
		{
			return;
		}
		switch (error)
		{
		case ErrorType.REQ_WEAPON_EQUIPPED:
			GameState.Get().GetFriendlySidePlayer().GetHeroCard()?.PlayEmote(EmoteType.ERROR_NEED_WEAPON);
			break;
		case ErrorType.REQ_NOT_EXHAUSTED_ACTIVATE:
			if (errorSource.IsHero())
			{
				GameState.Get().GetCurrentPlayer().GetHeroCard()?.PlayEmote(EmoteType.ERROR_I_ATTACKED);
			}
			else
			{
				GameState.Get().GetFriendlySidePlayer().GetHeroCard()?.PlayEmote(EmoteType.ERROR_MINION_ATTACKED);
			}
			break;
		case ErrorType.REQ_ENOUGH_MANA:
		{
			EmoteType errorEmote = EmoteType.ERROR_NEED_MANA;
			TAG_CARD_ALTERNATE_COST resourceCost = errorSource.GetTag<TAG_CARD_ALTERNATE_COST>(GAME_TAG.CARD_ALTERNATE_COST);
			if ((errorSource.IsSpell() && DoSpellsCostHealth()) || resourceCost != 0)
			{
				errorEmote = EmoteType.ERROR_PLAY;
			}
			GameState.Get().GetFriendlySidePlayer().GetHeroCard()?.PlayEmote(errorEmote);
			break;
		}
		case ErrorType.REQ_NONSTEALTH_ENEMY_TARGET:
			GameState.Get().GetFriendlySidePlayer().GetHeroCard()?.PlayEmote(EmoteType.ERROR_STEALTH);
			break;
		case ErrorType.REQ_TARGET_TAUNTER:
			DisplayTauntErrorEffects();
			break;
		case ErrorType.REQ_NUM_MINION_SLOTS:
		case ErrorType.REQ_MINION_CAP_IF_TARGET_AVAILABLE:
		case ErrorType.REQ_MINION_CAP:
			GameState.Get().GetFriendlySidePlayer().GetHeroCard()?.PlayEmote(EmoteType.ERROR_FULL_MINIONS);
			break;
		case ErrorType.REQ_NOT_MINION_JUST_PLAYED:
			GameState.Get().GetFriendlySidePlayer().GetHeroCard()?.PlayEmote(EmoteType.ERROR_SUMMON_SICKNESS);
			break;
		case ErrorType.REQ_TARGET_IF_AVAILABLE:
		case ErrorType.REQ_TARGET_FOR_COMBO:
		case ErrorType.REQ_TARGET_FOR_NO_COMBO:
		case ErrorType.REQ_STEADY_SHOT:
		case ErrorType.REQ_TARGET_IF_AVAILABLE_AND_DRAGON_IN_HAND:
		case ErrorType.REQ_FRIENDLY_MINION_DIED_THIS_TURN:
		case ErrorType.REQ_FRIENDLY_MINION_DIED_THIS_GAME:
		case ErrorType.REQ_ENEMY_WEAPON_EQUIPPED:
		case ErrorType.REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_MINIONS:
		case ErrorType.REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_SECRETS:
		case ErrorType.REQ_MINION_SLOT_OR_MANA_CRYSTAL_SLOT:
		case ErrorType.REQ_MUST_PLAY_OTHER_CARD_FIRST:
		case ErrorType.REQ_CANNOT_PLAY_THIS:
		case ErrorType.REQ_FRIENDLY_MINIONS_OF_RACE_DIED_THIS_GAME:
		case ErrorType.REQ_OPPONENT_PLAYED_CARDS_THIS_GAME:
		case ErrorType.REQ_FRIENDLY_MINION_OF_RACE_DIED_THIS_TURN:
		case ErrorType.REQ_FRIENDLY_MINION_OF_RACE_IN_HAND:
		case ErrorType.REQ_FRIENDLY_DEATHRATTLE_MINION_DIED_THIS_GAME:
		case ErrorType.REQ_FRIENDLY_REBORN_MINION_DIED_THIS_GAME:
		case ErrorType.REQ_MINION_DIED_THIS_GAME:
		case ErrorType.REQ_BOARD_NOT_COMPLETELY_FULL:
		case ErrorType.REQ_TARGET_IF_AVAILABLE_AND_PLAYER_HEALTH_CHANGED_THIS_TURN:
		case ErrorType.REQ_FRIENDLY_TAUNT_MINION_DIED_THIS_GAME:
			GameState.Get().GetFriendlySidePlayer().GetHeroCard()?.PlayEmote(EmoteType.ERROR_PLAY);
			break;
		case ErrorType.REQ_TARGET_TO_PLAY:
			if ((errorSource.IsMinion() || errorSource.IsHero()) && errorSource.GetZone() == TAG_ZONE.PLAY)
			{
				GameState.Get().GetFriendlySidePlayer().GetHeroCard()?.PlayEmote(EmoteType.ERROR_GENERIC);
			}
			else
			{
				GameState.Get().GetFriendlySidePlayer().GetHeroCard()?.PlayEmote(EmoteType.ERROR_PLAY);
			}
			break;
		case ErrorType.REQ_MINION_TARGET:
		case ErrorType.REQ_FRIENDLY_TARGET:
		case ErrorType.REQ_ENEMY_TARGET:
		case ErrorType.REQ_DAMAGED_TARGET:
		case ErrorType.REQ_FROZEN_TARGET:
		case ErrorType.REQ_TARGET_MAX_ATTACK:
		case ErrorType.REQ_TARGET_WITH_RACE:
		case ErrorType.REQ_HERO_TARGET:
		case ErrorType.REQ_HERO_OR_MINION_TARGET:
		case ErrorType.REQ_CAN_BE_TARGETED_BY_SPELLS:
		case ErrorType.REQ_CAN_BE_TARGETED_BY_OPPONENTS:
		case ErrorType.REQ_TARGET_MIN_ATTACK:
		case ErrorType.REQ_CAN_BE_TARGETED_BY_HERO_POWERS:
		case ErrorType.REQ_ENEMY_TARGET_NOT_IMMUNE:
		case ErrorType.REQ_CAN_BE_TARGETED_BY_BATTLECRIES:
		case ErrorType.REQ_MINION_OR_ENEMY_HERO:
		case ErrorType.REQ_LEGENDARY_TARGET:
		case ErrorType.REQ_TARGET_WITH_BATTLECRY:
		case ErrorType.REQ_TARGET_WITH_DEATHRATTLE:
		case ErrorType.REQ_TARGET_EXACT_COST:
		case ErrorType.REQ_STEALTHED_TARGET:
		case ErrorType.REQ_TARGET_NON_TRIPLED_MINION:
		case ErrorType.REQ_TWO_OF_A_KIND:
		case ErrorType.REQ_NOT_LEGENDARY_TARGET:
		case ErrorType.REQ_LOCATION_TARGET:
		case ErrorType.REQ_LOCATION_OR_MINION_TARGET:
		case ErrorType.REQ_TARGET_MAX_COST:
		case ErrorType.REQ_TARGET_EXACT_ATTACK:
			GameState.Get().GetFriendlySidePlayer().GetHeroCard()?.PlayEmote(EmoteType.ERROR_TARGET);
			break;
		case ErrorType.REQ_YOUR_TURN:
			return;
		default:
			GameState.Get().GetFriendlySidePlayer().GetHeroCard()?.PlayEmote(EmoteType.ERROR_GENERIC);
			break;
		case ErrorType.REQ_DRAG_TO_PLAY:
			break;
		}
		string errorMsg = ((messageOverride != null) ? GameStrings.Format(messageOverride) : GetErrorDescription(error, errorParam, errorSource, errorTarget));
		if (!string.IsNullOrEmpty(errorMsg))
		{
			GameplayErrorManager.Get().DisplayMessage(errorMsg);
		}
	}

	private static bool CanShowMinionTauntError()
	{
		Player opponentPlayer = GameState.Get().GetOpposingSidePlayer();
		GameState.Get().GetTauntCounts(opponentPlayer, out var minionTaunters, out var heroTaunters);
		if (minionTaunters > 0)
		{
			return heroTaunters == 0;
		}
		return false;
	}

	private static void DisplayTauntErrorEffects()
	{
		if (CanShowMinionTauntError())
		{
			GameState.Get().GetFriendlySidePlayer().GetHeroCard()?.PlayEmote(EmoteType.ERROR_TAUNT);
		}
		GameState.Get().ShowEnemyTauntCharacters();
	}

	private static bool DoSpellsCostHealth()
	{
		return GameState.Get().GetFriendlySidePlayer().HasTag(GAME_TAG.SPELLS_COST_HEALTH);
	}

	private static string GetErrorDescription(ErrorType type, int? errorParam, Entity errorSource, Entity errorTarget)
	{
		Logger playErrors = Log.PlayErrors;
		string text = type.ToString();
		int? num = errorParam;
		playErrors.Print("GetErrorDescription: " + text + " " + num);
		switch (type)
		{
		case ErrorType.NONE:
			Debug.LogWarning("PlayErrors.GetErrorDescription() - Action is not valid, but no error string found.");
			return "";
		case ErrorType.REQ_YOUR_TURN:
			return "";
		case ErrorType.REQ_ACTION_PWR_IS_MASTER_PWR:
			return ErrorInEditorOnly("[Unity Editor] Action power must be master power");
		case ErrorType.REQ_TARGET_MAX_ATTACK:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_TARGET_MAX_ATTACK", errorParam);
		case ErrorType.REQ_TARGET_WITH_RACE:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_TARGET_WITH_RACE", GameStrings.GetRaceName((TAG_RACE)errorParam.Value));
		case ErrorType.REQ_MAX_SECRETS:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_MAX_SECRETS", GameState.Get().GetMaxSecretsPerPlayer());
		case ErrorType.REQ_MAX_QUESTS:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_MAX_QUESTS", GameState.Get().GetMaxQuestsPerPlayer());
		case ErrorType.REQ_SECRET_ZONE_CAP:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_SECRET_ZONE_CAP", GameState.Get().GetMaxSecretZoneSizePerPlayer());
		case ErrorType.REQ_SECRET_ZONE_CAP_FOR_NON_SECRET:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_MAX_SECRETS", GameState.Get().GetMaxSecretsPerPlayer());
		case ErrorType.REQ_MINIMUM_ENEMY_MINIONS:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_MINIMUM_ENEMY_MINIONS", errorParam);
		case ErrorType.REQ_MINIMUM_NON_GOLDEN_ENEMY_MINIONS:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_MINIMUM_NON_GOLDEN_ENEMY_MINIONS", errorParam);
		case ErrorType.REQ_TARGET_MIN_ATTACK:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_TARGET_MIN_ATTACK", errorParam);
		case ErrorType.REQ_TARGET_EXACT_ATTACK:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_TARGET_EXACT_ATTACK", errorParam);
		case ErrorType.REQ_TARGET_EXACT_COST:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_TARGET_EXACT_COST", errorParam);
		case ErrorType.REQ_TARGET_NON_TRIPLED_MINION:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_TARGET_NON_TRIPLED_MINION", errorParam);
		case ErrorType.REQ_BOUGHT_MINION_THIS_TURN:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_BOUGHT_MINION_THIS_TURN", errorParam);
		case ErrorType.REQ_SOLD_MINION_THIS_TURN:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_SOLD_MINION_THIS_TURN", errorParam);
		case ErrorType.REQ_NOT_MINION_DORMANT:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_NOT_MINION_DORMANT", errorParam);
		case ErrorType.REQ_MINIMUM_TOTAL_MINIONS:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_MINIMUM_TOTAL_MINIONS", errorParam);
		case ErrorType.REQ_FRIENDLY_TAUNT_MINION_DIED_THIS_GAME:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_FRIENDLY_TAUNT_MINION_DIED_THIS_GAME", errorParam);
		case ErrorType.REQ_TARGET_TAUNTER:
			if (CanShowMinionTauntError())
			{
				return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_TARGET_TAUNTER_MINION");
			}
			return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_TARGET_TAUNTER_CHARACTER");
		case ErrorType.REQ_ENOUGH_MANA:
		{
			TAG_CARD_ALTERNATE_COST resourceCost = errorSource.GetTag<TAG_CARD_ALTERNATE_COST>(GAME_TAG.CARD_ALTERNATE_COST);
			if ((errorSource.IsSpell() && DoSpellsCostHealth()) || resourceCost == TAG_CARD_ALTERNATE_COST.HEALTH || errorSource.HasTag(GAME_TAG.BACON_COSTS_HEALTH_TO_BUY))
			{
				return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_ENOUGH_HEALTH");
			}
			switch (resourceCost)
			{
			case TAG_CARD_ALTERNATE_COST.ARMOR:
				return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_ENOUGH_ARMOR");
			case TAG_CARD_ALTERNATE_COST.CORPSES:
				return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_MINIMUM_CORPSES");
			default:
				if (errorSource.GetCard() != null && errorSource.GetCard().GetActor() != null && errorSource.GetCard().GetActor().UseCoinManaGem())
				{
					return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_ENOUGH_COIN");
				}
				return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_ENOUGH_MANA");
			}
		}
		case ErrorType.REQ_TARGET_TO_PLAY:
			if ((errorSource.IsMinion() || errorSource.IsHero()) && errorSource.GetZone() == TAG_ZONE.PLAY)
			{
				return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_TARGET_TO_ATTACK");
			}
			break;
		case ErrorType.REQ_STEADY_SHOT:
			if (errorSource.IsHeroPower() && errorSource.GetZone() == TAG_ZONE.PLAY)
			{
				return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_TARGET_IF_AVAILABLE");
			}
			break;
		case ErrorType.REQ_NOT_IN_COOLDOWN:
		{
			int currentCooldown = errorSource.GetTag(GAME_TAG.LETTUCE_CURRENT_COOLDOWN);
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_NOT_IN_COOLDOWN", currentCooldown);
		}
		case ErrorType.REQ_LETTUCE_ABILITY_CANNOT_TARGET_OWNER:
			return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_LETTUCE_ABILITY_CANNOT_TARGET_OWNER");
		case ErrorType.REQ_MINIMUM_TAVERN_TIER_LEVEL_TO_PLAY:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_MINIMUM_TAVERN_TIER_LEVEL_TO_PLAY", errorParam);
		case ErrorType.REQ_CARD_TAVERN_TIER_LEVEL_TO_PLAY:
			return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_CARD_TAVERN_TIER_LEVEL_TO_PLAY");
		case ErrorType.REQ_NOT_EXHAUSTED_LOCATION:
		{
			int numTurnOnCooldown = errorSource.GetLocationCooldown();
			if (numTurnOnCooldown == 1)
			{
				return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_NOT_EXHAUSTED_LOCATION");
			}
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_NOT_EXHAUSTED_LOCATION_COOLDOWN", numTurnOnCooldown);
		}
		case ErrorType.REQ_MINIMUM_CORPSES:
			return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_MINIMUM_CORPSES");
		case ErrorType.REQ_TARGET_NOT_UNTOUCHABLE:
			if (errorTarget != null && errorTarget.IsDormant())
			{
				return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_TARGET_NOT_DORMANT");
			}
			break;
		case ErrorType.REQ_SUBCARD_IS_PLAYABLE:
			if (errorSource.IsTitan())
			{
				return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_SUBCARD_IS_PLAYABLE_TITAN");
			}
			break;
		case ErrorType.REQ_NOT_EXHAUSTED_ACTIVATE:
		case ErrorType.REQ_CANNOT_PLAY_THIS:
			if (errorSource.IsTitan())
			{
				if (errorSource.IsFrozen())
				{
					return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_ATTACKER_NOT_FROZEN");
				}
				return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_NOT_EXHAUSTED_ACTIVATE_TITAN");
			}
			break;
		case ErrorType.REQ_TARGET_MAX_COST:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_TARGET_MAX_COST", errorParam);
		case ErrorType.REQ_TARGET_HAS_END_OF_TURN_POWER_TO_TRIGGER_THIS_TURN:
			return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_TARGET_HAS_END_OF_TURN_POWER_TO_TRIGGER_THIS_TURN");
		case ErrorType.REQ_MINIMUM_GAME_TURN:
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_MINIMUM_GAME_TURN", errorParam);
		case ErrorType.REQ_ENEMY_MINION_OF_RACE_IN_PLAY:
		{
			string raceName = GameStrings.GetRaceNameBattlegrounds((TAG_RACE)errorParam.Value);
			if (GameMgr.Get().IsBattlegrounds())
			{
				return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_ENEMY_MINION_OF_RACE_IN_PLAY_BG", raceName);
			}
			return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_ENEMY_MINION_OF_RACE_IN_PLAY", raceName);
		}
		}
		string key = null;
		if (s_playErrorsMessages.TryGetValue(type, out key))
		{
			return GameStrings.Get(key);
		}
		return ErrorInEditorOnly("[Unity Editor] Unknown play error ({0})", type);
	}

	private static string ErrorInEditorOnly(string format, params object[] args)
	{
		return "";
	}
}
