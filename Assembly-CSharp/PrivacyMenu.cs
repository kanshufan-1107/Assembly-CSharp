using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Attribution;
using Hearthstone.Core;
using Hearthstone.Http;
using Hearthstone.Privacy.CN.PersonalInfo;
using Hearthstone.Privacy.UI;
using Hearthstone.UI;
using Hearthstone.Util;
using UnityEngine;

[CustomEditClass]
public class PrivacyMenu : ButtonListMenu
{
	public Transform m_menuBone;

	[CustomEditField(Sections = "Do Not Share", Label = "Widget", T = EditType.GAME_OBJECT)]
	public AsyncReference m_doNotSellWidget;

	[CustomEditField(Sections = "Do Not Share", Label = "Position Offset")]
	public float m_doNotSellPositionOffset = -0.3f;

	[CustomEditField(Sections = "Phone Long List Overrides", Label = "Z Position")]
	public int m_longPositionOffset = 65;

	[CustomEditField(Sections = "Phone Long List Overrides", Label = "Scale")]
	public Vector3 m_longScale = new Vector3(150f, 150f, 150f);

	private static PrivacyMenu s_instance;

	private PrivacySettingsMenu m_privacySettingsMenu;

	private UIBButton m_privacyRightsButton;

	private UIBButton m_privacyPolicyButton;

	private UIBButton m_privacySettingsButton;

	private UIBButton m_accountDeletionButton;

	private UIBButton m_cnSdkButton;

	private UIBButton m_cnPersonalInfoButton;

	private UIBButton m_cnRegistrationButton;

	private ExternalUrlService m_urlService;

	public static readonly AssetReference OPTIONS_MENU_PRIVACY_SETTINGS = new AssetReference("OptionsMenuPrivacySettings.prefab:bb3df91bd8b46004db4fb741957b1eb4");

	public static readonly AssetReference OPTIONS_MENU_PRIVACY_SETTINGS_PHONE = new AssetReference("OptionsMenuPrivacySettingsWithDeviceSettings.prefab:c524014467fe1604eac8d35a4952b7e1");

	private static readonly PlatformDependentValue<int> DONOTSELL_BACKGROUND_WIDTH_EXTENSION = new PlatformDependentValue<int>(PlatformCategory.Screen)
	{
		PC = 50,
		Phone = 100
	};

	private const int CCPA_OPT_IN_VALUE = 7;

	private PersonalInfoUrl m_personalInfoUrl;

	private bool IsOpeningDataManagementLink { get; set; }

	private bool IsAccountDeletionEnabled
	{
		get
		{
			if (PlatformSettings.OS != OSCategory.Android)
			{
				return PlatformSettings.OS == OSCategory.iOS;
			}
			return true;
		}
	}

