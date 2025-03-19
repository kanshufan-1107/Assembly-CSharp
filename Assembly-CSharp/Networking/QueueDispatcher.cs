using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Network;
using Hearthstone.Core;
using PegasusUtil;
using UnityEngine;

namespace Networking;

public class QueueDispatcher : IDispatcher
{
	private class IPAddressProvider
	{
		private Queue<IPAddress> m_candidateIPAddresses;

		private Action<string, string> m_onIPv6ConversionEvent;

		private bool m_supportsIPv6;

		public IPAddressProvider(string originalAddress, int tryCount, Action<string, string> OnIPv6ConversionCallback, bool supportsIPv6)
		{
			m_onIPv6ConversionEvent = OnIPv6ConversionCallback;
			m_supportsIPv6 = supportsIPv6;
			IPAddress address = GetIPv6IfAvailable(originalAddress);
			if (address == null)
			{
				IPHostEntry hostEntry = Dns.GetHostEntry(originalAddress);
				address = ((hostEntry != null) ? hostEntry.AddressList[0] : null);
			}
			if (address == null)
			{
				throw new Exception("Could not resolve address");
			}
			Init(address, tryCount);
		}

		private void Init(IPAddress originalAddress, int tryCount)
		{
			m_candidateIPAddresses = new Queue<IPAddress>();
			m_candidateIPAddresses.Enqueue(originalAddress);
			try
			{
				IPHostEntry info = Dns.GetHostEntry(originalAddress);
				Array.Sort(info.AddressList, delegate(IPAddress x, IPAddress y)
				{
					if (x.AddressFamily < y.AddressFamily)
					{
						return -1;
					}
					return (x.AddressFamily > y.AddressFamily) ? 1 : 0;
				});
				tryCount %= info.AddressList.Length;
				for (int i = tryCount; i < info.AddressList.Length; i++)
				{
					if (!info.AddressList[i].Equals(originalAddress))
					{
						m_candidateIPAddresses.Enqueue(info.AddressList[i]);
					}
				}
				for (int j = 0; j < tryCount; j++)
				{
					if (!info.AddressList[j].Equals(originalAddress))
					{
						m_candidateIPAddresses.Enqueue(info.AddressList[j]);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"TcpConnection - failed to get possible ip address: {ex.Message}");
			}
			foreach (IPAddress address in m_candidateIPAddresses)
			{
				Debug.Log($"TcpConnection - possible ip address: {address}");
			}
		}

		public Queue<IPAddress> GetAddresses()
		{
			return m_candidateIPAddresses;
		}

		private IPAddress GetIPv6IfAvailable(string Host)
		{
			if (IPAddress.TryParse(Host, out var originalAddress))
			{
				return originalAddress;
			}
			return null;
		}
	}

	private IRpcController m_gameRpcController;

	private readonly Queue<PegasusPacket> m_gamePackets;

	private readonly IClientRequestManager m_utilConnection;

	private readonly IPacketDecoderManager m_packetDecoderManager;

	private IDispatcherListener m_dispatcherListener;

	[StatePrinter.IncludeState]
	private GameStartState m_gameStartState;

	public IPEndPoint CurrentGameServerEndPoint
	{
		get
		{
			if (m_gameRpcController == null)
			{
				return null;
			}
			return m_gameRpcController.CurrentServerEndPoint;
		}
	}

	public GameStartState GameStartState
	{
		get
		{
			return m_gameStartState;
		}
		set
		{
			m_gameStartState = value;
		}
	}

	public Blizzard.T5.Network.ConnectionStatus GameConnectionState
	{
		get
		{
			if (m_gameRpcController == null)
			{
				return Blizzard.T5.Network.ConnectionStatus.Disconnected;
			}
			return m_gameRpcController.ConnectionState;
		}
	}

	public int PingsSinceLastPong { get; set; }

	public double TimeLastPingReceived { get; set; }

	public double TimeLastPingSent { get; set; }

	public bool ShouldIgnorePong { get; set; }

	public bool SpoofDisconnected { get; set; }

	public event Action<BattleNetErrors, SocketOperationResult> OnGameServerConnectEvent;

	public event Action<BattleNetErrors, SocketOperationResult> OnGameServerDisconnectEvent;

	public event Action<string, string> OnIPv6ConversionEvent;

