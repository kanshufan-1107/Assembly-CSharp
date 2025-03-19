using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Jobs;
using Blizzard.T5.Logging;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.Core.Streaming;
using Hearthstone.DataModels;
using Hearthstone.InGameMessage.UI;
using Hearthstone.Progression;
using Hearthstone.Store;
using Hearthstone.Streaming;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class Box : MonoBehaviour
{
	public enum State
	{
		INVALID,
		STARTUP,
		PRESS_START,
		LOADING,
		LOADING_HUB,
		HUB,
		HUB_WITH_DRAWER,
		OPEN,
		CLOSED,
		ERROR,
		SET_ROTATION_LOADING,
		SET_ROTATION,
		SET_ROTATION_OPEN,
		APPRENTICE_OVERRIDE_CLOSED,
		APPRENTICE_OVERRIDE_HUB
	}

	public delegate void TransitionFinishedCallback(object userData);

	public delegate void TransitionStartedCallback();

	private class TransitionFinishedListener : EventListener<TransitionFinishedCallback>
	{
		public void Fire()
		{
			m_callback(m_userData);
		}
	}

	private class TransitionStartedListener : EventListener<TransitionStartedCallback>
	{
		public void Fire()
		{
			m_callback();
		}
	}

	private class BoxStateConfig
	{
		public class Part<T>
		{
			public bool m_ignore;

			public T m_state;
		}

		public Part<BoxLogo.State> m_logoState = new Part<BoxLogo.State>();

		public Part<StoreButton.State> m_storeButtonState = new Part<StoreButton.State>();

		public Part<BoxDoor.State> m_doorState = new Part<BoxDoor.State>();

		public Part<BoxDisk.State> m_diskState = new Part<BoxDisk.State>();

		public Part<BoxDrawer.State> m_drawerState = new Part<BoxDrawer.State>();

		public Part<BoxCamera.State> m_camState = new Part<BoxCamera.State>();

		public Part<EventBoxDressing.State> m_boxDressingState = new Part<EventBoxDressing.State>();

		public List<State> m_stateAllowList = new List<State>();

		public bool CanTransitionToState(State state)
		{
			if (m_stateAllowList != null && m_stateAllowList.Count != 0)
			{
				return m_stateAllowList.IndexOf(state) != -1;
			}
			return true;
		}
	}

	public enum ButtonType
	{
		START,
		TRADITIONAL,
		OPEN_PACKS,
		COLLECTION,
		SET_ROTATION,
		QUEST_LOG,
		STORE,
		GAME_MODES,
		BACON,
		PVP_DUNGEON_RUN,
		MERCENARIES,
		TAVERN_BRAWL,
		JOURNAL
	}

	public delegate void ButtonPressFunction(UIEvent e);

	public delegate void ButtonPressCallback(ButtonType buttonType, bool isShowingTutorialPreview, object userData);

	public class ButtonPressListener : EventListener<ButtonPressCallback>
	{
		public void Fire(ButtonType buttonType, bool isShowingTutorialPreview)
		{
			m_callback(buttonType, isShowingTutorialPreview, m_userData);
		}
	}

	[Header("General")]
	public AsyncReference m_boxWidgetRef;

	public GameObject m_rootObject;

	public WeakAssetReference m_defaultInnkeeperGreetings;

	public Widget m_eventBoxDressingWidget;

	public BoxStateInfoList m_StateInfoList;

	[Header("Box Parts")]
	public BoxLogo m_Logo;

	public BoxDoor m_LeftDoor;

	public BoxDoor m_RightDoor;

	public BoxDisk m_Disk;

	public GameObject m_DiskCenter;

	public BoxSpinner m_TopSpinner;

	public BoxSpinner m_BottomSpinner;

	public BoxDrawer m_Drawer;

	public BoxCamera m_Camera;

	public GameObject m_OuterFrame;

	public List<Collider> m_outerPanelColliders;

	public GameObject m_letterboxingContainer;

	public GameObject m_tableTop;

	[Header("Buttons")]
	public BoxMenuButton m_PlayButton;

	public BoxScrollButton m_BattleGroundsButton;

	public BoxScrollButton m_TavernBrawlButton;

	public GameObject m_TavernBrawlButtonCover;

	public Material m_TavernBrawlDisabledDiscMaterial;

	public Material m_ButtonEnabledMaterial;

	public Material m_ButtonDisabledMaterial;

	public List<WeakAssetReference> m_tavernBrawlEnterCrowdSounds;

	public GameObject m_EmptyFourthButton;

	public GameObject m_bnetBarBackground;

	public BoxMenuButton m_GameModesButton;

	public PackOpeningButton m_OpenPacksButton;

	public BoxMenuButton m_CollectionButton;

	public StoreButton m_StoreButton;

	public QuestLogButton m_QuestLogButton;

	public Widget m_journalButtonWidget;

	public RibbonButtonsUI m_ribbonButtons;

	[Header("Renderers")]
	public Renderer m_SpotLightRenderer;

	public Renderer m_DiscRenderer;

	public Renderer m_FirstButtonRenderer;

	public Renderer m_SecondButtonRenderer;

	public Renderer m_ThirdButtonRenderer;

	public Renderer m_FourthButtonRenderer;

	[Header("Text Colors")]
	public Color m_EnabledColor;

	public Color m_DisabledColor;

	public Color m_EnabledDrawerMaterial;

	public Color m_DisabledDrawerMaterial;

	[Header("Managers")]
	public BoxLightMgr m_LightMgr;

	public BoxEventMgr m_EventMgr;

	[Header("FTUE")]
	public GameObject m_newPlayerModeBanner;

	public WidgetInstance m_tutorialPreview;

	[Header("Shop")]
	public WidgetInstance m_mainShopWidget;

	[Header("Miscellaneous")]
	public Camera m_NoFxCamera;

	public AudioListener m_AudioListener;

	public Texture2D m_textureCompressionTest;

	public Widget m_battlegroundsDownloadCompleteWidgetInstance;

	private static Box s_instance;

	private BoxStateConfig[] m_stateConfigs;

	private State m_state = State.STARTUP;

	private int m_pendingEffects;

	private Queue<State> m_stateQueue = new Queue<State>();

	private bool m_transitioningToSceneMode;

	private List<TransitionFinishedListener> m_transitionFinishedListeners = new List<TransitionFinishedListener>();

	private List<TransitionStartedListener> m_transitionStartedListeners = new List<TransitionStartedListener>();

	private AssetHandle<Texture> m_tableTopTexture;

	private AssetHandle<Texture> m_boxTopTexture;

	private AssetHandle<Texture> m_specialEventTexture;

	private ButtonType? m_queuedButtonFire;

	private bool m_waitingForNetData;

	private GameLayer m_originalLeftDoorLayer;

	private GameLayer m_originalRightDoorLayer;

	private GameLayer m_originalDrawerLayer;

	private Material m_TavernBrawlEnabledDiscMaterial;

	private bool m_showRibbonButtons;

	private WeakAssetReference m_eventInnkeeperGreetings;

	private GameObject m_tempInputBlocker;

	private TableTopMgr m_tableTopMgr;

	private GameObject m_setRotationDisk;

	private BoxMenuButton m_setRotationButton;

	private Coroutine m_setRotationIntroCoroutine;

	private JournalButton m_journalButton;

	private Widget m_boxWidget;

	private const string SHOW_NEW_GAME_MODE_BADGE_STATE = "SHOW_NEW_GAME_MODE_BADGE";

	private const string HIDE_NEW_GAME_MODE_BADGE_STATE = "HIDE_NEW_GAME_MODE_BADGE";

	private const string SHOW_NEW_TAVERN_BRAWL_BADGE_STATE = "SHOW_NEW_TAVERN_BRAWL_BADGE";

	private const string HIDE_NEW_TAVERN_BRAWL_BADGE_STATE = "HIDE_NEW_TAVERN_BRAWL_BADGE";

	private const string ACTIVATE_EVENT_BOX_DRESSING = "EVENT_BOX_DRESSING_BIRTH";

	private const string DEACTIVATE_EVENT_BOX_DRESSING = "EVENT_BOX_DRESSING_DEATH";

	private bool m_eventBoxDressingActive;

	private DisableMesh_ColorBlack[] m_materialDependentComponents;

	private MusicPlaylistType m_activeMusicPlaylist = MusicPlaylistType.UI_MainTitle;

	private BoxRailroadManager m_railroadManager;

	protected List<ButtonPressListener> m_buttonPressListeners = new List<ButtonPressListener>();

	private int m_nextMissionId = -1;

	private bool m_waitingForSceneLoad;

	private const string SHOW_LOG_COROUTINE = "ShowQuestLogWhenReady";

	private TutorialPreviewController m_tutorialPreviewController;

	private DownloadConfirmationPopup m_downloadConfirmationPopup;

	private WidgetInstance m_downloadConfirmationPopupWidget;

	private bool m_loadingConfirmationPopup;

	private HashSet<PegUIElement> OnPressDisabledButtons = new HashSet<PegUIElement>();

	private HashSet<PegUIElement> OnHoverDisabledButtons = new HashSet<PegUIElement>();

	private static readonly AssetReference s_downloadConfirmationPopupReference = new AssetReference("NGDP_Download_PopUp.prefab:c9a71af6bd4c2ee4ebf5b3490b8158cf");

	private IGameDownloadManager DownloadManager => GameDownloadManagerProvider.Get();

	public event Action OnBoxDressingReadyOnce;

	public event Action OnTutorialTransitionFinished;

	private void Awake()
	{
		Log.LoadingScreen.Print("Box.Awake()");
		s_instance = this;
		InitializeStateConfigs();
		InitializeComponents();
		if (m_mainShopWidget != null)
		{
			if (m_mainShopWidget.gameObject.activeSelf)
			{
				Debug.LogError("Box setup error! m_mainShopWidget should NOT be enabled - this must be fixed");
			}
			m_mainShopWidget.gameObject.SetActive(value: false);
		}
		if (LoadingScreen.Get() != null)
		{
			LoadingScreen.Get().NotifyMainSceneObjectAwoke(base.gameObject);
		}
		m_originalLeftDoorLayer = (GameLayer)m_LeftDoor.gameObject.layer;
		m_originalRightDoorLayer = (GameLayer)m_RightDoor.gameObject.layer;
		m_originalDrawerLayer = (GameLayer)m_Drawer.gameObject.layer;
		m_railroadManager = base.gameObject.GetComponent<BoxRailroadManager>();
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (TransformUtil.GetAspectRatioDependentValue(0f, 1f, 1f) < 0.99f)
			{
				GameUtils.InstantiateGameObject("Letterboxing.prefab:303d7852a40ab4f178a3f97a102a0ea0", m_letterboxingContainer);
			}
			GameObject obj = AssetLoader.Get().InstantiatePrefab("RibbonButtons_Phone.prefab:1b805ba741fd649cabb72b2764c755f5");
			m_ribbonButtons = obj.GetComponent<RibbonButtonsUI>();
			m_ribbonButtons.UpdateRibbons(shouldShow: false);
			GameUtils.SetParent(obj, m_rootObject);
			m_tableTopMgr = m_tableTop.GetComponent<TableTopMgr>();
			AssetLoader.Get().LoadAsset<Texture>("TheBox_Top_phone.psd:666e602b70e7d6344be3e690de329636", OnBoxTopPhoneTextureLoaded);
			m_ribbonButtons.m_journalButtonWidget.RegisterReadyListener(delegate
			{
				m_journalButton = m_ribbonButtons.m_journalButtonWidget.GetComponentInChildren<JournalButton>();
			});
		}
		else
		{
			m_journalButtonWidget.RegisterReadyListener(delegate
			{
				m_journalButton = m_journalButtonWidget.GetComponentInChildren<JournalButton>();
			});
		}
		if (RewardTrackManager.Get().HasReceivedRewardTracksFromServer)
		{
			m_eventBoxDressingWidget.RegisterDoneChangingStatesListener(delegate
			{
				UpdateEventBoxDressingWithConfig();
			}, null, callImmediatelyIfSet: true, doOnce: true);
		}
		else
		{
			RewardTrackManager.Get().OnRewardTracksReceived += OnRewardTracksReceived;
		}
		GameSaveDataManager gsdManager = GameSaveDataManager.Get();
		if (gsdManager != null)
		{
			gsdManager.OnGameSaveDataUpdate += OnGameSaveDataUpdate;
		}
		m_materialDependentComponents = GetComponentsInChildren<DisableMesh_ColorBlack>();
		Processor.QueueJob("WaitForReadyToPlay", OnReadyToPlay(), new WaitForGameDownloadManagerState());
	}

	private IEnumerator<IAsyncJobResult> OnReadyToPlay()
	{
		UpdateStateForCurrentSceneMode();
		yield break;
	}

	private void Start()
	{
		InitializeNet(fromLogin: false);
		InitializeState();
		InitializeUI();
		if (DemoMgr.Get().IsExpoDemo())
		{
			m_StoreButton.gameObject.SetActive(value: false);
			m_Drawer.gameObject.SetActive(value: false);
			m_QuestLogButton.gameObject.SetActive(value: false);
		}
		if (m_state != State.HUB_WITH_DRAWER)
		{
			m_journalButtonWidget.Hide();
		}
		StoreManager.Get()?.RegisterStoreShownListener(HideEventBoxDressing);
		StoreManager.Get()?.RegisterStoreHiddenListener(UpdateEventBoxDressingWithConfig);
		OnStartButton();
	}

	private void OnDestroy()
	{
		Log.LoadingScreen.Print("Box.OnDestroy()");
		if (m_setRotationIntroCoroutine != null)
		{
			StopCoroutine(m_setRotationIntroCoroutine);
			m_setRotationIntroCoroutine = null;
		}
		if (PegUI.Get() != null)
		{
			PegUI.Get().RemoveInputCamera(m_Camera.GetComponent<Camera>());
		}
		StoreManager.Get()?.RemoveStoreShownListener(HideEventBoxDressing);
		StoreManager.Get()?.RemoveStoreHiddenListener(UpdateEventBoxDressingWithConfig);
		GameSaveDataManager gsdManager = GameSaveDataManager.Get();
		if (gsdManager != null)
		{
			gsdManager.OnGameSaveDataUpdate -= OnGameSaveDataUpdate;
		}
		if (LoadingScreen.Get() != null)
		{
			LoadingScreen.Get().UnregisterPreviousSceneDestroyedListener(OnTutorialSceneDestroyed);
		}
		ShutdownState();
		AssetHandle.SafeDispose(ref m_tableTopTexture);
		AssetHandle.SafeDispose(ref m_boxTopTexture);
		AssetHandle.SafeDispose(ref m_specialEventTexture);
		OnDestroyButton();
		s_instance = null;
	}

	public static Box Get()
	{
		return s_instance;
	}

	public Camera GetCamera()
	{
		return m_Camera.GetComponent<Camera>();
	}

	public BoxCamera GetBoxCamera()
	{
		return m_Camera;
	}

	public Camera GetNoFxCamera()
	{
		return m_NoFxCamera;
	}

	public AudioListener GetAudioListener()
	{
		return m_AudioListener;
	}

	public JournalButton GetJournalButton()
	{
		return m_journalButton;
	}

	public Texture2D GetTextureCompressionTestTexture()
	{
		return m_textureCompressionTest;
	}

	public State GetState()
	{
		return m_state;
	}

	public bool ChangeState(State state)
	{
		if (state == State.INVALID)
		{
			return false;
		}
		if (m_state == state)
		{
			return false;
		}
		if (IsBusy())
		{
			QueueStateChange(state);
		}
		else
		{
			ChangeStateNow(state);
		}
		RewardTrackManager.Get().ClearHasJustCompletedApprentice();
		return true;
	}

	public void UpdateState()
	{
		if (m_state == State.STARTUP)
		{
			UpdateState_Startup();
		}
		else if (m_state == State.PRESS_START)
		{
			UpdateState_PressStart();
		}
		else if (m_state == State.LOADING_HUB)
		{
			UpdateState_LoadingHub();
		}
		else if (m_state == State.LOADING)
		{
			UpdateState_Loading();
		}
		else if (m_state == State.HUB)
		{
			UpdateState_Hub();
		}
		else if (m_state == State.HUB_WITH_DRAWER)
		{
			UpdateState_HubWithDrawer();
		}
		else if (m_state == State.OPEN)
		{
			UpdateState_Open();
		}
		else if (m_state == State.CLOSED)
		{
			UpdateState_Closed();
		}
		else if (m_state == State.ERROR)
		{
			UpdateState_Error();
		}
		else if (m_state == State.SET_ROTATION_LOADING)
		{
			UpdateState_SetRotation();
		}
		else if (m_state == State.SET_ROTATION)
		{
			UpdateState_SetRotation();
		}
		else if (m_state == State.SET_ROTATION_OPEN)
		{
			UpdateState_SetRotationOpen();
		}
		else if (m_state == State.APPRENTICE_OVERRIDE_CLOSED)
		{
			UpdateState_ApprenticeOverrideClosed();
		}
		else if (m_state == State.APPRENTICE_OVERRIDE_HUB)
		{
			UpdateState_ApprenticeOverrideHub();
		}
		else
		{
			Debug.LogError($"Box.UpdateState() - unhandled state {m_state}");
		}
	}

	public BoxLightMgr GetLightMgr()
	{
		return m_LightMgr;
	}

	public BoxLightStateType GetLightState()
	{
		return m_LightMgr.GetActiveState();
	}

	public void ChangeLightState(BoxLightStateType stateType)
	{
		m_LightMgr.ChangeState(stateType);
	}

	public void SetLightState(BoxLightStateType stateType)
	{
		m_LightMgr.SetState(stateType);
	}

	public BoxEventMgr GetEventMgr()
	{
		return m_EventMgr;
	}

	public Spell GetEventSpell(BoxEventType eventType)
	{
		return m_EventMgr.GetEventSpell(eventType);
	}

	public bool HasPendingEffects()
	{
		return m_pendingEffects > 0;
	}

	public bool IsBusy()
	{
		if (!HasPendingEffects())
		{
			return m_stateQueue.Count > 0;
		}
		return true;
	}

	public bool IsTransitioningToSceneMode()
	{
		State overriddenState = m_railroadManager.GetOverriddenBoxState();
		if (m_transitioningToSceneMode)
		{
			return overriddenState != State.HUB_WITH_DRAWER;
		}
		return false;
	}

	public void OnAnimStarted()
	{
		m_pendingEffects++;
		FireTransitionStartedEvent();
	}

	public void OnAnimFinished()
	{
		m_pendingEffects--;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_OuterFrame.SetActive(value: false);
			if (GameUtils.CanCheckTutorialCompletion() && !GameUtils.IsAnyTutorialComplete() && m_state == State.OPEN)
			{
				m_tableTopMgr.HideTableTop();
			}
		}
		if (HasPendingEffects())
		{
			return;
		}
		if (m_stateQueue.Count == 0)
		{
			UpdateUIEvents();
			if (m_transitioningToSceneMode)
			{
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					bool shouldShowAllButtons = ShouldBeShowingState(State.HUB_WITH_DRAWER);
					ToggleRibbonUI(shouldShowAllButtons);
				}
				FireTransitionFinishedEvent();
				m_transitioningToSceneMode = false;
			}
		}
		else
		{
			ChangeStateQueued();
		}
	}

	public void OnLoggedIn()
	{
		InitializeNet(fromLogin: true);
	}

	public void AddTransitionStartedListener(TransitionStartedCallback callback)
	{
		TransitionStartedListener listener = new TransitionStartedListener();
		listener.SetCallback(callback);
		if (!m_transitionStartedListeners.Contains(listener))
		{
			m_transitionStartedListeners.Add(listener);
		}
	}

	public void AddTransitionFinishedListener(TransitionFinishedCallback callback)
	{
		AddTransitionFinishedListener(callback, null);
	}

	public void AddTransitionFinishedListener(TransitionFinishedCallback callback, object userData)
	{
		TransitionFinishedListener listener = new TransitionFinishedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_transitionFinishedListeners.Contains(listener))
		{
			m_transitionFinishedListeners.Add(listener);
		}
	}

	public bool RemoveTransitionStartedListener(TransitionStartedCallback callback)
	{
		TransitionStartedListener listener = new TransitionStartedListener();
		listener.SetCallback(callback);
		return m_transitionStartedListeners.Remove(listener);
	}

	public bool RemoveTransitionFinishedListener(TransitionFinishedCallback callback)
	{
		return RemoveTransitionFinishedListener(callback, null);
	}

	public bool RemoveTransitionFinishedListener(TransitionFinishedCallback callback, object userData)
	{
		TransitionFinishedListener listener = new TransitionFinishedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_transitionFinishedListeners.Remove(listener);
	}

	public void SetToIgnoreFullScreenEffects(bool ignoreEffects)
	{
		if (ignoreEffects)
		{
			LayerUtils.ReplaceLayer(m_LeftDoor.gameObject, GameLayer.IgnoreFullScreenEffects, m_originalLeftDoorLayer);
			LayerUtils.ReplaceLayer(m_RightDoor.gameObject, GameLayer.IgnoreFullScreenEffects, m_originalRightDoorLayer);
			LayerUtils.ReplaceLayer(m_Drawer.gameObject, GameLayer.IgnoreFullScreenEffects, m_originalDrawerLayer);
		}
		else
		{
			LayerUtils.ReplaceLayer(m_LeftDoor.gameObject, m_originalLeftDoorLayer, GameLayer.IgnoreFullScreenEffects);
			LayerUtils.ReplaceLayer(m_RightDoor.gameObject, m_originalRightDoorLayer, GameLayer.IgnoreFullScreenEffects);
			LayerUtils.ReplaceLayer(m_Drawer.gameObject, m_originalDrawerLayer, GameLayer.IgnoreFullScreenEffects);
		}
	}

	public void PlayBoxMusic()
	{
		MusicManager.Get()?.StartPlaylist(m_activeMusicPlaylist);
	}

	public void PlayInnkeeperGreetings()
	{
		if (string.IsNullOrEmpty(m_eventInnkeeperGreetings.AssetString) && string.IsNullOrEmpty(m_defaultInnkeeperGreetings.AssetString))
		{
			Debug.LogError("Innkeeper greetings missing, assign a value to 'Default Innkeeper Greetings' on the Box");
			return;
		}
		string soundPrefabToPlay = (string.IsNullOrEmpty(m_eventInnkeeperGreetings.AssetString) ? m_defaultInnkeeperGreetings.AssetString : m_eventInnkeeperGreetings.AssetString);
		SoundManager.Get()?.LoadAndPlay(soundPrefabToPlay);
	}

	public void ActivateShopUIIfReady()
	{
		if (GameDownloadManagerProvider.Get() == null || !GameDownloadManagerProvider.Get().IsReadyToPlay)
		{
			Log.Box.PrintWarning("Aborting preloading Shop UI as game was not ready...");
		}
		else
		{
			m_mainShopWidget.gameObject.SetActive(value: true);
		}
	}

	public void ShowBGDownloadCompletePopupIfNecessary()
	{
		if (m_battlegroundsDownloadCompleteWidgetInstance != null && UniversalInputManager.Get().IsTouchMode() && GameDownloadManagerProvider.Get() != null && GameDownloadManagerProvider.Get().IsModuleReadyToPlay(DownloadTags.Content.Bgs))
		{
			m_battlegroundsDownloadCompleteWidgetInstance.gameObject.SetActive(value: true);
			Options.Get().SetBool(Option.HAS_SEEN_BG_DOWNLOAD_FINISHED_POPUP, val: true);
		}
	}

	private void InitializeStateConfigs()
	{
		int stateCount = Enum.GetValues(typeof(State)).Length;
		m_stateConfigs = new BoxStateConfig[stateCount];
		m_stateConfigs[1] = new BoxStateConfig
		{
			m_logoState = 
			{
				m_state = BoxLogo.State.HIDDEN
			},
			m_doorState = 
			{
				m_state = BoxDoor.State.CLOSED
			},
			m_diskState = 
			{
				m_state = BoxDisk.State.LOADING
			},
			m_drawerState = 
			{
				m_state = BoxDrawer.State.CLOSED
			},
			m_camState = 
			{
				m_state = BoxCamera.State.CLOSED
			},
			m_boxDressingState = 
			{
				m_state = EventBoxDressing.State.ENABLED
			}
		};
		m_stateConfigs[2] = new BoxStateConfig
		{
			m_logoState = 
			{
				m_state = BoxLogo.State.SHOWN
			},
			m_doorState = 
			{
				m_state = BoxDoor.State.CLOSED
			},
			m_diskState = 
			{
				m_state = BoxDisk.State.LOADING
			},
			m_drawerState = 
			{
				m_state = BoxDrawer.State.CLOSED
			},
			m_camState = 
			{
				m_state = BoxCamera.State.CLOSED
			},
			m_boxDressingState = 
			{
				m_state = EventBoxDressing.State.ENABLED
			}
		};
		m_stateConfigs[4] = new BoxStateConfig
		{
			m_logoState = 
			{
				m_state = BoxLogo.State.HIDDEN
			},
			m_doorState = 
			{
				m_state = BoxDoor.State.CLOSED
			},
			m_diskState = 
			{
				m_state = BoxDisk.State.LOADING
			},
			m_drawerState = 
			{
				m_state = BoxDrawer.State.CLOSED
			},
			m_camState = 
			{
				m_state = BoxCamera.State.CLOSED
			},
			m_boxDressingState = 
			{
				m_state = EventBoxDressing.State.ENABLED
			}
		};
		m_stateConfigs[3] = new BoxStateConfig
		{
			m_logoState = 
			{
				m_state = BoxLogo.State.HIDDEN
			},
			m_doorState = 
			{
				m_state = BoxDoor.State.CLOSED
			},
			m_diskState = 
			{
				m_state = BoxDisk.State.LOADING
			},
			m_drawerState = 
			{
				m_ignore = true
			},
			m_camState = 
			{
				m_ignore = true
			},
			m_boxDressingState = 
			{
				m_state = EventBoxDressing.State.DISABLED
			}
		};
		m_stateConfigs[5] = new BoxStateConfig
		{
			m_logoState = 
			{
				m_state = BoxLogo.State.HIDDEN
			},
			m_doorState = 
			{
				m_state = BoxDoor.State.CLOSED
			},
			m_diskState = 
			{
				m_state = BoxDisk.State.MAINMENU
			},
			m_drawerState = 
			{
				m_state = BoxDrawer.State.CLOSED
			},
			m_camState = 
			{
				m_state = BoxCamera.State.CLOSED
			},
			m_boxDressingState = 
			{
				m_state = EventBoxDressing.State.ENABLED
			}
		};
		m_stateConfigs[6] = new BoxStateConfig
		{
			m_logoState = 
			{
				m_state = BoxLogo.State.HIDDEN
			},
			m_doorState = 
			{
				m_state = BoxDoor.State.CLOSED
			},
			m_diskState = 
			{
				m_state = BoxDisk.State.MAINMENU
			},
			m_drawerState = 
			{
				m_state = BoxDrawer.State.OPENED
			},
			m_camState = 
			{
				m_state = BoxCamera.State.CLOSED_WITH_DRAWER
			},
			m_boxDressingState = 
			{
				m_state = EventBoxDressing.State.ENABLED
			}
		};
		m_stateConfigs[7] = new BoxStateConfig
		{
			m_logoState = 
			{
				m_state = BoxLogo.State.HIDDEN
			},
			m_doorState = 
			{
				m_state = BoxDoor.State.OPENED
			},
			m_diskState = 
			{
				m_state = BoxDisk.State.LOADING
			},
			m_drawerState = 
			{
				m_state = BoxDrawer.State.CLOSED_BOX_OPENED
			},
			m_camState = 
			{
				m_state = BoxCamera.State.OPENED
			},
			m_boxDressingState = 
			{
				m_state = EventBoxDressing.State.DISABLED
			}
		};
		m_stateConfigs[8] = new BoxStateConfig
		{
			m_logoState = 
			{
				m_state = BoxLogo.State.HIDDEN
			},
			m_doorState = 
			{
				m_state = BoxDoor.State.CLOSED
			},
			m_diskState = 
			{
				m_state = BoxDisk.State.LOADING
			},
			m_drawerState = 
			{
				m_state = BoxDrawer.State.CLOSED
			},
			m_camState = 
			{
				m_state = BoxCamera.State.CLOSED
			},
			m_boxDressingState = 
			{
				m_state = EventBoxDressing.State.ENABLED
			}
		};
		m_stateConfigs[9] = new BoxStateConfig
		{
			m_logoState = 
			{
				m_state = BoxLogo.State.HIDDEN
			},
			m_doorState = 
			{
				m_state = BoxDoor.State.CLOSED
			},
			m_diskState = 
			{
				m_state = BoxDisk.State.LOADING
			},
			m_drawerState = 
			{
				m_state = BoxDrawer.State.CLOSED
			},
			m_camState = 
			{
				m_state = BoxCamera.State.CLOSED
			},
			m_boxDressingState = 
			{
				m_state = EventBoxDressing.State.ENABLED
			}
		};
		m_stateConfigs[10] = new BoxStateConfig
		{
			m_logoState = 
			{
				m_state = BoxLogo.State.HIDDEN
			},
			m_doorState = 
			{
				m_state = BoxDoor.State.CLOSED
			},
			m_diskState = 
			{
				m_state = BoxDisk.State.LOADING
			},
			m_drawerState = 
			{
				m_state = BoxDrawer.State.CLOSED
			},
			m_camState = 
			{
				m_state = BoxCamera.State.CLOSED
			},
			m_boxDressingState = 
			{
				m_state = EventBoxDressing.State.ENABLED
			},
			m_stateAllowList = 
			{
				State.SET_ROTATION,
				State.SET_ROTATION_OPEN,
				State.ERROR
			}
		};
		m_stateConfigs[11] = new BoxStateConfig
		{
			m_logoState = 
			{
				m_state = BoxLogo.State.HIDDEN
			},
			m_doorState = 
			{
				m_state = BoxDoor.State.CLOSED
			},
			m_diskState = 
			{
				m_state = BoxDisk.State.MAINMENU
			},
			m_drawerState = 
			{
				m_state = BoxDrawer.State.CLOSED
			},
			m_camState = 
			{
				m_state = BoxCamera.State.CLOSED
			},
			m_boxDressingState = 
			{
				m_state = EventBoxDressing.State.ENABLED
			},
			m_stateAllowList = 
			{
				State.SET_ROTATION_OPEN,
				State.ERROR
			}
		};
		m_stateConfigs[12] = new BoxStateConfig
		{
			m_logoState = 
			{
				m_state = BoxLogo.State.HIDDEN
			},
			m_doorState = 
			{
				m_state = BoxDoor.State.OPENED
			},
			m_diskState = 
			{
				m_state = BoxDisk.State.LOADING
			},
			m_drawerState = 
			{
				m_state = BoxDrawer.State.CLOSED
			},
			m_camState = 
			{
				m_state = BoxCamera.State.OPENED
			},
			m_boxDressingState = 
			{
				m_state = EventBoxDressing.State.ENABLED
			}
		};
		m_stateConfigs[13] = new BoxStateConfig
		{
			m_logoState = 
			{
				m_state = BoxLogo.State.HIDDEN
			},
			m_doorState = 
			{
				m_state = BoxDoor.State.CLOSED
			},
			m_diskState = 
			{
				m_state = BoxDisk.State.LOADING
			},
			m_drawerState = 
			{
				m_state = BoxDrawer.State.CLOSED
			},
			m_camState = 
			{
				m_state = BoxCamera.State.CLOSED_WITH_DRAWER
			},
			m_boxDressingState = 
			{
				m_state = EventBoxDressing.State.DISABLED
			}
		};
		m_stateConfigs[14] = new BoxStateConfig
		{
			m_logoState = 
			{
				m_state = BoxLogo.State.HIDDEN
			},
			m_doorState = 
			{
				m_state = BoxDoor.State.CLOSED
			},
			m_diskState = 
			{
				m_state = BoxDisk.State.MAINMENU
			},
			m_drawerState = 
			{
				m_state = BoxDrawer.State.CLOSED
			},
			m_camState = 
			{
				m_state = BoxCamera.State.CLOSED_WITH_DRAWER
			},
			m_boxDressingState = 
			{
				m_state = EventBoxDressing.State.DISABLED
			}
		};
	}

	private void InitializeState()
	{
		m_state = State.STARTUP;
		bool cameFromTutorial = GameMgr.Get().WasTutorial() && !GameMgr.Get().WasSpectator();
		if (ServiceManager.TryGet<SceneMgr>(out var sceneMgr))
		{
			if (cameFromTutorial)
			{
				m_state = State.LOADING;
			}
			else
			{
				sceneMgr.RegisterScenePreUnloadEvent(OnScenePreUnload);
				sceneMgr.RegisterSceneLoadedEvent(OnSceneLoaded);
				m_state = TranslateSceneModeToBoxState(sceneMgr.GetMode());
			}
		}
		UpdateState();
		m_TopSpinner.Spin();
		m_BottomSpinner.Spin();
		if (cameFromTutorial)
		{
			LoadingScreen.Get().RegisterPreviousSceneDestroyedListener(OnTutorialSceneDestroyed);
			m_Camera.ChangeState(BoxCamera.State.CLOSED_TUTORIAL);
		}
		bool shouldShowRibbons = ShouldBeShowingState(State.HUB_WITH_DRAWER);
		ToggleRibbonUI(shouldShowRibbons);
		if (shouldShowRibbons)
		{
			m_journalButtonWidget.Show();
			if (m_ribbonButtons == null)
			{
				m_journalButtonWidget.TriggerEvent("ENABLE_INTERACTION");
			}
		}
	}

	private void OnTutorialSceneDestroyed(object userData)
	{
		LoadingScreen.Get().UnregisterPreviousSceneDestroyedListener(OnTutorialSceneDestroyed);
		Spell eventSpell = GetEventSpell(BoxEventType.TUTORIAL_PLAY);
		eventSpell.AddStateFinishedCallback(OnTutorialPlaySpellStateDeathFinished);
		eventSpell.ActivateState(SpellStateType.DEATH);
	}

	private void OnTutorialPlaySpellStateDeathFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() != 0)
		{
			return;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			ToggleRibbonUI(show: true);
			BnetBar bnetBar = BnetBar.Get();
			if (bnetBar != null)
			{
				bnetBar.HideSkipTutorialButton();
			}
		}
		SceneMgr sceneMgr = SceneMgr.Get();
		sceneMgr.RegisterScenePreUnloadEvent(OnScenePreUnload);
		sceneMgr.RegisterSceneLoadedEvent(OnSceneLoaded);
		ChangeStateToReflectSceneMode(SceneMgr.Get().GetMode(), isSceneActuallyLoaded: false);
		this.OnTutorialTransitionFinished?.Invoke();
	}

	private void ShutdownState()
	{
		if (m_StoreButton != null)
		{
			m_StoreButton.Unload();
		}
		if (ServiceManager.TryGet<SceneMgr>(out var sceneMgr))
		{
			sceneMgr.UnregisterSceneLoadedEvent(OnSceneLoaded);
			sceneMgr.UnregisterScenePreUnloadEvent(OnScenePreUnload);
		}
	}

	private void QueueStateChange(State state)
	{
		if ((m_stateQueue.Count != 0 || state != m_state) && (m_stateQueue.Count <= 0 || state != m_stateQueue.Last()))
		{
			if (m_railroadManager != null && state == m_railroadManager.GetOverriddenBoxState())
			{
				Log.Box.PrintWarning("Tried to queue state {0}, but the state was overriden by RailroadManager ({1}), so skipping it...", state, m_state);
			}
			else
			{
				m_stateQueue.Enqueue(state);
			}
		}
	}

	private void ChangeStateQueued()
	{
		State state = m_stateQueue.Dequeue();
		ChangeStateNow(state);
	}

	private void ChangeStateNow(State state)
	{
		if (!m_stateConfigs[(int)m_state].CanTransitionToState(state))
		{
			return;
		}
		bool shouldShowSetRotation = SetRotationManager.Get().ShouldShowSetRotationIntro();
		if (!shouldShowSetRotation)
		{
			if (m_setRotationDisk != null)
			{
				m_setRotationDisk.SetActive(value: false);
			}
			if (m_DiskCenter != null)
			{
				m_DiskCenter.SetActive(value: true);
			}
		}
		if (state == State.OPEN && shouldShowSetRotation)
		{
			state = State.SET_ROTATION_OPEN;
		}
		m_railroadManager.UpdateRailroadState();
		state = m_railroadManager.GetBoxStateOverride(state);
		m_state = state;
		TrackBoxInteractable();
		switch (state)
		{
		case State.STARTUP:
			ChangeState_Startup();
			break;
		case State.PRESS_START:
			ChangeState_PressStart();
			break;
		case State.LOADING_HUB:
			ChangeState_LoadingHub();
			break;
		case State.LOADING:
			ChangeState_Loading();
			break;
		case State.HUB:
			ChangeState_Hub();
			break;
		case State.HUB_WITH_DRAWER:
			ChangeState_HubWithDrawer();
			break;
		case State.OPEN:
			ChangeState_Open();
			break;
		case State.CLOSED:
			ChangeState_Closed();
			break;
		case State.ERROR:
			ChangeState_Error();
			break;
		case State.SET_ROTATION_LOADING:
			ChangeState_SetRotationLoading();
			break;
		case State.SET_ROTATION:
			ChangeState_SetRotation();
			break;
		case State.SET_ROTATION_OPEN:
			ChangeState_SetRotationOpen();
			break;
		default:
			if (m_state == State.APPRENTICE_OVERRIDE_CLOSED)
			{
				ChangeState_ApprenticeOverrideClosed();
			}
			else if (m_state == State.APPRENTICE_OVERRIDE_HUB)
			{
				ChangeState_ApprenticeOverrideHub();
			}
			else
			{
				Debug.LogError($"Box.ChangeStateNow() - unhandled state {state}");
			}
			break;
		}
		UpdateUIEvents();
	}

	private void ChangeStateToReflectSceneMode(SceneMgr.Mode mode, bool isSceneActuallyLoaded)
	{
		State state = TranslateSceneModeToBoxState(mode);
		bool setRotation = SetRotationManager.Get().ShouldShowSetRotationIntro();
		if (mode == SceneMgr.Mode.HUB && setRotation)
		{
			if (!m_stateQueue.Contains(State.SET_ROTATION_LOADING))
			{
				ChangeState(State.SET_ROTATION_LOADING);
				if (isSceneActuallyLoaded && m_setRotationIntroCoroutine == null)
				{
					m_setRotationIntroCoroutine = StartCoroutine(SetRotation_StartSetRotationIntro());
				}
			}
			m_railroadManager.ToggleBoxTutorials(setEnabled: false);
		}
		else if (mode == SceneMgr.Mode.TOURNAMENT && setRotation)
		{
			ChangeState(State.SET_ROTATION_OPEN);
			UserAttentionManager.StartBlocking(UserAttentionBlocker.SET_ROTATION_INTRO);
			m_transitioningToSceneMode = true;
			m_railroadManager.ToggleBoxTutorials(setEnabled: false);
		}
		else if (!SceneMgr.Get().IsDoingSceneDrivenTransition() && ChangeState(state))
		{
			Log.Box.PrintDebug($"Trying to change state to {state}");
			if (GameDownloadManagerProvider.Get().IsReadyToPlay)
			{
				Log.Box.PrintDebug("The download is done. Set to transitioning.");
				m_transitioningToSceneMode = true;
				bool shouldShowAllButtons = ShouldBeShowingState(State.HUB_WITH_DRAWER);
				m_railroadManager.ToggleBoxTutorials(shouldShowAllButtons);
			}
		}
		BoxLightStateType lightState = TranslateSceneModeToLightState(mode);
		m_LightMgr.ChangeState(lightState);
		if (state == State.HUB)
		{
			StartCoroutine(UpdateCameraForTutorialPreview());
		}
	}

	public void UpdateStateForCurrentSceneMode()
	{
		ChangeStateToReflectSceneMode(SceneMgr.Get().GetMode(), isSceneActuallyLoaded: true);
	}

	private IEnumerator UpdateCameraForTutorialPreview()
	{
		while (NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>() == null)
		{
			yield return null;
		}
		BoxCamera.State camState = ((!GameUtils.IsAnyTutorialComplete()) ? BoxCamera.State.CLOSED_TUTORIAL : BoxCamera.State.CLOSED);
		m_Camera.ChangeState(camState);
	}

	public void TryToStartSetRotationFromHub()
	{
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		bool setRotation = SetRotationManager.Get().ShouldShowSetRotationIntro();
		if (mode == SceneMgr.Mode.HUB && setRotation && !m_stateQueue.Contains(State.SET_ROTATION_LOADING))
		{
			ChangeState(State.SET_ROTATION_LOADING);
			if (m_setRotationIntroCoroutine == null)
			{
				m_setRotationIntroCoroutine = StartCoroutine(SetRotation_StartSetRotationIntro());
			}
		}
	}

	private State TranslateSceneModeToBoxState(SceneMgr.Mode mode)
	{
		switch (mode)
		{
		case SceneMgr.Mode.STARTUP:
			return State.STARTUP;
		case SceneMgr.Mode.LOGIN:
			return State.INVALID;
		case SceneMgr.Mode.HUB:
			if (!GameUtils.IsAnyTutorialComplete() || !GameDownloadManagerProvider.Get().IsReadyToPlay)
			{
				return State.LOADING;
			}
			return State.HUB_WITH_DRAWER;
		case SceneMgr.Mode.GAMEPLAY:
			return State.INVALID;
		case SceneMgr.Mode.FATAL_ERROR:
			return State.ERROR;
		default:
			return State.OPEN;
		}
	}

	private BoxLightStateType TranslateSceneModeToLightState(SceneMgr.Mode mode)
	{
		switch (mode)
		{
		case SceneMgr.Mode.LOGIN:
		case SceneMgr.Mode.GAMEPLAY:
			return BoxLightStateType.INVALID;
		case SceneMgr.Mode.TOURNAMENT:
			return BoxLightStateType.TOURNAMENT;
		case SceneMgr.Mode.COLLECTIONMANAGER:
		case SceneMgr.Mode.TAVERN_BRAWL:
		case SceneMgr.Mode.BACON_COLLECTION:
			return BoxLightStateType.COLLECTION;
		case SceneMgr.Mode.PACKOPENING:
			return BoxLightStateType.PACK_OPENING;
		case SceneMgr.Mode.FRIENDLY:
		case SceneMgr.Mode.ADVENTURE:
		case SceneMgr.Mode.BACON:
		case SceneMgr.Mode.GAME_MODE:
		case SceneMgr.Mode.PVP_DUNGEON_RUN:
		case SceneMgr.Mode.LETTUCE_VILLAGE:
		case SceneMgr.Mode.LETTUCE_BOUNTY_BOARD:
		case SceneMgr.Mode.LETTUCE_MAP:
		case SceneMgr.Mode.LETTUCE_PLAY:
		case SceneMgr.Mode.LETTUCE_COLLECTION:
		case SceneMgr.Mode.LETTUCE_PACK_OPENING:
			return BoxLightStateType.ADVENTURE;
		case SceneMgr.Mode.DRAFT:
			return BoxLightStateType.ARENA;
		default:
			return BoxLightStateType.DEFAULT;
		}
	}

	private void OnScenePreUnload(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		if (mode != SceneMgr.Mode.GAMEPLAY && mode != SceneMgr.Mode.STARTUP && mode != SceneMgr.Mode.RESET)
		{
			if (prevMode == SceneMgr.Mode.HUB)
			{
				ChangeState(State.LOADING);
				m_StoreButton.Unload();
			}
			else if (mode == SceneMgr.Mode.HUB)
			{
				m_StoreButton.Load();
				ChangeStateToReflectSceneMode(mode, isSceneActuallyLoaded: false);
				m_waitingForSceneLoad = true;
			}
			else if (ShouldUseLoadingHubState(mode, prevMode))
			{
				ChangeState(State.LOADING_HUB);
			}
			else if (!SceneMgr.Get().IsDoingSceneDrivenTransition())
			{
				ChangeState(State.LOADING);
			}
			ClearQueuedButtonFireEvent();
			UpdateUIEvents();
		}
	}

	private bool ShouldUseLoadingHubState(SceneMgr.Mode mode, SceneMgr.Mode prevMode)
	{
		if (mode == SceneMgr.Mode.FRIENDLY && prevMode != SceneMgr.Mode.HUB)
		{
			return true;
		}
		if (prevMode == SceneMgr.Mode.COLLECTIONMANAGER && (mode == SceneMgr.Mode.ADVENTURE || mode == SceneMgr.Mode.TOURNAMENT))
		{
			return true;
		}
		if (mode == SceneMgr.Mode.COLLECTIONMANAGER && (prevMode == SceneMgr.Mode.ADVENTURE || prevMode == SceneMgr.Mode.TOURNAMENT))
		{
			return true;
		}
		if ((prevMode == SceneMgr.Mode.BACON_COLLECTION && mode == SceneMgr.Mode.BACON) || (mode == SceneMgr.Mode.BACON_COLLECTION && prevMode == SceneMgr.Mode.BACON))
		{
			return true;
		}
		return false;
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		ChangeStateToReflectSceneMode(mode, isSceneActuallyLoaded: true);
		if (m_waitingForSceneLoad)
		{
			m_waitingForSceneLoad = false;
			if (m_queuedButtonFire.HasValue)
			{
				FireButtonPressEvent(m_queuedButtonFire.Value);
				m_queuedButtonFire = null;
			}
		}
	}

	private void ChangeState_Startup()
	{
		m_state = State.STARTUP;
		ChangeStateUsingConfig();
	}

	private void ChangeState_PressStart()
	{
		m_state = State.PRESS_START;
		ChangeStateUsingConfig();
	}

	private void ChangeState_SetRotationLoading()
	{
		m_state = State.SET_ROTATION_LOADING;
		ChangeStateUsingConfig();
	}

	private void ChangeState_SetRotation()
	{
		m_state = State.SET_ROTATION;
		ChangeStateUsingConfig();
	}

	private void ChangeState_SetRotationOpen()
	{
		m_state = State.SET_ROTATION_OPEN;
		StartCoroutine(SetRotationOpen_ChangeState());
	}

	private void ChangeState_ApprenticeOverrideClosed()
	{
		m_state = State.APPRENTICE_OVERRIDE_CLOSED;
		ChangeStateUsingConfig();
	}

	private void ChangeState_ApprenticeOverrideHub()
	{
		m_state = State.APPRENTICE_OVERRIDE_HUB;
		ChangeStateUsingConfig();
	}

	private void ChangeState_LoadingHub()
	{
		m_state = State.LOADING_HUB;
		ChangeStateUsingConfig();
	}

	private void ChangeState_Loading()
	{
		m_state = State.LOADING;
		ChangeStateUsingConfig();
	}

	private void ChangeState_Hub()
	{
		m_state = State.HUB;
		UpdateUI();
		ChangeStateUsingConfig();
		InitializeTutorialPreviewController();
	}

	private void ChangeState_HubWithDrawer()
	{
		m_state = State.HUB_WITH_DRAWER;
		UpdateUI();
		m_Camera.EnableAccelerometer();
		ChangeStateUsingConfig();
		InitializeTutorialPreviewController();
	}

	private void ChangeState_Open()
	{
		m_state = State.OPEN;
		ChangeStateUsingConfig();
	}

	private void ChangeState_Closed()
	{
		m_state = State.CLOSED;
		ChangeStateUsingConfig();
	}

	private void ChangeState_Error()
	{
		m_state = State.ERROR;
		ChangeStateUsingConfig();
	}

	private void UpdateState_Startup()
	{
		m_state = State.STARTUP;
		UpdateStateUsingConfig();
	}

	private void UpdateState_PressStart()
	{
		m_state = State.PRESS_START;
		UpdateStateUsingConfig();
	}

	private void UpdateState_SetRotationLoading()
	{
		m_state = State.SET_ROTATION_LOADING;
		UpdateStateUsingConfig();
	}

	private void UpdateState_SetRotation()
	{
		m_state = State.SET_ROTATION;
		UpdateStateUsingConfig();
	}

	private void UpdateState_SetRotationOpen()
	{
		m_state = State.SET_ROTATION_OPEN;
		UpdateStateUsingConfig();
	}

	private void UpdateState_ApprenticeOverrideClosed()
	{
		m_state = State.APPRENTICE_OVERRIDE_CLOSED;
		UpdateStateUsingConfig();
	}

	private void UpdateState_ApprenticeOverrideHub()
	{
		m_state = State.APPRENTICE_OVERRIDE_HUB;
		UpdateStateUsingConfig();
	}

	private void UpdateState_LoadingHub()
	{
		m_state = State.LOADING_HUB;
		UpdateStateUsingConfig();
	}

	private void UpdateState_Loading()
	{
		m_state = State.LOADING;
		UpdateStateUsingConfig();
	}

	private void UpdateState_Hub()
	{
		m_state = State.HUB;
		UpdateUI();
		UpdateStateUsingConfig();
	}

	private void UpdateState_HubWithDrawer()
	{
		m_state = State.HUB_WITH_DRAWER;
		m_Camera.EnableAccelerometer();
		UpdateStateUsingConfig();
	}

	private void UpdateState_Open()
	{
		m_state = State.OPEN;
		UpdateStateUsingConfig();
	}

	private void UpdateState_Closed()
	{
		m_state = State.CLOSED;
		UpdateStateUsingConfig();
	}

	private void UpdateState_Error()
	{
		m_state = State.ERROR;
		UpdateStateUsingConfig();
	}

	private void ChangeStateUsingConfig()
	{
		BoxStateConfig config = m_stateConfigs[(int)m_state];
		if (!config.m_logoState.m_ignore)
		{
			m_Logo.ChangeState(config.m_logoState.m_state);
		}
		if (!config.m_doorState.m_ignore)
		{
			m_LeftDoor.ChangeState(config.m_doorState.m_state);
			m_RightDoor.ChangeState(config.m_doorState.m_state);
		}
		if (!config.m_diskState.m_ignore)
		{
			m_Disk.ChangeState(config.m_diskState.m_state);
		}
		if (!config.m_drawerState.m_ignore)
		{
			if (!UniversalInputManager.UsePhoneUI)
			{
				m_Drawer.ChangeState(config.m_drawerState.m_state);
			}
			else if (!ShouldBeShowingState(State.HUB_WITH_DRAWER))
			{
				ToggleRibbonUI(show: false);
			}
		}
		if (!config.m_camState.m_ignore)
		{
			m_Camera.ChangeState(config.m_camState.m_state);
		}
		UpdateEventBoxDressingWithConfig();
	}

	private void ToggleRibbonUI(bool show)
	{
		if (!(m_ribbonButtons == null))
		{
			m_ribbonButtons.UpdateRibbons(show);
			m_showRibbonButtons = show;
		}
	}

	public void UpdateRibbonUI()
	{
		m_ribbonButtons.UpdateRibbons(m_showRibbonButtons);
	}

	private void UpdateStateUsingConfig()
	{
		BoxStateConfig config = m_stateConfigs[(int)m_state];
		if (!config.m_logoState.m_ignore)
		{
			m_Logo.UpdateState(config.m_logoState.m_state);
		}
		if (!config.m_storeButtonState.m_ignore)
		{
			m_StoreButton.UpdateState(config.m_storeButtonState.m_state);
		}
		if (!config.m_doorState.m_ignore)
		{
			m_LeftDoor.ChangeState(config.m_doorState.m_state);
			m_RightDoor.ChangeState(config.m_doorState.m_state);
		}
		if (!config.m_diskState.m_ignore)
		{
			m_Disk.UpdateState(config.m_diskState.m_state);
		}
		m_TopSpinner.Reset();
		m_BottomSpinner.Reset();
		if (!config.m_drawerState.m_ignore)
		{
			m_Drawer.UpdateState(config.m_drawerState.m_state);
		}
		if (!config.m_camState.m_ignore)
		{
			m_Camera.UpdateState(config.m_camState.m_state);
		}
	}

	private void FireTransitionStartedEvent()
	{
		TransitionStartedListener[] listeners = m_transitionStartedListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire();
		}
	}

	private void FireTransitionFinishedEvent()
	{
		TransitionFinishedListener[] listeners = m_transitionFinishedListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire();
		}
	}

	private void InitializeUI()
	{
		PegUI.Get().AddInputCamera(m_Camera.GetComponent<Camera>());
		m_boxWidgetRef.RegisterReadyListener<Widget>(BoxWidgetIsReady);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_Drawer.gameObject.SetActive(value: false);
			m_ribbonButtons.m_collectionManagerRibbon.AddEventListener(UIEventType.RELEASE, OnCollectionButtonPressed);
			m_ribbonButtons.m_packOpeningRibbon.AddEventListener(UIEventType.RELEASE, OnOpenPacksButtonPressed);
			m_ribbonButtons.m_questLogRibbon.AddEventListener(UIEventType.RELEASE, OnQuestButtonPressed);
			m_ribbonButtons.m_storeRibbon.AddEventListener(UIEventType.RELEASE, OnStoreButtonReleased);
		}
		else
		{
			m_OpenPacksButton.SetText(GameStrings.Get("GLUE_OPEN_PACKS"));
			m_CollectionButton.SetText(GameStrings.Get("GLUE_MY_COLLECTION"));
			m_QuestLogButton.AddEventListener(UIEventType.RELEASE, OnQuestButtonPressed);
			m_StoreButton.AddEventListener(UIEventType.RELEASE, OnStoreButtonReleased);
		}
		ActivateShopUIIfReady();
		RegisterButtonEvents(m_OpenPacksButton);
		RegisterButtonEvents(m_CollectionButton);
		RegisterButtonEvents(m_PlayButton);
		RegisterButtonEvents(m_BattleGroundsButton);
		RegisterButtonEvents(m_TavernBrawlButton);
		RegisterButtonEvents(m_GameModesButton);
		SetupButtonText();
		UpdateUI();
	}

	private void InitializeComponents()
	{
		m_Logo.SetParent(this);
		m_Logo.SetInfo(m_StateInfoList.m_LogoInfo);
		m_LeftDoor.SetParent(this);
		m_LeftDoor.SetInfo(m_StateInfoList.m_LeftDoorInfo);
		m_RightDoor.SetParent(this);
		m_RightDoor.SetInfo(m_StateInfoList.m_RightDoorInfo);
		m_RightDoor.EnableMain(enable: true);
		m_Disk.SetParent(this);
		m_Disk.SetInfo(m_StateInfoList.m_DiskInfo);
		m_TopSpinner.SetParent(this);
		m_TopSpinner.SetInfo(m_StateInfoList.m_SpinnerInfo);
		m_BottomSpinner.SetParent(this);
		m_BottomSpinner.SetInfo(m_StateInfoList.m_SpinnerInfo);
		m_Drawer.SetParent(this);
		m_Drawer.SetInfo(m_StateInfoList.m_DrawerInfo);
		m_Camera.SetParent(this);
		m_Camera.SetInfo(m_StateInfoList.m_CameraInfo);
	}

	public void UpdateUI()
	{
		UpdateUIState();
		UpdateUIEvents();
	}

	private void UpdateUIState()
	{
		if (!GameDownloadManagerProvider.Get().IsCompletedInitialBaseDownload())
		{
			m_newPlayerModeBanner.SetActive(value: false);
		}
		else if (m_waitingForNetData)
		{
			SetPackCount(-1);
			HighlightButton(m_OpenPacksButton, highlightOn: false);
			HighlightButton(m_PlayButton, highlightOn: false);
			HighlightButton(m_BattleGroundsButton, highlightOn: false);
			HighlightButton(m_CollectionButton, highlightOn: false);
			HighlightButton(m_GameModesButton, highlightOn: false);
			HighlightButton(m_TavernBrawlButton, highlightOn: false);
			HideGameModesButton();
			m_newPlayerModeBanner.SetActive(value: false);
		}
		else
		{
			NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
			int boosterCount = BoosterPackUtils.GetTotalBoosterCount();
			SetPackCount(boosterCount);
			bool highlightOpenPacks = boosterCount > 0 && !Options.Get().GetBool(Option.HAS_SEEN_PACK_OPENING, defaultVal: false);
			HighlightButton(m_OpenPacksButton, highlightOpenPacks);
			bool num = UpdateModesButton();
			UpdateTavernBrawlButton();
			bool isHearthstoneButtonHighlighted = (!num && CollectionManager.Get().ShouldSeeTwistModeNotification()) || RewardTrackManager.Get().DidJustCompleteApprentice;
			HighlightButton(m_PlayButton, isHearthstoneButtonHighlighted);
			bool shouldHighlightCollectionAfterPractice = GameModeUtils.HasPlayedAPracticeMatch() && !Options.Get().GetBool(Option.HAS_SEEN_COLLECTIONMANAGER_AFTER_PRACTICE, defaultVal: false);
			bool highlightCollection = !num && !isHearthstoneButtonHighlighted && features.Collection.Manager && shouldHighlightCollectionAfterPractice;
			HighlightButton(m_CollectionButton, highlightCollection);
			ToggleDrawerButtonState(features.Collection.Manager, m_CollectionButton);
			SetupNewPlayerBanner();
		}
	}

	private void SetupNewPlayerBanner()
	{
		if (m_state != State.HUB)
		{
			m_newPlayerModeBanner.SetActive(value: false);
			return;
		}
		bool isNewPlayer = !GameUtils.IsAnyTutorialComplete();
		bool inTutorialMission = GameUtils.GetNextTutorial() > 5287;
		m_newPlayerModeBanner.SetActive(isNewPlayer && !inTutorialMission);
	}

	private bool UpdateModesButton()
	{
		if (!GameUtils.IsTraditionalTutorialComplete())
		{
			HideGameModesButton();
			return false;
		}
		ShowGameModesButton();
		m_GameModesButton.SetText(GameStrings.Get("GLUE_GAME_MODES"));
		if (m_boxWidget == null)
		{
			return false;
		}
		string newAdventureBadgeStateName = (GameModeDisplay.ShouldSeeNewSoloAdventureBanner() ? "SHOW_NEW_GAME_MODE_BADGE" : "HIDE_NEW_GAME_MODE_BADGE");
		m_boxWidget.TriggerEvent(newAdventureBadgeStateName);
		return false;
	}

	private void UpdateTavernBrawlButton()
	{
		if (!(m_boxWidget == null))
		{
			string newTavernBrawlBadgeStateName = (GameModeDisplay.ShouldSeeNewTavernBrawlBanner() ? "SHOW_NEW_TAVERN_BRAWL_BADGE" : "HIDE_NEW_TAVERN_BRAWL_BADGE");
			m_boxWidget.TriggerEvent(newTavernBrawlBadgeStateName);
		}
	}

	private void BoxWidgetIsReady(Widget widget)
	{
		m_boxWidget = widget;
	}

	private bool IsCollectionReady()
	{
		if (CollectionManager.Get() != null)
		{
			return CollectionManager.Get().IsFullyLoaded();
		}
		return false;
	}

	private IEnumerator UpdateUIWhenCollectionReady()
	{
		while (!IsCollectionReady())
		{
			yield return null;
		}
		UpdateUI();
	}

	public void DisableAllButtons()
	{
		SetBoxButtonInteractions(shouldEnable: false, m_PlayButton, shouldUpdateText: true);
		SetBoxButtonInteractions(shouldEnable: false, m_BattleGroundsButton, shouldUpdateText: true);
		SetBoxButtonInteractions(shouldEnable: false, m_GameModesButton, shouldUpdateText: true);
		SetBoxButtonInteractions(shouldEnable: false, m_TavernBrawlButton, shouldUpdateText: true);
		DisableButton(m_setRotationButton);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			DisableButton(m_ribbonButtons.m_collectionManagerRibbon);
			DisableButton(m_ribbonButtons.m_packOpeningRibbon);
			DisableButton(m_ribbonButtons.m_questLogRibbon);
			DisableButton(m_ribbonButtons.m_storeRibbon);
		}
		else
		{
			DisableButton(m_OpenPacksButton);
			DisableButton(m_CollectionButton);
			DisableButton(m_QuestLogButton);
			DisableButton(m_StoreButton);
			m_journalButtonWidget.TriggerEvent("DISABLE_INTERACTION");
		}
		ToggleButtonTextureState(enabled: false, m_PlayButton);
		ToggleButtonTextureState(enabled: false, m_BattleGroundsButton);
		ToggleButtonTextureState(enabled: false, m_GameModesButton);
		ToggleButtonTextureState(enabled: false, m_TavernBrawlButton);
		ToggleDrawerButtonState(enabled: false, m_OpenPacksButton);
		ToggleDrawerButtonState(enabled: false, m_CollectionButton);
		ToggleButtonTextureState(enabled: false, m_setRotationButton);
	}

	private bool CanEnableUIEvents()
	{
		if (HasPendingEffects())
		{
			return false;
		}
		if (m_stateQueue.Count > 0)
		{
			return false;
		}
		if (m_state == State.INVALID || m_state == State.STARTUP || m_state == State.LOADING || m_state == State.LOADING_HUB || m_state == State.OPEN)
		{
			return false;
		}
		return true;
	}

	private void ToggleButtonTextureState(bool enabled, BoxMenuButton button)
	{
		if (!(button == null))
		{
			if (enabled)
			{
				button.m_TextMesh.TextColor = m_EnabledColor;
			}
			else
			{
				button.m_TextMesh.TextColor = m_DisabledColor;
			}
		}
	}

	private void ToggleDrawerButtonState(bool enabled, BoxMenuButton button)
	{
		if (!(button == null))
		{
			if (enabled)
			{
				button.m_TextMesh.TextColor = m_EnabledDrawerMaterial;
			}
			else
			{
				button.m_TextMesh.TextColor = m_DisabledDrawerMaterial;
			}
		}
	}

	private void HighlightButton(BoxMenuButton button, bool highlightOn)
	{
		if (button.m_HighlightState == null)
		{
			Debug.LogWarning($"Box.HighlighButton {button} - highlight state is null");
			return;
		}
		ActorStateType highlightState = (highlightOn ? ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE : ActorStateType.HIGHLIGHT_OFF);
		button.m_HighlightState.ChangeState(highlightState);
	}

	private bool IsButtonHighlighted(BoxMenuButton button)
	{
		return button.m_HighlightState.CurrentState == ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE;
	}

	private void SetRotation_ShowRotationDisk()
	{
		if (m_DiskCenter != null)
		{
			m_DiskCenter.SetActive(value: false);
		}
		if (m_setRotationDisk != null)
		{
			m_setRotationDisk.SetActive(value: true);
			return;
		}
		m_StoreButton.gameObject.SetActive(value: false);
		m_QuestLogButton.gameObject.SetActive(value: false);
		m_journalButtonWidget.Hide();
		m_setRotationDisk = AssetLoader.Get().InstantiatePrefab("TheBox_CenterDisk_SetRotation.prefab:6f2fa714f0d129e4197fd2922f544816");
		m_setRotationDisk.SetActive(value: true);
		m_setRotationDisk.transform.parent = m_Disk.transform;
		m_setRotationDisk.transform.localPosition = Vector3.zero;
		m_setRotationDisk.transform.localRotation = Quaternion.identity;
		EventBoxDressing boxDressing = m_eventBoxDressingWidget?.GetComponentInChildren<EventBoxDressing>();
		CenterDiskSetRotation centerDisk = m_setRotationDisk.GetComponent<CenterDiskSetRotation>();
		if (boxDressing != null && centerDisk != null)
		{
			centerDisk.ApplyBoxDressingMaterials(boxDressing.GetBoxDressingMaterials());
		}
		m_setRotationButton = m_setRotationDisk.GetComponentInChildren<BoxMenuButton>();
		HighlightState highlight = m_setRotationButton.GetComponentInChildren<HighlightState>();
		if (highlight != null)
		{
			highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
		}
		RegisterButtonEvents(m_setRotationButton);
	}

	private IEnumerator SetRotationOpen_ChangeState()
	{
		BoxStateConfig config = m_stateConfigs[12];
		if (!config.m_logoState.m_ignore)
		{
			m_Logo.ChangeState(config.m_logoState.m_state);
		}
		if (!config.m_doorState.m_ignore)
		{
			m_LeftDoor.ChangeState(config.m_doorState.m_state);
			m_RightDoor.ChangeState(config.m_doorState.m_state);
		}
		if (!config.m_diskState.m_ignore)
		{
			m_Disk.ChangeState(config.m_diskState.m_state);
		}
		if (!config.m_camState.m_ignore)
		{
			m_Camera.ChangeState(BoxCamera.State.SET_ROTATION_OPENED);
		}
		SetRotationClock clock = SetRotationClock.Get();
		if (clock == null)
		{
			Debug.LogError("SetRotationOpen_ChangeState clock = null");
		}
		else
		{
			clock.StartTheClock();
		}
		yield break;
	}

	private IEnumerator SetRotation_StartSetRotationIntro()
	{
		ResetSetRotationPopupProgress();
		UserAttentionManager.StartBlocking(UserAttentionBlocker.SET_ROTATION_INTRO);
		NotificationManager.Get().DestroyAllPopUps();
		PopupDisplayManager.Get().ReadyToShowPopups();
		yield return StartCoroutine(PopupDisplayManager.Get().WaitForAllPopups());
		SetRotation_FinishShowingRewards();
		m_setRotationIntroCoroutine = null;
	}

	private void SetRotation_ShowNerfedCards_DialogHidden(DialogBase dialog, object userData)
	{
		SetRotation_FinishShowingRewards();
	}

	private void SetRotation_FinishShowingRewards()
	{
		ChangeState(State.SET_ROTATION);
		SetRotation_ShowRotationDisk();
	}

	public void ToggleSkipApprenticeLoading(bool isLoading)
	{
		if (isLoading)
		{
			ChangeState(State.LOADING_HUB);
		}
		else
		{
			UpdateStateForCurrentSceneMode();
		}
	}

	private void ShowEventBoxDressing()
	{
		if (m_eventBoxDressingWidget == null || m_eventBoxDressingActive)
		{
			return;
		}
		EventTimingManager evt = EventTimingManager.Get();
		if (evt == null)
		{
			return;
		}
		EventTimingManager.Get().OnReceivedEventTimingsFromServer -= ShowEventBoxDressing;
		if (!evt.HasReceivedEventTimingsFromServer)
		{
			EventTimingManager.Get().OnReceivedEventTimingsFromServer += ShowEventBoxDressing;
			return;
		}
		BoxDressingDataModel boxDressing = new BoxDressingDataModel();
		CatchupPackEventDbfRecord currentCatchupPackEvent = CatchupPackEventManager.GetCurrentCatchupPackBoxDressingEvent();
		if (currentCatchupPackEvent != null)
		{
			boxDressing.Type = currentCatchupPackEvent.CatchupEventType.ToString();
		}
		else
		{
			SpecialEventDataModel currentEvent = SpecialEventManager.Get()?.GetEventDataModelForCurrentEvent();
			if (currentEvent != null)
			{
				boxDressing.Type = currentEvent.SpecialEventType.ToString();
			}
		}
		Spawnable eventSpawnable = m_eventBoxDressingWidget?.GetComponentInChildren<Spawnable>();
		bool num = SceneMgr.Get().GetMode() == SceneMgr.Mode.HUB || SceneMgr.Get().GetMode() == SceneMgr.Mode.LOGIN;
		bool tutorialHidesBoxDressing = GameUtils.CanCheckTutorialCompletion() && !GameUtils.IsAnyTutorialComplete();
		if (!num || tutorialHidesBoxDressing || string.IsNullOrEmpty(boxDressing.Type) || eventSpawnable == null)
		{
			this.OnBoxDressingReadyOnce?.Invoke();
			this.OnBoxDressingReadyOnce = null;
			return;
		}
		eventSpawnable.RegisterDoneChangingStatesListener(delegate
		{
			EventBoxDressing eventBoxDressing = m_eventBoxDressingWidget?.GetComponentInChildren<EventBoxDressing>();
			if (eventBoxDressing != null)
			{
				ApplyBoxDressingMaterials(eventBoxDressing.GetBoxDressingMaterials());
				MusicPlaylistType playlistType = eventBoxDressing.GetPlaylistType();
				if (playlistType != 0 && playlistType != MusicPlaylistType.UI_MainTitle)
				{
					m_activeMusicPlaylist = playlistType;
				}
				if (!string.IsNullOrEmpty(eventBoxDressing.GetInnkeeperGreetings().AssetString))
				{
					m_eventInnkeeperGreetings = eventBoxDressing.GetInnkeeperGreetings();
				}
			}
			m_eventBoxDressingWidget?.TriggerEvent("EVENT_BOX_DRESSING_BIRTH", TriggerEventParameters.StandardPropagateDownward);
			this.OnBoxDressingReadyOnce?.Invoke();
			this.OnBoxDressingReadyOnce = null;
		}, null, callImmediatelyIfSet: true, doOnce: true);
		m_eventBoxDressingWidget?.BindDataModel(boxDressing);
		m_eventBoxDressingActive = true;
	}

	private void HideEventBoxDressing()
	{
		if (m_eventBoxDressingActive)
		{
			m_eventBoxDressingWidget.TriggerEvent("EVENT_BOX_DRESSING_DEATH", TriggerEventParameters.StandardPropagateDownward);
			m_eventBoxDressingActive = false;
		}
	}

	private void OnRewardTracksReceived()
	{
		UpdateEventBoxDressingWithConfig();
		RewardTrackManager.Get().OnRewardTracksReceived -= OnRewardTracksReceived;
	}

	private void OnGameSaveDataUpdate(GameSaveKeyId key)
	{
		if (key == GameSaveKeyId.PLAYER_FLAGS)
		{
			UpdateUI();
			if (SetRotationManager.Get().ShouldShowSetRotationIntro())
			{
				UpdateStateForCurrentSceneMode();
			}
		}
	}

	private void UpdateEventBoxDressingWithConfig()
	{
		BoxStateConfig config = m_stateConfigs[(int)m_state];
		if (!config.m_boxDressingState.m_ignore)
		{
			if (config.m_boxDressingState.m_state == EventBoxDressing.State.ENABLED)
			{
				ShowEventBoxDressing();
			}
			else
			{
				HideEventBoxDressing();
			}
		}
	}

	private void ApplyMaterialToRenderer(Renderer renderer, Material material)
	{
		if (renderer != null && material != null)
		{
			renderer.SetMaterial(material);
		}
	}

	private void ApplyBoxDressingMaterials(EventBoxDressing.BoxDressingMaterials materials)
	{
		if (materials == null)
		{
			return;
		}
		if (m_tableTop != null && materials.TableMaterial != null)
		{
			ApplyMaterialToRenderer(m_tableTop.GetComponent<Renderer>(), materials.TableMaterial);
		}
		if (m_BottomSpinner != null && materials.BottomSpinnerMaterial != null)
		{
			ApplyMaterialToRenderer(m_BottomSpinner.GetComponent<Renderer>(), materials.BottomSpinnerMaterial);
			m_BottomSpinner.MaterialChanged();
		}
		ApplyMaterialToRenderer(m_SpotLightRenderer, materials.SpotLightMaterial);
		if (!(materials.BoxMaterial == null))
		{
			if (m_LeftDoor != null)
			{
				ApplyMaterialToRenderer(m_LeftDoor.GetComponent<Renderer>(), materials.BoxMaterial);
			}
			if (m_RightDoor != null)
			{
				ApplyMaterialToRenderer(m_RightDoor.GetComponent<Renderer>(), materials.BoxMaterial);
			}
			if (m_DiskCenter != null)
			{
				ApplyMaterialToRenderer(m_DiskCenter.GetComponent<Renderer>(), materials.BoxMaterial);
			}
			if (m_Drawer != null)
			{
				ApplyMaterialToRenderer(m_Drawer.GetComponent<Renderer>(), materials.BoxMaterial);
			}
			if (m_CollectionButton != null)
			{
				ApplyMaterialToRenderer(m_CollectionButton.GetComponent<Renderer>(), materials.BoxMaterial);
			}
			if (m_OpenPacksButton != null)
			{
				ApplyMaterialToRenderer(m_OpenPacksButton.GetComponent<Renderer>(), materials.BoxMaterial);
			}
			ApplyMaterialToRenderer(m_FirstButtonRenderer, materials.BoxMaterial);
			ApplyMaterialToRenderer(m_SecondButtonRenderer, materials.BoxMaterial);
			ApplyMaterialToRenderer(m_ThirdButtonRenderer, materials.BoxMaterial);
			ApplyMaterialToRenderer(m_FourthButtonRenderer, materials.BoxMaterial);
			if (m_EmptyFourthButton != null)
			{
				ApplyMaterialToRenderer(m_EmptyFourthButton.GetComponent<Renderer>(), materials.BoxMaterial);
			}
			OnMaterialsUpdated();
		}
	}

	private void OnBoxTopPhoneTextureLoaded(AssetReference assetRef, AssetHandle<Texture> newTexture, object callbackData)
	{
		AssetHandle.Take(ref m_boxTopTexture, newTexture);
		MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer renderer in componentsInChildren)
		{
			Material material = renderer.GetSharedMaterial();
			if (material != null && material.HasProperty("_MainTex"))
			{
				Texture texture = material.mainTexture;
				if (texture != null && texture.name.Equals("TheBox_Top"))
				{
					renderer.GetMaterial().mainTexture = newTexture;
				}
			}
		}
		OnMaterialsUpdated();
	}

	private void OnMaterialsUpdated()
	{
		if (m_materialDependentComponents != null)
		{
			DisableMesh_ColorBlack[] materialDependentComponents = m_materialDependentComponents;
			for (int i = 0; i < materialDependentComponents.Length; i++)
			{
				materialDependentComponents[i].HandleMaterialChanged();
			}
		}
	}

	public BoxRailroadManager GetRailroadManager()
	{
		return m_railroadManager;
	}

	public bool ShouldBeShowingState(State state)
	{
		if (m_state != state)
		{
			return m_railroadManager.GetOverriddenBoxState() == state;
		}
		return true;
	}

	public void RegisterButtonEvents(PegUIElement button)
	{
		button.AddEventListener(UIEventType.RELEASE, OnButtonPressed);
		button.AddEventListener(UIEventType.ROLLOVER, OnButtonMouseOver);
		button.AddEventListener(UIEventType.ROLLOUT, OnButtonMouseOut);
	}

	public void SetButtonEnabled(bool shouldEnable, BoxMenuButton button, bool shouldUpdateText = false, bool forceDisableText = false)
	{
		bool disabledByRailroading = m_railroadManager != null && m_railroadManager.ShouldDisableButton(button);
		if (shouldEnable && !disabledByRailroading)
		{
			EnableButton(button);
			button.SetButtonMaterial(m_ButtonEnabledMaterial);
		}
		else
		{
			DisableButton(button);
			button.SetButtonMaterial(m_ButtonDisabledMaterial);
		}
		if (shouldUpdateText)
		{
			ToggleButtonTextureState(shouldEnable && !forceDisableText, button);
		}
	}

	public void EnableBattlegroundsButton()
	{
		SetBoxButtonInteractions(shouldEnable: true, m_BattleGroundsButton, shouldUpdateText: true);
	}

	public void SetBoxButtonInteractions(bool shouldEnable, BoxMenuButton button, bool shouldUpdateText = false)
	{
		bool num = m_railroadManager != null && m_railroadManager.ShouldDisableButton(button);
		if (!button.IsEnabled())
		{
			EnableButton(button);
		}
		if (shouldUpdateText)
		{
			ToggleButtonTextureState(shouldEnable, button);
		}
		if (num)
		{
			OnPressDisabledButtons.Add(button);
			OnHoverDisabledButtons.Add(button);
			button.SetEnabled(UIEventType.ROLLOVER, enabled: false);
			button.SetEnabled(UIEventType.ROLLOUT, enabled: false);
			button.DisableHoverSpell();
			button.DisablePressSpell();
			button.SetButtonMaterial(m_ButtonDisabledMaterial);
			if (shouldUpdateText)
			{
				ToggleButtonTextureState(enabled: false, button);
			}
		}
		else if (!shouldEnable)
		{
			OnPressDisabledButtons.Add(button);
			OnHoverDisabledButtons.Remove(button);
			button.SetEnabled(UIEventType.ROLLOVER, enabled: true);
			button.SetEnabled(UIEventType.ROLLOUT, enabled: true);
			button.DisableHoverSpell();
			button.DisablePressSpell();
			button.SetButtonMaterial(m_ButtonDisabledMaterial);
		}
		else
		{
			OnPressDisabledButtons.Remove(button);
			OnHoverDisabledButtons.Remove(button);
			button.SetEnabled(UIEventType.ROLLOVER, enabled: true);
			button.SetEnabled(UIEventType.ROLLOUT, enabled: true);
			button.EnableHoverSpell();
			button.EnablePressSpell();
			button.SetButtonMaterial(m_ButtonEnabledMaterial);
		}
	}

	public void EnableButton(PegUIElement button)
	{
		button.SetEnabled(enabled: true);
		PegUIElement ribbonButton = GetRibbonButtonFromButton(button);
		if (ribbonButton != null && ribbonButton != button)
		{
			EnableButton(ribbonButton);
		}
	}

	public void DisableButton(PegUIElement button)
	{
		if (!(button == null))
		{
			button.SetEnabled(enabled: false);
			TooltipZone tooltipZone = button.GetComponent<TooltipZone>();
			if (tooltipZone != null)
			{
				tooltipZone.HideTooltip();
			}
			PegUIElement ribbonButton = GetRibbonButtonFromButton(button);
			if (ribbonButton != null && ribbonButton != button)
			{
				DisableButton(ribbonButton);
			}
		}
	}

	private void ShowGameModesButton()
	{
		m_GameModesButton.gameObject.SetActive(value: true);
		m_EmptyFourthButton.gameObject.SetActive(value: false);
	}

	private void HideGameModesButton()
	{
		m_GameModesButton.gameObject.SetActive(value: false);
		m_EmptyFourthButton.gameObject.SetActive(value: true);
	}

	protected virtual void SetupButtonText()
	{
		m_PlayButton.SetText(GameStrings.Get("GLUE_TRADITIONAL"));
		m_BattleGroundsButton.SetText(GameStrings.Get("GLUE_BACON"));
		m_TavernBrawlButton.SetText(GameStrings.Get("GLOBAL_TAVERN_BRAWL"));
		m_GameModesButton.SetText(GameStrings.Get("GLUE_GAME_MODES"));
	}

	private void OnButtonPressed(UIEvent e)
	{
		PegUIElement button = e.GetElement();
		BoxMenuButton boxButton = (BoxMenuButton)button;
		if (!OnPressDisabledButtons.Contains(boxButton) && (IsInStateWithButtons() || (!(button == m_PlayButton) && !(button == m_BattleGroundsButton) && !(button == m_TavernBrawlButton) && !(button == m_GameModesButton))))
		{
			NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
			bool tournamentEnabled = false;
			bool collectionEnabled = false;
			if (features != null)
			{
				tournamentEnabled = features.Games.Tournament;
				collectionEnabled = features.Collection.Manager;
			}
			if (m_newPlayerModeBanner.activeSelf && boxButton != m_TavernBrawlButton)
			{
				m_newPlayerModeBanner.SetActive(value: false);
			}
			if (boxButton == m_PlayButton && tournamentEnabled)
			{
				OnTraditionalModeButtonPressed(e);
			}
			else if (boxButton == m_BattleGroundsButton)
			{
				OnBattleGroundsButtonPressed(e);
			}
			else if (boxButton == m_GameModesButton)
			{
				OnModesButtonPressed(e);
			}
			else if (boxButton == m_TavernBrawlButton)
			{
				OnTavernBrawlButtonPressed(e);
			}
			else if (boxButton == m_OpenPacksButton)
			{
				OnOpenPacksButtonPressed(e);
			}
			else if (boxButton == m_CollectionButton && collectionEnabled)
			{
				OnCollectionButtonPressed(e);
			}
			else if (boxButton == m_setRotationButton)
			{
				OnSetRotationButtonPressed(e);
			}
		}
	}

	private void OnButtonMouseOver(UIEvent e)
	{
		PegUIElement button = e.GetElement();
		if (OnHoverDisabledButtons.Contains(button) || (!IsInStateWithButtons() && (button == m_PlayButton || button == m_BattleGroundsButton || button == m_TavernBrawlButton || button == m_GameModesButton)))
		{
			return;
		}
		TooltipZone tooltipZone = button?.gameObject.GetComponent<TooltipZone>();
		if (tooltipZone == null || (m_tutorialPreviewController != null && m_tutorialPreviewController.IsPlayingPreview))
		{
			return;
		}
		string headline = "";
		string description = "GLUE_TOOLTIP_BUTTON_DISABLED_DESC";
		if (tooltipZone.targetObject == m_PlayButton.gameObject)
		{
			headline = "GLUE_TOOLTIP_BUTTON_TRADITIONAL_HEADLINE";
			description = "GLUE_TOOLTIP_BUTTON_TRADITIONAL_DESC";
		}
		else if (tooltipZone.targetObject == m_BattleGroundsButton.gameObject)
		{
			headline = "GLUE_TOOLTIP_BUTTON_BACON_HEADLINE";
			if (!m_BattleGroundsButton.CanDownloadMode() || DownloadManager.IsModuleReadyToPlay(DownloadTags.Content.Bgs))
			{
				description = ((!m_BattleGroundsButton.IsFeatureActive() || !GameModeUtils.HasUnlockedMode(Global.UnlockableGameMode.BATTLEGROUNDS)) ? "GLUE_TOOLTIP_BUTTON_BACON_DESC_LOCKED" : "GLUE_TOOLTIP_BUTTON_BACON_DESC");
			}
			else if (DownloadManager.IsModuleDownloading(DownloadTags.Content.Bgs))
			{
				description = GameStrings.Get("GLUE_TOOLTIP_BUTTON_BACON_DESC_DOWNLOADING");
			}
			else
			{
				long totalSize = DownloadManager.GetModuleDownloadSize(DownloadTags.Content.Bgs);
				description = GameStrings.Format("GLUE_TOOLTIP_BUTTON_BACON_DESC_DOWNLOAD_REQUIRED", DownloadUtils.FormatBytesAsHumanReadable(totalSize));
			}
		}
		else if (tooltipZone.targetObject == m_GameModesButton.gameObject)
		{
			headline = "GLUE_TOOLTIP_BUTTON_GAME_MODES_HEADLINE";
			description = ((!GameModeUtils.CanAccessGameModes()) ? "GLUE_TOOLTIP_BUTTON_GAME_MODES_LOCKED_DESC" : "GLUE_TOOLTIP_BUTTON_GAME_MODES_DESC");
		}
		else if (tooltipZone.targetObject == m_TavernBrawlButton.gameObject)
		{
			headline = "GLUE_TOOLTIP_BUTTON_TAVERN_BRAWL_HEADLINE";
			description = "GLUE_TOOLTIP_BUTTON_TAVERN_BRAWL_DESC";
			if (!TavernBrawlManager.Get().CanEnterStandardTavernBrawl(out var reason))
			{
				description = reason;
			}
		}
		else if (tooltipZone.targetObject == m_OpenPacksButton.gameObject)
		{
			headline = "GLUE_TOOLTIP_BUTTON_PACKOPEN_HEADLINE";
			description = "GLUE_TOOLTIP_BUTTON_PACKOPEN_DESC";
		}
		else if (tooltipZone.targetObject == m_CollectionButton.gameObject)
		{
			headline = "GLUE_TOOLTIP_BUTTON_COLLECTION_HEADLINE";
			description = "GLUE_TOOLTIP_BUTTON_COLLECTION_DESC";
		}
		if (headline != "")
		{
			tooltipZone.ShowBoxTooltip(GameStrings.Get(headline), GameStrings.Get(description));
		}
	}

	private void OnButtonMouseOut(UIEvent e)
	{
		PegUIElement button = e.GetElement();
		if (!OnHoverDisabledButtons.Contains(button))
		{
			TooltipZone tooltipZone = button.gameObject.GetComponent<TooltipZone>();
			if (!(tooltipZone == null))
			{
				tooltipZone.HideTooltip();
			}
		}
	}

	public virtual void OnStartButtonPressed(UIEvent e)
	{
		if (!ServiceManager.IsAvailable<SceneMgr>())
		{
			ChangeState(State.HUB);
		}
		else
		{
			FireButtonPressEvent(ButtonType.START);
		}
	}

	public virtual void OnOpenPacksButtonPressed(UIEvent e)
	{
		if (!Network.IsLoggedIn())
		{
			ShowReconnectPopup(e, OnOpenPacksButtonPressed);
		}
		else if (!ServiceManager.IsAvailable<SceneMgr>())
		{
			ChangeState(State.OPEN);
		}
		else
		{
			FireButtonPressEvent(ButtonType.OPEN_PACKS);
		}
	}

	public virtual void OnCollectionButtonPressed(UIEvent e)
	{
		if (!ServiceManager.IsAvailable<SceneMgr>())
		{
			ChangeState(State.OPEN);
		}
		else
		{
			FireButtonPressEvent(ButtonType.COLLECTION);
		}
	}

	public virtual void OnSetRotationButtonPressed(UIEvent e)
	{
		Log.Box.Print("Set Rotation Button Pressed!");
		if (!ServiceManager.IsAvailable<SceneMgr>())
		{
			ChangeState(State.SET_ROTATION_OPEN);
			return;
		}
		AchieveManager.Get().NotifyOfClick(Achievement.ClickTriggerType.BUTTON_PLAY);
		FireButtonPressEvent(ButtonType.SET_ROTATION);
	}

	public virtual void OnQuestButtonPressed(UIEvent e)
	{
		JournalButton journalButton = m_ribbonButtons.m_journalButtonWidget.GetComponentInChildren<JournalButton>();
		if (!(journalButton == null))
		{
			journalButton.ShowJournal();
		}
	}

	private void SetButtonSelected(BoxMenuButton button)
	{
		HighlightButton(m_PlayButton, highlightOn: false);
		HighlightButton(m_BattleGroundsButton, highlightOn: false);
		HighlightButton(m_TavernBrawlButton, highlightOn: false);
		if (!(button == null))
		{
			HighlightButton(button, highlightOn: true);
		}
	}

	private void OnTraditionalModeButtonPressed(UIEvent e)
	{
		m_nextMissionId = GameUtils.GetNextTutorial();
		if (m_nextMissionId == 5287)
		{
			if (GameUtils.TutorialPreviewVideosEnabled())
			{
				if (m_tutorialPreviewController == null)
				{
					Debug.LogWarning("Tutorial preview controller is not loaded yet.");
					return;
				}
				if (m_tutorialPreviewController.IsAnimating)
				{
					return;
				}
				ShowTraditionalPreviewVideo();
			}
			else
			{
				StartTraditionalTutorial();
			}
			TelemetryManager.Client().SendFTUEButtonClicked("traditional");
		}
		else if (m_nextMissionId != 0)
		{
			StartTraditionalTutorial();
		}
		else
		{
			PlayTraditionalMode();
		}
	}

	public bool HandleBattleGroundDownloadRequired(string downloadPromptTextOverride = "")
	{
		if (!DownloadManager.IsModuleReadyToPlay(DownloadTags.Content.Bgs))
		{
			if (DownloadManager.IsModuleDownloading(DownloadTags.Content.Bgs))
			{
				NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("GLUE_TOOLTIP_BUTTON_BACON_DESC_DOWNLOAD_MANAGER"), "", 3f);
			}
			else
			{
				HandleModuleDownloadRequired(DownloadTags.Content.Bgs, downloadPromptTextOverride);
			}
			return true;
		}
		return false;
	}

	public void OnBattleGroundsButtonPressed(UIEvent e)
	{
		HighlightButton(m_BattleGroundsButton, highlightOn: false);
		if (HandleBattleGroundDownloadRequired())
		{
			return;
		}
		if (UniversalInputManager.Get().IsTouchMode())
		{
			Options.Get().SetBool(Option.HAS_SEEN_BG_DOWNLOAD_FINISHED_POPUP, val: true);
		}
		if (!Network.IsLoggedIn())
		{
			ShowReconnectPopup(e, OnBattleGroundsButtonPressed);
			return;
		}
		if (GameUtils.IsBattleGroundsTutorialComplete())
		{
			PlayBattlegroundsMode();
			return;
		}
		if (GameUtils.TutorialPreviewVideosEnabled())
		{
			if (m_tutorialPreviewController == null)
			{
				Debug.LogWarning("Tutorial preview controller is not loaded yet.");
				return;
			}
			if (m_tutorialPreviewController.IsAnimating)
			{
				return;
			}
			ShowBattlegroundsPreviewVideo();
		}
		else
		{
			StartBattlegroundsTutorial();
		}
		TelemetryManager.Client().SendFTUEButtonClicked("battlegrounds");
	}

	public void OnTavernBrawlButtonPressed(UIEvent e)
	{
		string reason;
		if (!Network.IsLoggedIn())
		{
			ShowReconnectPopup(e, OnTavernBrawlButtonPressed);
		}
		else if (TavernBrawlManager.Get().CanEnterStandardTavernBrawl(out reason) && SceneMgr.Get() != null && !DialogManager.Get().ShowingDialog())
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.TAVERN_BRAWL);
			if (TavernBrawlManager.Get().IsFirstTimeSeeingThisFeature)
			{
				DoTavernBrawlIntroVO();
			}
			else
			{
				PlayTavernBrawlCrowdSFX();
			}
		}
	}

	public void OnModesButtonPressed(UIEvent e)
	{
		if (!Network.IsLoggedIn())
		{
			ShowReconnectPopup(e, OnModesButtonPressed);
		}
		else if (SceneMgr.Get() != null && !DialogManager.Get().ShowingDialog())
		{
			FireButtonPressEvent(ButtonType.GAME_MODES);
		}
	}

	private void ShowTraditionalPreviewVideo()
	{
		if (!GameUtils.IsAnyTutorialComplete())
		{
			m_Camera.ChangeState(BoxCamera.State.CLOSED_TUTORIAL_VIDEO_PREVIEW);
			SetButtonSelected(m_PlayButton);
		}
		m_tutorialPreviewController.StartTraditionalTutorialPreviewVideo(StartTraditionalTutorial);
		FireButtonPressEvent(ButtonType.TRADITIONAL, isShowingTutorialPreview: true);
	}

	private void ShowBattlegroundsPreviewVideo()
	{
		if (!GameUtils.IsAnyTutorialComplete())
		{
			m_Camera.ChangeState(BoxCamera.State.CLOSED_TUTORIAL_VIDEO_PREVIEW);
			SetButtonSelected(m_BattleGroundsButton);
		}
		m_tutorialPreviewController.StartBattleGroundsTutorialPreviewVideo(StartBattlegroundsTutorial);
		FireButtonPressEvent(ButtonType.BACON, isShowingTutorialPreview: true);
	}

	private void StartTraditionalTutorial()
	{
		SetButtonSelected(null);
		MusicManager.Get().StopPlaylist();
		ChangeState(State.CLOSED);
		GameMgr.Get().RegisterFindGameEvent(OnFindGameEvent);
		GameMgr.Get().FindGame(GameType.GT_TUTORIAL, FormatType.FT_WILD, m_nextMissionId, 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
	}

	private void StartBattlegroundsTutorial()
	{
		SetButtonSelected(null);
		GameMgr.Get().FindGame(GameType.GT_VS_AI, FormatType.FT_WILD, 3539, 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
	}

	private void HandleModuleDownloadRequired(DownloadTags.Content moduleTag, string downloadPromptTextOverride = "")
	{
		if (!m_loadingConfirmationPopup)
		{
			StartCoroutine(ShowDownloadConfirmationPopup(moduleTag, downloadPromptTextOverride));
		}
	}

	private IEnumerator ShowDownloadConfirmationPopup(DownloadTags.Content moduleTag, string downloadPromptTextOverride = "")
	{
		m_loadingConfirmationPopup = true;
		if (m_downloadConfirmationPopupWidget == null)
		{
			m_downloadConfirmationPopupWidget = WidgetInstance.Create(s_downloadConfirmationPopupReference);
		}
		while (!m_downloadConfirmationPopupWidget.IsReady)
		{
			yield return null;
		}
		m_loadingConfirmationPopup = false;
		m_downloadConfirmationPopup = m_downloadConfirmationPopupWidget.GetComponentInChildren<DownloadConfirmationPopup>();
		DownloadConfirmationPopup.DownloadConfirmationPopupData confirmationPopupData = new DownloadConfirmationPopup.DownloadConfirmationPopupData(moduleTag, OnDownloadPopupYesClicked, OnDownloadPopupNoClicked);
		if (downloadPromptTextOverride == "")
		{
			m_downloadConfirmationPopup.Init(confirmationPopupData);
		}
		else
		{
			m_downloadConfirmationPopup.Init(confirmationPopupData, downloadPromptTextOverride);
		}
		OverlayUI.Get().AddGameObject(m_downloadConfirmationPopupWidget.gameObject, CanvasAnchor.CENTER, destroyOnSceneLoad: true);
		UIContext.GetRoot().ShowPopup(m_downloadConfirmationPopupWidget.gameObject);
		m_downloadConfirmationPopupWidget.TriggerEvent("SHOW");
		BnetBar.Get()?.RequestDisableButtons();
		void DismissPopup()
		{
			m_downloadConfirmationPopupWidget.TriggerEvent("HIDE");
			UIContext.GetRoot().DismissPopup(m_downloadConfirmationPopupWidget.gameObject);
			BnetBar.Get()?.CancelRequestToDisableButtons();
		}
		void OnDownloadPopupNoClicked(DownloadTags.Content moduleTag)
		{
			DismissPopup();
			if (moduleTag == DownloadTags.Content.Bgs && !GameModeUtils.HasUnlockedMode(Global.UnlockableGameMode.BATTLEGROUNDS))
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
				{
					m_headerText = GameStrings.Get("GLUE_BACON_INVITE_NEW_PLAYER_DECLINED_TITLE"),
					m_text = GameStrings.Get("GLUE_BACON_INVITE_NEW_PLAYER_DECLINED_BODY_PLAYER"),
					m_responseDisplay = AlertPopup.ResponseDisplay.OK,
					m_showAlertIcon = false,
					m_okText = GameStrings.Get("GLOBAL_OKAY"),
					m_alertTextAlignment = UberText.AlignmentOptions.Center
				};
				DialogManager.Get().ShowPopup(info);
			}
		}
		void OnDownloadPopupYesClicked(DownloadTags.Content moduleTag)
		{
			if (moduleTag != 0)
			{
				DownloadManager.DownloadModule(moduleTag);
			}
			if (moduleTag == DownloadTags.Content.Bgs && !GameModeUtils.HasUnlockedMode(Global.UnlockableGameMode.BATTLEGROUNDS))
			{
				Options.Get().SetBool(Option.HAS_SEEN_BG_DOWNLOAD_FINISHED_POPUP, val: false);
				Network.Get().UnlockBattlegroundsDuringApprentice();
			}
			DismissPopup();
		}
	}

	private void PlayTraditionalMode()
	{
		if (!ServiceManager.IsAvailable<SceneMgr>())
		{
			ChangeState(State.OPEN);
			return;
		}
		AchieveManager.Get().NotifyOfClick(Achievement.ClickTriggerType.BUTTON_PLAY);
		FireButtonPressEvent(ButtonType.TRADITIONAL);
	}

	private void PlayBattlegroundsMode()
	{
		if (SceneMgr.Get() == null)
		{
			ChangeState(State.OPEN);
		}
		else
		{
			FireButtonPressEvent(ButtonType.BACON);
		}
	}

	public virtual void OnStoreButtonReleased(UIEvent e)
	{
		if (!Network.IsLoggedIn())
		{
			Log.Store.PrintDebug("Cannot open Shop due to being offline.");
			ShowReconnectPopup(e, OnStoreButtonReleased);
			return;
		}
		if (m_railroadManager.ShouldDisableButtonType(ButtonType.STORE))
		{
			Log.Store.PrintDebug("Railroading disabling Shop presses.");
			return;
		}
		if (ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			dataService.TryRefreshStaleProductAvailability();
		}
		if (!IsShopButtonReadyToOpen(out var unableToOpenReason, out var reasonLogLevel))
		{
			Log.Store.Print(reasonLogLevel, verbose: false, unableToOpenReason);
			return;
		}
		FireButtonPressEvent(ButtonType.STORE);
		FriendChallengeMgr.Get()?.OnStoreOpened();
		StoreManager storeManager = StoreManager.Get();
		if (storeManager != null)
		{
			storeManager.RegisterStoreShownListener(OnStoreShown);
			storeManager.StartGeneralTransaction();
		}
	}

	public bool IsShopButtonReadyToOpen(out string unableToOpenReason, out LogLevel reasonLogLevel)
	{
		unableToOpenReason = "";
		reasonLogLevel = LogLevel.None;
		if (ShopInitialization.Status != ShopStatus.Ready)
		{
			unableToOpenReason = ShopInitialization.StatusMessage;
			reasonLogLevel = ((ShopInitialization.Status != ShopStatus.Failed) ? LogLevel.Debug : LogLevel.Warning);
			return false;
		}
		if (FriendChallengeMgr.Get().HasChallenge())
		{
			unableToOpenReason = "Cannot open Shop due to having friendly challenge.";
			reasonLogLevel = LogLevel.Debug;
			return false;
		}
		if (m_StoreButton.IsVisualClosed())
		{
			unableToOpenReason = "Cannot open Shop due to button is visually closed.";
			reasonLogLevel = LogLevel.Debug;
			return false;
		}
		if (SceneMgr.Get() == null || SceneMgr.Get().GetMode() != SceneMgr.Mode.HUB || SceneMgr.Get().IsTransitionNowOrPending())
		{
			unableToOpenReason = "Cannot open Shop due to invalid scene state.";
			reasonLogLevel = LogLevel.Warning;
			return false;
		}
		return true;
	}

	public virtual void FireButtonPressEvent(ButtonType buttonType, bool isShowingTutorialPreview = false)
	{
		if (m_waitingForSceneLoad)
		{
			m_queuedButtonFire = buttonType;
			return;
		}
		ButtonPressListener[] listeners = m_buttonPressListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(buttonType, isShowingTutorialPreview);
		}
	}

	public void AddButtonPressListener(ButtonPressCallback callback)
	{
		AddButtonPressListener(callback, null);
	}

	public void AddButtonPressListener(ButtonPressCallback callback, object userData)
	{
		ButtonPressListener listener = new ButtonPressListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_buttonPressListeners.Contains(listener))
		{
			m_buttonPressListeners.Add(listener);
		}
	}

	public bool RemoveButtonPressListener(ButtonPressCallback callback)
	{
		return RemoveButtonPressListener(callback, null);
	}

	public bool RemoveButtonPressListener(ButtonPressCallback callback, object userData)
	{
		ButtonPressListener listener = new ButtonPressListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_buttonPressListeners.Remove(listener);
	}

	private void DoTavernBrawlIntroVO()
	{
		if (!NotificationManager.Get().HasSoundPlayedThisSession("VO_INNKEEPER_TAVERNBRAWL_PUSH_32.prefab:4f57cd2af5fe5194fbc46c91171ab135"))
		{
			Action<int> cbNextLine = delegate
			{
				NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_TAVERNBRAWL_DESC1_29"), "VO_INNKEEPER_TAVERNBRAWL_DESC1_29.prefab:44d1a6b322c3dcf4c950e68eb4f4a05f");
			};
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_TAVERNBRAWL_PUSH_32"), "VO_INNKEEPER_TAVERNBRAWL_PUSH_32.prefab:4f57cd2af5fe5194fbc46c91171ab135", cbNextLine);
			NotificationManager.Get().ForceAddSoundToPlayedList("VO_INNKEEPER_TAVERNBRAWL_PUSH_32.prefab:4f57cd2af5fe5194fbc46c91171ab135");
		}
	}

	public void PlayTavernBrawlCrowdSFX()
	{
		if (m_tavernBrawlEnterCrowdSounds.Count >= 1)
		{
			int sfxIndex = UnityEngine.Random.Range(0, m_tavernBrawlEnterCrowdSounds.Count);
			WeakAssetReference soundToPlay = m_tavernBrawlEnterCrowdSounds[sfxIndex];
			SoundManager.Get().LoadAndPlay(soundToPlay.AssetString);
		}
	}

	public void InitializeNet(bool fromLogin)
	{
		if (ServiceManager.TryGet<SceneMgr>(out var sceneMgr))
		{
			m_waitingForNetData = true;
			if (sceneMgr.GetMode() != SceneMgr.Mode.STARTUP || fromLogin)
			{
				Network.Get().RequestBaconRatingInfo();
				NetCache.Get().RegisterScreenBox(OnNetCacheReady);
				NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheBoosters), OnNetCacheBoostersUpdated);
				NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheMedalInfo), RankMgr.Get().SetRankPresenceField);
				NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheBaconRatingInfo), RankMgr.Get().SetRankPresenceField);
				SceneMgr.Get().RegisterSceneLoadedEvent(UpdateRankPresence);
				SceneMgr.Get().RegisterScenePreUnloadEvent(UpdateRankPresence);
			}
		}
	}

	private void ShutdownNet()
	{
		if (ServiceManager.TryGet<NetCache>(out var netCache))
		{
			netCache.UnregisterNetCacheHandler(OnNetCacheReady);
			netCache.RemoveUpdatedListener(typeof(NetCache.NetCacheBoosters), OnNetCacheBoostersUpdated);
		}
	}

	private void OnNetCacheReady()
	{
		m_waitingForNetData = false;
		StartCoroutine(UpdateUIWhenCollectionReady());
	}

	private void UpdateRankPresence(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (mode == SceneMgr.Mode.BACON || mode == SceneMgr.Mode.TOURNAMENT || mode == SceneMgr.Mode.GAMEPLAY)
		{
			RankMgr.Get().SetRankPresenceField();
		}
	}

	private void OnNetCacheBoostersUpdated()
	{
		UpdateUI();
	}

	private int ComputeBoosterCount()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>().GetTotalNumBoosters();
	}

	public void OnStoreShown()
	{
		if (ServiceManager.TryGet<MessagePopupDisplay>(out var popupDisplay))
		{
			popupDisplay.TriggerEvent(PopupEvent.OnShop);
		}
		StoreManager.Get().RemoveStoreShownListener(OnStoreShown);
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		if (eventData.m_state != FindGameState.SERVER_GAME_STARTED || GameMgr.Get().IsNextReconnect())
		{
			return false;
		}
		if (Get() == null)
		{
			return false;
		}
		Spell eventSpell = GetEventSpell(BoxEventType.TUTORIAL_PLAY);
		eventSpell.AddStateFinishedCallback(OnTutorialPlaySpellStateBirthFinished);
		eventSpell.ActivateState(SpellStateType.BIRTH);
		return true;
	}

	private void OnTutorialPlaySpellStateBirthFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		SpellStateType activeState = spell.GetActiveState();
		if (prevStateType == SpellStateType.BIRTH)
		{
			LoadingScreen.Get().SetFadeColor(Color.white);
			LoadingScreen.Get().EnableFadeOut(enable: false);
			LoadingScreen.Get().AddTransitionObject(Get().gameObject);
			LoadingScreen.Get().AddTransitionBlocker();
			SceneMgr.Get().RegisterSceneLoadedEvent(OnMissionSceneLoaded);
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.GAMEPLAY);
		}
		else if (activeState == SpellStateType.NONE)
		{
			LoadingScreen.Get().NotifyTransitionBlockerComplete();
		}
	}

	private void OnMissionSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		SceneMgr.Get().UnregisterSceneLoadedEvent(OnMissionSceneLoaded);
		GetEventSpell(BoxEventType.TUTORIAL_PLAY).ActivateState(SpellStateType.ACTION);
	}

	private void OnTutorialProgressScreenCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		TutorialProgressScreen component = go.GetComponent<TutorialProgressScreen>();
		component.SetCoinPressCallback(StartTraditionalTutorial);
		component.StartTutorialProgress();
	}

	private PegUIElement GetRibbonButtonFromButton(PegUIElement button)
	{
		if (button == null || m_ribbonButtons == null)
		{
			return null;
		}
		if (button == m_CollectionButton)
		{
			return m_ribbonButtons.m_collectionManagerRibbon;
		}
		if (button == m_QuestLogButton)
		{
			return m_ribbonButtons.m_questLogRibbon;
		}
		if (button == m_OpenPacksButton)
		{
			return m_ribbonButtons.m_packOpeningRibbon;
		}
		if (button == m_StoreButton)
		{
			return m_ribbonButtons.m_storeRibbon;
		}
		return null;
	}

	private void ShowReconnectPopup(UIEvent e, ButtonPressFunction onButtonPressed)
	{
		DialogManager.Get().ShowReconnectHelperDialog(delegate
		{
			if (onButtonPressed != null)
			{
				onButtonPressed(e);
			}
		});
	}

	private void TrackBoxInteractable()
	{
		if (m_state == State.PRESS_START || m_state == State.HUB || m_state == State.SET_ROTATION || m_state == State.HUB_WITH_DRAWER)
		{
			HearthstonePerformance.Get()?.CaptureBoxInteractableTime();
		}
	}

	private void ResetSetRotationPopupProgress()
	{
		GameSaveDataManager gameSaveDataManager = GameSaveDataManager.Get();
		if (gameSaveDataManager == null)
		{
			return;
		}
		bool num = gameSaveDataManager.IsDataReady(GameSaveKeyId.SET_ROTATION);
		bool saveNeeded = false;
		List<GameSaveDataManager.SubkeySaveRequest> saveRequests = new List<GameSaveDataManager.SubkeySaveRequest>();
		if (!num)
		{
			saveNeeded = true;
			saveRequests.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.SET_ROTATION, GameSaveKeySubkeyId.ROTATED_BOOSTER_POPUP_PROGRESS, default(long)));
			saveRequests.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.SET_ROTATION, GameSaveKeySubkeyId.INNKEEPER_STANDARD_DECKS_VO_PROGRESS, default(long)));
		}
		else
		{
			long rotatedBoosterPopupProgress = -1L;
			long innkeeperStandardDecksVOProgress = -1L;
			gameSaveDataManager.GetSubkeyValue(GameSaveKeyId.SET_ROTATION, GameSaveKeySubkeyId.ROTATED_BOOSTER_POPUP_PROGRESS, out rotatedBoosterPopupProgress);
			gameSaveDataManager.GetSubkeyValue(GameSaveKeyId.SET_ROTATION, GameSaveKeySubkeyId.INNKEEPER_STANDARD_DECKS_VO_PROGRESS, out innkeeperStandardDecksVOProgress);
			if (rotatedBoosterPopupProgress != 0L)
			{
				saveRequests.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.SET_ROTATION, GameSaveKeySubkeyId.ROTATED_BOOSTER_POPUP_PROGRESS, default(long)));
				saveNeeded = true;
			}
			if (innkeeperStandardDecksVOProgress != 0L)
			{
				saveRequests.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.SET_ROTATION, GameSaveKeySubkeyId.INNKEEPER_STANDARD_DECKS_VO_PROGRESS, default(long)));
				saveNeeded = true;
			}
		}
		if (saveNeeded)
		{
			gameSaveDataManager.SaveSubkeys(saveRequests);
		}
	}

	private void SetPackCount(int n)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_ribbonButtons.SetPackCount(n);
		}
		else
		{
			m_OpenPacksButton.SetPackCount(n);
		}
	}

	private void ClearQueuedButtonFireEvent()
	{
		m_queuedButtonFire = null;
	}

	private void InitializeTutorialPreviewController()
	{
		if (!GameUtils.AreAllTutorialsComplete() && !m_tutorialPreviewController && (bool)m_tutorialPreview)
		{
			m_tutorialPreview.gameObject.SetActive(value: true);
			m_tutorialPreview.Initialize();
			m_tutorialPreview.RegisterReadyListener(delegate
			{
				m_tutorialPreviewController = m_tutorialPreview.GetComponentInChildren<TutorialPreviewController>();
			});
		}
	}

	private bool IsIndirectCollectionTransition()
	{
		bool num = SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.COLLECTIONMANAGER || SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.BACON_COLLECTION || SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER || SceneMgr.Get().GetMode() == SceneMgr.Mode.BACON_COLLECTION;
		bool involvesHub = SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.HUB || SceneMgr.Get().GetMode() == SceneMgr.Mode.HUB;
		if (num)
		{
			return !involvesHub;
		}
		return false;
	}

	public bool IsInStateWithButtons()
	{
		if (!ShouldBeShowingState(State.INVALID) && !ShouldBeShowingState(State.STARTUP) && !ShouldBeShowingState(State.LOADING) && !ShouldBeShowingState(State.LOADING_HUB))
		{
			return SceneMgr.Get().GetMode() != SceneMgr.Mode.LOGIN;
		}
		return false;
	}

	private void UpdateUIEvents()
	{
		if (!CanEnableUIEvents() || m_waitingForNetData)
		{
			DisableAllBoxButtons(m_waitingForNetData);
			DisableButton(m_StoreButton);
			DisableButton(m_QuestLogButton);
			m_journalButtonWidget.TriggerEvent("DISABLE_INTERACTION");
		}
		else if (!IsInStateWithButtons())
		{
			DisableAllBoxButtons(updateText: false);
		}
		m_railroadManager.UpdateRailroadState();
		UpdateBoxButtons();
		UpdateNonBoxUIEvents();
	}

	private void UpdateBoxButtons()
	{
		bool canShowBoxButtons = IsInStateWithButtons() && !m_waitingForNetData;
		bool shouldEnableHearthstone = IsCollectionReady();
		SetBoxButtonInteractions(shouldEnableHearthstone && canShowBoxButtons, m_PlayButton, shouldUpdateText: true);
		bool num = m_BattleGroundsButton.IsFeatureActive() && GameModeUtils.HasUnlockedMode(Global.UnlockableGameMode.BATTLEGROUNDS);
		bool isBattlegroundsDownloading = DownloadManager.IsModuleDownloading(DownloadTags.Content.Bgs);
		bool shouldEnableBattlegroundsInteractivity = num && !isBattlegroundsDownloading;
		SetBoxButtonInteractions(shouldEnableBattlegroundsInteractivity && canShowBoxButtons, m_BattleGroundsButton, shouldUpdateText: true);
		m_BattleGroundsButton.UpdateButton();
		string reason;
		bool shouldEnableTavernBrawl = TavernBrawlManager.Get().CanEnterStandardTavernBrawl(out reason);
		SetBoxButtonInteractions(shouldEnableTavernBrawl && canShowBoxButtons, m_TavernBrawlButton, shouldUpdateText: true);
		bool shouldEnableModes = GameModeUtils.CanAccessGameModes();
		SetBoxButtonInteractions(shouldEnableModes && canShowBoxButtons, m_GameModesButton, shouldUpdateText: true);
		bool shouldEnablePacksButton = m_state == State.HUB_WITH_DRAWER;
		SetButtonEnabled(shouldEnablePacksButton, m_OpenPacksButton);
		bool shouldEnableCollectionButton = m_state == State.HUB_WITH_DRAWER;
		SetButtonEnabled(shouldEnableCollectionButton, m_CollectionButton);
	}

	private void UpdateNonBoxUIEvents()
	{
		bool num = !m_waitingForNetData && SetRotationManager.Get().ShouldShowSetRotationIntro();
		bool isAnyTutorialComplete = !m_waitingForNetData && GameUtils.IsAnyTutorialComplete();
		bool isWaitingForApprenticeComplete = RankMgr.Get().IsWaitingForApprenticeComplete;
		bool num2 = num || isWaitingForApprenticeComplete || !isAnyTutorialComplete || DemoMgr.Get().IsDemo() || m_state == State.LOADING_HUB || IsIndirectCollectionTransition();
		NetCache.NetCacheFeatures guardianVars = null;
		if (!m_waitingForNetData)
		{
			guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		}
		bool railroadingHidingShop = m_railroadManager.ShouldHideButtonType(ButtonType.STORE);
		if (num2 || railroadingHidingShop)
		{
			m_StoreButton.gameObject.SetActive(value: false);
		}
		else
		{
			m_StoreButton.gameObject.SetActive(value: true);
			EnableButton(m_StoreButton);
		}
		bool journalFeatureDisabled = guardianVars?.JournalButtonDisabled ?? false;
		bool railroadingHidingJournal = m_railroadManager.ShouldHideButtonType(ButtonType.JOURNAL);
		bool railroadingDisablingJournal = m_railroadManager.ShouldDisableButtonType(ButtonType.JOURNAL);
		if (num2 || journalFeatureDisabled || railroadingHidingJournal)
		{
			m_journalButtonWidget.Hide();
			m_journalButtonWidget.TriggerEvent("DISABLE_INTERACTION");
			DisableButton(m_QuestLogButton);
			return;
		}
		m_journalButtonWidget.Show();
		if (railroadingDisablingJournal)
		{
			DisableButton(m_QuestLogButton);
			m_journalButtonWidget.TriggerEvent("DISABLE_INTERACTION");
		}
		else
		{
			EnableButton(m_QuestLogButton);
			m_journalButtonWidget.TriggerEvent("ENABLE_INTERACTION");
		}
	}

	private void DisableAllBoxButtons(bool updateText)
	{
		DisableButton(m_PlayButton);
		DisableButton(m_BattleGroundsButton);
		DisableButton(m_TavernBrawlButton);
		DisableButton(m_GameModesButton);
		DisableButton(m_OpenPacksButton);
		DisableButton(m_CollectionButton);
		if (updateText)
		{
			ToggleButtonTextureState(enabled: false, m_PlayButton);
			ToggleButtonTextureState(enabled: false, m_BattleGroundsButton);
			ToggleButtonTextureState(enabled: false, m_TavernBrawlButton);
			ToggleButtonTextureState(enabled: false, m_GameModesButton);
		}
	}

	private void ToggleTavernBrawlLock(bool shouldLock)
	{
		m_TavernBrawlButton.m_animator.gameObject.SetActive(!shouldLock);
		m_TavernBrawlButton.GetComponent<PlayMakerFSM>().enabled = !shouldLock;
		m_TavernBrawlButtonCover.SetActive(shouldLock);
		if (shouldLock && m_TavernBrawlEnabledDiscMaterial == null)
		{
			Material[] newMaterials = m_DiscRenderer.materials;
			m_TavernBrawlEnabledDiscMaterial = newMaterials[0];
			newMaterials[0] = m_TavernBrawlDisabledDiscMaterial;
			m_DiscRenderer.materials = newMaterials;
		}
		else if (!shouldLock && m_TavernBrawlEnabledDiscMaterial != null)
		{
			Material[] newMaterials2 = m_DiscRenderer.materials;
			newMaterials2[0] = m_TavernBrawlEnabledDiscMaterial;
			m_DiscRenderer.materials = newMaterials2;
		}
	}

	private void OnModuleDownloadStateChange(DownloadTags.Content moduleTag, ModuleState state)
	{
		if (moduleTag == DownloadTags.Content.Bgs)
		{
			UpdateBoxButtons();
			if (state == ModuleState.ReadyToPlay && !OnPressDisabledButtons.Contains(m_BattleGroundsButton) && !Options.Get().GetBool(Option.BATTLEGROUND_DATA_EXISTS))
			{
				HighlightButton(m_BattleGroundsButton, highlightOn: true);
			}
			else if (state == ModuleState.NotRequested)
			{
				HighlightButton(m_BattleGroundsButton, highlightOn: false);
			}
		}
	}

	public void Unload()
	{
		GameMgr.Get().UnregisterFindGameEvent(OnFindGameEvent);
		m_tutorialPreview.Unload();
		m_tutorialPreview.gameObject.SetActive(value: false);
		m_tutorialPreviewController = null;
	}

	public Vector3 GetHearthstoneButtonPosition()
	{
		return m_PlayButton.transform.position;
	}

	public Vector3 GetModesButtonPosition()
	{
		return m_GameModesButton.transform.position;
	}

	private void OnStartButton()
	{
		DownloadManager.RegisterModuleInstallationStateChangeListener(OnModuleDownloadStateChange);
	}

	private void OnDestroyButton()
	{
		StoreManager.Get()?.RemoveStoreShownListener(OnStoreShown);
		DownloadManager.UnregisterModuleInstallationStateChangeListener(OnModuleDownloadStateChange);
		ShutdownNet();
	}
}
