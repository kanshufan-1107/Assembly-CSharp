using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.InGameMessage;
using Hearthstone.InGameMessage.UI;
using Hearthstone.Progression;
using Hearthstone.Store;
using Hearthstone.UI;
using PegasusLettuce;
using UnityEngine;

namespace Hearthstone;

public class LettuceVillage : MonoBehaviour
{
	[Serializable]
	public class BuildingReferences
	{
		public MercenaryBuilding.Mercenarybuildingtype BuildingType;

		public AsyncReference BuildingReference;

		[NonSerialized]
		public VisualController BuildingController;

		[NonSerialized]
		public MercenaryVillageBuildingDataModel DataModel;

		[NonSerialized]
		public bool IsGated;
	}

	public class LettuceTutorialNotification
	{
		public Notification helpPopup;

		public LettuceTutorialVo.LettuceTutorialEvent tutEvent;
	}

	private const string VILLAGE_INTRO_ANIMATION_FINISHED_EVENT = "VILLAGE_INTRO_ANIMATION_FINISHED";

	private const string SHOW_TIER_EVENT = "SHOW_TIER";

	private const string ANIMATE_TIER_CHANGE_EVENT = "ANIMATE_TIER_CHANGE";

	private const string BUILDING_CLICKED_EVENT = "BUILDING_CLICKED";

	private const string BUILDING_UPGRADE_ANIM_FINISHED_EVENT = "BUILDING_UPGRADE_ANIM_FINISHED_EVENT_CODE";

	private const string BUILDING_UPGRADE_ANIM_START_EVENT = "BUILDING_UPGRADE_ANIM_START_EVENT_CODE";

	private const string TUTORIAL_POPUP_EVENT = "TUTORIAL_POPUP_EVENT_";

	private const string VISITOR_DONE_LEAVING_EVENT = "DONE_LEAVING";

	private const string VISITOR_ARRIVE_EVENT = "ARRIVE";

	private const string VISITOR_SHOW_IMMEDIATELY_EVENT = "SHOW_IMMEDIATELY";

	private const string VISITOR_LEAVE_EVENT = "LEAVE";

	private const string PACK_BUTTON_CLICKED_EVENT = "PACK_BUTTON_CLICKED";

	private const int MILLHOUSE_GRANT_TUTORIAL_DIALOG_ID = 105;

	private const int SHOP_BUILD_TUTORIAL_DIALOG_ID = 107;

	private const int TASKBOARD_TIER_1_ID = 43;

	private const int TUTORIAL_ABILITY_UPGRADE_MERCENARY_ID = 69;

	private const int SECOND_TUTORIAL_TASK_ID = 27868;

	public AsyncReference VisualControllerReference;

	public AsyncReference[] Visitors;

	public BuildingReferences[] Buildings;

	public List<LettuceVillageTutorialBone> TutorialBones;

	public AsyncReference m_villageBackButtonReference;

	public float m_firstNewVisitorDelayInSeconds = 0.75f;

	public float m_nextNewVisitorDelayInSeconds = 0.5f;

	private VisualController m_visualController;

	private Widget m_widget;

	private LettuceVillagePopupManager m_popupManager;

	private LettuceVillageDisplay m_display;

	private List<MercenaryVillageVisitorDataModel> m_existingVisitorsWaitingToEnter = new List<MercenaryVillageVisitorDataModel>();

	private List<MercenaryVillageVisitorDataModel> m_newVisitorsWaitingToEnter = new List<MercenaryVillageVisitorDataModel>();

	private Dictionary<int, VisualController> m_visibleVisitors = new Dictionary<int, VisualController>();

	private Dictionary<MercenaryBuilding.Mercenarybuildingtype, BuildingReferences> m_buildingLookup = new Dictionary<MercenaryBuilding.Mercenarybuildingtype, BuildingReferences>();

	private List<VisualController> m_unusedVisitors = new List<VisualController>();

	private List<int> m_idsToRemove = new List<int>();

	private MercenaryVillageDataModel m_villageDataModel;

	private bool m_villageUpdateReceived;

	private bool m_referencesSet;

	private List<LettuceTutorialNotification> m_helpPopups = new List<LettuceTutorialNotification>();

	private static HashSet<long> s_mercsWhoseArrivalHasBeenShown;

	private HashSet<int> m_mercsWhoseArrivalHasNotBeenShown = new HashSet<int>();

	private bool m_runningNewlyArrivedVisitorsCoRoutine;

	private float m_timeToShowNextNewlyArrivedVisitor;

	private bool m_isBuildingAnimating;

	private bool m_introAnimationComplete;

	private bool m_showNotificationsAfterEntry;

	private int m_numBuildingsToBuild;

	private int m_numBuildingsToUpgrade;

	public bool VillageIsReady { get; private set; }

