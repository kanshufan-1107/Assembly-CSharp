using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusUtil;
using UnityEngine;

namespace Hearthstone.Progression;

public class QuestManager : IService
{
	public enum QuestStatus
	{
		UNKNOWN,
		NEW,
		ACTIVE,
		COMPLETED,
		REWARD_GRANTED,
		REWARD_ACKED,
		REROLLED,
		RESET,
		ABANDONED,
		EXPIRED
	}

	public enum QuestDisplayMode
	{
		Default,
		Notification,
		Inspection,
		NextQuestTime
	}

	public enum QuestChangeState
	{
		NONE,
		BUFF,
		NERF
	}

	public delegate void OnQuestStateUpdateHandler();

	public delegate void OnQuestRerolledHandler(int rerolledQuestId, int grantedQuestId, bool success);

	public delegate void OnQuestRerollCountChangedHandler(int questPoolId, int rerollCount);

	public delegate void OnQuestProgressHandler(QuestDataModel questDataModel);

	public static readonly AssetReference QUEST_NOTIFICATION_PREFAB = new AssetReference("QuestNotificationPopup.prefab:23a71f92e200b3243b16be8e4d42c0c8");

	private Dictionary<int, PlayerQuestState> m_questState = new Dictionary<int, PlayerQuestState>();

	private Dictionary<int, PlayerQuestPoolState> m_questPoolState = new Dictionary<int, PlayerQuestPoolState>();

	private HashSet<QuestPool.QuestPoolType> m_questPoolTypesToShow = new HashSet<QuestPool.QuestPoolType>();

	private HashSet<QuestPool.QuestPoolType> m_battlegroundsQuestPoolTypesToShow = new HashSet<QuestPool.QuestPoolType>();

	private bool m_showQuestPoolTypesFromLogin;

	private Dictionary<int, DateTime> m_questPoolNextQuestTime = new Dictionary<int, DateTime>();

	private float m_checkForNewQuestsIntervalSecs = 2f;

	private float m_checkForNewQuestsIntervalJitterSecs;

	private bool m_hasReceivedInitialClientState;

	private bool m_hasReceivedEventTimings;

	private RepeatingScheduledCallback m_questExpiryChecker = new RepeatingScheduledCallback();

	private bool m_questExpirationEnabled;

	private float m_questExpirationClientRetrySecs = 2f;

	private float m_questExpirationClientJitterSecs = 1f;

	private readonly RewardPresenter m_rewardPresenter = new RewardPresenter();

	private HashSet<int> m_chainQuestIds = new HashSet<int>();

	private Dictionary<int, int> m_chainQuestIdToPrevious = new Dictionary<int, int>();

	private HashSet<int> m_proxyLegacyAchieveIds = new HashSet<int>();

	public bool HasReceivedQuestStatesFromServer { get; private set; }

	public event OnQuestStateUpdateHandler OnQuestStateUpdate;

	public event OnQuestRerolledHandler OnQuestRerolled;

	public event OnQuestRerollCountChangedHandler OnQuestRerollCountChanged;

	public event OnQuestProgressHandler OnQuestProgress;

	public RewardPresenter GetRewardPresenter()
	{
		return m_rewardPresenter;
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		Network network = serviceLocator.Get<Network>();
		network.RegisterNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
		network.RegisterNetHandler(PlayerQuestStateUpdate.PacketID.ID, ReceivePlayerQuestStateUpdateMessage);
		network.RegisterNetHandler(PlayerQuestPoolStateUpdate.PacketID.ID, ReceivePlayerQuestPoolStateUpdateMessage);
		network.RegisterNetHandler(RerollQuestResponse.PacketID.ID, ReceiveRerollQuestResponseMessage);
		serviceLocator.Get<EventTimingManager>().OnReceivedEventTimingsFromServer += OnReceivedEventTimingsFromServer;
		InitializeQuestAssets();
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
	}

	private void WillReset()
	{
		m_showQuestPoolTypesFromLogin = false;
		m_questState.Clear();
		m_questPoolState.Clear();
		m_questPoolTypesToShow.Clear();
		m_battlegroundsQuestPoolTypesToShow.Clear();
		m_rewardPresenter.Clear();
		m_questPoolNextQuestTime.Clear();
		m_checkForNewQuestsIntervalSecs = ResetCheckForNewQuestsInterval();
		Processor.CancelScheduledCallback(CheckForNewQuestsScheduledCallback);
		m_hasReceivedInitialClientState = false;
		m_hasReceivedEventTimings = false;
		m_questExpiryChecker.Stop();
	}

	public static QuestManager Get()
	{
		return ServiceManager.Get<QuestManager>();
	}

	public bool HasQuestNotificationToShow()
	{
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		if (mode == SceneMgr.Mode.BACON || mode == SceneMgr.Mode.BACON_COLLECTION)
		{
			return m_battlegroundsQuestPoolTypesToShow.Count > 0;
		}
		return m_questPoolTypesToShow.Count > 0;
	}

	public QuestDataModel CreateQuestDataModelById(int questId)
	{
		if (!m_questState.TryGetValue(questId, out var questState))
		{
			questState = new PlayerQuestState
			{
				QuestId = questId
			};
		}
		return CreateQuestDataModel(questState);
	}

	public RewardScrollDataModel CreateRewardScrollDataModelByQuestId(int questId, List<RewardItemOutput> rewardItemOutput = null)
	{
		QuestDbfRecord questRecord = GameDbf.Quest.GetRecord(questId);
		if (questRecord == null)
		{
			return new RewardScrollDataModel();
		}
		RewardScrollDataModel rewardScrollDataModel = new RewardScrollDataModel();
		DbfLocValue name = questRecord.Name;
		rewardScrollDataModel.DisplayName = ((name != null) ? ((string)name) : string.Empty);
		rewardScrollDataModel.Description = ProgressUtils.FormatDescription(questRecord.Description, questRecord.Quota);
		rewardScrollDataModel.RewardList = RewardUtils.CreateRewardListDataModelFromRewardListId(questRecord.RewardList, 0, rewardItemOutput);
		return rewardScrollDataModel;
	}

