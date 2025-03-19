using System.Collections.Generic;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class HearthstoneChallengePopup : MonoBehaviour
{
	private struct ButtonInitializationParams
	{
		public Widget m_ButtonWidget;

		public FriendListFlyoutMenu.ButtonOption m_Option;
	}

	[SerializeField]
	private UberText m_challengeFriendNameText;

	[SerializeField]
	private WidgetInstance m_standardButton;

	[SerializeField]
	private WidgetInstance m_wildButton;

	[SerializeField]
	private WidgetInstance m_twistButton;

	[SerializeField]
	private WidgetInstance m_tavernBrawlButton;

	public const string EnabledEvent = "ENABLED";

	public const string DisabledEvent = "DISABLED";

	public const string DismissPopupEvent = "DISMISS_POPUP";

	private Widget m_widget;

	private BnetPlayer m_player;

	private FriendListFriendFrame m_friendListFriendFrame;

	private FriendListFlyoutMenu m_flyoutMenu;

	private Dictionary<FriendListFlyoutMenu.ButtonOption, FriendListFlyoutMenu.ButtonEvent> m_buttonEvents;

	private bool m_hasInitialized;

	private void Awake()
	{
		m_widget = base.gameObject.GetComponent<WidgetTemplate>();
		m_widget.SetLayerOverride(GameLayer.HighPriorityUI);
	}

	public void Init(BnetPlayer player, FriendListFriendFrame friendListFriendFrame, FriendListFlyoutMenu flyoutMenu)
	{
		if (!m_hasInitialized)
		{
			m_player = player;
			m_friendListFriendFrame = friendListFriendFrame;
			m_flyoutMenu = flyoutMenu;
			m_challengeFriendNameText.Text = (BnetFriendMgr.Get().IsFriend(m_player) ? GameStrings.Format("GLOBAL_FRIENDLIST_CHALLENGE_MENU_HEADER_FRIEND", m_player.GetBattleTag()) : GameStrings.Format("GLOBAL_FRIENDLIST_CHALLENGE_MENU_HEADER_NONFRIEND", m_player.GetBattleTag()));
			InitializeButtonEvents();
			InitializePopup();
			m_hasInitialized = true;
		}
	}

	private void InitializeButtonEvents()
	{
		m_buttonEvents = new Dictionary<FriendListFlyoutMenu.ButtonOption, FriendListFlyoutMenu.ButtonEvent>
		{
			{
				FriendListFlyoutMenu.ButtonOption.StandardHearthstone,
				new FriendListFlyoutMenu.ButtonEvent("STANDARD", m_standardButton, OnStandardButtonReleased, OnStandardButtonOver)
			},
			{
				FriendListFlyoutMenu.ButtonOption.WildHearthstone,
				new FriendListFlyoutMenu.ButtonEvent("WILD", m_wildButton, OnWildButtonReleased, OnWildButtonOver)
			},
			{
				FriendListFlyoutMenu.ButtonOption.TwistHearthstone,
				new FriendListFlyoutMenu.ButtonEvent("TWIST", m_twistButton, OnTwistButtonReleased, OnTwistButtonOver)
			},
			{
				FriendListFlyoutMenu.ButtonOption.TavernBrawl,
				new FriendListFlyoutMenu.ButtonEvent("TAVERN_BRAWL", m_tavernBrawlButton, OnTavernBrawlButtonReleased, OnTavernBrawlButtonOver)
			}
		};
	}

	private void InitializePopup()
	{
		foreach (FriendListFlyoutMenu.ButtonOption buttonOption in m_buttonEvents.Keys)
		{
			FriendListFlyoutMenu.ButtonEvent buttonEvent = m_buttonEvents[buttonOption];
			if (buttonEvent != null)
			{
				if (!buttonEvent.buttonWidget.IsReady)
				{
					ButtonInitializationParams buttonInitializationParams = default(ButtonInitializationParams);
					buttonInitializationParams.m_ButtonWidget = buttonEvent.buttonWidget;
					buttonInitializationParams.m_Option = buttonOption;
					ButtonInitializationParams currentParams = buttonInitializationParams;
					buttonEvent.buttonWidget.RegisterReadyListener(OnButtonReady, currentParams);
				}
				else
				{
					InitializeButton(buttonEvent.buttonWidget, buttonOption);
				}
			}
		}
	}

	private void InitializeButton(Widget buttonWidget, FriendListFlyoutMenu.ButtonOption option)
	{
		buttonWidget.SetLayerOverride(GameLayer.HighPriorityUI);
		UIBButton uibButton = buttonWidget.GetComponentInChildren<UIBButton>();
		if (m_buttonEvents.TryGetValue(option, out var buttonEvent))
		{
			if (ShouldEnableOption(option))
			{
				buttonWidget.TriggerEvent("ENABLED");
				buttonEvent.isEnabled = true;
				if (buttonEvent.onRelease != null)
				{
					uibButton.AddEventListener(UIEventType.RELEASE, buttonEvent.onRelease);
				}
			}
			else
			{
				buttonWidget.TriggerEvent("DISABLED");
				buttonEvent.isEnabled = false;
				UIBHighlight highlight = buttonWidget.GetComponentInChildren<UIBHighlight>();
				if (highlight.m_MouseOverHighlight != null)
				{
					highlight.m_MouseOverHighlight.SetActive(value: false);
					highlight.m_MouseOverHighlight = null;
				}
				if (buttonEvent.onHover != null)
				{
					TooltipZone tooltipZone = buttonWidget.GetComponentInChildren<TooltipZone>();
					uibButton.AddEventListener(UIEventType.ROLLOVER, delegate
					{
						buttonEvent.onHover(uibButton, tooltipZone);
					});
					uibButton.AddEventListener(UIEventType.ROLLOUT, delegate
					{
						tooltipZone.HideTooltip();
					});
				}
			}
		}
		buttonWidget.RemoveReadyListener(OnButtonReady);
	}

	private void OnButtonReady(object payload)
	{
		ButtonInitializationParams buttonParams = (ButtonInitializationParams)payload;
		InitializeButton(buttonParams.m_ButtonWidget, buttonParams.m_Option);
	}

	private bool ShouldEnableOption(FriendListFlyoutMenu.ButtonOption option)
	{
		switch (option)
		{
		case FriendListFlyoutMenu.ButtonOption.StandardHearthstone:
			if (FriendChallengeMgr.Get().IsHearthstoneFriendlyChallengeAvailable(m_player))
			{
				return CollectionManager.Get().AccountHasValidDeck(FormatType.FT_STANDARD);
			}
			return false;
		case FriendListFlyoutMenu.ButtonOption.WildHearthstone:
			if (FriendChallengeMgr.Get().IsHearthstoneFriendlyChallengeAvailable(m_player) && CollectionManager.Get().ShouldAccountSeeStandardWild())
			{
				return CollectionManager.Get().AccountHasValidDeck(FormatType.FT_WILD);
			}
			return false;
		case FriendListFlyoutMenu.ButtonOption.TwistHearthstone:
			if (FriendChallengeMgr.Get().IsHearthstoneFriendlyChallengeAvailable(m_player) && RankMgr.Get()?.GetCurrentTwistSeason() != null && CollectionManager.Get().ShouldAccountSeeStandardWild())
			{
				if (CollectionManager.Get().AccountHasValidDeck(FormatType.FT_TWIST))
				{
					return !RankMgr.IsCurrentTwistSeasonUsingHeroicDecks();
				}
				return false;
			}
			return false;
		case FriendListFlyoutMenu.ButtonOption.TavernBrawl:
			if (!FriendChallengeMgr.Get().IsHearthstoneFriendlyChallengeAvailable(m_player) || !TavernBrawlManager.Get().IsTavernBrawlActive(BrawlType.BRAWL_TYPE_TAVERN_BRAWL) || !TavernBrawlManager.Get().CanChallengeToTavernBrawl(BrawlType.BRAWL_TYPE_TAVERN_BRAWL))
			{
				return false;
			}
			if (TavernBrawlManager.Get().GetMission(BrawlType.BRAWL_TYPE_TAVERN_BRAWL).canCreateDeck)
			{
				return TavernBrawlManager.Get().HasValidDeck(BrawlType.BRAWL_TYPE_TAVERN_BRAWL);
			}
			return true;
		default:
			return false;
		}
	}

	private void ShowTooltip(BnetPlayer player, string headerKey, string descriptionFormat, TooltipZone tooltipZone, UIBButton button)
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
		tooltipZone.ShowSocialTooltip(button, header, description, 23f, GameLayer.HighPriorityUI);
		tooltipZone.AnchorTooltipTo(tooltipZone.gameObject, Anchor.TOP_RIGHT_XZ, Anchor.TOP_LEFT_XZ);
	}

	private void OnStandardButtonReleased(UIEvent e)
	{
		m_flyoutMenu.SendHearthstoneFriendlyChallenge();
	}

	private void OnWildButtonReleased(UIEvent e)
	{
		if (!CollectionManager.Get().AccountHasValidDeck(FormatType.FT_WILD))
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER"),
				m_text = GameStrings.Format("GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGER_NO_DECK"),
				m_showAlertIcon = true,
				m_responseDisplay = AlertPopup.ResponseDisplay.OK
			};
			DialogManager.Get().ShowPopup(info);
			m_friendListFriendFrame.CloseChallengeMenu();
		}
		else if (!FriendChallengeMgr.Get().IsOpponentAvailable(m_player))
		{
			m_flyoutMenu.ShowOpponentUnavailableAlert();
			m_friendListFriendFrame.CloseFriendsListMenu();
		}
		else
		{
			FriendChallengeMgr.Get().SetChallengeMethod(FriendChallengeMgr.ChallengeMethod.FROM_FRIEND_LIST);
			FriendChallengeMgr.Get().SendChallenge(m_player, FormatType.FT_WILD, enableDeckShare: true);
			m_friendListFriendFrame.CloseFriendsListMenu();
		}
	}

	private void OnTwistButtonReleased(UIEvent e)
	{
		if (!CollectionManager.Get().AccountHasValidDeck(FormatType.FT_TWIST) && !RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER"),
				m_text = GameStrings.Format("GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGER_NO_TWIST_DECK"),
				m_showAlertIcon = true,
				m_responseDisplay = AlertPopup.ResponseDisplay.OK
			};
			DialogManager.Get().ShowPopup(info);
			m_friendListFriendFrame.CloseChallengeMenu();
		}
		else if (!FriendChallengeMgr.Get().IsOpponentAvailable(m_player))
		{
			m_flyoutMenu.ShowOpponentUnavailableAlert();
			m_friendListFriendFrame.CloseFriendsListMenu();
		}
		else
		{
			FriendChallengeMgr.Get().SetChallengeMethod(FriendChallengeMgr.ChallengeMethod.FROM_FRIEND_LIST);
			FriendChallengeMgr.Get().SendChallenge(m_player, FormatType.FT_TWIST, enableDeckShare: true);
			m_friendListFriendFrame.CloseFriendsListMenu();
		}
	}

	private void OnTavernBrawlButtonReleased(UIEvent e)
	{
		if (!FriendChallengeMgr.Get().IsOpponentAvailable(m_player))
		{
			m_flyoutMenu.ShowOpponentUnavailableAlert();
			m_friendListFriendFrame.CloseFriendsListMenu();
		}
		else if (!TavernBrawlManager.Get().HasUnlockedTavernBrawl(BrawlType.BRAWL_TYPE_TAVERN_BRAWL))
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER"),
				m_text = GameStrings.Format("GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGER_TAVERN_BRAWL_LOCKED"),
				m_showAlertIcon = true,
				m_responseDisplay = AlertPopup.ResponseDisplay.OK
			};
			DialogManager.Get().ShowPopup(info);
			m_friendListFriendFrame.CloseChallengeMenu();
		}
		else if (!TavernBrawlManager.Get().CanChallengeToTavernBrawl(BrawlType.BRAWL_TYPE_TAVERN_BRAWL))
		{
			AlertPopup.PopupInfo info2 = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER"),
				m_text = GameStrings.Format("GLOBAL_TAVERN_BRAWL_ERROR_SEASON_INCREMENTED"),
				m_showAlertIcon = true,
				m_responseDisplay = AlertPopup.ResponseDisplay.OK
			};
			DialogManager.Get().ShowPopup(info2);
			m_friendListFriendFrame.CloseChallengeMenu();
		}
		else if (TavernBrawlManager.Get().GetMission(BrawlType.BRAWL_TYPE_TAVERN_BRAWL).canCreateDeck && !TavernBrawlManager.Get().HasValidDeck(BrawlType.BRAWL_TYPE_TAVERN_BRAWL))
		{
			FriendChallengeMgr.ShowChallengerNeedsToCreateTavernBrawlDeckAlert();
		}
		else
		{
			TavernBrawlManager.Get().CurrentBrawlType = BrawlType.BRAWL_TYPE_TAVERN_BRAWL;
			FriendChallengeMgr.Get().SendTavernBrawlChallenge(m_player, BrawlType.BRAWL_TYPE_TAVERN_BRAWL, TavernBrawlManager.Get().CurrentMission().seasonId, TavernBrawlManager.Get().CurrentMission().SelectedBrawlLibraryItemId);
			m_friendListFriendFrame.CloseFriendsListMenu();
		}
	}

	private bool ShouldShowGenericHearthstoneTooltip(out string reason)
	{
		if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.Friendly)
		{
			reason = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_MODE_UNAVAILABLE";
			return true;
		}
		if (!GameUtils.IsTraditionalTutorialComplete())
		{
			reason = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_TRADITIONAL_LOCKED";
			return true;
		}
		if (m_player.GetHearthstoneGameAccount().GetTutorialBeaten() < 1)
		{
			reason = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGEE_NO_HEARTHSTONE_TUTORIAL_COMPLETE";
			return true;
		}
		if (!FriendListFlyoutMenu.GetAvailability(m_player, out reason))
		{
			return true;
		}
		return false;
	}

	private void OnStandardButtonOver(UIBButton button, TooltipZone tooltipZone)
	{
		string headerKey = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_HEADER";
		string descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_AVAILABLE";
		if (ShouldShowGenericHearthstoneTooltip(out var reason))
		{
			descriptionFormat = reason;
		}
		else if (!CollectionManager.Get().AccountHasValidDeck(FormatType.FT_STANDARD))
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGER_NO_STANDARD_DECK";
		}
		ShowTooltip(m_player, headerKey, descriptionFormat, tooltipZone, button);
	}

	private void OnWildButtonOver(UIBButton button, TooltipZone tooltipZone)
	{
		string headerKey = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_HEADER";
		string descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_AVAILABLE";
		if (ShouldShowGenericHearthstoneTooltip(out var reason))
		{
			descriptionFormat = reason;
		}
		else if (!CollectionManager.Get().ShouldAccountSeeStandardWild())
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_GAME_MODE_LOCKED";
		}
		else if (!CollectionManager.Get().AccountHasValidDeck(FormatType.FT_WILD))
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGER_NO_DECK";
		}
		ShowTooltip(m_player, headerKey, descriptionFormat, tooltipZone, button);
	}

	private void OnTwistButtonOver(UIBButton button, TooltipZone tooltipZone)
	{
		string headerKey = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_HEADER";
		string descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_AVAILABLE";
		if (ShouldShowGenericHearthstoneTooltip(out var reason))
		{
			descriptionFormat = reason;
		}
		else if (RankMgr.Get()?.GetCurrentTwistSeason() == null || RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_TOOLTIP_NO_TWIST_SEASON";
		}
		else if (!CollectionManager.Get().AccountHasValidDeck(FormatType.FT_TWIST) && !RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGER_NO_TWIST_DECK";
		}
		ShowTooltip(m_player, headerKey, descriptionFormat, tooltipZone, button);
	}

	private void OnTavernBrawlButtonOver(UIBButton button, TooltipZone tooltipZone)
	{
		string headerKey = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_HEADER";
		string descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_BUTTON_AVAILABLE";
		if (ShouldShowGenericHearthstoneTooltip(out var reason))
		{
			descriptionFormat = reason;
		}
		else if (!TavernBrawlManager.Get().HasUnlockedTavernBrawl(BrawlType.BRAWL_TYPE_TAVERN_BRAWL))
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGER_TAVERN_BRAWL_LOCKED";
		}
		else if (!TavernBrawlManager.Get().IsTavernBrawlActive(BrawlType.BRAWL_TYPE_TAVERN_BRAWL))
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_TOOLTIP_NO_TAVERN_BRAWL";
		}
		else if (!TavernBrawlManager.Get().CanChallengeToTavernBrawl(BrawlType.BRAWL_TYPE_TAVERN_BRAWL))
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_TOOLTIP_TAVERN_BRAWL_NOT_CHALLENGEABLE";
		}
		else if (TavernBrawlManager.Get().GetMission(BrawlType.BRAWL_TYPE_TAVERN_BRAWL).canCreateDeck && !TavernBrawlManager.Get().HasValidDeck(BrawlType.BRAWL_TYPE_TAVERN_BRAWL))
		{
			descriptionFormat = "GLOBAL_FRIENDLIST_CHALLENGE_CHALLENGER_NO_TAVERN_BRAWL_DECK";
		}
		ShowTooltip(m_player, headerKey, descriptionFormat, tooltipZone, button);
	}
}
