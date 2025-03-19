using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

namespace Hearthstone.Progression;

public class RewardTrackManager : IService
{
	private readonly Dictionary<Global.RewardTrackType, RewardTrack> m_rewardTrackEntries = new Dictionary<Global.RewardTrackType, RewardTrack>();

	private bool m_isShowingSeasonRoll;

	private readonly Queue<RewardTrackUnclaimedRewards> m_rewardTrackUnclaimedNotifications = new Queue<RewardTrackUnclaimedRewards>();

	private readonly RewardPresenter m_rewardPresenter = new RewardPresenter();

	private RepeatingScheduledCallback m_trackSeasonRollChecker = new RepeatingScheduledCallback();

	private float m_trackSeasonRollClientRetrySecs = 2f;

	private float m_trackSeasonRollClientJitterSecs = 1f;

	private bool m_trackExpirationEnabled;

	private bool m_hasReceivedInitialClientState;

	private bool m_hasReceivedEventTimings;

	private bool m_haveCheckedApprenticePlayerFlag;

	private bool m_isApprenticeComplete;

	private bool m_justCompletedApprentice;

	public bool DidJustCompleteApprentice => m_justCompletedApprentice;

	public bool HasReceivedRewardTracksFromServer { get; private set; }

	public event Action OnRewardTracksReceived;

	public static RewardTrackManager Get()
	{
		return ServiceManager.Get<RewardTrackManager>();
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		Network network = serviceLocator.Get<Network>();
		network.RegisterNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
		network.RegisterNetHandler(PlayerRewardTrackStateUpdate.PacketID.ID, ReceiveRewardTrackStateUpdateMessage);
		network.RegisterNetHandler(RewardTrackUnclaimedNotification.PacketID.ID, OnRewardTrackUnclaimedNotification);
		AccountLicenseMgr.Get().RegisterAccountLicensesChangedListener(OnAccountLicensesChanged);
		serviceLocator.Get<NetCache>().RegisterUpdatedListener(typeof(NetCache.NetCacheAccountLicenses), OnAccountLicensesUpdated);
		serviceLocator.Get<EventTimingManager>().OnReceivedEventTimingsFromServer += OnReceivedEventTimingsFromServer;
		if (ServiceManager.TryGet<SceneMgr>(out var sceneMgr))
		{
			sceneMgr.RegisterScenePreLoadEvent(OnScenePreLoad);
			sceneMgr.RegisterSceneUnloadedEvent(OnSceneUnloaded);
		}
		foreach (Global.RewardTrackType trackType in Enum.GetValues(typeof(Global.RewardTrackType)))
		{
			if (trackType != 0)
			{
				m_rewardTrackEntries.Add(trackType, new RewardTrack(trackType));
			}
		}
		GameSaveDataManager.Get().OnGameSaveDataUpdate += OnGameSaveDataUpdate;
		CheckForApprenticeCompletion();
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[6]
		{
			typeof(AccountLicenseMgr),
			typeof(EventTimingManager),
			typeof(NetCache),
			typeof(Network),
			typeof(SceneMgr),
			typeof(AchievementManager)
		};
	}

	public void Shutdown()
	{
		if (ServiceManager.TryGet<Network>(out var net))
		{
			net.RemoveNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
			net.RemoveNetHandler(PlayerRewardTrackStateUpdate.PacketID.ID, ReceiveRewardTrackStateUpdateMessage);
			net.RemoveNetHandler(RewardTrackUnclaimedNotification.PacketID.ID, OnRewardTrackUnclaimedNotification);
		}
		if (ServiceManager.TryGet<AccountLicenseMgr>(out var accountLicenseMgr))
		{
			accountLicenseMgr.RemoveAccountLicensesChangedListener(OnAccountLicensesChanged);
		}
		if (ServiceManager.TryGet<NetCache>(out var netCache))
		{
			netCache.UnregisterNetCacheHandler(OnAccountLicensesUpdated);
		}
		if (ServiceManager.TryGet<SceneMgr>(out var sceneMgr))
		{
			sceneMgr.UnregisterScenePreLoadEvent(OnScenePreLoad);
			sceneMgr.UnregisterSceneUnloadedEvent(OnSceneUnloaded);
		}
		GameSaveDataManager.Get().OnGameSaveDataUpdate -= OnGameSaveDataUpdate;
	}

