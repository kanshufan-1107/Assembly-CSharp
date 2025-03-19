using System.Collections;
using Assets;
using Blizzard.GameService.Protocol.V2.Client;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core.Utils;
using Hearthstone.DataModels;
using Hearthstone.UI;

public class LettuceCoOpDisplay : AbsSceneDisplay
{
	public AsyncReference m_PlayButtonReference;

	public AsyncReference m_BackButtonReference;

	public AsyncReference m_EditTeamButtonReference;

	public AsyncReference m_TeamListDisplay;

	private bool m_playButtonFinishedLoading;

	private bool m_backButtonFinishedLoading;

	private bool m_editTeamButtonFinishedLoading;

	private PlayButton m_playButton;

	private UIBButton m_editTeamButton;

	private VisualController m_teamListVisualController;

	private LettuceTeam m_selectedTeam;

	private long m_coopPartnerTeamId;

	public override void Start()
	{
		base.Start();
		m_PlayButtonReference.RegisterReadyListener<VisualController>(OnPlayButtonReady);
		m_BackButtonReference.RegisterReadyListener<VisualController>(OnBackButtonReady);
		m_EditTeamButtonReference.RegisterReadyListener<UIBButton>(OnEditTeamButtonReady);
		m_TeamListDisplay.RegisterReadyListener<VisualController>(OnTeamListDisplayReady);
		PartyManager.Get().AddChangedListener(OnPartyChanged);
		PartyManager.Get().AddMemberAttributeChangedListener(OnPartyMemberAttributeChanged);
		PartyManager.Get().SetReadyStatus(ready: false);
		StartCoroutine(InitializeWhenReady());
	}

	public void OnDestroy()
	{
		if (PartyManager.Get() != null)
		{
			PartyManager.Get().RemoveChangedListener(OnPartyChanged);
			PartyManager.Get().RemoveMemberAttributeChangedListener(OnPartyMemberAttributeChanged);
		}
	}

	public override bool IsFinishedLoading(out string failureMessage)
	{
		if (!m_playButtonFinishedLoading)
		{
			failureMessage = "LettuceCoOpDisplay - Play button never loaded.";
			return false;
		}
		if (!m_backButtonFinishedLoading)
		{
			failureMessage = "LettuceCoOpDisplay - Back button never loaded.";
			return false;
		}
		if (!m_editTeamButtonFinishedLoading)
		{
			failureMessage = "LettuceCoOpDisplay - Edit Team button never loaded.";
			return false;
		}
		if (m_teamListVisualController == null)
		{
			failureMessage = "LettuceCoOpDisplay - Team List never loaded.";
			return false;
		}
		failureMessage = string.Empty;
		return true;
	}

	public void OnPlayButtonReady(VisualController buttonVisualController)
	{
		if (buttonVisualController == null)
		{
			Error.AddDevWarning("UI Error!", "PlayButton could not be found! You will not be able to click 'Play'!");
			return;
		}
		m_playButton = buttonVisualController.gameObject.GetComponent<PlayButton>();
		m_playButton.AddEventListener(UIEventType.RELEASE, PlayButtonRelease);
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
		buttonVisualController.gameObject.GetComponent<UIBButton>().AddEventListener(UIEventType.RELEASE, BackButtonRelease);
		m_backButtonFinishedLoading = true;
	}

	public void OnEditTeamButtonReady(UIBButton editTeamButton)
	{
		m_editTeamButtonFinishedLoading = true;
		if (editTeamButton == null)
		{
			Error.AddDevWarning("UI Error!", "EditTeamButton could not be found! You will not be able to click 'Edit Team'!");
			return;
		}
		m_editTeamButton = editTeamButton;
		m_editTeamButton.AddEventListener(UIEventType.RELEASE, OnEditTeamButtonRelease);
	}

	public void OnTeamListDisplayReady(VisualController visualController)
	{
		if (visualController != null)
		{
			visualController.GetComponent<Widget>().RegisterEventListener(TeamListEventListener);
		}
		m_teamListVisualController = visualController;
	}

	private void TeamListEventListener(string eventName)
	{
		if (eventName == "TEAM_SELECTED")
		{
			OnTeamSelected();
		}
	}