	public QueueDispatcher(IRpcController gameRpcController, IClientRequestManager clientRequestManager, IPacketDecoderManager packetDecoder, IDispatcherListener dispatcherListener)
	{
		m_utilConnection = clientRequestManager;
		m_gameRpcController = gameRpcController;
		if (!Vars.Key("Mobile.IPv6Conversion").GetBool(def: true))
		{
			m_gameRpcController.AllowIPv4MappedIPv6 = false;
		}
		m_gameRpcController.OnRpcConnection = OnConnect;
		m_gameRpcController.OnRpcDisconnection = OnDisconnect;
		m_gameRpcController.OnPacketReceived = OnPacketReceived;
		m_gamePackets = new Queue<PegasusPacket>();
		GameStartState = GameStartState.Invalid;
		m_packetDecoderManager = packetDecoder;
		m_dispatcherListener = dispatcherListener;
	}

	private void OnConnect(SocketOperationResult result)
	{
		BattleNetErrors error = ConvertSocketErrorToBlizzardError(result.Error);
		Processor.MainThreadContext.Send(delegate
		{
			m_dispatcherListener?.OnGameServerConnect?.Invoke(error);
			this.OnGameServerConnectEvent?.Invoke(error, result);
		}, null);
	}

	private void OnDisconnect(SocketOperationResult result)
	{
		BattleNetErrors error = ConvertSocketErrorToBlizzardError(result.Error);
		Processor.MainThreadContext.Send(delegate
		{
			m_dispatcherListener?.OnGameServerDisconnect?.Invoke(error);
			this.OnGameServerDisconnectEvent?.Invoke(error, result);
		}, null);
	}

	protected void OnPacketReceived(IPacket packet)
	{
		if (SpoofDisconnected)
		{
			return;
		}
		if (packet == null)
		{
			Debug.LogError("OnPacketReceived game packet is null!");
		}
		else if (m_packetDecoderManager.CanDecodePacket(packet.packetType))
		{
			PegasusPacket decodedPacket = m_packetDecoderManager.DecodePacket(packet.packetType, 0, packet.body);
			switch (packet.packetType)
			{
			case 16:
				GameStartState = GameStartState.Invalid;
				break;
			case 116:
				TimeLastPingReceived = Blizzard.GameService.SDK.Client.Integration.TimeUtils.GetElapsedTimeSinceEpoch(null).TotalSeconds;
				if (!ShouldIgnorePong)
				{
					double pingTravelTime = TimeLastPingReceived - TimeLastPingSent;
					Processor.MainThreadContext.Send(delegate
					{
						m_dispatcherListener?.OnGameServerPing?.Invoke((float)pingTravelTime * 1000f);
					}, null);
					PingsSinceLastPong = 0;
				}
				break;
			}
			if (decodedPacket != null)
			{
				OnGamePacketReceived(decodedPacket);
			}
		}
		else
		{
			Debug.LogError("Could not find a packet decoder for a packet of type " + packet.packetType);
		}
	}

	public void OnGamePacketReceived(PegasusPacket decodedPacket)
	{
		Processor.MainThreadContext.Send(delegate
		{
			m_gamePackets.Enqueue(decodedPacket);
			m_dispatcherListener?.OnGamePacketReceived?.Invoke(decodedPacket);
		}, null);
	}

	public void ConnectToGameServer(string address, uint port)
	{
		int myport;
		try
		{
			myport = Convert.ToInt32(port);
		}
		catch (Exception ex)
		{
			Debug.LogError($"Could not convert port(uint):{port} to int. Error:{ex.Message}");
			return;
		}
		IPAddressProvider addressProvider = new IPAddressProvider(address, new System.Random().Next(0, 9), this.OnIPv6ConversionEvent, m_gameRpcController.SupportsIPv6);
		if (addressProvider.GetAddresses() == null)
		{
			OnConnect(new SocketOperationResult(SocketAsyncOperation.Connect, SocketError.HostNotFound, null, "Could not resolve host"));
		}
		else
		{
			m_gameRpcController.Connect(addressProvider.GetAddresses(), myport);
		}
	}

	public void DisconnectFromGameServer()
	{
		m_gameRpcController?.Disconnect();
	}

	public bool IsConnectedToGameServer()
	{
		if (m_gameRpcController != null)
		{
			return m_gameRpcController.ConnectionState == Blizzard.T5.Network.ConnectionStatus.Connected;
		}
		return false;
	}

