using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Jobs;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Core.Streaming;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.Streaming;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class RewardPopups : IDisposable
{
	private static readonly AssetReference POPUP_DISPLAY_MANAGER_BONES = new AssetReference("PopupDisplayManagerBones.prefab:6e45ce09a4f1aab40880b7b446db87fa");

	private static readonly AssetReference POPUP_DISPLAY_MANAGER_BONES_FOR_QUEST_CHESTS = new AssetReference("PopupDisplayManagerBonesForQuestChests.prefab:5daf890336fb048448e4b5f8866e7746");

	private List<Reward> m_rewards = new List<Reward>();

	private List<Reward> m_purchasedCardRewards = new List<Reward>();

	private List<Reward> m_genericRewards = new List<Reward>();

	private readonly Queue<NetCache.ProfileNotice> m_cardReplacementNotices = new Queue<NetCache.ProfileNotice>();

	private readonly HashSet<long> m_genericRewardChestNoticeIdsReady = new HashSet<long>();

	private readonly HashSet<long> m_deckRewardIds = new HashSet<long>();

	private int m_numRewardsToLoad;

	private readonly Dictionary<long, HashSet<int>> m_seenNotices = new Dictionary<long, HashSet<int>>();

	private readonly Queue<NetCache.ProfileNotice> m_dustRewardNotices = new Queue<NetCache.ProfileNotice>();

	private Action OnPopupShown;

	private Action OnPopupClosed;

	private Action<bool> SetIsShowing;

	private static HashSet<Assets.Achieve.RewardTiming> s_timingOutAndImmediate = new HashSet<Assets.Achieve.RewardTiming>
	{
		Assets.Achieve.RewardTiming.OUT_OF_BAND,
		Assets.Achieve.RewardTiming.IMMEDIATE
	};

	private static HashSet<Assets.Achieve.RewardTiming> s_timingImmediate = new HashSet<Assets.Achieve.RewardTiming> { Assets.Achieve.RewardTiming.IMMEDIATE };

	private PopupDisplayManager m_popupDisplayManager;

	public bool IsLoadingRewards => m_numRewardsToLoad > 0;

	private static HashSet<Assets.Achieve.RewardTiming> CurrentRewardTimings
	{
		get
		{
			if (SceneMgr.Get().GetMode() != SceneMgr.Mode.LOGIN)
			{
				return s_timingImmediate;
			}
			return s_timingOutAndImmediate;
		}
	}

	private bool IsShowingPromptInStore
	{
		get
		{
			if (!UserAttentionManager.IsBlockedBy(UserAttentionBlocker.FATAL_ERROR_SCENE))
			{
				return StoreManager.Get().IsPromptShowing;
			}
			return true;
		}
	}

	private PopupDisplayManagerBones ChestBones { get; set; }

	private PopupDisplayManagerBones QuestChestBones { get; set; }

	private event Action<long> OnGenericRewardShown = delegate
	{
	};

	public RewardPopups(PopupDisplayManager popupDisplayManager, Action<bool> setIsShowing, Action onPopupShown, Action onPopupClosed)
	{
		m_popupDisplayManager = popupDisplayManager;
		SetIsShowing = setIsShowing;
		OnPopupShown = onPopupShown;
		OnPopupClosed = onPopupClosed;
		Processor.QueueJob("Load_Popup_Bones", LoadBones());
		AchievementManager.Get().GetRewardPresenter().OnRewardItemQueued += m_popupDisplayManager.OnRewardPresenterScrollQueued;
		QuestManager.Get().GetRewardPresenter().OnRewardItemQueued += m_popupDisplayManager.OnRewardPresenterScrollQueued;
		RewardTrackManager.Get().GetRewardPresenter().OnRewardItemQueued += m_popupDisplayManager.OnRewardPresenterScrollQueued;
		StartupRegistration();
	}

	private void StartupRegistration()
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		GenericRewardChestNoticeManager.Get().RegisterRewardsUpdatedListener(OnGenericRewardUpdated);
		NetCache.Get().RegisterNewNoticesListener(OnNewNotices);
		Network.Get().RegisterNetHandler(GetDeckContentsResponse.PacketID.ID, OnGetDeckContentsResponse);
		GameDownloadManagerProvider.Get().RegisterModuleInstallationStateChangeListener(OnGameModuleStateChanged, invokeImmediately: false);
	}

	public void Dispose()
	{
		HearthstoneApplication.Get().WillReset -= WillReset;
		GenericRewardChestNoticeManager.Get().RemoveRewardsUpdatedListener(OnGenericRewardUpdated, null);
		NetCache.Get().RemoveNewNoticesListener(OnNewNotices);
		Network.Get().RemoveNetHandler(GetDeckContentsResponse.PacketID.ID, OnGetDeckContentsResponse);
		AchievementManager.Get().GetRewardPresenter().OnRewardItemQueued -= m_popupDisplayManager.OnRewardPresenterScrollQueued;
		QuestManager.Get().GetRewardPresenter().OnRewardItemQueued -= m_popupDisplayManager.OnRewardPresenterScrollQueued;
		RewardTrackManager.Get().GetRewardPresenter().OnRewardItemQueued -= m_popupDisplayManager.OnRewardPresenterScrollQueued;
		GameDownloadManagerProvider.Get().UnregisterModuleInstallationStateChangeListener(OnGameModuleStateChanged);
	}

	private void WillReset()
	{
		m_cardReplacementNotices.Clear();
		m_dustRewardNotices.Clear();
		ClearSeenNotices();
	}

	public IEnumerator<IAsyncJobResult> LoadBones()
	{
		InstantiatePrefab loadBones = new InstantiatePrefab(POPUP_DISPLAY_MANAGER_BONES, InstantiatePrefabFlags.UsePrefabPosition | InstantiatePrefabFlags.FailOnError);
		yield return loadBones;
		ChestBones = loadBones.InstantiatedPrefab.GetComponent<PopupDisplayManagerBones>();
		loadBones = new InstantiatePrefab(POPUP_DISPLAY_MANAGER_BONES_FOR_QUEST_CHESTS, InstantiatePrefabFlags.UsePrefabPosition | InstantiatePrefabFlags.FailOnError);
		yield return loadBones;
		QuestChestBones = loadBones.InstantiatedPrefab.GetComponent<PopupDisplayManagerBones>();
	}

	public void RegisterGenericRewardShownListener(Action<long> callback)
	{
		if (callback != null)
		{
			OnGenericRewardShown -= callback;
			OnGenericRewardShown += callback;
		}
	}

	private void OnRewardObjectLoaded(Reward reward, object callbackData)
	{
		LoadReward(reward, ref m_rewards);
	}

	private void OnPurchasedCardRewardObjectLoaded(Reward reward, object callbackData)
	{
		LoadReward(reward, ref m_purchasedCardRewards);
	}

	private void OnGenericRewardObjectLoaded(Reward reward, object callbackData)
	{
		LoadReward(reward, ref m_genericRewards);
	}

	private void OnRewardShown(object callbackData)
	{
		Reward reward = callbackData as Reward;
		if (!(reward == null))
		{
			reward.RegisterClickListener(OnRewardClicked);
			reward.EnableClickCatcher(enabled: true);
		}
	}

	private void ShowChestRewardsWhenReady_DoneCallback()
	{
		SetIsShowing?.Invoke(obj: false);
	}

	private void ShowNextLeaguePromotionReward_DoneCallback()
	{
		m_popupDisplayManager.ShowRankedIntro();
		SetIsShowing?.Invoke(obj: false);
	}

	private void OnRewardClicked(Reward reward, object userData)
	{
		reward.RemoveClickListener(OnRewardClicked);
		reward.Hide(animate: true);
		SetIsShowing?.Invoke(obj: false);
	}

	private void OnGenericRewardUpdated(long rewardNoticeId, object userData)
	{
		m_genericRewardChestNoticeIdsReady.Add(rewardNoticeId);
		if (m_popupDisplayManager.CanShowPopups())
		{
			UpdateRewards(m_popupDisplayManager.AchievementPopups.CompletedAchieves);
		}
	}

	private void OnCollectionManagerUpdatedNetCacheDecks()
	{
		foreach (long deckId in m_deckRewardIds)
		{
			Network.Get().RequestDeckContents(deckId);
		}
		CollectionManager.Get().RemoveOnNetCacheDecksProcessedListener(OnCollectionManagerUpdatedNetCacheDecks);
	}

	private void OnGetDeckContentsResponse()
	{
		List<DeckContents> decksToProcess = new List<DeckContents>();
		foreach (DeckContents deck in Network.Get().GetDeckContentsResponse().Decks)
		{
			if (m_deckRewardIds.Contains(deck.DeckId))
			{
				decksToProcess.Add(deck);
			}
		}
		if (decksToProcess.Count <= 0)
		{
			return;
		}
		OfflineDataCache.OfflineData data = OfflineDataCache.ReadOfflineDataFromFile();
		List<DeckInfo> decks = NetCache.Get().GetDeckListFromNetCache();
		OfflineDataCache.CacheLocalAndOriginalDeckList(ref data, decks, decks);
		foreach (DeckContents deck2 in decksToProcess)
		{
			OfflineDataCache.CacheLocalAndOriginalDeckContents(ref data, deck2, deck2);
			m_deckRewardIds.Remove(deck2.DeckId);
		}
		OfflineDataCache.WriteOfflineDataToFile(data);
	}

	private void OnNewNotices(List<NetCache.ProfileNotice> newNotices, bool isInitialNoticeList)
	{
		if (!m_popupDisplayManager.CanShowPopups() || newNotices.Count <= 0)
		{
			return;
		}
		UpdateRewards(m_popupDisplayManager.AchievementPopups.CompletedAchieves);
		newNotices.ForEach(delegate(NetCache.ProfileNotice notice)
		{
			if (notice.Origin == NetCache.ProfileNotice.NoticeOrigin.CARD_REPLACEMENT)
			{
				m_cardReplacementNotices.Enqueue(notice);
			}
			else if (notice.Origin == NetCache.ProfileNotice.NoticeOrigin.HOF_COMPENSATION && notice.Type == NetCache.ProfileNotice.NoticeType.REWARD_DUST)
			{
				m_dustRewardNotices.Enqueue(notice);
			}
		});
	}

	public bool UpdateNoticesSeen(RewardData rewardData)
	{
		if (!rewardData.HasNotices())
		{
			return true;
		}
		bool hasUnseenNotices = false;
		foreach (long noticeID in rewardData.GetNoticeIDs())
		{
			if (rewardData.Origin == NetCache.ProfileNotice.NoticeOrigin.GENERIC_REWARD_CHEST_ACHIEVE && rewardData.RewardChestBagNum.HasValue)
			{
				if (!m_seenNotices.ContainsKey(noticeID))
				{
					m_seenNotices.Add(noticeID, new HashSet<int>());
				}
				if (m_seenNotices[noticeID].Add(rewardData.RewardChestBagNum.Value))
				{
					hasUnseenNotices = true;
				}
			}
			else if (rewardData.Origin == NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_LUCKY_DRAW)
			{
				if (!m_seenNotices.ContainsKey(noticeID))
				{
					m_seenNotices.Add(noticeID, new HashSet<int>());
				}
				if (m_seenNotices[noticeID].Add((int)rewardData.OriginData))
				{
					hasUnseenNotices = true;
				}
			}
			else if (!m_seenNotices.ContainsKey(noticeID))
			{
				m_seenNotices.Add(noticeID, new HashSet<int>());
				hasUnseenNotices = true;
			}
		}
		return hasUnseenNotices;
	}

	public void ClearSeenNotices()
	{
		m_seenNotices.Clear();
	}

	private void LoadReward(Reward reward, ref List<Reward> allRewards)
	{
		reward.Hide();
		PositionReward(reward);
		allRewards.Add(reward);
		if (Reward.Type.CARD == reward.RewardType && reward is CardReward)
		{
			(reward as CardReward).MakeActorsUnlit();
		}
		LayerUtils.SetLayer(reward.gameObject, GameLayer.Default);
		m_numRewardsToLoad--;
		if (m_numRewardsToLoad <= 0)
		{
			RewardUtils.SortRewards(ref allRewards);
		}
	}

	private void LoadRewards(List<RewardData> rewardsToLoad, Reward.DelOnRewardLoaded callback)
	{
		foreach (RewardData rewardData in rewardsToLoad)
		{
			if (UpdateNoticesSeen(rewardData))
			{
				if (ReturningPlayerMgr.Get().SuppressOldPopups && (rewardData.Origin == NetCache.ProfileNotice.NoticeOrigin.TOURNEY || rewardData.Origin == NetCache.ProfileNotice.NoticeOrigin.TAVERN_BRAWL_REWARD || rewardData.Origin == NetCache.ProfileNotice.NoticeOrigin.LEAGUE_PROMOTION))
				{
					Log.ReturningPlayer.Print("Suppressing popup for Reward {0} due to being a Returning Player!", rewardData);
					rewardData.AcknowledgeNotices();
				}
				else
				{
					m_numRewardsToLoad++;
					rewardData.LoadRewardObject(callback);
				}
			}
		}
	}

	internal bool ShowRewardPopups(List<Achievement> completedAchieves, bool suppressRewardPopups, Func<bool> ShowNextRankedIntro, Func<bool> ShowNextCompletedQuest)
	{
		if (ShowNextTavernBrawlReward())
		{
			return true;
		}
		if (ShowNextLeaguePromotionReward())
		{
			return true;
		}
		if (!PopupDisplayManager.SuppressPopupsForReturningPlayerWarmUp && ShowNextRankedIntro())
		{
			return true;
		}
		if (ShowNextFreeDeckReward())
		{
			return true;
		}
		if (ShowNextSellableDeckReward())
		{
			return true;
		}
		if (ShowNextQuestChestReward())
		{
			return true;
		}
		if (ShowNextProgressionAchievementReward())
		{
			return true;
		}
		if (ShowNextProgressionQuestReward())
		{
			return true;
		}
		if (ShowNextProgressionTrackReward())
		{
			return true;
		}
		if (ShowRewardTrackXpGains())
		{
			return true;
		}
		if (ShowEventEndedPopup())
		{
			return true;
		}
		if (ShowRewardTrackSeasonRoll())
		{
			return true;
		}
		if (completedAchieves.Count > 0 || m_rewards.Count > 0 || m_purchasedCardRewards.Count > 0 || m_genericRewards.Count > 0)
		{
			if (ShowNextCompletedQuest())
			{
				return true;
			}
			if (!suppressRewardPopups && ShowNextUnAckedReward())
			{
				return true;
			}
			if (ShowNextUnAckedGenericReward())
			{
				return true;
			}
			if (!suppressRewardPopups && ShowNextUnAckedPurchasedCardReward())
			{
				return true;
			}
		}
		if (ShowFixedRewards(CurrentRewardTimings))
		{
			return true;
		}
		return IsLoadingRewards;
	}

	private bool ShowNextTavernBrawlReward()
	{
		if (UserAttentionManager.IsBlockedBy(UserAttentionBlocker.FATAL_ERROR_SCENE) || !UserAttentionManager.CanShowAttentionGrabber("PopupDisplayManager.UpdateTavernBrawlRewards"))
		{
			return false;
		}
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.HUB)
		{
			return false;
		}
		NetCache.NetCacheProfileNotices notices = ((NetCache.Get() != null) ? NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>() : null);
		if (notices == null || notices.Notices == null)
		{
			return false;
		}
		NetCache.ProfileNoticeTavernBrawlRewards tavernBrawlRewardNotice = (NetCache.ProfileNoticeTavernBrawlRewards)notices.Notices.Find((NetCache.ProfileNotice obj) => obj.Type == NetCache.ProfileNotice.NoticeType.TAVERN_BRAWL_REWARDS);
		if (tavernBrawlRewardNotice != null)
		{
			Network net = Network.Get();
			if (PopupDisplayManager.ShouldDisableNotificationOnLogin())
			{
				net?.AckNotice(tavernBrawlRewardNotice.NoticeID);
			}
			else if (ReturningPlayerMgr.Get() != null && ReturningPlayerMgr.Get().SuppressOldPopups)
			{
				if (net != null)
				{
					net.AckNotice(tavernBrawlRewardNotice.NoticeID);
					Log.ReturningPlayer.Print("Suppressing popup for TavernBrawlRewardRewards due to being a Returning Player!");
				}
			}
			else
			{
				SetIsShowing?.Invoke(obj: true);
				OnPopupShown?.Invoke();
				Transform rewardChestBone = GetChestRewardBoneForScene();
				if (rewardChestBone == null)
				{
					Log.All.PrintWarning("No bone set for reward chest in scene={0}!", SceneMgr.Get().GetMode());
					return false;
				}
				List<RewardData> rewards = Network.ConvertRewardChest(tavernBrawlRewardNotice.Chest).Rewards;
				RewardUtils.ShowTavernBrawlRewards(tavernBrawlRewardNotice.Wins, rewards, rewardChestBone, ShowChestRewardsWhenReady_DoneCallback, fromNotice: true, tavernBrawlRewardNotice);
			}
			return true;
		}
		return false;
	}

	private bool ShowNextLeaguePromotionReward()
	{
		if (UserAttentionManager.IsBlockedBy(UserAttentionBlocker.FATAL_ERROR_SCENE) || !UserAttentionManager.CanShowAttentionGrabber("PopupDisplayManager.ShowNextLeaguePromotionReward"))
		{
			return false;
		}
		if (LoadingScreen.Get() != null && LoadingScreen.Get().IsTransitioning())
		{
			return false;
		}
		NetCache.NetCacheProfileNotices notices = ((NetCache.Get() != null) ? NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>() : null);
		if (notices == null || notices.Notices == null)
		{
			return false;
		}
		NetCache.ProfileNoticeLeaguePromotionRewards leaguePromotionRewardNotice = (NetCache.ProfileNoticeLeaguePromotionRewards)notices.Notices.Find((NetCache.ProfileNotice obj) => obj.Type == NetCache.ProfileNotice.NoticeType.LEAGUE_PROMOTION_REWARDS);
		if (leaguePromotionRewardNotice != null)
		{
			Network net = Network.Get();
			if (PopupDisplayManager.ShouldDisableNotificationOnLogin())
			{
				net?.AckNotice(leaguePromotionRewardNotice.NoticeID);
			}
			else if (ReturningPlayerMgr.Get() != null && ReturningPlayerMgr.Get().SuppressOldPopups)
			{
				if (net != null)
				{
					net.AckNotice(leaguePromotionRewardNotice.NoticeID);
					Log.ReturningPlayer.Print("Suppressing popup for ProfileNoticeLeaguePromotionRewards due to being a Returning Player!");
				}
			}
			else
			{
				SetIsShowing?.Invoke(obj: true);
				OnPopupShown?.Invoke();
				Transform rewardChestBone = GetChestRewardBoneForScene();
				if (rewardChestBone == null)
				{
					Log.All.PrintWarning("No bone set for reward chest in scene={0}!", SceneMgr.Get().GetMode());
					return false;
				}
				List<RewardData> rewards = Network.ConvertRewardChest(leaguePromotionRewardNotice.Chest).Rewards;
				RewardUtils.ShowLeaguePromotionRewards(leaguePromotionRewardNotice.LeagueId, rewards, rewardChestBone, ShowNextLeaguePromotionReward_DoneCallback, fromNotice: true, leaguePromotionRewardNotice.NoticeID);
			}
			return true;
		}
		return false;
	}

	private bool ShowNextFreeDeckReward()
	{
		if (UserAttentionManager.IsBlockedBy(UserAttentionBlocker.FATAL_ERROR_SCENE) || !UserAttentionManager.CanShowAttentionGrabber("PopupDisplayManager.ShowNextFreeDeckReward"))
		{
			return false;
		}
		NetCache.NetCacheProfileNotices notices = ((NetCache.Get() != null) ? NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>() : null);
		if (notices == null || notices.Notices == null)
		{
			return false;
		}
		int i = 0;
		for (int iMax = notices.Notices.Count; i < iMax; i++)
		{
			NetCache.ProfileNotice profileNotice = notices.Notices[i];
			if (profileNotice.Type == NetCache.ProfileNotice.NoticeType.DECK_GRANTED)
			{
				NetCache.ProfileNoticeDeckGranted deckRewardNotice = (NetCache.ProfileNoticeDeckGranted)profileNotice;
				SetIsShowing?.Invoke(obj: true);
				UpdateOfflineDeckCache();
				DeckRewardData rewardData = RewardUtils.CreateDeckRewardData(0, deckRewardNotice.DeckDbiID, deckRewardNotice.ClassId, null);
				DbfLocValue deckName = GameDbf.Deck.GetRecord((DeckDbfRecord deckRecord) => deckRecord.ID == deckRewardNotice.DeckDbiID).Name;
				ShowDeckRewardToast(deckRewardNotice, rewardData, deckName, GameStrings.Get("GLUE_FREE_DECK_TITLE"), GameStrings.Get("GLUE_FREE_DECK_DESC"));
				Options.Get().SetLong(Option.LAST_CUSTOM_DECK_CHOSEN, deckRewardNotice.PlayerDeckID);
				return true;
			}
		}
		return false;
	}

	private bool ShowNextSellableDeckReward()
	{
		if (UserAttentionManager.IsBlockedBy(UserAttentionBlocker.FATAL_ERROR_SCENE) || !UserAttentionManager.CanShowAttentionGrabber("PopupDisplayManager.ShowNextSellableDeckReward") || StoreManager.Get().IsPromptShowing)
		{
			return false;
		}
		NetCache.NetCacheProfileNotices notices = ((NetCache.Get() != null) ? NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>() : null);
		if (notices == null || notices.Notices == null)
		{
			return false;
		}
		int i = 0;
		for (int iMax = notices.Notices.Count; i < iMax; i++)
		{
			NetCache.ProfileNotice profileNotice = notices.Notices[i];
			if (profileNotice.Type == NetCache.ProfileNotice.NoticeType.SELLABLE_DECK_GRANTED)
			{
				return ShowSellablePopup((NetCache.ProfileNoticeSellableDeckGranted)profileNotice);
			}
		}
		return false;
	}

	private bool ShowSellablePopup(NetCache.ProfileNoticeSellableDeckGranted deckRewardNotice)
	{
		SetIsShowing?.Invoke(obj: true);
		UpdateOfflineDeckCache();
		if (!RewardUtils.TryGetSellableDeck(deckRewardNotice.SellableDeckID, out var sellableDeckDbfRecord))
		{
			return false;
		}
		DeckTemplateDbfRecord deckTemplateDBfRecord = sellableDeckDbfRecord.DeckTemplateRecord;
		int numDecksRewarded = 1;
		int numCardsRewarded = deckTemplateDBfRecord.DeckRecord.Cards.Count;
		string title = GameStrings.Get("GLUE_SELLABLE_DECK_TITLE");
		string description = GameStrings.Format("GLUE_SELLABLE_DECK_DESC", numCardsRewarded, numDecksRewarded);
		DbfLocValue deckName = ((deckRewardNotice.Premium == TAG_PREMIUM.GOLDEN) ? sellableDeckDbfRecord.GoldenName : deckTemplateDBfRecord.DeckRecord.Name);
		DeckRewardData rewardData = RewardUtils.CreateDeckRewardData(deckTemplateDBfRecord.ID, deckTemplateDBfRecord.DeckId, deckTemplateDBfRecord.ClassId, deckName?.GetString() ?? string.Empty);
		ShowDeckRewardToast(deckRewardNotice, rewardData, deckName, title, description);
		RewardUtils.SetNewRewardedDeck(deckRewardNotice.PlayerDeckID);
		return true;
	}

	private bool ShowNextQuestChestReward()
	{
		NetCache.NetCacheProfileNotices notices = ((NetCache.Get() != null) ? NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>() : null);
		if (notices == null || notices.Notices == null)
		{
			return false;
		}
		int i = 0;
		for (int iMax = notices.Notices.Count; i < iMax; i++)
		{
			NetCache.ProfileNotice profileNotice = notices.Notices[i];
			if (profileNotice.Type == NetCache.ProfileNotice.NoticeType.GENERIC_REWARD_CHEST && profileNotice.Origin == NetCache.ProfileNotice.NoticeOrigin.GENERIC_REWARD_CHEST_ACHIEVE)
			{
				return ShowRewardChest((NetCache.ProfileNoticeGenericRewardChest)profileNotice);
			}
		}
		return false;
	}

	private bool ShowRewardChest(NetCache.ProfileNoticeGenericRewardChest rewardChestNotice)
	{
		Network net = Network.Get();
		if (PopupDisplayManager.ShouldDisableNotificationOnLogin() && net != null)
		{
			net.AckNotice(rewardChestNotice.NoticeID);
			return false;
		}
		if (AchieveManager.Get() == null)
		{
			return false;
		}
		Achievement achieve = AchieveManager.Get().GetAchievement((int)rewardChestNotice.OriginData);
		if (!achieve.HasRewardChestVisuals)
		{
			Log.Achievements.PrintError("Achieve id = {0} not properly set up for chest visuals", (int)rewardChestNotice.OriginData);
			return false;
		}
		SetIsShowing?.Invoke(obj: true);
		List<RewardData> rewards = Network.ConvertRewardChest(rewardChestNotice.RewardChest).Rewards;
		RewardUtils.ShowQuestChestReward(achieve.Name, achieve.Description, rewards, GetChestRewardBoneForScene(QuestChestBones), delegate
		{
			if (net != null)
			{
				net.AckNotice(rewardChestNotice.NoticeID);
				OnPopupClosed?.Invoke();
			}
		}, fromNotice: true, achieve.ID, achieve.ChestVisualPrefabPath);
		return true;
	}

	private bool ShowNextProgressionAchievementReward()
	{
		AchievementManager achievementManager = AchievementManager.Get();
		if (achievementManager == null || !achievementManager.HasReward() || !achievementManager.ShowNextReward(OnPopupClosed))
		{
			return false;
		}
		OnPopupShown?.Invoke();
		SetIsShowing?.Invoke(obj: true);
		return true;
	}

	private bool ShowNextProgressionQuestReward()
	{
		QuestManager questManager = QuestManager.Get();
		if (questManager == null || !questManager.HasReward() || !questManager.ShowNextReward(OnPopupClosed))
		{
			return false;
		}
		OnPopupShown?.Invoke();
		SetIsShowing?.Invoke(obj: true);
		return true;
	}

	private bool ShowNextProgressionTrackReward()
	{
		RewardTrackManager rewardTrackManager = RewardTrackManager.Get();
		if (rewardTrackManager == null || !rewardTrackManager.HasReward() || !rewardTrackManager.ShowNextReward(OnPopupClosed))
		{
			return false;
		}
		OnPopupShown?.Invoke();
		SetIsShowing?.Invoke(obj: true);
		return true;
	}

	private bool ShowRewardTrackXpGains()
	{
		RewardXpNotificationManager rewardXpManager = RewardXpNotificationManager.Get();
		if (rewardXpManager == null)
		{
			return false;
		}
		if (!rewardXpManager.HasXpGainsToShow)
		{
			return false;
		}
		rewardXpManager.ShowXpNotificationsImmediate(delegate
		{
			SetIsShowing?.Invoke(obj: false);
		});
		SetIsShowing?.Invoke(obj: true);
		return true;
	}

	private bool ShowEventEndedPopup()
	{
		SpecialEventManager specialEventManager = SpecialEventManager.Get();
		if (specialEventManager == null || !specialEventManager.ShowEventEndedPopup(OnPopupClosed))
		{
			return false;
		}
		OnPopupShown?.Invoke();
		SetIsShowing?.Invoke(obj: true);
		return true;
	}

	private bool ShowRewardTrackSeasonRoll()
	{
		RewardTrackManager rewardTrackManager = RewardTrackManager.Get();
		if (rewardTrackManager == null || !rewardTrackManager.ShowUnclaimedTrackRewardsPopup(OnPopupClosed))
		{
			return false;
		}
		OnPopupShown?.Invoke();
		SetIsShowing?.Invoke(obj: true);
		return true;
	}

	public bool HasUnAckedRewards()
	{
		if (m_rewards.FindAll((Reward reward) => RewardUtils.IsRequiredDataLoadedToShowReward(reward) && RewardUtils.IsRequiredContextForReward(reward)).Count == 0)
		{
			return false;
		}
		return true;
	}

	private bool ShowNextUnAckedReward()
	{
		if (IsShowingPromptInStore)
		{
			return false;
		}
		Reward prunedReward = null;
		int i = 0;
		for (int iMax = m_rewards.Count; i < iMax; i++)
		{
			Reward reward = m_rewards[i];
			if (RewardUtils.IsRequiredDataLoadedToShowReward(reward) && RewardUtils.IsRequiredContextForReward(reward))
			{
				prunedReward = reward;
				m_rewards.RemoveAt(i);
				break;
			}
		}
		if (prunedReward == null)
		{
			return false;
		}
		RewardData rewardData = prunedReward.Data;
		UserAttentionBlocker blocker = RewardUtils.GetUserAttentionBlockerForReward(rewardData.Origin, rewardData.OriginData);
		if (rewardData.ShowQuestToast)
		{
			SetIsShowing?.Invoke(obj: true);
			OnPopupShown();
			RewardUtils.GetTitleAndDescriptionFromReward(prunedReward, out var title, out var description);
			QuestToast.ShowGenericRewardQuestToast(blocker, delegate
			{
				SetIsShowing?.Invoke(obj: false);
			}, rewardData, title, description);
		}
		else if (RewardUtils.ShowReward(blocker, prunedReward, updateCacheValues: false, GetRewardPunchScale(), GetRewardScale(), OnRewardShown, prunedReward))
		{
			SetIsShowing?.Invoke(obj: true);
			OnPopupShown?.Invoke();
		}
		return true;
	}

	private bool ShowNextUnAckedGenericReward()
	{
		if (m_genericRewards.Count == 0 || IsShowingPromptInStore)
		{
			return false;
		}
		SetIsShowing?.Invoke(obj: true);
		OnPopupShown?.Invoke();
		Reward rewardToShow = m_genericRewards[0];
		m_genericRewards.RemoveAt(0);
		UserAttentionBlocker userAttentionBlockerForReward = RewardUtils.GetUserAttentionBlockerForReward(rewardToShow.Data.Origin, rewardToShow.Data.OriginData);
		RewardUtils.GetTitleAndDescriptionFromReward(rewardToShow, out var title, out var description);
		QuestToast.ShowGenericRewardQuestToast(userAttentionBlockerForReward, delegate
		{
			SetIsShowing?.Invoke(obj: false);
		}, rewardToShow.Data, title, description);
		this.OnGenericRewardShown(rewardToShow.Data.OriginData);
		return true;
	}

	private bool ShowNextUnAckedPurchasedCardReward()
	{
		if (m_purchasedCardRewards.Count == 0 || IsShowingPromptInStore)
		{
			return false;
		}
		if (QuestToast.IsQuestActive())
		{
			QuestToast.GetCurrentToast().CloseQuestToast();
		}
		Reward rewardToShow = m_purchasedCardRewards[0];
		UserAttentionBlocker blocker = RewardUtils.GetUserAttentionBlockerForReward(rewardToShow.Data.Origin, rewardToShow.Data.OriginData);
		if (!UserAttentionManager.CanShowAttentionGrabber(blocker, "ShowNextUnAckedPurchasedCardReward"))
		{
			return false;
		}
		m_purchasedCardRewards.RemoveAt(0);
		SetIsShowing?.Invoke(obj: true);
		OnPopupShown?.Invoke();
		RewardUtils.GetTitleAndDescriptionFromReward(rewardToShow, out var title, out var description);
		QuestToast.ShowQuestToastPopup(blocker, delegate
		{
			SetIsShowing?.Invoke(obj: false);
		}, null, rewardToShow.Data, title, description, fullscreenEffects: false, updateCacheValues: false, null);
		return true;
	}

	private bool ShowFixedRewards(HashSet<Assets.Achieve.RewardTiming> rewardTimings)
	{
		FixedRewardsMgr rewards = FixedRewardsMgr.Get();
		if (rewards == null)
		{
			return false;
		}
		if (!rewards.HasRewardsToShow(rewardTimings))
		{
			return false;
		}
		Log.Achievements.Print("PopupDisplayManager: Showing Fixed Rewards");
		if (!rewards.ShowFixedRewards(UserAttentionBlocker.NONE, rewardTimings, delegate
		{
			SetIsShowing?.Invoke(obj: false);
		}, null))
		{
			SetIsShowing?.Invoke(obj: false);
			return false;
		}
		OnPopupShown?.Invoke();
		SetIsShowing?.Invoke(obj: true);
		return true;
	}

	private void ShowDeckRewardToast(NetCache.ProfileNotice profileNotice, DeckRewardData rewardData, DbfLocValue deckName, string displayTitle, string displayDescription)
	{
		QuestToast.ShowFixedRewardQuestToast(UserAttentionBlocker.NONE, delegate
		{
			if (Network.Get() != null)
			{
				SetIsShowing?.Invoke(obj: false);
				Network.Get().AckNotice(profileNotice.NoticeID);
				Network.Get().RenameDeck(profileNotice.OriginData, deckName, playerInitiated: false);
			}
		}, rewardData, displayTitle, displayDescription);
		m_deckRewardIds.Add(profileNotice.OriginData);
	}

	private void UpdateOfflineDeckCache()
	{
		if (NetCache.Get() != null)
		{
			CollectionManager.Get().AddOnNetCacheDecksProcessedListener(OnCollectionManagerUpdatedNetCacheDecks);
			NetCache.Get().RefreshNetObject<NetCache.NetCacheDecks>();
			NetCache.Get().RefreshNetObject<NetCache.NetCacheHeroLevels>();
		}
	}

	private Transform GetChestRewardBoneForScene(PopupDisplayManagerBones boneSet = null)
	{
		PopupDisplayManagerBones bones = ((boneSet != null) ? boneSet : ChestBones);
		switch (SceneMgr.Get().GetMode())
		{
		case SceneMgr.Mode.LOGIN:
		case SceneMgr.Mode.HUB:
			return bones.m_rewardChestBone_Box;
		case SceneMgr.Mode.TOURNAMENT:
			return bones.m_rewardChestBone_PlayMode;
		case SceneMgr.Mode.PACKOPENING:
		case SceneMgr.Mode.LETTUCE_VILLAGE:
		case SceneMgr.Mode.LETTUCE_MAP:
		case SceneMgr.Mode.LETTUCE_PLAY:
			return bones.m_rewardChestBone_PackOpening;
		default:
			return null;
		}
	}

	public void ShowRewardsForAdventureUnlocks(List<AdventureHeroPowerDbfRecord> unlockedHeroPowers, List<AdventureDeckDbfRecord> unlockedDecks, List<AdventureLoadoutTreasuresDbfRecord> unlockedLoadoutTreasures, List<AdventureLoadoutTreasuresDbfRecord> upgradedLoadoutTreasures, Action callback)
	{
		List<RewardData> rewardsToLoad = new List<RewardData>();
		if (unlockedHeroPowers != null)
		{
			foreach (AdventureHeroPowerDbfRecord unlockedHeroPower in unlockedHeroPowers)
			{
				rewardsToLoad.Add(new AdventureHeroPowerRewardData(unlockedHeroPower));
			}
		}
		if (unlockedDecks != null)
		{
			foreach (AdventureDeckDbfRecord unlockedDeck in unlockedDecks)
			{
				rewardsToLoad.Add(new AdventureDeckRewardData(unlockedDeck));
			}
		}
		if (unlockedLoadoutTreasures != null)
		{
			foreach (AdventureLoadoutTreasuresDbfRecord unlockedLoadoutTreasure in unlockedLoadoutTreasures)
			{
				rewardsToLoad.Add(new AdventureLoadoutTreasureRewardData(unlockedLoadoutTreasure, isUpgrade: false));
			}
		}
		if (upgradedLoadoutTreasures != null)
		{
			foreach (AdventureLoadoutTreasuresDbfRecord upgradedLoadoutTreasure in upgradedLoadoutTreasures)
			{
				rewardsToLoad.Add(new AdventureLoadoutTreasureRewardData(upgradedLoadoutTreasure, isUpgrade: true));
			}
		}
		LoadRewards(rewardsToLoad, OnRewardObjectLoaded);
		if (callback != null)
		{
			m_popupDisplayManager.RegisterAllPopupsShownListener(callback);
		}
		m_popupDisplayManager.ReadyToShowPopups();
	}

	public bool DisplayRewardObject(Reward reward, AnimationUtil.DelOnShownWithPunch onShowCallback)
	{
		return DisplayRewardObject(reward, onShowCallback, reward);
	}

	public bool DisplayRewardObject(Reward reward, AnimationUtil.DelOnShownWithPunch onShowCallback, object callbackData)
	{
		reward.Hide();
		PositionReward(reward);
		LayerUtils.SetLayer(reward.gameObject, GameLayer.IgnoreFullScreenEffects);
		return RewardUtils.ShowReward(UserAttentionBlocker.NONE, reward, updateCacheValues: false, GetRewardPunchScale(), GetRewardScale(), onShowCallback, callbackData);
	}

	public void DisplayLoadedRewardObject(Reward reward, object callbackData)
	{
		if (DisplayRewardObject(reward, OnRewardShown))
		{
			SetIsShowing?.Invoke(obj: true);
		}
	}

	public NetCache.ProfileNoticeMercenariesRewards GetNextNonAutoRetireRewardMercenariesRewardToShow()
	{
		return GetNextMercenariesRewardToShow((NetCache.ProfileNoticeMercenariesRewards notice) => notice.RewardType != ProfileNoticeMercenariesRewards.RewardType.REWARD_TYPE_PVE_AUTO_RETIRE);
	}

	public NetCache.ProfileNoticeMercenariesRewards GetNextBonusMercenariesRewardToShow()
	{
		return GetNextMercenariesRewardToShow((NetCache.ProfileNoticeMercenariesRewards notice) => notice.RewardType == ProfileNoticeMercenariesRewards.RewardType.REWARD_TYPE_PVE_BONUS_CHEST);
	}

	public NetCache.ProfileNoticeMercenariesRewards GetNextMercenariesRewardToShow(Predicate<NetCache.ProfileNoticeMercenariesRewards> filter = null)
	{
		return (NetCache.ProfileNoticeMercenariesRewards)NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>().Notices.Find(delegate(NetCache.ProfileNotice obj)
		{
			if (obj.Type != NetCache.ProfileNotice.NoticeType.MERCENARIES_REWARDS)
			{
				return false;
			}
			if (obj.Origin != NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_MERCENARIES)
			{
				return false;
			}
			if (filter != null)
			{
				if (!(obj is NetCache.ProfileNoticeMercenariesRewards obj2))
				{
					return false;
				}
				if (!filter(obj2))
				{
					return false;
				}
			}
			return true;
		});
	}

	public bool HasNonAutoRetireMercenariesRewardsToShow()
	{
		return HasMercenariesRewardsToShow((NetCache.ProfileNoticeMercenariesRewards notice) => notice.RewardType != ProfileNoticeMercenariesRewards.RewardType.REWARD_TYPE_PVE_AUTO_RETIRE);
	}

	public bool HasMercenariesRewardsToShow(Predicate<NetCache.ProfileNoticeMercenariesRewards> filter = null)
	{
		return GetNextMercenariesRewardToShow(filter) != null;
	}

	public bool ShowMercenariesRewards(bool autoOpenChest, NetCache.ProfileNoticeMercenariesRewards rewardNotice, NetCache.ProfileNoticeMercenariesRewards bonusRewardNotice = null, Action doneCallback = null)
	{
		if (rewardNotice != null)
		{
			SetIsShowing?.Invoke(obj: true);
			switch (rewardNotice.RewardType)
			{
			case ProfileNoticeMercenariesRewards.RewardType.REWARD_TYPE_PVE_CONSOLATION:
			{
				RewardListDataModel rewards2 = RewardFactory.CreateMercenaryRewardItemDataModel(rewardNotice.Chest);
				foreach (RewardItemDataModel item in rewards2.Items)
				{
					if (item.ItemType == RewardItemType.MERCENARY_COIN)
					{
						item.MercenaryCoin.NameActive = true;
					}
				}
				RewardUtils.ShowConsolationMercenariesReward(rewardNotice.RewardType, rewards2, GetChestRewardBoneForScene(ChestBones), delegate
				{
					Network.Get().AckNotice(rewardNotice.NoticeID);
					doneCallback?.Invoke();
					OnPopupClosed();
				});
				break;
			}
			case ProfileNoticeMercenariesRewards.RewardType.REWARD_TYPE_PVE_AUTO_RETIRE:
			{
				RewardListDataModel rewards3 = RewardFactory.CreateMercenaryRewardItemDataModel(rewardNotice.Chest);
				foreach (RewardItemDataModel item2 in rewards3.Items)
				{
					if (item2.ItemType == RewardItemType.MERCENARY_COIN)
					{
						item2.MercenaryCoin.NameActive = true;
					}
				}
				RewardUtils.ShowAutoRetireMercenariesReward(rewardNotice.RewardType, rewards3, GetChestRewardBoneForScene(ChestBones), delegate
				{
					Network.Get().AckNotice(rewardNotice.NoticeID);
					doneCallback?.Invoke();
					OnPopupClosed();
				});
				break;
			}
			default:
			{
				List<RewardData> rewards = Network.ConvertRewardChest(rewardNotice.Chest).Rewards;
				List<RewardData> bonusRewards = null;
				if (bonusRewardNotice != null)
				{
					bonusRewards = Network.ConvertRewardChest(bonusRewardNotice.Chest).Rewards;
				}
				RewardUtils.ShowMercenariesChestReward(rewards, bonusRewards, GetChestRewardBoneForScene(ChestBones), delegate
				{
					Network.Get().AckNotice(rewardNotice.NoticeID);
					if (bonusRewardNotice != null)
					{
						Network.Get().AckNotice(bonusRewardNotice.NoticeID);
					}
					doneCallback?.Invoke();
					OnPopupClosed();
				}, autoOpenChest, fromNotice: true, (int)rewardNotice.NoticeID);
				break;
			}
			}
			return true;
		}
		return false;
	}

	private NetCache.ProfileNoticeMercenariesMercenaryFullyUpgraded GetNextMercenaryFullUpgradedToShow()
	{
		return (NetCache.ProfileNoticeMercenariesMercenaryFullyUpgraded)NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>().Notices.Find((NetCache.ProfileNotice obj) => obj.Type == NetCache.ProfileNotice.NoticeType.MERCENARIES_MERC_FULL_UPGRADE);
	}

	public bool ShowMercenariesFullyUpgraded(Action doneCallback = null)
	{
		NetCache.ProfileNoticeMercenariesMercenaryFullyUpgraded upgradeNotice = GetNextMercenaryFullUpgradedToShow();
		if (upgradeNotice != null)
		{
			SetIsShowing?.Invoke(obj: true);
			RewardUtils.ShowMercenaryFullyUpgraded(RewardFactory.CreateFullyUpgradedMercenaryDataModel(upgradeNotice.MercenaryId), GetChestRewardBoneForScene(ChestBones), delegate
			{
				Network.Get().AckNotice(upgradeNotice.NoticeID);
				CollectionManager.Get().GetMercenary(upgradeNotice.MercenaryId).m_isFullyUpgraded = true;
				if (!LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_START))
				{
					LettuceTutorialUtils.FireEvent(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_VISIT_MYTHIC_TREASURE_START, null);
				}
				doneCallback?.Invoke();
				OnPopupClosed();
			});
			return true;
		}
		return false;
	}

	public NetCache.ProfileNoticeMercenariesSeasonRewards GetNextMercenariesSeasonRewardsNotice()
	{
		foreach (NetCache.ProfileNotice notice in NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>().Notices)
		{
			if (notice.Type == NetCache.ProfileNotice.NoticeType.MERCENARIES_SEASON_REWARDS)
			{
				return notice as NetCache.ProfileNoticeMercenariesSeasonRewards;
			}
		}
		return null;
	}

	public bool ShowNextMercenariesSeasonRewards(Action doneCallback = null)
	{
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.LOGIN)
		{
			return false;
		}
		NetCache.ProfileNoticeMercenariesSeasonRewards rewardNotice = GetNextMercenariesSeasonRewardsNotice();
		if (rewardNotice == null)
		{
			return false;
		}
		DialogManager.Get().ShowMercenariesSeasonRewardsDialog(rewardNotice, doneCallback);
		return true;
	}

	private NetCache.ProfileNoticeMercenariesZoneUnlock GetNextMercenariesZoneUnlockToShow()
	{
		return (NetCache.ProfileNoticeMercenariesZoneUnlock)NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>().Notices.Find((NetCache.ProfileNotice obj) => obj.Type == NetCache.ProfileNotice.NoticeType.MERCENARIES_ZONE_UNLOCK);
	}

	public bool ShowMercenariesZoneUnlockPopup(Action onPopupCompleteCallback = null)
	{
		NetCache.ProfileNoticeMercenariesZoneUnlock zoneNotice = GetNextMercenariesZoneUnlockToShow();
		if (zoneNotice != null)
		{
			if (GameDbf.LettuceBountySet.GetRecord(zoneNotice.ZoneId) != null)
			{
				SetIsShowing?.Invoke(obj: true);
				OnPopupShown?.Invoke();
				DialogManager.Get().ShowMercenariesZoneUnlockDialog(zoneNotice.ZoneId, delegate
				{
					Network.Get().AckNotice(zoneNotice.NoticeID);
					SetIsShowing?.Invoke(obj: false);
					onPopupCompleteCallback?.Invoke();
					OnPopupClosed?.Invoke();
				});
				return true;
			}
			Debug.LogError("ShowMercenariesZoneUnlockPopup attempted to show invalid zone unlock with id: " + zoneNotice.ZoneId);
			Network.Get().AckNotice(zoneNotice.NoticeID);
		}
		return false;
	}

	public NetCache.ProfileNoticeMercenariesAbilityUnlock GetNextMercenariesAbilityUnlockReward()
	{
		return (NetCache.ProfileNoticeMercenariesAbilityUnlock)NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>().Notices.Find((NetCache.ProfileNotice obj) => obj.Type == NetCache.ProfileNotice.NoticeType.MERCENARIES_ABILITY_UNLOCK);
	}

	public bool ShowNextMercenariesAbilityUnlockReward(NetCache.ProfileNoticeMercenariesAbilityUnlock rewardNotice = null, Action doneCallback = null)
	{
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.LETTUCE_VILLAGE)
		{
			doneCallback?.Invoke();
			return false;
		}
		if (rewardNotice == null)
		{
			rewardNotice = GetNextMercenariesAbilityUnlockReward();
		}
		if (rewardNotice == null)
		{
			doneCallback?.Invoke();
			return false;
		}
		SetIsShowing?.Invoke(obj: true);
		OnPopupShown?.Invoke();
		RewardUtils.LoadAndDisplayRewards(RewardUtils.GetRewards(new List<NetCache.ProfileNotice> { rewardNotice }), delegate
		{
			Network.Get().AckNotice(rewardNotice.NoticeID);
			SetIsShowing?.Invoke(obj: false);
			doneCallback?.Invoke();
			OnPopupClosed?.Invoke();
			foreach (Reward current in m_rewards)
			{
				long num = 0L;
				List<long> noticeIDs = current.Data.GetNoticeIDs();
				if (noticeIDs != null && noticeIDs.Count > 0)
				{
					num = noticeIDs[0];
				}
				if (num == rewardNotice.NoticeID)
				{
					m_rewards.Remove(current);
					break;
				}
			}
		});
		return true;
	}

	public void UpdateRewards(List<Achievement> completedAchieves)
	{
		NetCache.NetCacheProfileNotices notices = NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>();
		List<RewardData> rewardsToLoad = new List<RewardData>();
		List<RewardData> genericRewardsToLoad = new List<RewardData>();
		List<RewardData> purchasedCardRewardsToLoad = new List<RewardData>();
		if (notices != null)
		{
			AchieveManager.Get();
			List<RewardData> rewardDataList = RewardUtils.GetRewards(notices.Notices.Where(delegate(NetCache.ProfileNotice n)
			{
				if (n.Type == NetCache.ProfileNotice.NoticeType.GENERIC_REWARD_CHEST && n.Origin == NetCache.ProfileNotice.NoticeOrigin.GENERIC_REWARD_CHEST_ACHIEVE)
				{
					Achievement achievement = AchieveManager.Get().GetAchievement((int)n.OriginData);
					if (achievement != null && achievement.HasRewardChestVisuals)
					{
						return false;
					}
				}
				if (n.Type == NetCache.ProfileNotice.NoticeType.GENERIC_REWARD_CHEST && n.Origin == NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_DUELS)
				{
					return false;
				}
				return n.Type != NetCache.ProfileNotice.NoticeType.GENERIC_REWARD_CHEST || m_genericRewardChestNoticeIdsReady.Any((long r) => n.NoticeID == r);
			}).ToList());
			HashSet<Assets.Achieve.RewardTiming> rewardTimings = new HashSet<Assets.Achieve.RewardTiming>();
			foreach (Assets.Achieve.RewardTiming rewardTiming in Enum.GetValues(typeof(Assets.Achieve.RewardTiming)))
			{
				rewardTimings.Add(rewardTiming);
			}
			RewardUtils.GetViewableRewards(rewardDataList, rewardTimings, out rewardsToLoad, out genericRewardsToLoad, ref purchasedCardRewardsToLoad, ref completedAchieves);
		}
		if (ReturningPlayerMgr.Get().SuppressOldPopups)
		{
			List<Achievement> filteredAchieves = new List<Achievement>();
			foreach (Achievement achieve in completedAchieves)
			{
				if (achieve.ShowToReturningPlayer == Assets.Achieve.ShowToReturningPlayer.SUPPRESSED)
				{
					Log.ReturningPlayer.Print("Suppressing popup for Achievement {0} due to being a Returning Player!", achieve);
					achieve.AckCurrentProgressAndRewardNotices();
				}
				else
				{
					filteredAchieves.Add(achieve);
				}
			}
			completedAchieves = filteredAchieves;
			genericRewardsToLoad.RemoveAll(delegate(RewardData rewardData)
			{
				if (!rewardData.RewardChestAssetId.HasValue)
				{
					AckNotices(rewardData);
					return true;
				}
				RewardChestDbfRecord record = GameDbf.RewardChest.GetRecord(rewardData.RewardChestAssetId.Value);
				if (record == null || !record.ShowToReturningPlayer)
				{
					AckNotices(rewardData);
					return true;
				}
				return false;
			});
		}
		if (!PopupDisplayManager.ShouldDisableNotificationOnLogin())
		{
			LoadRewards(rewardsToLoad, OnRewardObjectLoaded);
			LoadRewards(purchasedCardRewardsToLoad, OnPurchasedCardRewardObjectLoaded);
			LoadRewards(genericRewardsToLoad, OnGenericRewardObjectLoaded);
		}
		Log.Achievements.Print("PopupDisplayManager: adding {0} rewards to load total={1}", rewardsToLoad.Count, m_numRewardsToLoad);
		static void AckNotices(RewardData rewardData)
		{
			foreach (long noticeID in rewardData.GetNoticeIDs())
			{
				Network.Get().AckNotice(noticeID);
			}
		}
	}

	private void PositionReward(Reward reward)
	{
		Transform transform = reward.transform;
		transform.parent = ChestBones.transform;
		transform.localRotation = Quaternion.identity;
		transform.localPosition = GetRewardLocalPos(reward);
	}

	private void OnGameModuleStateChanged(DownloadTags.Content moduleTag, ModuleState state)
	{
		if (m_popupDisplayManager.CanShowPopups() && state == ModuleState.FullyInstalled && (moduleTag == DownloadTags.Content.Bgs || moduleTag == DownloadTags.Content.Merc))
		{
			UpdateRewards(m_popupDisplayManager.AchievementPopups.CompletedAchieves);
		}
	}

	public Vector3 GetRewardLocalPos(Reward reward = null)
	{
		switch (SceneMgr.Get().GetMode())
		{
		case SceneMgr.Mode.GAMEPLAY:
			return new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(-7.72f, 8.371922f, -3.883112f),
				Phone = new Vector3(-7.72f, 7.3f, -3.94f)
			};
		case SceneMgr.Mode.LETTUCE_MAP:
		case SceneMgr.Mode.LETTUCE_PLAY:
			return new Vector3(0.1438589f, -7f, 10f);
		case SceneMgr.Mode.LETTUCE_VILLAGE:
			if (reward != null && reward.RewardType == Reward.Type.MERCENARY_EQUIPMENT)
			{
				return ChestBones.m_rewardChestBone_EquipOpening.localPosition;
			}
			break;
		}
		return new Vector3(0.1438589f, 31.27692f, 12.97332f);
	}

	public Vector3 GetRewardScale()
	{
		switch (SceneMgr.Get().GetMode())
		{
		case SceneMgr.Mode.GAMEPLAY:
			return new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = Vector3.one,
				Phone = new Vector3(0.8f, 0.8f, 0.8f)
			};
		case SceneMgr.Mode.ADVENTURE:
		case SceneMgr.Mode.LETTUCE_MAP:
		case SceneMgr.Mode.LETTUCE_PLAY:
			return new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(10f, 10f, 10f),
				Phone = new Vector3(7f, 7f, 7f)
			};
		case SceneMgr.Mode.LETTUCE_VILLAGE:
		case SceneMgr.Mode.LETTUCE_PACK_OPENING:
			return new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(7f, 7f, 7f),
				Phone = new Vector3(5f, 5f, 5f)
			};
		case SceneMgr.Mode.STARTUP:
		case SceneMgr.Mode.LOGIN:
			return new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(15f, 15f, 15f),
				Phone = new Vector3(14f, 14f, 14f)
			};
		case SceneMgr.Mode.PACKOPENING:
			return new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(8f, 8f, 8f),
				Phone = new Vector3(7.5f, 7.5f, 7.5f)
			};
		default:
			return new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(15f, 15f, 15f),
				Phone = new Vector3(8f, 8f, 8f)
			};
		}
	}

	public Vector3 GetRewardPunchScale()
	{
		switch (SceneMgr.Get().GetMode())
		{
		case SceneMgr.Mode.GAMEPLAY:
			return new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(1.2f, 1.2f, 1.2f),
				Phone = new Vector3(1.25f, 1.25f, 1.25f)
			};
		case SceneMgr.Mode.ADVENTURE:
		case SceneMgr.Mode.LETTUCE_MAP:
		case SceneMgr.Mode.LETTUCE_PLAY:
			return new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(10.2f, 10.2f, 10.2f),
				Phone = new Vector3(7.1f, 7.1f, 7.1f)
			};
		case SceneMgr.Mode.LETTUCE_VILLAGE:
		case SceneMgr.Mode.LETTUCE_PACK_OPENING:
			return new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(7.2f, 7.2f, 7.2f),
				Phone = new Vector3(5.1f, 5.1f, 5.1f)
			};
		case SceneMgr.Mode.STARTUP:
		case SceneMgr.Mode.LOGIN:
			return new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(15.1f, 15.1f, 15.1f),
				Phone = new Vector3(14.1f, 14.1f, 14.1f)
			};
		case SceneMgr.Mode.PACKOPENING:
			return new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(8f, 8f, 8f),
				Phone = new Vector3(7.5f, 7.5f, 7.5f)
			};
		default:
			return new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(15.1f, 15.1f, 15.1f),
				Phone = new Vector3(8.1f, 8.1f, 8.1f)
			};
		}
	}
}
