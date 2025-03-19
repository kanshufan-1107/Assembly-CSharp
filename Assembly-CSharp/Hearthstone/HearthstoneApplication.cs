using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using AntiCheatSDK;
using Blizzard.BlizzardErrorMobile;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core.Time;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Fonts;
using Blizzard.T5.Jobs;
using Blizzard.T5.Logging;
using Blizzard.T5.MaterialService;
using Blizzard.T5.Services;
using Hearthstone.APIGateway;
using Hearthstone.Attribution;
using Hearthstone.BreakingNews;
using Hearthstone.Core;
using Hearthstone.Core.Deeplinking;
using Hearthstone.Core.Streaming;
using Hearthstone.DataModels;
using Hearthstone.Http;
using Hearthstone.InGameMessage;
using Hearthstone.InGameMessage.UI;
using Hearthstone.Login;
using Hearthstone.MarketingImages;
using Hearthstone.PlayerExperiments;
using Hearthstone.Progression;
using Hearthstone.Startup;
using Hearthstone.Store;
using Hearthstone.Streaming;
using Hearthstone.UI;
using Hearthstone.Util;
using UnityEngine;

namespace Hearthstone;

public class HearthstoneApplication : MonoBehaviour
{
	private class FocusChangedListener : EventListener<FocusChangedCallback>
	{
		public void Fire(bool focused)
		{
			m_callback(focused, m_userData);
		}
	}

	public delegate void FocusChangedCallback(bool focused, object userData);

	private const ApplicationMode DEFAULT_MODE = ApplicationMode.INTERNAL;

	private const float kUnloadUnusedAssetsDelay = 1f;

	public static readonly PlatformDependentValue<bool> CanQuitGame = new PlatformDependentValue<bool>(PlatformCategory.OS)
	{
		PC = true,
		Mac = true,
		Android = true,
		iOS = false
	};

	public static readonly PlatformDependentValue<bool> AllowResetFromFatalError = new PlatformDependentValue<bool>(PlatformCategory.OS)
	{
		PC = false,
		Mac = false,
		Android = true,
		iOS = true
	};

	private static bool s_initializedMode = false;

	private static string[] s_cachedCmdLineArgs = null;

	private static bool s_cachedCmdLineArgsAreModified = false;

	private bool m_exiting;

	private bool m_focused = true;

	private List<FocusChangedListener> m_focusChangedListeners = new List<FocusChangedListener>();

	private float m_lastResumeTime = -999999f;

	private float m_lastPauseTime;

	private bool m_hasResetSinceLastResume;

	private const float AUTO_RESET_ON_ERROR_TIMEOUT = 1f;

	private bool m_resetting;

	private float m_lastResetTime;

	private bool m_unloadUnusedAssets;

	private float m_unloadUnusedAssetsDelay;

	private static ApplicationMode s_mode = ApplicationMode.INVALID;

	private static HearthstoneApplication s_instance = null;

	private static int s_mainThreadId = -1;

	public WaitForCallback DataTransferDependency = new WaitForCallback();

	public static string[] CommandLineArgs
	{
		get
		{
			if (s_cachedCmdLineArgs == null)
			{
				ReadCommandLineArgs();
			}
			return s_cachedCmdLineArgs;
		}
	}

	public static float AwakeTime { get; private set; }

	public static bool IsHearthstoneRunning => s_instance != null;

	public static bool IsHearthstoneClosing { get; private set; }

	public bool IsLocaleChanged { get; set; }

	public string TestType { get; private set; }

	private string UberTextCacheFolderPath => $"{PlatformFilePaths.CachePath}/UberText";

	private string UberTextCacheFilePath => $"{UberTextCacheFolderPath}/text_{Localization.GetLocale()}.cache";

	public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == s_mainThreadId;

	public static bool IsCNMobileBinary => false;

	public static string HSLaunchURI => LaunchArguments.CreateLaunchArgument(usingT5Mobile: true);

	public event Action WillReset;

	public event Action Resetting;

	public event Action Paused;

	public event Action Unpaused;

	public event Action OnShutdown;

	private void Awake()
	{
		AwakeTime = Time.realtimeSinceStartup;
		s_instance = this;
		base.gameObject.AddComponent<HSDontDestroyOnLoad>();
		IsLocaleChanged = false;
		s_mainThreadId = Thread.CurrentThread.ManagedThreadId;
	}

	private void Start()
	{
		PreInitialization();
		RunStartup();
	}

	private void LateUpdate()
	{
		if (m_unloadUnusedAssetsDelay > 0f)
		{
			m_unloadUnusedAssetsDelay -= Time.unscaledDeltaTime;
		}
		else if (m_unloadUnusedAssets)
		{
			m_unloadUnusedAssets = false;
			m_unloadUnusedAssetsDelay = 1f;
			Resources.UnloadUnusedAssets();
		}
		HearthstonePerformance.Get()?.DoLateUpdate();
	}