	public void SendGamePacket(int packetId, IProtoBuf body)
	{
		if (!SpoofDisconnected)
		{
			PegasusPacket packet = new PegasusPacket(packetId, 0, body);
			byte[] data = EncodePacket(packetId, body);
			if (!IsConnectedToGameServer())
			{
				Network.Get()?.GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Information, $"Trying to send a packet while disconnected. ID {packetId} , {body.ToString()}");
			}
			m_gameRpcController.SendData(data);
			Processor.MainThreadContext.Send(delegate
			{
				m_dispatcherListener?.OnGamePacketSent?.Invoke(packet);
			}, null);
		}
	}

	public void DropGamePacket()
	{
		if (m_gamePackets.Any())
		{
			m_gamePackets.Dequeue();
		}
	}

	public bool HasGamePackets()
	{
		return m_gamePackets.Count > 0;
	}

	public PegasusPacket NextGamePacket()
	{
		if (m_gamePackets.Count <= 0)
		{
			return null;
		}
		return m_gamePackets.Peek();
	}

	public int NextGameType()
	{
		if (NextGamePacket() == null)
		{
			return -1;
		}
		return NextGamePacket().Type;
	}

	public void Close()
	{
		m_utilConnection?.Terminate();
		m_gameRpcController?.Disconnect();
	}

	public void ResetForNewConnection()
	{
		m_gamePackets.Clear();
		m_utilConnection.SetDisconnectedFromBattleNet();
		GameStartState = GameStartState.Invalid;
	}

	public PegasusPacket DecodePacket(PegasusPacket packet)
	{
		if (!m_packetDecoderManager.CanDecodePacket(packet.Type))
		{
			Log.Net.PrintWarning("Could not find a packet decoder for a packet of type " + packet.Type);
			return null;
		}
		return m_packetDecoderManager.DecodePacket(packet);
	}

	public void SetDebugGameConnectionState(bool canConnect, SocketError socketError)
	{
		m_gameRpcController.SetDebugGameConnectionState(canConnect, socketError);
	}

	public void SetDisconnectedFromBattleNet()
	{
		m_utilConnection.SetDisconnectedFromBattleNet();
	}

	public bool ShouldIgnoreError(BnetErrorInfo errorInfo)
	{
		return m_utilConnection.ShouldIgnoreError(errorInfo);
	}

	public bool HasUtilErrors()
	{
		return m_utilConnection.HasErrors();
	}

	public void DropUtilPacket()
	{
		m_utilConnection.DropNextClientRequest();
	}

	public bool HasUtilPackets()
	{
		return m_utilConnection.HasPendingDeliveryPackets();
	}

	public ResponseWithRequest NextUtilPacket()
	{
		return m_utilConnection.GetNextClientRequest();
	}

	public int NextUtilType()
	{
		return m_utilConnection.PeekNetClientRequestType();
	}

	public void NotifyUtilResponseReceived(PegasusPacket packet)
	{
		m_utilConnection.NotifyResponseReceived(packet);
		m_dispatcherListener?.OnUtilPacketReceived?.Invoke(packet);
	}

	public void OnLoginComplete()
	{
		m_utilConnection.NotifyLoginSequenceCompleted();
	}

	public void OnStartupPacketSequenceComplete()
	{
		m_utilConnection.NotifyStartupSequenceComplete();
	}

	public void ProcessUtilPackets()
	{
		m_utilConnection.Update();
	}

	public void SendUtilPacket(int type, UtilSystemId system, IProtoBuf body, RequestPhase requestPhase = RequestPhase.RUNNING)
	{
		if (SpoofDisconnected)
		{
			return;
		}
		if (!Network.ShouldBeConnectedToAurora())
		{
			FakeUtilHandler.FakeUtilOutbound(type, body);
			return;
		}
		ClientRequestManager.ClientRequestConfig config = null;
		if (system != 0)
		{
			config = new ClientRequestManager.ClientRequestConfig();
			config.ShouldRetryOnError = false;
			config.ShouldRetryOnUnhandled = true;
			config.RequestedSystem = system;
		}
		if (m_utilConnection.SendClientRequest(type, body, config, requestPhase))
		{
			if (201 == type)
			{
				GetAccountInfo getAccountInfo = (GetAccountInfo)body;
				Network.Get().AddPendingRequestTimeout(type, (int)getAccountInfo.Request_);
			}
			else
			{
				Network.Get().AddPendingRequestTimeout(type, 0);
			}
		}
	}

