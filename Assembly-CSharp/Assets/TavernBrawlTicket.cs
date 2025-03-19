using Blizzard.T5.Core.Utils;

namespace Assets;

public static class TavernBrawlTicket
{
	public enum TicketBehaviorType
	{
		DEFAULT,
		ARENA_TAVERN_TICKET
	}

	public static TicketBehaviorType ParseTicketBehaviorTypeValue(string value)
	{
		EnumUtils.TryGetEnum<TicketBehaviorType>(value, out var e);
		return e;
	}
}
