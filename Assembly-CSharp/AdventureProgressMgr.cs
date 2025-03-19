using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using PegasusUtil;
using UnityEngine;

public class AdventureProgressMgr : IService
{
	public delegate void AdventureProgressUpdatedCallback(bool isStartupAction, AdventureMission.WingProgress oldProgress, AdventureMission.WingProgress newProgress, object userData);

	private class AdventureProgressUpdatedListener : EventListener<AdventureProgressUpdatedCallback>
	{
		public void Fire(bool isStartupAction, AdventureMission.WingProgress oldProgress, AdventureMission.WingProgress newProgress)
		{
			m_callback(isStartupAction, oldProgress, newProgress, m_userData);
		}
	}

	private Map<int, AdventureMission.WingProgress> m_wingProgress = new Map<int, AdventureMission.WingProgress>();

	private Map<int, int> m_wingAckState = new Map<int, int>();

	private Map<int, AdventureMission> m_missions = new Map<int, AdventureMission>();

	private List<AdventureProgressUpdatedListener> m_progressUpdatedListeners = new List<AdventureProgressUpdatedListener>();

	public bool IsReady { get; private set; }

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		LoadAdventureMissionsFromDBF();
		serviceLocator.Get<Network>().RegisterNetHandler(AdventureProgressResponse.PacketID.ID, OnAdventureProgress);
		serviceLocator.Get<NetCache>().RegisterNewNoticesListener(OnNewNotices);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[3]
		{
			typeof(Network),
			typeof(NetCache),
			typeof(GameDbf)
		};
	}

	public void Shutdown()
	{
	}

	private void WillReset()
	{
		m_wingProgress.Clear();
		m_wingAckState.Clear();
		m_progressUpdatedListeners.Clear();
		IsReady = false;
	}

	public static AdventureProgressMgr Get()
	{
		return ServiceManager.Get<AdventureProgressMgr>();
	}

	public static void InitRequests()
	{
		Network.Get().RequestAdventureProgress();
	}

	public bool RegisterProgressUpdatedListener(AdventureProgressUpdatedCallback callback)
	{
		return RegisterProgressUpdatedListener(callback, null);
	}

	public bool RegisterProgressUpdatedListener(AdventureProgressUpdatedCallback callback, object userData)
	{
		if (callback == null)
		{
			return false;
		}
		AdventureProgressUpdatedListener listener = new AdventureProgressUpdatedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_progressUpdatedListeners.Contains(listener))
		{
			return false;
		}
		m_progressUpdatedListeners.Add(listener);
		return true;
	}

	public bool RemoveProgressUpdatedListener(AdventureProgressUpdatedCallback callback)
	{
		return RemoveProgressUpdatedListener(callback, null);
	}

	public bool RemoveProgressUpdatedListener(AdventureProgressUpdatedCallback callback, object userData)
	{
		if (callback == null)
		{
			return false;
		}
		AdventureProgressUpdatedListener listener = new AdventureProgressUpdatedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_progressUpdatedListeners.Contains(listener))
		{
			return false;
		}
		m_progressUpdatedListeners.Remove(listener);
		return true;
	}

	public List<AdventureMission.WingProgress> GetAllProgress()
	{
		return new List<AdventureMission.WingProgress>(m_wingProgress.Values);
	}

	public AdventureMission.WingProgress GetProgress(int wing)
	{
		if (!m_wingProgress.TryGetValue(wing, out var result))
		{
			return null;
		}
		return result;
	}

	public int GetProgressValueForWing(int wing)
	{
		return GetProgress(wing)?.Progress ?? 0;
	}

	public bool OwnsOneOrMoreAdventureWings(AdventureDbId adventureID)
	{
		foreach (WingDbfRecord wingRecord in GameDbf.Wing.GetRecords())
		{
			if (wingRecord.AdventureId == (int)adventureID && OwnsWing(wingRecord.ID))
			{
				return true;
			}
		}
		return false;
	}

	public bool OwnsAllAdventureWings(AdventureDbId adventureID)
	{
		foreach (WingDbfRecord wingRecord in GameDbf.Wing.GetRecords())
		{
			if (wingRecord.AdventureId == (int)adventureID && !OwnsWing(wingRecord.ID))
			{
				return false;
			}
		}
		return true;
	}

	public bool OwnsWing(int wing)
	{
		if (!m_wingProgress.ContainsKey(wing))
		{
			return false;
		}
		return m_wingProgress[wing].IsOwned();
	}

	public WingDbfRecord GetFirstUnownedAdventureWing(AdventureDbId adventureID)
	{
		WingDbfRecord firstUnownedWing = null;
		foreach (WingDbfRecord wing in GameDbf.Wing.GetRecords((WingDbfRecord r) => r.AdventureId == (int)adventureID))
		{
			if (!OwnsWing(wing.ID) && (firstUnownedWing == null || wing.UnlockOrder < firstUnownedWing.UnlockOrder))
			{
				firstUnownedWing = wing;
			}
		}
		return firstUnownedWing;
	}

	public bool IsWingComplete(AdventureDbId adventureID, AdventureModeDbId modeID, WingDbId wingId)
	{
		bool wingHasUnackedProgress;
		return IsWingComplete(adventureID, modeID, wingId, out wingHasUnackedProgress);
	}

	public bool IsWingComplete(AdventureDbId adventureID, AdventureModeDbId modeID, WingDbId wingId, out bool wingHasUnackedProgress)
	{
		List<ScenarioDbfRecord> records = GameDbf.Scenario.GetRecords();
		wingHasUnackedProgress = false;
		foreach (ScenarioDbfRecord scenario in records)
		{
			if (scenario.AdventureId == (int)adventureID && scenario.ModeId == (int)modeID && scenario.WingId == (int)wingId)
			{
				bool missionHasUnackedProgress = false;
				if (!HasDefeatedScenario(scenario.ID, out missionHasUnackedProgress))
				{
					return false;
				}
				if (missionHasUnackedProgress)
				{
					wingHasUnackedProgress = true;
				}
			}
		}
		return true;
	}

	public bool IsAdventureModeAndSectionComplete(AdventureDbId adventureID, AdventureModeDbId modeID, int bookSection = 0)
	{
		foreach (ScenarioDbfRecord scenario in GameDbf.Scenario.GetRecords((ScenarioDbfRecord r) => r.AdventureId == (int)adventureID && r.ModeId == (int)modeID))
		{
			int dbfWingId = scenario.WingId;
			if (dbfWingId > 0)
			{
				WingDbfRecord wingRecord = GameDbf.Wing.GetRecord(dbfWingId);
				if (wingRecord != null && bookSection == wingRecord.BookSection && !HasDefeatedScenario(scenario.ID))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsAdventureComplete(AdventureDbId adventureID)
	{
		List<AdventureDataDbfRecord> modeRecords = GameDbf.AdventureData.GetRecords((AdventureDataDbfRecord r) => r.AdventureId == (int)adventureID);
		if (modeRecords.Count == 0)
		{
			Debug.LogWarningFormat("No Adventure mode records found for AdventureDbId {0}! Returning True for IsAdventureComplete()", adventureID);
			return true;
		}
		foreach (AdventureDataDbfRecord modeRecord in modeRecords)
		{
			if (!IsAdventureModeAndSectionComplete(adventureID, (AdventureModeDbId)modeRecord.ModeId))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsWingLocked(AdventureWingDef wingDef)
	{
		if (wingDef.GetWingId() == WingDbId.LOE_HALL_OF_EXPLORERS)
		{
			bool num = IsWingComplete(AdventureDbId.LOE, AdventureModeDbId.LINEAR, WingDbId.LOE_TEMPLE_OF_ORSIS);
			bool w2Complete = IsWingComplete(AdventureDbId.LOE, AdventureModeDbId.LINEAR, WingDbId.LOE_ULDAMAN);
			bool w3Complete = IsWingComplete(AdventureDbId.LOE, AdventureModeDbId.LINEAR, WingDbId.LOE_RUINED_CITY);
			return !(num && w2Complete && w3Complete);
		}
		if (wingDef.GetOpenPrereqId() != 0)
		{
			GetWingAck((int)wingDef.GetOpenPrereqId(), out var ackProgress);
			if (ackProgress < 1)
			{
				return true;
			}
			if (wingDef.GetMustCompleteOpenPrereq() && !IsWingComplete(wingDef.GetAdventureId(), AdventureConfig.Get().GetSelectedMode(), wingDef.GetOpenPrereqId()))
			{
				return true;
			}
		}
		return false;
	}

	public int GetNumPlayableAdventureScenarios(AdventureDbId adventureID, AdventureModeDbId modeID)
	{
		List<WingDbfRecord> records = GameDbf.Wing.GetRecords((WingDbfRecord r) => r.AdventureId == (int)adventureID);
		int numScenarios = 0;
		foreach (WingDbfRecord wing in records)
		{
			numScenarios += GetNumPlayableScenariosForWing(wing, modeID);
		}
		return numScenarios;
	}

	private int GetNumPlayableScenariosForWing(WingDbfRecord wing, AdventureModeDbId modeID)
	{
		int numScenarios = 0;
		if (!OwnsWing(wing.ID) || !IsWingEventActive(wing.ID))
		{
			return 0;
		}
		foreach (ScenarioDbfRecord scenario in GameDbf.Scenario.GetRecords((ScenarioDbfRecord r) => r.WingId == wing.ID && r.ModeId == (int)modeID))
		{
			if (!HasDefeatedScenario(scenario.ID) && CanPlayScenario(scenario.ID))
			{
				numScenarios++;
			}
		}
		return numScenarios;
	}

	public int GetPlayableClassChallenges(AdventureDbId adventureID, AdventureModeDbId modeID)
	{
		int progressCount = 0;
		foreach (ScenarioDbfRecord scenario in GameDbf.Scenario.GetRecords())
		{
			if (scenario.AdventureId == (int)adventureID && scenario.ModeId == (int)modeID && CanPlayScenario(scenario.ID) && !HasDefeatedScenario(scenario.ID))
			{
				progressCount++;
			}
		}
		return progressCount;
	}

	public static List<RewardData> GetRewardsForWing(int wing, HashSet<Assets.Achieve.RewardTiming> rewardTimings)
	{
		List<RewardData> rewardsForAdventureWing = AchieveManager.Get().GetRewardsForAdventureWing(wing, rewardTimings);
		List<RewardData> displayedRewards = new List<RewardData>();
		foreach (RewardData data in rewardsForAdventureWing)
		{
			if (Reward.Type.CARD == data.RewardType)
			{
				displayedRewards.Add(data as CardRewardData);
			}
			if (Reward.Type.CARD_BACK == data.RewardType)
			{
				displayedRewards.Add(data as CardBackRewardData);
			}
			if (Reward.Type.BOOSTER_PACK == data.RewardType)
			{
				displayedRewards.Add(data as BoosterPackRewardData);
			}
			if (Reward.Type.RANDOM_CARD == data.RewardType)
			{
				displayedRewards.Add(data as RandomCardRewardData);
			}
		}
		return displayedRewards;
	}

	public static List<RewardData> GetRewardsForAdventureByMode(int adventureId, int adventureModeId, HashSet<Assets.Achieve.RewardTiming> rewardTimings)
	{
		List<RewardData> rewardsForAdventureAndMode = AchieveManager.Get().GetRewardsForAdventureAndMode(adventureId, adventureModeId, rewardTimings);
		List<RewardData> displayedRewards = new List<RewardData>();
		foreach (RewardData data in rewardsForAdventureAndMode)
		{
			if (Reward.Type.CARD == data.RewardType)
			{
				displayedRewards.Add(data as CardRewardData);
			}
			if (Reward.Type.CARD_BACK == data.RewardType)
			{
				displayedRewards.Add(data as CardBackRewardData);
			}
			if (Reward.Type.BOOSTER_PACK == data.RewardType)
			{
				displayedRewards.Add(data as BoosterPackRewardData);
			}
			if (Reward.Type.RANDOM_CARD == data.RewardType)
			{
				displayedRewards.Add(data as RandomCardRewardData);
			}
		}
		return displayedRewards;
	}

	public static EventTimingType GetWingEventTiming(int wing)
	{
		WingDbfRecord wingRecord = GameDbf.Wing.GetRecord(wing);
		if (wingRecord == null)
		{
			Debug.LogWarning($"AdventureProgressMgr.GetWingEventTiming could not find DBF record for wing {wing}, assuming it is has no open event");
			return EventTimingType.IGNORE;
		}
		EventTimingType specialEvent = wingRecord.RequiredEvent;
		if (specialEvent == EventTimingType.UNKNOWN)
		{
			Debug.LogWarning($"AdventureProgressMgr.GetWing wing={wing} could not find SpecialEventType record for event");
			return EventTimingType.IGNORE;
		}
		return specialEvent;
	}

	public static string GetWingName(int wing)
	{
		WingDbfRecord wingRecord = GameDbf.Wing.GetRecord(wing);
		if (wingRecord == null)
		{
			Debug.LogWarning($"AdventureProgressMgr.GetWingName could not find DBF record for wing {wing}");
			return string.Empty;
		}
		return wingRecord.Name;
	}

	public static bool IsWingEventActive(int wing)
	{
		EventTimingType specialEvent = GetWingEventTiming(wing);
		return EventTimingManager.Get().IsEventActive(specialEvent);
	}

	public bool CanPlayScenario(int scenarioID, bool checkEventTiming = true)
	{
		if (DemoMgr.Get().GetMode() == DemoMode.BLIZZCON_2015 && 1061 != scenarioID)
		{
			return false;
		}
		if (!m_missions.ContainsKey(scenarioID))
		{
			return true;
		}
		AdventureMission adventureMission = m_missions[scenarioID];
		if (!adventureMission.HasRequiredProgress())
		{
			return true;
		}
		AdventureMission.WingProgress currentProgress = GetProgress(adventureMission.RequiredProgress.Wing);
		if (currentProgress == null)
		{
			return false;
		}
		if (!currentProgress.MeetsProgressAndFlagsRequirements(adventureMission.RequiredProgress))
		{
			return false;
		}
		if (checkEventTiming && !IsWingEventActive(adventureMission.RequiredProgress.Wing))
		{
			return false;
		}
		return true;
	}

	public bool HasDefeatedScenario(int scenarioID)
	{
		bool hasUnackedProgress;
		return HasDefeatedScenario(scenarioID, out hasUnackedProgress);
	}

	public bool HasDefeatedScenario(int scenarioID, out bool hasUnackedProgress)
	{
		hasUnackedProgress = false;
		if (!m_missions.TryGetValue(scenarioID, out var adventureMission))
		{
			return false;
		}
		if (adventureMission.RequiredProgress == null)
		{
			return false;
		}
		if (adventureMission.GrantedProgress == null)
		{
			return false;
		}
		AdventureMission.WingProgress currentProgress = GetProgress(adventureMission.GrantedProgress.Wing);
		if (currentProgress == null)
		{
			return false;
		}
		GetWingAck(adventureMission.GrantedProgress.Wing, out var wingProgressAck);
		hasUnackedProgress = wingProgressAck < adventureMission.GrantedProgress.Progress;
		return currentProgress.MeetsProgressAndFlagsRequirements(adventureMission.GrantedProgress);
	}

	public static bool GetGameSaveDataProgressForScenario(int scenarioId, out int progress, out int maxProgress)
	{
		if (!ScenarioUsesGameSaveDataProgress(scenarioId))
		{
			progress = 0;
			maxProgress = 0;
			Debug.LogError($"Attempting to get Game Save Data progress for Scenario={scenarioId}, which does not have any Game Save Data. Add a GSD Subkey to that scenario's dbi.");
			return false;
		}
		ScenarioDbfRecord scenario = GameDbf.Scenario.GetRecord(scenarioId);
		GameSaveKeyId key = (GameSaveKeyId)AdventureConfig.Get().GetSelectedAdventureDataRecord().GameSaveDataServerKey;
		GameSaveKeySubkeyId subkey = (GameSaveKeySubkeyId)scenario.GameSaveDataProgressSubkey;
		GameSaveDataManager.Get().GetSubkeyValue(key, subkey, out long progressValue);
		progress = (int)progressValue;
		maxProgress = scenario.GameSaveDataProgressMax;
		return true;
	}

	public static bool ScenarioUsesGameSaveDataProgress(int scenarioId)
	{
		ScenarioDbfRecord scenario = GameDbf.Scenario.GetRecord(scenarioId);
		if (scenario.GameSaveDataProgressSubkey != 0)
		{
			return Enum.IsDefined(typeof(GameSaveKeySubkeyId), scenario.GameSaveDataProgressSubkey);
		}
		return false;
	}

	public bool ScenarioHasRewardData(int scenarioId)
	{
		List<RewardData> rewardData = GetImmediateRewardsForDefeatingScenario(scenarioId);
		if (rewardData != null)
		{
			return rewardData.Count > 0;
		}
		return false;
	}

	public List<RewardData> GetImmediateRewardsForDefeatingScenario(int scenarioID)
	{
		HashSet<Assets.Achieve.RewardTiming> rewardTimings = new HashSet<Assets.Achieve.RewardTiming> { Assets.Achieve.RewardTiming.IMMEDIATE };
		return GetRewardsForDefeatingScenario(scenarioID, rewardTimings);
	}

	public List<RewardData> GetRewardsForDefeatingScenario(int scenarioID, HashSet<Assets.Achieve.RewardTiming> rewardTimings)
	{
		if (!m_missions.TryGetValue(scenarioID, out var adventureMission))
		{
			return new List<RewardData>();
		}
		List<RewardData> rewards = null;
		if (GameUtils.IsHeroicAdventureMission(scenarioID) || GameUtils.IsClassChallengeMission(scenarioID) || adventureMission.GrantedProgress != null)
		{
			rewards = AchieveManager.Get().GetRewardsForAdventureScenario(adventureMission.GrantedProgress.Wing, scenarioID, rewardTimings);
		}
		return rewards;
	}

	public bool SetWingAck(int wing, int ackId)
	{
		Log.Adventures.Print("SetWingAck for wing {0}", wing);
		if (m_wingAckState.TryGetValue(wing, out var oldAckValue))
		{
			if (ackId < oldAckValue)
			{
				return false;
			}
			if (ackId == oldAckValue)
			{
				return true;
			}
		}
		m_wingAckState[wing] = ackId;
		Network.Get().AckWingProgress(wing, ackId);
		return true;
	}

	public bool GetWingAck(int wing, out int ack)
	{
		return m_wingAckState.TryGetValue(wing, out ack);
	}

	public AdventureMissionState AdventureMissionStateForScenario(int scenarioID)
	{
		if (HasDefeatedScenario(scenarioID))
		{
			return AdventureMissionState.COMPLETED;
		}
		if (CanPlayScenario(scenarioID))
		{
			return AdventureMissionState.UNLOCKED;
		}
		return AdventureMissionState.LOCKED;
	}

	public AdventureChapterState AdventureBookChapterStateForWing(WingDbfRecord wingRecord, AdventureModeDbId adventureMode)
	{
		if (IsWingComplete((AdventureDbId)wingRecord.AdventureId, adventureMode, (WingDbId)wingRecord.ID))
		{
			return AdventureChapterState.COMPLETED;
		}
		if (GetNumPlayableScenariosForWing(wingRecord, adventureMode) > 0)
		{
			return AdventureChapterState.UNLOCKED;
		}
		return AdventureChapterState.LOCKED;
	}

	public bool OwnershipPrereqWingIsOwned(AdventureWingDef wingDef)
	{
		if (wingDef.GetOwnershipPrereqId() != 0)
		{
			return OwnsWing((int)wingDef.GetOwnershipPrereqId());
		}
		return true;
	}

	public bool OwnershipPrereqWingIsOwned(WingDbfRecord wingRecord)
	{
		if (wingRecord.OwnershipPrereqWingId != 0)
		{
			return OwnsWing(wingRecord.OwnershipPrereqWingId);
		}
		return true;
	}

	private void LoadAdventureMissionsFromDBF()
	{
		foreach (AdventureMissionDbfRecord record in GameDbf.AdventureMission.GetRecords())
		{
			int scenarioID = record.ScenarioId;
			if (m_missions.ContainsKey(scenarioID))
			{
				Debug.LogWarning($"AdventureProgressMgr.LoadAdventureMissionsFromDBF(): duplicate entry found for scenario ID {scenarioID}");
				continue;
			}
			string description = record.NoteDesc;
			AdventureMission.WingProgress requiredProgress = new AdventureMission.WingProgress(record.ReqWingId, record.ReqProgress, record.ReqFlags);
			AdventureMission.WingProgress grantedProgress = new AdventureMission.WingProgress(record.GrantsWingId, record.GrantsProgress, record.GrantsFlags);
			m_missions[scenarioID] = new AdventureMission(scenarioID, description, requiredProgress, grantedProgress);
		}
	}

	private void OnAdventureProgress()
	{
		foreach (Network.AdventureProgress responseProgress in Network.Get().GetAdventureProgressResponse())
		{
			CreateOrUpdateProgress(isStartupAction: true, responseProgress.Wing, responseProgress.Progress);
			CreateOrUpdateWingFlags(isStartupAction: true, responseProgress.Wing, responseProgress.Flags);
			CreateOrUpdateWingAck(responseProgress.Wing, responseProgress.Ack);
		}
		IsReady = true;
	}

	private void OnNewNotices(List<NetCache.ProfileNotice> newNotices, bool isInitialNoticeList)
	{
		List<long> noticeIDsToAck = new List<long>();
		foreach (NetCache.ProfileNotice notice in newNotices)
		{
			if (notice.Type == NetCache.ProfileNotice.NoticeType.ADVENTURE_PROGRESS)
			{
				NetCache.ProfileNoticeAdventureProgress adventureProgressNotice = notice as NetCache.ProfileNoticeAdventureProgress;
				if (adventureProgressNotice.Progress.HasValue)
				{
					CreateOrUpdateProgress(isStartupAction: false, adventureProgressNotice.Wing, adventureProgressNotice.Progress.Value);
				}
				if (adventureProgressNotice.Flags.HasValue)
				{
					CreateOrUpdateWingFlags(isStartupAction: false, adventureProgressNotice.Wing, adventureProgressNotice.Flags.Value);
				}
				noticeIDsToAck.Add(notice.NoticeID);
			}
		}
		foreach (long noticeID in noticeIDsToAck)
		{
			Network.Get().AckNotice(noticeID);
		}
	}

	private void FireProgressUpdate(bool isStartupAction, AdventureMission.WingProgress oldProgress, AdventureMission.WingProgress newProgress)
	{
		AdventureProgressUpdatedListener[] array = m_progressUpdatedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(isStartupAction, oldProgress, newProgress);
		}
	}

	private void CreateOrUpdateProgress(bool isStartupAction, int wing, int progress)
	{
		if (!m_wingProgress.ContainsKey(wing))
		{
			m_wingProgress[wing] = new AdventureMission.WingProgress(wing, progress, 0uL);
			FireProgressUpdate(isStartupAction, null, m_wingProgress[wing]);
			return;
		}
		AdventureMission.WingProgress formerProgress = m_wingProgress[wing].Clone();
		m_wingProgress[wing].SetProgress(progress);
		Log.Adventures.Print("AdventureProgressMgr.CreateOrUpdateProgress: updating wing {0} : PROGRESS {1} (former progress {2})", wing, m_wingProgress[wing], formerProgress);
		FireProgressUpdate(isStartupAction, formerProgress, m_wingProgress[wing]);
	}

	private void CreateOrUpdateWingFlags(bool isStartupAction, int wing, ulong flags)
	{
		if (!m_wingProgress.ContainsKey(wing))
		{
			m_wingProgress[wing] = new AdventureMission.WingProgress(wing, 0, flags);
			Log.Adventures.Print("AdventureProgressMgr.CreateOrUpdateWingFlags: creating wing {0} : PROGRESS {1}", wing, m_wingProgress[wing]);
			FireProgressUpdate(isStartupAction, null, m_wingProgress[wing]);
		}
		else
		{
			AdventureMission.WingProgress formerProgress = m_wingProgress[wing].Clone();
			m_wingProgress[wing].SetFlags(flags);
			FireProgressUpdate(isStartupAction, formerProgress, m_wingProgress[wing]);
		}
	}

	private void CreateOrUpdateWingAck(int wing, int ack)
	{
		m_wingAckState[wing] = ack;
	}
}
