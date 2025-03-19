using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class TwistDetailsDisplayManager : MonoBehaviour
{
	public AsyncReference m_twistModePageHeader;

	public AsyncReference m_twistIntroPopup;

	public AsyncReference m_twistDeckListTray;

	public AsyncReference m_twistCraftingButton;

	public AsyncReference m_DeckListingButton;

	public GameObject[] m_objectsToDisableOnDeckDisplay;

	public GameObject m_rankedTrayDisplay;

	public float m_twistDeckListTrayDisplayDelay = 3f;

	private const string SHOW_TWIST_PANEL_SLIDE_LEFT = "SHOW_TWIST_HEADER_SLIDE_LEFT";

	private const string HIDE_TWIST_PANEL = "HIDE_TWIST_HEADER";

	private const string HIDE_TWIST_HEADER_MODE_CHANGE = "HIDE_TWIST_HEADER_MODE_CHANGE";

	private const string SHOW_TWIST_PANEL_SLIDE_RIGHT = "SHOW_TWIST_HEADER_SLIDE_RIGHT";

	private const string HAS_SEEN_TWIST_INTRO_POPUP = "HAS_SEEN_TWIST_INTRO_POPUP";

	private const string DISMISS_NEW_SEASON_LABEL = "DISMISS_NEW_SEASON_LABEL";

	private const string DISMISS_MODE_INTRO_GLOW = "DISMISS_MODE_INTRO_GLOW";

	private const string SHOW_HEROIC_DECK_DISPLAY = "OPEN";

	private const string HIDE_HEROIC_DECK_DISPLAY = "CLOSE";

	private const string ENABLE_HEROIC_DECK_DISPLAY = "TRAY_ENABLING";

	private const string DISABLE_HEROIC_DECK_DISPLAY = "TRAY_DISABLING";

	private const string SHOW_TWIST_HEROIC_DECK_PANEL = "OPEN";

	private const string HIDE_TWIST_HEROIC_DECK_PANEL = "CLOSE";

	private static TwistSeasonInfoDataModel m_twistSeasonInfoDataModel;

	private Notification m_deckListingTutorialPopup;

	private Coroutine m_deckListingTutorialPopupCoroutine;

	private static TwistDetailsDisplayManager m_instance;

	private Widget m_twistPageHeaderWidget;

	private static TimeSpan m_seasonTimeLeft;

	private static RankedPlaySeason m_twistRankedPlaySeason;

	private Widget m_twistDeckTrayDisplayWidget;

	private UIBButton m_twistCraftingButtonWidget;

	private Dictionary<GameObject, bool> m_cacheActiveStateForDeckDisplay = new Dictionary<GameObject, bool>();

	private Transform m_DeckListingButton_Bone;

	public static TwistSeasonInfoDataModel TwistSeasonInfoModel
	{
		get
		{
			if (m_twistSeasonInfoDataModel == null)
			{
				m_twistSeasonInfoDataModel = new TwistSeasonInfoDataModel();
			}
			return m_twistSeasonInfoDataModel;
		}
	}

	public Widget TwistDeckTrayDisplayWidget => m_twistDeckTrayDisplayWidget;

	public void Awake()
	{
		if (m_instance == null)
		{
			m_instance = this;
		}
		m_twistRankedPlaySeason = RankMgr.Get()?.GetCurrentTwistSeason();
		if (m_twistRankedPlaySeason == null)
		{
			HandleTwistSeasonDisabled();
			if (RankMgr.IsNextSeasonValid())
			{
				StartCoroutine(TickTwistSeasonTimer());
			}
			return;
		}
		if (m_twistModePageHeader != null)
		{
			m_twistModePageHeader.RegisterReadyListener<Widget>(OnTwistHeaderWidgetReady);
		}
		if (m_twistIntroPopup != null)
		{
			m_twistIntroPopup.RegisterReadyListener<Widget>(OnTwistIntroPopupWidgetReady);
		}
		if (m_twistDeckListTray != null)
		{
			m_twistDeckListTray.RegisterReadyListener<Widget>(OnTwistDeckDisplayReady);
		}
		if (m_twistCraftingButton != null)
		{
			m_twistCraftingButton.RegisterReadyListener<UIBButton>(OnTwistCraftingButtonReady);
		}
		if (m_DeckListingButton != null)
		{
			m_DeckListingButton.RegisterReadyListener<Transform>(OnDeckListingButtonReady);
		}
		long hasSeenTwistIntroGlow = 0L;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_TWIST_INTRO_GLOW, out hasSeenTwistIntroGlow);
		TwistSeasonInfoModel.HasSeenTwistModeGlow = false;
		if (hasSeenTwistIntroGlow > 0)
		{
			TwistSeasonInfoModel.HasSeenTwistModeGlow = true;
		}
		TwistSeasonInfoModel.ShowFormatPickerOnSeasonEnd = false;
	}

	private void OnDestroy()
	{
		ForceHideTwistHeroicDeckListingTutorial(accept: false);
		StopCoroutine(TickTwistSeasonTimer());
		m_twistSeasonInfoDataModel = null;
	}

	private void OnTwistHeaderWidgetReady(Widget widget)
	{
		m_twistPageHeaderWidget = widget;
		if (m_twistRankedPlaySeason != null)
		{
			widget.BindDataModel(TwistSeasonInfoModel);
			if (m_twistRankedPlaySeason.SeasonDesc != null)
			{
				TwistSeasonInfoModel.SeasonDescription = GameStrings.Get(m_twistRankedPlaySeason.SeasonDesc);
			}
			else
			{
				Debug.LogError("Season record has no Desc, please update the SeasonDesc for season id" + m_twistRankedPlaySeason.ID);
			}
			if (m_twistRankedPlaySeason.SeasonTitle != null)
			{
				TwistSeasonInfoModel.SeasonTitle = GameStrings.Get(m_twistRankedPlaySeason.SeasonTitle);
			}
			else
			{
				Debug.LogError("Season record has no Title, please update the SeasonTitle for season id" + m_twistRankedPlaySeason.ID);
			}
			List<TAG_CARD_SET> filteredSets = GameUtils.GetTwistSetsWithFilter(m_twistRankedPlaySeason.HiddenCardSets);
			GameUtils.FillTwistDataModelWithValidSets(TwistSeasonInfoModel, filteredSets);
			GameUtils.FillTwistDataModelWithHeroicDecks(m_twistSeasonInfoDataModel);
			m_seasonTimeLeft = RankMgr.GetTimeLeftInCurrentSeason();
			TwistSeasonInfoModel.RemainingSeasonTime = TimeUtils.GetCountdownTimerString(m_seasonTimeLeft, getFinalSeconds: true);
			if (m_seasonTimeLeft.TotalSeconds > 1.0)
			{
				TwistSeasonInfoModel.IsTwistSeasonEnabled = true;
			}
			StartCoroutine(TickTwistSeasonTimer());
		}
	}

	private void OnTwistIntroPopupWidgetReady(Widget widget)
	{
		widget.BindDataModel(TwistSeasonInfoModel);
		long hasSeenTwistIntroPopup = 0L;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_TWIST_INTRO_POPUP, out hasSeenTwistIntroPopup);
		CollectionManager collectionManager = CollectionManager.Get();
		bool hasUnlockedWild = false;
		if (collectionManager != null)
		{
			hasUnlockedWild = collectionManager.AccountHasUnlockedWild();
		}
		TwistSeasonInfoModel.ShouldShowTwistLoginPopup = false;
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT && hasSeenTwistIntroPopup == 0 && hasUnlockedWild && CanShowingTwistPopups())
		{
			TwistSeasonInfoModel.ShouldShowTwistLoginPopup = true;
		}
		widget.RegisterEventListener(FlagPlayerHasSeenTwistIntroPopup);
	}

	private void OnTwistDeckDisplayReady(Widget widget)
	{
		if (widget == null)
		{
			return;
		}
		m_twistDeckTrayDisplayWidget = widget;
		m_twistDeckTrayDisplayWidget.gameObject.SetActive(value: false);
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		VisualsFormatType currentVisualsFormatType = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType();
		bool isCurrentModeTwist = mode == SceneMgr.Mode.TOURNAMENT && currentVisualsFormatType == VisualsFormatType.VFT_TWIST;
		if (RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
		{
			m_twistDeckTrayDisplayWidget.RegisterEventListener(TwistHeriocDeckDisplayEventListener);
			if (isCurrentModeTwist)
			{
				StartCoroutine(WaitAndShowTwistDeckPickerTrayDisplay());
			}
		}
	}

	private IEnumerator WaitAndShowTwistDeckPickerTrayDisplay()
	{
		yield return new WaitForSeconds(m_twistDeckListTrayDisplayDelay);
		m_twistDeckTrayDisplayWidget.gameObject.SetActive(value: true);
	}

	private void OnTwistCraftingButtonReady(UIBButton button)
	{
		if (!(button == null))
		{
			m_twistCraftingButtonWidget = button;
			if (RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
			{
				button.AddEventListener(UIEventType.RELEASE, TwistCraftingButtonReleaseEventListener);
				m_twistCraftingButtonWidget.enabled = true;
			}
		}
	}

	private void OnDeckListingButtonReady(Transform button)
	{
		if (!(button == null))
		{
			m_DeckListingButton_Bone = button;
		}
	}

	private void TwistHeriocDeckDisplayEventListener(string eventName)
	{
		if (eventName == "OPEN")
		{
			ForceHideTwistHeroicDeckListingTutorial(accept: true);
			m_cacheActiveStateForDeckDisplay.Clear();
			for (int i = 0; i < m_objectsToDisableOnDeckDisplay.Length; i++)
			{
				GameObject gameObject = m_objectsToDisableOnDeckDisplay[i].gameObject;
				m_cacheActiveStateForDeckDisplay.Add(gameObject, gameObject.activeSelf);
				gameObject.SetActive(value: false);
			}
		}
		if (eventName == "CLOSE")
		{
			for (int j = 0; j < m_objectsToDisableOnDeckDisplay.Length; j++)
			{
				GameObject gameObject2 = m_objectsToDisableOnDeckDisplay[j].gameObject;
				if (m_cacheActiveStateForDeckDisplay.TryGetValue(gameObject2, out var oldActive) && oldActive)
				{
					gameObject2.SetActive(value: true);
				}
			}
		}
		if (eventName == "TRAY_ENABLING")
		{
			ShowTwistHeroicDeckListingTutorialIfNecessary();
		}
		if (eventName == "TRAY_DISABLING")
		{
			ForceHideTwistHeroicDeckListingTutorial(accept: false);
		}
	}

	private void TwistCraftingButtonReleaseEventListener(UIEvent e)
	{
		if (DeckPickerTrayDisplay.Get() == null || DeckPickerTrayDisplay.Get()?.GetSelectedCollectionDeck() == null)
		{
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_TWIST_CRAFTING_POPUP_TITLE");
		info.m_text = GameStrings.Get("GLUE_TWIST_CRAFTING_POPUP_DESCRIPTION");
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		AlertPopup.ResponseCallback callback = delegate(AlertPopup.Response response, object userdata)
		{
			if (response == AlertPopup.Response.CONFIRM)
			{
				TwistCraftingConfirm();
			}
		};
		info.m_responseCallback = callback;
		DialogManager.Get().ShowPopup(info);
	}

	private void TwistCraftingConfirm()
	{
		if (DeckPickerTrayDisplay.Get() == null)
		{
			return;
		}
		CollectionDeck deck = DeckPickerTrayDisplay.Get()?.GetSelectedCollectionDeck();
		if (deck != null)
		{
			deck.DeckTemplate_HasUnownedRequirements(out var requiredCard);
			if (!string.IsNullOrEmpty(requiredCard))
			{
				TelemetryManager.Client().SendDeckPickerToCollection(DeckPickerToCollection.Path.DECK_PICKER_BUTTON);
				DeepLinkManager.ExecuteDeepLink(new string[3] { "collectionmanager", requiredCard, "crafting" }, DeepLinkManager.DeepLinkSource.DECK_PICKER, 0);
				ToggleTwistDeckDisplayAnimationStatePC(shouldBeOpen: false);
				ToggleTwistDeckDisplayPC(showDeckDisplay: false);
			}
		}
	}

	public static TwistDetailsDisplayManager Get()
	{
		return m_instance;
	}

	public void ToggleTwistDeckDisplayPC(bool showDeckDisplay)
	{
		if (m_twistDeckTrayDisplayWidget != null)
		{
			m_twistDeckTrayDisplayWidget.gameObject.SetActive(showDeckDisplay);
		}
		if (showDeckDisplay)
		{
			ShowTwistHeroicDeckListingTutorialIfNecessary();
		}
		else
		{
			ForceHideTwistHeroicDeckListingTutorial(accept: false);
		}
	}

	public void ToggleTwistDeckDisplayAnimationStatePC(bool shouldBeOpen)
	{
		if (m_twistDeckTrayDisplayWidget != null)
		{
			if (shouldBeOpen)
			{
				m_twistDeckTrayDisplayWidget.TriggerEvent("OPEN");
			}
			else
			{
				m_twistDeckTrayDisplayWidget.TriggerEvent("CLOSE");
			}
		}
	}

	public void ShowTwistHeaderPanelSlideLeft()
	{
		ShowTwistHeaderPanelSlideLeft("SHOW_TWIST_HEADER_SLIDE_LEFT");
	}

	public void ShowTwistHeaderPanelSlideRight()
	{
		ShowTwistHeaderPanelSlideRight("SHOW_TWIST_HEADER_SLIDE_RIGHT");
	}

	public void ShowTwistHeaderPanelSlideRight(string eventName)
	{
		if (!(eventName != "SHOW_TWIST_HEADER_SLIDE_RIGHT") && !(m_twistPageHeaderWidget == null))
		{
			if (m_twistModePageHeader != null)
			{
				m_twistPageHeaderWidget.TriggerEvent("SHOW_TWIST_HEADER_SLIDE_RIGHT");
			}
			TwistSeasonInfoModel.DoesCurrentPageHaveTwistHeader = true;
		}
	}

	public void ShowTwistHeaderPanelSlideLeft(string eventName)
	{
		if (!(eventName != "SHOW_TWIST_HEADER_SLIDE_LEFT") && !(m_twistPageHeaderWidget == null))
		{
			if (m_twistModePageHeader != null)
			{
				m_twistPageHeaderWidget.TriggerEvent("SHOW_TWIST_HEADER_SLIDE_LEFT");
			}
			TwistSeasonInfoModel.DoesCurrentPageHaveTwistHeader = true;
		}
	}

	public void HideTwistHeaderPanel()
	{
		HideTwistHeaderPanel("HIDE_TWIST_HEADER");
	}

	public void HideTwistHeaderPanelOnFormatChange()
	{
		HideTwistHeaderPanel("HIDE_TWIST_HEADER_MODE_CHANGE");
	}

	public void HideTwistHeaderPanel(string eventName)
	{
		if (!(m_twistPageHeaderWidget == null) && (!(eventName != "HIDE_TWIST_HEADER") || !(eventName != "HIDE_TWIST_HEADER_MODE_CHANGE")))
		{
			if (m_twistModePageHeader != null)
			{
				m_twistPageHeaderWidget.TriggerEvent(eventName);
			}
			TwistSeasonInfoModel.DoesCurrentPageHaveTwistHeader = false;
		}
	}

	private static void FlagPlayerHasSeenTwistIntroPopup(string eventName)
	{
		if (!(eventName != "HAS_SEEN_TWIST_INTRO_POPUP"))
		{
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_TWIST_INTRO_POPUP, 1L));
			TwistSeasonInfoModel.ShouldShowTwistLoginPopup = false;
			DeckPickerTrayDisplay deckPickerTrayDisplay = DeckPickerTrayDisplay.Get();
			if ((bool)deckPickerTrayDisplay)
			{
				deckPickerTrayDisplay.ShowNewTwistModePopupIfNecessary();
			}
		}
	}

	public static void DismissNewModeGlow(string eventName)
	{
		if (!(eventName != "DISMISS_MODE_INTRO_GLOW"))
		{
			if (m_twistRankedPlaySeason != null)
			{
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_TWIST_INTRO_GLOW, 1L));
			}
			TwistSeasonInfoModel.HasSeenTwistModeGlow = true;
		}
	}

	public static void DismissNewSeasonLabel(string eventName)
	{
		if (!(eventName != "DISMISS_NEW_SEASON_LABEL") && m_twistRankedPlaySeason != null)
		{
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.LATEST_TWIST_SEASON_SEEN, m_twistRankedPlaySeason.Season));
			TwistSeasonInfoModel.HasSeenTwistNewSeasonLabel = true;
		}
	}

	public void ToggleTwistHeroicDeckCardPanel(bool openPanel)
	{
		if (!UniversalInputManager.UsePhoneUI && !(m_twistDeckTrayDisplayWidget == null))
		{
			if (openPanel)
			{
				SendEventDownwardStateAction.SendEventDownward(m_twistDeckTrayDisplayWidget.gameObject, "OPEN", SendEventDownwardStateAction.BubbleDownEventDepth.DirectChildrenOnly);
			}
			else
			{
				SendEventDownwardStateAction.SendEventDownward(m_twistDeckTrayDisplayWidget.gameObject, "CLOSE", SendEventDownwardStateAction.BubbleDownEventDepth.DirectChildrenOnly);
			}
		}
	}

	public void ToggleRankedHeaderDisplayMobile(bool show)
	{
		if (!(m_rankedTrayDisplay == null))
		{
			m_rankedTrayDisplay.SetActive(show);
		}
	}

	public void HideTutorialPopups()
	{
		ForceHideTwistHeroicDeckListingTutorial(accept: false);
	}

	private IEnumerator TickTwistSeasonTimer()
	{
		while (TwistSeasonInfoModel != null)
		{
			yield return new WaitForSeconds(1f);
			m_seasonTimeLeft = m_seasonTimeLeft.Subtract(new TimeSpan(0, 0, 1));
			if (m_seasonTimeLeft.TotalSeconds <= 0.0 && m_twistRankedPlaySeason != null)
			{
				m_seasonTimeLeft = new TimeSpan(0, 0, 0);
				GameMgr gameManager = GameMgr.Get();
				if (gameManager.IsFindingGame())
				{
					gameManager.CancelFindGame();
				}
				if (CanShowingTwistPopups())
				{
					StopCoroutine(TickTwistSeasonTimer());
					if (Options.GetFormatType() == PegasusShared.FormatType.FT_TWIST)
					{
						DeckPickerTrayDisplay.Get().SwitchFormatTypeAndRankedPlayMode(VisualsFormatType.VFT_STANDARD);
						DeckPickerTrayDisplay.Get().SwitchFormatButtonPress();
						TwistSeasonInfoModel.ShowFormatPickerOnSeasonEnd = true;
					}
					HandleTwistSeasonDisabled();
					break;
				}
			}
			TwistSeasonInfoModel.RemainingSeasonTime = TimeUtils.GetCountdownTimerString(m_seasonTimeLeft, getFinalSeconds: true);
		}
	}

	private bool CanShowingTwistPopups()
	{
		bool isModeSwitchShowing = DeckPickerTrayDisplay.Get() != null && DeckPickerTrayDisplay.Get().IsModeSwitchShowing;
		if (!SetRotationManager.Get().ShouldShowSetRotationIntro() && SetRotationManager.HasSeenStandardModeTutorial() && !isModeSwitchShowing && !DialogManager.Get().WaitingToShowSeasonEndDialog())
		{
			return !DialogManager.Get().ShowingDialog();
		}
		return false;
	}

	private static void HandleTwistSeasonDisabled()
	{
		TwistSeasonInfoModel.IsTwistSeasonEnabled = false;
		m_seasonTimeLeft = RankMgr.GetTimeLeftInCurrentSeason();
		TwistSeasonInfoModel.RemainingSeasonTime = TimeUtils.GetCountdownTimerString(m_seasonTimeLeft, getFinalSeconds: true);
		if (!RankMgr.IsNextSeasonValid())
		{
			TwistSeasonInfoModel.RemainingSeasonTime = GameStrings.Get("GLUE_TIME_MORE_THAN_A_MONTH");
		}
		if (VisualsFormatTypeExtensions.GetCurrentVisualsFormatType() == VisualsFormatType.VFT_TWIST)
		{
			Options.SetFormatType(PegasusShared.FormatType.FT_STANDARD);
		}
	}

	private void ShowTwistHeroicDeckListingTutorialIfNecessary()
	{
		if (m_DeckListingButton_Bone == null || m_deckListingTutorialPopup != null)
		{
			return;
		}
		DeckPickerTrayDisplay deckPickerTray = DeckPickerTrayDisplay.Get();
		if (!(deckPickerTray == null) && deckPickerTray.GetSelectedCollectionDeck() != null)
		{
			long hasSeenTwistDeckListing = 0L;
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_TWIST_DECK_LISTING_POPUP, out hasSeenTwistDeckListing);
			if (hasSeenTwistDeckListing <= 0 && m_deckListingTutorialPopupCoroutine == null)
			{
				m_deckListingTutorialPopupCoroutine = StartCoroutine(ShowTwistHeroicDeckListingTutorial());
			}
		}
	}

	private IEnumerator ShowTwistHeroicDeckListingTutorial()
	{
		do
		{
			yield return null;
			if (Box.Get() == null)
			{
				m_deckListingTutorialPopupCoroutine = null;
				yield break;
			}
		}
		while (Box.Get().IsTransitioningToSceneMode());
		yield return new WaitForSecondsRealtime(1f);
		if (!CanShowingTwistPopups())
		{
			yield return null;
		}
		if (NotificationManager.Get() == null)
		{
			m_deckListingTutorialPopupCoroutine = null;
			yield break;
		}
		m_deckListingTutorialPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, m_DeckListingButton_Bone.position, m_DeckListingButton_Bone.localScale, GameStrings.Get("GLUE_TWIST_DECK_TRAY_LISTING_TUTORIAL"));
		if (m_deckListingTutorialPopup == null)
		{
			m_deckListingTutorialPopupCoroutine = null;
			yield break;
		}
		Notification.PopUpArrowDirection arrowDirection = Notification.PopUpArrowDirection.Up;
		m_deckListingTutorialPopup.ShowPopUpArrow(arrowDirection);
		m_deckListingTutorialPopup.PulseReminderEveryXSeconds(3f);
		m_deckListingTutorialPopupCoroutine = null;
	}

	private void ForceHideTwistHeroicDeckListingTutorial(bool accept)
	{
		if (m_deckListingTutorialPopup != null)
		{
			if (accept)
			{
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_TWIST_DECK_LISTING_POPUP, 1L));
			}
			if (NotificationManager.Get() != null)
			{
				NotificationManager.Get().DestroyNotification(m_deckListingTutorialPopup, 0f);
				m_deckListingTutorialPopup = null;
			}
		}
		if (m_deckListingTutorialPopupCoroutine != null)
		{
			StopCoroutine(m_deckListingTutorialPopupCoroutine);
			m_deckListingTutorialPopupCoroutine = null;
		}
	}
}
