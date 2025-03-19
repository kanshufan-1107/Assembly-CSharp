using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class FormulaChangePoint
{
	public enum Function
	{
		[Description("none")]
		NONE,
		[Description("Constant")]
		CONSTANT,
		[Description("Linear")]
		LINEAR
	}

	public static Function ParseFunctionValue(string value)
	{
		EnumUtils.TryGetEnum<Function>(value, out var e);
		return e;
	}
}
