using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Hearthstone;
using Hearthstone.UI;
using UnityEngine;

public class PrivacyPolicyPopup : DialogBase
{
	public delegate void ResponseCallback(bool confirmedPrivacyPolicy);

	public class Info
	{
		public ResponseCallback m_callback;
	}

	private const string PrivacyPolicyUrl = "https://legal.battlenet.com.cn/zh-cn/legal/a6deb98c-75f3-4df7-a8f7-5f42cff34283/non-printable";

	private const string EulaUrl = "https://legal.battlenet.com.cn/zh-cn/legal/2e90163d-612f-41a5-addd-8a837ac43d02/non-printable";

	private const string SdkUrl = "https://wow.blizzard.cn/news/privacy/20240402/40295_1147022.html";

	public PegUIElement m_confirmButton;

	public PegUIElement m_rejectButton;

	public PegUIElement m_privacyPolicyButton;

	public PegUIElement m_eulaButton;

	public PegUIElement m_sdkButton;

	private Vector3 m_buttonOffset = new Vector3(0.2f, 0f, 0.6f);

	private bool m_confirmedPrivacyPolicy;

	private Camera referenceCamera;

	private ResponseCallback m_responseCallback;

	protected override void Awake()
	{
		base.Awake();
		referenceCamera = CameraUtils.FindFirstByLayer(GameLayer.UI);
		base.transform.position = referenceCamera.transform.TransformPoint(0f, 0f, 200f);
	}

	private void Start()
	{
		GetComponent<WidgetTemplate>().InitializeWidgetBehaviors();
		List<Component> components = new List<Component>();
		GameObjectUtils.WalkSelfAndChildren(base.transform, delegate(Transform current)
		{
			bool flag = true;
			current.GetComponents(components);
			foreach (Component item in components)
			{
				if (item is Maskable maskable)
				{
					maskable.OverrideRenderPassEntryPoint(CustomViewEntryPoint.BattleNetChat);
					flag = false;
					break;
				}
			}
			if (flag)
			{
				current.gameObject.layer = 18;
			}
			components.Clear();
			return flag;
		});
		m_confirmButton.AddEventListener(UIEventType.RELEASEALL, ConfirmButtonReleaseAll);
		m_rejectButton.AddEventListener(UIEventType.RELEASEALL, RejectButtonReleaseAll);
		m_privacyPolicyButton.AddEventListener(UIEventType.RELEASEALL, PrivacyPolicyButtonReleaseAll);
		m_eulaButton.AddEventListener(UIEventType.RELEASEALL, EULAButtonReleaseAll);
		m_sdkButton.AddEventListener(UIEventType.RELEASEALL, SdkButtonReleaseAll);
		m_privacyPolicyButton.AddEventListener(UIEventType.PRESS, PrivacyPolicyButtonPress);
		m_eulaButton.AddEventListener(UIEventType.PRESS, EULAButtonPress);
		m_sdkButton.AddEventListener(UIEventType.PRESS, SdkButtonPress);
	}

	public override void Show()
	{
		base.Show();
		m_showAnimState = ShowAnimState.IN_PROGRESS;
		UniversalInputManager.Get().SetSystemDialogActive(active: true);
	}

	public void SetInfo(Info info)
	{
		m_responseCallback = info.m_callback;
	}

	protected void DownScale()
	{
		iTween.ScaleTo(base.gameObject, iTween.Hash("scale", new Vector3(0f, 0f, 0f), "delay", 0.1f, "easetype", iTween.EaseType.easeInOutCubic, "oncomplete", "OnHideAnimFinished", "time", 0.2f));
	}

	protected override void OnHideAnimFinished()
	{
		base.OnHideAnimFinished();
		m_shown = false;
		OnPrivacyPolicyPopupResponse(m_confirmedPrivacyPolicy);
	}

	private void ConfirmButtonReleaseAll(UIEvent e)
	{
		if (((UIReleaseAllEvent)e).GetMouseIsOver())
		{
			m_confirmedPrivacyPolicy = true;
			ScaleAway();
		}
	}

