using Blizzard.T5.Core.Utils;

namespace Assets;

public static class CatchupPackEvent
{
	public enum CatchupEventType
	{
		CATCH_UP_WILD_WEST
	}

	public static CatchupEventType ParseCatchupEventTypeValue(string value)
	{
		EnumUtils.TryGetEnum<CatchupEventType>(value, out var e);
		return e;
	}
}
