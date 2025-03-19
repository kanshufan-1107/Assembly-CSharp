using System.Collections;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class FriendListChallengeButton : MonoBehaviour
{
	public WidgetInstance m_flyoutMenuWidget;

	public TooltipZone m_tooltipZone;

	private BnetPlayer m_player;

	private Widget m_widget;

	private FriendListFlyoutMenu m_flyoutMenu;

	public bool IsChallengeMenuOpen { get; private set; }

	public FriendListFriendFrame FriendFrame { get; set; }

	protected void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterReadyListener(delegate
		{
			m_tooltipZone.gameObject.layer = 26;
		});
		m_flyoutMenuWidget.transform.position = Vector3.zero;
		m_flyoutMenuWidget.RegisterReadyListener(delegate
		{
			m_flyoutMenu = m_flyoutMenuWidget.GetComponentInChildren<FriendListFlyoutMenu>();
			StartCoroutine(InitializeFlyoutMenu());
		});
	}

	private void OnDestroy()
	{
		if (IsChallengeMenuOpen)
		{
			FriendlyChallengeHelper.Get().ActiveChallengeMenu = null;
		}
	}

	private IEnumerator InitializeFlyoutMenu()
	{
		yield return null;
		m_flyoutMenuWidget.SetLayerOverride(GameLayer.BattleNetDialog);
		FriendListFrame friendListFrame = ChatMgr.Get().FriendListFrame;
		m_flyoutMenuWidget.transform.SetParent(friendListFrame.friendFlyoutBone.transform);
		m_flyoutMenuWidget.transform.position = friendListFrame.friendFlyoutBone.transform.position;
		m_flyoutMenuWidget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "CODE_FLYOUT_MENU_DISMISSED")
			{
				FriendFrame.CloseChallengeMenu();
			}
		});
	}

	public bool SetPlayer(BnetPlayer player)
	{
		if (m_player == player)
		{
			return false;
		}
		m_player = player;
		return true;
	}

	public BnetPlayer GetPlayer()
	{
		return m_player;
	}

	public void UpdateFlyoutMenu()
	{
		if (m_flyoutMenu != null)
		{
			m_flyoutMenu.UpdateFlyoutMenu();
		}
	}

	public void DismissPopups(bool showAlert = false)
	{
		if (m_flyoutMenu != null)
		{
			m_flyoutMenu.DismissPopups(showAlert);
		}
	}
}
