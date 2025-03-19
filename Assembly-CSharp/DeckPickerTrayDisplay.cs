using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Configuration;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;
using UnityEngine.Serialization;

[CustomEditClass]
public class DeckPickerTrayDisplay : AbsDeckPickerTrayDisplay
{
	[Serializable]
	public class ModeTextures
	{
		[SerializeField]
		public Texture customStandardTex;

		[SerializeField]
		public Texture customWildTex;

		[SerializeField]
		public Texture customClassicTex;

		[SerializeField]
		public Texture customCasualTex;

		[SerializeField]
		public Texture standardTex;

		[SerializeField]
		public Texture wildTex;

		[SerializeField]
		public Texture classicTex;

		[SerializeField]
		public Texture casualTex;

		[FormerlySerializedAs("coreRewardsTex")]
		[SerializeField]
		public Texture apprenticeTex;

		[SerializeField]
		public Texture classDivotTex;

		[SerializeField]
		public Texture guestHeroDivotTex;

		public Texture GetTextureForFormat(VisualsFormatType visualsFormatType)
		{
			switch (visualsFormatType)
			{
			case VisualsFormatType.VFT_STANDARD:
				return standardTex;
			case VisualsFormatType.VFT_WILD:
				return wildTex;
			case VisualsFormatType.VFT_CLASSIC:
				return classicTex;
			case VisualsFormatType.VFT_TWIST:
				return classicTex;
			case VisualsFormatType.VFT_CASUAL:
				if (GameUtils.HasCompletedApprentice() || !(apprenticeTex != null))
				{
					return casualTex;
				}
				return apprenticeTex;
			default:
				Debug.LogError("DeckPickerTrayDisplay.GetTextureForFormat does not support " + visualsFormatType);
				return null;
			}
		}

		public Texture GetCustomTextureForFormat(VisualsFormatType visualsFormatType)
		{
			switch (visualsFormatType)
			{
			case VisualsFormatType.VFT_STANDARD:
				return customStandardTex;
			case VisualsFormatType.VFT_WILD:
				return customWildTex;
			case VisualsFormatType.VFT_CLASSIC:
				return customClassicTex;
			case VisualsFormatType.VFT_TWIST:
				return customClassicTex;
			case VisualsFormatType.VFT_CASUAL:
				return customCasualTex;
			default:
				Debug.LogError("DeckPickerTrayDisplay.GetCustomTextureForFormat does not support " + visualsFormatType);
				return null;
			}
		}
	}

	public delegate void FormatSwitchButtonPressed();

	private class VisualFormatTypeConfig
	{
		public HashSet<CustomDeckPage.PageType> EnabledPageTypes = new HashSet<CustomDeckPage.PageType>();

		public bool m_showDeckContentsForSelectedDeck;

		public bool ShowClassWins = true;
	}

	private enum SetRotationTutorialState
	{
		INACTIVE,
		PREPARING,
		READY,
		SHOW_TUTORIAL_POPUPS,
		SWITCH_MODE_WALKTHROUGH,
		SHOW_QUEST_LOG
	}

	public Transform m_rankedPlayDisplayWidgetBone;

	public Texture m_emptyHeroTexture;

	public NestedPrefab m_leftArrowNestedPrefab;

	public NestedPrefab m_rightArrowNestedPrefab;

	public GameObject m_modeLabelBg;

	public GameObject m_randomDecksHiddenBone;

	public GameObject m_suckedInRandomDecksBone;

	public HeroXPBar m_xpBarPrefab;

	public GameObject m_rankedWinsPlate;

	public UberText m_rankedWins;

	public BoxCollider m_clickBlocker;

	public Animator m_premadeDeckGlowAnimator;

	public GameObject m_hierarchyDeckTray;

	public GameObject m_replayableTutorial;

	[CustomEditField(Sections = "Deck Pages")]
	public GameObject m_customDeckPagesRoot;

	[CustomEditField(Sections = "Deck Pages")]
	public GameObject m_customDeckPageUpperBone;

	[CustomEditField(Sections = "Deck Pages")]
	public GameObject m_customDeckPageLowerBone;

	[CustomEditField(Sections = "Deck Pages")]
	public GameObject m_customDeckPageHideBone;

	public GameObject m_missingTwistDeck;

	[FormerlySerializedAs("m_coreCardRewardContainer")]
	public GameObject m_apprenticeProgressContainer;

	[FormerlySerializedAs("m_coreCardRewardWidget")]
	public Widget m_apprenticeProgressWidget;

	[FormerlySerializedAs("m_coreCardRewardPopupWidget")]
	public Widget m_apprenticeRewardPopupWidget;

	public Widget m_apprenticeHeroPreviewPopupWidget;

	public Widget m_apprenticeHeroPortraitButton;

	public TooltipZone m_apprenticeXpBarTooltipZone;

	public HighlightState m_collectionButtonGlow;

	public GameObject m_labelDecoration;

	public List<PlayMakerFSM> formatChangeGlowFSMs;

	public List<PlayMakerFSM> newDeckFormatChangeGlowFSMs;

	public List<GameObject> m_premadeDeckGlowBurstObjects;

	public NestedPrefab m_switchFormatButtonContainer;

	public float m_formatTypePickerYOffset;

	private SwitchFormatButton m_switchFormatButton;

	public GameObject m_TheClockButtonBone;

	public string m_leavingWildGlowEvent;

	public string m_leavingTwistGlowEvent;

	public string m_leavingCasualGlowEvent;

	public string m_leavingCasualWithRewardsGlowEvent;

	public string m_enteringWildGlowEvent;

	public string m_enteringTwistGlowEvent;

	public string m_enteringCasualGlowEvent;

	public string m_enteringCasualWithRewardsGlowEvent;

	public string m_newDeckLeavingClassicGlowEvent;

	public string m_newDeckEnteringClassicGlowEvent;

	public string m_newDeckLeavingWildGlowEvent;

	public string m_newDeckEnteringWildGlowEvent;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_standardTransitionSound;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_wildTransitionSound;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_classicTransitionSound;

	[CustomEditField(Sections = "Deck Sharing")]
	public UIBButton m_DeckShareRequestButton;

	[CustomEditField(Sections = "Deck Sharing")]
	public GameObject m_DeckShareGlowOutQuad;

	[CustomEditField(Sections = "Deck Sharing")]
	public float m_DeckShareGlowOutIntensity;

	[CustomEditField(Sections = "Deck Sharing")]
	public ParticleSystem m_DeckShareParticles;

	[CustomEditField(Sections = "Deck Sharing")]
	public float m_DeckShareTransitionTime = 1f;

	[CustomEditField(Sections = "Phone Only")]
	public SlidingTray m_rankedDetailsTray;

	[CustomEditField(Sections = "Phone Only")]
	public GameObject m_detailsTrayFrame;

	[CustomEditField(Sections = "Phone Only")]
	public Transform m_medalBone_phone;

	[CustomEditField(Sections = "Phone Only")]
	public Mesh m_alternateDetailsTrayMesh;

	[CustomEditField(Sections = "Phone Only")]
	public Material m_arrowButtonShadowMaterial;

	[CustomEditField(Sections = "Phone Only")]
	public Transform Ranked_HeroName_Bone;

	[CustomEditField(Sections = "Phone Only")]
	public Transform m_rankedTrayDeckPickerFrame;

	[CustomEditField(Sections = "Phone Only")]
	public Transform m_rankedTrayShownBone;

	[CustomEditField(Sections = "Phone Only")]
	public Transform m_deckPickerHeroPortraitClickable;

	[FormerlySerializedAs("m_coreRewardTray")]
	public SlidingTray m_apprenticeProgressTray;

	[FormerlySerializedAs("m_casualCoreRewardTrayMesh")]
	public GameObject m_apprenticeProgressTrayMesh;

	[CustomEditField(Sections = "Heroic Decks")]
	public RenderToTexture m_UnlockableDeckR2T;

	[CustomEditField(Sections = "Mode Background Textures")]
	public ModeTextures m_adventureTextures;

	[CustomEditField(Sections = "Mode Background Textures")]
	public ModeTextures m_collectionTextures;

	[CustomEditField(Sections = "Mode Background Textures")]
	public ModeTextures m_tavernBrawlTextures;

	[CustomEditField(Sections = "Mode Background Textures")]
	public ModeTextures m_tournamentTextures;

	[CustomEditField(Sections = "Mode Background Textures")]
	public ModeTextures m_friendlyTextures;

	public float m_rankedPlayDisplayShowDelay;

	public float m_rankedPlayDisplayHideDelay;

	[CustomEditField(Sections = "Offset For DeckDisplay")]
	public DeckDisplayConfig m_deckDisplayConfig;

	[CustomEditField(Sections = "Hero XP Bar")]
	public Vector3 m_xpBarPosition = new Vector3(-0.1776525f, 0.2245596f, -0.7309282f);

	public Vector3 m_xpBarScale = new Vector3(0.89f, 0.89f, 0.89f);

	private const float TRAY_SLIDE_TIME = 0.25f;

	private static readonly Vector3 INNKEEPER_QUOTE_POS = new Vector3(103f, NotificationManager.DEPTH, 42f);

	private static readonly AssetReference CUSTOM_DECK_PAGE = new AssetReference("CustomDeckPage_Top.prefab:650072e121717c04f89ac014eb3dc290");

	private static readonly AssetReference LOANER_DECK_TIMER = new AssetReference("LoanerDeckTimer.prefab:6d916f882c937614f89791cc925a6a9d");

	private static readonly AssetReference FORMAT_TYPE_PICKER_POPUP_PREFAB = new AssetReference("FormatTypePickerPopup.prefab:aa88133d144782b40b3fd8818084006c");

	private const string CREATE_WILD_DECK_STRING_FORMAT = "GLUE_CREATE_WILD_DECK";

	private const string CREATE_STANDARD_DECK_STRING_FORMAT = "GLUE_CREATE_STANDARD_DECK";

	private const string CREATE_CLASSIC_DECK_STRING_FORMAT = "GLUE_CREATE_CLASSIC_DECK";

	private const string CREATE_TWIST_DECK_STRING_FORMAT = "GLUE_CREATE_TWIST_DECK";

	private const string WILD_CLICKED_EVENT_NAME = "WILD_BUTTON_CLICKED";

	private const string STANDARD_CLICKED_EVENT_NAME = "STANDARD_BUTTON_CLICKED";

	private const string CLASSIC_CLICKED_EVENT_NAME = "CLASSIC_BUTTON_CLICKED";

	private const string TWIST_CLICKED_EVENT_NAME = "TWIST_BUTTON_CLICKED";

	private const string CASUAL_CLICKED_EVENT_NAME = "CASUAL_BUTTON_CLICKED";

	private const string OPEN = "OPEN";

	private const string SET_ROTATION_OPEN = "SET_ROTATION_OPEN";

	private const string HIDE = "HIDE";

	private const string FORMAT_PICKER_4_BUTTONS = "4BUTTONS";

	private const string FORMAT_PICKER_3_BUTTONS = "3BUTTONS";

	private const string FORMAT_PICKER_2_BUTTONS = "2BUTTONS";

	private const string FORMAT_PICKER_2_BUTTONS_WILD = "2BUTTONS_WILD";

	private const string CHEST_BUTTON_CLICKED = "CHEST_BUTTON_CLICKED";

	private const string GO_TO_JOURNAl_CLICKED = "GO_TO_JOURNAL_CLICKED";

	private const string SHOW_APPRENTICE_REWARD_POPUP = "SHOW_POPUP";

	private const string SHOW_XP_BAR_TOOLTIP = "SHOW_XP_BAR_TOOLTIP";

	private const string HIDE_XP_BAR_TOOLTIP = "HIDE_XP_BAR_TOOLTIP";

	private const string ENABLE_HERO_PORTRAIT_BUTTON = "CODE_ENABLE_PORTRAIT_BUTTON";

	private const string DISABLE_HERO_PORTRAIT_BUTTON = "CODE_DISABLE_PORTRAIT_BUTTON";

	private UIBButton m_leftArrow;

	private UIBButton m_rightArrow;

	private HeroXPBar m_xpBar;

	private CollectionDeckBoxVisual m_selectedCustomDeckBox;

	private ModeTextures m_currentModeTextures;

	private bool m_heroChosen;

	private static Coroutine s_selectHeroCoroutine = null;

	private DeckPickerMode m_deckPickerMode;

	private int m_currentPageIndex;

	private static DeckPickerTrayDisplay s_instance;

	private RankedPlayDisplay m_rankedPlayDisplay;

	private int m_numPagesToShow = 1;

	private List<CustomDeckPage> m_customPages = new List<CustomDeckPage>();

	private Notification m_expoThankQuote;

	private Notification m_expoIntroQuote;

	private Notification m_switchFormatPopup;

	private Notification m_innkeeperQuote;

	private GameLayer m_defaultDetailsLayer;

	private bool m_usingSharedDecks;

	private bool m_doingDeckShareTransition;

	private bool m_isDeckShareRequestButtonHovered;

	private long m_lastSeasonBonusStarPopUpSeen;

	private long m_bonusStarsPopUpSeenCount;

	private TranslatedMedalInfo m_currentMedalInfo;

	private bool m_inHeroPicker;

	private VisualsFormatType m_visualsFormatType;

	private Widget m_formatTypePickerWidget;

	private Widget m_rankedPlayDisplayWidget;

	private Hearthstone.Progression.RewardTrack m_rewardTrack;

	private RewardTrackDataModel m_rewardTrackDataModel;

	private bool m_HasSeenPlayStandardToWildVO;

	private bool m_HasSeenPlayStandardToTwistVO;

	private Coroutine m_showLeftArrowCoroutine;

	private Coroutine m_showRightArrowCoroutine;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private Vector3? m_heroPowerContainerOffset;

	private Dictionary<VisualsFormatType, VisualFormatTypeConfig> m_formatConfig;

	private bool appliedDeckTrayConfigChanges;

	private CustomFrameController m_customFrameController;

	private bool LegendarySkin_DynamicResolutionEnabled = LegendarySkin.DynamicResolutionEnabled;

	private const int s_nathriaMysteryScenarioId = 5026;

	private const long s_nathriaMysteryDeckHash = -7487123274682292298L;

	private const int s_nathriaMysteryDeckSize = 30;

	private const int s_tenthAnniversaryMysteryScenarioId = 5396;

	private const long s_tenthAnniversaryMysteryDeckHash = 4901740154402535512L;

	private const int s_tenthAnniversaryMysteryDeckSize = 43;

	[CustomEditField(Sections = "Set Rotation Tutorial")]
	public GameObject m_formatTutorialPopUpPrefab;

	[CustomEditField(Sections = "Set Rotation Tutorial")]
	public Transform m_formatTutorialPopUpBone;

	[CustomEditField(Sections = "Set Rotation Tutorial")]
	public Transform m_Switch_Format_Notification_Bone;

	[CustomEditField(Sections = "Set Rotation Tutorial")]
	public Transform m_Switch_Format_Notification_Bone_Mobile;

	[CustomEditField(Sections = "Set Rotation Tutorial")]
	public Animator m_dimQuad;

	[CustomEditField(Sections = "Set Rotation Tutorial")]
	public PegUIElement m_clickCatcher;

	[CustomEditField(Sections = "Set Rotation Tutorial", T = EditType.SOUND_PREFAB)]
	public string m_wildDeckTransitionSound;

	private const float FORMAT_PICKER_ICON_SLOT_TIME = 1f;

	private SetRotationTutorialState m_setRotationTutorialState;

	private float m_showQuestPause = 1f;

	private float m_playVOPause = 1f;

	private bool m_shouldContinue;

	private List<long> m_noticeIdsToAck = new List<long>();

	public bool IsModeSwitchShowing { get; private set; }

	private bool IsInApprentice => !GameUtils.HasCompletedApprentice();

	public static event FormatSwitchButtonPressed OnFormatSwitchButtonPressed;

	public override void Awake()
	{
		base.Awake();
		m_formatConfig = new Dictionary<VisualsFormatType, VisualFormatTypeConfig>();
		VisualFormatTypeConfig basic = new VisualFormatTypeConfig();
		basic.EnabledPageTypes.Add(CustomDeckPage.PageType.CUSTOM_DECK_DISPLAY);
		basic.EnabledPageTypes.Add(CustomDeckPage.PageType.LOANER_DECK_DISPLAY);
		m_formatConfig.Add(VisualsFormatType.VFT_STANDARD, basic);
		m_formatConfig.Add(VisualsFormatType.VFT_WILD, basic);
		m_formatConfig.Add(VisualsFormatType.VFT_TWIST, basic);
		m_formatConfig.Add(VisualsFormatType.VFT_CASUAL, basic);
		if (RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
		{
			VisualFormatTypeConfig preconTwist = new VisualFormatTypeConfig();
			preconTwist.EnabledPageTypes.Add(CustomDeckPage.PageType.PRECON_DECK_DISPLAY);
			m_formatConfig[VisualsFormatType.VFT_TWIST] = preconTwist;
			m_formatConfig[VisualsFormatType.VFT_TWIST].m_showDeckContentsForSelectedDeck = true;
			m_formatConfig[VisualsFormatType.VFT_TWIST].ShowClassWins = false;
		}
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
		SoundManager.Get().Load("hero_panel_slide_on.prefab:236147a924d7cb442872b46dddd56132");
		SoundManager.Get().Load("hero_panel_slide_off.prefab:ed410a050e783564384ca51e701ede4d");
		if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY)
		{
			LoadingScreen.Get().RegisterFinishedTransitionListener(OnTransitionFromGameplayFinished);
		}
		SceneMgr.Get().RegisterScenePreUnloadEvent(OnScenePreUnload);
		s_instance = this;
		if (m_collectionButton != null)
		{
			if (IsDeckSharingActive())
			{
				m_collectionButton.gameObject.SetActive(value: false);
			}
			else
			{
				m_collectionButton.gameObject.SetActive(value: true);
				SetCollectionButtonEnabled(ShouldShowCollectionButton());
				if (m_collectionButton.IsEnabled())
				{
					TelemetryWatcher.WatchFor(TelemetryWatcherWatchType.CollectionManagerFromDeckPicker);
					m_collectionButton.SetText(GameStrings.Get("GLUE_MY_COLLECTION"));
					m_collectionButton.AddEventListener(UIEventType.RELEASE, CollectionButtonPress);
				}
			}
		}
		if (m_DeckShareRequestButton != null)
		{
			if (IsDeckSharingActive())
			{
				m_DeckShareRequestButton.gameObject.SetActive(value: true);
				EnableRequestDeckShareButton(enable: true);
				m_DeckShareRequestButton.SetText(GameStrings.Get("GLUE_DECK_SHARE_BUTTON_BORROW_DECKS"));
				m_DeckShareRequestButton.AddEventListener(UIEventType.RELEASE, RequestDeckShareButtonPress);
				m_DeckShareRequestButton.AddEventListener(UIEventType.ROLLOVER, RequestDeckShareButtonOver);
				m_DeckShareRequestButton.AddEventListener(UIEventType.ROLLOUT, RequestDeckShareButtonOut);
			}
			else
			{
				m_DeckShareRequestButton.gameObject.SetActive(value: false);
			}
		}
		if (m_DeckShareGlowOutQuad != null)
		{
			m_DeckShareGlowOutQuad.SetActive(value: false);
		}
		m_xpBar = UnityEngine.Object.Instantiate(m_xpBarPrefab);
		NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		m_xpBar.m_soloLevelLimit = guardianVars?.XPSoloLimit ?? 60;
		LoanerDeckDisplay.LoanerDeckDisplayOpened = (Action)Delegate.Combine(LoanerDeckDisplay.LoanerDeckDisplayOpened, new Action(OnLoanerDeckDisplayOpened));
		IsModeSwitchShowing = false;
	}

	private void Start()
	{
		Navigation.PushIfNotOnTop(OnNavigateBack);
		GameObject go = m_leftArrowNestedPrefab.PrefabGameObject();
		m_leftArrow = go.GetComponent<UIBButton>();
		m_leftArrow.AddEventListener(UIEventType.RELEASE, OnShowPreviousPage);
		go = m_rightArrowNestedPrefab.PrefabGameObject();
		m_rightArrow = go.GetComponent<UIBButton>();
		m_rightArrow.AddEventListener(UIEventType.RELEASE, OnShowNextPage);
		UpdatePageArrows();
		m_currentMedalInfo = RankMgr.Get().GetLocalPlayerMedalInfo().GetCurrentMedalForCurrentFormatType();
		m_formatTypePickerWidget = WidgetInstance.Create(FORMAT_TYPE_PICKER_POPUP_PREFAB);
		m_formatTypePickerWidget.Hide();
		m_formatTypePickerWidget.RegisterReadyListener(delegate
		{
			OnFormatTypePickerPopupReady();
		});
		LegendarySkin_DynamicResolutionEnabled = LegendarySkin.DynamicResolutionEnabled;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		HideDemoQuotes();
		if (TournamentDisplay.Get() != null)
		{
			TournamentDisplay.Get().RemoveMedalChangedListener(OnMedalChanged);
		}
		if (FriendChallengeMgr.Get() != null && Get() != null)
		{
			FriendChallengeMgr.Get().RemoveChangedListener(Get().OnFriendChallengeChanged);
		}
		if (Box.Get() != null)
		{
			Box.Get().RemoveTransitionFinishedListener(OnBoxTransitionFinished);
		}
		if (SceneMgr.Get() != null && SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.FRIENDLY && SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY)
		{
			FriendChallengeMgr.Get().CancelChallenge();
		}
		LegendarySkin.DynamicResolutionEnabled = LegendarySkin_DynamicResolutionEnabled;
		LoanerDeckDisplay.LoanerDeckDisplayOpened = (Action)Delegate.Remove(LoanerDeckDisplay.LoanerDeckDisplayOpened, new Action(OnLoanerDeckDisplayOpened));
		s_instance = null;
	}

	public static DeckPickerTrayDisplay Get()
	{
		return s_instance;
	}

	public void SetInHeroPicker()
	{
		m_inHeroPicker = true;
	}

	public void OverridePlayButtonCallback(UIEvent.Handler callback)
	{
		if (m_playButton != null)
		{
			m_playButton.RemoveEventListener(UIEventType.RELEASE, OnPlayGameButtonReleased);
			m_playButton.AddEventListener(UIEventType.RELEASE, callback);
		}
	}

	private void OnShowNextPage(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay("hero_panel_slide_off.prefab:ed410a050e783564384ca51e701ede4d");
		ShowNextPage();
	}

	public override void ResetCurrentMode()
	{
		if (m_selectedCustomDeckBox != null)
		{
			SetPlayButtonEnabled(enable: true);
			SetHeroRaised(raised: true);
		}
		else if (m_selectedHeroButton != null)
		{
			SetHeroRaised(raised: true);
			SetPlayButtonEnabled(!m_selectedHeroButton.IsLocked());
		}
		SetHeroButtonsEnabled(enable: true);
	}

	public int GetSelectedHeroLevel()
	{
		if (m_selectedHeroButton == null)
		{
			return 0;
		}
		return GameUtils.GetHeroLevel(m_selectedHeroButton.GetEntityDef().GetClass()).CurrentLevel.Level;
	}

