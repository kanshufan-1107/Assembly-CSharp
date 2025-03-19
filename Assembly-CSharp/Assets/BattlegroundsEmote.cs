using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class BattlegroundsEmote
{
	public enum Bordertype
	{
		[Description("none")]
		NONE,
		[Description("silver")]
		SILVER,
		[Description("purple")]
		PURPLE,
		[Description("gold")]
		GOLD
	}

	public static Bordertype ParseBordertypeValue(string value)
	{
		EnumUtils.TryGetEnum<Bordertype>(value, out var e);
		return e;
	}
}
