using Hearthstone.InGameMessage.UI;

namespace Hearthstone.InGameMessage;

public static class ViewCountExtensions
{
	public static int GetViewCount(this ViewCountController controller, GameMessage message)
	{
		if (message == null)
		{
			return 0;
		}
		string id = GetIdForMessage(message);
		return controller.GetViewCount(id);
	}

	public static void IncreaseViewCount(this ViewCountController controller, GameMessage message)
	{
		if (message != null)
		{
			string id = GetIdForMessage(message);
			controller.IncreaseViewCount(id);
		}
	}

	public static int GetViewCount(this InGameMessageScheduler scheduler, GameMessage gameMessage)
	{
		if (gameMessage == null)
		{
			return 0;
		}
		string id = GetIdForMessage(gameMessage);
		return scheduler.GetViewCount(id);
	}

	public static int GetViewCount(this InGameMessageScheduler scheduler, MessageUIData messageUIData)
	{
		if (messageUIData == null)
		{
			return 0;
		}
		string id = GetIdForMessageData(messageUIData);
		return scheduler.GetViewCount(id);
	}

	private static string GetIdForMessage(GameMessage message)
	{
		if (!string.IsNullOrEmpty(message.ViewCountId))
		{
			return message.ViewCountId;
		}
		return message.UID;
	}

	private static string GetIdForMessageData(MessageUIData message)
	{
		if (!string.IsNullOrEmpty(message.ViewCountId))
		{
			return message.ViewCountId;
		}
		return message.UID;
	}
}
