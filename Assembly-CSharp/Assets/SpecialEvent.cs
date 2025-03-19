using Blizzard.T5.Core.Utils;

namespace Assets;

public static class SpecialEvent
{
	public enum SpecialEventType
	{
		INVALID,
		HALLOWS_END,
		DK_LAUNCH,
		WINTER_VEIL,
		LUNAR_NEW_YEAR,
		FESTIVAL_OF_LEGENDS,
		LEGENDS_TAKE_THE_STAGE,
		NOBLEGARDEN,
		FIRE_FESTIVAL,
		FROST_FESTIVAL,
		PIRATE_DAY,
		NEW_YEAR,
		ANNOUNCE_EVENT,
		LAUNCH_EVENT,
		BG_EVENT,
		ANNIVERSARY,
		BG_DUOS_LAUNCH,
		SPECIAL_EVENT_CN,
		LAUNCH_EVENT_ALT,
		STARCRAFT
	}

	public static SpecialEventType ParseSpecialEventTypeValue(string value)
	{
		EnumUtils.TryGetEnum<SpecialEventType>(value, out var e);
		return e;
	}
}