	private void PreInitialization()
	{
		PlayMakerPrefs.LogPerformanceWarnings = false;
		TracertReporter.Initialize();
		CustomPlayerLoop.SetupCustomPlayerLoop();
		Log.Initialize();
		string clientConfigPath = PlatformFilePaths.GetClientConfigPath();
		string varSaveConfigPath = PlatformFilePaths.GetSavePathForConfigFile(PlatformFilePaths.ClientConfigName);
		Vars.Initialize(clientConfigPath, varSaveConfigPath);
		PegasusUtils.SetStackTraceLoggingOptions(forceUseMinimumLogging: false);
		ReadCommandLineArgs();
		PlatformSettings.RecomputeDeviceSettings();
		UpdateWorkingDirectory();
		LocalOptions.Get().Initialize();
		HearthstoneLocalization.Initialize();
		DeeplinkService.Get().Initialize();
		LaunchArguments.ReadLaunchArgumentsFromDeeplink();
		Processor.SetLogger(Log.Jobs);
		ApplyInitializationSettingsFromConfig();
		Processor.UseJobQueueAlerts = !IsPublic();
		PreviousInstanceStatus.ReportAppStatus();
		new JobQueueTelemetry(Processor.JobQueue, Processor.JobQueueAlerts, TestType);
		Application.runInBackground = true;
		Application.backgroundLoadingPriority = ThreadPriority.Normal;
		if (PlatformSettings.IsMobile())
		{
			GameStrings.LoadNative();
			UpdateUtils.Initialize();
		}
		else
		{
			GameStrings.LoadAll();
		}
		if (IsPublic())
		{
			Options.Get().SetOption(Option.SOUND, OptionDataTables.s_defaultsMap[Option.SOUND]);
			Options.Get().SetOption(Option.MUSIC, OptionDataTables.s_defaultsMap[Option.MUSIC]);
		}
		BugReporter.Init();
		InitializeGlobalDataContext();
		UberTextInitialization.InitializeUberText();
		UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Splash/PreloadScreen"));
		UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/PegUI"));
		ShowPolicyPopupIfNeeded();
		Processor.QueueJob("HearthstoneApplication.InitializeDataTransferJobs", Job_InitializeDataTransferJobs(), DataTransferDependency);
		Processor.QueueJob("HearthstoneApplication.InitializePostFullLogin", Job_InitializePostFullLogin(), new WaitForFullLoginFlowComplete());
		string deviceModel = SystemInfo.deviceModel;
		if (!string.IsNullOrEmpty(deviceModel) && deviceModel.ToLower().Contains("moto"))
		{
			GeneralUtils.IsDelayedQuit = true;
		}
	}

	private void ShowPolicyPopupIfNeeded()
	{
		if (IsCNMobileBinary && !Options.Get().GetBool(Option.HAS_ACCEPTED_PRIVACY_POLICY_AND_EULA, defaultVal: false))
		{
			LoadResource policyPrefab = new LoadResource("Prefabs/PrivacyPolicyPopup", LoadResourceFlags.AutoInstantiateOnLoad | LoadResourceFlags.FailOnError);
			Processor.QueueJob(HearthstoneJobs.CreateJobFromDependency("HearthstoneApplication.LoadPrivacyPolicyPopup", policyPrefab, typeof(UniversalInputManager)));
		}
		else
		{
			DataTransferDependency.Callback();
		}
	}

	private IEnumerator<IAsyncJobResult> Job_InitializeDataTransferJobs()
	{
		BlizzardAttributionManager.Get().Initialize();
		TelemetryManager.Initialize();
		TracertReporter.SendTelemetry();
		ExceptionReporterControl.Get().ExceptionReportInitialize();
		TelemetryManager.Client().SendSystemDetail(TelemetryUtil.GetUnitySystemInfo());
		HearthstonePerformance.Initialize(TestType, 4078598.ToString(), 217964);
		HearthstonePerformance.Get()?.CaptureAppStartTime();
		AppLaunchTracker.TrackAppLaunch();
		yield return null;
	}

