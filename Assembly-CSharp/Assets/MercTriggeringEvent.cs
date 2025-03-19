using Blizzard.T5.Core.Utils;

namespace Assets;

public static class MercTriggeringEvent
{
	public enum EventType
	{
		NONE,
		CLAIM_VISITOR_TASK,
		COMPLETE_VISITOR_TASK_CHAIN,
		UPGRADE_BUILDING
	}

	public static EventType ParseEventTypeValue(string value)
	{
		EnumUtils.TryGetEnum<EventType>(value, out var e);
		return e;
	}
}
