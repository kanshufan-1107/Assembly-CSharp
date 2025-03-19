using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class PlayerExperiment
{
	public enum TestFeature
	{
		INVALID,
		BOTS_ON_LADDER,
		[Description("26_6_FTUE_ENGAGEMENT")]
		_26_6_FTUE_ENGAGEMENT
	}

	public static TestFeature ParseTestFeatureValue(string value)
	{
		EnumUtils.TryGetEnum<TestFeature>(value, out var e);
		return e;
	}
}
