using Blizzard.T5.Core.Utils;

namespace Assets;

public static class MercenariesRandomReward
{
	public enum RewardType
	{
		REWARD_TYPE_MERCENARY,
		REWARD_TYPE_CURRENCY
	}

	public enum MercenariesPremium
	{
		PREMIUM_NORMAL,
		PREMIUM_GOLDEN,
		PREMIUM_DIAMOND
	}

	public static RewardType ParseRewardTypeValue(string value)
	{
		EnumUtils.TryGetEnum<RewardType>(value, out var e);
		return e;
	}

	public static MercenariesPremium ParseMercenariesPremiumValue(string value)
	{
		EnumUtils.TryGetEnum<MercenariesPremium>(value, out var e);
		return e;
	}
}
