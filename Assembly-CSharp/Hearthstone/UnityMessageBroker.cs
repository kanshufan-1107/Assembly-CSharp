using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MiniJSON;
using UnityEngine;

namespace Hearthstone;

public class UnityMessageBroker : MonoBehaviour
{
	private static UnityMessageBroker s_instance;

	private readonly Dictionary<string, List<IUnityMessageHandler>> m_handlers = new Dictionary<string, List<IUnityMessageHandler>>();

	private readonly List<IUnityMessageHandler> m_handlersToFire = new List<IUnityMessageHandler>();

	private void Awake()
	{
		if ((bool)s_instance)
		{
			Debug.LogError("[UnityMessageBroker] Multiple UnityMessageBroker instances! Destroying the superlous one.");
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
		else
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			s_instance = this;
		}
	}

	public static void Initialize()
	{
		EnsureInitialized();
	}

	public static void RegisterHandler(string topic, IUnityMessageHandler handler)
	{
		if (EnsureInitialized())
		{
			s_instance.InternalRegisterUnityMessageHandler(topic, handler);
		}
	}

	public static void UnregisterHandler(string topic, IUnityMessageHandler handler)
	{
		if (EnsureInitialized())
		{
			s_instance.InternalUnregisterUnityMessageHandler(topic, handler);
		}
	}

	public static void UnregisterUnityMessageHandlerFromAllTopics(IUnityMessageHandler handler)
	{
		if (!EnsureInitialized())
		{
			return;
		}
		foreach (List<IUnityMessageHandler> value in s_instance.m_handlers.Values)
		{
			value.RemoveAll((IUnityMessageHandler h) => h == handler);
		}
	}

	private void InternalRegisterUnityMessageHandler(string topic, IUnityMessageHandler handler)
	{
		if (string.IsNullOrEmpty(topic))
		{
			Log.All.PrintError("[UnityMessageBroker] Ignoring register handler call with null or empty topic");
			return;
		}
		if (handler == null)
		{
			Log.All.PrintError("[UnityMessageBroker] Ignoring registering null handler");
			return;
		}
		if (!m_handlers.TryGetValue(topic, out var handlers))
		{
			handlers = (m_handlers[topic] = new List<IUnityMessageHandler>());
		}
		handlers.Add(handler);
	}

	private void InternalUnregisterUnityMessageHandler(string topic, IUnityMessageHandler handler)
	{
		List<IUnityMessageHandler> handlers;
		if (string.IsNullOrEmpty(topic))
		{
			Log.All.PrintError("[UnityMessageBroker] Ignoring unregister handler call with null or empty topic");
		}
		else if (handler == null)
		{
			Log.All.PrintError("[UnityMessageBroker] Ignoring unregistering null handler");
		}
		else if (m_handlers.TryGetValue(topic, out handlers))
		{
			handlers.Remove(handler);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool EnsureInitialized()
	{
		if ((bool)s_instance)
		{
			return true;
		}
		new GameObject("UnityMessageBroker", typeof(UnityMessageBroker));
		if (!s_instance)
		{
			Log.All.PrintError("[UnityMessageBroker] Failed to create UnityMessageBroker instances");
		}
		return s_instance != null;
	}

	private void OnHandleJsonMessage(string messageJson)
	{
		if (string.IsNullOrWhiteSpace(messageJson))
		{
			Log.All.PrintWarning("[UnityMessageBroker] Ignoring unexpected empty message");
			return;
		}
		Log.All.PrintDebug("[UnityMessageBroker] Received message: " + messageJson);
		JsonNode message;
		try
		{
			message = Json.Deserialize(messageJson) as JsonNode;
			if (message == null)
			{
				Log.All.PrintError("[UnityMessageBroker] Received null json message. Message data: " + messageJson);
				return;
			}
		}
		catch (Exception exception)
		{
			Log.All.PrintException(exception, "[UnityMessageBroker] Failed to parse json'" + messageJson + "'");
			return;
		}
		if (message.TryGetValueAs<string>("topic", out var topic) && !string.IsNullOrEmpty(topic))
		{
			if (!m_handlers.TryGetValue(topic, out var handlers))
			{
				return;
			}
			m_handlersToFire.Clear();
			m_handlersToFire.AddRange(handlers);
			{
				foreach (IUnityMessageHandler handler in m_handlersToFire)
				{
					try
					{
						if (handler is UnityEngine.Object && handler as UnityEngine.Object == null)
						{
							Log.All.PrintError("[UnityMessageBroker] Dead handler detected! Forgot to unsubscribe?");
							UnregisterHandler(topic, handler);
						}
						else
						{
							handler.HandleUnityMessage(messageJson, message);
						}
					}
					catch (Exception ex)
					{
						Log.All.PrintException(ex, "[UnityMessageBroker] HandlerException: " + ex.Message);
					}
				}
				return;
			}
		}
		Log.All.PrintError("[UnityMessageBroker] Ignoring message with no topic. Message data: " + messageJson);
	}
}
