using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class LettucePlayDisplay : AbsSceneDisplay, IMercDetailsDisplayProvider
{
	private static List<MercenariesRankedSeasonRewardRankDbfRecord> s_sortedRewardRecords;

	public AsyncReference m_PlayButtonReference;

	public AsyncReference m_PlayButtonPhoneReference;

	public AsyncReference m_BackButtonReference;

	public AsyncReference m_BackButtonPhoneReference;

	public AsyncReference m_collectionButtonReference;

	public AsyncReference m_TeamListDisplay;

	public AsyncReference m_TeamPreviewReference;

	public AsyncReference m_TeamPreviewPhoneReference;

	public AsyncReference m_TeamDisplayTrayMobileReference;

	public AsyncReference m_RewardChestReference;

	public AsyncReference m_RewardChestPhoneReference;

	public AsyncReference m_MercDetailsDisplayReference;

	public Vector3 m_RewardListPopupLocalPosition = new Vector3(0f, 100f, -3f);

	public float m_RewardListPopupLocalScale = 13.5f;

	public float m_delayBeforeChestAnimation = 2f;

	public float m_chestAnimationTime = 3f;

	private PlayButton m_playButton;

	private UIBButton m_backButton;

	private UIBButton m_collectionButton;

	private VisualController m_teamListVisualController;

	private Widget m_teamPreviewWidget;

	private VisualController m_teamDisplayTray;

	private VisualController m_rewardChestVisualController;

	private WidgetInstance m_seasonRewardsPopup;

	private bool m_showingRewardsPopup;

	private int m_highRatingTierIndex;

	private bool m_playButtonFinishedLoading;

	private bool m_backButtonFinishedLoading;

	private bool m_collectionButtonFinishedLoading;

	private bool m_teamListDisplayFinishedLoading;

	private bool m_teamPreviewFinishedLoading;

	private bool m_teamDisplayTrayFinishedLoading;

	private bool m_rewardChestFinishedLoading;

	private bool m_playerInfoReceived;

	private LettuceTeam m_selectedTeam;

	private LettuceTeamDataModel m_selectedTeamDataModel;

	private bool m_blockingPopupDisplayManager;

	public static List<MercenariesRankedSeasonRewardRankDbfRecord> SortedRewardRecords
	{
		get
		{
			if (s_sortedRewardRecords == null)
			{
				s_sortedRewardRecords = (from r in GameDbf.MercenariesRankedSeasonRewardRank.GetRecords()
					orderby r.MinPublicRatingUnlock
					select r).ToList();
			}
			return s_sortedRewardRecords;
		}
	}

	public MercenaryDetailDisplay MercenaryDetailDisplay { get; private set; }

	public override void Start()
	{
		base.Start();
		GameMgr.Get().RegisterFindGameEvent(OnFindGameEvent);
		m_sceneDisplayWidgetReference.RegisterReadyListener<Widget>(OnSceneDisplayWidgetReady);
		m_collectionButtonReference.RegisterReadyListener<UIBButton>(OnCollectionButtonReady);
		m_TeamListDisplay.RegisterReadyListener<VisualController>(OnTeamListDisplayReady);
		m_MercDetailsDisplayReference.RegisterReadyListener<MercenaryDetailDisplay>(OnMercDetailsDisplayReady);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_PlayButtonPhoneReference.RegisterReadyListener<PlayButton>(OnPlayButtonReady);
			m_BackButtonPhoneReference.RegisterReadyListener<UIBButton>(OnBackButtonReady);
			m_TeamPreviewPhoneReference.RegisterReadyListener<Widget>(OnTeamPreviewReady);
			m_TeamDisplayTrayMobileReference.RegisterReadyListener<VisualController>(OnTeamDisplayTrayReady);
			m_RewardChestPhoneReference.RegisterReadyListener<VisualController>(OnRewardChestReady);
		}
		else
		{
			m_PlayButtonReference.RegisterReadyListener<PlayButton>(OnPlayButtonReady);
			m_BackButtonReference.RegisterReadyListener<UIBButton>(OnBackButtonReady);
			m_TeamPreviewReference.RegisterReadyListener<Widget>(OnTeamPreviewReady);
			m_RewardChestReference.RegisterReadyListener<VisualController>(OnRewardChestReady);
		}
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_MercenariesPVPLobby);
		Network.Get().MercenariesPlayerInfoRequest();
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheMercenariesPlayerInfo), OnPlayerInfoReceived);
		CollectionManager.Get().MercenaryArtVariationChangedEvent += OnMercenaryArtVariationChangedEvent;
		StartCoroutine(InitializeWhenReady());
	}

	private void OnDestroy()
	{
		CollectionManager collectionMgr = CollectionManager.Get();
		if (collectionMgr != null)
		{
			collectionMgr.MercenaryArtVariationChangedEvent -= OnMercenaryArtVariationChangedEvent;
		}
		NetCache.Get()?.RemoveUpdatedListener(typeof(NetCache.NetCacheMercenariesPlayerInfo), OnPlayerInfoReceived);
		GameMgr.Get()?.UnregisterFindGameEvent(OnFindGameEvent);
		if (m_seasonRewardsPopup != null)
		{
			Object.Destroy(m_seasonRewardsPopup.gameObject);
		}
	}

	private void TeamListEventListener(string eventName)
	{
		if (eventName == "TEAM_SELECTED")
		{
			OnTeamSelected();
		}
	}

	private void PVPRatingEventListener(string eventName)
	{
		if (eventName == "PVP_RATING_CLICKED_code" && IsFinishedLoading(out var _) && !m_showingRewardsPopup)
		{
			m_showingRewardsPopup = true;
			StartCoroutine(ShowSeasonRewardsPopup());
		}
	}

	private IEnumerator ShowSeasonRewardsPopup()
	{
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (playerInfo == null)
		{
			Log.Lettuce.PrintError("InitializeChestData - No mercenaries player info in NetCache.");
			yield break;
		}
		if (m_seasonRewardsPopup == null)
		{
			m_seasonRewardsPopup = WidgetInstance.Create("MercenaryRewardListPopup.prefab:5df60e3fc26ac554685cbc730ea0a6ba");
			m_seasonRewardsPopup.RegisterReadyListener(OnRewardsPopupReady);
			m_seasonRewardsPopup.WillLoadSynchronously = true;
			m_seasonRewardsPopup.Initialize();
			MercenaryRewardListPopupDataModel dataModel = new MercenaryRewardListPopupDataModel
			{
				Title = GameUtils.GetMercenariesSeasonName(playerInfo.PvpSeasonId)
			};
			bool prevEarned = true;
			foreach (MercenariesRankedSeasonRewardRankDbfRecord record in SortedRewardRecords)
			{
				MercenaryRewardListPopupTierDataModel tierDataModel = new MercenaryRewardListPopupTierDataModel
				{
					Rating = record.MinPublicRatingUnlock.ToString(),
					Earned = (playerInfo.PvpSeasonHighestRating >= record.MinPublicRatingUnlock),
					IsNextTier = (prevEarned && playerInfo.PvpSeasonHighestRating < record.MinPublicRatingUnlock)
				};
				prevEarned = tierDataModel.Earned;
				dataModel.Tiers.Add(tierDataModel);
			}
			m_seasonRewardsPopup.BindDataModel(dataModel);
		}
		while (m_seasonRewardsPopup.IsChangingStates)
		{
			yield return null;
		}
		UIContext.GetRoot().ShowPopup(m_seasonRewardsPopup.gameObject);
		m_seasonRewardsPopup.Show();
		m_seasonRewardsPopup.TriggerEvent("SHOW");
		m_seasonRewardsPopup.GetComponentInChildren<RewardListAutoScroller>().Init(m_seasonRewardsPopup, m_highRatingTierIndex);
	}

	private void OnRewardsPopupReady(object o)
	{
		OverlayUI.Get().AddGameObject(m_seasonRewardsPopup.gameObject);
		m_seasonRewardsPopup.transform.localPosition = m_RewardListPopupLocalPosition;
		m_seasonRewardsPopup.transform.localScale = Vector3.one * m_RewardListPopupLocalScale;
		m_seasonRewardsPopup.RegisterEventListener(RewardsPopupEventHandler);
		m_seasonRewardsPopup.Hide();
	}

	private void RewardsPopupEventHandler(string eventName)
	{
		if (eventName == "HIDE")
		{
			UIContext.GetRoot().DismissPopup(m_seasonRewardsPopup.gameObject);
			m_showingRewardsPopup = false;
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
		m_playButton.AddEventListener(UIEventType.RELEASE, OnPlayButtonRelease);
		m_playButton.Disable(keepLabelTextVisible: true);
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
		m_backButton.AddEventListener(UIEventType.RELEASE, OnBackButtonRelease);
	}

	public void OnCollectionButtonReady(UIBButton collectionButton)
	{
		m_collectionButtonFinishedLoading = true;
		if (collectionButton == null)
		{
			Error.AddDevWarning("UI Error!", "CollectionButton could not be found! You will not be able to click 'Mercenary Collection'!");
			return;
		}
		m_collectionButton = collectionButton;
		m_collectionButton.AddEventListener(UIEventType.RELEASE, OnCollectionButtonRelease);
	}

	public void OnTeamListDisplayReady(VisualController visualController)
	{
		if (visualController != null)
		{
			visualController.GetComponent<Widget>().RegisterEventListener(TeamListEventListener);
		}
		m_teamListVisualController = visualController;
		m_teamListDisplayFinishedLoading = true;
	}

	private void OnMercDetailsDisplayReady(MercenaryDetailDisplay display)
	{
		MercenaryDetailDisplay = display;
	}

	public void OnTeamPreviewReady(Widget preview)
	{
		if (preview == null)
		{
			Error.AddDevWarning("UI Error!", "TeamPreview could not be found!");
			return;
		}
		m_teamPreviewWidget = preview;
		m_teamPreviewFinishedLoading = true;
		PopulateTeamPreviewData(new LettuceTeam());
	}

	public void OnTeamDisplayTrayReady(VisualController teamDisplayTray)
	{
		m_teamDisplayTrayFinishedLoading = true;
		if (teamDisplayTray == null)
		{
			Error.AddDevWarning("UI Error!", "Team Display Tray could not be found!");
		}
		else
		{
			m_teamDisplayTray = teamDisplayTray;
		}
	}

	public void OnRewardChestReady(VisualController visualController)
	{
		if (visualController == null)
		{
			Error.AddDevWarning("UI Error!", "FinalBossChest could not be found!");
			return;
		}
		m_rewardChestVisualController = visualController;
		m_rewardChestFinishedLoading = true;
	}

	private void OnSceneDisplayWidgetReady(Widget widget)
	{
		widget.RegisterEventListener(PVPRatingEventListener);
	}

	public void OnPlayerInfoReceived()
	{
		m_playerInfoReceived = true;
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		FindGameState state = eventData.m_state;
		if ((uint)(state - 2) <= 1u || (uint)(state - 7) <= 1u || state == FindGameState.SERVER_GAME_CANCELED)
		{
			m_playButton.Enable();
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.MERCENARIES_PLAY_SCREEN);
		}
		return false;
	}

	public override bool IsFinishedLoading(out string failureMessage)
	{
		if (!m_playButtonFinishedLoading)
		{
			failureMessage = "LettucePlayDisplay - Play button never loaded.";
			return false;
		}
		if (!m_backButtonFinishedLoading)
		{
			failureMessage = "LettucePlayDisplay - Back button never loaded.";
			return false;
		}
		if (!m_collectionButtonFinishedLoading)
		{
			failureMessage = "LettucePlayDisplay - Collection button never loaded.";
			return false;
		}
		if (!m_teamListDisplayFinishedLoading)
		{
			failureMessage = "LettucePlayDisplay - Team list display never loaded.";
			return false;
		}
		if (!m_teamPreviewFinishedLoading)
		{
			failureMessage = "LettucePlayDisplay - Team preview button never loaded.";
			return false;
		}
		if (!m_playerInfoReceived)
		{
			failureMessage = "LettucePlayDisplay - Player Info never received.";
			return false;
		}
		if ((bool)UniversalInputManager.UsePhoneUI && !m_teamDisplayTrayFinishedLoading)
		{
			failureMessage = "LettucePlayDisplay - Team display tray never loaded.";
			return false;
		}
		if (!m_rewardChestFinishedLoading)
		{
			failureMessage = "LettucePlayDisplay - Reward chest never loaded.";
			return false;
		}
		failureMessage = string.Empty;
		return true;
	}

	private void OnPlayButtonRelease(UIEvent e)
	{
		if (!Network.IsLoggedIn())
		{
			DialogManager.Get().ShowReconnectHelperDialog();
			return;
		}
		if (m_selectedTeam == null)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_LETTUCE_VALIDATE_TEAM_HEADER");
			info.m_text = GameStrings.Get("GLUE_LETTUCE_VALIDATE_NO_TEAM_SELECTED");
			info.m_showAlertIcon = true;
			info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info.m_okText = GameStrings.Get("GLOBAL_OKAY");
			DialogManager.Get().ShowPopup(info);
			return;
		}
		if (!m_selectedTeam.IsValid())
		{
			AlertPopup.PopupInfo info2 = new AlertPopup.PopupInfo();
			info2.m_headerText = GameStrings.Get("GLUE_LETTUCE_VALIDATE_TEAM_HEADER");
			info2.m_text = GameStrings.Get("GLUE_LETTUCE_VALIDATE_TEAM_INVALID");
			info2.m_showAlertIcon = true;
			info2.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info2.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info2.m_okText = GameStrings.Get("GLOBAL_OKAY");
			DialogManager.Get().ShowPopup(info2);
			return;
		}
		PresenceMgr.Get().SetStatus(Global.PresenceStatus.MERCENARIES_QUEUE);
		m_playButton.Disable();
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_teamDisplayTray.Owner.TriggerEvent("HIDE");
		}
		MercenaryDetailDisplay.Hide();
		GameMgr gameMgr = GameMgr.Get();
		long deckId = 0L;
		long iD = m_selectedTeam.ID;
		gameMgr.FindGame(GameType.GT_MERCENARIES_PVP, FormatType.FT_WILD, 3743, 0, deckId, null, null, restoreSavedGameState: false, null, null, iD);
	}

	private void OnBackButtonRelease(UIEvent e)
	{
		SetNextModeAndHandleTransition(SceneMgr.Mode.LETTUCE_VILLAGE, SceneMgr.TransitionHandlerType.CURRENT_SCENE);
	}

	private void OnCollectionButtonRelease(UIEvent e)
	{
		if (!Network.IsLoggedIn())
		{
			DialogManager.Get().ShowReconnectHelperDialog();
		}
		else
		{
			SetNextModeAndHandleTransition(SceneMgr.Mode.LETTUCE_COLLECTION, SceneMgr.TransitionHandlerType.CURRENT_SCENE);
		}
	}

	protected override bool ShouldStartShown()
	{
		if (SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LETTUCE_COLLECTION && SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.COLLECTIONMANAGER)
		{
			return SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LETTUCE_VILLAGE;
		}
		return false;
	}

	private IEnumerator InitializeWhenReady()
	{
		string failureMessage;
		while (!IsFinishedLoading(out failureMessage))
		{
			yield return null;
		}
		InitializeChestData();
		InitializeTeamListDataModel();
		RewardPopups rewardPopups = PopupDisplayManager.Get().RewardPopups;
		bool hasMercenariesRewards = rewardPopups.HasNonAutoRetireMercenariesRewardsToShow();
		NetCache.ProfileNoticeMercenariesSeasonRewards seasonRewardsNotice = rewardPopups.GetNextMercenariesSeasonRewardsNotice();
		if (hasMercenariesRewards || seasonRewardsNotice != null)
		{
			StartCoroutine(ShowMercenariesRewards(hasMercenariesRewards, seasonRewardsNotice));
		}
	}

	private LettuceTeamListDataModel GetTeamListDataModel()
	{
		VisualController visualController = GetComponent<VisualController>();
		if (visualController == null)
		{
			return null;
		}
		Widget owner = visualController.Owner;
		if (!owner.GetDataModel(218, out var dataModel))
		{
			dataModel = new LettuceTeamListDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as LettuceTeamListDataModel;
	}

	private LettuceTeamDataModel GetTeamPreviewDataModel()
	{
		if (m_teamPreviewWidget == null)
		{
			return null;
		}
		if (!m_teamPreviewWidget.GetDataModel(217, out var dataModel))
		{
			dataModel = new LettuceTeamDataModel();
			m_teamPreviewWidget.BindDataModel(dataModel);
		}
		return dataModel as LettuceTeamDataModel;
	}

	private void InitializeTeamListDataModel()
	{
		LettuceTeamListDataModel teamListDataModel = GetTeamListDataModel();
		bool skipAutoTeamSelection = (bool)UniversalInputManager.UsePhoneUI && SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.GAMEPLAY;
		CollectionUtils.PopulateMercenariesTeamListDataModel(teamListDataModel, !skipAutoTeamSelection);
	}

	private void InitializeChestData()
	{
		VisualController playDisplayVisualController = GetComponent<VisualController>();
		if (!(playDisplayVisualController != null))
		{
			return;
		}
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (playerInfo == null)
		{
			Log.Lettuce.PrintError("InitializeChestData - No mercenaries player info in NetCache.");
			return;
		}
		float progress = (float)playerInfo.PvpRewardChestWinsProgress / (float)playerInfo.PvpRewardChestWinsRequired;
		m_highRatingTierIndex = 0;
		using (List<MercenariesRankedSeasonRewardRankDbfRecord>.Enumerator enumerator = SortedRewardRecords.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.MinPublicRatingUnlock <= playerInfo.PvpSeasonHighestRating)
			{
				m_highRatingTierIndex++;
			}
		}
		LettucePlayDisplayDataModel lettucePlayDisplayDataModel = new LettucePlayDisplayDataModel();
		lettucePlayDisplayDataModel.ChestCurrentWins = (int)playerInfo.PvpRewardChestWinsProgress;
		lettucePlayDisplayDataModel.ChestMaxWins = (int)playerInfo.PvpRewardChestWinsRequired;
		lettucePlayDisplayDataModel.ChestProgressPercent = progress;
		lettucePlayDisplayDataModel.ChestProgressBarText = GameStrings.Format("GLUE_LETTUCE_PVP_CHEST_PROGRESS_BAR_TEXT", playerInfo.PvpRewardChestWinsProgress, playerInfo.PvpRewardChestWinsRequired);
		lettucePlayDisplayDataModel.Rating = playerInfo.PvpRating;
		lettucePlayDisplayDataModel.HighRatingTierIndex = m_highRatingTierIndex;
		LettucePlayDisplayDataModel dataModel = lettucePlayDisplayDataModel;
		playDisplayVisualController.BindDataModel(dataModel);
	}

	private void PopulateTeamPreviewData(LettuceTeam team)
	{
		if (team != null)
		{
			CollectionUtils.PopulateTeamPreviewData(GetTeamPreviewDataModel(), team, null, populateCards: false);
		}
	}

	private void OnTeamSelected()
	{
		EventDataModel eventDataModel = WidgetUtils.GetEventDataModel(m_teamListVisualController);
		if (eventDataModel == null)
		{
			Log.Lettuce.PrintError("No event data model attached to the LettucePlayDisplay");
			return;
		}
		LettuceTeamDataModel teamData = (LettuceTeamDataModel)eventDataModel.Payload;
		m_selectedTeam = CollectionManager.Get().GetTeam(teamData.TeamId);
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_LAST_SELECTED_PVP_TEAM, teamData.TeamId));
		PopulateTeamPreviewData(m_selectedTeam);
		if (m_selectedTeam.IsValid() && !m_selectedTeam.DoesContainDisabledMerc())
		{
			m_playButton.Enable();
			return;
		}
		m_playButton.Disable();
		if (!m_selectedTeam.DoesContainDisabledMerc())
		{
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_LETTUCE_DISABLED_TEAM_HEADER");
		info.m_text = GameStrings.Get("GLUE_LETTUCE_DISABLED_TEAM");
		info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_cancelText = GameStrings.Get("GLOBAL_BUTTON_NO");
		info.m_confirmText = GameStrings.Get("GLUE_EDIT_TEAM");
		info.m_blurWhenShown = true;
		info.m_responseCallback = delegate(AlertPopup.Response response, object userData)
		{
			if (response == AlertPopup.Response.CONFIRM)
			{
				OnCollectionButtonRelease(null);
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	private IEnumerator ShowMercenariesRewards(bool hasMercenariesRewards, NetCache.ProfileNoticeMercenariesSeasonRewards seasonRewardsNotice)
	{
		m_clickBlocker.SetActive(value: true);
		m_blockingPopupDisplayManager = true;
		while (SceneMgr.Get().IsTransitionNowOrPending())
		{
			yield return null;
		}
		yield return new WaitForSeconds(m_delayBeforeChestAnimation);
		while (PopupDisplayManager.Get().IsShowing)
		{
			yield return null;
		}
		if (hasMercenariesRewards)
		{
			m_rewardChestVisualController.SetState("OPEN_REWARD");
			yield return new WaitForSeconds(m_chestAnimationTime);
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_teamDisplayTray.Owner.TriggerEvent("HIDE");
			}
			NetCache.ProfileNoticeMercenariesRewards rewardNotice = PopupDisplayManager.Get().RewardPopups.GetNextNonAutoRetireRewardMercenariesRewardToShow();
			NetCache.ProfileNoticeMercenariesRewards bonusRewardNotice = PopupDisplayManager.Get().RewardPopups.GetNextBonusMercenariesRewardToShow();
			PopupDisplayManager.Get().RewardPopups.ShowMercenariesRewards(autoOpenChest: true, rewardNotice, bonusRewardNotice);
			while (PopupDisplayManager.Get().IsShowing)
			{
				yield return null;
			}
			m_rewardChestVisualController.SetState("CLOSE_REWARD");
		}
		if (seasonRewardsNotice != null)
		{
			DialogManager.Get().ShowMercenariesSeasonRewardsDialog(seasonRewardsNotice);
		}
		m_clickBlocker.SetActive(value: false);
		m_blockingPopupDisplayManager = false;
	}

	public override bool IsBlockingPopupDisplayManager()
	{
		return m_blockingPopupDisplayManager;
	}

	private void OnMercenaryArtVariationChangedEvent(int mercenaryDbId, int artVariationId, TAG_PREMIUM premium)
	{
		foreach (LettuceMercenary merc in m_selectedTeam.GetMercs())
		{
			if (merc.ID == mercenaryDbId)
			{
				CollectionUtils.PopulateTeamPreviewData(GetTeamPreviewDataModel(), m_selectedTeam, null, populateCards: false);
				break;
			}
		}
	}

	public void ShowMercDetailsDisplay(LettuceMercenary mercenary)
	{
		if (m_selectedTeam != null)
		{
			LettuceTeamDataModel selectedTeamDataModel = GetTeamPreviewDataModel();
			CollectionUtils.PopulateMercenariesTeamDataModel(selectedTeamDataModel, m_selectedTeam);
			MercenaryDetailDisplay.GetComponent<Widget>().BindDataModel(selectedTeamDataModel);
		}
		MercenaryDetailDisplay.Show(mercenary, UniversalInputManager.UsePhoneUI ? "SHOW_PARTIAL" : "SHOW_FULL", m_selectedTeam);
	}
}
