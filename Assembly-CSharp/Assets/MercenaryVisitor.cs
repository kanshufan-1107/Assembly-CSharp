using Blizzard.T5.Core.Utils;

namespace Assets;

public static class MercenaryVisitor
{
	public enum VillageVisitorType
	{
		STANDARD,
		EVENT,
		SPECIAL,
		PROCEDURAL
	}

	public static VillageVisitorType ParseVillageVisitorTypeValue(string value)
	{
		EnumUtils.TryGetEnum<VillageVisitorType>(value, out var e);
		return e;
	}
}
