using System.Collections.Generic;
using Blizzard.T5.Services;
using QRCoder;
using QRCoder.Unity;
using UnityEngine;

namespace Hearthstone.Login.UI;

public class AccountLinkingUIController : BasicPopup
{
	public UIBButton m_ButtonLink;

	public GameObject m_UserCodeObject;

	public MeshRenderer qrCodeMeshRenderer;

	private GenericConfirmationPopup m_confirmationPopup;

	private const string CONFIRMATION_POPUP_PREFAB = "AccountLinkingConfirmationPopup.prefab:f18ff26dfa2d4ee499874926b28af55e";

	protected override void Awake()
	{
		base.Awake();
		ServiceManager.Get<LoginManager>().OnLoginCompleted += OnLoginCompleted;
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

	public void ShowAccountLinkingUI(string userCode, string url, string urlComplete)
	{
		SetUserCode(m_UserCodeObject, userCode);
		m_ButtonLink.RemoveEventListener(UIEventType.RELEASE, delegate
		{
			OnLinkButtonReleased(urlComplete);
		});
		m_ButtonLink.AddEventListener(UIEventType.RELEASE, delegate
		{
			OnLinkButtonReleased(urlComplete);
		});
		List<Material> mats = new List<Material>();
		qrCodeMeshRenderer.GetMaterials(mats);
		mats[0].mainTexture = GetQRCode(urlComplete);
		if (!m_shown)
		{
			Show();
		}
	}

	private void SetUserCode(GameObject m_UserCodeObject, string userCode)
	{
		for (int i = 0; i < m_UserCodeObject.transform.childCount; i++)
		{
			m_UserCodeObject.transform.GetChild(i).GetComponent<UberText>().Text = userCode[i].ToString() ?? "";
		}
	}

	private void OnLinkButtonReleased(string url)
	{
		HearthstoneApplication.OpenURL(url);
	}

	[ContextMenu("Show confirmation UI")]
	public void OnAccountLinkingComplete()
	{
		GameObject confirmationPopupGo = AssetLoader.Get().InstantiatePrefab("AccountLinkingConfirmationPopup.prefab:f18ff26dfa2d4ee499874926b28af55e");
		m_confirmationPopup = confirmationPopupGo.GetComponent<GenericConfirmationPopup>();
		float hideDelay = 0.26f;
		m_confirmationPopup.ShowConfirmation(hideDelay);
		Hide();
		Object.Destroy(base.gameObject, hideDelay);
	}

	private void OnLoginCompleted()
	{
		if (m_confirmationPopup != null)
		{
			m_confirmationPopup.HideConfirmation();
		}
	}

	private Texture2D GetQRCode(string url)
	{
		return new UnityQRCode(new QRCodeGenerator().CreateQrCode(url, QRCodeGenerator.ECCLevel.L)).GetGraphic(20);
	}
}
