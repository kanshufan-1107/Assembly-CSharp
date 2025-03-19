using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Assets;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core.Streaming;
using Hearthstone.DataModels;
using Hearthstone.Streaming;
using Hearthstone.UI;
using PegasusGame;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

namespace Hearthstone.Progression;

public class AchievementManager : IService
{
	public enum AchievementStatus
	{
		UNKNOWN,
		ACTIVE,
		COMPLETED,
		REWARD_GRANTED,
		REWARD_ACKED,
		RESET
	}

	public enum AchievementDisplayMode
	{
		Default,
		Inspection
	}

	public delegate void StatusChangedDelegate(int achievementId, AchievementStatus status);

	public delegate void ProgressChangedDelegate(int achievementId, int progress);

	public delegate void CompletedDateChangedDelegate(int achievementId, long completedDate);

	public delegate void PointsChangedDelegate();

	public delegate void SelectedCategoryDelegate(AchievementCategoryDataModel subcategory);

	public delegate void SelectedSubcategoryDelegate(AchievementSubcategoryDataModel subcategory);

	public delegate void InGameProgressChangedDelegate(int achievementId, int progress);

	public static readonly AssetReference ACHIEVEMENT_TOAST_PREFAB = new AssetReference("AchievementToast.prefab:9fa72c338c657d54fb9f63de21c1e4f8");

	[CompilerGenerated]
	private ProgressChangedDelegate OnProgressChanged = delegate
	{
	};

	[CompilerGenerated]
	private CompletedDateChangedDelegate OnCompletedDateChanged = delegate
	{
	};

	[CompilerGenerated]
	private InGameProgressChangedDelegate OnInGameProgressChanged = delegate
	{
	};

	private readonly PlayerState<PlayerAchievementState> m_playerState = new PlayerState<PlayerAchievementState>(CreateDefaultPlayerState);

	private readonly Dictionary<int, int> m_claimingAchievements = new Dictionary<int, int>();

	private readonly Dictionary<int, int> m_achievementInGameProgress = new Dictionary<int, int>();

	private readonly HashSet<int> m_achievementToastShown = new HashSet<int>();

	private Lazy<AchievementCategoryListDataModel> m_categories;

	private Lazy<Dictionary<int, AchievementCategoryDataModel>> m_categoryMap;

	private Lazy<Dictionary<int, AchievementSectionDataModel>> m_sectionMap;

	private Lazy<Dictionary<int, AchievementSubcategoryDataModel>> m_subcategoryMap;

	private Lazy<Dictionary<int, int>> m_achievementToSubcategoryMap;

	private Lazy<Dictionary<int, int>> m_subcategoryToCategoryMap;

	private Lazy<Dictionary<int, AchievementDataModel>> m_achievementDataModelMap;

	private DataModelList<AchievementDataModel> m_completedAchievements = new DataModelList<AchievementDataModel>();

	private Queue<int> m_completedAchievementToastQueue;

	private int m_numberOfAchievementsQueued;

	private readonly RewardPresenter m_rewardPresenter = new RewardPresenter();

	private Widget m_achievementToast;

	private int m_toastNotificationSuppressionCount;

	private AchievementCategoryDataModel m_selectedCategory;

	private Dictionary<int, long> m_achievementClaimedDates = new Dictionary<int, long>();

	private float m_endOfTurnToastPauseBufferSecs = 1.5f;

	private readonly AchievementStats m_stats;

	private readonly HashSet<AchievementSectionDbfRecord> m_dirtySections = new HashSet<AchievementSectionDbfRecord>();

	private GameDownloadManager m_gameDownloadManager;

	public bool ToastNotificationsPaused => m_toastNotificationSuppressionCount > 0;

	public int TotalPoints => m_stats?.GetTotalPoints() ?? 0;

	public AchievementCategoryListDataModel Categories => m_categories.Value;

	public event StatusChangedDelegate OnStatusChanged = delegate
	{
	};

	public event PointsChangedDelegate OnPointsChanged = delegate
	{
	};

	public event SelectedCategoryDelegate OnSelectedCategoryChanged = delegate
	{
	};

	public event SelectedSubcategoryDelegate OnSelectedSubcategoryChanged = delegate
	{
	};

