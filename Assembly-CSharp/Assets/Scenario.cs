using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class Scenario
{
	public enum BoardLayout
	{
		[Description("standard")]
		STANDARD,
		[Description("lettuce")]
		LETTUCE
	}

	public enum RuleType
	{
		[Description("none")]
		NONE,
		[Description("choose_hero")]
		CHOOSE_HERO,
		[Description("choose_deck")]
		CHOOSE_DECK
	}

	public static BoardLayout ParseBoardLayoutValue(string value)
	{
		EnumUtils.TryGetEnum<BoardLayout>(value, out var e);
		return e;
	}

	public static RuleType ParseRuleTypeValue(string value)
	{
		EnumUtils.TryGetEnum<RuleType>(value, out var e);
		return e;
	}
}