	public QuestListDataModel CreateActiveQuestsDataModel(QuestPool.QuestPoolType questPoolType, QuestPool.RewardTrackType rewardTrackType, bool appendTimeUntilNextQuest)
	{
		QuestListDataModel questListDataModel = new QuestListDataModel();
		List<int> questPoolIds = null;
		if (questPoolType == QuestPool.QuestPoolType.NONE)
		{
			questPoolIds = new List<int>();
			questPoolIds.Add(0);
		}
		else if (rewardTrackType == QuestPool.RewardTrackType.NONE)
		{
			questPoolIds = (from r in GameDbf.QuestPool.GetRecords()
				where r.QuestPoolType == questPoolType
				select r.ID into x
				orderby x descending
				select x).ToList();
		}
		else if (rewardTrackType == QuestPool.RewardTrackType.EVENT)
		{
			RewardTrack eventTrack = RewardTrackManager.Get().GetCurrentEventRewardTrack();
			if (eventTrack == null)
			{
				return questListDataModel;
			}
			questPoolIds = (from r in GameDbf.QuestPool.GetRecords()
				where r.QuestPoolType == questPoolType && r.RewardTrackType == rewardTrackType && eventTrack.RewardTrackAsset.OverrideQuestPools.Exists((OverrideQuestPoolIdListDbfRecord questPoolOverride) => questPoolOverride.QuestPoolId == r.ID)
				select r.ID into x
				orderby x descending
				select x).ToList();
		}
		else
		{
			questPoolIds = (from r in GameDbf.QuestPool.GetRecords()
				where r.QuestPoolType == questPoolType && r.RewardTrackType == rewardTrackType
				select r.ID into x
				orderby x descending
				select x).ToList();
		}
		int numBankedQuests = 0;
		foreach (int questPoolId in questPoolIds)
		{
			int numActiveQuests = 0;
			foreach (PlayerQuestState questState in from q in m_questState.Values.Where(delegate(PlayerQuestState q)
				{
					QuestDbfRecord record = GameDbf.Quest.GetRecord(q.QuestId);
					return record != null && record.QuestPool == questPoolId && IsQuestActive(q);
				})
				orderby q.QuestId descending
				select q)
			{
				questListDataModel.Quests.Add(CreateQuestDataModel(questState));
				numActiveQuests++;
			}
			if (appendTimeUntilNextQuest)
			{
				bool poolWillRefresh = CanBeGrantedPoolQuests() || rewardTrackType == QuestPool.RewardTrackType.BATTLEGROUNDS;
				if (poolWillRefresh && questPoolType == QuestPool.QuestPoolType.EVENT)
				{
					RewardTrack eventTrack2 = RewardTrackManager.Get().GetCurrentEventRewardTrack();
					if (eventTrack2 == null || !eventTrack2.IsValid)
					{
						Debug.LogError("QuestManager: attempting to append time to next quest for an invalid or inactive event.");
					}
					else
					{
						EventTimingManager eventMgr = EventTimingManager.Get();
						EventTimingType eventType = eventTrack2.RewardTrackAsset.Event;
						if (eventType == EventTimingType.UNKNOWN)
						{
							Debug.LogError($"QuestManager: missing SpecialEventType for event {eventTrack2.RewardTrackAsset.Event}.");
						}
						else
						{
							TimeSpan timeLeftForEvent = eventMgr.GetTimeLeftForEvent(eventType);
							TimeSpan timeToNextQuest = GetTimeUntilNextQuest(questPoolId);
							poolWillRefresh = timeLeftForEvent > timeToNextQuest;
						}
					}
				}
				if (poolWillRefresh)
				{
					QuestPoolDbfRecord questPoolAsset = GameDbf.QuestPool.GetRecord(questPoolId);
					if (questPoolAsset != null)
					{
						if (!IsQuestPoolGrantingFromFallback(questPoolAsset))
						{
							int numFreeSlots = Mathf.Max(questPoolAsset.MaxQuestsActive - numActiveQuests, 0);
							int numQuestsToBeGranted = Mathf.Min(questPoolAsset.NumQuestsGranted, numFreeSlots);
							if (numQuestsToBeGranted > 0)
							{
								QuestDbfRecord nextChainQuest = GetNextChainQuestIfExists(questPoolAsset);
								if (nextChainQuest != null)
								{
									QuestDataModel upcomingChainQuestDM = CreateQuestDataModelById(nextChainQuest.ID);
									upcomingChainQuestDM.TimeUntilNextQuest = GetTimeUntilNextQuestString(questPoolAsset.ID);
									questListDataModel.Quests.Add(upcomingChainQuestDM);
									numQuestsToBeGranted--;
								}
								if (numQuestsToBeGranted > 0)
								{
									foreach (QuestDataModel dm in Enumerable.Repeat(CreateNextQuestTimeDataModel(questPoolAsset), numQuestsToBeGranted))
									{
										questListDataModel.Quests.Add(dm);
									}
								}
							}
						}
						else
						{
							QuestPoolDbfRecord fallbackQuestPoolAsset = questPoolAsset.FallbackQuestPoolRecord;
							int totalMaxQuests = fallbackQuestPoolAsset.MaxQuestsActive + questPoolAsset.MaxQuestsActive;
							int totalNumActiveQuests = m_questState.Values.Where((PlayerQuestState q) => GameDbf.Quest.GetRecord(q.QuestId)?.QuestPool == fallbackQuestPoolAsset.ID && IsQuestActive(q)).Count();
							if (totalNumActiveQuests < totalMaxQuests)
							{
								foreach (QuestDataModel dm2 in Enumerable.Repeat(CreateNextQuestTimeDataModel(questPoolAsset), totalMaxQuests - totalNumActiveQuests))
								{
									questListDataModel.Quests.Add(dm2);
								}
							}
						}
					}
				}
			}
			numBankedQuests += GetBankedQuestCountForPool(questPoolId);
		}
		questListDataModel.BankedQuestCountMessage = GameStrings.Format("GLUE_PROGRESSION_QUESTS_BANKED", numBankedQuests);
		return questListDataModel;
	}

