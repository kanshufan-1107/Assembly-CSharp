using System;
using System.Collections.Generic;
using Blizzard.T5.Configuration;
using Blizzard.T5.Fonts;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Streaming;
using UnityEngine;

[CustomEditClass]
public class OptionsMenu : MonoBehaviour
{
	public delegate void hideHandler();

	[CustomEditField(Sections = "Layout")]
	public MultiSliceElement m_leftPane;

	[CustomEditField(Sections = "Layout")]
	public MultiSliceElement m_rightPane;

	[CustomEditField(Sections = "Layout")]
	public MultiSliceElement m_middlePane;

	[CustomEditField(Sections = "Layout")]
	public MultiSliceElement m_middleBottomPane;

	[CustomEditField(Sections = "Layout")]
	public MultiSliceElement m_middleBottomLeftPane;

	[CustomEditField(Sections = "Layout")]
	public MultiSliceElement m_middleBottomRightPane;

	[CustomEditField(Sections = "Placeholder")]
	public GameObject m_middleLeftPaneLabel;

	[CustomEditField(Sections = "Placeholder")]
	public GameObject m_middleRightPaneLabel;

	[CustomEditField(Sections = "Graphics")]
	public GameObject m_graphicsGroup;

	[CustomEditField(Sections = "Graphics")]
	public DropdownControl m_graphicsRes;

	[CustomEditField(Sections = "Graphics")]
	public DropdownControl m_graphicsQuality;

	[CustomEditField(Sections = "Graphics")]
	public DropdownControl m_graphicsFps;

	[CustomEditField(Sections = "Graphics")]
	public CheckBox m_fullScreenCheckbox;

	[CustomEditField(Sections = "Sound")]
	public GameObject m_soundGroup;

	[CustomEditField(Sections = "Sound")]
	public ScrollbarControl m_masterVolume;

	[CustomEditField(Sections = "Sound")]
	public ScrollbarControl m_musicVolume;

	[CustomEditField(Sections = "Sound")]
	public AudioSliderAssetReferences m_audioSliderAssetReferences;

	[CustomEditField(Sections = "Language")]
	public GameObject m_languageGroup;

	[CustomEditField(Sections = "Language")]
	public DropdownControl m_languageDropdown;

	[CustomEditField(Sections = "Language")]
	public FontDefinition m_languageDropdownFont;

	[CustomEditField(Sections = "Language")]
	public CheckBox m_languagePackCheckbox;

	[CustomEditField(Sections = "Other")]
	public CheckBox m_spectatorOpenJoinCheckbox;

	[CustomEditField(Sections = "Other")]
	public CheckBox m_screenShakeCheckbox;

	[CustomEditField(Sections = "Other")]
	public UIBButton m_switchAccountButton;

	[CustomEditField(Sections = "Other")]
	public UIBButton m_miscellaneousButton;

	[CustomEditField(Sections = "Other")]
	public UIBButton m_privacyButton;

	[CustomEditField(Sections = "Other")]
	public UIBButton m_soundOptionsButton;

	[CustomEditField(Sections = "Internal Stuff")]
	public UberText m_versionLabel;

	private static OptionsMenu s_instance;

	private bool m_isShown;

	private hideHandler m_hideHandler;

	private MiscellaneousMenu m_miscellaneousMenu;

	private bool m_miscellaneousMenuLoading;

	private PrivacyMenu m_privacyMenu;

	private bool m_privacyMenuLoading;

	private SoundOptionsMenu m_soundOptionsMenu;

	private bool m_soundOptionsMenuLoading;

	private PegUIElement m_inputBlocker;

	private RegionSwitchMenuController m_controller = new RegionSwitchMenuController();

	private IGraphicsManager m_graphicsManager;

	private List<GraphicsResolution> m_fullScreenResolutions = new List<GraphicsResolution>();

	private Vector3 NORMAL_SCALE;

	private Vector3 HIDDEN_SCALE;

	private readonly PlatformDependentValue<bool> LANGUAGE_SELECTION = new PlatformDependentValue<bool>(PlatformCategory.OS)
	{
		iOS = true,
		Android = true,
		PC = false,
		Mac = false
	};

