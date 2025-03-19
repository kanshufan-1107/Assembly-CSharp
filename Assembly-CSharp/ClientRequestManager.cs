using System;
using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Services;
using Networking;
using PegasusUtil;
using UnityEngine;

public class ClientRequestManager : IClientRequestManager
{
	public class ClientRequestConfig
	{
		public bool ShouldRetryOnError { get; set; }

		public bool ShouldRetryOnUnhandled { get; set; }

		public UtilSystemId RequestedSystem { get; set; }
	}

	private class ClientRequestType
	{
		public int Type;

		public byte[] Body;

		public uint Context;

		public RequestPhase Phase;

		public uint SendCount;

		public uint RequestNotHandledCount;

		public float SendTime;

		public uint RequestId;

		public bool IsSubscribeRequest;

		public SystemChannel System;

		public bool ShouldRetryOnError;

		public bool ShouldRetryOnUnhandled;

		public ulong RouteDispatchedTo;

		public ClientRequestType(SystemChannel system)
		{
			System = system;
		}

		public byte[] GetUtilPacketBytes()
		{
			RpcHeader rpcHeader = new RpcHeader();
			rpcHeader.Type = (ulong)Type;
			if (SendCount != 0)
			{
				rpcHeader.RetryCount = SendCount;
			}
			if (RequestNotHandledCount != 0)
			{
				rpcHeader.RequestNotHandledCount = RequestNotHandledCount;
			}
			RpcMessage rpcMessage = new RpcMessage();
			rpcMessage.RpcHeader = rpcHeader;
			if (Body != null && Body.Length != 0)
			{
				rpcMessage.MessageBody = Body;
			}
			return ProtobufUtil.ToByteArray(rpcMessage);
		}
	}

	private class SubscriptionStatusType
	{
		public enum State
		{
			PENDING_SEND,
			PENDING_RESPONSE,
			SUBSCRIBED
		}

		public State CurrentState;

		public DateTime LastSend = DateTime.MinValue;

		public float SubscribedTime;

		public uint ContexId;
	}

	private class PendingMapType
	{
		public Queue<ClientRequestType> PendingSend = new Queue<ClientRequestType>();
	}

	private class PhaseMapType
	{
		public PendingMapType StartUp = new PendingMapType();

		public PendingMapType Running = new PendingMapType();
	}

	private class SystemChannel
	{
		public PhaseMapType Phases = new PhaseMapType();

		public SubscriptionStatusType SubscriptionStatus = new SubscriptionStatusType();

		public ulong Route;

		public RequestPhase CurrentPhase;

		public ulong KeepAliveSecs;

		public ulong MaxResubscribeAttempts;

		public ulong PendingResponseTimeout;

		public ulong PendingSubscribeTimeout = 15uL;

		public uint SubscribeAttempt;

		public bool WasEverInRunningPhase;

		public UtilSystemId SystemId;

		public uint m_subscribePacketsReceived;

		public bool HasAssignedRoute => Route != 0;
	}

	private class SystemMap
	{
		public Map<UtilSystemId, SystemChannel> Systems = new Map<UtilSystemId, SystemChannel>();
	}

	private class InternalState
	{
		public Queue<ResponseWithRequest> m_responsesPendingDelivery = new Queue<ResponseWithRequest>();

		public SystemMap m_systems = new SystemMap();

		public uint m_subscribePacketsSent;

		public bool m_loginCompleteNotificationReceived;

		public Map<uint, ClientRequestType> m_activePendingResponseMap = new Map<uint, ClientRequestType>();

		public HashSet<uint> m_ignorePendingResponseMap = new HashSet<uint>();

		public bool m_runningPhaseEnabled;

		public bool m_receivedErrorSignal;
	}

	private static Map<int, string> s_typeToStringMap = new Map<int, string>();

	private readonly ClientRequestConfig m_defaultConfig = new ClientRequestConfig
	{
		ShouldRetryOnError = true,
		ShouldRetryOnUnhandled = true,
		RequestedSystem = UtilSystemId.CLIENT
	};

	public uint m_nextContexId;

	public uint m_nextRequestId;

	private InternalState m_state = new InternalState();

	private Subscribe m_subscribePacket = new Subscribe();

	private bool m_hasSubscribedToUtilClient;

