using System.Collections;
using System.Collections.Generic;
using Blizzard.GameService.Protocol.V2.Client;
using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusLettuce;

public class LettuceFriendlyDisplay : AbsSceneDisplay, IMercDetailsDisplayProvider
{
	public AsyncReference m_PlayButtonReference;

	public AsyncReference m_PlayButtonPhoneReference;

	public AsyncReference m_BackButtonReference;

	public AsyncReference m_BackButtonPhoneReference;

	public AsyncReference m_SharingButtonReference;

	public AsyncReference m_TeamPreviewReference;

	public AsyncReference m_TeamPreviewPhoneReference;

	public AsyncReference m_TeamListDisplay;

	public AsyncReference m_MercDetailsDisplayReference;

	private PlayButton m_playButton;

	private UIBButton m_backButton;

	private UIBButton m_sharingButton;

	private Widget m_sharingButtonWidget;

	private VisualController m_teamListVisualController;

	private Widget m_teamPreviewWidget;

	private bool m_playButtonFinishedLoading;

	private bool m_backButtonFinishedLoading;

	private bool m_sharingButtonFinishedLoading;

	private bool m_teamListFinishedLoading;

	private bool m_teamPreviewFinishedLoading;

	private bool m_detailsDisplayFinishedLoading;

	private LettuceTeam m_selectedTeam;

	private bool m_isTeamLockedIn;

	private long m_opponentsSelectedTeam;

	private List<LettuceTeam> m_teamListInUse;

	private List<LettuceTeam> m_remoteTeamList;

	private PartyManager.MercTeamShareState m_teamSharingState;

	private bool m_isSharingButtonFlipped;

	private bool m_isSharingButtonDisabled;

	public MercenaryDetailDisplay MercenaryDetailDisplay { get; private set; }

	private bool IsSharingButtonFlipped
	{
		get
		{
			return m_isSharingButtonFlipped;
		}
		set
		{
			if (value != m_isSharingButtonFlipped)
			{
				m_isSharingButtonFlipped = value;
				m_sharingButtonWidget.TriggerEvent((m_isSharingButtonFlipped || m_isSharingButtonDisabled) ? "INACTIVE" : "SETACTIVE");
			}
		}
	}

	private bool IsSharingButtonDisabled
	{
		get
		{
			return m_isSharingButtonDisabled;
		}
		set
		{
			if (value != m_isSharingButtonDisabled)
			{
				m_isSharingButtonDisabled = value;
				m_sharingButtonWidget.TriggerEvent((m_isSharingButtonFlipped || m_isSharingButtonDisabled) ? "INACTIVE" : "SETACTIVE");
			}
		}
	}

	private PartyManager.MercTeamShareState TeamSharingState
	{
		get
		{
			return m_teamSharingState;
		}
		set
		{
			m_teamSharingState = value;
			PartyManager.Get().SetTeamSharingState(value);
			if (value == PartyManager.MercTeamShareState.NOT_SHARING)
			{
				m_remoteTeamList = null;
			}
		}
	}

	private bool UsingLocalTeams => TeamSharingState != PartyManager.MercTeamShareState.USING_REMOTE_TEAMS;

	private bool TeamSharingEnabled => TeamSharingState != PartyManager.MercTeamShareState.NOT_SHARING;