	private IGameDownloadManager DownloadManager => GameDownloadManagerProvider.Get();

	private void Awake()
	{
		s_instance = this;
		NORMAL_SCALE = base.transform.localScale;
		HIDDEN_SCALE = 0.01f * NORMAL_SCALE;
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
		OverlayUI.Get().AddGameObject(base.gameObject);
		m_graphicsManager = ServiceManager.Get<IGraphicsManager>();
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_graphicsRes.setUnselectedItemText(GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_RESOLUTION_CUSTOM"));
			m_graphicsRes.setItemTextCallback(OnGraphicsResolutionDropdownText);
			m_graphicsRes.setItemChosenCallback(OnNewGraphicsResolution);
			foreach (GraphicsResolution res in GetGoodGraphicsResolution())
			{
				m_graphicsRes.addItem(res);
			}
			m_graphicsRes.setSelection(GetCurrentGraphicsResolution());
			m_fullScreenCheckbox.AddEventListener(UIEventType.RELEASE, OnToggleFullScreenCheckbox);
			m_fullScreenCheckbox.SetChecked(Screen.fullScreen);
			m_graphicsQuality.addItem(GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_LOW"));
			m_graphicsQuality.addItem(GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_MEDIUM"));
			m_graphicsQuality.addItem(GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_HIGH"));
			m_graphicsQuality.setSelection(GetCurrentGraphicsQuality());
			m_graphicsQuality.setItemChosenCallback(OnNewGraphicsQuality);
		}
		m_graphicsFps.addItem(GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_FPS_DEFAULT"));
		if (Screen.currentResolution.refreshRate > 60)
		{
			m_graphicsFps.addItem(GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_FPS_MEDIUM"));
		}
		m_graphicsFps.addItem(GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_FPS_HIGH"));
		m_graphicsFps.setSelection(GetCurrentGraphicsFps());
		m_graphicsFps.setItemChosenCallback(OnNewGraphicsFps);
		m_graphicsFps.gameObject.SetActive(value: true);
		VolumeSlidersUtils.InitializeVolumeSlider(m_masterVolume, Option.SOUND_VOLUME, m_audioSliderAssetReferences.m_onMasterVolumeReleasedAudio, base.gameObject);
		VolumeSlidersUtils.InitializeVolumeSlider(m_musicVolume, Option.MUSIC_VOLUME, m_audioSliderAssetReferences.m_onMusicVolumeReleasedAudio, base.gameObject);
		m_languageGroup.gameObject.SetActive(LANGUAGE_SELECTION);
		if ((bool)LANGUAGE_SELECTION && (DownloadManager == null || !DownloadManager.ShouldNotDownloadOptionalData))
		{
			m_languageDropdown.setFont(m_languageDropdownFont.m_Font);
			foreach (Locale locale in Enum.GetValues(typeof(Locale)))
			{
				if (locale != Locale.UNKNOWN && (PlatformSettings.LocaleVariant != LocaleVariant.China || locale == Locale.enUS || locale == Locale.zhCN))
				{
					m_languageDropdown.addItem(GameStrings.Get(StringNameFromLocale(locale)));
				}
			}
			m_languageDropdown.setSelection(GetCurrentLanguage());
			m_languageDropdown.setItemChosenCallback(OnNewLanguage);
		}
		UpdateOtherUI();
		if (TemporaryAccountManager.IsTemporaryAccount())
		{
			m_spectatorOpenJoinCheckbox.gameObject.SetActive(value: false);
			m_switchAccountButton.gameObject.SetActive(value: true);
			m_switchAccountButton.AddEventListener(UIEventType.RELEASE, OnSwitchAccountButtonReleased);
		}
		else
		{
			m_spectatorOpenJoinCheckbox.AddEventListener(UIEventType.RELEASE, ToggleSpectatorOpenJoin);
			m_spectatorOpenJoinCheckbox.SetChecked(Options.Get().GetBool(Option.SPECTATOR_OPEN_JOIN));
		}
		if (m_screenShakeCheckbox != null)
		{
			m_screenShakeCheckbox.AddEventListener(UIEventType.RELEASE, ToggleScreenShake);
			m_screenShakeCheckbox.SetChecked(Options.Get().GetBool(Option.SCREEN_SHAKE_ENABLED));
		}
		m_miscellaneousButton.AddEventListener(UIEventType.RELEASE, OnMiscellaneousButtonReleased);
		m_privacyButton.AddEventListener(UIEventType.RELEASE, OnPrivacyButtonReleased);
		m_soundOptionsButton.AddEventListener(UIEventType.RELEASE, OnSoundOptionsButtonReleased);
		CreateInputBlocker();
		ShowOrHide(showOrHide: false);
		if (PlatformSettings.IsMobile())
		{
			if (m_graphicsRes != null)
			{
				m_graphicsRes.gameObject.SetActive(value: false);
			}
			if (m_graphicsQuality != null)
			{
				m_graphicsQuality.gameObject.SetActive(value: false);
			}
			if (m_fullScreenCheckbox != null)
			{
				m_fullScreenCheckbox.gameObject.SetActive(value: false);
			}
			string versionText = string.Format("{0} {1}.{2}", GameStrings.Get("GLOBAL_VERSION"), "31.6", 216423);
			string referral = Vars.Key("Application.Referral").GetStr("none");
			if (referral != "none")
			{
				versionText = versionText + "-" + referral;
			}
			m_versionLabel.Text = versionText;
			m_versionLabel.gameObject.SetActive(value: true);
		}
		UpdateUI();
		m_graphicsGroup.GetComponent<MultiSliceElement>().UpdateSlices();
		m_graphicsManager.OnResolutionChangedEvent += UpdateMenuItemValues;
	}

	public void OnDestroy()
	{
		if (FatalErrorMgr.Get() != null)
		{
			FatalErrorMgr.Get().RemoveErrorListener(OnFatalError);
		}
		if (m_graphicsManager != null)
		{
			m_graphicsManager.OnResolutionChangedEvent -= UpdateMenuItemValues;
		}
		s_instance = null;
	}

	public static OptionsMenu Get()
	{
		return s_instance;
	}

	public hideHandler GetHideHandler()
	{
		return m_hideHandler;
	}

	public void SetHideHandler(hideHandler handler)
	{
		m_hideHandler = handler;
	}

	public void RemoveHideHandler(hideHandler handler)
	{
		if (m_hideHandler == handler)
		{
			m_hideHandler = null;
		}
	}

	public bool IsShown()
	{
		return m_isShown;
	}

	public void Show()
	{
		UpdateOtherUI();
		ShowOrHide(showOrHide: true);
		AnimationUtil.ShowWithPunch(base.gameObject, HIDDEN_SCALE, 1.1f * NORMAL_SCALE, NORMAL_SCALE, null, noFade: true);
	}

	public void Hide(bool callHideHandler = true)
	{
		ShowOrHide(showOrHide: false);
		if (m_hideHandler != null && callHideHandler)
		{
			m_hideHandler();
			m_hideHandler = null;
		}
	}

	private GraphicsResolution GetCurrentGraphicsResolution()
	{
		int @int = Options.Get().GetInt(Option.GFX_WIDTH, Screen.currentResolution.width);
		int height = Options.Get().GetInt(Option.GFX_HEIGHT, Screen.currentResolution.height);
		return GraphicsResolution.create(@int, height);
	}

	private string GetCurrentGraphicsQuality()
	{
		return (GraphicsQuality)Options.Get().GetInt(Option.GFX_QUALITY) switch
		{
			GraphicsQuality.Low => GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_LOW"), 
			GraphicsQuality.Medium => GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_MEDIUM"), 
			GraphicsQuality.High => GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_HIGH"), 
			_ => GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_LOW"), 
		};
	}

	private string GetCurrentGraphicsFps()
	{
		int fps = Options.Get().GetInt(Option.GFX_TARGET_FRAME_RATE);
		if (fps == 30)
		{
			return GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_FPS_DEFAULT");
		}
		if (Screen.currentResolution.refreshRate == fps)
		{
			return GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_FPS_HIGH");
		}
		return GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_FPS_MEDIUM");
	}

	private List<GraphicsResolution> GetGoodGraphicsResolution()
	{
		if (m_fullScreenResolutions.Count == 0)
		{
			foreach (GraphicsResolution res in GraphicsResolution.list)
			{
				if (res.x >= 1024 && res.y >= 728 && !((double)res.aspectRatio - 0.01 > 1.7777777777777777) && !((double)res.aspectRatio + 0.01 < 1.3333333333333333))
				{
					m_fullScreenResolutions.Add(res);
				}
			}
		}
		return m_fullScreenResolutions;
	}

	private string GetCurrentLanguage()
	{
		return GameStrings.Get(StringNameFromLocale(Localization.GetLocale()));
	}

	private void ShowOrHide(bool showOrHide)
	{
		m_isShown = showOrHide;
		base.gameObject.SetActive(showOrHide);
		UpdateUI();
	}

	private string StringNameFromLocale(Locale locale)
	{
		return "GLOBAL_LANGUAGE_NATIVE_" + locale.ToString().ToUpper();
	}

	private void UpdateOtherUI()
	{
		bool canShowOtherMenuOptions = CanShowOtherMenuOptions();
		m_miscellaneousButton.gameObject.SetActive(canShowOtherMenuOptions);
		m_middleBottomRightPane.gameObject.SetActive(value: true);
	}

	private void UpdateUI()
	{
		m_middleLeftPaneLabel.SetActive(value: true);
		m_middleRightPaneLabel.SetActive(value: true);
		m_middleBottomLeftPane.UpdateSlices();
		m_middleBottomRightPane.UpdateSlices();
		m_middleBottomPane.UpdateSlices();
		m_leftPane.UpdateSlices();
		m_rightPane.UpdateSlices();
		m_middlePane.UpdateSlices();
		m_middleLeftPaneLabel.SetActive(value: false);
		m_middleRightPaneLabel.SetActive(value: false);
	}

	private bool CanShowOtherMenuOptions()
	{
		if (UserAttentionManager.GetAvailabilityBlockerReason(isFriendlyChallenge: false) != 0)
		{
			return false;
		}
		if (SceneMgr.Get().IsModeRequested(SceneMgr.Mode.PACKOPENING))
		{
			return false;
		}
		if (SceneMgr.Get().IsModeRequested(SceneMgr.Mode.ADVENTURE))
		{
			return false;
		}
		if (SceneMgr.Get().IsModeRequested(SceneMgr.Mode.CREDITS))
		{
			return false;
		}
		if (SceneMgr.Get().IsModeRequested(SceneMgr.Mode.FRIENDLY))
		{
			return false;
		}
		return true;
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

	private void UpdateMenuItemValues(int newWidth, int newHeight)
	{
		if (m_fullScreenCheckbox.IsChecked() != Screen.fullScreen)
		{
			m_fullScreenCheckbox.SetChecked(Screen.fullScreen);
			GraphicsResolution res = m_graphicsRes.getSelection() as GraphicsResolution;
			if (res == null || m_fullScreenCheckbox.IsChecked())
			{
				m_graphicsRes.setSelectionToFirstItem();
				res = m_graphicsRes.getSelection() as GraphicsResolution;
			}
			else if (!m_fullScreenCheckbox.IsChecked())
			{
				res = GraphicsResolution.create(newWidth, newHeight);
			}
			if (res != null)
			{
				int width = res.x;
				int height = res.y;
				m_graphicsRes.setSelection(GraphicsResolution.create(width, height));
			}
		}
		else if (!Screen.fullScreen)
		{
			m_graphicsRes.setSelection(GraphicsResolution.create(newWidth, newHeight));
		}
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		if (SceneMgr.Get().GetNextMode() == SceneMgr.Mode.FATAL_ERROR)
		{
			Hide();
		}
	}

	private void OnToggleFullScreenCheckbox(UIEvent e)
	{
		if (m_fullScreenCheckbox.IsChecked() == Screen.fullScreen)
		{
			return;
		}
		if (m_fullScreenCheckbox.IsChecked())
		{
			m_graphicsManager.SetFullScreen();
			return;
		}
		int width = 0;
		int height = 0;
		GraphicsResolution res = m_graphicsRes.getSelection() as GraphicsResolution;
		if (res == null)
		{
			m_graphicsRes.setSelectionToFirstItem();
			res = m_graphicsRes.getSelection() as GraphicsResolution;
		}
		if (res != null)
		{
			width = res.x;
			height = res.y;
		}
		m_graphicsManager.SetWindowedScreen(width, height);
	}

	private void OnNewGraphicsQuality(object selection, object prevSelection)
	{
		GraphicsQuality quality = GraphicsQuality.Low;
		string selected = (string)selection;
		if (selected == GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_LOW"))
		{
			quality = GraphicsQuality.Low;
		}
		else if (selected == GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_MEDIUM"))
		{
			quality = GraphicsQuality.Medium;
		}
		else if (selected == GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_QUALITY_HIGH"))
		{
			quality = GraphicsQuality.High;
		}
		Log.Options.Print("Graphics Quality: " + quality);
		m_graphicsManager.RenderQualityLevel = quality;
	}

	private void OnNewGraphicsResolution(object selection, object prevSelection)
	{
		GraphicsResolution res = (GraphicsResolution)selection;
		m_graphicsManager.SetScreenResolution(res.x, res.y, m_fullScreenCheckbox.IsChecked());
	}

	private void OnNewLanguage(object selection, object prevSelection)
	{
		if (selection != prevSelection)
		{
			long num = FreeSpace.Measure();
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			if (num < 314572800)
			{
				info.m_headerText = GameStrings.Get("GLOBAL_LANGUAGE_CHANGE_OUT_OF_SPACE_TITLE");
				info.m_text = string.Format(GameStrings.Get("GLOBAL_LANGUAGE_CHANGE_OUT_OF_SPACE_MESSAGE"), DownloadUtils.FormatBytesAsHumanReadable(314572800L));
				info.m_showAlertIcon = false;
				info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			}
			else
			{
				info.m_headerText = GameStrings.Get("GLOBAL_LANGUAGE_CHANGE_CONFIRM_TITLE");
				info.m_text = GameStrings.Get("GLOBAL_LANGUAGE_CHANGE_CONFIRM_MESSAGE");
				info.m_showAlertIcon = false;
				info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
				info.m_responseCallback = OnChangeLanguageConfirmationResponse;
				info.m_responseUserData = selection;
			}
			DialogManager.Get().ClearAllImmediatelyDontDestroy();
			DialogManager.Get().ShowPopup(info);
		}
	}

	private void OnNewGraphicsFps(object selection, object prevSelection)
	{
		string selected = (string)selection;
		if (string.Equals(selected, GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_FPS_DEFAULT")))
		{
			m_graphicsManager.UpdateTargetFramerate(30, dynamicFps: true);
		}
		else if (string.Equals(selected, GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_FPS_HIGH")))
		{
			m_graphicsManager.UpdateTargetFramerate(Screen.currentResolution.refreshRate, dynamicFps: false);
		}
		else if (string.Equals(selected, GameStrings.Get("GLOBAL_OPTIONS_GRAPHICS_FPS_MEDIUM")))
		{
			m_graphicsManager.UpdateTargetFramerate(60, dynamicFps: false);
		}
	}

	private void OnChangeLanguageConfirmationResponse(AlertPopup.Response response, object userData)
	{
		if (response == AlertPopup.Response.CANCEL)
		{
			m_languageDropdown.setSelection(GetCurrentLanguage());
			return;
		}
		string newLanguage = (string)userData;
		Locale newLocale = Locale.UNKNOWN;
		foreach (Locale locale in Enum.GetValues(typeof(Locale)))
		{
			if (newLanguage == GameStrings.Get(StringNameFromLocale(locale)))
			{
				newLocale = locale;
				break;
			}
		}
		if (newLocale == Locale.UNKNOWN)
		{
			Debug.LogError($"OptionsMenu.OnChangeLanguageConfirmationResponse() - locale not found");
			return;
		}
		TelemetryManager.Client().SendLanguageChanged(Localization.GetLocaleName(), newLocale.ToString());
		Localization.SetLocale(newLocale);
		Options.Get().SetString(Option.LOCALE, newLocale.ToString());
		Debug.LogFormat("Change Locale: {0}", newLocale);
		Hide(callHideHandler: false);
		HearthstoneApplication.Get().IsLocaleChanged = true;
		if (DownloadManager.ShouldDownloadLocalizedAssets)
		{
			HearthstoneApplication.Get().Resetting += StartUpdateProcessAfterReset;
		}
		HearthstoneApplication.Get().Reset();
	}

	private void StartUpdateProcessAfterReset()
	{
		DbfShared.Reset();
		HearthstoneApplication.Get().Resetting -= StartUpdateProcessAfterReset;
		DownloadManager.StartUpdateProcess(localeChange: true, resetDownloadFinishReport: true);
	}

	private string OnGraphicsResolutionDropdownText(object val)
	{
		GraphicsResolution res = (GraphicsResolution)val;
		return $"{res.x} x {res.y}";
	}

	private void OnSwitchAccountButtonReleased(UIEvent e)
	{
		Hide(callHideHandler: false);
		m_controller.ShowRegionMenuWithDefaultSettings();
	}

	private void ToggleSpectatorOpenJoin(UIEvent e)
	{
		Options.Get().SetBool(Option.SPECTATOR_OPEN_JOIN, m_spectatorOpenJoinCheckbox.IsChecked());
	}

	private void ToggleScreenShake(UIEvent e)
	{
		Options.Get().SetBool(Option.SCREEN_SHAKE_ENABLED, m_screenShakeCheckbox.IsChecked());
	}

	private void OnMiscellaneousButtonReleased(UIEvent e)
	{
		LoadMiscellaneousMenu();
		Hide(callHideHandler: false);
	}

	private void LoadMiscellaneousMenu()
	{
		if (!m_miscellaneousMenuLoading)
		{
			if (m_miscellaneousMenu == null)
			{
				m_miscellaneousMenuLoading = true;
				AssetLoader.Get().InstantiatePrefab("MiscellaneousMenu.prefab:ee334ff827a9f834ea8b96e3dd2f5c5d", ShowMiscellaneousMenu);
			}
			else
			{
				m_miscellaneousMenu.Show(playSound: false);
			}
		}
	}

	private void ShowMiscellaneousMenu(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_miscellaneousMenu = go.GetComponent<MiscellaneousMenu>();
		m_miscellaneousMenu.Show(playSound: false);
		m_miscellaneousMenuLoading = false;
	}

	private void OnPrivacyButtonReleased(UIEvent e)
	{
		LoadPrivacyMenu();
		Hide(callHideHandler: false);
	}

	private void LoadPrivacyMenu()
	{
		if (!m_privacyMenuLoading)
		{
			if (m_privacyMenu == null)
			{
				m_privacyMenuLoading = true;
				AssetLoader.Get().InstantiatePrefab("PrivacyMenu.prefab:57d6ca815c24ab948be8f1d27490ee86", ShowPrivacyMenu);
			}
			else
			{
				m_privacyMenu.Show(playSound: false);
			}
		}
	}

	private void ShowPrivacyMenu(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_privacyMenu = go.GetComponent<PrivacyMenu>();
		m_privacyMenu.Show(playSound: false);
		m_privacyMenuLoading = false;
	}

	private void OnSoundOptionsButtonReleased(UIEvent e)
	{
		if (!m_soundOptionsMenuLoading)
		{
			if (m_soundOptionsMenu == null)
			{
				m_soundOptionsMenuLoading = true;
				AssetLoader.Get().InstantiatePrefab(UniversalInputManager.UsePhoneUI ? "OptionsMenu_Sound.prefab:24e506e4b9be97b4fa45b0619a713517" : "OptionsMenu_Sound_phone.prefab:67dc77661d359d84ea46fbe496ee81d7", ShowSoundOptionsMenu);
			}
			else
			{
				m_soundOptionsMenu.Show();
			}
		}
		Hide(callHideHandler: false);
	}

	private void ShowSoundOptionsMenu(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_soundOptionsMenu = go.GetComponent<SoundOptionsMenu>();
		m_soundOptionsMenu.Show();
		m_soundOptionsMenuLoading = false;
	}
}
