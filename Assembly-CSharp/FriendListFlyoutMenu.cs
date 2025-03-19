using System;
using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Jobs;
using Hearthstone.Core;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class FriendListFlyoutMenu : MonoBehaviour
{
	public delegate void ShowTooltipEvent(UIBButton button, TooltipZone tooltipZone);

	public delegate void ButtonOverride(Widget widget);

	public class ButtonEvent
	{
		public bool isEnabled;

		public string eventName { get; }

		public Widget buttonWidget { get; }

		public UIEvent.Handler onRelease { get; }

		public ShowTooltipEvent onHover { get; }

		public ButtonOverride onOverride { get; }

		public ButtonEvent(string eventName, Widget buttonWidget, UIEvent.Handler onRelease, ShowTooltipEvent onHover = null, ButtonOverride onOverride = null)
		{
			this.eventName = eventName;
			this.buttonWidget = buttonWidget;
			this.onRelease = onRelease;
			this.onHover = onHover;
			this.onOverride = onOverride;
		}
	}

	public enum ButtonOption
	{
		Invalid = -1,
		Hearthstone,
		Battlegrounds,
		Mercenaries,
		Spectate,
		InviteToSpectate,
		KickSpectator,
		StopSpectating,
		InviteToParty,
		KickFromParty,
		AddFriend,
		Report,
		Options,
		StandardHearthstone,
		WildHearthstone,
		ClassicHearthstone,
		TwistHearthstone,
		TavernBrawl
	}

	public const string EnabledEvent = "ENABLED";

	public const string DisabledEvent = "DISABLED";

	public const string ChallengeFriendEvent = "IS_FRIEND";

	public const string ChallengeStrangerEvent = "IS_STRANGER";

	public const string CloseMenuEvent = "CODE_FLYOUT_MENU_DISMISSED";

	public const string GrowPopupEvent = "GROW";

	public const string ShrinkPopupEvent = "SHRINK";

	public static readonly Vector3 PopupOffset = new Vector3(0f, 25f, 0f);

	private const float FullscreenFxTime = 0.25f;

	private Dictionary<ButtonOption, ButtonEvent> m_buttonEvents;

	private static readonly HashSet<ButtonOption> m_sectionedButtons = new HashSet<ButtonOption>
	{
		ButtonOption.AddFriend,
		ButtonOption.Options,
		ButtonOption.Report
	};

	[SerializeField]
	private MultiSliceElement m_frameContainer;

	[SerializeField]
	private MultiSliceElement m_shadowContainer;

	[SerializeField]
	private MultiSliceElement m_menuList;

	[SerializeField]
	private MultiSliceElement m_middleFrame;

	[SerializeField]
	private GameObject m_middleShadow;

	[SerializeField]
	private GameObject m_challengeTitle;

	[SerializeField]
	private GameObject m_sectionDivider;

	[SerializeField]
	private Widget m_hearthstoneChallengePopupWidget;

	[SerializeField]
	private Widget m_reportingPopupWidget;

	[SerializeField]
	private Widget m_optionsPopupWidget;

	[SerializeField]
	private GameObject m_menuButtons;

	[SerializeField]
	private Widget m_hearthstoneButton;

	[SerializeField]
	private Widget m_battlegroundsButton;

	[SerializeField]
	private Widget m_mercenariesButton;

	[SerializeField]
	private Widget m_spectateButton;

	[SerializeField]
	private Widget m_inviteToSpectateButton;

	[SerializeField]
	private Widget m_kickSpectatorButton;

	[SerializeField]
	private Widget m_stopSpectatingButton;

	[SerializeField]
	private Widget m_inviteToPartyButton;

	[SerializeField]
	private Widget m_kickFromPartyButton;

	[SerializeField]
	private Widget m_addFriendButton;

	[SerializeField]
	private Widget m_reportButton;

	[SerializeField]
	private Widget m_optionsButton;

	private Widget m_widget;

	private BnetPlayer m_player;

	private FriendListFriendFrame m_friendListFrame;

	private HearthstoneChallengePopup m_hearthstoneChallengePopup;

	private ReportingPopup m_reportingPopup;

	private FriendsListOptionsPopup m_optionsPopup;

	private List<IJobDependency> m_buttonDependencies;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private void Awake()
	{
		m_widget = base.gameObject.GetComponent<WidgetTemplate>();
		m_friendListFrame = GetComponentInParent<FriendListFriendFrame>();
		if (m_friendListFrame != null)
		{
			m_player = m_friendListFrame.GetFriend();
			m_friendListFrame.OnFriendChanged += UpdatePlayer;
		}
		InitializeButtonEvents();
		InitializeHearthstoneChallengePopup();
		InitializeReportingPopup();
		InitializeOptionsPopup();
		InitializeFlyoutMenu();
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void OnEnable()
	{
		if (m_player != null)
		{
			UpdateFlyoutMenu();
		}
	}

	private void OnDestroy()
	{
		DismissPopups(showAlert: false);
		if (m_hearthstoneChallengePopupWidget != null)
		{
			UnityEngine.Object.Destroy(m_hearthstoneChallengePopupWidget.gameObject);
		}
		if (m_optionsPopupWidget != null)
		{
			UnityEngine.Object.Destroy(m_optionsPopupWidget.gameObject);
		}
		if (m_reportingPopupWidget != null)
		{
			UnityEngine.Object.Destroy(m_reportingPopupWidget.gameObject);
		}
		if (m_friendListFrame != null)
		{
			m_friendListFrame.OnFriendChanged -= UpdatePlayer;
		}
	}

	private void UpdatePlayer(BnetPlayer player)
	{
		m_player = player;
	}

	private void InitializeButtonEvents()
	{
		m_buttonEvents = new Dictionary<ButtonOption, ButtonEvent>
		{
			{
				ButtonOption.Hearthstone,
				new ButtonEvent("HEARTHSTONE", m_hearthstoneButton, OnHearthstoneButtonReleased, OnHearthstoneButtonOver, OnHearthstoneButtonOverride)
			},
			{
				ButtonOption.Battlegrounds,
				new ButtonEvent("BATTLEGROUNDS", m_battlegroundsButton, OnBattlegroundsButtonReleased, OnBattlegroundsButtonOver)
			},
			{
				ButtonOption.Mercenaries,
				new ButtonEvent("MERCENARIES", m_mercenariesButton, OnMercenariesButtonReleased, OnMercenariesButtonOver)
			},
			{
				ButtonOption.Spectate,
				new ButtonEvent("SPECTATE", m_spectateButton, OnSpectateButtonReleased, OnSpectateButtonOver)
			},
			{
				ButtonOption.InviteToSpectate,
				new ButtonEvent("SPECTATE_INVITE", m_inviteToSpectateButton, OnInviteToSpectateButtonReleased, OnInviteToSpectateButtonOver)
			},
			{
				ButtonOption.KickSpectator,
				new ButtonEvent("SPECTATE_KICK", m_kickSpectatorButton, OnKickSpectatorButtonReleased, OnKickSpectatorButtonOver)
			},
			{
				ButtonOption.StopSpectating,
				new ButtonEvent("SPECTATE_STOP", m_stopSpectatingButton, OnStopSpectatingButtonReleased, OnStopSpectatingButtonOver)
			},
			{
				ButtonOption.InviteToParty,
				new ButtonEvent("PARTY_INVITE", m_inviteToPartyButton, OnInviteToPartyButtonReleased, OnInviteToPartyButtonOver)
			},
			{
				ButtonOption.KickFromParty,
				new ButtonEvent("PARTY_KICK", m_kickFromPartyButton, OnKickFromPartyButtonReleased, OnKickFromPartyButtonOver)
			},
			{
				ButtonOption.AddFriend,
				new ButtonEvent("ADD_FRIEND", m_addFriendButton, OnAddFriendButtonReleased)
			},
			{
				ButtonOption.Options,
				new ButtonEvent("OPTIONS", m_optionsButton, OnOptionsButtonReleased)
			},
			{
				ButtonOption.Report,
				new ButtonEvent("REPORT", m_reportButton, OnReportButtonReleased, OnReportButtonOver)
			}
		};
	}

	private void InitializeButton(ButtonOption buttonOption)
	{
		if (!m_buttonEvents.TryGetValue(buttonOption, out var buttonEvent) || !(buttonEvent.buttonWidget != null))
		{
			return;
		}
		Widget buttonWidget = buttonEvent.buttonWidget;
		buttonWidget.RegisterReadyListener(delegate
		{
			buttonEvent.buttonWidget.TriggerEvent(buttonEvent.eventName);
			buttonEvent.onOverride?.Invoke(buttonWidget);
			UIBButton uibButton = buttonWidget.GetComponentInChildren<UIBButton>(includeInactive: true);
			if (buttonEvent.onRelease != null)
			{
				uibButton.AddEventListener(UIEventType.RELEASE, delegate(UIEvent e)
				{
					if (buttonEvent.isEnabled)
					{
						buttonEvent.onRelease(e);
					}
				});
			}
			if (buttonEvent.onHover != null)
			{
				TooltipZone tooltipZone = buttonWidget.GetComponentInChildren<TooltipZone>(includeInactive: true);
				uibButton.AddEventListener(UIEventType.ROLLOVER, delegate
				{
					buttonEvent.onHover(uibButton, tooltipZone);
				});
				uibButton.AddEventListener(UIEventType.ROLLOUT, delegate
				{
					tooltipZone.HideTooltip();
				});
				m_menuList.m_ignore.Add(tooltipZone.gameObject);
				m_frameContainer.m_ignore.Add(tooltipZone.gameObject);
				m_shadowContainer.m_ignore.Add(tooltipZone.gameObject);
				m_middleFrame.m_ignore.Add(tooltipZone.gameObject);
			}
		});
	}

	private void InitializeFlyoutMenu()
	{
		m_buttonDependencies = new List<IJobDependency>();
		foreach (ButtonOption option in Enum.GetValues(typeof(ButtonOption)))
		{
			InitializeButton(option);
		}
	}

	public void UpdateFlyoutMenu()
	{
		m_buttonDependencies = new List<IJobDependency>();
		List<GameObject> topSectionButtons = new List<GameObject>();
		List<GameObject> bottomSectionButtons = new List<GameObject>();
		bool showChallengeHeader = false;
		foreach (ButtonOption option in Enum.GetValues(typeof(ButtonOption)))
		{
			bool shouldShowOption = ShouldShowOption(option);
			m_buttonEvents.TryGetValue(option, out var buttonEvent);
			if (buttonEvent == null || !(buttonEvent.buttonWidget != null))
			{
				continue;
			}
			Widget buttonWidget = buttonEvent.buttonWidget;
			if (shouldShowOption)
			{
				if (!buttonWidget.gameObject.activeSelf)
				{
					buttonWidget.gameObject.transform.position = new Vector3(-9999f, -9999f);
				}
				buttonWidget.gameObject.SetActive(value: true);
				m_buttonDependencies.Add(new WaitForWidget(buttonWidget));
				if (ShouldEnableOption(option))
				{
					buttonWidget.TriggerEvent("ENABLED");
					buttonEvent.isEnabled = true;
				}
				else
				{
					buttonWidget.TriggerEvent("DISABLED");
					buttonEvent.isEnabled = false;
				}
				if (m_sectionedButtons.Contains(option))
				{
					bottomSectionButtons.Add(buttonWidget.gameObject);
				}
				else
				{
					topSectionButtons.Add(buttonWidget.gameObject);
				}
				if (option == ButtonOption.Hearthstone || option == ButtonOption.Battlegrounds || option == ButtonOption.Mercenaries)
				{
					showChallengeHeader = true;
				}
			}
			else
			{
				buttonWidget.gameObject.SetActive(value: false);
			}
		}
		Processor.QueueJob("FriendListChallengeMenu.FormatFlyoutMenu", Job_FormatFlyoutMenu(topSectionButtons, bottomSectionButtons, showChallengeHeader), m_buttonDependencies.ToArray());
	}

	private void InitializeHearthstoneChallengePopup()
	{
		OverlayUI.Get().AddGameObject(m_hearthstoneChallengePopupWidget.gameObject);
		m_hearthstoneChallengePopupWidget.transform.position += PopupOffset;
		m_hearthstoneChallengePopupWidget.RegisterReadyListener(delegate
		{
			m_hearthstoneChallengePopup = m_hearthstoneChallengePopupWidget.GetComponentInChildren<HearthstoneChallengePopup>(includeInactive: true);
			m_hearthstoneChallengePopup.Init(m_player, m_friendListFrame, this);
			m_hearthstoneChallengePopupWidget.RegisterEventListener(delegate(string eventName)
			{
				if (eventName == "DISMISS_POPUP")
				{
					HidePopup(m_hearthstoneChallengePopupWidget);
				}
			});
		});
	}

	private void InitializeReportingPopup()
	{
		OverlayUI.Get().AddGameObject(m_reportingPopupWidget.gameObject);
		m_reportingPopupWidget.transform.position += PopupOffset;
		Vector3 popupPosition = m_reportingPopupWidget.transform.position;
		m_reportingPopupWidget.transform.position = Vector3.zero;
		m_reportingPopupWidget.RegisterReadyListener(delegate
		{
			m_reportingPopupWidget.transform.position = popupPosition;
			m_reportingPopup = m_reportingPopupWidget.GetComponentInChildren<ReportingPopup>(includeInactive: true);
			m_reportingPopup.Init(m_player);
		});
		m_reportingPopupWidget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "DISMISS_POPUP")
			{
				HidePopup(m_reportingPopupWidget);
			}
		});
	}

	private void InitializeOptionsPopup()
	{
		OverlayUI.Get().AddGameObject(m_optionsPopupWidget.gameObject);
		m_optionsPopupWidget.transform.position += PopupOffset;
		m_optionsPopupWidget.RegisterReadyListener(delegate
		{
			m_optionsPopup = m_optionsPopupWidget.GetComponentInChildren<FriendsListOptionsPopup>(includeInactive: true);
			m_optionsPopup.Init(m_player, this);
		});
		m_optionsPopupWidget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "DISMISS_POPUP")
			{
				HidePopup(m_optionsPopupWidget);
			}
		});
	}

	private void ShowPopup(Widget popupWidget)
	{
		if (!popupWidget.gameObject.activeInHierarchy)
		{
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			screenEffectParameters.Time = 0.25f;
			m_screenEffectsHandle.StartEffect(screenEffectParameters);
		}
		popupWidget.gameObject.SetActive(value: true);
		popupWidget.TriggerEvent("GROW");
	}

	private void HidePopup(Widget popupWidget)
	{
		if (!(popupWidget != null))
		{
			return;
		}
		if (popupWidget.gameObject.activeInHierarchy)
		{
			m_screenEffectsHandle.StopEffect(delegate
			{
				if (popupWidget != null)
				{
					popupWidget.gameObject.SetActive(value: false);
				}
			});
		}
		popupWidget.TriggerEvent("SHRINK");
	}

	private bool ShouldShowOption(ButtonOption option)
	{
		bool isFriend = BnetFriendMgr.Get().IsFriend(m_player);
		switch (option)
		{
		case ButtonOption.AddFriend:
			return !isFriend;
		case ButtonOption.Options:
			return isFriend;
		case ButtonOption.Report:
			return !isFriend;
		default:
		{
			BnetGameAccountId targetGameAccountId = m_player.GetHearthstoneGameAccountId();
			switch (option)
			{
			case ButtonOption.Spectate:
				return SpectatorManager.Get().CanSpectate(m_player);
			case ButtonOption.StopSpectating:
				return SpectatorManager.Get().IsSpectatingPlayer(targetGameAccountId);
			case ButtonOption.InviteToSpectate:
				if (!SpectatorManager.Get().CanInviteToSpectateMyGame(targetGameAccountId))
				{
					return SpectatorManager.Get().IsInvitedToSpectateMyGame(targetGameAccountId);
				}
				return true;
			case ButtonOption.KickSpectator:
				return SpectatorManager.Get().IsSpectatingMe(targetGameAccountId);
			default:
			{
				bool isInBattlegroundsParty = PartyManager.Get().IsInBattlegroundsParty() && !SceneMgr.Get().IsInGame() && !GameMgr.Get().IsFindingGame();
				switch (option)
				{
				case ButtonOption.InviteToParty:
					if (isInBattlegroundsParty)
					{
						return !PartyManager.Get().IsPlayerInCurrentPartyOrPending(targetGameAccountId);
					}
					return false;
				case ButtonOption.KickFromParty:
					if (isInBattlegroundsParty)
					{
						return PartyManager.Get().CanKick(m_player.GetBestGameAccountId());
					}
					return false;
				default:
				{
					bool canShowFriendlyChallenge = FriendChallengeMgr.Get().CanShowFriendlyChallenge(m_player);
					return option switch
					{
						ButtonOption.Hearthstone => canShowFriendlyChallenge, 
						ButtonOption.Battlegrounds => canShowFriendlyChallenge, 
						ButtonOption.Mercenaries => canShowFriendlyChallenge, 
						_ => false, 
					};
				}
				}
			}
			}
		}
		}
	}

	private bool ShouldEnableOption(ButtonOption option)
	{
		switch (option)
		{
		case ButtonOption.Spectate:
		case ButtonOption.InviteToSpectate:
		case ButtonOption.KickSpectator:
		case ButtonOption.StopSpectating:
		case ButtonOption.Options:
			return true;
		case ButtonOption.InviteToParty:
			if (FriendChallengeMgr.Get().IsBattlegroundsFriendlyChallengeAvailable(m_player))
			{
				return PartyManager.Get().CanInvite(m_player.GetBestGameAccountId());
			}
			return false;
		case ButtonOption.KickFromParty:
			return true;
		case ButtonOption.Hearthstone:
			if (ShouldSeeHearthstoneChallengePopup())
			{
				return FriendChallengeMgr.Get().IsHearthstoneFriendlyChallengeAvailable(m_player);
			}
			if (FriendChallengeMgr.Get().IsHearthstoneFriendlyChallengeAvailable(m_player))
			{
				return CollectionManager.Get().AccountHasValidDeck(FormatType.FT_STANDARD);
			}
			return false;
		case ButtonOption.Battlegrounds:
			return FriendChallengeMgr.Get().IsBattlegroundsFriendlyChallengeAvailable(m_player);
		case ButtonOption.Mercenaries:
			return FriendChallengeMgr.Get().IsMercenariesFriendlyChallengeAvailable(m_player);
		case ButtonOption.Report:
			return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>()?.ReportPlayerEnabled ?? false;
		case ButtonOption.AddFriend:
			return !BnetRecentPlayerMgr.Get().IsCurrentOpponent(m_player);
		default:
			return false;
		}
	}

	private IEnumerator<IAsyncJobResult> Job_FormatFlyoutMenu(List<GameObject> topSectionButtons, List<GameObject> bottomSectionButtons, bool showChallengeHeader)
	{
		if (!(this != null))
		{
			yield break;
		}
		m_menuList.ClearSlices();
		if (showChallengeHeader)
		{
			m_challengeTitle.SetActive(value: true);
			m_menuList.AddSlice(m_challengeTitle, default(Vector3), default(Vector3), reverse: false, useMeshRenderers: true, useWidgetTransform: false, useTextSize: true);
			if (BnetFriendMgr.Get().IsFriend(m_player))
			{
				m_widget.TriggerEvent("IS_FRIEND");
			}
			else
			{
				m_widget.TriggerEvent("IS_STRANGER");
			}
		}
		else
		{
			m_challengeTitle.SetActive(value: false);
		}
		foreach (GameObject activeMenuItem in topSectionButtons)
		{
			GameUtils.SetParent(activeMenuItem, m_menuButtons);
			m_menuList.AddSlice(activeMenuItem, default(Vector3), default(Vector3), reverse: false, useMeshRenderers: true, useWidgetTransform: false, useTextSize: true);
		}
		if (topSectionButtons.Count > 0 && bottomSectionButtons.Count > 0)
		{
			m_sectionDivider.SetActive(value: true);
			m_menuList.AddSlice(m_sectionDivider);
		}
		else
		{
			m_sectionDivider.SetActive(value: false);
		}
		foreach (GameObject activeMenuItem2 in bottomSectionButtons)
		{
			GameUtils.SetParent(activeMenuItem2, m_menuButtons);
			m_menuList.AddSlice(activeMenuItem2);
		}
		m_menuList.UpdateSlices();
		GameObject go = m_menuList.gameObject;
		List<GameObject> ignore = m_menuList.m_ignore;
		float menuHeight = TransformUtil.ComputeOrientedWorldBounds(go, default(Vector3), default(Vector3), ignore, includeAllChildren: true, includeMeshRenderers: true, includeWidgetTransformBounds: false, includeUberTextSize: true).Extents[1].magnitude * 2f;
		TransformUtil.SetLocalScaleToWorldDimension(m_middleFrame.gameObject, new WorldDimensionIndex(menuHeight, 1));
		TransformUtil.SetLocalScaleToWorldDimension(m_middleShadow, new WorldDimensionIndex(menuHeight, 1));
		m_frameContainer.UpdateSlices();
		m_shadowContainer.UpdateSlices();
		m_middleFrame.UpdateSlices();
		yield return null;
	}

	public static void ShowTooltip(BnetPlayer player, string headerKey, string descriptionFormat, TooltipZone tooltipZone, UIBButton button)
	{
		if (UniversalInputManager.Get().IsTouchMode())
		{
			if (GameStrings.HasKey(headerKey + "_TOUCH"))
			{
				headerKey += "_TOUCH";
			}
			if (GameStrings.HasKey(descriptionFormat + "_TOUCH"))
			{
				descriptionFormat += "_TOUCH";
			}
		}
		string header = GameStrings.Get(headerKey);
		string description = GameStrings.Format(descriptionFormat, player.GetBestName());
		tooltipZone.ShowSocialTooltip(button, header, description, 18.75f, GameLayer.BattleNetDialog);
		tooltipZone.AnchorTooltipTo(button.gameObject, Anchor.TOP_RIGHT_XZ, Anchor.TOP_LEFT_XZ);
	}

	public static bool GetAvailability(BnetPlayer player, out string reason)
	{
		if (!FriendChallengeMgr.Get().AmIAvailable())
		{
			if (BnetPresenceMgr.Get().GetMyPlayer().IsAppearingOffline())
			{
				reason = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_IM_APPEARING_OFFLINE";
			}
			else if (PartyManager.Get().IsInBattlegroundsParty() && !PartyManager.Get().IsPartyLeader())
			{
				reason = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_BATTLEGROUNDS_PARTY_MEMBER";
			}
			else
			{
				reason = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_IM_UNAVAILABLE";
			}
			return false;
		}
		if (!FriendChallengeMgr.Get().IsOpponentAvailable(player))
		{
			reason = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_THEYRE_UNAVAILABLE";
			return false;
		}
		reason = string.Empty;
		return true;
	}

	public void ShowReportingPopup()
	{
		ShowPopup(m_reportingPopupWidget);
		if (m_reportingPopup != null)
		{
			m_reportingPopup.Init(m_player);
		}
		m_friendListFrame.CloseChallengeMenu();
	}

	public void DismissPopups(bool showAlert)
	{
		if (m_hearthstoneChallengePopupWidget != null)
		{
			if (showAlert && m_hearthstoneChallengePopupWidget.gameObject.activeInHierarchy)
			{
				ShowPlayerOfflineAlert();
			}
			HidePopup(m_hearthstoneChallengePopupWidget);
			m_hearthstoneChallengePopupWidget.gameObject.SetActive(value: false);
		}
		if (m_optionsPopupWidget != null)
		{
			HidePopup(m_optionsPopupWidget);
			m_optionsPopupWidget.gameObject.SetActive(value: false);
		}
		if (m_reportingPopupWidget != null)
		{
			HidePopup(m_reportingPopupWidget);
			m_reportingPopupWidget.gameObject.SetActive(value: false);
		}
	}

	public void SendHearthstoneFriendlyChallenge()
	{
		if (!CollectionManager.Get().AccountHasValidDeck(FormatType.FT_STANDARD))
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER"),
				m_text = GameStrings.Format("GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGER_NO_STANDARD_DECK"),
				m_showAlertIcon = true,
				m_responseDisplay = AlertPopup.ResponseDisplay.OK
			};
			DialogManager.Get().ShowPopup(info);
			m_friendListFrame.CloseChallengeMenu();
		}
		else if (!FriendChallengeMgr.Get().IsOpponentAvailable(m_player))
		{
			ShowOpponentUnavailableAlert();
			m_friendListFrame.CloseFriendsListMenu();
		}
		else
		{
			FriendChallengeMgr.Get().SetChallengeMethod(FriendChallengeMgr.ChallengeMethod.FROM_FRIEND_LIST);
			FriendChallengeMgr.Get().SendChallenge(m_player, FormatType.FT_STANDARD, enableDeckShare: true);
			m_friendListFrame.CloseFriendsListMenu();
		}
	}

	public void ShowOpponentUnavailableAlert()
	{
		AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
		popupInfo.m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER");
		popupInfo.m_text = GameStrings.Format("GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_THEYRE_UNAVAILABLE", m_player.GetBestName());
		popupInfo.m_showAlertIcon = true;
		popupInfo.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		AlertPopup.PopupInfo info = popupInfo;
		DialogManager.Get().ShowPopup(info);
	}

	private void NavigateToSceneForPartyChallenge(SceneMgr.Mode nextMode)
	{
		GameMgr.Get().SetPendingAutoConcede(pendingAutoConcede: true);
		if (CollectionManager.Get().IsInEditMode())
		{
			CollectionManager.Get().GetEditedDeck()?.SendChanges(CollectionDeck.ChangeSource.NavigateToSceneForPartyChallenge);
		}
		SceneMgr.Get().SetNextMode(nextMode);
	}

	private void ShowPlayerOfflineAlert()
	{
		AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
		popupInfo.m_headerText = GameStrings.Get("GLOBAL_DEFAULT_ALERT_HEADER");
		popupInfo.m_text = GameStrings.Format("GLOBAL_SOCIAL_ALERT_FRIEND_OFFLINE", m_player.GetBattleTag());
		popupInfo.m_showAlertIcon = true;
		popupInfo.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		AlertPopup.PopupInfo info = popupInfo;
		DialogManager.Get().ShowPopup(info);
	}

	private bool ShouldSeeHearthstoneChallengePopup()
	{
		if (!CollectionManager.Get().ShouldAccountSeeStandardWild())
		{
			return TavernBrawlManager.Get().HasUnlockedTavernBrawl(BrawlType.BRAWL_TYPE_TAVERN_BRAWL);
		}
		return true;
	}

	private void ShowBattlegroundsPrivatePartyDialog()
	{
		AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
		popupInfo.m_headerText = GameStrings.Get("GLUE_BACON_PRIVATE_PARTY_TITLE");
		popupInfo.m_text = GameStrings.Format("GLUE_BACON_PRIVATE_PARTY_WARNING", PartyManager.Get().GetBattlegroundsMaxRankedPartySize());
		popupInfo.m_iconSet = AlertPopup.PopupInfo.IconSet.Default;
		popupInfo.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		popupInfo.m_confirmText = GameStrings.Get("GLUE_COLLECTION_DECK_COMPLETE_POPUP_CONFIRM");
		popupInfo.m_cancelText = GameStrings.Get("GLUE_COLLECTION_DECK_COMPLETE_POPUP_CANCEL");
		popupInfo.m_responseCallback = delegate(AlertPopup.Response response, object userData)
		{
			if (response == AlertPopup.Response.CONFIRM)
			{
				PartyManager.Get().SetBattlegroundsPrivateParty(isPrivate: true);
				PartyManager.Get().SendInvite(PartyType.BATTLEGROUNDS_PARTY, m_player.GetBestGameAccountId());
			}
		};
		AlertPopup.PopupInfo info = popupInfo;
		DialogManager.Get().ShowPopup(info);
	}

	private void OnHearthstoneButtonReleased(UIEvent e)
	{
		if (ShouldSeeHearthstoneChallengePopup())
		{
			ShowPopup(m_hearthstoneChallengePopupWidget);
			if (m_hearthstoneChallengePopup != null)
			{
				m_hearthstoneChallengePopup.Init(m_player, m_friendListFrame, this);
			}
			m_friendListFrame.CloseChallengeMenu();
		}
		else
		{
			SendHearthstoneFriendlyChallenge();
		}
	}

	private void OnBattlegroundsButtonReleased(UIEvent e)
	{
		NavigateToSceneForPartyChallenge(SceneMgr.Mode.BACON);
		PartyManager.Get().SendInvite(PartyType.BATTLEGROUNDS_PARTY, m_player.GetBestGameAccountId());
		m_friendListFrame.CloseChallengeMenu();
	}

	private void OnMercenariesButtonReleased(UIEvent e)
	{
		PartyManager.Get().StartMercenariesFriendlyChallengeEntry(m_player);
		m_friendListFrame.CloseChallengeMenu();
		m_friendListFrame.CloseFriendsListMenu();
	}

	private void OnSpectateButtonReleased(UIEvent e)
	{
		SpectatorManager.Get().SpectatePlayer(m_player);
		m_friendListFrame.CloseChallengeMenu();
	}

	private void OnInviteToSpectateButtonReleased(UIEvent e)
	{
		SpectatorManager.Get().InviteToSpectateMe(m_player);
	}

	private void OnKickSpectatorButtonReleased(UIEvent e)
	{
		BnetGameAccountId gameAccountId = m_player.GetHearthstoneGameAccountId();
		if (!SpectatorManager.Get().IsSpectatingMe(gameAccountId))
		{
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_SPECTATOR_KICK_PROMPT_HEADER");
		info.m_text = GameStrings.Format("GLOBAL_SPECTATOR_KICK_PROMPT_TEXT", FriendUtils.GetUniqueName(m_player));
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_responseCallback = delegate(AlertPopup.Response response, object userData)
		{
			BnetPlayer player = (BnetPlayer)userData;
			if (response == AlertPopup.Response.CONFIRM)
			{
				SpectatorManager.Get().KickSpectator(player, regenerateSpectatorPassword: true);
			}
		};
		info.m_responseUserData = m_player;
		DialogManager.DialogProcessCallback processCallback = delegate(DialogBase dialog, object userData)
		{
			BnetGameAccountId gameAccountId2 = (BnetGameAccountId)userData;
			return SpectatorManager.Get().IsSpectatingMe(gameAccountId2) ? true : false;
		};
		DialogManager.Get().ShowPopup(info, processCallback, gameAccountId);
	}

	private void OnStopSpectatingButtonReleased(UIEvent e)
	{
		BnetGameAccountId gameAccountId = m_player.GetHearthstoneGameAccountId();
		SpectatorManager spectator = SpectatorManager.Get();
		if (GameMgr.Get().IsFindingGame() || SceneMgr.Get().IsTransitioning() || GameMgr.Get().IsTransitionPopupShown())
		{
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLOBAL_SPECTATOR_LEAVE_PROMPT_HEADER");
		info.m_text = GameStrings.Get("GLOBAL_SPECTATOR_LEAVE_PROMPT_TEXT");
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_responseCallback = delegate(AlertPopup.Response response, object userData)
		{
			if (response == AlertPopup.Response.CONFIRM)
			{
				SpectatorManager.Get().LeaveSpectatorMode();
			}
		};
		DialogManager.DialogProcessCallback processCallback = (DialogBase dialog, object userData) => spectator.IsSpectatingPlayer(gameAccountId) ? true : false;
		DialogManager.Get().ShowPopup(info, processCallback);
	}

	private void OnInviteToPartyButtonReleased(UIEvent e)
	{
		if (PartyManager.Get().IsPartyLeader())
		{
			if (PartyManager.Get().GetCurrentPartySize() >= PartyManager.Get().GetBattlegroundsMaxRankedPartySize() && !PartyManager.Get().IsInPrivateBattlegroundsParty())
			{
				ShowBattlegroundsPrivatePartyDialog();
			}
			else
			{
				PartyManager.Get().SendInvite(PartyType.BATTLEGROUNDS_PARTY, m_player.GetBestGameAccountId());
			}
		}
		else
		{
			PartyManager.Get().SendInviteSuggestion(PartyType.BATTLEGROUNDS_PARTY, m_player.GetBestGameAccountId());
		}
	}

	private void OnKickFromPartyButtonReleased(UIEvent e)
	{
		PartyManager.Get().KickPlayerFromParty(m_player.GetBestGameAccountId());
	}

	private void OnAddFriendButtonReleased(UIEvent e)
	{
		BnetFriendMgr.Get().SendInvite(m_player.GetBattleTag().GetString());
		m_friendListFrame.CloseChallengeMenu();
	}

	private void OnReportButtonReleased(UIEvent e)
	{
		ShowReportingPopup();
	}

	private void OnOptionsButtonReleased(UIEvent e)
	{
		ShowPopup(m_optionsPopupWidget);
		if (m_optionsPopup != null)
		{
			m_optionsPopup.Init(m_player, this);
		}
		m_friendListFrame.CloseChallengeMenu();
	}

	private void OnHearthstoneButtonOver(UIBButton button, TooltipZone tooltipZone)
	{
		string headerKey = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_HEADER";
		string descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_AVAILABLE";
		string reason;
		if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.Friendly)
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_MODE_UNAVAILABLE";
		}
		else if (!GameUtils.IsTraditionalTutorialComplete())
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_TRADITIONAL_LOCKED";
		}
		else if (m_player.GetHearthstoneGameAccount().GetTutorialBeaten() < 1)
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_NO_HEARTHSTONE_TUTORIAL_COMPLETE";
		}
		else if (!GetAvailability(m_player, out reason))
		{
			descriptionFormat = reason;
		}
		else
		{
			if (ShouldSeeHearthstoneChallengePopup() || CollectionManager.Get().AccountHasValidDeck(FormatType.FT_STANDARD))
			{
				return;
			}
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGER_NO_STANDARD_DECK";
		}
		ShowTooltip(m_player, headerKey, descriptionFormat, tooltipZone, button);
	}

	private void OnBattlegroundsButtonOver(UIBButton button, TooltipZone tooltipZone)
	{
		string headerKey = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_HEADER";
		string descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_AVAILABLE";
		if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.BattlegroundsFriendlyChallenge)
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_MODE_UNAVAILABLE";
		}
		else if (!GameUtils.IsBattleGroundsTutorialComplete())
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_BATTLEGROUNDS_LOCKED";
		}
		else if (!FriendChallengeMgr.Get().AllowBGInviteWhileInNPPGEnabled() && !m_player.GetHearthstoneGameAccount().GetBattlegroundsTutorialComplete())
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_NO_BATTLEGROUNDS_TUTORIAL_COMPLETE";
		}
		else
		{
			if (GetAvailability(m_player, out var reason))
			{
				return;
			}
			descriptionFormat = reason;
		}
		ShowTooltip(m_player, headerKey, descriptionFormat, tooltipZone, button);
	}

	private void OnMercenariesButtonOver(UIBButton button, TooltipZone tooltipZone)
	{
		string headerKey = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_HEADER";
		string descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_AVAILABLE";
		if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.BattlegroundsFriendlyChallenge)
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_MODE_UNAVAILABLE";
		}
		else if (!GameUtils.IsMercenariesVillageTutorialComplete())
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_MERCS_LOCKED";
		}
		else if (!m_player.GetHearthstoneGameAccount().GetMercenariesTutorialComplete())
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_NO_MERCS_TUTORIAL_COMPLETE";
		}
		else
		{
			if (GetAvailability(m_player, out var reason))
			{
				return;
			}
			descriptionFormat = reason;
		}
		ShowTooltip(m_player, headerKey, descriptionFormat, tooltipZone, button);
	}

	private void OnInviteToPartyButtonOver(UIBButton button, TooltipZone tooltipZone)
	{
		string headerKey = "GLOBAL_FRIENDLIST_BATTLEGROUNDS_TOOLTIP_INVITE_HEADER";
		string descriptionFormat = "GLOBAL_FRIENDLIST_BATTLEGROUNDS_TOOLTIP_INVITE_BODY";
		if (PartyManager.Get().IsPlayerPendingInCurrentParty(m_player.GetBestGameAccountId()))
		{
			headerKey = "GLOBAL_FRIENDLIST_BATTLEGROUNDS_TOOLTIP_INVITE_ALREADY_SENT_HEADER";
			descriptionFormat = "GLOBAL_FRIENDLIST_BATTLEGROUNDS_TOOLTIP_INVITE_ALREADY_SENT_BODY";
		}
		else if (PartyManager.Get().GetCurrentPartySize() >= PartyManager.Get().GetMaxPartySizeByPartyType(PartyType.BATTLEGROUNDS_PARTY))
		{
			headerKey = "GLOBAL_FRIENDLIST_BATTLEGROUNDS_TOOLTIP_INVITE_FULL_PARTY_HEADER";
			descriptionFormat = "GLOBAL_FRIENDLIST_BATTLEGROUNDS_TOOLTIP_INVITE_FULL_PARTY_BODY";
		}
		else if (!FriendChallengeMgr.Get().AllowBGInviteWhileInNPPGEnabled() && !m_player.GetHearthstoneGameAccount().GetBattlegroundsTutorialComplete())
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_NO_BATTLEGROUNDS_TUTORIAL_COMPLETE";
		}
		else if (!FriendChallengeMgr.Get().IsOpponentAvailable(m_player))
		{
			headerKey = "GLOBAL_FRIENDLIST_BUSYSTATUS";
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_USER_IS_BUSY";
		}
		ShowTooltip(m_player, headerKey, descriptionFormat, tooltipZone, button);
	}

	private void OnKickFromPartyButtonOver(UIBButton button, TooltipZone tooltipZone)
	{
		string headerKey = "GLOBAL_FRIENDLIST_BATTLEGROUNDS_TOOLTIP_KICK_HEADER";
		string descriptionFormat = "GLOBAL_FRIENDLIST_BATTLEGROUNDS_TOOLTIP_KICK_BODY";
		ShowTooltip(m_player, headerKey, descriptionFormat, tooltipZone, button);
	}

	private void OnSpectateButtonOver(UIBButton button, TooltipZone tooltipZone)
	{
		BnetGameAccountId gameAccountId = m_player.GetBestGameAccountId();
		if (SpectatorManager.Get().HasPreviouslyKickedMeFromGame(gameAccountId, SpectatorManager.GetSpectatorGameHandleFromPlayer(m_player)))
		{
			string headerKey = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_PREVIOUSLY_KICKED_HEADER";
			string descriptionFormat = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_PREVIOUSLY_KICKED_TEXT";
			ShowTooltip(m_player, headerKey, descriptionFormat, tooltipZone, button);
		}
		else if (SpectatorManager.Get().HasInvitedMeToSpectate(gameAccountId))
		{
			string headerKey2 = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_AVAILABLE_HEADER";
			string descriptionFormat2 = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_RECEIVED_INVITE_TEXT";
			ShowTooltip(m_player, headerKey2, descriptionFormat2, tooltipZone, button);
		}
	}

	private void OnInviteToSpectateButtonOver(UIBButton button, TooltipZone tooltipZone)
	{
		BnetGameAccountId gameAccountId = m_player.GetBestGameAccountId();
		string headerKey;
		string descriptionFormat;
		if (SpectatorManager.Get().IsInvitedToSpectateMyGame(gameAccountId))
		{
			headerKey = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_INVITED_HEADER";
			descriptionFormat = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_INVITED_TEXT";
		}
		else if (SpectatorManager.Get().IsPlayerSpectatingMyGamesOpposingSide(gameAccountId))
		{
			headerKey = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_INVITE_OTHER_SIDE_HEADER";
			descriptionFormat = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_INVITE_OTHER_SIDE_TEXT";
		}
		else
		{
			headerKey = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_INVITE_HEADER";
			descriptionFormat = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_INVITE_TEXT";
		}
		ShowTooltip(m_player, headerKey, descriptionFormat, tooltipZone, button);
	}

	private void OnKickSpectatorButtonOver(UIBButton button, TooltipZone tooltipZone)
	{
		string headerKey = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_KICK_HEADER";
		string descriptionFormat = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_KICK_TEXT";
		ShowTooltip(m_player, headerKey, descriptionFormat, tooltipZone, button);
	}

	private void OnStopSpectatingButtonOver(UIBButton button, TooltipZone tooltipZone)
	{
		string headerKey = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_SPECTATING_HEADER";
		string descriptionFormat = "GLOBAL_FRIENDLIST_SPECTATE_TOOLTIP_SPECTATING_TEXT";
		ShowTooltip(m_player, headerKey, descriptionFormat, tooltipZone, button);
	}

	private void OnReportButtonOver(UIBButton button, TooltipZone tooltipZone)
	{
		string headerKey = "GLOBAL_FRIENDLIST_REPORT_TOOLTIP_HEADER";
		string descriptionFormat = "GLOBAL_FRIENDLIST_REPORT_TOOLTIP_CURRENTLY_UNAVAILABLE_TEXT";
		NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (features != null && !features.ReportPlayerEnabled)
		{
			ShowTooltip(m_player, headerKey, descriptionFormat, tooltipZone, button);
		}
	}

	private void OnHearthstoneButtonOverride(Widget buttonWidget)
	{
		if (ShouldSeeHearthstoneChallengePopup())
		{
			buttonWidget.TriggerEvent("HEARTHSTONE_EXTENDED_MENU");
		}
		else
		{
			buttonWidget.TriggerEvent("HEARTHSTONE");
		}
	}
}
