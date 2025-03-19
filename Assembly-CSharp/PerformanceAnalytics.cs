using System;
using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration.PreferencesManager;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using UnityEngine;

public class PerformanceAnalytics : IService
{
	private float m_initStartTime;

	private bool m_isReconnecting;

	private string m_reconnectType = "INVALID";

	private float m_reconnectStartTime;

	private float m_disconnectTime;

	private string m_location = string.Empty;

	private readonly float LOW_MEMORY_THRESHOLD = 100f;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		BeginStartupTimer();
		if (BattleNet.IsInitialized() && BattleNet.IsConnected())
		{
			m_location = BattleNet.GetAccountCountry();
		}
		SendDisconnectAndTimeoutEvents();
		SceneMgr.Get().RegisterSceneLoadedEvent(OnSceneLoaded);
		SceneMgr.Get().RegisterSceneUnloadedEvent(OnSceneUnloaded);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(SceneMgr) };
	}

	public void Shutdown()
	{
		if (SceneMgr.Get() != null)
		{
			SceneMgr.Get().UnregisterSceneLoadedEvent(OnSceneLoaded);
			SceneMgr.Get().UnregisterSceneUnloadedEvent(OnSceneUnloaded);
		}
	}

	public static PerformanceAnalytics Get()
	{
		return ServiceManager.Get<PerformanceAnalytics>();
	}

	public void BeginStartupTimer()
	{
		m_initStartTime = Time.realtimeSinceStartup;
	}

	public void ReconnectStart(string reconnectType)
	{
		if (!m_isReconnecting)
		{
			m_isReconnecting = true;
			m_reconnectType = reconnectType;
			m_reconnectStartTime = Time.realtimeSinceStartup;
			SceneMgr.Get().RegisterSceneLoadedEvent(ReconnectSceneLoaded);
			SendDisconnectAndTimeoutEvents();
		}
	}

	public void ReconnectSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (m_isReconnecting && mode == SceneMgr.Mode.GAMEPLAY)
		{
			ReconnectEnd(success: true);
			SceneMgr.Get().UnregisterSceneLoadedEvent(ReconnectSceneLoaded);
		}
	}

	public void DisconnectEvent(string mode)
	{
		m_disconnectTime = Time.realtimeSinceStartup;
		SceneMgr.Get().RegisterSceneLoadedEvent(DisconnectTimeReset);
		PreferencesManager.SetInt("DisconnectEvent", 1);
		PreferencesManager.SetString("DisconnectEvent_Mode", mode);
		PreferencesManager.SetString("DisconnectEvent_Location", GetCountry());
		PreferencesManager.SetString("DisconnectEvent_Connection", GetConnectionType());
		PreferencesManager.SetString("DisconnectEvent_OS", PlatformSettings.OS.ToString());
	}

	public void SendDisconnectAndTimeoutEvents()
	{
		if (Application.internetReachability != 0)
		{
			if (PreferencesManager.GetInt("DisconnectEvent") == 1)
			{
				PreferencesManager.SetInt("DisconnectEvent", 0);
				Log.Performance.Print("Sent Disconnect Event");
			}
			if (PreferencesManager.GetInt("ReconnectTimeOut") == 1)
			{
				PreferencesManager.SetInt("ReconnectTimeOut", 0);
				TelemetryManager.Client().SendReconnectTimeout(PreferencesManager.GetString("ReconnectTimeOut_Type"));
				Log.Performance.Print("Sent Reconnect Timout Event");
			}
		}
	}

	public void DisconnectTimeReset(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (!m_isReconnecting && (mode == SceneMgr.Mode.GAMEPLAY || mode == SceneMgr.Mode.HUB))
		{
			m_disconnectTime = 0f;
		}
	}

	public void ReconnectEnd(bool success)
	{
		if (m_isReconnecting)
		{
			SendDisconnectAndTimeoutEvents();
			m_isReconnecting = false;
			float reconnectTime = Time.realtimeSinceStartup - m_reconnectStartTime;
			float disconnectedDuration = Time.realtimeSinceStartup - m_disconnectTime;
			if (success)
			{
				TelemetryManager.Client().SendReconnectSuccess(disconnectedDuration, reconnectTime, m_reconnectType);
				m_disconnectTime = 0f;
				Log.Performance.Print("Sent Reconnect Success Event");
				return;
			}
			PreferencesManager.SetInt("ReconnectTimeOut", 1);
			PreferencesManager.SetString("ReconnectTimeOut_Type", m_reconnectType);
			PreferencesManager.SetString("ReconnectTimeOut_Location", GetCountry());
			PreferencesManager.SetString("ReconnectTimeOut_Connection", GetConnectionType());
			PreferencesManager.SetString("ReconnectTimeOut_OS", PlatformSettings.OS.ToString());
			m_disconnectTime = 0f;
			Log.Performance.Print("Recorded Reconnect Timout");
		}
	}

	private string GetCountry()
	{
		if (string.IsNullOrEmpty(m_location) && BattleNet.IsConnected())
		{
			m_location = BattleNet.GetAccountCountry();
		}
		if (string.IsNullOrEmpty(m_location))
		{
			m_location = "Unknown";
		}
		return m_location;
	}

	private string GetConnectionType()
	{
		if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
		{
			return "Cellular";
		}
		if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
		{
			return "LAN";
		}
		return "None";
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
	}

	private void OnSceneUnloaded(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
	}
}
