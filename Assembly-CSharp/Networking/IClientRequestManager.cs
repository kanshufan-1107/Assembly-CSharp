using Blizzard.GameService.SDK.Client.Integration;

namespace Networking;

public interface IClientRequestManager
{
	bool SendClientRequest(int type, IProtoBuf body, ClientRequestManager.ClientRequestConfig clientRequestConfig, RequestPhase requestPhase);

	void NotifyResponseReceived(PegasusPacket packet);

	void NotifyStartupSequenceComplete();

	bool HasPendingDeliveryPackets();

	int PeekNetClientRequestType();

	ResponseWithRequest GetNextClientRequest();

	void DropNextClientRequest();

	void NotifyLoginSequenceCompleted();

	bool ShouldIgnoreError(BnetErrorInfo errorInfo);

	void Terminate();

	void SetDisconnectedFromBattleNet();

	void Update();

	bool HasErrors();
}
