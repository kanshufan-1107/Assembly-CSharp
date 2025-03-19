using System.Collections.Generic;
using Blizzard.MobileAuth;
using UnityEngine;

public class SwitchAccountMenu : ButtonListMenu
{
	public delegate void OnSwitchAccountLogInPressed(object account);

	private const int TEMPORARY_ACCOUNT_SHOWN_LIMIT = 4;

	public Transform m_menuBone;

	private List<UIBButton> m_temporaryAccountButtons;

	private OnSwitchAccountLogInPressed m_onSwitchAccountLoginInPressedHandler;

	protected override void Awake()
	{
		m_menuParent = m_menuBone;
		m_showAnimation = false;
		base.Awake();
		m_menu.m_headerText.Text = GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_SWITCH_ACCOUNT_HEADER");
		m_temporaryAccountButtons = new List<UIBButton>();
	}

	protected override void OnDestroy()
	{
	}

	public void Show(OnSwitchAccountLogInPressed onSwitchAccountLogInPressedHandler)
	{
		m_onSwitchAccountLoginInPressedHandler = onSwitchAccountLogInPressedHandler;
		base.Show();
	}

	public void AddTemporaryAccountButtons(IEnumerable<TemporaryAccountManager.TemporaryAccountData.TemporaryAccount> sortedTemporaryAccounts, string selectedTemporaryAccountId)
	{
		m_temporaryAccountButtons.Clear();
		UIBButton button = CreateMenuButton("Log In", "GLOBAL_LOGIN", OnLogInButtonPressed);
		m_temporaryAccountButtons.Add(button);
		m_temporaryAccountButtons.Add(null);
		int buttonCount = 0;
		foreach (TemporaryAccountManager.TemporaryAccountData.TemporaryAccount temporaryAccount in sortedTemporaryAccounts)
		{
			if (buttonCount >= 4)
			{
				break;
			}
			if ((selectedTemporaryAccountId == null || !string.Equals(selectedTemporaryAccountId, temporaryAccount.m_temporaryAccountId)) && !temporaryAccount.m_isHealedUp)
			{
				button = CreateMenuButton("TemporaryAccountButton" + buttonCount, temporaryAccount.m_battleTag, OnTemporaryAccountButtonPressed);
				button.SetData(temporaryAccount);
				m_temporaryAccountButtons.Add(button);
				buttonCount++;
			}
		}
	}

	public void AddAccountButtons(IEnumerable<Account> sortedAccounts)
	{
		m_temporaryAccountButtons.Clear();
		UIBButton button = CreateMenuButton("Log In", "GLOBAL_LOGIN", OnLogInButtonPressed);
		m_temporaryAccountButtons.Add(button);
		m_temporaryAccountButtons.Add(null);
		int buttonCount = 0;
		foreach (Account account in sortedAccounts)
		{
			if (buttonCount >= 4)
			{
				break;
			}
			button = CreateMenuButton("TemporaryAccountButton" + buttonCount, account.displayName, OnTemporaryAccountButtonPressed);
			button.SetData(account);
			m_temporaryAccountButtons.Add(button);
			buttonCount++;
		}
	}

	protected override List<UIBButton> GetButtons()
	{
		return m_temporaryAccountButtons;
	}

	private void OnLogInButtonPressed(UIEvent e)
	{
		if (m_onSwitchAccountLoginInPressedHandler != null)
		{
			m_onSwitchAccountLoginInPressedHandler(null);
			m_onSwitchAccountLoginInPressedHandler = null;
		}
		Hide();
	}

	private void OnTemporaryAccountButtonPressed(UIEvent e)
	{
		object temporaryAccount = e.GetElement().GetData();
		Hide();
		if (m_onSwitchAccountLoginInPressedHandler != null)
		{
			m_onSwitchAccountLoginInPressedHandler(temporaryAccount);
			m_onSwitchAccountLoginInPressedHandler = null;
		}
	}

	protected override void OnBlockerRelease(UIEvent _)
	{
	}
}
