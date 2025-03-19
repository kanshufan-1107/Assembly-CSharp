using System.Collections.Generic;
using System.Net;
using System.Text;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Hearthstone.Telemetry;
using HearthstoneTelemetry;

public class TelemetryManagerComponentNetwork : ITelemetryManagerComponent, ISocketEventListener
{
	public enum NetworkTelemetryEvent
	{
		CONNECT,
		DISCONNECT,
		SEND,
		RECEIVE,
		PING
	}

	private Map<string, TcpQualitySampler> m_samplers;

	public bool IsInitialized => m_samplers != null;

	public void Initialize()
	{
		m_samplers = new Map<string, TcpQualitySampler>();
	}

	public void Shutdown()
	{
		foreach (KeyValuePair<string, TcpQualitySampler> sampler2 in m_samplers)
		{
			sampler2.Value.EndConnection();
		}
	}

	public void OnSendNetworkTelemetryEvents(NetworkTelemetryEvent telemetryEvent, IPEndPoint endPoint, float value, BattleNetErrors errors = BattleNetErrors.ERROR_OK)
	{
		if (endPoint == null)
		{
			return;
		}
		string address = endPoint.Address.ToString();
		uint port = (uint)endPoint.Port;
		switch (telemetryEvent)
		{
		case NetworkTelemetryEvent.CONNECT:
			if (errors == BattleNetErrors.ERROR_OK)
			{
				ConnectEvent(address, port);
			}
			break;
		case NetworkTelemetryEvent.DISCONNECT:
			DisconnectEvent(address, port);
			break;
		case NetworkTelemetryEvent.SEND:
			SendPacketEvent(address, port, (uint)value);
			break;
		case NetworkTelemetryEvent.RECEIVE:
			ReceivePacketEvent(address, port, (uint)value);
			break;
		case NetworkTelemetryEvent.PING:
			ReceivePingEvent(address, port, value);
			break;
		}
	}

	public void ConnectEvent(string address, uint port)
	{
		if (IsInitialized)
		{
			string key = GetKey(address, port);
			if (!m_samplers.ContainsKey(key))
			{
				TcpQualitySampler sampler = new TcpQualitySampler(60000f);
				m_samplers.Add(key, sampler);
				sampler.StartConnection(address, port);
			}
		}
	}

	public void DisconnectEvent(string address, uint port)
	{
		if (IsInitialized)
		{
			string key = GetKey(address, port);
			if (m_samplers.TryGetValue(key, out var sampler))
			{
				sampler.EndConnection();
				m_samplers.Remove(key);
			}
		}
	}

	public void FlushSamplers()
	{
		if (!IsInitialized)
		{
			return;
		}
		foreach (KeyValuePair<string, TcpQualitySampler> sampler2 in m_samplers)
		{
			sampler2.Value.FlushSampler();
		}
	}

	public void SendPacketEvent(string address, uint port, uint bytes)
	{
		if (IsInitialized)
		{
			string key = GetKey(address, port);
			if (m_samplers.TryGetValue(key, out var sampler))
			{
				sampler.OnMessageSent(bytes);
			}
		}
	}

	public void ReceivePacketEvent(string address, uint port, uint bytes)
	{
		if (IsInitialized)
		{
			string key = GetKey(address, port);
			if (m_samplers.TryGetValue(key, out var sampler))
			{
				sampler.OnMessageReceived(bytes);
			}
		}
	}

	public void ReceivePingEvent(string address, uint port, float travelTime)
	{
		if (IsInitialized)
		{
			string key = GetKey(address, port);
			if (m_samplers.TryGetValue(key, out var sampler))
			{
				sampler.OnPing(travelTime);
			}
		}
	}

	private string GetKey(string address, uint host)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(address);
		stringBuilder.Append(':');
		stringBuilder.Append(host);
		return stringBuilder.ToString();
	}

	public void Update()
	{
	}
}
