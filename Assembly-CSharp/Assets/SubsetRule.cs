using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class SubsetRule
{
	public enum Type
	{
		[Description("invalid")]
		INVALID = 0,
		[Description("has_tag_value")]
		HAS_TAG_VALUE = 1,
		[Description("has_odd_numbered_tag_value")]
		HAS_ODD_NUMBERED_TAG_VALUE = 2,
		[Description("is_card_database_id")]
		IS_CARD_DATABASE_ID = 3,
		[Description("is_most_recent_card_set")]
		IS_MOST_RECENT_CARD_SET = 4,
		[Description("is_not_rotated")]
		IS_NOT_ROTATED = 6,
		[Description("can_draft")]
		CAN_DRAFT = 7,
		[Description("is_card_playable")]
		IS_CARD_PLAYABLE = 8,
		[Description("is_active_in_battlegrounds")]
		IS_ACTIVE_IN_BATTLEGROUNDS = 9,
		[Description("is_early_access_in_battlegrounds")]
		IS_EARLY_ACCESS_IN_BATTLEGROUNDS = 10,
		[Description("is_in_every_battlegrounds")]
		IS_IN_EVERY_BATTLEGROUNDS = 11,
		[Description("is_most_recent_expansion_card_set")]
		IS_MOST_RECENT_EXPANSION_CARD_SET = 12,
		[Description("is_multi_class")]
		IS_MULTI_CLASS = 13,
		[Description("has_multiple_types")]
		HAS_MULTIPLE_TYPES = 14
	}

	public static Type ParseTypeValue(string value)
	{
		EnumUtils.TryGetEnum<Type>(value, out var e);
		return e;
	}
}