	public RewardPresenter GetRewardPresenter()
	{
		return m_rewardPresenter;
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		Network network = serviceLocator.Get<Network>();
		network.RegisterNetHandler(PlayerAchievementStateUpdate.PacketID.ID, ReceiveAchievementStateUpdateMessage);
		network.RegisterNetHandler(GameSetup.PacketID.ID, OnGameSetup);
		network.RegisterNetHandler(AchievementProgress.PacketID.ID, OnAchievementInGameProgress);
		network.RegisterNetHandler(AchievementComplete.PacketID.ID, OnAchievementComplete);
		m_gameDownloadManager = serviceLocator.Get<GameDownloadManager>();
		if (m_gameDownloadManager != null)
		{
			m_gameDownloadManager.RegisterModuleInstallationStateChangeListener(OnModuleInstallationStateChanged, invokeImmediately: false);
		}
		else
		{
			Log.Achievements.PrintError("AchievementManger: GameDownloadManager cannot be located.");
		}
		m_playerState.OnStateChanged += OnStateChanged;
		m_toastNotificationSuppressionCount = 0;
		m_achievementToast = null;
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[3]
		{
			typeof(Network),
			typeof(NetCache),
			typeof(GameDownloadManager)
		};
	}

	public void Shutdown()
	{
		if (m_gameDownloadManager != null)
		{
			m_gameDownloadManager.UnregisterModuleInstallationStateChangeListener(OnModuleInstallationStateChanged);
			m_gameDownloadManager = null;
		}
		m_completedAchievementToastQueue.Clear();
	}

	private void WillReset()
	{
		m_playerState.Reset();
		m_claimingAchievements.Clear();
		m_categories = new Lazy<AchievementCategoryListDataModel>(InitializeCategories);
		m_categoryMap = new Lazy<Dictionary<int, AchievementCategoryDataModel>>(InitializeCategoryMap);
		m_sectionMap = new Lazy<Dictionary<int, AchievementSectionDataModel>>(InitializeSections);
		m_subcategoryMap = new Lazy<Dictionary<int, AchievementSubcategoryDataModel>>(InitializeSubcategories);
		m_achievementToSubcategoryMap = new Lazy<Dictionary<int, int>>(InitializeAchievementToSubcategoryMap);
		m_subcategoryToCategoryMap = new Lazy<Dictionary<int, int>>(InitializeSubcategoryToCategoryMap);
		m_achievementDataModelMap = new Lazy<Dictionary<int, AchievementDataModel>>(InitializeAllAchievementDataModels);
		m_completedAchievements.Clear();
		m_rewardPresenter.Clear();
		m_completedAchievementToastQueue = new Queue<int>();
		m_achievementInGameProgress.Clear();
		m_achievementToastShown.Clear();
		m_achievementToast = null;
		m_toastNotificationSuppressionCount = 0;
		m_stats.InvalidateAll();
		m_dirtySections.Clear();
		m_selectedCategory = null;
		m_achievementClaimedDates.Clear();
	}

	public static AchievementManager Get()
	{
		return ServiceManager.Get<AchievementManager>();
	}

	public AchievementManager()
	{
		m_categories = new Lazy<AchievementCategoryListDataModel>(InitializeCategories);
		m_categoryMap = new Lazy<Dictionary<int, AchievementCategoryDataModel>>(InitializeCategoryMap);
		m_sectionMap = new Lazy<Dictionary<int, AchievementSectionDataModel>>(InitializeSections);
		m_subcategoryMap = new Lazy<Dictionary<int, AchievementSubcategoryDataModel>>(InitializeSubcategories);
		m_achievementToSubcategoryMap = new Lazy<Dictionary<int, int>>(InitializeAchievementToSubcategoryMap);
		m_subcategoryToCategoryMap = new Lazy<Dictionary<int, int>>(InitializeSubcategoryToCategoryMap);
		m_achievementDataModelMap = new Lazy<Dictionary<int, AchievementDataModel>>(InitializeAllAchievementDataModels);
		m_completedAchievementToastQueue = new Queue<int>();
		m_stats = new AchievementStats(m_playerState.GetState);
	}

	public AchievementDataModel GetAchievementDataModel(int achievementId)
	{
		if (m_achievementDataModelMap.Value.TryGetValue(achievementId, out var achievementDataModel))
		{
			return achievementDataModel;
		}
		AchievementDbfRecord record = GameDbf.Achievement.GetRecord(achievementId);
		if (record == null)
		{
			Log.Achievements.PrintWarning($"[record] is null with achievementId [{achievementId}] in GetAchievementDataModel()");
			return null;
		}
		achievementDataModel = AchievementFactory.CreateAchievementDataModel(record, m_playerState.GetState(achievementId));
		m_achievementDataModelMap.Value.Add(achievementId, achievementDataModel);
		return achievementDataModel;
	}

	public AchievementDataModel GetAchievementDataModelFromSection(int achievementId)
	{
		AchievementDbfRecord record = GameDbf.Achievement.GetRecord(achievementId);
		if (record == null)
		{
			return null;
		}
		if (m_sectionMap.Value.TryGetValue(record.AchievementSection, out var section) && section.Achievements.Achievements.Count == 0)
		{
			section.LoadAchievements(m_playerState.GetState);
			section.Achievements.Achievements.UpdateTiers();
		}
		return section?.Achievements.Achievements.FirstOrDefault((AchievementDataModel x) => x.ID == achievementId);
	}

