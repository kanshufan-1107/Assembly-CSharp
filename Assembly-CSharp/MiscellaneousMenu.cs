using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class MiscellaneousMenu : ButtonListMenu
{
	[CustomEditField(Sections = "Template Items")]
	public Transform m_menuBone;

	public Material m_redButtonMaterial;

	private static MiscellaneousMenu s_instance;

	private UIBButton m_creditsButton;

	private UIBButton m_restorePurchasesButton;

	protected override void Awake()
	{
		m_menuParent = m_menuBone;
		m_targetLayer = GameLayer.HighPriorityUI;
		base.Awake();
		s_instance = this;
		m_creditsButton = CreateMenuButton("CreditsButton", "GLOBAL_OPTIONS_CREDITS", OnCreditsButtonReleased);
		m_restorePurchasesButton = CreateMenuButton("RestorePurchasesButton", "GLOBAL_OPTIONS_RESTORE_PURCHASES", OnRestorePurchasesButtonReleased);
		m_menu.m_headerText.Text = GameStrings.Get("GLOBAL_OPTIONS_MISCELLANEOUS_LABEL");
	}

	public static MiscellaneousMenu Get()
	{
		return s_instance;
	}

	protected override List<UIBButton> GetButtons()
	{
		List<UIBButton> buttons = new List<UIBButton>();
		buttons.Add(m_creditsButton);
		if (PlatformSettings.OS == OSCategory.iOS)
		{
			buttons.Add(m_restorePurchasesButton);
		}
		return buttons;
	}

	private void OnCreditsButtonReleased(UIEvent e)
	{
		Hide();
		if ((!(NarrativeManager.Get() != null) || !NarrativeManager.Get().IsShowingBlockingDialog()) && SceneMgr.Get().GetMode() != SceneMgr.Mode.LOGIN)
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.CREDITS);
		}
	}

	private void OnPrivacyPolicyButtonReleased(UIEvent e)
	{
		Application.OpenURL(ExternalUrlService.Get().GetPrivacyPolicyLink());
	}

	private void OnRestorePurchasesButtonReleased(UIEvent e)
	{
		Hide();
		AlertPopup.PopupInfo popup = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLOBAL_OPTIONS_RESTORE_PURCHASES"),
			m_text = GameStrings.Get("GLOBAL_OPTIONS_RESTORE_PURCHASES_POPUP_TEXT"),
			m_confirmText = GameStrings.Get("GLOBAL_SWITCH_ACCOUNT"),
			m_cancelText = GameStrings.Get("GLOBAL_BACK"),
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CONFIRM)
				{
					GameUtils.LogoutConfirmation();
				}
			}
		};
		DialogManager.Get().ShowPopup(popup);
	}
}
