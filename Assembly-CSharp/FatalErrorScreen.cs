using System;
using System.Collections;
using Hearthstone;
using Hearthstone.Core;
using UnityEngine;

public class FatalErrorScreen : MonoBehaviour
{
	public UberText m_closedSignText;

	public UberText m_closedSignTitle;

	public UberText m_reconnectTip;

	public UberText m_errorCodeText;

	private Camera m_camera;

	private PegUIElement m_inputBlocker;

	private bool m_allowClick;

	private bool m_redirectToStore;

	public float m_delayBeforeNextReset;

	private bool m_isUnrecoverable;

	private void Awake()
	{
		LogoAnimation logoAnimation = LogoAnimation.Get();
		if (logoAnimation != null)
		{
			logoAnimation.HideLogo();
		}
		m_closedSignTitle.Text = GameStrings.Get("GLOBAL_SPLASH_CLOSED_SIGN_TITLE");
		if (FatalErrorMgr.Get().HasError())
		{
			FatalErrorMessage[] messages = FatalErrorMgr.Get().GetMessages();
			m_closedSignText.Text = messages[0].m_text;
			m_allowClick = messages[0].m_allowClick;
			m_redirectToStore = messages[0].m_redirectToStore;
			m_delayBeforeNextReset = messages[0].m_delayBeforeNextReset;
		}
		else if (Application.isEditor)
		{
			m_closedSignText.Text = "Please make it sure FatalError scene is NOT in your Hierarchy window.";
		}
		m_isUnrecoverable = FatalErrorMgr.Get().IsUnrecoverable;
	}

	private void Start()
	{
		if ((bool)HearthstoneApplication.AllowResetFromFatalError)
		{
			if (m_isUnrecoverable)
			{
				m_allowClick = false;
				m_reconnectTip.gameObject.SetActive(value: true);
				m_reconnectTip.SetText(GameStrings.Get("GLOBAL_MOBILE_RESTART_APPLICATION"));
			}
			else if (m_allowClick)
			{
				m_reconnectTip.gameObject.SetActive(value: true);
				m_reconnectTip.SetText(GameStrings.Get(m_redirectToStore ? "GLOBAL_MOBILE_TAP_TO_UPDATE" : "GLOBAL_MOBILE_TAP_TO_RECONNECT"));
			}
		}
		StartCoroutine(WaitForUIThenFinishSetup());
	}

	private void Update()
	{
		if (m_reconnectTip.gameObject.activeSelf)
		{
			m_reconnectTip.TextAlpha = (Mathf.Sin(Time.time * (float)Math.PI / 1f) + 1f) / 2f;
		}
	}

	private void OnDestroy()
	{
		if (PegUI.Get() != null)
		{
			PegUI.Get().RemoveInputCamera(m_camera);
		}
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", 1f);
		args.Add("time", 0.25f);
		args.Add("easetype", iTween.EaseType.easeOutCubic);
		iTween.FadeTo(base.gameObject, args);
	}

	private IEnumerator WaitForUIThenFinishSetup()
	{
		while (PegUI.Get() == null || OverlayUI.Get() == null)
		{
			yield return null;
		}
		OverlayUI.Get().AddGameObject(base.gameObject);
		Show();
		m_camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
		PegUI.Get().AddInputCamera(m_camera);
		GameObject inputBlockerObject = CameraUtils.CreateInputBlocker(m_camera, "ClosedSignInputBlocker", this);
		LayerUtils.SetLayer(inputBlockerObject, base.gameObject.layer, null);
		m_inputBlocker = inputBlockerObject.AddComponent<PegUIElement>();
		if (m_allowClick)
		{
			m_inputBlocker.AddEventListener(UIEventType.RELEASE, OnClick);
		}
		if (FatalErrorMgr.Get().GetFormattedErrorCode() != null)
		{
			m_errorCodeText.gameObject.SetActive(value: true);
			m_errorCodeText.Text = FatalErrorMgr.Get().GetFormattedErrorCode();
			OverlayUI.Get().AddGameObject(m_errorCodeText.gameObject, CanvasAnchor.TOP_RIGHT);
		}
		if (m_isUnrecoverable)
		{
			Processor.TerminateAllProcessing();
		}
	}

	private void OnClick(UIEvent e)
	{
		if ((bool)HearthstoneApplication.AllowResetFromFatalError)
		{
			if (m_redirectToStore)
			{
				UpdateUtils.OpenAppStore();
				return;
			}
			float remainingDelayTime = HearthstoneApplication.Get().LastResetTime() + m_delayBeforeNextReset - Time.realtimeSinceStartup;
			if (remainingDelayTime > 0f)
			{
				m_inputBlocker.RemoveEventListener(UIEventType.RELEASE, OnClick);
				m_closedSignText.Text = GameStrings.Get("GLOBAL_SPLASH_CLOSED_RECONNECTING");
				m_allowClick = false;
				m_reconnectTip.gameObject.SetActive(value: false);
				StartCoroutine(WaitBeforeReconnecting(remainingDelayTime));
			}
			else
			{
				Debug.Log("resetting!");
				m_inputBlocker.RemoveEventListener(UIEventType.RELEASE, OnClick);
				HearthstoneApplication.Get().Reset();
			}
		}
		else
		{
			m_inputBlocker.RemoveEventListener(UIEventType.RELEASE, OnClick);
			HearthstoneApplication.Get().Exit();
		}
	}

	private IEnumerator WaitBeforeReconnecting(float waitDuration)
	{
		yield return new WaitForSeconds(waitDuration);
		HearthstoneApplication.Get().Reset();
	}
}