	public AchievementSectionDataModel GetAchievementSectionDataModelFromAchievement(AchievementDataModel achievement)
	{
		AchievementDbfRecord achievementRecord = GameDbf.Achievement.GetRecord(achievement.ID);
		m_sectionMap.Value.TryGetValue(achievementRecord.AchievementSection, out var sectionDataModel);
		return sectionDataModel;
	}

	public DataModelList<AchievementDataModel> GetRecentlyCompletedAchievements()
	{
		return m_completedAchievements;
	}

	public bool IsAchievementComplete(int achievementId)
	{
		return ProgressUtils.IsAchievementComplete((AchievementStatus)m_playerState.GetState(achievementId).Status);
	}

	public bool IsAchievementLocked(int achievementId)
	{
		if (m_achievementToSubcategoryMap.Value.TryGetValue(achievementId, out var subcategoryId))
		{
			return IsAchievementSubcategoryLocked(subcategoryId);
		}
		return false;
	}

	public bool IsAchievementSubcategoryLocked(int subcategoryId)
	{
		if (m_gameDownloadManager == null)
		{
			Log.Achievements.PrintError("AchievementManager: Tried to check is achievement subcategory is locked when m_gameDownloadManager is null!");
			return false;
		}
		if (!m_subcategoryMap.Value.TryGetValue(subcategoryId, out var subcategory))
		{
			return false;
		}
		DownloadTags.Content tag = ProgressUtils.GetContentTagFromAchievementCategoryOrSubcategoryName(subcategory.Name);
		if (tag != 0)
		{
			return !m_gameDownloadManager.IsModuleReadyToPlay(tag);
		}
		if (!m_subcategoryToCategoryMap.Value.TryGetValue(subcategoryId, out var categoryId))
		{
			return false;
		}
		return IsAchievementCategoryLocked(categoryId);
	}

	public bool IsAchievementCategoryLocked(int categoryId)
	{
		if (m_gameDownloadManager == null)
		{
			Log.Achievements.PrintError("AchievementManager: Tried to check is achievement category is locked when m_gameDownloadManager is null!");
			return false;
		}
		if (m_categoryMap.Value.TryGetValue(categoryId, out var category))
		{
			DownloadTags.Content tag = ProgressUtils.GetContentTagFromAchievementCategoryOrSubcategoryName(category.Name);
			if (tag != 0)
			{
				return !m_gameDownloadManager.IsModuleReadyToPlay(tag);
			}
		}
		return false;
	}

	public DownloadTags.Content GetContentTagFromAchievement(int achievementId)
	{
		if (!m_achievementToSubcategoryMap.Value.TryGetValue(achievementId, out var subcategoryId))
		{
			return DownloadTags.Content.Unknown;
		}
		return GetContentTagFromAchievementSubcategory(subcategoryId);
	}

	public DownloadTags.Content GetContentTagFromAchievementSubcategory(int subcategoryId)
	{
		if (!m_subcategoryMap.Value.TryGetValue(subcategoryId, out var subcategoryDataModel))
		{
			return DownloadTags.Content.Unknown;
		}
		DownloadTags.Content tag = ProgressUtils.GetContentTagFromAchievementCategoryOrSubcategoryName(subcategoryDataModel.Name);
		if (tag != 0)
		{
			return tag;
		}
		if (!m_subcategoryToCategoryMap.Value.TryGetValue(subcategoryId, out var categoryId))
		{
			return DownloadTags.Content.Unknown;
		}
		return GetContentTagFromAchievementCategory(categoryId);
	}

	public DownloadTags.Content GetContentTagFromAchievementCategory(int categoryId)
	{
		if (!m_categoryMap.Value.TryGetValue(categoryId, out var categoryDataModel))
		{
			return DownloadTags.Content.Unknown;
		}
		return ProgressUtils.GetContentTagFromAchievementCategoryOrSubcategoryName(categoryDataModel.Name);
	}

	public long GetCompletionDate(int achievementId)
	{
		return m_playerState.GetState(achievementId).CompletedDate;
	}

	public long GetClaimedDate(int achievementId)
	{
		if (!m_achievementClaimedDates.TryGetValue(achievementId, out var timeStamp))
		{
			return GetCompletionDate(achievementId);
		}
		return timeStamp;
	}

	public void ClearClaimedDates()
	{
		m_achievementClaimedDates.Clear();
	}

