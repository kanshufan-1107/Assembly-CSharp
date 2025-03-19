using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Progression;
using Hearthstone.Streaming;
using PegasusUtil;
using UnityEngine;

public class AchieveManager : IService, IHasUpdate
{
	public delegate void AchieveCanceledCallback(int achieveID, bool success, object userData);

	private class AchieveCanceledListener : EventListener<AchieveCanceledCallback>
	{
		public void Fire(int achieveID, bool success)
		{
			m_callback(achieveID, success, m_userData);
		}
	}

	public delegate void AchievesUpdatedCallback(List<Achievement> updatedAchieves, List<Achievement> completedAchieves, object userData);

	private class AchievesUpdatedListener : EventListener<AchievesUpdatedCallback>
	{
		public void Fire(List<Achievement> updatedAchieves, List<Achievement> completedAchieves)
		{
			m_callback(updatedAchieves, completedAchieves, m_userData);
		}
	}

	public delegate void LicenseAddedAchievesUpdatedCallback(List<Achievement> activeLicenseAddedAchieves, object userData);

	private class LicenseAddedAchievesUpdatedListener : EventListener<LicenseAddedAchievesUpdatedCallback>
	{
		public void Fire(List<Achievement> activeLicenseAddedAchieves)
		{
			m_callback(activeLicenseAddedAchieves, m_userData);
		}
	}

	private static readonly long TIMED_ACHIEVE_VALIDATION_DELAY_TICKS = 600000000L;

	private static readonly long CHECK_LICENSE_ADDED_ACHIEVE_DELAY_TICKS = 3000000000L;

	private static readonly long TIMED_AND_LICENSE_ACHIEVE_CHECK_DELAY_TICKS = Math.Min(TIMED_ACHIEVE_VALIDATION_DELAY_TICKS, CHECK_LICENSE_ADDED_ACHIEVE_DELAY_TICKS);

	private Map<int, Achievement> m_achievements = new Map<int, Achievement>();

	private bool m_allNetAchievesReceived;

	private int m_numEventResponsesNeeded;

	private HashSet<int> m_achieveValidationsToRequest = new HashSet<int>();

	private HashSet<int> m_achieveValidationsRequested = new HashSet<int>();

	private HashSet<int> m_achievesSeenByPlayerThisSession = new HashSet<int>();

	private bool m_disableCancelButtonUntilServerReturns;

	private Map<int, long> m_lastEventTimingValidationByAchieve = new Map<int, long>();

	private Map<int, long> m_lastCheckLicenseAddedByAchieve = new Map<int, long>();

	private long m_lastEventTimingAndLicenseAchieveCheck;

	private bool m_queueNotifications;

	private List<int> m_achieveNotificationsToQueue = new List<int>();

	private List<AchievementNotification> m_blockedAchievementNotifications = new List<AchievementNotification>();

	private List<AchieveCanceledListener> m_achieveCanceledListeners = new List<AchieveCanceledListener>();

	private List<AchievesUpdatedListener> m_achievesUpdatedListeners = new List<AchievesUpdatedListener>();

