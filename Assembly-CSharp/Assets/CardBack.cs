using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class CardBack
{
	public enum Source
	{
		[Description("unknown")]
		UNKNOWN = -1,
		[Description("startup")]
		STARTUP = 0,
		[Description("season")]
		SEASON = 1,
		[Description("achieve")]
		ACHIEVE = 2,
		[Description("fixed_reward")]
		FIXED_REWARD = 4,
		[Description("tavern_brawl")]
		TAVERN_BRAWL = 5,
		[Description("reward_system")]
		REWARD_SYSTEM = 6
	}

	public enum SortCategory
	{
		[Description("none")]
		NONE,
		[Description("base")]
		BASE,
		[Description("fireside")]
		FIRESIDE,
		[Description("expansions")]
		EXPANSIONS,
		[Description("hero_skins")]
		HERO_SKINS,
		[Description("seasonal")]
		SEASONAL,
		[Description("legend")]
		LEGEND,
		[Description("esports")]
		ESPORTS,
		[Description("game_licenses")]
		GAME_LICENSES,
		[Description("promotions")]
		PROMOTIONS,
		[Description("pre_purcahse")]
		PRE_PURCAHSE,
		[Description("blizzcon")]
		BLIZZCON,
		[Description("golden_celebration")]
		GOLDEN_CELEBRATION,
		[Description("events")]
		EVENTS
	}

	public static Source ParseSourceValue(string value)
	{
		EnumUtils.TryGetEnum<Source>(value, out var e);
		return e;
	}

	public static SortCategory ParseSortCategoryValue(string value)
	{
		EnumUtils.TryGetEnum<SortCategory>(value, out var e);
		return e;
	}
}
