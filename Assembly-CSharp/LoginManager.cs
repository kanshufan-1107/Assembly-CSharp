using System;
using System.Collections.Generic;
using System.Globalization;
using Assets;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.APIGateway;
using Hearthstone.Attribution;
using Hearthstone.BreakingNews;
using Hearthstone.Core;
using Hearthstone.Devices;
using Hearthstone.InGameMessage;
using Hearthstone.Login;
using Hearthstone.Streaming;
using Hearthstone.Util;
using PegasusUtil;
using UnityEngine;

public class LoginManager : IService
{
	private WaitForCallback UpdateLoginCompleteDependency;

	private WaitForCallback SetProgressDependency;

	private static SortedList<StartupSceneSource, DetermineStartupSceneCallback> s_determinePostLoginCallbacks = new SortedList<StartupSceneSource, DetermineStartupSceneCallback>();

	private JobDefinition WaitForLogin;

	private BreakingNews m_breakingNews;

	public WaitForCallback LoggedInDependency;

	public WaitForCallback ReadyToGoToNextModeDependency;

	public WaitForCallback ReadyToReconnectOrChangeModeDependency;

	public WaitForCallback InitialClientStateReceivedDependency;

	public WaitForCallback LoginScreenNetCacheReceivedDependency;

	public WaitForCallback OptInsReceivedDependency;

	public OptInApi OptInApi { get; private set; }

	public Network.QueueInfo CurrentQueueInfo { get; private set; }

	public bool IsFullLoginFlowComplete { get; private set; }

	public event Action OnLoginCompleted;

	public event Action OnAchievesLoaded;

	public event Action OnInitialClientStateReceived;

	public event Action OnFullLoginFlowComplete;

	public event Action<Network.QueueInfo> OnQueueModifiedEvent;

	public event Action OnLoginStarted;