	private DispatchListener m_dispatcerListner;

	public bool SendClientRequest(int type, IProtoBuf body, ClientRequestConfig clientRequestConfig, RequestPhase requestPhase = RequestPhase.RUNNING)
	{
		return SendClientRequestImpl(type, body, clientRequestConfig, requestPhase);
	}

	public void NotifyResponseReceived(PegasusPacket packet)
	{
		NotifyResponseReceivedImpl(packet);
	}

	public void NotifyStartupSequenceComplete()
	{
		NotifyStartupSequenceCompleteImpl();
	}

	public bool HasPendingDeliveryPackets()
	{
		return HasPendingDeliveryPacketsImpl();
	}

	public int PeekNetClientRequestType()
	{
		return PeekNetClientRequestTypeImpl();
	}

	public ResponseWithRequest GetNextClientRequest()
	{
		return GetNextClientRequestImpl();
	}

	public void DropNextClientRequest()
	{
		DropNextClientRequestImpl();
	}

	public void NotifyLoginSequenceCompleted()
	{
		NotifyLoginSequenceCompletedImpl();
	}

	public bool ShouldIgnoreError(BnetErrorInfo errorInfo)
	{
		return ShouldIgnoreErrorImpl(errorInfo);
	}

	public void Terminate()
	{
		TerminateImpl();
		Update();
	}

	public void SetDisconnectedFromBattleNet()
	{
		m_state = new InternalState();
	}

	public void Update()
	{
		UpdateImpl();
	}

	public bool HasErrors()
	{
		return HasErrorsImpl();
	}

	private bool ShouldIgnoreErrorImpl(BnetErrorInfo errorInfo)
	{
		uint contextId = (uint)errorInfo.GetContext();
		if (contextId == 0)
		{
			return false;
		}
		ClientRequestType clientRequest = GetClientRequest(contextId, "should_ignore_error", removeFromPendingResponse: true);
		if (clientRequest == null)
		{
			if (GetDroppedRequest(contextId, "should_ignore"))
			{
				return true;
			}
			if (GetPendingSendRequest(contextId, "should_ignore") != null)
			{
				return true;
			}
			return false;
		}
		BattleNetErrors errorCode = errorInfo.GetError();
		if (clientRequest.IsSubscribeRequest)
		{
			if (clientRequest.System.SubscribeAttempt >= clientRequest.System.MaxResubscribeAttempts)
			{
				return !clientRequest.ShouldRetryOnError;
			}
			return true;
		}
		switch (errorCode)
		{
		case BattleNetErrors.ERROR_GAME_UTILITY_SERVER_NO_SERVER:
			clientRequest.RequestNotHandledCount++;
			if (!clientRequest.ShouldRetryOnUnhandled)
			{
				return true;
			}
			return RescheduleSubscriptionAndRetryRequest(clientRequest, "received_error_util_server_no_server");
		case BattleNetErrors.ERROR_INTERNAL:
		case BattleNetErrors.ERROR_RPC_REQUEST_TIMED_OUT:
			if (!clientRequest.ShouldRetryOnError)
			{
				return true;
			}
			if (clientRequest.System.PendingResponseTimeout == 0L)
			{
				return false;
			}
			return RescheduleSubscriptionAndRetryRequest(clientRequest, "received_error_util_lost");
		default:
			return false;
		}
	}

	private bool RescheduleSubscriptionAndRetryRequest(ClientRequestType clientRequest, string errorReason)
	{
		if (clientRequest.RouteDispatchedTo == clientRequest.System.Route)
		{
			ScheduleResubscribeWithNewRoute(clientRequest.System);
		}
		AddRequestToPendingSendQueue(clientRequest, "resubscribe_and_retry_request");
		return true;
	}

	private void ProcessServiceUnavailable(ClientRequestResponse response, ClientRequestType clientRequest)
	{
		clientRequest.RequestNotHandledCount++;
		RescheduleSubscriptionAndRetryRequest(clientRequest, "received_CRRF_SERVICE_UNAVAILABLE");
	}

