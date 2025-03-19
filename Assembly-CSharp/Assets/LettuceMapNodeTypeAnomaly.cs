using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class LettuceMapNodeTypeAnomaly
{
	public enum MercenariesBonusRewardType
	{
		[Description("none")]
		NONE,
		[Description("mystery_treasure_bonus")]
		MYSTERY_TREASURE_BONUS,
		[Description("mystery_treasure_cursed")]
		MYSTERY_TREASURE_CURSED
	}

	public static MercenariesBonusRewardType ParseMercenariesBonusRewardTypeValue(string value)
	{
		EnumUtils.TryGetEnum<MercenariesBonusRewardType>(value, out var e);
		return e;
	}
}
