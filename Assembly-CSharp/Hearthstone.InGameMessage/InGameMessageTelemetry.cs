using Blizzard.Telemetry.WTCG.Client;
using HearthstoneTelemetry;

namespace Hearthstone.InGameMessage;

internal static class InGameMessageTelemetry
{
	private static ITelemetryClient TelemetryClient => TelemetryManager.Client();

	internal static void SendMessageAction(GameMessage message, int viewCount, InGameMessageAction.ActionType action)
	{
		TelemetryClient.SendInGameMessageAction(message.ContentType, message.Title, action, viewCount, message.UID);
	}

	internal static void SendMessageValidationError(string contentType, string title, string uid)
	{
		TelemetryClient.SendInGameMessageDataError(contentType, title, InGameMessageDataError.ErrorType.Validation_Error, uid);
	}

	internal static void SendMessageTranslationError(string contentType, string title, string uid)
	{
		TelemetryClient.SendInGameMessageDataError(contentType, title, InGameMessageDataError.ErrorType.Translation_Error, uid);
	}

	internal static void SendDelayedMessageData(string contentType, string eventId)
	{
		TelemetryClient.SendInGameMessageDelayedMessages(contentType, eventId);
	}

	internal static void SendRegisterContentType(string contentType)
	{
		TelemetryClient.SendInGameMessageSystemFlow(contentType, null, 0, InGameMessageSystemFlow.TelemetryMessageType.Register_Content_Type, null);
	}

	internal static void SendMessageUpdate(string contentType)
	{
		TelemetryClient.SendInGameMessageSystemFlow(contentType, null, 0, InGameMessageSystemFlow.TelemetryMessageType.Update_Messages, null);
	}

	internal static void SendDisplayQueueCountForEvent(string eventId, int messageCount)
	{
		TelemetryClient.SendInGameMessageSystemFlow(null, eventId, messageCount, InGameMessageSystemFlow.TelemetryMessageType.Event_Display_Queue, null);
	}

	internal static void SendMessageDisplayed(string contentType, string uid, string title)
	{
		TelemetryClient.SendInGameMessageDisplayed(contentType, uid, title);
	}
}
