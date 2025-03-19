using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Logging;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusUtil;
using UnityEngine;

namespace Hearthstone.Progression;

public class TavernGuideManager : IService
{
	public enum TavernGuideQuestStatus
	{
		UNKNOWN,
		ACTIVE,
		COMPLETED,
		LOCKED
	}

	public delegate void QuestSetsChangedDelegate();

	public delegate void InnerQuestStatusChanged(int tavernGuideQuestId, QuestManager.QuestStatus status);

	public const int TAVERN_GUIDE_QUESTS_UNLOCK_LEVEL = 2;

	private readonly HashSet<int> m_tavernGuideQuestInnerQuestIDs = new HashSet<int>();

	private readonly Dictionary<int, AchievementManager.AchievementStatus> m_achievementStatusMap = new Dictionary<int, AchievementManager.AchievementStatus>();

	private readonly Dictionary<int, PlayerQuestState> m_questState = new Dictionary<int, PlayerQuestState>();

	private bool m_hasReceivedInitialQuestUpdate;

	private bool m_hasReceivedInitialClientState;

	private bool m_hasReceivedEventTimings;

	private PlayerQuestStateUpdate m_cachedQuestStateUpdate;

	protected static Blizzard.T5.Core.ILogger m_logger;

	public const int STARTER_TAVERN_GUIDE_QUEST_SET_ID = 1;

	public event QuestSetsChangedDelegate OnQuestSetsChanged;

