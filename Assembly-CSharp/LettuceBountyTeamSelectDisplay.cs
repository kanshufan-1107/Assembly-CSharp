using System.Collections;
using System.Collections.Generic;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;

public class LettuceBountyTeamSelectDisplay : AbsSceneDisplay, IMercDetailsDisplayProvider
{
	public AsyncReference m_PlayButtonReference;

	public AsyncReference m_PlayButtonPhoneReference;

	public AsyncReference m_BackButtonReference;

	public AsyncReference m_BackButtonPhoneReference;

	public AsyncReference m_CollectionButtonReference;

	public AsyncReference m_TeamPreviewReference;

	public AsyncReference m_TeamPreviewPhoneReference;

	public AsyncReference m_TeamListDisplay;

	public AsyncReference m_MercDetailsDisplayReference;

	private PlayButton m_playButton;

	private UIBButton m_backButton;

	private UIBButton m_collectionButton;

	private VisualController m_teamListVisualController;

	private Widget m_teamPreviewWidget;

	private bool m_playButtonFinishedLoading;

	private bool m_backButtonFinishedLoading;

	private bool m_collectionButtonFinishedLoading;

	private bool m_teamListFinishedLoading;

	private bool m_teamPreviewFinishedLoading;

	private List<LettuceTeam> m_teamList;

	private LettuceTeam m_selectedTeam;

	private static bool m_hasSeenTeamLockConfirmationThisSession;

	public MercenaryDetailDisplay MercenaryDetailDisplay { get; private set; }

