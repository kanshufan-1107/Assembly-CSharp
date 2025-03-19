using System;
using System.Collections.Generic;
using HearthstoneTelemetry;
using UnityEngine;

namespace Hearthstone.Telemetry;

public class TelemetryManagerComponentAudio : ITelemetryManagerComponent
{
	private struct QueuedAction
	{
		public Action Action;

		public float ProcessTime;
	}

	private enum ActionType
	{
		MasterVolume,
		MusicVolume
	}

	private IDeviceAudioSettingsProvider m_deviceAudioSettingsProvider;

	private readonly ITelemetryClient m_telemetryClient;

	private float m_nextPollingTime;

	private float m_lastMasterVolume;

	private float m_lastMusicVolume;

	private float m_lastDeviceVolume;

	private bool m_wasDeviceMuted;

	private Dictionary<ActionType, QueuedAction> m_queuedActionsMap = new Dictionary<ActionType, QueuedAction>();

	public TelemetryManagerComponentAudio(ITelemetryClient telemetryClient)
	{
		m_telemetryClient = telemetryClient;
	}

	public void Initialize()
	{
		m_deviceAudioSettingsProvider = new DeviceAudioSettingsProviderMock();
		m_wasDeviceMuted = m_deviceAudioSettingsProvider.IsMuted;
		m_lastDeviceVolume = m_deviceAudioSettingsProvider.Volume;
		m_lastMasterVolume = Options.Get().GetFloat(Option.SOUND_VOLUME);
		m_lastMusicVolume = Options.Get().GetFloat(Option.MUSIC_VOLUME);
		m_telemetryClient.SendStartupAudioSettings(m_wasDeviceMuted, m_lastDeviceVolume, m_lastMasterVolume, m_lastMusicVolume);
		GameState.RegisterGameStateInitializedListener(OnGameCreated);
		Options.Get().RegisterChangedListener(Option.SOUND_VOLUME, OnMasterVolumeChanged);
		Options.Get().RegisterChangedListener(Option.MUSIC_VOLUME, OnMusicVolumeChanged);
	}

	private void OnMasterVolumeChanged(Option option, object prevValue, bool existed, object userData)
	{
		QueueDelayedAction(ActionType.MasterVolume, delegate
		{
			float @float = Options.Get().GetFloat(Option.SOUND_VOLUME);
			m_telemetryClient.SendMasterVolumeChanged(m_lastMasterVolume, @float);
			m_lastMasterVolume = @float;
		});
	}

	private void OnMusicVolumeChanged(Option option, object prevValue, bool existed, object userData)
	{
		QueueDelayedAction(ActionType.MusicVolume, delegate
		{
			float @float = Options.Get().GetFloat(Option.MUSIC_VOLUME);
			m_telemetryClient.SendMusicVolumeChanged(m_lastMusicVolume, @float);
			m_lastMusicVolume = @float;
		});
	}

	private void OnGameCreated(GameState instance, object userData)
	{
		m_telemetryClient.SendGameRoundStartAudioSettings(m_wasDeviceMuted, m_lastDeviceVolume, m_lastMasterVolume, m_lastMusicVolume);
	}

	private void QueueDelayedAction(ActionType audioType, Action action)
	{
		m_queuedActionsMap[audioType] = new QueuedAction
		{
			Action = action,
			ProcessTime = Time.realtimeSinceStartup + 1f
		};
	}

	public void Update()
	{
		ProcessNativeAudioEvents();
		ProcessQueuedActions();
	}

	private void ProcessNativeAudioEvents()
	{
		if (!(Time.realtimeSinceStartup < m_nextPollingTime))
		{
			m_nextPollingTime = Time.realtimeSinceStartup + 10f;
			float deviceVolume = m_deviceAudioSettingsProvider.Volume;
			bool isMuted = m_deviceAudioSettingsProvider.IsMuted;
			if (Math.Abs(deviceVolume - m_lastDeviceVolume) > float.Epsilon)
			{
				Log.Telemetry.Print("AudioTelemetry: Device volume changed from {0} to {1}", m_lastDeviceVolume, deviceVolume);
				m_telemetryClient.SendDeviceVolumeChanged(m_lastDeviceVolume, deviceVolume);
			}
			if (isMuted != m_wasDeviceMuted)
			{
				Log.Telemetry.Print("AudioTelemetry: Device mute state changed to {0}", isMuted);
				m_telemetryClient.SendDeviceMuteChanged(isMuted);
			}
			m_lastDeviceVolume = deviceVolume;
			m_wasDeviceMuted = isMuted;
		}
	}

	private void ProcessQueuedActions()
	{
		List<ActionType> processedActionTypes = null;
		foreach (KeyValuePair<ActionType, QueuedAction> kv in m_queuedActionsMap)
		{
			QueuedAction queuedAction = kv.Value;
			ActionType actionType = kv.Key;
			if (Time.realtimeSinceStartup > queuedAction.ProcessTime)
			{
				if (processedActionTypes == null)
				{
					processedActionTypes = new List<ActionType>();
				}
				Log.Telemetry.Print("AudioTelemetry: Sending event for {0}", actionType);
				queuedAction.Action();
				processedActionTypes.Add(actionType);
			}
		}
		if (processedActionTypes == null)
		{
			return;
		}
		foreach (ActionType t in processedActionTypes)
		{
			m_queuedActionsMap.Remove(t);
		}
	}

	public void Shutdown()
	{
	}
}
