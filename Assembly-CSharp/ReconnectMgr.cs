using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Streaming;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class ReconnectMgr : IService, IHasUpdate
{
	public delegate bool GameTimeoutCallback(object userData);

	private class GameTimeoutListener : EventListener<GameTimeoutCallback>
	{
		public bool Fire()
		{
			return m_callback(m_userData);
		}
	}

	private class SavedStartGameParameters
	{
		public GameType GameType;

		public FormatType FormatType;

		public GameReconnectType gameReconnectType;

		public GameServerInfo ServerInfo;

		public int ScenarioId;

		public bool LoadGame;

		public override string ToString()
		{
			return $"GameType: {GameType}, FormatType: {FormatType}, ReconnectType: {gameReconnectType}, ScenarioId: {ScenarioId}, LoadGame: {LoadGame}";
		}
	}

	private class OpResult
	{
		public bool isSuccess;

		public string error;

		public OpResult(bool isSuccess, string error)
		{
			this.isSuccess = isSuccess;
			this.error = error;
		}
	}

	private readonly float[] RECONNECT_RATE_SECONDS = new float[4] { 1f, 2f, 3f, 5f };

	[StatePrinter.IncludeState]
	private GameReconnectType m_gameReconnectType;

	[StatePrinter.IncludeState]
	private float m_gameReconnectStartTimestamp;

	[StatePrinter.IncludeState]
	private float m_gameRetryStartTimestamp;

	[StatePrinter.IncludeState]
	private float m_utilReconnectTimer;

	[StatePrinter.IncludeState]
	private int m_numUtilReconnectAttempts;

	[StatePrinter.IncludeState]
	private bool m_bypassGameReconnect;

	[StatePrinter.IncludeState]
	private readonly SavedStartGameParameters m_savedStartGameParams = new SavedStartGameParameters();

	private readonly List<GameTimeoutListener> m_gameTimeoutListeners = new List<GameTimeoutListener>();

	[StatePrinter.IncludeState]
	private bool m_initializedForOfflineAccess;

	private Action m_nextReLoginCallback;

	private IEnumerator m_gameReconnectCoroutine;

	private NetworkReachabilityManager m_networkReachabilityManager;

	[StatePrinter.IncludeState]
	private bool m_hasCompletedFirstLogin;

	[StatePrinter.IncludeState]
	private bool m_suppressUtilReconnect;

	private readonly Stopwatch m_utilReconnectStopwatch = new Stopwatch();

	[StatePrinter.IncludeState]
	private string m_utilDisconnectReason = string.Empty;

	[StatePrinter.IncludeState]
	public bool FullResetRequired { get; set; }

	[StatePrinter.IncludeState]
	public bool UpdateRequired { get; set; }

	[StatePrinter.IncludeState]
	public bool ReconnectBlockedByInactivity { get; set; }

	private float TimeoutSec
	{
		get
		{
			if (!HearthstoneApplication.IsInternal())
			{
				return (float)OptionDataTables.s_defaultsMap[Option.RECONNECT_TIMEOUT];
			}
			return Options.Get().GetFloat(Option.RECONNECT_TIMEOUT);
		}
	}

	private float RetryTimeSec
	{
		get
		{
			if (!HearthstoneApplication.IsInternal())
			{
				return (float)OptionDataTables.s_defaultsMap[Option.RECONNECT_RETRY_TIME];
			}
			return Options.Get().GetFloat(Option.RECONNECT_RETRY_TIME);
		}
	}

	private static Blizzard.T5.Core.ILogger GameNetLogger => Network.Get().GameNetLogger;

	public event Action OnUtilReconnectComplete;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		m_networkReachabilityManager = ServiceManager.Get<NetworkReachabilityManager>();
		GameMgr.Get().RegisterFindGameEvent(OnFindGameEvent);
		Network network = Network.Get();
		network.AddBnetErrorListener(OnBnetError);
		network.OnDisconnectedFromBattleNet += OnDisconnectedFromBattleNet;
		serviceLocator.Get<LoginManager>().OnLoginCompleted += OnLoginComplete;
		HearthstoneApplication.Get().WillReset += WillReset;
		yield break;
	}

	public void Update()
	{
		CheckGameplayReconnectTimeout();
		CheckGameplayReconnectRetry();
		if (!Network.IsLoggedIn() && Network.ShouldBeConnectedToAurora())
		{
			if (CanAttemptUtilReconnect().isSuccess)
			{
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, $"ReconnectMgr.UpdateWhileDisconnectedFromBattleNet() - Attempting to reconnect (Attempt {m_numUtilReconnectAttempts}).");
				StartUtilReconnect();
			}
		}
		else if (Network.IsLoggedIn() && m_initializedForOfflineAccess)
		{
			OnBoxReconnectComplete();
		}
	}

	public Type[] GetDependencies()
	{
		return new Type[3]
		{
			typeof(LoginManager),
			typeof(GameMgr),
			typeof(NetworkReachabilityManager)
		};
	}

	public void Shutdown()
	{
		if (ServiceManager.TryGet<Network>(out var network))
		{
			network.RemoveBnetErrorListener(OnBnetError);
			network.OnDisconnectedFromBattleNet -= OnDisconnectedFromBattleNet;
		}
		if (ServiceManager.TryGet<GameMgr>(out var gameMgr))
		{
			gameMgr.UnregisterFindGameEvent(OnFindGameEvent);
		}
		HearthstoneApplication hsApp = HearthstoneApplication.Get();
		if (hsApp != null)
		{
			hsApp.WillReset -= WillReset;
		}
	}

	public static ReconnectMgr Get()
	{
		return ServiceManager.Get<ReconnectMgr>();
	}

	public bool IsReconnectingToGame()
	{
		return m_gameReconnectType != GameReconnectType.INVALID;
	}

	public bool IsRestoringGameStateFromDatabase()
	{
		if (m_savedStartGameParams != null)
		{
			return m_savedStartGameParams.LoadGame;
		}
		return false;
	}

	public static bool IsReconnectAllowed(FatalErrorMessage fatalErrorMessage)
	{
		if (fatalErrorMessage != null)
		{
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "ReconnectMgr.IsReconnectAllowed() - Checking Fatal Error Reason: {0}", fatalErrorMessage.m_reason);
		}
		NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (guardianVars == null)
		{
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "ReconnectMgr.IsReconnectAllowed() - Unable to retrieve guardian vars.");
			return false;
		}
		if (!guardianVars.AllowOfflineClientActivity)
		{
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "ReconnectMgr.IsReconnectAllowed() - Reconnect disabled by guardian var.");
			return false;
		}
		if (fatalErrorMessage != null && !FatalErrorMgr.IsReconnectAllowedBasedOnFatalErrorReason(fatalErrorMessage.m_reason))
		{
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "ReconnectMgr.IsReconnectAllowed() - Reconnect not allowed because of Fatal Error Reason. Reason={0}", fatalErrorMessage.m_reason);
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, Network.Get().GetNetworkGameStateString());
			return false;
		}
		return true;
	}

	public bool AddGameTimeoutListener(GameTimeoutCallback callback)
	{
		GameTimeoutListener listener = new GameTimeoutListener();
		listener.SetCallback(callback);
		listener.SetUserData(null);
		if (m_gameTimeoutListeners.Contains(listener))
		{
			return false;
		}
		m_gameTimeoutListeners.Add(listener);
		return true;
	}

	public void StartUtilReconnect()
	{
		ConnectionState status = BattleNet.Get()?.BattleNetStatus() ?? ConnectionState.Disconnected;
		if (status == ConnectionState.Disconnected || status == ConnectionState.Error)
		{
			Network.Get().ResetForNewAuroraConnection();
			LoginManager.Get().BeginLoginProcess();
			LoginManager.Get().OnFullLoginFlowComplete += OnReconnectLoginComplete;
			LoginManager.Get().OnAchievesLoaded += OnReconnectAchievesLoaded;
		}
	}

	public bool ReconnectToGameFromLogin()
	{
		NetCache.ProfileNoticeDisconnectedGame dcGameNotice = GetDCGameNotice();
		if (dcGameNotice != null && dcGameNotice.GameResult != ProfileNoticeDisconnectedGameResult.GameResult.GR_PLAYING)
		{
			ReconnectMgr_UI.ShowDisconnectedGameResult(dcGameNotice, GameNetLogger);
			AckNotice(dcGameNotice);
		}
		NetCache.NetCacheDisconnectedGame dcGame = NetCache.Get().GetNetObject<NetCache.NetCacheDisconnectedGame>();
		if (dcGame == null || dcGame.ServerInfo == null || (HearthstoneApplication.IsInternal() && !Vars.Key("Developer.ReconnectToGameFromLogin").GetBool(def: true)))
		{
			return false;
		}
		if (!DownloadUtils.HasNecessaryModeInstalled(dcGame, out var missingTag))
		{
			ReconnectMgr_UI.ShowModeNotAvailableWarning(missingTag);
			return false;
		}
		StartReconnectingToGame(GameReconnectType.LOGIN);
		ReconnectToGameFromLogin_RequestRequiredData(dcGame);
		return true;
	}

	private void ReconnectToGameFromLogin_RequestRequiredData(NetCache.NetCacheDisconnectedGame dcGame)
	{
		switch (dcGame.GameType)
		{
		case GameType.GT_BATTLEGROUNDS:
			Network.Get().RegisterNetHandler(BattlegroundsRatingInfoResponse.PacketID.ID, ReconnectToGameFromLogin_OnBaconRatingInfo);
			Network.Get().RequestBaconRatingInfo();
			break;
		case GameType.GT_MERCENARIES_PVP:
		case GameType.GT_MERCENARIES_PVE:
		case GameType.GT_MERCENARIES_PVE_COOP:
		case GameType.GT_MERCENARIES_FRIENDLY:
		{
			CollectionManager cm = CollectionManager.Get();
			if (cm.IsLettuceLoaded())
			{
				ReconnectToGameFromLogin_StartGame(dcGame);
				break;
			}
			cm.OnLettuceLoaded += delegate
			{
				ReconnectToGameFromLogin_StartGame(dcGame);
			};
			cm.StartInitialMercenaryLoadIfRequired();
			break;
		}
		default:
			ReconnectToGameFromLogin_StartGame(dcGame);
			break;
		}
	}

	private void ReconnectToGameFromLogin_OnBaconRatingInfo()
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "ReconnectMgr.ReconnectToGameFromLogin_OnBaconRatingInfo()");
		Network.Get().RemoveNetHandler(BattlegroundsRatingInfoResponse.PacketID.ID, ReconnectToGameFromLogin_OnBaconRatingInfo);
		NetCache.NetCacheDisconnectedGame dcGame = NetCache.Get().GetNetObject<NetCache.NetCacheDisconnectedGame>();
		ReconnectToGameFromLogin_StartGame(dcGame);
	}

	private void ReconnectToGameFromLogin_StartGame(NetCache.NetCacheDisconnectedGame dcGame)
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "ReconnectMgr.ReconnectToGameFromLogin_StartGame()");
		StartGame(dcGame.GameType, dcGame.FormatType, GameReconnectType.LOGIN, dcGame.ServerInfo, dcGame.ServerInfo.Mission, dcGame.LoadGameState);
	}

	public bool ReconnectToGameFromGameplay()
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "ReconnectMgr.ReconnectToGameFromGameplay()");
		GameServerInfo serverInfo = Network.Get().GetLastGameServerJoined();
		if (serverInfo == null)
		{
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, "ReconnectMgr.ReconnectToGameFromGameplay() - LastGameServerJoined serverInfo is null. Cannot ReconnectFromGameplay!");
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, Network.Get().GetNetworkGameStateString());
			return false;
		}
		if (serverInfo.SpectatorMode)
		{
			return false;
		}
		ReconnectMgr_UI.HideGameplayReconnectDialog();
		GameType gameType = GameMgr.Get().GetGameType();
		FormatType formatType = GameMgr.Get().GetFormatType();
		GameReconnectType gameReconnectType = GameReconnectType.GAMEPLAY;
		StartReconnectingToGame(gameReconnectType);
		m_gameReconnectCoroutine = WaitForInternetAndReconnectToGame(gameType, formatType, gameReconnectType, serverInfo);
		HearthstoneApplication.Get().StartCoroutine(m_gameReconnectCoroutine);
		return true;
	}

	private IEnumerator WaitForInternetAndReconnectToGame(GameType gameType, FormatType formatType, GameReconnectType gameReconnectType, GameServerInfo serverInfo)
	{
		while (!m_networkReachabilityManager.InternetAvailable_Cached)
		{
			yield return new WaitForSeconds(1f);
		}
		StartGame(gameType, formatType, gameReconnectType, serverInfo);
		m_gameReconnectCoroutine = null;
	}

	public void SetNextReLoginCallback(Action nextCallback)
	{
		m_nextReLoginCallback = nextCallback;
	}

	private void WillReset()
	{
		ReconnectMgr_UI.Reset();
		FullResetRequired = false;
		UpdateRequired = false;
		m_initializedForOfflineAccess = false;
		m_hasCompletedFirstLogin = false;
		ClearGameReconnectData();
		LoginManager.Get().OnFullLoginFlowComplete -= OnReconnectLoginComplete;
		LoginManager.Get().OnAchievesLoaded -= OnReconnectAchievesLoaded;
		m_gameTimeoutListeners.Clear();
	}

	private void StartReconnectingToGame(GameReconnectType gameReconnectType)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		m_gameReconnectType = gameReconnectType;
		m_gameReconnectStartTimestamp = realtimeSinceStartup;
		m_gameRetryStartTimestamp = realtimeSinceStartup;
		PerformanceAnalytics.Get()?.ReconnectStart(gameReconnectType.ToString());
		ReconnectMgr_UI.ShowGameplayReconnectingDialog(gameReconnectType);
	}

	private void CheckGameplayReconnectTimeout()
	{
		if (IsReconnectingToGame() && Time.realtimeSinceStartup - m_gameReconnectStartTimestamp >= TimeoutSec && !Network.Get().IsConnectedToGameServer())
		{
			OnGameReconnectTimeout();
		}
	}

	private void CheckGameplayReconnectRetry()
	{
		if (!m_networkReachabilityManager.InternetAvailable_Cached || !IsReconnectingToGame() || Network.Get().GetGameServerConnectionState() != 0 || Network.Get().GameServerHasDisconnectEvents())
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!(realtimeSinceStartup - m_gameRetryStartTimestamp < RetryTimeSec))
		{
			if (m_savedStartGameParams.ServerInfo == null)
			{
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, $"ReconnectMgr.CheckGameplayReconnectRetry() - m_savedStartGameParams.ServerInfo is null and should not be! {m_savedStartGameParams.ToString()}");
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, Network.Get().GetNetworkGameStateString());
			}
			else
			{
				m_gameRetryStartTimestamp = realtimeSinceStartup;
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "ReconnectMgr.CheckGameplayReconnectRetry() - calling StartGame_Internal");
				StartGame_Internal();
			}
		}
	}

	private void OnGameReconnectTimeout()
	{
		SetBypassGameReconnect(shouldBypass: true);
		ClearGameReconnectData();
		FireGameTimeoutEvent();
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY && GameMgr.Get().GetGameType() != 0)
		{
			bool isAdventureGame = GameMgr.Get().IsAI() && !GameMgr.Get().IsTavernBrawl();
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, Network.Get().GetNetworkGameStateStringForErrors());
			Error.AddFatal(FatalErrorReason.RECONNECT_TIME_OUT, isAdventureGame ? "GLOBAL_ERROR_NETWORK_ADVENTURE_RECONNECT_TIMEOUT" : "GLOBAL_ERROR_NETWORK_LOST_GAME_CONNECTION");
		}
		else
		{
			AttemptToRestoreGameState();
		}
	}

	private void AttemptToRestoreGameState()
	{
		if (m_savedStartGameParams.LoadGame)
		{
			GameMgr.Get().SetPendingAutoConcede(pendingAutoConcede: false);
			GameMgr gameMgr = GameMgr.Get();
			GameType gameType = m_savedStartGameParams.GameType;
			FormatType formatType = m_savedStartGameParams.FormatType;
			int scenarioId = m_savedStartGameParams.ScenarioId;
			long deckId = 0L;
			bool loadGame = m_savedStartGameParams.LoadGame;
			gameMgr.FindGame(gameType, formatType, scenarioId, 0, deckId, null, null, loadGame, null, null, 0L);
		}
		else
		{
			ClearGameReconnectData();
			ReconnectMgr_UI.ChangeGameplayDialogToTimeout();
		}
	}

	private bool OnBnetError(BnetErrorInfo info, object userData)
	{
		if (!IsReconnectingToGame() && !IsRestoringGameStateFromDatabase())
		{
			return false;
		}
		ReconnectMgr_UI.ChangeGameplayDialogToTimeout();
		if (m_savedStartGameParams != null)
		{
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "ReconnectMgr.OnBnetError() - m_savedStartGameParams.LoadGame = false");
			m_savedStartGameParams.LoadGame = false;
		}
		return true;
	}

	private void OnDisconnectedFromBattleNet(BattleNetErrors error)
	{
		m_initializedForOfflineAccess = false;
		FatalErrorMessage message = new FatalErrorMessage();
		message.m_reason = FatalErrorReason.UNKNOWN;
		InitializeForOfflineAccess(message, error.ToString());
	}

	public void SetBypassGameReconnect(bool shouldBypass)
	{
		m_bypassGameReconnect = shouldBypass;
	}

	public bool GetBypassGameReconnect()
	{
		return m_bypassGameReconnect;
	}

	private void ClearGameReconnectData()
	{
		m_gameReconnectType = GameReconnectType.INVALID;
		m_gameReconnectStartTimestamp = 0f;
		m_gameRetryStartTimestamp = 0f;
	}

	private void InitializeForOfflineAccess(FatalErrorMessage fatalErrorMessage, string reason)
	{
		if (!m_initializedForOfflineAccess && IsReconnectAllowed(fatalErrorMessage))
		{
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "ReconnectMgr.InitializeForOfflineAccess() - Initializing for offline box access.");
			Analytics_SendSeamlessReconnectStart(reason);
			m_initializedForOfflineAccess = true;
		}
	}

	private OpResult CanAttemptUtilReconnect()
	{
		if (!m_hasCompletedFirstLogin)
		{
			return new OpResult(isSuccess: false, "DID_NOT_COMPLETE_FIRST_LOGIN");
		}
		if (FullResetRequired)
		{
			return new OpResult(isSuccess: false, "FULL_RESET_REQUIRED");
		}
		if (ReconnectBlockedByInactivity)
		{
			return new OpResult(isSuccess: false, "BLOCKED_BY_INACTIVITY");
		}
		if (!m_networkReachabilityManager.InternetAvailable_Cached)
		{
			return new OpResult(isSuccess: false, "NETWORK_NOT_REACHABLE");
		}
		if (BattleNet.Get().BattleNetStatus() != 0)
		{
			return new OpResult(isSuccess: false, "BNET_ALREADY_CONNECTED");
		}
		if (m_suppressUtilReconnect)
		{
			return new OpResult(isSuccess: false, "UTIL_RECONNECT_SUPPRESSED");
		}
		m_utilReconnectTimer -= Time.deltaTime;
		if (m_utilReconnectTimer <= 0f)
		{
			float reconnectTimerCap = RECONNECT_RATE_SECONDS[Mathf.Min(m_numUtilReconnectAttempts, RECONNECT_RATE_SECONDS.Length - 1)];
			m_utilReconnectTimer = reconnectTimerCap;
			m_numUtilReconnectAttempts++;
			return new OpResult(isSuccess: true, "");
		}
		return new OpResult(isSuccess: false, "UTIL_RECONNECT_TIMER_NOT_DONE");
	}

	private void OnLoginComplete()
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "ReconnectMgr.OnLoginComplete() - Stored web token provided to BattleNet successfully, login completed.");
		m_numUtilReconnectAttempts = 0;
		m_utilReconnectTimer = 0f;
		m_hasCompletedFirstLogin = true;
	}

	private void OnBoxReconnectComplete()
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "ReconnectMgr.OnBoxReconnectComplete() - Reconnect Successful!");
		Analytics_SendSeamlessReconnectEnd();
		m_initializedForOfflineAccess = false;
		FatalErrorMgr.Get().ClearAllErrors();
	}

	private void OnReconnectAchievesLoaded()
	{
		LoginManager.Get().OnAchievesLoaded -= OnReconnectAchievesLoaded;
		if (this.OnUtilReconnectComplete != null)
		{
			this.OnUtilReconnectComplete();
		}
		if (!LoginManager.Get().AttemptToReconnectToGame(OnLoginReconnectToGameTimeout))
		{
			ReconnectMgr_UI.ShowIntroPopups();
		}
	}

	private bool OnLoginReconnectToGameTimeout(object userData)
	{
		ReconnectMgr_UI.ShowIntroPopups();
		return true;
	}

	private void OnReconnectLoginComplete()
	{
		LoginManager.Get().OnFullLoginFlowComplete -= OnReconnectLoginComplete;
		PopupDisplayManager.Get().ShowAnyOutstandingPopups();
		if (m_nextReLoginCallback != null)
		{
			m_nextReLoginCallback();
			m_nextReLoginCallback = null;
		}
	}

	public void SetSuppressUtilReconnect(bool value)
	{
		m_suppressUtilReconnect = value;
	}

	private NetCache.ProfileNoticeDisconnectedGame GetDCGameNotice()
	{
		NetCache.NetCacheProfileNotices notices = NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>();
		if (notices == null || notices.Notices == null || notices.Notices.Count == 0)
		{
			return null;
		}
		NetCache.ProfileNoticeDisconnectedGame newestNotice = null;
		List<NetCache.ProfileNoticeDisconnectedGame> noticesToAck = new List<NetCache.ProfileNoticeDisconnectedGame>();
		foreach (NetCache.ProfileNotice notice in notices.Notices)
		{
			if (notice is NetCache.ProfileNoticeDisconnectedGame)
			{
				NetCache.ProfileNoticeDisconnectedGame dcNotice = notice as NetCache.ProfileNoticeDisconnectedGame;
				noticesToAck.Add(dcNotice);
				if (newestNotice == null)
				{
					newestNotice = dcNotice;
				}
				else if (dcNotice.NoticeID > newestNotice.NoticeID)
				{
					newestNotice = dcNotice;
				}
			}
		}
		if (newestNotice == null)
		{
			return null;
		}
		foreach (NetCache.ProfileNoticeDisconnectedGame notice2 in noticesToAck)
		{
			if (notice2.NoticeID != newestNotice.NoticeID)
			{
				AckNotice(notice2);
			}
		}
		return newestNotice;
	}

	private void AckNotice(NetCache.ProfileNoticeDisconnectedGame notice)
	{
		Network.Get().AckNotice(notice.NoticeID);
	}

	private void StartGame(GameType gameType, FormatType formatType, GameReconnectType gameReconnectType, GameServerInfo serverInfo, int scenarioId = 0, bool loadGameState = false)
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, $"ReconnectMgr.StartGame() - GameReconnectType: {gameReconnectType}, loadGameState: {loadGameState}");
		m_savedStartGameParams.GameType = gameType;
		m_savedStartGameParams.FormatType = formatType;
		m_savedStartGameParams.gameReconnectType = gameReconnectType;
		m_savedStartGameParams.ServerInfo = serverInfo;
		m_savedStartGameParams.ScenarioId = scenarioId;
		m_savedStartGameParams.LoadGame = loadGameState;
		StartGame_Internal();
	}

	private void StartGame_Internal()
	{
		if (m_gameReconnectCoroutine != null)
		{
			HearthstoneApplication.Get().StopCoroutine(m_gameReconnectCoroutine);
			m_gameReconnectCoroutine = null;
		}
		GameMgr.Get().ReconnectGame(m_savedStartGameParams.GameType, m_savedStartGameParams.FormatType, m_savedStartGameParams.gameReconnectType, m_savedStartGameParams.ServerInfo);
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		switch (eventData.m_state)
		{
		case FindGameState.SERVER_GAME_STARTED:
			if (IsReconnectingToGame() || IsRestoringGameStateFromDatabase())
			{
				m_gameTimeoutListeners.Clear();
				ReconnectMgr_UI.ChangeGameplayDialogToReconnected(m_gameReconnectType);
				ClearGameReconnectData();
			}
			break;
		case FindGameState.SERVER_GAME_CANCELED:
			if (IsReconnectingToGame() || IsRestoringGameStateFromDatabase())
			{
				OnGameReconnectTimeout();
				return true;
			}
			break;
		}
		return false;
	}

	private void FireGameTimeoutEvent()
	{
		PerformanceAnalytics.Get()?.ReconnectEnd(success: false);
		GameTimeoutListener[] listeners = m_gameTimeoutListeners.ToArray();
		m_gameTimeoutListeners.Clear();
		bool handled = false;
		for (int i = 0; i < listeners.Length; i++)
		{
			handled = listeners[i].Fire() || handled;
		}
		if (!handled && Network.IsLoggedIn())
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
		}
	}

	private void Analytics_SendSeamlessReconnectStart(string reason)
	{
		if (!m_utilReconnectStopwatch.IsRunning)
		{
			m_utilReconnectStopwatch.Start();
			m_utilDisconnectReason = reason;
			var (secSinceResume, secSpentPaused) = HearthstoneApplication.Get().GetPauseTimesInSeconds();
			TelemetryManager.Client().SendSeamlessReconnectStart(m_utilDisconnectReason, secSinceResume, secSpentPaused);
		}
	}

	private void Analytics_SendSeamlessReconnectEnd()
	{
		var (secSinceResume, secSpentPaused) = HearthstoneApplication.Get().GetPauseTimesInSeconds();
		TelemetryManager.Client().SendSeamlessReconnectEnd(m_utilReconnectStopwatch.ElapsedMilliseconds, m_utilDisconnectReason, secSinceResume, secSpentPaused);
		m_utilDisconnectReason = string.Empty;
		m_utilReconnectStopwatch.Reset();
	}
}
