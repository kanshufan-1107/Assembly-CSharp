using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.GameService.Protocol.V2.Client;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.InGameMessage.UI;
using Hearthstone.Progression;
using Hearthstone.Store;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class BaconDisplay : AbsSceneDisplay
{
	public AsyncReference m_PlayButtonReference;

	public AsyncReference m_PlayButtonPhoneReference;

	public AsyncReference m_BackButtonReference;

	public AsyncReference m_BackButtonPhoneReference;

	public AsyncReference m_StatsButtonReference;

	public AsyncReference m_StatsButtonPhoneReference;

	public AsyncReference m_StatsPageReference;

	public AsyncReference m_StatsPagePhoneReference;

	public AsyncReference m_InfoButtonReference;

	public AsyncReference m_InfoButtonPhoneReference;

	public AsyncReference m_luckyDrawButtonReference;

	public AsyncReference m_duosToggleButtonReference;

	public AsyncReference m_LobbyReference;

	public AsyncReference m_PartyReference;

	public AsyncReference m_LobbyPhoneReference;

	public AsyncReference m_PartyPhoneReference;

	public Transform m_OffScreenBonePC;

	public Transform m_OnScreenBonePC;

	public Transform m_OffScreenBoneMobile;

	public Transform m_OnScreenBoneMobile;

	private bool m_playButtonFinishedLoading;

	private bool m_backButtonFinishedLoading;

	private bool m_statsButtonFinishedLoading;

	private bool m_luckyDrawButtonFinishedLoading;

	private bool m_lobbyFinishedLoading;

	private bool m_shopProductPageOpened;

	private WidgetTemplate m_OwningWidget;

	private Widget m_lobbyWidget;

	private Widget m_toggleWidget;

	private Dictionary<int, Widget> m_duosSlotWidgets = new Dictionary<int, Widget>();

	private PlayButton m_playButton;

	private UIBButton m_statsButton;

	private Clickable m_statsButtonClickable;

	private LuckyDrawButton m_luckyDrawButton;

	private RewardPresenter m_rewardPresenter = new RewardPresenter();

	private const float LUCKY_DRAW_BUTTON_FANFARE_FX_DELAY_SEC = 0.5f;

	private const string LUCKY_DRAW_NEW_HAMMER_FX = "NewHammerFX";

	private const string LUCKY_DRAW_NEW_HAMMER_ANIM = "LuckyDrawNewHammer_Anim";

	private Notification m_luckyDrawFTUENotification;

	private Coroutine m_showLuckyDrawEndsCoroutine;

	private const float LUCKY_DRAW_FTUE_POPUP_DELAY_SEC = 4f;

	private const float LUCKY_DRAW_POPUP_DELAY_SEC = 3f;

	private const float LUCKY_DRAW_POPUP_IN_BETWEEN_DELAY_SEC = 1f;

	private const string FTUE_TOOLTIP_BONE = "FTUETooltip";

	private const string TOGGLE_BUTTON_STATE_DUOS = "DUOS";

	private const string TOGGLE_BUTTON_STATE_SOLO = "REGULAR";

	private const string TOGGLE_BUTTON_STATE_DUOS_DISABLED = "DISABLED_DUOS";

	private const string TOGGLE_BUTTON_STATE_SOLO_DISABLED = "DISABLED_REGULAR";

	private const string PLAY_MODE_SWITCH_FX = "PLAY_MODE_SWITCH_FX";

	private BaconParty m_baconParty;

	private BaconLobbyTutorial m_baconLobbyTutorial;

	private static BaconDisplay m_instance;

	private readonly PlatformDependentValue<string> PLATFORM_DEPENDENT_BONE_SUFFIX = new PlatformDependentValue<string>(PlatformCategory.Screen)
	{
		PC = "PC",
		Tablet = "PC",
		Phone = "Mobile"
	};

	private LuckyDrawManager m_luckyDrawManager;

	private LuckyDrawButtonDataModel m_luckyDrawButtonDataModel;

	private BaconStatsFrameDataModel m_statFrameDataModel;

	private readonly PlatformDependentValue<bool> ShowLowMemoryWarning = new PlatformDependentValue<bool>(PlatformCategory.Memory)
	{
		LowMemory = true,
		MediumMemory = true,
		HighMemory = false
	};

	private const int MIN_DAYS_LEFT_FOR_LUCKY_DRAW_END_SOON_POPUP = 1;

	private const int MAX_DAYS_LEFT_FOR_LUCKY_DRAW_END_SOON_POPUP = 3;

	private const int PAST_GAMES_TO_SHOW = 5;

	private const int MINIONS_PER_BOARD = 7;

	private const string SET_MODE_DUOS_EVENT = "SetModeDuos";

	private const string SET_MODE_SOLO_EVENT = "SetModeSolo";

	private const string OPEN_SHOP_EVENT = "OpenShop";

	private const string OPEN_COLLECTION_EVENT = "OpenCollection";

	private const string STATS_PANEL_SLIDE_COMPLETE = "CODE_STATS_SLIDE_FINISHED";

	private const string STATS_PANEL_PHONE_SLIDE_COMPLETE = "CODE_STATS_PHONE_SLIDE_FINISHED";

	private static bool m_hasSeenLowMemoryWarningThisSession;

	private const string SHOP_OPEN_COMPLETED = "ShopOpenCompleted";

	private const string SHOP_CLOSED = "ShopClosed";

	private void Awake()
	{
		InitSlidingTray();
		m_luckyDrawManager = LuckyDrawManager.Get();
		if (m_luckyDrawManager == null)
		{
			Log.All.PrintError("BaconDisplay.Awake() - LuckyDrawManger is null");
		}
		RegisterListeners();
		m_OwningWidget = GetComponent<WidgetTemplate>();
		m_instance = this;
	}

	public override void Start()
	{
		base.Start();
		m_luckyDrawManager.InitializeOrUpdateData();
		m_luckyDrawButtonDataModel = m_luckyDrawManager.GetLuckyDrawButtonDataModel();
		m_luckyDrawButtonReference.RegisterReadyListener<WidgetInstance>(OnLuckyDrawButtonReady);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_PlayButtonPhoneReference.RegisterReadyListener<VisualController>(OnPlayButtonReady);
			m_BackButtonPhoneReference.RegisterReadyListener<VisualController>(OnBackButtonReady);
			m_StatsButtonPhoneReference.RegisterReadyListener<VisualController>(OnStatsButtonReady);
			m_StatsPagePhoneReference.RegisterReadyListener<VisualController>(OnStatsPagePhoneReady);
			m_LobbyPhoneReference.RegisterReadyListener<Widget>(OnLobbyReady);
			m_PartyPhoneReference.RegisterReadyListener<Widget>(OnPartyReady);
			m_InfoButtonPhoneReference.RegisterReadyListener<Widget>(OnInfoButtonReady);
		}
		else
		{
			m_PlayButtonReference.RegisterReadyListener<VisualController>(OnPlayButtonReady);
			m_BackButtonReference.RegisterReadyListener<VisualController>(OnBackButtonReady);
			m_StatsButtonReference.RegisterReadyListener<VisualController>(OnStatsButtonReady);
			m_StatsPageReference.RegisterReadyListener<VisualController>(OnStatsPagePCReady);
			m_LobbyReference.RegisterReadyListener<Widget>(OnLobbyReady);
			m_PartyReference.RegisterReadyListener<Widget>(OnPartyReady);
			m_InfoButtonReference.RegisterReadyListener<Widget>(OnInfoButtonReady);
		}
		if (m_duosToggleButtonReference != null)
		{
			m_duosToggleButtonReference.RegisterReadyListener<Widget>(OnDuosToggleModeButtonReady);
		}
		NetCache.Get().RegisterScreenBattlegrounds(OnNetCacheReady);
		PartyManager.Get().AddChangedListener(OnPartyChanged);
		InitializeBaconLobbyData();
		NarrativeManager.Get().OnBattlegroundsEntered();
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_Battlegrounds);
		PresenceMgr.Get().SetStatus(Global.PresenceStatus.BATTLEGROUNDS_SCREEN);
		StoreManager.Get().RegisterStatusChangedListener(OnStoreStatusChanged);
		m_baconLobbyTutorial = GetComponent<BaconLobbyTutorial>();
	}

	private void OnDestroy()
	{
		m_instance = null;
		HidePopups();
		UnregisterListeners();
	}

	public static void ClearDuosTutorialFlags()
	{
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_VIEW_TEAMMATE, val: false);
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_VIEW_SELF, val: false);
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_HEALTH, val: false);
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_COMBAT, val: false);
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_HERO_SWAP, val: false);
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_PASS, val: false);
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_RECEIVE, val: false);
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_COMBAT_ORDER, val: false);
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_PING, val: false);
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_PING_WHEEL, val: false);
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_TEAMMATE_PINGED, val: false);
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_BONUS_DAMAGE, val: false);
	}

	public static BaconDisplay Get()
	{
		return m_instance;
	}

	private void BaconDisplayEventListener(string eventName)
	{
		switch (eventName)
		{
		case "OpenShop":
			HidePopups();
			OpenBattlegroundsShop();
			break;
		case "OpenCollection":
			HidePopups();
			OpenBattlegroundsCollection();
			break;
		case "CODE_STATS_SLIDE_FINISHED":
			StatsPanelFinishedLoading();
			break;
		case "CODE_STATS_PHONE_SLIDE_FINISHED":
			StatsPanelPhoneFinishedLoading();
			break;
		case "SetModeDuos":
			if (m_lobbyWidget != null && m_toggleWidget != null)
			{
				m_lobbyWidget.transform.GetChild(0).gameObject.GetComponent<VisualController>().SetState("DUOS");
			}
			break;
		case "SetModeSolo":
			if (m_lobbyWidget != null && m_toggleWidget != null)
			{
				m_lobbyWidget.transform.GetChild(0).gameObject.GetComponent<VisualController>().SetState("REGULAR");
			}
			break;
		case "PLAYER_drag_started":
			m_baconLobbyTutorial.HideArrangePartyPopup();
			break;
		}
	}

	private void OnPartyReady(Widget widget)
	{
		m_baconParty = widget.gameObject.GetComponentInChildren<BaconParty>();
	}

	private void StatsPanelFinishedLoading()
	{
	}

	private void OnLuckyDrawExpired()
	{
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.BACON && m_luckyDrawButton != null)
		{
			m_luckyDrawButton.gameObject.SetActive(value: false);
			StartCoroutine(WaitThenShowLuckDrawEndPopupIfExistsUnusedHammers());
		}
	}

	private IEnumerator WaitThenShowLuckDrawEndPopupIfExistsUnusedHammers()
	{
		while (m_luckyDrawManager.IsDataDirty())
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (m_luckyDrawManager.GetBattlegroundsLuckyDrawDataModel().Hammers > 0)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_BATTLEBASH_ALERT_EVENT_END_TITLE");
			info.m_text = GameStrings.Get("GLUE_BATTLEBASH_ALERT_EVENT_END_DESCRIPTION");
			info.m_iconSet = AlertPopup.PopupInfo.IconSet.Default;
			info.m_showAlertIcon = true;
			info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			info.m_responseCallback = delegate
			{
				SetNextModeAndHandleTransition(SceneMgr.Mode.HUB, SceneMgr.TransitionHandlerType.NEXT_SCENE);
			};
			DialogManager.Get().ShowPopup(info);
		}
	}

	private void StatsPanelPhoneFinishedLoading()
	{
	}

	public void OnDuosSlotButtonReady(Widget widget, int slot, UIEvent.Handler clickHandler)
	{
		if (widget == null)
		{
			Error.AddDevWarning("UI Error!", $"DuosSlot{slot}Button could not be found! You will not be able to switch to slot {slot}!");
			return;
		}
		m_duosSlotWidgets.Add(slot, widget);
		widget.gameObject.GetComponentInChildren<UIBButton>().AddEventListener(UIEventType.RELEASE, clickHandler);
		VisualController[] components = widget.gameObject.GetComponents<VisualController>();
		foreach (VisualController vc in components)
		{
			if (vc.Label == "SLOT")
			{
				vc.SetState($"SLOT{slot}");
			}
		}
		UpdateDuosSelectedSlotBasedOnPartyInfo();
	}

	public void OnDuosToggleModeButtonReady(Widget widget)
	{
		if (widget == null)
		{
			Error.AddDevWarning("UI Error!", "DuosToggleModeButton could not be found! You will not be able to swap between duos and regular!");
			return;
		}
		m_toggleWidget = widget;
		m_toggleWidget.gameObject.GetComponentInChildren<UIBButton>().AddEventListener(UIEventType.RELEASE, ToggleDuosRelease);
		UpdateDuosToggleButtonBasedOnPartyInfo();
		if (!BaconLobbyMgr.Get().IsInDuosMode())
		{
			m_baconLobbyTutorial.StartModeToggleTutorial();
		}
	}

	public void OnInfoButtonReady(Widget widget)
	{
		if (!(widget == null))
		{
			widget.gameObject.GetComponentInChildren<UIBButton>().AddEventListener(UIEventType.RELEASE, InfoButtonRelease);
		}
	}

	public void InfoButtonRelease(UIEvent e)
	{
		BaconLobbyDataModel dataModel = GetBaconLobbyDataModel();
		if (dataModel != null && !BaconLobbyMgr.Get().IsInDuosMode())
		{
			Options.Get().SetBool(Option.BG_NEW_RULES_VIEWED, val: true);
			dataModel.HasNewRules = false;
		}
	}

	private void UpdateInfoButtonNotification()
	{
		BaconLobbyDataModel dataModel = GetBaconLobbyDataModel();
		if (dataModel != null)
		{
			dataModel.HasNewRules = !Options.Get().GetBool(Option.BG_NEW_RULES_VIEWED) && !BaconLobbyMgr.Get().IsInDuosMode();
		}
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
		UpdatePlayButtonBasedOnPartyInfo();
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

	public void OnStatsButtonReady(VisualController buttonVisualController)
	{
		if (buttonVisualController == null)
		{
			Error.AddDevWarning("UI Error!", "StatsButton could not be found! You will not be able to show 'Stats'!");
			return;
		}
		m_statsButton = buttonVisualController.gameObject.GetComponent<UIBButton>();
		m_statsButtonClickable = buttonVisualController.gameObject.GetComponent<Clickable>();
		UpdateStatsButtonState();
		m_statsButtonFinishedLoading = true;
	}

	public void OnLuckyDrawButtonReady(WidgetInstance button)
	{
		if (button == null)
		{
			Error.AddDevWarning("UI Error!", "Lucky Draw Button could not be found! Cant show Lucky Draw button!");
			return;
		}
		m_luckyDrawButton = button.Widget.GetComponent<LuckyDrawButton>();
		WidgetTemplate luckyDrawWidget = m_luckyDrawButton.GetComponent<WidgetTemplate>();
		if (luckyDrawWidget == null)
		{
			Error.AddDevWarning("UI Error!", "Could not find widget component of Lucky Draw button! Cant show lucky Draw button!");
			return;
		}
		luckyDrawWidget.RegisterEventListener(delegate(string eventName)
		{
			LuckyDrawManager.Get().GetBattlegroundsLuckyDrawDataModel();
			if (eventName == "BUTTON_CLICKED")
			{
				LuckyDrawButtonClicked();
				HideLuckyDrawEndsSoonTooltip();
			}
			if (LuckyDrawManager.Get().IsBattleBashEndingSoon())
			{
				if (eventName == "LUCKY_DRAW_MOUSE_OVER")
				{
					float delay = (UniversalInputManager.UsePhoneUI ? 1f : 0f);
					ShowLuckyDrawEndsSoonTooltip(delay);
				}
				else if (eventName == "LUCKY_DRAW_MOUSE_OUT")
				{
					HideLuckyDrawEndsSoonTooltip();
				}
			}
		});
		m_luckyDrawButtonFinishedLoading = true;
		StartCoroutine("WaitThenShowLuckyDrawPopups");
	}

	private IEnumerator WaitThenShowLuckyDrawPopups()
	{
		while (m_luckyDrawManager.IsDataDirty())
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (!m_luckyDrawManager.HasActiveLuckyDrawBox() || !LuckyDrawManager.Get().GetLuckyDrawButtonDataModel().LuckyDrawEnabled)
		{
			yield break;
		}
		NarrativeManager.Get().OnBattlegroundsLuckyDrawButtonShown();
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_BATTLE_BASH_BUTTON_TOOLTIP, out long hasSeenBattleBashFTUE);
		if (hasSeenBattleBashFTUE <= 0)
		{
			yield return new WaitForSeconds(4f);
			m_luckyDrawManager.SetShowHighlight(show: true);
			ShowLuckyDrawFTUENotification();
			yield break;
		}
		bool showUnackHammerPopup = ShouldShowBattlegroundsUnacknowledgedEarnedHammersPopUp() && m_rewardPresenter != null;
		if (showUnackHammerPopup)
		{
			yield return new WaitForSeconds(3f);
			while (m_rewardPresenter.IsShowingReward() || PopupDisplayManager.Get().IsShowing)
			{
				yield return new WaitForSeconds(0.1f);
			}
			m_luckyDrawManager.SetShowHighlight(show: true);
			RewardScrollDataModel dataModel = new RewardScrollDataModel
			{
				DisplayName = GameStrings.Get("GLUE_BACON_REWARD_BATTLE_BASH_HAMMER"),
				Description = GameStrings.Get("GLUE_BACON_TOP_4_REWARD_DESC"),
				RewardList = new RewardListDataModel
				{
					Items = new DataModelList<RewardItemDataModel>
					{
						new RewardItemDataModel
						{
							Quantity = m_luckyDrawManager.NumUnacknowledgedEarnedHammers(),
							ItemType = RewardItemType.BATTLEGROUNDS_BATTLE_BASH_HAMMER
						}
					}
				}
			};
			m_rewardPresenter.EnqueueReward(dataModel, delegate
			{
			});
			m_rewardPresenter.ShowNextReward(OnLuckyDrawUnacknowledgedHammerPopupDismissed);
		}
		if (!ShouldShowLuckyDrawEndsSoonPopup())
		{
			yield break;
		}
		yield return new WaitForSeconds(showUnackHammerPopup ? 1f : 3f);
		while (m_rewardPresenter.IsShowingReward() || PopupDisplayManager.Get().IsShowing)
		{
			yield return new WaitForSeconds(0.1f);
		}
		DialogManager dialogManager = DialogManager.Get();
		if (!(dialogManager == null))
		{
			dialogManager.ShowBattlegroundsLuckyDrawEndSoonPopup(m_luckyDrawManager.GetBattlegroundsLuckyDrawDataModel(), delegate(DialogBase dialog, object userData)
			{
				dialog.GetComponent<Widget>()?.TriggerEvent("SHOW");
				m_luckyDrawManager.SetShowHighlight(show: true);
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.LAST_BATTLE_BASH_END_SOON_BOX_ID_SEEN, m_luckyDrawManager.GetActiveLuckyDrawBoxID()));
				return true;
			});
		}
	}

	private void ShowLuckyDrawFTUENotification()
	{
		string message = GameStrings.Get("GLUE_BATTLEBASH_FTUE_HINT");
		GameObject popupBone = GameObjectUtils.FindChildBySubstring(m_luckyDrawButton.gameObject, "FTUETooltip" + PLATFORM_DEPENDENT_BONE_SUFFIX);
		if (popupBone == null)
		{
			LuckyDrawManager.Get().LogWarning("[BaconDisplay.ShowLuckyDrawFTUENotifiation] - Popup bone is missing");
			return;
		}
		NotificationManager notificationManager = NotificationManager.Get();
		m_luckyDrawFTUENotification = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, popupBone.transform.position, popupBone.transform.localScale, GameStrings.Get(message));
		m_luckyDrawFTUENotification.ShowPopUpArrow(Notification.PopUpArrowDirection.Up);
	}

	private void HidePopups(bool animate = false)
	{
		HideLuckyDrawPopups(animate);
		m_baconLobbyTutorial.HideAllPopups();
	}

	private void HideLuckyDrawPopups(bool animate = false)
	{
		StopCoroutine("WaitThenShowLuckyDrawPopups");
		if (!(m_luckyDrawFTUENotification == null))
		{
			if (animate)
			{
				NotificationManager.Get()?.DestroyNotification(m_luckyDrawFTUENotification, 0f);
			}
			else
			{
				NotificationManager.Get()?.DestroyNotificationNowWithNoAnim(m_luckyDrawFTUENotification);
			}
		}
	}

	private void LuckyDrawButtonClicked()
	{
		SetNextModeAndHandleTransition(SceneMgr.Mode.LUCKY_DRAW, SceneMgr.TransitionHandlerType.CURRENT_SCENE);
		HidePopups();
		m_luckyDrawButton.SetUserInteractionEnabled(enabled: false);
	}

	private void UpdateStatsButtonState()
	{
		if (!(m_statsButton == null) && !(m_statsButtonClickable == null))
		{
			bool buttonEnabled = HasAccessToStatsPage();
			m_statsButton.Flip(buttonEnabled, forceImmediate: true);
			m_statsButton.SetEnabled(buttonEnabled);
			m_statsButton.AddEventListener(UIEventType.RELEASE, OnStatsButtonReleased);
			m_statsButtonClickable.Active = buttonEnabled;
		}
	}

	private void OnStatsButtonReleased(UIEvent e)
	{
		HidePopups();
	}

	public void DuosSlot1ButtonRelease(UIEvent e)
	{
		JoinDuosSlot(1);
	}

	public void DuosSlot2ButtonRelease(UIEvent e)
	{
		JoinDuosSlot(2);
	}

	public void DuosSlot3ButtonRelease(UIEvent e)
	{
		JoinDuosSlot(3);
	}

	public void DuosSlot4ButtonRelease(UIEvent e)
	{
		JoinDuosSlot(4);
	}

	public void DuosSlot5ButtonRelease(UIEvent e)
	{
		JoinDuosSlot(5);
	}

	public void DuosSlot6ButtonRelease(UIEvent e)
	{
		JoinDuosSlot(6);
	}

	public void DuosSlot7ButtonRelease(UIEvent e)
	{
		JoinDuosSlot(7);
	}

	public void DuosSlot8ButtonRelease(UIEvent e)
	{
		JoinDuosSlot(8);
	}

	public void DuosSlot0ButtonRelease(UIEvent e)
	{
		JoinDuosSlot(0);
	}

	public void JoinDuosSlot(int slotId)
	{
		if (BattleNet.IsConnected() && !GameMgr.Get().IsFindingGame() && PartyManager.Get().IsInBattlegroundsParty())
		{
			PartyManager.Get().SetMySelectedBattlegroundsDuosTeamSlotId(slotId);
		}
	}

	public void ToggleDuosRelease(UIEvent e)
	{
		if (BattleNet.IsConnected() && !GameMgr.Get().IsFindingGame() && (!PartyManager.Get().IsInBattlegroundsParty() || PartyManager.Get().IsPartyLeader()))
		{
			if (BaconLobbyMgr.Get().GetBattlegroundsGameMode() == "duos")
			{
				BaconLobbyMgr.Get().SetBattlegroundsGameMode("solo");
			}
			else
			{
				BaconLobbyMgr.Get().SetBattlegroundsGameMode("duos");
			}
			if (PartyManager.Get().IsInBattlegroundsParty())
			{
				PartyManager.Get().SetSelectedBattlegroundsGameMode(BaconLobbyMgr.Get().GetBattlegroundsGameMode());
			}
			UpdateDuosToggleButtonBasedOnPartyInfo();
			m_baconLobbyTutorial.HideAllPopups();
			if (BaconLobbyMgr.Get().IsInDuosMode())
			{
				m_baconLobbyTutorial.StartQueueTutorial();
				m_baconLobbyTutorial.StartArrangePartyTutorial();
			}
			UpdateInfoButtonNotification();
		}
	}

	private void DisplayDuosDisabledMessage()
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_TOOLTIP_BUTTON_BACON_HEADLINE"),
			m_text = GameStrings.Get("GLUE_TOOLTIP_BUTTON_BG_DUOS_DISABLED_DESC"),
			m_showAlertIcon = true,
			m_responseDisplay = AlertPopup.ResponseDisplay.OK
		};
		DialogManager.Get().ShowPopup(info);
	}

	public void PlayButtonRelease(UIEvent e)
	{
		HidePopups();
		if (!BattleNet.IsConnected() || GameMgr.Get().IsFindingGame())
		{
			return;
		}
		NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		bool tutorialDisabled = !features.Games.BattlegroundsTutorial;
		PresenceMgr.Get().SetStatus(Global.PresenceStatus.BATTLEGROUNDS_QUEUE);
		if (BaconLobbyMgr.Get().GetBattlegroundsGameMode() == "duos" && !features.Games.BattlegroundsDuos)
		{
			DisplayDuosDisabledMessage();
			return;
		}
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.BACON, GameSaveKeySubkeyId.BACON_HAS_SEEN_TUTORIAL, out long hasSeenTutorial);
		PartyManager partyMgr = PartyManager.Get();
		if (partyMgr.IsInParty() && partyMgr.IsInBattlegroundsParty() && partyMgr.IsPartyLeader())
		{
			partyMgr.FindGame();
		}
		else if (hasSeenTutorial == 0L && !tutorialDisabled)
		{
			PlayBaconTutorial();
		}
		else if (BaconLobbyMgr.Get().GetBattlegroundsGameMode() == "solo")
		{
			GameMgr.Get().FindGame(GameType.GT_BATTLEGROUNDS, FormatType.FT_WILD, 3459, 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
		}
		else if (BaconLobbyMgr.Get().GetBattlegroundsGameMode() == "duos")
		{
			GameMgr.Get().FindGame(GameType.GT_BATTLEGROUNDS_DUO, FormatType.FT_WILD, 5173, 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
		}
	}

	public void PlayBaconTutorial()
	{
		GameMgr.Get().FindGame(GameType.GT_VS_AI, FormatType.FT_WILD, 3539, 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
	}

	public void BackButtonRelease(UIEvent e)
	{
		HidePopups();
		if (PartyManager.Get().IsInBattlegroundsParty())
		{
			ShowLeavePartyDialog();
		}
		else
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
		}
	}

	private void ShowLeavePartyDialog()
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_BACON_LEAVE_PARTY_CONFIRMATION_HEADER");
		info.m_text = (PartyManager.Get().IsPartyLeader() ? GameStrings.Get("GLUE_BACON_DISBAND_PARTY_CONFIRMATION_BODY") : GameStrings.Get("GLUE_BACON_LEAVE_PARTY_CONFIRMATION_BODY"));
		info.m_iconSet = AlertPopup.PopupInfo.IconSet.Default;
		info.m_showAlertIcon = false;
		info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_confirmText = GameStrings.Get("GLUE_BACON_LEAVE_PARTY_CONFIRMATION_CONFIRM");
		info.m_cancelText = GameStrings.Get("GLUE_BACON_LEAVE_PARTY_CONFIRMATION_CANCEL");
		info.m_responseCallback = delegate(AlertPopup.Response response, object userData)
		{
			if (response == AlertPopup.Response.CONFIRM)
			{
				BaconParty.Get().LeaveParty();
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	private void OnNetCacheReady()
	{
		NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady);
		if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.Battlegrounds && !SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
			Error.AddWarningLoc("GLOBAL_FEATURE_DISABLED_TITLE", "GLOBAL_FEATURE_DISABLED_MESSAGE_BATTLEGROUNDS");
		}
	}

	private void OnStatsPagePCReady(VisualController visualController)
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			if (visualController == null)
			{
				Error.AddDevWarning("UI Error!", "StatsPage could not be found! You will not be able to view stats!");
				return;
			}
			m_statFrameDataModel = GetBaconStatsFrameDataModel(visualController);
			m_statFrameDataModel.GameType = BaconLobbyMgr.Get().GetBattlegroundsGameModeType();
			InitializeBaconStatsPageData(GameSaveKeyId.BACON, "Contents", visualController);
			InitializeBaconStatsPageData(GameSaveKeyId.BACON_DUOS, "BaconStatsContentsDuos", visualController);
		}
	}

	private void OnStatsPagePhoneReady(VisualController visualController)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (visualController == null)
			{
				Error.AddDevWarning("UI Error!", "StatsPage could not be found! You will not be able to view stats!");
				return;
			}
			m_statFrameDataModel = GetBaconStatsFrameDataModel(visualController);
			m_statFrameDataModel.GameType = BaconLobbyMgr.Get().GetBattlegroundsGameModeType();
			InitializeBaconStatsPageData(GameSaveKeyId.BACON, "BaconStatsContents", visualController);
			InitializeBaconStatsPageData(GameSaveKeyId.BACON_DUOS, "BaconStatsContentsDuos", visualController);
		}
	}

	private void OnLobbyReady(Widget widget)
	{
		if (widget == null)
		{
			Error.AddDevWarning("UI Error!", "LobbyReference could not be found!");
			return;
		}
		m_lobbyFinishedLoading = true;
		m_lobbyWidget = widget;
		widget.RegisterEventListener(BaconDisplayEventListener);
		UpdateDuosToggleButtonBasedOnPartyInfo();
		ShowSeasonPassShopFromLockedHeroFTUEIfNeeded();
	}

	public BaconLobbyDataModel GetBaconLobbyDataModel()
	{
		VisualController visualController = GetComponent<VisualController>();
		if (visualController == null)
		{
			return null;
		}
		Widget owner = visualController.Owner;
		if (owner == null)
		{
			return null;
		}
		if (!owner.GetDataModel(43, out var dataModel))
		{
			dataModel = new BaconLobbyDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as BaconLobbyDataModel;
	}

	private void InitializeBaconLobbyData()
	{
		BaconLobbyDataModel dataModel = GetBaconLobbyDataModel();
		if (dataModel != null)
		{
			dataModel.Rating = BaconLobbyMgr.Get().GetBattlegroundsActiveGameModeRating();
			dataModel.Top4Finishes = (int)GetBaconGameSaveValue(GameSaveKeyId.BACON, GameSaveKeySubkeyId.BACON_TOP_4_FINISHES);
			dataModel.FirstPlaceFinishes = (int)GetBaconGameSaveValue(GameSaveKeyId.BACON, GameSaveKeySubkeyId.BACON_FIRST_PLACE_FINISHES);
			dataModel.ShopOpen = StoreManager.Get().IsOpen();
			NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
			dataModel.BattlegroundsSkinsEnabled = guardianVars.BattlegroundsSkinsEnabled;
			dataModel.BattlegroundsRewardTrackEnabled = guardianVars.BattlegroundsRewardTrackEnabled;
			dataModel.HasNewSkins = CollectionManager.Get().HasAnyNewBattlegroundsSkins();
			dataModel.LuckyDraw = m_luckyDrawManager.GetBattlegroundsLuckyDrawDataModel();
			dataModel.BattlegroundsInDuosMode = BaconLobbyMgr.Get().GetBattlegroundsGameMode() == "duos";
			UpdateInfoButtonNotification();
		}
	}

	private void RegisterListeners()
	{
		GameMgr.Get().RegisterFindGameEvent(OnFindGameEvent);
		NetCache.Get().OwnedBattlegroundsSkinsChanged += RefreshHasAnyNewSkins;
		GameMgr.Get().OnTransitionPopupShown += OnTransitionPopupShown;
		BnetPresenceMgr.Get().AddPlayersChangedListener(OnPresenceUpdated);
		BnetNearbyPlayerMgr.Get().AddChangeListener(OnNearbyPlayersUpdated);
		m_luckyDrawManager.RegisterOnEventEndsListeners(OnLuckyDrawExpired);
		BaconLobbyMgr.Get().AddBattlegroundsGameModeChangedListener(OnBattlegroundsGameModeChanged);
		PartyManager.Get().AddMemberAttributeChangedListener(OnMemberAttributeChanged);
	}

	private void ShowSeasonPassShopFromLockedHeroFTUEIfNeeded()
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_BG_SEASON_PASS_POPUP, out long hasSeenBGSeasonPassFTUE);
		if (hasSeenBGSeasonPassFTUE == 2)
		{
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_BG_SEASON_PASS_POPUP, 1L));
			OpenBattlegroundsShop();
		}
	}

	private void UnregisterListeners()
	{
		if (GameMgr.Get() != null)
		{
			GameMgr.Get().UnregisterFindGameEvent(OnFindGameEvent);
			GameMgr.Get().OnTransitionPopupShown -= OnTransitionPopupShown;
		}
		if (NetCache.Get() != null)
		{
			NetCache.Get().OwnedBattlegroundsSkinsChanged -= RefreshHasAnyNewSkins;
		}
		if (PartyManager.Get() != null)
		{
			PartyManager.Get().RemoveChangedListener(OnPartyChanged);
		}
		if (BnetPresenceMgr.Get() != null)
		{
			BnetPresenceMgr.Get().RemovePlayersChangedListener(OnPresenceUpdated);
		}
		if (BnetNearbyPlayerMgr.Get() != null)
		{
			BnetNearbyPlayerMgr.Get().RemoveChangeListener(OnNearbyPlayersUpdated);
		}
		if (StoreManager.Get() != null)
		{
			StoreManager.Get().RemoveStatusChangedListener(OnStoreStatusChanged);
		}
		if (m_luckyDrawManager != null)
		{
			m_luckyDrawManager.RemoveOnEventEndsListenders(OnLuckyDrawExpired);
		}
		if (BaconLobbyMgr.Get() != null)
		{
			BaconLobbyMgr.Get().RemoveBattlegroundsGameModeChangedListener(OnBattlegroundsGameModeChanged);
		}
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		FindGameState state = eventData.m_state;
		if ((uint)(state - 2) <= 1u || (uint)(state - 7) <= 1u || state == FindGameState.SERVER_GAME_CANCELED)
		{
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.BATTLEGROUNDS_SCREEN);
			UpdatePlayButtonBasedOnPartyInfo();
		}
		return false;
	}

	private void OnTransitionPopupShown()
	{
		if (Shop.Get() != null)
		{
			Shop.Get().Close();
		}
		if (DialogManager.Get() != null)
		{
			DialogManager.Get().ClearAllImmediately();
		}
	}

	private void ShowLowMemoryAlertMessage()
	{
		if ((bool)ShowLowMemoryWarning && !m_hasSeenLowMemoryWarningThisSession)
		{
			m_hasSeenLowMemoryWarningThisSession = true;
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_BACON_LOW_MEMORY_HEADER"),
				m_text = GameStrings.Get("GLUE_BACON_LOW_MEMORY_BODY"),
				m_showAlertIcon = true,
				m_responseDisplay = AlertPopup.ResponseDisplay.OK
			};
			DialogManager.Get().ShowPopup(info);
		}
	}

	public BaconStatsFrameDataModel GetBaconStatsFrameDataModel(VisualController visualController)
	{
		if (visualController == null)
		{
			return null;
		}
		Widget owner = visualController.Owner;
		if (!owner.GetDataModel(936, out var dataModel))
		{
			dataModel = new BaconStatsFrameDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as BaconStatsFrameDataModel;
	}

	public BaconStatsPageDataModel GetBaconStatsPageDataModel(VisualController visualController, string widgetName)
	{
		if (visualController == null)
		{
			return null;
		}
		Widget owner = visualController.Owner;
		if (!owner.GetDataModel(122, widgetName, out var dataModel))
		{
			dataModel = new BaconStatsPageDataModel();
			owner.BindDataModel(dataModel, widgetName);
		}
		return dataModel as BaconStatsPageDataModel;
	}

	private CardDataModel CreateCardDataModel(int cardDatabaseId)
	{
		CardDataModel cardDataModel = null;
		string CardId = GameUtils.TranslateDbIdToCardId(cardDatabaseId);
		if (!string.IsNullOrEmpty(CardId))
		{
			cardDataModel = new CardDataModel
			{
				CardId = CardId,
				Premium = TAG_PREMIUM.NORMAL
			};
		}
		return cardDataModel;
	}

	private void InitializePastGameStatsPageData(List<BaconPastGameStatsDataModel> pastGames, GameSaveKeyId gameSaveKey, bool teammate)
	{
		List<long> pastGameHeroes = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_HEROES : GameSaveKeySubkeyId.BACON_PAST_GAME_HEROES);
		List<long> pastGameMinionIds = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_MINIONS_ID : GameSaveKeySubkeyId.BACON_PAST_GAME_MINIONS_ID);
		List<long> pastGameMinionAttack = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_MINIONS_ATTACK : GameSaveKeySubkeyId.BACON_PAST_GAME_MINIONS_ATTACK);
		List<long> pastGameMinionHealth = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_MINIONS_HEALTH : GameSaveKeySubkeyId.BACON_PAST_GAME_MINIONS_HEALTH);
		List<long> pastGameMinionGolden = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_MINIONS_GOLDEN : GameSaveKeySubkeyId.BACON_PAST_GAME_MINIONS_GOLDEN);
		List<long> pastGameMinionTaunt = new List<long>();
		List<long> pastGameMinionDivineShield = new List<long>();
		List<long> pastGameMinionPoisonous = new List<long>();
		List<long> pastGameMinionVenomous = new List<long>();
		List<long> pastGameMinionWindfury = new List<long>();
		List<long> pastGameMinionReborn = new List<long>();
		List<long> pastGameQuestIDs = new List<long>();
		List<long> pastGameRewardIDs = new List<long>();
		List<long> pastGameRewardIsCompleted = new List<long>();
		List<long> pastGameRewardCardDatabaseIDs = new List<long>();
		List<long> pastGameRewardMinionTypes = new List<long>();
		List<long> pastGameQuestProgressTotal = new List<long>();
		List<long> pastGameQuestRace1 = new List<long>();
		List<long> pastGameQuestRace2 = new List<long>();
		List<long> pastGameHeroBuddyMeter = new List<long>();
		List<long> pastGameNumHeroBuddiesGained = new List<long>();
		List<long> pastGameHeroBuddyDatabaseID = new List<long>();
		List<long> pastGameHeroBuddyCost = new List<long>();
		List<long> pastGameTrinket1DatabaseID = new List<long>();
		List<long> pastGameTrinket2DatabaseID = new List<long>();
		List<long> pastGameTrinketHeropowerDatabaseID = new List<long>();
		List<long> pastGameTrinket1MinionType = new List<long>();
		List<long> pastGameTrinket2MinionType = new List<long>();
		List<long> pastGameTrinketHeroPowerMinionType = new List<long>();
		List<long> dualHeroPowers = new List<long>();
		PopulateAdditionalInfoLists(pastGameHeroes?.Count ?? 0, gameSaveKey, ref pastGameMinionTaunt, ref pastGameMinionDivineShield, ref pastGameMinionPoisonous, ref pastGameMinionVenomous, ref pastGameMinionWindfury, ref pastGameMinionReborn, ref pastGameQuestIDs, ref pastGameRewardIDs, ref pastGameRewardIsCompleted, ref pastGameRewardCardDatabaseIDs, ref pastGameRewardMinionTypes, ref pastGameQuestProgressTotal, ref pastGameQuestRace1, ref pastGameQuestRace2, ref pastGameHeroBuddyMeter, ref pastGameNumHeroBuddiesGained, ref pastGameHeroBuddyDatabaseID, ref pastGameHeroBuddyCost, ref pastGameTrinket1DatabaseID, ref pastGameTrinket2DatabaseID, ref pastGameTrinketHeropowerDatabaseID, ref pastGameTrinket1MinionType, ref pastGameTrinket2MinionType, ref pastGameTrinketHeroPowerMinionType, ref dualHeroPowers, teammate);
		int i = 0;
		foreach (BaconPastGameStatsDataModel pastGame in pastGames)
		{
			if (pastGameHeroes == null || i >= pastGameHeroes.Count)
			{
				break;
			}
			string cardId = GameUtils.TranslateDbIdToCardId((int)pastGameHeroes[i]);
			CardDataModel pastGameHeroCard = new CardDataModel
			{
				CardId = GameUtils.TranslateDbIdToCardId((int)pastGameHeroes[i]),
				Premium = TAG_PREMIUM.NORMAL
			};
			CardDbfRecord heroCardRecord = GameUtils.GetCardRecord(cardId);
			string heroName = ((heroCardRecord == null) ? "" : ((string)heroCardRecord.Name));
			CardDataModel pastGameHeroPowerCard = new CardDataModel
			{
				CardId = GameUtils.GetHeroPowerCardIdFromHero((int)pastGameHeroes[i]),
				Premium = TAG_PREMIUM.NORMAL,
				SpellTypes = new DataModelList<SpellType> { SpellType.COIN_MANA_GEM }
			};
			CardDataModel pastGameDualHeroPowerCard = new CardDataModel
			{
				CardId = GameUtils.TranslateDbIdToCardId((int)dualHeroPowers[i]),
				Premium = TAG_PREMIUM.NORMAL,
				SpellTypes = new DataModelList<SpellType> { SpellType.COIN_MANA_GEM }
			};
			string questCardId = GameUtils.TranslateDbIdToCardId((int)pastGameQuestIDs[i]);
			string rewardCardId = GameUtils.TranslateDbIdToCardId((int)pastGameRewardIDs[i]);
			CardDataModel pastGameQuestCard = new CardDataModel
			{
				CardId = questCardId,
				Premium = TAG_PREMIUM.NORMAL
			};
			CardDataModel pastGameRewardCard = new CardDataModel
			{
				CardId = rewardCardId,
				Premium = TAG_PREMIUM.NORMAL
			};
			string heroBuddyMeterCardId = GameUtils.TranslateDbIdToCardId((int)pastGameHeroBuddyMeter[i]);
			CardDataModel pastGameHeroBuddyMeterCard = new CardDataModel
			{
				CardId = heroBuddyMeterCardId,
				Premium = TAG_PREMIUM.NORMAL
			};
			string heroBuddyCardId = GameUtils.TranslateDbIdToCardId((int)pastGameHeroBuddyDatabaseID[i]);
			CardDataModel pastGameHeroBuddyCard = new CardDataModel
			{
				CardId = heroBuddyCardId,
				Premium = TAG_PREMIUM.NORMAL,
				SpellTypes = new DataModelList<SpellType> { SpellType.TECH_LEVEL_MANA_GEM }
			};
			string trinket1CardId = GameUtils.TranslateDbIdToCardId((int)pastGameTrinket1DatabaseID[i]);
			CardDataModel pastGameTrinket1Card = new CardDataModel
			{
				CardId = trinket1CardId,
				Premium = TAG_PREMIUM.NORMAL
			};
			string trinket2CardId = GameUtils.TranslateDbIdToCardId((int)pastGameTrinket2DatabaseID[i]);
			CardDataModel pastGameTrinket2Card = new CardDataModel
			{
				CardId = trinket2CardId,
				Premium = TAG_PREMIUM.NORMAL
			};
			CardDataModel pastGameTrinketHeropowerCard = CreateCardDataModel((int)pastGameTrinketHeropowerDatabaseID[i]);
			DataModelList<CardDataModel> pastGameMinions = new DataModelList<CardDataModel>();
			for (int j = 0; j < 7; j++)
			{
				int minionIndex = i * 7 + j;
				if (pastGameMinionIds.Count <= minionIndex || pastGameMinionAttack.Count <= minionIndex || pastGameMinionHealth.Count <= minionIndex || pastGameMinionGolden.Count <= minionIndex || pastGameMinionTaunt.Count <= minionIndex || pastGameMinionDivineShield.Count <= minionIndex || pastGameMinionPoisonous.Count <= minionIndex || pastGameMinionVenomous.Count <= minionIndex || pastGameMinionWindfury.Count <= minionIndex || pastGameMinionReborn.Count <= minionIndex)
				{
					Debug.LogErrorFormat("Missing Minion Data for GameIndex={0}, MinionIndex={1}", i, minionIndex);
					break;
				}
				if (pastGameMinionIds[minionIndex] == 0L)
				{
					break;
				}
				DataModelList<SpellType> spellTypes = new DataModelList<SpellType>();
				bool isGolden = pastGameMinionGolden[minionIndex] > 0;
				if (pastGameMinionTaunt[minionIndex] > 0)
				{
					spellTypes.Add(isGolden ? SpellType.TAUNT_INSTANT_PREMIUM : SpellType.TAUNT_INSTANT);
				}
				if (pastGameMinionDivineShield[minionIndex] > 0)
				{
					spellTypes.Add(SpellType.DIVINE_SHIELD);
				}
				if (pastGameMinionWindfury[minionIndex] > 0)
				{
					spellTypes.Add(SpellType.WINDFURY_IDLE);
				}
				if (pastGameMinionReborn[minionIndex] > 0)
				{
					spellTypes.Add(SpellType.REBORN);
				}
				if (pastGameMinionPoisonous[minionIndex] > 0)
				{
					spellTypes.Add(SpellType.POISONOUS);
				}
				else if (pastGameMinionVenomous[minionIndex] > 0)
				{
					spellTypes.Add(SpellType.VENOMOUS);
				}
				pastGameMinions.Add(new CardDataModel
				{
					CardId = GameUtils.TranslateDbIdToCardId((int)pastGameMinionIds[minionIndex]),
					Premium = (isGolden ? TAG_PREMIUM.GOLDEN : TAG_PREMIUM.NORMAL),
					Attack = (int)pastGameMinionAttack[minionIndex],
					Health = (int)pastGameMinionHealth[minionIndex],
					SpellTypes = spellTypes
				});
			}
			if (teammate)
			{
				pastGame.TeammateHero = pastGameHeroCard;
				pastGame.TeammateHeroPower = pastGameHeroPowerCard;
				pastGame.TeammateHeroName = heroName;
				pastGame.TeammateMinions = pastGameMinions;
				pastGame.TeammateReward = pastGameRewardCard;
				pastGame.TeammateQuest = pastGameQuestCard;
				pastGame.TeammateRewardCompleted = pastGameRewardIsCompleted[i] != 0;
				pastGame.TeammateRewardCardDatabaseID = (int)pastGameRewardCardDatabaseIDs[i];
				pastGame.TeammateRewardMinionType = (int)pastGameRewardMinionTypes[i];
				pastGame.TeammateQuestProgressTotal = (int)pastGameQuestProgressTotal[i];
				pastGame.TeammateQuestRace1 = (int)pastGameQuestRace1[i];
				pastGame.TeammateQuestRace2 = (int)pastGameQuestRace2[i];
				pastGame.TeammateHeroBuddyMeter = pastGameHeroBuddyMeterCard;
				pastGame.TeammateNumHeroBuddiesGained = (int)pastGameNumHeroBuddiesGained[i];
				pastGame.TeammateHeroBuddy = pastGameHeroBuddyCard;
				pastGame.TeammateHeroBuddyDatabaseID = (int)pastGameHeroBuddyDatabaseID[i];
				pastGame.TeammateHeroBuddyCost = (int)pastGameHeroBuddyCost[i];
				pastGame.TeammateTrinket1 = pastGameTrinket1Card;
				pastGame.TeammateTrinket2 = pastGameTrinket2Card;
				pastGame.TeammateTrinketHeroPower = pastGameTrinketHeropowerCard;
				pastGame.TeammateTrinket1MinionType = (int)pastGameTrinket1MinionType[i];
				pastGame.TeammateTrinket2MinionType = (int)pastGameTrinket2MinionType[i];
				pastGame.TeammateTrinketHeroPowerMinionType = (int)pastGameTrinketHeroPowerMinionType[i];
				pastGame.TeammateDualHeroPower = pastGameDualHeroPowerCard;
			}
			else
			{
				pastGame.Hero = pastGameHeroCard;
				pastGame.HeroPower = pastGameHeroPowerCard;
				pastGame.HeroName = heroName;
				pastGame.Minions = pastGameMinions;
				pastGame.Reward = pastGameRewardCard;
				pastGame.Quest = pastGameQuestCard;
				pastGame.RewardCompleted = pastGameRewardIsCompleted[i] != 0;
				pastGame.RewardCardDatabaseID = (int)pastGameRewardCardDatabaseIDs[i];
				pastGame.RewardMinionType = (int)pastGameRewardMinionTypes[i];
				pastGame.QuestProgressTotal = (int)pastGameQuestProgressTotal[i];
				pastGame.QuestRace1 = (int)pastGameQuestRace1[i];
				pastGame.QuestRace2 = (int)pastGameQuestRace2[i];
				pastGame.HeroBuddyMeter = pastGameHeroBuddyMeterCard;
				pastGame.NumHeroBuddiesGained = (int)pastGameNumHeroBuddiesGained[i];
				pastGame.HeroBuddy = pastGameHeroBuddyCard;
				pastGame.HeroBuddyDatabaseID = (int)pastGameHeroBuddyDatabaseID[i];
				pastGame.HeroBuddyCost = (int)pastGameHeroBuddyCost[i];
				pastGame.Trinket1 = pastGameTrinket1Card;
				pastGame.Trinket2 = pastGameTrinket2Card;
				pastGame.TrinketHeroPower = pastGameTrinketHeropowerCard;
				pastGame.Trinket1MinionType = (int)pastGameTrinket1MinionType[i];
				pastGame.Trinket2MinionType = (int)pastGameTrinket2MinionType[i];
				pastGame.TrinketHeroPowerMinionType = (int)pastGameTrinketHeroPowerMinionType[i];
				pastGame.DualHeroPower = pastGameDualHeroPowerCard;
			}
			i++;
		}
	}

	private void InitializeBaconStatsPageData(GameSaveKeyId gameSaveKey, string widgetName, VisualController visualController)
	{
		BaconStatsPageDataModel dataModel = GetBaconStatsPageDataModel(visualController, widgetName);
		if (dataModel == null)
		{
			return;
		}
		dataModel.Top4Finishes = (int)GetBaconGameSaveValue(gameSaveKey, GameSaveKeySubkeyId.BACON_TOP_4_FINISHES);
		dataModel.FirstPlaceFinishes = (int)GetBaconGameSaveValue(gameSaveKey, GameSaveKeySubkeyId.BACON_FIRST_PLACE_FINISHES);
		dataModel.TriplesCreated = (int)GetBaconGameSaveValue(gameSaveKey, GameSaveKeySubkeyId.BACON_TRIPLES_CREATED);
		dataModel.TavernUpgrades = (int)GetBaconGameSaveValue(gameSaveKey, GameSaveKeySubkeyId.BACON_TAVERN_UPGRADES);
		dataModel.DamageInOneTurn = (int)GetBaconGameSaveValue(gameSaveKey, GameSaveKeySubkeyId.BACON_MOST_DAMAGE_ONE_TURN);
		dataModel.LongestWinStreak = (int)GetBaconGameSaveValue(gameSaveKey, GameSaveKeySubkeyId.BACON_LONGEST_COMBAT_WIN_STREAK);
		dataModel.SecondsPlayed = (int)GetBaconGameSaveValue(gameSaveKey, GameSaveKeySubkeyId.BACON_TIME_PLAYED);
		dataModel.CardsPassed = (int)GetBaconGameSaveValue(gameSaveKey, GameSaveKeySubkeyId.BACON_CARDS_PASSED);
		dataModel.PingsSent = (int)GetBaconGameSaveValue(gameSaveKey, GameSaveKeySubkeyId.BACON_PINGS_SENT);
		List<long> minionDestroyedCountList = GetBaconGameSaveValueList(gameSaveKey, GameSaveKeySubkeyId.BACON_MINIONS_KILLED_COUNT);
		dataModel.MinionsDestroyed = (int)((minionDestroyedCountList != null) ? minionDestroyedCountList.Sum() : 0);
		List<long> playersEliminatedCountList = GetBaconGameSaveValueList(gameSaveKey, GameSaveKeySubkeyId.BACON_HEROES_KILLED_COUNT);
		dataModel.PlayersEliminated = (int)((playersEliminatedCountList != null) ? playersEliminatedCountList.Sum() : 0);
		List<long> biggestMinionList = GetBaconGameSaveValueList(gameSaveKey, GameSaveKeySubkeyId.BACON_LARGEST_MINION_ATTACK_HEALTH);
		dataModel.BiggestMinionId = ((biggestMinionList == null || biggestMinionList.Count() < 3) ? null : new CardDataModel
		{
			CardId = GameUtils.TranslateDbIdToCardId((int)biggestMinionList[0]),
			Premium = TAG_PREMIUM.NORMAL
		});
		dataModel.BiggestMinionAttack = (int)((biggestMinionList != null && biggestMinionList.Count() >= 3) ? biggestMinionList[1] : 0);
		dataModel.BiggestMinionHealth = (int)((biggestMinionList != null && biggestMinionList.Count() >= 3) ? biggestMinionList[2] : 0);
		dataModel.BiggestMinionString = GameStrings.Format("GLUE_BACON_STATS_VALUE_BIGGEST_MINION", dataModel.BiggestMinionAttack, dataModel.BiggestMinionHealth);
		if (dataModel.SecondsPlayed > 3600)
		{
			dataModel.TimePlayedString = GameStrings.Format("GLUE_BACON_STATS_VALUE_HOURS_PLAYED", Mathf.FloorToInt(dataModel.SecondsPlayed / 3600));
		}
		else
		{
			dataModel.TimePlayedString = GameStrings.Format("GLUE_BACON_STATS_VALUE_MINUTES_PLAYED", Mathf.FloorToInt(dataModel.SecondsPlayed / 60));
		}
		List<KeyValuePair<long, long>> mostBoughtMinionList = GetSortedListFromGameSaveDataLists(GetBaconGameSaveValueList(gameSaveKey, GameSaveKeySubkeyId.BACON_BOUGHT_MINIONS), GetBaconGameSaveValueList(gameSaveKey, GameSaveKeySubkeyId.BACON_BOUGHT_MINIONS_COUNT));
		dataModel.MostBoughtMinionsCardIds = new DataModelList<CardDataModel>();
		dataModel.MostBoughtMinionsCardIds.AddRange(mostBoughtMinionList.Select((KeyValuePair<long, long> kvp) => new CardDataModel
		{
			CardId = GameUtils.TranslateDbIdToCardId((int)kvp.Key),
			Premium = TAG_PREMIUM.NORMAL
		}));
		dataModel.MostBoughtMinionsCount = new DataModelList<int>();
		dataModel.MostBoughtMinionsCount.AddRange(mostBoughtMinionList.Select((KeyValuePair<long, long> kvp) => (int)kvp.Value));
		List<KeyValuePair<long, long>> topHeroesByWinList = GetSortedListFromGameSaveDataLists(GetBaconGameSaveValueList(gameSaveKey, GameSaveKeySubkeyId.BACON_HEROES_WON_WITH), GetBaconGameSaveValueList(gameSaveKey, GameSaveKeySubkeyId.BACON_HEROES_WON_WITH_COUNT));
		dataModel.TopHeroesByWinCardIds = new DataModelList<CardDataModel>();
		dataModel.TopHeroesByWinCardIds.AddRange(topHeroesByWinList.Select((KeyValuePair<long, long> kvp) => new CardDataModel
		{
			CardId = GameUtils.TranslateDbIdToCardId((int)kvp.Key),
			Premium = TAG_PREMIUM.NORMAL
		}));
		dataModel.TopHeroesByWinCount = new DataModelList<int>();
		dataModel.TopHeroesByWinCount.AddRange(topHeroesByWinList.Select((KeyValuePair<long, long> kvp) => (int)kvp.Value));
		List<KeyValuePair<long, long>> topHeroesByGamesPlayedList = GetSortedListFromGameSaveDataLists(GetBaconGameSaveValueList(gameSaveKey, GameSaveKeySubkeyId.BACON_HEROES_PICKED), GetBaconGameSaveValueList(gameSaveKey, GameSaveKeySubkeyId.BACON_HEROES_PICKED_COUNT));
		dataModel.TopHeroesByGamesPlayedCardIds = new DataModelList<CardDataModel>();
		dataModel.TopHeroesByGamesPlayedCardIds.AddRange(topHeroesByGamesPlayedList.Select((KeyValuePair<long, long> kvp) => new CardDataModel
		{
			CardId = GameUtils.TranslateDbIdToCardId((int)kvp.Key),
			Premium = TAG_PREMIUM.NORMAL
		}));
		dataModel.TopHeroesByGamesPlayedCount = new DataModelList<int>();
		dataModel.TopHeroesByGamesPlayedCount.AddRange(topHeroesByGamesPlayedList.Select((KeyValuePair<long, long> kvp) => (int)kvp.Value));
		dataModel.PastGames = new DataModelList<BaconPastGameStatsDataModel>();
		List<long> pastGamePlaces = GetBaconGameSaveValueList(gameSaveKey, GameSaveKeySubkeyId.BACON_PAST_GAME_PLACES);
		if (pastGamePlaces == null)
		{
			pastGamePlaces = new List<long>();
		}
		List<long> pastGameAnomalies = GetBaconGameSaveValueList(gameSaveKey, GameSaveKeySubkeyId.BACON_PAST_GAME_ANOMALY);
		if (pastGameAnomalies == null)
		{
			pastGameAnomalies = new List<long>();
		}
		while (pastGameAnomalies.Count < pastGamePlaces.Count)
		{
			pastGameAnomalies.Insert(0, 0L);
		}
		List<BaconPastGameStatsDataModel> pastGames = new List<BaconPastGameStatsDataModel>();
		for (int i = 0; i < 5 && i < pastGamePlaces.Count; i++)
		{
			BaconPastGameStatsDataModel pastGame = new BaconPastGameStatsDataModel
			{
				Place = (int)pastGamePlaces[i]
			};
			if ((int)pastGameAnomalies[i] != 0)
			{
				string anomalyCardID = GameUtils.TranslateDbIdToCardId((int)pastGameAnomalies[i]);
				CardDataModel anomalyCard = new CardDataModel
				{
					CardId = anomalyCardID,
					Premium = TAG_PREMIUM.NORMAL
				};
				pastGame.Anomaly = anomalyCard;
			}
			pastGames.Add(pastGame);
		}
		InitializePastGameStatsPageData(pastGames, gameSaveKey, teammate: false);
		if (gameSaveKey == GameSaveKeyId.BACON_DUOS)
		{
			InitializePastGameStatsPageData(pastGames, gameSaveKey, teammate: true);
		}
		pastGames.Reverse();
		pastGames.ForEach(delegate(BaconPastGameStatsDataModel g)
		{
			dataModel.PastGames.Add(g);
		});
	}

	private void PopulateAdditionalInfoLists(int pastGames, GameSaveKeyId gameSaveKey, ref List<long> tauntList, ref List<long> divineShieldList, ref List<long> poisonousList, ref List<long> venomousList, ref List<long> windfuryList, ref List<long> rebornList, ref List<long> questIDList, ref List<long> rewardIDList, ref List<long> rewardIsCompletedList, ref List<long> rewardCardDatabaseIDList, ref List<long> rewardMinionTypeList, ref List<long> questProgressTotalList, ref List<long> questRace1List, ref List<long> questRace2List, ref List<long> heroBuddyList, ref List<long> numHeroBuddiesGainedList, ref List<long> heroBuddyDatabaseIDList, ref List<long> heroBuddyCostList, ref List<long> trinket1List, ref List<long> trinket2List, ref List<long> trinketHeropowerList, ref List<long> trinket1MinionTypeList, ref List<long> trinket2MinionTypeList, ref List<long> trinketHPMinionTypeList, ref List<long> dualHeroPowers, bool teammate)
	{
		tauntList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_MINIONS_TAUNT : GameSaveKeySubkeyId.BACON_PAST_GAME_MINIONS_TAUNT);
		divineShieldList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_MINIONS_DIVINE_SHIELD : GameSaveKeySubkeyId.BACON_PAST_GAME_MINIONS_DIVINE_SHIELD);
		poisonousList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_MINIONS_POISONOUS : GameSaveKeySubkeyId.BACON_PAST_GAME_MINIONS_POISONOUS);
		venomousList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_MINION_VENOMOUS : GameSaveKeySubkeyId.BACON_PAST_GAME_MINIONS_VENOMOUS);
		windfuryList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_MINIONS_WINDFURY : GameSaveKeySubkeyId.BACON_PAST_GAME_MINIONS_WINDFURY);
		rebornList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_MINIONS_REBORN : GameSaveKeySubkeyId.BACON_PAST_GAME_MINIONS_REBORN);
		questIDList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_QUEST_IDS : GameSaveKeySubkeyId.BACON_PAST_GAME_QUEST_IDS);
		rewardIDList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_REWARD_IDS : GameSaveKeySubkeyId.BACON_PAST_GAME_REWARD_IDS);
		rewardIsCompletedList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_REWARD_IS_COMPLETE : GameSaveKeySubkeyId.BACON_PAST_GAME_REWARD_IS_COMPLETED);
		rewardCardDatabaseIDList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_REWARD_CARD_ID : GameSaveKeySubkeyId.BACON_PAST_GAME_REWARD_CARD_DATABASE_ID);
		rewardMinionTypeList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_REWARD_MINION_TYPE : GameSaveKeySubkeyId.BACON_PAST_GAME_REWARD_MINION_TYPE);
		questProgressTotalList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_QUEST_PROGRESS_TOTAL : GameSaveKeySubkeyId.BACON_PAST_GAME_QUEST_PROGRESS_TOTAL);
		questRace1List = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_QUEST_RACE_1 : GameSaveKeySubkeyId.BACON_PAST_GAME_QUEST_RACE_1);
		questRace2List = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_QUEST_RACE_2 : GameSaveKeySubkeyId.BACON_PAST_GAME_QUEST_RACE_2);
		heroBuddyList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_BUDDY_METER : GameSaveKeySubkeyId.BACON_PAST_GAME_HERO_BUDDY);
		numHeroBuddiesGainedList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_BUDDY_GAINED_COUNT : GameSaveKeySubkeyId.BACON_PAST_GAME_NUM_HERO_BUDDIES_GAINED);
		heroBuddyDatabaseIDList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_BUDDY_ID : GameSaveKeySubkeyId.BACON_PAST_GAME_HERO_BUDDY_DATABASE_ID);
		heroBuddyCostList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_BUDDY_COST : GameSaveKeySubkeyId.BACON_PAST_GAME_HERO_BUDDY_COST);
		trinket1List = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_TRINKET_1 : GameSaveKeySubkeyId.BACON_PAST_GAME_TRINKET_1);
		trinket2List = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_TRINKET_2 : GameSaveKeySubkeyId.BACON_PAST_GAME_TRINKET_2);
		trinketHeropowerList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_TRINKET_HEROPOWER : GameSaveKeySubkeyId.BACON_PAST_GAME_TRINKET_HEROPOWER);
		trinket1MinionTypeList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_TRINKET_1_MINION_TYPE : GameSaveKeySubkeyId.BACON_PAST_GAME_TRINKET_1_MINION_TYPE);
		trinket2MinionTypeList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_TRINKET_2_MINION_TYPE : GameSaveKeySubkeyId.BACON_PAST_GAME_TRINKET_2_MINION_TYPE);
		trinketHPMinionTypeList = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_TRINKET_HEROPOWER_MINION_TYPE : GameSaveKeySubkeyId.BACON_PAST_GAME_TRINKET_HEROPOWER_MINION_TYPE);
		dualHeroPowers = GetBaconGameSaveValueList(gameSaveKey, teammate ? GameSaveKeySubkeyId.TEAMMATE_PAST_GAME_DUAL_HERO_POWER : GameSaveKeySubkeyId.BACON_PAST_GAME_DUAL_HERO_POWER);
		if (tauntList == null)
		{
			tauntList = new List<long>();
		}
		if (divineShieldList == null)
		{
			divineShieldList = new List<long>();
		}
		if (poisonousList == null)
		{
			poisonousList = new List<long>();
		}
		if (venomousList == null)
		{
			venomousList = new List<long>();
		}
		if (windfuryList == null)
		{
			windfuryList = new List<long>();
		}
		if (rebornList == null)
		{
			rebornList = new List<long>();
		}
		if (questIDList == null)
		{
			questIDList = new List<long>();
		}
		if (rewardIDList == null)
		{
			rewardIDList = new List<long>();
		}
		if (rewardIsCompletedList == null)
		{
			rewardIsCompletedList = new List<long>();
		}
		if (rewardCardDatabaseIDList == null)
		{
			rewardCardDatabaseIDList = new List<long>();
		}
		if (rewardMinionTypeList == null)
		{
			rewardMinionTypeList = new List<long>();
		}
		if (questProgressTotalList == null)
		{
			questProgressTotalList = new List<long>();
		}
		if (questRace1List == null)
		{
			questRace1List = new List<long>();
		}
		if (questRace2List == null)
		{
			questRace2List = new List<long>();
		}
		if (heroBuddyList == null)
		{
			heroBuddyList = new List<long>();
		}
		if (numHeroBuddiesGainedList == null)
		{
			numHeroBuddiesGainedList = new List<long>();
		}
		if (heroBuddyDatabaseIDList == null)
		{
			heroBuddyDatabaseIDList = new List<long>();
		}
		if (heroBuddyCostList == null)
		{
			heroBuddyCostList = new List<long>();
		}
		if (trinket1List == null)
		{
			trinket1List = new List<long>();
		}
		if (trinket2List == null)
		{
			trinket2List = new List<long>();
		}
		if (trinketHeropowerList == null)
		{
			trinketHeropowerList = new List<long>();
		}
		if (trinket1MinionTypeList == null)
		{
			trinket1MinionTypeList = new List<long>();
		}
		if (trinket2MinionTypeList == null)
		{
			trinket2MinionTypeList = new List<long>();
		}
		if (trinketHPMinionTypeList == null)
		{
			trinketHPMinionTypeList = new List<long>();
		}
		if (dualHeroPowers == null)
		{
			dualHeroPowers = new List<long>();
		}
		while (tauntList.Count < pastGames * 7)
		{
			tauntList.Insert(0, 0L);
		}
		while (divineShieldList.Count < pastGames * 7)
		{
			divineShieldList.Insert(0, 0L);
		}
		while (poisonousList.Count < pastGames * 7)
		{
			poisonousList.Insert(0, 0L);
		}
		while (venomousList.Count < pastGames * 7)
		{
			venomousList.Insert(0, 0L);
		}
		while (windfuryList.Count < pastGames * 7)
		{
			windfuryList.Insert(0, 0L);
		}
		while (rebornList.Count < pastGames * 7)
		{
			rebornList.Insert(0, 0L);
		}
		while (questIDList.Count < pastGames)
		{
			questIDList.Insert(0, 0L);
		}
		while (rewardIDList.Count < pastGames)
		{
			rewardIDList.Insert(0, 0L);
		}
		while (rewardIsCompletedList.Count < pastGames)
		{
			rewardIsCompletedList.Insert(0, 0L);
		}
		while (rewardCardDatabaseIDList.Count < pastGames)
		{
			rewardCardDatabaseIDList.Insert(0, 0L);
		}
		while (rewardMinionTypeList.Count < pastGames)
		{
			rewardMinionTypeList.Insert(0, 0L);
		}
		while (questProgressTotalList.Count < pastGames)
		{
			questProgressTotalList.Insert(0, 0L);
		}
		while (questRace1List.Count < pastGames)
		{
			questRace1List.Insert(0, 0L);
		}
		while (questRace2List.Count < pastGames)
		{
			questRace2List.Insert(0, 0L);
		}
		while (heroBuddyList.Count < pastGames)
		{
			heroBuddyList.Insert(0, 0L);
		}
		while (numHeroBuddiesGainedList.Count < pastGames)
		{
			numHeroBuddiesGainedList.Insert(0, 0L);
		}
		while (heroBuddyDatabaseIDList.Count < pastGames)
		{
			heroBuddyDatabaseIDList.Insert(0, 0L);
		}
		while (heroBuddyCostList.Count < pastGames)
		{
			heroBuddyCostList.Insert(0, 0L);
		}
		while (trinket1List.Count < pastGames)
		{
			trinket1List.Insert(0, 0L);
		}
		while (trinket2List.Count < pastGames)
		{
			trinket2List.Insert(0, 0L);
		}
		while (trinketHeropowerList.Count < pastGames)
		{
			trinketHeropowerList.Insert(0, 0L);
		}
		while (trinket1MinionTypeList.Count < pastGames)
		{
			trinket1MinionTypeList.Insert(0, 0L);
		}
		while (trinket2MinionTypeList.Count < pastGames)
		{
			trinket2MinionTypeList.Insert(0, 0L);
		}
		while (trinketHPMinionTypeList.Count < pastGames)
		{
			trinketHPMinionTypeList.Insert(0, 0L);
		}
		while (dualHeroPowers.Count < pastGames)
		{
			dualHeroPowers.Insert(0, 0L);
		}
	}

	private long GetBaconGameSaveValue(GameSaveKeyId gameSaveKey, GameSaveKeySubkeyId subkey)
	{
		GameSaveDataManager.Get().GetSubkeyValue(gameSaveKey, subkey, out long value);
		return value;
	}

	private List<long> GetBaconGameSaveValueList(GameSaveKeyId gameSaveKey, GameSaveKeySubkeyId subkey)
	{
		GameSaveDataManager.Get().GetSubkeyValue(gameSaveKey, subkey, out List<long> value);
		return value;
	}

	private List<KeyValuePair<long, long>> GetSortedListFromGameSaveDataLists(List<long> keys, List<long> values)
	{
		List<KeyValuePair<long, long>> sortedList = new List<KeyValuePair<long, long>>();
		if (keys == null || values == null)
		{
			return sortedList;
		}
		if (keys.Count != values.Count)
		{
			Debug.LogError("GetSortedListFromGameSaveDataLists: Stats Page Game Save Data Lists Length Not Equal!");
			return sortedList;
		}
		for (int i = 0; i < keys.Count; i++)
		{
			sortedList.Add(new KeyValuePair<long, long>(keys[i], values[i]));
		}
		return sortedList.OrderByDescending((KeyValuePair<long, long> kvp) => kvp.Value).ToList();
	}

	private bool HasAccessToStatsPage()
	{
		return true;
	}

	public void OpenBattlegroundsShop(string requestedTab = "battlegrounds")
	{
		BaconLobbyDataModel datamodel = GetBaconLobbyDataModel();
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService) || !datamodel.ShopOpen)
		{
			return;
		}
		if (!dataService.HasProductsAvailable())
		{
			ShowBattlegroundsStoreEmptyPopup();
			return;
		}
		StoreManager.Get().StartGeneralTransaction(requestedTab, OnStoreBackButtonPressed);
		Shop shop = Shop.Get();
		if (shop != null)
		{
			shop.OnOpenCompleted += OnShopOpenCompleted;
		}
	}

	private void OnStoreBackButtonPressed(bool authorizationBackButtonPressed, object userData)
	{
		OnShopClosed();
	}

	private void OnShopOpenCompleted()
	{
		m_OwningWidget.TriggerEvent("ShopOpenCompleted");
		Box.Get()?.m_rootObject.SetActive(value: false);
		Shop shop = Shop.Get();
		if (shop != null)
		{
			FullScreenEffects fullScreenEffects = BaseUI.Get().GetBnetCamera().GetComponent<FullScreenEffects>();
			if (fullScreenEffects != null)
			{
				fullScreenEffects.OnResultCached += FullScreenEffects_OnResultCached;
				fullScreenEffects.OnResultReleased += FullScreenEffects_OnResultReleased;
			}
			ProductPageController productPageController = shop.ProductPageController;
			productPageController.OnProductPageOpening = (Action<ProductPage>)Delegate.Combine(productPageController.OnProductPageOpening, new Action<ProductPage>(Shop_OnShopProductPageOpened));
			ProductPageController productPageController2 = shop.ProductPageController;
			productPageController2.OnProductPageClosed = (Action<ProductPage>)Delegate.Combine(productPageController2.OnProductPageClosed, new Action<ProductPage>(Shop_OnShopProductPageClosed));
			shop.OnOpenCompleted -= OnShopOpenCompleted;
		}
		if (ServiceManager.TryGet<MessagePopupDisplay>(out var popupDisplay))
		{
			popupDisplay.TriggerEvent(PopupEvent.OnBGShop);
		}
	}

	private void Shop_OnShopProductPageOpened(ProductPage _)
	{
		m_shopProductPageOpened = true;
	}

	private void Shop_OnShopProductPageClosed(ProductPage _)
	{
		m_shopProductPageOpened = false;
	}

	private void FullScreenEffects_OnResultReleased()
	{
		Shop shop = Shop.Get();
		if (shop != null)
		{
			shop.SetShopBrowserHidden(hidden: false);
		}
	}

	private void FullScreenEffects_OnResultCached()
	{
		if (m_shopProductPageOpened)
		{
			Shop shop = Shop.Get();
			if (shop != null)
			{
				shop.SetShopBrowserHidden(hidden: true);
			}
		}
	}

	private void OnShopClosed()
	{
		Box.Get()?.m_rootObject.SetActive(value: true);
		m_OwningWidget.TriggerEvent("ShopClosed");
		Shop shop = Shop.Get();
		if (shop != null)
		{
			FullScreenEffects fullScreenEffects = BaseUI.Get().GetBnetCamera().GetComponent<FullScreenEffects>();
			if (fullScreenEffects != null)
			{
				fullScreenEffects.OnResultCached -= FullScreenEffects_OnResultCached;
				fullScreenEffects.OnResultReleased -= FullScreenEffects_OnResultReleased;
			}
			ProductPageController productPageController = shop.ProductPageController;
			productPageController.OnProductPageOpening = (Action<ProductPage>)Delegate.Remove(productPageController.OnProductPageOpening, new Action<ProductPage>(Shop_OnShopProductPageOpened));
			ProductPageController productPageController2 = shop.ProductPageController;
			productPageController2.OnProductPageClosed = (Action<ProductPage>)Delegate.Remove(productPageController2.OnProductPageClosed, new Action<ProductPage>(Shop_OnShopProductPageClosed));
		}
	}

	private void OpenBattlegroundsCollection()
	{
		CollectionManager.Get().NotifyOfBoxTransitionStart();
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.BACON_COLLECTION);
	}

	private void RefreshHasAnyNewSkins()
	{
		GetBaconLobbyDataModel().HasNewSkins = CollectionManager.Get().HasAnyNewBattlegroundsSkins();
	}

	private void ShowBattlegroundsBonusErrorPopup()
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_BACON_PERKS_ERROR_HEADER"),
			m_text = GameStrings.Get("GLUE_BACON_PERKS_ERROR_BODY"),
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.OK
		};
		DialogManager.Get().ShowPopup(info);
	}

	private bool ShouldShowLuckyDrawEndsSoonPopup()
	{
		int daysLeft = LuckyDrawUtils.GetLuckyDrawTimeRemaining(LuckyDrawManager.Get().GetActiveLuckyDrawBoxID()).Days;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.LAST_BATTLE_BASH_END_SOON_BOX_ID_SEEN, out long lastSeenLuckyDrawEndSoonPopupBoxID);
		if (daysLeft <= 3 && daysLeft >= 1)
		{
			return lastSeenLuckyDrawEndSoonPopupBoxID != m_luckyDrawManager.GetActiveLuckyDrawBoxID();
		}
		return false;
	}

	public void ShowLuckyDrawEndsSoonTooltip(float delay)
	{
		m_showLuckyDrawEndsCoroutine = StartCoroutine(ShowLuckyDrawEndsSoonToolTip(delay));
	}

	private IEnumerator ShowLuckyDrawEndsSoonToolTip(float delay)
	{
		yield return new WaitForSeconds(delay);
		LuckyDrawDataModel battlegroundsLuckyDrawDataModel = LuckyDrawManager.Get().GetBattlegroundsLuckyDrawDataModel();
		string title = GameStrings.Get("GLUE_BATTLEBASH_EVENT_TIME_REM_TOOLTIP_TITLE");
		string message = battlegroundsLuckyDrawDataModel.TimeLeftToolTip;
		m_luckyDrawButton.GetTooltipZone().ShowTooltip(title, message, m_luckyDrawButton.GetToolTipScale());
	}

	public void HideLuckyDrawEndsSoonTooltip()
	{
		if (m_showLuckyDrawEndsCoroutine != null)
		{
			StopCoroutine(m_showLuckyDrawEndsCoroutine);
			m_showLuckyDrawEndsCoroutine = null;
		}
		TooltipZone toolTipZone = m_luckyDrawButton.GetTooltipZone();
		if (!(toolTipZone == null) && toolTipZone.Shown)
		{
			toolTipZone.HideTooltip();
		}
	}

	private bool ShouldShowBattlegroundsUnacknowledgedEarnedHammersPopUp()
	{
		return m_luckyDrawManager.NumUnacknowledgedEarnedHammers() > 0;
	}

	private IEnumerator PlayLuckyDrawButtonFanfareFX()
	{
		yield return new WaitForSeconds(0.5f);
		GameObject fanfare = GameObjectUtils.FindChild(m_luckyDrawButton.gameObject, "NewHammerFX");
		if (fanfare == null)
		{
			LuckyDrawManager.Get().LogError("BaconDisplay.PlayLuckyDrawButtonFanfareFX - New Hammer FX object is null");
			yield break;
		}
		Animator animator = fanfare.GetComponent<Animator>();
		if (animator == null)
		{
			LuckyDrawManager.Get().LogError("BaconDisplay.PlayLuckyDrawButtonFanfareFX - New Hammer FX object does not have animator component");
		}
		else
		{
			animator.Play("LuckyDrawNewHammer_Anim");
		}
	}

	private void OnLuckyDrawUnacknowledgedHammerPopupDismissed()
	{
		m_luckyDrawManager.AcknowledgeAllHammers();
		StartCoroutine(PlayLuckyDrawButtonFanfareFX());
	}

	private void OnPartyChanged(PartyManager.PartyInviteEvent inviteEvent, BnetGameAccountId playerGameAccountId, PartyManager.PartyData data, object userData)
	{
		if (inviteEvent == PartyManager.PartyInviteEvent.I_CREATED_PARTY || inviteEvent == PartyManager.PartyInviteEvent.FRIEND_RECEIVED_INVITE)
		{
			PartyManager.Get().SetReadyStatus(ready: true);
		}
		UpdatePlayButtonBasedOnPartyInfo();
		UpdateDuosToggleButtonBasedOnPartyInfo();
		UpdateDuosPartyPlanner();
		m_baconLobbyTutorial.StartArrangePartyTutorial();
	}

	private void OnBattlegroundsGameModeChanged(string gameMode, bool showPartyUI, object userData)
	{
		if (m_statFrameDataModel != null)
		{
			m_statFrameDataModel.GameType = BaconLobbyMgr.Get().GetBattlegroundsGameModeType();
		}
		BaconLobbyDataModel dataModel = GetBaconLobbyDataModel();
		if (dataModel != null)
		{
			dataModel.BattlegroundsInDuosMode = BaconLobbyMgr.Get().GetBattlegroundsGameMode() == "duos";
			dataModel.Rating = BaconLobbyMgr.Get().GetBattlegroundsActiveGameModeRating();
		}
		UpdateDuosToggleButtonBasedOnPartyInfo();
	}

	private void OnMemberAttributeChanged(BnetGameAccountId playerGameAccountId, Blizzard.GameService.Protocol.V2.Client.Attribute attribute, object userData)
	{
		if (playerGameAccountId == BnetPresenceMgr.Get().GetMyGameAccountId())
		{
			UpdateDuosSelectedSlotBasedOnPartyInfo();
		}
	}

	private void OnPresenceUpdated(BnetPlayerChangelist changelist, object userData)
	{
		UpdatePlayButtonBasedOnPartyInfo();
	}

	private void OnNearbyPlayersUpdated(BnetRecentOrNearbyPlayerChangelist changelist, object userData)
	{
		UpdatePlayButtonBasedOnPartyInfo();
	}

	private void UpdateDuosSelectedSlotBasedOnPartyInfo()
	{
		int selectedSlot = PartyManager.Get().GetPlayerDuosSlot(BnetPresenceMgr.Get().GetMyGameAccountId());
		foreach (KeyValuePair<int, Widget> slotWidget in m_duosSlotWidgets)
		{
			if (slotWidget.Value == null)
			{
				continue;
			}
			VisualController[] components = slotWidget.Value.GetComponents<VisualController>();
			foreach (VisualController vc in components)
			{
				if (vc.Label == "SELECTED")
				{
					vc.SetState((selectedSlot == slotWidget.Key) ? "SELECTED" : "DEFAULT");
				}
			}
		}
	}

	private void UpdateDuosToggleButtonBasedOnPartyInfo()
	{
		if (m_toggleWidget == null || m_lobbyWidget == null)
		{
			return;
		}
		bool isInParty = PartyManager.Get().IsInBattlegroundsParty();
		bool isLeader = PartyManager.Get().IsPartyLeader();
		string currentState = m_toggleWidget.gameObject.GetComponent<VisualController>().State;
		string desiredState = ((!isInParty || isLeader) ? "REGULAR" : "DISABLED_REGULAR");
		if (BaconLobbyMgr.Get().IsInDuosMode())
		{
			desiredState = ((!isInParty || isLeader) ? "DUOS" : "DISABLED_DUOS");
		}
		if (desiredState != currentState)
		{
			m_toggleWidget.gameObject.GetComponent<VisualController>().SetState(desiredState);
			m_lobbyWidget.TriggerEvent(BaconLobbyMgr.Get().IsInDuosMode() ? "SetModeDuos" : "SetModeSolo");
			Box box = Box.Get();
			SceneMgr sceneMgr = SceneMgr.Get();
			if (!box.IsBusy() && !box.IsTransitioningToSceneMode() && !sceneMgr.IsTransitioning())
			{
				m_lobbyWidget.TriggerEvent("PLAY_MODE_SWITCH_FX");
			}
		}
	}

	private void UpdateDuosPartyPlanner()
	{
		if (!PartyManager.Get().IsInBattlegroundsParty() && m_baconParty != null)
		{
			m_baconParty.ClearPartyData();
		}
	}

	private void UpdatePlayButtonBasedOnPartyInfo()
	{
		if (m_playButton == null)
		{
			return;
		}
		string buttonText = GameStrings.Get("GLOBAL_PLAY");
		if (PartyManager.Get().IsInBattlegroundsParty())
		{
			if (!PartyManager.Get().IsPartyLeader())
			{
				buttonText = GameStrings.Get("GLOBAL_PLAY_WAITING");
			}
			else if (!PartyManager.Get().IsPartySizeValidForCurrentGameMode())
			{
				buttonText = GameStrings.Get("GLUE_BACON_INCOMPLETE_TEAM");
			}
		}
		m_playButton.SetText(buttonText);
		int readyCount = PartyManager.Get().GetReadyPartyMemberCount();
		int partySize = PartyManager.Get().GetCurrentPartySize();
		string buttonSecondaryText = "";
		string buttonTertiaryText = "";
		if (PartyManager.Get().IsInBattlegroundsParty() && PartyManager.Get().IsPartyLeader() && !GameMgr.Get().IsFindingGame() && readyCount < partySize)
		{
			buttonSecondaryText = $"{readyCount}/{partySize}";
		}
		if (PartyManager.Get().IsInBattlegroundsParty() && PartyManager.Get().IsPartyLeader() && PartyManager.Get().IsPartySizeValidForCurrentGameMode())
		{
			buttonTertiaryText = ((partySize > PartyManager.Get().GetBattlegroundsMaxRankedPartySize()) ? GameStrings.Get("GLUE_BACON_PRIVATE_GAME") : GameStrings.Get("GLUE_BACON_RANKED_GAME"));
		}
		m_playButton.SetSecondaryText(buttonSecondaryText);
		m_playButton.SetTertiaryText(buttonTertiaryText);
		if (PartyManager.Get().IsInBattlegroundsParty() && (!PartyManager.Get().IsPartyLeader() || readyCount < partySize || !PartyManager.Get().IsPartySizeValidForCurrentGameMode()))
		{
			m_playButton.Disable(keepLabelTextVisible: true);
		}
		else
		{
			m_playButton.Enable();
		}
	}

	private void OnStoreStatusChanged(bool isOpen)
	{
		GetBaconLobbyDataModel().ShopOpen = isOpen;
	}

	private void ShowBattlegroundsStoreEmptyPopup()
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_BACON_SHOP_EMPTY_HEADER"),
			m_text = GameStrings.Get("GLUE_BACON_SHOP_EMPTY_BODY"),
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.OK
		};
		DialogManager.Get().ShowPopup(info);
	}

	protected override bool ShouldStartShown()
	{
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.LUCKY_DRAW)
		{
			return SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.LUCKY_DRAW;
		}
		return false;
	}

	public override bool IsFinishedLoading(out string failureMessage)
	{
		if (!m_playButtonFinishedLoading)
		{
			failureMessage = "BaconDisplay - Play button never finished loading";
			return false;
		}
		if (!m_backButtonFinishedLoading)
		{
			failureMessage = "BaconDisplay - Back button never finished loading";
			return false;
		}
		if (!m_statsButtonFinishedLoading)
		{
			failureMessage = "BaconDisplay - Stats button never finished loading";
			return false;
		}
		if (!m_luckyDrawButtonFinishedLoading)
		{
			failureMessage = "BaconDisplay - Lucky draw button never finished loading";
			return false;
		}
		if (!m_lobbyFinishedLoading)
		{
			failureMessage = "BaconDisplay - Lobby PC not finished loading";
			return false;
		}
		if (m_OwningWidget.IsChangingStates)
		{
			failureMessage = "BaconDisplay - owning widget is still transitioning";
			return false;
		}
		GetBaconLobbyDataModel().FullyLoaded = true;
		m_baconParty.EnableTrayMask();
		failureMessage = string.Empty;
		return true;
	}

	private void InitSlidingTray()
	{
		if (m_slidingTray == null)
		{
			Error.AddDevWarning("UI Error", "Warning [BaconDisplay] InitSlidingTray() reference to the sliding tray is missing! This may lead to an improper layout.");
		}
		else if (PlatformSettings.IsMobile() && (bool)UniversalInputManager.UsePhoneUI)
		{
			m_slidingTray.m_trayHiddenBone = m_OffScreenBoneMobile;
			m_slidingTray.m_trayShownBone = m_OnScreenBoneMobile;
		}
		else
		{
			m_slidingTray.m_trayHiddenBone = m_OffScreenBonePC;
			m_slidingTray.m_trayShownBone = m_OnScreenBonePC;
		}
	}
}
