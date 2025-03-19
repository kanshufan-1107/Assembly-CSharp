using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Logging;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.Streaming;

namespace Hearthstone.Startup;

public class ClientStartupService : IService
{
	private LoginManager m_loginManager;

	private GameDownloadManager m_gameDownloadManager;

	private SplashLoadingText m_splashLoadingText;

	private ILogger m_logger;

	private StartupStage m_lastStage = StartupStage.Start;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		LogInfo logInfo = new LogInfo
		{
			m_consolePrinting = true,
			m_minLevel = Blizzard.T5.Logging.LogLevel.Info,
			m_defaultLevel = Blizzard.T5.Logging.LogLevel.Info
		};
		m_logger = LogSystem.Get().CreateLogger("Startup", logInfo);
		m_gameDownloadManager = serviceLocator.Get<GameDownloadManager>();
		RegisterDownloadManagerEvents(m_gameDownloadManager);
		Processor.QueueJob(HearthstoneJobs.CreateJobFromAction("ClientStartup.RegisterLoginManager", delegate
		{
			RegisterLoginManagerEvents(serviceLocator);
		}, typeof(LoginManager)));
		SplashScreen.SplashScreenShown += OnSplashScreenShown;
		SplashScreen.SplashScreenHidden += OnSplashScreenHidden;
		HearthstoneApplication.Get().Resetting += OnResetting;
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(GameDownloadManager) };
	}

	public void Shutdown()
	{
		HearthstoneApplication.Get().Resetting -= OnResetting;
		SplashScreen.SplashScreenShown -= OnSplashScreenShown;
		SplashScreen.SplashScreenHidden -= OnSplashScreenHidden;
		UnregisterDownloadManagerEvents(m_gameDownloadManager);
		if (m_loginManager != null)
		{
			UnregisterLoginManagerEvents(m_loginManager);
		}
		m_loginManager = null;
		m_gameDownloadManager = null;
	}

	private void SetStartupStage(StartupStage stage)
	{
		m_lastStage = stage;
		m_splashLoadingText?.SetStartupStage(stage);
		LogStage(stage);
	}

	private void LogStage(StartupStage stage)
	{
		m_logger.Log(Blizzard.T5.Core.LogLevel.Information, "Startup stage " + stage);
	}

	private void UpdateStageProgress(StartupStage stage, int progress)
	{
		if (m_lastStage != stage)
		{
			LogStage(stage);
		}
		m_lastStage = stage;
		m_splashLoadingText?.UpdateStageProgress(stage, progress);
	}

	private void RegisterLoginManagerEvents(ServiceLocator serviceLocator)
	{
		m_loginManager = serviceLocator.Get<LoginManager>();
		m_loginManager.OnInitialClientStateReceived += OnInitialClientState;
		m_loginManager.OnLoginStarted += OnLoginStarted;
		m_loginManager.OnEnterNoAccountFlow += OnEnterNoAccountFlow;
		m_loginManager.OnLoginCompleted += OnLoginCompleted;
	}

	private void UnregisterLoginManagerEvents(LoginManager loginManager)
	{
		loginManager.OnInitialClientStateReceived -= OnInitialClientState;
		loginManager.OnLoginStarted -= OnLoginStarted;
		loginManager.OnEnterNoAccountFlow -= OnEnterNoAccountFlow;
		loginManager.OnLoginCompleted -= OnLoginCompleted;
	}

	private void OnInitialClientState()
	{
		SetStartupStage(StartupStage.LaunchGame);
	}

	private void OnLoginStarted()
	{
		SetStartupStage(StartupStage.ConnectingToBattlenet);
	}

	private void OnLoginCompleted()
	{
		SetStartupStage(StartupStage.ConnectingToHearthstone);
	}

	private void OnEnterNoAccountFlow()
	{
		SetStartupStage(StartupStage.NoAccountFlow);
	}

	private void RegisterDownloadManagerEvents(GameDownloadManager downloadManager)
	{
		downloadManager.VersioningStarted += OnVersioningStarted;
		downloadManager.InitialDownloadStarted += OnInitialDownloadStarted;
		downloadManager.ApkDownloadProgress += OnApkDownloadProgress;
		downloadManager.DbfDownloadProgress += OnDbfDownloadProgress;
	}

	private void UnregisterDownloadManagerEvents(GameDownloadManager downloadManager)
	{
		downloadManager.VersioningStarted -= OnVersioningStarted;
		downloadManager.InitialDownloadStarted -= OnInitialDownloadStarted;
		downloadManager.ApkDownloadProgress -= OnApkDownloadProgress;
		downloadManager.DbfDownloadProgress -= OnDbfDownloadProgress;
	}

	private void OnVersioningStarted()
	{
		SetStartupStage(StartupStage.VersioningAssets);
	}

	private void OnInitialDownloadStarted()
	{
		SetStartupStage(StartupStage.CheckingForUpdates);
	}

	private void OnDbfDownloadProgress(int progress)
	{
		UpdateStageProgress(StartupStage.DownloadDbf, progress);
	}

	private void OnApkDownloadProgress(int progress)
	{
		UpdateStageProgress(StartupStage.DownloadAPK, progress);
	}

	private void OnSplashScreenShown(SplashScreen splashScreen)
	{
		m_splashLoadingText = splashScreen.GetLoadingText();
		SetStartupStage(m_lastStage);
	}

	private void OnSplashScreenHidden()
	{
		m_splashLoadingText = null;
	}

	private void OnResetting()
	{
		m_lastStage = StartupStage.Start;
	}
}
