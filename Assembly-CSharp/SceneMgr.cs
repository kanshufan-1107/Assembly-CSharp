using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core.Time;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Fonts;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Cysharp.Threading.Tasks;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Streaming;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMgr : IService, IHasUpdate
{
	public enum Mode
	{
		INVALID,
		STARTUP,
		[Description("Login")]
		LOGIN,
		[Description("Hub")]
		HUB,
		[Description("Gameplay")]
		GAMEPLAY,
		[Description("CollectionManager")]
		COLLECTIONMANAGER,
		[Description("PackOpening")]
		PACKOPENING,
		[Description("Tournament")]
		TOURNAMENT,
		[Description("Friendly")]
		FRIENDLY,
		[Description("FatalError")]
		FATAL_ERROR,
		[Description("Draft")]
		DRAFT,
		[Description("Credits")]
		CREDITS,
		[Description("Reset")]
		RESET,
		[Description("Adventure")]
		ADVENTURE,
		[Description("TavernBrawl")]
		TAVERN_BRAWL,
		[Description("Bacon")]
		BACON,
		[Description("GameMode")]
		GAME_MODE,
		[Description("PvPDungeonRun")]
		PVP_DUNGEON_RUN,
		[Description("BaconCollection")]
		BACON_COLLECTION,
		[Description("Lettuce")]
		LETTUCE_VILLAGE,
		[Description("LettuceBountyBoard")]
		LETTUCE_BOUNTY_BOARD,
		[Description("LettuceMap")]
		LETTUCE_MAP,
		[Description("LettucePlay")]
		LETTUCE_PLAY,
		[Description("LettuceCollection")]
		LETTUCE_COLLECTION,
		[Description("LettuceCoOp")]
		LETTUCE_COOP,
		[Description("LettuceFriendly")]
		LETTUCE_FRIENDLY,
		[Description("LettuceBountyTeamSelect")]
		LETTUCE_BOUNTY_TEAM_SELECT,
		[Description("VillagePackOpening")]
		LETTUCE_PACK_OPENING,
		[Description("LuckyDraw")]
		LUCKY_DRAW
	}

	public enum TransitionHandlerType
	{
		INVALID,
		SCENEMGR,
		CURRENT_SCENE,
		NEXT_SCENE
	}

	public delegate void ScenePreUnloadCallback(Mode prevMode, PegasusScene prevScene, object userData);

	public delegate void SceneUnloadedCallback(Mode prevMode, PegasusScene prevScene, object userData);

	public delegate void ScenePreLoadCallback(Mode prevMode, Mode mode, object userData);

	public delegate void SceneLoadedCallback(Mode mode, PegasusScene scene, object userData);

	private class ScenePreUnloadListener : EventListener<ScenePreUnloadCallback>
	{
		public void Fire(Mode prevMode, PegasusScene prevScene)
		{
			m_callback(prevMode, prevScene, m_userData);
		}
	}

	private class SceneUnloadedListener : EventListener<SceneUnloadedCallback>
	{
		public void Fire(Mode prevMode, PegasusScene prevScene)
		{
			m_callback(prevMode, prevScene, m_userData);
		}
	}

	private class ScenePreLoadListener : EventListener<ScenePreLoadCallback>
	{
		public void Fire(Mode prevMode, Mode mode)
		{
			m_callback(prevMode, mode, m_userData);
		}
	}

	private class SceneLoadedListener : EventListener<SceneLoadedCallback>
	{
		public void Fire(Mode mode, PegasusScene scene)
		{
			m_callback(mode, scene, m_userData);
		}
	}

	public delegate void OnSceneLoadCompleteForSceneDrivenTransition(Action onTransitionComplete);

	public GameObject m_StartupCamera;

	private const float SCENE_UNLOAD_DELAY = 0.15f;

	private const float SCENE_LOADED_DELAY = 0.15f;

	private static SceneMgr s_instance;

	private int m_startupAssetLoads;

	private Mode m_mode = Mode.STARTUP;

	private Mode m_nextMode;

	private Mode m_prevMode;

	private bool m_reloadMode;

	private PegasusScene m_scene;

	private PegasusScene m_previousScene;

	private bool m_sceneLoaded;

	private bool m_transitioning;

	private bool m_performFullCleanup;

	private List<ScenePreUnloadListener> m_scenePreUnloadListeners = new List<ScenePreUnloadListener>();

	private List<SceneUnloadedListener> m_sceneUnloadedListeners = new List<SceneUnloadedListener>();

	private List<ScenePreLoadListener> m_scenePreLoadListeners = new List<ScenePreLoadListener>();

	private List<SceneLoadedListener> m_sceneLoadedListeners = new List<SceneLoadedListener>();

	private OnSceneLoadCompleteForSceneDrivenTransition m_onSceneLoadCompleteForSceneDrivenTransitionCallback;

	private TransitionHandlerType m_transitionHandlerType;

	private object m_sceneTransitionPayload;

	private long m_boxLoadTimestamp;

	private Coroutine m_switchModeCoroutine;

	private UniTask m_switchModeContextTask = UniTask.CompletedTask;

	private GameObject m_sceneObject;

	public bool DisableObjectDestroy { get; }

	public LoadingScreen LoadingScreen { get; private set; }

	public GameObject SceneObject
	{
		get
		{
			if (m_sceneObject == null)
			{
				m_sceneObject = new GameObject("SceneMgr", typeof(HSDontDestroyOnLoad));
			}
			return m_sceneObject;
		}
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
		m_transitioning = true;
		LoadComponentFromResource<LoadingScreen> loadLoadingScreen = new LoadComponentFromResource<LoadingScreen>("Prefabs/LoadingScreen", LoadResourceFlags.AutoInstantiateOnLoad | LoadResourceFlags.FailOnError);
		yield return loadLoadingScreen;
		LoadingScreen = loadLoadingScreen.LoadedComponent;
		LoadingScreen.RegisterSceneListeners(this);
		LoadingScreen.transform.parent = SceneObject.transform;
		HearthstoneApplication.Get().WillReset += WillReset;
		if (!IsModeRequested(Mode.FATAL_ERROR))
		{
			QueueLoadBoxJob();
			RegisterSceneLoadedEvent(UpdatePerformanceTrackingFromModeSwitch);
		}
	}

	public Type[] GetDependencies()
	{
		return new Type[6]
		{
			typeof(GameDownloadManager),
			typeof(Network),
			typeof(GameDbf),
			typeof(IAssetLoader),
			typeof(IFontTable),
			typeof(CameraManager)
		};
	}

	public void Shutdown()
	{
		s_instance = null;
		LoadingScreen.UnregisterSceneListeners(this);
		UnregisterSceneLoadedEvent(UpdatePerformanceTrackingFromModeSwitch);
		HearthstoneApplication.Get().WillReset -= WillReset;
	}

	public void LoadShaderPreCompiler()
	{
		if (PlatformSettings.IsMobile() && PlatformSettings.RuntimeOS != OSCategory.Android)
		{
			AssetReference assetRef = new AssetReference("ShaderPreCompiler.prefab:380ca3ee11a2643068cfb3d4766f3fd3");
			GameObject go = AssetLoader.Get().InstantiatePrefab(assetRef);
			if (go == null)
			{
				Debug.LogError(string.Format("SceneMgr.LoadShaderPreCompiler() - FAILED to load prefab", assetRef));
			}
			else
			{
				go.transform.parent = SceneObject.transform;
			}
		}
	}

	public void Update()
	{
		if (!m_reloadMode)
		{
			if (m_nextMode == Mode.INVALID)
			{
				return;
			}
			if (m_mode == m_nextMode)
			{
				m_nextMode = Mode.INVALID;
				return;
			}
		}
		m_transitioning = true;
		m_performFullCleanup = !m_reloadMode;
		m_prevMode = m_mode;
		m_mode = m_nextMode;
		m_nextMode = Mode.INVALID;
		m_reloadMode = false;
		if (m_scene != null)
		{
			if (m_switchModeCoroutine != null)
			{
				Processor.CancelCoroutine(m_switchModeCoroutine);
				if (m_previousScene != null)
				{
					Processor.RunCoroutine(ForceUnloadOrphanedScene(m_previousScene));
				}
			}
			IEnumerator switchModeEnumerator = (IsDoingSceneDrivenTransition() ? SwitchModeWithSceneDrivenTransition() : SwitchMode());
			m_switchModeCoroutine = Processor.RunCoroutine(switchModeEnumerator, this);
		}
		else
		{
			LoadMode();
		}
	}

	public static SceneMgr Get()
	{
		if (s_instance == null)
		{
			s_instance = ServiceManager.Get<SceneMgr>();
		}
		return s_instance;
	}

	public static bool IsInitialized()
	{
		return s_instance != null;
	}

	private void WillReset()
	{
		Log.Reset.Print("SceneMgr.WillReset()");
		if (HearthstoneApplication.IsPublic())
		{
			TimeScaleMgr.Get().SetGameTimeScale(1f);
			TimeScaleMgr.Get().SetTimeScaleMultiplier(1f);
		}
		Processor.StopAllCoroutinesWithObjectRef(this);
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
		m_mode = Mode.STARTUP;
		m_nextMode = Mode.INVALID;
		m_prevMode = Mode.INVALID;
		m_reloadMode = false;
		PegasusScene prevScene = m_scene;
		if (prevScene != null)
		{
			prevScene.PreUnload();
		}
		FireScenePreUnloadEvent(prevScene);
		if (m_scene != null)
		{
			m_scene.Unload();
			m_scene = null;
			m_sceneLoaded = false;
		}
		if (m_mode != Mode.FATAL_ERROR)
		{
			FireSceneUnloadedEvent(prevScene);
			string scene = ((prevScene == null) ? "" : prevScene.SceneName);
			PostUnloadCleanup(scene);
		}
		QueueLoadBoxJob();
		Log.Reset.Print("\tSceneMgr.WillReset() completed");
	}

	public void SetNextMode(Mode mode, TransitionHandlerType transitionHandler = TransitionHandlerType.SCENEMGR, OnSceneLoadCompleteForSceneDrivenTransition onLoadCompleteCallback = null, object sceneTransitionPayload = null)
	{
		if (IsModeRequested(Mode.FATAL_ERROR))
		{
			return;
		}
		CacheModeForResume(mode);
		m_nextMode = mode;
		m_reloadMode = false;
		m_transitionHandlerType = transitionHandler;
		m_sceneTransitionPayload = sceneTransitionPayload;
		if (transitionHandler == TransitionHandlerType.CURRENT_SCENE || transitionHandler == TransitionHandlerType.NEXT_SCENE)
		{
			if (transitionHandler == TransitionHandlerType.CURRENT_SCENE && onLoadCompleteCallback == null)
			{
				Log.All.PrintError("SceneMgr - SetNextMode did not provide the required callback!");
			}
			m_onSceneLoadCompleteForSceneDrivenTransitionCallback = onLoadCompleteCallback;
		}
	}

	public void ReloadMode()
	{
		if (!IsModeRequested(Mode.FATAL_ERROR))
		{
			m_nextMode = m_mode;
			m_reloadMode = true;
		}
	}

	public void ReturnToPreviousMode()
	{
		if (!IsModeRequested(Mode.FATAL_ERROR))
		{
			CacheModeForResume(m_prevMode);
			m_nextMode = m_prevMode;
			m_reloadMode = false;
		}
	}

	public Mode GetPrevMode()
	{
		return m_prevMode;
	}

	public Mode GetMode()
	{
		return m_mode;
	}

	public Mode GetNextMode()
	{
		return m_nextMode;
	}

	public PegasusScene GetScene()
	{
		return m_scene;
	}

	public void SetScene(PegasusScene scene)
	{
		m_scene = scene;
		m_scene.SetSceneTransitionPayload(m_sceneTransitionPayload);
	}

	public bool IsSceneLoaded()
	{
		return m_sceneLoaded;
	}

	public bool IsGoingToLoadGameplay()
	{
		if (Get().GetNextMode() == Mode.GAMEPLAY)
		{
			return true;
		}
		if (Get().GetMode() == Mode.GAMEPLAY)
		{
			return !Get().IsSceneLoaded();
		}
		return false;
	}

	public bool WillTransition()
	{
		if (m_reloadMode)
		{
			return true;
		}
		if (m_nextMode == Mode.INVALID)
		{
			return false;
		}
		if (m_nextMode != m_mode)
		{
			return true;
		}
		return false;
	}

	public bool IsTransitioning()
	{
		return m_transitioning;
	}

	public bool IsTransitionNowOrPending()
	{
		if (IsTransitioning())
		{
			return true;
		}
		if (WillTransition())
		{
			return true;
		}
		return false;
	}

	public bool IsDoingSceneDrivenTransition()
	{
		if (m_transitionHandlerType != TransitionHandlerType.CURRENT_SCENE)
		{
			return m_transitionHandlerType == TransitionHandlerType.NEXT_SCENE;
		}
		return true;
	}

	public bool IsModeRequested(Mode mode)
	{
		if (m_mode == mode)
		{
			return true;
		}
		if (m_nextMode == mode)
		{
			return true;
		}
		return false;
	}

	public bool IsInGame()
	{
		return IsModeRequested(Mode.GAMEPLAY);
	}

	public bool IsInTavernBrawlMode()
	{
		return GetMode() == Mode.TAVERN_BRAWL;
	}

	public bool IsInArenaDraftMode()
	{
		return GetMode() == Mode.DRAFT;
	}

	public bool IsInLettuceMode()
	{
		Mode currentMode = GetMode();
		if (currentMode != Mode.LETTUCE_VILLAGE && currentMode != Mode.LETTUCE_BOUNTY_BOARD && currentMode != Mode.LETTUCE_MAP && currentMode != Mode.LETTUCE_PLAY && currentMode != Mode.LETTUCE_COLLECTION && currentMode != Mode.LETTUCE_COOP && currentMode != Mode.LETTUCE_FRIENDLY && currentMode != Mode.LETTUCE_BOUNTY_TEAM_SELECT)
		{
			return currentMode == Mode.LETTUCE_PACK_OPENING;
		}
		return true;
	}

	public void NotifySceneLoaded()
	{
		m_sceneLoaded = true;
		if (m_mode == Mode.FATAL_ERROR)
		{
			DestroyAllObjectsOnModeSwitch();
		}
		m_scene.SetSceneName(GetSceneNameFromMode(m_mode));
		if (ShouldUseSceneLoadDelays())
		{
			Processor.RunCoroutine(WaitThenFireSceneLoadedEvent(), this);
		}
		else
		{
			FireSceneLoadedEvent();
		}
	}

	public void RegisterScenePreUnloadEvent(ScenePreUnloadCallback callback)
	{
		RegisterScenePreUnloadEvent(callback, null);
	}

	public void RegisterScenePreUnloadEvent(ScenePreUnloadCallback callback, object userData)
	{
		ScenePreUnloadListener listener = new ScenePreUnloadListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_scenePreUnloadListeners.Contains(listener))
		{
			m_scenePreUnloadListeners.Add(listener);
		}
	}

	public bool UnregisterScenePreUnloadEvent(ScenePreUnloadCallback callback)
	{
		return UnregisterScenePreUnloadEvent(callback, null);
	}

	public bool UnregisterScenePreUnloadEvent(ScenePreUnloadCallback callback, object userData)
	{
		ScenePreUnloadListener listener = new ScenePreUnloadListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_scenePreUnloadListeners.Remove(listener);
	}

	public static bool UnregisterScenePreUnloadEventFromInstance(ScenePreUnloadCallback callback)
	{
		if (s_instance == null)
		{
			return false;
		}
		return s_instance.UnregisterScenePreUnloadEvent(callback);
	}

	public void RegisterSceneUnloadedEvent(SceneUnloadedCallback callback)
	{
		RegisterSceneUnloadedEvent(callback, null);
	}

	public void RegisterSceneUnloadedEvent(SceneUnloadedCallback callback, object userData)
	{
		SceneUnloadedListener listener = new SceneUnloadedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_sceneUnloadedListeners.Contains(listener))
		{
			m_sceneUnloadedListeners.Add(listener);
		}
	}

	public bool UnregisterSceneUnloadedEvent(SceneUnloadedCallback callback)
	{
		return UnregisterSceneUnloadedEvent(callback, null);
	}

	public bool UnregisterSceneUnloadedEvent(SceneUnloadedCallback callback, object userData)
	{
		SceneUnloadedListener listener = new SceneUnloadedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_sceneUnloadedListeners.Remove(listener);
	}

	public void RegisterScenePreLoadEvent(ScenePreLoadCallback callback)
	{
		RegisterScenePreLoadEvent(callback, null);
	}

	public void RegisterScenePreLoadEvent(ScenePreLoadCallback callback, object userData)
	{
		ScenePreLoadListener listener = new ScenePreLoadListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_scenePreLoadListeners.Contains(listener))
		{
			m_scenePreLoadListeners.Add(listener);
		}
	}

	public bool UnregisterScenePreLoadEvent(ScenePreLoadCallback callback)
	{
		return UnregisterScenePreLoadEvent(callback, null);
	}

	public bool UnregisterScenePreLoadEvent(ScenePreLoadCallback callback, object userData)
	{
		ScenePreLoadListener listener = new ScenePreLoadListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_scenePreLoadListeners.Remove(listener);
	}

	public void RegisterSceneLoadedEvent(SceneLoadedCallback callback)
	{
		RegisterSceneLoadedEvent(callback, null);
	}

	public void RegisterSceneLoadedEvent(SceneLoadedCallback callback, object userData)
	{
		SceneLoadedListener listener = new SceneLoadedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_sceneLoadedListeners.Contains(listener))
		{
			m_sceneLoadedListeners.Add(listener);
		}
	}

	public bool UnregisterSceneLoadedEvent(SceneLoadedCallback callback)
	{
		return UnregisterSceneLoadedEvent(callback, null);
	}

	public bool UnregisterSceneLoadedEvent(SceneLoadedCallback callback, object userData)
	{
		SceneLoadedListener listener = new SceneLoadedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_sceneLoadedListeners.Remove(listener);
	}

	private IEnumerator WaitThenFireSceneLoadedEvent()
	{
		yield return new WaitForSeconds(0.15f);
		FireSceneLoadedEvent();
	}

	private void FireScenePreUnloadEvent(PegasusScene prevScene)
	{
		ScenePreUnloadListener[] listeners = m_scenePreUnloadListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(m_prevMode, prevScene);
		}
	}

	private void FireSceneUnloadedEvent(PegasusScene prevScene)
	{
		if (IsDoingSceneDrivenTransition())
		{
			m_transitioning = false;
		}
		SceneUnloadedListener[] listeners = m_sceneUnloadedListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(m_prevMode, prevScene);
		}
	}

	private void FireScenePreLoadEvent()
	{
		ScenePreLoadListener[] listeners = m_scenePreLoadListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(m_prevMode, m_mode);
		}
	}

	private void FireSceneLoadedEvent()
	{
		if (!IsDoingSceneDrivenTransition())
		{
			m_transitioning = false;
		}
		SceneLoadedListener[] listeners = m_sceneLoadedListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(m_mode, m_scene);
		}
		HearthstonePerformance.Get()?.SendCustomEvent("SceneLoaded" + Enum.GetName(typeof(Mode), m_mode));
	}

	private void LoadMode()
	{
		FireScenePreLoadEvent();
		SceneManager.LoadSceneAsync(EnumUtils.GetString(m_mode), LoadSceneMode.Additive);
	}

	private IEnumerator SwitchMode()
	{
		if (m_scene.IsUnloading())
		{
			yield break;
		}
		m_previousScene = m_scene;
		m_previousScene.PreUnload();
		FireScenePreUnloadEvent(m_previousScene);
		if (LoadingScreen.GetPhase() == LoadingScreen.Phase.WAITING_FOR_SCENE_UNLOAD && LoadingScreen.GetFreezeFrameCamera() != null)
		{
			yield return new WaitForEndOfFrame();
		}
		if (ShouldUseSceneUnloadDelays())
		{
			if (Box.Get() != null)
			{
				while (Box.Get().HasPendingEffects())
				{
					yield return 0;
				}
			}
			else
			{
				yield return new WaitForSeconds(0.15f);
			}
		}
		while (m_switchModeContextTask.Status == UniTaskStatus.Pending)
		{
			yield return null;
		}
		if (m_scene != null)
		{
			m_scene.Unload();
			m_scene = null;
			m_sceneLoaded = false;
		}
		FireSceneUnloadedEvent(m_previousScene);
		PostUnloadCleanup(m_previousScene.SceneName);
		LoadModeFromModeSwitch();
		m_switchModeCoroutine = null;
		m_switchModeContextTask = UniTask.CompletedTask;
	}

	private IEnumerator ForceUnloadOrphanedScene(PegasusScene scene)
	{
		if (scene.IsUnloading())
		{
			yield break;
		}
		FireScenePreUnloadEvent(scene);
		if (Box.Get() != null)
		{
			while (Box.Get().HasPendingEffects())
			{
				yield return null;
			}
		}
		scene.Unload();
		UnloadUnitySceneAsync(scene.SceneName);
		FireSceneUnloadedEvent(scene);
	}

	private void UnloadUnitySceneAsync(string sceneName)
	{
		if (!(sceneName == ""))
		{
			if (IsValidUnitySceneToUnload(sceneName))
			{
				SceneManager.UnloadSceneAsync(sceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
			}
			else
			{
				Log.All.PrintError("SceneMgr - Error calling UnloadUnitySceneAsync on invalid PegasusScene: '" + sceneName + "'. The underlying Unity scene won't be unloaded.");
			}
		}
	}

	private bool IsValidUnitySceneToUnload(string unitySceneName)
	{
		if (!string.IsNullOrWhiteSpace(unitySceneName) && SceneManager.GetSceneByName(unitySceneName).IsValid())
		{
			return SceneManager.GetSceneByName(unitySceneName).isLoaded;
		}
		return false;
	}

	private void OnUnloadPreviousScene()
	{
		m_previousScene.PreUnload();
		FireScenePreUnloadEvent(m_previousScene);
		m_previousScene.Unload();
		UnloadUnitySceneAsync(m_previousScene.SceneName);
		FireSceneUnloadedEvent(m_previousScene);
		m_previousScene = null;
	}

	private IEnumerator SwitchModeWithSceneDrivenTransition()
	{
		if (!m_scene.IsUnloading())
		{
			m_previousScene = m_scene;
			m_sceneLoaded = false;
			FireScenePreLoadEvent();
			SceneManager.LoadSceneAsync(GetSceneNameFromMode(m_mode), LoadSceneMode.Additive);
			while (!m_sceneLoaded)
			{
				yield return null;
			}
			if (m_transitionHandlerType == TransitionHandlerType.CURRENT_SCENE && m_onSceneLoadCompleteForSceneDrivenTransitionCallback != null)
			{
				m_onSceneLoadCompleteForSceneDrivenTransitionCallback(OnUnloadPreviousScene);
			}
			else if (m_transitionHandlerType == TransitionHandlerType.NEXT_SCENE)
			{
				m_scene.ExecuteSceneDrivenTransition(OnUnloadPreviousScene);
			}
			else
			{
				Log.All.PrintError("SceneMgr - No callback for scene driven scene transition.");
				OnUnloadPreviousScene();
			}
			m_switchModeCoroutine = null;
			m_onSceneLoadCompleteForSceneDrivenTransitionCallback = null;
		}
	}

	private bool ShouldUseSceneUnloadDelays()
	{
		if (m_prevMode == m_mode)
		{
			return false;
		}
		return true;
	}

	private bool ShouldUseSceneLoadDelays()
	{
		if (m_mode == Mode.LOGIN)
		{
			return false;
		}
		if (m_mode == Mode.HUB)
		{
			return false;
		}
		if (m_mode == Mode.FATAL_ERROR)
		{
			return false;
		}
		return true;
	}

	private void PostUnloadCleanup(string sceneName)
	{
		Time.captureFramerate = 0;
		DestroyAllObjectsOnModeSwitch();
		UnloadUnitySceneAsync(sceneName);
		if (m_performFullCleanup)
		{
			HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
			if (hearthstoneApplication != null)
			{
				hearthstoneApplication.UnloadUnusedAssets();
			}
		}
		m_previousScene = null;
	}

	private void DestroyAllObjectsOnModeSwitch()
	{
		if (DisableObjectDestroy)
		{
			return;
		}
		int numScenes = SceneManager.sceneCount;
		for (int sceneIndex = 0; sceneIndex < numScenes; sceneIndex++)
		{
			GameObject[] rootGameObjects = SceneManager.GetSceneAt(sceneIndex).GetRootGameObjects();
			foreach (GameObject go in rootGameObjects)
			{
				if (ShouldDestroyOnModeSwitch(go))
				{
					UnityEngine.Object.DestroyImmediate(go);
				}
			}
		}
	}

	private bool ShouldDestroyOnModeSwitch(GameObject go)
	{
		if (go == null)
		{
			return false;
		}
		if (go.transform.parent != null)
		{
			return false;
		}
		if (go.GetComponent<HSDontDestroyOnLoad>() != null)
		{
			return false;
		}
		if (go.scene.buildIndex == -1)
		{
			Debug.LogErrorFormat("GameObject ({0}) appears to be marked Don't Destroy On Load, but is being destroyed by our code anyway!", go.name);
		}
		if (PegUI.Get() != null && go == PegUI.Get().gameObject)
		{
			return false;
		}
		if (OverlayUI.Get() != null && go == OverlayUI.Get().gameObject)
		{
			return false;
		}
		if (Box.Get() != null && go == Box.Get().gameObject && DoesModeShowBox(m_mode))
		{
			return false;
		}
		if (AssetLoader.Get().IsSharedPrefabInstance(go))
		{
			return false;
		}
		if (AssetLoader.Get().IsWaitingOnObject(go))
		{
			return false;
		}
		if (go == iTweenManager.Get().gameObject)
		{
			return false;
		}
		return true;
	}

	private void CacheModeForResume(Mode mode)
	{
		if (PlatformSettings.OS == OSCategory.iOS || PlatformSettings.OS == OSCategory.Android)
		{
			switch (mode)
			{
			case Mode.HUB:
			case Mode.FRIENDLY:
				Options.Get().SetInt(Option.LAST_SCENE_MODE, 0);
				break;
			case Mode.COLLECTIONMANAGER:
			case Mode.TOURNAMENT:
			case Mode.DRAFT:
			case Mode.CREDITS:
			case Mode.ADVENTURE:
			case Mode.TAVERN_BRAWL:
			case Mode.BACON:
			case Mode.GAME_MODE:
			case Mode.LETTUCE_VILLAGE:
			case Mode.LETTUCE_BOUNTY_BOARD:
			case Mode.LETTUCE_MAP:
			case Mode.LETTUCE_PLAY:
			case Mode.LETTUCE_COLLECTION:
			case Mode.LETTUCE_COOP:
			case Mode.LETTUCE_BOUNTY_TEAM_SELECT:
			case Mode.LETTUCE_PACK_OPENING:
				Options.Get().SetInt(Option.LAST_SCENE_MODE, (int)mode);
				break;
			case Mode.GAMEPLAY:
			case Mode.PACKOPENING:
			case Mode.FATAL_ERROR:
			case Mode.RESET:
			case Mode.PVP_DUNGEON_RUN:
			case Mode.BACON_COLLECTION:
			case Mode.LETTUCE_FRIENDLY:
				break;
			}
		}
	}

	private bool DoesModeShowBox(Mode mode)
	{
		if (mode == Mode.STARTUP || mode == Mode.GAMEPLAY || mode == Mode.RESET)
		{
			return false;
		}
		return true;
	}

	private void LoadModeFromModeSwitch()
	{
		bool prevModeShowsBox = DoesModeShowBox(m_prevMode);
		bool modeShowsBox = DoesModeShowBox(m_mode);
		if (!prevModeShowsBox && modeShowsBox)
		{
			Processor.QueueJob("SceneMgr.Reload", Job_ReloadBox());
			return;
		}
		if (prevModeShowsBox && !modeShowsBox)
		{
			LoadingScreen.SetAssetLoadStartTimestamp(m_boxLoadTimestamp);
			m_boxLoadTimestamp = 0L;
		}
		LoadMode();
	}

	private void QueueLoadBoxJob()
	{
		IJobDependency[] loadBoxDependencies = HearthstoneJobs.BuildDependencies(typeof(SceneMgr), typeof(IAssetLoader), typeof(NetCache), new WaitForGameDownloadManagerAvailable(), new WaitForSplashScreen());
		Processor.QueueJob("SceneMgr.LoadBox", Job_LoadBox(), loadBoxDependencies);
	}

	private IEnumerator<IAsyncJobResult> Job_LoadBox()
	{
		yield return new LoadUIScreen("TheBox.prefab:6b55a928ffdc1b341b5dbe8f8a88e768");
		m_nextMode = Mode.LOGIN;
	}

	private IEnumerator<IAsyncJobResult> Job_ReloadBox()
	{
		yield return new LoadUIScreen("TheBox.prefab:6b55a928ffdc1b341b5dbe8f8a88e768");
		LoadMode();
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		if (UserAttentionManager.IsBlockedBy(UserAttentionBlocker.SET_ROTATION_INTRO))
		{
			Log.Offline.Print("SceneMgr.OnFatalError: Error blocked by set rotation.");
			SetNextMode(Mode.FATAL_ERROR);
			return;
		}
		if (!ReconnectMgr.IsReconnectAllowed(message))
		{
			if (message.m_reason == FatalErrorReason.MOBILE_GAME_SERVER_RPC_ERROR)
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
				{
					m_headerText = GameStrings.Get("GLOBAL_ERROR_GENERIC_HEADER"),
					m_text = GameStrings.Get("GLOBAL_MOBILE_ERROR_GAMESERVER_CONNECT"),
					m_showAlertIcon = false,
					m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
					m_confirmText = GameStrings.Get("GLOBAL_BUTTON_MORE_INFO"),
					m_cancelText = GameStrings.Get("GLOBAL_BUTTON_GOT_IT"),
					m_responseCallback = delegate(AlertPopup.Response response, object uData)
					{
						bool flag = false;
						bool flag2 = false;
						switch (response)
						{
						case AlertPopup.Response.CONFIRM:
							Application.OpenURL(ExternalUrlService.Get().GetMobileGameServerConnectionLink());
							flag = true;
							break;
						case AlertPopup.Response.CANCEL:
							flag2 = true;
							break;
						}
						GameServerInfo lastGameServerJoined = Network.Get().GetLastGameServerJoined();
						TelemetryManager.Client().SendMobileFailConnectGameServer(lastGameServerJoined?.Address ?? null, flag, flag2);
						Log.Telemetry.PrintInfo("{0}, {1}, {2}", lastGameServerJoined?.Address ?? null, flag, flag2);
					}
				};
				DialogManager.Get().ShowPopup(info);
			}
			else
			{
				FatalErrorMgr.Get().RemoveErrorListener(OnFatalError);
				if (ServiceManager.TryGet<ReconnectMgr>(out var reconnectMgr))
				{
					reconnectMgr.FullResetRequired = true;
				}
				GoToFatalErrorScreen(message);
			}
			return;
		}
		switch (m_mode)
		{
		case Mode.LOGIN:
		case Mode.GAMEPLAY:
			GoToFatalErrorScreen(message);
			return;
		case Mode.COLLECTIONMANAGER:
			CollectionManager.Get().HandleDisconnect();
			return;
		case Mode.HUB:
			StoreManager.Get().HandleDisconnect();
			return;
		case Mode.TAVERN_BRAWL:
		{
			CollectionManager collectionMgr = CollectionManager.Get();
			if (collectionMgr.IsInEditMode())
			{
				collectionMgr.HandleDisconnect();
			}
			return;
		}
		case Mode.STARTUP:
		case Mode.PACKOPENING:
		case Mode.TOURNAMENT:
		case Mode.CREDITS:
			return;
		}
		Log.Offline.PrintDebug("Bypassing Fatal Error To HUB.");
		Navigation.Clear();
		if (!IsTransitionNowOrPending() || m_nextMode != Mode.HUB)
		{
			DialogManager.Get().ShowReconnectHelperDialog();
		}
		SetNextMode(Mode.HUB);
	}

	private void GoToFatalErrorScreen(FatalErrorMessage message)
	{
		if (HearthstoneApplication.Get().ResetOnErrorIfNecessary())
		{
			Log.Offline.PrintDebug("SceneMgr.GoToFatalErrorScreen() - Auto resetting. Do not display Fatal Error Screen.");
			return;
		}
		Log.BattleNet.PrintDebug("Set FatalError mode={0}, m_allowClick={1}, m_redirectToStore={2}", m_mode, message.m_allowClick, message.m_redirectToStore);
		FatalErrorMgr.Get().SetUnrecoverable(m_mode == Mode.STARTUP && (!message.m_allowClick || !message.m_redirectToStore));
		SetNextMode(Mode.FATAL_ERROR);
	}

	public bool DoesCurrentSceneSupportOfflineActivity()
	{
		switch (m_mode)
		{
		case Mode.STARTUP:
		case Mode.HUB:
		case Mode.COLLECTIONMANAGER:
		case Mode.PACKOPENING:
		case Mode.TOURNAMENT:
		case Mode.CREDITS:
		case Mode.TAVERN_BRAWL:
		case Mode.LETTUCE_COLLECTION:
			return true;
		default:
			return false;
		}
	}

	private void UpdatePerformanceTrackingFromModeSwitch(Mode mode, PegasusScene scene, object userData)
	{
		if (mode == Mode.GAMEPLAY)
		{
			HearthstonePerformance hearthstonePerformance = HearthstonePerformance.Get();
			if (hearthstonePerformance != null)
			{
				hearthstonePerformance.CaptureBoxInteractableTime();
				UnregisterSceneLoadedEvent(UpdatePerformanceTrackingFromModeSwitch);
			}
		}
	}

	private string GetSceneNameFromMode(Mode mode)
	{
		return EnumUtils.GetString(mode);
	}
}
