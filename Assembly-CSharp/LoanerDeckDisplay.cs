using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LoanerDeckDisplay : MonoBehaviour
{
	private const string CLOSE_DECK_DETAILS = "CloseDetails";

	private const string CONFIRM_DECK_CHOICE = "ConfirmDeckChoice";

	private const string CONFIRM_DECK_CHOICE_BYPASS_TRIAL = "ConfirmDeckChoiceAndBypassTrial";

	private const string SHOW_DECK_DETAILS = "ShowLoanerDeckDetails";

	private const string OPEN_DECK_DETAILS_STATE = "OPEN";

	private const string CLOSE_DECK_DETAILS_STATE = "CLOSE";

	private const string TRIAL_EXPIRED_STATE = "SHOW_EXPIRED_NOTIFICATION";

	private const string CONFIRM_FREE_DECK_SELECTION = "CONFIRM_FREE_DECK_SELECTION";

	public const int MAX_LOANER_DECKS_DISPLAYED = 6;

	public Vector3 m_loanerDeckFTUEDisplayPosition = new Vector3(-19.2f, 0f, 10.2f);

	public Vector3 m_loanerDeckFTUEDisplayPositionMobile = new Vector3(-19.2f, 0f, 10.2f);

	public Vector3 m_loanerDeckFTUEDisplayScale = Vector3.one;

	public Vector3 m_loanerDeckFTUEDisplayScaleMobile = Vector3.one;

	public float m_notificationPulseTime;

	[Header("Widget References")]
	public AsyncReference LoanerDeckDetailsWidget;

	public List<AsyncReference> ClassButtonReferences;

	public static Action LoanerDeckDisplayOpened;

	private static LoanerDeckDisplay m_instance;

	private FreeDeckMgr m_freeDeckManager;

	private Widget m_loanerDeckDetails;

	private LoanerDeckDetailsController m_loanerDeckDetailsController;

	private AbsDeckPickerTrayDisplay m_deckPickerTray;

	private DeckTemplateDbfRecord m_currentlySelectedDeck;

	private TimeSpan m_trialPeriodTimeLeft;

	private Notification m_loanerFTUENotification;

	public LoanerDecksInfoDataModel LoanerDeckInfoDataModel { get; set; }

	public static event Action LoanerDeckExpiredDisplayed;

	private void Awake()
	{
		m_instance = this;
		m_freeDeckManager = FreeDeckMgr.Get();
		m_freeDeckManager?.GetLoanerDecksAsMap();
		m_deckPickerTray = DeckPickerTrayDisplay.Get();
		LoanerDeckInfoDataModel = new LoanerDecksInfoDataModel();
		if (!ShouldLoanerDecksBeDisplayed())
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		LoanerDeckDetailsWidget.RegisterReadyListener<Widget>(InitializeDeckDetailsWidget);
		m_deckPickerTray.AddDeckTrayLoadedListener(RefreshDisplayOnTrayLoaded);
		SceneMgr.Get().RegisterSceneLoadedEvent(SetSceneModePropertyOnSceneLoad);
		if (m_freeDeckManager != null)
		{
			if (m_freeDeckManager.TrialPeriodEndTime.HasValue)
			{
				m_trialPeriodTimeLeft = (m_freeDeckManager.TrialPeriodEndTime - DateTime.Now).Value;
				LoanerDeckInfoDataModel.RemainingDeckTrialTime = TimeUtils.GetCountdownTimerString(m_trialPeriodTimeLeft, getFinalSeconds: true);
			}
			LoanerDeckInfoDataModel.DeckChoices = new DataModelList<DeckChoiceDataModel>();
			foreach (KeyValuePair<int, DeckTemplateDbfRecord> pair in m_freeDeckManager.GetLoanerDeckTemplateMap())
			{
				DeckChoiceDataModel deckChoiceDataModel = new DeckChoiceDataModel();
				deckChoiceDataModel.ChoiceClassID = pair.Value.ClassId;
				deckChoiceDataModel.ChoiceClassName = ((TAG_CLASS)deckChoiceDataModel.ChoiceClassID/*cast due to .constrained prefix*/).ToString();
				deckChoiceDataModel.DeckDescription = pair.Value.DeckRecord.Description;
				LoanerDeckInfoDataModel.DeckChoices.Add(deckChoiceDataModel);
			}
		}
		long hasSeenLoanerDeckFTUE = 0L;
		LoanerDeckInfoDataModel.HasSeenLoanerDeckFTUE = false;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_LOANER_DECK_FTUE, out hasSeenLoanerDeckFTUE);
		if (hasSeenLoanerDeckFTUE > 0)
		{
			LoanerDeckInfoDataModel.HasSeenLoanerDeckFTUE = true;
		}
		StartCoroutine(TickDownEligibilityTimer());
		PegUIElement.OnUIElementPressed += HideFtueOnButtonPress;
	}

	public void Start()
	{
		DeckPickerTrayDisplay.OnFormatSwitchButtonPressed += HideLoanerFtueNotification;
	}

	private void OnDestroy()
	{
		SceneMgr.Get().UnregisterSceneLoadedEvent(ShowExpiredNotificationSceneLoad);
		SceneMgr.Get().UnregisterSceneLoadedEvent(SetSceneModePropertyOnSceneLoad);
		StopCoroutine(TickDownEligibilityTimer());
		if ((bool)m_deckPickerTray)
		{
			m_deckPickerTray.RemoveDeckTrayLoadedListener(RefreshDisplayOnTrayLoaded);
		}
		LoanerDeckInfoDataModel = null;
		if (m_loanerFTUENotification != null)
		{
			NotificationManager.Get()?.DestroyNotificationNowWithNoAnim(m_loanerFTUENotification);
		}
		DeckPickerTrayDisplay.OnFormatSwitchButtonPressed -= HideLoanerFtueNotification;
		PegUIElement.OnUIElementPressed -= HideFtueOnButtonPress;
	}

	public static LoanerDeckDisplay Get()
	{
		return m_instance;
	}

	public void SetSelectedDeckInDataModel(bool isLoaner)
	{
		if (LoanerDeckInfoDataModel != null)
		{
			LoanerDeckInfoDataModel.IsSelectedDeckLoaner = isLoaner;
		}
	}

	public void SetCurrentPageStatusInDataModel(bool isLoaner)
	{
		if (LoanerDeckInfoDataModel != null)
		{
			LoanerDeckInfoDataModel.IsCurrentPageLoaner = isLoaner;
		}
	}

	private void HideFtueOnButtonPress(PegUIElement pressedElement)
	{
		bool shouldHide = false;
		string[] buttonPrefixes = new string[6] { "CardBack", "CollectionDeck", "InteractionController", "InfoButton", "ExitButton", "CollectionButton" };
		for (int i = 0; i < buttonPrefixes.Length; i++)
		{
			if (pressedElement.name.StartsWith(buttonPrefixes[i]))
			{
				shouldHide = true;
				break;
			}
		}
		if (shouldHide)
		{
			HideLoanerFtueNotification();
		}
	}

	public void ShowLoanerFTUENotification(bool isShowingLoanerDeckPage)
	{
		bool shouldShowFTUENotifcation = isShowingLoanerDeckPage && m_loanerFTUENotification == null && !LoanerDeckInfoDataModel.HasSeenLoanerDeckFTUE;
		NotificationManager notificationManager = NotificationManager.Get();
		if (notificationManager == null)
		{
			return;
		}
		if (shouldShowFTUENotifcation)
		{
			Vector3 ftueDisplayLocation = m_loanerDeckFTUEDisplayPosition;
			Vector3 ftueDisplayScale = m_loanerDeckFTUEDisplayScale;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				ftueDisplayLocation = m_loanerDeckFTUEDisplayPositionMobile;
				ftueDisplayScale = m_loanerDeckFTUEDisplayScaleMobile;
			}
			m_loanerFTUENotification = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, ftueDisplayLocation, ftueDisplayScale, GameStrings.Get("GLUE_LOANER_DECK_FTUE_INFO_BUTTON"), convertLegacyPosition: false);
			if (m_loanerFTUENotification != null)
			{
				m_loanerFTUENotification.PulseReminderEveryXSeconds(m_notificationPulseTime);
				m_loanerFTUENotification.ShowPopUpArrow(Notification.PopUpArrowDirection.Up);
			}
		}
		else if (m_loanerFTUENotification != null)
		{
			notificationManager.DestroyNotificationNowWithNoAnim(m_loanerFTUENotification);
		}
	}

	public void OpenDeckDetailsWidget(string eventName)
	{
		if (!(eventName != "ShowLoanerDeckDetails"))
		{
			if (m_loanerDeckDetails != null)
			{
				m_loanerDeckDetails.TriggerEvent("OPEN");
				LoanerDeckDisplayOpened?.Invoke();
			}
			HideAndMarkLoanerFtueSeen();
		}
	}

	public void HideLoanerFtueNotification()
	{
		if (m_loanerFTUENotification != null)
		{
			NotificationManager.Get()?.DestroyNotificationNowWithNoAnim(m_loanerFTUENotification);
		}
	}

	private void HideAndMarkLoanerFtueSeen()
	{
		HideLoanerFtueNotification();
		if (LoanerDeckInfoDataModel != null)
		{
			LoanerDeckInfoDataModel.HasSeenLoanerDeckFTUE = true;
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_LOANER_DECK_FTUE, 1L));
		}
	}

	public void HideDeckDetailsWidget(string eventName)
	{
		if (!(eventName != "CloseDetails") && m_loanerDeckDetails != null)
		{
			m_loanerDeckDetails.TriggerEvent("CLOSE");
		}
	}

	public void ShowTrialTimerExpiredState()
	{
		if (m_loanerDeckDetails != null)
		{
			HideAndMarkLoanerFtueSeen();
			m_loanerDeckDetails.TriggerEvent("SHOW_EXPIRED_NOTIFICATION");
			LoanerDeckDisplay.LoanerDeckExpiredDisplayed?.Invoke();
		}
	}

	public void SetCurrentlySelectedDeckTemplate(DeckTemplateDbfRecord templateRecord)
	{
		if (templateRecord != null)
		{
			m_currentlySelectedDeck = templateRecord;
		}
	}

	public void ConfirmDeckSelection(string eventName)
	{
		if (eventName != "ConfirmDeckChoice" && eventName != "ConfirmDeckChoiceAndBypassTrial")
		{
			return;
		}
		bool shouldBypassTrialPeriod = false;
		if (eventName == "ConfirmDeckChoiceAndBypassTrial")
		{
			shouldBypassTrialPeriod = true;
		}
		bool hasClaimedDeck = false;
		TAG_CLASS classTag = TAG_CLASS.INVALID;
		if (m_currentlySelectedDeck != null)
		{
			classTag = (TAG_CLASS)m_currentlySelectedDeck.ClassId;
		}
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.RETURNING_PLAYER_EXPERIENCE, GameSaveKeySubkeyId.HAS_SEEN_LOANER_DECKS_ON_FIRST_LOGIN_TRIAL_START, default(long)));
		if (hasClaimedDeck)
		{
			return;
		}
		string headerText = "GLUE_FREE_DECK_CONFIRMATION_HEADER";
		string bodyText = GameStrings.Format("GLUE_FREE_DECK_CONFIRMATION_TEXT", GameStrings.GetClassName(classTag)) + GameStrings.Format("GLUE_LOANER_DECK_CLAIM_CONFIRM");
		AlertPopup.ResponseDisplay responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		if (shouldBypassTrialPeriod)
		{
			headerText = "GLUE_FREE_DECK_CONFIRMATION_HEADER_BYPASS_TIMER";
			bodyText = GameStrings.Format("GLUE_FREE_DECK_CONFIRMATION_TEXT_BYPASS_TIMER", GameStrings.GetClassName(classTag));
			responseDisplay = AlertPopup.ResponseDisplay.YES_NO;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get(headerText),
			m_text = bodyText,
			m_showAlertIcon = false,
			m_alertTextAlignment = UberText.AlignmentOptions.Center,
			m_responseDisplay = responseDisplay,
			m_id = "CONFIRM_FREE_DECK_SELECTION",
			m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CONFIRM && m_currentlySelectedDeck != null)
				{
					Network.Get().SendFreeDeckChoice(LoanerDeckInfoDataModel.DeckChoiceTemplateId, shouldBypassTrialPeriod);
					hasClaimedDeck = true;
					RewardUtils.CreateDeckRewardData(m_currentlySelectedDeck.ID, 0, m_currentlySelectedDeck.ClassId, m_currentlySelectedDeck.DeckRecord.Name);
					DialogManager.Get().RemoveUniquePopupRequestFromQueue("CONFIRM_FREE_DECK_SELECTION");
					StartCoroutine(DelayAndGoToCollectionManager());
					GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_LOANER_DECK_FTUE, default(long)));
					if (LoanerDeckInfoDataModel != null)
					{
						LoanerDeckInfoDataModel.HasSeenLoanerDeckFTUE = false;
					}
				}
			}
		};
		DialogManager.Get().ShowUniquePopup(info);
	}

	public void OnLoanerDeckTimeExpired()
	{
		OpenDeckDetailsWidget("ShowLoanerDeckDetails");
	}

	public bool ShouldLoanerDecksBeDisplayed()
	{
		FreeDeckMgr.FreeDeckStatus status = m_freeDeckManager.Status;
		bool num = status == FreeDeckMgr.FreeDeckStatus.AVAILABLE || status == FreeDeckMgr.FreeDeckStatus.TRIAL_PERIOD;
		bool validDeckTrayStatus = m_deckPickerTray != null && !m_deckPickerTray.IsChoosingHero();
		bool validSceneStatus = SceneMgr.Get().GetMode() != SceneMgr.Mode.FRIENDLY;
		if (!num || !validDeckTrayStatus || !validSceneStatus || m_freeDeckManager.GetLoanerDecksCount() == 0)
		{
			return false;
		}
		return true;
	}

	private IEnumerator TickDownEligibilityTimer()
	{
		while (LoanerDeckInfoDataModel != null)
		{
			yield return new WaitForSeconds(1f);
			m_trialPeriodTimeLeft = TimeSpan.MinValue;
			if (m_freeDeckManager != null && m_freeDeckManager.TrialPeriodEndTime.HasValue)
			{
				m_trialPeriodTimeLeft = (m_freeDeckManager.TrialPeriodEndTime - DateTime.Now).Value;
			}
			if (m_trialPeriodTimeLeft.TotalSeconds <= 0.0)
			{
				m_trialPeriodTimeLeft = TimeSpan.Zero;
			}
			LoanerDeckInfoDataModel.RemainingDeckTrialTime = TimeUtils.GetCountdownTimerString(m_trialPeriodTimeLeft, getFinalSeconds: true);
			if (!(m_trialPeriodTimeLeft.TotalSeconds <= 0.0))
			{
				continue;
			}
			if (NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().CancelMatchmakingDuringLoanerDeckGrant)
			{
				GameMgr gameManager = GameMgr.Get();
				if (gameManager.IsFindingGame())
				{
					gameManager.CancelFindGame();
				}
			}
			LoanerDeckInfoDataModel.IsLoanerDeckAvailable = true;
			if (CanShowTimerExpiredState())
			{
				ShowTrialTimerExpiredState();
				StopCoroutine(TickDownEligibilityTimer());
				break;
			}
		}
	}

	private void InitializeDeckDetailsWidget(Widget widget)
	{
		if (widget == null)
		{
			Log.Decks.PrintWarning("No DeckDetails widget available");
			return;
		}
		m_loanerDeckDetailsController = widget.GetComponentInChildren<LoanerDeckDetailsController>();
		if (m_loanerDeckDetailsController != null)
		{
			InitializeDeckSelectorButtons();
			if (m_freeDeckManager.GetLoanerDeckTemplateMap().Count > 6)
			{
				m_loanerDeckDetailsController.SetShouldShowPaginationFTUE(showFTUE: true);
			}
		}
		LoanerDeckInfoDataModel.DeckChoiceTemplateId = 0;
		if (FreeDeckMgr.Get().IsLoanerDeckAvailableToClaim())
		{
			LoanerDeckInfoDataModel.IsLoanerDeckAvailable = true;
			if (SceneMgr.Get().IsSceneLoaded())
			{
				ShowTrialTimerExpiredState();
			}
			else
			{
				SceneMgr.Get().RegisterSceneLoadedEvent(ShowExpiredNotificationSceneLoad);
			}
		}
		else
		{
			LoanerDeckInfoDataModel.IsLoanerDeckAvailable = false;
		}
		m_loanerDeckDetails = widget;
		LoanerDeckInfoDataModel.DeckChoiceTemplateId = 0;
		widget.RegisterEventListener(ConfirmDeckSelection);
		widget.BindDataModel(LoanerDeckInfoDataModel);
	}

	private void InitializeDeckSelectorButtons()
	{
		Dictionary<int, CollectionDeck> collectionDeckMap = FreeDeckMgr.Get().GetLoanerDecksAsMap();
		Dictionary<int, DeckTemplateDbfRecord> loanerDeckTemplateMap = FreeDeckMgr.Get().GetLoanerDeckTemplateMap();
		if (ClassButtonReferences.Count < loanerDeckTemplateMap.Count)
		{
			Log.Decks.PrintError("Not enough button widgets for available decks");
			return;
		}
		int buttonInitialized = 0;
		LoanerDeckSelectButton defaultDeckSelectButton = null;
		foreach (KeyValuePair<int, DeckTemplateDbfRecord> record in loanerDeckTemplateMap)
		{
			TAG_CLASS classAsTag = (TAG_CLASS)record.Value.ClassId;
			CollectionManager.GetHeroCardId(classAsTag, CardHero.HeroType.VANILLA);
			ClassButtonReferences[buttonInitialized].RegisterReadyListener(delegate(Widget widget)
			{
				widget.BindDataModel(new DeckChoiceDataModel
				{
					ButtonClass = classAsTag.ToString()
				});
				LoanerDeckSelectButton componentInChildren = widget.GetComponentInChildren<LoanerDeckSelectButton>();
				if (componentInChildren == null)
				{
					Log.Decks.PrintError("Could not find LoanerDeckSelectButton for deck Selection button");
					widget.Hide();
				}
				else
				{
					componentInChildren.DeckTemplateRecord = record.Value;
					componentInChildren.DeckDetailsController = m_loanerDeckDetailsController;
					componentInChildren.DataModel = LoanerDeckInfoDataModel;
					componentInChildren.SetDeckSelectButtonIcon(collectionDeckMap[record.Key]);
					widget.RegisterEventListener(componentInChildren.OnDeckChoiceButtonClicked);
					if (defaultDeckSelectButton == null)
					{
						defaultDeckSelectButton = componentInChildren;
						defaultDeckSelectButton.OnDeckChoiceButtonClicked("Selected");
					}
				}
			});
			buttonInitialized++;
		}
		for (int i = buttonInitialized; i < ClassButtonReferences.Count; i++)
		{
			ClassButtonReferences[i].RegisterReadyListener(delegate(Widget widget)
			{
				widget.gameObject.SetActive(value: false);
			});
		}
	}

	private void ShowExpiredNotificationSceneLoad(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (mode == SceneMgr.Mode.TOURNAMENT && GameUtils.IsAnyTutorialComplete() && CanShowTimerExpiredState())
		{
			ShowTrialTimerExpiredState();
		}
	}

	private void SetSceneModePropertyOnSceneLoad(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		LoanerDeckInfoDataModel.CurrentSceneMode = mode.ToString();
	}

	private void RefreshDisplayOnTrayLoaded()
	{
		if (ShouldLoanerDecksBeDisplayed())
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator DelayAndGoToCollectionManager()
	{
		yield return new WaitForSeconds(0.5f);
		HideDeckDetailsWidget("CloseDetails");
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
	}

	private bool CanShowTimerExpiredState()
	{
		bool num = SetRotationManager.Get().ShouldShowSetRotationIntro() || DialogManager.Get().WaitingToShowSeasonEndDialog();
		bool isModeSwitchShowing = DeckPickerTrayDisplay.Get() != null && DeckPickerTrayDisplay.Get().IsModeSwitchShowing;
		bool isPopupActive = (CollectionManager.Get().ShouldAccountSeeStandardWild() && !SetRotationManager.HasSeenStandardModeTutorial()) || DialogManager.Get().ShowingDialog();
		if (!num && !isModeSwitchShowing)
		{
			return !isPopupActive;
		}
		return false;
	}
}