	private bool IsDoNotSellEnabled
	{
		get
		{
			if (PlatformSettings.IsMobile())
			{
				return !RegionUtils.IsCNLegalRegion;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		m_menuParent = m_menuBone;
		m_targetLayer = GameLayer.HighPriorityUI;
		m_showAnimation = false;
		m_urlService = ServiceManager.Get<ExternalUrlService>();
		base.Awake();
		s_instance = this;
		bool isCNLegalRegion = RegionUtils.IsCNLegalRegion;
		if (!isCNLegalRegion)
		{
			m_privacyRightsButton = CreateMenuButton("PrivacyRightsButton", "GLOBAL_AADC_PRIVACYSETTINGSMENU_PRIVACYRIGHTS", OnPrivacyRightsButtonReleased);
		}
		m_privacyPolicyButton = CreateMenuButton("PrivacyPolicyButton", "GLUE_PRIVACY_POLICY_TITLE", OnPrivacyPolicyButtonReleased);
		m_privacySettingsButton = CreateMenuButton("PrivacySettingsButton", "GLOBAL_AADC_BUTTON_PRIVACYSETTINGS", OnPrivacySettingsButtonReleased);
		m_accountDeletionButton = (IsAccountDeletionEnabled ? CreateMenuButton("AccountDeletionButton", "GLUE_DELETE_ACCOUNT", OnAccountDeleteButtonReleased) : null);
		if (isCNLegalRegion)
		{
			m_cnSdkButton = CreateMenuButton("CNSDKButton", "GLOBAL_PRIVACY_SDK_BUTTON_CN", OnCNSDKButtonReleased);
			m_cnPersonalInfoButton = CreateMenuButton("CNPersonalInfoButton", "GLOBAL_PRIVACY_PERSONAL_INFO_BUTTON_CN", OnCNPersonalInfoButtonReleased);
			m_cnRegistrationButton = CreateMenuButton("CNRegistrationButton", "GLOBAL_PRIVACY_REGISTRATION_BUTTON_CN", OnCNRegistrationButtonReleased);
			string endpoint = m_urlService.GetPersonalInfoLink();
			m_personalInfoUrl = new PersonalInfoUrl(HttpRequestFactory.Get(), endpoint);
			if (PlatformSettings.Screen == ScreenCategory.Phone)
			{
				TransformUtil.SetLocalPosZ(m_menuBone, m_longPositionOffset);
				m_menuBone.localScale = m_longScale;
			}
		}
		m_menu.m_headerText.Text = GameStrings.Get("GLOBAL_AADC_BUTTON_PRIVACY");
		m_doNotSellWidget.RegisterReadyListener<Widget>(OnDoNotSellWidgetReady);
	}

	private void OnDoNotSellWidgetReady(Widget dnsWidget)
	{
		if (!IsDoNotSellEnabled)
		{
			dnsWidget.gameObject.SetActive(value: false);
			return;
		}
		DoNotSellView componentInChildren = dnsWidget.GetComponentInChildren<DoNotSellView>();
		componentInChildren.OnDoNotSellPressed += OnDoNotSellPressed;
		componentInChildren.OnMoreInfoPressed += OnDNSMoreInfoPressed;
	}

	public static PrivacyMenu Get()
	{
		return s_instance;
	}

	protected override List<UIBButton> GetButtons()
	{
		List<UIBButton> buttons = new List<UIBButton>();
		buttons.Add(m_privacyRightsButton);
		buttons.Add(m_privacyPolicyButton);
		buttons.Add(m_privacySettingsButton);
		if (m_accountDeletionButton != null)
		{
			buttons.Add(m_accountDeletionButton);
		}
		if (m_cnRegistrationButton != null)
		{
			buttons.Add(m_cnRegistrationButton);
		}
		if (m_cnSdkButton != null)
		{
			buttons.Add(m_cnSdkButton);
		}
		if (m_cnPersonalInfoButton != null)
		{
			buttons.Add(m_cnPersonalInfoButton);
		}
		return buttons;
	}

	public override void Show(bool playSound = true)
	{
		base.Show(playSound);
		AnimationUtil.ShowWithPunch(base.gameObject, ButtonListMenu.HIDDEN_SCALE, PUNCH_SCALE * NORMAL_SCALE, NORMAL_SCALE, null, noFade: true);
	}

	protected override void LayoutMenu()
	{
		LayoutMenuButtons();
		if (IsDoNotSellEnabled)
		{
			m_menu.m_buttonContainer.m_localPinnedPointOffset.x = m_doNotSellPositionOffset;
		}
		m_menu.m_buttonContainer.UpdateSlices();
		LayoutExtendedBackground();
	}

	private void LayoutExtendedBackground()
	{
		OrientedBounds orientedBounds = TransformUtil.ComputeOrientedWorldBounds(m_menu.m_buttonContainer.gameObject, default(Vector3), default(Vector3), null, includeAllChildren: true, includeMeshRenderers: true, includeWidgetTransformBounds: true);
		int widthAdjustment = (IsDoNotSellEnabled ? ((int)DONOTSELL_BACKGROUND_WIDTH_EXTENSION) : 0);
		float width = (orientedBounds.Extents[0].magnitude + (float)widthAdjustment) * 2f;
		float height = orientedBounds.Extents[2].magnitude * 2f;
		m_menu.m_background.SetSize(width, height);
		m_menu.m_border.SetSize(width, height);
	}

	private IEnumerator<IAsyncJobResult> Job_OpenDataManagementLink()
	{
		GenerateSSOToken tokenGenerator = new GenerateSSOToken();
		yield return tokenGenerator;
		if (!tokenGenerator.HasToken)
		{
			yield return new JobFailedResult("Could not generate SSO token to open data management link");
		}
		Application.OpenURL(m_urlService.GetDataManagementLink(tokenGenerator.Token));
	}

	private void OnPrivacyRightsButtonReleased(UIEvent e)
	{
		if (!IsOpeningDataManagementLink)
		{
			IsOpeningDataManagementLink = true;
			Processor.QueueJob("OpenDataManagementLink", Job_OpenDataManagementLink()).AddJobFinishedEventListener(delegate
			{
				IsOpeningDataManagementLink = false;
			});
		}
	}

	private void OnPrivacyPolicyButtonReleased(UIEvent e)
	{
		Application.OpenURL(m_urlService.GetPrivacyPolicyLink());
	}

	private void OnPrivacySettingsButtonReleased(UIEvent e)
	{
		Hide();
		if (m_privacySettingsMenu == null)
		{
			if (PlatformSettings.IsMobile())
			{
				m_privacySettingsMenu = AssetLoader.Get().InstantiatePrefab(OPTIONS_MENU_PRIVACY_SETTINGS_PHONE).GetComponent<PrivacySettingsMenu>();
			}
			else
			{
				m_privacySettingsMenu = AssetLoader.Get().InstantiatePrefab(OPTIONS_MENU_PRIVACY_SETTINGS).GetComponent<PrivacySettingsMenu>();
			}
		}
		m_privacySettingsMenu.Show();
	}

	private void OnAccountDeleteButtonReleased(UIEvent e)
	{
		if (TemporaryAccountManager.IsTemporaryAccount())
		{
			Processor.QueueJobIfNotExist("Job_OpenSoftAccountDeletionLink", Job_OpenSoftAccountDeletionLink());
		}
		else
		{
			Application.OpenURL(m_urlService.GetAccountDeletionLink());
		}
	}

	private IEnumerator<IAsyncJobResult> Job_OpenSoftAccountDeletionLink()
	{
		GenerateSSOToken tokenGenerator = new GenerateSSOToken();
		yield return tokenGenerator;
		if (!tokenGenerator.HasToken)
		{
			yield return new JobFailedResult("Could not generate SSO token to open account deletion link");
		}
		Application.OpenURL(m_urlService.GetSoftAccountDeletionLink(tokenGenerator.Token));
	}

	private void OnDoNotSellPressed()
	{
		if (ServiceManager.TryGet<NetworkReachabilityManager>(out var networkReachability))
		{
			if (!networkReachability.InternetAvailable_Cached)
			{
				ShowNoInternetError();
			}
			else
			{
				RequestDoNotSell();
			}
		}
	}

	private void ShowNoInternetError()
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_PRIVACY_DNS_ERROR_TITLE"),
			m_text = GameStrings.Get("GLUE_PRIVACY_DNS_ERROR_BODY"),
			m_showAlertIcon = true,
			m_responseDisplay = AlertPopup.ResponseDisplay.OK
		};
		DialogManager.Get().ShowPopup(info);
	}

