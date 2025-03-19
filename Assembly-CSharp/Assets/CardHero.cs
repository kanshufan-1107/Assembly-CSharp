using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class CardHero
{
	public enum HeroType
	{
		[Description("Unknown")]
		UNKNOWN,
		[Description("Vanilla")]
		VANILLA,
		[Description("Honored")]
		HONORED,
		[Description("Battlegrounds Hero")]
		BATTLEGROUNDS_HERO,
		[Description("Battlegrounds Guide")]
		BATTLEGROUNDS_GUIDE
	}

	public enum PortraitCurrency
	{
		[Description("Unknown")]
		UNKNOWN,
		[Description("Gold")]
		GOLD,
		[Description("Virtual Currency")]
		VIRTUAL_CURRENCY,
		[Description("Real Money")]
		REAL_MONEY
	}

	public static HeroType ParseHeroTypeValue(string value)
	{
		EnumUtils.TryGetEnum<HeroType>(value, out var e);
		return e;
	}

	public static PortraitCurrency ParsePortraitCurrencyValue(string value)
	{
		EnumUtils.TryGetEnum<PortraitCurrency>(value, out var e);
		return e;
	}
}
