using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class FriendsListOptionsPopup : MonoBehaviour
{
	[SerializeField]
	private UberText m_optionsMenuHeader;

	[SerializeField]
	private WidgetInstance m_removeFriendButton;

	[SerializeField]
	private WidgetInstance m_reportButton;

	public const string DismissPopupEvent = "DISMISS_POPUP";

	private Widget m_widget;

	private BnetPlayer m_player;

	private FriendListFlyoutMenu m_flyoutMenu;

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		m_removeFriendButton.RegisterReadyListener(delegate
		{
			m_removeFriendButton.SetLayerOverride(GameLayer.HighPriorityUI);
			m_removeFriendButton.GetComponentInChildren<UIBButton>().AddEventListener(UIEventType.RELEASE, OnRemoveFriendButtonReleased);
		});
		m_reportButton.RegisterReadyListener(delegate
		{
			m_reportButton.SetLayerOverride(GameLayer.HighPriorityUI);
			UIBButton uibButton = m_reportButton.GetComponentInChildren<UIBButton>();
			NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
			if (netObject != null && !netObject.ReportPlayerEnabled)
			{
				m_reportButton.TriggerEvent("DISABLED");
				TooltipZone tooltipZone = m_reportButton.GetComponentInChildren<TooltipZone>(includeInactive: true);
				uibButton.AddEventListener(UIEventType.ROLLOVER, delegate
				{
					OnReportButtonOver(uibButton, tooltipZone);
				});
				uibButton.AddEventListener(UIEventType.ROLLOUT, delegate
				{
					tooltipZone.HideTooltip();
				});
			}
			else
			{
				m_reportButton.TriggerEvent("ENABLED");
				uibButton.AddEventListener(UIEventType.RELEASE, OnReportButtonReleased);
			}
		});
	}

	public void Init(BnetPlayer player, FriendListFlyoutMenu flyoutMenu)
	{
		m_player = player;
		m_flyoutMenu = flyoutMenu;
		m_optionsMenuHeader.Text = player.GetFullName() ?? player.GetBattleTag().ToString();
	}

	private void OnRemoveFriendButtonReleased(UIEvent e)
	{
		ChatMgr.Get().FriendListFrame.ShowRemoveFriendPopup(m_player);
		m_widget.TriggerEvent("DISMISS_POPUP");
	}

	private void OnReportButtonReleased(UIEvent e)
	{
		m_flyoutMenu.ShowReportingPopup();
		m_widget.TriggerEvent("DISMISS_POPUP");
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
}
