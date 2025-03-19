using Blizzard.T5.Core.Utils;

namespace Assets;

public static class TestVariation
{
	public enum TestGroup
	{
		CONTROL_GROUP,
		GROUP_A,
		GROUP_B,
		GROUP_C,
		GROUP_D,
		GROUP_E,
		INVALID
	}

	public static TestGroup ParseTestGroupValue(string value)
	{
		EnumUtils.TryGetEnum<TestGroup>(value, out var e);
		return e;
	}
}