	public void PlayButtonRelease(UIEvent e)
	{
		PartyManager.Get().SetReadyStatus(ready: true);
		if (PartyManager.Get().IsPartyLeader())
		{
			if (!PartyManager.Get().AreAllPartyMembersReady())
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
				info.m_headerText = GameStrings.Get("Not Ready");
				info.m_text = GameStrings.Get("Wait for all party members to be ready.");
				info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
				info.m_showAlertIcon = true;
				info.m_okText = GameStrings.Get("GLOBAL_OKAY");
				DialogManager.Get().ShowPopup(info);
			}
			else if (m_coopPartnerTeamId == 0L)
			{
				AlertPopup.PopupInfo info2 = new AlertPopup.PopupInfo();
				info2.m_headerText = GameStrings.Get("Not Ready");
				info2.m_text = GameStrings.Get("Wait for your partner to select a team.");
				info2.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
				info2.m_showAlertIcon = true;
				info2.m_okText = GameStrings.Get("GLOBAL_OKAY");
				DialogManager.Get().ShowPopup(info2);
			}
			else
			{
				LettuceVillageDisplay.LettuceSceneTransitionPayload payload = new LettuceVillageDisplay.LettuceSceneTransitionPayload
				{
					m_SelectedBountySet = GameDbf.LettuceBountySet.GetRecord(1),
					m_DifficultyMode = LettuceBounty.MercenariesBountyDifficulty.NORMAL,
					m_SelectedBounty = GameDbf.LettuceBounty.GetRecord(48),
					m_TeamId = m_selectedTeam.ID,
					m_CoOpPartnerTeamId = m_coopPartnerTeamId
				};
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.LETTUCE_MAP, SceneMgr.TransitionHandlerType.SCENEMGR, null, payload);
			}
		}
	}

	public void BackButtonRelease(UIEvent e)
	{
		NavigateBack();
	}

	private void OnEditTeamButtonRelease(UIEvent e)
	{
		CollectionManager.Get().NotifyOfBoxTransitionStart();
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.LETTUCE_COLLECTION);
	}

	protected override bool ShouldStartShown()
	{
		return true;
	}

	private void NavigateBack()
	{
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.LETTUCE_VILLAGE, SceneMgr.TransitionHandlerType.NEXT_SCENE);
		PartyManager.Get().LeaveParty();
	}

	private void OnPartyChanged(PartyManager.PartyInviteEvent inviteEvent, BnetGameAccountId playerGameAccountId, PartyManager.PartyData challengeData, object userData)
	{
		Log.Party.PrintDebug("LettuceCoOpDisplay.OnPartyChanged(): Event={0}, gameAccountId={1}", inviteEvent, playerGameAccountId);
		if (inviteEvent == PartyManager.PartyInviteEvent.I_RESCINDED_INVITE || (uint)(inviteEvent - 5) <= 1u || (uint)(inviteEvent - 10) <= 1u)
		{
			NavigateBack();
		}
	}

	private void OnPartyMemberAttributeChanged(BnetGameAccountId playerGameAccountId, Attribute attribute, object userData)
	{
		if (PartyManager.Get().GetPartyLeaderGameAccountId() == playerGameAccountId && attribute.Name == "scene" && attribute.Value.HasStringValue)
		{
			SceneMgr.Mode mode = EnumUtils.Parse<SceneMgr.Mode>(attribute.Value.StringValue);
			if (mode != 0)
			{
				SceneMgr.Get().SetNextMode(mode);
			}
		}
		if (BnetPresenceMgr.Get().GetMyGameAccountId() != playerGameAccountId && attribute.Name == "team_id" && attribute.Value.HasIntValue)
		{
			m_coopPartnerTeamId = attribute.Value.IntValue;
		}
	}

	private IEnumerator InitializeWhenReady()
	{
		string message;
		while (!IsFinishedLoading(out message))
		{
			yield return null;
		}
		InitializeTeamListDataModel();
	}

	private void InitializeTeamListDataModel()
	{
		CollectionUtils.PopulateMercenariesTeamListDataModel(GetTeamListDataModel(), setAutoSelectedTeam: false);
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

	private void OnTeamSelected()
	{
		EventDataModel eventDataModel = WidgetUtils.GetEventDataModel(m_teamListVisualController);
		if (eventDataModel == null)
		{
			Log.Lettuce.PrintError("No event data model attached to the TeamListVisualController");
			return;
		}
		LettuceTeamDataModel teamData = (LettuceTeamDataModel)eventDataModel.Payload;
		m_selectedTeam = CollectionManager.Get().GetTeam(teamData.TeamId);
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_LAST_SELECTED_PVP_TEAM, teamData.TeamId));
		m_playButton.Enable();
		PartyManager.Get().SetSelectedMercenariesTeamId(teamData.TeamId);
	}
}
