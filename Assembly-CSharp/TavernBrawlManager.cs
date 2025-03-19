using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using PegasusClient;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class TavernBrawlManager : IService
{
	public delegate void CallbackEnsureServerDataReady();

	public delegate void TavernBrawlSessionLimitRaisedCallback(int oldLimit, int newLimit);

	private TavernBrawlMission[] m_missions = new TavernBrawlMission[3];

	private bool[] m_downloadableDbfAssetsPendingLoad = new bool[3];

	private TavernBrawlPlayerRecord[] m_playerRecords = new TavernBrawlPlayerRecord[3];

	private DateTime?[] m_scheduledRefreshTimes = new DateTime?[3];

	private DateTime?[] m_nextSeasonStartDates = new DateTime?[3];

	private int?[] m_latestSeenSeasonThisSession = new int?[3];

	private int?[] m_latestSeenChalkboardThisSession = new int?[3];

	private BrawlType m_currentBrawlType = BrawlType.BRAWL_TYPE_TAVERN_BRAWL;

	private List<CallbackEnsureServerDataReady> m_serverDataReadyCallbacks;

	private bool m_hasGottenClientOptionsAtLeastOnce;

	private bool m_isFirstTimeSeeingThisFeature;

	private bool m_isFirstTimeSeeingCurrentSeason;

	public BrawlType CurrentBrawlType
	{
		get
		{
			return m_currentBrawlType;
		}
		set
		{
			if (value >= BrawlType.BRAWL_TYPE_TAVERN_BRAWL && value < BrawlType.BRAWL_TYPE_COUNT)
			{
				m_currentBrawlType = value;
			}
		}
	}

	public bool IsCurrentBrawlTypeActive => IsTavernBrawlActive(m_currentBrawlType);

	public bool IsFirstTimeSeeingThisFeature
	{
		get
		{
			if (m_isFirstTimeSeeingThisFeature)
			{
				return IsCurrentBrawlTypeActive;
			}
			return false;
		}
	}

	public bool IsFirstTimeSeeingCurrentSeason
	{
		get
		{
			if (IsCurrentBrawlTypeActive)
			{
				return m_isFirstTimeSeeingCurrentSeason;
			}
			return false;
		}
	}

	public int LatestSeenTavernBrawlSeason
	{
		get
		{
			if (m_latestSeenSeasonThisSession[(int)m_currentBrawlType].HasValue)
			{
				return m_latestSeenSeasonThisSession[(int)m_currentBrawlType].Value;
			}
			Option option = Option.LATEST_SEEN_TAVERNBRAWL_SEASON;
			return Options.Get().GetInt(option);
		}
		set
		{
			m_latestSeenSeasonThisSession[(int)m_currentBrawlType] = value;
			if (value <= 100000)
			{
				Option option = Option.LATEST_SEEN_TAVERNBRAWL_SEASON;
				Options.Get().SetInt(option, value);
			}
		}
	}

	public int LatestSeenTavernBrawlChalkboard
	{
		get
		{
			if (m_latestSeenChalkboardThisSession[(int)m_currentBrawlType].HasValue)
			{
				return m_latestSeenChalkboardThisSession[(int)m_currentBrawlType].Value;
			}
			Option option = Option.LATEST_SEEN_TAVERNBRAWL_SEASON_CHALKBOARD;
			return Options.Get().GetInt(option);
		}
		set
		{
			m_latestSeenChalkboardThisSession[(int)m_currentBrawlType] = value;
			if (value <= 100000)
			{
				Option option = Option.LATEST_SEEN_TAVERNBRAWL_SEASON_CHALKBOARD;
				Options.Get().SetInt(option, value);
			}
		}
	}

	public long CurrentTavernBrawlSeasonEndInSeconds => TavernBrawlSeasonEndInSeconds(m_currentBrawlType);

	public float CurrentScheduledSecondsToRefresh => ScheduledSecondsToRefresh(m_currentBrawlType);

	public bool IsDeckLocked => CurrentDeck()?.Locked ?? false;

	public bool IsCurrentSeasonSessionBased => IsSeasonSessionBased(m_currentBrawlType);

	public TavernBrawlMode CurrentSeasonBrawlMode => GetBrawlModeForBrawlType(m_currentBrawlType);

	public long CurrentTavernBrawlSeasonNewSessionsClosedInSeconds => TavernBrawlSeasonNewSessionsClosedInSeconds(CurrentBrawlType);

	public bool IsCurrentTavernBrawlSeasonClosedToPlayer
	{
		get
		{
			if (CurrentTavernBrawlSeasonNewSessionsClosedInSeconds < 0)
			{
				if (MyRecord == null)
				{
					return false;
				}
				if (MyRecord.HasNumTicketsOwned && MyRecord.NumTicketsOwned > 0)
				{
					return false;
				}
				if (PlayerStatus != TavernBrawlStatus.TB_STATUS_ACTIVE)
				{
					return PlayerStatus != TavernBrawlStatus.TB_STATUS_IN_REWARDS;
				}
				return false;
			}
			return false;
		}
	}

	public bool IsPlayerAtSessionMaxForCurrentTavernBrawl
	{
		get
		{
			bool isCurrentSeasonSessionBased = IsCurrentSeasonSessionBased;
			bool atCap = NumSessionsAvailableForPurchase == 0;
			bool unlimitedSessions = NumSessionsAllowedThisSeason == 0;
			bool noActiveSession = PlayerStatus == TavernBrawlStatus.TB_STATUS_TICKET_REQUIRED;
			bool noTicketsAvailable = NumTicketsOwned == 0;
			return isCurrentSeasonSessionBased && atCap && !unlimitedSessions && noActiveSession && noTicketsAvailable;
		}
	}

	public TavernBrawlStatus PlayerStatus
	{
		get
		{
			if (MyRecord != null && MyRecord.HasSessionStatus)
			{
				return MyRecord.SessionStatus;
			}
			return TavernBrawlStatus.TB_STATUS_INVALID;
		}
	}

	public int NumTicketsOwned
	{
		get
		{
			if (MyRecord != null && MyRecord.HasNumTicketsOwned)
			{
				return MyRecord.NumTicketsOwned;
			}
			return 0;
		}
	}

	public int NumSessionsAllowedThisSeason
	{
		get
		{
			if (CurrentMission() != null)
			{
				return CurrentMission().maxSessions;
			}
			return -1;
		}
	}

	public int NumSessionsAvailableForPurchase
	{
		get
		{
			if (MyRecord != null && MyRecord.HasNumSessionsPurchasable)
			{
				return MyRecord.NumSessionsPurchasable;
			}
			return 0;
		}
	}

	public TavernBrawlPlayerSession CurrentSession
	{
		get
		{
			if (MyRecord != null && MyRecord.HasSession)
			{
				return MyRecord.Session;
			}
			return null;
		}
	}

	public int GamesWon
	{
		get
		{
			if (CurrentMission().IsSessionBased)
			{
				if (CurrentSession != null)
				{
					return CurrentSession.Wins;
				}
				return 0;
			}
			if (MyRecord != null)
			{
				return MyRecord.GamesWon;
			}
			return 0;
		}
	}

	public int GamesLost
	{
		get
		{
			if (CurrentMission().IsSessionBased)
			{
				if (CurrentSession != null)
				{
					return CurrentSession.Losses;
				}
				return 0;
			}
			return GamesPlayed - GamesWon;
		}
	}

	public int GamesPlayed
	{
		get
		{
			if (MyRecord != null && MyRecord.HasGamesPlayed)
			{
				return MyRecord.GamesPlayed;
			}
			return 0;
		}
	}

	public int RewardProgress
	{
		get
		{
			if (MyRecord != null)
			{
				return MyRecord.RewardProgress;
			}
			return 0;
		}
	}

	public string EndingTimeText
	{
		get
		{
			long endSecondsFromNow = ((CurrentMission() == null) ? (-1) : CurrentTavernBrawlSeasonEndInSeconds);
			if (endSecondsFromNow < 0)
			{
				return null;
			}
			TimeUtils.ElapsedStringSet timeStringSet = new TimeUtils.ElapsedStringSet
			{
				m_seconds = "GLUE_TAVERN_BRAWL_LABEL_ENDING_SECONDS",
				m_minutes = "GLUE_TAVERN_BRAWL_LABEL_ENDING_MINUTES",
				m_hours = "GLUE_TAVERN_BRAWL_LABEL_ENDING_HOURS",
				m_yesterday = null,
				m_days = "GLUE_TAVERN_BRAWL_LABEL_ENDING_DAYS",
				m_weeks = "GLUE_TAVERN_BRAWL_LABEL_ENDING_WEEKS",
				m_monthAgo = "GLUE_TAVERN_BRAWL_LABEL_ENDING_OVER_1_MONTH"
			};
			return TimeUtils.GetElapsedTimeString((int)endSecondsFromNow, timeStringSet, roundUp: true);
		}
	}

	public List<TavernBrawlMission> Missions => m_missions.Where((TavernBrawlMission m) => m != null).ToList();

	public List<RewardData> CurrentSessionRewards
	{
		get
		{
			if (CurrentSession != null && CurrentSession.Chest != null)
			{
				return Network.ConvertRewardChest(CurrentSession.Chest).Rewards;
			}
			return new List<RewardData>();
		}
	}

	public bool IsCurrentBrawlInfoReady
	{
		get
		{
			NetCache.NetCacheClientOptions netObject = NetCache.Get().GetNetObject<NetCache.NetCacheClientOptions>();
			NetCache.NetCacheHeroLevels herolevels = NetCache.Get().GetNetObject<NetCache.NetCacheHeroLevels>();
			if (netObject == null)
			{
				return false;
			}
			if (CurrentMission() == null)
			{
				return false;
			}
			if (herolevels == null)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsCurrentBrawlAllDataReady => IsAllDataReady(m_currentBrawlType);

	private TavernBrawlPlayerRecord MyRecord => m_playerRecords[(int)m_currentBrawlType];

	private bool CurrentBrawlDeckContentsLoaded
	{
		get
		{
			TavernBrawlMission mission = CurrentMission();
			if (mission == null)
			{
				return true;
			}
			BrawlType brawlType = m_currentBrawlType;
			int seasonId = mission.seasonId;
			foreach (CollectionDeck deck in CollectionManager.Get().GetDecks().Values)
			{
				if (TranslateDeckTypeToBrawlType(deck.Type) == brawlType && deck.SeasonId == seasonId && !deck.NetworkContentsLoaded())
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool IsCheated { get; private set; }

	public event Action OnTavernBrawlUpdated;

	public event TavernBrawlSessionLimitRaisedCallback OnSessionLimitRaised;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		Network network = serviceLocator.Get<Network>();
		NetCache netCache = NetCache.Get();
		network.RegisterNetHandler(TavernBrawlRequestSessionBeginResponse.PacketID.ID, OnBeginSession);
		network.RegisterNetHandler(TavernBrawlRequestSessionRetireResponse.PacketID.ID, OnRetireSession);
		network.RegisterNetHandler(TavernBrawlSessionAckRewardsResponse.PacketID.ID, OnAckRewards);
		network.RegisterNetHandler(TavernBrawlPlayerRecordResponse.PacketID.ID, OnTavernBrawlRecord);
		network.RegisterNetHandler(TavernBrawlInfo.PacketID.ID, OnTavernBrawlInfo);
		netCache.RegisterUpdatedListener(typeof(NetCache.NetCacheHeroLevels), NetCache_OnClientOptions);
		serviceLocator.Get<GameMgr>().RegisterFindGameEvent(OnFindGameEvent);
		RegisterOptionsListeners(register: true);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[3]
		{
			typeof(Network),
			typeof(GameMgr),
			typeof(NetCache)
		};
	}

	public void Shutdown()
	{
	}

	public static TavernBrawlManager Get()
	{
		return ServiceManager.Get<TavernBrawlManager>();
	}

	public TavernBrawlMission CurrentMission()
	{
		return GetMission(m_currentBrawlType);
	}

	public TavernBrawlMission GetMission(BrawlType brawlType)
	{
		if (brawlType < BrawlType.BRAWL_TYPE_TAVERN_BRAWL || brawlType >= BrawlType.BRAWL_TYPE_COUNT)
		{
			return null;
		}
		return m_missions[(int)brawlType];
	}

	public bool SelectHeroBeforeMission()
	{
		return SelectHeroBeforeMission(m_currentBrawlType);
	}

	public bool SelectHeroBeforeMission(BrawlType brawlType)
	{
		if (GetMission(brawlType) != null && GetMission(brawlType).canSelectHeroForDeck)
		{
			return !GetMission(brawlType).canCreateDeck;
		}
		return false;
	}

	public static bool IsInTavernBrawlFriendlyChallenge()
	{
		if (SceneMgr.Get().IsInTavernBrawlMode() || SceneMgr.Get().GetMode() == SceneMgr.Mode.FRIENDLY)
		{
			return FriendChallengeMgr.Get().IsChallengeTavernBrawl();
		}
		return false;
	}

	public TavernBrawlPlayerRecord GetRecord(BrawlType brawlType)
	{
		if (brawlType < BrawlType.BRAWL_TYPE_TAVERN_BRAWL || brawlType >= BrawlType.BRAWL_TYPE_COUNT)
		{
			return null;
		}
		return m_playerRecords[(int)brawlType];
	}

	public bool CanEnterStandardTavernBrawl(out string reason)
	{
		NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (features == null)
		{
			Log.TavernBrawl.Log(LogLevel.Warning, "TavernBrawlManager:CanEnterStandardTavernBrawl: NetCacheFeatures have not finished loading prior to this function call");
			reason = GameStrings.Get("GLUE_TOOLTIP_GAME_MODE_DATA_NOT_LOADED");
			return false;
		}
		if (NetCache.Get().GetNetObject<NetCache.NetCacheHeroLevels>() == null)
		{
			Log.TavernBrawl.Log(LogLevel.Warning, "TavernBrawlManager:CanEnterStandardTavernBrawl: NetCacheHeroLevels have not finished loading prior to this function call");
			reason = GameStrings.Get("GLUE_TOOLTIP_GAME_MODE_DATA_NOT_LOADED");
			return false;
		}
		if (!features.Games.TavernBrawl)
		{
			reason = GameStrings.Get("GLUE_TOOLTIP_BUTTON_DISABLED_DESC");
			return false;
		}
		if (!HasUnlockedTavernBrawl(BrawlType.BRAWL_TYPE_TAVERN_BRAWL))
		{
			reason = GameStrings.Get("GLUE_TOOLTIP_BUTTON_TAVERN_BRAWL_NOT_UNLOCKED");
			return false;
		}
		if (!GameUtils.IsTraditionalTutorialComplete())
		{
			reason = GameStrings.Get("GLUE_TAVERN_BRAWL_TRADITIONAL_TUTORIAL_INCOMPLETE");
			return false;
		}
		if (!IsTavernBrawlActive(BrawlType.BRAWL_TYPE_TAVERN_BRAWL))
		{
			reason = GameStrings.Get("GLUE_TAVERN_BRAWL_HAS_ENDED_TEXT");
			return false;
		}
		if (IsCurrentTavernBrawlSeasonClosedToPlayer)
		{
			reason = GameStrings.Get("GLUE_HEROIC_BRAWL_SIGNUPS_CLOSED");
			return false;
		}
		if (IsPlayerAtSessionMaxForCurrentTavernBrawl)
		{
			reason = GameStrings.Get("GLUE_HEROIC_BRAWL_SESSION_LIMIT_ALERT_LIMIT_HIT");
			return false;
		}
		reason = "";
		return true;
	}

	public DeckRuleset GetCurrentDeckRuleset()
	{
		return GetDeckRuleset(m_currentBrawlType);
	}

	public DeckRuleset GetDeckRuleset(BrawlType brawlType, int brawlLibraryItemId = 0)
	{
		TavernBrawlMission mission = GetMission(brawlType);
		if (mission == null)
		{
			return null;
		}
		if (brawlLibraryItemId == 0)
		{
			brawlLibraryItemId = mission.SelectedBrawlLibraryItemId;
		}
		DeckRuleset ruleset = mission.GetDeckRuleset(brawlLibraryItemId);
		if (ruleset != null)
		{
			return ruleset;
		}
		return DeckRuleset.GetRuleset(mission.formatType);
	}

	public void StartGame(long deckId = 0L)
	{
		if (CurrentMission() == null)
		{
			Error.AddDevFatal("TB: m_currentMission is null");
			return;
		}
		PresenceMgr.Get().SetStatus(Global.PresenceStatus.TAVERN_BRAWL_QUEUE);
		GameType gameType = CurrentMission().GameType;
		GameMgr.Get().FindGame(gameType, FormatType.FT_WILD, CurrentMission().missionId, 0, deckId, null, null, restoreSavedGameState: false, null, null, 0L);
	}

	public void StartGameWithHero(int heroCardDbId)
	{
		TavernBrawlMission mission = CurrentMission();
		if (mission == null)
		{
			Error.AddDevFatal("TB: m_currentMission is null");
			return;
		}
		PresenceMgr.Get().SetStatus(Global.PresenceStatus.TAVERN_BRAWL_QUEUE);
		GameMgr.Get().FindGameWithHero(mission.GameType, FormatType.FT_WILD, mission.missionId, mission.SelectedBrawlLibraryItemId, heroCardDbId, 0L);
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		if (!GameMgr.Get().IsNextTavernBrawl() || GameMgr.Get().IsNextSpectator())
		{
			return false;
		}
		switch (eventData.m_state)
		{
		case FindGameState.CLIENT_CANCELED:
		case FindGameState.CLIENT_ERROR:
		case FindGameState.BNET_QUEUE_CANCELED:
		case FindGameState.BNET_ERROR:
		case FindGameState.SERVER_GAME_CANCELED:
			if (PresenceMgr.Get().CurrentStatus == Global.PresenceStatus.TAVERN_BRAWL_QUEUE)
			{
				PresenceMgr.Get().SetPrevStatus();
			}
			break;
		case FindGameState.SERVER_GAME_CONNECTING:
			if (GameMgr.Get().IsNextTavernBrawl() && GameMgr.Get().IsNextReconnect() && IsCurrentSeasonSessionBased)
			{
				SessionRecord sessionRecord = new SessionRecord();
				sessionRecord.Wins = (uint)GamesWon;
				sessionRecord.Losses = (uint)GamesLost;
				sessionRecord.RunFinished = false;
				sessionRecord.SessionRecordType = ((CurrentSeasonBrawlMode != 0) ? SessionRecordType.HEROIC_BRAWL : SessionRecordType.TAVERN_BRAWL);
				BnetPresenceMgr.Get().SetGameFieldBlob(22u, sessionRecord);
			}
			break;
		}
		return false;
	}

	private void ShowSessionLimitWarning()
	{
		int numSessionsAllowed = Get().NumSessionsAllowedThisSeason;
		int numSessionsRemaining = Get().NumSessionsAvailableForPurchase;
		if (numSessionsAllowed == 0)
		{
			return;
		}
		string sessionLimitBody;
		if (numSessionsRemaining == 0)
		{
			sessionLimitBody = GameStrings.Get("GLUE_HEROIC_BRAWL_SESSION_LIMIT_ALERT_DESCRIPTION_FINAL");
		}
		else
		{
			if (numSessionsAllowed - numSessionsRemaining <= 1)
			{
				return;
			}
			sessionLimitBody = GameStrings.Format("GLUE_HEROIC_BRAWL_SESSION_LIMIT_ALERT_DESCRIPTION_NORMAL", numSessionsRemaining, numSessionsRemaining);
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		info.m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle;
		info.m_headerText = GameStrings.Get("GLUE_HEROIC_BRAWL_SESSION_LIMIT_ALERT_TITLE");
		info.m_text = sessionLimitBody;
		DialogManager.Get().ShowPopup(info);
	}

	public bool HasCreatedDeck()
	{
		return CurrentDeck() != null;
	}

	public CollectionDeck CurrentDeck()
	{
		return GetDeck(m_currentBrawlType);
	}

	public CollectionDeck GetDeck(BrawlType brawlType, int brawlLibraryItemId = 0)
	{
		TavernBrawlMission mission = GetMission(brawlType);
		if (mission == null)
		{
			return null;
		}
		if (brawlLibraryItemId == 0)
		{
			brawlLibraryItemId = mission.SelectedBrawlLibraryItemId;
		}
		foreach (CollectionDeck deck in CollectionManager.Get().GetDecks().Values)
		{
			if (TranslateDeckTypeToBrawlType(deck.Type) == brawlType && mission.seasonId == deck.SeasonId && brawlLibraryItemId == deck.BrawlLibraryItemId)
			{
				return deck;
			}
		}
		return null;
	}

	public bool HasValidDeckForCurrent()
	{
		return HasValidDeck(m_currentBrawlType);
	}

	public bool HasValidDeck(BrawlType brawlType, int brawlLibraryItemId = 0)
	{
		TavernBrawlMission mission = GetMission(brawlType);
		if (mission == null || !mission.CanCreateDeck(brawlLibraryItemId))
		{
			return false;
		}
		CollectionDeck deck = GetDeck(brawlType, brawlLibraryItemId);
		if (deck == null)
		{
			return false;
		}
		if (!deck.NetworkContentsLoaded())
		{
			CollectionManager.Get().RequestDeckContents(deck.ID);
			return false;
		}
		DeckRuleset deckRuleset = GetDeckRuleset(brawlType, brawlLibraryItemId);
		if (deckRuleset != null && !deckRuleset.IsDeckValid(deck))
		{
			return false;
		}
		return true;
	}

	public static bool IsBrawlDeckType(DeckType deckType)
	{
		return deckType == DeckType.TAVERN_BRAWL_DECK;
	}

	private static BrawlType TranslateDeckTypeToBrawlType(DeckType deckType)
	{
		if (deckType == DeckType.TAVERN_BRAWL_DECK)
		{
			return BrawlType.BRAWL_TYPE_TAVERN_BRAWL;
		}
		return BrawlType.BRAWL_TYPE_UNKNOWN;
	}

	public bool IsTavernBrawlActiveByDeckType(DeckType deckType)
	{
		BrawlType brawlType = TranslateDeckTypeToBrawlType(deckType);
		if (brawlType == BrawlType.BRAWL_TYPE_UNKNOWN)
		{
			return false;
		}
		return IsTavernBrawlActive(brawlType);
	}

	public bool IsSeasonActive(DeckType deckType, int seasonId, int brawlLibraryItemId)
	{
		BrawlType brawlType = TranslateDeckTypeToBrawlType(deckType);
		if (brawlType == BrawlType.BRAWL_TYPE_UNKNOWN)
		{
			return false;
		}
		if (!IsSeasonActive(brawlType, seasonId))
		{
			return false;
		}
		if (brawlLibraryItemId != 0)
		{
			TavernBrawlMission mission = GetMission(brawlType);
			if (mission == null || !mission.BrawlList.Any((GameContentScenario scen) => scen.LibraryItemId == brawlLibraryItemId))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsSeasonActive(BrawlType brawlType, int seasonId)
	{
		if (!IsTavernBrawlActive(brawlType))
		{
			return false;
		}
		TavernBrawlMission mission = m_missions[(int)brawlType];
		if (mission == null || mission.seasonId != seasonId)
		{
			return false;
		}
		return true;
	}

	public void EnsureAllDataReady(CallbackEnsureServerDataReady callback = null)
	{
		EnsureAllDataReady(m_currentBrawlType, callback);
	}

	public void EnsureAllDataReady(BrawlType brawlType, CallbackEnsureServerDataReady callback = null)
	{
		TavernBrawlMission mission = GetMission(brawlType);
		if (mission == null)
		{
			return;
		}
		if (IsAllDataReady(brawlType))
		{
			if (callback != null)
			{
				callback();
			}
			return;
		}
		if (callback != null)
		{
			if (m_serverDataReadyCallbacks == null)
			{
				m_serverDataReadyCallbacks = new List<CallbackEnsureServerDataReady>();
			}
			m_serverDataReadyCallbacks.Add(callback);
		}
		_ = mission.tavernBrawlSpec;
		List<AssetRecordInfo> requestInfos = new List<AssetRecordInfo>();
		foreach (GameContentScenario scen in mission.BrawlList)
		{
			AssetRecordInfo info = new AssetRecordInfo();
			info.Asset = new AssetKey();
			info.Asset.Type = AssetType.ASSET_TYPE_SCENARIO;
			info.Asset.AssetId = scen.ScenarioId;
			info.RecordByteSize = scen.ScenarioRecordByteSize;
			info.RecordHash = scen.ScenarioRecordHash;
			requestInfos.Add(info);
			if (scen.AdditionalAssets != null && scen.AdditionalAssets.Count > 0)
			{
				requestInfos.AddRange(scen.AdditionalAssets);
			}
		}
		if (DownloadableDbfCache.Get().IsAssetRequestInProgress(mission.missionId, AssetType.ASSET_TYPE_SCENARIO))
		{
			DownloadableDbfCache.Get().LoadCachedAssets(canRequestFromServer: false, delegate(AssetKey requestedKey, ErrorCode code, byte[] assetBytes)
			{
				OnDownloadableDbfAssetsLoaded(requestedKey, code, assetBytes, brawlType);
			}, requestInfos.ToArray());
		}
		else if (HearthstoneApplication.IsInternal())
		{
			Processor.ScheduleCallback(Mathf.Max(0f, UnityEngine.Random.Range(-3f, 3f)), realTime: false, delegate
			{
				TavernBrawlManager tavernBrawlManager = Get();
				if (tavernBrawlManager.IsAllDataReady(brawlType))
				{
					if (callback != null)
					{
						if (tavernBrawlManager.m_serverDataReadyCallbacks != null)
						{
							tavernBrawlManager.m_serverDataReadyCallbacks.Remove(callback);
						}
						callback();
					}
				}
				else
				{
					GameContentSeasonSpec gameContentSeason = mission.tavernBrawlSpec.GameContentSeason;
					List<AssetRecordInfo> list = new List<AssetRecordInfo>();
					foreach (GameContentScenario current in gameContentSeason.Scenarios)
					{
						AssetRecordInfo assetRecordInfo = new AssetRecordInfo();
						assetRecordInfo.Asset = new AssetKey();
						assetRecordInfo.Asset.Type = AssetType.ASSET_TYPE_SCENARIO;
						assetRecordInfo.Asset.AssetId = current.ScenarioId;
						assetRecordInfo.RecordByteSize = current.ScenarioRecordByteSize;
						assetRecordInfo.RecordHash = current.ScenarioRecordHash;
						list.Add(assetRecordInfo);
						if (current.AdditionalAssets != null && current.AdditionalAssets.Count > 0)
						{
							list.AddRange(current.AdditionalAssets);
						}
					}
					DownloadableDbfCache.Get().LoadCachedAssets(canRequestFromServer: true, delegate(AssetKey requestedKey, ErrorCode code, byte[] assetBytes)
					{
						OnDownloadableDbfAssetsLoaded(requestedKey, code, assetBytes, brawlType);
					}, list.ToArray());
				}
			});
		}
		else
		{
			DownloadableDbfCache.Get().LoadCachedAssets(canRequestFromServer: true, delegate(AssetKey requestedKey, ErrorCode code, byte[] assetBytes)
			{
				OnDownloadableDbfAssetsLoaded(requestedKey, code, assetBytes, brawlType);
			}, requestInfos.ToArray());
		}
	}

	private bool IsAllDataReady(BrawlType brawlType)
	{
		if (brawlType < BrawlType.BRAWL_TYPE_TAVERN_BRAWL || brawlType >= BrawlType.BRAWL_TYPE_COUNT)
		{
			return true;
		}
		TavernBrawlMission mission = GetMission(brawlType);
		if (mission == null)
		{
			return true;
		}
		if (m_downloadableDbfAssetsPendingLoad[(int)brawlType])
		{
			return false;
		}
		if (mission.BrawlList.Any((GameContentScenario brawl) => GameDbf.Scenario.GetRecord(brawl.ScenarioId) == null))
		{
			return false;
		}
		return true;
	}

	public void RefreshServerData(BrawlType brawlType = BrawlType.BRAWL_TYPE_UNKNOWN)
	{
		brawlType = ((brawlType == BrawlType.BRAWL_TYPE_UNKNOWN) ? m_currentBrawlType : brawlType);
		Network.Get().RequestTavernBrawlInfo(brawlType);
	}

	public bool HasUnlockedTavernBrawl(BrawlType brawlType)
	{
		if (brawlType == BrawlType.BRAWL_TYPE_TAVERN_BRAWL)
		{
			return GameModeUtils.HasUnlockedMode(Global.UnlockableGameMode.TAVERN_BRAWL);
		}
		return true;
	}

	public bool CanChallengeToTavernBrawl(BrawlType brawlType)
	{
		if (!GameUtils.IsTraditionalTutorialComplete())
		{
			return false;
		}
		if (!IsTavernBrawlActive(brawlType))
		{
			return false;
		}
		TavernBrawlMission mission = GetMission(brawlType);
		if (GameUtils.IsAIMission(mission.missionId))
		{
			return false;
		}
		if (mission.friendlyChallengeDisabled)
		{
			return false;
		}
		return true;
	}

	public bool IsEligibleForFreeTicket()
	{
		if (CurrentSession == null || CurrentMission() == null)
		{
			return false;
		}
		uint sessionCount = CurrentSession.SessionCount;
		uint freeSessions = CurrentMission().FreeSessions;
		if (freeSessions != 0)
		{
			return sessionCount < freeSessions;
		}
		return false;
	}

	public bool IsTavernBrawlActive(BrawlType brawlType)
	{
		if (m_missions[(int)brawlType] != null)
		{
			return TavernBrawlSeasonEndInSeconds(brawlType) > 0;
		}
		return false;
	}

	public void RefreshPlayerRecord()
	{
		Network.Get().RequestTavernBrawlPlayerRecord(m_currentBrawlType);
	}

	public long TavernBrawlSeasonStartInSeconds(BrawlType brawlType)
	{
		DateTime? datetime = m_nextSeasonStartDates[(int)brawlType];
		if (!datetime.HasValue || !datetime.HasValue)
		{
			return -1L;
		}
		return (long)(datetime.Value - DateTime.Now).TotalSeconds;
	}

	public float ScheduledSecondsToRefresh(BrawlType brawlType)
	{
		DateTime? datetime = m_scheduledRefreshTimes[(int)brawlType];
		if (!datetime.HasValue || !datetime.HasValue)
		{
			return -1f;
		}
		return (float)(datetime.Value - DateTime.Now).TotalSeconds;
	}

	public long TavernBrawlSeasonNewSessionsClosedInSeconds(BrawlType brawlType)
	{
		TavernBrawlMission mission = GetMission(brawlType);
		if (mission == null || !mission.closedToNewSessionsDateLocal.HasValue)
		{
			return 2147483647L;
		}
		return (long)(mission.closedToNewSessionsDateLocal.Value - DateTime.Now).TotalSeconds;
	}

	public bool IsSeasonSessionBased(BrawlType brawlType)
	{
		return GetMission(brawlType)?.IsSessionBased ?? false;
	}

	public TavernBrawlMode GetBrawlModeForBrawlType(BrawlType brawlType)
	{
		return GetMission(brawlType)?.brawlMode ?? TavernBrawlMode.TB_MODE_NORMAL;
	}

	public void RequestSessionBegin()
	{
		Network.Get().RequestTavernBrawlSessionBegin();
	}

	private void RegisterOptionsListeners(bool register)
	{
		if (register)
		{
			NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheClientOptions), NetCache_OnClientOptions);
			Options.Get().RegisterChangedListener(Option.LATEST_SEEN_TAVERNBRAWL_SEASON, OnOptionChangedCallback);
			Options.Get().RegisterChangedListener(Option.LATEST_SEEN_TAVERNBRAWL_SEASON_CHALKBOARD, OnOptionChangedCallback);
		}
		else
		{
			NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheClientOptions), NetCache_OnClientOptions);
			Options.Get().UnregisterChangedListener(Option.LATEST_SEEN_TAVERNBRAWL_SEASON, OnOptionChangedCallback);
			Options.Get().UnregisterChangedListener(Option.LATEST_SEEN_TAVERNBRAWL_SEASON_CHALKBOARD, OnOptionChangedCallback);
		}
	}

	private void NetCache_OnClientOptions()
	{
		RegisterOptionsListeners(register: false);
		bool seasonChanged = CheckLatestSeenSeason(canSetOption: true);
		CheckLatestSessionLimit(seasonChanged);
		RegisterOptionsListeners(register: true);
	}

	private void OnOptionChangedCallback(Option option, object prevValue, bool existed, object userData)
	{
		RegisterOptionsListeners(register: false);
		bool seasonChanged = CheckLatestSeenSeason(canSetOption: false);
		CheckLatestSessionLimit(seasonChanged);
		RegisterOptionsListeners(register: true);
	}

	private bool CheckLatestSeenSeason(bool canSetOption)
	{
		bool hasChanged = false;
		if (!IsCurrentBrawlInfoReady)
		{
			return hasChanged;
		}
		bool num = !m_hasGottenClientOptionsAtLeastOnce;
		m_hasGottenClientOptionsAtLeastOnce = true;
		bool prevFirstTimeFeature = IsFirstTimeSeeingThisFeature;
		bool prevFirstTimeSeason = CurrentMission() != null && LatestSeenTavernBrawlSeason < CurrentMission().seasonId;
		m_isFirstTimeSeeingThisFeature = false;
		m_isFirstTimeSeeingCurrentSeason = false;
		TavernBrawlMission mission = CurrentMission();
		if (mission != null)
		{
			NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
			bool tavernBrawlEnabled = features != null && features.Games.TavernBrawl && HasUnlockedTavernBrawl(BrawlType.BRAWL_TYPE_TAVERN_BRAWL);
			int latestSeenTavernBrawlSeason = LatestSeenTavernBrawlSeason;
			if (latestSeenTavernBrawlSeason == 0 && tavernBrawlEnabled)
			{
				m_isFirstTimeSeeingThisFeature = true;
				NotificationManager.Get().ForceRemoveSoundFromPlayedList("VO_INNKEEPER_TAVERNBRAWL_PUSH_32.prefab:4f57cd2af5fe5194fbc46c91171ab135");
				NotificationManager.Get().ForceRemoveSoundFromPlayedList("VO_INNKEEPER_TAVERNBRAWL_WELCOME1_27.prefab:094070b7fecad8548b0b8fdb02bde052");
			}
			if (latestSeenTavernBrawlSeason < mission.seasonId && tavernBrawlEnabled)
			{
				m_isFirstTimeSeeingCurrentSeason = true;
				NotificationManager.Get().ForceRemoveSoundFromPlayedList("VO_INNKEEPER_TAVERNBRAWL_DESC2_30.prefab:498657df8d08bc1468bfd1ad9f74ccac");
				if (canSetOption)
				{
					LatestSeenTavernBrawlSeason = mission.seasonId;
				}
				hasChanged = true;
			}
		}
		if ((num || prevFirstTimeFeature != IsFirstTimeSeeingThisFeature || prevFirstTimeSeason != IsFirstTimeSeeingCurrentSeason) && this.OnTavernBrawlUpdated != null)
		{
			this.OnTavernBrawlUpdated();
		}
		return hasChanged;
	}

	private void CheckLatestSessionLimit(bool seasonHasChanged)
	{
		if (!IsCurrentBrawlInfoReady)
		{
			return;
		}
		TavernBrawlMission mission = CurrentMission();
		if (mission == null)
		{
			return;
		}
		if (seasonHasChanged)
		{
			Options.Get().SetInt(Option.LATEST_SEEN_TAVERNBRAWL_SESSION_LIMIT, mission.maxSessions);
			return;
		}
		int lastSessionLimit = Options.Get().GetInt(Option.LATEST_SEEN_TAVERNBRAWL_SESSION_LIMIT);
		if (lastSessionLimit == mission.maxSessions)
		{
			return;
		}
		if (lastSessionLimit == 0)
		{
			Options.Get().SetInt(Option.LATEST_SEEN_TAVERNBRAWL_SESSION_LIMIT, mission.maxSessions);
			return;
		}
		if (mission.maxSessions > lastSessionLimit && this.OnSessionLimitRaised != null)
		{
			this.OnSessionLimitRaised(lastSessionLimit, mission.maxSessions);
		}
		Options.Get().SetInt(Option.LATEST_SEEN_TAVERNBRAWL_SESSION_LIMIT, mission.maxSessions);
	}

	private void ScheduleTimedCallbacksForBrawl(TavernBrawlInfo serverInfo)
	{
		if (serverInfo.HasNextStartSecondsFromNow)
		{
			m_nextSeasonStartDates[(int)serverInfo.BrawlType] = DateTime.Now + new TimeSpan(0, 0, (int)serverInfo.NextStartSecondsFromNow);
		}
		else
		{
			m_nextSeasonStartDates[(int)serverInfo.BrawlType] = null;
		}
		Processor.CancelScheduledCallback(ScheduledEndOfCurrentTBCallback, serverInfo.BrawlType);
		long secondsToEnd = TavernBrawlSeasonEndInSeconds(serverInfo.BrawlType);
		if (IsTavernBrawlActive(serverInfo.BrawlType) && secondsToEnd > 0)
		{
			Log.EventTiming.Print("Scheduling end of current {0} {1} secs from now.", serverInfo.BrawlType, secondsToEnd);
			Processor.ScheduleCallback(secondsToEnd, realTime: true, ScheduledEndOfCurrentTBCallback, serverInfo.BrawlType);
		}
		Processor.CancelScheduledCallback(ScheduledRefreshTBSpecCallback, serverInfo.BrawlType);
		long secondsToNextStart = TavernBrawlSeasonStartInSeconds(serverInfo.BrawlType);
		if (secondsToNextStart >= 0)
		{
			m_scheduledRefreshTimes[(int)serverInfo.BrawlType] = DateTime.Now + new TimeSpan(0, 0, 0, (int)secondsToNextStart, 0);
			Log.EventTiming.Print("Scheduling {0} refresh for {1} secs from now.", serverInfo.BrawlType, secondsToNextStart);
			Processor.ScheduleCallback(secondsToNextStart, realTime: true, ScheduledRefreshTBSpecCallback, serverInfo.BrawlType);
		}
		long secondsToClose = TavernBrawlSeasonNewSessionsClosedInSeconds(serverInfo.BrawlType);
		if (IsSeasonSessionBased(serverInfo.BrawlType) && secondsToClose > 0)
		{
			Log.EventTiming.Print("Scheduling {0} Closed Update for {1} secs from now.", serverInfo.BrawlType, secondsToClose);
			Processor.ScheduleCallback(secondsToClose, realTime: true, ScheduleTBClosedUpdateCallback, serverInfo.BrawlType);
		}
	}

	private void ScheduledEndOfCurrentTBCallback(object userData)
	{
		Log.EventTiming.Print("ScheduledEndOfCurrentTBCallback: ending current TB now.");
		bool viewingRewardsNow = TavernBrawlDisplay.Get() != null && TavernBrawlDisplay.Get().IsInRewards();
		BrawlType brawlType = (BrawlType)userData;
		TavernBrawlMission mission = m_missions[(int)m_currentBrawlType];
		TavernBrawlPlayerRecord record = m_playerRecords[(int)m_currentBrawlType];
		if (mission != null && mission.IsSessionBased && (record.SessionStatus == TavernBrawlStatus.TB_STATUS_ACTIVE || record.SessionStatus == TavernBrawlStatus.TB_STATUS_IN_REWARDS) && (brawlType != m_currentBrawlType || !viewingRewardsNow))
		{
			int getRewardsWaitSeconds = 2;
			getRewardsWaitSeconds = ((mission.SeasonEndSecondsSpreadCount <= 0) ? (getRewardsWaitSeconds + UnityEngine.Random.Range(0, 30)) : (getRewardsWaitSeconds + mission.SeasonEndSecondsSpreadCount));
			Processor.ScheduleCallback(getRewardsWaitSeconds, realTime: true, ScheduledEndOfCurrentTBCallback_AfterSpreadWhenRewardsExpected, brawlType);
		}
		if (brawlType == m_currentBrawlType)
		{
			m_missions[(int)brawlType] = null;
			if (GameMgr.Get().IsFindingGame())
			{
				GameMgr.Get().CancelFindGame();
			}
			if (this.OnTavernBrawlUpdated != null)
			{
				this.OnTavernBrawlUpdated();
			}
		}
	}

	private void ScheduledEndOfCurrentTBCallback_AfterSpreadWhenRewardsExpected(object userData)
	{
		BrawlType brawlType = (BrawlType)userData;
		Network.Get().RequestTavernBrawlPlayerRecord(brawlType);
	}

	private void ScheduledRefreshTBSpecCallback(object userData)
	{
		BrawlType brawlType = (BrawlType)userData;
		Log.EventTiming.Print("ScheduledRefreshTBSpecCallback: refreshing now.");
		RefreshServerData(brawlType);
	}

	private void ScheduleTBClosedUpdateCallback(object userData)
	{
		BrawlType num = (BrawlType)userData;
		Log.EventTiming.Print("ScheduledUpdateTBCallback: updating now.");
		if (num == m_currentBrawlType && this.OnTavernBrawlUpdated != null)
		{
			this.OnTavernBrawlUpdated();
		}
	}

	private void OnDownloadableDbfAssetsLoaded(AssetKey requestedKey, ErrorCode code, byte[] assetBytes, BrawlType brawlType)
	{
		if (requestedKey == null || requestedKey.Type != AssetType.ASSET_TYPE_SCENARIO)
		{
			Log.TavernBrawl.Print("OnDownloadableDbfAssetsLoaded bad AssetType assetId={0} assetType={1} {2}", requestedKey?.AssetId ?? 0, (int)(requestedKey?.Type ?? ((AssetType)0)), (requestedKey == null) ? "(null)" : requestedKey.Type.ToString());
			return;
		}
		if (assetBytes == null || assetBytes.Length == 0)
		{
			Log.TavernBrawl.PrintError("OnDownloadableDbfAssetsLoaded failed to load Asset: assetId={0} assetType={1} {2} error={3}", requestedKey?.AssetId ?? 0, (int)(requestedKey?.Type ?? ((AssetType)0)), (requestedKey == null) ? "(null)" : requestedKey.Type.ToString(), code);
			return;
		}
		TavernBrawlMission mission = m_missions[(int)brawlType];
		if (mission == null)
		{
			return;
		}
		ScenarioDbRecord protoScenario = ProtobufUtil.ParseFrom<ScenarioDbRecord>(assetBytes, 0, assetBytes.Length);
		if (mission.BrawlList.Count != 0 && mission.BrawlList.First().ScenarioId == protoScenario.Id)
		{
			m_downloadableDbfAssetsPendingLoad[(int)brawlType] = false;
			if (m_currentBrawlType == brawlType)
			{
				Processor.RunCoroutine(OnDownloadableDbfAssetsLoaded_EnsureCurrentBrawlDeckContentsLoaded());
			}
		}
	}

	private IEnumerator OnDownloadableDbfAssetsLoaded_EnsureCurrentBrawlDeckContentsLoaded()
	{
		foreach (CollectionDeck deck in CollectionManager.Get().GetDecks().Values)
		{
			if (TranslateDeckTypeToBrawlType(deck.Type) == m_currentBrawlType && !deck.NetworkContentsLoaded())
			{
				CollectionManager.Get().RequestDeckContents(deck.ID);
			}
		}
		if (CurrentMission() != null && !CurrentBrawlDeckContentsLoaded)
		{
			float timeAtStart = Time.realtimeSinceStartup;
			bool done = false;
			while (!done)
			{
				yield return null;
				if (Time.realtimeSinceStartup - timeAtStart > 30f)
				{
					done = true;
				}
				else if (!IsCurrentBrawlAllDataReady)
				{
					done = true;
				}
				else if (CurrentMission() == null)
				{
					done = true;
				}
				else if (CurrentBrawlDeckContentsLoaded)
				{
					done = true;
				}
			}
		}
		if (!IsCurrentBrawlAllDataReady)
		{
			yield break;
		}
		if (this.OnTavernBrawlUpdated != null)
		{
			this.OnTavernBrawlUpdated();
		}
		if (m_serverDataReadyCallbacks != null)
		{
			CallbackEnsureServerDataReady[] callbacks = m_serverDataReadyCallbacks.ToArray();
			m_serverDataReadyCallbacks.Clear();
			for (int i = 0; i < callbacks.Length; i++)
			{
				callbacks[i]();
			}
		}
	}

	private long TavernBrawlSeasonEndInSeconds(BrawlType brawlType)
	{
		TavernBrawlMission mission = m_missions[(int)brawlType];
		if (mission == null)
		{
			return -1L;
		}
		if (!mission.endDateLocal.HasValue)
		{
			return 2147483647L;
		}
		return (long)(mission.endDateLocal.Value - DateTime.Now).TotalSeconds;
	}

	private void OnTavernBrawlRecord_Internal(TavernBrawlPlayerRecord record)
	{
		if (record != null)
		{
			m_playerRecords[(int)record.BrawlType] = record;
			if (m_currentBrawlType == record.BrawlType && this.OnTavernBrawlUpdated != null)
			{
				this.OnTavernBrawlUpdated();
			}
		}
	}

	private void OnTavernBrawlInfo_Internal(TavernBrawlInfo serverInfo)
	{
		if (serverInfo == null)
		{
			return;
		}
		int brawlTypeArrayIndex = (int)serverInfo.BrawlType;
		if (brawlTypeArrayIndex < 0 || brawlTypeArrayIndex >= m_missions.Length)
		{
			Log.TavernBrawl.PrintError("OnTavernBrawlInfo_Internal: received invalid index for BrawlType={0} arrayLength={1}", brawlTypeArrayIndex, m_missions.Length);
			return;
		}
		if (!serverInfo.HasCurrentTavernBrawl)
		{
			m_missions[brawlTypeArrayIndex] = null;
		}
		else
		{
			if (m_missions[brawlTypeArrayIndex] == null)
			{
				m_missions[brawlTypeArrayIndex] = new TavernBrawlMission();
			}
			m_missions[brawlTypeArrayIndex].SetSeasonSpec(serverInfo.CurrentTavernBrawl, serverInfo.BrawlType);
			m_downloadableDbfAssetsPendingLoad[brawlTypeArrayIndex] = true;
			if (this.OnTavernBrawlUpdated != null)
			{
				EnsureAllDataReady(serverInfo.BrawlType);
			}
		}
		bool seasonHasChanged = CheckLatestSeenSeason(canSetOption: true);
		CheckLatestSessionLimit(seasonHasChanged);
		ScheduleTimedCallbacksForBrawl(serverInfo);
		if (serverInfo.HasMyRecord)
		{
			OnTavernBrawlRecord_Internal(serverInfo.MyRecord);
		}
		if (m_currentBrawlType == serverInfo.BrawlType && this.OnTavernBrawlUpdated != null)
		{
			this.OnTavernBrawlUpdated();
		}
	}

	private void OnBeginSession()
	{
		Log.TavernBrawl.Print($"TavernBrawlManager.OnBeginSession");
		TavernBrawlRequestSessionBeginResponse beginTavernBrawl = Network.Get().GetTavernBrawlSessionBegin();
		if (beginTavernBrawl.HasErrorCode && beginTavernBrawl.ErrorCode != 0)
		{
			string logError = beginTavernBrawl.ErrorCode.ToString();
			Debug.LogWarning("TavernBrawlManager.OnBeginSession: Got Error " + beginTavernBrawl.ErrorCode.ToString() + " : " + logError);
			if (SceneMgr.Get().IsSceneLoaded() && (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.TAVERN_BRAWL) || Get().PlayerStatus != TavernBrawlStatus.TB_STATUS_ACTIVE))
			{
				if (TavernBrawlStore.Get() != null)
				{
					TavernBrawlStore.Get().Hide();
				}
				if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
				{
					SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
				}
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
				if (CurrentMission().brawlMode == TavernBrawlMode.TB_MODE_HEROIC)
				{
					info.m_headerText = GameStrings.Get("GLUE_HEROIC_BRAWL_SESSION_ERROR_TITLE");
					info.m_text = GameStrings.Get("GLUE_HEROIC_BRAWL_SESSION_ERROR");
				}
				else
				{
					info.m_headerText = GameStrings.Get("GLUE_BRAWLISEUM_SESSION_ERROR_TITLE");
					info.m_text = GameStrings.Get("GLUE_BRAWLISEUM_SESSION_ERROR");
				}
				info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
				DialogManager.Get().ShowPopup(info);
			}
		}
		else
		{
			SessionRecord sessionRecord = new SessionRecord();
			sessionRecord.Wins = 0u;
			sessionRecord.Losses = 0u;
			sessionRecord.RunFinished = false;
			sessionRecord.SessionRecordType = SessionRecordType.TAVERN_BRAWL;
			BnetPresenceMgr.Get().SetGameFieldBlob(22u, sessionRecord);
			if (beginTavernBrawl.HasPlayerRecord)
			{
				OnTavernBrawlRecord_Internal(beginTavernBrawl.PlayerRecord);
			}
			ShowSessionLimitWarning();
			if (this.OnTavernBrawlUpdated != null)
			{
				this.OnTavernBrawlUpdated();
			}
		}
	}

	private void OnRetireSession()
	{
		Log.TavernBrawl.Print($"TavernBrawlManager.OnRetireSession");
		CollectionManager.Get()?.DoneEditing();
		TavernBrawlRequestSessionRetireResponse retiredTavernBrawl = Network.Get().GetTavernBrawlSessionRetired();
		if (retiredTavernBrawl.ErrorCode != 0)
		{
			string logError = retiredTavernBrawl.ErrorCode.ToString();
			Debug.LogWarning("TavernBrawlManager.OnRetireSession: Got Error " + retiredTavernBrawl.ErrorCode.ToString() + " : " + logError);
			return;
		}
		if (retiredTavernBrawl.HasPlayerRecord)
		{
			OnTavernBrawlRecord_Internal(retiredTavernBrawl.PlayerRecord);
		}
		MyRecord.SessionStatus = TavernBrawlStatus.TB_STATUS_IN_REWARDS;
		CurrentSession.Chest = retiredTavernBrawl.Chest;
		if (this.OnTavernBrawlUpdated != null)
		{
			this.OnTavernBrawlUpdated();
		}
	}

	private void OnAckRewards()
	{
		Log.TavernBrawl.Print($"TavernBrawlManager.OnAckRewards");
		SessionRecord sessionRecord = new SessionRecord();
		sessionRecord.Wins = (uint)GamesWon;
		sessionRecord.Losses = (uint)GamesLost;
		sessionRecord.RunFinished = true;
		sessionRecord.SessionRecordType = ((CurrentSeasonBrawlMode != 0) ? SessionRecordType.HEROIC_BRAWL : SessionRecordType.TAVERN_BRAWL);
		BnetPresenceMgr.Get().SetGameFieldBlob(22u, sessionRecord);
		Network.Get().RequestTavernBrawlPlayerRecord(m_currentBrawlType);
		if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
		}
	}

	private void OnTavernBrawlRecord()
	{
		TavernBrawlPlayerRecord record = Network.Get().GetTavernBrawlRecord();
		OnTavernBrawlRecord_Internal(record);
	}

	private void OnTavernBrawlInfo()
	{
		TavernBrawlInfo serverInfo = Network.Get().GetTavernBrawlInfo();
		OnTavernBrawlInfo_Internal(serverInfo);
	}

	public void Cheat_SetScenario(int scenarioId, BrawlType brawlType = BrawlType.BRAWL_TYPE_TAVERN_BRAWL)
	{
		if (!HearthstoneApplication.IsPublic())
		{
			IsCheated = true;
			int brawlTypeArrayIndex = (int)brawlType;
			if (m_missions[brawlTypeArrayIndex] == null)
			{
				m_missions[brawlTypeArrayIndex] = new TavernBrawlMission();
			}
			m_missions[brawlTypeArrayIndex].SetSeasonSpec(new TavernBrawlSeasonSpec(), brawlType);
			m_missions[brawlTypeArrayIndex].tavernBrawlSpec.GameContentSeason.Scenarios.Add(new GameContentScenario());
			m_missions[brawlTypeArrayIndex].tavernBrawlSpec.GameContentSeason.Scenarios[0].ScenarioId = scenarioId;
			m_downloadableDbfAssetsPendingLoad[(int)brawlType] = true;
			if (this.OnTavernBrawlUpdated != null)
			{
				this.OnTavernBrawlUpdated();
			}
			AssetRecordInfo info = new AssetRecordInfo();
			info.Asset = new AssetKey();
			info.Asset.Type = AssetType.ASSET_TYPE_SCENARIO;
			info.Asset.AssetId = scenarioId;
			info.RecordByteSize = 0u;
			info.RecordHash = null;
			DownloadableDbfCache.Get().LoadCachedAssets(true, delegate(AssetKey requestedKey, ErrorCode code, byte[] assetBytes)
			{
				OnDownloadableDbfAssetsLoaded(requestedKey, code, assetBytes, brawlType);
			}, info);
		}
	}

	public void Cheat_ResetToServerData()
	{
		if (HearthstoneApplication.IsPublic())
		{
			return;
		}
		IsCheated = false;
		OnTavernBrawlInfo();
		if (CurrentMission() != null)
		{
			AssetRecordInfo info = new AssetRecordInfo();
			info.Asset = new AssetKey();
			info.Asset.Type = AssetType.ASSET_TYPE_SCENARIO;
			info.Asset.AssetId = CurrentMission().missionId;
			info.RecordByteSize = 0u;
			info.RecordHash = null;
			DownloadableDbfCache.Get().LoadCachedAssets(true, delegate(AssetKey requestedKey, ErrorCode code, byte[] assetBytes)
			{
				OnDownloadableDbfAssetsLoaded(requestedKey, code, assetBytes, BrawlType.BRAWL_TYPE_TAVERN_BRAWL);
			}, info);
		}
	}

	public void Cheat_ResetSeenStuff(int newValue)
	{
		if (!HearthstoneApplication.IsPublic())
		{
			RegisterOptionsListeners(register: false);
			LatestSeenTavernBrawlChalkboard = newValue;
			LatestSeenTavernBrawlSeason = newValue;
			Options.Get().SetInt(Option.TIMES_SEEN_TAVERNBRAWL_CRAZY_RULES_QUOTE, 0);
			bool seasonChanged = CheckLatestSeenSeason(canSetOption: false);
			CheckLatestSessionLimit(seasonChanged);
			RegisterOptionsListeners(register: true);
		}
	}

	public void Cheat_SetWins(int numWins)
	{
		if (!HearthstoneApplication.IsPublic())
		{
			CurrentSession.Wins = numWins;
			if (this.OnTavernBrawlUpdated != null)
			{
				this.OnTavernBrawlUpdated();
			}
		}
	}

	public void Cheat_SetLosses(int numLosses)
	{
		if (!HearthstoneApplication.IsPublic())
		{
			CurrentSession.Losses = numLosses;
			if (this.OnTavernBrawlUpdated != null)
			{
				this.OnTavernBrawlUpdated();
			}
		}
	}

	public void Cheat_SetActiveSession(int status)
	{
		MyRecord.SessionStatus = (TavernBrawlStatus)status;
		TavernBrawlPlayerSession session = new TavernBrawlPlayerSession();
		MyRecord.Session = session;
	}

	public void Cheat_DoHeroicRewards(int wins, TavernBrawlMode mode)
	{
		MyRecord.SessionStatus = TavernBrawlStatus.TB_STATUS_IN_REWARDS;
		CurrentSession.Chest = RewardUtils.GenerateTavernBrawlRewardChest_CHEAT(wins, mode);
		CurrentSession.Wins = wins;
		if (this.OnTavernBrawlUpdated != null)
		{
			this.OnTavernBrawlUpdated();
		}
	}
}
