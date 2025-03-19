using System;
using System.Collections.Generic;
using System.Diagnostics;
using Blizzard.Telemetry.Standard.Network;
using Hearthstone.Core;

namespace HearthstoneTelemetry;

public class TcpQualitySampler
{
	private enum MessageType
	{
		SENT,
		RECEIVED
	}

	private class Message
	{
		public MessageType Type { get; private set; }

		public uint Bytes { get; private set; }

		public float TimeSincePreviousMessage { get; private set; }

		public Message(MessageType type, uint bytes, float timeSincePreviousMessage)
		{
			Type = type;
			Bytes = bytes;
			TimeSincePreviousMessage = timeSincePreviousMessage;
		}
	}

	private float m_sampleTimeMs;

	private Stopwatch m_sampleTimeStopwatch;

	private string m_ipv4 = "";

	private uint m_port;

	private Stopwatch m_timeSinceLastMessageStopwatch;

	private Pool<Message> m_messagePool;

	private List<Message> m_messages;

	private List<Message> m_pingPongMessages;

	public TcpQualitySampler(float sampleTimeMs)
	{
		m_messagePool = new Pool<Message>();
		m_messagePool.SetMaxReleasedItemCount(300);
		m_sampleTimeMs = sampleTimeMs;
	}

	public void StartConnection(string ipv4, uint port)
	{
		m_ipv4 = ipv4;
		m_port = port;
		m_sampleTimeStopwatch = new Stopwatch();
		m_sampleTimeStopwatch.Start();
		m_messages = new List<Message>();
		m_pingPongMessages = new List<Message>();
		m_timeSinceLastMessageStopwatch = new Stopwatch();
		m_timeSinceLastMessageStopwatch.Start();
		Processor.RegisterUpdateDelegate(TrySampleNetworkQuality);
		Log.Telemetry.Print("Registered network quality sampler for " + ipv4 + ":" + port);
	}

	public void EndConnection()
	{
		FlushSampler();
		Processor.UnregisterUpdateDelegate(TrySampleNetworkQuality);
		Log.Telemetry.Print("Successfully unregistered network quality sampler for " + m_ipv4 + ":" + m_port);
	}

	private void TrySampleNetworkQuality()
	{
		if ((float)m_sampleTimeStopwatch.ElapsedMilliseconds > m_sampleTimeMs)
		{
			FlushSampler();
		}
	}

	public void FlushSampler()
	{
		SendNetworkQualityMessage();
		lock (m_messagePool)
		{
			m_messagePool.ReleaseAll();
		}
		m_messages.Clear();
		m_pingPongMessages.Clear();
		m_sampleTimeStopwatch.Restart();
	}

	private void SendNetworkQualityMessage()
	{
		uint bytesReceived = 0u;
		uint bytesSent = 0u;
		uint messagesReceived = 0u;
		uint messagesSent = 0u;
		float pingMin = float.MaxValue;
		float pingMax = float.MinValue;
		float pingAvg = 0f;
		float pingStdDev = 0f;
		float timeSincePrevMessageMin = float.MaxValue;
		float timeSincePrevMessageMax = float.MinValue;
		lock (m_messages)
		{
			if (m_messages != null && m_messages.Count > 0)
			{
				foreach (Message message in m_messages)
				{
					if (message.Type == MessageType.SENT)
					{
						messagesSent++;
						bytesSent += message.Bytes;
						continue;
					}
					messagesReceived++;
					bytesReceived += message.Bytes;
					timeSincePrevMessageMin = Math.Min(timeSincePrevMessageMin, message.TimeSincePreviousMessage);
					timeSincePrevMessageMax = Math.Max(timeSincePrevMessageMax, message.TimeSincePreviousMessage);
				}
			}
		}
		lock (m_pingPongMessages)
		{
			if (m_pingPongMessages != null && m_pingPongMessages.Count > 0)
			{
				float pingRTTSum = 0f;
				float pingDeltaSum = 0f;
				foreach (Message pingPongMessage in m_pingPongMessages)
				{
					pingMin = Math.Min(pingMin, pingPongMessage.TimeSincePreviousMessage);
					pingMax = Math.Max(pingMax, pingPongMessage.TimeSincePreviousMessage);
					pingRTTSum += pingPongMessage.TimeSincePreviousMessage;
				}
				pingAvg = pingRTTSum / (float)m_pingPongMessages.Count;
				foreach (Message pingPongMessage2 in m_pingPongMessages)
				{
					pingDeltaSum += (float)Math.Pow(pingAvg - pingPongMessage2.TimeSincePreviousMessage, 2.0);
				}
				pingStdDev = (float)Math.Sqrt(pingDeltaSum / (float)m_pingPongMessages.Count);
			}
		}
		if (m_messages.Count != 0 || m_pingPongMessages.Count != 0)
		{
			TcpQualitySample.Metric timeSincePrevMessageMs = null;
			if (timeSincePrevMessageMin != float.MaxValue)
			{
				timeSincePrevMessageMs = new TcpQualitySample.Metric
				{
					Min = timeSincePrevMessageMin,
					Max = timeSincePrevMessageMax
				};
			}
			TcpQualitySample.Metric pingMs = null;
			if (pingMin != float.MaxValue)
			{
				pingMs = new TcpQualitySample.Metric
				{
					Min = pingMin,
					Max = pingMax,
					Avg = pingAvg,
					StdDev = pingStdDev
				};
			}
			TelemetryManager.Client().SendTcpQualitySample(m_ipv4, m_port, m_sampleTimeStopwatch.ElapsedMilliseconds, bytesSent, bytesReceived, messagesSent, messagesReceived, timeSincePrevMessageMs, pingMs);
			Log.Telemetry.Print("Sent network quality message for " + m_ipv4 + ":" + m_port);
		}
	}

	public void OnMessageSent(uint bytes)
	{
		Message message;
		lock (m_messagePool)
		{
			message = m_messagePool.Acquire();
		}
		if (message == null)
		{
			message = new Message(MessageType.SENT, bytes, 0f);
		}
		AddMessage(m_messages, message);
	}

	public void OnMessageReceived(uint bytes)
	{
		float timeSinceLastMessageMs = m_timeSinceLastMessageStopwatch.ElapsedMilliseconds;
		Message message;
		lock (m_messagePool)
		{
			message = m_messagePool.Acquire();
		}
		if (message == null)
		{
			message = new Message(MessageType.RECEIVED, bytes, timeSinceLastMessageMs);
		}
		AddMessage(m_messages, message);
		m_timeSinceLastMessageStopwatch.Restart();
	}

	public void OnPing(float travelTime)
	{
		Message message;
		lock (m_messagePool)
		{
			message = m_messagePool.Acquire();
		}
		if (message == null)
		{
			message = new Message(MessageType.RECEIVED, 0u, travelTime);
		}
		AddMessage(m_pingPongMessages, message);
	}

	private void AddMessage(List<Message> messageList, Message message)
	{
		lock (messageList)
		{
			if (m_messagePool.GetActiveList().Count >= 300)
			{
				FlushSampler();
				messageList.Add(message);
			}
			messageList.Add(message);
		}
	}
}
