using Blizzard.T5.Core;
using Hearthstone.InGameMessage.UI;

namespace Hearthstone.InGameMessage;

public class MessageModalUtils
{
	private static readonly Map<MessageLayoutType, string> LAYOUT_ID_MAP = new Map<MessageLayoutType, string>
	{
		{
			MessageLayoutType.DEBUG,
			"DEBUG"
		},
		{
			MessageLayoutType.TEXT,
			"TEXT"
		},
		{
			MessageLayoutType.SHOP,
			"SHOP"
		},
		{
			MessageLayoutType.LAUNCH,
			"LAUNCH"
		},
		{
			MessageLayoutType.CHANGE,
			"CHANGE"
		},
		{
			MessageLayoutType.MERCENARY,
			"MERCENARY"
		},
		{
			MessageLayoutType.MERCENARY_BOUNTY,
			"MERCENARY_BOUNTY"
		},
		{
			MessageLayoutType.EMPTY,
			"EMPTY"
		}
	};

	private const string UNKNOWN_LAYOUT_TYPE_ID = "UNKNOWN";

	public static string GetLayoutTypeID(MessageLayoutType type)
	{
		if (LAYOUT_ID_MAP.TryGetValue(type, out var id))
		{
			return id;
		}
		return "UNKNOWN";
	}
}
