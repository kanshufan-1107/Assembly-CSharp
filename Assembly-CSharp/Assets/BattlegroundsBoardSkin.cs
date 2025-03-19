using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class BattlegroundsBoardSkin
{
	public enum Bordertype
	{
		[Description("default")]
		DEFAULT,
		[Description("Epic")]
		EPIC,
		[Description("Legendary")]
		LEGENDARY
	}

	public static Bordertype ParseBordertypeValue(string value)
	{
		EnumUtils.TryGetEnum<Bordertype>(value, out var e);
		return e;
	}
}
