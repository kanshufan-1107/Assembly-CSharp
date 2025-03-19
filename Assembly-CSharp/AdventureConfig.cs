using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core;
using Hearthstone.DataModels;
using Hearthstone.DungeonCrawl;
using Hearthstone.Store;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class AdventureConfig : MonoBehaviour
{
	public delegate void DelBossDefLoaded(AdventureBossDef bossDef, bool success);

	public delegate void AdventureModeChange(AdventureDbId adventureId, AdventureModeDbId modeId);

	public delegate void AdventureMissionSet(ScenarioDbId mission, bool showDetails);

	public delegate void SubSceneChange(AdventureData.Adventuresubscene newscene, bool forward);

	public delegate void SelectedModeChange(AdventureDbId adventureId, AdventureModeDbId modeId);

	public delegate void AnomalyModeChangedHandler(bool anomalyModeActived);

	public const string DEFAULT_SET_UP_STATE = "SetUpState";

	public const string PLAY_BUTTON_ANOMALY_ACTIVE_STATE = "PURPLE_SWIRL";

	public const string PLAY_BUTTON_ANOMALY_INACTIVE_STATE = "BLUE_SWIRL";

	private static AdventureConfig s_instance;

	private AdventureDbId m_SelectedAdventure = AdventureDbId.PRACTICE;

	private AdventureModeDbId m_SelectedMode = AdventureModeDbId.LINEAR;

	private Stack<AdventureData.Adventuresubscene> m_SubSceneBackStack = new Stack<AdventureData.Adventuresubscene>();

	private AdventureData.Adventuresubscene m_CurrentSubScene;

	private AdventureData.Adventuresubscene m_PreviousSubScene = AdventureData.Adventuresubscene.INVALID;

	private ScenarioDbId m_SelectedMission;

	private ScenarioDbId m_MissionOverride;

	private bool m_anomalyModeActivated;

	private List<long> NeedsChapterNewlyUnlockedHighlight = new List<long>();

	private bool m_allChaptersOwned;

	private Reward.Type m_completionRewardType;

	private int m_completionRewardId;

	private List<AdventureModeChange> m_AdventureModeChangeEventList = new List<AdventureModeChange>();

	private List<SubSceneChange> m_SubSceneChangeEventList = new List<SubSceneChange>();

	private List<SelectedModeChange> m_SelectedModeChangeEventList = new List<SelectedModeChange>();

	private List<AdventureMissionSet> m_AdventureMissionSetEventList = new List<AdventureMissionSet>();

	private Map<string, int> m_WingBossesDefeatedCache = new Map<string, int>();

	private Map<string, ScenarioDbId> m_LastSelectedMissions = new Map<string, ScenarioDbId>();

	private Map<ScenarioDbId, bool> m_CachedDefeatedScenario = new Map<ScenarioDbId, bool>();

	private Map<ScenarioDbId, AdventureBossDef> m_CachedBossDef = new Map<ScenarioDbId, AdventureBossDef>();

	private Map<AdventureDbId, AdventureModeDbId> m_ClientChooserAdventureModes = new Map<AdventureDbId, AdventureModeDbId>();

	private AdventureDbId SelectedAdventure
	{
		get
		{
			return m_SelectedAdventure;
		}
		set
		{
			if (value != m_SelectedAdventure)
			{
				ResetLoadout();
			}
			m_SelectedAdventure = value;
			AdventureDataModel adventureDataModel = GetAdventureDataModel();
			if (adventureDataModel != null)
			{
				adventureDataModel.SelectedAdventure = value;
				adventureDataModel.IsDuelsAdventure = AdventureUtils.IsDuelsAdventure(value);
				AdventureDbfRecord record = GameDbf.Adventure.GetRecord((int)value);
				adventureDataModel.StoreDescriptionTextTimelockedTrue = ((record != null) ? ((string)record.StoreBuyRemainingWingsDescTimelockedTrue) : string.Empty);
				adventureDataModel.StoreDescriptionTextTimelockedFalse = ((record != null) ? ((string)record.StoreBuyRemainingWingsDescTimelockedFalse) : string.Empty);
			}
		}
	}

	private AdventureModeDbId SelectedMode
	{
		get
		{
			return m_SelectedMode;
		}
		set
		{
			if (value != m_SelectedMode)
			{
				ResetLoadout();
			}
			m_SelectedMode = value;
			AdventureDataModel adventureDataModel = GetAdventureDataModel();
			if (adventureDataModel != null)
			{
				adventureDataModel.SelectedAdventureMode = value;
				adventureDataModel.IsSelectedModeHeroic = GameUtils.IsModeHeroic(value);
			}
		}
	}

	public AdventureData.Adventuresubscene CurrentSubScene => m_CurrentSubScene;

	public AdventureData.Adventuresubscene PreviousSubScene => m_PreviousSubScene;

	public bool AnomalyModeActivated
	{
		get
		{
			return m_anomalyModeActivated;
		}
		set
		{
			if (value != m_anomalyModeActivated)
			{
				m_anomalyModeActivated = value;
				AdventureDataModel dataModel = GetAdventureDataModel();
				if (dataModel != null)
				{
					dataModel.AnomalyActivated = m_anomalyModeActivated;
				}
				if (this.OnAnomalyModeChanged != null)
				{
					this.OnAnomalyModeChanged(value);
				}
			}
		}
	}

	public long SelectedHeroCardDbId { get; set; }

	public long SelectedLoadoutTreasureDbId { get; set; }

	public long SelectedDeckId { get; set; }

	public long SelectedHeroPowerDbId { get; set; }

	public bool ShouldSeeFirstTimeFlow
	{
		get
		{
			return GetAdventureDataModel().ShouldSeeFirstTimeFlow;
		}
		set
		{
			GetAdventureDataModel().ShouldSeeFirstTimeFlow = value;
		}
	}

	public bool AllChaptersOwned
	{
		get
		{
			return m_allChaptersOwned;
		}
		set
		{
			m_allChaptersOwned = value;
			AdventureDataModel dataModel = GetAdventureDataModel();
			if (dataModel != null)
			{
				dataModel.AllChaptersOwned = m_allChaptersOwned;
			}
		}
	}

	public RewardListDataModel CompletionRewards
	{
		get
		{
			AdventureDataModel dataModel = GetAdventureDataModel();
			if (dataModel != null)
			{
				if (dataModel.CompletionRewards == null)
				{
					dataModel.CompletionRewards = new RewardListDataModel();
				}
				return dataModel.CompletionRewards;
			}
			return null;
		}
		set
		{
			AdventureDataModel dataModel = GetAdventureDataModel();
			if (dataModel != null)
			{
				dataModel.CompletionRewards = value;
			}
		}
	}

	public Reward.Type CompletionRewardType
	{
		get
		{
			return m_completionRewardType;
		}
		set
		{
			m_completionRewardType = value;
			AdventureDataModel dataModel = GetAdventureDataModel();
			if (dataModel != null)
			{
				dataModel.CompletionRewardType = m_completionRewardType;
			}
		}
	}

	public int CompletionRewardId
	{
		get
		{
			return m_completionRewardId;
		}
		set
		{
			m_completionRewardId = value;
			AdventureDataModel dataModel = GetAdventureDataModel();
			if (dataModel != null)
			{
				dataModel.CompletionRewardId = m_completionRewardId;
			}
		}
	}

	public event AnomalyModeChangedHandler OnAnomalyModeChanged;

	public event Action OnAdventureSceneUnloadEvent;

	public static AdventureConfig Get()
	{
		return s_instance;
	}

	public static AdventureData.Adventuresubscene GetSubSceneFromMode(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)adventureId, (int)modeId);
		if (dataRecord == null)
		{
			Debug.LogErrorFormat("AdventureConfig.GetSubSceneFromMode() - No Adventure Data record found for Adventure {0} and Mode {1}", (int)adventureId, (int)modeId);
			return AdventureData.Adventuresubscene.CHOOSER;
		}
		if (dataRecord.StartingSubscene == AdventureData.Adventuresubscene.DUNGEON_CRAWL)
		{
			return Get().GetCorrectSubSceneWhenLoadingDungeonCrawlMode();
		}
		return dataRecord.StartingSubscene;
	}

	public AdventureDbId GetSelectedAdventure()
	{
		return SelectedAdventure;
	}

	public AdventureModeDbId GetSelectedMode()
	{
		return SelectedMode;
	}

	public AdventureDataModel GetAdventureDataModel()
	{
		if (!GlobalDataContext.Get().GetDataModel(7, out var dataModel))
		{
			dataModel = new AdventureDataModel();
			GlobalDataContext.Get().BindDataModel(dataModel);
		}
		AdventureDataModel obj = dataModel as AdventureDataModel;
		if (obj == null)
		{
			Log.Adventures.PrintWarning("AdventureDataModel is null!");
		}
		return obj;
	}

	public AdventureDataDbfRecord GetSelectedAdventureDataRecord()
	{
		return GetAdventureDataRecord(GetSelectedAdventure(), GetSelectedMode());
	}

	public AdventureModeDbId GetClientChooserAdventureMode(AdventureDbId adventureDbId)
	{
		if (m_ClientChooserAdventureModes.TryGetValue(adventureDbId, out var advMode))
		{
			return advMode;
		}
		if (SelectedAdventure != adventureDbId)
		{
			return AdventureModeDbId.LINEAR;
		}
		return SelectedMode;
	}

	public static AdventureDataDbfRecord GetAdventureDataRecord(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		return GameDbf.AdventureData.GetRecord((AdventureDataDbfRecord r) => r.AdventureId == (int)adventureId && r.ModeId == (int)modeId);
	}

	public static bool CanPlayMode(AdventureDbId adventureId, AdventureModeDbId modeId, bool checkEventTimings = true)
	{
		bool unlockedAllHeroes = GameModeUtils.HasUnlockedAllDefaultHeroes();
		if (adventureId == AdventureDbId.PRACTICE)
		{
			if (modeId == AdventureModeDbId.EXPERT)
			{
				return unlockedAllHeroes;
			}
			return true;
		}
		if (!unlockedAllHeroes && AdventureUtils.DoesAdventureRequireAllHeroesUnlocked(adventureId, modeId))
		{
			return false;
		}
		if (modeId == AdventureModeDbId.LINEAR || modeId == AdventureModeDbId.DUNGEON_CRAWL)
		{
			return true;
		}
		return GameDbf.Scenario.GetRecord((ScenarioDbfRecord r) => r.AdventureId == (int)adventureId && r.ModeId == (int)modeId && r.WingId > 0 && AdventureProgressMgr.Get().CanPlayScenario(r.ID, checkEventTimings)) != null;
	}

	public static bool IsFeaturedMode(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		if (!CanPlayMode(adventureId, modeId))
		{
			return false;
		}
		Option hasSeenFeaturedOption = GetHasSeenFeaturedModeOptionFromAdventureData(adventureId, modeId);
		if (hasSeenFeaturedOption == Option.INVALID)
		{
			return false;
		}
		return !Options.Get().GetBool(hasSeenFeaturedOption, defaultVal: false);
	}

	public static bool MarkFeaturedMode(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		if (!CanPlayMode(adventureId, modeId))
		{
			return false;
		}
		Option hasSeenFeaturedOption = GetHasSeenFeaturedModeOptionFromAdventureData(adventureId, modeId);
		if (hasSeenFeaturedOption == Option.INVALID)
		{
			return false;
		}
		Options.Get().SetBool(hasSeenFeaturedOption, val: true);
		return true;
	}

	public static bool ShouldShowNewModePopup(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		if (!CanPlayMode(adventureId, modeId))
		{
			return false;
		}
		Option hasSeenFeaturedOption = GetHasSeenNewModePopupOptionFromAdventureData(adventureId, modeId);
		if (hasSeenFeaturedOption == Option.INVALID)
		{
			return false;
		}
		return !Options.Get().GetBool(hasSeenFeaturedOption, defaultVal: false);
	}

	public static bool MarkHasSeenNewModePopup(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		if (!CanPlayMode(adventureId, modeId))
		{
			return false;
		}
		Option hasSeenFeaturedOption = GetHasSeenNewModePopupOptionFromAdventureData(adventureId, modeId);
		if (hasSeenFeaturedOption == Option.INVALID)
		{
			return false;
		}
		Options.Get().SetBool(hasSeenFeaturedOption, val: true);
		return true;
	}

	private static Option GetHasSeenFeaturedModeOptionFromAdventureData(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		AdventureDataDbfRecord record = GameUtils.GetAdventureDataRecord((int)adventureId, (int)modeId);
		if (record == null)
		{
			return Option.INVALID;
		}
		return OptionUtils.GetOptionFromString(record.HasSeenFeaturedModeOption);
	}

	private static Option GetHasSeenNewModePopupOptionFromAdventureData(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		AdventureDataDbfRecord record = GameUtils.GetAdventureDataRecord((int)adventureId, (int)modeId);
		if (record == null)
		{
			return Option.INVALID;
		}
		return OptionUtils.GetOptionFromString(record.HasSeenNewModePopupOption);
	}

	public string GetSelectedAdventureAndModeString()
	{
		return $"{SelectedAdventure}_{SelectedMode}";
	}

	public void SetSelectedAdventureMode(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		SelectedAdventure = adventureId;
		SelectedMode = modeId;
		m_ClientChooserAdventureModes[adventureId] = modeId;
		Options.Get().SetInt(Option.SELECTED_ADVENTURE, (int)SelectedAdventure);
		Options.Get().SetInt(Option.SELECTED_ADVENTURE_MODE, (int)SelectedMode);
		SetPropertiesForAdventureAndMode();
		FireSelectedModeChangeEvent();
	}

	public void MarkHasSeenFirstTimeFlowComplete()
	{
		if (!GameUtils.IsModeHeroic(SelectedMode))
		{
			AdventureDataDbfRecord adventureRecord = GameDbf.AdventureData.GetRecord((AdventureDataDbfRecord r) => r.AdventureId == (int)SelectedAdventure && r.ModeId == (int)SelectedMode);
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest((GameSaveKeyId)adventureRecord.GameSaveDataClientKey, GameSaveKeySubkeyId.ADVENTURE_HAS_SEEN_FIRST_TIME_FLOW, 1L));
			ShouldSeeFirstTimeFlow = false;
		}
	}

	public void UpdateShouldSeeFirstTimeFlowForSelectedMode()
	{
		if (GameUtils.IsModeHeroic(SelectedMode))
		{
			ShouldSeeFirstTimeFlow = false;
			return;
		}
		long firstTimeUserFlowSubkeyValue = 0L;
		AdventureDataDbfRecord adventureRecord = GameDbf.AdventureData.GetRecord((AdventureDataDbfRecord r) => r.AdventureId == (int)SelectedAdventure && r.ModeId == (int)SelectedMode);
		if (adventureRecord == null)
		{
			ShouldSeeFirstTimeFlow = true;
			return;
		}
		if (adventureRecord.GameSaveDataClientKey <= 0)
		{
			ShouldSeeFirstTimeFlow = false;
			return;
		}
		GameSaveDataManager.Get().GetSubkeyValue((GameSaveKeyId)adventureRecord.GameSaveDataClientKey, GameSaveKeySubkeyId.ADVENTURE_HAS_SEEN_FIRST_TIME_FLOW, out firstTimeUserFlowSubkeyValue);
		ShouldSeeFirstTimeFlow = firstTimeUserFlowSubkeyValue <= 0;
	}

	public static AdventureModeDbId GetDefaultModeDbIdForAdventure(AdventureDbId adventureId)
	{
		if (adventureId == AdventureDbId.INVALID)
		{
			return AdventureModeDbId.INVALID;
		}
		return (AdventureModeDbId)(GameDbf.AdventureData.GetRecord((AdventureDataDbfRecord r) => r.AdventureId == (int)adventureId)?.ModeId ?? 0);
	}

	public ScenarioDbId GetMission()
	{
		return m_SelectedMission;
	}

	public ScenarioDbId GetMissionToPlay()
	{
		if (m_MissionOverride == ScenarioDbId.INVALID)
		{
			return GetMission();
		}
		return m_MissionOverride;
	}

	public ScenarioDbId GetLastSelectedMission()
	{
		string currentMode = GetSelectedAdventureAndModeString();
		ScenarioDbId mission = ScenarioDbId.INVALID;
		m_LastSelectedMissions.TryGetValue(currentMode, out mission);
		return mission;
	}

	public bool IsScenarioDefeatedAndInitCache(ScenarioDbId mission)
	{
		bool defeated = AdventureProgressMgr.Get().HasDefeatedScenario((int)mission);
		if (!m_CachedDefeatedScenario.ContainsKey(mission))
		{
			m_CachedDefeatedScenario[mission] = defeated;
		}
		return defeated;
	}

	public bool IsScenarioJustDefeated(ScenarioDbId mission)
	{
		bool defeated = AdventureProgressMgr.Get().HasDefeatedScenario((int)mission);
		bool previousDefeated = false;
		m_CachedDefeatedScenario.TryGetValue(mission, out previousDefeated);
		m_CachedDefeatedScenario[mission] = defeated;
		return defeated != previousDefeated;
	}

	public AdventureBossDef GetBossDef(ScenarioDbId mission)
	{
		AdventureBossDef bossDef = null;
		if (!m_CachedBossDef.TryGetValue(mission, out bossDef) && !string.IsNullOrEmpty(GetBossDefAssetPath(mission)))
		{
			Debug.LogErrorFormat("Boss def for mission not loaded: {0}\nCall LoadBossDef first.", mission);
		}
		return bossDef;
	}

	public void LoadBossDef(ScenarioDbId mission, DelBossDefLoaded callback)
	{
		AdventureBossDef cachedBossDef = null;
		if (m_CachedBossDef.TryGetValue(mission, out cachedBossDef))
		{
			callback(cachedBossDef, success: true);
			return;
		}
		string assetPath = GetBossDefAssetPath(mission);
		if (string.IsNullOrEmpty(assetPath))
		{
			if (callback != null)
			{
				callback(null, success: false);
			}
			return;
		}
		PrefabCallback<GameObject> loadAttemptedCallback = delegate(AssetReference path, GameObject go, object data)
		{
			if (go == null)
			{
				Debug.LogError($"Unable to instantiate boss def: {path}");
				callback?.Invoke(null, success: false);
			}
			else
			{
				AdventureBossDef component = go.GetComponent<AdventureBossDef>();
				if (component == null)
				{
					Debug.LogError($"Object does not contain AdventureBossDef component: {path}");
				}
				else
				{
					m_CachedBossDef[mission] = component;
				}
				callback?.Invoke(component, component != null);
			}
		};
		if (!AssetLoader.Get().InstantiatePrefab(assetPath, loadAttemptedCallback))
		{
			loadAttemptedCallback(assetPath, null, null);
		}
	}

	public static string GetBossDefAssetPath(ScenarioDbId mission)
	{
		return GameDbf.AdventureMission.GetRecord((AdventureMissionDbfRecord r) => r.ScenarioId == (int)mission)?.BossDefAssetPath;
	}

	public void ClearBossDefs()
	{
		foreach (KeyValuePair<ScenarioDbId, AdventureBossDef> item in m_CachedBossDef)
		{
			UnityEngine.Object.Destroy(item.Value);
		}
		m_CachedBossDef.Clear();
	}

	public void SetMission(ScenarioDbId mission, bool showDetails = true)
	{
		m_SelectedMission = mission;
		Log.Adventures.Print("Selected Mission set to {0}", mission);
		string currentMode = GetSelectedAdventureAndModeString();
		m_LastSelectedMissions[currentMode] = mission;
		AdventureMissionSet[] array = m_AdventureMissionSetEventList.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](mission, showDetails);
		}
	}

	public void SetMissionOverride(ScenarioDbId missionOverride)
	{
		m_MissionOverride = missionOverride;
	}

	public ScenarioDbId GetMissionOverride()
	{
		return m_MissionOverride;
	}

	public bool DoesSelectedMissionRequireDeck()
	{
		return DoesMissionRequireDeck(m_SelectedMission);
	}

	public static bool DoesMissionRequireDeck(ScenarioDbId scenario)
	{
		ScenarioDbfRecord mission = GameDbf.Scenario.GetRecord((int)scenario);
		if (mission == null)
		{
			return true;
		}
		return mission.Player1DeckId == 0;
	}

	public void AddAdventureMissionSetListener(AdventureMissionSet dlg)
	{
		m_AdventureMissionSetEventList.Add(dlg);
	}

	public void RemoveAdventureMissionSetListener(AdventureMissionSet dlg)
	{
		m_AdventureMissionSetEventList.Remove(dlg);
	}

	public void AddAdventureModeChangeListener(AdventureModeChange dlg)
	{
		m_AdventureModeChangeEventList.Add(dlg);
	}

	public void RemoveAdventureModeChangeListener(AdventureModeChange dlg)
	{
		m_AdventureModeChangeEventList.Remove(dlg);
	}

	public void AddSubSceneChangeListener(SubSceneChange dlg)
	{
		m_SubSceneChangeEventList.Add(dlg);
	}

	public void RemoveSubSceneChangeListener(SubSceneChange dlg)
	{
		m_SubSceneChangeEventList.Remove(dlg);
	}

	public void AddSelectedModeChangeListener(SelectedModeChange dlg)
	{
		m_SelectedModeChangeEventList.Add(dlg);
	}

	public void RemoveSelectedModeChangeListener(SelectedModeChange dlg)
	{
		m_SelectedModeChangeEventList.Remove(dlg);
	}

	public void ResetSubScene(AdventureData.Adventuresubscene subscene)
	{
		m_CurrentSubScene = subscene;
		m_PreviousSubScene = AdventureData.Adventuresubscene.INVALID;
		m_SubSceneBackStack.Clear();
	}

	public void ChangeSubScene(AdventureData.Adventuresubscene subscene, bool pushToBackStack = true)
	{
		if (subscene == m_CurrentSubScene)
		{
			Debug.Log($"Sub scene {subscene} is already set.");
			return;
		}
		if (pushToBackStack)
		{
			m_SubSceneBackStack.Push(m_CurrentSubScene);
		}
		m_PreviousSubScene = m_CurrentSubScene;
		m_CurrentSubScene = subscene;
		FireSubSceneChangeEvent(forward: true);
		FireAdventureModeChangeEvent();
	}

	public void SubSceneGoBack(bool fireevent = true)
	{
		if (m_SubSceneBackStack.Count == 0)
		{
			Debug.Log("No sub scenes exist in the back stack.");
			return;
		}
		m_PreviousSubScene = m_CurrentSubScene;
		m_CurrentSubScene = m_SubSceneBackStack.Pop();
		if (fireevent)
		{
			FireSubSceneChangeEvent(forward: false);
		}
		FireAdventureModeChangeEvent();
	}

	public void RemoveSubScenesFromStackUntilTargetReached(AdventureData.Adventuresubscene targetSubscene)
	{
		while (m_SubSceneBackStack.Count > 0 && m_SubSceneBackStack.Peek() != targetSubscene)
		{
			m_SubSceneBackStack.Pop();
		}
	}

	public void RemoveSubSceneIfOnTopOfStack(AdventureData.Adventuresubscene subscene)
	{
		if (m_SubSceneBackStack.Peek() == subscene)
		{
			m_SubSceneBackStack.Pop();
		}
	}

	public void ChangeSubSceneToSelectedAdventure()
	{
		RequestGameSaveDataKeysForSelectedAdventure(delegate(bool success)
		{
			if (success)
			{
				if (GameUtils.DoesAdventureModeUseDungeonCrawlFormat(GetSelectedMode()))
				{
					AdventureDataDbfRecord selectedAdventureDataRecord = GetSelectedAdventureDataRecord();
					if (selectedAdventureDataRecord != null)
					{
						DungeonCrawlUtil.MigrateDungeonCrawlSubkeys((GameSaveKeyId)selectedAdventureDataRecord.GameSaveDataClientKey, (GameSaveKeyId)selectedAdventureDataRecord.GameSaveDataServerKey);
					}
				}
			}
			else
			{
				Debug.LogError("ChangeSubSceneToSelectedAdventure - Request for Adventure Game Save Keys failed.");
			}
			AdventureData.Adventuresubscene subSceneFromMode = GetSubSceneFromMode(SelectedAdventure, SelectedMode);
			UpdateShouldSeeFirstTimeFlowForSelectedMode();
			if (ShouldSeeFirstTimeFlow && AllChaptersOwned && !AdventureUtils.IsEntireAdventureFree(SelectedAdventure))
			{
				MarkHasSeenFirstTimeFlowComplete();
			}
			ChangeSubScene(subSceneFromMode);
		});
	}

	public void RequestGameSaveDataKeysForSelectedAdventure(GameSaveDataManager.OnRequestDataResponseDelegate onCompleteCallback)
	{
		AdventureDataDbfRecord dataRecord = GetSelectedAdventureDataRecord();
		List<GameSaveKeyId> gameSaveKeysToRequest = new List<GameSaveKeyId>();
		if (dataRecord != null && dataRecord.GameSaveDataClientKey != 0)
		{
			gameSaveKeysToRequest.Add((GameSaveKeyId)dataRecord.GameSaveDataClientKey);
		}
		if (dataRecord != null && dataRecord.GameSaveDataServerKey != 0)
		{
			gameSaveKeysToRequest.Add((GameSaveKeyId)dataRecord.GameSaveDataServerKey);
		}
		if (GameUtils.IsModeHeroic(GetSelectedMode()))
		{
			AdventureDataDbfRecord normalModeDataRecord = GetAdventureDataRecord(GetSelectedAdventure(), GameUtils.GetNormalModeFromHeroicMode(GetSelectedMode()));
			if (normalModeDataRecord != null && normalModeDataRecord.GameSaveDataClientKey != 0)
			{
				gameSaveKeysToRequest.Add((GameSaveKeyId)normalModeDataRecord.GameSaveDataClientKey);
			}
		}
		if (gameSaveKeysToRequest.Count > 0)
		{
			GameSaveDataManager.Get().Request(gameSaveKeysToRequest, onCompleteCallback);
		}
		else
		{
			onCompleteCallback(success: true);
		}
	}

	public static bool IsMissionAvailable(int missionId)
	{
		bool canPlay = AdventureProgressMgr.Get().CanPlayScenario(missionId);
		if (!canPlay)
		{
			return false;
		}
		int missionReqProgress = 0;
		int wingId = 0;
		if (!GetMissionPlayableParameters(missionId, ref wingId, ref missionReqProgress))
		{
			return false;
		}
		int ackProgress = 0;
		AdventureProgressMgr.Get().GetWingAck(wingId, out ackProgress);
		if (canPlay)
		{
			return missionReqProgress <= ackProgress;
		}
		return false;
	}

	public static bool IsMissionNewlyAvailableAndGetReqs(int missionId, ref int wingId, ref int missionReqProgress)
	{
		if (!GetMissionPlayableParameters(missionId, ref wingId, ref missionReqProgress))
		{
			return false;
		}
		bool canPlay = AdventureProgressMgr.Get().CanPlayScenario(missionId);
		int ackProgress = 0;
		AdventureProgressMgr.Get().GetWingAck(wingId, out ackProgress);
		if (ackProgress < missionReqProgress && canPlay)
		{
			return true;
		}
		return false;
	}

	public static bool AckCurrentWingProgress(int wingId)
	{
		return SetWingAckIfGreater(wingId, AdventureProgressMgr.Get().GetProgressValueForWing(wingId));
	}

	public static bool SetWingAckIfGreater(int wingId, int ackProgress)
	{
		int currentAckProgress = 0;
		AdventureProgressMgr.Get().GetWingAck(wingId, out currentAckProgress);
		if (ackProgress > currentAckProgress)
		{
			AdventureProgressMgr.Get().SetWingAck(wingId, ackProgress);
			return true;
		}
		return false;
	}

	public static bool ShouldDisplayAdventure(AdventureDbId adventureId)
	{
		if (GameUtils.IsAdventureRotated(adventureId) && !AdventureProgressMgr.Get().OwnsOneOrMoreAdventureWings(adventureId))
		{
			return false;
		}
		if (adventureId != AdventureDbId.PRACTICE && !GameModeUtils.HasUnlockedAllDefaultHeroes() && !AdventureProgressMgr.Get().OwnsOneOrMoreAdventureWings(adventureId) && AdventureUtils.DoesAdventureRequireAllHeroesUnlocked(adventureId))
		{
			return false;
		}
		if (IsAdventureComingSoon(adventureId))
		{
			return true;
		}
		if (!IsAdventureEventActive(adventureId))
		{
			return false;
		}
		return true;
	}

	public static bool IsAdventureEventActive(AdventureDbId advId)
	{
		bool isEventActive = true;
		foreach (WingDbfRecord wing in GameDbf.Wing.GetRecords())
		{
			if (wing.AdventureId == (int)advId)
			{
				if (AdventureProgressMgr.IsWingEventActive(wing.ID))
				{
					return true;
				}
				isEventActive = false;
			}
		}
		return isEventActive;
	}

	public static EventTimingType GetEarliestWingEventTiming(AdventureDbId advId)
	{
		EventTimingType earliestEventTiming = EventTimingType.SPECIAL_EVENT_NEVER;
		foreach (WingDbfRecord wing in GameDbf.Wing.GetRecords())
		{
			if (wing.AdventureId == (int)advId)
			{
				EventTimingType currentEventTiming = AdventureProgressMgr.GetWingEventTiming(wing.ID);
				if (earliestEventTiming == EventTimingType.SPECIAL_EVENT_NEVER || EventTimingManager.Get().GetEventStartTimeUtc(currentEventTiming) < EventTimingManager.Get().GetEventStartTimeUtc(earliestEventTiming))
				{
					earliestEventTiming = currentEventTiming;
				}
			}
		}
		return earliestEventTiming;
	}

	public static bool IsAdventureComingSoon(AdventureDbId advId)
	{
		AdventureDbfRecord adventureRecord = GameDbf.Adventure.GetRecord((int)advId);
		if (adventureRecord == null)
		{
			Debug.LogErrorFormat("IsAdventureComingSoon - Adventure Id is invalid: {0}", (int)advId);
			return false;
		}
		return EventTimingManager.Get().IsEventActive(adventureRecord.ComingSoonEvent);
	}

	public static AdventureDbId GetAdventurePlayerShouldSee(out int latestActiveAdventureWing)
	{
		latestActiveAdventureWing = 0;
		if (!Options.Get().GetBool(Option.HAS_SEEN_PRACTICE_MODE, defaultVal: false) && !GameUtils.IsTraditionalTutorialComplete())
		{
			return AdventureDbId.INVALID;
		}
		AdventureDbfRecord adventureWithHighestSortOrder = GetActiveExpansionAdventureWithHighestSortOrder();
		if (adventureWithHighestSortOrder == null)
		{
			return AdventureDbId.INVALID;
		}
		long latestWingId = AdventureUtils.GetFinalAdventureWing(adventureWithHighestSortOrder.ID, excludeOwnedWings: false, excludeInactiveWings: true);
		latestActiveAdventureWing = (int)latestWingId;
		long latestSeenWingId = 0L;
		if (!GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PLAYER_OPTIONS, GameSaveKeySubkeyId.LATEST_ADVENTURE_WING_SEEN, out latestSeenWingId))
		{
			latestSeenWingId = 2522L;
		}
		if (latestWingId != latestSeenWingId)
		{
			return (AdventureDbId)adventureWithHighestSortOrder.ID;
		}
		return AdventureDbId.INVALID;
	}

	public static AdventureDbId GetAdventurePlayerShouldSee()
	{
		int latestWing = 0;
		return GetAdventurePlayerShouldSee(out latestWing);
	}

	public static AdventureDbfRecord GetActiveExpansionAdventureWithHighestSortOrder()
	{
		List<AdventureDbfRecord> adventureRecordsWithDefPrefab = GameUtils.GetAdventureRecordsWithDefPrefab();
		AdventureDbfRecord adventureWithHighestSortOrder = null;
		foreach (AdventureDbfRecord advRecord in adventureRecordsWithDefPrefab)
		{
			if (GameUtils.IsExpansionAdventure((AdventureDbId)advRecord.ID) && ShouldDisplayAdventure((AdventureDbId)advRecord.ID) && !IsAdventureComingSoon((AdventureDbId)advRecord.ID) && (adventureWithHighestSortOrder == null || advRecord.SortOrder > adventureWithHighestSortOrder.SortOrder))
			{
				adventureWithHighestSortOrder = advRecord;
			}
		}
		return adventureWithHighestSortOrder;
	}

	public static bool GetMissionPlayableParameters(int missionId, ref int wingId, ref int missionReqProgress)
	{
		ScenarioDbfRecord scenarioRecord = GameDbf.Scenario.GetRecord(missionId);
		if (scenarioRecord == null)
		{
			return false;
		}
		AdventureMissionDbfRecord progressRecord = GameDbf.AdventureMission.GetRecord((AdventureMissionDbfRecord r) => r.ScenarioId == scenarioRecord.ID);
		if (progressRecord == null)
		{
			return false;
		}
		WingDbfRecord wingRecord = GameDbf.Wing.GetRecord(progressRecord.ReqWingId);
		if (wingRecord == null)
		{
			return false;
		}
		missionReqProgress = progressRecord.ReqProgress;
		wingId = wingRecord.ID;
		return true;
	}

	public int GetWingBossesDefeated(AdventureDbId advId, AdventureModeDbId mode, WingDbId wing, int defaultvalue = 0)
	{
		int bosses = 0;
		if (m_WingBossesDefeatedCache.TryGetValue(GetWingUniqueId(advId, mode, wing), out bosses))
		{
			return bosses;
		}
		return defaultvalue;
	}

	public void UpdateWingBossesDefeated(AdventureDbId advId, AdventureModeDbId mode, WingDbId wing, int bossesDefeated)
	{
		m_WingBossesDefeatedCache[GetWingUniqueId(advId, mode, wing)] = bossesDefeated;
	}

	private string GetWingUniqueId(AdventureDbId advId, AdventureModeDbId modeId, WingDbId wing)
	{
		return $"{advId}_{modeId}_{wing}";
	}

	private void Awake()
	{
		s_instance = this;
		base.gameObject.AddComponent<HSDontDestroyOnLoad>();
	}

	private void Start()
	{
		StoreManager.Get().RegisterSuccessfulPurchaseAckListener(OnSuccessfulPurchaseAck);
		AddSubSceneChangeListener(OnSubSceneChange);
	}

	private void OnDestroy()
	{
		StoreManager.Get().RemoveSuccessfulPurchaseAckListener(OnSuccessfulPurchaseAck);
		s_instance = null;
	}

	public void OnAdventureSceneAwake()
	{
		SelectedAdventure = Options.Get().GetEnum(Option.SELECTED_ADVENTURE, AdventureDbId.PRACTICE);
		SelectedMode = Options.Get().GetEnum(Option.SELECTED_ADVENTURE_MODE, AdventureModeDbId.LINEAR);
		if (!ShouldDisplayAdventure(SelectedAdventure))
		{
			SelectedAdventure = AdventureDbId.PRACTICE;
			SelectedMode = AdventureModeDbId.LINEAR;
		}
		SetPropertiesForAdventureAndMode();
	}

	public void OnAdventureSceneUnload()
	{
		if (this.OnAdventureSceneUnloadEvent != null)
		{
			this.OnAdventureSceneUnloadEvent();
		}
		SelectedAdventure = AdventureDbId.INVALID;
		SelectedMode = AdventureModeDbId.INVALID;
	}

	public void ResetSubScene()
	{
		ResetSubScene(AdventureData.Adventuresubscene.CHOOSER);
	}

	private void FireAdventureModeChangeEvent()
	{
		AdventureModeChange[] array = m_AdventureModeChangeEventList.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](SelectedAdventure, SelectedMode);
		}
	}

	private void FireSubSceneChangeEvent(bool forward)
	{
		UpdatePresence();
		SubSceneChange[] array = m_SubSceneChangeEventList.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](m_CurrentSubScene, forward);
		}
	}

	private void FireSelectedModeChangeEvent()
	{
		SelectedModeChange[] array = m_SelectedModeChangeEventList.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](SelectedAdventure, SelectedMode);
		}
	}

	public void UpdatePresence()
	{
		AdventureData.Adventuresubscene currentSubScene = m_CurrentSubScene;
		if ((uint)(currentSubScene - 2) <= 4u || currentSubScene == AdventureData.Adventuresubscene.LOCATION_SELECT)
		{
			PresenceMgr.Get().SetStatus_EnteringAdventure(SelectedAdventure, SelectedMode);
		}
		else if (AdventureScene.Get() != null && !AdventureScene.Get().IsUnloading())
		{
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.ADVENTURE_CHOOSING_MODE);
		}
	}

	public bool IsHeroSelectedBeforeDungeonCrawlScreenForSelectedAdventure()
	{
		return GetSelectedAdventureDataRecord()?.DungeonCrawlPickHeroFirst ?? false;
	}

	public bool IsChapterSelectedBeforeDungeonCrawlScreenForSelectedAdventure()
	{
		return GetSelectedAdventureDataRecord()?.DungeonCrawlSelectChapter ?? false;
	}

	private bool ValidLoadoutIsLockedInForSelectedAdventure()
	{
		AdventureDataDbfRecord dataRecord = GetSelectedAdventureDataRecord();
		GameSaveKeyId adventureServerKey = (GameSaveKeyId)dataRecord.GameSaveDataServerKey;
		if (!GameSaveDataManager.Get().ValidateIfKeyCanBeAccessed(adventureServerKey, dataRecord.Name))
		{
			return false;
		}
		GameSaveDataManager.Get().GetSubkeyValue(adventureServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_SCENARIO_ID, out long selectedScenario);
		GameSaveDataManager.Get().GetSubkeyValue(adventureServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_POWER, out long selectedHeroPower);
		GameSaveDataManager.Get().GetSubkeyValue(adventureServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_DECK, out long selectedDeck);
		GameSaveDataManager.Get().GetSubkeyValue(adventureServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_LOADOUT_TREASURE_ID, out long selectedTreasure);
		if (dataRecord.DungeonCrawlSaveHeroUsingHeroDbId)
		{
			GameSaveDataManager.Get().GetSubkeyValue(adventureServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_CARD_DB_ID, out long selectedHeroCardDbId);
			return AdventureUtils.IsValidLoadoutForSelectedAdventureAndHero(SelectedAdventure, SelectedMode, (ScenarioDbId)selectedScenario, (int)selectedHeroCardDbId, (int)selectedHeroPower, (int)selectedTreasure);
		}
		GameSaveDataManager.Get().GetSubkeyValue(adventureServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_CLASS, out long selectedHeroClass);
		return AdventureUtils.IsValidLoadoutForSelectedAdventureAndClass(SelectedAdventure, SelectedMode, (ScenarioDbId)selectedScenario, (TAG_CLASS)selectedHeroClass, (int)selectedHeroPower, (int)selectedDeck, (int)selectedTreasure);
	}

	public bool GuestHeroesExistForCurrentAdventure()
	{
		return GameDbf.AdventureGuestHeroes.HasRecord((AdventureGuestHeroesDbfRecord r) => r.AdventureId == (int)GetSelectedAdventure());
	}

	public List<GuestHero> GetGuestHeroesForCurrentAdventure()
	{
		return AdventureUtils.GetGuestHeroesForAdventure(GetSelectedAdventure());
	}

	public static List<int> GetGuestHeroesForWing(int wingId)
	{
		List<AdventureGuestHeroesDbfRecord> records = GameDbf.AdventureGuestHeroes.GetRecords((AdventureGuestHeroesDbfRecord r) => r.WingId == wingId);
		records.Sort((AdventureGuestHeroesDbfRecord a, AdventureGuestHeroesDbfRecord b) => a.SortOrder.CompareTo(b.SortOrder));
		List<int> cardDbIds = new List<int>();
		foreach (AdventureGuestHeroesDbfRecord record in records)
		{
			cardDbIds.Add(GameUtils.GetCardIdFromGuestHeroDbId(record.GuestHeroId));
		}
		return cardDbIds;
	}

	public static int GetAdventureBossesInRun(WingDbfRecord wingRecord)
	{
		if (wingRecord == null)
		{
			Debug.LogError("GetAdventureBossesInRun - no WingDbfRecord passed in!");
			return 0;
		}
		return wingRecord.DungeonCrawlBosses;
	}

	public AdventureData.Adventuresubscene SubSceneForPickingHeroForCurrentAdventure()
	{
		if (!GuestHeroesExistForCurrentAdventure())
		{
			return AdventureData.Adventuresubscene.MISSION_DECK_PICKER;
		}
		return AdventureData.Adventuresubscene.ADVENTURER_PICKER;
	}

	public AdventureData.Adventuresubscene GetCorrectSubSceneWhenLoadingDungeonCrawlMode()
	{
		bool isCurrentDungeonRunActiveOrLockedIn = DungeonCrawlUtil.IsDungeonRunInProgress(SelectedAdventure, SelectedMode) || ValidLoadoutIsLockedInForSelectedAdventure();
		if (!isCurrentDungeonRunActiveOrLockedIn && IsChapterSelectedBeforeDungeonCrawlScreenForSelectedAdventure())
		{
			return AdventureData.Adventuresubscene.LOCATION_SELECT;
		}
		if (!isCurrentDungeonRunActiveOrLockedIn && IsHeroSelectedBeforeDungeonCrawlScreenForSelectedAdventure())
		{
			return SubSceneForPickingHeroForCurrentAdventure();
		}
		return AdventureData.Adventuresubscene.DUNGEON_CRAWL;
	}

	private void OnSuccessfulPurchaseAck(ProductInfo bundle, PaymentMethod purchaseMethod)
	{
		EvaluateIfAllWingsOwnedForSelectedAdventure();
	}

	private void OnSubSceneChange(AdventureData.Adventuresubscene subScene, bool forward)
	{
		bool num = GameUtils.DoesAdventureModeUseDungeonCrawlFormat(GetSelectedMode());
		bool isHeroPickerSubScene = subScene == AdventureData.Adventuresubscene.MISSION_DECK_PICKER || subScene == AdventureData.Adventuresubscene.ADVENTURER_PICKER;
		if (num && isHeroPickerSubScene)
		{
			WingDbId wingId = GameUtils.GetWingIdFromMissionId(GetMission());
			DungeonCrawlSubDef_VOLines.PlayVOLine(GetSelectedAdventure(), wingId, 0, DungeonCrawlSubDef_VOLines.VOEventType.CHARACTER_SELECT);
		}
	}

	private void SetPropertiesForAdventureAndMode()
	{
		EvaluateIfAllWingsOwnedForSelectedAdventure();
		UpdateCompletionRewards();
	}

	private void EvaluateIfAllWingsOwnedForSelectedAdventure()
	{
		if (SelectedAdventure != 0 && SelectedMode != 0)
		{
			AllChaptersOwned = AdventureProgressMgr.Get().OwnsAllAdventureWings(SelectedAdventure);
		}
	}

	private void UpdateCompletionRewards()
	{
		List<RewardData> adventureCompletionRewards = AdventureProgressMgr.GetRewardsForAdventureByMode((int)SelectedAdventure, (int)SelectedMode, new HashSet<Achieve.RewardTiming> { Achieve.RewardTiming.ADVENTURE_CHEST });
		Legacy_UpdateCompletionRewardData(adventureCompletionRewards);
		CompletionRewards.Items.Clear();
		foreach (RewardData item in adventureCompletionRewards)
		{
			RewardItemDataModel rewardDataModel = RewardUtils.RewardDataToRewardItemDataModel(item);
			if (rewardDataModel != null)
			{
				CompletionRewards.Items.Add(rewardDataModel);
			}
		}
	}

	private void Legacy_UpdateCompletionRewardData(List<RewardData> adventureCompletionRewards)
	{
		bool foundReward = false;
		foreach (RewardData reward in adventureCompletionRewards)
		{
			if (reward is CardBackRewardData)
			{
				foundReward = true;
				CardBackRewardData cardbackReward = reward as CardBackRewardData;
				CompletionRewardType = Reward.Type.CARD_BACK;
				CompletionRewardId = cardbackReward.CardBackID;
			}
		}
		if (adventureCompletionRewards.Count < 1 || !foundReward)
		{
			CompletionRewardType = Reward.Type.NONE;
			CompletionRewardId = 0;
		}
	}

	public void ResetLoadout()
	{
		AnomalyModeActivated = false;
		SelectedHeroCardDbId = 0L;
		SelectedLoadoutTreasureDbId = 0L;
		SelectedHeroPowerDbId = 0L;
		SelectedDeckId = 0L;
		SetMissionOverride(ScenarioDbId.INVALID);
	}

	public void SetHasSeenUnlockedChapterPage(WingDbId wingId, bool hasSeen)
	{
		if (hasSeen)
		{
			NeedsChapterNewlyUnlockedHighlight.Remove((long)wingId);
		}
		else if (GetHasSeenUnlockedChapterPage(wingId))
		{
			NeedsChapterNewlyUnlockedHighlight.Add((long)wingId);
		}
	}

	public bool GetHasSeenUnlockedChapterPage(WingDbId wingId)
	{
		return !NeedsChapterNewlyUnlockedHighlight.Contains((long)wingId);
	}

	public bool HasUnacknowledgedChapterUnlocks()
	{
		foreach (WingDbfRecord wingRecord in GameDbf.Wing.GetRecords((WingDbfRecord r) => r.AdventureId == (int)SelectedAdventure))
		{
			AdventureChapterState num = AdventureProgressMgr.Get().AdventureBookChapterStateForWing(wingRecord, SelectedMode);
			AdventureProgressMgr.Get().GetWingAck(wingRecord.ID, out var chapterProgressAck);
			if (num == AdventureChapterState.UNLOCKED && chapterProgressAck == 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasValidLoadoutForSelectedAdventure()
	{
		if (GetSelectedAdventureDataRecord().DungeonCrawlSaveHeroUsingHeroDbId)
		{
			return AdventureUtils.IsValidLoadoutForSelectedAdventureAndHero(SelectedAdventure, SelectedMode, m_SelectedMission, (int)SelectedHeroCardDbId, (int)SelectedHeroPowerDbId, (int)SelectedLoadoutTreasureDbId);
		}
		return AdventureUtils.IsValidLoadoutForSelectedAdventureAndClass(SelectedAdventure, SelectedMode, m_SelectedMission, AdventureUtils.GetHeroClassFromHeroId((int)SelectedHeroCardDbId), (int)SelectedHeroPowerDbId, (int)SelectedDeckId, (int)SelectedLoadoutTreasureDbId);
	}
}