	private void RejectButtonReleaseAll(UIEvent e)
	{
		if (((UIReleaseAllEvent)e).GetMouseIsOver())
		{
			m_confirmedPrivacyPolicy = false;
			ScaleAway();
		}
	}

	private void PrivacyPolicyButtonReleaseAll(UIEvent e)
	{
		m_privacyPolicyButton.transform.localPosition -= m_buttonOffset;
		if (((UIReleaseAllEvent)e).GetMouseIsOver())
		{
			Application.OpenURL("https://legal.battlenet.com.cn/zh-cn/legal/a6deb98c-75f3-4df7-a8f7-5f42cff34283/non-printable");
		}
	}

	private void EULAButtonReleaseAll(UIEvent e)
	{
		m_eulaButton.transform.localPosition -= m_buttonOffset;
		if (((UIReleaseAllEvent)e).GetMouseIsOver())
		{
			Application.OpenURL("https://legal.battlenet.com.cn/zh-cn/legal/2e90163d-612f-41a5-addd-8a837ac43d02/non-printable");
		}
	}

	private void SdkButtonReleaseAll(UIEvent e)
	{
		m_sdkButton.transform.localPosition -= m_buttonOffset;
		if (((UIReleaseAllEvent)e).GetMouseIsOver())
		{
			Application.OpenURL("https://wow.blizzard.cn/news/privacy/20240402/40295_1147022.html");
		}
	}

	private void PrivacyPolicyButtonPress(UIEvent e)
	{
		m_privacyPolicyButton.transform.localPosition += m_buttonOffset;
	}

	private void EULAButtonPress(UIEvent e)
	{
		m_eulaButton.transform.localPosition += m_buttonOffset;
	}

	private void SdkButtonPress(UIEvent e)
	{
		m_sdkButton.transform.localPosition += m_buttonOffset;
	}

	private void ScaleAway()
	{
		iTween.ScaleTo(base.gameObject, iTween.Hash("scale", Vector3.Scale(PUNCH_SCALE, base.gameObject.transform.localScale), "easetype", iTween.EaseType.easeInOutCubic, "oncomplete", "DownScale", "time", 0.1f));
	}

	private void OnPrivacyPolicyPopupResponse(bool confirmedPrivacyPolicy)
	{
		if (confirmedPrivacyPolicy)
		{
			OnPolicyConfirmed();
			return;
		}
		GameObject alertPopupObj = Object.Instantiate(Resources.Load("Prefabs/EmbeddedAlertPopup")) as GameObject;
		if (referenceCamera != null)
		{
			alertPopupObj.transform.position = referenceCamera.transform.TransformPoint(0f, 0f, 200f);
		}
		AlertPopup component = alertPopupObj.GetComponent<AlertPopup>();
		AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_PRIVACY_POLICY_EULA_TITLE"),
			m_text = GameStrings.Get("GLUE_PRIVACY_POLICY_EULA_CONFIRMATION_TEXT"),
			m_confirmText = GameStrings.Get("GLUE_PRIVACY_POLICY_EULA_CONFIRMATION_ACCEPT"),
			m_cancelText = GameStrings.Get("GLUE_PRIVACY_POLICY_EULA_CONFIRMATION_REJECT"),
			m_showAlertIcon = true,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CONFIRM)
				{
					OnPolicyConfirmed();
				}
				else
				{
					m_responseCallback?.Invoke(confirmedPrivacyPolicy: false);
					HearthstoneApplication.Get().Exit();
				}
				Object.Destroy(alertPopupObj);
			}
		};
		component.UpdateInfo(popupInfo);
	}

	private void OnPolicyConfirmed()
	{
		Options.Get().SetBool(Option.HAS_ACCEPTED_PRIVACY_POLICY_AND_EULA, val: true);
		m_responseCallback?.Invoke(confirmedPrivacyPolicy: true);
		HearthstoneApplication.Get().DataTransferDependency.Callback();
	}
}