	public event Action OnEnterNoAccountFlow;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		LoggedInDependency = new WaitForCallback();
		ReadyToGoToNextModeDependency = new WaitForCallback();
		ReadyToReconnectOrChangeModeDependency = new WaitForCallback();
		InitialClientStateReceivedDependency = new WaitForCallback();
		LoginScreenNetCacheReceivedDependency = new WaitForCallback();
		OptInsReceivedDependency = new WaitForCallback();
		UpdateLoginCompleteDependency = new WaitForCallback();
		SetProgressDependency = new WaitForCallback();
		m_breakingNews = serviceLocator.Get<BreakingNews>();
		Network.Get().RegisterNetHandler(InitialClientState.PacketID.ID, OnInitialClientStateResponse);
		Network.Get().RegisterNetHandler(SetProgressResponse.PacketID.ID, OnSetProgressResponse);
		Network.Get().RegisterNetHandler(UpdateLoginComplete.PacketID.ID, UpdateLoginCompleteDependency.Callback.Invoke);
		OnInitialClientStateReceived += InitializeManagers;
		OnAchievesLoaded += UpdateTutorialPresence;
		HearthstoneApplication.Get().Resetting += OnReset;
		CurrentQueueInfo = null;
		if (!Vars.Key("Aurora.ClientCheck").GetBool(def: true) || !BattleNetClient.needsToRun)
		{
			Network.Get().RegisterQueueInfoHandler(QueueInfoHandler);
		}
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[7]
		{
			typeof(Network),
			typeof(GameDownloadManager),
			typeof(NetCache),
			typeof(ILoginService),
			typeof(SceneMgr),
			typeof(AchieveManager),
			typeof(BreakingNews)
		};
	}

	public void Shutdown()
	{
		if (ServiceManager.TryGet<NetCache>(out var netCache))
		{
			netCache.UnregisterNetCacheHandler(OnNetCacheReady);
		}
		if (ServiceManager.TryGet<Network>(out var net))
		{
			net.RemoveNetHandler(InitialClientState.PacketID.ID, OnInitialClientStateResponse);
			net.RemoveNetHandler(SetProgressResponse.PacketID.ID, OnSetProgressResponse);
			net.RemoveNetHandler(UpdateLoginComplete.PacketID.ID, UpdateLoginCompleteDependency.Callback.Invoke);
		}
	}

	private void OnReset()
	{
		WaitForLogin = null;
		InitializeForNewLogin();
	}

	public static LoginManager Get()
	{
		return ServiceManager.Get<LoginManager>();
	}

	public static SortedList<StartupSceneSource, DetermineStartupSceneCallback> GetPostLoginCallbacks()
	{
		return s_determinePostLoginCallbacks;
	}

	public void BeginLoginProcess()
	{
		InitializeForNewLogin();
		if (!Network.ShouldBeConnectedToAurora())
		{
			Log.Login.Print("Entering No Account flow.");
			DefLoader.Get().Initialize();
			this.OnEnterNoAccountFlow?.Invoke();
			ReadyToReconnectOrChangeModeDependency.Callback();
		}
		else if (WaitForLogin == null)
		{
			Log.Login.Print("Entering Login flow.");
			ServiceManager.Get<ILoginService>().StartLogin();
			Network.Get().OnLoginStarted();
			this.OnLoginStarted?.Invoke();
			WaitForLogin = new JobDefinition("LoginManager.WaitForLogin", Job_WaitForLogin(), new WaitForGameDownloadManagerAvailable());
			Processor.QueueJob(WaitForLogin);
			HearthstoneApplication.SendStartupTimeTelemetry("LoginManager.BeginLoginProcess");
		}
	}

	private void InitializeForNewLogin()
	{
		UpdateLoginCompleteDependency.Reset();
		SetProgressDependency.Reset();
		ReadyToGoToNextModeDependency.Reset();
		ReadyToReconnectOrChangeModeDependency.Reset();
		LoggedInDependency.Reset();
		InitialClientStateReceivedDependency.Reset();
		LoginScreenNetCacheReceivedDependency.Reset();
		OptInsReceivedDependency.Reset();
		IsFullLoginFlowComplete = false;
	}

	private IEnumerator<IAsyncJobResult> Job_WaitForLogin()
	{
		Log.Login.PrintDebug("LoginManager started Job_WaitForLogin");
		while (true)
		{
			ConnectionState bnetStatus = Network.BattleNetStatus();
			if (bnetStatus == ConnectionState.Ready && BattleNet.GetAccountCountry() != null && BattleNet.GetAccountRegion() != BnetRegion.REGION_UNINITIALIZED)
			{
				WaitForLogin = null;
				Log.TemporaryAccount.Print("Is Temporary Account: " + (BattleNet.IsHeadlessAccount() ? "Yes" : "No"));
				OnLoginComplete();
				yield break;
			}
			if (bnetStatus == ConnectionState.Error || bnetStatus == ConnectionState.Disconnected)
			{
				break;
			}
			yield return null;
		}
		WaitForLogin = null;
		Network.Get().ShowConnectionFailureError("GLOBAL_ERROR_NETWORK_LOGIN_FAILURE");
	}

	private IEnumerator<IAsyncJobResult> Job_WaitForStartupPacketSequenceComplete()
	{
		HearthstoneApplication.SendStartupTimeTelemetry("LoginManager.WaitForStartupPacketSequenceComplete");
		Network.Get().OnStartupPacketSequenceComplete();
		NetCache.Get().RegisterScreenLogin(OnNetCacheReady);
		yield break;
	}

	private void InitializeManagers()
	{
		if (Network.IsLoggedIn())
		{
			TelemetryManager.RebuildContext();
			BnetPresenceMgr.Get().Initialize();
			BnetFriendMgr.Get().Initialize();
			BnetWhisperMgr.Get().Initialize();
			BnetRecentPlayerMgr.Get().Initialize();
			BnetNearbyPlayerMgr.Get().Initialize();
			FriendChallengeMgr.Get().OnLoggedIn();
			SpectatorManager.Get().InitializeConnectedToBnet();
			NarrativeManager.Get().Initialize();
			BlizzardAttributionManager.Get().OnConnected();
			if (!Options.Get().GetBool(Option.CONNECT_TO_AURORA))
			{
				Options.Get().SetBool(Option.CONNECT_TO_AURORA, val: true);
			}
			if ((PlatformSettings.IsMobile() || PlatformSettings.IsSteam) && Options.Get().GetInt(Option.PREFERRED_REGION) != (int)DeviceLocaleHelper.GetCurrentRegionId())
			{
				Options.Get().SetInt(Option.PREFERRED_REGION, (int)DeviceLocaleHelper.GetCurrentRegionId());
			}
			if (Options.Get().GetBool(Option.CREATED_ACCOUNT))
			{
				AdTrackingManager.Get().TrackAccountCreated();
				if (PlatformSettings.IsMobile())
				{
					AchieveManager.Get().NotifyOfAccountCreation();
				}
				Options.Get().DeleteOption(Option.CREATED_ACCOUNT);
			}
		}
		if (!RegionUtils.IsCNLegalRegion)
		{
			RAFManager.Get().InitializeRequests();
		}
		Tournament.Init();
		PresenceMgr.Get().SetStatus(Global.PresenceStatus.LOGIN);
		TemporaryAccountManager.Get().Initialize();
		if (PlatformSettings.IsMobile())
		{
			AdTrackingManager.Get().TrackLogin();
		}
		SceneMgr.Get().LoadShaderPreCompiler();
	}

	private void OnProfileProgressResponse()
	{
		HearthstoneApplication.SendStartupTimeTelemetry("LoginManager.OnProfileProgressResponse");
		Cinematic cine2;
		if (!Options.Get().GetBool(Option.HAS_SEEN_NEW_CINEMATIC, defaultVal: false) && PlatformSettings.OS == OSCategory.PC)
		{
			if (ServiceManager.TryGet<Cinematic>(out var cine))
			{
				cine.Play(delegate
				{
					ReadyToReconnectOrChangeModeDependency.Callback();
				});
			}
		}
		else if (!ServiceManager.TryGet<Cinematic>(out cine2) || !cine2.IsPlaying)
		{
			ReadyToReconnectOrChangeModeDependency.Callback();
		}
	}

	private void UpdateTutorialPresence()
	{
		BnetPresenceMgr.Get().SetGameField(15u, GameUtils.IsTraditionalTutorialComplete() ? 1 : 0);
		BnetPresenceMgr.Get().SetGameField(28u, GameUtils.IsBattleGroundsTutorialComplete() ? 1 : 0);
		BnetPresenceMgr.Get().SetGameField(29u, GameUtils.IsMercenariesVillageTutorialComplete() ? 1 : 0);
	}

	private void OnNetCacheReady()
	{
		Log.Login.Print("LoginManager: Net Cache Ready");
		NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady);
		LoginScreenNetCacheReceivedDependency.Callback();
		HearthstoneApplication.SendStartupTimeTelemetry("LoginManager.OnNetCacheReady");
		Processor.QueueJob("LoginManager.WaitForAchievesThenInit", Job_WaitForAchievesThenInit(), ServiceManager.CreateServiceDependency(typeof(SceneMgr)), new WaitForBox());
		BaconTelemetry.SendBattlegroundsCollectionResultLogin();
	}

	private void OnInitialClientStateResponse()
	{
		Log.Login.Print("LoginManager: Assets Version Check Completed");
		InitialClientStateReceivedDependency.Callback();
		HearthstoneApplication.SendStartupTimeTelemetry("LoginManager.OnInitialClientStateResponse");
		if (this.OnInitialClientStateReceived != null)
		{
			this.OnInitialClientStateReceived();
		}
		if (Box.Get() != null)
		{
			Box.Get().OnLoggedIn();
		}
		BaseUI.Get().OnLoggedIn();
		InactivePlayerKicker.Get().OnLoggedIn();
		HealthyGamingMgr.Get().OnLoggedIn();
		GameMgr.Get().OnLoggedIn();
		DraftManager.Get().OnLoggedIn();
		AccountLicenseMgr.Get().InitRequests();
		AdventureProgressMgr.InitRequests();
		Network network = Network.Get();
		if (Network.IsLoggedIn())
		{
			TutorialProgress progress = Options.Get().GetEnum<TutorialProgress>(Option.LOCAL_TUTORIAL_PROGRESS);
			if (progress > TutorialProgress.NOTHING_COMPLETE)
			{
				network.SetProgress((long)progress);
			}
			else
			{
				SetProgressDependency.Callback();
			}
		}
		network.ResetConnectionFailureCount();
		network.DoLoginUpdate();
		Processor.QueueJob("LoginManager.WaitForStartupPacketSequenceComplete", Job_WaitForStartupPacketSequenceComplete(), SetProgressDependency, UpdateLoginCompleteDependency);
	}

	private void OnSetProgressResponse()
	{
		HearthstoneApplication.SendStartupTimeTelemetry("LoginManager.OnSetProgressResponse");
		SetProgressResponse response = Network.Get().GetSetProgressResponse();
		SetProgressResponse.Result result_ = response.Result_;
		if (result_ == SetProgressResponse.Result.SUCCESS || result_ == SetProgressResponse.Result.ALREADY_DONE)
		{
			Options.Get().DeleteOption(Option.LOCAL_TUTORIAL_PROGRESS);
		}
		else
		{
			Debug.LogWarning($"LoginManager.OnSetProgressResponse(): received unexpected result {response.Result_}");
		}
		SetProgressDependency.Callback();
	}

	private void OnLoginComplete()
	{
		LoggedInDependency.Callback();
		Log.Login.Print("LoginManager: OnLoginComplete");
		HearthstoneApplication.SendStartupTimeTelemetry("LoginManager.OnLoginComplete");
		RegionUtils.ResetRegion();
		if (PlatformSettings.IsMobile())
		{
			Processor.QueueJob("SetupMobilePushRegistration", Job_SetupMobilePushRegistration());
		}
		string userId = string.Format(CultureInfo.InvariantCulture, "{0:D}", BattleNet.GetMyAccoundId().Low);
		BlizzardAttributionManager.Get().SetCustomerUserId(userId);
		DefLoader.Get().Initialize();
		CollectionManager.Init();
		Log.Login.Print("LoginManager: CollectionManager is initialized.");
		InnKeepersSpecial.Get().InitializeURLAndUpdate();
		InGameMessageScheduler.Get()?.OnLoginCompleted();
		ExceptionReporterControl.Get().OnLoginCompleted();
		PushNotificationManager.Get()?.OnLoginComplete();
		Processor.QueueJob("LoginManager.SetupOptIns", Job_SetupOptIns(), ServiceManager.CreateServiceDependency(typeof(APIGatewayService)));
		StoreManager.Get().Init();
		Network network = Network.Get();
		network.LoginOk();
		network.MercenariesPlayerInfoRequest();
		network.UpdateCachedBnetValues();
		Log.Login.Print("LoginManager: Requesting assets version and initial client state.");
		network.RequestCollectionClientState();
		NetCache.Get().RegisterScreenStartup(OnProfileProgressResponse);
		this.OnLoginCompleted?.Invoke();
	}

	private IEnumerator<IAsyncJobResult> Job_SetupMobilePushRegistration()
	{
		GenerateSSOToken tassadarAuthenticationToken = new GenerateSSOToken();
		yield return tassadarAuthenticationToken;
		while (!tassadarAuthenticationToken.HasToken)
		{
			yield return null;
		}
		Log.Login.Print("LoginManager: Setting up mobile push registration...");
		TelemetryManager.SetAccountUserContext(tassadarAuthenticationToken.Token, BattleNet.GetMyAccoundId().Low, ExternalUrlService.GetRegionString(), Localization.GetLocaleName());
	}

	private IEnumerator<IAsyncJobResult> Job_SetupOptIns()
	{
		APIGatewayService apiGatewayService = ServiceManager.Get<APIGatewayService>();
		apiGatewayService.OnLoginComplete();
		OptInApi = new OptInApi(apiGatewayService, Log.BattleNet);
		OptInApi.Init(OptInsReceivedDependency.Callback);
		yield return null;
	}

	private IEnumerator<IAsyncJobResult> Job_WaitForAchievesThenInit()
	{
		while (DownloadableDbfCache.Get().IsRequiredClientStaticAssetsStillPending)
		{
			yield return null;
		}
		while (!AdventureProgressMgr.Get().IsReady)
		{
			yield return null;
		}
		FixedRewardsMgr.Get().InitStartupFixedRewards();
		if (this.OnAchievesLoaded != null)
		{
			this.OnAchievesLoaded();
		}
		Log.Login.Print("LoginManager: Achieves Loaded");
		Log.Downloader.Print("LOADING PROCESS COMPLETE at " + Time.realtimeSinceStartup);
	}

	public IEnumerator<IAsyncJobResult> ShowIntroPopups()
	{
		Log.Login.Print("LoginManager: Showing Intro Popups");
		if (GameDownloadManagerProvider.Get().IsReadyToPlay)
		{
			Log.Login.PrintInfo("Initial download done, Waiting Intro popup...");
			yield return new JobDefinition("DialogManager.WaitForSeasonEndPopup", DialogManager.Get().Job_WaitForSeasonEndPopup());
			yield return new JobDefinition("PopupDisplayManager.WaitForAllPopups", PopupDisplayManager.Get().Job_WaitForAllPopups());
			yield return new JobDefinition("NarrativeManager.WaitForOutstandingCharacterDialog", NarrativeManager.Get().Job_WaitForOutstandingCharacterDialog());
			yield return new JobDefinition("LoginManager.ShowBreakingNews", Job_ShowBreakingNews());
		}
		else
		{
			Log.Login.PrintInfo("Initial download not done, skipping Intro Popups...");
			UserAttentionManager.StartBlocking(UserAttentionBlocker.INITIAL_DOWNLOAD);
		}
	}

	public bool AttemptToReconnectToGame(ReconnectMgr.GameTimeoutCallback gameTimeoutCallback)
	{
		if (GameMgr.Get().ConnectToGameIfHaveDeferredConnectionPacket())
		{
			return true;
		}
		if (ReconnectMgr.Get().ReconnectToGameFromLogin())
		{
			ReconnectMgr.Get().AddGameTimeoutListener(gameTimeoutCallback);
			return true;
		}
		return false;
	}

	private IEnumerator<IAsyncJobResult> Job_ShowBreakingNews()
	{
		if (m_breakingNews.ShouldShowForCurrentPlatform || Cheats.ShowFakeBreakingNews)
		{
			WaitForCallback waitForBreakingNews = new WaitForCallback();
			ShowBreakingNews(waitForBreakingNews.Callback);
			yield return waitForBreakingNews;
		}
	}

	public IEnumerator<IAsyncJobResult> CompleteLoginFlow()
	{
		IsFullLoginFlowComplete = true;
		if (SceneMgr.Get().IsModeRequested(SceneMgr.Mode.LOGIN))
		{
			if (this.OnFullLoginFlowComplete != null)
			{
				this.OnFullLoginFlowComplete();
			}
			Log.Login.Print("LoginManager: Complete Login Flow");
			ReadyToGoToNextModeDependency.Callback();
		}
		yield break;
	}

	private void ShowBreakingNews(Action callback)
	{
		if (m_breakingNews.GetStatus() == BreakingNews.Status.Available || Cheats.ShowFakeBreakingNews)
		{
			string text = m_breakingNews.GetText();
			if (string.IsNullOrEmpty(text) && Cheats.ShowFakeBreakingNews)
			{
				text = "FAKE BREAKING NEWS ARE BREAKING NOW";
				UIStatus.Get().AddInfo("SHOWING FAKE BREAKING NEWS!\nTo disable this, remove ShowFakeBreakingNews from client.config", 5f);
			}
			if (!string.IsNullOrEmpty(text))
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
				info.m_headerText = GameStrings.Get("GLUE_MOBILE_SPLASH_SCREEN_BREAKING_NEWS");
				info.m_text = text;
				info.m_showAlertIcon = true;
				info.m_richTextEnabled = false;
				info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
				info.m_responseCallback = delegate
				{
					callback();
				};
				DialogManager.Get().ShowPopup(info);
			}
			else
			{
				callback();
			}
		}
		else
		{
			if (!Application.isEditor)
			{
				Debug.LogWarning("Breaking News response is taking too long!");
			}
			callback();
		}
	}

	private void QueueInfoHandler(Network.QueueInfo queueInfo)
	{
		CurrentQueueInfo = queueInfo;
		if (this.OnQueueModifiedEvent != null)
		{
			this.OnQueueModifiedEvent(queueInfo);
		}
	}

	public void RegisterQueueModifiedListener(Action<Network.QueueInfo> listener)
	{
		OnQueueModifiedEvent -= listener;
		OnQueueModifiedEvent += listener;
	}

	public void RemoveQueueModifiedListener(Action<Network.QueueInfo> listener)
	{
		OnQueueModifiedEvent -= listener;
	}
}