	public bool ClaimAchievementReward(int achievementId, int chooseOneRewardItemId = 0)
	{
		if (m_playerState.GetState(achievementId).Status != 2)
		{
			return false;
		}
		if (m_claimingAchievements.ContainsKey(achievementId))
		{
			return false;
		}
		Network.Get().ClaimAchievementReward(achievementId, chooseOneRewardItemId);
		m_claimingAchievements.Add(achievementId, chooseOneRewardItemId);
		return true;
	}

	public void AckAchievement(int achievementId)
	{
		PlayerAchievementState achievementState = m_playerState.GetState(achievementId);
		if (achievementState.Status == 3)
		{
			achievementState.Status = 4;
			m_playerState.UpdateState(achievementId, achievementState);
			m_achievementClaimedDates.Add(achievementId, TimeUtils.UnixTimestampMilliseconds);
			Network.Get().AckAchievement(achievementId);
		}
	}

	public float GetNotificationPauseBufferSeconds()
	{
		return m_endOfTurnToastPauseBufferSecs;
	}

	public bool IsShowingToast()
	{
		return m_achievementToast != null;
	}

	public bool CheckToastQueue()
	{
		NetCache.NetCacheFeatures guardianVars = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>();
		if (guardianVars != null && guardianVars.AchievementToastDisabled)
		{
			return false;
		}
		if (m_numberOfAchievementsQueued > 0)
		{
			int achievementId = DequeueAchievement();
			if (achievementId != 0)
			{
				ShowAchievementComplete(achievementId);
			}
			else
			{
				ShowAndXMoreAchievementToast();
			}
			return true;
		}
		return false;
	}

	public void PauseToastNotifications()
	{
		m_toastNotificationSuppressionCount++;
	}

	public void UnpauseToastNotifications()
	{
		if (m_toastNotificationSuppressionCount != 0)
		{
			m_toastNotificationSuppressionCount--;
			if (!ToastNotificationsPaused)
			{
				CheckToastQueue();
			}
		}
	}

	public void ShowAchievementComplete(int achievementId)
	{
		if (m_achievementToastShown.Contains(achievementId))
		{
			return;
		}
		NetCache.NetCacheFeatures guardianVars = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>();
		if (guardianVars != null && guardianVars.AchievementToastDisabled)
		{
			return;
		}
		AchievementDbfRecord achievementRecord = GameDbf.Achievement.GetRecord(achievementId);
		if (achievementRecord == null || achievementRecord.AchievementVisibility == Assets.Achievement.AchievementVisibility.HIDDEN)
		{
			return;
		}
		if (IsShowingToast() || ToastNotificationsPaused)
		{
			EnqueueAchievement(achievementId);
			return;
		}
		m_achievementToastShown.Add(achievementId);
		m_achievementToast = WidgetInstance.Create(ACHIEVEMENT_TOAST_PREFAB);
		AchievementDataModel dataModel = GetAchievementDataModelFromSection(achievementId);
		if (dataModel == null)
		{
			return;
		}
		m_achievementToast.RegisterReadyListener(delegate
		{
			AchievementToast componentInChildren = m_achievementToast.GetComponentInChildren<AchievementToast>();
			componentInChildren.Initialize(dataModel);
			componentInChildren.Show();
		});
		m_achievementToast.RegisterDeactivatedListener(delegate
		{
			m_achievementToast = null;
			if (!CheckToastQueue())
			{
				SocialToastMgr.Get().CheckToastQueue();
			}
		});
		if (achievementRecord.SocialToast)
		{
			BnetPresenceMgr.Get().SetGameField(27u, achievementId);
		}
	}

	private void EnqueueAchievement(int achievementId)
	{
		m_numberOfAchievementsQueued++;
		if (m_numberOfAchievementsQueued <= 3)
		{
			m_completedAchievementToastQueue.Enqueue(achievementId);
		}
	}

	private int DequeueAchievement()
	{
		if (m_completedAchievementToastQueue.Count > 0)
		{
			m_numberOfAchievementsQueued--;
			return m_completedAchievementToastQueue.Dequeue();
		}
		return 0;
	}

	private void ShowAndXMoreAchievementToast()
	{
		if (m_completedAchievementToastQueue.Count <= 0 && m_numberOfAchievementsQueued != 0)
		{
			AchievementToast.ShowAndXMoreToast(m_numberOfAchievementsQueued);
			m_numberOfAchievementsQueued = 0;
		}
	}

	public bool ShowNextReward(Action callback)
	{
		return m_rewardPresenter.ShowNextReward(callback);
	}

	public bool HasReward()
	{
		return m_rewardPresenter.HasReward();
	}

