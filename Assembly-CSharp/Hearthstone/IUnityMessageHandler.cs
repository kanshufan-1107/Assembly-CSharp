using MiniJSON;

namespace Hearthstone;

public interface IUnityMessageHandler
{
	void HandleUnityMessage(string rawJson, JsonNode jsonMessage);
}