	public bool ShowQuestNotification(Action callback)
	{
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		List<PlayerQuestState> playerQuestStateList = new List<PlayerQuestState>();
		if (mode == SceneMgr.Mode.BACON || mode == SceneMgr.Mode.BACON_COLLECTION)
		{
			if (!m_battlegroundsQuestPoolTypesToShow.Contains(QuestPool.QuestPoolType.WEEKLY))
			{
				return false;
			}
			m_battlegroundsQuestPoolTypesToShow.Remove(QuestPool.QuestPoolType.WEEKLY);
			playerQuestStateList = GetActiveQuestStatesForPool(QuestPool.QuestPoolType.WEEKLY, Global.RewardTrackType.BATTLEGROUNDS);
		}
		else if (m_questPoolTypesToShow.Contains(QuestPool.QuestPoolType.NONE) || m_questPoolTypesToShow.Contains(QuestPool.QuestPoolType.DAILY))
		{
			m_questPoolTypesToShow.Remove(QuestPool.QuestPoolType.NONE);
			m_questPoolTypesToShow.Remove(QuestPool.QuestPoolType.DAILY);
			playerQuestStateList = GetActiveQuestStatesForPool(QuestPool.QuestPoolType.NONE, Global.RewardTrackType.NONE);
			playerQuestStateList.AddRange(GetActiveQuestStatesForPool(QuestPool.QuestPoolType.DAILY, Global.RewardTrackType.GLOBAL));
		}
		else if (m_questPoolTypesToShow.Contains(QuestPool.QuestPoolType.WEEKLY))
		{
			m_questPoolTypesToShow.Remove(QuestPool.QuestPoolType.WEEKLY);
			playerQuestStateList = GetActiveQuestStatesForPool(QuestPool.QuestPoolType.WEEKLY, Global.RewardTrackType.GLOBAL);
		}
		else
		{
			if (!m_questPoolTypesToShow.Contains(QuestPool.QuestPoolType.EVENT))
			{
				return false;
			}
			m_questPoolTypesToShow.Remove(QuestPool.QuestPoolType.EVENT);
			RewardTrack eventTrack = RewardTrackManager.Get().GetCurrentEventRewardTrack();
			if (eventTrack != null && eventTrack.IsValid)
			{
				playerQuestStateList = GetActiveQuestStatesForPool(QuestPool.QuestPoolType.EVENT, eventTrack.TrackDataModel.RewardTrackType);
			}
		}
		if (playerQuestStateList.Count == 0)
		{
			return false;
		}
		QuestListDataModel questListDataModel = new QuestListDataModel
		{
			Quests = (from state in playerQuestStateList
				orderby state.QuestId descending
				select CreateQuestDataModel(state) into dataModel
				orderby dataModel.PoolType
				select dataModel).ToDataModelList()
		};
		bool showIKS = false;
		if (m_questPoolTypesToShow.Count == 0 && mode != SceneMgr.Mode.BACON && mode != SceneMgr.Mode.BACON_COLLECTION)
		{
			showIKS = m_showQuestPoolTypesFromLogin;
			m_showQuestPoolTypesFromLogin = false;
		}
		Widget widget = WidgetInstance.Create(QUEST_NOTIFICATION_PREFAB);
		SpecialEventDataModel specialEventDataModel = SpecialEventManager.Get()?.GetEventDataModelForCurrentEvent();
		if (specialEventDataModel != null)
		{
			widget.BindDataModel(specialEventDataModel);
		}
		widget.RegisterReadyListener(delegate
		{
			QuestNotificationPopup componentInChildren = widget.GetComponentInChildren<QuestNotificationPopup>();
			componentInChildren.Initialize(questListDataModel, callback, showIKS);
			componentInChildren.Show();
		});
		return true;
	}

	public bool AckQuest(int questId)
	{
		if (!m_questState.TryGetValue(questId, out var questState))
		{
			return false;
		}
		if (!NeedsAck(questState))
		{
			return false;
		}
		switch ((QuestStatus)questState.Status)
		{
		case QuestStatus.NEW:
			questState.Status = 2;
			break;
		case QuestStatus.REWARD_GRANTED:
			questState.Status = 5;
			break;
		}
		Network.Get().AckQuest(questId);
		return false;
	}

	public bool RerollQuest(int questId)
	{
		if (!m_questState.TryGetValue(questId, out var questState))
		{
			return false;
		}
		if (!IsQuestActive(questState))
		{
			return false;
		}
		QuestDbfRecord questRecord = GameDbf.Quest.GetRecord(questState.QuestId);
		if (questRecord == null)
		{
			return false;
		}
		if (GetQuestPoolRerollCount(questRecord.QuestPool) <= 0)
		{
			return false;
		}
		Network.Get().RerollQuest(questId);
		return true;
	}

	public bool AbandonQuest(int questId)
	{
		if (!m_questState.TryGetValue(questId, out var questState))
		{
			return false;
		}
		if (!IsQuestActive(questState))
		{
			return false;
		}
		Network.Get().AbandonQuest(questId);
		return true;
	}

