using System;
using System.Collections.Generic;
using UnityEngine;

public class PrivacyFeaturesPopup : DialogBase
{
	private enum DialogState
	{
		START = 1,
		SEARCH,
		SUCCESS
	}

	[Serializable]
	private class FeatureUISettings
	{
		public PrivacyFeatures privacyFeature = PrivacyFeatures.INVALID;

		public GameObject enablePanel;

		public GameObject disablePanel;

		public string titleText;

		public string searchText;

		public string successText;
	}

	[SerializeField]
	private UIBButton m_continueButton;

	[SerializeField]
	private UIBButton m_choiceOneButton;

	[SerializeField]
	private UIBButton m_choiceTwoButton;

	[SerializeField]
	private GameObject m_continueButtonContainer;

	[SerializeField]
	private GameObject m_choiceButtonContainer;

	[SerializeField]
	private GameObject m_searchPanel;

	[SerializeField]
	private GameObject m_successPanel;

	[SerializeField]
	private UberText m_titleText;

	[SerializeField]
	private UberText m_searchText;

	[SerializeField]
	private UberText m_successText;

	[SerializeField]
	private List<FeatureUISettings> m_featureUISettings = new List<FeatureUISettings>();

	private FeatureUISettings m_currentFeatureUISettings;

	private GameObject m_activePanel;

	private DialogState m_activeState;

	private Action m_onAcceptCallback;

	private Action m_onSuccessCallback;

	private Action m_onCancelCallback;

	private Vector3 NORMAL_SCALE;

	private Vector3 HIDDEN_SCALE;

	private PegUIElement m_inputBlocker;

	private const float BLUR_TIME = 0.1f;

	private const float BUTTON_BLOCK_TIME = 0.5f;

	private float m_buttonBlockTimer;

	private bool m_buttonBlocked = true;

	protected override void Awake()
	{
		base.Awake();
		NORMAL_SCALE = base.transform.localScale;
		HIDDEN_SCALE = 0.01f * NORMAL_SCALE;
	}

	private void Update()
	{
		if (m_buttonBlockTimer >= 0f)
		{
			m_buttonBlockTimer -= Time.deltaTime;
			if (m_buttonBlockTimer < 0f)
			{
				m_buttonBlocked = false;
			}
		}
	}

	private void OnEnable()
	{
		m_continueButton.AddEventListener(UIEventType.RELEASE, OnContinueButton);
		m_choiceOneButton.AddEventListener(UIEventType.RELEASE, OnChoiceOneButton);
		m_choiceTwoButton.AddEventListener(UIEventType.RELEASE, OnChoiceTwoButton);
	}

	private void OnDisable()
	{
		m_continueButton.RemoveEventListener(UIEventType.RELEASE, OnContinueButton);
		m_choiceOneButton.RemoveEventListener(UIEventType.RELEASE, OnChoiceOneButton);
		m_choiceTwoButton.RemoveEventListener(UIEventType.RELEASE, OnChoiceTwoButton);
	}

	private void CreateInputBlocker()
	{
		GameObject inputBlockerObject = CameraUtils.CreateInputBlocker(CameraUtils.FindFirstByLayer(base.gameObject.layer), "PrivacyFeaturesInputBlocker");
		inputBlockerObject.transform.parent = base.gameObject.transform;
		m_inputBlocker = inputBlockerObject.AddComponent<PegUIElement>();
		m_inputBlocker.AddEventListener(UIEventType.RELEASE, delegate
		{
		});
		TransformUtil.SetPosY(m_inputBlocker, base.gameObject.transform.position.y - 0.1f);
	}

	private void OnChoiceOneButton(UIEvent e)
	{
		if (!m_buttonBlocked && m_activeState == DialogState.START)
		{
			m_onCancelCallback?.Invoke();
		}
	}

	private void OnChoiceTwoButton(UIEvent e)
	{
		if (!m_buttonBlocked && m_activeState == DialogState.START)
		{
			m_onAcceptCallback?.Invoke();
			m_onSuccessCallback?.Invoke();
		}
	}

	private void OnContinueButton(UIEvent e)
	{
		if (!m_buttonBlocked)
		{
			if (m_activeState == DialogState.SEARCH)
			{
				m_onCancelCallback?.Invoke();
			}
			else if (m_activeState == DialogState.SUCCESS)
			{
				m_onSuccessCallback?.Invoke();
			}
		}
	}

	private void OnStartState(bool isEnabled)
	{
		m_activePanel = (isEnabled ? m_currentFeatureUISettings.disablePanel : m_currentFeatureUISettings.enablePanel);
		m_activeState = DialogState.START;
		m_choiceOneButton.SetText(GameStrings.Get("GLOBAL_CANCEL"));
		m_choiceTwoButton.SetText(isEnabled ? "GLOBAL_AADC_FRIENDSETTINGS_TURN_OFF_BUTTON" : "GLOBAL_AADC_FRIENDSETTINGS_TURN_ON_BUTTON");
		m_continueButtonContainer.SetActive(value: false);
		m_choiceButtonContainer.SetActive(value: true);
		SetActivePanel();
	}

	private bool SetCurrentSettings(PrivacyFeatures privacyFeature)
	{
		FeatureUISettings panelSettings = m_featureUISettings.Find((FeatureUISettings x) => x.privacyFeature == privacyFeature);
		if (panelSettings == null)
		{
			Log.Privacy.PrintError("Privacy feature not supported in UI: " + privacyFeature);
			return false;
		}
		m_titleText.Text = panelSettings.titleText;
		m_searchText.Text = panelSettings.searchText;
		m_successText.Text = panelSettings.successText;
		m_currentFeatureUISettings = panelSettings;
		return true;
	}

	private void SetActivePanel()
	{
		m_currentFeatureUISettings.enablePanel.SetActive(m_currentFeatureUISettings.enablePanel == m_activePanel);
		m_currentFeatureUISettings.disablePanel.SetActive(m_currentFeatureUISettings.disablePanel == m_activePanel);
		m_searchPanel.SetActive(m_searchPanel == m_activePanel);
		m_successPanel.SetActive(m_successPanel == m_activePanel);
	}

	public void Set(PrivacyFeatures privacyFeature, bool isEnabled, Action acceptCallback, Action successCallback, Action cancelCallback)
	{
		if (!SetCurrentSettings(privacyFeature))
		{
			cancelCallback?.Invoke();
			return;
		}
		m_onAcceptCallback = acceptCallback;
		m_onSuccessCallback = successCallback;
		m_onCancelCallback = cancelCallback;
		OnStartState(isEnabled);
	}

	public override void Show()
	{
		base.Show();
		m_buttonBlockTimer = 0.5f;
		m_buttonBlocked = true;
		CreateInputBlocker();
		AnimationUtil.ShowWithPunch(base.gameObject, HIDDEN_SCALE, 1.1f * NORMAL_SCALE, NORMAL_SCALE, null, noFade: true);
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
		screenEffectParameters.Time = 0.1f;
		DialogBase.m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	public override void Hide()
	{
		DialogBase.m_screenEffectsHandle.StopEffect();
		base.Hide();
	}
}