	public void SelectCategory(AchievementCategoryDataModel category)
	{
		if (m_selectedCategory != null)
		{
			m_selectedCategory.SelectedSubcategory = null;
		}
		m_selectedCategory = category;
		m_selectedCategory.Subcategories.Subcategories.ForEach(delegate(AchievementSubcategoryDataModel subcategory)
		{
			subcategory.UpdateIsLocked();
		});
		this.OnSelectedCategoryChanged(m_selectedCategory);
		AchievementSubcategoryDataModel subcategory2 = category.Subcategories.Subcategories.FirstOrDefault((AchievementSubcategoryDataModel x) => !x.IsLocked);
		if (subcategory2 == null)
		{
			subcategory2 = category.Subcategories.Subcategories[0];
		}
		SelectSubcategory(subcategory2);
	}

	public void SelectSubcategory(AchievementSubcategoryDataModel subcategory)
	{
		if (m_selectedCategory.SelectedSubcategory != subcategory)
		{
			m_selectedCategory.SelectedSubcategory = subcategory;
			subcategory.Sections.Sections.SelectMany((AchievementSectionDataModel section) => section.Achievements.Achievements).ForEach(ProgressUtils.UpdateAchievementIsLocked);
			LoadRewards(subcategory.Sections.Sections.SelectMany((AchievementSectionDataModel x) => x.Achievements.Achievements).ToDataModelList());
			this.OnSelectedSubcategoryChanged(subcategory);
		}
	}

	public void LoadRewards(DataModelList<AchievementDataModel> achievements)
	{
		foreach (AchievementDataModel achievement in achievements)
		{
			LoadReward(achievement);
		}
	}

	public void LoadReward(AchievementDataModel achievement)
	{
		if (!achievement.IsLocked && achievement.RewardList == null)
		{
			AchievementDbfRecord record = GameDbf.Achievement.GetRecord(achievement.ID);
			RewardListDataModel rewards = RewardUtils.CreateRewardListDataModelFromRewardListId(record.RewardList) ?? new RewardListDataModel();
			RewardTrack rewardTrack = null;
			if (record.RewardTrackType != 0)
			{
				rewardTrack = RewardTrackManager.Get().GetRewardTrack(record.RewardTrackType);
			}
			int rewardTrackXp = record.RewardTrackXp;
			int rewardTrackXpBonusAdjusted = rewardTrack?.ApplyXpBonusPercent(rewardTrackXp) ?? 0;
			int rewardTrackXpBonusPercent = rewardTrack?.TrackDataModel.XpBonusPercent ?? 0;
			achievement.RewardList = rewards;
			achievement.RewardSummary = ProgressUtils.FormatRewardsSummary(rewards, rewardTrackXpBonusAdjusted, record.Points, rewardTrackXpBonusPercent > 0);
			achievement.RewardTrackXp = rewardTrackXp;
			achievement.RewardTrackXpBonusAdjusted = rewardTrackXpBonusAdjusted;
			achievement.RewardTrackXpBonusPercent = rewardTrackXpBonusPercent;
		}
	}

	private static PlayerAchievementState CreateDefaultPlayerState(int id)
	{
		return new PlayerAchievementState
		{
			HasAchievementId = true,
			AchievementId = id,
			HasStatus = true,
			Status = 1,
			HasProgress = true,
			Progress = 0,
			HasCompletedDate = true,
			CompletedDate = 0L
		};
	}

	private void ReceiveAchievementStateUpdateMessage()
	{
		PlayerAchievementStateUpdate updateMessage = Network.Get().GetPlayerAchievementStateUpdate();
		if (updateMessage == null)
		{
			return;
		}
		foreach (PlayerAchievementState newState in updateMessage.Achievement)
		{
			m_playerState.UpdateState(newState.AchievementId, newState);
		}
		UpdateStats();
	}

	private void OnGameSetup()
	{
		m_achievementInGameProgress.Clear();
		foreach (PlayerAchievementState state in m_playerState)
		{
			m_achievementInGameProgress[state.AchievementId] = state.Progress;
		}
	}

	private void OnAchievementInGameProgress()
	{
		if (SpectatorManager.Get().IsSpectatingOrWatching)
		{
			return;
		}
		AchievementProgress packet = Network.Get().GetAchievementInGameProgress();
		if (packet == null || !packet.HasAchievementId || !packet.HasOpType || !packet.HasAmount)
		{
			return;
		}
		int achievementId = packet.AchievementId;
		AchievementDbfRecord achievementAsset = GameDbf.Achievement.GetRecord(achievementId);
		if (achievementAsset == null)
		{
			return;
		}
		m_achievementInGameProgress.TryGetValue(achievementId, out var inGameProgress);
		if (inGameProgress >= achievementAsset.Quota)
		{
			return;
		}
		int previousProgress = inGameProgress;
		switch (packet.OpType)
		{
		case ProgOpType.PROG_OP_ADD:
			inGameProgress += packet.Amount;
			break;
		case ProgOpType.PROG_OP_SET:
			inGameProgress = packet.Amount;
			break;
		}
		if (inGameProgress != previousProgress)
		{
			m_achievementInGameProgress[achievementId] = inGameProgress;
			OnInGameProgressChanged(achievementId, inGameProgress);
			if (previousProgress < achievementAsset.Quota && inGameProgress >= achievementAsset.Quota)
			{
				ShowAchievementComplete(achievementId);
			}
		}
	}

