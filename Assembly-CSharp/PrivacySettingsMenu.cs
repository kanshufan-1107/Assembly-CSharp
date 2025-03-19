using System;
using Hearthstone.Util;
using UnityEngine;

public class PrivacySettingsMenu : MonoBehaviour
{
	[SerializeField]
	public CheckBox m_chatCheckbox;

	[SerializeField]
	public CheckBox m_personalizedShopOffersCheckbox;

	[SerializeField]
	public CheckBox m_nearbyFriendsCheckbox;

	[SerializeField]
	public CheckBox m_pushNotificationsCheckbox;

	[SerializeField]
	public UIBButton m_doneButton;

	[SerializeField]
	public UIBButton m_personalizedShopRulesButton;

	private string m_privacyPopupPrefab = "PrivacyPopups.prefab:99a8f571a8a35a54e90790c904bc94f8";

	private PegUIElement m_inputBlocker;

	private Vector3 NORMAL_SCALE;

	private Vector3 HIDDEN_SCALE;

	private static PrivacySettingsMenu s_instance;

	private void Awake()
	{
		s_instance = this;
		NORMAL_SCALE = base.transform.localScale;
		HIDDEN_SCALE = 0.01f * NORMAL_SCALE;
		OverlayUI.Get().AddGameObject(base.gameObject);
		CreateInputBlocker();
		SetupPrivacySettingsMenu();
	}

	public static PrivacySettingsMenu Get()
	{
		return s_instance;
	}

	private void OnEnable()
	{
		UpdateCheckboxes();
	}

	private void SetupPrivacySettingsMenu()
	{
		InitializeOptIn(PrivacyFeatures.CHAT);
		InitializeOptIn(PrivacyFeatures.PERSONALIZED_STORE_ITEMS);
		InitializeOptIn(PrivacyFeatures.NEARBY_FRIENDS);
		InitializeOptIn(PrivacyFeatures.PUSH_NOTIFICATIONS);
		if (m_doneButton != null)
		{
			m_doneButton.AddEventListener(UIEventType.RELEASE, delegate
			{
				Hide();
			});
		}
		if (m_personalizedShopRulesButton != null && RegionUtils.IsCNLegalRegion)
		{
			m_personalizedShopRulesButton.gameObject.SetActive(value: true);
			m_personalizedShopRulesButton.AddEventListener(UIEventType.RELEASE, OnPersonalizedShopRulesButtonReleased);
		}
	}

	private void UpdateCheckboxes()
	{
		if (m_chatCheckbox != null)
		{
			m_chatCheckbox.SetChecked(PrivacyGate.Get().FeatureEnabled(PrivacyFeatures.CHAT));
		}
		if (m_personalizedShopOffersCheckbox != null)
		{
			m_personalizedShopOffersCheckbox.SetChecked(PrivacyGate.Get().FeatureEnabled(PrivacyFeatures.PERSONALIZED_STORE_ITEMS));
		}
		if (m_nearbyFriendsCheckbox != null)
		{
			m_nearbyFriendsCheckbox.SetChecked(PrivacyGate.Get().FeatureEnabled(PrivacyFeatures.NEARBY_FRIENDS));
		}
		if (m_pushNotificationsCheckbox != null)
		{
			m_pushNotificationsCheckbox.SetChecked(PrivacyGate.Get().FeatureEnabled(PrivacyFeatures.PUSH_NOTIFICATIONS));
		}
	}

	private void InitializeOptIn(PrivacyFeatures privacyFeature)
	{
		CheckBox checkbox = null;
		switch (privacyFeature)
		{
		case PrivacyFeatures.CHAT:
			checkbox = m_chatCheckbox;
			break;
		case PrivacyFeatures.PERSONALIZED_STORE_ITEMS:
			checkbox = m_personalizedShopOffersCheckbox;
			break;
		case PrivacyFeatures.NEARBY_FRIENDS:
			checkbox = m_nearbyFriendsCheckbox;
			break;
		case PrivacyFeatures.PUSH_NOTIFICATIONS:
			if (PlatformSettings.IsMobile())
			{
				checkbox = m_pushNotificationsCheckbox;
			}
			else if (m_pushNotificationsCheckbox != null)
			{
				m_pushNotificationsCheckbox.gameObject.SetActive(value: false);
			}
			break;
		}
		if (!(checkbox == null))
		{
			checkbox.SetChecked(PrivacyGate.Get().FeatureEnabled(privacyFeature));
			checkbox.AddEventListener(UIEventType.RELEASE, delegate
			{
				OnCheckboxReleasedEvent(checkbox, privacyFeature);
			});
		}
	}

	private void OnCheckboxReleasedEvent(CheckBox checkbox, PrivacyFeatures privacyFeature)
	{
		GameObject popupGO = AssetLoader.Get().InstantiatePrefab(m_privacyPopupPrefab);
		PrivacyFeaturesPopup privacyPopup = popupGO.GetComponent<PrivacyFeaturesPopup>();
		bool isBoxChecked = checkbox.IsChecked();
		Action acceptPopupAction = delegate
		{
			PrivacyGate.Get().SetFeature(privacyFeature, isBoxChecked);
		};
		privacyPopup.Set(privacyFeature, !isBoxChecked, acceptPopupAction, delegate
		{
			OnPopupSuccess(privacyPopup);
		}, delegate
		{
			OnCancelPopup(privacyPopup, checkbox);
		});
		privacyPopup.Show();
	}

	private void OnPopupSuccess(PrivacyFeaturesPopup privacyPopup)
	{
		UpdateCheckboxes();
		privacyPopup.Hide();
		UnityEngine.Object.Destroy(privacyPopup.gameObject, 1f);
	}

	private void OnCancelPopup(PrivacyFeaturesPopup privacyPopup, CheckBox checkbox)
	{
		checkbox.SetChecked(!checkbox.IsChecked());
		privacyPopup.Hide();
		UnityEngine.Object.Destroy(privacyPopup.gameObject, 1f);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		AnimationUtil.ShowWithPunch(base.gameObject, HIDDEN_SCALE, 1.1f * NORMAL_SCALE, NORMAL_SCALE, null, noFade: true);
	}

	public bool IsShown()
	{
		return base.gameObject.activeSelf;
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void CreateInputBlocker()
	{
		GameObject inputBlocker = CameraUtils.CreateInputBlocker(CameraUtils.FindFirstByLayer(base.gameObject.layer), "OptionMenuInputBlocker", this, base.transform, 10f);
		inputBlocker.layer = base.gameObject.layer;
		m_inputBlocker = inputBlocker.AddComponent<PegUIElement>();
		m_inputBlocker.AddEventListener(UIEventType.RELEASE, delegate
		{
			Hide();
		});
	}

	private void OnPersonalizedShopRulesButtonReleased(UIEvent e)
	{
		Application.OpenURL(ExternalUrlService.Get().GetPersonalizedShopRulesLink());
	}
}
