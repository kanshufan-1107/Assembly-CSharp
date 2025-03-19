using System.Collections;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusUtil;
using UnityEngine;

namespace Hearthstone.Progression;

public class JournalButton : MonoBehaviour
{
	[SerializeField]
	private string popupAssetReference = "JournalPopup.prefab:b61d6e9bd58789647a62494cf92fb93e";

	[SerializeField]
	private Global.RewardTrackType m_rewardTrackType = Global.RewardTrackType.GLOBAL;

	private Widget m_widget;

	private Widget m_journalPopUpWidget;

	private TooltipZone m_toolTip;

	private JournalMetaDataModel m_journalMetaDatamodel;

	private const string BUTTON_CLICKED = "BUTTON_CLICKED";

	private const string JOURNAL_OPENED = "JOURNAL_OPENED";

	private const string SHOW_TOOLTIP = "SHOW_TOOLTIP";

	private const string HIDE_TOOLTIP = "HIDE_TOOLTIP";

	private Coroutine m_updateApprenticeTrackAvailabilityCoroutine;

	private Coroutine m_updateTavernGuideAvailabilityCoroutine;

	private void Awake()
	{
		m_toolTip = GetComponent<TooltipZone>();
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterReadyListener(OnReady);
		m_widget.RegisterEventListener(HandleEvent);
		UpdateJournalMetadata();
		RewardTrackManager.Get().OnRewardTracksReceived += UpdateJournalMetadata;
		QuestManager.Get().OnQuestStateUpdate += UpdateJournalMetadata;
		Network network = Network.Get();
		if (network != null)
		{
			network.RegisterNetHandler(InitialClientState.PacketID.ID, UpdateJournalMetadata);
			network.RegisterNetHandler(HeroXP.PacketID.ID, UpdateJournalMetadata);
		}
		SpecialEventManager.Get().OnCurrentEventChanged += OnCurrentEventChanged;
		TavernGuideManager.Get().OnQuestSetsChanged += UpdateJournalMetadata;
		GameSaveDataManager.Get().OnGameSaveDataUpdate += OnGameSaveDataUpdate;
	}

	private void OnDestroy()
	{
		RewardTrackManager rewardTrackManager = RewardTrackManager.Get();
		if (rewardTrackManager != null)
		{
			rewardTrackManager.OnRewardTracksReceived -= UpdateJournalMetadata;
		}
		QuestManager questManager = QuestManager.Get();
		if (questManager != null)
		{
			questManager.OnQuestStateUpdate -= UpdateJournalMetadata;
		}
		Network network = Network.Get();
		if (network != null)
		{
			network.RemoveNetHandler(InitialClientState.PacketID.ID, UpdateJournalMetadata);
			network.RemoveNetHandler(HeroXP.PacketID.ID, UpdateJournalMetadata);
		}
		SpecialEventManager specialEventManager = SpecialEventManager.Get();
		if (specialEventManager != null)
		{
			specialEventManager.OnCurrentEventChanged -= OnCurrentEventChanged;
		}
		TavernGuideManager tavernGuideManager = TavernGuideManager.Get();
		if (tavernGuideManager != null)
		{
			tavernGuideManager.OnQuestSetsChanged -= UpdateJournalMetadata;
		}
		if (m_updateApprenticeTrackAvailabilityCoroutine != null)
		{
			StopCoroutine(m_updateApprenticeTrackAvailabilityCoroutine);
		}
		if (m_updateTavernGuideAvailabilityCoroutine != null)
		{
			StopCoroutine(m_updateTavernGuideAvailabilityCoroutine);
		}
		GameSaveDataManager.Get().OnGameSaveDataUpdate -= OnGameSaveDataUpdate;
	}