	private IEnumerator<IAsyncJobResult> Job_InitializePostFullLogin()
	{
		Log.All.PrintDebug("[HearthstoneApplication] InitializePostFullLogin started");
		while (!ServiceManager.IsAvailable<LoginManager>() || !ServiceManager.IsAvailable<SceneMgr>())
		{
			yield return null;
		}
		LoginManager loginMgr = LoginManager.Get();
		SceneMgr sceneMgr = SceneMgr.Get();
		while (!loginMgr.IsFullLoginFlowComplete || sceneMgr.IsTransitioning() || sceneMgr.GetMode() == SceneMgr.Mode.INVALID || sceneMgr.GetMode() == SceneMgr.Mode.LOGIN)
		{
			yield return null;
		}
		if (!IsHearthstoneClosing)
		{
			try
			{
				DeepLinkManager.TryExecuteStartupDeepLinkPostLogin();
			}
			catch (Exception exception)
			{
				Log.All.PrintException(exception, "[HearthstoneApplication] DeepLinkManager.TryExecuteStartupDeepLinkPostLogin failed");
			}
		}
		yield return null;
		if (!IsHearthstoneClosing)
		{
			try
			{
				DeepLinkManager.StartListeningForDeepLinks();
			}
			catch (Exception exception2)
			{
				Log.All.PrintException(exception2, "[HearthstoneApplication] DeepLinkManager.StartListeningForDeepLinks failed");
			}
		}
		Log.All.PrintDebug("[HearthstoneApplication] InitializePostFullLogin finished");
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	private void OnApplicationQuit()
	{
		IsHearthstoneClosing = true;
		UberText.StoreCachedData(UberTextCacheFolderPath, UberTextCacheFilePath, 217964, Log.UberText);
		if (this.OnShutdown != null)
		{
			this.OnShutdown();
		}
		PreviousInstanceStatus.ClosedWithoutCrash = true;
		TelemetryManager.Shutdown();
		ServiceManager.Shutdown();
		HearthstonePerformance.Shutdown();
		Resources.UnloadUnusedAssets();
		DeepLinkManager.StopListeningForDeepLinks();
	}

	private void OnApplicationFocus(bool focus)
	{
		if (m_focused != focus && !FatalErrorMgr.Get().IsUnrecoverable)
		{
			m_focused = focus;
			FireFocusChangedEvent();
		}
	}

	public void OnApplicationPause(bool pauseStatus)
	{
		Debug.Log("Application paused: " + pauseStatus);
		ExceptionReporter.Get().OnApplicationPause(pauseStatus);
		if (FatalErrorMgr.Get().IsUnrecoverable || Time.frameCount == 0)
		{
			return;
		}
		if (pauseStatus)
		{
			HearthstonePerformance.Get()?.OnApplicationPause();
			m_lastPauseTime = Time.realtimeSinceStartup;
			IsHearthstoneClosing = true;
			if (this.Paused != null)
			{
				this.Paused();
			}
			PreviousInstanceStatus.ClosedWithoutCrash = true;
			UberText.StoreCachedData(UberTextCacheFolderPath, UberTextCacheFilePath, 217964, Log.UberText);
			Network.ApplicationPaused();
			TelemetryManager.Client().SendAppPaused(pauseStatus: true, 0f);
			TelemetryManager.NetworkComponent.FlushSamplers();
			TelemetryManager.FlushSync();
			return;
		}
		HearthstonePerformance.Get()?.OnApplicationResume();
		m_hasResetSinceLastResume = false;
		IsHearthstoneClosing = false;
		float timePaused = Time.realtimeSinceStartup - m_lastPauseTime;
		TelemetryManager.Client().SendAppPaused(pauseStatus: false, timePaused);
		Debug.Log("Time spent paused: " + timePaused);
		if (ServiceManager.TryGet<DemoMgr>(out var demoMgr) && demoMgr.GetMode() == DemoMode.BLIZZ_MUSEUM && timePaused > 180f)
		{
			ResetImmediately(forceLogin: false);
		}
		m_lastResumeTime = Time.realtimeSinceStartup;
		if (this.Unpaused != null)
		{
			this.Unpaused();
		}
		PreviousInstanceStatus.ClosedWithoutCrash = false;
		Network.ApplicationUnpaused();
		if (ServiceManager.TryGet<SceneMgr>(out var sceneMgr) && sceneMgr.IsModeRequested(SceneMgr.Mode.FATAL_ERROR))
		{
			ResetImmediately(forceLogin: false);
		}
	}

	public (int secSinceResume, int secSpentPaused) GetPauseTimesInSeconds()
	{
		if (m_lastPauseTime == 0f)
		{
			return (secSinceResume: 0, secSpentPaused: 0);
		}
		int item = (int)(Time.time - m_lastResumeTime);
		int secSpentPaused = (int)(m_lastResumeTime - m_lastPauseTime);
		return (secSinceResume: item, secSpentPaused: secSpentPaused);
	}

	public static HearthstoneApplication Get()
	{
		return s_instance;
	}

	public static ApplicationMode GetMode()
	{
		if (!s_initializedMode)
		{
			return ApplicationMode.PUBLIC;
		}
		return s_mode;
	}

	public static bool IsInternal()
	{
		return GetMode() == ApplicationMode.INTERNAL;
	}

	public static bool IsPublic()
	{
		return GetMode() == ApplicationMode.PUBLIC;
	}

	public static bool UseDevWorkarounds()
	{
		return false;
	}

	public static void SendStartupTimeTelemetry(string eventName)
	{
	}

	public static MobileEnv GetMobileEnvironment()
	{
		string mobileMode = Vars.Key("Mobile.Mode").GetStr("undefined");
		if (mobileMode == "undefined")
		{
			mobileMode = "Production";
		}
		if (mobileMode == "Production")
		{
			return MobileEnv.PRODUCTION;
		}
		return MobileEnv.DEVELOPMENT;
	}

	public bool IsResetting()
	{
		return m_resetting;
	}

	public void Reset()
	{
		StartCoroutine(WaitThenReset(forceLogin: false));
	}

	public void ResetAndForceLogin(bool isNewUser = false)
	{
		if (isNewUser)
		{
			Options.Get().SetBool(Option.NEW_USER_LOGIN, val: true);
		}
		StartCoroutine(WaitThenReset(forceLogin: true));
	}

	public bool ResetOnErrorIfNecessary()
	{
		if (!m_hasResetSinceLastResume && Time.realtimeSinceStartup < m_lastResumeTime + 1f)
		{
			StartCoroutine(WaitThenReset(forceLogin: false));
			return true;
		}
		return false;
	}

	public void Exit()
	{
		m_exiting = true;
		if (ExceptionReporter.Get().Busy)
		{
			StartCoroutine(WaitThenExit());
		}
		else
		{
			GeneralUtils.ExitApplication();
		}
	}

	public bool IsExiting()
	{
		return m_exiting;
	}

	public bool HasFocus()
	{
		return m_focused;
	}

	public bool AddFocusChangedListener(FocusChangedCallback callback)
	{
		return AddFocusChangedListener(callback, null);
	}

	public bool AddFocusChangedListener(FocusChangedCallback callback, object userData)
	{
		FocusChangedListener listener = new FocusChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_focusChangedListeners.Contains(listener))
		{
			return false;
		}
		m_focusChangedListeners.Add(listener);
		return true;
	}

	public bool RemoveFocusChangedListener(FocusChangedCallback callback)
	{
		return RemoveFocusChangedListener(callback, null);
	}

