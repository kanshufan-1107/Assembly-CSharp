using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class DeckRulesetRule
{
	public enum RuleType
	{
		[Description("invalid_rule_type")]
		INVALID_RULE_TYPE = 0,
		[Description("has_tag_value")]
		HAS_TAG_VALUE = 1,
		[Description("has_odd_numbered_tag_value")]
		HAS_ODD_NUMBERED_TAG_VALUE = 2,
		[Description("count_cards_in_deck")]
		COUNT_CARDS_IN_DECK = 3,
		[Description("count_copies_of_each_card")]
		COUNT_COPIES_OF_EACH_CARD = 4,
		[Description("count_cards_with_tag_value")]
		COUNT_CARDS_WITH_TAG_VALUE = 5,
		[Description("count_cards_with_tag_value_odd_numbered")]
		COUNT_CARDS_WITH_TAG_VALUE_ODD_NUMBERED = 6,
		[Description("count_cards_with_same_tag_value")]
		COUNT_CARDS_WITH_SAME_TAG_VALUE = 7,
		[Description("count_unique_tag_values")]
		COUNT_UNIQUE_TAG_VALUES = 8,
		[Description("is_in_any_subset")]
		IS_IN_ANY_SUBSET = 9,
		[Description("is_in_all_subsets")]
		IS_IN_ALL_SUBSETS = 10,
		[Description("card_text_contains_substring")]
		CARD_TEXT_CONTAINS_SUBSTRING = 11,
		[Description("player_owns_each_copy")]
		PLAYER_OWNS_EACH_COPY = 12,
		[Description("is_not_rotated")]
		IS_NOT_ROTATED = 13,
		[Description("deck_size")]
		DECK_SIZE = 14,
		[Description("is_class_or_neutral_card")]
		IS_CLASS_OR_NEUTRAL_CARD = 15,
		[Description("is_card_playable")]
		IS_CARD_PLAYABLE = 16,
		[Description("is_not_banned_in_league")]
		IS_NOT_BANNED_IN_LEAGUE = 17,
		[Description("is_active_in_battlegrounds")]
		IS_ACTIVE_IN_BATTLEGROUNDS = 18,
		[Description("is_early_access_in_battlegrounds")]
		IS_EARLY_ACCESS_IN_BATTLEGROUNDS = 19,
		[Description("is_in_cardset")]
		IS_IN_CARDSET = 20,
		[Description("is_in_format")]
		IS_IN_FORMAT = 21,
		[Description("editing_deck_extra_card_count")]
		EDITING_DECK_EXTRA_CARD_COUNT = 22,
		[Description("deathknight_rune_limit")]
		DEATHKNIGHT_RUNE_LIMIT = 23,
		[Description("sideboard_card_count_limit")]
		SIDEBOARD_CARD_COUNT_LIMIT = 24,
		[Description("sideboard_has_tag_value")]
		SIDEBOARD_HAS_TAG_VALUE = 25,
		[Description("player_owns_deck_template")]
		PLAYER_OWNS_DECK_TEMPLATE = 27,
		[Description("tourist_limit")]
		TOURIST_LIMIT = 28
	}

	public static RuleType ParseRuleTypeValue(string value)
	{
		EnumUtils.TryGetEnum<RuleType>(value, out var e);
		return e;
	}
}
