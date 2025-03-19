using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class BattlegroundsFinisher
{
	public enum CapsuleType
	{
		[Description("Default")]
		DEFAULT,
		[Description("Legendary")]
		LEGENDARY
	}

	public static CapsuleType ParseCapsuleTypeValue(string value)
	{
		EnumUtils.TryGetEnum<CapsuleType>(value, out var e);
		return e;
	}
}
