using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Streaming;
using Hearthstone.UI;
using Networking;
using PegasusUtil;
using UnityEngine;

[CustomEditClass]
public class BnetBar : MonoBehaviour
{
	private const string SkipTutorialPopupPrefab = "SkipTutorial_Popup.prefab:8f770936aa3f2e542bec83d77d0d6733";

	public UberText m_currentTime;

	public BnetBarMenuButton m_menuButton;

	public GameObject m_menuButtonMesh;

	public BnetBarFriendButton m_friendButton;

	public Renderer FriendButtonMesh;

	public GameObject m_currencyFrameContainer;

	public Flipbook m_batteryLevel;

	public Flipbook m_batteryLevelPhone;

	public GameObject m_socialToastBone;

	public ConnectionIndicator m_connectionIndicator;

	public GameObject SkipTutorialButton;

	public SpriteRenderer m_gameConnectionStatusSprite;

	public Vector3 SkipButtonOffset = new Vector3(-60f, 0f, 235f);

	public Vector3 SkipButtonOffsetDesktop = new Vector3(-60f, 0f, 240f);

	public Vector3 SkipButtonOffsetMobileDownloadActive = new Vector3(-54f, 0f, 235f);

	public float SkipButtonScaleFactorDesktop;

	public float SkipButtonMobileXPadding = 4f;

	public Material DimButtonMaterial;

	public float IconColorDimFactor = 0.75f;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string m_spectatorCountPrefabPath;

	public TooltipZone m_spectatorCountTooltipZone;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string m_spectatorModeIndicatorPrefab;

	[Header("Phone Aspect Ratio")]
	public float HorizontalMarginMinAspectRatio;

	public float HorizontalMarginWideAspectRatio;

	public float HorizontalMarginExtraWideAspectRatio;

	public static readonly int CameraDepth = 47;

	private GameObject m_tutorialSkipButton;

	private WidgetInstance m_tutorialSkipPopup;

	private static BnetBar s_instance;

	private float m_initialWidth;

	private float m_initialFriendButtonScaleX;

	private float m_initialMenuButtonScaleX;

	private Vector3 m_initialSkipButtonScale;

	private float m_initialSpectatorModeIndicatorScaleX;

	private float m_initialSpectatorCountScaleX;

	private GameMenuInterface m_gameMenu;

	private bool m_gameMenuLoading;

	private bool m_isInitting = true;

	private GameObject m_loginTooltip;

	private bool m_hasUnacknowledgedPendingInvites;

	private GameObject m_spectatorCountPanel;

	private GameObject m_spectatorModeIndicator;

	private bool m_isLoggedIn;

	private bool m_buttonsEnabled;

	private bool m_buttonsDisabledPermanently;

	private int m_buttonsDisabledByRefCount;

	private HashSet<DialogBase> m_buttonsDisabledByDialog = new HashSet<DialogBase>();

	private bool m_suppressLoginTooltip;

	private float m_lastClockUpdate;

	private bool m_lastClockUpdateCanShowServerTime;

	private double m_serverClientOffsetInSec;

	private const float MENU_BUTTON_LOCAL_X_OFFSET = 0.14f;

	private const float CURRENCY_CONTAINER_LOCAL_Y = -2.850989f;

	private const float CURRENCY_CONTAINER_LOCAL_Y_MOBILE = 189.703f;

	private readonly Vector3 BATTERY_LEVEL_LAYOUT_OFFSET = new Vector3(3f, 1.25f, 0f);

	private List<CurrencyFrame> m_currencyFrames = new List<CurrencyFrame>();

	private bool m_showSkipTutorialButton;

	private bool m_lastShowSkipTutorialButton;

	private Material m_defaultUiMaterial;

	private VarKey m_showServerTime = Vars.Key("Application.ShowServerTime");

	private static readonly Vector3 LAYOUT_TOPLEFT_START_POINT = new Vector3(1.5f, 189f, 0f);

	private static readonly Vector3 LAYOUT_BOTTOMLEFT_START_POINT = new Vector3(1.5f, 0f, 1.25f);