	public bool RemoveFocusChangedListener(FocusChangedCallback callback, object userData)
	{
		FocusChangedListener listener = new FocusChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_focusChangedListeners.Remove(listener);
	}

	public float LastResetTime()
	{
		return m_lastResetTime;
	}

	public static bool UsingStandaloneLocalData()
	{
		return !string.IsNullOrEmpty(GetStandaloneLocalDataPath());
	}

	public static bool TryGetStandaloneLocalDataPath(string subPath, out string outPath)
	{
		string localDataPath = GetStandaloneLocalDataPath();
		if (!string.IsNullOrEmpty(localDataPath))
		{
			outPath = Path.Combine(localDataPath, subPath);
			return true;
		}
		outPath = null;
		return false;
	}

	public void UnloadUnusedAssets()
	{
		m_unloadUnusedAssets = true;
	}

	public static void OpenURL(string externalURL)
	{
		Application.OpenURL(externalURL);
	}

	private ServiceLocator RegisterRuntimeServices()
	{
		ServiceLocator runtimeServices = new ServiceLocator("Hearthstone Services", Processor.JobQueue, Log.Services, delegate(Exception exception)
		{
			ExceptionReporter.Get()?.ReportCaughtException(exception);
		});
		runtimeServices.AddServiceLocatorStateChangedListener(delegate(ServiceLocator.ServiceState x)
		{
			OnServiceLocatorStateChanged(x, runtimeServices);
		});
		runtimeServices.RegisterService<IErrorService>(new ErrorService());
		runtimeServices.RegisterService<GameDownloadManager>();
		runtimeServices.RegisterService<Network>();
		runtimeServices.RegisterService<DownloadableDbfCache>();
		runtimeServices.RegisterService<EventTimingManager>();
		runtimeServices.RegisterService<PlayerExperimentManager>();
		runtimeServices.RegisterService<GameMgr>();
		runtimeServices.RegisterService<DraftManager>();
		runtimeServices.RegisterService<AdventureProgressMgr>();
		runtimeServices.RegisterService<AchieveManager>();
		runtimeServices.RegisterService<AchievementManager>();
		runtimeServices.RegisterService<QuestManager>();
		runtimeServices.RegisterService<TavernGuideManager>();
		runtimeServices.RegisterService<RewardTrackManager>();
		runtimeServices.RegisterService<SpecialEventManager>();
		runtimeServices.RegisterService<GenericRewardChestNoticeManager>();
		runtimeServices.RegisterService<AccountLicenseMgr>();
		runtimeServices.RegisterService<FixedRewardsMgr>();
		runtimeServices.RegisterService<ReturningPlayerMgr>();
		runtimeServices.RegisterService<FreeDeckMgr>();
		runtimeServices.RegisterService<DemoMgr>();
		runtimeServices.RegisterService<NetCache>();
		runtimeServices.RegisterService<GameDbf>();
		runtimeServices.RegisterService<DebugConsole>();
		runtimeServices.RegisterService<TavernBrawlManager>();
		runtimeServices.RegisterService<IAssetLoader>(new AssetLoader());
		runtimeServices.RegisterService<IAliasedAssetResolver>(new AliasedAssetResolver());
		runtimeServices.RegisterService<LoginManager>();
		runtimeServices.RegisterService<CardBackManager>();
		runtimeServices.RegisterService<CheatMgr>();
		runtimeServices.RegisterService<Cheats>();
		runtimeServices.RegisterService<ReconnectMgr>();
		runtimeServices.RegisterService<DisconnectMgr>();
		runtimeServices.RegisterService<HealthyGamingMgr>();
		runtimeServices.RegisterService<SoundManager>();
		runtimeServices.RegisterService<MusicManager>();
		runtimeServices.RegisterService<RAFManager>();
		runtimeServices.RegisterService<InactivePlayerKicker>();
		runtimeServices.RegisterService<AdTrackingManager>();
		runtimeServices.RegisterService<SpellManager>();
		runtimeServices.RegisterService<GameplayErrorManager>();
		runtimeServices.RegisterService<IFontTable>(new FontTable(new FontLoader(Log.Font)));
		runtimeServices.RegisterService<UniversalInputManager>();
		runtimeServices.RegisterService<ScreenEffectsMgr>();
		runtimeServices.RegisterService<ShownUIMgr>();
		runtimeServices.RegisterService<PerformanceAnalytics>();
		runtimeServices.RegisterService<PopupDisplayManager>();
		runtimeServices.RegisterService<IGraphicsManager>(new GraphicsManager());
		runtimeServices.RegisterService<MobilePermissionsManager>();
		runtimeServices.RegisterService<ShaderTime>();
		runtimeServices.RegisterService<MobileCallbackManager>(ServiceManager.CreateMonobehaviourService<MobileCallbackManager>(typeof(HSDontDestroyOnLoad)));
		runtimeServices.RegisterService<FullScreenFXMgr>();
		runtimeServices.RegisterService<SceneMgr>();
		runtimeServices.RegisterService<SetRotationManager>();
		runtimeServices.RegisterService<Cinematic>();
		runtimeServices.RegisterService<WidgetRunner>();
		runtimeServices.RegisterService<SpriteAtlasProvider>();
		runtimeServices.RegisterService<IProductDataService>(new ProductDataService());
		runtimeServices.RegisterService<HearthstoneCheckout>();
		runtimeServices.RegisterService<NetworkReachabilityManager>();
		runtimeServices.RegisterService<VersionConfigurationService>();
		runtimeServices.RegisterService<ILoginService>(new LoginService(Log.Login));
		runtimeServices.RegisterService<PartyManager>();
		runtimeServices.RegisterService<PlayerMigrationManager>();
		runtimeServices.RegisterService<CosmeticCoinManager>();
		runtimeServices.RegisterService<QuestToastManager>();
		runtimeServices.RegisterService<RewardXpNotificationManager>();
		runtimeServices.RegisterService<IMaterialService>(new MaterialService(new UnityTimeProvider()));
		runtimeServices.RegisterService<InGameMessageScheduler>();
		runtimeServices.RegisterService<DisposablesCleaner>();
		runtimeServices.RegisterService<PrefabInstanceLoadTracker>();
		runtimeServices.RegisterService<ExternalUrlService>();
		runtimeServices.RegisterService<DiamondRenderToTextureService>();
		runtimeServices.RegisterService<MessagePopupDisplay>();
		runtimeServices.RegisterService<PrivacyGate>();
		runtimeServices.RegisterService<APIGatewayService>(new APIGatewayService(Log.Net));
		runtimeServices.RegisterService<Hearthstone.BreakingNews.BreakingNews>(new Hearthstone.BreakingNews.BreakingNews(Log.BreakingNews, HttpRequestFactory.Get()));
		runtimeServices.RegisterService<IGameStringsService>(new GameStringsService());
		runtimeServices.RegisterService<LuckyDrawManager>();
		runtimeServices.RegisterService<CameraManager>();
		runtimeServices.RegisterService<LegendaryHeroRenderToTextureService>();
		runtimeServices.RegisterService<Benchmarker>();
		runtimeServices.RegisterService<CurrencyManager>();
		runtimeServices.RegisterService<PurchaseManager>();
		runtimeServices.RegisterService<IGamemodeAvailabilityService>(new GamemodeAvailabilityService());
		runtimeServices.RegisterService<BaconLobbyMgr>();
		runtimeServices.RegisterService<MarketingImagesService>();
		runtimeServices.RegisterService<ClientStartupService>();
		runtimeServices.RegisterService<ITouchScreenService>(new W8Touch());
		runtimeServices.RegisterService<AntiCheatManager>();
		return runtimeServices;
	}

