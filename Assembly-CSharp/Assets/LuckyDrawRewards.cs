using Blizzard.T5.Core.Utils;

namespace Assets;

public static class LuckyDrawRewards
{
	public enum LuckyDrawStyle
	{
		COMMON,
		LEGENDARY
	}

	public static LuckyDrawStyle ParseLuckyDrawStyleValue(string value)
	{
		EnumUtils.TryGetEnum<LuckyDrawStyle>(value, out var e);
		return e;
	}
}