	private void ProcessClientRequestResponse(PegasusPacket packet, ClientRequestType clientRequest)
	{
		if (packet.Body is ClientRequestResponse)
		{
			ClientRequestResponse clientRequestResponse = (ClientRequestResponse)packet.Body;
			ClientRequestResponse.ClientRequestResponseFlags serviceUnavailableFlag = ClientRequestResponse.ClientRequestResponseFlags.CRRF_SERVICE_UNAVAILABLE;
			if ((clientRequestResponse.ResponseFlags & serviceUnavailableFlag) != 0)
			{
				ProcessServiceUnavailable(clientRequestResponse, clientRequest);
			}
			ClientRequestResponse.ClientRequestResponseFlags unknownErrorFlag = ClientRequestResponse.ClientRequestResponseFlags.CRRF_SERVICE_UNKNOWN_ERROR;
			if ((clientRequestResponse.ResponseFlags & unknownErrorFlag) != 0)
			{
				m_state.m_receivedErrorSignal = true;
			}
		}
	}

	private bool HasPendingDeliveryPacketsImpl()
	{
		return m_state.m_responsesPendingDelivery.Count > 0;
	}

	private int PeekNetClientRequestTypeImpl()
	{
		if (m_state.m_responsesPendingDelivery.Count == 0)
		{
			return 0;
		}
		return m_state.m_responsesPendingDelivery.Peek().Response.Type;
	}

	private ResponseWithRequest GetNextClientRequestImpl()
	{
		if (m_state.m_responsesPendingDelivery.Count == 0)
		{
			return null;
		}
		return m_state.m_responsesPendingDelivery.Peek();
	}

	private void DropNextClientRequestImpl()
	{
		if (m_state.m_responsesPendingDelivery.Count != 0)
		{
			m_state.m_responsesPendingDelivery.Dequeue();
		}
	}

	private bool HasErrorsImpl()
	{
		return m_state.m_receivedErrorSignal;
	}

	public ClientRequestManager(DispatchListener dispatcerListner)
	{
		m_dispatcerListner = dispatcerListner;
	}

	private void UpdateImpl()
	{
		if (!m_state.m_loginCompleteNotificationReceived || !m_hasSubscribedToUtilClient)
		{
			return;
		}
		SystemChannel migrationSystemChannel = m_state.m_systems.Systems[UtilSystemId.CLIENT];
		if (!UpdateStateSubscribeImpl(migrationSystemChannel))
		{
			return;
		}
		ProcessClientRequests(migrationSystemChannel);
		foreach (KeyValuePair<UtilSystemId, SystemChannel> system in m_state.m_systems.Systems)
		{
			if (system.Key != 0 && UpdateStateSubscribeImpl(system.Value))
			{
				ProcessClientRequests(system.Value);
			}
		}
	}

