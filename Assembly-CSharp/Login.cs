using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Login;
using Hearthstone.Progression;
using Hearthstone.Streaming;
using HutongGames.PlayMaker;
using PegasusShared;
using UnityEngine;

public class Login : PegasusScene
{
	private int m_nextMissionId;

	private ExistingAccountPopup m_existingAccountPopup;

	private static Login s_instance;

	private bool m_blockingBnetBar;

	protected override void Awake()
	{
		s_instance = this;
		base.Awake();
		if (LoginManager.Get() != null && SplashScreen.Get() != null)
		{
			Processor.QueueJob("Login.GoToNextMode", GoToNextMode(), LoginManager.Get().ReadyToGoToNextModeDependency);
			JobDefinition loginQueueJob = Processor.QueueJob("Splashscreen.ShowLoginQueue", SplashScreen.Get().Job_ShowLoginQueue());
			Processor.QueueJob("Login.OnLoginStateResolved", OnLoginStateResolved(), LoginManager.Get().ReadyToReconnectOrChangeModeDependency, loginQueueJob.CreateDependency());
		}
	}

	private void Start()
	{
		SceneMgr.Get().NotifySceneLoaded();
	}

	private void OnDestroy()
	{
		if (s_instance == this)
		{
			s_instance = null;
		}
	}

	private void Update()
	{
		if (Network.Get() != null)
		{
			Network.Get().ProcessNetwork();
		}
	}

	public static Login Get()
	{
		return s_instance;
	}

	public override void Unload()
	{
		GameMgr.Get().UnregisterFindGameEvent(OnFindGameEvent);
		SetBlockingBnetBar(blocked: false);
	}

	private IEnumerator<IAsyncJobResult> OnLoginStateResolved()
	{
		if (!Network.ShouldBeConnectedToAurora())
		{
			Log.Login.PrintDebug("Login.cs: OnLoginStateResolved Show (I'm New / Login) popup");
			bool isCN = PlatformSettings.LocaleVariant == LocaleVariant.China;
			DialogManager.Get().ShowExistingAccountPopup(OnExistingAccountPopupResponse, OnExistingAccountLoadedCallback, isCN);
		}
		else
		{
			Log.Login.PrintDebug("Login.cs: OnLoginStateResolved Show box animation");
			JobDefinition reconnectOrChangeModeJob = new JobDefinition("Login.ReconnectOrChangeMode", ReconnectOrChangeMode());
			Processor.QueueJob("SplashScreen.Hide", SplashScreen.Get().Hide(reconnectOrChangeModeJob));
		}
		yield break;
	}

	private IEnumerator<IAsyncJobResult> ReconnectOrChangeMode()
	{
		HearthstoneApplication.SendStartupTimeTelemetry("Login.ReconnectOrChangeMode");
		if (BaseUI.Get() != null)
		{
			BaseUI.Get().OnLoggedIn();
		}
		if (Cheats.Get().IsLaunchingQuickGame() || !LoginManager.Get().AttemptToReconnectToGame(OnReconnectTimeout))
		{
			ChangeMode();
		}
		yield break;
	}

	private void ChangeMode()
	{
		m_nextMissionId = GameUtils.GetNextTutorial();
		bool hearthstoneTutorialInProgress = m_nextMissionId > 5287;
		if (!GameUtils.IsTraditionalTutorialComplete() || hearthstoneTutorialInProgress)
		{
			ChangeMode_Tutorial();
			return;
		}
		if (RewardTrackManager.Get().HasReceivedRewardTracksFromServer)
		{
			Box.Get().PlayBoxMusic();
		}
		else
		{
			Box.Get().OnBoxDressingReadyOnce += Box.Get().PlayBoxMusic;
		}
		if (SetRotationManager.Get().ShouldShowSetRotationIntro())
		{
			if (!CreateSkipHelper.ShouldShowCreateSkip() || !CreateSkipHelper.ShowCreateSkipDialog(ChangeToAppropriateHubMode))
			{
				ChangeToAppropriateHubMode();
				ChangeMode_SetRotation();
			}
		}
		else
		{
			ChangeMode_Hub();
		}
	}