	private void WillReset()
	{
		foreach (RewardTrack rewardTrack in m_rewardTrackEntries.Values)
		{
			if (rewardTrack.IsValid)
			{
				rewardTrack.Reset();
			}
		}
		m_hasReceivedInitialClientState = false;
		m_hasReceivedEventTimings = false;
		HasReceivedRewardTracksFromServer = false;
		m_trackSeasonRollChecker.Stop();
		m_rewardTrackUnclaimedNotifications.Clear();
	}

	public void ClearHasJustCompletedApprentice()
	{
		m_justCompletedApprentice = false;
	}

	public bool ShowNextReward(Action callback)
	{
		return GetRewardPresenter().ShowNextReward(callback);
	}

	public bool HasReward()
	{
		return GetRewardPresenter().HasReward();
	}

	public RewardPresenter GetRewardPresenter()
	{
		return m_rewardPresenter;
	}

	public bool HasBattlegroundsPreviewHeroes()
	{
		RewardTrack bgRewardTrack = GetRewardTrack(Global.RewardTrackType.BATTLEGROUNDS);
		if (bgRewardTrack == null || !bgRewardTrack.IsValid)
		{
			return false;
		}
		if (bgRewardTrack.TrackDataModel.RewardTrackId == 0)
		{
			return false;
		}
		int unlockedLevel = bgRewardTrack.TrackDataModel.Level;
		HashSet<RewardTrackPaidType> ownedPaidTypes = bgRewardTrack.GetOwnedRewardTrackPaidTypes();
		for (int level = 1; level <= unlockedLevel; level++)
		{
			RewardTrackLevelDbfRecord levelRecord = bgRewardTrack.GetRewardTrackLevelRecord(level);
			foreach (RewardTrackPaidType paidType in ownedPaidTypes)
			{
				RewardListDbfRecord rewardList = ProgressUtils.GetRewardListRecord(levelRecord, paidType);
				if (rewardList == null)
				{
					continue;
				}
				foreach (RewardItemDbfRecord item in rewardList.RewardItems)
				{
					if (item.RewardType == RewardItem.RewardType.BATTLEGROUNDS_SEASON_BONUS && item.BattlegroundsBonusType == RewardItem.BattlegroundsBonusType.EARLY_ACCESS_HEROES)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool ShowUnclaimedTrackRewardsPopup(Action callback)
	{
		if (m_isShowingSeasonRoll || m_rewardTrackUnclaimedNotifications.Count == 0)
		{
			return false;
		}
		CreateRewardWidget(callback);
		return true;
	}

	public RewardTrack GetRewardTrack(RewardTrackDataModel rewardTrackDataModel)
	{
		if (rewardTrackDataModel == null)
		{
			return null;
		}
		return GetRewardTrack(rewardTrackDataModel.RewardTrackType);
	}

	public RewardTrack GetRewardTrack(RewardTrackDbfRecord rewardTrackDbfRecord)
	{
		if (rewardTrackDbfRecord == null)
		{
			return null;
		}
		return GetRewardTrack(rewardTrackDbfRecord.RewardTrackType);
	}

	public RewardTrack GetRewardTrack(Assets.Achievement.RewardTrackType rewardTrackType)
	{
		return GetRewardTrack((Global.RewardTrackType)rewardTrackType);
	}

	public RewardTrack GetRewardTrack(Assets.RewardTrack.RewardTrackType rewardTrackType)
	{
		return GetRewardTrack((Global.RewardTrackType)rewardTrackType);
	}

	public RewardTrack GetRewardTrack(Global.RewardTrackType rewardTrackType)
	{
		if (rewardTrackType == Global.RewardTrackType.NONE)
		{
			Debug.LogError(string.Format("{0}: Attempting to get invalid reward track of type {1}.", "RewardTrackManager", rewardTrackType));
			return null;
		}
		if (m_rewardTrackEntries.TryGetValue(rewardTrackType, out var rewardTrack))
		{
			return rewardTrack;
		}
		return null;
	}

	public RewardTrack GetRewardTrackWithOverrideType(Global.RewardTrackType overrideTrackType)
	{
		if (overrideTrackType == Global.RewardTrackType.NONE)
		{
			return null;
		}
		foreach (Global.RewardTrackType trackType in Enum.GetValues(typeof(Global.RewardTrackType)))
		{
			if (trackType == Global.RewardTrackType.NONE || !m_rewardTrackEntries.ContainsKey(trackType))
			{
				continue;
			}
			RewardTrack rewardTrack = m_rewardTrackEntries[trackType];
			if (rewardTrack.RewardTrackId != 0 && rewardTrack.RewardTrackAsset != null)
			{
				Assets.RewardTrack.RewardTrackType trackOverrideType = rewardTrack.RewardTrackAsset.OverrideQuestRewardTrackType;
				if (overrideTrackType == (Global.RewardTrackType)trackOverrideType)
				{
					return rewardTrack;
				}
			}
		}
		return null;
	}

	public RewardTrack GetCurrentEventRewardTrack()
	{
		foreach (Global.RewardTrackType rewardTrackType in Enum.GetValues(typeof(Global.RewardTrackType)))
		{
			if (ProgressUtils.IsEventRewardTrackType(rewardTrackType))
			{
				RewardTrack rewardTrack = GetRewardTrack(rewardTrackType);
				if (rewardTrack != null && rewardTrack.IsValid && rewardTrack.IsActive)
				{
					return rewardTrack;
				}
			}
		}
		return null;
	}

	public SpecialEventDataModel GetEventDataModelFromRewardTrack(RewardTrackDataModel trackDataModel)
	{
		if (trackDataModel == null)
		{
			return null;
		}
		EventRewardTrackDbfRecord eventTrackRecord = GameDbf.EventRewardTrack.GetRecord((EventRewardTrackDbfRecord record) => record.EventRewardTrackId == trackDataModel.RewardTrackId);
		if (eventTrackRecord == null)
		{
			return null;
		}
		SpecialEventDbfRecord parentSpecialEvent = GameDbf.SpecialEvent.GetRecord(eventTrackRecord.SpecialEventId);
		if (parentSpecialEvent == null)
		{
			return null;
		}
		return SpecialEventManager.Get().GetEventDataModelFromSpecialEvent(parentSpecialEvent);
	}

	public void UpdateJournalMetaWithRewardTrack(JournalMetaDataModel journalMeta, Global.RewardTrackType journalRewardTrackType)
	{
		RewardTrack mainRewardTrack = GetRewardTrack(journalRewardTrackType);
		if (mainRewardTrack == null || !mainRewardTrack.IsValid)
		{
			Debug.LogError(string.Format("{0}: no reward track found of type {1}.", "JournalPopup", journalRewardTrackType));
			return;
		}
		if (journalRewardTrackType == Global.RewardTrackType.APPRENTICE && GameSaveDataManager.Get().IsDataReady(GameSaveKeyId.FTUE))
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_WELCOME_APPRENTICE, out long hasSeenWelcomeApprenticeFlag);
			if (hasSeenWelcomeApprenticeFlag == 1)
			{
				journalMeta.HasSeenWelcomeApprentice = true;
			}
			else
			{
				journalMeta.HasSeenWelcomeApprentice = false;
			}
		}
		journalMeta.RewardTrackHasUnclaimed = mainRewardTrack.TrackDataModel.Unclaimed > 0;
		journalMeta.RewardTrackSeasonName = mainRewardTrack.TrackDataModel.Name;
		journalMeta.RewardTrackPaidUnlocked = mainRewardTrack.HasOwnedRewardTrackPaidType(RewardTrackPaidType.RTPT_PAID);
		journalMeta.RewardTrackPaidPremiumUnlocked = mainRewardTrack.HasOwnedRewardTrackPaidType(RewardTrackPaidType.RTPT_PAID_PREMIUM);
		journalMeta.RewardTrackHasNewSeason = mainRewardTrack.SeasonNewerThanLastSeen();
	}

	public void OnSeasonRollDuringClientSession(RewardTrackDataModel rewardTrackDataModel, bool shouldActivate)
	{
		m_trackSeasonRollChecker.Stop();
		if (!shouldActivate)
		{
			rewardTrackDataModel.Expired = true;
		}
	}

	public bool SetActiveEventRewardTrack(int rewardTrackId)
	{
		if (!Network.IsLoggedIn())
		{
			return false;
		}
		Network.Get().SetActiveEventRewardTrack(rewardTrackId);
		return true;
	}

	public static void OpenTavernPassErrorPopup(Global.RewardTrackType trackType = Global.RewardTrackType.NONE)
	{
		AlertPopup.PopupInfo info = ((trackType != Global.RewardTrackType.BATTLEGROUNDS) ? new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_PROGRESSION_BONUS_ERROR_HEADER"),
			m_text = GameStrings.Get("GLUE_PROGRESSION_BONUS_ERROR_BODY"),
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.OK
		} : new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_PROGRESSION_BATTLEGROUNDS_BONUS_ERROR_HEADER"),
			m_text = GameStrings.Get("GLUE_PROGRESSION_BATTLEGROUNDS_BONUS_ERROR_BODY"),
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.OK
		});
		DialogManager.Get().ShowPopup(info);
	}

