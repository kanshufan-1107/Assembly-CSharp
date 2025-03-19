using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.GameService.Protocol.V2.Client;
using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using PegasusLettuce;
using PegasusShared;
using UnityEngine;

public class LettuceMapDisplay : AbsSceneDisplay
{
	private enum CurrentResultState
	{
		NEW_MAP,
		LOST_MAP,
		WON_NODE,
		WON_MAP
	}

	private const float DEFAULT_MAP_SCROLL_TIME = 0.5f;

	private const float INTRO_MAP_SCROLL_TIME = 2f;

	private const float VICTORY_MAP_SCROLL_TIME = 2f;

	private const float CHEST_ANIMATION_TIME = 3f;

	private const float DELAY_BEFORE_MAP_SCROLL = 0.25f;

	private const float DELAY_AFTER_MAP_SCROLL = 0.25f;

	private const float COIN_FLIP_TIME = 1f;

	private const float FULLSCREEN_FX_TIME = 0.25f;

	private const float DELAY_BEFORE_SHOWING_CAMPFIRE = 1f;

	public static readonly AssetReference REWARD_PREFAB = new AssetReference("MercenariesRewardScroll.prefab:b8b2a8f8d472c5945aafd50c39464e4c");

	public static readonly AssetReference VISITOR_FALLBACK_REWARD_PREFAB = new AssetReference("MercenariesVisitorFallbackRewardScroll.prefab:726d3a412ad3d0b46a66a18f2289e41d");

	public static readonly AssetReference VISITOR_TASK_REWARD_PREFAB = new AssetReference("MercenariesNewTaskRewardScroll.prefab:24bc75ecdd51b344daa0830c9041207b");

	public AsyncReference m_PlayButtonReference;

	public AsyncReference m_PlayButtonPhoneReference;

	public AsyncReference m_BackButtonReference;

	public AsyncReference m_BackButtonPhoneReference;

	public AsyncReference m_LettuceMapReference;

	public AsyncReference m_TeamPreviewReference;

	public AsyncReference m_TeamPreviewPhoneReference;

	public AsyncReference m_RetireButtonReference;

	public AsyncReference m_RetireButtonPhoneReference;

	public AsyncReference m_FinalBossChestReference;

	public AsyncReference m_MapMaskableReference;

	public AsyncReference m_EndOfRunReference;

	public AsyncReference m_EndOfRunBackButtonReference;

	public AsyncReference m_TreasureTeamViewReference;

	public UIBScrollable m_Scrollable;

	private PlayButton m_playButton;

	private LettuceMap m_lettuceMap;

	private VisualController m_teamPreviewVisualController;

	private VisualController m_finalBossChestVisualController;

	private VisualController m_endOfRunVisualController;

	private VisualController m_treasureTeamViewVisualController;

	private Widget m_treasureSelectWidget;

	private Maskable m_mapMaskable;

	private int m_selectedTreasureChoices = -1;

	private int m_numTreasuresChosen;

	private List<int> m_selectedTreasureIndex;

	private int m_selectedVisitorIndex = -1;

	private bool m_playButtonFinishedLoading;

	private bool m_backButtonFinishedLoading;

	private bool m_lettuceMapFinishedLoading;

	private bool m_lettuceMapFinishedChangingStates;

	private bool m_teamPreviewFinishedLoading;

	private bool m_retireButtonFinishedLoading;

	private bool m_finalBossChestFinishedLoading;

	private bool m_endOfRunFinishedLoading;

	private bool m_treasureTeamViewLoading;

	private bool m_mapMaskableFinishedLoading;

	private bool m_lettuceMapDataInitialized;

	private bool m_loadingScreenTransitionFromGameplayStarted;

	private bool m_loadingScreenTransitionFromGameplayComplete;

	private PegasusLettuce.LettuceMap m_lettuceMapProto;

	private LettuceMapCoinDataModel m_selectedMapCoin;

	private RewardPresenter m_rewardPresenter = new RewardPresenter
	{
		m_rewardPrefab = REWARD_PREFAB
	};

	private bool m_isNewMap;

	private bool m_waitingForTreasureSelection;

	private bool m_waitingForVisitorSelectionServerResponse;

	private bool m_waitingForVisualControllerState;

	private List<DefLoader.DisposableCardDef> m_disposables = new List<DefLoader.DisposableCardDef>();

	private bool m_isTeamViewVisible;

	private bool m_taskBoardIsOpen;

	private CurrentResultState m_currentMapResult;

	private bool m_currentMapIsComplete;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private bool m_isMythicMode;

	private bool LettuceVillageVisitorDataInitialized => NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageInfo>()?.Initialized ?? false;