	public override void Start()
	{
		base.Start();
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_PlayButtonPhoneReference.RegisterReadyListener<PlayButton>(OnPlayButtonReady);
			m_BackButtonPhoneReference.RegisterReadyListener<UIBButton>(OnBackButtonReady);
			m_TeamPreviewPhoneReference.RegisterReadyListener<Widget>(OnTeamPreviewReady);
		}
		else
		{
			m_PlayButtonReference.RegisterReadyListener<PlayButton>(OnPlayButtonReady);
			m_BackButtonReference.RegisterReadyListener<UIBButton>(OnBackButtonReady);
			m_TeamPreviewReference.RegisterReadyListener<Widget>(OnTeamPreviewReady);
		}
		m_CollectionButtonReference.RegisterReadyListener<UIBButton>(OnCollectionButtonReady);
		m_TeamListDisplay.RegisterReadyListener<VisualController>(OnTeamListDisplayReady);
		m_MercDetailsDisplayReference.RegisterReadyListener<MercenaryDetailDisplay>(OnMercDetailsDisplayReady);
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_MercenariesSubMenus);
		CollectionManager.Get().MercenaryArtVariationChangedEvent += OnMercenaryArtVariationChangedEvent;
		StartCoroutine(InitializeWhenReady());
	}

	private void OnDestroy()
	{
		if (MercenaryDetailDisplay != null)
		{
			MercenaryDetailDisplay.UnregisterOnHideEvent(OnDetailDisplayClosed);
		}
		CollectionManager collectionManager = CollectionManager.Get();
		if (collectionManager != null)
		{
			collectionManager.MercenaryArtVariationChangedEvent -= OnMercenaryArtVariationChangedEvent;
		}
	}

	private void TeamListEventListener(string eventName)
	{
		if (eventName == "TEAM_SELECTED")
		{
			OnTeamSelected();
		}
	}

	public void OnPlayButtonReady(PlayButton playButton)
	{
		if (playButton == null)
		{
			Error.AddDevWarning("UI Error!", "PlayButton could not be found! You will not be able to click 'Play'!");
			return;
		}
		m_playButtonFinishedLoading = true;
		m_playButton = playButton;
		m_playButton.AddEventListener(UIEventType.RELEASE, OnPlayButtonRelease);
		m_playButton.Disable();
	}

	public void OnBackButtonReady(UIBButton backButton)
	{
		if (backButton == null)
		{
			Error.AddDevWarning("UI Error!", "BackButton could not be found! You will not be able to click 'Back'!");
			return;
		}
		m_backButtonFinishedLoading = true;
		m_backButton = backButton;
		m_backButton.AddEventListener(UIEventType.RELEASE, OnBackButtonRelease);
	}

	public void OnCollectionButtonReady(UIBButton collectionButton)
	{
		if (collectionButton == null)
		{
			Error.AddDevWarning("UI Error!", "CollectionButton could not be found! You will not be able to click 'Back'!");
			return;
		}
		m_collectionButtonFinishedLoading = true;
		m_collectionButton = collectionButton;
		m_collectionButton.AddEventListener(UIEventType.RELEASE, OnCollectionButtonRelease);
	}

	public void OnTeamListDisplayReady(VisualController visualController)
	{
		if (visualController != null)
		{
			visualController.GetComponent<Widget>().RegisterEventListener(TeamListEventListener);
		}
		m_teamListFinishedLoading = true;
		m_teamListVisualController = visualController;
	}

	private void OnMercDetailsDisplayReady(MercenaryDetailDisplay display)
	{
		if (MercenaryDetailDisplay != null)
		{
			MercenaryDetailDisplay.UnregisterOnHideEvent(OnDetailDisplayClosed);
		}
		MercenaryDetailDisplay = display;
		if (MercenaryDetailDisplay != null)
		{
			MercenaryDetailDisplay.RegisterOnHideEvent(OnDetailDisplayClosed);
		}
	}

	public void OnTeamPreviewReady(Widget preview)
	{
		if (preview == null)
		{
			Error.AddDevWarning("UI Error!", "TeamPreview could not be found!");
			return;
		}
		m_teamPreviewFinishedLoading = true;
		m_teamPreviewWidget = preview;
		PopulateTeamPreviewData(new LettuceTeam());
	}

	private void OnPlayButtonRelease(UIEvent e)
	{
		if (!Network.IsLoggedIn())
		{
			DialogManager.Get().ShowReconnectHelperDialog();
		}
		else if (m_selectedTeam == null)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_LETTUCE_VALIDATE_TEAM_HEADER");
			info.m_text = GameStrings.Get("GLUE_LETTUCE_VALIDATE_NO_TEAM_SELECTED");
			info.m_showAlertIcon = true;
			info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info.m_okText = GameStrings.Get("GLOBAL_OKAY");
			DialogManager.Get().ShowPopup(info);
		}
		else if (!m_selectedTeam.IsValid())
		{
			AlertPopup.PopupInfo info2 = new AlertPopup.PopupInfo();
			info2.m_headerText = GameStrings.Get("GLUE_LETTUCE_VALIDATE_TEAM_HEADER");
			info2.m_text = GameStrings.Get("GLUE_LETTUCE_VALIDATE_TEAM_INVALID");
			info2.m_showAlertIcon = true;
			info2.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info2.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info2.m_okText = GameStrings.Get("GLOBAL_OKAY");
			DialogManager.Get().ShowPopup(info2);
		}
		else if (!m_hasSeenTeamLockConfirmationThisSession)
		{
			AlertPopup.PopupInfo info3 = new AlertPopup.PopupInfo();
			info3.m_headerText = GameStrings.Get("GLUE_LETTUCE_BOUNTY_BOARD_TEAM_LOCK_HEADER");
			info3.m_text = GameStrings.Get("GLUE_LETTUCE_BOUNTY_BOARD_TEAM_LOCK_BODY");
			info3.m_showAlertIcon = false;
			info3.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info3.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			info3.m_confirmText = GameStrings.Get("GLUE_LETTUCE_BOUNTY_BOARD_TEAM_LOCK_CONFIRM");
			info3.m_cancelText = GameStrings.Get("GLOBAL_CANCEL");
			info3.m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CONFIRM)
				{
					m_hasSeenTeamLockConfirmationThisSession = true;
					NavigateToMapScene();
				}
			};
			DialogManager.Get().ShowPopup(info3);
		}
		else
		{
			NavigateToMapScene();
		}
	}

	private void OnBackButtonRelease(UIEvent e)
	{
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.LETTUCE_BOUNTY_BOARD, SceneMgr.TransitionHandlerType.NEXT_SCENE, null, m_sceneTransitionPayload);
	}

	private void OnCollectionButtonRelease(UIEvent e)
	{
		if (!Network.IsLoggedIn())
		{
			DialogManager.Get().ShowReconnectHelperDialog();
			return;
		}
		((LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload).m_TeamId = 0L;
		SetNextModeAndHandleTransition(SceneMgr.Mode.LETTUCE_COLLECTION, SceneMgr.TransitionHandlerType.CURRENT_SCENE, m_sceneTransitionPayload);
	}

	public override bool IsFinishedLoading(out string failureMessage)
	{
		if (!m_playButtonFinishedLoading)
		{
			failureMessage = "LettuceBountyTeamSelectDisplay - Play button never loaded.";
			return false;
		}
		if (!m_backButtonFinishedLoading)
		{
			failureMessage = "LettuceBountyTeamSelectDisplay - Back button never loaded.";
			return false;
		}
		if (!m_collectionButtonFinishedLoading)
		{
			failureMessage = "LettuceBountyTeamSelectDisplay - Collection button never loaded.";
			return false;
		}
		if (!m_teamListFinishedLoading)
		{
			failureMessage = "LettuceBountyTeamSelectDisplay - Team list never loaded.";
			return false;
		}
		if (!m_teamPreviewFinishedLoading)
		{
			failureMessage = "LettuceBountyTeamSelectDisplay - Team preview never loaded.";
			return false;
		}
		failureMessage = string.Empty;
		return true;
	}

	protected override bool ShouldStartShown()
	{
		return SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LETTUCE_COLLECTION;
	}

	private IEnumerator InitializeWhenReady()
	{
		m_teamList = CollectionManager.Get().GetTeams();
		string failureMessage;
		while (!IsFinishedLoading(out failureMessage))
		{
			yield return null;
		}
		InitializeTeamListDataModel();
		InitializeBountyTeamSelectDataModel();
	}

	private void InitializeTeamListDataModel()
	{
		CollectionUtils.PopulateMercenariesTeamListDataModel(GetTeamListDataModel(), showMythicLevel: ((LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload).m_SelectedBounty.DifficultyMode == LettuceBounty.MercenariesBountyDifficulty.MYTHIC, setAutoSelectedTeam: !UniversalInputManager.UsePhoneUI, teamList: m_teamList);
	}

	private void InitializeBountyTeamSelectDataModel()
	{
		LettuceBountyTeamSelectDataModel dataModel = GetBountyTeamSelectDataModel();
		LettuceBountySetDbfRecord bountySetRecord = ((LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload).m_SelectedBountySet;
		if (bountySetRecord != null)
		{
			dataModel.HeaderText = GameStrings.Format("GLUE_LETTUCE_BOUNTY_BOARD_TEAM_SELECT_HEADER", bountySetRecord.Name.GetString());
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

	private LettuceBountyTeamSelectDataModel GetBountyTeamSelectDataModel()
	{
		VisualController visualController = GetComponent<VisualController>();
		if (visualController == null)
		{
			return null;
		}
		Widget owner = visualController.Owner;
		if (!owner.GetDataModel(518, out var dataModel))
		{
			dataModel = new LettuceBountyTeamSelectDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as LettuceBountyTeamSelectDataModel;
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

	private void OnTeamSelected()
	{
		EventDataModel eventDataModel = WidgetUtils.GetEventDataModel(m_teamListVisualController);
		if (eventDataModel == null)
		{
			Log.Lettuce.PrintError("No event data model attached to the TeamListVisualController");
			return;
		}
		LettuceVillageDisplay.LettuceSceneTransitionPayload obj = (LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload;
		_ = obj.m_SelectedBounty;
		NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		LettuceTeamDataModel teamData = (LettuceTeamDataModel)eventDataModel.Payload;
		m_selectedTeam = CollectionManager.Get().GetTeam(teamData.TeamId);
		obj.m_TeamId = m_selectedTeam.ID;
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

	private void PopulateTeamPreviewData(LettuceTeam team)
	{
		if (team != null)
		{
			LettuceTeamDataModel teamPreviewDataModel = GetTeamPreviewDataModel();
			LettuceBountyDbfRecord selectedBounty = ((LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload).m_SelectedBounty;
			CollectionUtils.PopulateTeamPreviewData(teamPreviewDataModel, team, null, populateCards: false, isRemote: false, selectedBounty.DifficultyMode == LettuceBounty.MercenariesBountyDifficulty.MYTHIC);
			teamPreviewDataModel.TeamName = team.Name;
		}
	}

	private void NavigateToMapScene()
	{
		SetNextModeAndHandleTransition(SceneMgr.Mode.LETTUCE_MAP, m_sceneTransitionPayload);
		m_playButton.Disable();
	}

	private void OnMercenaryArtVariationChangedEvent(int mercenaryDbId, int artVariationId, TAG_PREMIUM premium)
	{
		foreach (LettuceMercenary merc in m_selectedTeam.GetMercs())
		{
			if (merc.ID == mercenaryDbId)
			{
				LettuceBountyDbfRecord selectedBounty = ((LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload).m_SelectedBounty;
				CollectionUtils.PopulateTeamPreviewData(GetTeamPreviewDataModel(), m_selectedTeam, null, populateCards: false, isRemote: false, selectedBounty.DifficultyMode == LettuceBounty.MercenariesBountyDifficulty.MYTHIC);
				break;
			}
		}
	}

	public void ShowMercDetailsDisplay(LettuceMercenary mercenary)
	{
		if (MercenaryDetailDisplay == null)
		{
			Log.Lettuce.PrintError("ShowMercDetailsDisplay - MercenaryDetailDisplay is null");
			return;
		}
		if (m_selectedTeam != null)
		{
			LettuceTeamDataModel selectedTeamDataModel = GetTeamPreviewDataModel();
			LettuceBountyDbfRecord selectedBounty = ((LettuceVillageDisplay.LettuceSceneTransitionPayload)m_sceneTransitionPayload).m_SelectedBounty;
			CollectionUtils.PopulateMercenariesTeamDataModel(selectedTeamDataModel, m_selectedTeam, isRemote: false, showLevelInList: false, selectedBounty.DifficultyMode == LettuceBounty.MercenariesBountyDifficulty.MYTHIC);
			MercenaryDetailDisplay.GetComponent<Widget>().BindDataModel(selectedTeamDataModel);
		}
		MercenaryDetailDisplay.Show(mercenary, UniversalInputManager.UsePhoneUI ? "SHOW_PARTIAL" : "SHOW_FULL", m_selectedTeam);
	}

	private void OnDetailDisplayClosed()
	{
		CollectionManager.Get()?.GetEditingTeam()?.SendChanges();
	}
}
