using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core.Utils;
using Hearthstone.Core.Streaming;
using Hearthstone.DataModels;
using Hearthstone.Streaming;
using Hearthstone.UI;
using UnityEngine;

public class GameModeDisplay : MonoBehaviour
{
	public enum ModeLockedStatus
	{
		Unlocked,
		Locked,
		DownloadRequired,
		Downloading
	}

	public SlidingTray m_slidingTray;

	public GameObject m_clickBlocker;

	public GameObject m_nameText;

	public UberText m_lockedNameText;

	public GameObject m_lockedPlateMesh;

	public VisualController m_gameModeButtonController;

	[SerializeField]
	private AsyncReference m_DisplayReference;

	[SerializeField]
	private AsyncReference m_PlayButtonReference;

	[SerializeField]
	private AsyncReference m_downloadButtonReference;

	[SerializeField]
	private AsyncReference m_BackButtonReference;

	[SerializeField]
	private AsyncReference m_BackButtonMobileReference;

	[SerializeField]
	private Widget m_downloadPopupWidget;

	private PlayButton m_playButton;

	private UIBButton m_downloadButton;

	private UIBButton m_backButton;

	private bool m_playButtonFinishedLoading;

	private bool m_backButtonFinishedLoading;

	private DownloadConfirmationPopup m_downloadConfirmationPopup;

	private Action m_onSceneTransitionCompleteCallback;

	private List<GameModeDbfRecord> m_activeGameModeRecords = new List<GameModeDbfRecord>();

	private GameModeButtonDataModel m_selectedGameModeButtonDataModel;

	private List<long> m_seenGameModes = new List<long>();

	private static bool s_hasAlreadyShownTavernBrawlNewBanner;

	private static GameModeDisplay m_instance;

	private const string GAME_MODE_ACTIVE_EVENT_NAME = "GAME_MODE_ACTIVE";

	private const string GAME_MODE_LOCKED_EVENT_NAME = "GAME_MODE_LOCKED";

	private const string GAME_MODE_DOWNLOAD_REQUIRED_EVENT_NAME = "GAME_MODE_DOWNLOAD_REQUIRED";

	private const string GAME_MODE_DOWNLOADING = "GAME_MODE_DOWNLOADING";

	private static Comparison<GameModeDbfRecord> OrderGameModes = (GameModeDbfRecord a, GameModeDbfRecord b) => a.SortOrder.CompareTo(b.SortOrder);

	private IGameDownloadManager DownloadManager => GameDownloadManagerProvider.Get();

	public bool IsFinishedLoading
	{
		get
		{
			if (m_playButtonFinishedLoading)
			{
				return m_backButtonFinishedLoading;
			}
			return false;
		}
	}

	public static GameModeDisplay Get()
	{
		return m_instance;
	}

	private void Awake()
	{
		m_instance = this;
		DownloadManager.RegisterModuleInstallationStateChangeListener(OnModuleDownloadStateChange, invokeImmediately: false);
	}

	private void OnDestroy()
	{
		DownloadManager.UnregisterModuleInstallationStateChangeListener(OnModuleDownloadStateChange);
	}