	public bool IsApprenticeTrackReady()
	{
		return GetRewardTrack(Global.RewardTrackType.APPRENTICE)?.IsValid ?? false;
	}

	public int GetApprenticeTrackLevel()
	{
		RewardTrack apprenticeTrack = GetRewardTrack(Global.RewardTrackType.APPRENTICE);
		if (apprenticeTrack == null || !apprenticeTrack.IsValid)
		{
			return 0;
		}
		return apprenticeTrack.TrackDataModel.Level;
	}

	public bool HasUnclaimedRewardsForApprenticeLevel(int level)
	{
		RewardTrack apprenticeTrack = GetRewardTrack(Global.RewardTrackType.APPRENTICE);
		if (apprenticeTrack == null || !apprenticeTrack.IsValid)
		{
			return false;
		}
		return apprenticeTrack.HasUnclaimedRewardsForLevel(level);
	}

	public bool HasAnyUnclaimedApprenticeRewards()
	{
		RewardTrack apprenticeTrack = GetRewardTrack(Global.RewardTrackType.APPRENTICE);
		if (apprenticeTrack == null || !apprenticeTrack.IsValid)
		{
			return false;
		}
		int apprenticeTrackLevel = GetApprenticeTrackLevel();
		for (int level = 1; level <= apprenticeTrackLevel; level++)
		{
			if (apprenticeTrack.HasUnclaimedRewardsForLevel(level))
			{
				return true;
			}
		}
		return false;
	}