	private static void OnServiceLocatorStateChanged(ServiceLocator.ServiceState state, ServiceLocator runtimeServices)
	{
		if (state == ServiceLocator.ServiceState.Ready)
		{
			HearthstonePerformance.Get()?.CaptureAppInitializedTime();
			runtimeServices.RemoveServiceLocatorStateChangedListener(delegate(ServiceLocator.ServiceState x)
			{
				OnServiceLocatorStateChanged(x, runtimeServices);
			});
		}
	}

	private void RunStartup()
	{
		ServiceLocator runtimeServiceLocator = RegisterRuntimeServices();
		ServiceManager.SetDependencies(serviceManagerCallbacks: new ServiceManager.ServiceManagerCallbacks
		{
			RegisterUpdateServices = Processor.RegisterUpdateDelegate,
			UnregisterUpdateServices = Processor.UnregisterUpdateDelegate,
			RegisterLateUpdateServices = Processor.RegisterLateUpdateDelegate,
			UnregisterLateUpdateServices = Processor.UnregisterLateUpdateDelegate
		}, jobQueue: Processor.JobQueue, logger: Log.Services, serviceFactory: HearthstoneServiceFactory.CreateServiceFactory());
		ServiceManager.StartRuntimeServices(runtimeServiceLocator);
		Processor.QueueJob("HearthstoneApplication.CacheCommandLineArgs", Job_CacheCommandLineArgs());
		FatalErrorMgr.Get().RunProcessJob();
		QueueStartupJobs();
	}

	private IEnumerator<IAsyncJobResult> Job_CacheCommandLineArgs()
	{
		string[] cmdLineArgs = CommandLineArgs;
		Debug.LogFormat("Command Line: {0}", string.Join(" ", cmdLineArgs));
		yield break;
	}