	public void ToggleRankedDetailsTray(bool shown)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_rankedDetailsTray.ToggleTraySlider(shown);
		}
	}

	public void ToggleApprenticeProgressTray(bool shown)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			bool isLadder = SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT;
			m_apprenticeProgressTrayMesh.SetActive(isLadder);
			bool shouldShow = shown && IsInApprentice;
			m_apprenticeProgressTray.ToggleTraySlider(shouldShow);
			m_apprenticeProgressWidget.RegisterEventListener(HandleApprenticeProgressWidgetEvent);
			m_apprenticeRewardPopupWidget.RegisterEventListener(HandleApprenticeProgressWidgetEvent);
		}
	}

	public override long GetSelectedDeckID()
	{
		if (null != m_selectedCustomDeckBox)
		{
			return m_selectedCustomDeckBox.GetDeckID();
		}
		return base.GetSelectedDeckID();
	}

	public int GetSelectedDeckTemplateID()
	{
		if (m_selectedCustomDeckBox != null)
		{
			return m_selectedCustomDeckBox.GetDeckTemplateId();
		}
		return 0;
	}

	public CollectionDeck GetSelectedCollectionDeck()
	{
		if (!(m_selectedCustomDeckBox == null))
		{
			return m_selectedCustomDeckBox.GetCollectionDeck();
		}
		return null;
	}

	public void UpdateCreateDeckText()
	{
		string glueHeader;
		if (SceneMgr.Get().IsInTavernBrawlMode())
		{
			glueHeader = ((TavernBrawlManager.Get().CurrentSeasonBrawlMode == TavernBrawlMode.TB_MODE_HEROIC) ? "GLOBAL_HEROIC_BRAWL" : "GLOBAL_TAVERN_BRAWL");
		}
		else
		{
			PegasusShared.FormatType currentFormatType = Options.GetFormatType();
			switch (currentFormatType)
			{
			case PegasusShared.FormatType.FT_WILD:
				glueHeader = "GLUE_CREATE_WILD_DECK";
				break;
			case PegasusShared.FormatType.FT_STANDARD:
				glueHeader = "GLUE_CREATE_STANDARD_DECK";
				break;
			case PegasusShared.FormatType.FT_CLASSIC:
				glueHeader = "GLUE_CREATE_CLASSIC_DECK";
				break;
			case PegasusShared.FormatType.FT_TWIST:
				glueHeader = "GLUE_CREATE_TWIST_DECK";
				break;
			default:
				Debug.LogError("DeckPickerTrayDisplay.UpdateCreateDeckText called in unsupported format type: " + currentFormatType);
				SetHeaderText("UNSUPPORTED DECK TEXT " + currentFormatType);
				return;
			}
		}
		SetHeaderText(GameStrings.Get(glueHeader));
	}

	public bool UpdateRankedClassWinsPlate()
	{
		VisualsFormatType currentVisualsFormatType = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType();
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT && m_heroActor != null && m_heroActor.GetEntityDef() != null && Options.GetInRankedPlayMode() && m_formatConfig[currentVisualsFormatType].ShowClassWins)
		{
			if (!GameUtils.HERO_SKIN_ACHIEVEMENTS.TryGetValue(m_heroActor.GetEntityDef().GetClass(), out var heroSkinAchievements))
			{
				m_rankedWinsPlate.SetActive(value: false);
				return false;
			}
			RankedWinsPlate rankedWinsPlate = m_rankedWinsPlate.GetComponent<RankedWinsPlate>();
			rankedWinsPlate.TooltipString = GameStrings.Get("GLUE_TOOLTIP_GOLDEN_WINS_DESC");
			AchievementDataModel golden500WinAchievement = AchievementManager.Get().GetAchievementDataModel(heroSkinAchievements.Golden500Win);
			AchievementDataModel honored1KWinAchievement = AchievementManager.Get().GetAchievementDataModel(heroSkinAchievements.Honored1kWin);
			int numWins = golden500WinAchievement?.Progress ?? 0;
			int maxWins = golden500WinAchievement?.Quota ?? 0;
			if (golden500WinAchievement != null && AchievementManager.Get().IsAchievementComplete(golden500WinAchievement.ID))
			{
				numWins = honored1KWinAchievement?.Progress ?? numWins;
				maxWins = honored1KWinAchievement?.Quota ?? maxWins;
				rankedWinsPlate.TooltipString = GameStrings.Format("GLUE_TOOLTIP_ALTERNATE_WINS_DESC", maxWins);
			}
			if (numWins == 0)
			{
				m_rankedWinsPlate.SetActive(value: false);
				return false;
			}
			if (numWins >= maxWins)
			{
				m_rankedWins.Text = GameStrings.Format(UniversalInputManager.UsePhoneUI ? "GLOBAL_HERO_WINS_PAST_MAX_PHONE" : "GLOBAL_HERO_WINS_PAST_MAX", numWins);
				rankedWinsPlate.TooltipEnabled = false;
			}
			else
			{
				m_rankedWins.Text = GameStrings.Format(UniversalInputManager.UsePhoneUI ? "GLOBAL_HERO_WINS_PHONE" : "GLOBAL_HERO_WINS", numWins, maxWins);
				rankedWinsPlate.TooltipEnabled = true;
			}
			m_rankedWinsPlate.SetActive(value: true);
			return true;
		}
		m_rankedWinsPlate.SetActive(value: false);
		return false;
	}

	public override void OnServerGameStarted()
	{
		base.OnServerGameStarted();
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.ADVENTURE)
		{
			AdventureConfig ac = AdventureConfig.Get();
			if (ac.CurrentSubScene == AdventureData.Adventuresubscene.MISSION_DECK_PICKER && DemoMgr.Get().GetMode() != DemoMode.BLIZZCON_2015)
			{
				ac.SubSceneGoBack(fireevent: false);
			}
		}
	}

	public override void HandleGameStartupFailure()
	{
		base.HandleGameStartupFailure();
		switch (SceneMgr.Get().GetMode())
		{
		case SceneMgr.Mode.ADVENTURE:
			if (AdventureConfig.Get().CurrentSubScene == AdventureData.Adventuresubscene.PRACTICE)
			{
				PracticePickerTrayDisplay.Get().OnGameDenied();
			}
			break;
		case SceneMgr.Mode.TOURNAMENT:
			if (PresenceMgr.Get().CurrentStatus == Global.PresenceStatus.PLAY_QUEUE)
			{
				PresenceMgr.Get().SetPrevStatus();
			}
			break;
		}
	}

	public void SetHeroDetailsTrayToIgnoreFullScreenEffects(bool ignoreEffects)
	{
		if (!(m_hierarchyDetails == null))
		{
			if (ignoreEffects)
			{
				LayerUtils.ReplaceLayer(m_hierarchyDetails, GameLayer.IgnoreFullScreenEffects, m_defaultDetailsLayer);
			}
			else
			{
				LayerUtils.ReplaceLayer(m_hierarchyDetails, m_defaultDetailsLayer, GameLayer.IgnoreFullScreenEffects);
			}
		}
	}

	public void ShowClickedTwistDeckInWildPopup()
	{
		ShowClickedTwistDeckInStandardPopup();
	}

	public void ShowClickedTwistDeckInStandardPopup()
	{
		if ((SceneMgr.Get().GetMode() != SceneMgr.Mode.TOURNAMENT && SceneMgr.Get().GetMode() != SceneMgr.Mode.FRIENDLY) || m_switchFormatPopup != null || m_innkeeperQuote != null)
		{
			return;
		}
		if (!m_switchFormatButton.IsCovered())
		{
			StopCoroutine(ShowNewTwistModePopup());
			Action<int> notificationCallback = delegate
			{
				m_switchFormatPopup = null;
			};
			m_switchFormatPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.SET_ROTATION_INTRO, m_Switch_Format_Notification_Bone.position, m_Switch_Format_Notification_Bone.localScale, GameStrings.Get("GLUE_TOURNAMENT_SWITCH_TO_TWIST"));
			if (m_switchFormatPopup != null)
			{
				Notification.PopUpArrowDirection arrowDirection = (UniversalInputManager.UsePhoneUI ? Notification.PopUpArrowDirection.RightUp : Notification.PopUpArrowDirection.Up);
				m_switchFormatPopup.ShowPopUpArrow(arrowDirection);
				Notification switchFormatPopup = m_switchFormatPopup;
				switchFormatPopup.OnFinishDeathState = (Action<int>)Delegate.Combine(switchFormatPopup.OnFinishDeathState, notificationCallback);
			}
		}
		Action<int> innkeeperCallback = delegate
		{
			if (m_switchFormatButton != null)
			{
				NotificationManager.Get().DestroyNotification(m_switchFormatPopup, 0f);
			}
			m_innkeeperQuote = null;
		};
		m_innkeeperQuote = NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.SET_ROTATION_INTRO, INNKEEPER_QUOTE_POS, GameStrings.Get("VO_INNKEEPER_TWIST_DECK_WARNING"), "", 0f, innkeeperCallback);
	}

	public void ShowClickedStandardDeckInTwistPopup()
	{
		if ((SceneMgr.Get().GetMode() != SceneMgr.Mode.TOURNAMENT && SceneMgr.Get().GetMode() != SceneMgr.Mode.FRIENDLY) || !(m_switchFormatPopup == null) || !(m_innkeeperQuote == null))
		{
			return;
		}
		if (!m_switchFormatButton.IsCovered())
		{
			Action<int> notificationCallback = delegate
			{
				m_switchFormatPopup = null;
			};
			m_switchFormatPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.SET_ROTATION_INTRO, m_Switch_Format_Notification_Bone.position, m_Switch_Format_Notification_Bone.localScale, GameStrings.Get("GLUE_TOURNAMENT_SWITCH_TO_STANDARD"));
			if (m_switchFormatPopup != null)
			{
				Notification.PopUpArrowDirection arrowDirection = (UniversalInputManager.UsePhoneUI ? Notification.PopUpArrowDirection.RightUp : Notification.PopUpArrowDirection.Up);
				m_switchFormatPopup.ShowPopUpArrow(arrowDirection);
				Notification switchFormatPopup = m_switchFormatPopup;
				switchFormatPopup.OnFinishDeathState = (Action<int>)Delegate.Combine(switchFormatPopup.OnFinishDeathState, notificationCallback);
			}
		}
		Action<int> innkeeperCallback = delegate
		{
			if (m_switchFormatButton != null)
			{
				NotificationManager.Get().DestroyNotification(m_switchFormatPopup, 0f);
			}
			m_innkeeperQuote = null;
		};
		m_innkeeperQuote = NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.SET_ROTATION_INTRO, INNKEEPER_QUOTE_POS, GameStrings.Get("VO_INNKEEPER_STANDARD_DECK_WARNING"), "", 0f, innkeeperCallback);
	}

	public void ShowClickedWildDeckInTwistPopup()
	{
		ShowClickedWildDeckInStandardPopup();
	}

	public void ShowClickedWildDeckInStandardPopup()
	{
		if ((SceneMgr.Get().GetMode() != SceneMgr.Mode.TOURNAMENT && SceneMgr.Get().GetMode() != SceneMgr.Mode.FRIENDLY) || !(m_switchFormatPopup == null) || !(m_innkeeperQuote == null))
		{
			return;
		}
		if (!m_switchFormatButton.IsCovered())
		{
			StopCoroutine("ShowSwitchToWildTutorialAfterTransitionsComplete");
			Action<int> notificationCallback = delegate
			{
				m_switchFormatPopup = null;
			};
			m_switchFormatPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.SET_ROTATION_INTRO, m_Switch_Format_Notification_Bone.position, m_Switch_Format_Notification_Bone.localScale, GameStrings.Get("GLUE_TOURNAMENT_SWITCH_TO_WILD"));
			if (m_switchFormatPopup != null)
			{
				Notification.PopUpArrowDirection arrowDirection = (UniversalInputManager.UsePhoneUI ? Notification.PopUpArrowDirection.RightUp : Notification.PopUpArrowDirection.Up);
				m_switchFormatPopup.ShowPopUpArrow(arrowDirection);
				Notification switchFormatPopup = m_switchFormatPopup;
				switchFormatPopup.OnFinishDeathState = (Action<int>)Delegate.Combine(switchFormatPopup.OnFinishDeathState, notificationCallback);
			}
		}
		Action<int> innkeeperCallback = delegate
		{
			if (m_switchFormatButton != null)
			{
				NotificationManager.Get().DestroyNotification(m_switchFormatPopup, 0f);
			}
			m_innkeeperQuote = null;
		};
		m_innkeeperQuote = NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.SET_ROTATION_INTRO, INNKEEPER_QUOTE_POS, GameStrings.Get("VO_INNKEEPER_WILD_DECK_WARNING"), "VO_INNKEEPER_Male_Dwarf_SetRotation_32.prefab:3377790e79f276a4484ed43edde342c4", 0f, innkeeperCallback);
	}

	public void ShowSwitchToWildTutorialIfNecessary()
	{
		if (!(m_switchFormatPopup != null) && !CollectionManager.Get().ShouldSeeTwistModeNotification() && UserAttentionManager.CanShowAttentionGrabber(UserAttentionBlocker.SET_ROTATION_INTRO, "DeckPickerTrayDisplay.ShowSwitchToWildTutorialIfNecessary"))
		{
			if (Options.GetFormatType() == PegasusShared.FormatType.FT_WILD)
			{
				Options.Get().SetBool(Option.SHOW_SWITCH_TO_WILD_ON_CREATE_DECK, val: false);
				Options.Get().SetBool(Option.SHOW_SWITCH_TO_WILD_ON_PLAY_SCREEN, val: false);
			}
			bool showTutorial = false;
			SceneMgr.Mode mode = SceneMgr.Get().GetMode();
			if (Options.Get().GetBool(Option.SHOW_SWITCH_TO_WILD_ON_CREATE_DECK) && mode == SceneMgr.Mode.COLLECTIONMANAGER)
			{
				showTutorial = true;
				Options.Get().SetBool(Option.SHOW_SWITCH_TO_WILD_ON_CREATE_DECK, val: false);
			}
			if (Options.Get().GetBool(Option.SHOW_SWITCH_TO_WILD_ON_PLAY_SCREEN) && mode == SceneMgr.Mode.TOURNAMENT)
			{
				showTutorial = true;
				Options.Get().SetBool(Option.SHOW_SWITCH_TO_WILD_ON_PLAY_SCREEN, val: false);
			}
			if (showTutorial)
			{
				StartCoroutine("ShowSwitchToWildTutorialAfterTransitionsComplete");
			}
		}
	}

	public void ShowNewTwistModePopupIfNecessary(float delaySeconds = 0f)
	{
		if (!(m_switchFormatPopup != null) && !m_switchFormatButton.IsCovered() && CollectionManager.Get().ShouldSeeTwistModeNotification() && !TwistDetailsDisplayManager.TwistSeasonInfoModel.ShouldShowTwistLoginPopup)
		{
			StartCoroutine(ShowNewTwistModePopup(delaySeconds));
		}
	}

	private IEnumerator ShowNewTwistModePopup(float delaySeconds = 0f)
	{
		m_switchFormatButton.EnableHighlight(enabled: true);
		yield return new WaitForSeconds(delaySeconds);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_switchFormatPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.SET_ROTATION_INTRO, m_Switch_Format_Notification_Bone_Mobile, GameStrings.Get("GLUE_NEW_TWIST_MODE_HINT"));
		}
		else
		{
			m_switchFormatPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.SET_ROTATION_INTRO, m_Switch_Format_Notification_Bone, GameStrings.Get("GLUE_NEW_TWIST_MODE_HINT"));
		}
		Notification.PopUpArrowDirection arrowDirection = (UniversalInputManager.UsePhoneUI ? Notification.PopUpArrowDirection.RightUp : Notification.PopUpArrowDirection.Up);
		m_switchFormatPopup.ShowPopUpArrow(arrowDirection);
		m_switchFormatPopup.PulseReminderEveryXSeconds(3f);
	}

	private void DismissSwitchFormatPopup()
	{
		if ((bool)m_switchFormatPopup)
		{
			NotificationManager.Get().DestroyNotification(m_switchFormatPopup, 0f);
			m_switchFormatButton.EnableHighlight(enabled: false);
			if (CollectionManager.Get().ShouldSeeTwistModeNotification())
			{
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_TWIST_MODE_NOTIFICATION, 1L));
			}
		}
	}

	private bool IsShowingMobileTwistHeroicDecks()
	{
		bool isCurrentSceneTournament = SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT;
		bool isCurrentVisualFormatTwist = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType() == VisualsFormatType.VFT_TWIST;
		if (RankMgr.IsCurrentTwistSeasonUsingHeroicDecks() && (bool)UniversalInputManager.UsePhoneUI && isCurrentVisualFormatTwist && isCurrentSceneTournament)
		{
			return true;
		}
		return false;
	}

	private IEnumerator ShowSwitchToWildTutorialAfterTransitionsComplete()
	{
		yield return new WaitForSeconds(1f);
		m_switchFormatPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.SET_ROTATION_INTRO, m_Switch_Format_Notification_Bone, GameStrings.Get("GLUE_TOURNAMENT_SWITCH_TO_WILD"));
		Notification.PopUpArrowDirection arrowDirection = (UniversalInputManager.UsePhoneUI ? Notification.PopUpArrowDirection.RightUp : Notification.PopUpArrowDirection.Up);
		m_switchFormatPopup.ShowPopUpArrow(arrowDirection);
		m_switchFormatPopup.PulseReminderEveryXSeconds(3f);
		NotificationManager.Get().DestroyNotification(m_switchFormatPopup, 6f);
	}

	public void SkipHeroSelectionAndCloseTray()
	{
		if (m_playButton != null)
		{
			m_backButton.RemoveEventListener(UIEventType.RELEASE, OnBackButtonReleased);
			m_playButton.RemoveEventListener(UIEventType.RELEASE, OnPlayGameButtonReleased);
		}
		SetPlayButtonEnabled(enable: false);
		Navigation.RemoveHandler(OnNavigateBack);
		if (m_slidingTray != null)
		{
			m_slidingTray.ToggleTraySlider(show: false);
		}
		if (HeroPickerDisplay.Get() != null)
		{
			HeroPickerDisplay.Get().HideTray(UniversalInputManager.UsePhoneUI ? 0.25f : 0f);
		}
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null && !cmd.GetHeroPickerDisplay().IsShown())
		{
			CollectionManager.Get().GetCollectibleDisplay().EnableInput(enable: false);
		}
		CollectionDeckTray.Get().RegisterModeSwitchedListener(OnModeSwitchedAfterSkippingHeroSelection);
	}

	public void ShowRankedIntroPopup()
	{
		OnPopupShown();
		DialogManager.Get().ShowRankedIntroPopUp(ShouldShowBonusStarsPopUp() ? new Action(ShowBonusStarsPopup) : new Action(PlayEnterModeDialogues));
	}

	public void ShowBonusStarsPopup()
	{
		OnPopupShown();
		DialogManager.Get().ShowBonusStarsPopup(GetBonusStarsPopupDataModel(), PlayEnterModeDialogues);
	}

	private bool ShouldShowBonusStarsPopUp()
	{
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.LOGIN && (SceneMgr.Get().GetMode() != SceneMgr.Mode.TOURNAMENT || SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY))
		{
			return false;
		}
		if (IsInApprentice)
		{
			return false;
		}
		if (m_currentMedalInfo.starsPerWin < 2)
		{
			return false;
		}
		int currentRankedSeasonId = m_currentMedalInfo.seasonId;
		int bonusStarsPopUpSeenCountReq = m_currentMedalInfo.LeagueConfig.RankedIntroSeenRequirement;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_LAST_SEASON_BONUS_STARS_POPUP_SEEN, out m_lastSeasonBonusStarPopUpSeen);
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_BONUS_STARS_POPUP_SEEN_COUNT, out m_bonusStarsPopUpSeenCount);
		if (m_lastSeasonBonusStarPopUpSeen < currentRankedSeasonId && m_bonusStarsPopUpSeenCount < bonusStarsPopUpSeenCountReq)
		{
			return true;
		}
		return false;
	}

	private void CheckIfShouldShowFirstClassSelectionDialog(CollectionDeckBoxVisual selectedDeckBox)
	{
		if (!IsInApprentice || selectedDeckBox == null)
		{
			return;
		}
		long classesSeenFlags = 0L;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_CLASS_FIRST_SELECTION_QUOTE, out classesSeenFlags);
		TAG_CLASS classTag = selectedDeckBox.GetCollectionDeck().GetClass();
		int classBit = 1 << (int)(classTag - 1);
		if ((classesSeenFlags & classBit) > 0)
		{
			return;
		}
		ClassDbfRecord classRecord = GameDbf.Class.GetRecord((int)classTag);
		if (classRecord != null)
		{
			string quote = classRecord.ClassFirstSelectionQuote.GetString();
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, INNKEEPER_QUOTE_POS, quote, "");
			classesSeenFlags |= classBit;
			if (!GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_CLASS_FIRST_SELECTION_QUOTE, classesSeenFlags)))
			{
				Log.All.PrintError("Unable to update seen class first selection value {0}", classesSeenFlags);
			}
		}
	}

	private void OnModeSwitchedAfterSkippingHeroSelection()
	{
		CollectionDeckTray.Get().UnregisterModeSwitchedListener(OnModeSwitchedAfterSkippingHeroSelection);
		CollectionManager.Get().GetCollectibleDisplay().EnableInput(enable: true);
	}

	protected bool ValidTwistDecksExist()
	{
		int numTwistDecks = CollectionManager.Get().GetNumberOfTwistDecks();
		if (!RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
		{
			return numTwistDecks > 0;
		}
		return true;
	}

	protected override IEnumerator InitDeckDependentElements()
	{
		Log.PlayModeInvestigation.PrintInfo("DeckPickerTrayDisplay.InitDeckDependentElements() called");
		bool isChoosingHero = IsChoosingHero();
		DeckPickerMode defaultDeckPickerMode = (m_deckPickerMode = DeckPickerMode.CUSTOM);
		m_numPagesToShow = 1;
		m_basicDeckPageContainer.gameObject.SetActive(isChoosingHero);
		if (!isChoosingHero)
		{
			while (!NetCache.Get().IsNetObjectAvailable<NetCache.NetCacheDecks>())
			{
				yield return null;
			}
			CollectionManager.Get().RequestDeckContentsForDecksWithoutContentsLoaded();
			while (!CollectionManager.Get().AreAllDeckContentsReady())
			{
				yield return null;
			}
			m_usingSharedDecks = FriendChallengeMgr.Get().ShouldUseSharedDecks();
			m_deckPickerMode = (m_usingSharedDecks ? DeckPickerMode.CUSTOM : defaultDeckPickerMode);
			UpdateDeckShareRequestButton();
			List<CollectionDeck> decks = CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK);
			if (FriendChallengeMgr.Get().IsChallengeFriendlyDuel)
			{
				decks = ((!m_usingSharedDecks) ? decks.FindAll((CollectionDeck deck) => deck.IsValidForFormat(FriendChallengeMgr.Get().GetFormatType())) : FriendChallengeMgr.Get().GetSharedDecks());
			}
			SetupDeckPages(decks);
		}
		if (m_rankedPlayDisplay != null)
		{
			VisualsFormatType currentVisualsFormatType = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType();
			UpdateRankedPlayDisplay(currentVisualsFormatType);
		}
		InitSwitchFormatButton();
		yield return StartCoroutine(base.InitDeckDependentElements());
	}

	private void SetupDeckPages(List<CollectionDeck> decks)
	{
		m_numPagesToShow = GetDeckPageCount(decks.Count);
		Log.PlayModeInvestigation.PrintInfo($"DeckPickerTrayDisplay.SetupDeckPages() called. m_numPagesToShow={m_numPagesToShow}, decks.Count={decks.Count}");
		InitDeckPages();
		LoanerDeckDisplay loanerDeckDisplay = LoanerDeckDisplay.Get();
		FreeDeckMgr freeDeckManager = FreeDeckMgr.Get();
		if (freeDeckManager != null && loanerDeckDisplay != null && freeDeckManager.Status == FreeDeckMgr.FreeDeckStatus.TRIAL_PERIOD && loanerDeckDisplay.ShouldLoanerDecksBeDisplayed())
		{
			int numPagesLoanerDecks = Mathf.CeilToInt((float)freeDeckManager.GetLoanerDecksCount() / 6f);
			numPagesLoanerDecks = Mathf.Max(numPagesLoanerDecks, 1);
			m_numPagesToShow += numPagesLoanerDecks;
			for (int i = 0; i < numPagesLoanerDecks; i++)
			{
				CreateCustomDeckPage(CustomDeckPage.PageType.LOANER_DECK_DISPLAY);
			}
		}
		RankedPlaySeason rankedPlaySeason = RankMgr.Get()?.GetCurrentTwistSeason();
		if (rankedPlaySeason != null && rankedPlaySeason.UsesPrebuiltDecks)
		{
			int numPagesPreconDecks = GetDeckPageCount(rankedPlaySeason.GetDeckCount(), CustomDeckPage.DeckPageDisplayFormat.EMPTY_FIRST_ROW);
			m_numPagesToShow += numPagesPreconDecks;
			for (int j = 0; j < numPagesPreconDecks; j++)
			{
				CreateCustomDeckPage(CustomDeckPage.PageType.PRECON_DECK_DISPLAY);
			}
		}
		Log.PlayModeInvestigation.PrintInfo("DeckPickerTrayDisplay.InitDeckPages() -- added page for Loaner decks");
		CreateCustomDeckPage(CustomDeckPage.PageType.RESERVE);
		SetupPageDecksBasedOnFormat(VisualsFormatTypeExtensions.GetCurrentVisualsFormatType(), decks);
		UpdateDeckVisuals();
	}

	private int GetDeckPageCount(int numberOfDecks, CustomDeckPage.DeckPageDisplayFormat firstdeckFormat = CustomDeckPage.DeckPageDisplayFormat.FILL_ALL_DECKSLOTS)
	{
		int remainingDecks = numberOfDecks;
		int pagesToShow = 0;
		if (firstdeckFormat == CustomDeckPage.DeckPageDisplayFormat.EMPTY_FIRST_ROW)
		{
			pagesToShow++;
			remainingDecks -= 6;
		}
		if (remainingDecks > 0)
		{
			pagesToShow += Mathf.CeilToInt((float)remainingDecks / 9f);
		}
		return Mathf.Max(pagesToShow, 1);
	}

	private void UpdateDeckVisuals()
	{
		for (int i = 0; i < m_customPages.Count; i++)
		{
			m_customPages[i].UpdateDeckVisuals();
		}
	}

	protected override void InitForMode(SceneMgr.Mode mode)
	{
		SetMissingTwistDeckActive(active: false);
		switch (mode)
		{
		case SceneMgr.Mode.TOURNAMENT:
			m_rankedPlayDisplayWidget = WidgetInstance.Create(UniversalInputManager.UsePhoneUI ? "RankedPlayDisplay_phone.prefab:22b0793a4bc044e47a1948619c2aa896" : "RankedPlayDisplay.prefab:1f884a817dbbdd84b9f8713dc21759f1");
			m_rankedPlayDisplayWidget.RegisterReadyListener(delegate
			{
				OnRankedPlayDisplayWidgetReady();
			});
			UpdateApprenticeProgressInfo();
			SetPlayButtonText(GameStrings.Get("GLOBAL_PLAY"));
			ChangePlayButtonTextAlpha();
			UpdateRankedClassWinsPlate();
			UpdatePageArrows();
			if (Options.GetFormatType() == PegasusShared.FormatType.FT_TWIST && !ValidTwistDecksExist())
			{
				SetMissingTwistDeckActive(active: true);
			}
			break;
		case SceneMgr.Mode.TAVERN_BRAWL:
			SetHeaderForTavernBrawl();
			break;
		}
		UnityEngine.Vector2 modeOffset = new UnityEngine.Vector2(0f, 0f);
		m_currentModeTextures = m_collectionTextures;
		switch (mode)
		{
		case SceneMgr.Mode.ADVENTURE:
			m_currentModeTextures = m_adventureTextures;
			modeOffset.x = 0.5f;
			break;
		case SceneMgr.Mode.COLLECTIONMANAGER:
			m_currentModeTextures = m_collectionTextures;
			break;
		case SceneMgr.Mode.TAVERN_BRAWL:
			m_currentModeTextures = m_tavernBrawlTextures;
			modeOffset.x = 0.5f;
			modeOffset.y = 0.61f;
			break;
		case SceneMgr.Mode.TOURNAMENT:
			m_currentModeTextures = m_tournamentTextures;
			break;
		case SceneMgr.Mode.FRIENDLY:
			m_currentModeTextures = m_friendlyTextures;
			modeOffset.y = 0.61f;
			break;
		}
		VisualsFormatType currentVisualsFormatType = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType();
		Texture formatTexture = m_currentModeTextures.GetTextureForFormat(currentVisualsFormatType);
		Texture customFormatTexture = m_currentModeTextures.GetCustomTextureForFormat(currentVisualsFormatType);
		bool isCasual = mode == SceneMgr.Mode.TOURNAMENT && currentVisualsFormatType == VisualsFormatType.VFT_CASUAL;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			ToggleApprenticeProgressTray(isCasual);
		}
		else
		{
			ToggleApprenticeProgressUI(isCasual);
		}
		InitClassPreviewUI(currentVisualsFormatType, mode);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (SceneMgr.Mode.TOURNAMENT != mode)
			{
				m_detailsTrayFrame.GetComponent<MeshFilter>().mesh = m_alternateDetailsTrayMesh;
			}
			SetPhoneDetailsTrayTextures(formatTexture, formatTexture);
		}
		else
		{
			SetTrayFrameAndBasicDeckPageTextures(formatTexture, formatTexture);
		}
		SetCustomDeckPageTextures(customFormatTexture, customFormatTexture);
		SetKeyholeTextureOffsets(modeOffset);
		UpdateDeckVisuals();
		base.InitForMode(mode);
	}

	private void ToggleApprenticeProgressUI(bool shouldEnable)
	{
		if (!(m_apprenticeProgressContainer == null) && !UniversalInputManager.UsePhoneUI)
		{
			if (shouldEnable && IsInApprentice)
			{
				m_apprenticeProgressContainer.SetActive(value: true);
				m_apprenticeProgressWidget.RegisterEventListener(HandleApprenticeProgressWidgetEvent);
				m_apprenticeRewardPopupWidget.RegisterEventListener(HandleApprenticeProgressWidgetEvent);
			}
			else
			{
				m_apprenticeProgressContainer.SetActive(value: false);
				m_apprenticeProgressWidget.RemoveEventListener(HandleApprenticeProgressWidgetEvent);
				m_apprenticeRewardPopupWidget.RemoveEventListener(HandleApprenticeProgressWidgetEvent);
			}
		}
	}

	private void HandleApprenticeProgressWidgetEvent(string eventName)
	{
		if (!IsInApprentice)
		{
			return;
		}
		switch (eventName)
		{
		case "SHOW_XP_BAR_TOOLTIP":
			ShowXPBarTooltip();
			break;
		case "HIDE_XP_BAR_TOOLTIP":
			HideXPBarTooltip();
			break;
		case "CHEST_BUTTON_CLICKED":
			if (m_rewardTrack != null && m_rewardTrack.IsValid)
			{
				m_apprenticeRewardPopupWidget.TriggerEvent("SHOW_POPUP");
			}
			else
			{
				Log.All.PrintError("Unable to show Apprentice rewards since data is missing");
			}
			break;
		case "GO_TO_JOURNAL_CLICKED":
		{
			BoxRailroadManager railroadManager = Box.Get().GetRailroadManager();
			if (railroadManager != null)
			{
				railroadManager.ShowApprenticeTrack();
				BackOutToHub();
			}
			break;
		}
		}
	}

	private void InitClassPreviewUI(VisualsFormatType formatType, SceneMgr.Mode mode)
	{
		if (m_apprenticeHeroPortraitButton == null || m_apprenticeHeroPreviewPopupWidget == null)
		{
			Log.DeckTray.PrintError("Hero preview content missing!");
			return;
		}
		string layoutEvent = "CODE_LAYOUT_STANDARD";
		switch (mode)
		{
		case SceneMgr.Mode.TOURNAMENT:
			switch (formatType)
			{
			case VisualsFormatType.VFT_STANDARD:
				layoutEvent = "CODE_LAYOUT_STANDARD";
				break;
			case VisualsFormatType.VFT_CASUAL:
				layoutEvent = "CODE_LAYOUT_CASUAL";
				break;
			case VisualsFormatType.VFT_WILD:
				layoutEvent = "CODE_LAYOUT_WILD";
				break;
			case VisualsFormatType.VFT_TWIST:
				layoutEvent = "CODE_LAYOUT_TWIST";
				break;
			}
			break;
		case SceneMgr.Mode.ADVENTURE:
			layoutEvent = "CODE_LAYOUT_PRACTICE";
			break;
		case SceneMgr.Mode.FRIENDLY:
			layoutEvent = "CODE_LAYOUT_FRIENDLY";
			break;
		}
		m_apprenticeHeroPortraitButton.TriggerEvent(layoutEvent);
		m_apprenticeHeroPortraitButton.RegisterEventListener(HandleHeroPortraitButtonEvents);
		m_apprenticeHeroPreviewPopupWidget.RegisterEventListener(HandleHeroPreviewPopupEvents);
	}

	private void HandleHeroPortraitButtonEvents(string evt)
	{
		if (m_selectedCustomDeckBox == null || !(evt == "CODE_HERO_PORTRAIT_CLICKED"))
		{
			return;
		}
		CollectionDeck colDeck = m_selectedCustomDeckBox.GetCollectionDeck();
		TAG_CLASS tagClass = colDeck.GetClass();
		EntityDef deckHeroDef = DefLoader.Get().GetEntityDef(colDeck.GetDisplayHeroCardID(rerollFavoriteHero: false));
		if (deckHeroDef != null && !deckHeroDef.IsLettuceMercenary())
		{
			tagClass = deckHeroDef.GetClass();
		}
		if (tagClass != TAG_CLASS.WHIZBANG)
		{
			if (!OverlayUI.Get().HasObject(m_apprenticeHeroPreviewPopupWidget.gameObject))
			{
				OverlayUI.Get().AddGameObject(m_apprenticeHeroPreviewPopupWidget.gameObject);
			}
			m_apprenticeHeroPreviewPopupWidget.gameObject.SetActive(value: true);
			ClassPreviewDataModel classPreviewDM = ClassUnlockPopup.BuildClassPreviewDataModel(tagClass, !IsInApprentice);
			m_apprenticeHeroPreviewPopupWidget.BindDataModel(classPreviewDM);
			m_apprenticeHeroPreviewPopupWidget.TriggerEvent("OPEN");
			UIContext.GetRoot().ShowPopup(m_apprenticeHeroPreviewPopupWidget.gameObject);
		}
	}

	private void HandleHeroPreviewPopupEvents(string evt)
	{
		if (evt == "HIDE_POPUP")
		{
			UIContext.GetRoot().DismissPopup(m_apprenticeHeroPreviewPopupWidget.gameObject);
			m_apprenticeHeroPreviewPopupWidget.gameObject.SetActive(value: false);
		}
	}

	private void ShowXPBarTooltip()
	{
		if (m_rewardTrackDataModel != null && !(m_apprenticeXpBarTooltipZone == null))
		{
			string bodyText = string.Format("{0}\n\n{1}", m_rewardTrackDataModel.XpProgress, GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_BAR_TOOLTIP_DESCRIPTION"));
			m_apprenticeXpBarTooltipZone.ShowTooltip("GLUE_PROGRESSION_REWARD_TRACK_BAR_TOOLTIP_TITLE", bodyText, 5f);
		}
	}

	private void HideXPBarTooltip()
	{
		if (m_apprenticeXpBarTooltipZone != null)
		{
			m_apprenticeXpBarTooltipZone.HideTooltip();
		}
	}

	private PegasusShared.GameType GetGameTypeForNewPlayModeGame()
	{
		bool inRankedPlayMode = Options.GetInRankedPlayMode();
		if (IsInApprentice)
		{
			inRankedPlayMode = false;
		}
		if (!inRankedPlayMode)
		{
			return PegasusShared.GameType.GT_CASUAL;
		}
		return PegasusShared.GameType.GT_RANKED;
	}

	private PegasusShared.FormatType GetFormatTypeForNewPlayModeGame()
	{
		if (GetGameTypeForNewPlayModeGame() == PegasusShared.GameType.GT_CASUAL)
		{
			CollectionDeck selectedDeck = GetSelectedCollectionDeck();
			if (selectedDeck == null || IsInApprentice)
			{
				return PegasusShared.FormatType.FT_STANDARD;
			}
			return selectedDeck.FormatType;
		}
		return Options.GetFormatType();
	}

	private void UpdateFormat_Tournament(VisualsFormatType newVisualsFormatType)
	{
		Options.GetFormatType();
		bool num = CollectionManager.Get().ShouldAccountSeeStandardWild();
		SetPlayButtonText(GameStrings.Get("GLOBAL_PLAY"));
		if (num)
		{
			if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT && SetRotationManager.HasSeenStandardModeTutorial())
			{
				if (newVisualsFormatType == VisualsFormatType.VFT_WILD && !Options.Get().GetBool(Option.HAS_SEEN_WILD_MODE_VO) && UserAttentionManager.CanShowAttentionGrabber("DeckPickerTrayDisplay.UpdateFormat_Tournament:" + Option.HAS_SEEN_WILD_MODE_VO))
				{
					HideSetRotationNotifications();
					NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, INNKEEPER_QUOTE_POS, GameStrings.Get("VO_INNKEEPER_WILD_GAME"), "VO_INNKEEPER_Male_Dwarf_SetRotation_35.prefab:db2f6e3818fa49b4d8423121eba762f6");
					Options.Get().SetBool(Option.HAS_SEEN_WILD_MODE_VO, val: true);
				}
				if (newVisualsFormatType == VisualsFormatType.VFT_CLASSIC && !Options.Get().GetBool(Option.HAS_SEEN_CLASSIC_MODE_VO) && UserAttentionManager.CanShowAttentionGrabber("DeckPickerTrayDisplay.UpdateFormat_Tournament:" + Option.HAS_SEEN_CLASSIC_MODE_VO))
				{
					HideSetRotationNotifications();
					NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, INNKEEPER_QUOTE_POS, GameStrings.Get("VO_INNKEEPER_CLASSIC_TAKES_YOU_BACK_ORIGINAL_HEARTHSTONE"), "VO_Innkeeper_Male_Dwarf_ClassicMode_06.prefab:f91da6f7e66fd754fb4e568d15d49116");
					Options.Get().SetBool(Option.HAS_SEEN_CLASSIC_MODE_VO, val: true);
				}
				if (newVisualsFormatType == VisualsFormatType.VFT_TWIST && !Options.Get().GetBool(Option.HAS_SEEN_TWIST_MODE_VO) && UserAttentionManager.CanShowAttentionGrabber("DeckPickerTrayDisplay.UpdateFormat_Tournament:" + Option.HAS_SEEN_TWIST_MODE_VO))
				{
					HideSetRotationNotifications();
					NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, INNKEEPER_QUOTE_POS, GameStrings.Get("GLUE_TWIST_NEW_MODE"), "");
					Options.Get().SetBool(Option.HAS_SEEN_TWIST_MODE_VO, val: true);
				}
			}
			if (m_selectedCustomDeckBox != null && !m_selectedCustomDeckBox.CanSelectDeck())
			{
				Deselect();
			}
			UpdateCustomTournamentBackgroundAndDecks();
		}
		ChangePlayButtonTextAlpha();
		UpdateRankedClassWinsPlate();
		UpdateRankedPlayDisplay(newVisualsFormatType);
	}

	private void ChangePlayButtonTextAlpha()
	{
		if (m_playButton != null)
		{
			if (m_playButton.IsEnabled())
			{
				m_playButton.m_newPlayButtonText.TextAlpha = 1f;
			}
			else
			{
				m_playButton.m_newPlayButtonText.TextAlpha = 0f;
			}
		}
	}

	private void UpdateRankedPlayDisplay(VisualsFormatType newVisualsFormatType)
	{
		if (!newVisualsFormatType.IsRanked())
		{
			m_rankedPlayDisplay.Hide();
			return;
		}
		m_rankedPlayDisplay.Show();
		m_rankedPlayDisplay.UpdateMode(newVisualsFormatType);
		RankedRewardInfoButton rankedRewardInfoButton = m_rankedPlayDisplay.GetComponentInChildren<RankedRewardInfoButton>();
		if (!(rankedRewardInfoButton != null))
		{
			return;
		}
		TournamentDisplay tournamentDisplay = TournamentDisplay.Get();
		if (!(tournamentDisplay == null))
		{
			NetCache.NetCacheMedalInfo netCacheMedalInfo = tournamentDisplay.GetCurrentMedalInfo();
			if (netCacheMedalInfo != null)
			{
				MedalInfoTranslator mit = new MedalInfoTranslator(netCacheMedalInfo);
				rankedRewardInfoButton.Initialize(mit);
			}
		}
	}

	private void UpdateFormat_CollectionManager()
	{
		PegasusShared.FormatType currentFormatType = Options.GetFormatType();
		bool inRankedPlayMode = Options.GetInRankedPlayMode();
		if (currentFormatType == PegasusShared.FormatType.FT_WILD && !m_HasSeenPlayStandardToWildVO)
		{
			m_HasSeenPlayStandardToWildVO = true;
			m_HasSeenPlayStandardToTwistVO = false;
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, NotificationManager.DEFAULT_CHARACTER_POS, GameStrings.Get("VO_INNKEEPER_PLAY_STANDARD_TO_WILD"), "VO_INNKEEPER_Male_Dwarf_SetRotation_43.prefab:4b4ce858139927946905ec0d40d5b3c1");
		}
		else if (currentFormatType == PegasusShared.FormatType.FT_TWIST && !m_HasSeenPlayStandardToTwistVO)
		{
			m_HasSeenPlayStandardToTwistVO = true;
			m_HasSeenPlayStandardToWildVO = false;
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, NotificationManager.DEFAULT_CHARACTER_POS, GameStrings.Get("GLUE_TWIST_NEW_DECK"), "");
		}
		else if (currentFormatType == PegasusShared.FormatType.FT_STANDARD)
		{
			m_HasSeenPlayStandardToTwistVO = false;
			m_HasSeenPlayStandardToWildVO = false;
		}
		StartCoroutine(InitModeWhenReady());
		TransitionToFormatType(currentFormatType, inRankedPlayMode);
	}

	private void UpdateCustomTournamentBackgroundAndDecks()
	{
		TransitionToFormatType(Options.GetFormatType(), Options.GetInRankedPlayMode());
		UpdateDeckVisuals();
	}

	private void InitButtonAchievements()
	{
		UpdateCollectionButtonGlow();
		foreach (TAG_CLASS validClass in m_validClasses)
		{
			VisualsFormatTypeExtensions.GetCurrentVisualsFormatType();
			if (IsHeroPickerButtonLocked(validClass) != HeroPickerLockedStatus.UNLOCKED)
			{
				continue;
			}
			HeroPickerButton button = m_heroButtons.Find((HeroPickerButton obj) => obj.GetEntityDef().GetClass() == validClass);
			button.Unlock();
			if (IsChoosingHero())
			{
				CollectionManager.PreconDeck preconDeck = CollectionManager.Get().GetPreconDeck(validClass);
				long preconDeckID = 0L;
				if (preconDeck != null)
				{
					preconDeckID = preconDeck.ID;
				}
				button.SetPreconDeckID(preconDeckID);
				if (preconDeckID == 0L)
				{
					Debug.LogError($"DeckPickerTrayDisplay.InitButtonAchievements() - preconDeckID = 0 for class {validClass}");
				}
				SceneMgr.Mode mode = SceneMgr.Get().GetMode();
				if (mode == SceneMgr.Mode.TAVERN_BRAWL || (mode == SceneMgr.Mode.FRIENDLY && FriendChallengeMgr.Get().IsChallengeTavernBrawl()))
				{
					button.Unlock();
				}
			}
		}
	}

	protected override void SetHeaderForTavernBrawl()
	{
		if (m_labelDecoration != null)
		{
			m_labelDecoration.SetActive(value: false);
		}
		base.SetHeaderForTavernBrawl();
	}

	protected override void InitHeroPickerButtons()
	{
		base.InitHeroPickerButtons();
		CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK);
		m_heroDefsLoading = m_validClasses.Count;
		for (int i = 0; i < m_validClasses.Count; i++)
		{
			if (i >= m_heroButtons.Count || m_heroButtons[i] == null)
			{
				Debug.LogWarning("InitHeroPickerButtons: not enough buttons for total guest heroes.");
				break;
			}
			HeroPickerButton button = m_heroButtons[i];
			button.Lock();
			TAG_CLASS heroClass = m_validClasses[i];
			NetCache.CardDefinition heroCardDef = CollectionManager.Get().GetRandomFavoriteHero(heroClass, null);
			if (heroCardDef == null)
			{
				if (heroClass != TAG_CLASS.WHIZBANG)
				{
					Debug.LogWarning("Couldn't find Favorite Hero for hero class: " + heroClass.ToString() + " defaulting to Vanilla Hero!");
				}
				string heroCardID = CollectionManager.GetVanillaHero(heroClass);
				TAG_PREMIUM heroPremium = CollectionManager.Get().GetHeroPremium(heroClass);
				HeroFullDefLoadedCallbackData callbackData = new HeroFullDefLoadedCallbackData(button, heroPremium);
				DefLoader.Get().LoadFullDef(heroCardID, OnHeroFullDefLoaded, callbackData);
			}
			else
			{
				HeroFullDefLoadedCallbackData callbackData2 = new HeroFullDefLoadedCallbackData(button, heroCardDef.Premium);
				DefLoader.Get().LoadFullDef(heroCardDef.Name, OnHeroFullDefLoaded, callbackData2);
			}
			if (IsChoosingHero())
			{
				button.SetDivotTexture(m_currentModeTextures.classDivotTex);
			}
			else
			{
				button.SetDivotTexture(m_currentModeTextures.guestHeroDivotTex);
			}
		}
		if (IsChoosingHeroForDungeonCrawlAdventure())
		{
			SetUpHeroCrowns();
		}
	}

	private void InitDeckPages()
	{
		Log.PlayModeInvestigation.PrintInfo("DeckPickerTrayDisplay.InitDeckPages() called." + $"m_numPagesToShow={m_numPagesToShow}, m_customPages.Count={m_customPages.Count}");
		if (m_numPagesToShow <= 0)
		{
			Debug.LogWarning("DeckPickerTrayDisplay.InitDeckPages() called with invalid amount of pages");
			return;
		}
		while (m_numPagesToShow > m_customPages.Count)
		{
			CreateCustomDeckPage(CustomDeckPage.PageType.CUSTOM_DECK_DISPLAY);
		}
		while (m_numPagesToShow < m_customPages.Count)
		{
			CustomDeckPage pageToRemove = m_customPages[m_customPages.Count - 1];
			m_customPages.Remove(pageToRemove);
			UnityEngine.Object.Destroy(pageToRemove.gameObject);
			Log.PlayModeInvestigation.PrintInfo("DeckPickerTrayDisplay.InitDeckPages() -- Deck page removed." + $"New total: {m_customPages.Count}");
		}
		for (int i = 0; i < m_customPages.Count; i++)
		{
			m_customPages[i].DeckPageType = ((i >= m_numPagesToShow) ? CustomDeckPage.PageType.RESERVE : CustomDeckPage.PageType.CUSTOM_DECK_DISPLAY);
			m_customPages[i].DeckDisplayFormat = CustomDeckPage.DeckPageDisplayFormat.FILL_ALL_DECKSLOTS;
		}
	}

	private void CreateCustomDeckPage(CustomDeckPage.PageType pageType)
	{
		GameObject go = AssetLoader.Get().InstantiatePrefab(CUSTOM_DECK_PAGE);
		go.transform.SetParent(m_customDeckPagesRoot.transform, worldPositionStays: false);
		go.transform.localPosition = ((m_customPages.Count == 0) ? m_customDeckPageUpperBone.transform.localPosition : m_customDeckPageLowerBone.transform.localPosition);
		CustomDeckPage deckPage = go.GetComponent<CustomDeckPage>();
		deckPage.DeckPageType = pageType;
		deckPage.DeckDisplayFormat = CustomDeckPage.DeckPageDisplayFormat.FILL_ALL_DECKSLOTS;
		deckPage.SetDeckButtonCallback(OnCustomDeckPressed);
		switch (pageType)
		{
		case CustomDeckPage.PageType.LOANER_DECK_DISPLAY:
		{
			m_customPages.Insert(0, deckPage);
			GameObject obj = AssetLoader.Get().InstantiatePrefab(LOANER_DECK_TIMER);
			obj.transform.parent = go.transform;
			obj.transform.position = Vector3.zero;
			break;
		}
		case CustomDeckPage.PageType.PRECON_DECK_DISPLAY:
		case CustomDeckPage.PageType.RESERVE:
			m_customPages.Insert(m_customPages.Count, deckPage);
			break;
		default:
			m_customPages.Add(deckPage);
			break;
		}
		Log.PlayModeInvestigation.PrintInfo("DeckPickerTrayDisplay.InitDeckPages() -- Deck page added." + $" New total: {m_customPages.Count}");
	}

	private void SetPageDecks(List<CollectionDeck> decks)
	{
		if (m_customPages == null)
		{
			Debug.LogError("{0}.UpdateCustomPages(): m_customPages is null. Make sure you call InitCustomPages() first!", this);
		}
		int loanerDecksDisplayed = 0;
		List<CollectionDeck> loanerDecks = new List<CollectionDeck>();
		FreeDeckMgr freeDeckManager = FreeDeckMgr.Get();
		if (freeDeckManager.Status == FreeDeckMgr.FreeDeckStatus.TRIAL_PERIOD && freeDeckManager.GetLoanerDecksCount() > 0)
		{
			foreach (KeyValuePair<int, CollectionDeck> item in freeDeckManager.GetLoanerDecksAsMap())
			{
				loanerDecks.Add(item.Value);
			}
		}
		List<CollectionDeck> twistDecks = RankMgr.Get()?.GetCurrentTwistSeason()?.CreateCopyOfDeckList();
		foreach (CustomDeckPage page in m_customPages)
		{
			if (page.DeckPageType == CustomDeckPage.PageType.LOANER_DECK_DISPLAY)
			{
				SetLoanerDeckPageDecks(page, loanerDecks, ref loanerDecksDisplayed);
			}
		}
		FillDeckPages(CustomDeckPage.PageType.PRECON_DECK_DISPLAY, twistDecks, CustomDeckPage.DeckPageDisplayFormat.EMPTY_FIRST_ROW);
		FillDeckPages(CustomDeckPage.PageType.CUSTOM_DECK_DISPLAY, decks, null);
	}

	private void FillDeckPages(CustomDeckPage.PageType pageType, List<CollectionDeck> decks, CustomDeckPage.DeckPageDisplayFormat? firstPageFormat = null)
	{
		if (decks == null || decks.Count == 0)
		{
			return;
		}
		bool firstPage = true;
		foreach (CustomDeckPage page in m_customPages)
		{
			if (page.DeckPageType != pageType)
			{
				continue;
			}
			if (firstPage && firstPageFormat.HasValue)
			{
				page.DeckDisplayFormat = firstPageFormat.Value;
				firstPage = false;
			}
			int decksOnThisPageCount = page.GetAvailableDeckDisplaySlotCount(decks.Count);
			List<CollectionDeck> decksOnThisPage = decks.GetRange(0, decksOnThisPageCount);
			page.InitDecks(decksOnThisPage);
			foreach (CollectionDeck deck in decksOnThisPage)
			{
				string heroPowerCardId = GameUtils.GetHeroPowerCardIdFromHero(deck.HeroCardID);
				if (string.IsNullOrEmpty(heroPowerCardId))
				{
					Debug.LogErrorFormat("No hero power set up for hero {0}", deck.HeroCardID);
				}
				else
				{
					LoadHeroPowerDef(heroPowerCardId, CollectionManager.Get().GetHeroPremium(deck.GetClass()));
				}
			}
			decks.RemoveRange(0, decksOnThisPageCount);
			if (decks.Count <= 0)
			{
				break;
			}
		}
		if (decks.Count > 0)
		{
			Debug.LogWarningFormat("DeckPickerTrayDisplay - {0} more {1} decks than we can display!", decks.Count, pageType);
		}
	}

	private void SetLoanerDeckPageDecks(CustomDeckPage page, List<CollectionDeck> loanerDecks, ref int loanerDecksDisplayed)
	{
		if (loanerDecksDisplayed >= loanerDecks.Count)
		{
			return;
		}
		page.DeckDisplayFormat = CustomDeckPage.DeckPageDisplayFormat.EMPTY_FIRST_ROW;
		int decksOnThisPageCount = Mathf.Min(loanerDecks.Count - loanerDecksDisplayed, 6);
		List<CollectionDeck> decksOnThisPage = loanerDecks.GetRange(loanerDecksDisplayed, decksOnThisPageCount);
		foreach (CollectionDeck deck in decksOnThisPage)
		{
			string heroPowerCardId = GameUtils.GetHeroPowerCardIdFromHero(deck.HeroCardID);
			if (string.IsNullOrEmpty(heroPowerCardId))
			{
				Debug.LogErrorFormat("No hero power set up for hero {0}", deck.HeroCardID);
			}
			else
			{
				LoadHeroPowerDef(heroPowerCardId, CollectionManager.Get().GetHeroPremium(deck.GetClass()));
			}
		}
		page.InitDecks(decksOnThisPage);
		loanerDecksDisplayed = decksOnThisPageCount;
	}

	private void SetupPageDecksBasedOnFormat(VisualsFormatType format, List<CollectionDeck> decks)
	{
		if (m_customPages == null || decks == null)
		{
			return;
		}
		if (m_customPages.Count > 0)
		{
			CustomDeckPage reservePage = null;
			CustomDeckPage firstCustomDeckPage = null;
			int regularDeckPageCount = 0;
			foreach (CustomDeckPage page in m_customPages)
			{
				if (page.DeckPageType == CustomDeckPage.PageType.CUSTOM_DECK_DISPLAY)
				{
					if (firstCustomDeckPage == null)
					{
						firstCustomDeckPage = page;
					}
					page.ClearDeckSlots();
					regularDeckPageCount++;
				}
				if (page.DeckPageType == CustomDeckPage.PageType.RESERVE)
				{
					page.ClearDeckSlots();
					reservePage = page;
				}
			}
			if (reservePage != null)
			{
				bool shouldUseReserve = ShouldShowReservePage(decks.Count, regularDeckPageCount);
				reservePage.SetReservePageStatus(shouldUseReserve);
				m_numPagesToShow++;
			}
			if ((bool)firstCustomDeckPage)
			{
				if (format == VisualsFormatType.VFT_TWIST)
				{
					firstCustomDeckPage.DeckDisplayFormat = CustomDeckPage.DeckPageDisplayFormat.EMPTY_FIRST_ROW;
				}
				else
				{
					firstCustomDeckPage.DeckDisplayFormat = CustomDeckPage.DeckPageDisplayFormat.FILL_ALL_DECKSLOTS;
				}
			}
		}
		SetPageDecks(decks);
	}

	private bool ShouldShowReservePage(int deckCount, int regularDeckPageCount)
	{
		if (deckCount <= 0)
		{
			return false;
		}
		if (VisualsFormatTypeExtensions.GetCurrentVisualsFormatType() != VisualsFormatType.VFT_TWIST)
		{
			return false;
		}
		deckCount += 3;
		if (deckCount <= 9)
		{
			return false;
		}
		int pageCount = Mathf.Max(1, deckCount / 9);
		if (deckCount % 9 > 0)
		{
			pageCount++;
		}
		if (pageCount <= regularDeckPageCount)
		{
			return false;
		}
		return true;
	}

	private void InitMode()
	{
		if (IsChoosingHero())
		{
			ShowHeroPickerPage(skipTraySlidingAnimation: true);
		}
		else
		{
			if (SceneMgr.Get().GetMode() == SceneMgr.Mode.ADVENTURE)
			{
				if (CollectionManager.Get().ShouldAccountSeeStandardWild())
				{
					SwitchFormatTypeAndRankedPlayMode(VisualsFormatType.VFT_WILD);
				}
				else
				{
					SwitchFormatTypeAndRankedPlayMode(VisualsFormatType.VFT_STANDARD);
				}
			}
			SetSelectionAndPageFromOptions();
		}
		InitExpoDemoMode();
		ShowNewTwistModePopupIfNecessary(1f);
		ShowSwitchToWildTutorialIfNecessary();
	}

	private void InitExpoDemoMode()
	{
		if (DemoMgr.Get().IsExpoDemo())
		{
			UpdatePageArrows();
			SetBackButtonEnabled(enable: false);
			StartCoroutine("ShowDemoQuotes");
		}
	}

	private IEnumerator ShowDemoQuotes()
	{
		string thankQuote = Vars.Key("Demo.ThankQuote").GetStr("");
		int thankQuoteTime = Vars.Key("Demo.ThankQuoteMsTime").GetInt(0);
		thankQuote = thankQuote.Replace("\\n", "\n");
		if (!string.IsNullOrEmpty(thankQuote) && thankQuoteTime > 0)
		{
			m_expoThankQuote = NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, new Vector3(158.1f, NotificationManager.DEPTH, 80.2f), thankQuote, "", (float)thankQuoteTime / 1000f);
			EnableClickBlocker(enable: true);
			yield return new WaitForSeconds((float)thankQuoteTime / 1000f);
			EnableClickBlocker(enable: false);
		}
		ShowIntroQuote();
	}

	private void ShowIntroQuote()
	{
		HideIntroQuote();
		string introQuote = Vars.Key("Demo.IntroQuote").GetStr("");
		introQuote = introQuote.Replace("\\n", "\n");
		if (!string.IsNullOrEmpty(introQuote))
		{
			m_expoIntroQuote = NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, new Vector3(147.6f, NotificationManager.DEPTH, 23.1f), introQuote, "");
		}
	}

	private void EnableClickBlocker(bool enable)
	{
		if (!(m_clickBlocker == null))
		{
			if (enable)
			{
				ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignettePerspective;
				screenEffectParameters.Blur = new BlurParameters(1f, 1f);
				m_screenEffectsHandle.StartEffect(screenEffectParameters);
			}
			else
			{
				m_screenEffectsHandle.StopEffect();
			}
			m_clickBlocker.gameObject.SetActive(enable);
		}
	}

	private void HideDemoQuotes()
	{
		DemoMgr demoMgr = DemoMgr.Get();
		if (demoMgr != null && !demoMgr.IsExpoDemo())
		{
			return;
		}
		StopCoroutine("ShowDemoQuotes");
		if (m_expoThankQuote != null)
		{
			NotificationManager notificationManager = NotificationManager.Get();
			if (notificationManager != null)
			{
				notificationManager.DestroyNotification(m_expoThankQuote, 0f);
			}
			m_expoThankQuote = null;
			m_screenEffectsHandle.StopEffect();
		}
		HideIntroQuote();
	}

	private void HideIntroQuote()
	{
		if (m_expoIntroQuote != null)
		{
			NotificationManager.Get().DestroyNotification(m_expoIntroQuote, 0f);
			m_expoIntroQuote = null;
		}
	}

	private void HideSetRotationNotifications()
	{
		if (m_innkeeperQuote != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_innkeeperQuote);
			m_innkeeperQuote = null;
		}
		if (m_switchFormatPopup != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_switchFormatPopup);
			m_switchFormatPopup = null;
		}
	}

	private void OnTransitionFromGameplayFinished(bool cutoff, object userData)
	{
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.FRIENDLY && !FriendChallengeMgr.Get().HasChallenge())
		{
			GoBackUntilOnNavigateBackCalled();
		}
		LoadingScreen.Get().UnregisterFinishedTransitionListener(OnTransitionFromGameplayFinished);
	}

	private void CollectionButtonPress(UIEvent e)
	{
		NavigateToCollectionManager();
	}

	private void NavigateToCollectionManager()
	{
		if (ShouldGlowCollectionButton())
		{
			if (!Options.Get().GetBool(Option.HAS_CLICKED_COLLECTION_BUTTON_FOR_NEW_DECK) && HaveDecksThatNeedNames())
			{
				Options.Get().SetBool(Option.HAS_CLICKED_COLLECTION_BUTTON_FOR_NEW_DECK, val: true);
			}
			else if (!Options.Get().GetBool(Option.HAS_CLICKED_COLLECTION_BUTTON_FOR_NEW_CARD) && HaveUnseenCards())
			{
				Options.Get().SetBool(Option.HAS_CLICKED_COLLECTION_BUTTON_FOR_NEW_CARD, val: true);
			}
			if (Options.Get().GetBool(Option.GLOW_COLLECTION_BUTTON_AFTER_SET_ROTATION) && SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT)
			{
				Options.Get().SetBool(Option.GLOW_COLLECTION_BUTTON_AFTER_SET_ROTATION, val: false);
			}
		}
		if (PracticePickerTrayDisplay.Get() != null && PracticePickerTrayDisplay.Get().IsShown())
		{
			Navigation.GoBack();
		}
		TelemetryWatcher.StopWatchingFor(TelemetryWatcherWatchType.CollectionManagerFromDeckPicker);
		TelemetryManager.Client().SendDeckPickerToCollection(DeckPickerToCollection.Path.DECK_PICKER_BUTTON);
		CollectionManager.Get().NotifyOfBoxTransitionStart();
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.COLLECTIONMANAGER);
	}

	private void RequestDeckShareButtonPress(UIEvent e)
	{
		if (m_doingDeckShareTransition)
		{
			return;
		}
		if (m_usingSharedDecks)
		{
			FriendChallengeMgr.Get().EndDeckShare();
		}
		else
		{
			if (!FriendChallengeMgr.Get().HasOpponentSharedDecks())
			{
				EnableRequestDeckShareButton(enable: false);
			}
			FriendChallengeMgr.Get().RequestDeckShare();
		}
		UpdateDeckShareTooltip();
	}

	private void RequestDeckShareButtonOver(UIEvent e)
	{
		m_isDeckShareRequestButtonHovered = true;
		UpdateDeckShareTooltip();
	}

	private void RequestDeckShareButtonOut(UIEvent e)
	{
		m_isDeckShareRequestButtonHovered = false;
		UpdateDeckShareTooltip();
	}

	private void EnableRequestDeckShareButton(bool enable)
	{
		if (m_DeckShareRequestButton.IsEnabled() != enable)
		{
			if (!enable)
			{
				m_DeckShareRequestButton.TriggerOut();
			}
			m_DeckShareRequestButton.SetEnabled(enable);
			m_DeckShareRequestButton.Flip(enable);
		}
		UpdateDeckShareRequestButton();
	}

	private void UpdateDeckShareRequestButton()
	{
		if (!(m_DeckShareRequestButton == null) && IsDeckSharingActive())
		{
			if (!FriendChallengeMgr.Get().HasOpponentSharedDecks())
			{
				m_DeckShareRequestButton.SetText(GameStrings.Get("GLUE_DECK_SHARE_BUTTON_BORROW_DECKS"));
			}
			else if (m_usingSharedDecks)
			{
				m_DeckShareRequestButton.SetText(GameStrings.Get("GLUE_DECK_SHARE_BUTTON_SHOW_MY_DECKS"));
			}
			else
			{
				m_DeckShareRequestButton.SetText(GameStrings.Format("GLUE_DECK_SHARE_BUTTON_SHOW_OPPONENT_DECKS"));
			}
			UpdateDeckShareTooltip();
		}
	}

	private void UpdateDeckShareTooltip()
	{
		if (m_DeckShareRequestButton == null)
		{
			return;
		}
		TooltipZone tooltipZone = m_DeckShareRequestButton.GetComponentInChildren<TooltipZone>();
		if (tooltipZone == null)
		{
			return;
		}
		if (!FriendChallengeMgr.Get().HasOpponentSharedDecks())
		{
			if (m_isDeckShareRequestButtonHovered && !tooltipZone.IsShowingTooltip())
			{
				string opponentName = string.Empty;
				BnetPlayer opponent = FriendChallengeMgr.Get().GetMyOpponent();
				if (opponent != null)
				{
					opponentName = opponent.GetBestName();
				}
				tooltipZone.ShowTooltip(GameStrings.Get("GLUE_DECK_SHARE_TOOLTIP_HEADER"), GameStrings.Format("GLUE_DECK_SHARE_TOOLTIP_BODY_REQUEST", opponentName), 5f);
			}
			else if (!m_isDeckShareRequestButtonHovered && tooltipZone.IsShowingTooltip())
			{
				tooltipZone.HideTooltip();
			}
		}
		else if (tooltipZone.IsShowingTooltip())
		{
			tooltipZone.HideTooltip();
		}
	}

	private void OnDeckShareRequestCancelDeclineOrError()
	{
		StopCoroutine("WaitThanEnableRequestDeckShareButton");
		StartCoroutine("WaitThanEnableRequestDeckShareButton");
	}

	private IEnumerator WaitThanEnableRequestDeckShareButton()
	{
		yield return new WaitForSeconds(1f);
		EnableRequestDeckShareButton(enable: true);
	}

	public void UseSharedDecks(List<CollectionDeck> decks)
	{
		StartCoroutine(UseSharedDecksImpl(decks));
	}

	private IEnumerator UseSharedDecksImpl(List<CollectionDeck> decks)
	{
		if (m_usingSharedDecks || decks == null)
		{
			yield break;
		}
		m_doingDeckShareTransition = true;
		m_clickBlocker.gameObject.SetActive(value: true);
		m_usingSharedDecks = true;
		UpdateDeckShareRequestButton();
		Deselect();
		m_deckPickerMode = DeckPickerMode.CUSTOM;
		if (!string.IsNullOrEmpty(m_wildDeckTransitionSound))
		{
			SoundManager.Get().LoadAndPlay(m_wildDeckTransitionSound);
		}
		if (m_DeckShareGlowOutQuad != null)
		{
			m_DeckShareGlowOutQuad.SetActive(value: true);
			yield return StartCoroutine(FadeDeckShareGlowOutQuad(0f, m_DeckShareGlowOutIntensity, m_DeckShareTransitionTime * 0.5f));
		}
		if (m_DeckShareParticles != null)
		{
			m_DeckShareParticles.Stop();
			m_DeckShareParticles.Play();
		}
		SetupDeckPages(decks);
		m_basicDeckPageContainer.gameObject.SetActive(value: false);
		foreach (CollectionDeck deck in decks)
		{
			deck.Locked = false;
		}
		VisualsFormatType currentVisualsFormatType = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType();
		Texture customFormatTexture = m_currentModeTextures.GetCustomTextureForFormat(currentVisualsFormatType);
		SetCustomDeckPageTextures(customFormatTexture, customFormatTexture);
		int pageNum = 0;
		if (FreeDeckMgr.Get().Status == FreeDeckMgr.FreeDeckStatus.TRIAL_PERIOD && currentVisualsFormatType == VisualsFormatType.VFT_TWIST)
		{
			for (int i = 0; i < m_customPages.Count; i++)
			{
				if (m_customPages[i].DeckPageType != CustomDeckPage.PageType.LOANER_DECK_DISPLAY)
				{
					pageNum = i;
					break;
				}
			}
		}
		ShowPage(pageNum, skipTraySlidingAnimation: true);
		if (m_DeckShareGlowOutQuad != null)
		{
			yield return StartCoroutine(FadeDeckShareGlowOutQuad(m_DeckShareGlowOutIntensity, 0f, m_DeckShareTransitionTime * 0.5f));
			m_DeckShareGlowOutQuad.SetActive(value: false);
		}
		EnableRequestDeckShareButton(enable: true);
		m_clickBlocker.gameObject.SetActive(value: false);
		m_doingDeckShareTransition = false;
	}

	public void StopUsingSharedDecks()
	{
		StartCoroutine(StopUsingSharedDecksImpl());
	}

	private IEnumerator StopUsingSharedDecksImpl()
	{
		if (m_usingSharedDecks)
		{
			m_clickBlocker.gameObject.SetActive(value: true);
			m_doingDeckShareTransition = true;
			m_usingSharedDecks = false;
			UpdateDeckShareRequestButton();
			Deselect();
			if (!string.IsNullOrEmpty(m_wildDeckTransitionSound))
			{
				SoundManager.Get().LoadAndPlay(m_wildDeckTransitionSound);
			}
			if (m_DeckShareGlowOutQuad != null)
			{
				m_DeckShareGlowOutQuad.SetActive(value: true);
				yield return StartCoroutine(FadeDeckShareGlowOutQuad(0f, m_DeckShareGlowOutIntensity, m_DeckShareTransitionTime * 0.5f));
			}
			if (m_DeckShareParticles != null)
			{
				m_DeckShareParticles.Stop();
				m_DeckShareParticles.Play();
			}
			List<CollectionDeck> decks = CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK).FindAll((CollectionDeck deck) => deck.IsValidForFormat(FriendChallengeMgr.Get().GetFormatType()));
			SetupDeckPages(decks);
			ShowPage(0, skipTraySlidingAnimation: true);
			if (m_DeckShareGlowOutQuad != null)
			{
				yield return StartCoroutine(FadeDeckShareGlowOutQuad(m_DeckShareGlowOutIntensity, 0f, m_DeckShareTransitionTime * 0.5f));
				m_DeckShareGlowOutQuad.SetActive(value: false);
			}
			EnableRequestDeckShareButton(enable: true);
			m_doingDeckShareTransition = false;
			m_clickBlocker.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator FadeDeckShareGlowOutQuad(float startingIntensity, float finalIntensity, float fadeTime)
	{
		if (!(m_DeckShareGlowOutQuad == null))
		{
			int propertyID = Shader.PropertyToID("_Intensity");
			float currentIntensity = startingIntensity;
			Material mat = m_DeckShareGlowOutQuad.GetComponentInChildren<MeshRenderer>(includeInactive: true).GetMaterial();
			mat.SetFloat(propertyID, currentIntensity);
			float transitionSpeed = Mathf.Abs(finalIntensity - startingIntensity) / fadeTime;
			while (currentIntensity != finalIntensity)
			{
				currentIntensity = Mathf.MoveTowards(currentIntensity, finalIntensity, transitionSpeed * Time.deltaTime);
				mat.SetFloat(propertyID, currentIntensity);
				yield return null;
			}
		}
	}

	public void SwitchFormatButtonPress(UIEvent e)
	{
		SwitchFormatButtonPress();
		if (DeckPickerTrayDisplay.OnFormatSwitchButtonPressed != null)
		{
			DeckPickerTrayDisplay.OnFormatSwitchButtonPressed();
		}
	}

	public void SwitchFormatButtonPress()
	{
		m_switchFormatButton.Disable();
		m_switchFormatButton.gameObject.SetActive(value: false);
		ShowFormatTypePickerPopup();
		DismissSwitchFormatPopup();
	}

	public void ShowFormatTypePickerPopup()
	{
		if (m_formatConfig.TryGetValue(VisualsFormatTypeExtensions.GetCurrentVisualsFormatType(), out var config) && config.m_showDeckContentsForSelectedDeck && TwistDetailsDisplayManager.Get() != null)
		{
			TwistDetailsDisplayManager.Get().ToggleTwistHeroicDeckCardPanel(openPanel: false);
		}
		IsModeSwitchShowing = true;
		m_formatTypePickerWidget.transform.position = new Vector3(0f, m_formatTypePickerYOffset, 0f);
		m_formatTypePickerWidget.Show();
		UpdateAvailableFormatOptions();
		m_formatTypePickerWidget.TriggerEvent("OPEN", new TriggerEventParameters(null, m_visualsFormatType));
	}

	public void ShowPopupDuringSetRotation(VisualsFormatType visualsFormatType)
	{
		IsModeSwitchShowing = true;
		m_formatTypePickerWidget.transform.position = new Vector3(0f, m_formatTypePickerYOffset, 0f);
		m_formatTypePickerWidget.Show();
		m_formatTypePickerWidget.TriggerEvent("SET_ROTATION_OPEN", new TriggerEventParameters(null, (int)visualsFormatType));
	}

	public void SwitchFormatTypeAndRankedPlayMode(VisualsFormatType newVisualsFormatType)
	{
		VisualsFormatType oldVisualsFormatType = VisualsFormatTypeExtensions.ToVisualsFormatType(Options.GetFormatType(), Options.GetInRankedPlayMode());
		if (oldVisualsFormatType != newVisualsFormatType)
		{
			if (newVisualsFormatType.ToFormatType() == PegasusShared.FormatType.FT_UNKNOWN)
			{
				RankMgr.LogMessage("newVisualsFormatType.ToFormatType() = FT_UNKOWN", "SwitchFormatTypeAndRankedPlayMode", "D:\\p4Workspace\\31.6.0\\Pegasus\\Client\\Assets\\Game\\DeckPickerTray\\DeckPickerTrayDisplay.cs", 2954);
				return;
			}
			Options.SetFormatType(newVisualsFormatType.ToFormatType());
			Options.SetInRankedPlayMode(newVisualsFormatType.IsRanked());
			if (oldVisualsFormatType == VisualsFormatType.VFT_TWIST || newVisualsFormatType == VisualsFormatType.VFT_TWIST)
			{
				SetupPageDecksBasedOnFormat(newVisualsFormatType, CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK));
				SetSelectionAndPageFromOptions();
			}
			TransitionToFormatType(newVisualsFormatType.ToFormatType(), newVisualsFormatType.IsRanked());
		}
		RankMgr.Get().SetRankPresenceField();
		m_visualsFormatType = newVisualsFormatType;
		m_switchFormatButton.SetVisualsFormatType(m_visualsFormatType);
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		switch (mode)
		{
		case SceneMgr.Mode.COLLECTIONMANAGER:
			UpdateCreateDeckText();
			UpdateFormat_CollectionManager();
			break;
		case SceneMgr.Mode.TOURNAMENT:
			UpdateFormat_Tournament(newVisualsFormatType);
			TournamentDisplay.Get().UpdateHeaderText();
			m_rankedPlayDisplay.OnSwitchFormat(newVisualsFormatType);
			ToggleApprenticeProgressTray(newVisualsFormatType == VisualsFormatType.VFT_CASUAL);
			break;
		}
		SetMissingTwistDeckActive(active: false);
		if (newVisualsFormatType == VisualsFormatType.VFT_TWIST && mode == SceneMgr.Mode.TOURNAMENT && !ValidTwistDecksExist())
		{
			SetMissingTwistDeckActive(active: true);
		}
		UpdatePageArrows();
		m_formatTypePickerWidget.TriggerEvent("HIDE");
		if (CanSwitchFormats())
		{
			StartCoroutine(m_switchFormatButton.EnableWithDelay(0.8f));
		}
		StartCoroutine(DisableIsModeSwitchShowing(0.8f));
		if (mode != SceneMgr.Mode.TOURNAMENT)
		{
			return;
		}
		DeckPickerTrayDisplay capturedThis = this;
		if (ShouldShowRotatedBoosterPopup(newVisualsFormatType))
		{
			Action finishedCallback = delegate
			{
				if (capturedThis != null)
				{
					capturedThis.StartCoroutine(ShowIntroPopupsIfNeeded_Routine());
				}
			};
			StartCoroutine(ShowRotatedBoostersPopup(finishedCallback));
		}
		else if (ShouldShowStandardDeckVO(newVisualsFormatType))
		{
			Action<int> finishedCallback2 = delegate
			{
				if (capturedThis != null)
				{
					capturedThis.StartCoroutine(ShowIntroPopupsIfNeeded_Routine());
				}
			};
			StartCoroutine(ShowStandardDeckVO(finishedCallback2));
		}
		else
		{
			StartCoroutine(ShowIntroPopupsIfNeeded_Routine(1f));
		}
		ToggleAlternateDeckFrame(m_formatConfig[VisualsFormatTypeExtensions.GetCurrentVisualsFormatType()].m_showDeckContentsForSelectedDeck);
	}

	private IEnumerator DisableIsModeSwitchShowing(float delay)
	{
		yield return new WaitForSeconds(delay);
		IsModeSwitchShowing = false;
	}

	private bool CanSwitchFormats()
	{
		SceneMgr.Mode currentMode = SceneMgr.Get().GetMode();
		if (currentMode == SceneMgr.Mode.FRIENDLY || (uint)(currentMode - 13) <= 1u)
		{
			return false;
		}
		return true;
	}

	public static bool OnNavigateBack()
	{
		if (Get() != null)
		{
			return Get().OnNavigateBackImplementation();
		}
		Debug.LogError("HeroPickerTrayDisplay: tried to navigate back but had null instance!");
		return false;
	}

	protected override bool OnNavigateBackImplementation()
	{
		if (!m_backButton.IsEnabled())
		{
			return false;
		}
		switch ((SceneMgr.Get() != null) ? SceneMgr.Get().GetMode() : SceneMgr.Mode.INVALID)
		{
		case SceneMgr.Mode.COLLECTIONMANAGER:
		case SceneMgr.Mode.TAVERN_BRAWL:
		{
			CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
			if (CollectionDeckTray.Get() != null)
			{
				CollectionDeckTray.Get().GetDecksContent().CreateNewDeckCancelled();
			}
			if (Get() != null && !Get().m_heroChosen && cmd != null)
			{
				cmd.CancelSelectNewDeckHeroMode();
			}
			if (HeroPickerDisplay.Get() != null)
			{
				HeroPickerDisplay.Get().HideTray();
			}
			PresenceMgr.Get().SetPrevStatus();
			if (SceneMgr.Get().IsInTavernBrawlMode())
			{
				TavernBrawlDisplay.Get().EnablePlayButton();
			}
			if (cmd != null)
			{
				DeckTemplatePicker deckTemplatePicker = (UniversalInputManager.UsePhoneUI ? cmd.GetPhoneDeckTemplateTray() : cmd.m_pageManager.GetDeckTemplatePicker());
				if (deckTemplatePicker != null)
				{
					Navigation.RemoveHandler(deckTemplatePicker.OnNavigateBack);
				}
			}
			break;
		}
		case SceneMgr.Mode.TOURNAMENT:
			BackOutToHub();
			GameMgr.Get().CancelFindGame();
			break;
		case SceneMgr.Mode.ADVENTURE:
			AdventureConfig.Get().SubSceneGoBack();
			if (AdventureConfig.Get().CurrentSubScene == AdventureData.Adventuresubscene.PRACTICE)
			{
				PracticePickerTrayDisplay.Get().gameObject.SetActive(value: false);
			}
			GameMgr.Get().CancelFindGame();
			break;
		}
		return base.OnNavigateBackImplementation();
	}

	protected override void GoBackUntilOnNavigateBackCalled()
	{
		Navigation.GoBackUntilOnNavigateBackCalled(OnNavigateBack);
	}

	public override void PreUnload()
	{
		if (!IsShowingFirstPage() && m_randomDeckPickerTray.activeSelf)
		{
			HideHeroPickerPage();
		}
	}

	private void ShowNextPage(bool skipTraySlidingAnimation = false)
	{
		ShowPage(FindNextValidPage(1), skipTraySlidingAnimation);
	}

	private void ShowPreviousPage(bool skipTraySlidingAnimation = false)
	{
		ShowPage(FindNextValidPage(-1), skipTraySlidingAnimation);
	}

	private void ShowPage(int pageNum, bool skipTraySlidingAnimation = false)
	{
		if (iTween.Count(m_randomDeckPickerTray) > 0 || pageNum < 0 || pageNum >= m_customPages.Count)
		{
			return;
		}
		if (!IsPageValidForCurrentFormat(pageNum))
		{
			pageNum = FindNextValidPage(1);
		}
		for (int i = 0; i < m_customPages.Count; i++)
		{
			m_customPages[i].gameObject.SetActive((i == m_currentPageIndex && !skipTraySlidingAnimation) || i == pageNum);
			if (skipTraySlidingAnimation)
			{
				Vector3 newLocalPos = m_customDeckPageUpperBone.transform.localPosition;
				if (i < pageNum)
				{
					newLocalPos = m_customDeckPageHideBone.transform.localPosition;
				}
				else if (i > pageNum)
				{
					newLocalPos = m_customDeckPageLowerBone.transform.localPosition;
				}
				m_customPages[i].gameObject.transform.localPosition = newLocalPos;
			}
		}
		bool isCurrentVisualFormatTwist = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType() == VisualsFormatType.VFT_TWIST;
		bool pageHasEmptyFirstRow = m_customPages[pageNum].DeckDisplayFormat == CustomDeckPage.DeckPageDisplayFormat.EMPTY_FIRST_ROW && (m_customPages[pageNum].DeckPageType == CustomDeckPage.PageType.CUSTOM_DECK_DISPLAY || m_customPages[pageNum].DeckPageType == CustomDeckPage.PageType.PRECON_DECK_DISPLAY);
		bool shouldShowTwistHeader = isCurrentVisualFormatTwist && pageHasEmptyFirstRow;
		TwistDetailsDisplayManager twistDisplayManager = TwistDetailsDisplayManager.Get();
		if (m_currentPageIndex != pageNum && !skipTraySlidingAnimation)
		{
			GameObject currentPage = m_customPages[m_currentPageIndex].gameObject;
			GameObject nextPage = m_customPages[pageNum].gameObject;
			bool isPagingForward = pageNum > m_currentPageIndex;
			if (isPagingForward)
			{
				ToggleTwistHeader(shouldShowTwistHeader, isPagingForward);
				iTween.MoveTo(currentPage, iTween.Hash("time", 0.25f, "position", m_customDeckPageHideBone.transform.localPosition, "islocal", true, "easetype", iTween.EaseType.easeOutCubic, "oncomplete", (Action<object>)delegate
				{
					currentPage.SetActive(value: false);
				}, "oncompletetarget", base.gameObject));
				iTween.MoveTo(nextPage, iTween.Hash("time", 0.25f, "delay", 0.25f, "easetype", iTween.EaseType.easeOutCubic, "position", m_customDeckPageUpperBone.transform.localPosition, "islocal", true));
			}
			else
			{
				iTween.MoveTo(currentPage, iTween.Hash("time", 0.25f, "easetype", iTween.EaseType.easeOutCubic, "oncomplete", (Action<object>)delegate
				{
					if (!shouldShowTwistHeader)
					{
						twistDisplayManager.HideTwistHeaderPanel();
					}
					currentPage.SetActive(value: false);
				}, "position", m_customDeckPageLowerBone.transform.localPosition, "islocal", true));
				if (shouldShowTwistHeader)
				{
					twistDisplayManager.ShowTwistHeaderPanelSlideLeft();
				}
				iTween.MoveTo(nextPage, iTween.Hash("time", 0.25f, "delay", 0.25f, "easetype", iTween.EaseType.easeOutCubic, "position", m_customDeckPageUpperBone.transform.localPosition, "islocal", true));
			}
		}
		else
		{
			ToggleTwistHeader(shouldShowTwistHeader, isPagingForward: false);
		}
		m_currentPageIndex = pageNum;
		HideAllPreconHighlights();
		LowerHeroButtons();
		if (ShouldHandleBoxTransition() || skipTraySlidingAnimation)
		{
			HideHeroPickerPage();
			Box.Get().AddTransitionFinishedListener(OnBoxTransitionFinished);
		}
		else
		{
			iTween.MoveTo(m_randomDeckPickerTray, iTween.Hash("time", 0.25f, "position", m_randomDecksHiddenBone.transform.localPosition, "oncomplete", (Action<object>)delegate
			{
				HideHeroPickerPage();
			}, "oncompletetarget", base.gameObject, "islocal", true));
		}
		LoanerDeckDisplay loanerDeckDisplay = LoanerDeckDisplay.Get();
		if (loanerDeckDisplay != null)
		{
			bool isPageLoaner = m_customPages[pageNum].DeckPageType == CustomDeckPage.PageType.LOANER_DECK_DISPLAY;
			loanerDeckDisplay.SetCurrentPageStatusInDataModel(isPageLoaner);
			loanerDeckDisplay.ShowLoanerFTUENotification(isPageLoaner);
		}
		UpdatePageArrows();
		Options.Get().SetBool(Option.HAS_SEEN_CUSTOM_DECK_PICKER, val: true);
	}

	private void ToggleTwistHeader(bool shouldShowTwistHeader, bool isPagingForward)
	{
		TwistDetailsDisplayManager twistDisplayManager = TwistDetailsDisplayManager.Get();
		if (!shouldShowTwistHeader)
		{
			VisualsFormatType num = VisualsFormatTypeExtensions.ToVisualsFormatType(m_PreviousFormatType, m_PreviousInRankedPlayMode);
			VisualsFormatType newVisualsFormatType = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType();
			if (num != newVisualsFormatType)
			{
				twistDisplayManager.HideTwistHeaderPanelOnFormatChange();
			}
			else
			{
				twistDisplayManager.HideTwistHeaderPanel();
			}
		}
		else if (isPagingForward)
		{
			twistDisplayManager.ShowTwistHeaderPanelSlideRight();
		}
		else
		{
			twistDisplayManager.ShowTwistHeaderPanelSlideLeft();
		}
	}

	private IEnumerator ArrowDelayedActivate(UIBButton arrow, float delay)
	{
		yield return new WaitForSeconds(delay);
		arrow.gameObject.SetActive(value: true);
	}

	private bool ShouldHandleBoxTransition()
	{
		if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY)
		{
			return false;
		}
		if (Box.Get().IsBusy() || Box.Get().GetState() == Box.State.LOADING || Box.Get().GetState() == Box.State.LOADING_HUB)
		{
			return true;
		}
		return false;
	}

	private void OnBoxTransitionFinished(object userData)
	{
		if (m_randomDeckPickerTray != null && IsShowingFirstPage())
		{
			PositionHeroPickerPageAtStartingPos();
		}
		Box.Get().RemoveTransitionFinishedListener(OnBoxTransitionFinished);
	}

	private void OnScenePreUnload(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		HideSetRotationNotifications();
		SceneMgr.Get().UnregisterScenePreUnloadEvent(OnScenePreUnload);
	}

	private void LowerHeroButtons()
	{
		foreach (HeroPickerButton button in m_heroButtons)
		{
			if (button.gameObject.activeSelf)
			{
				button.Lower();
			}
		}
	}

	private void RaiseHeroButtons()
	{
		foreach (HeroPickerButton button in m_heroButtons)
		{
			if (button.gameObject.activeSelf)
			{
				button.Raise();
			}
		}
	}

	protected void SetKeyholeTextureOffsets(UnityEngine.Vector2 offset)
	{
		if (IsChoosingHero())
		{
			return;
		}
		int keyholeTextureIndex = (UniversalInputManager.UsePhoneUI ? 1 : 0);
		foreach (HeroPickerButton heroButton in m_heroButtons)
		{
			Renderer renderer = heroButton.m_buttonFrame.GetComponent<Renderer>();
			if (renderer == null)
			{
				Debug.LogWarning("Couldn't set keyhole texture offset on invalid renderer");
			}
			else
			{
				renderer.GetMaterial(keyholeTextureIndex).mainTextureOffset = offset;
			}
		}
	}

	private void HideHeroPickerPage()
	{
		m_randomDeckPickerTray.transform.localPosition = new Vector3(-5000f, -5000f, -5000f);
	}

	private void PositionHeroPickerPageAtStartingPos()
	{
		m_randomDeckPickerTray.transform.localPosition = m_randomDecksHiddenBone.transform.localPosition;
	}

	private void OnShowPreviousPage(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay("hero_panel_slide_on.prefab:236147a924d7cb442872b46dddd56132");
		ShowPreviousPage();
	}

	private void ShowHeroPickerPage(bool skipTraySlidingAnimation = false)
	{
		if (iTween.Count(m_randomDeckPickerTray) <= 0)
		{
			m_currentPageIndex = 0;
			if (m_modeLabelBg != null)
			{
				m_modeLabelBg.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
			}
			if (skipTraySlidingAnimation)
			{
				m_randomDeckPickerTray.transform.localPosition = m_randomDecksShownBone.transform.localPosition;
				RaiseHeroButtons();
			}
			else
			{
				PositionHeroPickerPageAtStartingPos();
				iTween.MoveTo(m_randomDeckPickerTray, iTween.Hash("time", 0.25f, "position", m_randomDecksShownBone.transform.localPosition, "islocal", true, "oncomplete", "RaiseHeroButtons", "oncompletetarget", base.gameObject));
			}
			UpdatePageArrows();
		}
	}

	private void OnCustomDeckPressed(CollectionDeckBoxVisual deckbox)
	{
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT && Options.GetInRankedPlayMode())
		{
			CollectionDeck deck = deckbox.GetCollectionDeck();
			if (deck == null)
			{
				return;
			}
			if (deck.FormatType == PegasusShared.FormatType.FT_STANDARD && Options.GetFormatType() == PegasusShared.FormatType.FT_TWIST)
			{
				ShowClickedStandardDeckInTwistPopup();
				return;
			}
			if (deck.FormatType == PegasusShared.FormatType.FT_WILD && Options.GetFormatType() == PegasusShared.FormatType.FT_TWIST)
			{
				ShowClickedWildDeckInTwistPopup();
				return;
			}
			if (deck.FormatType == PegasusShared.FormatType.FT_WILD && Options.GetFormatType() == PegasusShared.FormatType.FT_STANDARD)
			{
				ShowClickedWildDeckInStandardPopup();
				return;
			}
			if (deck.FormatType == PegasusShared.FormatType.FT_TWIST && Options.GetFormatType() == PegasusShared.FormatType.FT_WILD)
			{
				ShowClickedTwistDeckInWildPopup();
				return;
			}
			if (deck.FormatType == PegasusShared.FormatType.FT_TWIST && Options.GetFormatType() == PegasusShared.FormatType.FT_STANDARD)
			{
				ShowClickedTwistDeckInStandardPopup();
				return;
			}
		}
		if (SelectCustomDeck(deckbox))
		{
			CheckIfShouldShowFirstClassSelectionDialog(deckbox);
		}
		else
		{
			HandleClickToFixDeck(deckbox);
		}
	}

	private bool SelectCustomDeck(CollectionDeckBoxVisual deckbox)
	{
		if (!deckbox.CanSelectDeck())
		{
			return false;
		}
		HideDemoQuotes();
		SetPlayButtonEnabled(deckbox.IsDeckPlayable());
		RemoveHeroLockedTooltip();
		CollectionDeck deck = deckbox.GetCollectionDeck();
		if (deck != null && deck.IsDeckTemplate)
		{
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.RETURNING_PLAYER_EXPERIENCE, GameSaveKeySubkeyId.LAST_LOANER_DECK_SELECTED_TEMPLATE_ID, deckbox.GetDeckTemplateId()));
			Options.Get().SetLong(Option.LAST_CUSTOM_DECK_CHOSEN, 0L);
		}
		else
		{
			Options.Get().SetLong(Option.LAST_CUSTOM_DECK_CHOSEN, deckbox.GetDeckID());
		}
		deckbox.SetIsSelected(isSelected: true);
		if ((bool)AbsDeckPickerTrayDisplay.HIGHLIGHT_SELECTED_DECK)
		{
			deckbox.SetHighlightState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
		}
		else
		{
			deckbox.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
		}
		if (m_selectedCustomDeckBox != null && m_selectedCustomDeckBox != deckbox)
		{
			m_selectedCustomDeckBox.SetIsSelected(isSelected: false);
			m_selectedCustomDeckBox.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
		}
		m_selectedCustomDeckBox = deckbox;
		UpdateHeroInfo(deckbox);
		ShowPreconHero(show: true);
		bool enableHeroPortraitButtonEnable = true;
		if (m_selectedCustomDeckBox.GetFormatType() == PegasusShared.FormatType.FT_TWIST && RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
		{
			enableHeroPortraitButtonEnable = false;
		}
		EnableHeroPortraitButton(enableHeroPortraitButtonEnable);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_slidingTray.ToggleTraySlider(show: true);
		}
		LoanerDeckDisplay loanerDecksDisplay = LoanerDeckDisplay.Get();
		if (loanerDecksDisplay != null && deck != null)
		{
			loanerDecksDisplay.SetSelectedDeckInDataModel(deck.IsLoanerDeck);
		}
		m_heroActor.UpdateDeckRunesComponent(deck);
		if (m_formatConfig[VisualsFormatTypeExtensions.GetCurrentVisualsFormatType()].m_showDeckContentsForSelectedDeck && deck != null && deck.IsTwistHeroicDeck && RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
		{
			TwistHeroicDeckDataModel heroicDeckDataModel = new TwistHeroicDeckDataModel();
			GameUtils.FillTwistHeroicDeckModelWithDeckChoices(heroicDeckDataModel, deck);
			base.UpdateDeckDisplayDataForHeroicDecks(deck, heroicDeckDataModel);
		}
		return true;
	}

	private void EnableHeroPortraitButton(bool enable)
	{
		if (m_apprenticeHeroPortraitButton != null)
		{
			string eventName = (enable ? "CODE_ENABLE_PORTRAIT_BUTTON" : "CODE_DISABLE_PORTRAIT_BUTTON");
			m_apprenticeHeroPortraitButton.TriggerEvent(eventName, new TriggerEventParameters(null, null, noDownwardPropagation: true));
		}
	}

	private void HandleClickToFixDeck(CollectionDeckBoxVisual deckBox)
	{
		if (deckBox == null || !deckBox.IsDeckEnabled())
		{
			return;
		}
		CollectionDeck deck = deckBox.GetCollectionDeck();
		if (deck == null)
		{
			return;
		}
		DeckRuleset deckrules = deck.GetRuleset(null);
		if (deckrules != null && deckrules.EntityInDeckIgnoresRuleset(deck))
		{
			return;
		}
		PegasusShared.FormatType? format = deckBox.GetFormatTypeToValidateAgainst();
		CollectionDeck.CardCountByStatus cardCount = deck.CountCardsByStatus(format);
		if (cardCount.Extra > 0)
		{
			HandleExtraCards(deck, cardCount);
			return;
		}
		if (cardCount.MissingPlusInvalid > 0)
		{
			HandleMissingAndInvalidCards(deck, cardCount);
			return;
		}
		int invalidSideboardCardCount = deck.GetInvalidSideboardCardCount(format);
		if (invalidSideboardCardCount > 0)
		{
			HandleInvalidSideboardCards(invalidSideboardCardCount);
		}
	}

	private void HandleExtraCards(CollectionDeck deck, CollectionDeck.CardCountByStatus deckCardCount)
	{
		bool isClickToFixDeckEnabled = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().EnableClickToFixDeck;
		GameStrings.PluralNumber[] extraCardCountPlurals = GameStrings.MakePlurals(deckCardCount.Extra);
		AlertPopup.PopupInfo info;
		if (isClickToFixDeckEnabled)
		{
			AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
			popupInfo.m_showAlertIcon = false;
			popupInfo.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			popupInfo.m_confirmText = GameStrings.Get("GLOBAL_BUTTON_YES");
			popupInfo.m_cancelText = GameStrings.Get("GLOBAL_BUTTON_NO");
			popupInfo.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			popupInfo.m_headerText = GameStrings.FormatPlurals("GLUE_COLLECTION_DECK_EXTRA_CARDS_POPUP_HEADER", extraCardCountPlurals);
			popupInfo.m_text = GameStrings.Format("GLUE_COLLECTION_DECK_EXTRA_CARDS_POPUP_TEXT", deckCardCount.Extra);
			info = popupInfo;
			info.m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CONFIRM && !deck.IsSavingChanges())
				{
					deck.RemoveExtraCards(Options.GetFormatType());
					UpdateDeckVisualsAndSelectDeck(deck);
					deck.SendChanges(CollectionDeck.ChangeSource.ClickToFixExtraCards);
				}
			};
		}
		else
		{
			AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
			popupInfo.m_showAlertIcon = false;
			popupInfo.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			popupInfo.m_headerText = GameStrings.FormatPlurals("GLUE_COLLECTION_DECK_EXTRA_CARDS_POPUP_HEADER", extraCardCountPlurals);
			popupInfo.m_text = GameStrings.Format("GLUE_COLLECTION_DECK_EXTRA_CARDS_POPUP_TEXT_CLICK_TO_FIX_DISABLED", deckCardCount.Extra);
			info = popupInfo;
		}
		DialogManager.Get().ShowPopup(info);
	}

	private void HandleMissingAndInvalidCards(CollectionDeck deck, CollectionDeck.CardCountByStatus deckCardCount)
	{
		bool isClickToFixDeckEnabled = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().EnableClickToFixDeck;
		AlertPopup.PopupInfo info;
		if (deck.IsLoanerDeck)
		{
			GameStrings.MakePlurals(deckCardCount.MissingPlusInvalid);
			info = new AlertPopup.PopupInfo
			{
				m_showAlertIcon = false,
				m_alertTextAlignment = UberText.AlignmentOptions.Center,
				m_headerText = GameStrings.Get("GLUE_COLLECTION_DECK_INVALID_LOANER_DECK_HEADER"),
				m_text = GameStrings.Get("GLUE_COLLECTION_DECK_INVALID_LOANER_DECK_BODY")
			};
		}
		else if (isClickToFixDeckEnabled)
		{
			if (CollectionManager.Get().HasPendingSmartDeckRequest(deck.ID))
			{
				return;
			}
			info = new AlertPopup.PopupInfo
			{
				m_showAlertIcon = false,
				m_alertTextAlignment = UberText.AlignmentOptions.Center,
				m_confirmText = GameStrings.Get("GLOBAL_BUTTON_YES"),
				m_cancelText = GameStrings.Get("GLOBAL_BUTTON_NO"),
				m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
				m_headerText = GameStrings.Get("GLUE_COLLECTION_DECK_INCOMPLETE_POPUP_HEADER")
			};
			int nonApprenticeCardCount = deck.GetSlots().Count((CollectionDeckSlot slot) => RankMgr.Get().IsCardLockedInCurrentLeague(slot.GetEntityDef()));
			if (nonApprenticeCardCount > 0)
			{
				info.m_text = GameStrings.Format("GLUE_COLLECTION_DECK_INCOMPLETE_POPUP_TEXT_NPR", nonApprenticeCardCount);
			}
			else
			{
				info.m_text = GameStrings.Format("GLUE_COLLECTION_DECK_INCOMPLETE_POPUP_TEXT", deckCardCount.MissingPlusInvalid);
			}
			info.m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CONFIRM && !deck.IsSavingChanges())
				{
					deck.RemoveInvalidCards(Options.GetFormatType());
					CollectionManager.Get().AutoFillDeck(deck, allowSmartDeckCompletion: true, OnClickToFixAutoFillCallback);
				}
			};
		}
		else
		{
			GameStrings.PluralNumber[] notEnoughCardsPlurals = GameStrings.MakePlurals(deckCardCount.MissingPlusInvalid);
			AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
			popupInfo.m_showAlertIcon = false;
			popupInfo.m_alertTextAlignment = UberText.AlignmentOptions.Center;
			popupInfo.m_headerText = GameStrings.FormatPlurals("GLUE_COLLECTION_DECK_INCOMPLETE_POPUP_HEADER", notEnoughCardsPlurals);
			popupInfo.m_text = GameStrings.Format("GLUE_COLLECTION_DECK_INCOMPLETE_POPUP_TEXT_CLICK_TO_FIX_DISABLED", deckCardCount.MissingPlusInvalid);
			info = popupInfo;
		}
		DialogManager.Get().ShowPopup(info);
	}

	private void HandleInvalidSideboardCards(int cardCount)
	{
		GameStrings.PluralNumber[] plurals = GameStrings.MakePlurals(cardCount);
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_showAlertIcon = false,
			m_alertTextAlignment = UberText.AlignmentOptions.Center,
			m_confirmText = GameStrings.Get("GLOBAL_BUTTON_YES"),
			m_cancelText = GameStrings.Get("GLOBAL_BUTTON_NO"),
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_headerText = GameStrings.Get("GLUE_COLLECTION_DECK_INCOMPLETE_POPUP_HEADER"),
			m_text = GameStrings.FormatPlurals("GLUE_COLLECTION_DECK_INVALID_SIDEBOARD_POPUP_TEXT", plurals),
			m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CONFIRM)
				{
					NavigateToCollectionManager();
				}
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	private void OnClickToFixAutoFillCallback(CollectionDeck deck, IEnumerable<DeckMaker.DeckFill> fillCards)
	{
		if (deck == null)
		{
			return;
		}
		deck.FillFromCardList(fillCards, CollectionDeck.ChangeSource.ClickToFixMissingAndInvalidCards);
		foreach (SideboardDeck value in deck.GetAllSideboards().Values)
		{
			value.AutoFillSideboard();
		}
		UpdateDeckVisualsAndSelectDeck(deck);
	}

	private void UpdateDeckVisualsAndSelectDeck(CollectionDeck deck)
	{
		CustomDeckPage page = GetCurrentCustomPage();
		if (!(page == null))
		{
			page.UpdateDeckVisuals();
			CollectionDeckBoxVisual deckbox = page.FindDeckVisual(deck);
			if (!(deckbox == null) && SelectCustomDeck(deckbox) && !UniversalInputManager.UsePhoneUI)
			{
				deckbox.PlayGlowAnim();
			}
		}
	}

	protected override void OnHeroButtonReleased(UIEvent e)
	{
		base.OnHeroButtonReleased(e);
		HideDemoQuotes();
	}

	protected override void SelectHero(HeroPickerButton button, bool showTrayForPhone = true)
	{
		if (button == m_selectedHeroButton && !UniversalInputManager.UsePhoneUI)
		{
			return;
		}
		base.SelectHero(button, showTrayForPhone);
		Options.Get().SetInt(Option.LAST_PRECON_HERO_CHOSEN, (int)button.m_heroClass);
		if (button.IsLocked())
		{
			HeroPickerLockedStatus lockedReason = IsHeroPickerButtonLocked(button.m_heroClass);
			button.GetEntityDef().GetShortName();
			switch (lockedReason)
			{
			case HeroPickerLockedStatus.LOCKED_HERO_UNLOCK_NOT_DONE:
			{
				int level = GetApprenticeTrackLevelForHeroClassUnlock(button.m_heroClass);
				AddHeroLockedTooltip(GameStrings.Get("GLUE_HERO_LOCKED_NAME"), GameStrings.Format("GLUE_HERO_LOCKED_DESC", level), button.m_heroClass);
				break;
			}
			case HeroPickerLockedStatus.LOCKED_IN_TWIST_SCENARIO:
				AddHeroLockedTooltip(GameStrings.Get("GLUE_HERO_LOCKED_NAME"), GameStrings.Get("GLUE_EXCLUDED_IN_TWIST_SCENARIO"), lockedReason, button.m_heroClass);
				break;
			}
		}
	}

	private int GetApprenticeTrackLevelForHeroClassUnlock(TAG_CLASS heroClass)
	{
		List<RewardTrackDbfRecord> records = GameDbf.RewardTrack.GetRecords((RewardTrackDbfRecord record) => record.RewardTrackType == Assets.RewardTrack.RewardTrackType.APPRENTICE);
		RewardTrackDbfRecord apprenticeTrack = null;
		foreach (RewardTrackDbfRecord record2 in records)
		{
			if (apprenticeTrack == null)
			{
				apprenticeTrack = record2;
			}
			else if (record2.Version > apprenticeTrack.Version)
			{
				apprenticeTrack = record2;
			}
		}
		foreach (RewardTrackLevelDbfRecord record3 in GameUtils.GetRewardTrackLevelsForRewardTrack(apprenticeTrack.ID))
		{
			foreach (RewardItemDbfRecord reward in record3.FreeRewardListRecord.RewardItems)
			{
				if (reward.RewardType == RewardItem.RewardType.HERO_CLASS && reward.HeroClassId == (int)heroClass)
				{
					return record3.Level;
				}
			}
		}
		return -1;
	}

	protected void AddHeroLockedTooltip(string name, string description, HeroPickerLockedStatus lockReason, TAG_CLASS lockedClass = TAG_CLASS.INVALID)
	{
		AddHeroLockedTooltip(name, description, lockedClass);
	}

	private void Deselect()
	{
		if (m_selectedHeroButton == null && m_selectedCustomDeckBox == null)
		{
			return;
		}
		SetPlayButtonEnabled(enable: false);
		if (m_heroLockedTooltip != null)
		{
			UnityEngine.Object.DestroyImmediate(m_heroLockedTooltip.gameObject);
		}
		if (m_selectedCustomDeckBox != null)
		{
			m_selectedCustomDeckBox.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
			m_selectedCustomDeckBox.SetEnabled(enabled: true);
			m_selectedCustomDeckBox.SetIsSelected(isSelected: false);
			m_selectedCustomDeckBox = null;
		}
		EnableHeroPortraitButton(enable: false);
		m_heroActor.SetEntityDef(null);
		m_heroActor.SetCardDef(null);
		m_heroActor.Hide();
		if (m_selectedHeroButton != null)
		{
			m_selectedHeroButton.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
			m_selectedHeroButton.SetSelected(isSelected: false);
			m_selectedHeroButton = null;
		}
		if (ShouldShowHeroPower())
		{
			if (m_heroPowerActor != null)
			{
				m_heroPowerActor.SetCardDef(null);
				m_heroPowerActor.SetEntityDef(null);
				m_heroPowerActor.Hide();
			}
			if (m_goldenHeroPowerActor != null)
			{
				m_goldenHeroPowerActor.SetCardDef(null);
				m_goldenHeroPowerActor.SetEntityDef(null);
				m_goldenHeroPowerActor.Hide();
			}
			if (m_mythicHeroPowerActor != null)
			{
				m_mythicHeroPowerActor.SetCardDef(null);
				m_mythicHeroPowerActor.SetEntityDef(null);
				m_mythicHeroPowerActor.Hide();
			}
			if (m_heroPower != null)
			{
				m_heroPower.GetComponent<Collider>().enabled = false;
			}
			if (m_goldenHeroPower != null)
			{
				m_goldenHeroPower.GetComponent<Collider>().enabled = false;
			}
			if (m_heroPowerShadowQuad != null)
			{
				m_heroPowerShadowQuad.SetActive(value: false);
			}
		}
		m_selectedHeroPowerFullDef = null;
		if (m_heroPowerBigCard != null)
		{
			iTween.Stop(m_heroPowerBigCard.gameObject);
			m_heroPowerBigCard.Hide();
		}
		if (m_goldenHeroPowerBigCard != null)
		{
			iTween.Stop(m_goldenHeroPowerBigCard.gameObject);
			m_goldenHeroPowerBigCard.Hide();
		}
		m_selectedHeroName = null;
		m_heroName.Text = "";
		if (LoanerDeckDisplay.Get() != null)
		{
			LoanerDeckDisplay.Get().SetSelectedDeckInDataModel(isLoaner: false);
		}
		if (m_UnlockableDeckR2T != null)
		{
			m_UnlockableDeckR2T.enabled = false;
		}
	}

	private HeroPickerLockedStatus IsHeroPickerButtonLocked(TAG_CLASS heroClass)
	{
		if (!GameUtils.HasUnlockedClass(heroClass))
		{
			return HeroPickerLockedStatus.LOCKED_HERO_UNLOCK_NOT_DONE;
		}
		if (VisualsFormatTypeExtensions.GetCurrentVisualsFormatType() == VisualsFormatType.VFT_TWIST && RankMgr.IsClassLockedForTwist(heroClass))
		{
			return HeroPickerLockedStatus.LOCKED_IN_TWIST_SCENARIO;
		}
		return HeroPickerLockedStatus.UNLOCKED;
	}

	private void UpdateApprenticeProgressInfo()
	{
		if (!IsInApprentice || RewardTrackManager.Get() == null || !RewardTrackManager.Get().IsApprenticeTrackReady())
		{
			return;
		}
		if (m_apprenticeProgressWidget == null)
		{
			Log.All.PrintError("Unable to update Apprentice UI since it is missing");
			return;
		}
		if (m_rewardTrack == null)
		{
			m_rewardTrack = RewardTrackManager.Get().GetRewardTrack(Global.RewardTrackType.APPRENTICE);
			if (!m_rewardTrack.IsValid || m_rewardTrack.TrackDataModel == null)
			{
				Log.All.PrintError("Unable to update ranked display for apprentice when data isn't ready");
				return;
			}
		}
		m_rewardTrackDataModel = m_rewardTrack.TrackDataModel;
		m_apprenticeProgressWidget.BindDataModel(m_rewardTrack.TrackDataModel);
		int firstUnclaimedRewardLvl = 0;
		int currentLvl = m_rewardTrack.TrackDataModel.Level;
		for (int i = 1; i <= currentLvl; i++)
		{
			if (m_rewardTrack.HasUnclaimedRewardsForLevel(i))
			{
				firstUnclaimedRewardLvl = i;
				break;
			}
		}
		RewardTrackLevelDbfRecord rewardLvlToShow = ((firstUnclaimedRewardLvl > 0) ? m_rewardTrack.GetRewardTrackLevelRecord(firstUnclaimedRewardLvl) : m_rewardTrack.GetRewardTrackLevelRecord(currentLvl + 1));
		RewardTrackNodeRewardsDataModel nodeDM = null;
		if (rewardLvlToShow != null)
		{
			RewardListDbfRecord rewardListRecord = rewardLvlToShow.FreeRewardListRecord;
			DataModelList<RewardItemDataModel> rewardItemListDM = rewardListRecord.RewardItems.SelectMany((RewardItemDbfRecord r) => RewardFactory.CreateRewardItemDataModel(r)).OrderBy((RewardItemDataModel item) => item, new RewardUtils.RewardItemComparer()).ToDataModelList();
			bool hasClaimableReward = firstUnclaimedRewardLvl > 0;
			string levelUnlockDescription = (hasClaimableReward ? string.Empty : GameStrings.Format("GLUE_NEW_PLAYER_REWARD_PREVIEW_REQUIRED_LEVEL", rewardLvlToShow.Level));
			string rewardItemsDescription = BuildApprenticeRewardPopupRewardString(rewardListRecord, rewardItemListDM);
			RewardListDataModel rewardListDM = new RewardListDataModel
			{
				ChooseOne = rewardListRecord.ChooseOne,
				Description = rewardItemsDescription,
				Items = rewardItemListDM
			};
			nodeDM = new RewardTrackNodeRewardsDataModel
			{
				Level = rewardLvlToShow.Level,
				IsClaimable = hasClaimableReward,
				IsClaimed = false,
				IsPremium = false,
				PaidType = RewardTrackPaidType.RTPT_FREE,
				RewardTrackId = m_rewardTrack.RewardTrackId,
				RewardTrackType = (int)m_rewardTrack.TrackDataModel.RewardTrackType,
				Items = rewardListDM,
				Summary = levelUnlockDescription
			};
		}
		if (nodeDM != null)
		{
			m_apprenticeProgressWidget.BindDataModel(nodeDM);
		}
		else
		{
			m_apprenticeProgressWidget.UnbindDataModel(236);
		}
		if (m_apprenticeRewardPopupWidget != null)
		{
			if (nodeDM != null)
			{
				m_apprenticeRewardPopupWidget.BindDataModel(nodeDM);
			}
			else
			{
				m_apprenticeRewardPopupWidget.UnbindDataModel(236);
			}
		}
	}

	private string BuildApprenticeRewardPopupRewardString(RewardListDbfRecord rewardListRecord, DataModelList<RewardItemDataModel> rewardDMList)
	{
		if (rewardListRecord == null)
		{
			return string.Empty;
		}
		if (rewardListRecord.ChooseOne && rewardListRecord.RewardItems.Any((RewardItemDbfRecord r) => r.RewardType == RewardItem.RewardType.DECK))
		{
			return GameStrings.Format("GLUE_NEW_PLAYER_REWARD_PREVIEW_CHOOSE_DECK", 1, rewardListRecord.RewardItems.Count);
		}
		if (!rewardListRecord.RewardItems.Any((RewardItemDbfRecord r) => RewardUtils.IsAdditionalRewardType(r.RewardType)))
		{
			return rewardListRecord.Description?.GetString() ?? string.Empty;
		}
		string rewardString = string.Empty;
		string[] individualRewardStrings = rewardDMList.Select((RewardItemDataModel rewardDM) => GetRewardItemText(rewardDM)).ToArray();
		if (individualRewardStrings.Length == 1)
		{
			rewardString = individualRewardStrings[0];
		}
		else if (individualRewardStrings.Length == 2)
		{
			object[] args = individualRewardStrings;
			rewardString = GameStrings.Format("GLUE_NEW_PLAYER_REWARD_PREVIEW_TWO_ITEMS", args);
		}
		else if (individualRewardStrings.Length == 3)
		{
			object[] args = individualRewardStrings;
			rewardString = GameStrings.Format("GLUE_NEW_PLAYER_REWARD_PREVIEW_THREE_ITEMS", args);
		}
		else if (individualRewardStrings.Length == 4)
		{
			object[] args = individualRewardStrings;
			rewardString = GameStrings.Format("GLUE_NEW_PLAYER_REWARD_PREVIEW_FOUR_ITEMS", args);
		}
		else if (individualRewardStrings.Length > 4)
		{
			Log.All.PrintError($"Expected 4 or less reward items in list but encountered {individualRewardStrings.Length}");
			rewardString = string.Join(", ", individualRewardStrings);
		}
		if (string.IsNullOrEmpty(rewardString))
		{
			rewardString = rewardListRecord.Description?.GetString() ?? string.Empty;
		}
		return rewardString;
	}

	private string GetRewardItemText(RewardItemDataModel rewardItemDataModel)
	{
		switch (rewardItemDataModel.ItemType)
		{
		case RewardItemType.GOLD:
			return GameStrings.Get("GLUE_TOOLTIP_GOLD_HEADER");
		case RewardItemType.LOANER_DECKS:
			return GameStrings.Get("GLUE_LOANER_DECK_TITLE_PLURAL");
		case RewardItemType.HERO_CLASS:
		{
			string className = GameStrings.GetClassName((TAG_CLASS)rewardItemDataModel.HeroClassId);
			return GameStrings.Format("GLUE_NEW_PLAYER_REWARD_PREVIEW_CLASS_UNLOCK", className);
		}
		case RewardItemType.GAME_MODE:
			return GameStrings.GetGameModeName((RewardItem.UnlockableGameMode)rewardItemDataModel.GameModeId);
		case RewardItemType.ARENA_TICKET:
			return GameStrings.Get("GLOBAL_REWARD_ARENA_TICKET");
		default:
			return "UNKNOWN";
		}
	}

	private void UpdateHeroInfo(CollectionDeckBoxVisual deckBox)
	{
		using DefLoader.DisposableFullDef fullDef = deckBox.SharedDisposableFullDef();
		UpdateHeroInfo(fullDef, deckBox.GetDeckNameText().Text, deckBox.GetHeroCardPremium(), locked: false, deckBox.IsDeckUnlockable());
	}

	protected override void UpdateHeroInfo(HeroPickerButton button)
	{
		using DefLoader.DisposableFullDef fullDef = button.ShareFullDef();
		string heroShortName = fullDef.EntityDef.GetShortName();
		string heroName = ((!string.IsNullOrEmpty(heroShortName)) ? heroShortName : fullDef.EntityDef.GetName());
		TAG_PREMIUM heroPremium = CollectionManager.Get().GetHeroPremium(fullDef.EntityDef.GetClass());
		UpdateHeroInfo(fullDef, heroName, heroPremium, button.IsLocked());
	}

	private void UpdateHeroInfo(DefLoader.DisposableFullDef fullDef, string heroName, TAG_PREMIUM premium, bool locked = false, bool unlockedable = false)
	{
		if (fullDef == null)
		{
			Debug.LogWarningFormat("UpdateHeroInfo: fullDef is null for heroName {0}", heroName);
			return;
		}
		if (m_heroName != null)
		{
			m_heroName.Text = heroName;
		}
		if (fullDef.EntityDef != null)
		{
			m_selectedHeroName = fullDef.EntityDef.GetName();
		}
		if (m_heroActor == null)
		{
			Debug.LogWarningFormat("UpdateHeroInfo: m_heroActor is null for heroName {0}", heroName);
			return;
		}
		m_heroActor.SetPremium(premium);
		m_heroActor.SetFullDef(fullDef);
		m_heroActor.UpdateAllComponents();
		m_heroActor.SetUnlit();
		SetXPBarPositionAndScale(m_customFrameController);
		NetCache.HeroLevel heroLevel = (locked ? null : ((fullDef.EntityDef != null) ? GameUtils.GetHeroLevel(fullDef.EntityDef.GetClass()) : null));
		int totalLevel = GameUtils.GetTotalHeroLevel().GetValueOrDefault();
		m_xpBar.UpdateDisplay(heroLevel, totalLevel);
		if (Options.GetFormatType() == PegasusShared.FormatType.FT_TWIST && RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
		{
			NetCache.NetCacheFeatures features = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>();
			if (features != null)
			{
				NetCache.CardDefinition cardDef = new NetCache.CardDefinition
				{
					Name = ((fullDef.EntityDef != null) ? fullDef.EntityDef.GetCardId() : string.Empty),
					Premium = TAG_PREMIUM.NORMAL
				};
				if (features.TwistHeroicDeckHeroHealthOverrides.TryGetValue(cardDef, out var health) && fullDef.EntityDef != null)
				{
					fullDef.EntityDef.SetTag(GAME_TAG.HEALTH, health);
				}
			}
			m_heroActor.HealthDisplayRequiresHeroCard(required: false);
			m_heroActor.UpdateHealthTextMesh(fullDef.EntityDef);
			m_heroActor.GetHealthObject().Show();
			if ((bool)m_UnlockableDeckR2T)
			{
				m_UnlockableDeckR2T.enabled = unlockedable;
			}
			LegendarySkin.DynamicResolutionEnabled = !unlockedable && LegendarySkin_DynamicResolutionEnabled;
		}
		else
		{
			m_heroActor.HealthDisplayRequiresHeroCard(required: true);
			m_heroActor.GetHealthObject().Hide();
		}
		string heroPowerID = ((fullDef.EntityDef != null) ? GameUtils.GetHeroPowerCardIdFromHero(fullDef.EntityDef.GetCardId()) : string.Empty);
		if (!locked && ShouldShowHeroPower() && !string.IsNullOrEmpty(heroPowerID))
		{
			if (m_heroPowerContainer != null)
			{
				m_heroPowerContainer.SetActive(value: true);
			}
			if (m_heroPowerDefs != null)
			{
				m_heroPowerDefs.TryGetValue(heroPowerID, out var value);
				if (value == null)
				{
					LoadHeroPowerDef(heroPowerID, premium);
					m_heroPowerDefs.TryGetValue(heroPowerID, out value);
				}
				if (value != null)
				{
					UpdateHeroPowerInfo(fullDef, value, premium);
				}
			}
		}
		else
		{
			SetHeroPowerActorColliderEnabled(enable: false);
			HideHeroPowerActor();
			if (m_heroPowerContainer != null)
			{
				m_heroPowerContainer.SetActive(value: false);
			}
		}
		UpdateRankedClassWinsPlate();
	}

	protected override void TransitionToFormatType(PegasusShared.FormatType formatType, bool inRankedPlayMode, float transitionSpeed = 2f)
	{
		VisualsFormatType oldVisualsFormatType = VisualsFormatTypeExtensions.ToVisualsFormatType(m_PreviousFormatType, m_PreviousInRankedPlayMode);
		VisualsFormatType newVisualsFormatType = VisualsFormatTypeExtensions.ToVisualsFormatType(formatType, inRankedPlayMode);
		m_PreviousFormatType = formatType;
		m_PreviousInRankedPlayMode = inRankedPlayMode;
		base.TransitionToFormatType(formatType, inRankedPlayMode, transitionSpeed);
		UpdateTrayBackgroundTransitionValues(oldVisualsFormatType, newVisualsFormatType, transitionSpeed);
		if (!inRankedPlayMode)
		{
			ToggleApprenticeProgressUI(shouldEnable: true);
			if (m_rankedPlayDisplay != null)
			{
				m_rankedPlayDisplay.Hide(m_rankedPlayDisplayHideDelay);
			}
		}
		else
		{
			ToggleApprenticeProgressUI(shouldEnable: false);
			if (m_rankedPlayDisplay != null)
			{
				m_rankedPlayDisplay.Show(m_rankedPlayDisplayShowDelay);
				m_rankedPlayDisplay.OnSwitchFormat(newVisualsFormatType);
			}
		}
		UpdateValidHeroClasses();
		if (m_inHeroPicker && ((oldVisualsFormatType == VisualsFormatType.VFT_TWIST && newVisualsFormatType != VisualsFormatType.VFT_TWIST) || (oldVisualsFormatType != VisualsFormatType.VFT_TWIST && newVisualsFormatType == VisualsFormatType.VFT_TWIST)))
		{
			Deselect();
			StartCoroutine(LoadHeroButtons(null));
		}
		PlayTrayTransitionSound(newVisualsFormatType);
		PlayTrayTransitionGlowBursts(oldVisualsFormatType, newVisualsFormatType);
		if (ShouldShowHeroPower())
		{
			LoadHeroPower();
			LoadGoldenHeroPower();
		}
	}

	private void UpdateTrayBackgroundTransitionValues(VisualsFormatType oldVisualsFormatType, VisualsFormatType visualsFormatType, float transitionSpeed = 2f)
	{
		if (m_currentModeTextures != null)
		{
			float targetValue = 1f;
			Texture currentTexture = m_currentModeTextures.GetTextureForFormat(oldVisualsFormatType);
			Texture currentCustomTexture = m_currentModeTextures.GetCustomTextureForFormat(oldVisualsFormatType);
			Texture targetTexture = m_currentModeTextures.GetTextureForFormat(visualsFormatType);
			Texture targetCustomTexture = m_currentModeTextures.GetCustomTextureForFormat(visualsFormatType);
			SetCustomDeckPageTextures(currentCustomTexture, targetCustomTexture);
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				SetPhoneDetailsTrayTextures(currentTexture, targetTexture);
			}
			else
			{
				SetTrayFrameAndBasicDeckPageTextures(currentTexture, targetTexture);
			}
			StopCoroutine("TransitionTrayMaterial");
			StartCoroutine(TransitionTrayMaterial(targetValue, transitionSpeed));
		}
	}

	private void PlayTrayTransitionGlowBursts(VisualsFormatType oldVisualsFormatType, VisualsFormatType visualsFormatType)
	{
		if (oldVisualsFormatType == visualsFormatType)
		{
			return;
		}
		if (m_customPages != null && (oldVisualsFormatType == VisualsFormatType.VFT_WILD || visualsFormatType == VisualsFormatType.VFT_WILD))
		{
			bool useFX = oldVisualsFormatType == VisualsFormatType.VFT_WILD;
			bool hasValidStandardDecks = GetNumValidStandardDecks() != 0;
			if (m_customPages.Count > 1 && !IsShowingFirstPage())
			{
				m_customPages[1].PlayVineGlowBurst(useFX, hasValidStandardDecks);
			}
			else if (m_customPages.Count > 0)
			{
				if (hasValidStandardDecks)
				{
					GameObject[] customVineGlowToggle = m_customPages[0].m_customVineGlowToggle;
					for (int i = 0; i < customVineGlowToggle.Length; i++)
					{
						customVineGlowToggle[i].SetActive(value: true);
					}
				}
				m_customPages[0].PlayVineGlowBurst(useFX, hasValidStandardDecks);
			}
		}
		if (m_inHeroPicker)
		{
			PlayTransitionGlowBurstsForNewDeckFSMs(oldVisualsFormatType, visualsFormatType);
		}
		else
		{
			PlayTransitionGlowBurstsForNonNewDeckFSMs(oldVisualsFormatType, visualsFormatType);
		}
	}

	private void PlayTransitionGlowBurstsForNonNewDeckFSMs(VisualsFormatType oldVisualsFormatType, VisualsFormatType visualsFormatType)
	{
		string fxEvent = oldVisualsFormatType switch
		{
			VisualsFormatType.VFT_WILD => m_leavingWildGlowEvent, 
			VisualsFormatType.VFT_TWIST => m_leavingTwistGlowEvent, 
			VisualsFormatType.VFT_CASUAL => GameUtils.HasEarnedAllVanillaClassCards() ? m_leavingCasualGlowEvent : m_leavingCasualWithRewardsGlowEvent, 
			_ => null, 
		};
		if (!string.IsNullOrEmpty(fxEvent))
		{
			foreach (PlayMakerFSM fsm in formatChangeGlowFSMs)
			{
				if (fsm != null)
				{
					fsm.SendEvent(fxEvent);
				}
			}
			if (m_rankedPlayDisplay != null)
			{
				m_rankedPlayDisplay.PlayTransitionGlowBurstsForNonNewDeckFSMs(fxEvent);
			}
		}
		fxEvent = visualsFormatType switch
		{
			VisualsFormatType.VFT_WILD => m_enteringWildGlowEvent, 
			VisualsFormatType.VFT_TWIST => m_enteringTwistGlowEvent, 
			VisualsFormatType.VFT_CASUAL => GameUtils.HasEarnedAllVanillaClassCards() ? m_enteringCasualGlowEvent : m_enteringCasualWithRewardsGlowEvent, 
			_ => null, 
		};
		if (string.IsNullOrEmpty(fxEvent))
		{
			return;
		}
		foreach (PlayMakerFSM fsm2 in formatChangeGlowFSMs)
		{
			if (fsm2 != null)
			{
				fsm2.SendEvent(fxEvent);
			}
		}
		if (m_rankedPlayDisplay != null)
		{
			m_rankedPlayDisplay.PlayTransitionGlowBurstsForNonNewDeckFSMs(fxEvent);
		}
	}

	private void PlayTransitionGlowBurstsForNewDeckFSMs(VisualsFormatType oldVisualsFormatType, VisualsFormatType visualsFormatType)
	{
		string fxEvent = null;
		if (oldVisualsFormatType == VisualsFormatType.VFT_CLASSIC && visualsFormatType != VisualsFormatType.VFT_CLASSIC)
		{
			fxEvent = m_newDeckLeavingClassicGlowEvent;
		}
		else if (oldVisualsFormatType != VisualsFormatType.VFT_CLASSIC && visualsFormatType == VisualsFormatType.VFT_CLASSIC)
		{
			fxEvent = m_newDeckEnteringClassicGlowEvent;
		}
		if (oldVisualsFormatType == VisualsFormatType.VFT_TWIST && visualsFormatType != VisualsFormatType.VFT_TWIST)
		{
			fxEvent = m_newDeckLeavingClassicGlowEvent;
		}
		else if (oldVisualsFormatType != VisualsFormatType.VFT_TWIST && visualsFormatType == VisualsFormatType.VFT_TWIST)
		{
			fxEvent = m_newDeckEnteringClassicGlowEvent;
		}
		else if (oldVisualsFormatType == VisualsFormatType.VFT_WILD && visualsFormatType != VisualsFormatType.VFT_WILD)
		{
			fxEvent = m_newDeckLeavingWildGlowEvent;
		}
		else if (oldVisualsFormatType != VisualsFormatType.VFT_WILD && visualsFormatType == VisualsFormatType.VFT_WILD)
		{
			fxEvent = m_newDeckEnteringWildGlowEvent;
		}
		if (string.IsNullOrEmpty(fxEvent))
		{
			return;
		}
		foreach (PlayMakerFSM fsm in newDeckFormatChangeGlowFSMs)
		{
			if (fsm != null)
			{
				fsm.SendEvent(fxEvent);
			}
		}
		if (m_rankedPlayDisplay != null)
		{
			m_rankedPlayDisplay.PlayTransitionGlowBurstsForNewDeckFSMs(fxEvent);
		}
	}

	private void PlayTrayTransitionSound(VisualsFormatType visualsFormatType)
	{
		switch (Box.Get().GetState())
		{
		case Box.State.SET_ROTATION_OPEN:
			if (m_setRotationTutorialState == SetRotationTutorialState.PREPARING)
			{
				return;
			}
			break;
		case Box.State.LOADING:
			return;
		}
		string soundFilePath;
		switch (visualsFormatType)
		{
		case VisualsFormatType.VFT_STANDARD:
			soundFilePath = m_standardTransitionSound;
			break;
		case VisualsFormatType.VFT_WILD:
		case VisualsFormatType.VFT_CASUAL:
			soundFilePath = m_wildTransitionSound;
			break;
		case VisualsFormatType.VFT_CLASSIC:
			soundFilePath = m_classicTransitionSound;
			break;
		case VisualsFormatType.VFT_TWIST:
			soundFilePath = m_classicTransitionSound;
			break;
		default:
			Debug.LogError("No transition sound for format " + visualsFormatType);
			soundFilePath = "";
			break;
		}
		if (!string.IsNullOrEmpty(soundFilePath))
		{
			SoundManager.Get().LoadAndPlay(soundFilePath);
		}
	}

	private IEnumerator TransitionTrayMaterial(float targetValue, float speed)
	{
		Material detailsTrayMat = null;
		Material randomTrayMat = null;
		Material trayMat;
		float currentValue;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			trayMat = null;
			detailsTrayMat = m_detailsTrayFrame.GetComponent<Renderer>().GetSharedMaterial();
			randomTrayMat = m_basicDeckPage.GetComponent<Renderer>().GetSharedMaterial();
			currentValue = randomTrayMat.GetFloat("_Transistion");
		}
		else
		{
			trayMat = m_trayFrame.GetComponentInChildren<Renderer>().GetSharedMaterial();
			currentValue = trayMat.GetFloat("_Transistion");
			Renderer renderer = m_basicDeckPage.GetComponentInChildren<Renderer>();
			if (renderer != null)
			{
				randomTrayMat = renderer.GetSharedMaterial();
			}
		}
		do
		{
			currentValue = Mathf.MoveTowards(currentValue, targetValue, speed * Time.deltaTime);
			if (trayMat != null)
			{
				trayMat.SetFloat("_Transistion", currentValue);
			}
			if (detailsTrayMat != null)
			{
				detailsTrayMat.SetFloat("_Transistion", currentValue);
			}
			if (randomTrayMat != null)
			{
				randomTrayMat.SetFloat("_Transistion", currentValue);
			}
			if (m_customPages != null)
			{
				foreach (CustomDeckPage customPage in m_customPages)
				{
					customPage.UpdateTrayTransitionValue(currentValue);
				}
			}
			yield return null;
		}
		while (currentValue != targetValue);
	}

	private void SetTrayFrameAndBasicDeckPageTextures(Texture mainTexture, Texture transitionToTexture)
	{
		Material sharedMaterial = m_trayFrame.GetComponentInChildren<Renderer>().GetSharedMaterial();
		sharedMaterial.mainTexture = mainTexture;
		sharedMaterial.SetTexture("_MainTex2", transitionToTexture);
		sharedMaterial.SetFloat("_Transistion", 0f);
		Renderer renderer = m_basicDeckPage.GetComponentInChildren<Renderer>();
		if (renderer != null)
		{
			Material sharedMaterial2 = renderer.GetSharedMaterial();
			sharedMaterial2.mainTexture = mainTexture;
			sharedMaterial2.SetTexture("_MainTex2", transitionToTexture);
			sharedMaterial2.SetFloat("_Transistion", 0f);
		}
	}

	private void SetCustomDeckPageTextures(Texture transitionFromTexture, Texture targetTexture)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			Material sharedMaterial = m_basicDeckPage.GetComponent<Renderer>().GetSharedMaterial();
			sharedMaterial.mainTexture = transitionFromTexture;
			sharedMaterial.SetTexture("_MainTex2", targetTexture);
			sharedMaterial.SetFloat("_Transistion", 0f);
		}
		if (m_customPages == null)
		{
			return;
		}
		foreach (CustomDeckPage customPage in m_customPages)
		{
			customPage.SetTrayTextures(transitionFromTexture, targetTexture);
		}
	}

	private void SetPhoneDetailsTrayTextures(Texture transitionFromTexture, Texture targetTexture)
	{
		Material sharedMaterial = m_detailsTrayFrame.GetComponent<Renderer>().GetSharedMaterial();
		if (!m_slidingTray.IsShown() || m_slidingTray.IsAnimatingToShow())
		{
			sharedMaterial.mainTexture = targetTexture;
			sharedMaterial.SetTexture("_MainTex2", targetTexture);
			sharedMaterial.SetFloat("_Transistion", 0f);
		}
		else
		{
			sharedMaterial.mainTexture = transitionFromTexture;
			sharedMaterial.SetTexture("_MainTex2", targetTexture);
			sharedMaterial.SetFloat("_Transistion", 0f);
		}
	}

	private void OnRankedPlayDisplayWidgetReady()
	{
		m_rankedPlayDisplayWidget.transform.SetParent(m_rankedPlayDisplayWidgetBone, worldPositionStays: false);
		m_rankedPlayDisplay = m_rankedPlayDisplayWidget.GetComponentInChildren<RankedPlayDisplay>();
		VisualsFormatType currentVisualsFormatType = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType();
		UpdateRankedPlayDisplay(currentVisualsFormatType);
		StartCoroutine(SetRankedMedalWhenReady());
	}

	private void OnFormatTypePickerPopupReady()
	{
		m_formatTypePickerWidget.transform.SetParent(base.gameObject.transform);
		m_formatTypePickerWidget.RegisterEventListener(OnFormatTypePickerPopupEvent);
		m_formatTypePickerWidget.BindDataModel(TwistDetailsDisplayManager.TwistSeasonInfoModel);
		RankedPlaySeason rankedPlaySeason = RankMgr.Get()?.GetCurrentTwistSeason();
		if (rankedPlaySeason != null)
		{
			long latestTwistSeasonSeen = 0L;
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.LATEST_TWIST_SEASON_SEEN, out latestTwistSeasonSeen);
			TwistDetailsDisplayManager.TwistSeasonInfoModel.HasSeenTwistNewSeasonLabel = true;
			if (rankedPlaySeason.Season > latestTwistSeasonSeen)
			{
				TwistDetailsDisplayManager.TwistSeasonInfoModel.HasSeenTwistNewSeasonLabel = false;
			}
		}
		m_formatTypePickerWidget.RegisterEventListener(TwistDetailsDisplayManager.DismissNewSeasonLabel);
		m_formatTypePickerWidget.RegisterEventListener(TwistDetailsDisplayManager.DismissNewModeGlow);
		UpdateAvailableFormatOptions();
	}

	private string GetFormatPickerEvent()
	{
		bool num = CollectionManager.Get().ShouldAccountSeeStandardWild();
		bool isCreatingDeck = m_inHeroPicker;
		if (!num)
		{
			return "2BUTTONS";
		}
		if (isCreatingDeck)
		{
			if (!RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
			{
				return "3BUTTONS";
			}
			return "2BUTTONS_WILD";
		}
		return "4BUTTONS";
	}

	private void UpdateAvailableFormatOptions()
	{
		m_formatTypePickerWidget.TriggerEvent(GetFormatPickerEvent());
	}

	private void OnFormatTypePickerPopupEvent(string eventName)
	{
		switch (eventName)
		{
		case "WILD_BUTTON_CLICKED":
			SwitchFormatTypeAndRankedPlayMode(VisualsFormatType.VFT_WILD);
			break;
		case "STANDARD_BUTTON_CLICKED":
			SwitchFormatTypeAndRankedPlayMode(VisualsFormatType.VFT_STANDARD);
			break;
		case "CLASSIC_BUTTON_CLICKED":
			SwitchFormatTypeAndRankedPlayMode(VisualsFormatType.VFT_CLASSIC);
			break;
		case "TWIST_BUTTON_CLICKED":
			SwitchFormatTypeAndRankedPlayMode(VisualsFormatType.VFT_TWIST);
			break;
		case "CASUAL_BUTTON_CLICKED":
			SwitchFormatTypeAndRankedPlayMode(VisualsFormatType.VFT_CASUAL);
			break;
		case "HIDE":
			FireFormatTypePickerClosedEvent();
			break;
		}
	}

	private IEnumerator SetRankedMedalWhenReady()
	{
		while (TournamentDisplay.Get().GetCurrentMedalInfo() == null)
		{
			yield return null;
		}
		OnMedalChanged(TournamentDisplay.Get().GetCurrentMedalInfo());
		TournamentDisplay.Get().RegisterMedalChangedListener(OnMedalChanged);
	}

	private void OnMedalChanged(NetCache.NetCacheMedalInfo medalInfo)
	{
		m_rankedPlayDisplay.OnMedalChanged(medalInfo);
	}

	protected override void OnPlayGameButtonReleased(UIEvent e)
	{
		if (!SetRotationManager.Get().CheckForSetRotationRollover() && (PlayerMigrationManager.Get() == null || !PlayerMigrationManager.Get().CheckForPlayerMigrationRequired()))
		{
			HideDemoQuotes();
			HideSetRotationNotifications();
			TwistDetailsDisplayManager manager = TwistDetailsDisplayManager.Get();
			if (manager != null)
			{
				manager.HideTutorialPopups();
			}
			m_heroChosen = true;
			base.OnPlayGameButtonReleased(e);
		}
	}

	protected override void SetCollectionButtonEnabled(bool enable)
	{
		base.SetCollectionButtonEnabled(enable);
		UpdateCollectionButtonGlow();
	}

	private void UpdateCollectionButtonGlow()
	{
		if (ShouldGlowCollectionButton())
		{
			m_collectionButtonGlow.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
		}
		else
		{
			m_collectionButtonGlow.ChangeState(ActorStateType.HIGHLIGHT_OFF);
		}
	}

	private void InitSwitchFormatButton()
	{
		if (m_switchFormatButtonContainer == null || m_switchFormatButtonContainer.PrefabGameObject() == null)
		{
			return;
		}
		m_switchFormatButton = m_switchFormatButtonContainer.PrefabGameObject().GetComponent<SwitchFormatButton>();
		if (m_switchFormatButton == null)
		{
			return;
		}
		if (RankMgr.Get().IsNewPlayer())
		{
			m_switchFormatButton.Cover();
			m_switchFormatButton.Disable();
			Options.SetFormatType(PegasusShared.FormatType.FT_STANDARD);
			return;
		}
		PegasusShared.FormatType formatTypeToUse;
		bool inRankedPlayModeToUse;
		if (m_inHeroPicker)
		{
			formatTypeToUse = Options.GetFormatType();
			inRankedPlayModeToUse = true;
		}
		else
		{
			formatTypeToUse = Options.GetFormatType();
			inRankedPlayModeToUse = Options.GetInRankedPlayMode();
		}
		m_visualsFormatType = VisualsFormatTypeExtensions.ToVisualsFormatType(formatTypeToUse, inRankedPlayModeToUse);
		m_switchFormatButton.SetVisualsFormatType(m_visualsFormatType);
		m_switchFormatButton.AddEventListener(UIEventType.RELEASE, SwitchFormatButtonPress);
		switch (SceneMgr.Get().GetMode())
		{
		case SceneMgr.Mode.TOURNAMENT:
			m_switchFormatButton.Uncover();
			break;
		case SceneMgr.Mode.COLLECTIONMANAGER:
			if (CollectionManager.Get().AccountHasUnlockedWild())
			{
				m_switchFormatButton.Uncover();
				break;
			}
			m_switchFormatButton.Cover();
			m_switchFormatButton.Disable();
			break;
		case SceneMgr.Mode.FRIENDLY:
		case SceneMgr.Mode.ADVENTURE:
		case SceneMgr.Mode.TAVERN_BRAWL:
			m_switchFormatButton.Cover();
			m_switchFormatButton.Disable();
			break;
		}
	}

	protected override void ShowHero()
	{
		if (m_selectedHeroButton != null)
		{
			UpdateHeroInfo(m_selectedHeroButton);
		}
		else
		{
			if (!(m_selectedCustomDeckBox != null))
			{
				Log.All.PrintError("DeckPickerTrayDisplay.ShowHero with no button or deck box selected!");
				return;
			}
			UpdateHeroInfo(m_selectedCustomDeckBox);
		}
		base.ShowHero();
		SetLockedPortraitMaterial(m_selectedHeroButton);
	}

	protected override void SetHeroRaised(bool raised)
	{
		m_xpBar.SetEnabled(raised);
		base.SetHeroRaised(raised);
	}

	private void HideAllPreconHighlights()
	{
		foreach (HeroPickerButton heroButton in m_heroButtons)
		{
			heroButton.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
		}
	}

	protected override void PlayGame()
	{
		base.PlayGame();
		switch (SceneMgr.Get().GetMode())
		{
		case SceneMgr.Mode.TOURNAMENT:
		{
			if (BlockOnInvalidDeckHero())
			{
				return;
			}
			long selectedDeckId = GetSelectedDeckID();
			if (GetSelectedDeckID() == 0L && GetSelectedDeckTemplateID() == 0)
			{
				Debug.LogError("Trying to play game with deck ID 0!");
				return;
			}
			SetBackButtonEnabled(enable: false);
			PegasusShared.GameType gameType = GetGameTypeForNewPlayModeGame();
			PegasusShared.FormatType formatType = GetFormatTypeForNewPlayModeGame();
			Options.Get().SetEnum(Option.FORMAT_TYPE_LAST_PLAYED, formatType);
			if (HandleMysteriousDeck(gameType, selectedDeckId))
			{
				break;
			}
			int scenarioId = 2;
			if (formatType == PegasusShared.FormatType.FT_TWIST)
			{
				RankedPlaySeason twistSeason = RankMgr.Get()?.GetCurrentTwistSeason();
				if (twistSeason != null)
				{
					scenarioId = twistSeason.GetScenario()?.ID ?? scenarioId;
				}
				if (RankMgr.IsCurrentTwistSeasonUsingHeroicDecks() && !UniversalInputManager.UsePhoneUI)
				{
					TwistDetailsDisplayManager manager = TwistDetailsDisplayManager.Get();
					if (manager != null)
					{
						manager.ToggleTwistDeckDisplayAnimationStatePC(shouldBeOpen: false);
					}
				}
			}
			int deckTemplateId2 = m_selectedCustomDeckBox.GetDeckTemplateId();
			if (deckTemplateId2 == 0)
			{
				GameMgr.Get().FindGame(gameType, formatType, scenarioId, 0, selectedDeckId, null, null, restoreSavedGameState: false, null, null, 0L);
			}
			else
			{
				GameMgr gameMgr2 = GameMgr.Get();
				int missionId = scenarioId;
				long deckId2 = 0L;
				int deckTemplateId = deckTemplateId2;
				gameMgr2.FindGame(gameType, formatType, missionId, 0, deckId2, null, null, restoreSavedGameState: false, null, null, 0L, PegasusShared.GameType.GT_UNKNOWN, deckTemplateId);
			}
			bool publishMatchmakingStatus = true;
			if (gameType == PegasusShared.GameType.GT_RANKED && RankMgr.Get().IsLegendRank(formatType))
			{
				publishMatchmakingStatus = false;
			}
			if (publishMatchmakingStatus)
			{
				PresenceMgr.Get().SetStatus(Global.PresenceStatus.PLAY_QUEUE);
			}
			break;
		}
		case SceneMgr.Mode.FRIENDLY:
			if (!FriendChallengeMgr.Get().IsChallengeTavernBrawl())
			{
				if (BlockOnInvalidDeckHero())
				{
					return;
				}
				long myDeckID2 = GetSelectedDeckID();
				if (myDeckID2 == 0L)
				{
					Debug.LogError("Trying to play friendly game with deck ID 0!");
					return;
				}
				FriendChallengeMgr.Get().SelectDeck(myDeckID2);
				FriendlyChallengeHelper.Get().StartChallengeOrWaitForOpponent("GLOBAL_FRIEND_CHALLENGE_OPPONENT_WAITING_DECK", OnFriendChallengeWaitingForOpponentDialogResponse);
				break;
			}
			goto case SceneMgr.Mode.TAVERN_BRAWL;
		case SceneMgr.Mode.ADVENTURE:
		{
			long myDeckID = GetSelectedDeckID();
			AdventureConfig ac = AdventureConfig.Get();
			if (ac.GetSelectedAdventure() == AdventureDbId.NAXXRAMAS && !Options.Get().GetBool(Option.HAS_PLAYED_NAXX))
			{
				AdTrackingManager.Get().TrackAdventureProgress(Option.HAS_PLAYED_NAXX.ToString());
				Options.Get().SetBool(Option.HAS_PLAYED_NAXX, val: true);
			}
			switch (ac.CurrentSubScene)
			{
			case AdventureData.Adventuresubscene.PRACTICE:
				if (BlockOnInvalidDeckHero())
				{
					return;
				}
				PracticePickerTrayDisplay.Get().Show();
				SetHeroRaised(raised: false);
				break;
			case AdventureData.Adventuresubscene.MISSION_DECK_PICKER:
			{
				if (OnPlayButtonPressed_SaveHeroAndAdvanceToDungeonRunIfNecessary())
				{
					break;
				}
				int selectedHeroCard = 0;
				if (m_selectedHeroButton != null && m_selectedHeroButton.m_heroClass != 0)
				{
					selectedHeroCard = GameUtils.GetFavoriteHeroCardDBIdFromClass(m_selectedHeroButton.m_heroClass);
				}
				ScenarioDbId mission = ac.GetMissionToPlay();
				if (GameDbf.Scenario.GetRecord((int)mission).RuleType == Scenario.RuleType.CHOOSE_HERO)
				{
					GameMgr.Get().FindGameWithHero(PegasusShared.GameType.GT_VS_AI, PegasusShared.FormatType.FT_WILD, (int)mission, 0, selectedHeroCard, 0L);
					break;
				}
				int loanerDeckTemplateId = m_selectedCustomDeckBox.GetDeckTemplateId();
				if (loanerDeckTemplateId == 0)
				{
					GameMgr.Get().FindGame(PegasusShared.GameType.GT_VS_AI, PegasusShared.FormatType.FT_WILD, (int)mission, 0, myDeckID, null, null, restoreSavedGameState: false, null, null, 0L);
					break;
				}
				GameMgr gameMgr = GameMgr.Get();
				long deckId = 0L;
				int deckTemplateId = loanerDeckTemplateId;
				gameMgr.FindGame(PegasusShared.GameType.GT_VS_AI, PegasusShared.FormatType.FT_WILD, (int)mission, 0, deckId, null, null, restoreSavedGameState: false, null, null, 0L, PegasusShared.GameType.GT_UNKNOWN, deckTemplateId);
				break;
			}
			}
			break;
		}
		case SceneMgr.Mode.TAVERN_BRAWL:
			if (!TavernBrawlManager.Get().SelectHeroBeforeMission())
			{
				SelectHeroForCollectionManager();
			}
			break;
		case SceneMgr.Mode.COLLECTIONMANAGER:
			SelectHeroForCollectionManager();
			break;
		}
		bool isPractice = SceneMgr.Get().GetMode() == SceneMgr.Mode.ADVENTURE && AdventureConfig.Get().CurrentSubScene == AdventureData.Adventuresubscene.PRACTICE;
		if ((bool)UniversalInputManager.UsePhoneUI && !isPractice)
		{
			m_slidingTray.ToggleTraySlider(show: false);
		}
	}

	private bool BlockOnInvalidDeckHero()
	{
		if (!GameUtils.IsCardGameplayEventActive(m_selectedCustomDeckBox.GetHeroCardID()))
		{
			DialogManager.Get().ShowClassUpcomingPopup();
			return true;
		}
		return false;
	}

	private void HashCombine(ref long seed, long val)
	{
		seed ^= val + 2654435769u + (seed << 6) + (seed >> 2);
	}

	private bool TryRedirectToMysteryScenario(CollectionDeck deck, long currentDeckHash, int currentDeckLength, long deckTargetHash, int scenarioId)
	{
		if (deck == null)
		{
			return false;
		}
		if (currentDeckLength != deck.GetTotalDeckSizeIncludingSideboards())
		{
			return false;
		}
		if (currentDeckHash != deckTargetHash)
		{
			return false;
		}
		ScenarioDbfRecord scenario = GameDbf.Scenario.GetRecord(scenarioId);
		if (scenario == null)
		{
			return false;
		}
		GameMgr.Get().FindGameWithHero(PegasusShared.GameType.GT_VS_AI, PegasusShared.FormatType.FT_WILD, scenarioId, 0, scenario.Player1HeroCardId, scenario.Player1DeckId);
		return true;
	}

	private long ComputeCurrentDeckHash(CollectionDeck deck)
	{
		if (deck == null)
		{
			return 0L;
		}
		List<int> deckList = new List<int>();
		foreach (CollectionDeckSlot slot in deck.GetSlots())
		{
			if (slot != null)
			{
				int cardDbid = GameUtils.TranslateCardIdToDbId(slot.CardID);
				for (int i = 0; i < slot.Count; i++)
				{
					deckList.Add(cardDbid);
				}
			}
		}
		foreach (KeyValuePair<string, SideboardDeck> sideboardPair in deck.GetAllSideboards())
		{
			_ = sideboardPair.Key;
			foreach (CollectionDeckSlot slot2 in sideboardPair.Value.GetSlots())
			{
				if (slot2 != null)
				{
					int cardDbid2 = GameUtils.TranslateCardIdToDbId(slot2.CardID);
					for (int j = 0; j < slot2.Count; j++)
					{
						deckList.Add(cardDbid2);
					}
				}
			}
		}
		deckList.Sort();
		long deckHash = 0L;
		foreach (int cardDbid3 in deckList)
		{
			HashCombine(ref deckHash, cardDbid3);
		}
		return deckHash;
	}

	private bool HandleMysteriousDeck(PegasusShared.GameType gameType, long deckId)
	{
		if (gameType != PegasusShared.GameType.GT_RANKED)
		{
			return false;
		}
		CollectionDeck deck = CollectionManager.Get().GetDeck(deckId);
		if (deck == null)
		{
			return false;
		}
		long currentDeckHash = ComputeCurrentDeckHash(deck);
		if (TryRedirectToMysteryScenario(deck, currentDeckHash, 30, -7487123274682292298L, 5026))
		{
			return true;
		}
		if (TryRedirectToMysteryScenario(deck, currentDeckHash, 43, 4901740154402535512L, 5396))
		{
			return true;
		}
		return false;
	}

	private void SelectHeroForCollectionManager()
	{
		if (m_selectedHeroButton == null)
		{
			Debug.LogError("DeckPickerTrayDisplay.SelectHeroForCollectionManager called when m_selectedHeroButton was null");
			return;
		}
		m_backButton.RemoveEventListener(UIEventType.RELEASE, OnBackButtonReleased);
		Navigation.RemoveHandler(OnNavigateBack);
		if (s_selectHeroCoroutine != null)
		{
			Processor.CancelCoroutine(s_selectHeroCoroutine);
		}
		s_selectHeroCoroutine = Processor.RunCoroutine(SelectHeroForCollectionManagerImpl(m_selectedHeroButton.GetEntityDef()));
	}

	private static IEnumerator SelectHeroForCollectionManagerImpl(EntityDef heroDef)
	{
		PegasusShared.FormatType ft = Options.GetFormatType();
		if (ft == PegasusShared.FormatType.FT_UNKNOWN)
		{
			RankMgr.LogMessage("Options.GetFormatType() = FT_UNKOWN", "SelectHeroForCollectionManagerImpl", "D:\\p4Workspace\\31.6.0\\Pegasus\\Client\\Assets\\Game\\DeckPickerTray\\DeckPickerTrayDisplay.cs", 5656);
			yield break;
		}
		CollectionManager.s_HeroPickerFormat = ft;
		DeckTrayDeckListContent.s_HeroPickerFormat = ft;
		if (HeroPickerDisplay.Get() != null)
		{
			HeroPickerDisplay.Get().HideTray(UniversalInputManager.UsePhoneUI ? 0.25f : 0f);
		}
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		DeckTrayDeckListContent decksContent = deckTray.GetDecksContent();
		if (SceneMgr.Get().IsInTavernBrawlMode())
		{
			decksContent.CreateNewDeckFromUserSelection(heroDef.GetClass(), heroDef.GetCardId());
			CollectionManager.Get().GetCollectibleDisplay().EnableInput(enable: true);
			yield break;
		}
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		DeckTemplatePicker deckTemplatePicker = null;
		if (CollectionManager.Get().GetNonStarterTemplateDecks(Options.GetFormatType(), heroDef.GetClass()).Count > 0)
		{
			deckTemplatePicker = (UniversalInputManager.UsePhoneUI ? cmd.GetPhoneDeckTemplateTray() : cmd.m_pageManager.GetDeckTemplatePicker());
		}
		if (deckTemplatePicker != null && (bool)UniversalInputManager.UsePhoneUI)
		{
			deckTemplatePicker.m_phoneBackButton.SetEnabled(enabled: false);
		}
		if (deckTemplatePicker != null)
		{
			deckTemplatePicker.CurrentSelectedFormat = ft;
		}
		deckTray.m_doneButton.SetEnabled(enabled: false);
		while (deckTray.IsUpdatingTrayMode() || decksContent.NumDecksToDelete() > 0 || deckTray.IsWaitingToDeleteDeck())
		{
			yield return null;
		}
		decksContent.CreateNewDeckFromUserSelection(heroDef.GetClass(), heroDef.GetCardId());
		while (deckTemplatePicker != null && !deckTemplatePicker.IsShowingPacks())
		{
			yield return null;
		}
		if (CollectionManager.Get().GetCollectibleDisplay() != null)
		{
			CollectionManager.Get().GetCollectibleDisplay().EnableInput(enable: true);
		}
		while (deckTray != null && deckTray.IsUpdatingTrayMode())
		{
			yield return null;
		}
		if (deckTemplatePicker != null && (bool)UniversalInputManager.UsePhoneUI)
		{
			deckTemplatePicker.m_phoneBackButton.SetEnabled(enabled: true);
		}
		if (deckTray != null)
		{
			deckTray.m_doneButton.SetEnabled(enabled: true);
		}
	}

	protected override void OnSlidingTrayToggled(bool isShowing)
	{
		base.OnSlidingTrayToggled(isShowing);
		if (isShowing)
		{
			TransitionToFormatType(Options.GetFormatType(), Options.GetInRankedPlayMode());
		}
	}

	protected override IEnumerator InitModeWhenReady()
	{
		while ((ShouldLoadCustomDecks() && !CustomPagesReady()) || (SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT && m_rankedPlayDisplay == null))
		{
			if (!SceneMgr.Get().DoesCurrentSceneSupportOfflineActivity() && !Network.IsLoggedIn())
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
				yield break;
			}
			yield return null;
		}
		if (!IsChoosingHero())
		{
			while (!NetCache.Get().IsNetObjectAvailable<NetCache.NetCacheDecks>())
			{
				yield return null;
			}
		}
		yield return StartCoroutine(base.InitModeWhenReady());
		InitMode();
		while (LoadingScreen.Get().IsTransitioning())
		{
			yield return null;
		}
		BoxRailroadManager railroadManager = Box.Get().GetRailroadManager();
		if (railroadManager != null && railroadManager.ShouldLinkToJournal())
		{
			railroadManager.ShowApprenticeTrack();
			BackOutToHub();
		}
		else
		{
			StartCoroutine(ShowIntroPopupsIfNeeded_Routine());
		}
	}

	private IEnumerator ShowIntroPopupsIfNeeded_Routine(float delay = 0f)
	{
		if (delay > 0f)
		{
			yield return new WaitForSeconds(delay);
		}
		if (IsInApprentice)
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_DECK_DIFFERENCES_DIALOGUE, out long hasSeenArchetypesIntro);
			if (hasSeenArchetypesIntro < 1)
			{
				bool waitingForPopup = true;
				DialogManager.Get().ShowDeckArchetypesIntroPopup(delegate
				{
					waitingForPopup = false;
				});
				while (waitingForPopup)
				{
					yield return null;
				}
			}
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_REWARD_TRACK_INFOGRAPHIC, out long hasSeenRewardTrackInfographic);
			if (hasSeenRewardTrackInfographic < 1)
			{
				RewardTrackManager rewardTrackManager = RewardTrackManager.Get();
				if (rewardTrackManager != null && rewardTrackManager.GetApprenticeTrackLevel() >= 3)
				{
					bool waitingForPopup2 = true;
					DialogManager.Get().ShowRewardTrackInfographic(delegate
					{
						waitingForPopup2 = false;
					});
					while (waitingForPopup2)
					{
						yield return null;
					}
				}
			}
		}
		while (!UserAttentionManager.CanShowAttentionGrabber(UserAttentionBlocker.SET_ROTATION_INTRO, "ShowIntroPopupsIfNeeded"))
		{
			yield return null;
		}
		if (PopupDisplayManager.CanShowRankedIntroForNewPlayer())
		{
			ShowRankedIntroPopup();
		}
		else if (ShouldShowBonusStarsPopUp())
		{
			ShowBonusStarsPopup();
		}
		else
		{
			PlayEnterModeDialogues();
		}
	}

	private bool CustomPagesReady()
	{
		if (m_customPages == null)
		{
			return false;
		}
		foreach (CustomDeckPage page in m_customPages)
		{
			if (page == null || !page.PageReady())
			{
				return false;
			}
		}
		return true;
	}

	private CustomDeckPage GetCurrentCustomPage()
	{
		if (m_currentPageIndex < m_customPages.Count)
		{
			return m_customPages[m_currentPageIndex];
		}
		return null;
	}

	protected override void InitRichPresence(Global.PresenceStatus? newStatus = null)
	{
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT)
		{
			newStatus = Global.PresenceStatus.PLAY_DECKPICKER;
		}
		base.InitRichPresence(newStatus);
	}

	private void SetSelectionAndPageFromOptions()
	{
		if (HasNewRewardedDeck(out var deckId))
		{
			RewardUtils.MarkNewestRewardedDeckAsSeen();
		}
		else
		{
			deckId = GetLastChosenDeckId();
		}
		int pageNum;
		CollectionDeckBoxVisual deckBoxToSelect = GetDeckboxWithDeckID(deckId, out pageNum);
		if (deckBoxToSelect == null && Options.GetFormatType() == PegasusShared.FormatType.FT_TWIST && RankMgr.IsCurrentTwistSeasonUsingHeroicDecks() && !UniversalInputManager.UsePhoneUI)
		{
			long lastHeroicDeckSeenTemplateId = 0L;
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.RETURNING_PLAYER_EXPERIENCE, GameSaveKeySubkeyId.LAST_LOANER_DECK_SELECTED_TEMPLATE_ID, out lastHeroicDeckSeenTemplateId);
			if (lastHeroicDeckSeenTemplateId != 0L)
			{
				deckBoxToSelect = GetDeckBoxWithDeckTemplateId(lastHeroicDeckSeenTemplateId, out pageNum);
			}
		}
		FreeDeckMgr.FreeDeckStatus freeDeckStatus = FreeDeckMgr.Get().Status;
		if (deckBoxToSelect == null && freeDeckStatus == FreeDeckMgr.FreeDeckStatus.TRIAL_PERIOD && (Options.GetFormatType() != PegasusShared.FormatType.FT_TWIST || !UniversalInputManager.UsePhoneUI))
		{
			long lastLoanerDeckSeenTemplateId = 0L;
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.RETURNING_PLAYER_EXPERIENCE, GameSaveKeySubkeyId.LAST_LOANER_DECK_SELECTED_TEMPLATE_ID, out lastLoanerDeckSeenTemplateId);
			if (lastLoanerDeckSeenTemplateId != 0L)
			{
				deckBoxToSelect = GetDeckBoxWithDeckTemplateId(lastLoanerDeckSeenTemplateId, out pageNum);
			}
		}
		long hasSeenLoanerDecksOnOnFirstLogin = 0L;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.RETURNING_PLAYER_EXPERIENCE, GameSaveKeySubkeyId.HAS_SEEN_LOANER_DECKS_ON_FIRST_LOGIN_TRIAL_START, out hasSeenLoanerDecksOnOnFirstLogin);
		if (freeDeckStatus == FreeDeckMgr.FreeDeckStatus.TRIAL_PERIOD && hasSeenLoanerDecksOnOnFirstLogin <= 0)
		{
			deckBoxToSelect = null;
			pageNum = 0;
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.RETURNING_PLAYER_EXPERIENCE, GameSaveKeySubkeyId.HAS_SEEN_LOANER_DECKS_ON_FIRST_LOGIN_TRIAL_START, 1L));
		}
		long hasSeenTwistFirstTime = 0L;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_TWIST_FIRST_TIME, out hasSeenTwistFirstTime);
		if (VisualsFormatTypeExtensions.GetCurrentVisualsFormatType() == VisualsFormatType.VFT_TWIST && hasSeenTwistFirstTime <= 0)
		{
			pageNum = 0;
			if (freeDeckStatus == FreeDeckMgr.FreeDeckStatus.TRIAL_PERIOD)
			{
				pageNum++;
			}
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_TWIST_FIRST_TIME, 1L));
		}
		ShowPage(pageNum, skipTraySlidingAnimation: true);
		bool skipSettingSelection = false;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			skipSettingSelection = SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.GAMEPLAY;
		}
		if (!skipSettingSelection && deckBoxToSelect != null)
		{
			SelectCustomDeck(deckBoxToSelect);
		}
	}

	private bool HasNewRewardedDeck(out long deckId)
	{
		bool result = RewardUtils.HasNewRewardedDeck(out deckId);
		if (result && !HasValidDeckboxWithId(deckId))
		{
			Log.DeckTray.PrintWarning("HasNewRewardedDeckId - Newest rewarded deck ID option was set to an invalid deck ID: {0}", deckId);
			return false;
		}
		return result;
	}

	private bool HasValidDeckboxWithId(long deckId)
	{
		return GetDeckboxWithDeckID(deckId) != null;
	}

	private long GetLastChosenDeckId()
	{
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.FRIENDLY)
		{
			return Options.Get().GetLong(Option.LAST_CUSTOM_DECK_CHOSEN);
		}
		return 0L;
	}

	private CollectionDeckBoxVisual GetDeckboxWithDeckID(long deckID)
	{
		int pageNum;
		return GetDeckboxWithDeckID(deckID, out pageNum);
	}

	private CollectionDeckBoxVisual GetDeckboxWithDeckID(long deckID, out int pageNum)
	{
		for (pageNum = 0; pageNum < m_customPages.Count; pageNum++)
		{
			CollectionDeckBoxVisual deckBox = m_customPages[pageNum].GetDeckboxWithDeckID(deckID);
			if (deckBox != null)
			{
				return deckBox;
			}
		}
		pageNum = 0;
		return null;
	}

	private CollectionDeckBoxVisual GetDeckBoxWithDeckTemplateId(long deckTemplateId, out int pageNum)
	{
		for (pageNum = 0; pageNum < m_customPages.Count; pageNum++)
		{
			CustomDeckPage page = m_customPages[pageNum];
			if (page.DeckPageType == CustomDeckPage.PageType.LOANER_DECK_DISPLAY || page.DeckPageType == CustomDeckPage.PageType.PRECON_DECK_DISPLAY)
			{
				CollectionDeckBoxVisual deckBox = page.GetDeckboxWithDeckTemplateID(deckTemplateId);
				if (deckBox != null)
				{
					return deckBox;
				}
			}
		}
		pageNum = 0;
		return null;
	}

	private CollectionDeckBoxVisual GetFirstDeckbox()
	{
		return m_customPages[0].GetFirstDeckbox();
	}

	protected override void OnFriendChallengeWaitingForOpponentDialogResponse(AlertPopup.Response response, object userData)
	{
		if (response == AlertPopup.Response.CANCEL && !FriendChallengeMgr.Get().AmIInGameState())
		{
			Deselect();
			base.OnFriendChallengeWaitingForOpponentDialogResponse(response, userData);
		}
	}

	protected override void OnFriendChallengeChanged(FriendChallengeEvent challengeEvent, BnetPlayer player, FriendlyChallengeData challengeData, object userData)
	{
		base.OnFriendChallengeChanged(challengeEvent, player, challengeData, userData);
		switch (challengeEvent)
		{
		case FriendChallengeEvent.I_RECEIVED_SHARED_DECKS:
			UseSharedDecks(FriendChallengeMgr.Get().GetSharedDecks());
			break;
		case FriendChallengeEvent.I_ENDED_DECK_SHARE:
			StopUsingSharedDecks();
			break;
		case FriendChallengeEvent.I_CANCELED_DECK_SHARE_REQUEST:
			OnDeckShareRequestCancelDeclineOrError();
			break;
		case FriendChallengeEvent.OPPONENT_DECLINED_DECK_SHARE_REQUEST:
			OnDeckShareRequestCancelDeclineOrError();
			break;
		case FriendChallengeEvent.DECK_SHARE_ERROR_OCCURED:
			OnDeckShareRequestCancelDeclineOrError();
			break;
		case FriendChallengeEvent.I_ACCEPTED_DECK_SHARE_REQUEST:
		case FriendChallengeEvent.I_DECLINED_DECK_SHARE_REQUEST:
			if (FriendChallengeMgr.Get().DidISelectDeckOrHero())
			{
				FriendlyChallengeHelper.Get().StartChallengeOrWaitForOpponent("GLOBAL_FRIEND_CHALLENGE_OPPONENT_WAITING_DECK", OnFriendChallengeWaitingForOpponentDialogResponse);
			}
			break;
		}
	}

	protected override void OnHeroFullDefLoaded(string cardId, DefLoader.DisposableFullDef fullDef, object userData)
	{
		base.OnHeroFullDefLoaded(cardId, fullDef, userData);
		if (IsChoosingHero() && m_heroDefsLoading <= 0)
		{
			InitButtonAchievements();
		}
	}

	protected override void OnHeroActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		base.OnHeroActorLoaded(assetRef, go, callbackData);
		if (!(m_heroActor == null))
		{
			m_xpBar.transform.parent = m_heroActor.GetRootObject().transform;
			m_xpBar.transform.localScale = m_xpBarScale;
			m_xpBar.transform.localPosition = m_xpBarPosition;
			m_xpBar.m_isOnDeck = false;
			m_heroActor.AddCustomFrameCallback(OnCustomFrameLoadedCallback);
		}
	}

	protected override bool ShouldShowHeroPower()
	{
		if ((bool)UniversalInputManager.UsePhoneUI && !IsChoosingHero())
		{
			return IsChoosingHeroForHeroicDeck();
		}
		return true;
	}

	private bool IsDeckSharingActive()
	{
		if (m_DeckShareRequestButton == null)
		{
			return false;
		}
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.FRIENDLY)
		{
			return false;
		}
		if (!FriendChallengeMgr.Get().IsDeckShareEnabled())
		{
			return false;
		}
		if (IsChoosingHero())
		{
			return false;
		}
		return true;
	}

	private bool ShouldShowCollectionButton()
	{
		BoxRailroadManager boxRailroadManager = Box.Get().GetRailroadManager();
		if (boxRailroadManager != null && !boxRailroadManager.ShouldShowCollectionButton())
		{
			return false;
		}
		if (IsDeckSharingActive())
		{
			return false;
		}
		if (IsChoosingHero())
		{
			return false;
		}
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.FRIENDLY)
		{
			return false;
		}
		return true;
	}

	private bool ShouldGlowCollectionButton()
	{
		if (!ShouldShowCollectionButton())
		{
			return false;
		}
		if (!m_collectionButton.IsEnabled())
		{
			return false;
		}
		if (!GameModeUtils.HasPlayedAPracticeMatch())
		{
			return false;
		}
		if (!Options.Get().GetBool(Option.HAS_CLICKED_COLLECTION_BUTTON_FOR_NEW_DECK) && HaveDecksThatNeedNames())
		{
			return true;
		}
		if (!Options.Get().GetBool(Option.HAS_CLICKED_COLLECTION_BUTTON_FOR_NEW_CARD) && HaveUnseenCards())
		{
			return true;
		}
		if (Options.Get().GetBool(Option.GLOW_COLLECTION_BUTTON_AFTER_SET_ROTATION) && SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT)
		{
			return true;
		}
		return false;
	}

	private bool HaveDecksThatNeedNames()
	{
		foreach (CollectionDeck deck in CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK))
		{
			if (deck.NeedsName)
			{
				return true;
			}
		}
		return false;
	}

	private uint GetNumValidStandardDecks()
	{
		uint numStandardDecks = 0u;
		foreach (CollectionDeck deck in CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK))
		{
			if (deck.IsValidForFormat(PegasusShared.FormatType.FT_STANDARD) && deck.IsValidForRuleset)
			{
				numStandardDecks++;
			}
		}
		return numStandardDecks;
	}

	private bool HaveUnseenCards()
	{
		CollectionManager collectionManager = CollectionManager.Get();
		int? minOwned = 1;
		bool? notSeen = true;
		bool? isHero = false;
		return collectionManager.FindCards(null, null, null, null, null, null, null, null, null, isHero, minOwned, notSeen, null, null, null, returnAfterFirstResult: true, null, null, null).m_cards.Count > 0;
	}

	private void PlayEnterModeDialogues()
	{
		if (!ShowInnkeeperQuoteIfNeeded())
		{
			ShowWhizbangPopupIfNeeded();
		}
	}

	private bool ShowInnkeeperQuoteIfNeeded()
	{
		bool quoteShown = false;
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER && Options.Get().GetBool(Option.SHOW_WILD_DISCLAIMER_POPUP_ON_CREATE_DECK) && Options.GetFormatType() == PegasusShared.FormatType.FT_WILD && UserAttentionManager.CanShowAttentionGrabber("DeckPickTrayDisplay.ShowInnkeeperQuoteIfNeeded:" + Option.SHOW_WILD_DISCLAIMER_POPUP_ON_CREATE_DECK))
		{
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, NotificationManager.DEFAULT_CHARACTER_POS, GameStrings.Get("VO_INNKEEPER_PLAY_STANDARD_TO_WILD"), "VO_INNKEEPER_Male_Dwarf_SetRotation_43.prefab:4b4ce858139927946905ec0d40d5b3c1");
			Options.Get().SetBool(Option.SHOW_WILD_DISCLAIMER_POPUP_ON_CREATE_DECK, val: false);
			quoteShown = true;
		}
		return quoteShown;
	}

	private bool ShowWhizbangPopupIfNeeded()
	{
		if (SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.GAMEPLAY)
		{
			return false;
		}
		LastGameData lastGameData = GameMgr.Get().LastGameData;
		if (lastGameData.GameResult != TAG_PLAYSTATE.WON || !lastGameData.HasWhizbangDeckID())
		{
			return false;
		}
		int whizbangPopupCounter = Options.Get().GetInt(Option.WHIZBANG_POPUP_COUNTER);
		if (whizbangPopupCounter >= 7)
		{
			return false;
		}
		CollectionManager.TemplateDeck whizbangDeck = CollectionManager.Get().GetTemplateDeck(lastGameData.WhizbangDeckID);
		if (whizbangDeck == null || (whizbangDeck.m_event != EventTimingType.UNKNOWN && !EventTimingManager.Get().IsEventActive(whizbangDeck.m_event)))
		{
			return false;
		}
		bool popUpShown = false;
		if (whizbangPopupCounter == 0 || whizbangPopupCounter == 2 || whizbangPopupCounter == 6)
		{
			if (UserAttentionManager.CanShowAttentionGrabber("DeckPickerTrayDisplay.ShowWhizbangPopupIfNeeded()"))
			{
				StartCoroutine(ShowWhizbangPopup(whizbangDeck));
				whizbangPopupCounter++;
				popUpShown = true;
			}
		}
		else
		{
			whizbangPopupCounter++;
		}
		Options.Get().SetInt(Option.WHIZBANG_POPUP_COUNTER, whizbangPopupCounter);
		return popUpShown;
	}

	private IEnumerator ShowWhizbangPopup(CollectionManager.TemplateDeck whizbangDeck)
	{
		if (whizbangDeck != null)
		{
			yield return new WaitForSeconds(1f);
			BasicPopup.PopupInfo info = new BasicPopup.PopupInfo();
			info.m_prefabAssetRefs.Add("WhizbangDialog_notification.prefab:89912cf72b2d5cf47820d2328de40f3f");
			info.m_headerText = GameStrings.Get("GLUE_COLLECTION_MANAGER_WHIZBANG_POPUP_HEADER");
			info.m_bodyText = GameStrings.Format("GLUE_COLLECTION_MANAGER_WHIZBANG_POPUP_BODY", GameStrings.GetClassName(whizbangDeck.m_class), whizbangDeck.m_title);
			info.m_disableBnetBar = true;
			DialogManager.Get().ShowBasicPopup(UserAttentionBlocker.NONE, info);
		}
	}

	private void SetLockedPortraitMaterial(HeroPickerButton button)
	{
		if (!(button != null) || !button.IsLocked())
		{
			return;
		}
		switch (SceneMgr.Get().GetMode())
		{
		case SceneMgr.Mode.FRIENDLY:
			if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
			{
				break;
			}
			goto default;
		default:
		{
			using DefLoader.DisposableFullDef fullDef = button.ShareFullDef();
			if (!(fullDef.CardDef.m_LockedClassPortrait == null))
			{
				m_heroActor.SetPortraitMaterial(fullDef.CardDef.m_LockedClassPortrait);
			}
			break;
		}
		case SceneMgr.Mode.TAVERN_BRAWL:
			break;
		}
	}

	private bool ShouldLoadCustomDecks()
	{
		if (m_deckPickerMode == DeckPickerMode.INVALID)
		{
			Debug.LogWarning("DeckPickerTrayDisplay.ShouldLoadCustomDecks() - querying m_deckPickerMode when it hasn't been set yet!");
		}
		if (IsDeckSharingActive())
		{
			return true;
		}
		return m_deckPickerMode == DeckPickerMode.CUSTOM;
	}

	private RankedPlayDataModel GetBonusStarsPopupDataModel()
	{
		TournamentDisplay tournamentDisplay = TournamentDisplay.Get();
		if (tournamentDisplay == null)
		{
			return null;
		}
		NetCache.NetCacheMedalInfo netCacheMedalInfo = tournamentDisplay.GetCurrentMedalInfo();
		if (netCacheMedalInfo == null)
		{
			return null;
		}
		return new MedalInfoTranslator(netCacheMedalInfo).CreateDataModel(Options.GetFormatType(), RankedMedal.DisplayMode.Default);
	}

	private void ShowInvalidClassPopup()
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_CLASS_INVALID_DECK_TITLE");
		info.m_text = GameStrings.Get("GLUE_CLASS_INVALID_DECK_DESC");
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		DialogManager.DialogProcessCallback callback = delegate
		{
			SetPlayButtonEnabled(enable: true);
			return true;
		};
		DialogManager.Get().ShowPopup(info, callback);
	}

	private void UpdatePageArrows()
	{
		bool showLeftArrow = true;
		bool showRightArrow = true;
		VisualsFormatTypeExtensions.GetCurrentVisualsFormatType();
		if (m_numPagesToShow <= 1 || (Options.GetFormatType() == PegasusShared.FormatType.FT_TWIST && !ValidTwistDecksExist()) || DemoMgr.Get().IsExpoDemo() || IsChoosingHero())
		{
			showLeftArrow = false;
			showRightArrow = false;
		}
		else
		{
			if (IsShowingFirstPage())
			{
				showLeftArrow = false;
			}
			if (IsShowingLastPage() || IsNextPageDisabled())
			{
				showRightArrow = false;
			}
		}
		if (showLeftArrow)
		{
			if (!m_leftArrow.gameObject.activeInHierarchy)
			{
				m_showLeftArrowCoroutine = StartCoroutine(ArrowDelayedActivate(m_leftArrow, 0.25f));
			}
		}
		else
		{
			if (m_showLeftArrowCoroutine != null)
			{
				StopCoroutine(m_showLeftArrowCoroutine);
			}
			m_leftArrow.gameObject.SetActive(value: false);
		}
		if (showRightArrow)
		{
			if (!m_rightArrow.gameObject.activeInHierarchy)
			{
				m_showRightArrowCoroutine = StartCoroutine(ArrowDelayedActivate(m_rightArrow, 0.25f));
			}
			return;
		}
		if (m_showRightArrowCoroutine != null)
		{
			StopCoroutine(m_showRightArrowCoroutine);
		}
		m_rightArrow.gameObject.SetActive(value: false);
	}

	private bool IsPageValidForCurrentFormat(int pageNum)
	{
		VisualsFormatType currentVisualsFormatType = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType();
		return m_formatConfig[currentVisualsFormatType].EnabledPageTypes.Contains(m_customPages[pageNum].DeckPageType);
	}

	private int FindNextValidPage(int dir)
	{
		VisualsFormatType currentVisualsFormatType = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType();
		HashSet<CustomDeckPage.PageType> enabledPageTypes = m_formatConfig[currentVisualsFormatType].EnabledPageTypes;
		int nextPageIndex = m_currentPageIndex;
		do
		{
			nextPageIndex += dir;
			if (dir == 0 || nextPageIndex < 0 || nextPageIndex >= m_customPages.Count)
			{
				return m_currentPageIndex;
			}
		}
		while (!enabledPageTypes.Contains(m_customPages[nextPageIndex].DeckPageType));
		return nextPageIndex;
	}

	private bool IsShowingFirstPage()
	{
		return m_currentPageIndex == FindNextValidPage(-1);
	}

	private bool IsShowingLastPage()
	{
		return m_currentPageIndex == FindNextValidPage(1);
	}

	private bool IsNextPageDisabled()
	{
		int nextPageIndex = m_currentPageIndex + 1;
		if (nextPageIndex >= m_customPages.Count)
		{
			return true;
		}
		if (m_customPages[nextPageIndex].DeckPageType != CustomDeckPage.PageType.RESERVE)
		{
			return false;
		}
		return !m_customPages[nextPageIndex].EnabledAsReserve;
	}

	private void OnCustomFrameLoadedCallback(CustomFrameController customFrameController)
	{
		m_customFrameController = customFrameController;
		if (m_customFrameController != null)
		{
			SetContainerOffsets(m_customFrameController.HeroPowerContainerOffset);
			SetXPBarPositionAndScale(m_customFrameController);
		}
		else
		{
			SetContainerOffsets(0f);
		}
	}

	private void SetContainerOffsets(float offset)
	{
		if (m_heroPowerContainer != null)
		{
			if (!m_heroPowerContainerOffset.HasValue)
			{
				m_heroPowerContainerOffset = m_heroPowerContainer.transform.localPosition;
			}
			m_heroPowerContainer.transform.localPosition = m_heroPowerContainerOffset.Value + new Vector3(0f, offset, 0f);
		}
	}

	private void SetXPBarPositionAndScale(CustomFrameController customFrameController)
	{
		if (!(m_xpBar == null))
		{
			if (customFrameController == null || !customFrameController.UseMetaCalibration)
			{
				m_xpBar.transform.localScale = m_xpBarScale;
				m_xpBar.transform.localPosition = m_xpBarPosition;
			}
			else
			{
				m_xpBar.transform.localScale = customFrameController.DeckPickerXPBarScale;
				m_xpBar.transform.localPosition = customFrameController.DeckPickerXPBarPosition;
			}
		}
	}

	private void SetMissingTwistDeckActive(bool active)
	{
		if (m_missingTwistDeck != null)
		{
			m_missingTwistDeck.SetActive(active);
		}
		if (m_replayableTutorial != null)
		{
			m_replayableTutorial.SetActive(!active);
		}
	}

	protected override void ApplyDeckPickerTrayConfig()
	{
		if (m_deckDisplayConfig == null || appliedDeckTrayConfigChanges)
		{
			return;
		}
		if (Ranked_HeroName_Bone != null)
		{
			Ranked_HeroName_Bone.position += m_deckDisplayConfig.m_heroNameOffset;
		}
		if (m_rankedWinsPlate != null)
		{
			VisualsFormatType currentVisualsFormatType = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType();
			m_rankedWinsPlate.SetActive(m_formatConfig[currentVisualsFormatType].ShowClassWins);
		}
		if (m_playButtonRoot != null)
		{
			m_playButtonRoot.transform.position += m_deckDisplayConfig.m_playButtonOffset;
		}
		if (m_Ranked_Hero_Bone != null)
		{
			m_Ranked_Hero_Bone.position += m_deckDisplayConfig.m_rankedPlayHeroBonePostionOffset;
		}
		if (m_HeroPower_Bone != null)
		{
			m_HeroPower_Bone.position += m_deckDisplayConfig.m_cardPlayHeroPowerBoneDownOffset;
		}
		if (m_rankedTrayShownBone != null)
		{
			m_rankedTrayShownBone.transform.position += m_deckDisplayConfig.m_rankedTrayShownBoneOffset;
			if ((bool)m_rankedTrayDeckPickerFrame)
			{
				m_rankedTrayDeckPickerFrame.transform.position += m_deckDisplayConfig.m_rankedTrayShownBoneOffset;
			}
		}
		if (m_deckPickerHeroPortraitClickable != null)
		{
			m_deckPickerHeroPortraitClickable.transform.position += m_deckDisplayConfig.m_deckPickerHeroPortraitClickableOffset;
			m_deckPickerHeroPortraitClickable.gameObject.SetActive(m_deckDisplayConfig.m_deckPickerHeroPortraitClickableToggle);
		}
		if (m_heroName != null)
		{
			m_heroName.Width += m_deckDisplayConfig.m_heroNameTextWidthOffset;
			m_heroName.Height += m_deckDisplayConfig.m_heroNameTextHeightOffset;
			m_heroName.WordWrap = m_deckDisplayConfig.m_heroNameParagraphWordWrapToggle;
		}
		appliedDeckTrayConfigChanges = true;
	}

	protected override void RemoveDeckPickerTrayConfig()
	{
		if (m_deckDisplayConfig == null || !appliedDeckTrayConfigChanges)
		{
			return;
		}
		if (Ranked_HeroName_Bone != null)
		{
			Ranked_HeroName_Bone.position -= m_deckDisplayConfig.m_heroNameOffset;
		}
		if (m_rankedWinsPlate != null)
		{
			VisualsFormatType currentVisualsFormatType = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType();
			m_rankedWinsPlate.SetActive(m_formatConfig[currentVisualsFormatType].ShowClassWins);
		}
		if (m_playButtonRoot != null)
		{
			m_playButtonRoot.transform.position -= m_deckDisplayConfig.m_playButtonOffset;
		}
		if (m_Ranked_Hero_Bone != null)
		{
			m_Ranked_Hero_Bone.position -= m_deckDisplayConfig.m_rankedPlayHeroBonePostionOffset;
		}
		if (m_HeroPower_Bone != null)
		{
			m_HeroPower_Bone.position -= m_deckDisplayConfig.m_cardPlayHeroPowerBoneDownOffset;
		}
		if (m_rankedTrayShownBone != null)
		{
			m_rankedTrayShownBone.transform.position -= m_deckDisplayConfig.m_rankedTrayShownBoneOffset;
			if ((bool)m_rankedTrayDeckPickerFrame)
			{
				m_rankedTrayDeckPickerFrame.transform.position -= m_deckDisplayConfig.m_rankedTrayShownBoneOffset;
			}
		}
		if (m_deckPickerHeroPortraitClickable != null)
		{
			m_deckPickerHeroPortraitClickable.transform.position -= m_deckDisplayConfig.m_deckPickerHeroPortraitClickableOffset;
			m_deckPickerHeroPortraitClickable.gameObject.SetActive(!m_deckDisplayConfig.m_deckPickerHeroPortraitClickableToggle);
		}
		if (m_heroName != null)
		{
			m_heroName.Width -= m_deckDisplayConfig.m_heroNameTextWidthOffset;
			m_heroName.Height -= m_deckDisplayConfig.m_heroNameTextHeightOffset;
			m_heroName.WordWrap = !m_deckDisplayConfig.m_heroNameParagraphWordWrapToggle;
		}
		appliedDeckTrayConfigChanges = false;
	}

	public void OnLoanerDeckDisplayOpened()
	{
		if (m_switchFormatPopup != null)
		{
			NotificationManager.Get().DestroyNotification(m_switchFormatPopup, 0f);
		}
	}

	public void InitSetRotationTutorial(bool veteranFlow)
	{
		if (m_setRotationTutorialState != 0)
		{
			Debug.LogError("Tried to call DeckPickerTrayDisplay.InitTutorial() when m_setRotationTutorialState was " + m_setRotationTutorialState);
			return;
		}
		m_setRotationTutorialState = SetRotationTutorialState.PREPARING;
		m_switchFormatButton.Disable();
		m_switchFormatButton.gameObject.SetActive(value: false);
		TransitionToFormatType(PegasusShared.FormatType.FT_STANDARD, inRankedPlayMode: true);
		Options.SetFormatType(PegasusShared.FormatType.FT_STANDARD);
		Options.SetInRankedPlayMode(inRankedPlayMode: true);
		Deselect();
		ShowPage(0, skipTraySlidingAnimation: true);
		m_rankedPlayDisplay.StartSetRotationTutorial();
		SetPlayButtonEnabled(enable: false);
		SetBackButtonEnabled(enable: false);
		SetCollectionButtonEnabled(enable: false);
		m_rightArrow.gameObject.SetActive(value: false);
		m_leftArrow.gameObject.SetActive(value: false);
		m_rightArrow.SetEnabled(enabled: false);
		m_leftArrow.SetEnabled(enabled: false);
		SetHeaderText(GameStrings.Get("GLUE_TOURNAMENT"));
		if (m_heroPower != null)
		{
			m_heroPower.GetComponent<Collider>().enabled = false;
		}
		if (m_goldenHeroPower != null)
		{
			m_goldenHeroPower.GetComponent<Collider>().enabled = false;
		}
		foreach (CustomDeckPage customPage in m_customPages)
		{
			customPage.EnableDeckButtons(enable: false);
		}
		m_setRotationTutorialState = SetRotationTutorialState.READY;
	}

	public void StartSetRotationTutorial(SetRotationClock.DisableTheClockCallback callback)
	{
		if (m_setRotationTutorialState == SetRotationTutorialState.READY)
		{
			StartCoroutine(ShowSetRotationTutorialPopups(callback));
			return;
		}
		Debug.LogError("Tried to start Play Screen Set Rotation Tutorial without calling DeckPickerTrayDisplay.InitTutorial()");
		callback();
	}

	private IEnumerator ShowSetRotationTutorialPopups(SetRotationClock.DisableTheClockCallback disableClockCallback)
	{
		bool veteranFlow = SetRotationManager.HasSeenStandardModeTutorial();
		m_setRotationTutorialState = SetRotationTutorialState.SHOW_TUTORIAL_POPUPS;
		m_dimQuad.GetComponent<Renderer>().enabled = true;
		m_dimQuad.enabled = true;
		m_dimQuad.StopPlayback();
		m_dimQuad.Play("DimQuad_FadeIn");
		GameSaveDataManager gameSaveDataManager = GameSaveDataManager.Get();
		if (gameSaveDataManager != null)
		{
			long rotatedBoosterPopupProgress = -1L;
			long innkeeperStandardDecksVOProgress = -1L;
			gameSaveDataManager.GetSubkeyValue(GameSaveKeyId.SET_ROTATION, GameSaveKeySubkeyId.ROTATED_BOOSTER_POPUP_PROGRESS, out rotatedBoosterPopupProgress);
			gameSaveDataManager.GetSubkeyValue(GameSaveKeyId.SET_ROTATION, GameSaveKeySubkeyId.INNKEEPER_STANDARD_DECKS_VO_PROGRESS, out innkeeperStandardDecksVOProgress);
			bool saveNeeded = false;
			List<GameSaveDataManager.SubkeySaveRequest> saveRequests = new List<GameSaveDataManager.SubkeySaveRequest>();
			if (rotatedBoosterPopupProgress == 0L)
			{
				saveRequests.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.SET_ROTATION, GameSaveKeySubkeyId.ROTATED_BOOSTER_POPUP_PROGRESS, 1L));
				saveNeeded = true;
			}
			if (innkeeperStandardDecksVOProgress == 0L)
			{
				saveRequests.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.SET_ROTATION, GameSaveKeySubkeyId.INNKEEPER_STANDARD_DECKS_VO_PROGRESS, 1L));
				saveNeeded = true;
			}
			if (saveNeeded)
			{
				gameSaveDataManager.SaveSubkeys(saveRequests);
			}
		}
		m_shouldContinue = false;
		Get().AddFormatTypePickerClosedListener(ContinueTutorial);
		Get().ShowPopupDuringSetRotation(VisualsFormatType.VFT_STANDARD);
		disableClockCallback();
		while (!m_shouldContinue)
		{
			yield return null;
		}
		Get().RemoveFormatTypePickerClosedListener(ContinueTutorial);
		if (veteranFlow)
		{
			StartCoroutine(ShowWelcomeQuests());
		}
		else
		{
			StartSwitchModeWalkthrough();
		}
	}

	private void ContinueTutorial(DialogBase dialog, object userData)
	{
		m_shouldContinue = true;
	}

	private void ContinueTutorial()
	{
		m_shouldContinue = true;
	}

	private bool ShouldShowRotatedBoosterPopup(VisualsFormatType newVisualsFormatType)
	{
		if (newVisualsFormatType == VisualsFormatType.VFT_STANDARD || (newVisualsFormatType == VisualsFormatType.VFT_WILD && newVisualsFormatType.IsRanked()))
		{
			GameSaveDataManager gameSaveDataManager = GameSaveDataManager.Get();
			if (gameSaveDataManager != null)
			{
				long rotatedBoosterPopupProgress = -1L;
				gameSaveDataManager.GetSubkeyValue(GameSaveKeyId.SET_ROTATION, GameSaveKeySubkeyId.ROTATED_BOOSTER_POPUP_PROGRESS, out rotatedBoosterPopupProgress);
				if (rotatedBoosterPopupProgress == 1)
				{
					return true;
				}
			}
		}
		return false;
	}

	private IEnumerator ShowRotatedBoostersPopup(Action callbackOnHide = null)
	{
		yield return new WaitForSeconds(1f);
		if (UserAttentionManager.CanShowAttentionGrabber(UserAttentionBlocker.SET_ROTATION_INTRO, "ShowSetRotationTutorialDialog"))
		{
			SetRotationRotatedBoostersPopup.SetRotationRotatedBoostersPopupInfo info = new SetRotationRotatedBoostersPopup.SetRotationRotatedBoostersPopupInfo();
			info.m_onHiddenCallback = callbackOnHide;
			DialogManager.Get().ShowSetRotationTutorialPopup(UserAttentionBlocker.SET_ROTATION_INTRO, info);
			GameSaveDataManager gameSaveDataManager = GameSaveDataManager.Get();
			if (gameSaveDataManager != null)
			{
				List<GameSaveDataManager.SubkeySaveRequest> saveRequests = new List<GameSaveDataManager.SubkeySaveRequest>();
				saveRequests.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.SET_ROTATION, GameSaveKeySubkeyId.ROTATED_BOOSTER_POPUP_PROGRESS, 2L));
				gameSaveDataManager.SaveSubkeys(saveRequests);
			}
		}
	}

	private void StartSwitchModeWalkthrough()
	{
		m_setRotationTutorialState = SetRotationTutorialState.SWITCH_MODE_WALKTHROUGH;
		StartCoroutine(TutorialSwitchToStandard());
	}

	private IEnumerator TutorialSwitchToStandard()
	{
		Transform notificationBone = (UniversalInputManager.UsePhoneUI ? m_Switch_Format_Notification_Bone_Mobile : m_Switch_Format_Notification_Bone);
		m_switchFormatPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.SET_ROTATION_INTRO, notificationBone.position, notificationBone.localScale, GameStrings.Get("GLUE_TOURNAMENT_SWITCH_MODE"));
		if (m_switchFormatPopup != null)
		{
			Notification.PopUpArrowDirection arrowDirection = (UniversalInputManager.UsePhoneUI ? Notification.PopUpArrowDirection.RightUp : Notification.PopUpArrowDirection.Up);
			m_switchFormatPopup.ShowPopUpArrow(arrowDirection);
		}
		m_switchFormatButton.EnableHighlight(enabled: true);
		m_switchFormatButton.AddEventListener(UIEventType.RELEASE, OnSwitchFormatReleased);
		m_switchFormatButton.Enable();
		yield break;
	}

	private void OnSwitchFormatReleased(UIEvent e)
	{
		if (m_setRotationTutorialState == SetRotationTutorialState.SWITCH_MODE_WALKTHROUGH)
		{
			m_switchFormatButton.Disable();
			m_switchFormatButton.RemoveEventListener(UIEventType.RELEASE, OnSwitchFormatReleased);
			Processor.QueueJob("LoginManager.CompleteLoginFlow", LoginManager.Get().CompleteLoginFlow());
			StartCoroutine(ShowWelcomeQuests());
		}
		else
		{
			Debug.Log("OnSwitchFormatReleased called when not in SWITCH_MODE_WALKTHROUGH Set Rotation Tutorial state");
		}
	}

	private void PlayTransitionSounds()
	{
		if (m_customPages[m_currentPageIndex].HasWildDeck() && !string.IsNullOrEmpty(m_wildDeckTransitionSound))
		{
			SoundManager.Get().LoadAndPlay(m_wildDeckTransitionSound);
		}
	}

	private void MarkSetRotationComplete()
	{
		m_setRotationTutorialState = SetRotationTutorialState.INACTIVE;
		Options.Get().SetBool(Option.HAS_SEEN_STANDARD_MODE_TUTORIAL, val: true);
		SetRotationManager.Get().SetRotationIntroProgress();
		GameUtils.SetGSDFlag(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_SEEN_WILD_CRAFT_ALERT, enableFlag: false);
		foreach (long noticeId in m_noticeIdsToAck)
		{
			Network.Get().AckNotice(noticeId);
		}
	}

	private IEnumerator ShowWelcomeQuests()
	{
		MarkSetRotationComplete();
		m_switchFormatButton.EnableHighlight(enabled: false);
		NotificationManager.Get().DestroyNotification(m_switchFormatPopup, 0f);
		m_switchFormatPopup = null;
		m_dimQuad.StopPlayback();
		m_dimQuad.Play("DimQuad_FadeOut");
		yield return new WaitForEndOfFrame();
		float animDuration = m_dimQuad.GetCurrentAnimatorStateInfo(0).length;
		yield return new WaitForSeconds(animDuration);
		m_dimQuad.GetComponent<Renderer>().enabled = false;
		m_dimQuad.enabled = false;
		yield return new WaitForSeconds(m_showQuestPause);
		OnWelcomeQuestDismiss();
	}

	private void OnWelcomeQuestDismiss()
	{
		StartCoroutine(EndTutorial());
	}

	private IEnumerator EndTutorial()
	{
		yield return new WaitForSeconds(m_playVOPause);
		if (m_heroPower != null)
		{
			m_heroPower.GetComponent<Collider>().enabled = true;
		}
		if (m_goldenHeroPower != null)
		{
			m_goldenHeroPower.GetComponent<Collider>().enabled = true;
		}
		SetBackButtonEnabled(enable: true);
		SetCollectionButtonEnabled(enable: true);
		m_rightArrow.SetEnabled(enabled: true);
		m_leftArrow.SetEnabled(enabled: true);
		m_leftArrow.gameObject.SetActive(!IsShowingFirstPage());
		m_rightArrow.gameObject.SetActive(!IsShowingLastPage());
		foreach (CustomDeckPage customPage in m_customPages)
		{
			customPage.EnableDeckButtons(enable: true);
		}
		Options.Get().SetBool(Option.GLOW_COLLECTION_BUTTON_AFTER_SET_ROTATION, val: true);
		UpdateCollectionButtonGlow();
		if (m_switchFormatButton != null)
		{
			m_switchFormatButton.Enable();
		}
		UserAttentionManager.StopBlocking(UserAttentionBlocker.SET_ROTATION_INTRO);
	}

	private bool ShouldShowStandardDeckVO(VisualsFormatType newVisualsFormatType)
	{
		if (newVisualsFormatType == VisualsFormatType.VFT_STANDARD)
		{
			GameSaveDataManager gameSaveDataManager = GameSaveDataManager.Get();
			if (gameSaveDataManager != null)
			{
				long innkeeperStandardDecksVOProgress = -1L;
				gameSaveDataManager.GetSubkeyValue(GameSaveKeyId.SET_ROTATION, GameSaveKeySubkeyId.INNKEEPER_STANDARD_DECKS_VO_PROGRESS, out innkeeperStandardDecksVOProgress);
				if (innkeeperStandardDecksVOProgress == 1)
				{
					return true;
				}
			}
		}
		return false;
	}

	private IEnumerator ShowStandardDeckVO(Action<int> finishedCallback)
	{
		yield return new WaitForSeconds(1f);
		switch (GetNumValidStandardDecks())
		{
		case 1u:
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.SET_ROTATION_INTRO, INNKEEPER_QUOTE_POS, GameStrings.Get("VO_INNKEEPER_HAVE_ONE_STANDARD_DECK"), "VO_INNKEEPER_Male_Dwarf_HAVE_STANDARD_DECK_07.prefab:282cd0db8b3737d4bb55d71f915074e4", 0f, finishedCallback);
			break;
		default:
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.SET_ROTATION_INTRO, INNKEEPER_QUOTE_POS, GameStrings.Get("VO_INNKEEPER_HAVE_STANDARD_DECKS"), "VO_INNKEEPER_Male_Dwarf_HAVE_STANDARD_DECKS_08.prefab:0c1c2ab2c4ead094abc69ec278aa4878", 0f, finishedCallback);
			break;
		case 0u:
			break;
		}
		GameSaveDataManager gameSaveDataManager = GameSaveDataManager.Get();
		if (gameSaveDataManager != null)
		{
			List<GameSaveDataManager.SubkeySaveRequest> saveRequests = new List<GameSaveDataManager.SubkeySaveRequest>();
			saveRequests.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.SET_ROTATION, GameSaveKeySubkeyId.INNKEEPER_STANDARD_DECKS_VO_PROGRESS, 2L));
			gameSaveDataManager.SaveSubkeys(saveRequests);
		}
	}
}
