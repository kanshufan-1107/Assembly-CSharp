using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class RewardBag
{
	public enum Reward
	{
		[Description("unknown")]
		UNKNOWN = -1,
		[Description("none")]
		NONE = 0,
		[Description("pack")]
		PACK = 1,
		[Description("gold")]
		GOLD = 2,
		[Description("dust")]
		DUST = 3,
		[Description("com")]
		COM = 4,
		[Description("rare")]
		RARE = 5,
		[Description("epic")]
		EPIC = 6,
		[Description("leg")]
		LEG = 7,
		[Description("grare")]
		GRARE = 8,
		[Description("gcom")]
		GCOM = 9,
		[Description("gepic")]
		GEPIC = 10,
		[Description("gleg")]
		GLEG = 11,
		[Description("pack2_deprecated")]
		PACK2_DEPRECATED = 12,
		[Description("seasonal_card_back")]
		SEASONAL_CARD_BACK = 13,
		[Description("latest_pack")]
		LATEST_PACK = 14,
		[Description("not_latest_pack")]
		NOT_LATEST_PACK = 15,
		[Description("random_card")]
		RANDOM_CARD = 16,
		[Description("golden_random_card")]
		GOLDEN_RANDOM_CARD = 17,
		[Description("forge")]
		FORGE = 18,
		[Description("specific_pack")]
		SPECIFIC_PACK = 19,
		[Description("pack_offset_from_latest")]
		PACK_OFFSET_FROM_LATEST = 20,
		[Description("reward_chest_contents")]
		REWARD_CHEST_CONTENTS = 21,
		[Description("specific_card")]
		SPECIFIC_CARD = 22,
		[Description("specific_card_2x")]
		SPECIFIC_CARD_2X = 23,
		[Description("ranked_season_reward_pack")]
		RANKED_SEASON_REWARD_PACK = 24,
		[Description("random_prerelease_card")]
		RANDOM_PRERELEASE_CARD = 31,
		[Description("battlegrounds_token")]
		BATTLEGROUNDS_TOKEN = 32
	}

	public static Reward ParseRewardValue(string value)
	{
		EnumUtils.TryGetEnum<Reward>(value, out var e);
		return e;
	}
}