	private void QueueStartupJobs()
	{
		WaitForGameDownloadManagerAvailable readyToInitDependency = new WaitForGameDownloadManagerAvailable();
		WaitForGameDownloadManagerState readyToPlayDependency = new WaitForGameDownloadManagerState();
		JobDefinition loadStringsJob = new JobDefinition("GameStrings.LoadAll", GameStrings.Job_LoadAll(), readyToPlayDependency);
		Processor.QueueJob("HearthstoneApplication.InitializeMode", Job_InitializeMode());
		Processor.QueueJob(loadStringsJob);
		Processor.QueueJob("UberText.LoadCachedData", UberText.Job_LoadCachedData(UberTextCacheFolderPath, UberTextCacheFilePath, PlatformFilePaths.PersistentDataPath, 217964, Log.UberText));
		Processor.QueueJob(HearthstoneJobs.CreateJobFromAction("HearthstoneApplication.SetWindowText", SetWindowText, loadStringsJob.CreateDependency()));
		Processor.QueueJob(HearthstoneJobs.CreateJobFromDependency("Load_LogoAnimation", new LoadUIScreen("LogoAnimation.prefab:d2af09653759c2449b0426037b7fe9eb"), typeof(GameDownloadManager), typeof(IAssetLoader)));
		Processor.QueueJob(HearthstoneJobs.CreateJobFromDependency("Load_OverlayUI", new LoadUIScreen("OverlayUI.prefab:af7221edeeba8412cb55e9d6b58bb8dc"), typeof(GameDownloadManager), typeof(IAssetLoader)));
		Processor.QueueJob(HearthstoneJobs.CreateJobFromDependency("Load_SplashScreen", new InstantiatePrefab("SplashScreen.prefab:c9347f27a19520a49af412dad268db15"), typeof(GameDownloadManager), typeof(IAssetLoader), typeof(DemoMgr), new WaitForLogoAnimation(), new WaitForOverlayUI()));
		Processor.QueueJob(HearthstoneJobs.CreateJobFromDependency("Load_BaseUI", new LoadUIScreen("BaseUI.prefab:4d9d926d0cb3bc24380df232133b009b"), readyToInitDependency, typeof(GameDownloadManager), typeof(SceneMgr), typeof(SoundManager), typeof(Network), typeof(ITouchScreenService), typeof(IAssetLoader), typeof(LoginManager)));
		Processor.QueueJob(HearthstoneJobs.CreateJobFromDependency("Load_AdventureConfig", new InstantiatePrefab("AdventureConfig.prefab:6c56645a84199884fbb351611099d9a8"), readyToInitDependency, typeof(GameDownloadManager), typeof(SceneMgr), typeof(AchieveManager), typeof(AdventureProgressMgr), typeof(IAssetLoader), typeof(EventTimingManager)));
		Processor.QueueJob(HearthstoneJobs.CreateJobFromDependency("Load_CardColorSwitcher", new InstantiatePrefab("CardColorSwitcher.prefab:b30c8322821f1524d9e08a59e78e2c85"), readyToInitDependency, typeof(GameDownloadManager), typeof(SceneMgr), typeof(IAssetLoader)));
		Processor.QueueJob("DelayedStartupJobs", Job_StartDelayedStartupJobs());
		Processor.QueueJob("HearthstoneApplication.StartInitialBattleNetConnection", Job_StartInitialBattleNetConnection(), readyToInitDependency, ServiceManager.CreateServiceDependency(typeof(Network)), new WaitForSplashScreen());
	}

	private IEnumerator<IAsyncJobResult> Job_StartDelayedStartupJobs()
	{
		while (!ServiceManager.IsAvailable<IFontTable>())
		{
			yield return null;
		}
		ServiceManager.Get<IFontTable>().StartFontLoadingJob();
	}

	private IEnumerator<IAsyncJobResult> Job_StartInitialBattleNetConnection()
	{
		while (!ServiceManager.IsAvailable<ILoginService>())
		{
			yield return null;
		}
		Log.Net.PrintDebug("Job_StartInitialBattleNetConnection done wait for LoginService");
		while (!ServiceManager.IsAvailable<LoginManager>())
		{
			yield return null;
		}
		Log.Net.PrintDebug("Job_StartInitialBattleNetConnection  done wait for LoginManager");
		if (!Network.ShouldBeConnectedToAurora())
		{
			Log.Net.PrintDebug("Job_StartInitialBattleNetConnection !ShouldBeConnectedToAurora");
			LoginManager.Get().BeginLoginProcess();
		}
		else
		{
			Log.Net.PrintDebug("Job_StartInitialBattleNetConnection done wait for ShouldBeConnectedToAurora");
			Network.StartInitalBattleNetConnection();
			LoginManager.Get().BeginLoginProcess();
		}
	}

	private void InitializeGlobalDataContext()
	{
		DataContext dataContext = GlobalDataContext.Get();
		dataContext.BindDataModel(new DeviceDataModel
		{
			Category = PlatformSettings.OS,
			Mobile = PlatformSettings.IsMobile(),
			Notch = false,
			Screen = PlatformSettings.Screen
		});
		dataContext.BindDataModel(new AccountDataModel
		{
			Language = Localization.GetLocale()
		});
	}

	private static IEnumerator<IAsyncJobResult> Job_InitializeMode()
	{
		if (!s_initializedMode)
		{
			s_mode = ApplicationMode.PUBLIC;
		}
		yield break;
	}

	private IEnumerator WaitThenReset(bool forceLogin)
	{
		m_resetting = true;
		Navigation.Clear();
		yield return new WaitForEndOfFrame();
		ResetImmediately(forceLogin);
	}

	private void ResetImmediately(bool forceLogin)
	{
		if (IsInternal())
		{
			Debug.Log("HearthstoneApplication.ResetImmediately - forceLogin? " + forceLogin + "  Stack trace: " + Environment.StackTrace);
		}
		else
		{
			Debug.Log("HearthstoneApplication.ResetImmediately - forceLogin? " + forceLogin);
		}
		TelemetryManager.Client().SendClientReset(forceLogin, forceNoAccountTutorial: false);
		Processor.JobQueue?.ClearJobs();
		UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Splash/PreloadScreen"));
		if (this.WillReset != null)
		{
			this.WillReset();
		}
		m_resetting = true;
		m_lastResetTime = Time.realtimeSinceStartup;
		ServiceManager.Get<IFontTable>()?.Reset();
		if (DialogManager.Get() != null)
		{
			DialogManager.Get().ClearAllImmediately();
		}
		if (UniversalInputManager.Get() != null)
		{
			UniversalInputManager.Get().SetSystemDialogActive(active: false);
			UniversalInputManager.Get().SetGameDialogActive(active: false);
		}
		if (!Network.CONNECT_TO_AURORA_BY_DEFAULT)
		{
			Network.SetShouldBeConnectedToAurora(forceLogin || Options.Get().GetBool(Option.CONNECT_TO_AURORA));
		}
		FatalErrorMgr.Get().ClearAllErrors();
		m_hasResetSinceLastResume = true;
		if (this.Resetting != null)
		{
			this.Resetting();
		}
		m_resetting = false;
		Debug.Log("\tHearthstoneApplication.ResetImmediately completed");
		Processor.QueueJobIfNotExist("HearthstoneApplication.OnResetCheckForDataPolicy", Job_OnResetCheckForDataPolicy());
	}

