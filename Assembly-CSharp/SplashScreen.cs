using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.Devices;
using Hearthstone.Login;
using Hearthstone.Startup;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class SplashScreen : MonoBehaviour
{
	private enum RatingsScreenRegion
	{
		NONE,
		KOREA,
		CHINA
	}

	private const float RATINGS_SCREEN_DISPLAY_TIME = 5f;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public GameObject m_queueSign;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public GameObject m_quitButtonParent;

	public UberText m_queueTitle;

	public UberText m_queueText;

	public UberText m_queueTime;

	public UberText m_loadingText;

	public GameObject m_loadingBar;

	public ProgressBar m_loadingProgress;

	public StandardPegButtonNew m_quitButton;

	public Glow m_glow1;

	public Glow m_glow2;

	public GameObject m_blizzardLogo;

	public GameObject m_demoDisclaimer;

	public StandardPegButtonNew m_devClearLoginButton;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string m_cnRatingsPrefab;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string m_krRatingsPrefab;

	private const float GLOW_FADE_TIME = 1f;

	private static SplashScreen s_instance;

	private bool m_queueShown;

	private bool m_fadingStarted;

	private bool m_inputCameraSet;

	private SplashLoadingText m_splashLoadingText;

	private const long MAX_MINUTES_TO_SHOW_FOR_QUEUE_ETA = 15L;

	public static float FadeOutCompleteTime { get; set; }

	public static event Action<SplashScreen> SplashScreenShown;

	public static event Action SplashScreenHidden;

	private void Awake()
	{
		s_instance = this;
		m_splashLoadingText = new SplashLoadingText(m_loadingText, m_loadingBar, m_loadingProgress);
		OverlayUI.Get().AddGameObject(base.gameObject);
		Show();
		LogoAnimation.Get().ShowLogo();
		if (Vars.Key("Aurora.ClientCheck").GetBool(def: true) && BattleNetClient.needsToRun)
		{
			BattleNetClient.quitHearthstoneAndRun();
			return;
		}
		if (DemoMgr.Get().GetMode() == DemoMode.BLIZZ_MUSEUM)
		{
			m_demoDisclaimer.SetActive(value: true);
		}
		if (HearthstoneApplication.IsInternal() && (bool)HearthstoneApplication.AllowResetFromFatalError)
		{
			Processor.QueueJob(HearthstoneJobs.CreateJobFromAction("ShowClearLogin", delegate
			{
				m_devClearLoginButton.gameObject.SetActive(value: true);
				m_devClearLoginButton.AddEventListener(UIEventType.RELEASE, ClearLogin);
			}, typeof(ILoginService)));
		}
		HearthstoneApplication.Get().WillReset += OnWillReset;
	}

	private void OnDestroy()
	{
		if (m_inputCameraSet)
		{
			if (PegUI.Get() != null && OverlayUI.Get() != null)
			{
				PegUI.Get().RemoveInputCamera(OverlayUI.Get().m_UICamera);
			}
			m_inputCameraSet = false;
		}
		m_splashLoadingText = null;
		if (HearthstoneApplication.Get() != null)
		{
			HearthstoneApplication.Get().WillReset -= OnWillReset;
		}
		s_instance = null;
	}

	private void Update()
	{
		if (!m_inputCameraSet && PegUI.Get() != null && OverlayUI.Get() != null)
		{
			m_inputCameraSet = true;
			PegUI.Get().AddInputCamera(OverlayUI.Get().m_UICamera);
		}
		HandleKeyboardInput();
	}

	public static SplashScreen Get()
	{
		return s_instance;
	}

	public SplashLoadingText GetLoadingText()
	{
		return m_splashLoadingText;
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", 1f);
		args.Add("time", 0.25f);
		args.Add("easetype", iTween.EaseType.easeOutCubic);
		iTween.FadeTo(base.gameObject, args);
		if (!m_fadingStarted)
		{
			FadeGlowsIn();
		}
		SplashScreen.SplashScreenShown?.Invoke(this);
	}

	public IEnumerator<IAsyncJobResult> Hide(JobDefinition sceneTransitionJob)
	{
		yield return new JobDefinition("Splashscreen.AnimateStartupSequence", Job_AnimateStartupSequence(sceneTransitionJob));
	}

	private void UpdateQueueInfo(Network.QueueInfo queueInfo)
	{
		if (queueInfo.secondsTilEnd / 60 > 15)
		{
			m_queueTime.Text = GameStrings.Format("GLOBAL_DATETIME_GREATER_THAN_X_MINUTES", 15L);
		}
		else
		{
			TimeUtils.ElapsedStringSet timeStringSet = TimeUtils.SPLASHSCREEN_DATETIME_STRINGSET;
			m_queueTime.Text = TimeUtils.GetElapsedTimeString((int)queueInfo.secondsTilEnd, timeStringSet);
		}
		m_queueTime.TextAlpha = 1f;
		if (!m_queueShown && queueInfo.secondsTilEnd > 1)
		{
			m_queueShown = true;
			if (PlatformSettings.IsMobile())
			{
				m_quitButtonParent.SetActive(value: false);
			}
			else
			{
				m_quitButton.SetOriginalLocalPosition();
				m_quitButton.AddEventListener(UIEventType.RELEASE, QuitGame);
			}
			RenderUtils.SetAlpha(m_queueSign, 0f);
			m_queueSign.SetActive(value: true);
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("amount", 1f);
			args.Add("time", 0.5f);
			args.Add("easetype", iTween.EaseType.easeInCubic);
			iTween.FadeTo(m_queueSign, args);
			Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
			args2.Add("amount", 0f);
			args2.Add("time", 0.5f);
			args2.Add("includechildren", true);
			args2.Add("easetype", iTween.EaseType.easeOutCubic);
			iTween.FadeTo(LogoAnimation.Get().m_logoContainer, args2);
		}
	}

	private void QuitGame(UIEvent e)
	{
		HearthstoneApplication.Get().Exit();
	}

	private void ClearLogin(UIEvent e)
	{
		Debug.Log("Clear Login Button pressed from the Splash Screen!");
		ServiceManager.Get<ILoginService>()?.WipeAllAuthenticationData();
	}

	private IEnumerator FadeGlowInOut(Glow glow, float timeDelay, bool shouldStartOver)
	{
		yield return new WaitForSeconds(timeDelay);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("time", 1f);
		args.Add("easetype", iTween.EaseType.linear);
		args.Add("from", 0f);
		args.Add("to", 0.4f);
		args.Add("onupdate", "UpdateAlpha");
		args.Add("onupdatetarget", glow.gameObject);
		iTween.ValueTo(glow.gameObject, args);
		Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
		args2.Add("delay", 1f);
		args2.Add("time", 1f);
		args2.Add("easetype", iTween.EaseType.linear);
		args2.Add("from", 0.4f);
		args2.Add("to", 0f);
		args2.Add("onupdate", "UpdateAlpha");
		args2.Add("onupdatetarget", glow.gameObject);
		if (shouldStartOver)
		{
			args2.Add("oncomplete", "FadeGlowsIn");
			args2.Add("oncompletetarget", base.gameObject);
		}
		iTween.ValueTo(glow.gameObject, args2);
	}

	private void FadeGlowsIn()
	{
		m_fadingStarted = true;
		StartCoroutine(FadeGlowInOut(m_glow1, 0f, shouldStartOver: false));
		StartCoroutine(FadeGlowInOut(m_glow2, 1f, shouldStartOver: true));
	}

	private RatingsScreenRegion GetRatingsScreenRegion()
	{
		RatingsScreenRegion ratingsScreenRegion = RatingsScreenRegion.NONE;
		string overrideRegion = Vars.Key("Debug.ForceRatingScreen").GetStr(string.Empty);
		if (!string.IsNullOrEmpty(overrideRegion))
		{
			if (EnumUtils.TryGetEnum<RatingsScreenRegion>(overrideRegion, StringComparison.OrdinalIgnoreCase, out var regionOverride))
			{
				return regionOverride;
			}
			Debug.LogWarning("Unknown rating screen override " + overrideRegion);
		}
		string accountCountry = BattleNet.GetAccountCountry();
		if (!(accountCountry == "CHN"))
		{
			if (accountCountry == "KOR")
			{
				ratingsScreenRegion = RatingsScreenRegion.KOREA;
			}
		}
		else
		{
			ratingsScreenRegion = RatingsScreenRegion.CHINA;
		}
		if (PlatformSettings.IsMobile() && ratingsScreenRegion == RatingsScreenRegion.NONE && DeviceLocaleHelper.GetCountryCode() == "KR")
		{
			ratingsScreenRegion = RatingsScreenRegion.KOREA;
		}
		if (PlatformSettings.LocaleVariant == LocaleVariant.China)
		{
			ratingsScreenRegion = RatingsScreenRegion.CHINA;
		}
		return ratingsScreenRegion;
	}

	public bool HandleKeyboardInput()
	{
		return false;
	}

	public IEnumerator<IAsyncJobResult> Job_AnimateStartupSequence(JobDefinition sceneTransitionJob)
	{
		yield return new JobDefinition("Splashscreen,ShowScaryWarnings", Job_ShowScaryWarnings());
		yield return new JobDefinition("Splashscreen.AnimateRatings", Job_AnimateRatings());
		yield return new JobDefinition("SplashScreen.FadeLogoIn", LogoAnimation.Get().Job_FadeLogoIn());
		Processor.QueueJob(sceneTransitionJob);
		yield return new WaitForDuration(2f);
		yield return new JobDefinition("Splashscreen.FadeOutSplashscreen", Job_FadeOutSplashscreen());
		OnSplashScreenFadeOutComplete();
	}

	public IEnumerator<IAsyncJobResult> Job_ShowLoginQueue()
	{
		Log.Login.PrintDebug("SplashScreen: Job_ShowLoginQueue");
		if (!Network.ShouldBeConnectedToAurora())
		{
			yield break;
		}
		WaitForCallback<Network.QueueInfo> OnQueueModified = new WaitForCallback<Network.QueueInfo>();
		LoginManager.Get().RegisterQueueModifiedListener(OnQueueModified.Callback);
		if (LoginManager.Get().CurrentQueueInfo != null)
		{
			OnQueueModified.Callback(LoginManager.Get().CurrentQueueInfo);
		}
		Action OnLogin = delegate
		{
			Network.QueueInfo obj = new Network.QueueInfo
			{
				position = 0
			};
			OnQueueModified.Callback(obj);
		};
		LoginManager.Get().OnLoginCompleted += OnLogin;
		Log.Login.PrintDebug("SplashScreen: wait OnQueueModified position 0");
		while (true)
		{
			yield return OnQueueModified;
			Network.QueueInfo queueInfo = OnQueueModified.Data.Arg1;
			if (queueInfo.position == 0)
			{
				break;
			}
			UpdateQueueInfo(queueInfo);
			OnQueueModified.Reset();
		}
		Log.Login.PrintDebug("SplashScreen: done wait OnQueueModified position 0");
		ServiceManager.Get<LoginManager>().RemoveQueueModifiedListener(OnQueueModified.Callback);
		LoginManager.Get().OnLoginCompleted -= OnLogin;
		m_queueShown = false;
		m_queueSign.SetActive(value: false);
	}

	private IEnumerator<IAsyncJobResult> Job_ShowScaryWarnings()
	{
		while (DialogManager.Get() == null)
		{
			yield return null;
		}
		while (DialogManager.Get().ShowingDialog())
		{
			yield return null;
		}
		ShowDevicePerformanceWarning();
		ShowGraphicsDeviceWarning();
		ShowTextureCompressionWarning();
	}

	public IEnumerator<IAsyncJobResult> Job_AnimateRatings()
	{
		RatingsScreenRegion ratingsScreenRegion = GetRatingsScreenRegion();
		if (ratingsScreenRegion != 0)
		{
			m_splashLoadingText.SetStartupStage(StartupStage.RatingsScreen);
			WidgetInstance widget = WidgetInstance.Create((ratingsScreenRegion == RatingsScreenRegion.CHINA) ? m_cnRatingsPrefab : m_krRatingsPrefab);
			while (!widget.IsReady)
			{
				yield return null;
			}
			IDataModel dataModel = GetRatingsDataModel(ratingsScreenRegion);
			if (dataModel != null)
			{
				widget.BindDataModel(dataModel);
			}
			OverlayUI.Get().AddGameObject(widget.gameObject);
			Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
			args2.Add("amount", 0f);
			args2.Add("time", 0.5f);
			args2.Add("includechildren", true);
			args2.Add("easetype", iTween.EaseType.easeOutCubic);
			LogoAnimation logoAnimation = LogoAnimation.Get();
			iTween.FadeTo(logoAnimation.m_logoContainer, args2);
			yield return new WaitForDuration(0.5f);
			logoAnimation.HideLogo();
			widget.Show();
			Hashtable fadeInRatings = iTweenManager.Get().GetTweenHashTable();
			fadeInRatings.Add("amount", 1f);
			fadeInRatings.Add("time", 0.5f);
			fadeInRatings.Add("includechildren", true);
			fadeInRatings.Add("easetype", iTween.EaseType.easeInCubic);
			iTween.FadeTo(widget.gameObject, fadeInRatings);
			RatingsPopupControl popupControl = widget.GetComponentInChildren<RatingsPopupControl>();
			if (popupControl != null && popupControl.WaitForUserToStart)
			{
				WaitForCallback waitForCB = new WaitForCallback();
				popupControl.OnUserStartPressed += waitForCB.Callback;
				yield return waitForCB;
				popupControl.OnUserStartPressed -= waitForCB.Callback;
			}
			else
			{
				yield return new WaitForDuration(5.5f);
			}
			Hashtable fadeOutRatings = iTweenManager.Get().GetTweenHashTable();
			fadeOutRatings.Add("amount", 0f);
			fadeOutRatings.Add("time", 0.5f);
			fadeOutRatings.Add("includechildren", true);
			fadeOutRatings.Add("easetype", iTween.EaseType.easeInCubic);
			iTween.FadeTo(widget.gameObject, fadeOutRatings);
			yield return new WaitForDuration(0.5f);
			widget.Hide();
			UnityEngine.Object.Destroy(widget.gameObject);
		}
	}

	private static IDataModel GetRatingsDataModel(RatingsScreenRegion ratingsScreenRegion)
	{
		if (ratingsScreenRegion == RatingsScreenRegion.CHINA && ServiceManager.TryGet<ExternalUrlService>(out var externalUrlService))
		{
			return new RatingsScreenDataModel
			{
				Url = externalUrlService.GetChinaRatingsWebsiteLink()
			};
		}
		return null;
	}

	public IEnumerator<IAsyncJobResult> Job_FadeOutSplashscreen()
	{
		float fadeTime = 0.5f;
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", 0f);
		args.Add("delay", 0f);
		args.Add("time", fadeTime);
		args.Add("easetype", iTween.EaseType.linear);
		args.Add("oncompletetarget", base.gameObject);
		iTween.FadeTo(base.gameObject, args);
		if (m_glow1 != null)
		{
			Hashtable g1args = iTweenManager.Get().GetTweenHashTable();
			g1args.Add("amount", 0f);
			g1args.Add("delay", 0f);
			g1args.Add("time", fadeTime);
			g1args.Add("easetype", iTween.EaseType.linear);
			g1args.Add("oncompletetarget", base.gameObject);
			iTween.FadeTo(m_glow1.gameObject, g1args);
		}
		if (m_glow2 != null)
		{
			Hashtable g2args = iTweenManager.Get().GetTweenHashTable();
			g2args.Add("amount", 0f);
			g2args.Add("delay", 0f);
			g2args.Add("time", fadeTime);
			g2args.Add("easetype", iTween.EaseType.linear);
			g2args.Add("oncompletetarget", base.gameObject);
			iTween.FadeTo(m_glow2.gameObject, g2args);
		}
		Processor.QueueJob("SplashScreen.FadeLogoOut", LogoAnimation.Get().Job_FadeLogoOut());
		yield return new WaitForDuration(fadeTime);
		if (m_glow1 != null)
		{
			m_glow1.gameObject.SetActive(value: false);
		}
		if (m_glow2 != null)
		{
			m_glow2.gameObject.SetActive(value: false);
		}
	}

	private void OnSplashScreenFadeOutComplete()
	{
		FadeOutCompleteTime = Time.realtimeSinceStartup;
		SplashScreen.SplashScreenHidden?.Invoke();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void ShowDevicePerformanceWarning()
	{
		if (Vars.Key("Mobile.CheckNewMinSpec").GetBool(def: true) || Options.Get().GetBool(Option.HAS_SHOWN_DEVICE_PERFORMANCE_WARNING, defaultVal: false) || PlatformSettings.s_isDeviceInMinSpec)
		{
			return;
		}
		Options.Get().SetBool(Option.HAS_SHOWN_DEVICE_PERFORMANCE_WARNING, val: true);
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_DEVICE_PERFORMANCE_WARNING_TITLE");
		info.m_text = GameStrings.Get("GLUE_DEVICE_PERFORMANCE_WARNING");
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_iconSet = AlertPopup.PopupInfo.IconSet.None;
		info.m_confirmText = GameStrings.Get("GLOBAL_OKAY");
		info.m_cancelText = GameStrings.Get("GLOBAL_SUPPORT");
		info.m_responseCallback = delegate(AlertPopup.Response response, object data)
		{
			if (response == AlertPopup.Response.CANCEL)
			{
				Application.OpenURL(ExternalUrlService.Get().GetSystemRequirementsLink());
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	private void ShowGraphicsDeviceWarning()
	{
		if (PlatformSettings.RuntimeOS != OSCategory.Android || Options.Get().GetBool(Option.SHOWN_GFX_DEVICE_WARNING, defaultVal: false))
		{
			return;
		}
		Options.Get().SetBool(Option.SHOWN_GFX_DEVICE_WARNING, val: true);
		string gfxCardName = SystemInfo.graphicsDeviceName.ToLower();
		if (!gfxCardName.Contains("powervr") || (!gfxCardName.Contains("540") && !gfxCardName.Contains("544")))
		{
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_UNRELIABLE_GPU_WARNING_TITLE");
		info.m_text = GameStrings.Get("GLUE_UNRELIABLE_GPU_WARNING");
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_iconSet = AlertPopup.PopupInfo.IconSet.None;
		info.m_cancelText = GameStrings.Get("GLOBAL_SUPPORT");
		info.m_confirmText = GameStrings.Get("GLOBAL_OKAY");
		info.m_responseCallback = delegate(AlertPopup.Response response, object data)
		{
			if (response == AlertPopup.Response.CANCEL)
			{
				Application.OpenURL(ExternalUrlService.Get().GetSystemRequirementsLink());
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	private void ShowTextureCompressionWarning()
	{
		if (PlatformSettings.RuntimeOS != OSCategory.Android || !HearthstoneApplication.IsInternal() || PlatformSettings.LocaleVariant != LocaleVariant.China || AndroidDeviceSettings.Get().IsCurrentTextureFormatSupported())
		{
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_TEXTURE_COMPRESSION_WARNING_TITLE");
		info.m_text = GameStrings.Get("GLUE_TEXTURE_COMPRESSION_WARNING");
		info.m_showAlertIcon = true;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_iconSet = AlertPopup.PopupInfo.IconSet.None;
		info.m_cancelText = GameStrings.Get("GLOBAL_SUPPORT");
		info.m_confirmText = GameStrings.Get("GLOBAL_OKAY");
		info.m_responseCallback = delegate(AlertPopup.Response response, object data)
		{
			if (response == AlertPopup.Response.CANCEL)
			{
				Application.OpenURL("http://www.hearthstone.com.cn/download");
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	private void OnWillReset()
	{
		SplashScreen.SplashScreenHidden?.Invoke();
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