	private void OnInitialClientState()
	{
		foreach (RewardTrack value in m_rewardTrackEntries.Values)
		{
			value.InitializeClientState();
		}
		InitialClientState packet = Network.Get().GetInitialClientState();
		if (packet != null && packet.HasGuardianVars)
		{
			m_hasReceivedInitialClientState = true;
			m_trackExpirationEnabled = packet.GuardianVars.RewardTrackSeasonRollClientCheckEnabled;
			m_trackSeasonRollClientRetrySecs = packet.GuardianVars.RewardTrackSeasonRollClientRetrySecs;
			m_trackSeasonRollClientJitterSecs = packet.GuardianVars.RewardTrackSeasonRollClientJitterSecs;
			ScheduleCheckForSeasonRoll();
		}
	}

	private void OnReceivedEventTimingsFromServer()
	{
		m_hasReceivedEventTimings = true;
		ScheduleCheckForSeasonRoll();
	}

	private RewardTrack GetCurrentRewardTrack(PlayerRewardTrackStateUpdate playerRewardTrackStateUpdate)
	{
		if (playerRewardTrackStateUpdate == null)
		{
			return null;
		}
		return GetCurrentRewardTrack(playerRewardTrackStateUpdate.RewardTrackId);
	}

	private RewardTrack GetCurrentRewardTrack(PlayerRewardTrackState playerRewardTrackState)
	{
		if (playerRewardTrackState == null)
		{
			return null;
		}
		return GetCurrentRewardTrack(playerRewardTrackState.RewardTrackId);
	}

	private RewardTrack GetCurrentRewardTrack(int rewardTrackId)
	{
		if (rewardTrackId == 0)
		{
			return null;
		}
		RewardTrackDbfRecord rewardTrackDbfRecord = GameDbf.RewardTrack.GetRecord(rewardTrackId);
		if (rewardTrackDbfRecord == null)
		{
			Debug.LogWarning(string.Format("Received a {0} with nonzero {1}:{2}, ", "PlayerRewardTrackState", "RewardTrackId", rewardTrackId) + "but no RewardTrackDbfRecord for this RewardTrackId was found.");
		}
		return GetRewardTrack(rewardTrackDbfRecord);
	}

