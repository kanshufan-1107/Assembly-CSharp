using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class LeagueBgPublicRatingEquiv
{
	public enum FormatType
	{
		[Description("ft_unknown")]
		FT_UNKNOWN,
		[Description("ft_wild")]
		FT_WILD,
		[Description("ft_standard")]
		FT_STANDARD,
		[Description("ft_classic")]
		FT_CLASSIC,
		[Description("ft_twist")]
		FT_TWIST
	}

	public enum Region
	{
		[Description("region_unknown")]
		REGION_UNKNOWN = 0,
		[Description("region_us")]
		REGION_US = 1,
		[Description("region_eu")]
		REGION_EU = 2,
		[Description("region_kr")]
		REGION_KR = 3,
		[Description("region_tw")]
		REGION_TW = 4,
		[Description("region_cn")]
		REGION_CN = 5,
		[Description("region_sg")]
		REGION_SG = 6,
		[Description("region_ptr")]
		REGION_PTR = 98
	}

	public static FormatType ParseFormatTypeValue(string value)
	{
		EnumUtils.TryGetEnum<FormatType>(value, out var e);
		return e;
	}

	public static Region ParseRegionValue(string value)
	{
		EnumUtils.TryGetEnum<Region>(value, out var e);
		return e;
	}
}