	private void ChangeToAppropriateHubMode()
	{
		Log.Login.PrintInfo("Changing mode");
		if (SetRotationManager.Get().ShouldShowSetRotationIntro())
		{
			ChangeMode_SetRotation();
		}
		else
		{
			ChangeMode_Hub();
		}
	}

	private bool OnReconnectTimeout(object userData)
	{
		ChangeMode();
		return true;
	}

	private void ChangeMode_Hub()
	{
		SetBlockingBnetBar(blocked: true);
		ServiceManager.Get<LoginManager>().OnFullLoginFlowComplete += delegate
		{
			SetBlockingBnetBar(blocked: false);
		};
		if (Options.Get().GetBool(Option.HAS_SEEN_HUB, defaultVal: false))
		{
			PlayInnkeeperIntroVO();
		}
		Spell eventSpell = Box.Get().GetEventSpell(BoxEventType.STARTUP_HUB);
		eventSpell.AddFinishedCallback(OnStartupHubSpellFinished);
		eventSpell.Activate();
	}

	private void SetBlockingBnetBar(bool blocked)
	{
		if (blocked != m_blockingBnetBar)
		{
			m_blockingBnetBar = blocked;
			if (blocked)
			{
				BaseUI.Get().m_BnetBar.RequestDisableButtons();
			}
			else
			{
				BaseUI.Get().m_BnetBar.CancelRequestToDisableButtons();
			}
		}
	}

	private void PlayInnkeeperIntroVO()
	{
		if (!ReturningPlayerMgr.Get().PlayReturningPlayerInnkeeperGreetingIfNeeded())
		{
			if (RewardTrackManager.Get().HasReceivedRewardTracksFromServer && GameDownloadManagerProvider.Get().IsReadyToPlay)
			{
				Box.Get().PlayInnkeeperGreetings();
			}
			else
			{
				Box.Get().OnBoxDressingReadyOnce += Box.Get().PlayInnkeeperGreetings;
			}
		}
	}