	private void Start()
	{
		m_widget = GetComponent<Widget>();
		m_widget.RegisterEventListener(HandleEvent);
		VisualControllerReference.RegisterReadyListener<VisualController>(OnVisualControllerReady);
		AsyncReference[] visitors = Visitors;
		for (int i = 0; i < visitors.Length; i++)
		{
			visitors[i].RegisterReadyListener<VisualController>(OnVisitorReady);
		}
		BuildingReferences[] buildings = Buildings;
		foreach (BuildingReferences bldg in buildings)
		{
			if (bldg.BuildingReference != null)
			{
				m_buildingLookup.Add(bldg.BuildingType, bldg);
				bldg.BuildingReference.RegisterReadyListener(delegate(VisualController v)
				{
					OnBuildingControllerReady(v, bldg);
				});
			}
		}
		m_villageBackButtonReference.RegisterReadyListener<UIBButton>(OnBackButtonReady);
		InitializeVillageData();
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_MercenariesVillage);
		StartCoroutine(WaitForVillageToBeReady());
	}

	private void OnDestroy()
	{
		StopWaitingForUpdates();
	}

	private void OnVisualControllerReady(VisualController visualController)
	{
		m_visualController = visualController;
	}

	private void OnBuildingControllerReady(VisualController visualController, BuildingReferences bldg)
	{
		_ = visualController.OwningWidget;
		bldg.BuildingController = visualController;
	}

	private void OnVisitorReady(VisualController visualController)
	{
		m_unusedVisitors.Add(visualController);
		visualController.OwningWidget.RegisterEventListener(delegate(string eventName)
		{
			HandleVisitorEvent(eventName, visualController);
		});
	}

	private void OnBackButtonReady(UIBButton buttonController)
	{
		if (buttonController == null)
		{
			Error.AddDevWarning("UI Error!", "BackButton could not be found! You will not be able to click 'Back'!");
			return;
		}
		buttonController.AddEventListener(UIEventType.RELEASE, delegate
		{
			ExitScene();
		});
	}

	private void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "VILLAGE_INTRO_ANIMATION_FINISHED":
			OnVillageEntered();
			break;
		case "BUILDING_CLICKED":
			HideHelpPopups();
			if (m_widget.GetDataModel<EventDataModel>()?.Payload is MercenaryVillageBuildingDataModel bldgModel3)
			{
				ExecuteBuildingClicked(bldgModel3.BuildingType);
			}
			break;
		case "BUILDING_UPGRADE_ANIM_START_EVENT_CODE":
			if (m_widget.GetDataModel<EventDataModel>()?.Payload is MercenaryVillageBuildingDataModel bldgModel2)
			{
				bldgModel2.IsConstructionAnimationPlaying = true;
				m_isBuildingAnimating = true;
			}
			break;
		case "BUILDING_UPGRADE_ANIM_FINISHED_EVENT_CODE":
			if (m_widget.GetDataModel<EventDataModel>()?.Payload is MercenaryVillageBuildingDataModel bldgModel)
			{
				bldgModel.IsConstructionAnimationPlaying = false;
				bool tutorialTriggered = false;
				if (bldgModel.BuildingType == MercenaryBuilding.Mercenarybuildingtype.COLLECTION)
				{
					tutorialTriggered = LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_TAVERN_POPUP, base.gameObject);
				}
				if (bldgModel.BuildingType == MercenaryBuilding.Mercenarybuildingtype.MAILBOX)
				{
					MercenaryMessageUtils.AddMercenaryWelcomeMessage();
					UpdateMailCount();
					tutorialTriggered = LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_MAILBOX_BUILD_END, base.gameObject);
				}
				if (bldgModel.BuildingType == MercenaryBuilding.Mercenarybuildingtype.TASKBOARD && bldgModel.CurrentTierId == 43)
				{
					ProcessWaitingVisitors();
				}
				if (!tutorialTriggered)
				{
					BuildingTierDbfRecord record = LettuceVillageDataUtil.GetTierRecordByTierId(bldgModel.CurrentTierId);
					if (record.OnUpgradedDialog == 105)
					{
						NarrativeManager.Get().OnVillageBuildingUpgraded(record, delegate
						{
							ShowMercenaryTutorialGrant();
						});
					}
					else if (record.OnUpgradedDialog == 107)
					{
						NarrativeManager.Get().OnVillageBuildingUpgraded(record, delegate
						{
							if (LettuceVillageDataUtil.IsMercShopAvailable())
							{
								LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_SHOP, base.gameObject);
							}
							else
							{
								LettuceTutorialUtils.ForceCompleteEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_SHOP_CLAIM_PACK_POPUP);
								UpdatePackOpeningButton();
								if (m_villageDataModel.NumPacksAvailable > 0)
								{
									LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_OPEN_PACK, base.gameObject);
								}
							}
						});
					}
					else
					{
						NarrativeManager.Get().OnVillageBuildingUpgraded(record);
					}
				}
			}
			m_isBuildingAnimating = false;
			break;
		case "PACK_BUTTON_CLICKED":
			ShowPackOpeningScreen();
			break;
		}
		if (eventName.StartsWith("TUTORIAL_POPUP_EVENT_"))
		{
			string value = eventName.Remove(0, "TUTORIAL_POPUP_EVENT_".Length);
			LettuceTutorialVo.LettuceTutorialEvent tutEvent = LettuceTutorialVo.LettuceTutorialEvent.INVALID;
			Enum.TryParse<LettuceTutorialVo.LettuceTutorialEvent>(value, ignoreCase: true, out tutEvent);
			HandleTutorialAttentionEvent(tutEvent);
		}
	}

	public void OnVillageEntered()
	{
		if (!LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_START) && CollectionManager.Get().HasFullyUpgradedAnyCollectibleMercenary())
		{
			LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_START, base.gameObject);
		}
		m_introAnimationComplete = true;
		if (m_showNotificationsAfterEntry)
		{
			UpdateNotificationsForBuildingsBasedOnTutorialState();
			m_showNotificationsAfterEntry = false;
		}
		PopupDisplayManager pdm = PopupDisplayManager.Get();
		if (pdm.IsShowing || (!PopupDisplayManager.ShouldSuppressPopups() && pdm.RewardPopups.HasUnAckedRewards()))
		{
			StartCoroutine(WaitForPopupManager());
			return;
		}
		TelemetryManager.Client().SendMercenariesVillageEnter();
		NetCache.ProfileNoticeMercenariesRewards rewardNotice = pdm.RewardPopups.GetNextNonAutoRetireRewardMercenariesRewardToShow();
		NetCache.ProfileNoticeMercenariesRewards bonusRewardNotice = PopupDisplayManager.Get().RewardPopups.GetNextBonusMercenariesRewardToShow();
		if (rewardNotice == null || !pdm.RewardPopups.ShowMercenariesRewards(autoOpenChest: false, rewardNotice, bonusRewardNotice, OnVillageEntered))
		{
			LettuceVillageDataUtil.ZoneWasRecentlyUnlocked = pdm.RewardPopups.ShowMercenariesZoneUnlockPopup(OnVillageEntered);
			if (!LettuceVillageDataUtil.ZoneWasRecentlyUnlocked && !pdm.RewardPopups.ShowMercenariesFullyUpgraded(OnVillageEntered) && !pdm.RewardPopups.ShowNextMercenariesSeasonRewards(OnVillageEntered))
			{
				SendEventUpwardStateAction.SendEventUpward(base.gameObject, "UNBLOCK_INPUT");
				OnVillageEnteredRewardsFinished();
			}
		}
	}

	private IEnumerator WaitForPopupManager()
	{
		PopupDisplayManager pdm = PopupDisplayManager.Get();
		yield return new WaitWhile(() => pdm.IsShowing || (!PopupDisplayManager.ShouldSuppressPopups() && pdm.RewardPopups.HasUnAckedRewards()));
		OnVillageEntered();
	}

	public void OnVillageEnteredRewardsFinished()
	{
		bool tutorialTriggered = LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_START, base.gameObject);
		if (!tutorialTriggered && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_WORKSHOP_INTRO_POPUP))
		{
			tutorialTriggered = LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_WORKSHOP_INTRO_POPUP, base.gameObject);
		}
		if (!tutorialTriggered)
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_HAS_ENTERED_COLLECTION, out long value);
			if (value > 0 && LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_UPGRADE_ABILITY_END))
			{
				tutorialTriggered = LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_TASK_BOARD_START, base.gameObject);
			}
		}
		if (!tutorialTriggered && (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.LETTUCE_PACK_OPENING || BoosterPackUtils.GetBoosterOpenedCount(629) > 0))
		{
			tutorialTriggered = LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_END, base.gameObject);
		}
		BuildingTierDbfRecord taskboardTargetTier = LettuceVillageDataUtil.GetTierRecordByTierIndex(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD, 2);
		if (!tutorialTriggered && taskboardTargetTier != null && LettuceVillageDataUtil.IsBuildingReadyToUpgrade(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD, taskboardTargetTier.ID))
		{
			tutorialTriggered = LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_BUILDING_UPGRADE_AVAILABLE_TASKBOARD, base.gameObject);
		}
		BuildingTierDbfRecord pvpTargetTier = LettuceVillageDataUtil.GetTierRecordByTierIndex(MercenaryBuilding.Mercenarybuildingtype.PVP, 1);
		if (!tutorialTriggered && pvpTargetTier != null && LettuceVillageDataUtil.IsBuildingReadyToUpgrade(MercenaryBuilding.Mercenarybuildingtype.PVP, pvpTargetTier.ID))
		{
			tutorialTriggered = LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_BUILDING_UPGRADE_AVAILABLE_PVP, base.gameObject);
		}
		BuildingTierDbfRecord pveTargetTier = LettuceVillageDataUtil.GetTierRecordByTierIndex(MercenaryBuilding.Mercenarybuildingtype.PVEZONES, 2);
		if (!tutorialTriggered && pveTargetTier != null && LettuceVillageDataUtil.IsBuildingReadyToUpgrade(MercenaryBuilding.Mercenarybuildingtype.PVEZONES, pveTargetTier.ID))
		{
			tutorialTriggered = LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_BUILDING_UPGRADE_AVAILABLE_PVEZONES, base.gameObject);
		}
		BuildingTierDbfRecord trainingHallTargetTier = LettuceVillageDataUtil.GetTierRecordByTierIndex(MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL, 1);
		if (!tutorialTriggered && trainingHallTargetTier != null && LettuceVillageDataUtil.IsBuildingReadyToUpgrade(MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL, trainingHallTargetTier.ID))
		{
			tutorialTriggered = LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_BUILDING_UPGRADE_AVAILABLE_TRAINING_HALL, base.gameObject);
		}
		if (GameUtils.IsMercenariesVillageTutorialComplete() && !BnetPresenceMgr.Get().GetMyPlayer().GetHearthstoneGameAccount()
			.GetMercenariesTutorialComplete())
		{
			BnetPresenceMgr.Get().SetGameField(29u, 1);
		}
		if (!tutorialTriggered)
		{
			ValidateTutorialStateAndFixIfNeeded();
			NarrativeManager.Get().OnVillageEntered();
		}
	}

	private void ShowMercenaryTutorialGrant()
	{
		BuildingTierDbfRecord PveZoneTierRecord = LettuceVillageDataUtil.GetCurrentTierRecordFromBuilding(MercenaryBuilding.Mercenarybuildingtype.PVEZONES);
		if (PveZoneTierRecord != null && PveZoneTierRecord.TutorialEventType == BuildingTier.VillageTutorialServerEvent.GRANT_TUTORIAL_MERCENARY)
		{
			RewardPresenter rewardPresenter = new RewardPresenter();
			LettuceMercenary mercenary = CollectionManager.Get().GetMercenary(PveZoneTierRecord.TutorialEventValue);
			LettuceMercenaryDataModel lettuceMercenaryDataModel = MercenaryFactory.CreateMercenaryDataModel(mercenary);
			lettuceMercenaryDataModel.Owned = true;
			RewardListDataModel rewards = new RewardListDataModel();
			RewardItemDataModel tutorialGrantReward = new RewardItemDataModel
			{
				Quantity = 1,
				ItemType = RewardItemType.MERCENARY,
				Mercenary = lettuceMercenaryDataModel
			};
			rewards.Items.Add(tutorialGrantReward);
			RewardScrollDataModel rewardScrollDataModel = new RewardScrollDataModel
			{
				DisplayName = GameStrings.Get("GLUE_LETTUCE_VILLAGE_TUTORIAL_GRANT_TITLE"),
				Description = mercenary.m_mercName,
				RewardList = rewards
			};
			rewardPresenter.EnqueueReward(rewardScrollDataModel, GeneralUtils.noOp);
			rewardPresenter.ShowNextReward(delegate
			{
				LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_TRAVEL, base.gameObject);
			});
		}
	}

	private void HandleVisitorEvent(string eventName, VisualController visualController)
	{
		if (eventName == "DONE_LEAVING")
		{
			if (!m_unusedVisitors.Contains(visualController))
			{
				m_unusedVisitors.Add(visualController);
				visualController.Owner.UnbindDataModel(336);
			}
			ProcessWaitingVisitors();
		}
	}

	private void HandleTutorialAttentionEvent(LettuceTutorialVo.LettuceTutorialEvent tutEvent)
	{
		LettuceVillageTutorialBone targetTutorialBone = null;
		foreach (LettuceVillageTutorialBone bone in TutorialBones)
		{
			if (bone.EventType == tutEvent)
			{
				targetTutorialBone = bone;
				break;
			}
		}
		if (targetTutorialBone != null && UserAttentionManager.CanShowAttentionGrabber("VillageDisplay.HandleTutorialAttentionEvent:" + tutEvent))
		{
			Notification helpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, targetTutorialBone.transform.localPosition, targetTutorialBone.transform.localScale, GameStrings.Get(targetTutorialBone.LocalizationKey));
			helpPopup.ShowPopUpArrow(targetTutorialBone.ArrowDirection);
			helpPopup.PulseReminderEveryXSeconds(3f);
			LettuceTutorialNotification notificationPopup = new LettuceTutorialNotification();
			notificationPopup.tutEvent = tutEvent;
			notificationPopup.helpPopup = helpPopup;
			m_helpPopups.Add(notificationPopup);
		}
	}

	public bool IsPlayingBoneTutorialEvent(LettuceTutorialVo.LettuceTutorialEvent tutEvent)
	{
		foreach (LettuceTutorialNotification helpPopup in m_helpPopups)
		{
			if (helpPopup.tutEvent == tutEvent)
			{
				return true;
			}
		}
		return false;
	}

	public void HideHelpPopups()
	{
		if (m_helpPopups == null)
		{
			return;
		}
		foreach (LettuceTutorialNotification notificationPopup in m_helpPopups)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(notificationPopup.helpPopup);
		}
		m_helpPopups.Clear();
	}

	private void OnPopupClosed(LettuceVillagePopupManager.PopupType popupType)
	{
		switch (popupType)
		{
		case LettuceVillagePopupManager.PopupType.WORKSHOP:
			CloseMercenariesWorkshop();
			if (m_villageUpdateReceived)
			{
				UpdateBuildingStates();
			}
			else
			{
				UpdateNotificationsForBuildingsBasedOnTutorialState();
			}
			break;
		case LettuceVillagePopupManager.PopupType.TASKBOARD:
			UpdateNotificationsForBuildingsBasedOnTutorialState();
			if (!ShouldShowCampfireTutorialNotification())
			{
				LettuceVillageDataUtil.MarkNotificationAsSeenForBuilding(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD);
				RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD);
			}
			UpdatePackOpeningButton();
			NarrativeManager.Get().OnVillageEntered();
			break;
		case LettuceVillagePopupManager.PopupType.TRAININGHALL:
			RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL);
			break;
		case LettuceVillagePopupManager.PopupType.MAILBOX:
			UpdateMailCount();
			break;
		case LettuceVillagePopupManager.PopupType.PVE:
			break;
		}
	}

	private void UpdateMailCount()
	{
		if (ServiceManager.TryGet<MessagePopupDisplay>(out var popupDisplay))
		{
			m_villageDataModel.NumMailboxUpdates = popupDisplay.GetUnreadMessageCountForFeed(PopupEvent.OnMercInbox);
		}
	}

	public void SetUIReferences(LettuceVillageDisplay display, LettuceVillagePopupManager popupManager)
	{
		m_popupManager = popupManager;
		m_display = display;
		m_referencesSet = true;
		LettuceVillagePopupManager popupManager2 = m_popupManager;
		popupManager2.OnPopupClosed = (Action<LettuceVillagePopupManager.PopupType>)Delegate.Combine(popupManager2.OnPopupClosed, new Action<LettuceVillagePopupManager.PopupType>(OnPopupClosed));
	}

	public MercenaryVillageDataModel GetVillageDataModel()
	{
		if (m_villageDataModel != null)
		{
			return m_villageDataModel;
		}
		if (!m_widget.GetDataModel(305, out var dataModel))
		{
			dataModel = new MercenaryVillageDataModel();
			m_widget.BindDataModel(dataModel);
		}
		return dataModel as MercenaryVillageDataModel;
	}

	private void ExitScene()
	{
		HideHelpPopups();
		NotificationManager.Get().DestroyAllPopUps();
		TelemetryManager.Client().SendMercenariesVillageExit();
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.GAME_MODE, SceneMgr.TransitionHandlerType.NEXT_SCENE);
	}

	private void ShowCollection()
	{
		if (!Network.IsLoggedIn())
		{
			DialogManager.Get().ShowReconnectHelperDialog(ShowCollection);
			return;
		}
		GameUtils.SetGSDFlag(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_HAS_ENTERED_COLLECTION, enableFlag: true);
		m_display.SetNextModeAndHandleTransition(SceneMgr.Mode.LETTUCE_COLLECTION, SceneMgr.TransitionHandlerType.NEXT_SCENE);
	}

	private void ShowPVPScreen()
	{
		LettuceVillageDisplay.LettuceSceneTransitionPayload sceneTransitionPayload = new LettuceVillageDisplay.LettuceSceneTransitionPayload();
		m_display.SetNextModeAndHandleTransition(SceneMgr.Mode.LETTUCE_PLAY, SceneMgr.TransitionHandlerType.NEXT_SCENE, sceneTransitionPayload);
	}

	private void ShowMercShop()
	{
		if (!Network.IsLoggedIn())
		{
			DialogManager.Get().ShowReconnectHelperDialog(ShowMercShop);
			return;
		}
		if (FriendChallengeMgr.Get().HasChallenge())
		{
			Debug.LogWarning("Cannot open Shop due to having friendly challenge.", base.gameObject);
			return;
		}
		StoreManager storeMgr = StoreManager.Get();
		if (storeMgr == null)
		{
			Debug.LogWarning("Cannot open Shop due to null StoreManager.", base.gameObject);
			return;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			Log.Store.PrintError("Product service not initialized.");
			return;
		}
		bool shouldFailToOpen = false;
		dataService.TryRefreshStaleProductAvailability();
		if (!storeMgr.IsOpen())
		{
			shouldFailToOpen = true;
		}
		else if (!dataService.HasStoreLoaded())
		{
			Debug.LogWarning("Cannot open Shop due to no valid tier data received.", base.gameObject);
			shouldFailToOpen = true;
		}
		else if (SceneMgr.Get() == null || SceneMgr.Get().GetMode() != SceneMgr.Mode.LETTUCE_VILLAGE || SceneMgr.Get().IsTransitionNowOrPending())
		{
			Debug.LogWarning("Cannot open Shop due to invalid scene state.", base.gameObject);
			shouldFailToOpen = true;
		}
		if (!shouldFailToOpen)
		{
			FriendChallengeMgr.Get().OnStoreOpened();
			storeMgr.StartGeneralTransaction("mercs", OnStoreBackButtonPressed);
			MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_MercenariesShop);
			storeMgr.GetCurrentStore().OnClosed += OnStoreClosed;
			storeMgr.GetCurrentStore().OnProductOpened += OnStoreProductOpened;
			LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_SHOP_CLAIM_PACK_POPUP, base.gameObject);
		}
	}

	private void OnStoreClosed(StoreClosedArgs e)
	{
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_MercenariesVillage);
		if (StoreManager.Get().GetCurrentStore() != null)
		{
			StoreManager.Get().GetCurrentStore().OnClosed -= OnStoreClosed;
			StoreManager.Get().GetCurrentStore().OnProductOpened -= OnStoreProductOpened;
		}
		UpdatePackOpeningButton();
		if (m_villageDataModel.NumPacksAvailable > 0)
		{
			LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_OPEN_PACK, base.gameObject);
		}
		HideHelpPopups();
	}

	private void OnStoreBackButtonPressed(bool authorizationBackButtonPressed, object userData)
	{
	}

	private void OnStoreProductOpened()
	{
		HideHelpPopups();
	}

	private void OpenMercenariesWorkshop()
	{
		if (!Network.IsLoggedIn())
		{
			DialogManager.Get().ShowReconnectHelperDialog(OpenMercenariesWorkshop);
			return;
		}
		StoreManager.Get().StartFakeStoreForMercenariesWorkshop();
		BnetBar.Get()?.RefreshCurrency();
	}

	private void CloseMercenariesWorkshop()
	{
		StoreManager.Get().StopFakeMercenariesWorkshopStoreAndRestorePrevious();
		BnetBar.Get()?.RefreshCurrency();
	}

	private void ShowPackOpeningScreen()
	{
		if (!Network.IsLoggedIn())
		{
			DialogManager.Get().ShowReconnectHelperDialog(ShowPackOpeningScreen);
			return;
		}
		HideHelpPopups();
		m_display.SetNextModeAndHandleTransition(SceneMgr.Mode.LETTUCE_PACK_OPENING, SceneMgr.TransitionHandlerType.NEXT_SCENE);
	}

	private void InitializeVillageData()
	{
		m_villageDataModel = GetVillageDataModel();
		if (m_villageDataModel == null)
		{
			Log.Net.PrintError("Could not retrieve village data model.");
			return;
		}
		NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (guardianVars != null)
		{
			m_villageDataModel.Enabled = guardianVars.MercenariesEnableVillages;
		}
	}

	private void StartWaitingForUpdates()
	{
		NetCache netCache = NetCache.Get();
		netCache.RegisterUpdatedListener(typeof(NetCache.NetCacheMercenariesVillageVisitorInfo), OnVisitorDataUpdated);
		netCache.RegisterUpdatedListener(typeof(NetCache.NetCacheMercenariesVillageInfo), OnVillageDataUpdated);
	}

	private void StopWaitingForUpdates()
	{
		NetCache netCache = NetCache.Get();
		if (netCache != null)
		{
			netCache.RemoveUpdatedListener(typeof(NetCache.NetCacheMercenariesVillageVisitorInfo), OnVisitorDataUpdated);
			netCache.RemoveUpdatedListener(typeof(NetCache.NetCacheMercenariesVillageInfo), OnVillageDataUpdated);
		}
	}

	private IEnumerator WaitForVillageToBeReady()
	{
		while (!VisualControllerReference.IsReady)
		{
			yield return null;
		}
		AsyncReference[] visitors = Visitors;
		foreach (AsyncReference visitorReference in visitors)
		{
			while (!visitorReference.IsReady)
			{
				yield return null;
			}
		}
		BuildingReferences[] buildings = Buildings;
		foreach (BuildingReferences bldg in buildings)
		{
			while (!bldg.BuildingReference.IsReady)
			{
				yield return null;
			}
		}
		while (!LettuceVillageDataUtil.Initialized)
		{
			yield return null;
		}
		while (!m_referencesSet)
		{
			yield return null;
		}
		VillageIsReady = true;
		StartWaitingForUpdates();
		OnVisitorDataUpdated();
		UpdateBuildingStates(updateTutorialNotifications: true);
		UpdatePackOpeningButton();
		MessagePopupDisplay popupDisplay = ServiceManager.Get<MessagePopupDisplay>();
		if (popupDisplay == null)
		{
			UIStatus.Get().AddError("Message Popup Display was not available to show a message");
			yield break;
		}
		List<NetCache.ProfileNoticeMercenariesSeasonRoll> seasonNotices = null;
		foreach (NetCache.ProfileNotice notice in NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>().Notices)
		{
			if (notice.Type == NetCache.ProfileNotice.NoticeType.MERCENARIES_SEASON_ROLL)
			{
				if (seasonNotices == null)
				{
					seasonNotices = new List<NetCache.ProfileNoticeMercenariesSeasonRoll>();
				}
				seasonNotices.Add(notice as NetCache.ProfileNoticeMercenariesSeasonRoll);
			}
		}
		if (seasonNotices != null)
		{
			seasonNotices.Sort((NetCache.ProfileNoticeMercenariesSeasonRoll a, NetCache.ProfileNoticeMercenariesSeasonRoll b) => a.EndedSeasonId.CompareTo(b.EndedSeasonId));
			for (int j = 0; j < seasonNotices.Count - 1; j++)
			{
				Network.Get().AckNotice(seasonNotices[j].NoticeID);
			}
			NetCache.ProfileNoticeMercenariesSeasonRoll currentNotice = seasonNotices[seasonNotices.Count - 1];
			MessageUIData data = new MessageUIData
			{
				LayoutType = MessageLayoutType.TEXT,
				MessageData = new TextMessageContent
				{
					ImageType = "mercs",
					Title = GameStrings.Get("GLUE_MERCENARIES"),
					TextBody = GameUtils.GetMercenariesSeasonEndDescription(currentNotice.EndedSeasonId, currentNotice.HighestSeasonRating)
				}
			};
			data.Callbacks.OnViewed = delegate
			{
				Network.Get().AckNotice(currentNotice.NoticeID);
			};
			popupDisplay.AddMessages(new List<MessageUIData> { data });
		}
	}

	private void UpdatePackOpeningButton()
	{
		m_villageDataModel.NumPacksAvailable = LettuceVillageDataUtil.GetNumberOfMercPacksToOpen();
		m_villageDataModel.ShouldAnimatePackButtonIn = LettuceVillageDataUtil.DidPackCountChangeFromZero(m_villageDataModel.NumPacksAvailable);
		LettuceVillageDataUtil.UpdatePrevPackCount(m_villageDataModel.NumPacksAvailable);
		m_villageDataModel.ShouldAnimatePackButtonOut = m_villageDataModel.NumPacksAvailable == 0 && SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.LETTUCE_PACK_OPENING;
	}

	public void Dev_ShowTutorialPopups()
	{
		HideHelpPopups();
		foreach (LettuceVillageTutorialBone bone in TutorialBones)
		{
			Notification helpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, bone.transform.localPosition, bone.transform.localScale, GameStrings.Get(bone.LocalizationKey));
			helpPopup.ShowPopUpArrow(bone.ArrowDirection);
			helpPopup.PulseReminderEveryXSeconds(3f);
			m_helpPopups.Add(new LettuceTutorialNotification
			{
				tutEvent = bone.EventType,
				helpPopup = helpPopup
			});
		}
	}

	public void UpdateBuildingStates(bool updateTutorialNotifications = false)
	{
		if (!VillageIsReady)
		{
			return;
		}
		NetCache netCache = NetCache.Get();
		if (netCache == null)
		{
			Log.Lettuce.PrintError("UpdateBuildingStates - NetCache is null");
			return;
		}
		NetCache.NetCacheMercenariesVillageInfo villageInfo = netCache.GetNetObject<NetCache.NetCacheMercenariesVillageInfo>();
		if (villageInfo == null)
		{
			Log.Lettuce.PrintError("UpdateBuildingStates - VillageInfo is null");
			return;
		}
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = netCache.GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (playerInfo == null)
		{
			Log.Lettuce.PrintError("UpdateBuildingStates - Can't access NetCacheMercenariesPlayerInfo");
			return;
		}
		Dictionary<MercenaryBuilding.Mercenarybuildingtype, bool> buildingsEnabledMap = playerInfo.BuildingEnabledMap;
		m_villageUpdateReceived = false;
		List<MercenariesBuildingState> buildingStates = LettuceVillageDataUtil.BuildingStates;
		m_numBuildingsToBuild = 0;
		m_numBuildingsToUpgrade = 0;
		List<MercenaryBuilding.Mercenarybuildingtype> availableTutorialBuildings = LettuceVillageDataUtil.GetAvailableBuildingsForCurrentFTUEState();
		foreach (MercenariesBuildingState state in buildingStates)
		{
			MercenaryBuildingDbfRecord bldgRecord = LettuceVillageDataUtil.GetBuildingRecordByID(state.BuildingId);
			if (!m_buildingLookup.TryGetValue(bldgRecord.MercenaryBuildingType, out var bldg))
			{
				continue;
			}
			bool needsUpdate = false;
			bool animateUpdate = false;
			MercenaryVillageBuildingDataModel bldgModel = bldg.DataModel;
			bool operational;
			bool shouldBeEnabled = buildingsEnabledMap.TryGetValue(bldgRecord.MercenaryBuildingType, out operational) && m_villageDataModel.Enabled && operational;
			if (bldgModel == null)
			{
				needsUpdate = true;
				animateUpdate = false;
				MercenaryVillageBuildingDataModel dataModel = new MercenaryVillageBuildingDataModel
				{
					BuildingType = bldgRecord.MercenaryBuildingType,
					CurrentTierId = state.CurrentTierId,
					Enabled = shouldBeEnabled,
					IsConstructionAnimationPlaying = false
				};
				bldgModel = (bldg.DataModel = dataModel);
				bldg.BuildingController.BindDataModel(bldgModel);
			}
			else if (bldgModel.CurrentTierId != state.CurrentTierId)
			{
				needsUpdate = true;
				animateUpdate = true;
				bldgModel.CurrentTierId = state.CurrentTierId;
				bldgModel.Enabled = shouldBeEnabled;
			}
			else if (bldgModel.Enabled != shouldBeEnabled)
			{
				bldgModel.Enabled = shouldBeEnabled;
			}
			int nextTierId = -1;
			if (bldgModel != null)
			{
				BuildingTierDbfRecord record = LettuceVillageDataUtil.GetTierRecordByTierId(bldgModel.CurrentTierId);
				if (record != null)
				{
					BuildingTierDbfRecord nextTierRecord = LettuceVillageDataUtil.GetNextTierRecord(record);
					if (nextTierRecord != null)
					{
						nextTierId = nextTierRecord.ID;
					}
				}
			}
			bool buildingIsBuilt = villageInfo.BuildingIsBuilt(state);
			bool nextTierAchievementComplete = nextTierId == -1 || LettuceVillageDataUtil.IsBuildingTierAchievementComplete(nextTierId);
			bool buildingAvailableInTutorial = LettuceVillageDataUtil.IsBuildingAvailableInTutorial(state.BuildingId, availableTutorialBuildings);
			if (shouldBeEnabled && !buildingIsBuilt && buildingAvailableInTutorial && nextTierAchievementComplete)
			{
				m_numBuildingsToBuild++;
			}
			if (shouldBeEnabled && buildingIsBuilt && buildingAvailableInTutorial && nextTierAchievementComplete && LettuceVillageDataUtil.IsBuildingReadyToUpgrade(bldgRecord.MercenaryBuildingType))
			{
				m_numBuildingsToUpgrade++;
			}
			if (needsUpdate)
			{
				if (animateUpdate)
				{
					bldg.BuildingController.SetState("ANIMATE_TIER_CHANGE");
				}
				else
				{
					bldg.BuildingController.SetState("SHOW_TIER");
				}
			}
		}
		bool showPVEZoneNotification = false;
		if (NetCache.Get().GetNetObject<NetCache.NetCacheLettuceMap>()?.Map?.Active != true)
		{
			if (villageInfo.UnlockedBountyDifficultyLevel >= 2)
			{
				GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_HAS_SEEN_HEROIC_PVE_ZONE_FANFARE, out long value);
				showPVEZoneNotification = value == 0;
			}
			if (villageInfo.UnlockedBountyDifficultyLevel >= 3)
			{
				GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_HAS_SEEN_MYTHIC_PVE_ZONE_FANFARE, out long value2);
				showPVEZoneNotification = value2 == 0;
			}
		}
		RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.PVEZONES, showPVEZoneNotification);
		RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.BUILDINGMANAGER, m_numBuildingsToBuild > 0);
		RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.PVP);
		RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.COLLECTION);
		RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL);
		RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.SHOP);
		RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD);
		UpdateMailCount();
		if (updateTutorialNotifications)
		{
			UpdateNotificationsForBuildingsBasedOnTutorialState();
		}
	}

	private void RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype buildingType, bool alternateCondition = false)
	{
		if (m_villageDataModel != null)
		{
			bool notificationStatus = LettuceVillageDataUtil.GetNotificationStatusForBuilding(buildingType) || alternateCondition;
			switch (buildingType)
			{
			case MercenaryBuilding.Mercenarybuildingtype.BUILDINGMANAGER:
				m_villageDataModel.WorkshopHasUpdates = notificationStatus;
				break;
			case MercenaryBuilding.Mercenarybuildingtype.COLLECTION:
				m_villageDataModel.CollectionHasUpdates = notificationStatus;
				break;
			case MercenaryBuilding.Mercenarybuildingtype.PVEZONES:
				m_villageDataModel.ZoneSelectionHasUpdates = notificationStatus;
				break;
			case MercenaryBuilding.Mercenarybuildingtype.PVP:
				m_villageDataModel.ArenaHasUpdates = notificationStatus;
				break;
			case MercenaryBuilding.Mercenarybuildingtype.SHOP:
				m_villageDataModel.ShopHasUpdates = notificationStatus;
				break;
			case MercenaryBuilding.Mercenarybuildingtype.TASKBOARD:
				m_villageDataModel.TaskboardHasUpdates = notificationStatus;
				break;
			case MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL:
				m_villageDataModel.TrainingHallHasUpdates = notificationStatus;
				break;
			case MercenaryBuilding.Mercenarybuildingtype.MAILBOX:
				UpdateMailCount();
				break;
			}
		}
	}

	private bool ShouldShowCampfireTutorialNotification()
	{
		if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_OPEN_TASK_DETAIL))
		{
			return !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_TASK_COLLECTION_POPUP);
		}
		return false;
	}

	private void UpdateNotificationsForBuildingsBasedOnTutorialState()
	{
		if (!m_introAnimationComplete)
		{
			m_showNotificationsAfterEntry = true;
			return;
		}
		bool workShopNotificationCanShow = m_numBuildingsToBuild > 0 || m_numBuildingsToUpgrade > 0;
		if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_WORKSHOP_INTRO_POPUP) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_TAVERN_POPUP))
		{
			RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.BUILDINGMANAGER, workShopNotificationCanShow);
			HandleTutorialAttentionEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_WORKSHOP_INTRO_POPUP);
		}
		else if ((LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_TASK_BOARD_END) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_TRAVEL)) || (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_SHOP_BUILD_START) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_SHOP)) || (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_END) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_MAILBOX_BUILD_END)))
		{
			RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.BUILDINGMANAGER, workShopNotificationCanShow);
		}
		else if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_TASK_BOARD_START) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_TASK_BOARD_END))
		{
			MercenaryBuildingDbfRecord taskBoardRecord = LettuceVillageDataUtil.GetBuildingRecordByType(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD);
			BuildingTierDbfRecord currentTaskBoardTier = LettuceVillageDataUtil.GetCurrentTierRecordFromBuilding(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD);
			if (taskBoardRecord != null && currentTaskBoardTier != null && taskBoardRecord.DefaultTier == currentTaskBoardTier.ID)
			{
				RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.BUILDINGMANAGER, workShopNotificationCanShow);
			}
		}
		if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_TAVERN_POPUP) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_UPGRADE_ABILITY_END))
		{
			RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.COLLECTION, alternateCondition: true);
			HandleTutorialAttentionEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_TAVERN_POPUP);
		}
		else if (LettuceVillageDataUtil.RecentlyClaimedTaskId == 27868)
		{
			LettuceVillageDataUtil.RecentlyClaimedTaskId = 0;
			RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.COLLECTION, alternateCondition: true);
		}
		if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_START) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_END))
		{
			HandleTutorialAttentionEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_START);
		}
		if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_START) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_BOSS_RUSH_POPUP))
		{
			RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.BUILDINGMANAGER, workShopNotificationCanShow);
		}
		if (ShouldShowCampfireTutorialNotification())
		{
			RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD, alternateCondition: true);
		}
	}

	private void ValidateTutorialStateAndFixIfNeeded()
	{
		MercenaryBuildingDbfRecord travelPointRecord = LettuceVillageDataUtil.GetBuildingRecordByType(MercenaryBuilding.Mercenarybuildingtype.PVEZONES);
		BuildingTierDbfRecord currentTravelPointTier = LettuceVillageDataUtil.GetCurrentTierRecordFromBuilding(MercenaryBuilding.Mercenarybuildingtype.PVEZONES);
		if (travelPointRecord != null && currentTravelPointTier != null && travelPointRecord.DefaultTier != currentTravelPointTier.ID && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_TRAVEL))
		{
			LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_TRAVEL, base.gameObject);
		}
		if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_UPGRADE_ABILITY_START) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_UPGRADE_ABILITY_END) && !CollectionManager.Get().GetMercenary(69L).CanAnyAbilityBeUpgraded())
		{
			LettuceTutorialUtils.ForceCompleteEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_UPGRADE_ABILITY_END);
			UpdateNotificationsForBuildingsBasedOnTutorialState();
		}
		if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_TRAVEL) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_SHOP_BUILD_START))
		{
			foreach (MercenariesVisitorState visitorState in LettuceVillageDataUtil.VisitorStates)
			{
				if (visitorState.ActiveTaskState.TaskId == 27868)
				{
					LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_SHOP_BUILD_START, base.gameObject, 0, 0, delegate
					{
						UpdateNotificationsForBuildingsBasedOnTutorialState();
					});
					break;
				}
			}
		}
		if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_OPEN_TASK_DETAIL) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VIEW_TASKS_POPUP))
		{
			LettuceTutorialUtils.ForceCompleteEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VIEW_TASKS_POPUP);
		}
	}

	private void ExecuteBuildingClicked(MercenaryBuilding.Mercenarybuildingtype buildingType)
	{
		if (m_isBuildingAnimating)
		{
			return;
		}
		switch (buildingType)
		{
		case MercenaryBuilding.Mercenarybuildingtype.BUILDINGMANAGER:
			if (!IsPlayingBoneTutorialEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_WORKSHOP_INTRO_POPUP) && !LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_WORKSHOP_INTRO_POPUP))
			{
				return;
			}
			OpenMercenariesWorkshop();
			m_popupManager.Show(LettuceVillagePopupManager.PopupType.WORKSHOP);
			break;
		case MercenaryBuilding.Mercenarybuildingtype.COLLECTION:
			ShowCollection();
			break;
		case MercenaryBuilding.Mercenarybuildingtype.PVEZONES:
			m_popupManager.Show(LettuceVillagePopupManager.PopupType.PVE);
			break;
		case MercenaryBuilding.Mercenarybuildingtype.PVP:
			ShowPVPScreen();
			break;
		case MercenaryBuilding.Mercenarybuildingtype.SHOP:
			ShowMercShop();
			break;
		case MercenaryBuilding.Mercenarybuildingtype.TASKBOARD:
			m_popupManager.Show(LettuceVillagePopupManager.PopupType.TASKBOARD);
			break;
		case MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL:
			m_popupManager.Show(LettuceVillagePopupManager.PopupType.TRAININGHALL);
			break;
		case MercenaryBuilding.Mercenarybuildingtype.MAILBOX:
			m_popupManager.Show(LettuceVillagePopupManager.PopupType.MAILBOX);
			break;
		}
		MercenaryBuildingDbfRecord buildingTierDbfRecord = LettuceVillageDataUtil.GetBuildingRecordByType(buildingType);
		TelemetryManager.Client().SendMercenariesVillageBuildingClicked(buildingTierDbfRecord.ID);
		LettuceVillageDataUtil.MarkNotificationAsSeenForBuilding(buildingType);
		RefreshNotificationForBuilding(buildingType);
	}

	private int IndexOfMercInVisitingMercsList(int mercId, int[] visitingMercenaries)
	{
		if (visitingMercenaries != null)
		{
			for (int result = 0; result < visitingMercenaries.Length; result++)
			{
				if (visitingMercenaries[result] == mercId)
				{
					return result;
				}
			}
		}
		return -1;
	}

	private MercenaryVillageVisitorDataModel CreateVisitorDataModel(int mercId)
	{
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(mercId, AttemptToGenerate: true);
		if (merc == null)
		{
			return null;
		}
		MercenaryVillageVisitorDataModel model = new MercenaryVillageVisitorDataModel
		{
			MercenaryId = mercId,
			NewlyArrived = m_mercsWhoseArrivalHasNotBeenShown.Contains(mercId)
		};
		CardDbfRecord record = merc.GetCardRecord();
		model.MercenaryCard = new CardDataModel
		{
			CardId = record.NoteMiniGuid,
			Premium = TAG_PREMIUM.NORMAL,
			FlavorText = record?.FlavorText
		};
		MercenaryVisitorDbfRecord visitorRecord = LettuceVillageDataUtil.GetVisitorDbfRecordByMercenaryId(mercId);
		if (visitorRecord != null)
		{
			EventTimingType eventType = visitorRecord.Event;
			if (eventType != EventTimingType.SPECIAL_EVENT_ALWAYS && eventType != EventTimingType.UNKNOWN && eventType != EventTimingType.SPECIAL_EVENT_NEVER && eventType != 0)
			{
				model.IsTimedEvent = true;
			}
		}
		return model;
	}

	private void OnVillageDataUpdated()
	{
		if (VillageIsReady)
		{
			m_villageUpdateReceived = true;
			if (!m_popupManager.IsOpen(LettuceVillagePopupManager.PopupType.WORKSHOP))
			{
				UpdateBuildingStates();
			}
			UpdatePackOpeningButton();
		}
	}

	private bool RemoveOldVisitorsFromArrivalData(int[] visitingMercenaries)
	{
		int count = s_mercsWhoseArrivalHasBeenShown.Count;
		s_mercsWhoseArrivalHasBeenShown.RemoveWhere((long id) => !visitingMercenaries.Contains((int)id));
		return count != s_mercsWhoseArrivalHasBeenShown.Count;
	}

	private void AddNewVisitorsToArrivalData(int[] visitingMercenaries)
	{
		foreach (int mercId in visitingMercenaries)
		{
			if (!s_mercsWhoseArrivalHasBeenShown.Contains(mercId))
			{
				m_mercsWhoseArrivalHasNotBeenShown.Add(mercId);
			}
		}
	}

	private void SaveVisitorArrivalData()
	{
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARY_VISITORS_WHOSE_ARRIVAL_HAS_BEEN_SHOWN, s_mercsWhoseArrivalHasBeenShown.ToArray()));
	}

	private void UpdateVisitorArrivalData(int[] visitingMercenaries)
	{
		if (s_mercsWhoseArrivalHasBeenShown == null)
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARY_VISITORS_WHOSE_ARRIVAL_HAS_BEEN_SHOWN, out List<long> previousVisitorList);
			s_mercsWhoseArrivalHasBeenShown = new HashSet<long>();
			if (previousVisitorList != null)
			{
				foreach (long id in previousVisitorList)
				{
					s_mercsWhoseArrivalHasBeenShown.Add(id);
				}
			}
		}
		if (RemoveOldVisitorsFromArrivalData(visitingMercenaries))
		{
			SaveVisitorArrivalData();
		}
		AddNewVisitorsToArrivalData(visitingMercenaries);
	}

	private void RemoveWaitingVisitorsThatHaveDeparted(List<MercenaryVillageVisitorDataModel> waitingList, int[] visitingMercenaries, ref ulong maskOfActiveVisitors)
	{
		for (int visitorIndex = waitingList.Count - 1; visitorIndex >= 0; visitorIndex--)
		{
			int idxOfVisitorInVisitorList = IndexOfMercInVisitingMercsList(waitingList[visitorIndex].MercenaryId, visitingMercenaries);
			if (-1 == idxOfVisitorInVisitorList)
			{
				waitingList.RemoveAt(visitorIndex);
			}
			else
			{
				maskOfActiveVisitors |= (ulong)(1L << idxOfVisitorInVisitorList);
			}
		}
	}

	private void OnVisitorDataUpdated()
	{
		if (!VillageIsReady)
		{
			return;
		}
		int[] visitingMercenaries = LettuceVillageDataUtil.VisitingMercenaries;
		UpdateVisitorArrivalData(visitingMercenaries);
		ulong mapOfActiveVisitorsInList = 0uL;
		RemoveWaitingVisitorsThatHaveDeparted(m_existingVisitorsWaitingToEnter, visitingMercenaries, ref mapOfActiveVisitorsInList);
		RemoveWaitingVisitorsThatHaveDeparted(m_newVisitorsWaitingToEnter, visitingMercenaries, ref mapOfActiveVisitorsInList);
		m_idsToRemove.Clear();
		foreach (KeyValuePair<int, VisualController> pair in m_visibleVisitors)
		{
			int mercIndex = IndexOfMercInVisitingMercsList(pair.Key, visitingMercenaries);
			if (-1 == mercIndex)
			{
				m_idsToRemove.Add(pair.Key);
			}
			else
			{
				mapOfActiveVisitorsInList |= (ulong)(1L << mercIndex);
			}
		}
		foreach (int id in m_idsToRemove)
		{
			VisualController visualController = m_visibleVisitors[id];
			m_visibleVisitors.Remove(id);
			visualController.SetState("LEAVE");
		}
		if (visitingMercenaries != null)
		{
			int[] array = visitingMercenaries;
			foreach (int merc in array)
			{
				if ((mapOfActiveVisitorsInList & 1) == 0L)
				{
					MercenaryVillageVisitorDataModel visitorModel = CreateVisitorDataModel(merc);
					if (visitorModel != null)
					{
						if (visitorModel.NewlyArrived)
						{
							m_newVisitorsWaitingToEnter.Add(visitorModel);
						}
						else
						{
							m_existingVisitorsWaitingToEnter.Add(visitorModel);
						}
					}
				}
				mapOfActiveVisitorsInList >>= 1;
			}
		}
		ProcessWaitingVisitors();
		RefreshNotificationForBuilding(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD);
	}

	public static bool TaskboardIsOkayToShowVisitors()
	{
		MercenaryBuildingDbfRecord taskBoardRecord = LettuceVillageDataUtil.GetBuildingRecordByType(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD);
		if (taskBoardRecord == null)
		{
			return false;
		}
		MercenariesBuildingState taskboardState = LettuceVillageDataUtil.GetBuildingStateByID(taskBoardRecord.ID);
		if (taskboardState == null)
		{
			return false;
		}
		if (!LettuceVillageDataUtil.BuildingIsBuilt(taskboardState))
		{
			return false;
		}
		return true;
	}

	private void ProcessWaitingVisitors()
	{
		if (TaskboardIsOkayToShowVisitors())
		{
			ProcessVisitorsThatDontRequireAnimation();
			if (!m_runningNewlyArrivedVisitorsCoRoutine && m_newVisitorsWaitingToEnter.Count != 0 && m_unusedVisitors.Count != 0)
			{
				m_runningNewlyArrivedVisitorsCoRoutine = true;
				StartCoroutine(ProcessNewlyArrivedVisitorsCoRoutine());
			}
		}
	}

	private bool ShowVisitor(MercenaryVillageVisitorDataModel visitor)
	{
		string arriveEvent = ((!visitor.NewlyArrived) ? "SHOW_IMMEDIATELY" : "ARRIVE");
		int tempIndex = UnityEngine.Random.Range(0, m_unusedVisitors.Count - 1);
		VisualController controller = m_unusedVisitors[tempIndex];
		m_unusedVisitors.RemoveAt(tempIndex);
		m_visibleVisitors.Add(visitor.MercenaryId, controller);
		controller.BindDataModel(visitor);
		controller.SetState(arriveEvent);
		return false;
	}

	private void ProcessVisitorsThatDontRequireAnimation()
	{
		int visitorIndex = 0;
		while (visitorIndex < m_existingVisitorsWaitingToEnter.Count && m_unusedVisitors.Count > 0)
		{
			MercenaryVillageVisitorDataModel visitor = m_existingVisitorsWaitingToEnter[visitorIndex];
			if (!visitor.NewlyArrived)
			{
				m_existingVisitorsWaitingToEnter.RemoveAt(visitorIndex);
				ShowVisitor(visitor);
			}
			else
			{
				visitorIndex++;
			}
		}
	}

	private IEnumerator ProcessNewlyArrivedVisitorsCoRoutine()
	{
		if (m_timeToShowNextNewlyArrivedVisitor == 0f)
		{
			m_timeToShowNextNewlyArrivedVisitor = Time.time + m_firstNewVisitorDelayInSeconds;
		}
		while (m_newVisitorsWaitingToEnter.Count > 0 && m_unusedVisitors.Count > 0)
		{
			if (m_timeToShowNextNewlyArrivedVisitor > Time.time)
			{
				yield return new WaitForSeconds(m_timeToShowNextNewlyArrivedVisitor - Time.time);
			}
			if (m_newVisitorsWaitingToEnter.Count == 0)
			{
				break;
			}
			MercenaryVillageVisitorDataModel visitor = m_newVisitorsWaitingToEnter[0];
			m_newVisitorsWaitingToEnter.RemoveAt(0);
			m_mercsWhoseArrivalHasNotBeenShown.Remove(visitor.MercenaryId);
			s_mercsWhoseArrivalHasBeenShown.Add(visitor.MercenaryId);
			ShowVisitor(visitor);
			m_timeToShowNextNewlyArrivedVisitor = Time.time + m_nextNewVisitorDelayInSeconds;
		}
		SaveVisitorArrivalData();
		m_runningNewlyArrivedVisitorsCoRoutine = false;
	}
}
