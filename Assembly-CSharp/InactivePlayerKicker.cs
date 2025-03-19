using System;
using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Streaming;
using UnityEngine;

public class InactivePlayerKicker : IService, IHasUpdate
{
	private bool m_checkingForInactivity;

	private bool m_shouldCheckForInactivity = true;

	private float m_kickSec = 1800f;

	private bool m_activityDetected;

	private float m_inactivityStartTimestamp;

	private GameMgr m_gameMgr;

	public bool WasKickedForInactivity { get; private set; }

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		serviceLocator.Get<SceneMgr>().RegisterScenePreUnloadEvent(OnScenePreUnload);
		serviceLocator.Get<LoginManager>().OnAchievesLoaded += OnUtilReconnect;
		m_gameMgr = serviceLocator.Get<GameMgr>();
		if (HearthstoneApplication.IsInternal())
		{
			Options.Get().RegisterChangedListener(Option.IDLE_KICK_TIME, OnOptionChanged);
			Options.Get().RegisterChangedListener(Option.IDLE_KICKER, OnOptionChanged);
		}
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[6]
		{
			typeof(Network),
			typeof(SceneMgr),
			typeof(ReconnectMgr),
			typeof(LoginManager),
			typeof(GameMgr),
			typeof(GameDownloadManager)
		};
	}

	public void Shutdown()
	{
		HearthstoneApplication.Get().WillReset -= WillReset;
		if (ServiceManager.TryGet<SceneMgr>(out var sceneMgr))
		{
			sceneMgr.UnregisterScenePreUnloadEvent(OnScenePreUnload);
		}
		if (ServiceManager.TryGet<LoginManager>(out var loginMgr))
		{
			loginMgr.OnAchievesLoaded -= OnUtilReconnect;
		}
		if (HearthstoneApplication.IsInternal())
		{
			Options.Get().UnregisterChangedListener(Option.IDLE_KICK_TIME, OnOptionChanged);
			Options.Get().UnregisterChangedListener(Option.IDLE_KICKER, OnOptionChanged);
		}
	}

	public void Update()
	{
		CheckInactivity();
		CheckActivity();
	}

	private void WillReset()
	{
		SetShouldCheckForInactivity(check: true);
	}

	public static InactivePlayerKicker Get()
	{
		return ServiceManager.Get<InactivePlayerKicker>();
	}

	public void OnLoggedIn()
	{
		UpdateIdleKickTimeOption();
		UpdateCheckForInactivity();
	}

	private void OnUtilReconnect()
	{
		SetShouldCheckForInactivity(check: true);
		WasKickedForInactivity = false;
	}

	public bool IsCheckingForInactivity()
	{
		return m_checkingForInactivity;
	}

	public void SetShouldCheckForInactivity(bool check)
	{
		if (m_shouldCheckForInactivity != check)
		{
			m_shouldCheckForInactivity = check;
			UpdateCheckForInactivity();
		}
	}

	public void SetKickSec(float sec)
	{
		m_kickSec = sec;
	}

	public bool SetKickTimeStr(string timeStr)
	{
		if (!TimeUtils.TryParseDevSecFromElapsedTimeString(timeStr, out var sec))
		{
			return false;
		}
		SetKickSec(sec);
		return true;
	}

	private bool CanCheckForInactivity()
	{
		if (DemoMgr.Get().IsExpoDemo())
		{
			return false;
		}
		if (!m_shouldCheckForInactivity)
		{
			return false;
		}
		if (HearthstoneApplication.IsInternal() && !Options.Get().GetBool(Option.IDLE_KICKER))
		{
			return false;
		}
		if (!GameDownloadManagerProvider.Get().IsReadyToPlay)
		{
			return false;
		}
		return true;
	}

	private void UpdateCheckForInactivity()
	{
		bool wasCheckingForInactivity = m_checkingForInactivity;
		m_checkingForInactivity = CanCheckForInactivity();
		if (m_checkingForInactivity && !wasCheckingForInactivity)
		{
			StartCheckForInactivity();
		}
	}

	private void StartCheckForInactivity()
	{
		m_activityDetected = false;
		m_inactivityStartTimestamp = Time.realtimeSinceStartup;
	}

	private void CheckActivity()
	{
		if (IsCheckingForInactivity())
		{
			if (Input.anyKey || Input.touchCount > 0)
			{
				m_activityDetected = true;
			}
			else if (m_gameMgr.IsSpectator())
			{
				m_activityDetected = true;
			}
		}
	}

	private void CheckInactivity()
	{
		if (IsCheckingForInactivity())
		{
			if (m_activityDetected)
			{
				m_inactivityStartTimestamp = Time.realtimeSinceStartup;
				m_activityDetected = false;
				ReconnectMgr.Get().ReconnectBlockedByInactivity = false;
			}
			else if (!WasKickedForInactivity && Time.realtimeSinceStartup - m_inactivityStartTimestamp >= m_kickSec)
			{
				Error.AddFatal(FatalErrorReason.INACTIVITY_TIMEOUT, "GLOBAL_ERROR_INACTIVITY_KICK");
				ReconnectMgr.Get().ReconnectBlockedByInactivity = true;
				WasKickedForInactivity = true;
				BattleNet.RequestCloseAurora();
				DialogManager.Get().ShowReconnectHelperDialog();
			}
		}
	}

	private void OnScenePreUnload(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.FATAL_ERROR)
		{
			SetShouldCheckForInactivity(check: false);
		}
	}

	private void UpdateIdleKickTimeOption()
	{
		if (HearthstoneApplication.IsInternal())
		{
			SetKickTimeStr(Options.Get().GetString(Option.IDLE_KICK_TIME));
		}
	}

	private void OnOptionChanged(Option option, object prevValue, bool existed, object userData)
	{
		switch (option)
		{
		case Option.IDLE_KICKER:
			UpdateCheckForInactivity();
			break;
		case Option.IDLE_KICK_TIME:
			UpdateIdleKickTimeOption();
			break;
		default:
			Error.AddDevFatal("InactivePlayerKicker.OnOptionChanged() - unhandled option {0}", option);
			break;
		}
	}
}
