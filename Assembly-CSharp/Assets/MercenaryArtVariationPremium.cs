using Blizzard.T5.Core.Utils;

namespace Assets;

public static class MercenaryArtVariationPremium
{
	public enum MercenariesPremium
	{
		PREMIUM_NORMAL,
		PREMIUM_GOLDEN,
		PREMIUM_DIAMOND
	}

	public static MercenariesPremium ParseMercenariesPremiumValue(string value)
	{
		EnumUtils.TryGetEnum<MercenariesPremium>(value, out var e);
		return e;
	}
}
