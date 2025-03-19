using Blizzard.T5.Core.Utils;

namespace Assets;

public static class MercTriggeredEvent
{
	public enum EventType
	{
		NONE,
		ADD_RANDOM_VISITORS,
		ADD_SPECIFIC_VISITOR,
		REPOPULATE_VILLAGE
	}

	public static EventType ParseEventTypeValue(string value)
	{
		EnumUtils.TryGetEnum<EventType>(value, out var e);
		return e;
	}
}