	private void CreateRewardWidget(Action callback)
	{
		RewardTrackUnclaimedRewards rewardTrackUnclaimedNotif = m_rewardTrackUnclaimedNotifications.Dequeue();
		Widget widget = WidgetInstance.Create(RewardTrackSeasonRoll.REWARD_TRACK_SEASON_ROLL_PREFAB);
		m_isShowingSeasonRoll = true;
		widget.RegisterReadyListener(delegate
		{
			RewardTrackSeasonRoll componentInChildren = widget.GetComponentInChildren<RewardTrackSeasonRoll>();
			componentInChildren.Initialize(callback, rewardTrackUnclaimedNotif);
			componentInChildren.Show();
		});
	}

	private void ReceiveRewardTrackStateUpdateMessage()
	{
		PlayerRewardTrackStateUpdate updateMessage = Network.Get().GetPlayerRewardTrackStateUpdate();
		if (updateMessage == null)
		{
			return;
		}
		RewardTrack topLevelRewardTrack = GetCurrentRewardTrack(updateMessage);
		foreach (PlayerRewardTrackState stateUpdate in updateMessage.State)
		{
			RewardTrack rewardTrack = GetCurrentRewardTrack(stateUpdate) ?? topLevelRewardTrack;
			if (rewardTrack == null)
			{
				Log.All.PrintError("PlayerRewardTrackState and its containing PlayerRewardTrackStateUpdate contain no valid RewardTrack.");
			}
			else
			{
				rewardTrack.HandleRewardTrackStateUpdate(stateUpdate);
			}
		}
		this.OnRewardTracksReceived?.Invoke();
		HasReceivedRewardTracksFromServer = true;
		ScheduleCheckForSeasonRoll();
	}

	private void OnRewardTrackUnclaimedNotification()
	{
		RewardTrackUnclaimedNotification rewardTrackUnclaimedNotif = Network.Get().GetRewardTrackUnclaimedNotification();
		if (rewardTrackUnclaimedNotif == null)
		{
			return;
		}
		foreach (RewardTrackUnclaimedRewards rewardTrackUnclaimedRewards in rewardTrackUnclaimedNotif.Notif)
		{
			m_rewardTrackUnclaimedNotifications.Enqueue(rewardTrackUnclaimedRewards);
		}
	}

	private void OnAccountLicensesChanged(List<AccountLicenseInfo> changedLicensesInfo, object userData)
	{
		OnAccountLicensesUpdated();
	}

	private void OnAccountLicensesUpdated()
	{
		foreach (RewardTrack rewardTrack in m_rewardTrackEntries.Values)
		{
			if (rewardTrack.IsValid)
			{
				rewardTrack.UpdateRewardsAndBonuses();
			}
		}
	}

	private TimeSpan? GetTimeUntilNextTrackSeasonRoll()
	{
		TimeSpan? timeUntilNextSeasonRoll = null;
		EventTimingManager eventTimingManager = EventTimingManager.Get();
		SpecialEventManager specialEventManager = SpecialEventManager.Get();
		foreach (RewardTrack rewardTrack in m_rewardTrackEntries.Values)
		{
			if (!ProgressUtils.IsEventRewardTrackType(rewardTrack.TrackDataModel.RewardTrackType) || !rewardTrack.IsActive)
			{
				continue;
			}
			if (!rewardTrack.IsValid || rewardTrack.RewardTrackAsset == null)
			{
				Log.All.PrintError("Trying to access reward track data but track id is invalid or missing");
				continue;
			}
			EventTimingType trackEventType = rewardTrack.RewardTrackAsset.Event;
			if (!eventTimingManager.GetEventEndTimeUtc(trackEventType).HasValue)
			{
				continue;
			}
			TimeSpan timeLeftOnTrack = eventTimingManager.GetTimeLeftForEvent(trackEventType);
			if (timeUntilNextSeasonRoll.HasValue)
			{
				TimeSpan value = timeLeftOnTrack;
				TimeSpan? timeSpan = timeUntilNextSeasonRoll;
				if (!(value < timeSpan))
				{
					continue;
				}
			}
			timeUntilNextSeasonRoll = timeLeftOnTrack;
		}
		SpecialEventDbfRecord relevantSpecialEvent = null;
		SpecialEventDbfRecord currentSpecialEvent = specialEventManager.GetCurrentSpecialEvent();
		if (!timeUntilNextSeasonRoll.HasValue && currentSpecialEvent != null)
		{
			relevantSpecialEvent = currentSpecialEvent;
		}
		if (relevantSpecialEvent == null)
		{
			TimeSpan timeSpanToCheckForNextTrack = new TimeSpan(2, 0, 0, 0);
			relevantSpecialEvent = specialEventManager.GetUpcomingSpecialEvent(timeSpanToCheckForNextTrack);
		}
		if (relevantSpecialEvent != null && relevantSpecialEvent.RewardTracks != null && relevantSpecialEvent.RewardTracks.Count == 1)
		{
			EventTimingType trackEventType2 = relevantSpecialEvent.EventTiming;
			TimeSpan timeUntilTrackStart = eventTimingManager.GetTimeUntilEventStart(trackEventType2);
			if (timeUntilNextSeasonRoll.HasValue)
			{
				TimeSpan value = timeUntilTrackStart;
				TimeSpan? timeSpan = timeUntilNextSeasonRoll;
				if (!(value < timeSpan))
				{
					goto IL_018b;
				}
			}
			timeUntilNextSeasonRoll = timeUntilTrackStart;
		}
		goto IL_018b;
		IL_018b:
		return timeUntilNextSeasonRoll;
	}

