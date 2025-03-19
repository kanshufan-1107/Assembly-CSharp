using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class FixedReward
{
	public enum Type
	{
		[Description("unknown")]
		UNKNOWN = 0,
		[Description("virtual_card")]
		VIRTUAL_CARD = 1,
		[Description("cardback")]
		CARDBACK = 3,
		[Description("craftable_card")]
		CRAFTABLE_CARD = 4,
		[Description("meta_action_flags")]
		META_ACTION_FLAGS = 5,
		[Description("battlegrounds_guide_skin")]
		BATTLEGROUNDS_GUIDE_SKIN = 6,
		[Description("battlegrounds_hero_skin")]
		BATTLEGROUNDS_HERO_SKIN = 7,
		[Description("battlegrounds_board_skin")]
		BATTLEGROUNDS_BOARD_SKIN = 8,
		[Description("battlegrounds_finisher")]
		BATTLEGROUNDS_FINISHER = 9,
		[Description("battlegrounds_emote")]
		BATTLEGROUNDS_EMOTE = 10,
		[Description("lucky_draw_bonus_hammers")]
		LUCKY_DRAW_BONUS_HAMMERS = 11,
		[Description("hero_skin")]
		HERO_SKIN = 12
	}

	public static Type ParseTypeValue(string value)
	{
		EnumUtils.TryGetEnum<Type>(value, out var e);
		return e;
	}
}
