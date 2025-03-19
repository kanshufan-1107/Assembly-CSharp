using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.InGameMessage.UI;
using Hearthstone.Store;
using Hearthstone.Streaming;
using PegasusUtil;
using UnityEngine;

public class Hub : PegasusScene
{
	private static readonly WaitForSeconds HumanInteractionPollSpan = new WaitForSeconds(0.1f);

	private Notification m_tooltipNotification;

	private bool m_hasCheckedForNewPlayer;

	private bool m_isTutorialPreviewOpen;

	private bool m_hasTutorialTransitionFinished;

	private readonly Vector3 TooltipPopupScalePC = new Vector3(15f, 15f, 15f);

	private readonly Vector3 TooltipPopupScalePhone = new Vector3(25f, 25f, 25f);

	private const float PracticeModePopupOffsetX = 33.62785f;

	private const float PracticeModePopupOffsetXPhoneUI = 30.46f;

	private const float NewTwistHearthstonePopupOffsetX = 33.62785f;

	private const float NewTwistHearthstonePopupOffsetXPhoneUI = 30.46f;

	private Notification m_innkeeperPopup;

	private void Start()
	{
		IJobDependency[] loadBoxDependencies = HearthstoneJobs.BuildDependencies(typeof(SceneMgr), typeof(IAssetLoader), typeof(NetCache), typeof(EventTimingManager), typeof(DemoMgr), typeof(AchieveManager), typeof(HealthyGamingMgr), typeof(TavernBrawlManager), typeof(GameMgr), typeof(ShownUIMgr), typeof(MusicManager), typeof(SoundManager), typeof(SetRotationManager), typeof(PopupDisplayManager));
		Processor.QueueJob("Hub.Initialize", Job_Initialize(), loadBoxDependencies);
	}