	public event InnerQuestStatusChanged OnInnerQuestStatusChanged;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		InitializeQuestAssets();
		InitLogger();
		Network network = serviceLocator.Get<Network>();
		network.RegisterNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
		network.RegisterNetHandler(PlayerQuestStateUpdate.PacketID.ID, ReceivePlayerQuestStateUpdateMessage);
		network.RegisterNetHandler(PlayerAchievementStateUpdate.PacketID.ID, ReceiveAchievementUpdateMessage);
		network.RegisterNetHandler(PlayerRewardTrackStateUpdate.PacketID.ID, ReceiveRewardTrackStateUpdateMessage);
		serviceLocator.Get<EventTimingManager>().OnReceivedEventTimingsFromServer += OnReceivedEventTimingsFromServer;
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(Network),
			typeof(EventTimingManager)
		};
	}

	public void Shutdown()
	{
		Network net = Network.Get();
		if (net != null)
		{
			net.RemoveNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
			net.RemoveNetHandler(PlayerQuestStateUpdate.PacketID.ID, ReceivePlayerQuestStateUpdateMessage);
			net.RemoveNetHandler(PlayerAchievementStateUpdate.PacketID.ID, ReceiveAchievementUpdateMessage);
			net.RemoveNetHandler(PlayerRewardTrackStateUpdate.PacketID.ID, ReceiveRewardTrackStateUpdateMessage);
		}
		EventTimingManager evt = EventTimingManager.Get();
		if (evt != null)
		{
			evt.OnReceivedEventTimingsFromServer -= OnReceivedEventTimingsFromServer;
		}
	}

	private void WillReset()
	{
		m_achievementStatusMap.Clear();
		m_questState.Clear();
		m_hasReceivedInitialQuestUpdate = false;
		m_hasReceivedInitialClientState = false;
		m_hasReceivedEventTimings = false;
		m_cachedQuestStateUpdate = null;
	}

	public static TavernGuideManager Get()
	{
		return ServiceManager.Get<TavernGuideManager>();
	}

	public TavernGuideDataModel GetTavernGuideDataModel()
	{
		TavernGuideDataModel datamodel = new TavernGuideDataModel
		{
			TavernGuideQuestSetCategories = new DataModelList<TavernGuideQuestSetCategoryDataModel>()
		};
		if (!m_hasReceivedInitialClientState)
		{
			return datamodel;
		}
		foreach (int category in Enum.GetValues(typeof(TavernGuideQuestSet.TavernGuideCategory)))
		{
			if (category != 0)
			{
				string title = "";
				switch ((TavernGuideQuestSet.TavernGuideCategory)category)
				{
				case TavernGuideQuestSet.TavernGuideCategory.PROGRESSION:
					title = "GLUE_PROGRESSION_APPRENTICE_TAVERN_GUIDE_QUEST_CATEGORY_PROGRESSION";
					break;
				case TavernGuideQuestSet.TavernGuideCategory.GAMEPLAY:
					title = "GLUE_PROGRESSION_APPRENTICE_TAVERN_GUIDE_QUEST_CATEGORY_GAMEPLAY";
					break;
				case TavernGuideQuestSet.TavernGuideCategory.COLLECTION:
					title = "GLUE_PROGRESSION_APPRENTICE_TAVERN_GUIDE_QUEST_CATEGORY_COLLECTION";
					break;
				}
				TavernGuideQuestSetCategoryDataModel model = new TavernGuideQuestSetCategoryDataModel
				{
					Enabled = false,
					Title = GameStrings.Get(title),
					HasNewQuest = false
				};
				datamodel.TavernGuideQuestSetCategories.Add(model);
			}
		}
		HashSet<int> lockedQuestSets = new HashSet<int>();
		foreach (TavernGuideQuestSetDbfRecord questSet in GameDbf.TavernGuideQuestSet.GetRecords())
		{
			if (questSet.NextUnlockedTavernGuideSets.Count == 0 || HasEndAchievementGrantedReward(questSet) || (questSet.ID == 1 && GameUtils.ShouldSkipRailroading()))
			{
				continue;
			}
			foreach (UnlockedTavernGuideSetDbfRecord lockedQuestSet in questSet.NextUnlockedTavernGuideSets)
			{
				lockedQuestSets.Add(lockedQuestSet.UnlockedTavernGuideQuestSet);
			}
		}
		bool questSetsUnlocked = CanShowAllQuestSets();
		foreach (TavernGuideQuestSetDbfRecord questSet2 in GameDbf.TavernGuideQuestSet.GetRecords())
		{
			if (lockedQuestSets.Contains(questSet2.ID))
			{
				continue;
			}
			bool endAchievementCompleted = IsEndAchievementCompleted(questSet2);
			TavernGuideQuestSetDataModel setDatamodel = GetTavernGuideQuestSetDatamodel(questSet2, endAchievementCompleted);
			if (setDatamodel != null)
			{
				int i = (int)(setDatamodel.Category - 1);
				int insertPosition = questSet2.CategoryPosition;
				int questSetListSize = datamodel.TavernGuideQuestSetCategories[i].TavernGuideQuestSets.Count;
				if (insertPosition > questSetListSize)
				{
					insertPosition = questSetListSize;
				}
				datamodel.TavernGuideQuestSetCategories[i].TavernGuideQuestSets.Insert(insertPosition, setDatamodel);
				datamodel.TavernGuideQuestSetCategories[i].Enabled = setDatamodel.Category == TavernGuideQuestSet.TavernGuideCategory.PROGRESSION || questSetsUnlocked;
				if (setDatamodel.HasNewQuest)
				{
					datamodel.TavernGuideQuestSetCategories[i].HasNewQuest = true;
				}
			}
		}
		return datamodel;
	}

	public int GetRewardAchievementFromTavernGuideQuestId(int tavernGuideQuestId)
	{
		foreach (TavernGuideQuestSetDbfRecord set in GameDbf.TavernGuideQuestSet.GetRecords())
		{
			if (set.ID == tavernGuideQuestId)
			{
				return set.CompletionAchievement;
			}
		}
		return 0;
	}

	public bool CanShowAllQuestSets()
	{
		if (GameUtils.ShouldSkipRailroading())
		{
			return true;
		}
		if (GameUtils.HasCompletedApprentice())
		{
			return true;
		}
		RewardTrackManager rewardTrackManager = RewardTrackManager.Get();
		if (rewardTrackManager == null)
		{
			return false;
		}
		if (rewardTrackManager.GetApprenticeTrackLevel() < 2)
		{
			return false;
		}
		return !rewardTrackManager.HasUnclaimedRewardsForApprenticeLevel(2);
	}

	public bool IsTavernGuideActive()
	{
		GameSaveDataManager gsdManager = GameSaveDataManager.Get();
		if (!gsdManager.IsDataReady(GameSaveKeyId.PLAYER_FLAGS) || !QuestManager.Get().HasReceivedQuestStatesFromServer)
		{
			return false;
		}
		gsdManager.GetSubkeyValue(GameSaveKeyId.PLAYER_FLAGS, GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_COMPLETED_TAVERN_GUIDE, out long hasCompletedTavernGuideFlag);
		if (hasCompletedTavernGuideFlag == 1)
		{
			return false;
		}
		gsdManager.GetSubkeyValue(GameSaveKeyId.PLAYER_FLAGS, GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_UNLOCKED_TAVERN_GUIDE, out long hasUnlockedTavernGuideFlag);
		if (hasUnlockedTavernGuideFlag == 1)
		{
			return true;
		}
		return CanShowAllQuestSets();
	}

	public bool IsTavernGuideCompletionAchievement(int achievementId)
	{
		return m_achievementStatusMap.ContainsKey(achievementId);
	}

	public void UpdateJournalMetaForTavernGuide(JournalMetaDataModel journalMetaDatamodel)
	{
		if (journalMetaDatamodel != null)
		{
			GameSaveDataManager gsdManager = GameSaveDataManager.Get();
			if (gsdManager.IsDataReady(GameSaveKeyId.FTUE) && gsdManager.IsDataReady(GameSaveKeyId.PLAYER_FLAGS))
			{
				gsdManager.GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_TAVERN_GUIDE_INTRODUCTION, out long hasSeenTavernGuideFlag);
				gsdManager.GetSubkeyValue(GameSaveKeyId.PLAYER_FLAGS, GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_COMPLETED_TAVERN_GUIDE, out long hasCompletedTavernGuideFlag);
				journalMetaDatamodel.HasPassedTavernGuideButtonIntro = hasSeenTavernGuideFlag == 1 || hasCompletedTavernGuideFlag == 1;
			}
			journalMetaDatamodel.TavernGuideHasUnclaimed = HasUnclaimedSetReward() || HasNewQuest();
		}
	}

	private bool HasNewQuest()
	{
		QuestManager questManager = QuestManager.Get();
		if (questManager == null)
		{
			return false;
		}
		foreach (int questId in m_tavernGuideQuestInnerQuestIDs)
		{
			PlayerQuestState questState = questManager.GetPlayerQuestStateById(questId);
			if (questState.HasStatus && questState.Status == 1)
			{
				return true;
			}
		}
		return false;
	}

	public void AckQuest(TavernGuideQuestDataModel questDatamodel)
	{
		if (questDatamodel != null)
		{
			QuestManager.Get().AckQuest(questDatamodel.Quest.QuestId);
			this.OnInnerQuestStatusChanged?.Invoke(questDatamodel.ID, QuestManager.QuestStatus.ACTIVE);
			this.OnQuestSetsChanged?.Invoke();
		}
	}

	protected static void InitLogger()
	{
		if (m_logger == null)
		{
			LogInfo logInfo = new LogInfo
			{
				m_filePrinting = true,
				m_consolePrinting = true,
				m_screenPrinting = true
			};
			m_logger = LogSystem.Get().CreateLogger("Apprentice", logInfo);
		}
	}

	private void InitializeQuestAssets()
	{
		foreach (TavernGuideQuestDbfRecord quest in GameDbf.TavernGuideQuest.GetRecords())
		{
			m_tavernGuideQuestInnerQuestIDs.Add(quest.QuestRecord.ID);
		}
	}

	private void OnInitialClientState()
	{
		InitialClientState packet = Network.Get().GetInitialClientState();
		if (packet != null && packet.HasGuardianVars)
		{
			m_hasReceivedInitialClientState = true;
			if (m_cachedQuestStateUpdate != null && m_hasReceivedEventTimings)
			{
				ProcessPlayerQuestStateUpdateMessage(m_cachedQuestStateUpdate);
				m_cachedQuestStateUpdate = null;
			}
		}
	}

	private void ReceivePlayerQuestStateUpdateMessage()
	{
		PlayerQuestStateUpdate updateMessage = Network.Get().GetPlayerQuestStateUpdate();
		if (updateMessage != null)
		{
			if (!m_hasReceivedInitialClientState && !m_hasReceivedEventTimings)
			{
				m_cachedQuestStateUpdate = updateMessage;
			}
			else
			{
				ProcessPlayerQuestStateUpdateMessage(updateMessage);
			}
		}
	}

	private void ProcessPlayerQuestStateUpdateMessage(PlayerQuestStateUpdate updateMessage)
	{
		m_hasReceivedInitialQuestUpdate = true;
		foreach (PlayerQuestState newQuestState in updateMessage.Quest)
		{
			if (m_tavernGuideQuestInnerQuestIDs.Contains(newQuestState.QuestId))
			{
				m_questState.TryGetValue(newQuestState.QuestId, out var prevQuestState);
				m_questState[newQuestState.QuestId] = newQuestState;
				HandlePlayerQuestStateChange(prevQuestState, newQuestState);
			}
		}
		UpdateAllAchievementStatusMap();
		this.OnQuestSetsChanged?.Invoke();
	}

	private void OnReceivedEventTimingsFromServer()
	{
		m_hasReceivedEventTimings = true;
		if (m_cachedQuestStateUpdate != null && m_hasReceivedInitialClientState)
		{
			ProcessPlayerQuestStateUpdateMessage(m_cachedQuestStateUpdate);
			m_cachedQuestStateUpdate = null;
		}
	}

	private void ReceiveAchievementUpdateMessage()
	{
		PlayerAchievementStateUpdate updateMessage = Network.Get().GetPlayerAchievementStateUpdate();
		if (updateMessage == null)
		{
			return;
		}
		bool hasUpdatedTavernGuideAchievement = false;
		foreach (PlayerAchievementState newState in updateMessage.Achievement)
		{
			int achievementId = newState.AchievementId;
			if (m_achievementStatusMap.ContainsKey(achievementId))
			{
				m_achievementStatusMap[achievementId] = (AchievementManager.AchievementStatus)newState.Status;
				hasUpdatedTavernGuideAchievement = true;
			}
		}
		if (hasUpdatedTavernGuideAchievement)
		{
			this.OnQuestSetsChanged?.Invoke();
		}
	}

	private void ReceiveRewardTrackStateUpdateMessage()
	{
		if (Network.Get().GetPlayerRewardTrackStateUpdate() != null)
		{
			this.OnQuestSetsChanged?.Invoke();
		}
	}

	private void HandlePlayerQuestStateChange(PlayerQuestState oldState, PlayerQuestState newState)
	{
		if (newState == null)
		{
			return;
		}
		QuestDbfRecord questAsset = GameDbf.Quest.GetRecord(newState.QuestId);
		if (questAsset != null)
		{
			switch ((QuestManager.QuestStatus)newState.Status)
			{
			case QuestManager.QuestStatus.REWARD_GRANTED:
				_ = questAsset.RewardList;
				return;
			case QuestManager.QuestStatus.NEW:
			case QuestManager.QuestStatus.ACTIVE:
			case QuestManager.QuestStatus.COMPLETED:
			case QuestManager.QuestStatus.REWARD_ACKED:
			case QuestManager.QuestStatus.REROLLED:
			case QuestManager.QuestStatus.RESET:
			case QuestManager.QuestStatus.ABANDONED:
			case QuestManager.QuestStatus.EXPIRED:
				return;
			}
			Debug.LogWarningFormat("TavernGuideManager: unknown status {0} for quest id {1}", newState.Status, newState.QuestId);
		}
	}

	private TavernGuideQuestSetDataModel GetTavernGuideQuestSetDatamodel(TavernGuideQuestSetDbfRecord record, bool endAchievementCompleted)
	{
		if (record == null)
		{
			return null;
		}
		TavernGuideQuestSetDataModel questSet = InitializeTavernGuideQuestSetDataModel(record);
		if (!endAchievementCompleted && !CanShowAllQuestSets() && record.CategoryPosition != 0)
		{
			return null;
		}
		foreach (TavernGuideQuestDbfRecord quest in record.TavernGuideQuests)
		{
			QuestDbfRecord innerQuestRecord = quest.QuestRecord;
			m_questState.TryGetValue(innerQuestRecord.ID, out var questState);
			TavernGuideQuestStatus questStatus = GetTavernGuideQuestStatus(quest, endAchievementCompleted, questState);
			TavernGuideQuestDataModel data = new TavernGuideQuestDataModel
			{
				ID = quest.ID,
				Title = innerQuestRecord.Name,
				SelectedDescription = quest.SelectedDescription,
				RecommendedClasses = GetRecommendedClassesName(quest),
				Status = questStatus,
				ShouldShowProgressAmount = quest.ShowQuota,
				UnlockRequirementsDescription = quest.UnlockRequirementDescription,
				Quest = QuestManager.Get().CreateQuestDataModelById(innerQuestRecord.ID)
			};
			if (data.Status == TavernGuideQuestStatus.COMPLETED && data.Quest.Status == QuestManager.QuestStatus.UNKNOWN)
			{
				data.Quest.Status = QuestManager.QuestStatus.REWARD_ACKED;
			}
			if (data.Quest.Status == QuestManager.QuestStatus.NEW)
			{
				questSet.HasNewQuest = true;
			}
			questSet.Quests.Add(data);
		}
		return questSet;
	}

	private static string GetRecommendedClassesName(TavernGuideQuestDbfRecord quest)
	{
		List<TAG_CLASS> classes = new List<TAG_CLASS>();
		foreach (TavernGuideQuestRecommendedClassesDbfRecord record in quest.RecommendedClasses)
		{
			classes.Add((TAG_CLASS)record.Class);
		}
		return GameStrings.GetClassesName(classes);
	}

	private void UpdateAllAchievementStatusMap()
	{
		foreach (TavernGuideQuestSetDbfRecord questSet in GameDbf.TavernGuideQuestSet.GetRecords())
		{
			int completionAchievementId = questSet.CompletionAchievement;
			if (completionAchievementId == 0)
			{
				m_logger.Log(Blizzard.T5.Core.LogLevel.Warning, $"TavernGuideQuest {questSet.ID} is missing a completion achievement");
				continue;
			}
			AchievementManager achievementManager = AchievementManager.Get();
			AchievementDataModel achievementModel = achievementManager.GetAchievementDataModel(completionAchievementId);
			if (achievementModel == null)
			{
				m_logger.Log(Blizzard.T5.Core.LogLevel.Warning, $"Could not retrieve a datamodel for TavernGuideQuest {questSet.ID}'s achievement {completionAchievementId}");
				continue;
			}
			achievementManager.LoadReward(achievementModel);
			AchievementManager.AchievementStatus achievementStatus = achievementModel.Status;
			m_achievementStatusMap[completionAchievementId] = achievementStatus;
		}
	}

	private bool IsEndAchievementCompleted(TavernGuideQuestSetDbfRecord record)
	{
		if (!m_achievementStatusMap.TryGetValue(record.CompletionAchievement, out var achievementStatus))
		{
			m_logger.Log(Blizzard.T5.Core.LogLevel.Warning, $"Missing Achievement Status from map {record.CompletionAchievement}");
		}
		return IsACompletedAchievementStatus(achievementStatus);
	}

	private bool HasEndAchievementGrantedReward(TavernGuideQuestSetDbfRecord record)
	{
		if (!m_achievementStatusMap.TryGetValue(record.CompletionAchievement, out var achievementStatus))
		{
			m_logger.Log(Blizzard.T5.Core.LogLevel.Warning, $"Missing Achievement Status from map {record.CompletionAchievement}");
		}
		return HasAchievementGrantedReward(achievementStatus);
	}

	private static bool IsACompletedAchievementStatus(AchievementManager.AchievementStatus status)
	{
		if (status != AchievementManager.AchievementStatus.COMPLETED && status != AchievementManager.AchievementStatus.REWARD_GRANTED)
		{
			return status == AchievementManager.AchievementStatus.REWARD_ACKED;
		}
		return true;
	}

	private static bool HasAchievementGrantedReward(AchievementManager.AchievementStatus status)
	{
		if (status != AchievementManager.AchievementStatus.REWARD_GRANTED)
		{
			return status == AchievementManager.AchievementStatus.REWARD_ACKED;
		}
		return true;
	}

	private static bool HasPendingAchievementReward(AchievementManager.AchievementStatus status)
	{
		return status == AchievementManager.AchievementStatus.COMPLETED;
	}

	private static TavernGuideQuestSetDataModel InitializeTavernGuideQuestSetDataModel(TavernGuideQuestSetDbfRecord record)
	{
		return new TavernGuideQuestSetDataModel
		{
			Title = record.Title,
			Description = record.Description,
			QuestLayoutType = record.QuestDisplayType,
			Category = record.Category,
			ID = record.ID,
			Quests = new DataModelList<TavernGuideQuestDataModel>(),
			CompletionAchievement = AchievementManager.Get().GetAchievementDataModel(record.CompletionAchievement)
		};
	}

	private TavernGuideQuestStatus GetTavernGuideQuestStatus(TavernGuideQuestDbfRecord quest, bool endAchievementCompleted, PlayerQuestState questState)
	{
		if (!m_hasReceivedInitialQuestUpdate)
		{
			return TavernGuideQuestStatus.UNKNOWN;
		}
		if (endAchievementCompleted)
		{
			return TavernGuideQuestStatus.COMPLETED;
		}
		if (RewardTrackManager.Get().IsApprenticeTrackReady() && quest.UnlockRequirementLevel > RewardTrackManager.Get().GetApprenticeTrackLevel())
		{
			return TavernGuideQuestStatus.LOCKED;
		}
		if (quest.UnlockRequirementMode != 0 && !GameModeUtils.HasUnlockedMode((Global.UnlockableGameMode)quest.UnlockRequirementMode))
		{
			return TavernGuideQuestStatus.LOCKED;
		}
		if (questState == null)
		{
			return TavernGuideQuestStatus.COMPLETED;
		}
		QuestManager.QuestStatus questStatus = (QuestManager.QuestStatus)questState.Status;
		if (questStatus == QuestManager.QuestStatus.COMPLETED || questStatus == QuestManager.QuestStatus.REWARD_GRANTED || questStatus == QuestManager.QuestStatus.REWARD_ACKED || questStatus == QuestManager.QuestStatus.UNKNOWN)
		{
			return TavernGuideQuestStatus.COMPLETED;
		}
		return TavernGuideQuestStatus.ACTIVE;
	}

	public bool HasUnclaimedSetReward()
	{
		foreach (KeyValuePair<int, AchievementManager.AchievementStatus> item in m_achievementStatusMap)
		{
			if (HasPendingAchievementReward(item.Value))
			{
				return true;
			}
		}
		return false;
	}
}