	private void ScheduleCheckForSeasonRoll()
	{
		if (m_hasReceivedInitialClientState && m_trackExpirationEnabled && m_hasReceivedEventTimings && HasReceivedRewardTracksFromServer && SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY && !m_trackSeasonRollChecker.IsRunning)
		{
			TimeSpan? timeUntilNextSeasonRoll = GetTimeUntilNextTrackSeasonRoll();
			if (timeUntilNextSeasonRoll.HasValue)
			{
				m_trackSeasonRollChecker.Start(SendCheckForSeasonRoll, (float)timeUntilNextSeasonRoll.Value.TotalSeconds, m_trackSeasonRollClientRetrySecs, 2f, m_trackSeasonRollClientJitterSecs);
			}
		}
	}

	private bool SendCheckForSeasonRoll()
	{
		if (!Network.IsLoggedIn())
		{
			return false;
		}
		Network.Get().CheckForRewardTrackSeasonRoll();
		return true;
	}

	private void OnScenePreLoad(SceneMgr.Mode prevMode, SceneMgr.Mode mode, object userData)
	{
		if (mode == SceneMgr.Mode.GAMEPLAY)
		{
			m_trackSeasonRollChecker.Stop();
		}
	}

	private void OnSceneUnloaded(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		if (prevMode == SceneMgr.Mode.GAMEPLAY)
		{
			ScheduleCheckForSeasonRoll();
		}
	}

	private void OnGameSaveDataUpdate(GameSaveKeyId key)
	{
		if (key == GameSaveKeyId.PLAYER_FLAGS)
		{
			CheckForApprenticeCompletion();
		}
	}

	private void CheckForApprenticeCompletion()
	{
		if (!GameSaveDataManager.Get().IsDataReady(GameSaveKeyId.PLAYER_FLAGS))
		{
			return;
		}
		bool hasCompletedApprentice = GameUtils.HasCompletedApprentice();
		if (!m_haveCheckedApprenticePlayerFlag)
		{
			m_haveCheckedApprenticePlayerFlag = true;
			m_isApprenticeComplete = hasCompletedApprentice;
			if (!m_isApprenticeComplete)
			{
				AchievementManager.Get().PauseToastNotifications();
			}
		}
		else if (!m_isApprenticeComplete && hasCompletedApprentice)
		{
			m_isApprenticeComplete = true;
			m_justCompletedApprentice = true;
			Box box = Box.Get();
			if (!RankMgr.Get().DidSkipApprenticeThisSession && box != null)
			{
				box.GetRailroadManager().ToggleBoxTutorials(setEnabled: false);
				box.GetRailroadManager().UpdateRailroadingOnBox();
				box.UpdateUI();
			}
			BnetBar.Get()?.RefreshCurrency();
			AchievementManager.Get().UnpauseToastNotifications();
			GameSaveDataManager.Get().OnGameSaveDataUpdate -= OnGameSaveDataUpdate;
		}
	}

	public void Cheat_ShowRewardScroll(int rewardListId, int level)
	{
		GetRewardPresenter().EnqueueReward(RewardTrackFactory.CreateRewardScrollDataModel(rewardListId, level, GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_REWARD_SCROLL_TITLE", level)), delegate
		{
		});
	}
}