	public bool ShowNextReward(Action callback)
	{
		return m_rewardPresenter.ShowNextReward(callback);
	}

	public bool HasReward()
	{
		return m_rewardPresenter.HasReward();
	}

	public bool CanBeGrantedPoolQuests()
	{
		return !TavernGuideManager.Get().IsTavernGuideActive();
	}

	public int GetBankedQuestCountForPool(int questPoolId)
	{
		if (m_questPoolState.TryGetValue(questPoolId, out var questPoolState))
		{
			return questPoolState.BankedQuestCount;
		}
		return 0;
	}

	public static Global.RewardTrackType GetRewardTrackType(int questAssetId)
	{
		return (Global.RewardTrackType)(GameDbf.Quest.GetRecord(questAssetId)?.RewardTrackType ?? Quest.RewardTrackType.NONE);
	}

	public bool IsChainQuest(int questAssetId)
	{
		return m_chainQuestIds.Contains(questAssetId);
	}

	public bool IsProxyLegacyAchieve(int legacyAchieveId)
	{
		return m_proxyLegacyAchieveIds.Contains(legacyAchieveId);
	}

	public static int SortChainQuestsToFront(QuestDataModel q1, QuestDataModel q2)
	{
		if (q1.IsChainQuest)
		{
			return -1;
		}
		if (q2.IsChainQuest)
		{
			return 1;
		}
		return 0;
	}

	public PlayerQuestState GetPlayerQuestStateById(int questId)
	{
		if (!m_questState.TryGetValue(questId, out var questState))
		{
			return new PlayerQuestState
			{
				QuestId = questId
			};
		}
		return questState;
	}

	private QuestDataModel CreateQuestDataModel(PlayerQuestState questState)
	{
		if (questState == null || RewardTrackManager.Get() == null)
		{
			return new QuestDataModel();
		}
		QuestDbfRecord questRecord = GameDbf.Quest.GetRecord(questState.QuestId);
		if (questRecord == null)
		{
			return new QuestDataModel();
		}
		DataModelList<string> iconList = (from s in questRecord.Icon?.Split(',')
			select s.Trim() into s
			where s != ""
			select s).ToDataModelList() ?? new DataModelList<string>();
		Global.RewardTrackType rewardTrackType = (Global.RewardTrackType)questRecord.RewardTrackType;
		RewardTrack rewardTrack = null;
		if (rewardTrackType != 0)
		{
			rewardTrack = RewardTrackManager.Get().GetRewardTrack(rewardTrackType);
			if (rewardTrack == null || !rewardTrack.IsValid)
			{
				rewardTrack = RewardTrackManager.Get().GetRewardTrackWithOverrideType(rewardTrackType);
				if (rewardTrack != null)
				{
					rewardTrackType = rewardTrack.TrackDataModel.RewardTrackType;
				}
			}
		}
		int rewardTrackXp = rewardTrack?.ApplyXpBonusPercent(questRecord.RewardTrackXp) ?? questRecord.RewardTrackXp;
		bool isChainQuest = IsChainQuest(questRecord.ID);
		int rerollCount = ((!isChainQuest) ? GetQuestPoolRerollCount(questRecord.QuestPool) : 0);
		QuestDataModel questDataModel = new QuestDataModel
		{
			QuestId = questState.QuestId,
			PoolId = GetQuestPoolId(questRecord),
			PoolType = GetQuestPoolType(questRecord),
			DisplayMode = QuestDisplayMode.Default,
			Name = questRecord.Name?.GetString(),
			Description = ProgressUtils.FormatDescription(questRecord.Description, questRecord.Quota),
			Icon = iconList,
			Progress = questState.Progress,
			Quota = questRecord.Quota,
			RerollCount = rerollCount,
			Rewards = RewardUtils.CreateRewardListDataModelFromRewardListId(questRecord.RewardList),
			RewardTrackXp = rewardTrackXp,
			ProgressMessage = ProgressUtils.FormatProgressMessage(questState.Progress, questRecord.Quota),
			Status = (QuestStatus)questState.Status,
			Abandonable = questRecord.CanAbandon,
			NextInChain = questRecord.NextInChain,
			IsChainQuest = isChainQuest,
			TimeUntilExpiration = GetTimeUntilQuestExpiresString(questState),
			RewardTrackType = rewardTrackType,
			DeepLink = questRecord.DeepLink
		};
		if (questRecord.ToastDescription == null)
		{
			questDataModel.ToastDescription = questDataModel.Description;
		}
		else
		{
			questDataModel.ToastDescription = ProgressUtils.FormatDescription(questRecord.ToastDescription, questRecord.Quota);
		}
		return questDataModel;
	}

	private QuestDataModel CreateNextQuestTimeDataModel(QuestPoolDbfRecord questPoolRecord)
	{
		string timeTilNextKey = "GLOBAL_PROGRESSION_QUEST_TIME_UNTIL_NEXT";
		if (questPoolRecord.RewardTrackType == QuestPool.RewardTrackType.BATTLEGROUNDS)
		{
			timeTilNextKey = "BATTLEGROUNDS_PROGRESSION_QUEST_TIME_UNTIL_NEXT";
		}
		QuestDataModel questDataModel = new QuestDataModel();
		questDataModel.DisplayMode = QuestDisplayMode.NextQuestTime;
		questDataModel.PoolType = questPoolRecord.QuestPoolType;
		questDataModel.TimeUntilNextQuest = GameStrings.Format(timeTilNextKey, GetTimeUntilNextQuestString(questPoolRecord.ID));
		return questDataModel;
	}

	private bool NeedsAck(PlayerQuestState questState)
	{
		QuestStatus status = (QuestStatus)questState.Status;
		if (status == QuestStatus.NEW || status == QuestStatus.REWARD_GRANTED)
		{
			return true;
		}
		return false;
	}