	private bool SendClientRequestImpl(int type, IProtoBuf body, ClientRequestConfig clientRequestConfig, RequestPhase requestPhase)
	{
		if (type == 0)
		{
			return false;
		}
		if (requestPhase < RequestPhase.STARTUP || requestPhase > RequestPhase.RUNNING)
		{
			return false;
		}
		ClientRequestConfig config = ((clientRequestConfig == null) ? m_defaultConfig : clientRequestConfig);
		UtilSystemId systemId = config.RequestedSystem;
		SystemChannel system = GetOrCreateSystem(systemId);
		if (requestPhase < system.CurrentPhase)
		{
			return false;
		}
		if (system.WasEverInRunningPhase && requestPhase < RequestPhase.RUNNING)
		{
			return false;
		}
		if (body == null)
		{
			return false;
		}
		ClientRequestType request = new ClientRequestType(system);
		request.Type = type;
		request.ShouldRetryOnError = config.ShouldRetryOnError;
		request.ShouldRetryOnUnhandled = config.ShouldRetryOnUnhandled;
		request.Body = ProtobufUtil.ToByteArray(body);
		request.Phase = requestPhase;
		request.SendCount = 0u;
		request.RequestNotHandledCount = 0u;
		request.RequestId = GetNextRequestId();
		try
		{
			PegasusPacket pegasusPacket = new PegasusPacket(request.Type, 0, body);
			m_dispatcerListner?.OnUtilPacketSent?.Invoke(pegasusPacket);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		if (request.Phase == RequestPhase.STARTUP)
		{
			system.Phases.StartUp.PendingSend.Enqueue(request);
		}
		else
		{
			system.Phases.Running.PendingSend.Enqueue(request);
		}
		return true;
	}

	private SystemChannel GetOrCreateSystem(UtilSystemId systemId)
	{
		SystemChannel output = null;
		if (m_state.m_systems.Systems.TryGetValue(systemId, out output))
		{
			return output;
		}
		output = new SystemChannel();
		output.SystemId = systemId;
		m_state.m_systems.Systems[systemId] = output;
		if (systemId == UtilSystemId.CLIENT)
		{
			m_hasSubscribedToUtilClient = true;
		}
		return output;
	}

	private uint GenerateContextId()
	{
		return ++m_nextContexId;
	}

	private void NotifyResponseReceivedImpl(PegasusPacket packet)
	{
		uint contextId = (uint)packet.Context;
		ClientRequestType clientRequest = GetClientRequest(contextId, "received_response", removeFromPendingResponse: true);
		if (clientRequest == null)
		{
			if (packet.Context == 0 || !GetDroppedRequest(contextId, "received_response"))
			{
				m_state.m_responsesPendingDelivery.Enqueue(new ResponseWithRequest(packet));
			}
			return;
		}
		switch (packet.Type)
		{
		case 315:
			ProcessSubscribeResponse(packet, clientRequest);
			break;
		case 328:
			ProcessClientRequestResponse(packet, clientRequest);
			break;
		default:
			ProcessResponse(packet, clientRequest);
			break;
		}
	}

	private void NotifyStartupSequenceCompleteImpl()
	{
		m_state.m_runningPhaseEnabled = true;
	}

	private void NotifyLoginSequenceCompletedImpl()
	{
		m_state.m_loginCompleteNotificationReceived = true;
	}

	private uint SendToUtil(ClientRequestType request)
	{
		uint contextId = GenerateContextId();
		ulong SendToRoute = request.System.Route;
		byte[] requestBytes = request.GetUtilPacketBytes();
		BattleNet.SendUtilPacket(request.System.SystemId, requestBytes, (int)contextId, SendToRoute);
		request.Context = contextId;
		request.SendTime = Time.realtimeSinceStartup;
		request.SendCount++;
		request.RouteDispatchedTo = SendToRoute;
		AddRequestToPendingResponse(request, "send_to_util");
		if (!request.IsSubscribeRequest)
		{
			request.Phase.ToString();
		}
		return contextId;
	}

	private uint GetNextRequestId()
	{
		return ++m_nextRequestId;
	}

	private void SendSubscriptionRequest(SystemChannel system)
	{
		UtilSystemId systemId = system.SystemId;
		if (!system.HasAssignedRoute)
		{
			m_subscribePacket.FirstSubscribeForRoute = true;
		}
		else
		{
			m_subscribePacket.FirstSubscribeForRoute = false;
		}
		m_subscribePacket.FirstSubscribe = system.SubscriptionStatus.LastSend == DateTime.MinValue;
		m_subscribePacket.UtilSystemId = (int)systemId;
		ClientRequestType newRequest = new ClientRequestType(system);
		newRequest.Type = 314;
		newRequest.Body = ProtobufUtil.ToByteArray(m_subscribePacket);
		newRequest.RequestId = GetNextRequestId();
		newRequest.IsSubscribeRequest = true;
		system.SubscriptionStatus.CurrentState = SubscriptionStatusType.State.PENDING_RESPONSE;
		system.SubscriptionStatus.LastSend = DateTime.Now;
		system.SubscriptionStatus.ContexId = SendToUtil(newRequest);
		system.SubscribeAttempt++;
		m_state.m_subscribePacketsSent++;
	}

	private void ScheduleResubscribeWithNewRoute(SystemChannel system)
	{
		system.Route = 0uL;
		system.SubscriptionStatus.CurrentState = SubscriptionStatusType.State.PENDING_SEND;
	}

	private void TerminateImpl()
	{
		Unsubscribe unsubscribePacket = new Unsubscribe();
		foreach (KeyValuePair<UtilSystemId, SystemChannel> system2 in m_state.m_systems.Systems)
		{
			SystemChannel system = system2.Value;
			if (system.SubscriptionStatus.CurrentState == SubscriptionStatusType.State.SUBSCRIBED && system.HasAssignedRoute && ServiceManager.TryGet<Network>(out var network))
			{
				network.SendUnsubcribeRequest(unsubscribePacket, system.SystemId);
			}
		}
	}

	private bool UpdateStateSubscribeImpl(SystemChannel system)
	{
		return system.SubscriptionStatus.CurrentState switch
		{
			SubscriptionStatusType.State.PENDING_SEND => ProcessSubscribeStatePendingSend(system), 
			SubscriptionStatusType.State.PENDING_RESPONSE => ProcessSubscribeStatePendingResponse(system), 
			SubscriptionStatusType.State.SUBSCRIBED => ProcessSubscribeStateSubscribed(system), 
			_ => system.SubscriptionStatus.CurrentState == SubscriptionStatusType.State.SUBSCRIBED, 
		};
	}

	private bool ProcessSubscribeStatePendingSend(SystemChannel system)
	{
		if ((DateTime.Now - system.SubscriptionStatus.LastSend).TotalSeconds > (double)system.PendingSubscribeTimeout)
		{
			SendSubscriptionRequest(system);
		}
		return system.HasAssignedRoute;
	}

	private bool ProcessSubscribeStatePendingResponse(SystemChannel system)
	{
		if ((DateTime.Now - system.SubscriptionStatus.LastSend).TotalSeconds > (double)system.PendingSubscribeTimeout)
		{
			ScheduleResubscribeWithNewRoute(system);
		}
		return system.HasAssignedRoute;
	}

	private int CountPendingResponsesForSystemId(SystemChannel system)
	{
		int count = 0;
		foreach (KeyValuePair<uint, ClientRequestType> item in m_state.m_activePendingResponseMap)
		{
			if (item.Value.System.SystemId == system.SystemId)
			{
				count++;
			}
		}
		return count;
	}

	private bool ProcessSubscribeStateSubscribed(SystemChannel system)
	{
		if ((ulong)(Time.realtimeSinceStartup - system.SubscriptionStatus.SubscribedTime) < system.KeepAliveSecs)
		{
			return true;
		}
		if (CountPendingResponsesForSystemId(system) > 0)
		{
			return true;
		}
		if (system.KeepAliveSecs != 0)
		{
			system.SubscriptionStatus.CurrentState = SubscriptionStatusType.State.PENDING_SEND;
		}
		return true;
	}

	private void ProcessSubscribeResponse(PegasusPacket packet, ClientRequestType request)
	{
		if (packet.Body is SubscribeResponse)
		{
			SystemChannel system = request.System;
			_ = system.SystemId;
			SubscribeResponse response = (SubscribeResponse)packet.Body;
			if (response.Result == SubscribeResponse.ResponseResult.FAILED_UNAVAILABLE)
			{
				ScheduleResubscribeWithNewRoute(system);
				return;
			}
			system.SubscriptionStatus.CurrentState = SubscriptionStatusType.State.SUBSCRIBED;
			system.SubscriptionStatus.SubscribedTime = Time.realtimeSinceStartup;
			system.Route = response.Route;
			system.CurrentPhase = RequestPhase.STARTUP;
			system.SubscribeAttempt = 0u;
			system.KeepAliveSecs = response.KeepAliveSecs;
			system.MaxResubscribeAttempts = response.MaxResubscribeAttempts;
			system.PendingResponseTimeout = response.PendingResponseTimeout;
			system.PendingSubscribeTimeout = response.PendingSubscribeTimeout;
			PegasusPacket requestPacket = new PegasusPacket(request.Type, packet.Context, request.Body);
			m_state.m_responsesPendingDelivery.Enqueue(new ResponseWithRequest(packet, requestPacket));
			system.m_subscribePacketsReceived++;
		}
	}

	private void ProcessClientRequests(SystemChannel system)
	{
		PendingMapType pendingMap = ((system.CurrentPhase == RequestPhase.STARTUP) ? system.Phases.StartUp : system.Phases.Running);
		foreach (KeyValuePair<uint, ClientRequestType> request in m_state.m_activePendingResponseMap)
		{
			ClientRequestType clientRequest = request.Value;
			if (!clientRequest.IsSubscribeRequest && clientRequest.System != null && clientRequest.System.SystemId == system.SystemId && system.PendingResponseTimeout != 0L && Time.realtimeSinceStartup - clientRequest.SendTime >= (float)system.PendingResponseTimeout)
			{
				m_state.m_activePendingResponseMap.Remove(request.Key);
				ScheduleResubscribeWithNewRoute(system);
				return;
			}
		}
		if (system.HasAssignedRoute)
		{
			bool shouldReturn = pendingMap.PendingSend.Count > 0;
			while (pendingMap.PendingSend.Count > 0)
			{
				ClientRequestType request2 = pendingMap.PendingSend.Dequeue();
				SendToUtil(request2);
			}
			if (!shouldReturn && system.CurrentPhase == RequestPhase.STARTUP && m_state.m_runningPhaseEnabled)
			{
				system.CurrentPhase = RequestPhase.RUNNING;
			}
		}
	}

	private void ProcessResponse(PegasusPacket packet, ClientRequestType clientRequest)
	{
		if (packet.Type != 254)
		{
			PegasusPacket requestPacket = new PegasusPacket(clientRequest.Type, packet.Context, clientRequest.Body);
			m_state.m_responsesPendingDelivery.Enqueue(new ResponseWithRequest(packet, requestPacket));
		}
	}

	private ClientRequestType GetClientRequest(uint contextId, string reason, bool removeFromPendingResponse)
	{
		if (contextId == 0)
		{
			return null;
		}
		if (!m_state.m_activePendingResponseMap.TryGetValue(contextId, out var clientRequest))
		{
			if (GetDroppedRequest(contextId, "get_client_request", removeIfFound: false))
			{
				GetPendingSendRequest(contextId, "get_client_request", removeIfFound: false);
			}
			return null;
		}
		if (removeFromPendingResponse)
		{
			m_state.m_activePendingResponseMap.Remove(contextId);
		}
		return clientRequest;
	}

	private void AddRequestToPendingSendQueue(ClientRequestType clientRequest, string reason)
	{
		if (clientRequest.Phase == RequestPhase.STARTUP)
		{
			clientRequest.System.Phases.StartUp.PendingSend.Enqueue(clientRequest);
			_ = clientRequest.System.Phases.StartUp.PendingSend.Count;
		}
		else
		{
			clientRequest.System.Phases.Running.PendingSend.Enqueue(clientRequest);
			_ = clientRequest.System.Phases.Running.PendingSend.Count;
		}
	}

	private void AddRequestToPendingResponse(ClientRequestType clientRequest, string reason)
	{
		if (!m_state.m_activePendingResponseMap.ContainsKey(clientRequest.Context))
		{
			m_state.m_activePendingResponseMap.Add(clientRequest.Context, clientRequest);
		}
	}

	private bool GetDroppedRequest(uint contextId, string reason, bool removeIfFound = true)
	{
		if (m_state.m_ignorePendingResponseMap.Contains(contextId) && removeIfFound)
		{
			m_state.m_ignorePendingResponseMap.Remove(contextId);
			return true;
		}
		return false;
	}

	private ClientRequestType GetPendingSendRequestForPhase(uint contextId, bool removeIfFound, PendingMapType pendingMap)
	{
		ClientRequestType foundRequest = null;
		Queue<ClientRequestType> newQueue = new Queue<ClientRequestType>();
		foreach (ClientRequestType request in pendingMap.PendingSend)
		{
			if (foundRequest == null && request.Context == contextId)
			{
				foundRequest = request;
				if (!removeIfFound)
				{
					newQueue.Enqueue(request);
				}
			}
			else
			{
				newQueue.Enqueue(request);
			}
		}
		pendingMap.PendingSend = newQueue;
		return foundRequest;
	}

	private ClientRequestType GetPendingSendRequest(uint contextId, string reason, bool removeIfFound = true)
	{
		ClientRequestType foundRequest = null;
		foreach (KeyValuePair<UtilSystemId, SystemChannel> system2 in m_state.m_systems.Systems)
		{
			SystemChannel system = system2.Value;
			foundRequest = GetPendingSendRequestForPhase(contextId, removeIfFound, system.Phases.Running);
			if (foundRequest == null)
			{
				foundRequest = GetPendingSendRequestForPhase(contextId, removeIfFound, system.Phases.StartUp);
				continue;
			}
			break;
		}
		return foundRequest;
	}
}