	private IEnumerator<IAsyncJobResult> Job_Initialize()
	{
		yield return new WaitForLogin();
		VerifyPrequisitesInitialized();
		if (Network.ShouldBeConnectedToAurora())
		{
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.HUB);
		}
		else
		{
			Error.AddDevWarning("Alert", "There is no connection to Battle.net, please restart Hearthstone to log in.");
		}
		RegisterEventListeners();
		Box box = Box.Get();
		if (box != null)
		{
			ShowHubStartNotificationsWhenReadyToPlay();
			if (SceneMgr.Get() != null && SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LOGIN)
			{
				box.PlayBoxMusic();
			}
			if (!Network.ShouldBeConnectedToAurora())
			{
				box.DisableAllButtons();
			}
		}
		SceneMgr.Get()?.NotifySceneLoaded();
	}

	private IEnumerator<IAsyncJobResult> Job_WaitForReadyToPlay()
	{
		yield return new WaitForGameDownloadManagerAvailable();
		Box box = Box.Get();
		NetCache.Get().IsNetObjectAvailable<NetCache.NetCacheProfileProgress>();
		do
		{
			yield return null;
		}
		while (!NetCache.Get().IsNetObjectAvailable<NetCache.NetCacheProfileProgress>());
		if (!GameUtils.IsAnyTutorialComplete())
		{
			Log.Box.PrintDebug("Tutorial isn't complete. Waiting until tutorial is finished.");
			bool finishedBoxTransition = false;
			box.OnTutorialTransitionFinished += delegate
			{
				finishedBoxTransition = true;
			};
			Spell closeSpell = box.GetEventSpell(BoxEventType.DOORS_CLOSE);
			while (!GameUtils.IsAnyTutorialComplete() && !closeSpell.IsFinished() && finishedBoxTransition)
			{
				yield return null;
			}
		}
		while (!GameDownloadManagerProvider.Get().IsReadyToPlay || InitialDownloadDialog.IsVisible || SceneMgr.Get().GetMode() != SceneMgr.Mode.HUB)
		{
			yield return null;
			if (!GameDownloadManagerProvider.Get().IsReadyToPlay && !InitialDownloadDialog.IsVisible)
			{
				Log.Box.PrintDebug("Initial download isn't finished, but initial download pop up isn't visible. Showing it.");
				float timeRemaining = 1.75f;
				while (0f < timeRemaining)
				{
					yield return null;
					timeRemaining -= Time.deltaTime;
				}
				if (!GameDownloadManagerProvider.Get().IsReadyToPlay && !InitialDownloadDialog.IsVisible && GameDownloadManagerProvider.Get().IsDownloading)
				{
					DialogManager.Get().AllowShowInitialDownloadPopup();
				}
			}
		}
		while (RankMgr.Get().DidSkipApprenticeThisSession && RankMgr.Get().IsWaitingForApprenticeComplete)
		{
			yield return null;
		}
		Log.Box.PrintDebug("Initial download finished. Transitioning box to main hub.");
		box.ChangeState(Box.State.HUB_WITH_DRAWER);
		box.UpdateUI();
		RewardData.LoadPendingRewards();
		while (!box.ShouldBeShowingState(Box.State.HUB_WITH_DRAWER))
		{
			yield return null;
		}
		if (!(this == null))
		{
			PreloadShopUI();
			Log.Box.PrintInfo("Box transition finished and ready to play. Showing notifications.");
			ShowHubStartNotifications();
		}
	}

	private void ShowHubStartNotificationsWhenReadyToPlay()
	{
		if (GameDownloadManagerProvider.Get().IsReadyToPlay && Box.Get().ShouldBeShowingState(Box.State.HUB_WITH_DRAWER))
		{
			Log.Box.PrintInfo("Box transition finished and ready to play. Showing notifications.");
			PreloadShopUI();
			ShowHubStartNotifications();
		}
		else
		{
			Log.Box.PrintInfo("Box transition finished but not ready to play. Waiting until we should show notifications.");
			Processor.QueueJobIfNotExist("Hub.WaitForReadyToPlay", Job_WaitForReadyToPlay());
		}
	}

	private void OnTutorialPreviewOpened()
	{
		NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_innkeeperPopup);
	}

	private void OnTutorialPreviewClosed()
	{
	}

	private void OnMessageModalShopButtonPressed()
	{
		HideTooltipNotification(animate: false);
	}

	private void OnDestroy()
	{
		UnregisterEventListeners();
	}

	private void VerifyPrequisitesInitialized()
	{
		if (CollectionManager.Get() == null)
		{
			Debug.LogError("Hub.Start Error - CollectionManager is null");
		}
		if (PresenceMgr.Get() == null)
		{
			Debug.LogError("Hub.Start Error - PresenceMgr is null");
		}
		if (Box.Get() == null)
		{
			Debug.LogError("Hub.Start Error - Box is null");
		}
		if (Options.Get() == null)
		{
			Debug.LogError("Hub.Start Error - Options is null");
		}
		if (NotificationManager.Get() == null)
		{
			Debug.LogError("Hub.Start Error - NotificationManager is null");
		}
		if (StoreManager.Get() == null)
		{
			Debug.LogError("Hub.Start Error - StoreManager is null");
		}
	}

	private void RegisterEventListeners()
	{
		Box box = Box.Get();
		if (box != null)
		{
			box.AddButtonPressListener(OnBoxButtonPressed);
			box.AddTransitionFinishedListener(OnBoxTransitionFinished);
			if (box.m_QuestLogButton == null)
			{
				Debug.LogError("Hub.Start Error - QuestLogButton is null");
			}
			else
			{
				box.m_QuestLogButton.AddEventListener(UIEventType.RELEASE, OnButtonReleasedHideTooltipNotification);
			}
			if (box.m_journalButtonWidget == null)
			{
				Debug.LogError("Hub.Start Error - JournalButton is null");
			}
			else
			{
				box.m_journalButtonWidget.RegisterEventListener(OnButtonClickedHandleJournalWidgetTooltipNotification);
			}
			if (box.m_StoreButton == null)
			{
				Debug.LogError("Hub.Start Error - StoreButton is null");
			}
			else
			{
				box.m_StoreButton.AddEventListener(UIEventType.RELEASE, OnButtonReleasedHideTooltipNotification);
			}
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				if (box.m_ribbonButtons == null)
				{
					Debug.LogError("Hub.Start Error - RibbonButtons is null");
				}
				else
				{
					if (box.m_ribbonButtons.m_questLogRibbon == null)
					{
						Debug.LogError("Hub.Start Error - QuestLogRibbon is null");
					}
					else
					{
						box.m_ribbonButtons.m_questLogRibbon.AddEventListener(UIEventType.RELEASE, OnButtonReleasedHideTooltipNotification);
					}
					if (box.m_ribbonButtons.m_storeRibbon == null)
					{
						Debug.LogError("Hub.Start Error - StoreRibbon is null");
					}
					else
					{
						box.m_ribbonButtons.m_storeRibbon.AddEventListener(UIEventType.RELEASE, OnButtonReleasedHideTooltipNotification);
					}
					if (box.m_ribbonButtons.m_journalButtonWidget == null)
					{
						Debug.LogError("Hub.Start Error - JournalRibbon is null");
					}
					else
					{
						box.m_ribbonButtons.m_journalButtonWidget.RegisterEventListener(OnButtonClickedHandleJournalWidgetTooltipNotification);
					}
				}
			}
		}
		else
		{
			Debug.LogError("Hub.Start Error - box is null");
		}
		if (StoreManager.IsInitialized())
		{
			StoreManager.Get().RegisterSuccessfulPurchaseListener(OnAdventureBundlePurchase);
		}
		else
		{
			Debug.LogError("Hub.Start Error - RegisterSuccessfulPurchaseListener not assigned");
		}
		EventTimingType eventType = EventTimingType.IGNORE;
		EventTimingManager eventTimingManager = EventTimingManager.Get();
		if (eventTimingManager != null)
		{
			eventType = eventTimingManager.GetActiveEventType();
		}
		else
		{
			Debug.LogError("Hub.Start Error - EventTimingManager was null and eventType was not received");
		}
		if (eventType != 0 && GameModeUtils.HasUnlockedMode(Global.UnlockableGameMode.ARENA))
		{
			EventTimingVisualMgr visualMgr = eventTimingManager?.Visuals;
			if (visualMgr != null)
			{
				visualMgr.LoadEvent(eventType);
			}
			if (SceneMgr.IsInitialized())
			{
				SceneMgr.Get().RegisterSceneUnloadedEvent(OnSceneUnloaded);
			}
			else
			{
				Debug.LogError("Hub.Start Error - SceneMgr did not register scene unload event");
			}
		}
		TavernBrawlManager tavernBrawlManager = TavernBrawlManager.Get();
		if (tavernBrawlManager != null)
		{
			tavernBrawlManager.OnSessionLimitRaised += MaybeDoTavernBrawlLimitRaisedAlert;
		}
		else
		{
			Debug.LogError("Hub.Start Error - TavernBrawlManager did not register certain events");
		}
		TutorialPreviewController.PreviewOpened += OnTutorialPreviewOpened;
		TutorialPreviewController.PreviewClosed += OnTutorialPreviewClosed;
		MessageModal.ShopButtonPressed += OnMessageModalShopButtonPressed;
		HearthstoneApplication.Get().Resetting += ShowHubStartNotificationsWhenReadyToPlay;
	}

	private void UnregisterEventListeners()
	{
		if (ServiceManager.TryGet<TavernBrawlManager>(out var tavernBrawlManager))
		{
			tavernBrawlManager.OnSessionLimitRaised -= MaybeDoTavernBrawlLimitRaisedAlert;
		}
		TutorialPreviewController.PreviewOpened -= OnTutorialPreviewOpened;
		TutorialPreviewController.PreviewClosed -= OnTutorialPreviewClosed;
		MessageModal.ShopButtonPressed -= OnMessageModalShopButtonPressed;
		if (HearthstoneApplication.Get() != null)
		{
			HearthstoneApplication.Get().Resetting -= ShowHubStartNotificationsWhenReadyToPlay;
		}
		Box box = Box.Get();
		if (!(box != null))
		{
			return;
		}
		box.RemoveButtonPressListener(OnBoxButtonPressed);
		box.RemoveTransitionFinishedListener(OnBoxTransitionFinished);
		if (box.m_QuestLogButton != null)
		{
			box.m_QuestLogButton.RemoveEventListener(UIEventType.RELEASE, OnButtonReleasedHideTooltipNotification);
		}
		if (box.m_journalButtonWidget != null)
		{
			box.m_journalButtonWidget.RemoveEventListener(OnButtonClickedHandleJournalWidgetTooltipNotification);
		}
		if (box.m_StoreButton != null)
		{
			box.m_StoreButton.RemoveEventListener(UIEventType.RELEASE, OnButtonReleasedHideTooltipNotification);
		}
		if ((bool)UniversalInputManager.UsePhoneUI && box.m_ribbonButtons != null)
		{
			if (box.m_ribbonButtons.m_questLogRibbon != null)
			{
				box.m_ribbonButtons.m_questLogRibbon.RemoveEventListener(UIEventType.RELEASE, OnButtonReleasedHideTooltipNotification);
			}
			if (box.m_ribbonButtons.m_storeRibbon != null)
			{
				box.m_ribbonButtons.m_storeRibbon.RemoveEventListener(UIEventType.RELEASE, OnButtonReleasedHideTooltipNotification);
			}
			if (box.m_ribbonButtons.m_journalButtonWidget != null)
			{
				box.m_ribbonButtons.m_journalButtonWidget.RemoveEventListener(OnButtonClickedHandleJournalWidgetTooltipNotification);
			}
		}
	}

	private void OnButtonClickedHandleJournalWidgetTooltipNotification(string eventName)
	{
		if (eventName == "JOURNAL_OPENED")
		{
			HideTooltipNotification(animate: false);
		}
	}

	private void ShowNewTwistHearthstoneNotification(string message)
	{
		HideTooltipNotification(animate: false);
		Vector3 popupPosition = Box.Get().GetHearthstoneButtonPosition();
		Vector3 popupScale;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			popupPosition.x -= 30.46f;
			popupScale = TooltipPopupScalePhone;
		}
		else
		{
			popupPosition.x -= 33.62785f;
			popupScale = TooltipPopupScalePC;
		}
		m_tooltipNotification = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popupPosition, popupScale, GameStrings.Get(message));
		m_tooltipNotification.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
	}

	private void ShowModesButtonNotification(string message)
	{
		HideTooltipNotification(animate: false);
		Vector3 popupPos = Box.Get().GetModesButtonPosition();
		NotificationManager notificationManager = NotificationManager.Get();
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			popupPos.x -= 30.46f;
			m_tooltipNotification = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, popupPos, TooltipPopupScalePhone, GameStrings.Get(message));
		}
		else
		{
			popupPos.x -= 33.62785f;
			m_tooltipNotification = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, popupPos, TooltipPopupScalePC, GameStrings.Get(message));
		}
		m_tooltipNotification.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
	}

	private void PreloadShopUI()
	{
		Log.Box.PrintDebug("Preloading Shop UI.");
		Box.Get().ActivateShopUIIfReady();
		BnetBar.Get().RefreshCurrency();
	}

	private void ShowHubStartNotifications()
	{
		Log.Box.PrintInfo("Box.ShowHubStartNotifications()");
		PopupDisplayManager.Get().ReadyToShowPopups();
		if (Network.ShouldBeConnectedToAurora())
		{
			if (!Options.Get().GetBool(Option.HAS_SEEN_HUB, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("Hub.Start:" + Option.HAS_SEEN_HUB))
			{
				StartCoroutine(DoFirstTimeHubWelcome());
			}
			else if (CollectionManager.Get().ShouldSeeTwistModeNotification() && UserAttentionManager.CanShowAttentionGrabber("Hub.Start:" + GameSaveKeySubkeyId.HAS_SEEN_TWIST_MODE_NOTIFICATION))
			{
				ShowNewTwistHearthstoneNotification("GLUE_NEW_TWIST_MODE_HINT");
			}
			else if (!Options.Get().GetBool(Option.HAS_SEEN_100g_REMINDER, defaultVal: false))
			{
				NetCache.NetCacheGoldBalance netObject = NetCache.Get().GetNetObject<NetCache.NetCacheGoldBalance>();
				if (netObject == null)
				{
					Debug.LogError("Hub.Start Error - NetCache.NetCacheGoldBalance is null");
				}
				if (netObject.GetTotal() >= 100 && UserAttentionManager.CanShowAttentionGrabber("Hub.Start:" + Option.HAS_SEEN_100g_REMINDER))
				{
					NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_FIRST_100_GOLD"), "VO_INNKEEPER_FIRST_100_GOLD.prefab:c6a50337099a454488acd96d2f37320f");
					Options.Get().SetBool(Option.HAS_SEEN_100g_REMINDER, val: true);
				}
			}
			if (!Options.Get().GetBool(Option.HAS_SEEN_BG_DOWNLOAD_FINISHED_POPUP) && !GameUtils.HasCompletedApprentice())
			{
				Box.Get().ShowBGDownloadCompletePopupIfNecessary();
			}
		}
		RewardData.LoadPendingRewards();
	}

	private void MaybeDoTavernBrawlLimitRaisedAlert(int lastSeenLimit, int newLimit)
	{
		int numNowAvailableToPlayer = TavernBrawlManager.Get().NumSessionsAvailableForPurchase;
		if (numNowAvailableToPlayer == newLimit - lastSeenLimit)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info.m_headerText = GameStrings.Get("GLUE_HEROIC_BRAWL_SESSION_LIMIT_ALERT_TITLE");
			info.m_text = GameStrings.Format("GLUE_HEROIC_BRAWL_SESSION_LIMIT_ALERT_LIMIT_RAISED", numNowAvailableToPlayer);
			DialogManager.Get().ShowPopup(info);
		}
	}

	private void Update()
	{
		Network.Get().ProcessNetwork();
	}

	public override void Unload()
	{
		StoreManager.Get().RemoveSuccessfulPurchaseListener(OnAdventureBundlePurchase);
		HideTooltipNotification(animate: true);
		Box box = Box.Get();
		if (box != null)
		{
			box.m_QuestLogButton.RemoveEventListener(UIEventType.RELEASE, OnButtonReleasedHideTooltipNotification);
			box.m_StoreButton.RemoveEventListener(UIEventType.RELEASE, OnButtonReleasedHideTooltipNotification);
			box.RemoveButtonPressListener(OnBoxButtonPressed);
			box.Unload();
		}
	}

	private void OnSceneUnloaded(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		EventTimingType eventType = EventTimingManager.Get().GetActiveEventType();
		if (eventType != 0)
		{
			EventTimingVisualMgr visualMgr = EventTimingManager.Get().Visuals;
			if (visualMgr != null)
			{
				visualMgr.UnloadEvent(eventType);
			}
		}
		SceneMgr.Get().UnregisterSceneUnloadedEvent(OnSceneUnloaded);
	}

	private void OnBoxButtonPressed(Box.ButtonType buttonType, bool isShowingTutorialVideo, object userData)
	{
		switch (buttonType)
		{
		case Box.ButtonType.TRADITIONAL:
			if (isShowingTutorialVideo)
			{
				HideTooltipNotification(animate: true);
				break;
			}
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.TOURNAMENT);
			Tournament.Get().NotifyOfBoxTransitionStart();
			break;
		case Box.ButtonType.GAME_MODES:
			HandleGameModesButtonPressed();
			break;
		case Box.ButtonType.BACON:
			if (isShowingTutorialVideo)
			{
				HideTooltipNotification(animate: true);
			}
			else
			{
				HandleBaconButtonPressed();
			}
			break;
		case Box.ButtonType.COLLECTION:
			CollectionManager.Get().NotifyOfBoxTransitionStart();
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.COLLECTIONMANAGER);
			break;
		case Box.ButtonType.OPEN_PACKS:
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.PACKOPENING);
			break;
		case Box.ButtonType.SET_ROTATION:
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.TOURNAMENT);
			Tournament.Get().NotifyOfBoxTransitionStart();
			break;
		}
	}

	private void HandleGameModesButtonPressed()
	{
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.GAME_MODE);
	}

	private void HandleBaconButtonPressed()
	{
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.BACON);
	}

	private void OnBoxTransitionFinished(object userData)
	{
		ShowHubStartNotificationsWhenReadyToPlay();
	}

	private IEnumerator DoFirstTimeHubWelcome()
	{
		Box theBox = Box.Get();
		while (theBox == null || theBox.IsBusy() || theBox.m_GameModesButton == null || !theBox.m_GameModesButton.IsEnabled() || theBox.ShouldBeShowingState(Box.State.HUB_WITH_DRAWER) || theBox.GetBoxCamera() == null || theBox.GetBoxCamera().GetState() != BoxCamera.State.CLOSED_WITH_DRAWER)
		{
			yield return HumanInteractionPollSpan;
			theBox = Box.Get();
		}
		while (!GameUtils.IsTraditionalTutorialComplete())
		{
			yield return HumanInteractionPollSpan;
		}
		StoreManager storeManager = StoreManager.Get();
		QuestLog questLog = QuestLog.Get();
		while ((storeManager != null && storeManager.IsShown()) || (questLog != null && questLog.IsShown()))
		{
			yield return HumanInteractionPollSpan;
			storeManager = StoreManager.Get();
			questLog = QuestLog.Get();
		}
		AchieveManager achieveManager = AchieveManager.Get();
		while ((achieveManager != null && achieveManager.HasQuestsToShow(onlyNewlyActive: true)) || WelcomeQuests.Get() != null)
		{
			yield return HumanInteractionPollSpan;
			achieveManager = AchieveManager.Get();
		}
		yield return StartCoroutine(PopupDisplayManager.Get().WaitForAllPopups());
		Options.Get().SetBool(Option.HAS_SEEN_HUB, val: true);
		AdTrackingManager adTrackingManager = AdTrackingManager.Get();
		if (adTrackingManager != null)
		{
			adTrackingManager.TrackFirstLogin();
		}
		else
		{
			Debug.LogWarning("AdTrackingManager was not initialized during Hub.DoFirstTimeHubWelcome()");
		}
	}

	private void OnAdventureBundlePurchase(ProductInfo bundle, PaymentMethod purchaseMethod)
	{
		if (bundle == null || bundle.Items == null)
		{
			return;
		}
		foreach (Network.BundleItem item in bundle.Items)
		{
			if (item.ItemType == ProductType.PRODUCT_TYPE_NAXX)
			{
				AdventureConfig.Get().SetSelectedAdventureMode(AdventureDbId.NAXXRAMAS, AdventureModeDbId.LINEAR);
				break;
			}
		}
	}

	private void OnButtonReleasedHideTooltipNotification(UIEvent e)
	{
		HideTooltipNotification(animate: true);
	}

	private void HideTooltipNotification(bool animate)
	{
		if (!(m_tooltipNotification == null))
		{
			if (animate)
			{
				NotificationManager.Get().DestroyNotification(m_tooltipNotification, 0f);
			}
			else
			{
				NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_tooltipNotification);
			}
		}
	}
}
