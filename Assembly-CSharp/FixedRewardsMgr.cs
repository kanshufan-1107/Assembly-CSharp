using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using FixedReward;
using Hearthstone;
using PegasusShared;
using UnityEngine;

public class FixedRewardsMgr : IService
{
	public delegate void DelOnAllFixedRewardsShown();

	public delegate void DelPositionNonToastReward(Reward reward);

	private class OnAllFixedRewardsShownCallbackInfo
	{
		public List<RewardMapIDToShow> rewardMapIDsToShow;

		public DelOnAllFixedRewardsShown onAllRewardsShownCallback;

		public DelPositionNonToastReward positionNonToastRewardCallback;

		public bool showingCheatRewards;
	}

	private enum ShowVisualOption
	{
		DO_NOT_SHOW,
		SHOW,
		FORCE_SHOW
	}

	private readonly HashSet<NetCache.CardDefinition> m_craftableCardRewards = new HashSet<NetCache.CardDefinition>();

	private readonly Map<int, MetaAction> m_earnedMetaActionRewards = new Map<int, MetaAction>();

	private readonly RewardQueue m_rewardQueue = new RewardQueue();

	private readonly HashSet<int> m_rewardMapIDsAwarded = new HashSet<int>();

	private bool m_isStartupFinished;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		HearthstoneApplication.Get().Resetting += OnReset;
		serviceLocator.Get<AdventureProgressMgr>().RegisterProgressUpdatedListener(OnAdventureProgressUpdate);
		serviceLocator.Get<NetCache>().RegisterNewNoticesListener(OnNewNotices);
		serviceLocator.Get<AchieveManager>().RegisterAchievesUpdatedListener(OnAchievesUpdated);
		serviceLocator.Get<AccountLicenseMgr>().RegisterAccountLicensesChangedListener(OnAccountLicensesUpdate);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[6]
		{
			typeof(AdventureProgressMgr),
			typeof(NetCache),
			typeof(GameDbf),
			typeof(AchieveManager),
			typeof(AccountLicenseMgr),
			typeof(CardBackManager)
		};
	}

	public void Shutdown()
	{
	}

	private void WillReset()
	{
		m_craftableCardRewards.Clear();
		m_earnedMetaActionRewards.Clear();
		m_rewardQueue.Clear();
		m_rewardMapIDsAwarded.Clear();
		m_isStartupFinished = false;
	}

	private void OnReset()
	{
		ServiceManager.Get<AdventureProgressMgr>().RegisterProgressUpdatedListener(OnAdventureProgressUpdate);
		ServiceManager.Get<AchieveManager>().RegisterAchievesUpdatedListener(OnAchievesUpdated);
	}

	public static FixedRewardsMgr Get()
	{
		return ServiceManager.Get<FixedRewardsMgr>();
	}

	public void InitStartupFixedRewards()
	{
		m_rewardMapIDsAwarded.Clear();
		List<CardRewardData> cardRewards = new List<CardRewardData>();
		foreach (AdventureMission.WingProgress adventureProgress in AdventureProgressMgr.Get().GetAllProgress())
		{
			if (adventureProgress.MeetsFlagsRequirement(1uL))
			{
				TriggerWingProgressAction(ShowVisualOption.DO_NOT_SHOW, adventureProgress.Wing, adventureProgress.Progress, cardRewards);
				TriggerWingFlagsAction(ShowVisualOption.DO_NOT_SHOW, adventureProgress.Wing, adventureProgress.Flags, cardRewards);
			}
		}
		GrantAchieveRewards(cardRewards);
		foreach (AccountLicenseInfo accountLicenseInfo in AccountLicenseMgr.Get().GetAllOwnedAccountLicenseInfo())
		{
			TriggerAccountLicenseFlagsAction(ShowVisualOption.DO_NOT_SHOW, accountLicenseInfo.License, accountLicenseInfo.Flags_, cardRewards);
		}
		m_isStartupFinished = true;
	}

	public bool IsStartupFinished()
	{
		return m_isStartupFinished;
	}

	public bool HasRewardsToShow(IEnumerable<Achieve.RewardTiming> rewardTimings)
	{
		return m_rewardQueue.HasRewardsToShow(rewardTimings);
	}

	public bool ShowFixedRewards(UserAttentionBlocker blocker, HashSet<Achieve.RewardTiming> rewardVisualTimings, DelOnAllFixedRewardsShown allRewardsShownCallback, DelPositionNonToastReward positionNonToastRewardCallback)
	{
		if (UserAttentionManager.IsBlockedBy(UserAttentionBlocker.FATAL_ERROR_SCENE) || !UserAttentionManager.CanShowAttentionGrabber(blocker, $"FixedRewardsMgr.ShowFixedRewards:{blocker}") || StoreManager.Get().IsPromptShowing)
		{
			return false;
		}
		OnAllFixedRewardsShownCallbackInfo callbackInfo = new OnAllFixedRewardsShownCallbackInfo
		{
			rewardMapIDsToShow = new List<RewardMapIDToShow>(),
			onAllRewardsShownCallback = allRewardsShownCallback,
			positionNonToastRewardCallback = positionNonToastRewardCallback,
			showingCheatRewards = false
		};
		foreach (Achieve.RewardTiming rewardVisualTiming in rewardVisualTimings)
		{
			if (!m_rewardQueue.TryGetRewards(rewardVisualTiming, out var rewards))
			{
				continue;
			}
			if (PopupDisplayManager.SuppressPopupsForNewPlayer)
			{
				foreach (RewardMapIDToShow rewardID in rewards)
				{
					FixedRewardMapDbfRecord fixedRewardMapDbf = GameDbf.FixedRewardMap.GetRecord(rewardID.rewardMapID);
					if (fixedRewardMapDbf != null && fixedRewardMapDbf.ActionRecord.Type == FixedRewardAction.Type.HERO_LEVEL)
					{
						callbackInfo.rewardMapIDsToShow.Add(rewardID);
					}
				}
				foreach (RewardMapIDToShow rewardIDToClear in callbackInfo.rewardMapIDsToShow)
				{
					rewards.Remove(rewardIDToClear);
				}
			}
			else
			{
				callbackInfo.rewardMapIDsToShow.AddRange(rewards);
				m_rewardQueue.Clear(rewardVisualTiming);
			}
		}
		if (callbackInfo.rewardMapIDsToShow.Count == 0)
		{
			return false;
		}
		if (PopupDisplayManager.ShouldDisableNotificationOnLogin())
		{
			RewardMapIDToShow rewardMapIDToShow = callbackInfo.rewardMapIDsToShow[0];
			callbackInfo.rewardMapIDsToShow.RemoveAt(0);
			if (rewardMapIDToShow.achieveID != RewardMapIDToShow.NoAchieveID)
			{
				AchieveManager.Get().GetAchievement(rewardMapIDToShow.achieveID)?.AckCurrentProgressAndRewardNotices();
			}
			if (callbackInfo.onAllRewardsShownCallback != null)
			{
				callbackInfo.onAllRewardsShownCallback();
			}
			return false;
		}
		callbackInfo.rewardMapIDsToShow.Sort((RewardMapIDToShow a, RewardMapIDToShow b) => a.sortOrder - b.sortOrder);
		ShowFixedRewards_Internal(blocker, callbackInfo);
		return true;
	}

	public bool Cheat_ShowFixedReward(int fixedRewardMapID, DelPositionNonToastReward positionNonToastRewardCallback)
	{
		if (!HearthstoneApplication.IsInternal())
		{
			return false;
		}
		int sortOrder = GameDbf.FixedRewardMap.GetRecord(fixedRewardMapID)?.SortOrder ?? 0;
		OnAllFixedRewardsShownCallbackInfo callbackInfo = new OnAllFixedRewardsShownCallbackInfo
		{
			rewardMapIDsToShow = new List<RewardMapIDToShow>
			{
				new RewardMapIDToShow(fixedRewardMapID, RewardMapIDToShow.NoAchieveID, sortOrder)
			},
			onAllRewardsShownCallback = null,
			positionNonToastRewardCallback = positionNonToastRewardCallback,
			showingCheatRewards = true
		};
		ShowFixedRewards_Internal(UserAttentionBlocker.NONE, callbackInfo);
		return true;
	}

	public bool CanCraftCard(string cardID, TAG_PREMIUM premium)
	{
		if (GameUtils.GetFixedRewardForCard(cardID, premium) != null)
		{
			NetCache.CardDefinition cardToCheck = new NetCache.CardDefinition
			{
				Name = cardID,
				Premium = premium
			};
			if (m_craftableCardRewards.Contains(cardToCheck))
			{
				return true;
			}
			if (GameUtils.IsCardCraftableWhenWild(cardID) && GameUtils.IsWildCard(cardID))
			{
				return true;
			}
			if (GameUtils.IsClassicCard(cardID))
			{
				List<CounterpartCardsDbfRecord> counterpartCards = GameUtils.GetCounterpartCards(cardID);
				if (counterpartCards != null)
				{
					foreach (CounterpartCardsDbfRecord item in counterpartCards)
					{
						string counterpartCardId = item.DeckEquivalentCardRecord.NoteMiniGuid;
						if (CanCraftCard(counterpartCardId, premium))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		return true;
	}

	private void OnAdventureProgressUpdate(bool isStartupAction, AdventureMission.WingProgress oldProgress, AdventureMission.WingProgress newProgress, object userData)
	{
		List<CardRewardData> cardRewards = new List<CardRewardData>();
		if (isStartupAction || newProgress == null || !newProgress.IsOwned())
		{
			return;
		}
		if (oldProgress == null)
		{
			TriggerWingProgressAction(ShowVisualOption.SHOW, newProgress.Wing, newProgress.Progress, cardRewards);
			TriggerWingFlagsAction(ShowVisualOption.SHOW, newProgress.Wing, newProgress.Flags, cardRewards);
		}
		else
		{
			bool newlyOwned = !oldProgress.IsOwned() && newProgress.IsOwned();
			if (newlyOwned || oldProgress.Progress != newProgress.Progress)
			{
				TriggerWingProgressAction((!newlyOwned) ? ShowVisualOption.SHOW : ShowVisualOption.DO_NOT_SHOW, newProgress.Wing, newProgress.Progress, cardRewards);
			}
			if (oldProgress.Flags != newProgress.Flags)
			{
				TriggerWingFlagsAction(ShowVisualOption.SHOW, newProgress.Wing, newProgress.Flags, cardRewards);
			}
		}
		CollectionManager.Get().AddCardRewards(cardRewards, markAsNew: false);
	}

	private void OnNewNotices(List<NetCache.ProfileNotice> newNotices, bool isInitialNoticeList)
	{
		bool deckRewardFound = false;
		foreach (NetCache.ProfileNotice notice in newNotices)
		{
			if (NetCache.ProfileNotice.NoticeType.HERO_LEVEL_UP == notice.Type)
			{
				NetCache.ProfileNoticeLevelUp levelUpEvent = notice as NetCache.ProfileNoticeLevelUp;
				ShowVisualOption visualOption = ((notice.Origin == NetCache.ProfileNotice.NoticeOrigin.LEVEL_UP) ? ShowVisualOption.SHOW : ShowVisualOption.DO_NOT_SHOW);
				TriggerHeroLevelAction(visualOption, levelUpEvent.HeroClass, levelUpEvent.NewLevel);
				TriggerTotalHeroLevelAction(visualOption, levelUpEvent.TotalLevel);
				Network.Get().AckNotice(notice.NoticeID);
			}
			else if (NetCache.ProfileNotice.NoticeType.DECK_GRANTED == notice.Type)
			{
				deckRewardFound = true;
			}
		}
		if (CollectionManager.Get() != null && deckRewardFound && SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
		}
	}

	private void OnAchievesUpdated(List<Achievement> updatedAchieves, List<Achievement> achieves, object userData)
	{
		List<CardRewardData> cardRewards = new List<CardRewardData>();
		foreach (Achievement achieve in achieves)
		{
			TriggerAchieveAction(ShowVisualOption.SHOW, achieve.ID, cardRewards);
		}
		if (CollectionManager.Get() != null)
		{
			CollectionManager.Get().AddCardRewards(cardRewards, markAsNew: false);
		}
	}

	private void OnAccountLicensesUpdate(List<AccountLicenseInfo> changedAccountLicenses, object userData)
	{
		List<CardRewardData> cardRewards = new List<CardRewardData>();
		foreach (AccountLicenseInfo accountLicense in changedAccountLicenses)
		{
			if (AccountLicenseMgr.Get().OwnsAccountLicense(accountLicense))
			{
				TriggerAccountLicenseFlagsAction(ShowVisualOption.FORCE_SHOW, accountLicense.License, accountLicense.Flags_, cardRewards);
			}
		}
		CollectionManager.Get().AddCardRewards(cardRewards, markAsNew: false);
	}

	private MetaAction GetEarnedMetaActionReward(int metaActionID)
	{
		if (!m_earnedMetaActionRewards.ContainsKey(metaActionID))
		{
			m_earnedMetaActionRewards[metaActionID] = new MetaAction(metaActionID);
		}
		return m_earnedMetaActionRewards[metaActionID];
	}

	private void UpdateEarnedMetaActionFlags(int metaActionID, ulong addFlags, ulong removeFlags)
	{
		GetEarnedMetaActionReward(metaActionID).UpdateFlags(addFlags, removeFlags);
	}

	private bool QueueRewardVisual(FixedRewardMapDbfRecord record, int achieveID)
	{
		Achieve.RewardTiming rewardTiming = record.GetRewardTiming();
		Log.Achievements.Print($"QueueRewardVisual achieveID={achieveID} fixedRewardMapId={record.ID} {record.NoteDesc} {rewardTiming}");
		FixedReward.Reward reward = record.GetFixedReward();
		if (FixedRewardUtils.ShouldSkipRewardVisual(rewardTiming, reward))
		{
			return false;
		}
		m_rewardQueue.Add(rewardTiming, new RewardMapIDToShow(record.ID, achieveID, record.SortOrder));
		return true;
	}

	private void TriggerRewardsForAction(int actionID, ShowVisualOption showRewardVisual, List<CardRewardData> cardRewards)
	{
		TriggerRewardsForAction(actionID, showRewardVisual, cardRewards, RewardMapIDToShow.NoAchieveID);
	}

	private void TriggerRewardsForAction(int actionID, ShowVisualOption showRewardVisual, List<CardRewardData> cardRewards, int achieveID)
	{
		foreach (FixedRewardMapDbfRecord rec in GameUtils.GetFixedRewardMapRecordsForAction(actionID))
		{
			FixedReward.Reward fixedReward = rec.GetFixedReward();
			int rewardMapID = rec.ID;
			if (m_rewardMapIDsAwarded.Contains(rewardMapID))
			{
				if (showRewardVisual != ShowVisualOption.FORCE_SHOW)
				{
					continue;
				}
			}
			else
			{
				m_rewardMapIDsAwarded.Add(rewardMapID);
			}
			if (rec.RewardCount > 0)
			{
				bool show = showRewardVisual != ShowVisualOption.DO_NOT_SHOW;
				if (fixedReward.FixedCardRewardData != null && (!show || !QueueRewardVisual(rec, achieveID)))
				{
					cardRewards.Add(fixedReward.FixedCardRewardData);
				}
				if (fixedReward.FixedCardBackRewardData != null && (!show || !QueueRewardVisual(rec, achieveID)))
				{
					CardBackManager.Get().AddNewCardBack(fixedReward.FixedCardBackRewardData.CardBackID);
				}
				if (fixedReward.FixedCraftableCardRewardData != null)
				{
					m_craftableCardRewards.Add(fixedReward.FixedCraftableCardRewardData);
				}
				if (fixedReward.MetaActionData != null)
				{
					UpdateEarnedMetaActionFlags(fixedReward.MetaActionData.MetaActionID, fixedReward.MetaActionData.MetaActionFlags, 0uL);
					TriggerMetaActionFlagsAction(showRewardVisual, fixedReward.MetaActionData.MetaActionID, cardRewards);
				}
			}
		}
	}

	private void TriggerWingProgressAction(ShowVisualOption showRewardVisual, int wingID, int progress, List<CardRewardData> cardRewards)
	{
		foreach (FixedRewardActionDbfRecord rec in GameUtils.GetFixedActionRecords(FixedRewardAction.Type.WING_PROGRESS))
		{
			if (rec.WingId == wingID && rec.WingProgress <= progress && EventTimingManager.Get().IsEventActive(rec.ActiveEvent))
			{
				TriggerRewardsForAction(rec.ID, showRewardVisual, cardRewards);
			}
		}
	}

	private void TriggerWingFlagsAction(ShowVisualOption showRewardVisual, int wingID, ulong flags, List<CardRewardData> cardRewards)
	{
		foreach (FixedRewardActionDbfRecord rec in GameUtils.GetFixedActionRecords(FixedRewardAction.Type.WING_FLAGS))
		{
			if (rec.WingId == wingID)
			{
				ulong requiredFlags = rec.WingFlags;
				if ((requiredFlags & flags) == requiredFlags && EventTimingManager.Get().IsEventActive(rec.ActiveEvent))
				{
					TriggerRewardsForAction(rec.ID, showRewardVisual, cardRewards);
				}
			}
		}
	}

	private void TriggerAchieveAction(ShowVisualOption showRewardVisual, int achieveId, List<CardRewardData> cardRewards)
	{
		foreach (FixedRewardActionDbfRecord rec in GameUtils.GetFixedActionRecords(FixedRewardAction.Type.ACHIEVE))
		{
			if (rec.AchieveId == achieveId && EventTimingManager.Get().IsEventActive(rec.ActiveEvent))
			{
				TriggerRewardsForAction(rec.ID, showRewardVisual, cardRewards, achieveId);
			}
		}
	}

	private void TriggerTotalHeroLevelAction(ShowVisualOption showRewardVisual, int totalHeroLevel)
	{
		List<CardRewardData> cardRewards = new List<CardRewardData>();
		foreach (FixedRewardActionDbfRecord rec in GameUtils.GetFixedActionRecords(FixedRewardAction.Type.TOTAL_HERO_LEVEL))
		{
			if (rec.TotalHeroLevel == totalHeroLevel && EventTimingManager.Get().IsEventActive(rec.ActiveEvent))
			{
				TriggerRewardsForAction(rec.ID, showRewardVisual, cardRewards);
			}
		}
	}

	private void TriggerHeroLevelAction(ShowVisualOption showRewardVisual, int classID, int heroLevel)
	{
		List<CardRewardData> cardRewards = new List<CardRewardData>();
		foreach (FixedRewardActionDbfRecord rec in GameUtils.GetFixedActionRecords(FixedRewardAction.Type.HERO_LEVEL))
		{
			if (rec.ClassId == classID && rec.HeroLevel == heroLevel && EventTimingManager.Get().IsEventActive(rec.ActiveEvent))
			{
				TriggerRewardsForAction(rec.ID, showRewardVisual, cardRewards);
			}
		}
	}

	private void TriggerAccountLicenseFlagsAction(ShowVisualOption showRewardVisual, long license, ulong flags, List<CardRewardData> cardRewards)
	{
		foreach (FixedRewardActionDbfRecord rec in GameUtils.GetFixedActionRecords(FixedRewardAction.Type.ACCOUNT_LICENSE_FLAGS))
		{
			if (rec.AccountLicenseId == license)
			{
				ulong requiredFlags = rec.AccountLicenseFlags;
				if ((requiredFlags & flags) == requiredFlags && EventTimingManager.Get().IsEventActive(rec.ActiveEvent))
				{
					TriggerRewardsForAction(rec.ID, showRewardVisual, cardRewards);
				}
			}
		}
	}

	private void TriggerMetaActionFlagsAction(ShowVisualOption showRewardVisual, int metaActionID, List<CardRewardData> cardRewards)
	{
		FixedRewardActionDbfRecord metaActionDbfRec = GameDbf.FixedRewardAction.GetRecord(metaActionID);
		if (metaActionDbfRec != null)
		{
			ulong requiredFlags = metaActionDbfRec.MetaActionFlags;
			if (GetEarnedMetaActionReward(metaActionID).HasAllRequiredFlags(requiredFlags) && EventTimingManager.Get().IsEventActive(metaActionDbfRec.ActiveEvent))
			{
				TriggerRewardsForAction(metaActionID, showRewardVisual, cardRewards);
			}
		}
	}

	private void ShowFixedRewards_Internal(UserAttentionBlocker blocker, OnAllFixedRewardsShownCallbackInfo callbackInfo)
	{
		if (callbackInfo.rewardMapIDsToShow.Count == 0)
		{
			if (callbackInfo.onAllRewardsShownCallback != null)
			{
				callbackInfo.onAllRewardsShownCallback();
			}
			return;
		}
		RewardMapIDToShow rewardMapIDToShow = callbackInfo.rewardMapIDsToShow[0];
		callbackInfo.rewardMapIDsToShow.RemoveAt(0);
		FixedRewardMapDbfRecord fixedRewardMapRec = GameDbf.FixedRewardMap.GetRecord(rewardMapIDToShow.rewardMapID);
		FixedReward.Reward fixedReward = fixedRewardMapRec.GetFixedReward();
		RewardData rewardDataToShow = null;
		if (fixedReward.FixedCardRewardData != null)
		{
			rewardDataToShow = fixedReward.FixedCardRewardData;
		}
		else if (fixedReward.FixedCardBackRewardData != null)
		{
			rewardDataToShow = fixedReward.FixedCardBackRewardData;
		}
		else if (fixedReward.FixedRewardData != null)
		{
			rewardDataToShow = fixedReward.FixedRewardData;
		}
		Logger achievements = Log.Achievements;
		int achieveID = rewardMapIDToShow.achieveID;
		achievements.Print("Showing Fixed Reward: " + achieveID);
		if (rewardDataToShow == null)
		{
			ShowFixedRewards_Internal(blocker, callbackInfo);
			return;
		}
		if (callbackInfo.showingCheatRewards)
		{
			rewardDataToShow.MarkAsDummyReward();
		}
		if (rewardMapIDToShow.achieveID != RewardMapIDToShow.NoAchieveID)
		{
			AchieveManager.Get().GetAchievement(rewardMapIDToShow.achieveID)?.AckCurrentProgressAndRewardNotices();
		}
		if (fixedRewardMapRec.SuppressRewardToast)
		{
			ShowFixedRewards_Internal(blocker, callbackInfo);
			return;
		}
		if (fixedRewardMapRec.UseQuestToast)
		{
			string fixedRewardName = fixedRewardMapRec.ToastName;
			string fixedRewardDescription = fixedRewardMapRec.ToastDescription;
			QuestToast.ShowFixedRewardQuestToast(blocker, delegate
			{
				ShowFixedRewards_Internal(blocker, callbackInfo);
			}, rewardDataToShow, fixedRewardName, fixedRewardDescription);
			return;
		}
		rewardDataToShow.LoadRewardObject(delegate(Reward reward, object callbackData)
		{
			reward.transform.localPosition = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(0f, 0f, 43f),
				Phone = new Vector3(0f, 0f, 35f)
			};
			PlatformDependentValue<Vector3> platformDependentValue = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(27.6f, 27.6f, 27.6f),
				Phone = new Vector3(26.4f, 26.4f, 26.4f)
			};
			PlatformDependentValue<Vector3> platformDependentValue2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
			{
				PC = new Vector3(23f, 23f, 23f),
				Phone = new Vector3(22f, 22f, 22f)
			};
			OverlayUI.Get().AddGameObject(reward.gameObject);
			LayerUtils.SetLayer(reward.gameObject, GameLayer.UI);
			if (callbackInfo.positionNonToastRewardCallback != null)
			{
				callbackInfo.positionNonToastRewardCallback(reward);
			}
			bool updateCacheValues = true;
			RewardUtils.ShowReward(blocker, reward, updateCacheValues, platformDependentValue, platformDependentValue2, delegate
			{
				reward.RegisterClickListener(OnNonToastRewardClicked, callbackInfo);
				reward.EnableClickCatcher(enabled: true);
			}, null);
		});
	}

	private void OnNonToastRewardClicked(Reward reward, object userData)
	{
		OnAllFixedRewardsShownCallbackInfo callbackInfo = userData as OnAllFixedRewardsShownCallbackInfo;
		reward.RemoveClickListener(OnNonToastRewardClicked, callbackInfo);
		reward.Hide(animate: true);
		ShowFixedRewards_Internal(UserAttentionBlocker.NONE, callbackInfo);
	}

	private void GrantAchieveRewards(List<CardRewardData> cardRewards)
	{
		AchieveManager achieveManager = AchieveManager.Get();
		if (achieveManager == null)
		{
			Debug.LogWarning("FixedRewardsMgr.GrantAchieveRewards(): null == AchieveManager.Get()");
			return;
		}
		foreach (Achievement achievement in achieveManager.GetCompletedAchieves())
		{
			ShowVisualOption showRewardVisual = (achievement.IsNewlyCompleted() ? ShowVisualOption.SHOW : ShowVisualOption.DO_NOT_SHOW);
			TriggerAchieveAction(showRewardVisual, achievement.ID, cardRewards);
		}
		if (CollectionManager.Get() != null)
		{
			CollectionManager.Get().AddCardRewards(cardRewards, markAsNew: false);
		}
	}

	public RewardData GetNextHeroLevelReward(TAG_CLASS classID, int currentHeroLevel, out int nextRewardLevel)
	{
		List<RewardData> levelRewards = new List<RewardData>();
		List<FixedRewardActionDbfRecord> fixedActionRecords = GameUtils.GetFixedActionRecords(FixedRewardAction.Type.HERO_LEVEL);
		FixedRewardActionDbfRecord nearestRewardActionRecord = null;
		nextRewardLevel = 0;
		int levelsToNextReward = int.MaxValue;
		foreach (FixedRewardActionDbfRecord rewardActionRec in fixedActionRecords)
		{
			if (rewardActionRec.ClassId == (int)classID && EventTimingManager.Get().IsEventActive(rewardActionRec.ActiveEvent) && rewardActionRec.HeroLevel > currentHeroLevel && rewardActionRec.HeroLevel - currentHeroLevel < levelsToNextReward)
			{
				levelsToNextReward = rewardActionRec.HeroLevel - currentHeroLevel;
				nearestRewardActionRecord = rewardActionRec;
			}
		}
		if (nearestRewardActionRecord == null)
		{
			return null;
		}
		foreach (FixedRewardMapDbfRecord rewardMapRec in GameUtils.GetFixedRewardMapRecordsForAction(nearestRewardActionRecord.ID))
		{
			if (rewardMapRec.RewardRecord != null)
			{
				FixedReward.Reward fixedReward = rewardMapRec.GetFixedReward();
				if (fixedReward.FixedCardRewardData != null)
				{
					levelRewards.Add(fixedReward.FixedCardRewardData);
				}
			}
		}
		if (levelRewards.Count == 0)
		{
			Debug.LogFormat("No subsequent reward found for Hero Class: {0} after Level: {1}. Check FIXED REWARD MAPS if you think there should be one", classID.ToString(), currentHeroLevel);
			return null;
		}
		if (levelRewards.Count > 1)
		{
			Debug.LogWarningFormat("More than one reward listed for the subsequent reward for Hero Class: {0} after Level: {1}. Check FIXED REWARD ACTIONS and FIXED REWARD MAPS to ensure there is only one reward per level", classID.ToString(), currentHeroLevel);
		}
		nextRewardLevel = nearestRewardActionRecord.HeroLevel;
		return levelRewards[0];
	}

	public RewardData GetNextTotalLevelReward(int currentTotalLevel, out int nextRewardLevel)
	{
		List<RewardData> levelRewards = new List<RewardData>();
		List<FixedRewardActionDbfRecord> fixedActionRecords = GameUtils.GetFixedActionRecords(FixedRewardAction.Type.TOTAL_HERO_LEVEL);
		FixedRewardActionDbfRecord nearestRewardActionRecord = null;
		nextRewardLevel = 0;
		int levelsToNextReward = int.MaxValue;
		foreach (FixedRewardActionDbfRecord rewardActionRec in fixedActionRecords)
		{
			if (EventTimingManager.Get().IsEventActive(rewardActionRec.ActiveEvent) && rewardActionRec.TotalHeroLevel > currentTotalLevel && rewardActionRec.TotalHeroLevel - currentTotalLevel < levelsToNextReward)
			{
				levelsToNextReward = rewardActionRec.TotalHeroLevel - currentTotalLevel;
				nearestRewardActionRecord = rewardActionRec;
			}
		}
		if (nearestRewardActionRecord == null)
		{
			return null;
		}
		foreach (FixedRewardMapDbfRecord rewardMapRec in GameUtils.GetFixedRewardMapRecordsForAction(nearestRewardActionRecord.ID))
		{
			if (rewardMapRec.RewardRecord != null)
			{
				FixedReward.Reward fixedReward = rewardMapRec.GetFixedReward();
				if (fixedReward.FixedCardRewardData != null)
				{
					levelRewards.Add(fixedReward.FixedCardRewardData);
				}
			}
		}
		if (levelRewards.Count == 0)
		{
			Debug.LogFormat("No subsequent reward found for after Total Level: {0}. Check FIXED REWARD MAPS if you think there should be one", currentTotalLevel);
			return null;
		}
		if (levelRewards.Count > 1)
		{
			Debug.LogErrorFormat("More than one reward listed for the subsequent reward after Total Level: {0}. Check FIXED REWARD ACTIONS and FIXED REWARD MAPS to ensure there is only one reward per level", currentTotalLevel);
		}
		nextRewardLevel = nearestRewardActionRecord.TotalHeroLevel;
		return levelRewards[0];
	}
}