	private void OnAchievementComplete()
	{
		AchievementComplete packet = Network.Get().GetAchievementComplete();
		if (packet == null)
		{
			return;
		}
		foreach (int achievementId in packet.AchievementIds)
		{
			ShowAchievementComplete(achievementId);
		}
	}

	private void OnStateChanged(PlayerAchievementState oldState, PlayerAchievementState newState)
	{
		if (newState != null)
		{
			if (oldState.Status != newState.Status)
			{
				UpdateStatus(newState.AchievementId, (AchievementStatus)oldState.Status, (AchievementStatus)newState.Status, newState.RewardItemOutput);
			}
			if (oldState.Progress != newState.Progress)
			{
				UpdateProgress(newState.AchievementId, newState.Progress);
			}
			if (oldState.CompletedDate != newState.CompletedDate)
			{
				UpdateCompletionDate(newState.AchievementId, newState.CompletedDate);
			}
		}
	}

	private void UpdateCompletionDate(int achievementId, long completedDate)
	{
		if (m_achievementDataModelMap.Value.TryGetValue(achievementId, out var achievementDataModel))
		{
			achievementDataModel.CompletionDate = ProgressUtils.FormatAchievementCompletionDate(completedDate);
			OnCompletedDateChanged(achievementId, completedDate);
		}
	}

	private void UpdateProgress(int achievementId, int progress)
	{
		if (m_achievementDataModelMap.Value.TryGetValue(achievementId, out var achievementDataModel))
		{
			achievementDataModel.Progress = progress;
			achievementDataModel.UpdateProgress();
			OnProgressChanged(achievementId, progress);
		}
	}

	private void UpdateStatus(int achievementId, AchievementStatus oldStatus, AchievementStatus newStatus, List<RewardItemOutput> rewardItemOutput = null)
	{
		AchievementDbfRecord achievementRecord = GameDbf.Achievement.GetRecord(achievementId);
		if (achievementRecord == null || (achievementRecord.AchievementVisibility == Assets.Achievement.AchievementVisibility.HIDDEN && !TavernGuideManager.Get().IsTavernGuideCompletionAchievement(achievementId)))
		{
			return;
		}
		if (achievementRecord.AchievementVisibility != Assets.Achievement.AchievementVisibility.HIDDEN && ProgressUtils.IsAchievementComplete(newStatus) && !m_completedAchievements.Any((AchievementDataModel x) => x.ID == achievementId))
		{
			AchievementDataModel achievement = GetAchievementDataModel(achievementId);
			m_completedAchievements.Add(achievement);
		}
		switch (newStatus)
		{
		case AchievementStatus.COMPLETED:
		case AchievementStatus.REWARD_ACKED:
			SetSteamAchievementIfNeeded(achievementRecord);
			break;
		case AchievementStatus.REWARD_GRANTED:
		{
			int chooseOneRewardItemId = 0;
			if (m_claimingAchievements.ContainsKey(achievementId))
			{
				chooseOneRewardItemId = m_claimingAchievements[achievementId];
				m_claimingAchievements.Remove(achievementId);
			}
			if ((GameDbf.RewardList.GetRecord(achievementRecord.RewardList)?.RewardItems?.Count).GetValueOrDefault() > 0)
			{
				m_rewardPresenter.EnqueueReward(AchievementFactory.CreateRewardScrollDataModel(achievementId, chooseOneRewardItemId, rewardItemOutput), delegate
				{
					AckAchievement(achievementId);
				});
			}
			else
			{
				AckAchievement(achievementId);
			}
			break;
		}
		default:
			Debug.LogWarning($"AchievementManager: unknown status {newStatus} for achievement id {achievementId}");
			break;
		case AchievementStatus.ACTIVE:
			break;
		}
		if (achievementRecord.AchievementSectionRecord != null)
		{
			m_dirtySections.Add(achievementRecord.AchievementSectionRecord);
		}
		if (m_achievementDataModelMap.Value.TryGetValue(achievementId, out var achievementDataModel))
		{
			achievementDataModel.Status = newStatus;
			this.OnStatusChanged(achievementId, newStatus);
		}
	}

