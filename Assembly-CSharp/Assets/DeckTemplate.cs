using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class DeckTemplate
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

	public enum SourceType
	{
		[Description("None")]
		NONE,
		[Description("Loaner")]
		LOANER,
		[Description("Scenario")]
		SCENARIO
	}

	public static FormatType ParseFormatTypeValue(string value)
	{
		EnumUtils.TryGetEnum<FormatType>(value, out var e);
		return e;
	}

	public static SourceType ParseSourceTypeValue(string value)
	{
		EnumUtils.TryGetEnum<SourceType>(value, out var e);
		return e;
	}
}