	private BattleNetErrors ConvertSocketErrorToBlizzardError(SocketError error)
	{
		return error switch
		{
			SocketError.Success => BattleNetErrors.ERROR_OK, 
			SocketError.SocketError => BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED, 
			SocketError.OperationAborted => BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED, 
			SocketError.IOPending => BattleNetErrors.ERROR_RPC_PEER_UNKNOWN, 
			SocketError.Interrupted => BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED, 
			SocketError.AccessDenied => BattleNetErrors.ERROR_RPC_CONNECTION_UNREACHABLE, 
			SocketError.Fault => BattleNetErrors.ERROR_RPC_INVALID_CONNECTION_STATE, 
			SocketError.InvalidArgument => BattleNetErrors.ERROR_RPC_INVALID_CONNECTION_STATE, 
			SocketError.TooManyOpenSockets => BattleNetErrors.ERROR_RPC_QUOTA_EXCEEDED, 
			SocketError.WouldBlock => BattleNetErrors.ERROR_RPC_PEER_UNKNOWN, 
			SocketError.InProgress => BattleNetErrors.ERROR_RPC_PEER_UNKNOWN, 
			SocketError.AlreadyInProgress => BattleNetErrors.ERROR_RPC_PEER_UNKNOWN, 
			SocketError.NotSocket => BattleNetErrors.ERROR_RPC_CONNECTION_UNREACHABLE, 
			SocketError.DestinationAddressRequired => BattleNetErrors.ERROR_RPC_INVALID_ADDRESS, 
			SocketError.HostNotFound => BattleNetErrors.ERROR_RPC_INVALID_ADDRESS, 
			SocketError.HostDown => BattleNetErrors.ERROR_RPC_CONNECTION_UNREACHABLE, 
			SocketError.MessageSize => BattleNetErrors.ERROR_RPC_PROTOCOL_ERROR, 
			SocketError.ProtocolType => BattleNetErrors.ERROR_RPC_PROTOCOL_ERROR, 
			SocketError.ProtocolOption => BattleNetErrors.ERROR_RPC_PROTOCOL_ERROR, 
			SocketError.ProtocolNotSupported => BattleNetErrors.ERROR_RPC_PROTOCOL_ERROR, 
			SocketError.SocketNotSupported => BattleNetErrors.ERROR_RPC_PROTOCOL_ERROR, 
			SocketError.OperationNotSupported => BattleNetErrors.ERROR_RPC_PROTOCOL_ERROR, 
			SocketError.ProtocolFamilyNotSupported => BattleNetErrors.ERROR_RPC_PROTOCOL_ERROR, 
			SocketError.AddressFamilyNotSupported => BattleNetErrors.ERROR_RPC_INVALID_ADDRESS, 
			SocketError.AddressAlreadyInUse => BattleNetErrors.ERROR_RPC_INVALID_ADDRESS, 
			SocketError.AddressNotAvailable => BattleNetErrors.ERROR_RPC_INVALID_ADDRESS, 
			SocketError.NetworkDown => BattleNetErrors.ERROR_RPC_CONNECTION_UNREACHABLE, 
			SocketError.NetworkUnreachable => BattleNetErrors.ERROR_RPC_CONNECTION_UNREACHABLE, 
			SocketError.NetworkReset => BattleNetErrors.ERROR_RPC_CONNECTION_UNREACHABLE, 
			_ => BattleNetErrors.ERROR_RPC_PEER_UNKNOWN, 
		};
	}

	public byte[] EncodePacket(int packetType, IProtoBuf Body)
	{
		int TYPE_BYTES = 4;
		int SIZE_BYTES = 4;
		if (Body != null)
		{
			int Size = (int)Body.GetSerializedSize();
			byte[] result = new byte[Size + TYPE_BYTES + SIZE_BYTES];
			Array.Copy(BitConverter.GetBytes(packetType), 0, result, 0, TYPE_BYTES);
			Array.Copy(BitConverter.GetBytes(Size), 0, result, TYPE_BYTES, SIZE_BYTES);
			Body.Serialize(new MemoryStream(result, TYPE_BYTES + SIZE_BYTES, Size));
			return result;
		}
		return null;
	}
}