	private IEnumerator<IAsyncJobResult> GoToNextMode()
	{
		if (m_nextMissionId == 0 || m_nextMissionId <= 5287)
		{
			SceneMgr.Mode nextMode = SceneMgr.Mode.INVALID;
			if (!DeterminePostLoginScene(ref nextMode))
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
			}
		}
		yield break;
	}

	private bool DeterminePostLoginScene(ref SceneMgr.Mode nextScene)
	{
		foreach (KeyValuePair<StartupSceneSource, DetermineStartupSceneCallback> kv in new SortedList<StartupSceneSource, DetermineStartupSceneCallback>(LoginManager.GetPostLoginCallbacks()))
		{
			if (kv.Key == StartupSceneSource.DEFAULT_NORMAL_STARTUP)
			{
				break;
			}
			DetermineStartupSceneCallback value = kv.Value;
			nextScene = SceneMgr.Mode.INVALID;
			if (value(ref nextScene))
			{
				return true;
			}
		}
		return false;
	}

	private void ChangeMode_Tutorial()
	{
		if (m_nextMissionId == 5287)
		{
			StartTutorial();
		}
		else
		{
			ShowTutorialProgressScreen();
		}
	}

	private void ShowTutorialProgressScreen()
	{
		AssetLoader.Get().InstantiatePrefab("TutorialInterstitialPopup.prefab:a5140e1dc7b29634cb548f42574bce5b", OnTutorialProgressScreenCallback, null, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private void OnTutorialProgressScreenCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		TutorialProgressScreen component = go.GetComponent<TutorialProgressScreen>();
		component.SetCoinPressCallback(StartTutorial);
		component.StartTutorialProgress();
	}

	private void OnExistingAccountPopupResponse(bool hasAccount)
	{
		m_existingAccountPopup.gameObject.SetActive(value: false);
		HearthstoneApplication.Get().ResetAndForceLogin(!hasAccount);
	}

	private void StartTutorial()
	{
		MusicManager.Get().StopPlaylist();
		Box.Get().ChangeState(Box.State.CLOSED);
		GameMgr.Get().RegisterFindGameEvent(OnFindGameEvent);
		GameMgr.Get().FindGame(GameType.GT_TUTORIAL, FormatType.FT_WILD, m_nextMissionId, 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
	}

	private bool OnExistingAccountLoadedCallback(DialogBase dialog, object userData)
	{
		m_existingAccountPopup = (ExistingAccountPopup)dialog;
		m_existingAccountPopup.gameObject.SetActive(value: true);
		return true;
	}

	private void ChangeMode_SetRotation()
	{
		UserAttentionManager.StartBlocking(UserAttentionBlocker.SET_ROTATION_INTRO);
		Spell spell = Box.Get().GetEventSpell(BoxEventType.STARTUP_SET_ROTATION);
		Box.Get().m_StoreButton.gameObject.SetActive(value: false);
		Box.Get().m_QuestLogButton.gameObject.SetActive(value: false);
		if (PlatformSettings.IsMobile())
		{
			PlayMakerFSM fsm = spell.gameObject.GetComponent<PlayMakerFSM>();
			if (fsm == null)
			{
				Debug.LogError("Missing FSM on Startup_Hub");
			}
			else
			{
				FsmFloat duration = fsm.FsmVariables.GetFsmFloat("PanDuration");
				FsmFloat fsmFloat = fsm.FsmVariables.GetFsmFloat("PanStartTime");
				duration.Value = 3f;
				fsmFloat.Value = 2f;
			}
		}
		spell.AddFinishedCallback(OnSetRotationSpellFinished);
		spell.Activate();
	}

	private void OnSetRotationSpellFinished(Spell spell, object userData)
	{
		Processor.QueueJob("Login.GoToNextMode", GoToNextMode());
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		if (eventData.m_state == FindGameState.SERVER_GAME_STARTED && !GameMgr.Get().IsNextReconnect())
		{
			Spell eventSpell = Box.Get().GetEventSpell(BoxEventType.TUTORIAL_PLAY);
			eventSpell.AddStateFinishedCallback(OnTutorialPlaySpellStateBirthFinished);
			eventSpell.ActivateState(SpellStateType.BIRTH);
			return true;
		}
		return false;
	}

	private void OnTutorialPlaySpellStateBirthFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		SpellStateType activeState = spell.GetActiveState();
		if (prevStateType == SpellStateType.BIRTH)
		{
			LoadingScreen.Get().SetFadeColor(Color.white);
			LoadingScreen.Get().EnableFadeOut(enable: false);
			LoadingScreen.Get().AddTransitionObject(Box.Get().gameObject);
			LoadingScreen.Get().AddTransitionBlocker();
			SceneMgr.Get().RegisterSceneLoadedEvent(OnMissionSceneLoaded);
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.GAMEPLAY);
		}
		else if (activeState == SpellStateType.NONE)
		{
			LoadingScreen.Get().NotifyTransitionBlockerComplete();
		}
	}

	private void OnMissionSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		SceneMgr.Get().UnregisterSceneLoadedEvent(OnMissionSceneLoaded);
		Box.Get().GetEventSpell(BoxEventType.TUTORIAL_PLAY).ActivateState(SpellStateType.ACTION);
	}

	private void OnStartupHubSpellFinished(Spell spell, object userData)
	{
		Log.Login.PrintDebug("Login.cs: OnStartupHubSpellFinished");
		HearthstoneApplication.SendStartupTimeTelemetry("Login.OnStartupHubSpellFinished");
		if (Network.ShouldBeConnectedToAurora() && m_nextMissionId <= 5287)
		{
			JobDefinition waitForIntroPopups = Processor.QueueJob("LoginManager.ShowIntroPopups", LoginManager.Get().ShowIntroPopups());
			IJobDependency[] dependencies = new IJobDependency[1] { waitForIntroPopups.CreateDependency() };
			Log.Login.PrintDebug("Login.cs: OnStartupHubSpellFinished CompleteLoginFlow");
			Processor.QueueJob("LoginManager.CompleteLoginFlow", LoginManager.Get().CompleteLoginFlow(), dependencies);
		}
	}
}