	private static readonly PlatformDependentValue<Vector3> LAYOUT_OFFSET_PADDING = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(0f, 0f, 0f),
		Tablet = new Vector3(4f, 0f, 0f),
		MiniTablet = new Vector3(4f, 0f, 0f),
		Phone = new Vector3(4f, 0f, 8f)
	};

	private static readonly PlatformDependentValue<Vector3> LAYOUT_OFFSET_CURRENCY = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(0f, 0f, 1f),
		Phone = new Vector3(0f, 0f, -3.4f)
	};

	private static readonly PlatformDependentValue<Vector3> LAYOUT_OFFSET_SPECTATOR_WIDGET = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(2f, 0f, 0f),
		Phone = new Vector3(8f, 0f, 0f)
	};

	private static readonly PlatformDependentValue<Vector3> LAYOUT_OFFSET_SPECTATOR_COUNT_WIDGET = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(4f, 0f, 1f),
		Phone = new Vector3(0f, 0f, 0f)
	};

	[CustomEditField(Hide = true)]
	public float HorizontalMargin
	{
		get
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				return TransformUtil.GetAspectRatioDependentValue(HorizontalMarginMinAspectRatio, HorizontalMarginWideAspectRatio, HorizontalMarginExtraWideAspectRatio);
			}
			return 0f;
		}
	}

	public bool ShowShowSkipTutorialButton
	{
		get
		{
			if (SpectatorManager.Get() != null && SpectatorManager.Get().IsInSpectatorMode())
			{
				return false;
			}
			return m_showSkipTutorialButton;
		}
	}

	private bool ShouldShowSpectatorModeIndicator
	{
		get
		{
			bool showReplayRecording = false;
			bool showInSpectatorMode = SpectatorManager.Get().IsInSpectatorMode();
			bool showIndicator = (byte)(0u | (showReplayRecording ? 1u : 0u) | (showInSpectatorMode ? 1u : 0u)) != 0;
			if ((bool)UniversalInputManager.UsePhoneUI && SceneMgr.Get() != null && !SceneMgr.Get().IsInGame())
			{
				showIndicator = false;
			}
			if (SpectatorManager.Get().IsBeingSpectated())
			{
				showIndicator = false;
			}
			return showIndicator;
		}
	}

	public event Action OnMenuOpened;

	public static event Action<bool> SkipTutorialSelected;

	private void Awake()
	{
		s_instance = this;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_menuButton.transform.localScale *= 2f;
			m_friendButton.transform.localScale *= 2f;
		}
		else
		{
			m_connectionIndicator.gameObject.SetActive(value: false);
		}
		m_initialWidth = GetComponent<Renderer>().bounds.size.x;
		m_initialFriendButtonScaleX = m_friendButton.transform.localScale.x;
		m_initialMenuButtonScaleX = m_menuButton.transform.localScale.x;
		m_initialSkipButtonScale = SkipTutorialButton.transform.localScale;
		m_menuButton.StateChanged = UpdateLayout;
		m_menuButton.AddEventListener(UIEventType.RELEASE, OnMenuButtonReleased);
		m_friendButton.AddEventListener(UIEventType.RELEASE, OnFriendButtonReleased);
		UpdateButtonEnableState();
		m_batteryLevel.gameObject.SetActive(value: false);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_batteryLevel = m_batteryLevelPhone;
			m_currentTime.gameObject.SetActive(value: false);
		}
		m_menuButton.SetPhoneStatusBarState(0);
		m_friendButton.gameObject.SetActive(value: false);
		m_defaultUiMaterial = m_menuButtonMesh.GetComponent<Renderer>().GetSharedMaterial();
		ToggleActive(active: false);
		if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			currencyManager.AddCurrencyBalanceChangedCallback(OnCurrencyBalanceChangedInternal);
		}
	}

	private void OnDestroy()
	{
		if (!HearthstoneApplication.IsHearthstoneClosing)
		{
			if (SpectatorManager.Get() != null)
			{
				SpectatorManager.Get().OnInviteReceived -= SpectatorManager_OnInviteReceived;
				SpectatorManager.Get().OnSpectatorToMyGame -= SpectatorManager_OnSpectatorToMyGame;
				SpectatorManager.Get().OnSpectatorModeChanged -= SpectatorManager_OnSpectatorModeChanged;
			}
			Network.Get()?.RemoveNetHandler(GetServerTimeResponse.PacketID.ID, OnRequestGetServerTimeResponse);
			HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
			if (hearthstoneApplication != null)
			{
				hearthstoneApplication.WillReset -= WillReset;
			}
		}
		if (ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
		{
			currencyManager.RemoveCurrencyBalanceChangedCallback(OnCurrencyBalanceChangedInternal);
		}
		if (Network.Get() != null)
		{
			DispatchListener networkDispatchListener = Network.Get().NetworkDispatchListener;
			networkDispatchListener.OnGameServerConnect = (Action<BattleNetErrors>)Delegate.Remove(networkDispatchListener.OnGameServerConnect, new Action<BattleNetErrors>(OnGameServerConnect));
			DispatchListener networkDispatchListener2 = Network.Get().NetworkDispatchListener;
			networkDispatchListener2.OnGameServerDisconnect = (Action<BattleNetErrors>)Delegate.Remove(networkDispatchListener2.OnGameServerDisconnect, new Action<BattleNetErrors>(OnGameServerDisconnect));
		}
		s_instance = null;
	}

	private void Start()
	{
		SceneMgr.Get()?.RegisterSceneLoadedEvent(OnSceneLoaded);
		if (SpectatorManager.Get() != null)
		{
			SpectatorManager.Get().OnInviteReceived += SpectatorManager_OnInviteReceived;
			SpectatorManager.Get().OnSpectatorToMyGame += SpectatorManager_OnSpectatorToMyGame;
			SpectatorManager.Get().OnSpectatorModeChanged += SpectatorManager_OnSpectatorModeChanged;
		}
		Network.Get()?.RegisterNetHandler(GetServerTimeResponse.PacketID.ID, OnRequestGetServerTimeResponse);
		if (Network.Get() != null)
		{
			DispatchListener networkDispatchListener = Network.Get().NetworkDispatchListener;
			networkDispatchListener.OnGameServerConnect = (Action<BattleNetErrors>)Delegate.Combine(networkDispatchListener.OnGameServerConnect, new Action<BattleNetErrors>(OnGameServerConnect));
			DispatchListener networkDispatchListener2 = Network.Get().NetworkDispatchListener;
			networkDispatchListener2.OnGameServerDisconnect = (Action<BattleNetErrors>)Delegate.Combine(networkDispatchListener2.OnGameServerDisconnect, new Action<BattleNetErrors>(OnGameServerDisconnect));
		}
		if (HearthstoneApplication.Get() != null)
		{
			HearthstoneApplication.Get().WillReset += WillReset;
		}
		m_friendButton.gameObject.SetActive(value: false);
		if (m_friendButton != null)
		{
			m_friendButton.ShowPendingInvitesIcon(m_hasUnacknowledgedPendingInvites);
		}
		Processor.QueueJob("Enable friends button", OnInitialDownloadComplete(), new WaitForGameDownloadManagerState());
	}

	private void OnGameServerConnect(BattleNetErrors errors)
	{
		UpdateGameServerConnectionStatus();
	}

	private void OnGameServerDisconnect(BattleNetErrors errors)
	{
		UpdateGameServerConnectionStatus();
	}

	private IEnumerator<IAsyncJobResult> OnInitialDownloadComplete()
	{
		if (m_isLoggedIn)
		{
			m_friendButton.gameObject.SetActive(value: true);
		}
		yield break;
	}

	private void Update()
	{
		float time = Time.realtimeSinceStartup;
		if (time - m_lastClockUpdate > 1f)
		{
			m_lastClockUpdate = time;
			bool canShowServerTime = !HearthstoneApplication.IsPublic() && m_showServerTime.GetBool(def: true);
			if (canShowServerTime && TryGetServerTime(out var serverTimeNow))
			{
				m_currentTime.Text = GameStrings.Format("GLOBAL_CURRENT_TIME_AND_DATE_DEV", GameStrings.Format("GLOBAL_CURRENT_TIME", DateTime.Now), GameStrings.Format("GLOBAL_CURRENT_DATE", serverTimeNow), GameStrings.Format("GLOBAL_CURRENT_TIME", serverTimeNow));
			}
			else if (Localization.GetLocale() == Locale.enGB)
			{
				m_currentTime.Text = $"{DateTime.Now:HH:mm}";
			}
			else
			{
				m_currentTime.Text = GameStrings.Format("GLOBAL_CURRENT_TIME", DateTime.Now);
			}
			if (Localization.GetLocale() == Locale.koKR)
			{
				m_currentTime.Text = m_currentTime.Text.Replace("AM", GameStrings.Format("GLOBAL_CURRENT_TIME_AM")).Replace("PM", GameStrings.Format("GLOBAL_CURRENT_TIME_PM"));
			}
			if (canShowServerTime != m_lastClockUpdateCanShowServerTime)
			{
				UpdateLayout();
				m_lastClockUpdateCanShowServerTime = canShowServerTime;
			}
		}
		if (m_showSkipTutorialButton != m_lastShowSkipTutorialButton && m_showSkipTutorialButton)
		{
			UpdateLayout();
		}
		m_lastShowSkipTutorialButton = m_showSkipTutorialButton;
	}

	public static BnetBar Get()
	{
		return s_instance;
	}

	public void OnLoggedIn()
	{
		if (Network.ShouldBeConnectedToAurora() && GameDownloadManagerProvider.Get().IsReadyToPlay)
		{
			m_friendButton.gameObject.SetActive(value: true);
		}
		Network.Get().GetServerTimeRequest();
		m_isLoggedIn = true;
		ToggleActive(active: true);
		Update();
		UpdateLayout();
	}

	public void UpdateLayout()
	{
		if (!m_isLoggedIn)
		{
			return;
		}
		if (!SpectatorManager.Get().IsInSpectatorMode())
		{
			SkipTutorialButton.SetActive(m_showSkipTutorialButton);
		}
		else
		{
			SkipTutorialButton.SetActive(value: false);
		}
		float fudgeFactor = 0.5f;
		Bounds screenBounds = CameraUtils.GetNearClipBounds(PegUI.Get().orthographicUICam);
		screenBounds.size = new Vector3(screenBounds.size.x, screenBounds.size.z, screenBounds.size.y);
		screenBounds.min += new Vector3(HorizontalMargin / 4f, 0f, 0f);
		screenBounds.max -= new Vector3(HorizontalMargin / 4f, 0f, 0f);
		float adjustedXScale = (screenBounds.size.x + fudgeFactor) / m_initialWidth;
		float quarterScale = adjustedXScale * 0.25f;
		TransformUtil.SetLocalPosX(base.gameObject, (screenBounds.min.x - base.transform.parent.localPosition.x - fudgeFactor) * 4f);
		TransformUtil.SetLocalScaleX(base.gameObject, adjustedXScale);
		float xPadding = -0.03f * quarterScale;
		if (GeneralUtils.IsDevelopmentBuildTextVisible())
		{
			xPadding -= CameraUtils.ScreenToWorldDist(PegUI.Get().orthographicUICam, 115f);
		}
		float yPadding = 1f * base.transform.localScale.y;
		bool showMenuButton = true;
		if (!DemoMgr.Get().IsHubEscMenuEnabled(SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY))
		{
			showMenuButton = false;
		}
		m_menuButton.gameObject.SetActive(showMenuButton);
		TransformUtil.SetLocalScaleX(m_menuButton, m_initialMenuButtonScaleX / adjustedXScale);
		TransformUtil.SetPoint(m_menuButton, Anchor.RIGHT, base.gameObject, Anchor.RIGHT, new Vector3(xPadding, yPadding, 0f) - LAYOUT_OFFSET_PADDING);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			TransformUtil.SetPoint(m_menuButton, Anchor.RIGHT, base.gameObject, Anchor.RIGHT, new Vector3(xPadding * 4f, yPadding, 0f));
			TransformUtil.SetLocalPosX(m_menuButton, m_menuButton.transform.localPosition.x + 0.14f);
			TransformUtil.SetLocalPosY(m_menuButton, LAYOUT_TOPLEFT_START_POINT.y);
			m_batteryLevel.gameObject.SetActive(value: true);
			int statusBarState = 1 + (m_connectionIndicator.IsVisible() ? 1 : 0);
			m_menuButton.SetPhoneStatusBarState(statusBarState);
			TransformUtil.SetLocalScaleX(m_currencyFrameContainer, 2f / adjustedXScale);
			TransformUtil.SetLocalScaleY(m_currencyFrameContainer, 0.4f);
			if (showMenuButton)
			{
				PositionCurrencyFrame(m_batteryLevel.gameObject, new Vector3(m_menuButton.GetCurrencyFrameOffsetX(), 0f, LAYOUT_OFFSET_CURRENCY.Value.z));
			}
			else
			{
				PositionCurrencyFrame(m_batteryLevel.gameObject, new Vector3(100f, 0f, LAYOUT_OFFSET_CURRENCY.Value.z));
			}
		}
		else
		{
			TransformUtil.SetPoint(m_menuButton, Anchor.RIGHT, base.gameObject, Anchor.RIGHT, new Vector3(xPadding, yPadding, 0f));
			TransformUtil.SetLocalScaleX(m_currencyFrameContainer, 1f / adjustedXScale);
			PositionCurrencyFrame(m_menuButton.gameObject, new Vector3(m_menuButton.GetCurrencyFrameOffsetX(), 0f, LAYOUT_OFFSET_CURRENCY.Value.z));
		}
		bool showSpectatorCount = m_spectatorCountPanel != null && m_spectatorCountPanel.activeInHierarchy && SpectatorManager.Get().IsBeingSpectated();
		bool showSpectatingIndicator = !showSpectatorCount && m_spectatorModeIndicator != null && ShouldShowSpectatorModeIndicator;
		if ((bool)UniversalInputManager.UsePhoneUI && SceneMgr.Get() != null && !SceneMgr.Get().IsInGame())
		{
			showSpectatorCount = false;
			showSpectatingIndicator = false;
		}
		ShowSpectatorModeIndicator(showSpectatingIndicator);
		UpdateGameServerConnectionStatus();
		GameObject previousBottomLeftWidget = null;
		bool isFriendListShowingOnPhone = false;
		if (m_friendButton.gameObject.activeInHierarchy)
		{
			TransformUtil.SetLocalScaleX(m_friendButton, m_initialFriendButtonScaleX / adjustedXScale);
			LayoutWidget_BottomLeft_Relative(m_friendButton.transform, ref previousBottomLeftWidget);
			TransformUtil.SetLocalScaleX(m_socialToastBone, 1f / adjustedXScale);
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				previousBottomLeftWidget = null;
				TransformUtil.SetLocalPosY(m_friendButton, LAYOUT_TOPLEFT_START_POINT.y);
				if (ChatMgr.Get() != null && ChatMgr.Get().FriendListFrame != null)
				{
					TransformUtil.SetPosY(m_friendButton, ChatMgr.Get().FriendListFrame.transform.position.y - 1f);
					isFriendListShowingOnPhone = true;
				}
			}
		}
		if (showSpectatorCount)
		{
			TransformUtil.SetLocalScaleX(m_spectatorCountPanel, m_initialSpectatorCountScaleX / adjustedXScale);
			LayoutWidget_BottomLeft_Relative(m_spectatorCountPanel.transform, ref previousBottomLeftWidget, LAYOUT_OFFSET_SPECTATOR_COUNT_WIDGET);
			if (isFriendListShowingOnPhone)
			{
				TransformUtil.SetPosY(m_spectatorCountPanel, ChatMgr.Get().FriendListFrame.transform.position.y + 1f);
			}
		}
		if (showSpectatingIndicator)
		{
			TransformUtil.SetLocalScaleX(m_spectatorModeIndicator, m_initialSpectatorModeIndicatorScaleX / adjustedXScale);
			LayoutWidget_BottomLeft_Relative(m_spectatorModeIndicator.transform, ref previousBottomLeftWidget, LAYOUT_OFFSET_SPECTATOR_WIDGET);
			if (isFriendListShowingOnPhone)
			{
				TransformUtil.SetPosY(m_spectatorModeIndicator, ChatMgr.Get().FriendListFrame.transform.position.y + 1f);
			}
		}
		GameObject socialToastPrevWidget = previousBottomLeftWidget;
		Vector3 socialToastBoneOffset;
		Vector3 currentTimeOffset;
		if (previousBottomLeftWidget == null)
		{
			socialToastBoneOffset = LAYOUT_BOTTOMLEFT_START_POINT;
			currentTimeOffset = Vector3.zero;
		}
		else if (previousBottomLeftWidget == m_friendButton.gameObject)
		{
			socialToastBoneOffset = new Vector3(3.75f, 1f, 4f);
			currentTimeOffset = new Vector3(5.5f, 0f, LAYOUT_OFFSET_CURRENCY.Value.z);
		}
		else
		{
			socialToastBoneOffset = new Vector3(1.75f, 1f, 4f);
			currentTimeOffset = new Vector3(3.5f, 0f, LAYOUT_OFFSET_CURRENCY.Value.z);
		}
		socialToastBoneOffset += (Vector3)LAYOUT_OFFSET_PADDING;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			socialToastPrevWidget = m_friendButton.gameObject;
			if (!m_friendButton.gameObject.activeInHierarchy)
			{
				socialToastPrevWidget = null;
				socialToastBoneOffset = LAYOUT_TOPLEFT_START_POINT;
			}
		}
		socialToastBoneOffset.z = -1f;
		TransformUtil.SetPoint(m_socialToastBone, Anchor.LEFT_XZ, socialToastPrevWidget, Anchor.RIGHT_XZ, socialToastBoneOffset);
		TransformUtil.SetLocalScaleX(m_currentTime, 1f / adjustedXScale);
		LayoutWidget_BottomLeft_Relative(m_currentTime.transform, ref previousBottomLeftWidget, currentTimeOffset);
		if (PlatformSettings.IsTablet && m_isLoggedIn)
		{
			m_batteryLevel.gameObject.SetActive(value: true);
			LayoutWidget_LeftAligned_SetExactOffset(m_batteryLevel.transform, m_currentTime.gameObject, new Vector3(12f, 5f, 0f));
		}
		UpdateLoginTooltip();
		if (m_isInitting)
		{
			foreach (CurrencyFrame currencyFrame in m_currencyFrames)
			{
				currencyFrame.Hide(isImmediate: true);
			}
			m_isInitting = false;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			UpdateForPhone();
		}
		if (!m_showSkipTutorialButton || SpectatorManager.Get().IsInSpectatorMode())
		{
			return;
		}
		Vector3 scale = m_initialSkipButtonScale;
		scale.x = m_initialSkipButtonScale.x / adjustedXScale;
		Vector3 skipButtonOffset = new Vector3(0f, 0f, SkipButtonOffsetDesktop.z);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			skipButtonOffset = new Vector3(xPadding * SkipButtonMobileXPadding, yPadding, 0f) + SkipButtonOffset;
			if (GameDownloadManagerProvider.Get().GetCurrentDownloadStatus() != null)
			{
				skipButtonOffset = new Vector3(xPadding * SkipButtonMobileXPadding, yPadding, 0f) + SkipButtonOffsetMobileDownloadActive;
			}
		}
		else
		{
			scale *= SkipButtonScaleFactorDesktop;
		}
		TransformUtil.SetPoint(SkipTutorialButton, Anchor.TOP_RIGHT, base.gameObject, Anchor.TOP_RIGHT, skipButtonOffset);
		SkipTutorialButton.transform.localScale = scale;
	}

	private void UpdateGameServerConnectionStatus()
	{
		if (SceneMgr.Get() != null && SceneMgr.Get().IsInGame())
		{
			if (Network.Get() != null && Network.Get().IsConnectedToGameServer())
			{
				m_gameConnectionStatusSprite.enabled = false;
			}
			else if (GameState.Get() != null && GameState.Get().IsGameOverNowOrPending())
			{
				m_gameConnectionStatusSprite.enabled = false;
			}
			else
			{
				m_gameConnectionStatusSprite.enabled = true;
			}
		}
		else
		{
			m_gameConnectionStatusSprite.enabled = false;
		}
	}

	public bool TryGetRelevantCurrencyFrame(CurrencyType currencyType, out CurrencyFrame currencyFrame)
	{
		currencyFrame = null;
		foreach (CurrencyFrame targetCurrencyFrame in m_currencyFrames)
		{
			if (targetCurrencyFrame.CurrentCurrencyType == currencyType)
			{
				currencyFrame = targetCurrencyFrame;
				return true;
			}
		}
		return false;
	}

	public void RefreshCurrency()
	{
		List<CurrencyType> visibleCurrencies = CurrencyFrame.GetVisibleCurrencies().ToList();
		if (visibleCurrencies.Count > m_currencyFrames.Count)
		{
			Log.BattleNet.PrintWarning("More visible currencies then there are existing Currency Frames. This will lead to some currencies not being displayed");
		}
		for (int i = 0; i < m_currencyFrames.Count; i++)
		{
			CurrencyFrame currencyFrame = m_currencyFrames[i];
			if (i < visibleCurrencies.Count)
			{
				currencyFrame.Bind(visibleCurrencies[i]);
				currencyFrame.Show();
			}
			else
			{
				currencyFrame.Bind(CurrencyType.NONE);
				currencyFrame.Hide();
			}
		}
		UpdateLayout();
	}

	public void RegisterCurrencyFrame(CurrencyFrame currencyFrame)
	{
		if (m_currencyFrames.Contains(currencyFrame))
		{
			return;
		}
		for (int i = 0; i < m_currencyFrameContainer.transform.childCount; i++)
		{
			if (m_currencyFrameContainer.transform.GetChild(i).GetComponentInChildren<CurrencyFrame>(includeInactive: true) == currencyFrame)
			{
				if (i < 0 || i >= m_currencyFrames.Count)
				{
					m_currencyFrames.Add(currencyFrame);
				}
				else
				{
					m_currencyFrames.Insert(i, currencyFrame);
				}
				return;
			}
		}
		m_currencyFrames.Add(currencyFrame);
	}

	public void SetBlockCurrencyFrames(bool isBlocked)
	{
		foreach (CurrencyFrame currencyFrame in m_currencyFrames)
		{
			currencyFrame.SetBlocked(isBlocked);
		}
	}

	public void ShowCurrencyFrames(bool isImmediate = false)
	{
		foreach (CurrencyFrame currencyFrame in m_currencyFrames)
		{
			currencyFrame.Show(isImmediate);
		}
	}

	public void HideCurrencyFrames(bool isImmediate = false)
	{
		foreach (CurrencyFrame currencyFrame in m_currencyFrames)
		{
			currencyFrame.Hide(isImmediate);
		}
	}

	public bool IsCurrencyFrameActive()
	{
		foreach (CurrencyFrame currencyFrame in m_currencyFrames)
		{
			if (currencyFrame.IsShown())
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetServerTimeUTC(out DateTime serverTime)
	{
		if (m_serverClientOffsetInSec != double.MaxValue)
		{
			serverTime = DateTime.UtcNow.AddSeconds(m_serverClientOffsetInSec);
			return true;
		}
		serverTime = DateTime.UtcNow;
		return false;
	}

	public bool TryGetServerTime(out DateTime serverTime)
	{
		if (TryGetServerTimeUTC(out serverTime))
		{
			serverTime = serverTime.ToLocalTime();
			return true;
		}
		return false;
	}

	private static void LayoutWidget_LeftAligned_SetExactOffset(Transform transform, GameObject previousWidget, Vector3 exactOffset)
	{
		if (transform.gameObject.activeInHierarchy)
		{
			if (previousWidget == null)
			{
				TransformUtil.SetPoint(transform, Anchor.LEFT, Get().gameObject, Anchor.LEFT, exactOffset);
			}
			else
			{
				TransformUtil.SetPoint(transform, Anchor.LEFT, previousWidget, Anchor.RIGHT, exactOffset);
			}
		}
	}

	private static void LayoutWidget_BottomLeft_Relative(Transform transform, ref GameObject previousWidget, Vector3 offsetFromPrevious = default(Vector3))
	{
		if (transform.gameObject.activeInHierarchy)
		{
			if (previousWidget == null)
			{
				LayoutWidget_LeftAligned_SetExactOffset(transform, previousWidget, LAYOUT_BOTTOMLEFT_START_POINT);
				previousWidget = transform.gameObject;
			}
			else
			{
				LayoutWidget_LeftAligned_SetExactOffset(transform, previousWidget, offsetFromPrevious + LAYOUT_OFFSET_PADDING);
				previousWidget = transform.gameObject;
			}
		}
	}

	private void PositionCurrencyFrame(GameObject parent, Vector3 offset)
	{
		List<GameObject> activeTooltips = new List<GameObject>();
		foreach (CurrencyFrame currencyFrame in m_currencyFrames)
		{
			GameObject tooltipObj = currencyFrame.GetTooltipObject();
			if (tooltipObj != null)
			{
				tooltipObj.SetActive(value: false);
				activeTooltips.Add(tooltipObj);
			}
		}
		TransformUtil.SetPoint(m_currencyFrameContainer, Anchor.RIGHT, parent, Anchor.LEFT, offset, includeInactive: false);
		if (m_currencyFrames.Count > 1)
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				TransformUtil.SetLocalPosY(m_currencyFrameContainer, 189.703f);
			}
			else
			{
				TransformUtil.SetLocalPosY(m_currencyFrameContainer, -2.850989f);
			}
		}
		activeTooltips.ForEach(delegate(GameObject obj)
		{
			obj.SetActive(value: true);
		});
	}

	public bool HandleKeyboardInput()
	{
		if (InputCollection.GetKeyUp(BackButton.backKey) || InputCollection.GetKeyUp(KeyCode.Escape))
		{
			return HandleEscapeKey();
		}
		ChatMgr chatMgr = ChatMgr.Get();
		if (chatMgr != null && chatMgr.HandleKeyboardInput())
		{
			return true;
		}
		return false;
	}

	public void ToggleGameMenu()
	{
		if (m_gameMenu == null)
		{
			LoadGameMenu();
			return;
		}
		if (m_gameMenu.GameMenuIsShown())
		{
			HideGameMenu();
			return;
		}
		m_gameMenu.GameMenuShow();
		if (this.OnMenuOpened != null)
		{
			this.OnMenuOpened();
		}
	}

	public bool IsActive()
	{
		return base.gameObject.activeSelf;
	}

	public void ToggleActive(bool active)
	{
		base.gameObject.SetActive(active);
		if (active)
		{
			UpdateLayout();
		}
	}

	public void PermanentlyDisableButtons()
	{
		m_buttonsDisabledPermanently = true;
		UpdateButtonEnableState();
	}

	public void ForceEnableButtons()
	{
		m_buttonsDisabledPermanently = false;
		m_buttonsDisabledByDialog.Clear();
		m_buttonsDisabledByRefCount = 0;
		UpdateButtonEnableState();
	}

	public void DisableButtonsByDialog(DialogBase dialog)
	{
		dialog.AddHiddenOrDestroyedListener(OnDisablingDialogHiddenOrDestroyed);
		m_buttonsDisabledByDialog.Add(dialog);
		UpdateButtonEnableState();
	}

	public void RequestDisableButtons()
	{
		m_buttonsDisabledByRefCount++;
		UpdateButtonEnableState();
	}

	public void CancelRequestToDisableButtons()
	{
		m_buttonsDisabledByRefCount--;
		UpdateButtonEnableState();
	}

	private void OnDisablingDialogHiddenOrDestroyed(DialogBase dialog, object userData)
	{
		m_buttonsDisabledByDialog.Remove(dialog);
		UpdateButtonEnableState();
	}

	public bool AreButtonsEnabled()
	{
		return m_buttonsEnabled;
	}

	public void HideGameMenu()
	{
		if (m_gameMenu != null && m_gameMenu.GameMenuIsShown())
		{
			m_gameMenu.GameMenuHide();
		}
	}

	public void HideOptionsMenu()
	{
		if (OptionsMenu.Get() != null && OptionsMenu.Get().IsShown())
		{
			OptionsMenu.Get().Hide();
		}
	}

	public void HideMiscellaneousMenu()
	{
		if (MiscellaneousMenu.Get() != null && MiscellaneousMenu.Get().IsShown())
		{
			MiscellaneousMenu.Get().Hide();
		}
	}

	public bool IsGameMenuShown()
	{
		if (m_gameMenu != null)
		{
			return m_gameMenu.GameMenuIsShown();
		}
		return false;
	}

	public void UpdateForPhone()
	{
		SceneMgr.Mode sceneMode = SceneMgr.Get().GetMode();
		bool menuAvailable = sceneMode == SceneMgr.Mode.HUB || sceneMode == SceneMgr.Mode.LOGIN || sceneMode == SceneMgr.Mode.GAMEPLAY || sceneMode == SceneMgr.Mode.LETTUCE_VILLAGE || IsCurrencyFrameActive();
		m_menuButton.gameObject.SetActive(menuAvailable);
	}

	public void UpdateLoginTooltip()
	{
		if (!Network.ShouldBeConnectedToAurora() && !m_suppressLoginTooltip && SceneMgr.Get().IsInGame() && GameMgr.Get().IsTraditionalTutorial() && !GameMgr.Get().IsSpectator() && DemoMgr.Get().GetMode() != DemoMode.BLIZZ_MUSEUM)
		{
			if (m_loginTooltip == null)
			{
				m_loginTooltip = AssetLoader.Get().InstantiatePrefab("LoginPointer.prefab:e26056ee6e4b89c45899d54bc9497bb0");
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					m_loginTooltip.transform.localScale = new Vector3(60f, 60f, 60f);
				}
				else
				{
					m_loginTooltip.transform.localScale = new Vector3(40f, 40f, 40f);
				}
				TransformUtil.SetEulerAngleX(m_loginTooltip, 270f);
				LayerUtils.SetLayer(m_loginTooltip, GameLayer.BattleNet);
				m_loginTooltip.transform.parent = base.transform;
			}
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				TransformUtil.SetPoint(m_loginTooltip, Anchor.RIGHT, m_batteryLevel.gameObject, Anchor.LEFT, new Vector3(-32f, 0f, 0f));
			}
			else
			{
				TransformUtil.SetPoint(m_loginTooltip, Anchor.RIGHT, m_menuButton, Anchor.LEFT, new Vector3(-80f, 0f, 0f));
			}
		}
		else
		{
			DestroyLoginTooltip();
		}
	}

	private void DestroyLoginTooltip()
	{
		if (m_loginTooltip != null)
		{
			UnityEngine.Object.Destroy(m_loginTooltip);
			m_loginTooltip = null;
		}
	}

	public void SuppressLoginTooltip(bool val)
	{
		m_suppressLoginTooltip = val;
		UpdateLayout();
	}

	private void ShowFriendList()
	{
		ChatMgr.Get().ShowFriendsList();
		m_hasUnacknowledgedPendingInvites = false;
		m_friendButton.ShowPendingInvitesIcon(m_hasUnacknowledgedPendingInvites);
	}

	public void HideFriendList()
	{
		ChatMgr.Get()?.CloseChatUI();
	}

	private void OnFriendButtonReleased(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay("Small_Click.prefab:2a1c5335bf08dc84eb6e04fc58160681");
		ToggleFriendListShowing();
		UpdateLayout();
	}

	private void ToggleFriendListShowing()
	{
		if (ChatMgr.Get().IsFriendListShowing())
		{
			HideFriendList();
		}
		else
		{
			ShowFriendList();
		}
		m_friendButton.HideTooltip();
	}

	private void UpdateButtonEnableState()
	{
		if (m_buttonsDisabledPermanently || m_buttonsDisabledByRefCount > 0 || m_buttonsDisabledByDialog.Any())
		{
			m_buttonsEnabled = false;
			m_menuButton.SetEnabled(enabled: false);
			m_friendButton.SetEnabled(enabled: false);
			SetBlockCurrencyFrames(isBlocked: true);
			HideMiscellaneousMenu();
			HideOptionsMenu();
			HideGameMenu();
			HideFriendList();
		}
		else
		{
			m_buttonsEnabled = true;
			m_menuButton.SetEnabled(enabled: true);
			m_friendButton.SetEnabled(enabled: true);
			SetBlockCurrencyFrames(isBlocked: false);
		}
	}

	private void WillReset()
	{
		if (m_gameMenu != null)
		{
			if (m_gameMenu.GameMenuIsShown())
			{
				m_gameMenu.GameMenuHide();
			}
			UnityEngine.Object.DestroyImmediate(m_gameMenu.GameMenuGetGameObject());
			m_gameMenu = null;
		}
		DestroyLoginTooltip();
		ToggleActive(active: false);
		m_isLoggedIn = false;
	}

	private bool HandleEscapeKey()
	{
		if (m_gameMenu != null && m_gameMenu.GameMenuIsShown())
		{
			m_gameMenu.GameMenuHide();
			return true;
		}
		if (OptionsMenu.Get() != null && OptionsMenu.Get().IsShown())
		{
			OptionsMenu.Get().Hide();
			return true;
		}
		if (MiscellaneousMenu.Get() != null && MiscellaneousMenu.Get().IsShown())
		{
			MiscellaneousMenu.Get().Hide();
			return true;
		}
		if (QuestLog.Get() != null && QuestLog.Get().IsShown())
		{
			QuestLog.Get().Hide();
			return true;
		}
		ChatMgr chatMgr = ChatMgr.Get();
		if (chatMgr != null && chatMgr.HandleKeyboardInput())
		{
			return true;
		}
		if (CraftingTray.Get() != null && CraftingTray.Get().IsShown())
		{
			CraftingTray.Get().Hide();
			return true;
		}
		if (PrivacyMenu.Get() != null && PrivacyMenu.Get().IsShown())
		{
			PrivacyMenu.Get().Hide();
			if (OptionsMenu.Get() != null)
			{
				OptionsMenu.Get().Show();
			}
			return true;
		}
		if (PrivacySettingsMenu.Get() != null && PrivacySettingsMenu.Get().IsShown())
		{
			PrivacySettingsMenu.Get().Hide();
			if (PrivacyMenu.Get() != null)
			{
				PrivacyMenu.Get().Show();
			}
			return true;
		}
		if (SoundOptionsMenu.Get() != null && SoundOptionsMenu.Get().IsShown())
		{
			SoundOptionsMenu.Get().Hide();
			if (OptionsMenu.Get() != null)
			{
				OptionsMenu.Get().Show();
			}
			return true;
		}
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		switch (mode)
		{
		case SceneMgr.Mode.FATAL_ERROR:
			return true;
		case SceneMgr.Mode.LOGIN:
			return true;
		case SceneMgr.Mode.STARTUP:
			return true;
		default:
			if (!DemoMgr.Get().IsHubEscMenuEnabled(mode == SceneMgr.Mode.GAMEPLAY))
			{
				return true;
			}
			ToggleGameMenu();
			return true;
		}
	}

	private void OnMenuButtonReleased(UIEvent e)
	{
		if (GameMgr.Get().IsSpectator() || GameState.Get() == null || !GameState.Get().IsInTargetMode())
		{
			ToggleGameMenu();
		}
	}

	private void LoadGameMenu()
	{
		if (!m_gameMenuLoading && m_gameMenu == null)
		{
			m_gameMenuLoading = true;
			AssetLoader.Get().InstantiatePrefab("GameMenu.prefab:dc76cbcfb64a34d7e93755df33db2f80", ShowGameMenu);
		}
	}

	private void ShowGameMenu(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_gameMenu = go.GetComponent<GameMenu>();
		m_gameMenu.GameMenuShow();
		if (this.OnMenuOpened != null)
		{
			this.OnMenuOpened();
		}
		m_gameMenuLoading = false;
	}

	private void UpdateForDemoMode()
	{
		if (DemoMgr.Get().IsExpoDemo())
		{
			SceneMgr.Mode sceneMode = SceneMgr.Get().GetMode();
			bool menuAvailable = false;
			bool friendsAvailable = true;
			switch (DemoMgr.Get().GetMode())
			{
			case DemoMode.PAX_EAST_2013:
			case DemoMode.BLIZZCON_2013:
			case DemoMode.BLIZZCON_2015:
			case DemoMode.BLIZZCON_2017_ADVENTURE:
			case DemoMode.BLIZZCON_2017_BRAWL:
				menuAvailable = sceneMode == SceneMgr.Mode.GAMEPLAY;
				friendsAvailable = false;
				m_currencyFrameContainer.SetActive(value: false);
				break;
			case DemoMode.BLIZZCON_2014:
				friendsAvailable = (menuAvailable = sceneMode != SceneMgr.Mode.FRIENDLY);
				break;
			case DemoMode.BLIZZ_MUSEUM:
				menuAvailable = (friendsAvailable = false);
				break;
			case DemoMode.ANNOUNCEMENT_5_0:
				friendsAvailable = true;
				menuAvailable = true;
				break;
			case DemoMode.BLIZZCON_2016:
			case DemoMode.BLIZZCON_2018_BRAWL:
			case DemoMode.BLIZZCON_2019_BATTLEGROUNDS:
				menuAvailable = sceneMode == SceneMgr.Mode.GAMEPLAY;
				friendsAvailable = sceneMode == SceneMgr.Mode.HUB;
				break;
			default:
				menuAvailable = sceneMode != SceneMgr.Mode.FRIENDLY && sceneMode != SceneMgr.Mode.TOURNAMENT;
				break;
			}
			if ((sceneMode == SceneMgr.Mode.GAMEPLAY || (uint)(sceneMode - 7) <= 1u) && DemoMgr.Get().GetMode() != DemoMode.ANNOUNCEMENT_5_0)
			{
				friendsAvailable = false;
			}
			if (!menuAvailable)
			{
				m_menuButton.gameObject.SetActive(value: false);
			}
			if (!friendsAvailable)
			{
				m_friendButton.gameObject.SetActive(value: false);
			}
		}
	}

	private void UpdateForTutorialPreviewVideos(SceneMgr.Mode mode)
	{
		if (mode == SceneMgr.Mode.HUB && !GameUtils.IsAnyTutorialComplete())
		{
			m_friendButton.gameObject.SetActive(value: false);
			m_currencyFrameContainer.SetActive(value: false);
			m_currentTime.gameObject.SetActive(value: false);
		}
		else
		{
			m_currencyFrameContainer.SetActive(value: true);
			m_currentTime.gameObject.SetActive(value: true);
			m_friendButton.gameObject.SetActive(Network.ShouldBeConnectedToAurora());
		}
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (mode == SceneMgr.Mode.FATAL_ERROR)
		{
			return;
		}
		m_suppressLoginTooltip = false;
		RefreshCurrency();
		int num;
		if (mode != 0)
		{
			num = ((mode != SceneMgr.Mode.FATAL_ERROR) ? 1 : 0);
			if (num != 0)
			{
				if (SpectatorManager.Get().IsInSpectatorMode())
				{
					SpectatorManager_OnSpectatorModeChanged(OnlineEventType.ADDED, null);
				}
				goto IL_0061;
			}
		}
		else
		{
			num = 0;
		}
		if (m_spectatorModeIndicator != null && m_spectatorModeIndicator.activeSelf)
		{
			m_spectatorModeIndicator.SetActive(value: false);
		}
		goto IL_0061;
		IL_0061:
		if (num != 0 && m_spectatorCountPanel != null)
		{
			bool showSpectatorCount = SpectatorManager.Get().IsBeingSpectated();
			if ((bool)UniversalInputManager.UsePhoneUI && SceneMgr.Get() != null && !SceneMgr.Get().IsInGame())
			{
				showSpectatorCount = false;
			}
			m_spectatorCountPanel.SetActive(showSpectatorCount);
		}
		UpdateForTutorialPreviewVideos(mode);
		UpdateForDemoMode();
		UpdateLayout();
	}

	private void OnCurrencyBalanceChangedInternal(CurrencyBalanceChangedEventArgs e)
	{
		if (e.Currency == CurrencyType.GOLD)
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_GOLD, out long hasSeenGoldFlag);
			if (hasSeenGoldFlag == 0L)
			{
				RefreshCurrency();
			}
		}
	}

	private void SpectatorManager_OnInviteReceived(OnlineEventType evt, BnetPlayer inviter)
	{
		if (ChatMgr.Get().IsFriendListShowing() || !SpectatorManager.Get().HasAnyReceivedInvites())
		{
			m_hasUnacknowledgedPendingInvites = false;
		}
		else
		{
			m_hasUnacknowledgedPendingInvites = m_hasUnacknowledgedPendingInvites || evt == OnlineEventType.ADDED;
		}
		if (m_friendButton != null)
		{
			m_friendButton.ShowPendingInvitesIcon(m_hasUnacknowledgedPendingInvites);
		}
	}

	private void SpectatorManager_OnSpectatorToMyGame(OnlineEventType evt, BnetPlayer spectator)
	{
		int countSpectators = SpectatorManager.Get().GetCountSpectatingMe();
		if (countSpectators <= 0)
		{
			if (m_spectatorCountPanel == null)
			{
				return;
			}
		}
		else if (m_spectatorCountPanel == null)
		{
			string path = m_spectatorCountPrefabPath;
			AssetLoader.Get().InstantiatePrefab(path, delegate(AssetReference n, GameObject go, object d)
			{
				BnetBar bnetBar = Get();
				if (!(bnetBar == null))
				{
					if (bnetBar.m_spectatorCountPanel != null)
					{
						UnityEngine.Object.Destroy(go);
					}
					else
					{
						bnetBar.m_spectatorCountPanel = go;
						bnetBar.m_spectatorCountPanel.transform.parent = bnetBar.transform;
						bnetBar.m_spectatorCountPanel.transform.localEulerAngles = Vector3.zero;
						TransformOverride component = bnetBar.m_spectatorCountPanel.GetComponent<TransformOverride>();
						if (component != null)
						{
							int bestScreenMatch = PlatformSettings.GetBestScreenMatch(component.m_screenCategory);
							m_initialSpectatorCountScaleX = component.m_localScale[bestScreenMatch].x;
						}
						PegUIElement component2 = go.GetComponent<PegUIElement>();
						if (component2 != null)
						{
							component2.AddEventListener(UIEventType.ROLLOVER, SpectatorCount_OnRollover);
							component2.AddEventListener(UIEventType.ROLLOUT, SpectatorCount_OnRollout);
						}
						Material material = bnetBar.m_spectatorCountPanel.transform.Find("BeingWatchedHighlight").gameObject.GetComponent<Renderer>().GetMaterial();
						Color color = material.color;
						color.a = 0f;
						material.color = color;
					}
					Get().SpectatorManager_OnSpectatorToMyGame(evt, spectator);
				}
			});
			return;
		}
		m_spectatorCountPanel.transform.Find("UberText").GetComponent<UberText>().Text = countSpectators.ToString();
		bool showSpectatorCount = countSpectators > 0;
		if ((bool)UniversalInputManager.UsePhoneUI && SceneMgr.Get() != null && !SceneMgr.Get().IsInGame())
		{
			showSpectatorCount = false;
		}
		m_spectatorCountPanel.SetActive(showSpectatorCount);
		UpdateLayout();
		GameObject target = m_spectatorCountPanel.transform.Find("BeingWatchedHighlight").gameObject;
		iTween.Stop(target, includechildren: true);
		Action<object> fadeOutCb = delegate
		{
			if (!(Get() == null))
			{
				iTween.FadeTo(Get().m_spectatorCountPanel.transform.Find("BeingWatchedHighlight").gameObject, 0f, 0.5f);
			}
		};
		Hashtable fadeInArgs = iTweenManager.Get().GetTweenHashTable();
		fadeInArgs.Add("alpha", 1f);
		fadeInArgs.Add("time", 0.5f);
		fadeInArgs.Add("oncomplete", fadeOutCb);
		iTween.FadeTo(target, fadeInArgs);
	}

	private static void SpectatorCount_OnRollover(UIEvent evt)
	{
		BnetBar bnetBar = Get();
		if (bnetBar == null)
		{
			return;
		}
		string header = GameStrings.Get("GLOBAL_SPECTATOR_COUNT_PANEL_HEADER");
		BnetGameAccountId[] spectators = SpectatorManager.Get().GetSpectatorPartyMembers();
		string description;
		if (spectators.Length == 1)
		{
			string name = BnetUtils.GetPlayerBestName(spectators[0]);
			description = GameStrings.Format("GLOBAL_SPECTATOR_COUNT_PANEL_TEXT_ONE", name);
		}
		else
		{
			string[] names = spectators.Select((BnetGameAccountId id) => BnetUtils.GetPlayerBestName(id)).ToArray();
			description = string.Join(", ", names);
		}
		bnetBar.m_spectatorCountTooltipZone.ShowSocialTooltip(bnetBar.m_spectatorCountPanel, header, description, 18.75f, GameLayer.BattleNetDialog);
		bnetBar.m_spectatorCountTooltipZone.AnchorTooltipTo(bnetBar.m_spectatorCountPanel, Anchor.TOP_LEFT_XZ, Anchor.BOTTOM_LEFT_XZ);
	}

	private static void SpectatorCount_OnRollout(UIEvent evt)
	{
		BnetBar bnetBar = Get();
		if (!(bnetBar == null))
		{
			bnetBar.m_spectatorCountTooltipZone.HideTooltip();
		}
	}

	private void ShowSpectatorModeIndicator(bool show)
	{
		if (m_spectatorModeIndicator != null)
		{
			m_spectatorModeIndicator.SetActive(show);
		}
		if (show)
		{
			UberText label = m_spectatorModeIndicator.GetComponentInChildren<UberText>();
			if (label != null && SpectatorManager.Get().IsInSpectatorMode())
			{
				label.Text = GameStrings.Get("GLOBAL_SPECTATOR_MODE_INDICATOR_TEXT");
			}
		}
	}

	private void CheckSpectatorModeIndicator()
	{
		if (ShouldShowSpectatorModeIndicator && m_spectatorModeIndicator == null)
		{
			string path = m_spectatorModeIndicatorPrefab;
			AssetLoader.Get().InstantiatePrefab(path, delegate(AssetReference n, GameObject go, object d)
			{
				BnetBar bnetBar = Get();
				if (!(bnetBar == null) && !(go == null))
				{
					if (bnetBar.m_spectatorModeIndicator != null)
					{
						UnityEngine.Object.Destroy(go);
					}
					else
					{
						bnetBar.m_spectatorModeIndicator = go;
						bnetBar.m_spectatorModeIndicator.transform.parent = bnetBar.transform;
						TransformOverride component = go.GetComponent<TransformOverride>();
						if (component != null)
						{
							int bestScreenMatch = PlatformSettings.GetBestScreenMatch(component.m_screenCategory);
							m_initialSpectatorModeIndicatorScaleX = component.m_localScale[bestScreenMatch].x;
						}
					}
					Get().CheckSpectatorModeIndicator();
				}
			});
		}
		else if (!(m_spectatorModeIndicator == null))
		{
			UpdateLayout();
		}
	}

	private void SpectatorManager_OnSpectatorModeChanged(OnlineEventType evt, BnetPlayer spectatee)
	{
		CheckSpectatorModeIndicator();
	}

	public void ShowSkipTutorialButton()
	{
		if (SpectatorManager.Get().IsInSpectatorMode())
		{
			HideSkipTutorialButton();
			m_showSkipTutorialButton = false;
		}
		else
		{
			m_showSkipTutorialButton = true;
			UpdateLayout();
		}
	}

	public void SkipTutorialPopupDismissed()
	{
		Get().ShowSkipTutorialButton();
		BnetBar.SkipTutorialSelected?.Invoke(obj: false);
	}

	public void HideSkipTutorialButton()
	{
		m_showSkipTutorialButton = false;
		UpdateLayout();
	}

	public void SetupSkipTutorialButton()
	{
		SkipTutorialButton.GetComponent<WidgetInstance>().RegisterReadyListener(delegate
		{
			UpdateLayout();
			SkipTutorialButton.GetComponentInChildren<Clickable>().AddEventListener(UIEventType.RELEASE, OnSkipTutorialButtonClicked);
		});
	}

	public void Dim()
	{
		Material sharedMaterial = m_menuButtonMesh.GetComponent<Renderer>().GetSharedMaterial();
		Material friendButtonMat = FriendButtonMesh.GetComponent<Renderer>().GetSharedMaterial();
		Color matColor = DimButtonMaterial.color;
		Color iconColor = Color.Lerp(matColor, Color.black, IconColorDimFactor);
		sharedMaterial.shader = DimButtonMaterial.shader;
		sharedMaterial.color = matColor;
		friendButtonMat.shader = DimButtonMaterial.shader;
		friendButtonMat.color = matColor;
		m_menuButton.SetEnabled(enabled: false);
		m_friendButton.SetEnabled(enabled: false);
		m_friendButton.SetColor(matColor);
		m_menuButton.SetColor(matColor, DimButtonMaterial.shader);
		m_batteryLevelPhone.GetComponent<Renderer>().GetSharedMaterial().color = iconColor;
		UpdateLayout();
	}

	public void Undim()
	{
		Material mat = m_menuButtonMesh.GetComponent<Renderer>().GetSharedMaterial();
		Material sharedMaterial = FriendButtonMesh.GetComponent<Renderer>().GetSharedMaterial();
		Color returnColor = Color.white;
		mat.shader = m_defaultUiMaterial.shader;
		sharedMaterial.shader = m_defaultUiMaterial.shader;
		m_menuButton.SetEnabled(enabled: true);
		m_friendButton.SetEnabled(enabled: true);
		m_friendButton.SetColor(returnColor);
		m_menuButton.SetColor(returnColor, m_defaultUiMaterial.shader);
		m_batteryLevelPhone.GetComponent<Renderer>().GetSharedMaterial().color = returnColor;
		UpdateLayout();
	}

	public void OnSkipTutorialButtonClicked(UIEvent e)
	{
		BnetBar.SkipTutorialSelected?.Invoke(obj: true);
		WidgetInstance tutorialSkipPopup = m_tutorialSkipPopup;
		if (tutorialSkipPopup != null)
		{
			tutorialSkipPopup.gameObject.SetActive(value: true);
			return;
		}
		WidgetInstance inst = WidgetInstance.Create("SkipTutorial_Popup.prefab:8f770936aa3f2e542bec83d77d0d6733");
		m_tutorialSkipPopup = inst;
	}

	private void OnRequestGetServerTimeResponse()
	{
		ResponseWithRequest<GetServerTimeResponse, GetServerTimeRequest> response = Network.Get().GetServerTimeResponse();
		ulong now = TimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);
		m_serverClientOffsetInSec = response.Response.ServerUnixTime - (long)now;
	}

	public void Cheat_SetServerTimeOffset(double offsetSec)
	{
		m_serverClientOffsetInSec = offsetSec;
	}
}
