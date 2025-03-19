using Blizzard.T5.Core.Utils;

namespace Assets;

public static class LuckyDrawBox
{
	public enum LuckyDrawEarnHammerCondition
	{
		BATTLEGROUNDS_WIN
	}

	public static LuckyDrawEarnHammerCondition ParseLuckyDrawEarnHammerConditionValue(string value)
	{
		EnumUtils.TryGetEnum<LuckyDrawEarnHammerCondition>(value, out var e);
		return e;
	}
}