	private bool AreAnyLocalTeamsValid
	{
		get
		{
			foreach (LettuceTeam team in CollectionManager.Get().GetTeams())
			{
				if (team.IsValid())
				{
					return true;
				}
			}
			return false;
		}
	}

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
		m_SharingButtonReference.RegisterReadyListener<UIBButton>(OnSharingButtonReady);
		m_TeamListDisplay.RegisterReadyListener<VisualController>(OnTeamListDisplayReady);
		m_MercDetailsDisplayReference.RegisterReadyListener<MercenaryDetailDisplay>(OnDetailsDisplayReady);
		CancelLockedInTeam();
		InitPartySelections();
		GameMgr.Get().RegisterFindGameEvent(OnFindGameEvent);
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_MercenariesSubMenus);
		CollectionManager.Get().StartInitialMercenaryLoadIfRequired();
		StartCoroutine(InitializeWhenReady());
	}

	private void InitPartySelections()
	{
		PartyManager partyManager = PartyManager.Get();
		partyManager.AddChangedListener(OnPartyChanged);
		partyManager.AddMemberAttributeChangedListener(OnPartyMemberAttributeChanged);
		if (partyManager.GetCurrentPartySize() == 2)
		{
			m_opponentsSelectedTeam = partyManager.GetOpponentSelectedTeam();
			IsSharingButtonDisabled = partyManager.GetMyTeamSharingButtonStatus() == PartyManager.MercTeamSharingButtonStatus.DISABLED;
			if (!IsSharingButtonDisabled)
			{
				m_teamSharingState = partyManager.GetTeamSharingState();
				if (TeamSharingEnabled)
				{
					GetRemoteTeams();
				}
				return;
			}
		}
		TeamSharingState = PartyManager.MercTeamShareState.NOT_SHARING;
	}

	private void GetRemoteTeams()
	{
		LettuceTeamList sharedTeamList = PartyManager.Get().GetSharedTeams();
		if (sharedTeamList == null)
		{
			TeamSharingState = PartyManager.MercTeamShareState.NOT_SHARING;
		}
		else
		{
			m_remoteTeamList = MakeTeamListFromSharedProtos(sharedTeamList);
		}
	}

	public void OnDestroy()
	{
		if (MercenaryDetailDisplay != null)
		{
			MercenaryDetailDisplay.UnregisterOnHideEvent(OnDetailDisplayClosed);
		}
		PartyManager partyManager = PartyManager.Get();
		if (partyManager != null)
		{
			partyManager.RemoveChangedListener(OnPartyChanged);
			partyManager.RemoveMemberAttributeChangedListener(OnPartyMemberAttributeChanged);
		}
		CollectionManager collectionManager = CollectionManager.Get();
		if (collectionManager != null)
		{
			LettuceTeam editedTeam = collectionManager.GetEditingTeam();
			collectionManager.ClearEditingTeam();
			editedTeam?.SendChanges();
		}
		GameMgr.Get()?.UnregisterFindGameEvent(OnFindGameEvent);
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

	public void OnSharingButtonReady(UIBButton sharingButton)
	{
		if (sharingButton == null)
		{
			Error.AddDevWarning("UI Error!", "SharingButton could not be found! You will not be able to click 'Back'!");
			return;
		}
		m_sharingButtonFinishedLoading = true;
		m_sharingButton = sharingButton;
		m_sharingButton.AddEventListener(UIEventType.RELEASE, OnTeamSharingButtonReleased);
		m_sharingButtonWidget = m_sharingButton.GetComponent<Widget>();
		UpdateSharingButtonText();
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

	public void OnDetailsDisplayReady(MercenaryDetailDisplay details)
	{
		if (details == null)
		{
			Error.AddDevWarning("UI Error!", "MercenaryDetailsDisplay could not be found!");
			return;
		}
		MercenaryDetailDisplay = details;
		MercenaryDetailDisplay.RegisterOnHideEvent(OnDetailDisplayClosed);
		m_detailsDisplayFinishedLoading = true;
	}

	private void UpdateSharingButtonText()
	{
		string newMsg = TeamSharingState switch
		{
			PartyManager.MercTeamShareState.USING_LOCAL_TEAMS => "FRIENDLY_CHALLENGE_OPPONENT_TEAMS", 
			PartyManager.MercTeamShareState.USING_REMOTE_TEAMS => "FRIENDLY_CHALLENGE_MY_TEAMS", 
			_ => "FRIENDLY_CHALLENGE_BORROW", 
		};
		m_sharingButtonWidget.TriggerEvent(newMsg);
	}

	private void UpdatePlayButton()
	{
		if (m_selectedTeam != null && m_selectedTeam.IsValid() && !m_selectedTeam.DoesContainDisabledMerc())
		{
			m_playButton.Enable();
		}
		else
		{
			m_playButton.Disable();
		}
	}

	private void OnPlayButtonRelease(UIEvent e)
	{
		if (m_selectedTeam == null)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_LETTUCE_VALIDATE_TEAM_HEADER");
			info.m_text = GameStrings.Get("GLUE_LETTUCE_VALIDATE_NO_TEAM_SELECTED");
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
			info2.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info2.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info2.m_okText = GameStrings.Get("GLOBAL_OKAY");
			DialogManager.Get().ShowPopup(info2);
			return;
		}
		PartyManager.Get().SetSelectedMercenariesTeamId(m_selectedTeam.ID);
		m_isTeamLockedIn = true;
		if (m_opponentsSelectedTeam <= 0)
		{
			AlertPopup.PopupInfo info3 = new AlertPopup.PopupInfo();
			info3.m_text = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_OPPONENT_WAITING_TEAM");
			info3.m_showAlertIcon = false;
			info3.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info3.m_responseDisplay = AlertPopup.ResponseDisplay.CANCEL;
			info3.m_responseCallback = delegate
			{
				CancelLockedInTeam();
			};
			DialogManager.Get().ShowPopup(info3);
		}
	}

	private void OnBackButtonRelease(UIEvent e)
	{
		NavigateBack();
	}

	private void OnTeamSharingButtonReleased(UIEvent e)
	{
		if (!TeamSharingEnabled)
		{
			RequestTeamSharing();
			return;
		}
		if (TeamSharingState == PartyManager.MercTeamShareState.USING_LOCAL_TEAMS)
		{
			TeamSharingState = PartyManager.MercTeamShareState.USING_REMOTE_TEAMS;
		}
		else
		{
			TeamSharingState = PartyManager.MercTeamShareState.USING_LOCAL_TEAMS;
		}
		UpdateSharingButtonText();
		CreateTeamListDataModel();
		UpdatePlayButton();
	}

	public override bool IsFinishedLoading(out string failureMessage)
	{
		if (!m_playButtonFinishedLoading)
		{
			failureMessage = "LettuceFriendlyDisplay - Play button never loaded.";
			return false;
		}
		if (!m_backButtonFinishedLoading)
		{
			failureMessage = "LettuceFriendlyDisplay - Back button never loaded.";
			return false;
		}
		if (!m_sharingButtonFinishedLoading)
		{
			failureMessage = "LettuceFriendlyDisplay - Sharing button never loaded.";
			return false;
		}
		if (!m_teamListFinishedLoading)
		{
			failureMessage = "LettuceFriendlyDisplay - Team list never loaded.";
			return false;
		}
		if (!m_teamPreviewFinishedLoading)
		{
			failureMessage = "LettuceFriendlyDisplay - Team preview never loaded.";
			return false;
		}
		if (!m_detailsDisplayFinishedLoading)
		{
			failureMessage = "LettuceFriendlyDisplay - Details Display never loaded.";
			return false;
		}
		if (!CollectionManager.Get().IsLettuceLoaded())
		{
			failureMessage = "LettuceFriendlyDisplay - Mercenaries collection was never loaded.";
			return false;
		}
		failureMessage = string.Empty;
		return true;
	}

	protected override bool ShouldStartShown()
	{
		if (SceneMgr.Get().IsDoingSceneDrivenTransition())
		{
			return SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LETTUCE_COLLECTION;
		}
		return true;
	}

	private IEnumerator InitializeWhenReady()
	{
		string failureMessage;
		while (!IsFinishedLoading(out failureMessage))
		{
			yield return null;
		}
		if (!PartyManager.Get().IsInMercenariesFriendlyChallenge())
		{
			ShowChallengeCanceledDialog(GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_QUEUE_CANCELED"));
			NavigateBack();
		}
		PartyManager.Get()?.SetOpponentTeamSharingButtonStatus((!AreAnyLocalTeamsValid) ? PartyManager.MercTeamSharingButtonStatus.DISABLED : PartyManager.MercTeamSharingButtonStatus.ENABLED);
		CreateTeamListDataModel();
	}

	private LettuceTeam GetTeamFromTeamList(int teamId)
	{
		foreach (LettuceTeam team in m_teamListInUse)
		{
			if (team.ID == teamId)
			{
				return team;
			}
		}
		return null;
	}

	private bool TeamIdIsValid(int teamId)
	{
		foreach (LettuceTeam team in m_teamListInUse)
		{
			if (team.ID == teamId)
			{
				return team.IsValid();
			}
		}
		return false;
	}

	private int GetDefaultTeamIDToSelect()
	{
		long teamIdToSelect = 0L;
		if (UsingLocalTeams)
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_LAST_SELECTED_PVP_TEAM, out teamIdToSelect);
			if (!TeamIdIsValid((int)teamIdToSelect))
			{
				teamIdToSelect = 0L;
			}
		}
		if (teamIdToSelect == 0L)
		{
			if (m_selectedTeam != null && TeamIdIsValid((int)m_selectedTeam.ID))
			{
				teamIdToSelect = m_selectedTeam.ID;
			}
			if (teamIdToSelect == 0L)
			{
				foreach (LettuceTeam team in m_teamListInUse)
				{
					if (team.IsValid())
					{
						teamIdToSelect = team.ID;
						break;
					}
				}
			}
		}
		return (int)teamIdToSelect;
	}

	private void CreateTeamListDataModel()
	{
		LettuceTeamListDataModel dataModel = new LettuceTeamListDataModel();
		if (UsingLocalTeams)
		{
			m_teamListInUse = CollectionManager.Get().GetTeams();
		}
		else
		{
			m_teamListInUse = m_remoteTeamList;
		}
		if (m_selectedTeam != null)
		{
			m_selectedTeam = GetTeamFromTeamList((int)m_selectedTeam.ID);
		}
		CollectionUtils.PopulateMercenariesTeamListDataModel(dataModel, setAutoSelectedTeam: false, m_teamListInUse, TeamSharingState == PartyManager.MercTeamShareState.USING_REMOTE_TEAMS, showLevelInList: true, hideInvalidTeams: true);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (m_selectedTeam != null)
			{
				PopulateTeamPreviewData(m_selectedTeam);
			}
		}
		else
		{
			if (m_teamListInUse.Count == 0)
			{
				PopulateTeamPreviewData(null);
			}
			dataModel.AutoSelectedTeamId = GetDefaultTeamIDToSelect();
		}
		BindTeamListDataModel(dataModel);
	}

	private void BindTeamListDataModel(LettuceTeamListDataModel dataModel)
	{
		VisualController visualController = GetComponent<VisualController>();
		if (!(visualController == null))
		{
			Widget owner = visualController.Owner;
			if (owner != null)
			{
				owner.BindDataModel(dataModel);
			}
		}
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

	private void NavigateBack()
	{
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
		PartyManager.Get().LeaveParty();
	}

	private void OnTeamSelected()
	{
		EventDataModel eventDataModel = WidgetUtils.GetEventDataModel(m_teamListVisualController);
		if (eventDataModel == null)
		{
			Log.Lettuce.PrintError("No event data model attached to the TeamListVisualController");
			return;
		}
		LettuceTeamDataModel teamData = (LettuceTeamDataModel)eventDataModel.Payload;
		m_selectedTeam = GetTeamFromTeamList((int)teamData.TeamId);
		if (UsingLocalTeams)
		{
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_LAST_SELECTED_PVP_TEAM, teamData.TeamId));
		}
		PopulateTeamPreviewData(m_selectedTeam);
		UpdatePlayButton();
		if (m_selectedTeam == null || !m_selectedTeam.IsValid() || m_selectedTeam.DoesContainDisabledMerc())
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_LETTUCE_DISABLED_TEAM_HEADER");
			if (m_selectedTeam != null && m_selectedTeam.DoesContainDisabledMerc())
			{
				info.m_text = GameStrings.Get("GLUE_LETTUCE_DISABLED_TEAM_NO_EDIT");
			}
			else
			{
				info.m_text = GameStrings.Get("GLUE_LETTUCE_DISABLED_INVALID_TEAM");
			}
			info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM;
			info.m_showAlertIcon = true;
			info.m_confirmText = GameStrings.Get("GLOBAL_OKAY");
			info.m_blurWhenShown = true;
			DialogManager.Get().ShowPopup(info);
		}
	}

	private void CancelLockedInTeam()
	{
		m_isTeamLockedIn = false;
		PartyManager.Get().SetSelectedMercenariesTeamId(0L);
	}

	private void PopulateTeamPreviewData(LettuceTeam team)
	{
		CollectionUtils.PopulateTeamPreviewData(GetTeamPreviewDataModel(), team, null, populateCards: false, !UsingLocalTeams);
	}

	private void ShowChallengeCanceledDialog(string message)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER");
		info.m_text = message;
		info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		info.m_showAlertIcon = false;
		info.m_okText = GameStrings.Get("GLOBAL_OKAY");
		DialogManager.Get().ShowPopup(info);
	}

	private void OnPartyChanged(PartyManager.PartyInviteEvent inviteEvent, BnetGameAccountId playerGameAccountId, PartyManager.PartyData challengeData, object userData)
	{
		Log.Party.PrintDebug("LettuceFriendlyDisplay.OnPartyChanged(): Event={0}, gameAccountId={1}", inviteEvent, playerGameAccountId);
		if (inviteEvent == PartyManager.PartyInviteEvent.I_RESCINDED_INVITE || inviteEvent == PartyManager.PartyInviteEvent.INVITE_EXPIRED || (uint)(inviteEvent - 10) <= 1u)
		{
			DialogManager.Get().ClearAllImmediately();
			ShowChallengeCanceledDialog(GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_QUEUE_CANCELED"));
			NavigateBack();
		}
	}

	private void OnPartyMemberAttributeChanged(BnetGameAccountId playerGameAccountId, Attribute attribute, object userData)
	{
		switch (attribute.Name)
		{
		case "team_id":
			if (attribute.Value.HasIntValue)
			{
				if (BnetPresenceMgr.Get().GetMyGameAccountId() != playerGameAccountId)
				{
					m_opponentsSelectedTeam = attribute.Value.IntValue;
				}
				if (PartyManager.Get().IsPartyLeader() && m_isTeamLockedIn && m_opponentsSelectedTeam > 0)
				{
					PartyManager.Get().FindGame();
				}
			}
			break;
		case "ts_status":
			if (BnetPresenceMgr.Get().GetMyGameAccountId() != playerGameAccountId && attribute.Value.HasIntValue)
			{
				IsSharingButtonDisabled = attribute.Value.IntValue == 1;
			}
			break;
		case "ts_MSG":
			if (BnetPresenceMgr.Get().GetMyGameAccountId() != playerGameAccountId && attribute.Value.HasIntValue && attribute.Value.IntValue != 0L)
			{
				HandleTeamSharingMessage((PartyManager.MercTeamShareMSG)attribute.Value.IntValue);
			}
			break;
		case "ts_teams":
			if (BnetPresenceMgr.Get().GetMyGameAccountId() != playerGameAccountId)
			{
				HandleSharedTeamsReceived(attribute.Value.BlobValue.ToByteArray());
			}
			break;
		}
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		Log.Party.PrintDebug("LettuceFriendlyDisplay.OnFindGameEvent(): State={0}", eventData.m_state);
		switch (eventData.m_state)
		{
		case FindGameState.CLIENT_STARTED:
		case FindGameState.BNET_QUEUE_ENTERED:
			DialogManager.Get().ClearAllImmediately();
			break;
		case FindGameState.CLIENT_CANCELED:
		case FindGameState.CLIENT_ERROR:
		case FindGameState.BNET_QUEUE_CANCELED:
		case FindGameState.BNET_ERROR:
		case FindGameState.SERVER_GAME_CANCELED:
			CancelLockedInTeam();
			break;
		case FindGameState.SERVER_GAME_STARTED:
			CancelLockedInTeam();
			break;
		}
		return false;
	}

	private void OnDetailDisplayClosed()
	{
		CollectionManager collectionManager = CollectionManager.Get();
		if (collectionManager == null)
		{
			return;
		}
		LettuceTeam editedTeam = collectionManager.GetEditingTeam();
		collectionManager.ClearEditingTeam();
		if (editedTeam != null && editedTeam.SendChanges())
		{
			CreateTeamListDataModel();
			PartyManager partyManager = PartyManager.Get();
			if (partyManager.GetTeamSharingState(getOpponentState: true) != 0)
			{
				partyManager.SetSharedTeams(MakeSharableTeamList());
			}
		}
	}

	public void ShowMercDetailsDisplay(LettuceMercenary mercenary)
	{
		if (UsingLocalTeams)
		{
			if (m_selectedTeam != null)
			{
				LettuceTeamDataModel selectedTeamDataModel = GetTeamPreviewDataModel();
				CollectionUtils.PopulateMercenariesTeamDataModel(selectedTeamDataModel, m_selectedTeam);
				CollectionManager.Get().SetEditingTeam(m_selectedTeam);
				MercenaryDetailDisplay.GetComponent<Widget>().BindDataModel(selectedTeamDataModel);
			}
			MercenaryDetailDisplay.Show(mercenary, UniversalInputManager.UsePhoneUI ? "SHOW_PARTIAL" : "SHOW_FULL", m_selectedTeam);
		}
	}

	private void RequestTeamSharing()
	{
		PartyManager.Get().SetTeamSharingMsg(PartyManager.MercTeamShareMSG.REQUEST_SHARING);
		IsSharingButtonFlipped = true;
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_TEAM_SHARE_HEADER");
		info.m_text = GameStrings.Format("GLUE_TEAM_SHARE_REQUEST_WAITING_RESPONSE", PartyManager.Get().GetOpponentBestName());
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CANCEL;
		info.m_responseCallback = delegate
		{
			PartyManager.Get().SetTeamSharingMsg(PartyManager.MercTeamShareMSG.SHARING_REQUEST_CANCELLED);
			UpdateSharingButtonText();
			IsSharingButtonFlipped = false;
		};
		DialogManager.Get().ShowPopup(info);
	}

	private void OnTeamSharingRequestDialogResponse(AlertPopup.Response response, object userData)
	{
		if (response == AlertPopup.Response.CANCEL)
		{
			PartyManager.Get().SetTeamSharingMsg(PartyManager.MercTeamShareMSG.SHARING_REQUEST_DENIED);
		}
		else
		{
			PartyManager.Get().SetSharedTeams(MakeSharableTeamList());
		}
	}

	private void HandleTeamSharingRequest()
	{
		DialogManager dialogManager = DialogManager.Get();
		dialogManager.ClearAllImmediately();
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_TEAM_SHARE_HEADER");
		info.m_text = GameStrings.Format("GLUE_TEAM_SHARE_REQUESTED", PartyManager.Get().GetOpponentBestName());
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_responseCallback = OnTeamSharingRequestDialogResponse;
		info.m_confirmText = GameStrings.Get("GLUE_TEAM_SHARE_ACCEPT_REQUEST");
		info.m_cancelText = GameStrings.Get("GLUE_TEAM_SHARE_DECLINE_REQUEST");
		dialogManager.ShowPopup(info);
	}

	private void HandleTeamSharingRequestCancelled()
	{
		DialogManager dialogManager = DialogManager.Get();
		dialogManager.ClearAllImmediately();
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_TEAM_SHARE_HEADER");
		info.m_text = GameStrings.Format("GLUE_TEAM_SHARE_REQUEST_CANCELED", PartyManager.Get().GetOpponentBestName());
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM;
		dialogManager.ShowPopup(info);
	}

	private void HandleTeamSharingRequestDenied()
	{
		DialogManager dialogManager = DialogManager.Get();
		dialogManager.ClearAllImmediately();
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_TEAM_SHARE_HEADER");
		info.m_text = GameStrings.Format("GLUE_TEAM_SHARE_REQUEST_DECLINED", PartyManager.Get().GetOpponentBestName());
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM;
		dialogManager.ShowPopup(info);
		PartyManager.Get().SetTeamSharingMsg(PartyManager.MercTeamShareMSG.NO_MSG);
		TeamSharingState = PartyManager.MercTeamShareState.NOT_SHARING;
		m_remoteTeamList = null;
		UpdateSharingButtonText();
		IsSharingButtonFlipped = false;
	}

	private void ShowTeamSharingError()
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_TEAM_SHARE_HEADER");
		info.m_text = GameStrings.Get("GLUE_TEAM_SHARE_ERROR");
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM;
		DialogManager.Get().ShowPopup(info);
		UpdateSharingButtonText();
		IsSharingButtonFlipped = false;
	}

	private void HandleSharedTeamsReceived(byte[] blob)
	{
		DialogManager.Get().ClearAllImmediately();
		if (blob == null || blob.Length == 0)
		{
			ShowTeamSharingError();
			return;
		}
		LettuceTeamList lettuceTeams = ProtobufUtil.ParseFrom<LettuceTeamList>(blob);
		List<LettuceTeam> teamList = MakeTeamListFromSharedProtos(lettuceTeams);
		if (teamList.Count == 0)
		{
			ShowTeamSharingError();
			return;
		}
		m_remoteTeamList = teamList;
		if (TeamSharingState == PartyManager.MercTeamShareState.NOT_SHARING)
		{
			TeamSharingState = PartyManager.MercTeamShareState.USING_REMOTE_TEAMS;
			PartyManager.Get().SetTeamSharingMsg(PartyManager.MercTeamShareMSG.NO_MSG);
			UpdateSharingButtonText();
			IsSharingButtonFlipped = false;
		}
		if (!UsingLocalTeams)
		{
			CreateTeamListDataModel();
		}
	}

	private void HandleTeamSharingMessage(PartyManager.MercTeamShareMSG msg)
	{
		switch (msg)
		{
		case PartyManager.MercTeamShareMSG.REQUEST_SHARING:
			HandleTeamSharingRequest();
			break;
		case PartyManager.MercTeamShareMSG.SHARING_REQUEST_CANCELLED:
			HandleTeamSharingRequestCancelled();
			break;
		case PartyManager.MercTeamShareMSG.SHARING_REQUEST_DENIED:
			HandleTeamSharingRequestDenied();
			break;
		}
	}

	private LettuceMercenary MakeMercenaryFromSharedProto(LettuceTeamMercenary protoMerc)
	{
		LettuceMercenaryDbfRecord mercenaryRecord = GameDbf.LettuceMercenary.GetRecord(protoMerc.MercenaryId);
		if (mercenaryRecord == null)
		{
			Log.CollectionManager.PrintError("CollectionManager_Lettuce.RegisterMercenary(): Invalid mercenary ID [{0}]!", protoMerc.MercenaryId);
			return null;
		}
		LettuceMercenary mercenary = null;
		mercenary = new LettuceMercenary
		{
			ID = protoMerc.MercenaryId,
			m_rarity = (TAG_RARITY)mercenaryRecord.Rarity,
			m_acquireType = (TAG_ACQUIRE_TYPE)mercenaryRecord.AcquireType,
			m_customAcquireText = mercenaryRecord.HowToAcquireText,
			m_isFullyUpgraded = protoMerc.SharedTeamMercenaryIsFullyUpgraded
		};
		mercenary.SetExperience(protoMerc.SharedTeamMercenaryXp);
		MercenaryArtVariationPremiumDbfRecord portraitRecord = GameDbf.MercenaryArtVariationPremium.GetRecord(protoMerc.SelectedPortraitId);
		if (portraitRecord != null)
		{
			MercenaryArtVariationDbfRecord artRecord = GameDbf.MercenaryArtVariation.GetRecord(portraitRecord.MercenaryArtVariationId);
			if (artRecord != null)
			{
				TAG_PREMIUM premium = (TAG_PREMIUM)portraitRecord.Premium;
				EntityDef mercenaryDef = DefLoader.Get().GetEntityDef(artRecord.CardId);
				string shortName = mercenaryDef.GetShortName();
				mercenary.m_mercName = mercenaryDef.GetName();
				mercenary.m_mercShortName = (string.IsNullOrEmpty(shortName) ? mercenary.m_mercName : shortName);
				mercenary.m_role = mercenaryDef.GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE);
				mercenary.m_artVariations.Add(new LettuceMercenary.ArtVariation(artRecord, premium, artRecord.DefaultVariation));
				mercenary.GetBaseLoadout().SetArtVariation(artRecord, premium);
			}
		}
		return mercenary;
	}

	private LettuceTeam MakeTeamFromSharedProto(PegasusLettuce.LettuceTeam protoTeam)
	{
		if (protoTeam.MercenaryList.Mercenaries.Count == 0)
		{
			return null;
		}
		LettuceTeam result = new LettuceTeam
		{
			Name = protoTeam.Name,
			SortOrder = protoTeam.SortOrder,
			ID = protoTeam.TeamId,
			TeamType = protoTeam.Type_
		};
		foreach (LettuceTeamMercenary protoMerc in protoTeam.MercenaryList.Mercenaries)
		{
			LettuceMercenary merc = MakeMercenaryFromSharedProto(protoMerc);
			if (merc != null)
			{
				result.AddMerc(merc);
			}
		}
		result.ClearDirty();
		return result;
	}

	private List<LettuceTeam> MakeTeamListFromSharedProtos(LettuceTeamList protoList)
	{
		List<LettuceTeam> result = new List<LettuceTeam>();
		foreach (PegasusLettuce.LettuceTeam teamProto in protoList.Teams)
		{
			LettuceTeam team = MakeTeamFromSharedProto(teamProto);
			if (team != null && team.GetMercCount() != 0)
			{
				result.Add(team);
			}
		}
		return result;
	}

	private LettuceTeamList MakeSharableTeamList()
	{
		LettuceTeamList result = new LettuceTeamList();
		foreach (LettuceTeam team in CollectionManager.Get().GetTeams())
		{
			result.Teams.Add(LettuceTeam.Convert(team, includeDataForRemoteSharing: true));
		}
		return result;
	}
}
