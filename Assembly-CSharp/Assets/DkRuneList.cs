using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class DkRuneList
{
	public enum DkruneTypes
	{
		[Description("NoneRune")]
		NONERUNE,
		[Description("BloodRune")]
		BLOODRUNE,
		[Description("FrostRune")]
		FROSTRUNE,
		[Description("UnholyRune")]
		UNHOLYRUNE
	}

	public static DkruneTypes ParseDkruneTypesValue(string value)
	{
		EnumUtils.TryGetEnum<DkruneTypes>(value, out var e);
		return e;
	}
}
