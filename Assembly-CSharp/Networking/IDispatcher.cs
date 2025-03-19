using System;
using System.Net;
using System.Net.Sockets;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Network;

namespace Networking;

public interface IDispatcher
{
	GameStartState GameStartState { get; set; }

	Blizzard.T5.Network.ConnectionStatus GameConnectionState { get; }

	int PingsSinceLastPong { get; set; }

	double TimeLastPingReceived { get; set; }

	double TimeLastPingSent { get; set; }

	bool ShouldIgnorePong { set; }

	bool SpoofDisconnected { set; }

	IPEndPoint CurrentGameServerEndPoint { get; }

	event Action<BattleNetErrors, SocketOperationResult> OnGameServerConnectEvent;

	event Action<BattleNetErrors, SocketOperationResult> OnGameServerDisconnectEvent;

	event Action<string, string> OnIPv6ConversionEvent;

	void Close();

	PegasusPacket DecodePacket(PegasusPacket packet);

	void SetDisconnectedFromBattleNet();

	bool ShouldIgnoreError(BnetErrorInfo errorInfo);

	void ResetForNewConnection();

	void ConnectToGameServer(string address, uint port);

	void DisconnectFromGameServer();

	void DropGamePacket();

	bool HasGamePackets();

	bool IsConnectedToGameServer();

	PegasusPacket NextGamePacket();

	int NextGameType();

	void SendGamePacket(int packetId, IProtoBuf body);

	void DropUtilPacket();

	bool HasUtilErrors();

	bool HasUtilPackets();

	ResponseWithRequest NextUtilPacket();

	int NextUtilType();

	void NotifyUtilResponseReceived(PegasusPacket packet);

	void OnLoginComplete();

	void OnStartupPacketSequenceComplete();

	void ProcessUtilPackets();

	void SendUtilPacket(int type, UtilSystemId system, IProtoBuf body, RequestPhase requestPhase);

	void SetDebugGameConnectionState(bool canConnect, SocketError socketError);
}