	private void UpdateStats()
	{
		if (m_dirtySections.Count == 0)
		{
			return;
		}
		int currentPoints = m_stats.GetTotalPoints();
		foreach (AchievementSectionDbfRecord dirtySection in m_dirtySections)
		{
			m_stats.InvalidateSection(dirtySection);
		}
		List<AchievementSubcategoryDbfRecord> dirtySubcategoryRecords = GameDbf.AchievementSubcategory.GetRecords((AchievementSubcategoryDbfRecord record) => record.Sections.Any((AchievementSectionItemDbfRecord sectionItem) => m_dirtySections.Contains(sectionItem.AchievementSectionRecord)));
		foreach (AchievementSubcategoryDbfRecord record2 in dirtySubcategoryRecords)
		{
			m_stats.InvalidSubcategory(record2);
		}
		List<AchievementCategoryDbfRecord> dirtyCategoryRecords = GameDbf.AchievementCategory.GetRecords((AchievementCategoryDbfRecord record) => record.Subcategories.Intersect(dirtySubcategoryRecords).Any());
		foreach (AchievementCategoryDbfRecord record3 in dirtyCategoryRecords)
		{
			m_stats.InvalidateCategory(record3);
		}
		IEnumerable<AchievementCategoryDataModel> dirtyCategoryDataModels = Categories.Categories.Where((AchievementCategoryDataModel dataModel) => dirtyCategoryRecords.Any((AchievementCategoryDbfRecord record) => record.ID == dataModel.ID));
		IEnumerable<AchievementSubcategoryDataModel> dirtySubcategoryDataModels = from dataModel in dirtyCategoryDataModels.SelectMany((AchievementCategoryDataModel category) => category.Subcategories.Subcategories)
			where dirtySubcategoryRecords.Any((AchievementSubcategoryDbfRecord record) => record.ID == dataModel.ID)
			select dataModel;
		foreach (AchievementSubcategoryDbfRecord dirtySubcategoryRecord in dirtySubcategoryRecords)
		{
			AchievementSubcategoryDataModel dataModel2 = dirtySubcategoryDataModels.FirstOrDefault((AchievementSubcategoryDataModel x) => x.ID == dirtySubcategoryRecord.ID);
			if (dataModel2 != null)
			{
				dataModel2.Stats.Points = m_stats.GetSubcategoryPoints(dirtySubcategoryRecord);
				dataModel2.Stats.Unclaimed = m_stats.GetSubcategoryUnclaimed(dirtySubcategoryRecord);
				dataModel2.Stats.CompletedAchievements = m_stats.GetSubcategoryCompleted(dirtySubcategoryRecord);
				dataModel2.Stats.UpdateCompletionPercentage();
			}
		}
		foreach (AchievementCategoryDbfRecord dirtyCategoryRecord in dirtyCategoryRecords)
		{
			AchievementCategoryDataModel dataModel3 = dirtyCategoryDataModels.FirstOrDefault((AchievementCategoryDataModel x) => x.ID == dirtyCategoryRecord.ID);
			if (dataModel3 != null)
			{
				dataModel3.Stats.Points = m_stats.GetCategoryPoints(dirtyCategoryRecord);
				dataModel3.Stats.Unclaimed = m_stats.GetCategoryUnclaimed(dirtyCategoryRecord);
				dataModel3.Stats.CompletedAchievements = m_stats.GetCategoryCompleted(dirtyCategoryRecord);
				dataModel3.Stats.UpdateCompletionPercentage();
			}
		}
		Categories.Stats.Points = m_stats.GetTotalPoints();
		Categories.Stats.Unclaimed = m_stats.GetTotalUnclaimed();
		Categories.Stats.CompletedAchievements = m_stats.GetTotalCompleted();
		Categories.Stats.UpdateCompletionPercentage();
		if (currentPoints < m_stats.GetTotalPoints())
		{
			this.OnPointsChanged();
		}
		m_dirtySections.Clear();
	}

	private void OnModuleInstallationStateChanged(DownloadTags.Content moduleTag, ModuleState state)
	{
		if (m_selectedCategory == null)
		{
			return;
		}
		m_selectedCategory.Subcategories.Subcategories.ForEach(delegate(AchievementSubcategoryDataModel subcategoryDataModel)
		{
			subcategoryDataModel.UpdateIsLocked();
		});
		if (m_selectedCategory.SelectedSubcategory != null && ProgressUtils.GetContentTagFromAchievementCategoryOrSubcategoryName(m_selectedCategory.SelectedSubcategory.Name) == moduleTag)
		{
			m_selectedCategory.SelectedSubcategory.Sections.Sections.SelectMany((AchievementSectionDataModel sections) => sections.Achievements.Achievements).ForEach(ProgressUtils.UpdateAchievementIsLocked);
		}
	}