	private IEnumerator<IAsyncJobResult> Job_OnResetCheckForDataPolicy()
	{
		if (!IsCNMobileBinary || Options.Get().GetBool(Option.HAS_ACCEPTED_PRIVACY_POLICY_AND_EULA, defaultVal: false))
		{
			OnResetStartupFlow();
			yield break;
		}
		BlizzardAttributionManager.Get().StopCollection();
		TelemetryManager.Stop();
		ExceptionReporterControl.Get().Pause(paused: true);
		WaitForCallback policyAccepted = new WaitForCallback();
		LoadResource loadPopupRequest = new LoadResource("Prefabs/PrivacyPolicyPopup", LoadResourceFlags.AutoInstantiateOnLoad | LoadResourceFlags.FailOnError);
		yield return loadPopupRequest;
		GameObject popupGameObject = (GameObject)loadPopupRequest.LoadedAsset;
		popupGameObject.transform.localScale = new Vector3(2f, 2f, 2f);
		PrivacyPolicyPopup component = popupGameObject.GetComponent<PrivacyPolicyPopup>();
		PrivacyPolicyPopup.Info info = new PrivacyPolicyPopup.Info
		{
			m_callback = delegate(bool accepted)
			{
				if (accepted)
				{
					policyAccepted.Callback();
				}
			}
		};
		component.SetInfo(info);
		yield return policyAccepted;
		UnityEngine.Object.Destroy(popupGameObject);
		BlizzardAttributionManager.Get().ResumeCollection();
		ExceptionReporterControl.Get().Pause(paused: false);
		OnResetStartupFlow();
	}

	private void OnResetStartupFlow()
	{
		FatalErrorMgr.Get().RunProcessJob();
		TelemetryManager.Reset();
		TelemetryManager.SetAppUserContext(TelemetryManager.ProgramId, TelemetryManager.ProgramName, TelemetryManager.ProgramVersion, TelemetryManager.SessionId);
		Navigation.Clear();
		Processor.QueueJob("HearthstoneApplication.OnResetDownloadComplete", Job_OnResetDownloadComplete(), new WaitForGameDownloadManagerAvailable());
	}

	public IEnumerator<IAsyncJobResult> Job_OnResetDownloadComplete()
	{
		if (IsLocaleChanged)
		{
			Log.Downloader.PrintInfo("Wait for DBF download");
			yield return new WaitForDbfBundleReady();
			Log.Downloader.PrintInfo("Reload new locale data");
			AssetManifest.Get().ReadLocaleCatalogs();
			yield return GameDbf.CreateLoadDbfJob();
			AchieveManager.Get().LoadAchievesFromDBF();
			UberText.RebuildAllUberText();
			yield return new WaitForStringsReady();
			GameStrings.ReloadAll();
			InnKeepersSpecial.Get().ResetAdUrl();
			IsLocaleChanged = false;
		}
		else if (new WaitForStringsReady().IsReady())
		{
			GameStrings.ReloadAll();
		}
		else
		{
			GameStrings.LoadNative();
		}
		Processor.QueueJob("UberText.LoadCachedData", UberText.Job_LoadCachedData(UberTextCacheFolderPath, UberTextCacheFilePath, PlatformFilePaths.PersistentDataPath, 217964, Log.UberText));
		if (ServiceManager.TryGet<IFontTable>(out var _))
		{
			ServiceManager.Get<IFontTable>().StartFontLoadingJob();
		}
		yield return HearthstoneJobs.CreateJobFromDependency("Load_LogoAnimation", new InstantiatePrefab("LogoAnimation.prefab:d2af09653759c2449b0426037b7fe9eb"));
		yield return HearthstoneJobs.CreateJobFromDependency("Load_SplashScreen", new InstantiatePrefab("SplashScreen.prefab:c9347f27a19520a49af412dad268db15"));
		Network.Get().ResetForNewAuroraConnection();
		LoginManager.Get().BeginLoginProcess();
	}

	private IEnumerator<IAsyncJobResult> Job_PostDBFLoadOnLocaleChange()
	{
		yield break;
	}

	private IEnumerator WaitThenExit()
	{
		while (ExceptionReporter.Get().Busy)
		{
			yield return null;
		}
		GeneralUtils.ExitApplication();
	}