	private void RequestDoNotSell()
	{
		BlizzardAttributionManager.Get().OptOutOfDataSharing();
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_PRIVACY_DNS_REQUEST_TITLE"),
			m_text = GameStrings.Get("GLUE_PRIVACY_DNS_REQUEST_PROCESSING"),
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.NONE
		};
		DialogManager.Get().ShowPopup(info, OnSharingDialogProcessed);
	}

	private bool OnSharingDialogProcessed(DialogBase dialog, object _)
	{
		SetOptInSharingSettingAsync(delegate(bool success)
		{
			Processor.MainThreadContext.Post(delegate
			{
				HandleOptInSharingResult(dialog, success);
			}, null);
		});
		return true;
	}

	private static void HandleOptInSharingResult(DialogBase dialog, bool success)
	{
		AlertPopup obj = dialog as AlertPopup;
		AlertPopup.PopupInfo finalinfo = obj.GetInfo();
		finalinfo.m_text = GameStrings.Get(success ? "GLUE_PRIVACY_DNS_REQUEST_BODY" : "GLUE_PRIVACY_DNS_REQUEST_ERROR");
		finalinfo.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		obj.UpdateInfo(finalinfo);
	}

	private void SetOptInSharingSettingAsync(Action<bool> callback)
	{
		if (!ServiceManager.TryGet<LoginManager>(out var loginManager))
		{
			callback?.Invoke(obj: false);
		}
		loginManager.OptInApi.UncachedSetOptInAsync(7, value: true, callback);
	}

	private void OnDNSMoreInfoPressed()
	{
		Application.OpenURL(m_urlService.GetDoNotSellMoreInfo());
	}

	private void OnCNRegistrationButtonReleased(UIEvent e)
	{
		Application.OpenURL(m_urlService.GetRegistrationInfoLink());
	}

	private void OnCNPersonalInfoButtonReleased(UIEvent e)
	{
		e.GetElement().SetEnabled(enabled: false);
		m_personalInfoUrl.OpenPersonalInfoUrl(delegate(bool success)
		{
			if (!success)
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
				{
					m_attentionCategory = UserAttentionBlocker.ALL_EXCEPT_FATAL_ERROR_SCENE,
					m_headerText = GameStrings.Get("GLOBAL_PRIVACY_PERSONAL_INFO_ERROR_TITLE"),
					m_text = GameStrings.Get("GLOBAL_PRIVACY_PERSONAL_INFO_ERROR_BODY"),
					m_showAlertIcon = true,
					m_responseDisplay = AlertPopup.ResponseDisplay.OK,
					m_alertTextAlignment = UberText.AlignmentOptions.Center
				};
				DialogManager.Get().ShowPopup(info);
			}
			e.GetElement().SetEnabled(enabled: true);
		});
	}

	private void OnCNSDKButtonReleased(UIEvent e)
	{
		Application.OpenURL(m_urlService.GetSdkInfoLink());
	}
}