	private AchievementCategoryListDataModel InitializeCategories()
	{
		return AchievementFactory.CreateAchievementListDataModel(m_stats);
	}

	private Dictionary<int, AchievementCategoryDataModel> InitializeCategoryMap()
	{
		return m_categories.Value.Categories.ToDictionary((AchievementCategoryDataModel x) => x.ID);
	}

	private Dictionary<int, AchievementSectionDataModel> InitializeSections()
	{
		return m_categories.Value.Categories.Select((AchievementCategoryDataModel category) => category.Subcategories).SelectMany((AchievementSubcategoryListDataModel subCategory) => subCategory.Subcategories).SelectMany((AchievementSubcategoryDataModel x) => x.Sections.Sections)
			.ToDictionary((AchievementSectionDataModel x) => x.ID);
	}

	private Dictionary<int, AchievementSubcategoryDataModel> InitializeSubcategories()
	{
		return m_categories.Value.Categories.Select((AchievementCategoryDataModel category) => category.Subcategories).SelectMany((AchievementSubcategoryListDataModel subCategory) => subCategory.Subcategories).ToDictionary((AchievementSubcategoryDataModel x) => x.ID);
	}

	private Dictionary<int, int> InitializeAchievementToSubcategoryMap()
	{
		Dictionary<int, int> map = new Dictionary<int, int>();
		foreach (AchievementSubcategoryDataModel subcategory in m_categories.Value.Categories.Select((AchievementCategoryDataModel category) => category.Subcategories).SelectMany((AchievementSubcategoryListDataModel subCategory) => subCategory.Subcategories))
		{
			subcategory.Sections.Sections.Select((AchievementSectionDataModel sectionDataModel) => sectionDataModel.Achievements).SelectMany((AchievementListDataModel achievementListDataModel) => achievementListDataModel.Achievements).ForEach(delegate(AchievementDataModel achievementDataModel)
			{
				map.Add(achievementDataModel.ID, subcategory.ID);
			});
		}
		return map;
	}

	private Dictionary<int, int> InitializeSubcategoryToCategoryMap()
	{
		Dictionary<int, int> map = new Dictionary<int, int>();
		foreach (AchievementCategoryDataModel category in m_categories.Value.Categories)
		{
			foreach (AchievementSubcategoryDataModel subcategory in category.Subcategories.Subcategories)
			{
				map.Add(subcategory.ID, category.ID);
			}
		}
		return map;
	}

	private Dictionary<int, AchievementDataModel> InitializeAllAchievementDataModels()
	{
		Dictionary<int, AchievementDataModel> achievementMap = new Dictionary<int, AchievementDataModel>();
		foreach (AchievementSectionDataModel value in m_sectionMap.Value.Values)
		{
			value.LoadAchievements(m_playerState.GetState);
			foreach (AchievementDataModel achievement in value.Achievements.Achievements)
			{
				achievementMap.Add(achievement.ID, achievement);
			}
		}
		return achievementMap;
	}

	private void SetSteamAchievementIfNeeded(AchievementDbfRecord achievementRecord)
	{
		_ = PlatformSettings.IsSteam;
	}

	public AchievementDataModel Debug_GetAchievementDataModel(int achievementId)
	{
		return GetAchievementDataModel(achievementId);
	}

	public string Debug_GetAchievementHudString()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendFormat("Score: {0} / {1}", m_stats.GetTotalPoints(), CategoryList.CountAvailablePoints());
		sb.AppendLine();
		foreach (PlayerAchievementState achieveState in from a in m_playerState
			orderby a.Status, a.AchievementId
			select a)
		{
			sb.AppendLine(Debug_AchievementStateToString(achieveState));
		}
		return sb.ToString();
	}

	private string Debug_AchievementStateToString(PlayerAchievementState achieveState)
	{
		AchievementDbfRecord achievementAsset = GameDbf.Achievement.GetRecord(achieveState.AchievementId);
		if (achievementAsset == null)
		{
			return $"id={achieveState.AchievementId} INVALID";
		}
		string completedDateString = "";
		if (IsAchievementComplete(achieveState.AchievementId))
		{
			completedDateString = TimeUtils.UnixTimeStampToDateTimeLocal(achieveState.CompletedDate).ToString();
		}
		return string.Format("id={0} {1} [{2}/{3}] \"{4}\" {5}", achieveState.AchievementId, Enum.GetName(typeof(AchievementStatus), achieveState.Status), achieveState.Progress, achievementAsset.Quota, achievementAsset.Name?.GetString() ?? "<no name>", completedDateString);
	}
}
