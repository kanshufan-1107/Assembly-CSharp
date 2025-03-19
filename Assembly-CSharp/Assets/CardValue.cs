using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class CardValue
{
	public enum SellState
	{
		[Description("normal")]
		NORMAL,
		[Description("recently_nerfed_use_buy_value")]
		RECENTLY_NERFED_USE_BUY_VALUE,
		[Description("recently_nerfed_use_custom_value")]
		RECENTLY_NERFED_USE_CUSTOM_VALUE,
		[Description("permanent_override_use_custom_value")]
		PERMANENT_OVERRIDE_USE_CUSTOM_VALUE
	}

	public enum OverrideRegion
	{
		[Description("none")]
		NONE = 0,
		[Description("us")]
		US = 1,
		[Description("eu")]
		EU = 2,
		[Description("kr")]
		KR = 3,
		[Description("tw")]
		TW = 4,
		[Description("cn")]
		CN = 5,
		[Description("sg")]
		SG = 6,
		[Description("ptr")]
		PTR = 98
	}

	public static SellState ParseSellStateValue(string value)
	{
		EnumUtils.TryGetEnum<SellState>(value, out var e);
		return e;
	}

	public static OverrideRegion ParseOverrideRegionValue(string value)
	{
		EnumUtils.TryGetEnum<OverrideRegion>(value, out var e);
		return e;
	}
}