	private void OnReady(object unused)
	{
		m_widget.BindDataModel(AchievementManager.Get().Categories);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_widget.TriggerEvent("DISABLE_INTERACTION");
		}
	}

	private void OnCurrentEventChanged(bool eventEnded)
	{
		if (eventEnded && JournalPopup.s_isShowing)
		{
			CloseJournal();
		}
		UpdateJournalMetadata();
	}

	private void UpdateJournalMetadata()
	{
		RewardTrack rewardTrack = RewardTrackManager.Get().GetRewardTrack(m_rewardTrackType);
		GameSaveDataManager gsdManager = GameSaveDataManager.Get();
		bool num = rewardTrack?.IsValid ?? false;
		bool shouldShowApprenticeTrack = !GameUtils.HasCompletedApprentice() && RewardTrackManager.Get().IsApprenticeTrackReady();
		bool isMainJournal = m_rewardTrackType == Global.RewardTrackType.GLOBAL;
		if ((!num && !(isMainJournal && shouldShowApprenticeTrack)) || !gsdManager.IsDataReady(GameSaveKeyId.PROGRESSION) || !gsdManager.IsDataReady(GameSaveKeyId.PLAYER_FLAGS) || !QuestManager.Get().HasReceivedQuestStatesFromServer)
		{
			return;
		}
		if (ButtonNeedsRewardTrackDatamodel() && m_widget.GetDataModel<RewardTrackDataModel>() == null && rewardTrack.TrackDataModel != null)
		{
			m_widget.BindDataModel(rewardTrack.TrackDataModel);
		}
		if (m_journalMetaDatamodel == null)
		{
			m_journalMetaDatamodel = new JournalMetaDataModel();
			m_widget.BindDataModel(m_journalMetaDatamodel);
		}
		Box box = Box.Get();
		if (box != null)
		{
			BoxRailroadManager railroadManager = box.GetRailroadManager();
			m_journalMetaDatamodel.HidingAllBadges = railroadManager != null && railroadManager.ShouldDisableButtonType(Box.ButtonType.JOURNAL);
		}
		RewardTrackManager.Get().UpdateJournalMetaWithRewardTrack(m_journalMetaDatamodel, (isMainJournal && shouldShowApprenticeTrack) ? Global.RewardTrackType.APPRENTICE : m_rewardTrackType);
		if (m_rewardTrackType != Global.RewardTrackType.GLOBAL)
		{
			return;
		}
		gsdManager.GetSubkeyValue(GameSaveKeyId.PLAYER_FLAGS, GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_COMPLETED_APPRENTICE, out long hasCompletedApprenticeFlag);
		bool shouldApprentriceTrackBeActive = hasCompletedApprenticeFlag == 0;
		bool shouldTavernGuideBeActive = TavernGuideManager.Get().IsTavernGuideActive();
		if (JournalPopup.s_isShowing)
		{
			if (m_updateApprenticeTrackAvailabilityCoroutine == null && shouldApprentriceTrackBeActive != m_journalMetaDatamodel.IsApprenticeTrackActive)
			{
				m_updateApprenticeTrackAvailabilityCoroutine = StartCoroutine(UpdateIsApprenticeTrackActive(shouldApprentriceTrackBeActive));
			}
			if (m_updateTavernGuideAvailabilityCoroutine == null && shouldTavernGuideBeActive != m_journalMetaDatamodel.IsTavernGuideActive)
			{
				m_updateTavernGuideAvailabilityCoroutine = StartCoroutine(UpdateIsTavernGuideActive(shouldTavernGuideBeActive));
			}
		}
		else
		{
			m_journalMetaDatamodel.IsApprenticeTrackActive = shouldApprentriceTrackBeActive;
			m_journalMetaDatamodel.IsTavernGuideActive = shouldTavernGuideBeActive;
		}
		TavernGuideManager.Get().UpdateJournalMetaForTavernGuide(m_journalMetaDatamodel);
		SpecialEventManager specialEventManager = SpecialEventManager.Get();
		specialEventManager.UpdateJournalMetaWithSpecialEvent(m_journalMetaDatamodel);
		if (m_journalMetaDatamodel.EventActive)
		{
			SpecialEventDataModel currentEvent = specialEventManager.GetEventDataModelForCurrentEvent();
			if (currentEvent != null)
			{
				m_widget.BindDataModel(currentEvent);
			}
		}
		UpdateDefaultTab();
		m_journalMetaDatamodel.HasJustCompletedApprentice = RewardTrackManager.Get().DidJustCompleteApprentice;
	}

	private void OnGameSaveDataUpdate(GameSaveKeyId key)
	{
		if (key == GameSaveKeyId.PLAYER_FLAGS)
		{
			UpdateJournalMetadata();
		}
	}

	private IEnumerator UpdateIsApprenticeTrackActive(bool shouldApprentriceTrackBeActive)
	{
		if (m_journalMetaDatamodel.IsApprenticeTrackActive && !shouldApprentriceTrackBeActive)
		{
			yield return StartCoroutine(PopupDisplayManager.Get().WaitForAllPopups());
		}
		m_journalMetaDatamodel.IsApprenticeTrackActive = shouldApprentriceTrackBeActive;
	}

	private IEnumerator UpdateIsTavernGuideActive(bool shouldTavernGuideBeActive)
	{
		RewardTrack rewardTrack = RewardTrackManager.Get().GetRewardTrack(Assets.Achievement.RewardTrackType.APPRENTICE);
		if (rewardTrack != null)
		{
			while (rewardTrack.IsRewardClaimPending)
			{
				yield return null;
			}
			yield return null;
			yield return StartCoroutine(PopupDisplayManager.Get().WaitForAllPopups());
		}
		if (m_journalMetaDatamodel.IsTavernGuideActive && !shouldTavernGuideBeActive)
		{
			yield return null;
			yield return StartCoroutine(PopupDisplayManager.Get().WaitForAllPopups());
		}
		m_journalMetaDatamodel.IsTavernGuideActive = shouldTavernGuideBeActive;
		m_updateTavernGuideAvailabilityCoroutine = null;
	}

	private void UpdateDefaultTab()
	{
		JournalTrayDisplay.JournalTab currentTab = JournalTrayDisplay.GetActiveTabForTrackType(Global.RewardTrackType.GLOBAL);
		bool eventIsActive = SpecialEventManager.Get().GetCurrentSpecialEvent() != null;
		if (m_journalMetaDatamodel.IsApprenticeTrackActive && currentTab != JournalTrayDisplay.JournalTab.ApprenticeTrack && currentTab != JournalTrayDisplay.JournalTab.TavernGuide && (currentTab != JournalTrayDisplay.JournalTab.Quest || m_journalMetaDatamodel.IsTavernGuideActive))
		{
			JournalTrayDisplay.SetActiveTabForTrackType(Global.RewardTrackType.GLOBAL, JournalTrayDisplay.JournalTab.ApprenticeTrack);
		}
		else if (!m_journalMetaDatamodel.IsApprenticeTrackActive && currentTab == JournalTrayDisplay.JournalTab.ApprenticeTrack)
		{
			JournalTrayDisplay.SetActiveTabForTrackType(Global.RewardTrackType.GLOBAL, JournalTrayDisplay.JournalTab.Reward);
		}
		else if (m_journalMetaDatamodel.IsTavernGuideActive && currentTab == JournalTrayDisplay.JournalTab.Quest)
		{
			JournalTrayDisplay.SetActiveTabForTrackType(Global.RewardTrackType.GLOBAL, JournalTrayDisplay.JournalTab.TavernGuide);
		}
		else if (!m_journalMetaDatamodel.IsTavernGuideActive && currentTab == JournalTrayDisplay.JournalTab.TavernGuide)
		{
			JournalTrayDisplay.SetActiveTabForTrackType(Global.RewardTrackType.GLOBAL, JournalTrayDisplay.JournalTab.Quest);
		}
		else if (!eventIsActive && currentTab == JournalTrayDisplay.JournalTab.Event)
		{
			JournalTrayDisplay.SetActiveTabForTrackType(Global.RewardTrackType.GLOBAL, JournalTrayDisplay.JournalTab.Quest);
		}
		else if (m_journalMetaDatamodel.EventIsNew && !JournalPopup.s_isShowing && GameUtils.HasCompletedApprentice())
		{
			JournalTrayDisplay.SetActiveTabForTrackType(Global.RewardTrackType.GLOBAL, JournalTrayDisplay.JournalTab.Event);
		}
	}

	private void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "BUTTON_CLICKED":
			ShowJournal();
			break;
		case "SHOW_TOOLTIP":
			if (m_toolTip != null && PlatformSettings.Screen >= ScreenCategory.Tablet)
			{
				m_toolTip.ShowBoxTooltip(GameStrings.Get("GLUE_TOOLTIP_BUTTON_JOURNAL_HEADLINE"), GameStrings.Get("GLUE_TOOLTIP_BUTTON_JOURNAL_DESC"));
			}
			break;
		case "HIDE_TOOLTIP":
			if (m_toolTip != null && PlatformSettings.Screen >= ScreenCategory.Tablet)
			{
				m_toolTip.HideTooltip();
			}
			break;
		}
	}

	private void OnJournalPopupWidgetReady()
	{
		JournalPopup journalPopup = m_journalPopUpWidget.GetComponentInChildren<JournalPopup>();
		if (!(journalPopup == null))
		{
			journalPopup.UpdateJournalMeta();
			journalPopup.Show();
		}
	}

	private bool ButtonNeedsRewardTrackDatamodel()
	{
		return m_rewardTrackType == Global.RewardTrackType.BATTLEGROUNDS;
	}

	public void ShowJournal()
	{
		if (Box.Get().IsInStateWithButtons() && !PopupDisplayManager.Get().IsShowing && !(m_journalPopUpWidget != null) && m_journalMetaDatamodel != null)
		{
			m_journalPopUpWidget = WidgetInstance.Create(popupAssetReference);
			m_journalPopUpWidget.RegisterReadyListener(delegate
			{
				OnJournalPopupWidgetReady();
			});
			m_journalMetaDatamodel.DoneChangingTabs = false;
			m_journalPopUpWidget.BindDataModel(m_journalMetaDatamodel);
			m_widget.TriggerEvent("JOURNAL_OPENED");
		}
	}

	public void CloseJournal()
	{
		if (!(m_journalPopUpWidget == null))
		{
			m_journalPopUpWidget.TriggerEvent("CODE_CLOSE");
		}
	}

	public JournalTrayDisplay GetJournalTrayDisplay()
	{
		if (m_journalPopUpWidget == null)
		{
			return null;
		}
		return m_journalPopUpWidget.GetComponentInChildren<JournalTrayDisplay>();
	}
}
