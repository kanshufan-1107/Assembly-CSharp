using System;
using System.Collections.Generic;
using Hearthstone;
using UnityEngine;

public class LegalChallengeUIController : BasicPopup
{
	public GameObject oneButtonLayout;

	public GameObject twoButtonLayout;

	public GameObject threeButtonLayout;

	private Action m_onUserAccept;

	protected override void Awake()
	{
		base.Awake();
		oneButtonLayout.SetActive(value: false);
		twoButtonLayout.SetActive(value: false);
		threeButtonLayout.SetActive(value: false);
		if ((bool)HearthstoneApplication.CanQuitGame)
		{
			m_cancelButton.AddEventListener(UIEventType.RELEASE, delegate
			{
				OnExitButtonPressed();
			});
		}
		else
		{
			m_cancelButton.gameObject.SetActive(value: false);
		}
	}

	public void ShowLegalChallengePopup(LoginLegalChallengeFlow.LegalChallengeInitialResponseData data, Action OnUserAccept)
	{
		m_onUserAccept = OnUserAccept;
		SetupButtons(data);
		string docTitles = GetDocTitles(data.legalAgreements);
		m_headerText.Text = data.promptTitle;
		m_bodyText.Text = string.Format(data.promptText, docTitles);
		if (!m_shown)
		{
			Show();
		}
	}

	private string GetDocTitles(List<LoginLegalChallengeFlow.LegalAgreementData> legalAgreements)
	{
		string text = "";
		for (int i = 0; i < legalAgreements.Count; i++)
		{
			if (i == legalAgreements.Count - 1)
			{
				text += " and ";
			}
			else if (i != 0)
			{
				text += ", ";
			}
			text = text + "<b>" + legalAgreements[i].title + "</b>";
		}
		return text;
	}

	private void SetupButtons(LoginLegalChallengeFlow.LegalChallengeInitialResponseData data)
	{
		GameObject layout = null;
		layout = data.legalAgreements.Count switch
		{
			1 => oneButtonLayout, 
			2 => twoButtonLayout, 
			_ => threeButtonLayout, 
		};
		UIBButton[] buttons = layout.GetComponentsInChildren<UIBButton>();
		if (buttons.Length != data.legalAgreements.Count)
		{
			Debug.LogError($"Legal docs count is {data.legalAgreements.Count} and popup buttons are {buttons.Length}");
		}
		for (int i = 0; i < buttons.Length; i++)
		{
			buttons[i].SetText(data.legalAgreements[i].title);
			string url = new string(data.legalAgreements[i].externalURL);
			buttons[i].AddEventListener(UIEventType.RELEASE, delegate
			{
				OnDocButtonPressed(url);
			});
		}
		layout.SetActive(value: true);
		m_customButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			OnUserPressedContinueButton();
		});
	}

	private void OnUserPressedContinueButton()
	{
		m_onUserAccept?.Invoke();
		Hide();
		UnityEngine.Object.Destroy(base.gameObject, 0.25f);
	}

	private void OnDocButtonPressed(string externalURL)
	{
		HearthstoneApplication.OpenURL(externalURL);
	}

	private void OnExitButtonPressed()
	{
		GameUtils.ExitConfirmation(OnExitResponce);
	}

	private void OnExitResponce(AlertPopup.Response response, object userData)
	{
		if (response == AlertPopup.Response.CANCEL)
		{
			Show();
			return;
		}
		Network.Get().AutoConcede();
		HearthstoneApplication.Get().Exit();
	}
}