	public override void Start()
	{
		base.Start();
		CollectionManager.Get().StartInitialMercenaryLoadIfRequired();
		m_sceneDisplayWidgetReference.RegisterReadyListener<VisualController>(OnLettuceMapDisplayReady);
		m_LettuceMapReference.RegisterReadyListener<VisualController>(OnLettuceMapReady);
		m_FinalBossChestReference.RegisterReadyListener<VisualController>(OnFinalBossChestReady);
		m_MapMaskableReference.RegisterReadyListener<Maskable>(OnMapMaskableReady);
		m_EndOfRunReference.RegisterReadyListener<VisualController>(OnEndOfRunReady);
		m_EndOfRunBackButtonReference.RegisterReadyListener<VisualController>(OnEndOfRunBackButtonReady);
		m_TreasureTeamViewReference.RegisterReadyListener<VisualController>(OnTreasureTeamViewReady);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_TeamPreviewPhoneReference.RegisterReadyListener<VisualController>(OnTeamPreviewReady);
			m_PlayButtonPhoneReference.RegisterReadyListener<VisualController>(OnPlayButtonReady);
			m_BackButtonPhoneReference.RegisterReadyListener<VisualController>(OnBackButtonReady);
			m_RetireButtonPhoneReference.RegisterReadyListener<VisualController>(OnRetireButtonReady);
		}
		else
		{
			m_TeamPreviewReference.RegisterReadyListener<VisualController>(OnTeamPreviewReady);
			m_PlayButtonReference.RegisterReadyListener<VisualController>(OnPlayButtonReady);
			m_BackButtonReference.RegisterReadyListener<VisualController>(OnBackButtonReady);
			m_RetireButtonReference.RegisterReadyListener<VisualController>(OnRetireButtonReady);
		}
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheLettuceMap), OnLettuceMapReceived);
		Network.Get().RegisterNetHandler(LettuceMapChooseNodeResponse.PacketID.ID, OnLettuceMapChooseNodeResponseReceived);
		Network.Get().RegisterNetHandler(LettuceMapRetireResponse.PacketID.ID, OnLettuceMapRetireResponseReceived);
		Network.Get().RegisterNetHandler(MercenariesMapTreasureSelectionResponse.PacketID.ID, OnMercenariesMapTreasureSelectionResponseReceived);
		Network.Get().RegisterNetHandler(MercenariesMapVisitorSelectionResponse.PacketID.ID, OnVisitorSelectionResponseReceived);
		PartyManager.Get().AddChangedListener(OnPartyChanged);
		PartyManager.Get().AddPartyAttributeChangedListener(OnPartyAttributeChanged);
		if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY)
		{
			LoadingScreen.Get().RegisterFinishedTransitionListener(OnTransitionFromGameplayFinished);
			LoadingScreen.Get().OnFadeInStart += OnLoadingScreenFadeInStarted;
		}
		else
		{
			m_loadingScreenTransitionFromGameplayStarted = true;
			m_loadingScreenTransitionFromGameplayComplete = true;
		}
		PegUI.Get().RegisterForRenderPassPriorityHitTest(this);
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_MercenariesSubMenus);
		if (!GameUtils.IsMercenariesVillageTutorialComplete())
		{
			NarrativeManager.Get().PreloadMercenaryTutorialDialogue();
		}
		StartCoroutine(InitializeWhenReady());
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	public void OnDestroy()
	{
		if (NetCache.Get() != null)
		{
			NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheLettuceMap), OnLettuceMapReceived);
		}
		if (Network.Get() != null)
		{
			Network.Get().RemoveNetHandler(LettuceMapChooseNodeResponse.PacketID.ID, OnLettuceMapChooseNodeResponseReceived);
			Network.Get().RemoveNetHandler(LettuceMapRetireResponse.PacketID.ID, OnLettuceMapRetireResponseReceived);
			Network.Get().RemoveNetHandler(MercenariesMapTreasureSelectionResponse.PacketID.ID, OnMercenariesMapTreasureSelectionResponseReceived);
			Network.Get().RemoveNetHandler(MercenariesMapVisitorSelectionResponse.PacketID.ID, OnVisitorSelectionResponseReceived);
		}
		if (PartyManager.Get() != null)
		{
			PartyManager.Get().RemoveChangedListener(OnPartyChanged);
			PartyManager.Get().RemovePartyAttributeChangedListener(OnPartyAttributeChanged);
		}
		if (LoadingScreen.Get() != null)
		{
			LoadingScreen.Get().UnregisterFinishedTransitionListener(OnTransitionFromGameplayFinished);
			LoadingScreen.Get().OnFadeInStart -= OnLoadingScreenFadeInStarted;
		}
		if (PegUI.Get() != null)
		{
			PegUI.Get().UnregisterFromRenderPassPriorityHitTest(this);
		}
		m_disposables.DisposeValuesAndClear();
	}

	public override bool IsFinishedLoading(out string failureMessage)
	{
		if (!AsyncAssetsFinishedLoading())
		{
			failureMessage = "LettuceMapDisplay - Widget references never loaded.";
			return false;
		}
		if (!m_lettuceMapFinishedChangingStates)
		{
			failureMessage = "LettuceMapDisplay - Map never finished changing states.";
			return false;
		}
		if (!m_lettuceMapDataInitialized)
		{
			failureMessage = "LettuceMapDisplay - Map data was never initialized.";
			return false;
		}
		if (!LettuceVillageVisitorDataInitialized)
		{
			failureMessage = "LettuceMapDisplay - Village visitor data was never initialized.";
			return false;
		}
		if (m_lettuceMap == null || !m_lettuceMap.IsFinishedLoading())
		{
			failureMessage = "LettuceMapDisplay - Map never finished loading.";
			return false;
		}
		if (!CollectionManager.Get().IsLettuceLoaded())
		{
			failureMessage = "LettuceMapDisplay - Lettuce Collection Manager never loaded.";
			return false;
		}
		failureMessage = string.Empty;
		return true;
	}

	public bool AsyncAssetsFinishedLoading()
	{
		if (m_playButtonFinishedLoading && m_backButtonFinishedLoading && m_lettuceMapFinishedLoading && m_teamPreviewFinishedLoading && m_retireButtonFinishedLoading && m_finalBossChestFinishedLoading && m_mapMaskableFinishedLoading && m_endOfRunFinishedLoading)
		{
			return m_treasureTeamViewLoading;
		}
		return false;
	}

	public override bool IsBlockingPopupDisplayManager()
	{
		if (m_loadingScreenTransitionFromGameplayComplete && !m_waitingForTreasureSelection && !m_waitingForVisualControllerState && !m_rewardPresenter.IsShowingReward() && !IsCurrentBountyTutorial())
		{
			if (m_lettuceMapProto != null)
			{
				return !m_lettuceMapProto.Active;
			}
			return false;
		}
		return true;
	}

	public static LettuceMapNodeTypeAnomaly.MercenariesBonusRewardType GetBonusRewardTypeForCardId(int cardId)
	{
		return GameDbf.LettuceMapNodeTypeAnomaly.GetRecord((LettuceMapNodeTypeAnomalyDbfRecord r) => r.AnomalyCard == cardId)?.BonusRewardType ?? LettuceMapNodeTypeAnomaly.MercenariesBonusRewardType.NONE;
	}

	private void OnPlayButtonRelease(UIEvent e)
	{
		ExecutePlayLogic();
	}

	private void OnEndOfRunButtonRelease(UIEvent e)
	{
		m_screenEffectsHandle.StopEffect();
		if (ShouldShowTaskboard())
		{
			StartCoroutine(ShowAndWaitForTaskBoard(exitWhenComplete: true, e));
		}
		else
		{
			OnBackButtonRelease(e);
		}
	}

	private void OnBackButtonRelease(UIEvent e)
	{
		SceneMgr.Mode nextMode = SceneMgr.Mode.LETTUCE_VILLAGE;
		if (IsCurrentBountyTutorial())
		{
			if (m_currentMapIsComplete)
			{
				LettuceVillageDisplay.LettuceSceneTransitionPayload transitionPayload = new LettuceVillageDisplay.LettuceSceneTransitionPayload();
				transitionPayload.m_SelectedBountySet = (transitionPayload.m_SelectedBounty = GameDbf.LettuceBounty.GetRecord((int)m_lettuceMapProto.BountyId)).BountySetRecord;
				m_sceneTransitionPayload = transitionPayload;
				BackOutOfScene(SceneMgr.Mode.LETTUCE_VILLAGE);
			}
			else
			{
				BackOutOfScene(SceneMgr.Mode.GAME_MODE);
			}
			return;
		}
		if (m_currentMapIsComplete)
		{
			if (PartyManager.Get().IsInMercenariesCoOpParty())
			{
				PartyManager.Get().LeaveParty();
				BackOutOfScene(SceneMgr.Mode.LETTUCE_VILLAGE);
				return;
			}
			nextMode = SceneMgr.Mode.LETTUCE_BOUNTY_BOARD;
			LettuceVillageDisplay.LettuceSceneTransitionPayload transitionPayload2 = new LettuceVillageDisplay.LettuceSceneTransitionPayload();
			LettuceBountyDbfRecord bountyRecord = (transitionPayload2.m_SelectedBounty = GameDbf.LettuceBounty.GetRecord((int)m_lettuceMapProto.BountyId));
			transitionPayload2.m_SelectedBountySet = bountyRecord.BountySetRecord;
			transitionPayload2.m_DifficultyMode = bountyRecord.DifficultyMode;
			m_sceneTransitionPayload = transitionPayload2;
		}
		else if (PartyManager.Get().IsInMercenariesCoOpParty())
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = "Leave Party?";
			info.m_text = "Would you like to leave the party and end the run?";
			info.m_iconSet = AlertPopup.PopupInfo.IconSet.Default;
			info.m_showAlertIcon = true;
			info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			info.m_confirmText = "Leave";
			info.m_cancelText = "Stay";
			info.m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CONFIRM)
				{
					PartyManager.Get().LeaveParty();
					BackOutOfScene(SceneMgr.Mode.LETTUCE_VILLAGE);
				}
			};
			DialogManager.Get().ShowPopup(info);
			return;
		}
		BackOutOfScene(nextMode);
	}

	private void BackOutOfScene(SceneMgr.Mode nextMode)
	{
		if (NetCache.Get() != null)
		{
			NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheLettuceMap), OnLettuceMapReceived);
		}
		if (nextMode == SceneMgr.Mode.HUB)
		{
			SceneMgr.Get().SetNextMode(nextMode);
			return;
		}
		SceneMgr.TransitionHandlerType handler = SceneMgr.TransitionHandlerType.NEXT_SCENE;
		if (nextMode == SceneMgr.Mode.LETTUCE_VILLAGE)
		{
			handler = SceneMgr.TransitionHandlerType.CURRENT_SCENE;
		}
		SetNextModeAndHandleTransition(nextMode, handler, m_sceneTransitionPayload);
	}

	private void OnRetireButtonRelease(UIEvent e)
	{
		if (!Network.IsLoggedIn())
		{
			DialogManager.Get().ShowReconnectHelperDialog();
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_LETTUCE_MAP_RETIRE_DIALOG_HEADER");
		info.m_text = GameStrings.Get("GLUE_LETTUCE_MAP_RETIRE_DIALOG_BODY");
		info.m_iconSet = AlertPopup.PopupInfo.IconSet.Default;
		info.m_showAlertIcon = false;
		info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_confirmText = GameStrings.Get("GLUE_LETTUCE_MAP_RETIRE_DIALOG_CONFIRM");
		info.m_cancelText = GameStrings.Get("GLUE_LETTUCE_MAP_RETIRE_DIALOG_CANCEL");
		info.m_responseCallback = delegate(AlertPopup.Response response, object userData)
		{
			if (response == AlertPopup.Response.CONFIRM)
			{
				m_clickBlocker.SetActive(value: true);
				Network.Get().RetireLettuceMap();
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	private void LettuceMapEventListener(string eventName)
	{
		switch (eventName)
		{
		case "LETTUCE_COIN_RELEASED":
			OnCoinSelected();
			break;
		case "TREASURE_SELECTED":
			OnTreasureSelected();
			break;
		case "TREASURE_CHOSEN":
			OnTreasureChosen();
			break;
		case "VISITOR_SELECTED":
			OnVisitorSelected();
			break;
		case "VISITOR_CHOSEN":
			OnVisitorChosen();
			break;
		case "SHOW_TEAM_code":
			OnTeamViewShow();
			break;
		case "HIDE_TEAM_code":
			OnTeamViewHide();
			break;
		case "VISUAL_CONTROLLER_STATE_COMPLETE":
			m_waitingForVisualControllerState = false;
			break;
		}
	}

	private LettuceMapDisplayDataModel GetDisplayDataModel()
	{
		VisualController visualController = GetComponent<VisualController>();
		if (visualController == null)
		{
			return null;
		}
		Widget owner = visualController.Owner;
		if (!owner.GetDataModel(201, out var dataModel))
		{
			dataModel = new LettuceMapDisplayDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as LettuceMapDisplayDataModel;
	}

	public void OnPlayButtonReady(VisualController buttonVisualController)
	{
		if (buttonVisualController == null)
		{
			Error.AddDevWarning("UI Error!", "PlayButton could not be found! You will not be able to click 'Play'!");
			return;
		}
		m_playButton = buttonVisualController.gameObject.GetComponent<PlayButton>();
		m_playButton.AddEventListener(UIEventType.RELEASE, OnPlayButtonRelease);
		m_playButton.Disable();
		m_playButtonFinishedLoading = true;
	}

	public void OnBackButtonReady(VisualController buttonVisualController)
	{
		if (buttonVisualController == null)
		{
			Error.AddDevWarning("UI Error!", "BackButton could not be found! You will not be able to click 'Back'!");
			return;
		}
		buttonVisualController.gameObject.GetComponent<UIBButton>().AddEventListener(UIEventType.RELEASE, OnBackButtonRelease);
		m_backButtonFinishedLoading = true;
	}

	public void OnEndOfRunBackButtonReady(VisualController buttonVisualController)
	{
		if (buttonVisualController == null)
		{
			Error.AddDevWarning("UI Error!", "End of Run BackButton could not be found! You will not be able to click 'Back'!");
		}
		else
		{
			buttonVisualController.gameObject.GetComponent<UIBButton>().AddEventListener(UIEventType.RELEASE, OnEndOfRunButtonRelease);
		}
	}

	private void OnLettuceMapDisplayReady(VisualController lettuceMapDisplayController)
	{
		if (lettuceMapDisplayController == null)
		{
			Error.AddDevWarning("UI Error!", "LettuceMapDisplay could not be found!");
		}
		else
		{
			lettuceMapDisplayController.GetComponent<Widget>().RegisterEventListener(LettuceMapEventListener);
		}
	}

	private void OnLettuceMapReady(VisualController lettuceMapController)
	{
		if (lettuceMapController == null)
		{
			Error.AddDevWarning("UI Error!", "LettuceMap could not be found!");
			return;
		}
		m_lettuceMap = lettuceMapController.GetComponent<LettuceMap>();
		m_lettuceMapFinishedLoading = true;
		Widget component = lettuceMapController.GetComponent<Widget>();
		component.RegisterEventListener(LettuceMapEventListener);
		component.RegisterDoneChangingStatesListener(OnMapWidgetDoneChangingStates);
	}

	private void OnMapWidgetDoneChangingStates(object widget)
	{
		m_lettuceMapFinishedChangingStates = true;
	}

	public void OnTeamPreviewReady(VisualController previewController)
	{
		if (previewController == null)
		{
			Error.AddDevWarning("UI Error!", "TeamPreview could not be found!");
			return;
		}
		m_teamPreviewVisualController = previewController;
		m_teamPreviewFinishedLoading = true;
	}

	public void OnEndOfRunReady(VisualController previewController)
	{
		if (previewController == null)
		{
			Error.AddDevWarning("UI Error!", "EndOfRun could not be found!");
			return;
		}
		m_endOfRunVisualController = previewController;
		m_endOfRunFinishedLoading = true;
	}

	public void OnTreasureTeamViewReady(VisualController previewController)
	{
		if (previewController == null)
		{
			Error.AddDevWarning("UI Error!", "TreasureTeamVioew could not be found!");
			return;
		}
		m_treasureTeamViewVisualController = previewController;
		m_treasureTeamViewLoading = true;
	}

	public void OnRetireButtonReady(VisualController buttonVisualController)
	{
		if (buttonVisualController == null)
		{
			Error.AddDevWarning("UI Error!", "RetireButton could not be found! You will not be able to click 'Retire'!");
			return;
		}
		buttonVisualController.gameObject.GetComponent<UIBButton>().AddEventListener(UIEventType.RELEASE, OnRetireButtonRelease);
		m_retireButtonFinishedLoading = true;
	}

	public void OnFinalBossChestReady(VisualController visualController)
	{
		if (visualController == null)
		{
			Error.AddDevWarning("UI Error!", "FinalBossChest could not be found!");
			return;
		}
		m_finalBossChestVisualController = visualController;
		m_finalBossChestFinishedLoading = true;
	}

	public void OnMapMaskableReady(Maskable maskable)
	{
		m_mapMaskable = maskable;
		m_mapMaskableFinishedLoading = true;
	}

	public EventDataModel GetLettuceMapEventDataModel()
	{
		Widget mapWidget = m_lettuceMap.GetComponent<Widget>();
		if (mapWidget == null)
		{
			return null;
		}
		return mapWidget.GetDataModel<EventDataModel>();
	}

	private LettuceTeamDataModel GetTeamPreviewDataModel()
	{
		if (m_teamPreviewVisualController == null)
		{
			return null;
		}
		Widget owner = m_teamPreviewVisualController.Owner;
		if (!owner.GetDataModel(217, out var dataModel))
		{
			dataModel = new LettuceTeamDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as LettuceTeamDataModel;
	}

	private LettuceBountyBoardDataModel GetBountyBoardDataModel()
	{
		if (m_endOfRunVisualController == null)
		{
			return null;
		}
		Widget owner = m_endOfRunVisualController.Owner;
		if (!owner.GetDataModel(194, out var dataModel))
		{
			dataModel = new LettuceBountyBoardDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as LettuceBountyBoardDataModel;
	}

	private LettuceBountyDataModel GetBountyDataModel()
	{
		if (m_endOfRunVisualController == null)
		{
			return null;
		}
		Widget owner = m_endOfRunVisualController.Owner;
		if (!owner.GetDataModel(193, out var dataModel))
		{
			dataModel = new LettuceBountyDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as LettuceBountyDataModel;
	}

	protected override bool ShouldStartShown()
	{
		return SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LETTUCE_VILLAGE;
	}

	private IEnumerator InitializeWhenReady()
	{
		m_clickBlocker.SetActive(value: true);
		while (!AsyncAssetsFinishedLoading())
		{
			yield return null;
		}
		LettuceVillageDataUtil.InitializeData();
		InitializeLettuceMapData();
		while (!m_lettuceMapDataInitialized || !LettuceVillageVisitorDataInitialized)
		{
			yield return null;
		}
		if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY)
		{
			CheckAndEnqueueMercenaryGrant();
		}
		PopulateTeamPreviewData(GetCurrentPlayerData());
		PartyManager.Get().SetSceneAttribute(SceneMgr.Get().GetMode().ToString());
		while (SceneMgr.Get().IsTransitioning())
		{
			yield return null;
		}
		while ((GameToastMgr.Get()?.AreToastsActive() ?? false) || (QuestToastManager.Get()?.AreToastsActive() ?? false))
		{
			yield return null;
		}
		if (ShouldShowTreasureSelection(m_lettuceMapProto))
		{
			m_treasureTeamViewVisualController.BindDataModel(GetTeamPreviewDataModel());
			m_treasureTeamViewVisualController.BindDataModel(GetDisplayDataModel());
			while (!m_loadingScreenTransitionFromGameplayStarted)
			{
				yield return null;
			}
			m_waitingForTreasureSelection = true;
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			screenEffectParameters.Time = 0.25f;
			m_screenEffectsHandle.StartEffect(screenEffectParameters);
			bool tutorialDone = false;
			LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.MAP_PRE_TREASURE_SELECTION, base.gameObject, 0, (int)m_lettuceMapProto.BountyId, delegate
			{
				tutorialDone = true;
			});
			while (!tutorialDone)
			{
				yield return null;
			}
			InitializeTreasureSelectionData(m_lettuceMapProto);
			while (m_waitingForTreasureSelection)
			{
				yield return null;
			}
		}
		else
		{
			SetSelectedTreasureChoices(-1);
		}
		yield return TryShowingVisitorSelection(m_lettuceMapProto);
		NetCache.NetCacheMercenariesVillageVisitorInfo visitorInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageVisitorInfo>();
		List<MercenariesTaskState> completedTasks = null;
		if (visitorInfo != null && visitorInfo.CompletedTasks != null && visitorInfo.CompletedTasks.Count > 0)
		{
			completedTasks = visitorInfo.CompletedTasks.ToList();
			visitorInfo.CompletedTasks.Clear();
			yield return LettuceVillageDataUtil.ShowTaskToast(completedTasks);
		}
		if (m_lettuceMapProto.WonLastCombatWithNoLivingMercenaries)
		{
			while (!m_loadingScreenTransitionFromGameplayComplete)
			{
				yield return null;
			}
			bool waitingForRunEndedDialog = true;
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_LETTUCE_MAP_WIPEOUT_DIALOG_HEADER");
			info.m_text = GameStrings.Get("GLUE_LETTUCE_MAP_WIPEOUT_DIALOG_BODY");
			info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info.m_okText = GameStrings.Get("GLOBAL_OKAY");
			info.m_responseCallback = delegate
			{
				waitingForRunEndedDialog = false;
			};
			DialogManager.Get().ShowPopup(info);
			while (waitingForRunEndedDialog)
			{
				yield return null;
			}
		}
		m_endOfRunVisualController.BindDataModel(GetTeamPreviewDataModel());
		m_endOfRunVisualController.BindDataModel(GetDisplayDataModel());
		bool waitingForRewardPopupToClose = false;
		if (m_currentMapResult != CurrentResultState.WON_MAP && PopupDisplayManager.Get().RewardPopups.HasNonAutoRetireMercenariesRewardsToShow())
		{
			while (!m_loadingScreenTransitionFromGameplayStarted)
			{
				yield return null;
			}
			ScreenEffectParameters screenEffectParameters2 = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			screenEffectParameters2.Time = 0.25f;
			m_screenEffectsHandle.StartEffect(screenEffectParameters2);
			NetCache.ProfileNoticeMercenariesRewards rewardNotice = PopupDisplayManager.Get().RewardPopups.GetNextNonAutoRetireRewardMercenariesRewardToShow();
			NetCache.ProfileNoticeMercenariesRewards bonusRewardNotice = PopupDisplayManager.Get().RewardPopups.GetNextBonusMercenariesRewardToShow();
			waitingForRewardPopupToClose = true;
			PopupDisplayManager.Get().RewardPopups.ShowMercenariesRewards(autoOpenChest: true, rewardNotice, bonusRewardNotice, delegate
			{
				m_screenEffectsHandle.StopEffect();
				waitingForRewardPopupToClose = false;
			});
		}
		while (!m_loadingScreenTransitionFromGameplayComplete)
		{
			yield return null;
		}
		switch (m_currentMapResult)
		{
		case CurrentResultState.NEW_MAP:
			yield return PlayIntroMapScroll();
			break;
		case CurrentResultState.WON_MAP:
			yield return PlayVictoryMapScroll();
			break;
		default:
			yield return PlayDefaultMapScroll();
			break;
		}
		while (waitingForRewardPopupToClose)
		{
			yield return null;
		}
		if (ShouldShowTaskboard() && completedTasks != null && m_currentMapResult != CurrentResultState.WON_MAP)
		{
			completedTasks.Sort(delegate(MercenariesTaskState a, MercenariesTaskState b)
			{
				VisitorTaskDbfRecord taskRecordByID = LettuceVillageDataUtil.GetTaskRecordByID(a.TaskId);
				VisitorTaskDbfRecord taskRecordByID2 = LettuceVillageDataUtil.GetTaskRecordByID(b.TaskId);
				MercenaryVisitorDbfRecord visitorRecordByID = LettuceVillageDataUtil.GetVisitorRecordByID(taskRecordByID?.MercenaryVisitorId ?? 0);
				MercenaryVisitorDbfRecord visitorRecordByID2 = LettuceVillageDataUtil.GetVisitorRecordByID(taskRecordByID2?.MercenaryVisitorId ?? 0);
				return (visitorRecordByID == null || visitorRecordByID2 == null) ? (visitorRecordByID == null).CompareTo(visitorRecordByID2 == null) : (visitorRecordByID2.VisitorType == MercenaryVisitor.VillageVisitorType.STANDARD).CompareTo(visitorRecordByID.VisitorType == MercenaryVisitor.VillageVisitorType.STANDARD);
			});
			int visitorId = 0;
			if (completedTasks.Count > 0 && completedTasks[0] != null)
			{
				VisitorTaskDbfRecord mainCompletedTaskVisitor = LettuceVillageDataUtil.GetTaskRecordByID(completedTasks[0].TaskId);
				if (mainCompletedTaskVisitor != null)
				{
					visitorId = mainCompletedTaskVisitor.MercenaryVisitorId;
				}
			}
			yield return ShowAndWaitForTaskBoard(exitWhenComplete: false, null, visitorId);
		}
		if (m_currentMapResult == CurrentResultState.WON_MAP || m_currentMapResult == CurrentResultState.WON_NODE)
		{
			yield return CheckLastCompletedNodeTutorialEvents();
			yield return CheckForNodeDialogueEvents();
		}
		m_clickBlocker.SetActive(value: false);
		if (m_currentMapIsComplete)
		{
			HandleEndOfRun();
		}
	}

	private void HandleEndOfRun()
	{
		if (m_currentMapResult == CurrentResultState.WON_MAP)
		{
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			screenEffectParameters.Time = 0.25f;
			m_screenEffectsHandle.StartEffect(screenEffectParameters);
			LettuceBountyBoardDataModel bountyBoardDataModel = GetBountyBoardDataModel();
			LettuceBountyDbfRecord lettuceBountyRecord = GameDbf.LettuceBounty.GetRecord((int)m_lettuceMapProto.BountyId);
			bountyBoardDataModel.BossName = (LettuceVillageDataUtil.TryGetBountyBossData(lettuceBountyRecord, out var bossName, out var _) ? bossName : string.Empty);
			LettuceBountyDataModel bountyDataModel = GetBountyDataModel();
			InitializeBountyDataModel(bountyDataModel, lettuceBountyRecord);
			m_endOfRunVisualController.SetState("SHOW");
		}
		else
		{
			OnBackButtonRelease(null);
		}
	}

	private bool ShouldShowTaskboard()
	{
		if ((SceneMgr.Get()?.GetPrevMode() ?? SceneMgr.Mode.INVALID) == SceneMgr.Mode.GAMEPLAY)
		{
			return LettuceVillageDataUtil.GetNotificationStatusForBuilding(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD);
		}
		return false;
	}

	private void OnTaskboardClosed(LettuceVillagePopupManager.PopupType popupType)
	{
		if (popupType == LettuceVillagePopupManager.PopupType.TASKBOARD)
		{
			m_taskBoardIsOpen = false;
			LettuceVillagePopupManager pupManager = LettuceVillagePopupManager.Get();
			if (pupManager != null)
			{
				pupManager.OnPopupClosed = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Remove(pupManager.OnPopupClosed, new Action<LettuceVillagePopupManager.PopupType>(OnTaskboardClosed));
			}
			LettuceVillageDataUtil.MarkNotificationAsSeenForBuilding(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD);
		}
	}

	private IEnumerator ShowAndWaitForTaskBoard(bool exitWhenComplete, UIEvent e, int focusedVisitorStateId = 0)
	{
		yield return new WaitForSeconds(1f);
		LettuceVillagePopupManager pupManager = LettuceVillagePopupManager.Get();
		if (pupManager != null)
		{
			m_taskBoardIsOpen = true;
			pupManager.OnPopupClosed = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Combine(pupManager.OnPopupClosed, new Action<LettuceVillagePopupManager.PopupType>(OnTaskboardClosed));
			pupManager.FocusedVisitorId = focusedVisitorStateId;
			pupManager.Show(LettuceVillagePopupManager.PopupType.TASKBOARD);
			while (m_taskBoardIsOpen)
			{
				yield return null;
			}
		}
		if (exitWhenComplete)
		{
			OnBackButtonRelease(e);
		}
	}

	private void InitializeLettuceMapData()
	{
		int selectedBountyRecord = 0;
		long selectedTeamId = 0L;
		long coopPartnerTeamId = 0L;
		int mythicLevel = -1;
		if (m_sceneTransitionPayload != null)
		{
			LettuceBountyDbfRecord bountyRecord = ((LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload).m_SelectedBounty;
			if (bountyRecord != null && bountyRecord.DifficultyMode == LettuceBounty.MercenariesBountyDifficulty.MYTHIC)
			{
				mythicLevel = ((LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload).m_MythicLevel;
			}
			selectedBountyRecord = bountyRecord?.ID ?? 0;
			selectedTeamId = ((LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload).m_TeamId;
			coopPartnerTeamId = ((LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload).m_CoOpPartnerTeamId;
		}
		if (PartyManager.Get().IsInMercenariesCoOpParty() && !PartyManager.Get().IsPartyLeader())
		{
			BnetGameAccountId leaderGameAccountId = PartyManager.Get().GetPartyLeaderGameAccountId();
			if (leaderGameAccountId == null)
			{
				Log.Lettuce.PrintError("InitializeLettuceMapData - No party leader for co-op map!");
			}
			else
			{
				Network.Get().RequestLettuceMap((uint)selectedBountyRecord, null, leaderGameAccountId, mythicLevel);
			}
			return;
		}
		NetCache.NetCacheLettuceMap cachedLettuceMap = NetCache.Get().GetNetObject<NetCache.NetCacheLettuceMap>();
		if (cachedLettuceMap != null && cachedLettuceMap.Map != null && (cachedLettuceMap.Map.Active || selectedBountyRecord == 0) && SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.GAMEPLAY)
		{
			InitializeMapDataFromProto(cachedLettuceMap.Map);
			return;
		}
		List<LettuceMapPlayerData> playerDataList = new List<LettuceMapPlayerData>();
		if (PartyManager.Get().IsInMercenariesCoOpParty())
		{
			if (PartyManager.Get().GetCurrentPartySize() < 2)
			{
				Log.Lettuce.PrintError("InitializeLettuceMapData - Not enough party members!");
				return;
			}
			BnetId partyLeaderBnetId = new BnetId
			{
				Hi = PartyManager.Get().GetPartyLeaderGameAccountId().High,
				Lo = PartyManager.Get().GetPartyLeaderGameAccountId().Low
			};
			BnetId coopPartnerBnetId = new BnetId
			{
				Hi = PartyManager.Get().GetMembers()[1].GameAccountId.High,
				Lo = PartyManager.Get().GetMembers()[1].GameAccountId.Low
			};
			playerDataList.Add(new LettuceMapPlayerData
			{
				PlayerId = partyLeaderBnetId,
				TeamId = selectedTeamId
			});
			playerDataList.Add(new LettuceMapPlayerData
			{
				PlayerId = coopPartnerBnetId,
				TeamId = coopPartnerTeamId
			});
		}
		else
		{
			BnetId playerBnetId = new BnetId
			{
				Hi = BnetPresenceMgr.Get().GetMyGameAccountId().High,
				Lo = BnetPresenceMgr.Get().GetMyGameAccountId().Low
			};
			playerDataList.Add(new LettuceMapPlayerData
			{
				PlayerId = playerBnetId,
				TeamId = selectedTeamId
			});
		}
		m_isNewMap = selectedBountyRecord > 0;
		Network.Get().RequestLettuceMap((uint)selectedBountyRecord, playerDataList, null, mythicLevel);
	}

	private void OnLettuceMapReceived()
	{
		NetCache.NetCacheLettuceMap cachedLettuceMap = NetCache.Get().GetNetObject<NetCache.NetCacheLettuceMap>();
		if (cachedLettuceMap.Map != null)
		{
			InitializeMapDataFromProto(cachedLettuceMap.Map);
		}
	}

	private void InitializeMapDataFromProto(PegasusLettuce.LettuceMap map)
	{
		m_lettuceMapProto = map;
		m_lettuceMap.CreateMapFromProto(map);
		LettuceBountyDbfRecord bountyRecord = GameDbf.LettuceBounty.GetRecord((int)map.BountyId);
		LettuceMapDisplayDataModel dataModel = GetDisplayDataModel();
		dataModel.FinalBossRewardList = new RewardListDataModel();
		dataModel.FinalBossRewardList.Items.AddRange(LettuceVillageDataUtil.GetFinalBossRewards(bountyRecord, out var rewardDescription, out var additionalRewardDescription));
		dataModel.FinalBossRewardList.Description = rewardDescription;
		dataModel.FinalBossRewardList.AdditionalDescription = additionalRewardDescription;
		dataModel.MapSeed = map.Seed;
		dataModel.RunEnded = !map.Active;
		dataModel.RunLost = !map.Active && !m_lettuceMap.IsFinalBossDefeated();
		dataModel.Heroic = bountyRecord.Heroic;
		dataModel.Tutorial = IsCurrentBountyTutorial();
		if (bountyRecord.BountySetRecord != null)
		{
			dataModel.ZoneIdentifier = bountyRecord.BountySetRecord.ShortGuid;
			if (!string.IsNullOrEmpty(bountyRecord.BountySetRecord.WatermarkTexture))
			{
				AssetLoader.Get().LoadTexture(bountyRecord.BountySetRecord.WatermarkTexture, delegate(AssetReference assetRef, UnityEngine.Object obj, object callbackData)
				{
					dataModel.BountySetWatermark = obj as Texture;
				});
			}
		}
		NetCache.NetCacheMercenariesPlayerInfo mercenariesPlayerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (mercenariesPlayerInfo != null && mercenariesPlayerInfo.BountyInfoMap != null && mercenariesPlayerInfo.BountyInfoMap.ContainsKey(bountyRecord.ID))
		{
			dataModel.FewestTurns = mercenariesPlayerInfo.BountyInfoMap[bountyRecord.ID].FewestTurns;
		}
		dataModel.CurrentTurns = (int)map.TurnsTaken;
		foreach (LettuceMapAnomalyAssignment anomalyCard in map.AnomalyCards)
		{
			if (GetBonusRewardTypeForCardId(anomalyCard.AnomalyCard) != 0)
			{
				dataModel.BonusRewardsActive = true;
				break;
			}
		}
		m_currentMapIsComplete = !m_lettuceMapProto.Active;
		if (m_isNewMap)
		{
			m_currentMapResult = CurrentResultState.NEW_MAP;
		}
		else if (m_currentMapIsComplete)
		{
			m_currentMapResult = ((!m_lettuceMap.IsFinalBossDefeated()) ? CurrentResultState.LOST_MAP : CurrentResultState.WON_MAP);
		}
		else
		{
			m_currentMapResult = CurrentResultState.WON_NODE;
		}
		int rowToFocusOn = 0;
		if (m_currentMapResult == CurrentResultState.WON_MAP)
		{
			ScrollMapToRow(rowToFocusOn);
			DisplayBossPortraitForCoin(m_lettuceMap.GetFinalBossCoinDataModel());
			RequestAndUpdateTutorialTeam();
		}
		else
		{
			if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY && !m_currentMapIsComplete)
			{
				rowToFocusOn = Mathf.Max(0, DetermineRowToFocusOn() - 1);
				ScrollMapToRow(rowToFocusOn);
			}
			else if (m_currentMapResult != 0)
			{
				ScrollMapToRow(DetermineRowToFocusOn());
			}
			if (m_currentMapResult == CurrentResultState.LOST_MAP)
			{
				LettuceMapCoinDataModel defeatCoin = m_lettuceMap.GetDefeatCoinDataModel();
				if (defeatCoin != null)
				{
					DisplayBossPortraitForCoin(defeatCoin);
				}
			}
		}
		m_isMythicMode = bountyRecord.DifficultyMode == LettuceBounty.MercenariesBountyDifficulty.MYTHIC;
		m_lettuceMapDataInitialized = true;
	}

	private bool ShouldShowTreasureSelection(PegasusLettuce.LettuceMap map)
	{
		if (PartyManager.Get().IsInMercenariesCoOpParty() && !PartyManager.Get().IsPartyLeader())
		{
			return false;
		}
		foreach (LettuceMapPendingTreasureSelection item in map.PendingTreasureSelection)
		{
			if (item.TreasureOptions.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	private bool ShouldShowVisitorSelection(PegasusLettuce.LettuceMap map)
	{
		bool isCoOpSecondaryPlayer = PartyManager.Get().IsInMercenariesCoOpParty() && !PartyManager.Get().IsPartyLeader();
		if (map.HasPendingVisitorSelection && map.PendingVisitorSelection.VisitorOptions.Count > 0)
		{
			return !isCoOpSecondaryPlayer;
		}
		return false;
	}

	private void SetSelectedTreasureChoices(int selectedTreasureChoices)
	{
		LettuceMapDisplayDataModel displayModel = GetDisplayDataModel();
		m_selectedTreasureChoices = Mathf.Clamp(selectedTreasureChoices, 0, displayModel.TreasureSelectionData.Count - 1);
		displayModel.SelectedTreasureChoices = m_selectedTreasureChoices;
	}

	private void InitializeTreasureSelectionData(PegasusLettuce.LettuceMap map)
	{
		LettuceMapDisplayDataModel dataModel = GetDisplayDataModel();
		m_numTreasuresChosen = 0;
		dataModel.TreasureSelectionData.Clear();
		m_selectedTreasureIndex = new List<int>();
		foreach (LettuceMapPendingTreasureSelection item in map.PendingTreasureSelection)
		{
			LettuceTreasureSelectionDataModel treasureSelectionData = new LettuceTreasureSelectionDataModel();
			dataModel.TreasureSelectionData.Add(treasureSelectionData);
			m_selectedTreasureIndex.Add(0);
			treasureSelectionData.TreasureOptions = new DataModelList<CardDataModel>();
			foreach (LettuceMapTreasure treasureOption in item.TreasureOptions)
			{
				treasureSelectionData.TreasureOptions.Add(CollectionUtils.CreateTreasureCardDataModel(treasureOption, m_isMythicMode));
			}
			treasureSelectionData.MercenaryTreasure = new DataModelList<CardDataModel>();
			LettuceMapPlayerData playerData = GetCurrentPlayerData();
			if (playerData == null || !playerData.HasTeamId)
			{
				Log.Lettuce.PrintError("InitializeTreasureSelectionData - no player data or teamId!");
				return;
			}
			PegasusLettuce.LettuceTeam mapTeam = GetTeamForPlayer(playerData);
			if (mapTeam == null)
			{
				Log.Lettuce.PrintError("InitializeTreasureSelectionData - no team found for player!");
				return;
			}
			foreach (LettuceTeamMercenary teamMercenary in mapTeam.MercenaryList.Mercenaries)
			{
				LettuceMercenary collectionMercenary = CollectionManager.Get().GetMercenary(teamMercenary.MercenaryId);
				MercenaryArtVariationPremiumDbfRecord portraitRecord = GameDbf.MercenaryArtVariationPremium.GetRecord(teamMercenary.SelectedPortraitId);
				int artVariationId = 0;
				TAG_PREMIUM portraitPremium = TAG_PREMIUM.NORMAL;
				if (portraitRecord != null)
				{
					artVariationId = portraitRecord.MercenaryArtVariationId;
					portraitPremium = (TAG_PREMIUM)portraitRecord.Premium;
				}
				treasureSelectionData.Mercenaries.Add(MercenaryFactory.CreateMercenaryDataModel(collectionMercenary.ID, artVariationId, portraitPremium, collectionMercenary, m_isMythicMode ? CollectionUtils.MercenaryDataPopluateExtra.MythicStats : CollectionUtils.MercenaryDataPopluateExtra.None));
				LettuceMapTreasureAssignment treasureAssignment = null;
				if (map.TreasureAssignmentList?.TreasureAssignments != null)
				{
					treasureAssignment = map.TreasureAssignmentList.TreasureAssignments.FirstOrDefault((LettuceMapTreasureAssignment e) => e.AssignedMercenary == teamMercenary.MercenaryId);
				}
				if (treasureAssignment != null)
				{
					treasureSelectionData.MercenaryTreasure.Add(CollectionUtils.CreateTreasureCardDataModel(treasureAssignment.Treasure, m_isMythicMode));
				}
				else
				{
					treasureSelectionData.MercenaryTreasure.Add(new CardDataModel());
				}
			}
		}
		PopulateChoiceMercenaryData(dataModel, map);
		SetSelectedTreasureChoices(0);
	}

	private void PopulateChoiceMercenaryData(LettuceMapDisplayDataModel dataModel, PegasusLettuce.LettuceMap map)
	{
		for (int index = 0; index < dataModel.TreasureSelectionData.Count; index++)
		{
			if (map.PendingTreasureSelection.Count <= index)
			{
				break;
			}
			LettuceTreasureSelectionDataModel treasureSelectionData = dataModel.TreasureSelectionData[index];
			int mercenaryId = map.PendingTreasureSelection[index].MercenaryId;
			LettuceMercenaryDataModel choiceMercenaryDataModel = null;
			MercenaryDetailed matchingRecruitedMercenary = map.RecruitedMercenaries.FirstOrDefault((MercenaryDetailed m) => m.Mercenary.AssetId == mercenaryId);
			if (matchingRecruitedMercenary != null)
			{
				choiceMercenaryDataModel = MercenaryFactory.CreateEmptyMercenaryDataModel();
				CollectionUtils.PopulateMercenaryCardDataModel(choiceMercenaryDataModel, LettuceMercenary.CreateDefaultArtVariation(mercenaryId));
				choiceMercenaryDataModel.MercenaryId = matchingRecruitedMercenary.Mercenary.AssetId;
				choiceMercenaryDataModel.MercenaryLevel = GameUtils.GetMercenaryLevelFromExperience((int)matchingRecruitedMercenary.Mercenary.Exp);
				choiceMercenaryDataModel.ExperienceInitial = (int)matchingRecruitedMercenary.Mercenary.Exp;
				choiceMercenaryDataModel.FullyUpgradedInitial = matchingRecruitedMercenary.IsFullyUpgraded;
				choiceMercenaryDataModel.Owned = true;
				CollectionUtils.SetMercenaryStatsByLevel(choiceMercenaryDataModel, mercenaryId, choiceMercenaryDataModel.MercenaryLevel, matchingRecruitedMercenary.IsFullyUpgraded);
			}
			else
			{
				choiceMercenaryDataModel = treasureSelectionData.Mercenaries.Where((LettuceMercenaryDataModel m) => m.MercenaryId == mercenaryId).FirstOrDefault();
			}
			if (choiceMercenaryDataModel == null)
			{
				Log.Lettuce.PrintError($"PopulateChoiceMercenaryData - no mercenary {mercenaryId} found in team or recruit list!");
				break;
			}
			treasureSelectionData.ChoiceMercenary = choiceMercenaryDataModel;
			LettuceMapTreasureAssignment choiceMercenaryTreasureAssignment = null;
			if (map.TreasureAssignmentList?.TreasureAssignments != null)
			{
				choiceMercenaryTreasureAssignment = map.TreasureAssignmentList.TreasureAssignments.Where((LettuceMapTreasureAssignment e) => e.AssignedMercenary == mercenaryId).FirstOrDefault();
			}
			if (choiceMercenaryTreasureAssignment != null)
			{
				treasureSelectionData.ChoiceMercenaryTreasure = CollectionUtils.CreateTreasureCardDataModel(choiceMercenaryTreasureAssignment.Treasure, m_isMythicMode);
			}
			treasureSelectionData.ChoiceMercenaryHasTreasure = choiceMercenaryTreasureAssignment != null;
		}
	}

	private IEnumerator TryShowingVisitorSelection(PegasusLettuce.LettuceMap map)
	{
		if (!ShouldShowVisitorSelection(map))
		{
			yield break;
		}
		LettuceMapDisplayDataModel dataModel = GetDisplayDataModel();
		dataModel.VisitorSelectionData = new LettuceVisitorSelectionDataModel();
		foreach (LettuceMapVisitorSelectionOption visitorOption in map.PendingVisitorSelection.VisitorOptions)
		{
			int mercenaryId = 0;
			if (visitorOption.HasVisitorId)
			{
				mercenaryId = LettuceVillageDataUtil.GetMercenaryIdForVisitor(GameDbf.MercenaryVisitor.GetRecord(visitorOption.VisitorId));
			}
			if (mercenaryId == 0 && visitorOption.HasFallbackMercenaryId)
			{
				mercenaryId = visitorOption.FallbackMercenaryId;
			}
			LettuceMercenary visitorMercenary = CollectionManager.Get().GetMercenary(mercenaryId);
			if (visitorMercenary == null)
			{
				Debug.LogError("TryShowingVisitorSelection() - Invalid Mercenary in selection.");
				continue;
			}
			LettuceMercenaryDataModel visitorMercenaryDataModel = MercenaryFactory.CreateMercenaryDataModel(visitorMercenary);
			visitorMercenaryDataModel.Card.Premium = TAG_PREMIUM.NORMAL;
			dataModel.VisitorSelectionData.VisitorOptions.Add(visitorMercenaryDataModel);
			if (visitorOption.HasTaskId)
			{
				VisitorTaskDbfRecord visitorTaskRecord = GameDbf.VisitorTask.GetRecord(visitorOption.TaskId);
				if (visitorTaskRecord != null)
				{
					int taskChainIndex = GameDbf.GetIndex().GetTaskChainIndexForTask(visitorOption.TaskId);
					taskChainIndex = ((taskChainIndex >= 0) ? taskChainIndex : 0);
					MercenaryVillageTaskItemDataModel taskDataModel = LettuceVillageDataUtil.CreateTaskModel(visitorTaskRecord, 0, taskChainIndex, MercenariesTaskState.Status.ACTIVE);
					dataModel.VisitorSelectionData.TaskOptions.Add(taskDataModel);
				}
				else
				{
					Debug.LogError($"TryShowingVisitorSelection() - Invalid Task {visitorOption.TaskId} in selection.");
				}
			}
		}
		if (dataModel.VisitorSelectionData.VisitorOptions.Count == 0)
		{
			Debug.LogError("TryShowingVisitorSelection() - Was not able to create any visitors choosing default instead");
			Network.Get().MakeMercenariesMapVisitorSelection(0);
			yield break;
		}
		m_waitingForVisitorSelectionServerResponse = true;
		while (m_waitingForVisitorSelectionServerResponse)
		{
			yield return null;
		}
	}

	private void InitializeBountyDataModel(LettuceBountyDataModel dataModel, LettuceBountyDbfRecord lettuceBountyRecord)
	{
		dataModel.BountyId = lettuceBountyRecord.ID;
		dataModel.AdventureMission = new AdventureMissionDataModel
		{
			ScenarioId = ScenarioDbId.LETTUCE_TAVERN,
			MissionState = AdventureMissionState.UNLOCKED
		};
		LettuceVillageDataUtil.ApplyMercenaryBossCoinMaterials(dataModel.AdventureMission, LettuceVillageDataUtil.GetBountyBossIds(lettuceBountyRecord), m_disposables);
		dataModel.PosterText = LettuceVillageDataUtil.GeneratePosterName(lettuceBountyRecord);
		dataModel.Available = true;
	}

	private int DetermineRowToFocusOn()
	{
		if (m_Scrollable == null)
		{
			return 0;
		}
		int rowToFocusOn = 0;
		foreach (LettuceMapNode node in m_lettuceMap.NodeData)
		{
			if (node.NodeState_ == LettuceMapNode.NodeState.UNLOCKED || node.NodeState_ == LettuceMapNode.NodeState.DEFEAT)
			{
				rowToFocusOn = (int)node.Row;
				break;
			}
		}
		return rowToFocusOn;
	}

	private bool ScrollMapToRow(int rowToFocusOn, float tweenTime = 0f, Action onCompleteCallback = null)
	{
		if (m_Scrollable == null)
		{
			return false;
		}
		int numRowsFitOnScreen = 3;
		float scrollPercentage = 1f - ((float)rowToFocusOn - 1f) / (float)(m_lettuceMap.NumberOfRows - numRowsFitOnScreen);
		if (tweenTime == 0f)
		{
			m_Scrollable.SetScrollImmediate(scrollPercentage);
			onCompleteCallback?.Invoke();
		}
		else
		{
			if (m_Scrollable.ScrollValue == scrollPercentage)
			{
				onCompleteCallback?.Invoke();
				return false;
			}
			m_Scrollable.SetScroll(scrollPercentage, delegate
			{
				onCompleteCallback?.Invoke();
			}, iTween.EaseType.easeInOutCubic, tweenTime, blockInputWhileScrolling: true);
		}
		return true;
	}

	private void OnLettuceMapChooseNodeResponseReceived()
	{
		LettuceMapChooseNodeResponse response = Network.Get().GetLettuceMapChooseNodeResponse();
		if (response == null)
		{
			Debug.LogError("OnLettuceMapChooseNodeResponseReceived() - No response received.");
			return;
		}
		if (!response.Success)
		{
			Debug.LogError("OnLettuceMapChooseNodeResponseReceived() - Choice was not successful!");
			return;
		}
		m_lettuceMapProto = response.UpdatedMap;
		StartCoroutine(HandleChooseNodeResponseFlowWithTiming(response.ChosenNode));
	}

	private void OnLettuceMapRetireResponseReceived()
	{
		LettuceMapRetireResponse response = Network.Get().GetLettuceMapRetireResponse();
		if (response == null)
		{
			Debug.LogError("OnLettuceMapRetireResponseReceived() - No response received.");
			return;
		}
		if (!response.Success)
		{
			Debug.LogError("OnLettuceMapRetireResponseReceived() - Retire was not successful!");
			return;
		}
		m_lettuceMapProto = response.UpdatedMap;
		if (!response.HasReceivedConsolationReward || !response.ReceivedConsolationReward)
		{
			TransitionToBountyBoardAfterRetire();
		}
		StartCoroutine(DisplayMercenariesConsolationRewardsWhenReady());
	}

	private IEnumerator DisplayMercenariesConsolationRewardsWhenReady()
	{
		while (!PopupDisplayManager.Get().RewardPopups.HasNonAutoRetireMercenariesRewardsToShow())
		{
			yield return null;
		}
		NetCache.ProfileNoticeMercenariesRewards rewardNotice = PopupDisplayManager.Get().RewardPopups.GetNextNonAutoRetireRewardMercenariesRewardToShow();
		NetCache.ProfileNoticeMercenariesRewards bonusRewardNotice = PopupDisplayManager.Get().RewardPopups.GetNextBonusMercenariesRewardToShow();
		if (!PopupDisplayManager.Get().RewardPopups.ShowMercenariesRewards(autoOpenChest: false, rewardNotice, bonusRewardNotice, TransitionToBountyBoardAfterRetire))
		{
			Log.Lettuce.PrintError("GetMercenariesConsolationRewards() - Could not get Consolation reward!");
			TransitionToBountyBoardAfterRetire();
		}
	}

	private void TransitionToBountyBoardAfterRetire()
	{
		NetCache.Get().UnloadNetObject<NetCache.NetCacheLettuceMap>();
		GetComponent<VisualController>().SetState("HIDE_TEAM_TRAY");
		if (PartyManager.Get().IsInMercenariesCoOpParty())
		{
			PartyManager.Get().LeaveParty();
		}
		LettuceVillageDisplay.LettuceSceneTransitionPayload transitionPayload = new LettuceVillageDisplay.LettuceSceneTransitionPayload();
		LettuceBountyDbfRecord bountyRecord = (transitionPayload.m_SelectedBounty = GameDbf.LettuceBounty.GetRecord((int)m_lettuceMapProto.BountyId));
		transitionPayload.m_SelectedBountySet = bountyRecord.BountySetRecord;
		transitionPayload.m_DifficultyMode = bountyRecord.DifficultyMode;
		m_sceneTransitionPayload = transitionPayload;
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.LETTUCE_BOUNTY_BOARD, SceneMgr.TransitionHandlerType.NEXT_SCENE, null, m_sceneTransitionPayload);
	}

	private void OnMercenariesMapTreasureSelectionResponseReceived()
	{
		MercenariesMapTreasureSelectionResponse response = Network.Get().GetMercenariesMapTreasureSelectionResponse();
		if (response == null)
		{
			Debug.LogError("OnMercenariesMapTreasureSelectionResponseReceived() - No response received.");
			return;
		}
		if (!response.Success)
		{
			Debug.LogError("OnMercenariesMapTreasureSelectionResponseReceived() - Choice was not successful!");
			return;
		}
		m_lettuceMapProto = response.UpdatedMap;
		CollectionUtils.PopulateTeamTreasures(GetTeamPreviewDataModel(), m_lettuceMapProto.TreasureAssignmentList?.TreasureAssignments, m_isMythicMode);
	}

	private void OnVisitorSelectionResponseReceived()
	{
		MercenariesMapVisitorSelectionResponse response = Network.Get().GetMercenariesMapVisitorSelectionResponse();
		if (response == null)
		{
			Log.Lettuce.PrintError("OnVisitorSelectionResponse() - No response received.");
			m_waitingForVisitorSelectionServerResponse = false;
			return;
		}
		if (!response.Success)
		{
			Log.Lettuce.PrintError("OnVisitorSelectionResponse() - Choice was not successful!");
			m_waitingForVisitorSelectionServerResponse = false;
			return;
		}
		if (response.HasReward && response.Reward.Components.Count != 0)
		{
			LettuceRewardComponent lettuceRewardComponent = response.Reward.Components[0];
			int mercenaryId = lettuceRewardComponent.MercenaryId;
			_ = lettuceRewardComponent.Amount;
			OnVisitorSelectionFallbackReward(mercenaryId, response);
		}
		else if (response.HasVisitorState)
		{
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			screenEffectParameters.Time = 0.25f;
			m_screenEffectsHandle.StartEffect(screenEffectParameters);
			MercenaryVillageTaskItemDataModel taskDataModel = LettuceVillageDataUtil.CreateTaskModelFromTaskState(response.VisitorState.ActiveTaskState, response.VisitorState);
			Widget rewardWidget = WidgetInstance.Create(VISITOR_TASK_REWARD_PREFAB);
			rewardWidget.BindDataModel(taskDataModel);
			rewardWidget.RegisterDoneChangingStatesListener(delegate
			{
				RewardScroll componentInChildren = rewardWidget.GetComponentInChildren<RewardScroll>();
				componentInChildren.Initialize(delegate
				{
					m_waitingForVisitorSelectionServerResponse = false;
					m_screenEffectsHandle.StopEffect();
				});
				componentInChildren.Show();
			}, null, callImmediatelyIfSet: true, doOnce: true);
		}
		else
		{
			m_waitingForVisitorSelectionServerResponse = false;
		}
		GetDisplayDataModel().VisitorSelectionData = null;
	}

	private void OnVisitorSelectionFallbackReward(int mercenaryId, MercenariesMapVisitorSelectionResponse response)
	{
		string cardId = GameUtils.GetCardIdFromMercenaryId(mercenaryId);
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardId);
		if (entityDef == null)
		{
			Log.Lettuce.PrintError("OnVisitorSelectionFallbackReward - Failed to load def for card {0}", cardId);
			m_waitingForVisitorSelectionServerResponse = false;
			return;
		}
		long quantity = 0L;
		if (response.HasReward && response.Reward.Components.Count != 0)
		{
			quantity = response.Reward.Components[0].Amount;
		}
		string description = response.FallbackReason_ switch
		{
			MercenariesMapVisitorSelectionResponse.FallbackReason.REASON_DUPLICATE_VISITOR => GameStrings.Get("GLUE_LETTUCE_MAP_VISITOR_FALLBACK_DUPLICATE"), 
			MercenariesMapVisitorSelectionResponse.FallbackReason.REASON_FULL => GameStrings.Get("GLUE_LETTUCE_MAP_VISITOR_FALLBACK_FULL"), 
			_ => GameStrings.Get("GLUE_LETTUCE_MAP_VISITOR_FALLBACK_NO_TASKS"), 
		};
		GetDisplayDataModel();
		RewardScrollDataModel rewardScrollDataModel = new RewardScrollDataModel
		{
			DisplayName = GameStrings.Get("GLUE_LETTUCE_MAP_VISITOR_FALLBACK_REWARD_TITLE"),
			Description = description,
			RewardList = new RewardListDataModel
			{
				Items = new DataModelList<RewardItemDataModel>
				{
					new RewardItemDataModel
					{
						ItemType = RewardItemType.MERCENARY_COIN,
						MercenaryCoin = new LettuceMercenaryCoinDataModel
						{
							MercenaryId = mercenaryId,
							MercenaryName = entityDef.GetName(),
							Quantity = (int)quantity,
							GlowActive = true,
							NameActive = true
						}
					}
				}
			}
		};
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
		screenEffectParameters.Time = 0.25f;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
		Widget rewardWidget = WidgetInstance.Create(VISITOR_FALLBACK_REWARD_PREFAB);
		rewardWidget.BindDataModel(rewardScrollDataModel);
		rewardWidget.RegisterDoneChangingStatesListener(delegate
		{
			RewardScroll componentInChildren = rewardWidget.GetComponentInChildren<RewardScroll>();
			componentInChildren.Initialize(delegate
			{
				m_waitingForVisitorSelectionServerResponse = false;
				m_screenEffectsHandle.StopEffect();
			});
			componentInChildren.Show();
		}, null, callImmediatelyIfSet: true, doOnce: true);
	}

	private void OnTransitionFromGameplayFinished(bool cutoff, object userData)
	{
		LoadingScreen.Get().UnregisterFinishedTransitionListener(OnTransitionFromGameplayFinished);
		m_loadingScreenTransitionFromGameplayComplete = true;
	}

	private void OnLoadingScreenFadeInStarted()
	{
		m_loadingScreenTransitionFromGameplayStarted = true;
	}

	private void OnCoinSelected()
	{
		if ((PartyManager.Get().IsInMercenariesCoOpParty() && !PartyManager.Get().IsPartyLeader()) || !m_lettuceMapDataInitialized || m_currentMapIsComplete)
		{
			return;
		}
		EventDataModel eventDataModel = GetLettuceMapEventDataModel();
		if (eventDataModel == null)
		{
			Log.All.PrintError("No event data model attached to the LettuceMapDisplay.");
			return;
		}
		LettuceMapCoinDataModel coinDataModel = (LettuceMapCoinDataModel)eventDataModel.Payload;
		if (coinDataModel.CoinState == LettuceMapNode.NodeState.UNLOCKED)
		{
			LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.MAP_ACTIVE_COIN_RELEASED, base.gameObject, coinDataModel.NodeTypeId, (int)m_lettuceMapProto.BountyId);
		}
		SelectCoinInternal(coinDataModel);
	}

	private void SelectCoinInternal(LettuceMapCoinDataModel selectedCoin)
	{
		m_selectedMapCoin = selectedCoin;
		m_lettuceMap.SelectCoin(m_selectedMapCoin);
		SetPlayButtonText();
		DisplayBossPortraitForCoin(selectedCoin);
		if (PartyManager.Get().IsInMercenariesCoOpParty())
		{
			if (!PartyManager.Get().IsPartyLeader())
			{
				return;
			}
			PartyManager.Get().SetSelectedMercenariesCoOpMapNodeId(m_selectedMapCoin.Id);
		}
		TryEnablePlayButton();
		if (GameDbf.LettuceMapNodeType.GetRecord(m_selectedMapCoin.NodeTypeId).AutoPlay)
		{
			ExecutePlayLogic();
		}
	}

	private void TryAutoNextSelectCoin()
	{
		List<LettuceMapCoinDataModel> unlockedCoins = m_lettuceMap.GetUnlockedCoinDataModels();
		if (unlockedCoins.Count == 1)
		{
			SelectCoinInternal(unlockedCoins.FirstOrDefault());
		}
	}

	private void DisplayBossPortraitForCoin(LettuceMapCoinDataModel selectedCoin)
	{
		List<string> bossCardIds = GetBossCardIdsFromNodeId(selectedCoin.Id);
		if (bossCardIds.Count == 0)
		{
			return;
		}
		DataModelList<CardDataModel> cards = new DataModelList<CardDataModel>();
		string bossName = string.Empty;
		bool isFirst = true;
		foreach (string bossCardId in bossCardIds)
		{
			CardDataModel card = new CardDataModel
			{
				CardId = bossCardId
			};
			cards.Add(card);
			if (!isFirst)
			{
				bossName += "\n";
			}
			else
			{
				isFirst = false;
			}
			EntityDef def = DefLoader.Get().GetEntityDef(bossCardId);
			bossName += def.GetName();
		}
		LettuceMapDisplayDataModel displayDataModel = GetDisplayDataModel();
		displayDataModel.BossCard = cards;
		displayDataModel.BossName = bossName;
	}

	private void OnTreasureSelected()
	{
		EventDataModel eventDataModel = GetLettuceMapEventDataModel();
		if (eventDataModel == null)
		{
			Log.All.PrintError("No event data model attached to the LettuceMapDisplay.");
		}
		else
		{
			m_selectedTreasureIndex[m_selectedTreasureChoices] = Convert.ToInt32(eventDataModel.Payload);
		}
	}

	private void OnTreasureChosen()
	{
		if (!Network.IsLoggedIn())
		{
			DialogManager.Get().ShowReconnectHelperDialog(OnTreasureChosen, delegate
			{
				BackOutOfScene(SceneMgr.Mode.LETTUCE_VILLAGE);
			});
		}
		if (m_selectedTreasureIndex[m_selectedTreasureChoices] < 0)
		{
			Log.Lettuce.PrintError("OnTreasureChosen() - No treasure selected!");
			return;
		}
		if (m_lettuceMapProto.PendingTreasureSelection == null)
		{
			Log.Lettuce.PrintError("OnTreasureChosen() - No pending treasure selection!");
			return;
		}
		m_screenEffectsHandle.StopEffect();
		LettuceMapDisplayDataModel dataModel = GetDisplayDataModel();
		int mercenary = dataModel.TreasureSelectionData[m_selectedTreasureChoices].ChoiceMercenary.MercenaryId;
		_ = m_selectedTreasureChoices;
		Network.Get().MakeMercenariesMapTreasureSelection(mercenary, m_selectedTreasureIndex[m_selectedTreasureChoices]);
		m_waitingForTreasureSelection = ++m_numTreasuresChosen != dataModel.TreasureSelectionData.Count;
		dataModel.TreasureSelectionData[m_selectedTreasureChoices].TreasureSelected = true;
		if (m_waitingForTreasureSelection)
		{
			SetSelectedTreasureChoices((m_selectedTreasureChoices + 1) % dataModel.TreasureSelectionData.Count);
		}
		else
		{
			SetSelectedTreasureChoices(-1);
			dataModel.TreasureSelectionData.Clear();
		}
		NetCache.ProfileNoticeMercenariesRewards rewardNotice = PopupDisplayManager.Get().RewardPopups.GetNextNonAutoRetireRewardMercenariesRewardToShow();
		NetCache.ProfileNoticeMercenariesRewards bonusRewardNotice = PopupDisplayManager.Get().RewardPopups.GetNextBonusMercenariesRewardToShow();
		if (!PopupDisplayManager.Get().RewardPopups.ShowMercenariesRewards(autoOpenChest: true, rewardNotice, bonusRewardNotice, OnTreasureChosenMercenaryRewardsComplete))
		{
			OnTreasureChosenMercenaryRewardsComplete();
		}
	}

	private void OnTreasureChosenMercenaryRewardsComplete()
	{
		PopupDisplayManager.Get().RewardPopups.ShowMercenariesFullyUpgraded();
	}

	private void OnVisitorSelected()
	{
		EventDataModel eventDataModel = GetLettuceMapEventDataModel();
		if (eventDataModel == null)
		{
			Log.All.PrintError("No event data model attached to the LettuceMapDisplay.");
		}
		else
		{
			m_selectedVisitorIndex = Convert.ToInt32(eventDataModel.Payload);
		}
	}

	private void OnVisitorChosen()
	{
		if (!Network.IsLoggedIn())
		{
			DialogManager.Get().ShowReconnectHelperDialog(OnVisitorChosen, delegate
			{
				BackOutOfScene(SceneMgr.Mode.LETTUCE_VILLAGE);
			});
		}
		if (m_selectedVisitorIndex < 0)
		{
			Log.Lettuce.PrintError("OnVisitorChosen() - No visitor selected!");
			return;
		}
		if (m_lettuceMapProto.PendingVisitorSelection == null)
		{
			Log.Lettuce.PrintError("OnVisitorChosen() - No pending visitor selection!");
			return;
		}
		m_screenEffectsHandle.StopEffect();
		Network.Get().MakeMercenariesMapVisitorSelection(m_selectedVisitorIndex);
	}

	private void OnTeamViewShow()
	{
		m_isTeamViewVisible = true;
		m_playButton.Disable();
	}

	private void OnTeamViewHide()
	{
		m_isTeamViewVisible = false;
		TryEnablePlayButton();
	}

	private void SetPlayButtonText()
	{
		LettuceMapNodeTypeDbfRecord nodeTypeRecord = GameDbf.LettuceMapNodeType.GetRecord(m_selectedMapCoin.NodeTypeId);
		if (nodeTypeRecord.PlayButtonText != null && !string.IsNullOrWhiteSpace(nodeTypeRecord.PlayButtonText.GetString()))
		{
			m_playButton.SetText(nodeTypeRecord.PlayButtonText.GetString());
		}
		else
		{
			m_playButton.SetText(GameStrings.Get("GLOBAL_PLAY"));
		}
	}

	private bool ShouldEnablePlayButton()
	{
		if (m_isTeamViewVisible)
		{
			return false;
		}
		if (m_selectedMapCoin == null)
		{
			return false;
		}
		LettuceMapNodeTypeDbfRecord nodeTypeRecord = GameDbf.LettuceMapNodeType.GetRecord(m_selectedMapCoin.NodeTypeId);
		if (m_selectedMapCoin.CoinState != LettuceMapNode.NodeState.UNLOCKED)
		{
			if (m_selectedMapCoin.CoinState == LettuceMapNode.NodeState.COMPLETE)
			{
				return nodeTypeRecord.Repeatable;
			}
			return false;
		}
		return true;
	}

	private void TryEnablePlayButton()
	{
		if (ShouldEnablePlayButton())
		{
			m_playButton.Enable();
		}
		else
		{
			m_playButton.Disable();
		}
	}

	private List<string> GetBossCardIdsFromNodeId(int nodeId)
	{
		List<string> bossCardIds = new List<string>();
		if (m_lettuceMapProto == null)
		{
			Debug.LogError("GetBossCardIdsFromNodeId called before the proto has been received!");
			return bossCardIds;
		}
		LettuceMapNode node = m_lettuceMapProto.Nodes.Find((LettuceMapNode n) => n.NodeId == nodeId);
		if (node == null)
		{
			Debug.LogErrorFormat("GetBossCardIdsFromNodeId - Node {0} not found in the proto!", nodeId);
			return bossCardIds;
		}
		if (node.BossCard.Count == 0)
		{
			Debug.LogErrorFormat("GetBossCardIdsFromNodeId - Node {0} has no boss card set!", nodeId);
			return bossCardIds;
		}
		foreach (PegasusShared.CardDef item in node.BossCard)
		{
			string cardId = GameUtils.TranslateDbIdToCardId(item.Asset);
			if (!bossCardIds.Contains(cardId))
			{
				bossCardIds.Add(cardId);
			}
		}
		bossCardIds.Sort();
		return bossCardIds;
	}

	private LettuceMapPlayerData GetCurrentPlayerData()
	{
		if (m_lettuceMapProto == null)
		{
			Log.Lettuce.PrintError("GetCurrentPlayerData - No map proto.");
			return null;
		}
		if (m_lettuceMapProto.PlayerData == null || m_lettuceMapProto.PlayerData.Count == 0)
		{
			Log.Lettuce.PrintError("GetCurrentPlayerData - No player data in map.");
			return null;
		}
		if (PartyManager.Get().IsInMercenariesCoOpParty() && !PartyManager.Get().IsPartyLeader())
		{
			if (m_lettuceMapProto.PlayerData.Count < 2)
			{
				Log.Lettuce.PrintError("GetCurrentPlayerData - No co-op partner in map.");
				return null;
			}
			return m_lettuceMapProto.PlayerData[1];
		}
		return m_lettuceMapProto.PlayerData.FirstOrDefault();
	}

	private PegasusLettuce.LettuceTeam GetTeamForPlayer(LettuceMapPlayerData playerData)
	{
		if (m_lettuceMapProto == null)
		{
			Log.Lettuce.PrintError("GetTeamForPlayer - No map proto.");
			return null;
		}
		if (playerData == null)
		{
			Log.Lettuce.PrintError("GetTeamForPlayer - No playerData.");
			return null;
		}
		if (m_lettuceMapProto.TeamData == null || m_lettuceMapProto.TeamData.Count == 0)
		{
			Log.Lettuce.PrintError("GetTeamForPlayer - No team data in map.");
			return null;
		}
		foreach (PegasusLettuce.LettuceTeam team in m_lettuceMapProto.TeamData)
		{
			if (team.HasTeamId && team.TeamId == playerData.TeamId)
			{
				return team;
			}
		}
		return null;
	}

	private void PopulateTeamPreviewData(LettuceMapPlayerData playerData)
	{
		LettuceTeamDataModel dataModel = GetTeamPreviewDataModel();
		if (playerData == null)
		{
			Log.Lettuce.PrintError("PopulateTeamPreviewData - Unable to retrieve playerData");
			return;
		}
		PegasusLettuce.LettuceTeam mapTeam = GetTeamForPlayer(playerData);
		if (mapTeam == null)
		{
			Log.Lettuce.PrintError("PopulateTeamPreviewData - Unable to retrieve team");
			return;
		}
		int deadMercenaryListIndex = 2;
		if (PartyManager.Get().IsInMercenariesCoOpParty() && !PartyManager.Get().IsPartyLeader())
		{
			deadMercenaryListIndex = 3;
		}
		if (m_lettuceMapProto.DeadMercenaries.Count < deadMercenaryListIndex)
		{
			Log.Lettuce.PrintError($"PopulateTeamPreviewData - Unable to retrieve dead mercenaries for index={deadMercenaryListIndex}");
			return;
		}
		dataModel.TeamId = mapTeam.TeamId;
		dataModel.TeamName = mapTeam.Name;
		LettuceTeam team = LettuceTeam.Convert(mapTeam);
		CollectionUtils.PopulateTeamPreviewData(dataModel, team, m_lettuceMapProto.DeadMercenaries[deadMercenaryListIndex].MercenaryIds, populateCards: true, isRemote: false, showMythicTeamLevel: false, m_isMythicMode ? CollectionUtils.MercenaryDataPopluateExtra.MythicStats : CollectionUtils.MercenaryDataPopluateExtra.None);
		CollectionUtils.PopulateTeamTreasures(dataModel, m_lettuceMapProto.TreasureAssignmentList?.TreasureAssignments, m_isMythicMode);
	}

	private IEnumerator PlayIntroMapScroll()
	{
		yield return new WaitForSeconds(0.25f);
		ScrollMapToRow(DetermineRowToFocusOn(), 2f, delegate
		{
			StartCoroutine(PlayIntroMapScroll_OnScrollFinished());
		});
	}

	private IEnumerator PlayIntroMapScroll_OnScrollFinished()
	{
		yield return new WaitForSeconds(0.25f);
		m_lettuceMap.FlipUnlockedCoins();
		TryAutoNextSelectCoin();
		yield return WaitForTutorialEvent(LettuceTutorialVo.LettuceTutorialEvent.MAP_STARTED, 0);
	}

	private IEnumerator PlayDefaultMapScroll()
	{
		if (ScrollMapToRow(DetermineRowToFocusOn(), 0.5f, m_lettuceMap.FlipUnlockedCoins))
		{
			yield return new WaitForSeconds(0.5f);
		}
		if (m_currentMapResult != CurrentResultState.LOST_MAP)
		{
			yield return new WaitForSeconds(1f);
			TryAutoNextSelectCoin();
		}
	}

	private IEnumerator PlayVictoryMapScroll()
	{
		m_Scrollable.SetScrollImmediate(1f);
		while (SceneMgr.Get().IsTransitionNowOrPending())
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.25f);
		m_Scrollable.SetScroll(0f, iTween.EaseType.easeInOutCubic, 2f, blockInputWhileScrolling: true);
		List<LettuceMapCoin> completedCoins = m_lettuceMap.GetCompletedCoins();
		foreach (LettuceMapCoin item in completedCoins)
		{
			item.FlashCheckMark();
			yield return new WaitForSeconds(2f / (float)completedCoins.Count);
		}
		m_finalBossChestVisualController.SetState("OPEN_REWARD");
		yield return new WaitForSeconds(3f);
		NetCache.ProfileNoticeMercenariesRewards rewardNotice = PopupDisplayManager.Get().RewardPopups.GetNextNonAutoRetireRewardMercenariesRewardToShow();
		NetCache.ProfileNoticeMercenariesRewards bonusRewardNotice = PopupDisplayManager.Get().RewardPopups.GetNextBonusMercenariesRewardToShow();
		bool popupDone = !PopupDisplayManager.Get().RewardPopups.ShowMercenariesRewards(autoOpenChest: true, rewardNotice, bonusRewardNotice, OnMercenariesRewardsPopupHidden);
		while (!popupDone)
		{
			yield return null;
		}
		popupDone = !PopupDisplayManager.Get().RewardPopups.ShowMercenariesFullyUpgraded(OnMercenariesRewardsPopupHidden);
		while (!popupDone)
		{
			yield return null;
		}
		void OnMercenariesRewardsPopupHidden()
		{
			popupDone = true;
		}
	}

	private void OnPartyChanged(PartyManager.PartyInviteEvent inviteEvent, BnetGameAccountId playerGameAccountId, PartyManager.PartyData challengeData, object userData)
	{
		Log.Party.PrintDebug("LettuceCoOpDisplay.OnPartyChanged(): Event={0}, gameAccountId={1}", inviteEvent, playerGameAccountId);
		if (inviteEvent == PartyManager.PartyInviteEvent.I_RESCINDED_INVITE || (uint)(inviteEvent - 5) <= 1u || (uint)(inviteEvent - 10) <= 1u)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_LETTUCE_PARTY_DISBANDED_HEADER");
			info.m_text = GameStrings.Get("GLUE_LETTUCE_PARTY_DISBANDED_BODY");
			info.m_iconSet = AlertPopup.PopupInfo.IconSet.Default;
			info.m_showAlertIcon = false;
			info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info.m_okText = GameStrings.Get("GLOBAL_OKAY");
			DialogManager.Get().ShowPopup(info);
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.LETTUCE_VILLAGE, SceneMgr.TransitionHandlerType.NEXT_SCENE, null, m_sceneTransitionPayload);
		}
	}

	private void OnPartyAttributeChanged(Blizzard.GameService.Protocol.V2.Client.Attribute attribute, object userData)
	{
		if (attribute.Name == "node_id" && attribute.Value.HasIntValue && !PartyManager.Get().IsPartyLeader())
		{
			LettuceMapCoinDataModel selectedCoinDataModel = m_lettuceMap.GetCoinDataModelById((int)attribute.Value.IntValue);
			if (selectedCoinDataModel == null)
			{
				Log.Lettuce.PrintError("OnPartyAttributeChanged - Invalid map node id={0} in attributes.", attribute.Value.IntValue);
			}
			SelectCoinInternal(selectedCoinDataModel);
		}
	}

	private bool CheckAndEnqueueMercenaryGrant()
	{
		LettuceMapCoinDataModel coinDataModel = m_lettuceMap.GetLastCompletedCoinDataModel();
		if (coinDataModel == null)
		{
			return false;
		}
		LettuceMapNodeTypeDbfRecord nodeTypeToGrant = GameDbf.LettuceMapNodeType.GetRecord(coinDataModel.NodeTypeId);
		if (nodeTypeToGrant == null)
		{
			return false;
		}
		if (nodeTypeToGrant.GrantMercenary <= 0)
		{
			return false;
		}
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(nodeTypeToGrant.GrantMercenary);
		if (merc == null)
		{
			Log.Lettuce.PrintError("CheckAndEnqueueMercenaryGrant - Unable to get mercenary {0}", nodeTypeToGrant.GrantMercenary);
			return false;
		}
		merc.m_owned = true;
		RewardScrollDataModel scrollDataModel = new RewardScrollDataModel
		{
			DisplayName = GameStrings.Get("GLUE_LETTUCE_MERCENARY_REWARD_TITLE"),
			Description = GameStrings.Get("GLUE_LETTUCE_MERCENARY_REWARD_DESC"),
			RewardList = new RewardListDataModel
			{
				Items = new DataModelList<RewardItemDataModel>
				{
					new RewardItemDataModel
					{
						Quantity = 1,
						ItemType = RewardItemType.MERCENARY,
						Mercenary = MercenaryFactory.CreateMercenaryDataModel(merc)
					}
				}
			}
		};
		m_rewardPresenter.EnqueueReward(scrollDataModel, delegate
		{
		});
		return true;
	}

	private IEnumerator DisplayNewlyGrantedAnomalyCards(PegasusLettuce.LettuceMap lettuceMap, int completedNodeId)
	{
		if (lettuceMap == null)
		{
			Log.Lettuce.PrintError("DisplayNewlyGrantedAnomalyCards - null map proto was provided.");
			yield break;
		}
		int grantedAnomalyCardId = 0;
		foreach (LettuceMapAnomalyAssignment anomalyCard in lettuceMap.AnomalyCards)
		{
			if (anomalyCard.SourceNodeId == completedNodeId)
			{
				grantedAnomalyCardId = anomalyCard.AnomalyCard;
			}
		}
		if (grantedAnomalyCardId == 0)
		{
			yield break;
		}
		if (GetBonusRewardTypeForCardId(grantedAnomalyCardId) != 0)
		{
			LettuceMapDisplayDataModel dataModel = GetDisplayDataModel();
			if (dataModel != null)
			{
				dataModel.BonusRewardsActive = true;
			}
		}
		Vector3 sourceNodePosition = m_lettuceMap.GetWorldSpacePositionOfCoin(completedNodeId);
		LettuceMapAnomalyGrantDataModel anomalyGrantDataModel = new LettuceMapAnomalyGrantDataModel
		{
			GrantedCard = new CardDataModel
			{
				CardId = GameUtils.TranslateDbIdToCardId(grantedAnomalyCardId),
				Premium = TAG_PREMIUM.NORMAL
			},
			SourceNodePosition = new DataModelList<float> { sourceNodePosition.x, sourceNodePosition.y, sourceNodePosition.z }
		};
		EventDataModel eventDataModel = new EventDataModel();
		eventDataModel.Payload = anomalyGrantDataModel;
		SendEventUpwardStateAction.SendEventUpward(base.gameObject, "ANOMALY_GRANTED_FROM_CODE", eventDataModel);
		m_waitingForVisualControllerState = true;
		while (m_waitingForVisualControllerState)
		{
			yield return null;
		}
	}

	private IEnumerator HandleChooseNodeResponseFlowWithTiming(int chosenNodeId)
	{
		m_clickBlocker.SetActive(value: true);
		yield return HandleVisitResponseByNodeType(GetNodeTypeRecordFromNodeId(chosenNodeId));
		yield return DisplayNewlyGrantedAnomalyCards(m_lettuceMapProto, chosenNodeId);
		yield return TryShowingVisitorSelection(m_lettuceMapProto);
		m_lettuceMap.RefreshWithNewData(m_lettuceMapProto);
		CheckAndEnqueueMercenaryGrant();
		PopulateTeamPreviewData(GetCurrentPlayerData());
		ScrollMapToRow(DetermineRowToFocusOn(), 0.5f);
		yield return new WaitForSeconds(0.5f);
		m_lettuceMap.FlipUnlockedCoins();
		yield return new WaitForSeconds(1f);
		TryAutoNextSelectCoin();
		yield return CheckLastCompletedNodeTutorialEvents();
		yield return CheckForNodeDialogueEvents();
		m_clickBlocker.SetActive(value: false);
	}

	private IEnumerator HandleVisitResponseByNodeType(LettuceMapNodeTypeDbfRecord nodeTypeRecord)
	{
		switch (nodeTypeRecord.VisitLogic)
		{
		case LettuceMapNodeType.Visitlogictype.HEAL_TEAM:
			SendEventUpwardStateAction.SendEventUpward(base.gameObject, "PLAY_SPIRIT_HEALER_FX");
			m_waitingForVisualControllerState = true;
			while (m_waitingForVisualControllerState)
			{
				yield return null;
			}
			break;
		case LettuceMapNodeType.Visitlogictype.SKIP_TO_FINAL_BOSS:
			SendEventUpwardStateAction.SendEventUpward(base.gameObject, "PLAY_PORTAL_FX");
			m_waitingForVisualControllerState = true;
			while (m_waitingForVisualControllerState)
			{
				yield return null;
			}
			break;
		case LettuceMapNodeType.Visitlogictype.REASSIGN_MAP_ROLE:
			SendEventUpwardStateAction.SendEventUpward(base.gameObject, "PLAY_ROLE_RUSH_FX");
			m_waitingForVisualControllerState = true;
			while (m_waitingForVisualControllerState)
			{
				yield return null;
			}
			break;
		}
	}

	private IEnumerator WaitForTutorialEvent(LettuceTutorialVo.LettuceTutorialEvent tutorialEvent, int nodeTypeId)
	{
		bool done = false;
		LettuceTutorialUtils.FireEvent(tutorialEvent, base.gameObject, nodeTypeId, (int)m_lettuceMapProto.BountyId, delegate
		{
			done = true;
		});
		while (!done)
		{
			yield return null;
		}
	}

	private IEnumerator CheckForNodeDialogueEvents()
	{
		List<LettuceMapCoinDataModel> unlockedCoins = m_lettuceMap.GetUnlockedCoinDataModels();
		foreach (LettuceMapCoinDataModel item in unlockedCoins)
		{
			int unlockedNodeTypeId = item.NodeTypeId;
			yield return WaitForTutorialEvent(LettuceTutorialVo.LettuceTutorialEvent.MAP_NODE_COMPLETED_PRE_REWARDS, unlockedNodeTypeId);
		}
		LettuceMapCoinDataModel lastCompletedCoinDataModel = m_lettuceMap.GetLastCompletedCoinDataModel();
		if (lastCompletedCoinDataModel != null)
		{
			int lastNodeTypeId = lastCompletedCoinDataModel.NodeTypeId;
			yield return WaitForTutorialEvent(LettuceTutorialVo.LettuceTutorialEvent.MAP_NODE_REVEALED, lastNodeTypeId);
		}
	}

	private IEnumerator CheckLastCompletedNodeTutorialEvents()
	{
		LettuceMapCoinDataModel lastCompletedCoinDataModel = m_lettuceMap.GetLastCompletedCoinDataModel();
		if (lastCompletedCoinDataModel != null)
		{
			int lastNodeTypeId = lastCompletedCoinDataModel.NodeTypeId;
			yield return WaitForTutorialEvent(LettuceTutorialVo.LettuceTutorialEvent.MAP_NODE_COMPLETED_PRE_MERC_GRANT, lastNodeTypeId);
			bool rewardDone = !m_rewardPresenter.ShowNextReward(delegate
			{
				rewardDone = true;
			});
			while (!rewardDone)
			{
				yield return null;
			}
			yield return WaitForTutorialEvent(LettuceTutorialVo.LettuceTutorialEvent.MAP_NODE_COMPLETED_POST_MERC_GRANT, lastNodeTypeId);
		}
	}

	private void RequestAndUpdateTutorialTeam()
	{
		if (!IsCurrentBountyTutorial())
		{
			Network.Get().MercenariesTeamListRequest();
			return;
		}
		NetCache.Get().RegisterUpdatedListener(typeof(LettuceTeamList), RequestAndUpdateTutorialTeam_OnMercenariesTeamListResponse);
		Network.Get().MercenariesTeamListRequest();
	}

	private void RequestAndUpdateTutorialTeam_OnMercenariesTeamListResponse()
	{
		NetCache.Get().RemoveUpdatedListener(typeof(LettuceTeamList), RequestAndUpdateTutorialTeam_OnMercenariesTeamListResponse);
		if (m_lettuceMapProto.PlayerData.Count == 0)
		{
			Log.Lettuce.PrintError("RequestAndUpdateTutorialTeam - Player not found in map.");
			return;
		}
		LettuceMapPlayerData playerData = m_lettuceMapProto.PlayerData.FirstOrDefault();
		LettuceTeam mapTeam = CollectionManager.Get().GetTeam(playerData.TeamId);
		if (mapTeam == null)
		{
			Log.Lettuce.PrintError("RequestAndUpdateTutorialTeam - Team not found! Team!d={0}", playerData.TeamId);
		}
		else
		{
			mapTeam.Name = GameStrings.Get("GLUE_LETTUCE_MERCENARY_TUTORIAL_TEAM_NAME");
			mapTeam.SendChanges();
		}
	}

	private bool IsCurrentBountyTutorial()
	{
		if (m_lettuceMapProto == null)
		{
			return false;
		}
		return LettuceVillageDataUtil.IsBountyTutorial(GameDbf.LettuceBounty.GetRecord((int)m_lettuceMapProto.BountyId));
	}

	private LettuceMapNodeTypeDbfRecord GetNodeTypeRecordFromNodeId(int nodeId)
	{
		if (m_lettuceMapProto == null)
		{
			return null;
		}
		foreach (LettuceMapNode node in m_lettuceMapProto.Nodes)
		{
			if (node.NodeId == nodeId)
			{
				return GameDbf.LettuceMapNodeType.GetRecord((int)node.NodeTypeId);
			}
		}
		return null;
	}

	private void ExecutePlayLogic()
	{
		if (m_selectedMapCoin == null)
		{
			Debug.LogError("OnPlayButtonRelease() - No coin selected!");
			return;
		}
		if (!Network.IsLoggedIn())
		{
			DialogManager.Get().ShowReconnectHelperDialog();
			return;
		}
		if (m_mapMaskable != null)
		{
			m_mapMaskable.enabled = false;
		}
		LettuceMapNodeTypeDbfRecord nodeTypeRecord = GameDbf.LettuceMapNodeType.GetRecord(m_selectedMapCoin.NodeTypeId);
		if (nodeTypeRecord.UsesGameplayScene)
		{
			m_playButton.Disable();
			int scenario = 3790;
			if (nodeTypeRecord.ScenarioOverride != 0)
			{
				scenario = nodeTypeRecord.ScenarioOverride;
			}
			if (PartyManager.Get().IsInMercenariesCoOpParty())
			{
				PartyManager.Get().FindGame();
				return;
			}
			GameMgr gameMgr = GameMgr.Get();
			int missionId = scenario;
			long deckId = 0L;
			int? lettuceMapNodeId = m_selectedMapCoin.Id;
			gameMgr.FindGame(GameType.GT_MERCENARIES_PVE, FormatType.FT_WILD, missionId, 0, deckId, null, null, restoreSavedGameState: false, null, lettuceMapNodeId, 0L);
		}
		else if (nodeTypeRecord.VisitLogic == LettuceMapNodeType.Visitlogictype.VIEW_TASK_LIST)
		{
			LettuceVillagePopupManager lettuceVillagePopupManager = LettuceVillagePopupManager.Get();
			lettuceVillagePopupManager.OnPopupClosed = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Combine(lettuceVillagePopupManager.OnPopupClosed, new Action<LettuceVillagePopupManager.PopupType>(OnTaskboardClosed));
			lettuceVillagePopupManager.Show(LettuceVillagePopupManager.PopupType.TASKBOARD);
		}
		else
		{
			m_playButton.Disable();
			Network.Get().ChooseLettuceMapNode((uint)m_selectedMapCoin.Id);
			m_clickBlocker.SetActive(value: true);
		}
	}
}