	private List<LicenseAddedAchievesUpdatedListener> m_licenseAddedAchievesUpdatedListeners = new List<LicenseAddedAchievesUpdatedListener>();

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		HearthstoneApplication.Get().Resetting += OnReset;
		LoadAchievesFromDBF();
		Network network = serviceLocator.Get<Network>();
		network.RegisterNetHandler(CancelQuestResponse.PacketID.ID, OnQuestCanceled);
		network.RegisterNetHandler(ValidateAchieveResponse.PacketID.ID, OnAchieveValidated);
		network.RegisterNetHandler(TriggerEventResponse.PacketID.ID, OnEventTriggered);
		network.RegisterNetHandler(AccountLicenseAchieveResponse.PacketID.ID, OnAccountLicenseAchieveResponse);
		serviceLocator.Get<NetCache>().RegisterNewNoticesListener(OnNewNotices);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[4]
		{
			typeof(Network),
			typeof(NetCache),
			typeof(GameDbf),
			typeof(EventTimingManager)
		};
	}

	public void Shutdown()
	{
	}

	private void WillReset()
	{
		m_allNetAchievesReceived = false;
		m_achieveValidationsToRequest.Clear();
		m_achieveValidationsRequested.Clear();
		m_achievesUpdatedListeners.Clear();
		m_lastEventTimingValidationByAchieve.Clear();
		m_lastCheckLicenseAddedByAchieve.Clear();
		m_licenseAddedAchievesUpdatedListeners.Clear();
		m_achievements.Clear();
	}

	private void OnReset()
	{
		LoadAchievesFromDBF();
	}

	public static AchieveManager Get()
	{
		return ServiceManager.Get<AchieveManager>();
	}

	public static bool IsPredicateTrue(Assets.Achieve.AltTextPredicate predicate)
	{
		if (predicate == Assets.Achieve.AltTextPredicate.CAN_SEE_WILD && CollectionManager.Get() != null && CollectionManager.Get().ShouldAccountSeeStandardWild())
		{
			return true;
		}
		return false;
	}

	public void InitAchieveManager()
	{
		WillReset();
		LoadAchievesFromDBF();
	}

	public bool IsReady()
	{
		if (!m_allNetAchievesReceived)
		{
			return false;
		}
		if (m_numEventResponsesNeeded > 0)
		{
			return false;
		}
		if (m_achieveValidationsToRequest.Count > 0)
		{
			return false;
		}
		if (m_achieveValidationsRequested.Count > 0)
		{
			return false;
		}
		if (GameUtils.IsTraditionalTutorialComplete() && !GameDownloadManagerProvider.Get().IsReadyToPlay)
		{
			return false;
		}
		if (NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>() == null)
		{
			return false;
		}
		return true;
	}

	public bool RegisterAchievesUpdatedListener(AchievesUpdatedCallback callback, object userData = null)
	{
		if (callback == null)
		{
			return false;
		}
		AchievesUpdatedListener listener = new AchievesUpdatedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_achievesUpdatedListeners.Contains(listener))
		{
			return false;
		}
		m_achievesUpdatedListeners.Add(listener);
		return true;
	}

	public bool RemoveAchievesUpdatedListener(AchievesUpdatedCallback callback)
	{
		return RemoveAchievesUpdatedListener(callback, null);
	}

	public bool RemoveAchievesUpdatedListener(AchievesUpdatedCallback callback, object userData)
	{
		if (callback == null)
		{
			return false;
		}
		AchievesUpdatedListener listener = new AchievesUpdatedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_achievesUpdatedListeners.Contains(listener))
		{
			return false;
		}
		m_achievesUpdatedListeners.Remove(listener);
		return true;
	}

	public List<Achievement> GetNewCompletedAchievesToShow()
	{
		List<Achievement> newlyCompletedQuests = new List<Achievement>();
		QuestManager questManager = QuestManager.Get();
		foreach (KeyValuePair<int, Achievement> achievement in m_achievements)
		{
			Achievement currentAchievment = achievement.Value;
			if (currentAchievment.IsNewlyCompleted() && !currentAchievment.IsInternal() && currentAchievment.RewardTiming != Assets.Achieve.RewardTiming.NEVER)
			{
				Assets.Achieve.Type achieveType = currentAchievment.AchieveType;
				if ((uint)(achieveType - 2) > 1u && achieveType != Assets.Achieve.Type.DAILY_REPEATABLE && !currentAchievment.IsGenericRewardChest && (questManager == null || !questManager.IsProxyLegacyAchieve(currentAchievment.ID)))
				{
					newlyCompletedQuests.Add(currentAchievment);
				}
			}
		}
		return newlyCompletedQuests;
	}

	private static bool IsActiveQuest(Achievement obj, bool onlyNewlyActive)
	{
		if (!obj.Active)
		{
			return false;
		}
		if (!obj.CanShowInQuestLog)
		{
			return false;
		}
		if (onlyNewlyActive)
		{
			return obj.IsNewlyActive();
		}
		return true;
	}

	private static bool IsAutoDestroyQuest(Achievement obj)
	{
		if (!obj.CanShowInQuestLog)
		{
			return false;
		}
		return obj.AutoDestroy;
	}

	private static bool IsDialogQuest(Achievement obj)
	{
		if (!obj.CanShowInQuestLog)
		{
			return false;
		}
		return obj.QuestDialogId != 0;
	}

	public List<Achievement> GetActiveQuests(bool onlyNewlyActive = false)
	{
		List<Achievement> activeQuests = new List<Achievement>();
		foreach (KeyValuePair<int, Achievement> achievement2 in m_achievements)
		{
			Achievement achievement = achievement2.Value;
			if (IsActiveQuest(achievement, onlyNewlyActive))
			{
				activeQuests.Add(achievement);
			}
		}
		return activeQuests;
	}

	public bool HasQuestsToShow(bool onlyNewlyActive = false)
	{
		bool hasQuests = false;
		foreach (KeyValuePair<int, Achievement> item in m_achievements)
		{
			if (IsActiveQuest(item.Value, onlyNewlyActive: false) && (item.Value.IsNewlyActive() || item.Value.AutoDestroy))
			{
				hasQuests = true;
				break;
			}
		}
		return hasQuests;
	}

	public bool MarkQuestAsSeenByPlayerThisSession(Achievement obj)
	{
		return m_achievesSeenByPlayerThisSession.Add(obj.ID);
	}

	public bool ResetQuestSeenByPlayerThisSession(Achievement obj)
	{
		return m_achievesSeenByPlayerThisSession.Remove(obj.ID);
	}

	public bool HasActiveAutoDestroyQuests()
	{
		foreach (KeyValuePair<int, Achievement> kvp in m_achievements)
		{
			if (IsActiveQuest(kvp.Value, onlyNewlyActive: false) && IsAutoDestroyQuest(kvp.Value))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasActiveUnseenWelcomeQuestDialog()
	{
		int lastSeen = Options.Get().GetInt(Option.LATEST_SEEN_WELCOME_QUEST_DIALOG);
		foreach (KeyValuePair<int, Achievement> achievement2 in m_achievements)
		{
			Achievement achievement = achievement2.Value;
			if (IsActiveQuest(achievement, onlyNewlyActive: false) && IsDialogQuest(achievement) && lastSeen != achievement.ID)
			{
				return true;
			}
		}
		return false;
	}

	public List<Achievement> GetNewlyProgressedQuests()
	{
		return Get().GetActiveQuests().FindAll((Achievement obj) => obj.AcknowledgedProgress < obj.Progress && obj.Progress > 0 && obj.Progress < obj.MaxProgress);
	}

	public bool HasUnlockedFeature(Assets.Achieve.Unlocks feature)
	{
		if (DemoMgr.Get().ArenaIs1WinMode() && feature == Assets.Achieve.Unlocks.FORGE)
		{
			return true;
		}
		Achievement achieve = null;
		foreach (KeyValuePair<int, Achievement> kvp in m_achievements)
		{
			if (kvp.Value.UnlockedFeature == feature)
			{
				achieve = kvp.Value;
				break;
			}
		}
		if (achieve == null)
		{
			Debug.LogWarning($"AchieveManager.HasUnlockedFeature(): could not find achieve that unlocks feature {feature}");
			return false;
		}
		return achieve.IsCompleted();
	}

	public Achievement GetAchievement(int achieveID)
	{
		if (!m_achievements.ContainsKey(achieveID))
		{
			return null;
		}
		return m_achievements[achieveID];
	}

	public IEnumerable<Achievement> GetCompletedAchieves()
	{
		return GetAchieves((Achievement a) => a.IsCompleted());
	}

	public List<Achievement> GetAchievesInGroup(Assets.Achieve.Type achieveGroup)
	{
		return new List<Achievement>(m_achievements.Values).FindAll((Achievement obj) => obj.AchieveType == achieveGroup);
	}

	public List<Achievement> GetAchievesInGroup(Assets.Achieve.Type achieveGroup, bool isComplete)
	{
		return GetAchievesInGroup(achieveGroup).FindAll((Achievement obj) => obj.IsCompleted() == isComplete);
	}

	public List<Achievement> GetAchievesForAdventureWing(int wingID)
	{
		return new List<Achievement>(m_achievements.Values).FindAll((Achievement obj) => obj.Enabled && obj.WingID == wingID);
	}

	public List<Achievement> GetAchievesForAdventureAndMode(int adventureId, int modeId)
	{
		return new List<Achievement>(m_achievements.Values).FindAll((Achievement obj) => obj.AdventureID == adventureId && obj.AdventureModeID == modeId);
	}

	public bool CanCancelQuest(int achieveID)
	{
		if (m_disableCancelButtonUntilServerReturns)
		{
			return false;
		}
		if (!CanCancelQuestNow())
		{
			return false;
		}
		if (!HasAccessToDailies())
		{
			return false;
		}
		Achievement achieve = GetAchievement(achieveID);
		if (achieve == null)
		{
			return false;
		}
		if (!achieve.CanBeCancelled)
		{
			return false;
		}
		return achieve.Active;
	}

	public static bool HasAccessToDailies()
	{
		if (!Get().HasUnlockedFeature(Assets.Achieve.Unlocks.DAILY))
		{
			return false;
		}
		return true;
	}

	public bool RegisterQuestCanceledListener(AchieveCanceledCallback callback)
	{
		return RegisterQuestCanceledListener(callback, null);
	}

	public bool RegisterQuestCanceledListener(AchieveCanceledCallback callback, object userData)
	{
		AchieveCanceledListener listener = new AchieveCanceledListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_achieveCanceledListeners.Contains(listener))
		{
			return false;
		}
		m_achieveCanceledListeners.Add(listener);
		return true;
	}

	public bool RemoveQuestCanceledListener(AchieveCanceledCallback callback)
	{
		return RemoveQuestCanceledListener(callback, null);
	}

	public bool RemoveQuestCanceledListener(AchieveCanceledCallback callback, object userData)
	{
		AchieveCanceledListener listener = new AchieveCanceledListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_achieveCanceledListeners.Remove(listener);
	}

	public void CancelQuest(int achieveID)
	{
		if (!CanCancelQuest(achieveID))
		{
			FireAchieveCanceledEvent(achieveID, success: false);
			return;
		}
		BlockAllNotifications();
		m_disableCancelButtonUntilServerReturns = true;
		Network.Get().RequestCancelQuest(achieveID);
	}

	public bool RegisterLicenseAddedAchievesUpdatedListener(LicenseAddedAchievesUpdatedCallback callback)
	{
		return RegisterLicenseAddedAchievesUpdatedListener(callback, null);
	}

	public bool RegisterLicenseAddedAchievesUpdatedListener(LicenseAddedAchievesUpdatedCallback callback, object userData)
	{
		LicenseAddedAchievesUpdatedListener listener = new LicenseAddedAchievesUpdatedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_licenseAddedAchievesUpdatedListeners.Contains(listener))
		{
			return false;
		}
		m_licenseAddedAchievesUpdatedListeners.Add(listener);
		return true;
	}

	public bool RemoveLicenseAddedAchievesUpdatedListener(LicenseAddedAchievesUpdatedCallback callback)
	{
		return RemoveLicenseAddedAchievesUpdatedListener(callback, null);
	}

	public bool RemoveLicenseAddedAchievesUpdatedListener(LicenseAddedAchievesUpdatedCallback callback, object userData)
	{
		LicenseAddedAchievesUpdatedListener listener = new LicenseAddedAchievesUpdatedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_licenseAddedAchievesUpdatedListeners.Remove(listener);
	}

	public bool HasActiveLicenseAddedAchieves()
	{
		return GetActiveLicenseAddedAchieves().Count > 0;
	}

	public bool HasActiveLicenseForAdventure(AdventureDbId adventureId)
	{
		List<Achievement> achievements = GetActiveLicenseAddedAchieves();
		for (int i = 0; i < achievements.Count; i++)
		{
			if (achievements[i].AdventureID == (int)adventureId)
			{
				return true;
			}
		}
		return false;
	}

	public void NotifyOfClick(Achievement.ClickTriggerType clickType)
	{
		Log.Achievements.Print("AchieveManager.NotifyOfClick(): clickType {0}", clickType);
		bool hasAllVanillaHeroes = GameModeUtils.HasUnlockedAllDefaultHeroes();
		foreach (Achievement achieve in GetAchieves(delegate(Achievement obj)
		{
			if (obj.AchieveTrigger != Assets.Achieve.Trigger.CLICK)
			{
				return false;
			}
			if (!obj.Enabled)
			{
				Log.Achievements.Print("AchieveManager.NotifyOfClick(): skip disabled achieve {0}", obj.ID);
				return false;
			}
			if (obj.IsCompleted())
			{
				Log.Achievements.Print("AchieveManager.NotifyOfClick(): skip already completed achieve {0}", obj.ID);
				return false;
			}
			if (!obj.ClickType.HasValue)
			{
				Log.Achievements.Print("AchieveManager.NotifyOfClick(): skip missing ClickType achieve {0}", obj.ID);
				return false;
			}
			if (obj.ClickType.Value != clickType)
			{
				Log.Achievements.Print("AchieveManager.NotifyOfClick(): skip achieve {0} with non-matching ClickType {1}", obj.ID, obj.ClickType.Value);
				return false;
			}
			if (clickType == Achievement.ClickTriggerType.BUTTON_ADVENTURE && !hasAllVanillaHeroes && AdventureUtils.DoesAdventureRequireAllHeroesUnlocked((AdventureDbId)obj.AdventureID))
			{
				Log.Achievements.Print("AchieveManager.NotifyOfClick(): skip achieve {0} for BUTTON_ADVENTURE requiring all heroes unlocked", obj.ID);
				return false;
			}
			return true;
		}))
		{
			Log.Achievements.Print("AchieveManager.NotifyOfClick(): add achieve {0}", achieve.ID);
			m_achieveValidationsToRequest.Add(achieve.ID);
		}
		ValidateAchievesNow();
	}

	public void CompleteAutoDestroyAchieve(int achieveId)
	{
		foreach (Achievement achieve in GetAchieves(delegate(Achievement obj)
		{
			if (obj.IsCompleted())
			{
				return false;
			}
			if (!obj.Enabled)
			{
				return false;
			}
			return obj.Active && obj.AchieveTrigger == Assets.Achieve.Trigger.DESTROYED;
		}))
		{
			if (achieve.ID == achieveId)
			{
				m_achieveValidationsToRequest.Add(achieve.ID);
			}
		}
		ValidateAchievesNow();
	}

	public void NotifyOfAccountCreation()
	{
		foreach (Achievement achieve in GetAchieves(delegate(Achievement obj)
		{
			if (obj.IsCompleted())
			{
				return false;
			}
			return obj.Enabled && obj.AchieveTrigger == Assets.Achieve.Trigger.ACCOUNT_CREATED;
		}))
		{
			m_achieveValidationsToRequest.Add(achieve.ID);
		}
		ValidateAchievesNow();
	}

	public void NotifyOfPacksReadyToOpen(UnopenedPack unopenedPack)
	{
		IEnumerable<Achievement> achieves = GetAchieves(delegate(Achievement obj)
		{
			if (!obj.Enabled)
			{
				return false;
			}
			if (obj.IsCompleted())
			{
				return false;
			}
			if (obj.AchieveTrigger != Assets.Achieve.Trigger.PACK_READY_TO_OPEN)
			{
				return false;
			}
			if (obj.BoosterRequirement != unopenedPack.GetBoosterId())
			{
				return false;
			}
			if (unopenedPack.GetCount() == 0)
			{
				return false;
			}
			return unopenedPack.CanOpenPack() ? true : false;
		});
		bool hasAny = false;
		foreach (Achievement achieve in achieves)
		{
			m_achieveValidationsToRequest.Add(achieve.ID);
			hasAny = true;
		}
		if (hasAny)
		{
			ValidateAchievesNow();
		}
	}

	public void Update()
	{
		if (Network.IsRunning())
		{
			CheckTimedEventsAndLicenses(DateTime.UtcNow);
		}
	}

	public void ValidateAchievesNow()
	{
		if (m_achieveValidationsToRequest.Count == 0)
		{
			return;
		}
		EventTimingManager eventTimingManager = EventTimingManager.Get();
		List<AchieveRegionDataDbfRecord> regionDataDbfRecords = GameDbf.AchieveRegionData.GetRecords();
		foreach (int achieveID in m_achieveValidationsToRequest)
		{
			AchieveRegionDataDbfRecord regionData = null;
			foreach (AchieveRegionDataDbfRecord record in regionDataDbfRecords)
			{
				if (record.AchieveId == achieveID && !eventTimingManager.IsEventActive(record.ProgressableEvent))
				{
					regionData = record;
					break;
				}
			}
			if (regionData != null && !eventTimingManager.IsEventActive(regionData.ProgressableEvent))
			{
				Log.Achievements.Print("AchieveManager.ValidateAchievesNow(): skip non-progressable achieve {0} event {1}", achieveID, regionData.ProgressableEvent);
			}
			else
			{
				Log.Achievements.Print("AchieveManager.ValidateAchievesNow(): ValidateAchieve {0}", achieveID);
				m_achieveValidationsRequested.Add(achieveID);
				Network.Get().ValidateAchieve(achieveID);
			}
		}
		m_achieveValidationsToRequest.Clear();
	}

	public void LoadAchievesFromDBF()
	{
		m_achievements.Clear();
		List<AchieveDbfRecord> achieveRecords = GameDbf.Achieve.GetRecords();
		List<CharacterDialogDbfRecord> questDialogRecords = GameDbf.CharacterDialog.GetRecords();
		Map<int, int> achieveToParentMap = new Map<int, int>();
		foreach (AchieveDbfRecord achieveRecord in achieveRecords)
		{
			int id = achieveRecord.ID;
			int dbfRace = achieveRecord.Race;
			TAG_RACE? raceReq = null;
			if (dbfRace != 0)
			{
				raceReq = (TAG_RACE)dbfRace;
			}
			int dbfCardSet = achieveRecord.CardSet;
			TAG_CARD_SET? cardSetReq = null;
			if (dbfCardSet != 0)
			{
				cardSetReq = (TAG_CARD_SET)dbfCardSet;
			}
			int myHeroClassId = achieveRecord.MyHeroClassId;
			TAG_CLASS? myHeroClassReq = null;
			if (myHeroClassId != 0)
			{
				myHeroClassReq = (TAG_CLASS)myHeroClassId;
			}
			long dbfRewardData1 = achieveRecord.RewardData1;
			long dbfRewardData2 = achieveRecord.RewardData2;
			bool isGenericRewardChest = false;
			string chestVisualPrefabPath = "";
			List<RewardData> rewards = new List<RewardData>();
			TAG_CLASS? classReward = null;
			switch (achieveRecord.Reward)
			{
			case "basic":
				Debug.LogWarning($"AchieveManager.LoadAchievesFromFile(): unable to define reward {achieveRecord.Reward} for achieve {id}");
				break;
			case "card":
			{
				string cardId2 = GameUtils.TranslateDbIdToCardId((int)dbfRewardData1);
				TAG_PREMIUM premium2 = (TAG_PREMIUM)dbfRewardData2;
				rewards.Add(new CardRewardData(cardId2, premium2, 1));
				break;
			}
			case "card2x":
			{
				string cardId = GameUtils.TranslateDbIdToCardId((int)dbfRewardData1);
				TAG_PREMIUM premium = (TAG_PREMIUM)dbfRewardData2;
				rewards.Add(new CardRewardData(cardId, premium, 2));
				break;
			}
			case "cardback":
				rewards.Add(new CardBackRewardData((int)dbfRewardData1));
				break;
			case "dust":
				rewards.Add(new ArcaneDustRewardData((int)dbfRewardData1));
				break;
			case "forge":
				rewards.Add(new ForgeTicketRewardData((int)dbfRewardData1));
				break;
			case "gold":
				rewards.Add(new GoldRewardData((int)dbfRewardData1));
				break;
			case "goldhero":
			{
				string cardId3 = GameUtils.TranslateDbIdToCardId((int)dbfRewardData1);
				TAG_PREMIUM premium3 = (TAG_PREMIUM)dbfRewardData2;
				rewards.Add(new CardRewardData(cardId3, premium3, 1));
				break;
			}
			case "hero":
			{
				classReward = (TAG_CLASS)dbfRewardData2;
				string heroCardId = CollectionManager.GetVanillaHero(classReward.Value);
				if (!string.IsNullOrEmpty(heroCardId))
				{
					rewards.Add(new CardRewardData(heroCardId, TAG_PREMIUM.NORMAL, 1));
				}
				break;
			}
			case "mount":
				rewards.Add(new MountRewardData((MountRewardData.MountType)dbfRewardData1));
				break;
			case "pack":
			{
				int boosterId = (int)((dbfRewardData2 <= 0) ? 1 : dbfRewardData2);
				rewards.Add(new BoosterPackRewardData(boosterId, (int)dbfRewardData1));
				break;
			}
			case "event_notice":
			{
				int eventType = (int)((dbfRewardData1 > 0) ? dbfRewardData1 : 0);
				rewards.Add(new EventRewardData(eventType));
				break;
			}
			case "generic_reward_chest":
				isGenericRewardChest = true;
				rewards.AddRange(RewardUtils.GetRewardDataFromRewardChestAsset((int)dbfRewardData1, (int)dbfRewardData2));
				chestVisualPrefabPath = GameDbf.RewardChest.GetRecord((int)dbfRewardData1).ChestPrefab;
				break;
			case "arcane_orbs":
				rewards.Add(RewardUtils.CreateArcaneOrbRewardData((int)dbfRewardData1));
				break;
			case "deck":
				rewards.Add(RewardUtils.CreateDeckRewardData(0, (int)dbfRewardData1, (int)dbfRewardData2, null));
				break;
			case "mercenary":
				rewards.Add(RewardUtils.CreateMercenaryRewardData((int)dbfRewardData1, 0, TAG_PREMIUM.NORMAL));
				break;
			case "mercenary_coins":
				rewards.Add(RewardUtils.CreateMercenaryCoinsRewardData((int)dbfRewardData1, (int)dbfRewardData2, glowActive: true, nameActive: false));
				break;
			case "renown":
				rewards.Add(new MercenaryRenownRewardData((int)dbfRewardData1));
				break;
			}
			Assets.Achieve.RewardTiming rewardTiming = achieveRecord.RewardTiming;
			int parentID = 0;
			int linkToId = 0;
			string parentAch = achieveRecord.ParentAch;
			string linkTo = achieveRecord.LinkTo;
			int i = 0;
			for (int iMax = achieveRecords.Count; i < iMax; i++)
			{
				string noteDesc = achieveRecords[i].NoteDesc;
				if (parentID == 0 && noteDesc == parentAch)
				{
					parentID = achieveRecords[i].ID;
				}
				if (linkToId == 0 && noteDesc == linkTo)
				{
					linkToId = achieveRecords[i].ID;
				}
				if (parentID != 0 && linkToId != 0)
				{
					break;
				}
			}
			achieveToParentMap[id] = parentID;
			Achievement.ClickTriggerType? clickTrigger = null;
			if (achieveRecord.Triggered == Assets.Achieve.Trigger.CLICK)
			{
				clickTrigger = (Achievement.ClickTriggerType)dbfRewardData1;
			}
			if (id == 94)
			{
				clickTrigger = Achievement.ClickTriggerType.BUTTON_ARENA;
			}
			List<int> scenarios = new List<int>();
			List<AchieveConditionDbfRecord> achieveConditionDbfRecords = GameDbf.AchieveCondition.GetRecords();
			int j = 0;
			for (int iMax2 = achieveConditionDbfRecords.Count; j < iMax2; j++)
			{
				AchieveConditionDbfRecord achieveConditionDbfRecord = achieveConditionDbfRecords[j];
				if (achieveConditionDbfRecord.AchieveId == id)
				{
					scenarios.Add(achieveConditionDbfRecord.ScenarioId);
				}
			}
			CharacterDialogDbfRecord questDialogRecord = null;
			int recordQuestDialogId = achieveRecord.QuestDialogId;
			int k = 0;
			for (int iMax3 = questDialogRecords.Count; k < iMax3; k++)
			{
				if (questDialogRecords[k].ID == recordQuestDialogId)
				{
					questDialogRecord = questDialogRecords[k];
					break;
				}
			}
			int questDialogId = questDialogRecord?.ID ?? 0;
			CharacterDialogSequence onReceivedDialogSequence = null;
			CharacterDialogSequence onCompleteDialogSequence = null;
			CharacterDialogSequence onProgress1DialogSequence = null;
			CharacterDialogSequence onProgress2DialogSequence = null;
			CharacterDialogSequence onDismissDialogSequence = null;
			if (questDialogRecord != null)
			{
				onReceivedDialogSequence = new CharacterDialogSequence(questDialogId, CharacterDialogEventType.RECEIVE);
				onCompleteDialogSequence = new CharacterDialogSequence(questDialogId, CharacterDialogEventType.COMPLETE);
				onProgress1DialogSequence = new CharacterDialogSequence(questDialogId, CharacterDialogEventType.PROGRESS1);
				onProgress2DialogSequence = new CharacterDialogSequence(questDialogId, CharacterDialogEventType.PROGRESS2);
				onDismissDialogSequence = new CharacterDialogSequence(questDialogId, CharacterDialogEventType.DISMISS);
			}
			int onCompleteQuestDialogBannerId = questDialogRecord?.OnCompleteBannerId ?? 0;
			Achievement achievement = new Achievement(achieveRecord, id, achieveRecord.AchType, achieveRecord.AchQuota, linkToId, achieveRecord.Triggered, achieveRecord.GameMode, raceReq, classReward, cardSetReq, myHeroClassReq, clickTrigger, achieveRecord.Unlocks, rewards, scenarios, achieveRecord.AdventureWingId, achieveRecord.AdventureId, achieveRecord.AdventureModeId, rewardTiming, achieveRecord.Booster, achieveRecord.UseGenericRewardVisual, achieveRecord.ShowToReturningPlayer, questDialogId, achieveRecord.AutoDestroy, achieveRecord.QuestTilePrefab, onCompleteQuestDialogBannerId, onReceivedDialogSequence, onCompleteDialogSequence, onProgress1DialogSequence, onProgress2DialogSequence, onDismissDialogSequence, isGenericRewardChest, chestVisualPrefabPath, achieveRecord.CustomVisualWidget, achieveRecord.EnemyHeroClassId);
			EventTimingType eventTrigger = EventTimingType.IGNORE;
			Assets.Achieve.Trigger triggered = achieveRecord.Triggered;
			if (triggered == Assets.Achieve.Trigger.FINISH || triggered == Assets.Achieve.Trigger.EVENT_TIMING_ONLY)
			{
				AchieveRegionDataDbfRecord regionData = achievement.GetCurrentRegionData();
				if (regionData != null)
				{
					eventTrigger = regionData.ProgressableEvent;
				}
			}
			achievement.SetEventTrigger(eventTrigger);
			achievement.SetClientFlags(achieveRecord.ClientFlags);
			achievement.SetAltTextPredicate(achieveRecord.AltTextPredicate);
			achievement.SetName(achieveRecord.Name, achieveRecord.AltName);
			achievement.SetDescription(achieveRecord.Description, achieveRecord.AltDescription);
			InitAchievement(achievement);
		}
	}

	private void InitAchievement(Achievement achievement)
	{
		if (m_achievements.ContainsKey(achievement.ID))
		{
			Debug.LogWarning($"AchieveManager.InitAchievement() - already registered achievement with ID {achievement.ID}");
		}
		else
		{
			m_achievements.Add(achievement.ID, achievement);
		}
	}

	private IEnumerable<Achievement> GetAchieves(Func<Achievement, bool> filter = null)
	{
		List<Achievement> results = new List<Achievement>();
		foreach (KeyValuePair<int, Achievement> kvp in m_achievements)
		{
			if (filter == null || filter(kvp.Value))
			{
				results.Add(kvp.Value);
			}
		}
		return results;
	}

	public void OnInitialAchievements(Achieves achievements)
	{
		if (achievements != null)
		{
			OnAllAchieves(achievements);
		}
	}

	private void OnAllAchieves(Achieves allAchievesList)
	{
		foreach (PegasusUtil.Achieve netAchieve in allAchievesList.List)
		{
			Achievement achievement = GetAchievement(netAchieve.Id);
			if (achievement != null)
			{
				achievement.OnAchieveData(netAchieve);
				if (achievement.IsCompleted())
				{
					SetSteamAchievementIfNeeded(achievement.DbfRecord);
				}
			}
		}
		CheckAllCardGainAchieves();
		m_allNetAchievesReceived = true;
		UnblockAllNotifications();
	}

	public void OnAchievementNotifications(List<AchievementNotification> achievementNotifications)
	{
		List<Achievement> newlyCompletedAchieves = new List<Achievement>();
		List<Achievement> updatedAchieves = new List<Achievement>();
		bool resetLastEventTimingAndLicenseCheck = false;
		foreach (AchievementNotification notification in achievementNotifications)
		{
			if (m_queueNotifications || !m_allNetAchievesReceived || m_achieveNotificationsToQueue.Contains((int)notification.AchievementId))
			{
				Log.Achievements.Print("Blocking AchievementNotification: ID={0}", notification.AchievementId);
				m_blockedAchievementNotifications.Add(notification);
				continue;
			}
			Achievement achievement = GetAchievement((int)notification.AchievementId);
			if (achievement != null)
			{
				if (PlatformSettings.IsSteam && notification.Complete)
				{
					SetSteamAchievementIfNeeded(achievement.DbfRecord);
				}
				if (achievement.AchieveTrigger == Assets.Achieve.Trigger.LICENSEADDED || achievement.AchieveTrigger == Assets.Achieve.Trigger.EVENT_TIMING_ONLY)
				{
					resetLastEventTimingAndLicenseCheck = true;
				}
				achievement.OnAchieveNotification(notification);
				if (!achievement.Active && notification.Complete)
				{
					newlyCompletedAchieves.Add(achievement);
				}
				else
				{
					updatedAchieves.Add(achievement);
				}
				Log.Achievements.Print("OnAchievementNotification: Achievement={0}", achievement);
			}
		}
		if (resetLastEventTimingAndLicenseCheck)
		{
			m_lastEventTimingAndLicenseAchieveCheck = 0L;
		}
		AchievesUpdatedListener[] array = m_achievesUpdatedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(updatedAchieves, newlyCompletedAchieves);
		}
	}

	public void BlockAllNotifications()
	{
		m_queueNotifications = true;
	}

	public void UnblockAllNotifications()
	{
		m_queueNotifications = false;
		if (m_blockedAchievementNotifications.Count > 0)
		{
			OnAchievementNotifications(m_blockedAchievementNotifications);
			m_blockedAchievementNotifications.Clear();
		}
	}

	private void OnQuestCanceled()
	{
		Network.CanceledQuest response = Network.Get().GetCanceledQuest();
		Log.Achievements.Print("OnQuestCanceled: CanceledQuest={0}", response);
		m_disableCancelButtonUntilServerReturns = false;
		if (response.Canceled)
		{
			GetAchievement(response.AchieveID).OnCancelSuccess();
			NetCache.NetCacheRewardProgress netCacheRewardProgress = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>();
			if (netCacheRewardProgress != null)
			{
				netCacheRewardProgress.NextQuestCancelDate = response.NextQuestCancelDate;
			}
		}
		FireAchieveCanceledEvent(response.AchieveID, response.Canceled);
		UnblockAllNotifications();
	}

	private void OnAchieveValidated()
	{
		ValidateAchieveResponse response = Network.Get().GetValidatedAchieve();
		m_achieveValidationsRequested.Remove(response.Achieve);
		Log.Achievements.Print("AchieveManager.OnAchieveValidated(): achieve={0} success={1}", response.Achieve, response.Success);
	}

	private void OnEventTriggered()
	{
		Network.Get().GetTriggerEventResponse();
		m_numEventResponsesNeeded--;
	}

	private void OnAccountLicenseAchieveResponse()
	{
		Network.AccountLicenseAchieveResponse response = Network.Get().GetAccountLicenseAchieveResponse();
		if (response.Result != Network.AccountLicenseAchieveResponse.AchieveResult.COMPLETE)
		{
			FireLicenseAddedAchievesUpdatedEvent();
			return;
		}
		Log.Achievements.Print("AchieveManager.OnAccountLicenseAchieveResponse(): achieve {0} is now complete, refreshing achieves", response.Achieve);
		OnAccountLicenseAchievesUpdated(response.Achieve);
	}

	private void OnAccountLicenseAchievesUpdated(object userData)
	{
		int achieveID = (int)userData;
		Log.Achievements.Print("AchieveManager.OnAccountLicenseAchievesUpdated(): refreshing achieves complete, triggered by achieve {0}", achieveID);
		FireLicenseAddedAchievesUpdatedEvent();
	}

	private void FireLicenseAddedAchievesUpdatedEvent()
	{
		List<Achievement> activeLicenseAddedAchieves = GetActiveLicenseAddedAchieves();
		LicenseAddedAchievesUpdatedListener[] array = m_licenseAddedAchievesUpdatedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(activeLicenseAddedAchieves);
		}
	}

	private void OnNewNotices(List<NetCache.ProfileNotice> newNotices, bool isInitialNoticeList)
	{
		foreach (NetCache.ProfileNotice notice in newNotices)
		{
			if (NetCache.ProfileNotice.NoticeOrigin.ACHIEVEMENT == notice.Origin)
			{
				int achieveID = (int)notice.OriginData;
				GetAchievement(achieveID)?.AddRewardNoticeID(notice.NoticeID);
			}
		}
	}

	private bool CanCancelQuestNow()
	{
		if (Vars.Key("Quests.CanCancelManyTimes").GetBool(def: false))
		{
			return true;
		}
		NetCache.NetCacheRewardProgress netCacheProgress = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>();
		if (netCacheProgress == null)
		{
			return false;
		}
		long utcNow = DateTime.Now.ToFileTimeUtc();
		return netCacheProgress.NextQuestCancelDate <= utcNow;
	}

	private void FireAchieveCanceledEvent(int achieveID, bool success)
	{
		AchieveCanceledListener[] array = m_achieveCanceledListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Fire(achieveID, success);
		}
	}

	private void CheckAllCardGainAchieves()
	{
		GetAchieves(delegate(Achievement obj)
		{
			if (!obj.Enabled)
			{
				return false;
			}
			if (obj.IsCompleted())
			{
				return false;
			}
			Assets.Achieve.Trigger achieveTrigger = obj.AchieveTrigger;
			return (uint)(achieveTrigger - 6) <= 1u && obj.RaceRequirement.HasValue;
		});
		GetAchieves(delegate(Achievement obj)
		{
			if (!obj.Enabled)
			{
				return false;
			}
			if (obj.IsCompleted())
			{
				return false;
			}
			return obj.AchieveTrigger == Assets.Achieve.Trigger.CARDSET && obj.CardSetRequirement.HasValue;
		});
		ValidateAchievesNow();
	}

	private void CheckTimedEventsAndLicenses(DateTime utcNow)
	{
		if (!m_allNetAchievesReceived)
		{
			return;
		}
		DateTime localNow = utcNow.ToLocalTime();
		if (localNow.Ticks - m_lastEventTimingAndLicenseAchieveCheck < TIMED_AND_LICENSE_ACHIEVE_CHECK_DELAY_TICKS)
		{
			return;
		}
		m_lastEventTimingAndLicenseAchieveCheck = localNow.Ticks;
		int numAchievesToValidate = 0;
		foreach (Achievement achieve in m_achievements.Values)
		{
			if (achieve.Enabled && !achieve.IsCompleted() && achieve.Active && Assets.Achieve.Trigger.EVENT_TIMING_ONLY == achieve.AchieveTrigger && EventTimingManager.Get().IsEventActive(achieve.EventTrigger) && (!m_lastEventTimingValidationByAchieve.ContainsKey(achieve.ID) || localNow.Ticks - m_lastEventTimingValidationByAchieve[achieve.ID] >= TIMED_ACHIEVE_VALIDATION_DELAY_TICKS))
			{
				Log.Achievements.Print("AchieveManager.CheckTimedEventsAndLicenses(): checking on timed event achieve {0} time {1}", achieve.ID, localNow);
				m_lastEventTimingValidationByAchieve[achieve.ID] = localNow.Ticks;
				m_achieveValidationsToRequest.Add(achieve.ID);
				numAchievesToValidate++;
			}
			if (achieve.IsActiveLicenseAddedAchieve() && (!m_lastCheckLicenseAddedByAchieve.ContainsKey(achieve.ID) || utcNow.Ticks - m_lastCheckLicenseAddedByAchieve[achieve.ID] >= CHECK_LICENSE_ADDED_ACHIEVE_DELAY_TICKS))
			{
				Log.Achievements.Print("AchieveManager.CheckTimedEventsAndLicenses(): checking on license added achieve {0} time {1}", achieve.ID, localNow);
				m_lastCheckLicenseAddedByAchieve[achieve.ID] = utcNow.Ticks;
				Network.Get().CheckAccountLicenseAchieve(achieve.ID);
			}
		}
		if (numAchievesToValidate != 0)
		{
			ValidateAchievesNow();
		}
	}

	private List<Achievement> GetActiveLicenseAddedAchieves()
	{
		List<Achievement> activeLicenseAddedAchieves = new List<Achievement>();
		foreach (KeyValuePair<int, Achievement> achievement2 in m_achievements)
		{
			Achievement achievement = achievement2.Value;
			if (achievement.IsActiveLicenseAddedAchieve())
			{
				activeLicenseAddedAchieves.Add(achievement);
			}
		}
		return activeLicenseAddedAchieves;
	}

	public List<RewardData> GetRewardsForAdventureAndMode(int adventureId, int modeId, HashSet<Assets.Achieve.RewardTiming> rewardTimings)
	{
		List<RewardData> rewards = new List<RewardData>();
		foreach (Achievement achieve in GetAchievesForAdventureAndMode(adventureId, modeId))
		{
			rewards.AddRange(GetRewardsForAchieve(achieve.ID, rewardTimings));
		}
		return rewards;
	}

	public List<RewardData> GetRewardsForAdventureWing(int wingID, HashSet<Assets.Achieve.RewardTiming> rewardTimings)
	{
		List<RewardData> rewards = new List<RewardData>();
		foreach (Achievement achieve in GetAchievesForAdventureWing(wingID))
		{
			rewards.AddRange(GetRewardsForAchieve(achieve.ID, rewardTimings));
		}
		return rewards;
	}

	public List<RewardData> GetRewardsForAdventureScenario(int wingID, int scenarioID, HashSet<Assets.Achieve.RewardTiming> rewardTimings)
	{
		List<RewardData> rewards = new List<RewardData>();
		foreach (Achievement achieve in GetAchievesForAdventureWing(wingID))
		{
			if (achieve.Scenarios.Contains(scenarioID))
			{
				rewards.AddRange(GetRewardsForAchieve(achieve.ID, rewardTimings));
			}
		}
		return rewards;
	}

	public List<RewardData> GetRewardsForAchieve(int achieveID, HashSet<Assets.Achieve.RewardTiming> rewardTimings)
	{
		List<RewardData> rewards = new List<RewardData>();
		Achievement achieve = GetAchievement(achieveID);
		List<RewardData> achieveRewards = achieve.Rewards;
		if (rewardTimings.Contains(achieve.RewardTiming))
		{
			foreach (RewardData reward in achieveRewards)
			{
				rewards.Add(reward);
			}
		}
		return rewards;
	}

	private void SetSteamAchievementIfNeeded(AchieveDbfRecord achieveRecord)
	{
	}
}