	private void Start()
	{
		m_DisplayReference.RegisterReadyListener<Widget>(OnDisplayReady);
		m_PlayButtonReference.RegisterReadyListener<PlayButton>(OnPlayButtonReady);
		m_downloadButtonReference.RegisterReadyListener<UIBButton>(OnDownloadButtonReady);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_BackButtonMobileReference.RegisterReadyListener<UIBButton>(OnBackButtonReady);
		}
		else
		{
			m_BackButtonReference.RegisterReadyListener<UIBButton>(OnBackButtonReady);
		}
		InitializeGameModeSceneData();
		m_slidingTray.OnTransitionComplete += OnSlidingTrayAnimationComplete;
		InitializeSlidingTray();
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_Tournament);
	}

	private void Update()
	{
	}

	public void RegisterOnHideTrayListener(Action action)
	{
		if (m_slidingTray != null)
		{
			m_slidingTray.OnTransitionComplete += action;
		}
	}

	public void UnRegisterOnHideTrayListener(Action action)
	{
		if (m_slidingTray != null)
		{
			m_slidingTray.OnTransitionComplete -= action;
		}
	}

	private void GameModeDisplayEventListener(string eventName)
	{
		switch (eventName)
		{
		case "CHOOSE":
			NavigateToSelectedMode();
			break;
		case "BACK":
			GoToHub();
			break;
		case "GAME_MODE_CLICKED":
			OnGameModeSelected();
			break;
		}
	}

	private void OnDisplayReady(Widget widget)
	{
		if (widget == null)
		{
			Error.AddDevWarning("UI Error!", "DisplayReference could not be found!");
		}
		else
		{
			widget.RegisterEventListener(GameModeDisplayEventListener);
		}
	}

	public void OnPlayButtonReady(PlayButton playButton)
	{
		m_playButtonFinishedLoading = true;
		if (playButton == null)
		{
			Error.AddDevWarning("UI Error!", "PlayButton could not be found! You will not be able to click 'Play'!");
			return;
		}
		m_playButton = playButton;
		m_playButton.AddEventListener(UIEventType.RELEASE, PlayButtonRelease);
		m_playButton.Disable();
	}

	private void OnDownloadButtonReady(UIBButton downloadButton)
	{
		if (downloadButton == null)
		{
			Error.AddDevWarning("UI Error!", "DownloadButton could not be found! You will not be able to click 'Download'!");
			return;
		}
		m_downloadButton = downloadButton;
		m_downloadButton.AddEventListener(UIEventType.PRESS, DownloadButtonPress);
		m_downloadButton.AddEventListener(UIEventType.RELEASE, DownloadButtonRelease);
		m_downloadButton.AddEventListener(UIEventType.ROLLOUT, DownloadButtonRollout);
	}

	public void OnBackButtonReady(UIBButton backButton)
	{
		m_backButtonFinishedLoading = true;
		if (backButton == null)
		{
			Error.AddDevWarning("UI Error!", "BackButton could not be found! You will not be able to click 'Back'!");
			return;
		}
		m_backButton = backButton;
		m_backButton.AddEventListener(UIEventType.RELEASE, BackButtonRelease);
	}

	public GameModeSceneDataModel GetGameModeSceneDataModel()
	{
		VisualController visualController = GetComponent<VisualController>();
		if (visualController == null)
		{
			return null;
		}
		Widget owner = visualController.Owner;
		if (!owner.GetDataModel(173, out var dataModel))
		{
			dataModel = new GameModeSceneDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as GameModeSceneDataModel;
	}

	public EventDataModel GetEventDataModel()
	{
		VisualController visualController = GetComponent<VisualController>();
		if (visualController == null)
		{
			return null;
		}
		return visualController.Owner.GetDataModel<EventDataModel>();
	}

	private void InitializeGameModeSceneData()
	{
		GameModeSceneDataModel dataModel = GetGameModeSceneDataModel();
		if (dataModel == null)
		{
			return;
		}
		EventTimingManager eventTimingManager = EventTimingManager.Get();
		m_activeGameModeRecords.Clear();
		foreach (GameModeDbfRecord record in GameDbf.GameMode.GetRecords())
		{
			if (eventTimingManager.IsEventActive(record.Event))
			{
				m_activeGameModeRecords.Add(record);
			}
		}
		m_activeGameModeRecords.Sort(OrderGameModes);
		GameSaveDataManager gameSaveDataManager = GameSaveDataManager.Get();
		gameSaveDataManager.GetSubkeyValue(GameSaveKeyId.GAME_MODE_SCENE, GameSaveKeySubkeyId.GAME_MODE_SCENE_LAST_SELECTED_GAME_MODE, out long lastSelectedGameModeId);
		dataModel.LastSelectedGameModeRecordId = (int)lastSelectedGameModeId;
		gameSaveDataManager.GetSubkeyValue(GameSaveKeyId.GAME_MODE_SCENE, GameSaveKeySubkeyId.GAME_MODE_SCENE_SEEN_GAME_MODES, out m_seenGameModes);
		if (m_seenGameModes == null)
		{
			m_seenGameModes = new List<long>();
		}
		dataModel.GameModeButtons = new DataModelList<GameModeButtonDataModel>();
		foreach (GameModeDbfRecord gameModeRecord in m_activeGameModeRecords)
		{
			bool isNew = ShouldShowNewBanner(gameModeRecord);
			bool isEarlyAccess = eventTimingManager.IsEventActive(gameModeRecord.ShowAsEarlyAccessEvent);
			bool isBeta = eventTimingManager.IsEventActive(gameModeRecord.ShowAsBetaEvent);
			bool isDownloadRequired = ShouldShowDownloadRequired(gameModeRecord);
			bool isDownloading = ShouldShowDownloadingProgress(gameModeRecord);
			string moduleTag = DownloadTags.GetTagString(GetModuleTagForGameMode(gameModeRecord));
			dataModel.GameModeButtons.Add(new GameModeButtonDataModel
			{
				GameModeRecordId = gameModeRecord.ID,
				Name = gameModeRecord.Name,
				Description = gameModeRecord.Description,
				ButtonState = gameModeRecord.GameModeButtonState,
				IsNew = isNew,
				IsEarlyAccess = isEarlyAccess,
				IsBeta = isBeta,
				IsDownloadRequired = isDownloadRequired,
				IsDownloading = isDownloading,
				ModuleTag = moduleTag
			});
		}
	}

	private bool ShouldShowNewBanner(GameModeDbfRecord gameModeRecord)
	{
		if (gameModeRecord == null)
		{
			Debug.LogError("GameModeDisplay:ShouldShowNewBanner received a null gameModeRecord value");
			return false;
		}
		if (EventTimingManager.Get().IsEventActive(gameModeRecord.ShowAsNewEvent) && !m_seenGameModes.Contains(gameModeRecord.ID))
		{
			return true;
		}
		if (EnumUtils.Parse<SceneMgr.Mode>(gameModeRecord.LinkedScene) == SceneMgr.Mode.ADVENTURE)
		{
			return ShouldSeeNewSoloAdventureBanner();
		}
		return false;
	}

	private bool ShouldShowDownloadRequired(GameModeDbfRecord gameModeRecord)
	{
		DownloadTags.Content moduleTag = GetModuleTagForGameMode(gameModeRecord);
		if (moduleTag != 0)
		{
			return !DownloadManager.IsModuleReadyToPlay(moduleTag);
		}
		return false;
	}

	private bool ShouldShowDownloadingProgress(GameModeDbfRecord gameModeRecord)
	{
		DownloadTags.Content moduleTag = GetModuleTagForGameMode(gameModeRecord);
		if (moduleTag != 0)
		{
			return DownloadManager.IsModuleDownloading(moduleTag);
		}
		return false;
	}

	private bool CanEnterMode(out string reason, out ModeLockedStatus lockedStatus)
	{
		reason = "";
		lockedStatus = ModeLockedStatus.Locked;
		GameModeDbfRecord currentMode = null;
		foreach (GameModeDbfRecord record in m_activeGameModeRecords)
		{
			if (record.ID == m_selectedGameModeButtonDataModel.GameModeRecordId)
			{
				currentMode = record;
				break;
			}
		}
		if (currentMode == null)
		{
			return false;
		}
		NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (features == null)
		{
			reason = GameStrings.Get("GLUE_TOOLTIP_GAME_MODE_DATA_NOT_LOADED");
			return false;
		}
		bool featureFlag = features.Games.GetFeatureFlag((NetCache.NetCacheFeatures.CacheGames.FeatureFlags)currentMode.FeatureUnlockId);
		bool featureFlag2Unlocked = currentMode.FeatureUnlockId2 == 0 || features.Games.GetFeatureFlag((NetCache.NetCacheFeatures.CacheGames.FeatureFlags)currentMode.FeatureUnlockId2);
		if (!featureFlag && !featureFlag2Unlocked)
		{
			reason = GameStrings.Get("GLUE_TOOLTIP_BUTTON_DISABLED_DESC");
			return false;
		}
		if (!GameModeUtils.HasUnlockedMode((Global.UnlockableGameMode)currentMode.ModeKey))
		{
			reason = GameStrings.Format("GLUE_GAME_MODE_LOCKED_APPRENTICE_COMPLETION_REQUIRED", m_selectedGameModeButtonDataModel.Name);
			return false;
		}
		DownloadTags.Content moduleTag = GetModuleTagForGameMode(currentMode);
		if (moduleTag != 0)
		{
			if (DownloadManager.IsModuleDownloading(moduleTag))
			{
				reason = GameStrings.Format("GLUE_GAME_MODE_LOCKED_DOWNLOAD_REQUIRED", DownloadUtils.GetGameModeName(moduleTag));
				lockedStatus = ModeLockedStatus.Downloading;
				return false;
			}
			if (!DownloadManager.IsModuleReadyToPlay(moduleTag))
			{
				reason = GameStrings.Format("GLUE_GAME_MODE_LOCKED_DOWNLOAD_REQUIRED", DownloadUtils.GetGameModeName(moduleTag));
				lockedStatus = ModeLockedStatus.DownloadRequired;
				return false;
			}
		}
		lockedStatus = ModeLockedStatus.Unlocked;
		return true;
	}

	private void HandleModuleDownload()
	{
		if (m_selectedGameModeButtonDataModel == null)
		{
			Log.All.PrintError("No game mode selected!");
			return;
		}
		GameModeDbfRecord selectedGameMode = GameDbf.GameMode.GetRecord(m_selectedGameModeButtonDataModel.GameModeRecordId);
		if (selectedGameMode == null)
		{
			Log.All.PrintError($"Game mode with invalid id {m_selectedGameModeButtonDataModel.GameModeRecordId} selected!");
			return;
		}
		DownloadTags.Content content = GetModuleTagForGameMode(selectedGameMode);
		if (content == DownloadTags.Content.Unknown)
		{
			Log.All.PrintError("Module tag not found for a selected mode : " + selectedGameMode.LinkedScene);
		}
		else if (!DownloadManager.IsModuleRequested(content))
		{
			StartCoroutine(ShowDownloadConfirmationPopup(content));
		}
		else
		{
			Log.All.PrintInfo($"Module tag : {content} already downloading!");
		}
	}

	private IEnumerator ShowDownloadConfirmationPopup(DownloadTags.Content content)
	{
		if (m_downloadConfirmationPopup == null)
		{
			m_downloadPopupWidget.RegisterReadyListener(delegate
			{
				m_downloadConfirmationPopup = m_downloadPopupWidget.GetComponentInChildren<DownloadConfirmationPopup>(includeInactive: true);
			});
		}
		while (m_downloadConfirmationPopup == null)
		{
			yield return null;
		}
		DownloadConfirmationPopup.DownloadConfirmationPopupData confirmationPopupData = new DownloadConfirmationPopup.DownloadConfirmationPopupData(content, OnDownloadPopupYesClicked, OnDownloadPopupNoClicked);
		m_downloadConfirmationPopup.Init(confirmationPopupData);
		UIContext.GetRoot().ShowPopup(m_downloadPopupWidget.gameObject);
		m_downloadPopupWidget.TriggerEvent("SHOW");
	}

	private void OnDownloadPopupYesClicked(DownloadTags.Content moduleTag)
	{
		if (moduleTag != 0)
		{
			DownloadManager.DownloadModule(moduleTag);
			m_selectedGameModeButtonDataModel.IsDownloading = true;
			m_gameModeButtonController.SetState("GAME_MODE_DOWNLOADING");
			m_playButton.Disable(keepLabelTextVisible: true);
		}
		DismissPopup();
	}

	private void OnDownloadPopupNoClicked(DownloadTags.Content moduleTag)
	{
		DismissPopup();
	}

	private void DismissPopup()
	{
		m_downloadPopupWidget.TriggerEvent("HIDE");
		UIContext.GetRoot().DismissPopup(m_downloadPopupWidget.gameObject);
	}

	private void OnModuleDownloadStateChange(DownloadTags.Content moduleTag, ModuleState state)
	{
		foreach (GameModeButtonDataModel gameModeButtonData in GetGameModeSceneDataModel().GameModeButtons)
		{
			if (DownloadTags.GetContentTag(gameModeButtonData.ModuleTag) == moduleTag && state == ModuleState.ReadyToPlay)
			{
				gameModeButtonData.IsDownloading = false;
				gameModeButtonData.IsDownloadRequired = false;
			}
		}
		if (m_selectedGameModeButtonDataModel != null && DownloadTags.GetContentTag(m_selectedGameModeButtonDataModel.ModuleTag) == moduleTag && state >= ModuleState.ReadyToPlay)
		{
			m_gameModeButtonController.SetState("GAME_MODE_ACTIVE");
			m_playButton.Enable();
		}
	}

	private DownloadTags.Content GetModuleTagForGameMode(GameModeDbfRecord gameModeRecord)
	{
		if (gameModeRecord == null)
		{
			Debug.LogError("GameModeDisplay:GetModuleTagForGameMode received a null gameModeRecord value");
			return DownloadTags.Content.Unknown;
		}
		return EnumUtils.Parse<SceneMgr.Mode>(gameModeRecord.LinkedScene) switch
		{
			SceneMgr.Mode.ADVENTURE => DownloadTags.Content.Adventure, 
			SceneMgr.Mode.LETTUCE_VILLAGE => DownloadTags.Content.Merc, 
			_ => DownloadTags.Content.Unknown, 
		};
	}

	private void InitializeSlidingTray()
	{
		bool startHidden = SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.HUB;
		m_slidingTray.ToggleTraySlider(startHidden, null, animate: false);
	}

	private void PlayButtonRelease(UIEvent e)
	{
		NavigateToSelectedMode();
	}

	private void DownloadButtonPress(UIEvent e)
	{
		SendEventDownwardStateAction.SendEventDownward(m_downloadButton.gameObject, "PRESSED", SendEventDownwardStateAction.BubbleDownEventDepth.DirectChildrenOnly);
	}

	private void DownloadButtonRelease(UIEvent e)
	{
		SendEventDownwardStateAction.SendEventDownward(m_downloadButton.gameObject, "RELEASED", SendEventDownwardStateAction.BubbleDownEventDepth.DirectChildrenOnly);
		HandleModuleDownload();
	}

	private void DownloadButtonRollout(UIEvent e)
	{
		SendEventDownwardStateAction.SendEventDownward(m_downloadButton.gameObject, "ENABLED", SendEventDownwardStateAction.BubbleDownEventDepth.DirectChildrenOnly);
	}

	private void BackButtonRelease(UIEvent e)
	{
		GoToHub();
	}

	private void GoToHub()
	{
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
	}

	private void NavigateToSelectedMode()
	{
		m_playButton.Disable();
		if (m_selectedGameModeButtonDataModel == null)
		{
			Log.All.PrintError("No game mode selected!");
			return;
		}
		if (!CanEnterMode(out var lockedReason, out var _))
		{
			ShowDisabledPopupForCurrentMode(lockedReason);
			return;
		}
		GameModeDbfRecord selectedGameMode = GameDbf.GameMode.GetRecord(m_selectedGameModeButtonDataModel.GameModeRecordId);
		if (selectedGameMode == null)
		{
			Log.All.PrintError($"Game mode with invalid id {m_selectedGameModeButtonDataModel.GameModeRecordId} selected!");
			return;
		}
		if (!m_seenGameModes.Contains(selectedGameMode.ID))
		{
			m_seenGameModes.Add(selectedGameMode.ID);
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.GAME_MODE_SCENE, GameSaveKeySubkeyId.GAME_MODE_SCENE_SEEN_GAME_MODES, m_seenGameModes.ToArray()));
		}
		SceneMgr.Mode sceneToGoTo = EnumUtils.Parse<SceneMgr.Mode>(selectedGameMode.LinkedScene);
		switch (sceneToGoTo)
		{
		case SceneMgr.Mode.DRAFT:
		{
			ulong secondsUntilEndOfSeason = DraftManager.Get().SecondsUntilEndOfSeason;
			NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
			if (!DraftManager.Get().HasActiveRun && secondsUntilEndOfSeason <= features.ArenaClosedToNewSessionsSeconds)
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
				info.m_headerText = GameStrings.Get("GLUE_ARENA_1ST_TIME_HEADER");
				info.m_text = GameStrings.Get("GLUE_ARENA_SIGNUPS_CLOSED");
				info.m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle;
				info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
				DialogManager.Get().ShowPopup(info);
				return;
			}
			break;
		}
		case SceneMgr.Mode.LETTUCE_VILLAGE:
		{
			NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
			if (playerInfo == null)
			{
				Debug.LogWarning("GameModeDisplay: Mercenaries Player info has not loaded yet.");
				return;
			}
			if (!GameUtils.IsMercenariesPrologueBountyComplete(playerInfo))
			{
				StartMercenariesTutorial();
				return;
			}
			NarrativeManager.Get().PreloadMercenaryTutorialDialogue();
			break;
		}
		}
		m_clickBlocker.SetActive(value: true);
		SceneMgr.Get().SetNextMode(sceneToGoTo, SceneMgr.TransitionHandlerType.CURRENT_SCENE, OnSceneLoadCompleteHandleTransition);
		if (sceneToGoTo == SceneMgr.Mode.DRAFT)
		{
			AchieveManager.Get().NotifyOfClick(Achievement.ClickTriggerType.BUTTON_ARENA);
		}
	}

	private void StartMercenariesTutorial()
	{
		LettuceBountySetDbfRecord prologueRecord = GameDbf.LettuceBountySet.GetRecord((LettuceBountySetDbfRecord r) => r.IsTutorial && EventTimingManager.Get().IsEventActive(r.Event));
		LettuceVillageDisplay.LettuceSceneTransitionPayload sceneTransitionPayload = new LettuceVillageDisplay.LettuceSceneTransitionPayload
		{
			m_SelectedBountySet = prologueRecord,
			m_SelectedBounty = GameDbf.LettuceBounty.GetRecord((LettuceBountyDbfRecord r) => r.BountySetId == prologueRecord.ID)
		};
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.LETTUCE_MAP, SceneMgr.TransitionHandlerType.CURRENT_SCENE, OnSceneLoadCompleteHandleTransition, sceneTransitionPayload);
	}

	public static bool ShouldSeeNewSoloAdventureBanner()
	{
		NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		bool hasNewAdventure = AdventureConfig.GetAdventurePlayerShouldSee() != AdventureDbId.INVALID;
		bool shouldShowNewAdventure = GameModeUtils.HasUnlockedAllDefaultHeroes() && hasNewAdventure;
		return netObject.Games.Practice && shouldShowNewAdventure;
	}

	public static bool ShouldSeeNewTavernBrawlBanner()
	{
		TavernBrawlManager tavernBrawlManager = TavernBrawlManager.Get();
		if (s_hasAlreadyShownTavernBrawlNewBanner || tavernBrawlManager == null)
		{
			return false;
		}
		string reason;
		if (tavernBrawlManager.IsFirstTimeSeeingCurrentSeason)
		{
			return tavernBrawlManager.CanEnterStandardTavernBrawl(out reason);
		}
		return false;
	}

	private void OnSceneLoadCompleteHandleTransition(Action onTransitionComplete)
	{
		m_onSceneTransitionCompleteCallback = onTransitionComplete;
		m_slidingTray.HideTray();
	}

	public void ShowSlidingTrayAfterSceneLoad(Action onCompleteCallback)
	{
		m_clickBlocker.SetActive(value: true);
		m_onSceneTransitionCompleteCallback = onCompleteCallback;
		m_slidingTray.ShowTray();
	}

	private void OnSlidingTrayAnimationComplete()
	{
		m_clickBlocker.SetActive(value: false);
		if (m_onSceneTransitionCompleteCallback != null)
		{
			m_onSceneTransitionCompleteCallback();
			m_onSceneTransitionCompleteCallback = null;
		}
	}

	private void OnGameModeSelected()
	{
		EventDataModel eventDataModel = GetEventDataModel();
		if (eventDataModel == null)
		{
			Log.All.PrintError("No event data model attached to the GameModeDisplay.");
			return;
		}
		m_selectedGameModeButtonDataModel = (GameModeButtonDataModel)eventDataModel.Payload;
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.GAME_MODE_SCENE, GameSaveKeySubkeyId.GAME_MODE_SCENE_LAST_SELECTED_GAME_MODE, m_selectedGameModeButtonDataModel.GameModeRecordId));
		GameModeSceneDataModel dataModel = GetGameModeSceneDataModel();
		if (dataModel != null)
		{
			dataModel.LastSelectedGameModeRecordId = m_selectedGameModeButtonDataModel.GameModeRecordId;
		}
		if (!CanEnterMode(out var lockedReason, out var lockedStatus))
		{
			m_lockedNameText.Text = GameStrings.Format(lockedReason, m_selectedGameModeButtonDataModel.Name);
			switch (lockedStatus)
			{
			case ModeLockedStatus.DownloadRequired:
				m_selectedGameModeButtonDataModel.IsDownloadRequired = true;
				m_gameModeButtonController.SetState("GAME_MODE_DOWNLOAD_REQUIRED");
				break;
			case ModeLockedStatus.Downloading:
				m_selectedGameModeButtonDataModel.IsDownloading = true;
				m_gameModeButtonController.SetState("GAME_MODE_DOWNLOADING");
				m_playButton.Disable(keepLabelTextVisible: true);
				break;
			default:
				m_playButton.Disable(keepLabelTextVisible: true);
				m_gameModeButtonController.SetState("GAME_MODE_LOCKED");
				break;
			}
		}
		else
		{
			m_playButton.Enable();
			m_gameModeButtonController.SetState("GAME_MODE_ACTIVE");
		}
	}

	private void ShowDisabledPopupForCurrentMode(string lockReason)
	{
		if (!string.IsNullOrEmpty(lockReason))
		{
			string header = GameStrings.Get(m_selectedGameModeButtonDataModel.Name);
			ShowDisabledPopup(header, lockReason);
		}
	}

	private void ShowDisabledPopup(string header, string description)
	{
		if (string.IsNullOrEmpty(description))
		{
			description = GameStrings.Get("GLUE_TOOLTIP_BUTTON_DISABLED_DESC");
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = header,
			m_text = description,
			m_showAlertIcon = true,
			m_responseDisplay = AlertPopup.ResponseDisplay.OK
		};
		DialogManager.Get().ShowPopup(info);
	}
}