	public static bool IsQuestActive(PlayerQuestState questState)
	{
		QuestStatus status = (QuestStatus)questState.Status;
		if ((uint)(status - 1) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsQuestComplete(PlayerQuestState questState)
	{
		QuestStatus status = (QuestStatus)questState.Status;
		if ((uint)(status - 3) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static QuestChangeState TranslateQuestChangeState(QuestChange.ChangeType changeType)
	{
		return changeType switch
		{
			QuestChange.ChangeType.BUFF => QuestChangeState.BUFF, 
			QuestChange.ChangeType.NERF => QuestChangeState.NERF, 
			_ => QuestChangeState.NONE, 
		};
	}

	private void InitializeQuestAssets()
	{
		foreach (QuestDbfRecord questAsset in GameDbf.Quest.GetRecords())
		{
			if (questAsset.NextInChain != 0)
			{
				m_chainQuestIds.Add(questAsset.ID);
				m_chainQuestIds.Add(questAsset.NextInChain);
				m_chainQuestIdToPrevious.Add(questAsset.NextInChain, questAsset.ID);
			}
			if (questAsset.ProxyForLegacyId != 0)
			{
				m_proxyLegacyAchieveIds.Add(questAsset.ProxyForLegacyId);
			}
		}
	}

	private void OnInitialClientState()
	{
		InitialClientState packet = Network.Get().GetInitialClientState();
		if (packet != null && packet.HasGuardianVars)
		{
			m_hasReceivedInitialClientState = true;
			m_checkForNewQuestsIntervalJitterSecs = packet.GuardianVars.CheckForNewQuestsIntervalJitterSecs;
			m_questExpirationEnabled = packet.GuardianVars.QuestExpirationEnabled;
			m_questExpirationClientRetrySecs = packet.GuardianVars.QuestExpirationClientRetrySecs;
			m_questExpirationClientJitterSecs = packet.GuardianVars.QuestExpirationClientJitterSecs;
			HandleExpiredQuests();
			ScheduleCheckForNewQuests(null);
		}
	}

	private void OnReceivedEventTimingsFromServer()
	{
		m_hasReceivedEventTimings = true;
		HandleExpiredQuests();
	}

	private void ReceivePlayerQuestStateUpdateMessage()
	{
		PlayerQuestStateUpdate updateMessage = Network.Get().GetPlayerQuestStateUpdate();
		if (updateMessage == null)
		{
			return;
		}
		foreach (PlayerQuestState newQuestState in updateMessage.Quest)
		{
			m_questState.TryGetValue(newQuestState.QuestId, out var prevQuestState);
			m_questState[newQuestState.QuestId] = newQuestState;
			HandlePlayerQuestStateChange(prevQuestState, newQuestState);
		}
		if (updateMessage.ShowQuestNotificationForPoolType.Count > 0 && SceneMgr.Get().GetMode() == SceneMgr.Mode.LOGIN)
		{
			m_showQuestPoolTypesFromLogin = true;
		}
		foreach (int poolType in updateMessage.ShowQuestNotificationForPoolType)
		{
			QuestPool.QuestPoolType questPoolType = (QuestPool.QuestPoolType)poolType;
			if (questPoolType == QuestPool.QuestPoolType.NONE || questPoolType == QuestPool.QuestPoolType.DAILY || questPoolType == QuestPool.QuestPoolType.WEEKLY)
			{
				m_questPoolTypesToShow.Add(questPoolType);
				if (questPoolType == QuestPool.QuestPoolType.WEEKLY)
				{
					m_battlegroundsQuestPoolTypesToShow.Add((QuestPool.QuestPoolType)poolType);
				}
			}
		}
		HandleExpiredQuests();
		HasReceivedQuestStatesFromServer = true;
		this.OnQuestStateUpdate?.Invoke();
	}

	private void HandlePlayerQuestStateChange(PlayerQuestState oldState, PlayerQuestState newState)
	{
		if (newState == null)
		{
			return;
		}
		QuestDbfRecord questAsset = GameDbf.Quest.GetRecord(newState.QuestId);
		if (questAsset == null)
		{
			return;
		}
		switch ((QuestStatus)newState.Status)
		{
		case QuestStatus.NEW:
		{
			QuestDbfRecord quest = GameDbf.Quest.GetRecord(newState.QuestId);
			QuestPoolDbfRecord pool = GameDbf.QuestPool.GetRecord(quest.QuestPool);
			QuestPool.QuestPoolType questPoolType = GetQuestPoolType(questAsset);
			if (pool != null && pool.RewardTrackType == QuestPool.RewardTrackType.BATTLEGROUNDS)
			{
				m_battlegroundsQuestPoolTypesToShow.Add(questPoolType);
			}
			else if (questPoolType != QuestPool.QuestPoolType.EVENT || !JournalPopup.s_isShowing)
			{
				m_questPoolTypesToShow.Add(GetQuestPoolType(questAsset));
			}
			break;
		}
		case QuestStatus.ACTIVE:
		case QuestStatus.COMPLETED:
			if (oldState != null && oldState.Progress < newState.Progress)
			{
				this.OnQuestProgress?.Invoke(CreateQuestDataModel(newState));
			}
			break;
		case QuestStatus.REWARD_GRANTED:
			if (questAsset.RewardList == 0)
			{
				AckQuest(newState.QuestId);
				break;
			}
			m_rewardPresenter.EnqueueReward(CreateRewardScrollDataModelByQuestId(newState.QuestId, newState.RewardItemOutput), delegate
			{
				AckQuest(newState.QuestId);
			});
			break;
		default:
			Debug.LogWarningFormat("QuestManager: unknown status {0} for quest id {1}", newState.Status, newState.QuestId);
			break;
		case QuestStatus.REWARD_ACKED:
		case QuestStatus.REROLLED:
		case QuestStatus.RESET:
		case QuestStatus.ABANDONED:
		case QuestStatus.EXPIRED:
			break;
		}
	}

	private void ReceivePlayerQuestPoolStateUpdateMessage()
	{
		PlayerQuestPoolStateUpdate updateMessage = Network.Get().GetPlayerQuestPoolStateUpdate();
		if (updateMessage == null)
		{
			return;
		}
		foreach (PlayerQuestPoolState questPoolState in updateMessage.QuestPool)
		{
			m_questPoolState[questPoolState.QuestPoolId] = questPoolState;
			this.OnQuestRerollCountChanged?.Invoke(questPoolState.QuestPoolId, questPoolState.RerollAvailableCount);
			DateTime nextQuestTime = DateTime.Now.AddSeconds(questPoolState.SecondsUntilNextGrant);
			m_questPoolNextQuestTime[questPoolState.QuestPoolId] = nextQuestTime;
		}
		m_checkForNewQuestsIntervalSecs = ResetCheckForNewQuestsInterval();
		ScheduleCheckForNewQuests(null);
	}

	private void ReceiveRerollQuestResponseMessage()
	{
		RerollQuestResponse response = Network.Get().GetRerollQuestResponse();
		if (response != null)
		{
			this.OnQuestRerolled?.Invoke(response.RerolledQuestId, response.GrantedQuestId, response.Success);
		}
	}

	private int GetQuestPoolId(QuestDbfRecord questAsset)
	{
		return (questAsset?.QuestPoolRecord?.ID).GetValueOrDefault();
	}

	private QuestPool.QuestPoolType GetQuestPoolType(QuestDbfRecord questAsset)
	{
		return (questAsset?.QuestPoolRecord?.QuestPoolType).GetValueOrDefault();
	}

	private List<PlayerQuestState> GetActiveQuestStatesForPool(QuestPool.QuestPoolType questPoolType, Global.RewardTrackType questType)
	{
		return (from state in m_questState.Values
			where IsQuestActive(state)
			where GetQuestPoolType(GameDbf.Quest.GetRecord(state.QuestId)) == questPoolType
			where GameDbf.Quest.GetRecord(state.QuestId)?.RewardTrackType == (Quest.RewardTrackType?)questType
			select state).ToList();
	}

	private List<PlayerQuestState> GetQuestStatesForPool(int questPoolId)
	{
		return m_questState.Values.Where(delegate(PlayerQuestState state)
		{
			QuestDbfRecord record = GameDbf.Quest.GetRecord(state.QuestId);
			return record != null && record.QuestPool == questPoolId;
		}).ToList();
	}

	private TimeSpan GetTimeUntilNextQuest(int questPoolId)
	{
		if (IsQuestPoolGrantingFromFallback(questPoolId))
		{
			questPoolId = GameDbf.QuestPool.GetRecord(questPoolId).FallbackQuestPool;
		}
		TimeSpan timeUntilNextQuest = TimeSpan.MinValue;
		if (m_questPoolNextQuestTime.TryGetValue(questPoolId, out var nextQuestTime))
		{
			timeUntilNextQuest = nextQuestTime - DateTime.Now;
		}
		return timeUntilNextQuest;
	}

	public string GetTimeUntilNextQuestString(int questPoolId)
	{
		TimeSpan timeUntilNextQuest = GetTimeUntilNextQuest(questPoolId);
		if (timeUntilNextQuest <= TimeSpan.Zero)
		{
			return "";
		}
		return TimeUtils.GetElapsedTimeString((long)timeUntilNextQuest.TotalSeconds, TimeUtils.SPLASHSCREEN_DATETIME_STRINGSET, roundUp: true);
	}

	private int GetQuestPoolRerollCount(int questPoolId)
	{
		if (IsQuestPoolGrantingFromFallback(questPoolId))
		{
			questPoolId = GameDbf.QuestPool.GetRecord(questPoolId).FallbackQuestPool;
		}
		int rerollCount = 0;
		if (m_questPoolState.TryGetValue(questPoolId, out var questPoolState))
		{
			rerollCount = questPoolState.RerollAvailableCount;
		}
		return rerollCount;
	}

	private bool IsQuestPoolGrantingFromFallback(int questPoolId)
	{
		QuestPoolDbfRecord questPoolAsset = GameDbf.QuestPool.GetRecord(questPoolId);
		return IsQuestPoolGrantingFromFallback(questPoolAsset);
	}

	private bool IsQuestPoolGrantingFromFallback(QuestPoolDbfRecord questPoolAsset)
	{
		if (questPoolAsset != null && questPoolAsset.FallbackQuestPoolRecord != null)
		{
			int totalNumberOfQuests = GameDbf.Quest.GetRecords().Count((QuestDbfRecord questDbfRecord) => questDbfRecord.QuestPool == questPoolAsset.ID);
			List<PlayerQuestState> questStates = GetQuestStatesForPool(questPoolAsset.ID);
			if (questStates.Count == totalNumberOfQuests)
			{
				return questStates.All((PlayerQuestState questState) => IsQuestComplete(questState));
			}
			return false;
		}
		return false;
	}

	private QuestDbfRecord GetNextChainQuestIfExists(QuestPoolDbfRecord questPoolAsset)
	{
		List<PlayerQuestState> questStates = GetQuestStatesForPool(questPoolAsset.ID);
		foreach (QuestDbfRecord questDbfRecord in from questRecord in GameDbf.Quest.GetRecords()
			where questRecord.QuestPool == questPoolAsset.ID
			select questRecord)
		{
			PlayerQuestState questState = questStates.Find((PlayerQuestState playerQuestState) => playerQuestState.QuestId == questDbfRecord.ID);
			if (questState != null && IsQuestComplete(questState))
			{
				continue;
			}
			if (IsChainQuest(questDbfRecord.ID))
			{
				int previousQuestId = -1;
				if (!m_chainQuestIdToPrevious.TryGetValue(questDbfRecord.ID, out previousQuestId))
				{
					return questDbfRecord;
				}
				PlayerQuestState previousQuestState = questStates.Find((PlayerQuestState playerQuestState) => playerQuestState.QuestId == previousQuestId);
				if (previousQuestState != null && IsQuestComplete(previousQuestState))
				{
					return questDbfRecord;
				}
			}
		}
		return null;
	}

	private bool ScheduleCheckForNewQuests(float? delaySecondsOverride = null)
	{
		Processor.CancelScheduledCallback(CheckForNewQuestsScheduledCallback);
		float delaySeconds = NextCheckForNewQuestsInterval();
		if (delaySecondsOverride.HasValue)
		{
			delaySeconds = delaySecondsOverride.Value;
		}
		else
		{
			TimeSpan? shortestTimeUntilNextQuest = null;
			foreach (KeyValuePair<int, DateTime> kvp in m_questPoolNextQuestTime)
			{
				_ = kvp.Key;
				TimeSpan timeUntilNextQuest = kvp.Value - DateTime.Now;
				if (shortestTimeUntilNextQuest.HasValue)
				{
					TimeSpan value = timeUntilNextQuest;
					TimeSpan? timeSpan = shortestTimeUntilNextQuest;
					if (!(value < timeSpan))
					{
						continue;
					}
				}
				shortestTimeUntilNextQuest = timeUntilNextQuest;
			}
			if (shortestTimeUntilNextQuest.HasValue && shortestTimeUntilNextQuest.Value.TotalSeconds > (double)delaySeconds)
			{
				delaySeconds = (float)shortestTimeUntilNextQuest.Value.TotalSeconds + UnityEngine.Random.Range(0f, m_checkForNewQuestsIntervalJitterSecs);
			}
		}
		return Processor.ScheduleCallback(delaySeconds, realTime: true, CheckForNewQuestsScheduledCallback);
	}

	private void CheckForNewQuestsScheduledCallback(object userData)
	{
		if (Network.IsLoggedIn())
		{
			if (!GameMgr.Get().IsFindingGame())
			{
				Network.Get().CheckForNewQuests();
			}
			ScheduleCheckForNewQuests(null);
		}
	}

	private float ResetCheckForNewQuestsInterval()
	{
		m_checkForNewQuestsIntervalSecs = 2f;
		return m_checkForNewQuestsIntervalSecs;
	}

	private float NextCheckForNewQuestsInterval()
	{
		float result = m_checkForNewQuestsIntervalSecs + UnityEngine.Random.Range(0f, m_checkForNewQuestsIntervalJitterSecs);
		m_checkForNewQuestsIntervalSecs *= 2f;
		return result;
	}

	private TimeSpan? GetTimeUntilQuestExpires(PlayerQuestState questState, bool includeInactive)
	{
		if (!m_questExpirationEnabled)
		{
			return null;
		}
		if (!IsQuestActive(questState) && !includeInactive)
		{
			return null;
		}
		QuestDbfRecord questAsset = GameDbf.Quest.GetRecord(questState.QuestId);
		if (questAsset == null)
		{
			return null;
		}
		if (!questAsset.CanAbandon)
		{
			return null;
		}
		if (questAsset.QuestPool != 0)
		{
			return null;
		}
		EventTimingManager eventMgr = EventTimingManager.Get();
		EventTimingType expirationEventType = questAsset.Event;
		if (expirationEventType == EventTimingType.UNKNOWN)
		{
			if (questAsset.RewardTrackType == Quest.RewardTrackType.NONE)
			{
				return null;
			}
			RewardTrack activeRewardTrack = RewardTrackManager.Get().GetRewardTrack((Global.RewardTrackType)questAsset.RewardTrackType);
			if (activeRewardTrack == null || !activeRewardTrack.IsValid)
			{
				return null;
			}
			expirationEventType = activeRewardTrack.RewardTrackAsset.Event;
		}
		if (!eventMgr.GetEventEndTimeUtc(expirationEventType).HasValue)
		{
			return null;
		}
		TimeSpan timeLeft = eventMgr.GetTimeLeftForEvent(expirationEventType);
		if (timeLeft < TimeSpan.Zero)
		{
			timeLeft = TimeSpan.Zero;
		}
		return timeLeft;
	}

	private string GetTimeUntilQuestExpiresString(PlayerQuestState questState)
	{
		TimeSpan? timeLeft = GetTimeUntilQuestExpires(questState, includeInactive: true);
		if (!timeLeft.HasValue)
		{
			return "";
		}
		return TimeUtils.GetElapsedTimeString((long)timeLeft.Value.TotalSeconds, TimeUtils.SPLASHSCREEN_DATETIME_STRINGSET, roundUp: true);
	}

	private TimeSpan? GetTimeUntilNextQuestExpiration()
	{
		TimeSpan? timeUntilNextExpiration = null;
		foreach (PlayerQuestState questState in m_questState.Values)
		{
			TimeSpan? timeLeftOnQuest = GetTimeUntilQuestExpires(questState, includeInactive: false);
			if (timeLeftOnQuest.HasValue && (!timeUntilNextExpiration.HasValue || timeLeftOnQuest < timeUntilNextExpiration))
			{
				timeUntilNextExpiration = timeLeftOnQuest;
			}
		}
		return timeUntilNextExpiration;
	}

	private void HandleExpiredQuests()
	{
		if ((!m_hasReceivedInitialClientState && !m_hasReceivedEventTimings) || !m_questExpirationEnabled)
		{
			return;
		}
		TimeSpan? timeUntilNextExpiration = GetTimeUntilNextQuestExpiration();
		if (!timeUntilNextExpiration.HasValue)
		{
			if (m_questExpiryChecker.IsRunning)
			{
				m_questExpiryChecker.Stop();
			}
			return;
		}
		if (m_questExpiryChecker.IsRunning)
		{
			DateTime updatedNextCallbackTime = DateTime.Now + timeUntilNextExpiration.Value;
			if ((m_questExpiryChecker.NextCallbackTime - updatedNextCallbackTime).Duration() <= TimeSpan.FromSeconds(m_questExpirationClientRetrySecs))
			{
				return;
			}
		}
		m_questExpiryChecker.Start(SendCheckForExpiredQuests, (float)timeUntilNextExpiration.Value.TotalSeconds, m_questExpirationClientRetrySecs, 2f, m_questExpirationClientJitterSecs);
	}

	private bool SendCheckForExpiredQuests()
	{
		if (!Network.IsLoggedIn())
		{
			return false;
		}
		if (m_questExpiryChecker.CallbackCount > 0 && !GetTimeUntilNextQuestExpiration().HasValue)
		{
			return false;
		}
		Network.Get().CheckForExpiredQuests();
		return true;
	}

	public string GetQuestDebugHudString()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("---------- Daily Quests ----------");
		AppendQuestPoolStateStringForDebugHud(sb, 1);
		AppendQuestStateStringsForDebugHud(sb, GetQuestStatesForPool(1));
		sb.AppendLine();
		sb.AppendLine("---------- Weekly Quests ----------");
		AppendQuestPoolStateStringForDebugHud(sb, 2);
		AppendQuestStateStringsForDebugHud(sb, GetQuestStatesForPool(2));
		sb.AppendLine();
		sb.AppendLine("---------- Other Quests ----------");
		AppendQuestStateStringsForDebugHud(sb, GetQuestStatesForPool(0));
		return sb.ToString();
	}

	private void AppendQuestPoolStateStringForDebugHud(StringBuilder sb, int questPoolId)
	{
		int rerollCount = GetQuestPoolRerollCount(questPoolId);
		sb.AppendFormat("Rerolls: {0} | Next Quest In: \"{1}\" ({2})\n", rerollCount, GetTimeUntilNextQuestString(questPoolId), GetTimeUntilNextQuestDebugString(questPoolId));
	}

	private void AppendQuestStateStringsForDebugHud(StringBuilder sb, List<PlayerQuestState> questStates)
	{
		foreach (PlayerQuestState questState in from q in questStates
			orderby q.Status, q.QuestId
			select q)
		{
			sb.AppendLine(QuestStateToString(questState));
		}
	}

	private string QuestStateToString(PlayerQuestState questState)
	{
		QuestDbfRecord questAsset = GameDbf.Quest.GetRecord(questState.QuestId);
		if (questAsset == null)
		{
			return $"id={questState.QuestId} INVALID";
		}
		StringBuilder sb = new StringBuilder($"id={questState.QuestId} {Enum.GetName(typeof(QuestStatus), questState.Status)} [{questState.Progress}/{questAsset.Quota}]");
		string expirationString = GetTimeUntilExpirationDebugString(questState);
		if (!string.IsNullOrEmpty(expirationString))
		{
			sb.Append(" (expires: " + expirationString + ")");
		}
		if (!string.IsNullOrEmpty(questAsset.Name))
		{
			sb.Append(" ");
			sb.Append(questAsset.Name);
		}
		return sb.ToString();
	}

	private string GetTimeUntilNextQuestDebugString(int questPoolId)
	{
		if (!m_questPoolNextQuestTime.TryGetValue(questPoolId, out var nextQuestTime))
		{
			return "unknown";
		}
		TimeSpan timeUntilNextQuest = nextQuestTime - DateTime.Now;
		return GetTimeSpanDebugString(timeUntilNextQuest);
	}

	private string GetTimeUntilExpirationDebugString(PlayerQuestState questState)
	{
		return GetTimeSpanDebugString(GetTimeUntilQuestExpires(questState, includeInactive: false));
	}

	private string GetTimeSpanDebugString(TimeSpan? timeSpan)
	{
		if (!timeSpan.HasValue)
		{
			return "";
		}
		if (timeSpan.Value <= TimeSpan.Zero)
		{
			return "now";
		}
		return TimeUtils.GetDevElapsedTimeString(timeSpan.Value);
	}

	public bool DebugScheduleCheckForNewQuests(float delaySeconds)
	{
		m_checkForNewQuestsIntervalSecs = ResetCheckForNewQuestsInterval();
		return ScheduleCheckForNewQuests(delaySeconds);
	}

	public void DebugScheduleCheckForExpiredQuests(float delaySeconds)
	{
		m_questExpiryChecker.Start(SendCheckForExpiredQuests, delaySeconds, 2f, 2f, 1f);
	}

	public void SimulateQuestProgress(int questId)
	{
		this.OnQuestProgress?.Invoke(CreateQuestDataModelById(questId));
	}

	public void SimulateQuestNotificationPopup(QuestPool.QuestPoolType poolType)
	{
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		if ((mode == SceneMgr.Mode.BACON || mode == SceneMgr.Mode.BACON_COLLECTION) && poolType == QuestPool.QuestPoolType.WEEKLY)
		{
			m_battlegroundsQuestPoolTypesToShow.Clear();
			m_battlegroundsQuestPoolTypesToShow.Add(poolType);
		}
		else
		{
			m_questPoolTypesToShow.Clear();
			m_questPoolTypesToShow.Add(poolType);
			m_showQuestPoolTypesFromLogin = true;
		}
	}
}
