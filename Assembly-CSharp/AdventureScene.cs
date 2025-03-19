using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core.Utils;
using Hearthstone;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class AdventureScene : PegasusScene
{
	public enum TransitionDirection
	{
		INVALID = -1,
		X,
		Y,
		Z,
		NX,
		NY,
		NZ
	}

	[Serializable]
	public class AdventureModeMusicWingOverride
	{
		public WingDbId m_wingId;

		public MusicPlaylistType m_playlist;
	}

	[Serializable]
	public class AdventureModeMusic
	{
		public AdventureData.Adventuresubscene m_subsceneId;

		public AdventureDbId m_adventureId;

		public MusicPlaylistType m_playlist;

		[CustomEditField(ListSortable = true)]
		public List<AdventureModeMusicWingOverride> m_wingOverrides;
	}

	[Serializable]
	public class AdventureSubSceneDef
	{
		[CustomEditField(ListSortable = true)]
		public AdventureData.Adventuresubscene m_SubScene;

		[CustomEditField(T = EditType.GAME_OBJECT)]
		public String_MobileOverride m_Prefab;

		public bool isWidget;
	}

	private static AdventureScene s_instance;

	[CustomEditField(Sections = "Transition Blocker")]
	public GameObject m_transitionClickBlocker;

	[CustomEditField(Sections = "Transition Motions")]
	public Vector3 m_SubScenePosition = Vector3.zero;

	[CustomEditField(Sections = "Transition Motions")]
	public float m_DefaultTransitionAnimationTime = 1f;

	[CustomEditField(Sections = "Transition Motions")]
	public iTween.EaseType m_TransitionEaseType = iTween.EaseType.easeInOutSine;

	[CustomEditField(Sections = "Transition Motions")]
	public TransitionDirection m_TransitionDirection;

	[CustomEditField(Sections = "Transition Sounds", T = EditType.SOUND_PREFAB)]
	public string m_SlideInSound;

	[CustomEditField(Sections = "Transition Sounds", T = EditType.SOUND_PREFAB)]
	public string m_SlideOutSound;

	[CustomEditField(Sections = "Adventure Subscene Prefabs")]
	public List<AdventureSubSceneDef> m_SubSceneDefs = new List<AdventureSubSceneDef>();

	[CustomEditField(Sections = "Music Settings")]
	public List<AdventureModeMusic> m_AdventureModeMusic = new List<AdventureModeMusic>();

	private GameObject m_TransitionOutSubSceneParent;

	private GameObject m_CurrentSubSceneParent;

	private GameObject m_TransitionOutSubScene;

	private GameObject m_CurrentSubScene;

	private bool m_transitionIsGoingBack;

	private int m_StartupAssetLoads;

	private int m_SubScenesLoaded;

	private bool m_MusicStopped;

	private bool m_Unloading;

	private TransitionDirection m_CurrentTransitionDirection;

	private bool m_isTransitioning;

	private bool m_isLoading;

	private Coroutine m_waitForSubSceneToLoadCoroutine;

	private const AdventureData.Adventuresubscene s_StartMode = AdventureData.Adventuresubscene.CHOOSER;

	private List<AdventureDbId> m_adventuresThatRequestedGameSaveData = new List<AdventureDbId>();

	private AdventureDefCache m_adventureDefCache;

	private AdventureWingDefCache m_adventureWingDefCache;

	public bool IsDevMode { get; set; }

	public int DevModeSetting { get; set; }

	protected override void Awake()
	{
		base.Awake();
		s_instance = this;
		m_CurrentSubScene = null;
		m_TransitionOutSubScene = null;
		m_CurrentTransitionDirection = m_TransitionDirection;
		AdventureConfig ac = AdventureConfig.Get();
		ac.OnAdventureSceneAwake();
		ac.AddSubSceneChangeListener(OnSubSceneChange);
		ac.AddSelectedModeChangeListener(OnSelectedModeChanged);
		ac.AddAdventureModeChangeListener(OnAdventureModeChanged);
		ac.AddAdventureMissionSetListener(OnAdventureMissionChanged);
		m_StartupAssetLoads++;
		SetCurrentTransitionDirection();
		if (HearthstoneApplication.IsInternal())
		{
			CheatMgr.Get().RegisterCategory("adventure");
			CheatMgr.Get().RegisterCheatHandler("advdev", OnDevCheat);
			CheatMgr.Get().DefaultCategory();
		}
		m_adventureDefCache = new AdventureDefCache(preloadRecords: true);
		m_adventureWingDefCache = new AdventureWingDefCache(preloadRecords: true);
		NotifyAchieveManagerOfAdventureSceneLoaded();
		LoadSubScene(ac.CurrentSubScene, OnFirstSubSceneLoaded, new Action(OnStartupAssetLoaded));
	}

	private void Start()
	{
		AdventureConfig.Get().UpdatePresence();
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	private void Update()
	{
		Network.Get().ProcessNetwork();
	}

	public static AdventureScene Get()
	{
		return s_instance;
	}

	public override bool IsUnloading()
	{
		return m_Unloading;
	}

	public override void Unload()
	{
		m_Unloading = true;
		AdventureConfig adventureConfig = AdventureConfig.Get();
		adventureConfig.ClearBossDefs();
		DeckPickerTray.Get().Unload();
		adventureConfig.RemoveAdventureModeChangeListener(OnAdventureModeChanged);
		adventureConfig.RemoveSelectedModeChangeListener(OnSelectedModeChanged);
		adventureConfig.RemoveSubSceneChangeListener(OnSubSceneChange);
		adventureConfig.OnAdventureSceneUnload();
		CheatMgr.Get().UnregisterCheatHandler("advdev", OnDevCheat);
		if (m_CurrentSubScene != null)
		{
			UnityEngine.Object.Destroy(m_CurrentSubScene);
		}
		if (m_transitionClickBlocker != null)
		{
			UnityEngine.Object.Destroy(m_transitionClickBlocker);
		}
		m_Unloading = false;
	}

	public override bool IsTransitioning()
	{
		return m_isTransitioning;
	}

	public bool IsInitialScreen()
	{
		return m_SubScenesLoaded <= 1;
	}

	public AdventureDef GetAdventureDef(AdventureDbId advId)
	{
		return m_adventureDefCache.GetDef(advId);
	}

	public List<AdventureDef> GetSortedAdventureDefs()
	{
		List<AdventureDef> list = new List<AdventureDef>(m_adventureDefCache.Values);
		list.Sort((AdventureDef l, AdventureDef r) => r.GetSortOrder() - l.GetSortOrder());
		return list;
	}

	public AdventureWingDef GetWingDef(WingDbId wingId)
	{
		return m_adventureWingDefCache.GetDef(wingId);
	}

	private void UpdateAdventureModeMusic()
	{
		AdventureDbId adventureId = AdventureConfig.Get().GetSelectedAdventure();
		AdventureData.Adventuresubscene subsceneId = AdventureConfig.Get().CurrentSubScene;
		MusicPlaylistType? usePlaylist = null;
		foreach (AdventureModeMusic music in m_AdventureModeMusic)
		{
			if (music.m_subsceneId == subsceneId && music.m_adventureId == adventureId)
			{
				usePlaylist = ((!GetAdventureModeMusicWingOverride(music, out var wingPlaylist)) ? new MusicPlaylistType?(music.m_playlist) : wingPlaylist);
				break;
			}
			if (music.m_subsceneId == subsceneId && music.m_adventureId == AdventureDbId.INVALID)
			{
				usePlaylist = music.m_playlist;
			}
		}
		if (usePlaylist.HasValue)
		{
			MusicManager.Get().StartPlaylist(usePlaylist.Value);
		}
	}

	private static bool GetAdventureModeMusicWingOverride(AdventureModeMusic music, out MusicPlaylistType? playlist)
	{
		playlist = null;
		if (music == null || music.m_wingOverrides.Count == 0)
		{
			return false;
		}
		ScenarioDbId scenarioId = AdventureConfig.Get().GetLastSelectedMission();
		if (scenarioId == ScenarioDbId.INVALID)
		{
			return false;
		}
		WingDbId wingId = (WingDbId)(GameUtils.GetWingRecordFromMissionId((int)scenarioId)?.ID ?? 0);
		if (wingId == WingDbId.INVALID)
		{
			return false;
		}
		foreach (AdventureModeMusicWingOverride wingOverride in music.m_wingOverrides)
		{
			if (wingOverride.m_wingId == wingId)
			{
				playlist = wingOverride.m_playlist;
				return true;
			}
		}
		return false;
	}

	private void OnStartupAssetLoaded()
	{
		m_StartupAssetLoads--;
		if (m_StartupAssetLoads <= 0)
		{
			UpdateAdventureModeMusic();
			SceneMgr.Get().NotifySceneLoaded();
		}
	}

	private void LoadSubScene(AdventureData.Adventuresubscene subscene)
	{
		LoadSubScene(subscene, OnSubSceneLoaded);
	}

	private void LoadSubScene(AdventureData.Adventuresubscene subscene, GameObjectCallback callback, object callbackData = null)
	{
		AdventureSubSceneDef subSceneDef = m_SubSceneDefs.Find((AdventureSubSceneDef item) => item.m_SubScene == subscene);
		if (subSceneDef == null)
		{
			Debug.LogErrorFormat("Subscene {0} prefab not defined in m_SubSceneDefs", subscene);
			return;
		}
		if (m_isLoading || m_isTransitioning)
		{
			Debug.LogErrorFormat("Attempting to load subscene {0}, but another subscene is already loading! This is a bad idea!", subscene);
			return;
		}
		m_isTransitioning = true;
		m_isLoading = true;
		EnableTransitionBlocker(block: true);
		if (m_waitForSubSceneToLoadCoroutine != null)
		{
			StopCoroutine(m_waitForSubSceneToLoadCoroutine);
		}
		GameObjectCallback runCallback = callback;
		if (subSceneDef.isWidget)
		{
			WidgetInstance widgetInstance = WidgetInstance.Create(subSceneDef.m_Prefab);
			widgetInstance.RegisterReadyListener(delegate
			{
				SetUpSubSceneParent(widgetInstance.gameObject);
				if (runCallback != null)
				{
					runCallback((string)subSceneDef.m_Prefab, widgetInstance.Widget.gameObject, callbackData);
				}
				UpdateAdventureModeMusic();
				m_isLoading = false;
			});
			return;
		}
		AssetLoader.Get().InstantiatePrefab((string)subSceneDef.m_Prefab, delegate(AssetReference assetRef, GameObject go, object data)
		{
			SetUpSubSceneParent(go);
			if (runCallback != null)
			{
				runCallback(assetRef, go, data);
			}
			UpdateAdventureModeMusic();
			m_isLoading = false;
		}, callbackData, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private void OnSubSceneChange(AdventureData.Adventuresubscene newscene, bool forward)
	{
		m_transitionIsGoingBack = !forward;
		LoadSubScene(newscene);
	}

	private Vector3 GetMoveDirection()
	{
		float mag = 1f;
		if (m_CurrentTransitionDirection >= TransitionDirection.NX)
		{
			mag *= -1f;
		}
		Vector3 dir = Vector3.zero;
		dir[(int)m_CurrentTransitionDirection % 3] = mag;
		return dir;
	}

	private void OnFirstSubSceneLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		OnSubSceneLoaded(assetRef, go, callbackData);
	}

	private void SetUpSubSceneParent(GameObject parent)
	{
		m_TransitionOutSubSceneParent = m_CurrentSubSceneParent;
		m_CurrentSubSceneParent = parent;
		GameUtils.SetParent(m_CurrentSubSceneParent, base.transform);
		m_CurrentSubSceneParent.transform.position = new Vector3(-500f, 0f, 0f);
	}

	private void OnSubSceneLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_TransitionOutSubScene = m_CurrentSubScene;
		m_CurrentSubScene = go;
		m_SubScenesLoaded++;
		AdventureSubScene subscene = m_CurrentSubScene.GetComponent<AdventureSubScene>();
		Action callback = (Action)callbackData;
		if (subscene == null)
		{
			DoSubSceneTransition(subscene);
			callback?.Invoke();
		}
		else
		{
			m_waitForSubSceneToLoadCoroutine = StartCoroutine(WaitForSubSceneToLoad(callback));
		}
	}

	private void DoSubSceneTransition(AdventureSubScene subscene)
	{
		m_CurrentSubSceneParent.transform.localPosition = m_SubScenePosition;
		if (m_TransitionOutSubSceneParent == null)
		{
			CompleteTransition();
			return;
		}
		float animtime = ((subscene == null) ? m_DefaultTransitionAnimationTime : subscene.m_TransitionAnimationTime);
		Vector3 movedir = GetMoveDirection();
		GameObject delobj = m_TransitionOutSubSceneParent;
		AdventureSubScene fromSubscene = m_TransitionOutSubScene.GetComponent<AdventureSubScene>();
		bool useBackTransition = m_transitionIsGoingBack;
		bool num = fromSubscene != null && fromSubscene.m_reverseTransitionAfterThisSubscene && !m_transitionIsGoingBack;
		bool goingBackToSubsceneWithReverseTransitionAfterThis = subscene != null && subscene.m_reverseTransitionAfterThisSubscene && m_transitionIsGoingBack;
		bool advancingToSubsceneWithReverseTransitionBeforeThis = subscene != null && subscene.m_reverseTransitionBeforeThisSubscene && !m_transitionIsGoingBack;
		bool goingBackFromSubsceneWithReverseTransitionBeforeThis = fromSubscene != null && fromSubscene.m_reverseTransitionBeforeThisSubscene && m_transitionIsGoingBack;
		if (num || goingBackToSubsceneWithReverseTransitionAfterThis || advancingToSubsceneWithReverseTransitionBeforeThis || goingBackFromSubsceneWithReverseTransitionBeforeThis)
		{
			useBackTransition = !useBackTransition;
		}
		if (useBackTransition)
		{
			AdventureSubScene boundscheck = fromSubscene;
			Vector3 outbounds = ((boundscheck == null) ? TransformUtil.GetBoundsOfChildren(m_TransitionOutSubScene).size : ((Vector3)boundscheck.m_SubSceneBounds));
			Vector3 outpos = m_TransitionOutSubSceneParent.transform.localPosition;
			outpos.x -= outbounds.x * movedir.x;
			outpos.y -= outbounds.y * movedir.y;
			outpos.z -= outbounds.z * movedir.z;
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("islocal", true);
			args.Add("position", outpos);
			args.Add("time", animtime);
			args.Add("easetype", m_TransitionEaseType);
			args.Add("oncomplete", (Action<object>)delegate
			{
				DestroyTransitioningSubScene(delobj);
				CompleteTransition();
			});
			args.Add("oncompletetarget", base.gameObject);
			iTween.MoveTo(m_TransitionOutSubScene, args);
			if (!string.IsNullOrEmpty(m_SlideOutSound))
			{
				SoundManager.Get().LoadAndPlay(m_SlideOutSound);
			}
		}
		else
		{
			AdventureSubScene boundscheck2 = m_CurrentSubScene.GetComponent<AdventureSubScene>();
			Vector3 inbounds = ((boundscheck2 == null) ? TransformUtil.GetBoundsOfChildren(m_CurrentSubScene).size : ((Vector3)boundscheck2.m_SubSceneBounds));
			Vector3 inpos = m_CurrentSubSceneParent.transform.localPosition;
			Vector3 setpos = m_CurrentSubSceneParent.transform.localPosition;
			setpos.x -= inbounds.x * movedir.x;
			setpos.y -= inbounds.y * movedir.y;
			setpos.z -= inbounds.z * movedir.z;
			m_CurrentSubScene.transform.localPosition = setpos;
			Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
			args2.Add("islocal", true);
			args2.Add("position", inpos);
			args2.Add("time", animtime);
			args2.Add("easetype", m_TransitionEaseType);
			args2.Add("oncomplete", (Action<object>)delegate
			{
				DestroyTransitioningSubScene(delobj);
				CompleteTransition();
			});
			args2.Add("oncompletetarget", base.gameObject);
			iTween.MoveTo(m_CurrentSubScene, args2);
			if (!string.IsNullOrEmpty(m_SlideInSound))
			{
				SoundManager.Get().LoadAndPlay(m_SlideInSound);
			}
		}
		m_TransitionOutSubScene = null;
	}

	private void DestroyTransitioningSubScene(GameObject destroysubscene)
	{
		if (destroysubscene != null)
		{
			UnityEngine.Object.Destroy(destroysubscene);
		}
	}

	private void CompleteTransition()
	{
		m_isTransitioning = false;
		AdventureSubScene subscene = m_CurrentSubScene.GetComponent<AdventureSubScene>();
		if (subscene != null)
		{
			subscene.NotifyTransitionComplete();
			UpdateAdventureModeMusic();
		}
		EnableTransitionBlocker(block: false);
	}

	private IEnumerator WaitForSubSceneToLoad(Action callback = null)
	{
		AdventureSubScene subscene = m_CurrentSubScene.GetComponent<AdventureSubScene>();
		while (!subscene.IsLoaded())
		{
			yield return null;
		}
		DoSubSceneTransition(subscene);
		callback?.Invoke();
	}

	private void OnSelectedModeChanged(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		UpdateAdventureModeMusic();
		if (!AdventureConfig.CanPlayMode(adventureId, modeId))
		{
			return;
		}
		AdventureDataDbfRecord adventureDataRecord = GameUtils.GetAdventureDataRecord((int)adventureId, (int)modeId);
		SetCurrentTransitionDirection();
		GameSaveKeyId gameSaveDataClientKey = (GameSaveKeyId)adventureDataRecord.GameSaveDataClientKey;
		if (gameSaveDataClientKey != 0)
		{
			bool isGameSaveDataReady = GameSaveDataManager.Get().IsDataReady(gameSaveDataClientKey);
			if (!isGameSaveDataReady && !m_adventuresThatRequestedGameSaveData.Contains(adventureId))
			{
				m_adventuresThatRequestedGameSaveData.Add(adventureId);
				GameSaveDataManager.Get().Request(gameSaveDataClientKey, OnRequestGameSaveDataClientResponse_CreateIntroConversation);
			}
			else if (isGameSaveDataReady)
			{
				OnRequestGameSaveDataClientResponse_CreateIntroConversation(success: true);
			}
		}
	}

	private void OnRequestGameSaveDataClientResponse_CreateIntroConversation(bool success)
	{
		AdventureDbId selectedAdventure = AdventureConfig.Get().GetSelectedAdventure();
		AdventureModeDbId selectedMode = AdventureConfig.Get().GetSelectedMode();
		if (!success)
		{
			Log.Adventures.PrintWarning($"Unable to request game save data key for adventure: {selectedAdventure}.");
		}
		AdventureDef adventureDef = GetAdventureDef(selectedAdventure);
		if (adventureDef == null)
		{
			Log.Adventures.PrintError($"Unable to get adventure def for adventure: {selectedAdventure}.");
			return;
		}
		List<AdventureDef.IntroConversationLine> conversationLines = adventureDef.m_IntroConversationLines;
		bool shouldOnlyPlayIntroOnFirstSeen = adventureDef.m_ShouldOnlyPlayIntroOnFirstSeen;
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)selectedAdventure, (int)selectedMode);
		if (dataRecord == null)
		{
			Log.Adventures.PrintError($"Unable to get adventure data record for adventure = {selectedAdventure}, mode = {selectedMode}.");
			return;
		}
		GameSaveKeyId gameSaveDataClientKey = (GameSaveKeyId)dataRecord.GameSaveDataClientKey;
		long hasSeenAdventure = 0L;
		if (gameSaveDataClientKey != GameSaveKeyId.INVALID)
		{
			GameSaveDataManager.Get().GetSubkeyValue(gameSaveDataClientKey, GameSaveKeySubkeyId.ADVENTURE_HAS_SEEN_ADVENTURE, out hasSeenAdventure);
		}
		if (!shouldOnlyPlayIntroOnFirstSeen || hasSeenAdventure == 0L || IsDevMode)
		{
			OnSelectedModeChanged_CreateIntroConversation(0, conversationLines, gameSaveDataClientKey);
		}
	}

	private void OnSelectedModeChanged_CreateIntroConversation(int index, List<AdventureDef.IntroConversationLine> convoLines, GameSaveKeyId gameSaveClientKey)
	{
		Action<int> cbNextLine = null;
		if (index < convoLines.Count - 1)
		{
			cbNextLine = delegate
			{
				if (SceneMgr.Get() != null && SceneMgr.Get().GetMode() == SceneMgr.Mode.ADVENTURE)
				{
					OnSelectedModeChanged_CreateIntroConversation(index + 1, convoLines, gameSaveClientKey);
				}
			};
		}
		bool isDevMode = Get() != null && Get().IsDevMode;
		if (index >= convoLines.Count - 1 && !isDevMode && gameSaveClientKey != 0)
		{
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(gameSaveClientKey, GameSaveKeySubkeyId.ADVENTURE_HAS_SEEN_ADVENTURE, 1L));
		}
		if (index < convoLines.Count)
		{
			string text = GameStrings.Get(new AssetReference(convoLines[index].VoLinePrefab).GetLegacyAssetName());
			bool allowRepeat = isDevMode;
			NotificationManager.Get().CreateCharacterQuote(convoLines[index].CharacterPrefab, NotificationManager.DEFAULT_CHARACTER_POS, text, convoLines[index].VoLinePrefab, allowRepeat, 0f, cbNextLine);
		}
	}

	private void OnAdventureModeChanged(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		if (GameUtils.IsModeHeroic(modeId))
		{
			ShowHeroicWarning();
		}
		if (adventureId == AdventureDbId.NAXXRAMAS && !Options.Get().GetBool(Option.HAS_ENTERED_NAXX))
		{
			NotificationManager.Get().CreateKTQuote("VO_KT_INTRO2_40", "VO_KT_INTRO2_40.prefab:5615c7daf91a7ea4e8a4127b70a09682");
			Options.Get().SetBool(Option.HAS_ENTERED_NAXX, val: true);
		}
		UpdateAdventureModeMusic();
	}

	private void OnAdventureMissionChanged(ScenarioDbId mission, bool showDetails)
	{
		UpdateAdventureModeMusic();
	}

	private void ShowHeroicWarning()
	{
		if (!Options.Get().GetBool(Option.HAS_SEEN_HEROIC_WARNING))
		{
			Options.Get().SetBool(Option.HAS_SEEN_HEROIC_WARNING, val: true);
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_HEROIC_WARNING_TITLE");
			info.m_text = GameStrings.Get("GLUE_HEROIC_WARNING");
			info.m_showAlertIcon = true;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			DialogManager.Get().ShowPopup(info);
		}
	}

	private bool OnDevCheat(string func, string[] args, string rawArgs)
	{
		if (!HearthstoneApplication.IsInternal())
		{
			return true;
		}
		IsDevMode = true;
		if (args.Length != 0)
		{
			int devMode = 1;
			if (int.TryParse(args[0], out devMode))
			{
				if (devMode > 0)
				{
					IsDevMode = true;
					DevModeSetting = devMode;
				}
				else
				{
					IsDevMode = false;
					DevModeSetting = 0;
				}
			}
		}
		if (UIStatus.Get() != null)
		{
			UIStatus.Get().AddInfo($"{func}: IsDevMode={IsDevMode} DevModeSetting={DevModeSetting}");
		}
		return true;
	}

	private void EnableTransitionBlocker(bool block)
	{
		if (m_transitionClickBlocker != null)
		{
			m_transitionClickBlocker.SetActive(block);
		}
	}

	private void NotifyAchieveManagerOfAdventureSceneLoaded()
	{
		AchieveManager.Get().NotifyOfClick(Achievement.ClickTriggerType.BUTTON_ADVENTURE);
	}

	private void SetCurrentTransitionDirection()
	{
		AdventureDataDbfRecord dataRecord = AdventureConfig.Get().GetSelectedAdventureDataRecord();
		if (dataRecord == null)
		{
			m_CurrentTransitionDirection = m_TransitionDirection;
			return;
		}
		TransitionDirection parsedTransitionDirection = EnumUtils.SafeParse(dataRecord.SubsceneTransitionDirection, TransitionDirection.INVALID, ignoreCase: true);
		if (parsedTransitionDirection != TransitionDirection.INVALID)
		{
			m_CurrentTransitionDirection = parsedTransitionDirection;
		}
		else
		{
			m_CurrentTransitionDirection = m_TransitionDirection;
		}
	}
}