	private static void ReadCommandLineArgs()
	{
		string[] args = null;
		string argsOverride = Vars.Key("Application.CommandLineOverride").GetStr(null);
		if (argsOverride != null)
		{
			args = argsOverride.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			s_cachedCmdLineArgsAreModified = true;
		}
		if (args == null)
		{
			if (Environment.GetCommandLineArgs() != null)
			{
				args = Environment.GetCommandLineArgs().Skip(1).ToArray();
			}
			else
			{
				args = new string[0];
				Debug.LogFormat("Command Line is null");
			}
		}
		string argsAppend = Vars.Key("Application.CommandLineAppend").GetStr(null);
		if (!string.IsNullOrEmpty(argsAppend))
		{
			List<string> appendedArgs = new List<string>(args);
			appendedArgs.AddRange(argsAppend.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			args = appendedArgs.ToArray();
			s_cachedCmdLineArgsAreModified = true;
		}
		if (s_cachedCmdLineArgsAreModified)
		{
			Debug.LogFormat("Modified Command Line: {0}", string.Join(" ", args));
		}
		s_cachedCmdLineArgs = args;
		ProcessCommandLineArgs();
	}

	private static void ProcessCommandLineArgs()
	{
		for (int i = 0; i < s_cachedCmdLineArgs.Length; i++)
		{
			string[] argAndValue = s_cachedCmdLineArgs[i].Split('=');
			switch (argAndValue[0].ToLower())
			{
			case "confignameoverride":
				if (argAndValue.Length > 1)
				{
					PlatformFilePaths.SetConfigNameOverride(argAndValue[1]);
					Vars.SetOverridePath(argAndValue[1]);
					Vars.RefreshVars();
				}
				break;
			case "optionsnameoverride":
				if (argAndValue.Length > 1)
				{
					PlatformFilePaths.SetOptionsNameOverride(argAndValue[1]);
				}
				break;
			case "logs":
				if (argAndValue.Length > 1)
				{
					LaunchArguments.AddEnabledLogInOptions(argAndValue[1]);
				}
				break;
			}
		}
	}

	public void ApplyInitializationSettingsFromConfig()
	{
		bool @bool = Vars.Key("Jobs.EnableMonitor").GetBool(def: false);
		Processor.SetJobQueueMonitorEnabled(@bool);
		if (@bool)
		{
			string jobQueueDataFilePrefix = Path.GetFileNameWithoutExtension(LogSystem.Get().DefaultFullLogger.GetName());
			string logSessionDirectory = LogSystem.Get().Configuration.SessionConfig.LogSessionDirectory;
			Processor.SetTrackedQueueDataFilePrefix(jobQueueDataFilePrefix);
			Processor.SetTrackedQueueDataDirectory(logSessionDirectory);
		}
		if (!IsInternal())
		{
			return;
		}
		TestType = Vars.Key("Test.TestType").GetStr(string.Empty);
		if (string.IsNullOrEmpty(TestType) || !(TestType == "JobDelay"))
		{
			return;
		}
		if (Processor.JobQueue != null)
		{
			Processor.JobQueue.Debug.DelayTest = true;
			string minJobDelay = Vars.Key("Test.MinDelay").GetStr(string.Empty);
			string maxJobDelay = Vars.Key("Test.MaxDelay").GetStr(string.Empty);
			if (float.TryParse(minJobDelay, out var result))
			{
				Processor.JobQueue.Debug.JobDelayMin = result;
			}
			else if (!string.IsNullOrEmpty(minJobDelay))
			{
				Log.ConfigFile.PrintWarning("Unable to evaluate Minimum  Job Delay value {0}, defaulting to {1}" + minJobDelay, Processor.JobQueue.Debug.JobDelayMin);
			}
			if (float.TryParse(maxJobDelay, out result))
			{
				Processor.JobQueue.Debug.JobDelayMax = result;
			}
			else if (!string.IsNullOrEmpty(maxJobDelay))
			{
				Log.ConfigFile.PrintWarning("Unable to evaluate Maximum Job Delay value {0}, defaulting to {1}" + maxJobDelay, Processor.JobQueue.Debug.JobDelayMax);
			}
		}
		else
		{
			Log.ConfigFile.PrintWarning("Unable to set Test.TestType because JobQueue is null!");
		}
	}

	private static string GetStandaloneLocalDataPath()
	{
		return null;
	}

	private void FireFocusChangedEvent()
	{
		FocusChangedListener[] listeners = m_focusChangedListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(m_focused);
		}
	}

	private void UpdateWorkingDirectory()
	{
		bool workingDirModified = false;
		if (!Application.isEditor && !PlatformSettings.IsMobileRuntimeOS)
		{
			DirectoryInfo dataPath = new DirectoryInfo(Application.dataPath);
			string oldWorkingDir = Directory.GetCurrentDirectory();
			string newWorkingDir = ((!dataPath.Exists) ? string.Empty : ((dataPath.Parent == null) ? dataPath.FullName : dataPath.Parent.FullName));
			if (PlatformSettings.RuntimeOS == OSCategory.Mac && dataPath.Exists && dataPath.Parent != null && dataPath.Parent.Parent != null)
			{
				newWorkingDir = dataPath.Parent.Parent.FullName;
			}
			if (!string.IsNullOrEmpty(newWorkingDir) && !newWorkingDir.Equals(oldWorkingDir, StringComparison.CurrentCultureIgnoreCase))
			{
				workingDirModified = true;
				Directory.SetCurrentDirectory(newWorkingDir);
				Debug.LogFormat("Set current working dir from={0} to={1}", oldWorkingDir, Directory.GetCurrentDirectory());
			}
		}
		if (!workingDirModified)
		{
			Debug.LogFormat("Current working dir={0}", Directory.GetCurrentDirectory());
		}
	}

	private void SetWindowText()
	{
		IntPtr hearthstoneWindow = FindWindow(null, "Hearthstone");
		if (hearthstoneWindow != IntPtr.Zero)
		{
			SetWindowTextW(hearthstoneWindow, GameStrings.Get("GLOBAL_PROGRAMNAME_HEARTHSTONE"));
		}
	}

	[DllImport("user32.dll")]
	public static extern int SetWindowTextW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string text);

	[DllImport("user32.dll")]
	public static extern IntPtr FindWindow(string className, string windowName);
}
