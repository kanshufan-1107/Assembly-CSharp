using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Assets;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Hearthstone.Progression;
using PegasusClient;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class RankMgr
{
	private static RankMgr s_instance;

	private Map<int, int> m_maxStarLevelByLeagueId;

	private int m_maxChestVisualIndex;

	private Map<int, Map<int, LeagueRankDbfRecord>> m_rankConfigByLeagueAndStarLevel;

	private Map<int, Map<LeagueBgPublicRatingEquiv.FormatType, List<LeagueBgPublicRatingEquivDbfRecord>>> m_bgPublicRatingEquiv;

	private bool m_receivedSkipApprenticeResponse;

	private bool m_receivedPostApprenticeQuests;

	private bool m_receivedPostApprenticeRewardTrack;

	private bool m_receivedApprenticeCompletedFlag;

	private RankedPlaySeason m_twistRankedSeason;

	public static readonly AssetReference RANK_CHANGE_TWO_SCOOP_PREFAB_LEGACY = new AssetReference("RankChangeTwoScoop_LEGACY.prefab:c10232b70104d6e42b2dd9e6e1233495");

	public static readonly AssetReference RANK_CHANGE_TWO_SCOOP_PREFAB_NEW = new AssetReference("RankChangeTwoScoop_NEW.prefab:606c949d2ac1a8643a5ab70f4d8f67e6");

	public static readonly AssetReference RANKED_REWARD_DISPLAY_PREFAB = new AssetReference("RankedRewardDisplay.prefab:f95c6e7ec80adde4aa6c2f6df24262ea");

	public static readonly AssetReference RANKED_CARDBACK_PROGRESS_DISPLAY_PREFAB = new AssetReference("RankedCardBackProgressDisplay.prefab:b7a7de3cdf473fe4784b100111f02cbb");

	public static readonly AssetReference RANKED_REWARD_LIST_POPUP = new AssetReference("RankedRewardListPopup.prefab:6ee69b3ca628c0047b9016ffda861c5c");

	public static readonly AssetReference BONUS_STAR_POPUP_PREFAB = new AssetReference("RankedBonusStarsPopUp.prefab:d3e043ebff5163846a986cb55a69760c");

	public static readonly AssetReference RANKED_INTRO_POPUP_PREFAB = new AssetReference("RankedIntroPopUp.prefab:b0edfa4af7328bc4d92b637af2f1c32d");

	private NetCache.NetCacheMedalInfo m_cachedMedalInfo;

	private MedalInfoTranslator m_medalInfoTranslator;

	public bool DidSkipApprenticeThisSession { get; set; }

	public bool IsWaitingForApprenticeComplete { get; set; }

	public bool HasLocalPlayerMedalInfo => NetCache.Get().GetNetObject<NetCache.NetCacheMedalInfo>() != null;

	public bool IsLegendRankInAnyFormat
	{
		get
		{
			foreach (FormatType enumValue in Enum.GetValues(typeof(FormatType)))
			{
				if (enumValue != 0 && IsLegendRank(enumValue))
				{
					return true;
				}
			}
			return false;
		}
	}

	public static RankMgr Get()
	{
		if (s_instance == null)
		{
			s_instance = new RankMgr();
		}
		return s_instance;
	}

	public static void LogMessage(string message, [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int lineNumber = 0)
	{
		string msg = $"[{DateTime.Now} -- {methodName}] - {message} ({sourceFile}:{lineNumber})";
		Debug.LogError(msg);
		TelemetryManager.Client().SendLiveIssue("Gameplay-Option.FormatType", msg);
	}

	public void PostProcessDbfLoad_League()
	{
		m_maxStarLevelByLeagueId = new Map<int, int>();
		m_maxChestVisualIndex = 0;
		m_rankConfigByLeagueAndStarLevel = new Map<int, Map<int, LeagueRankDbfRecord>>();
		foreach (LeagueRankDbfRecord record in GameDbf.LeagueRank.GetRecords())
		{
			if (!m_maxStarLevelByLeagueId.TryGetValue(record.LeagueId, out var maxStarLevel))
			{
				m_maxStarLevelByLeagueId.Add(record.LeagueId, record.StarLevel);
			}
			else if (record.StarLevel > maxStarLevel)
			{
				m_maxStarLevelByLeagueId[record.LeagueId] = record.StarLevel;
			}
			if (record.RewardChestVisualIndex > m_maxChestVisualIndex)
			{
				m_maxChestVisualIndex = record.RewardChestVisualIndex;
			}
			if (!m_rankConfigByLeagueAndStarLevel.TryGetValue(record.LeagueId, out var rankConfigByStarLevel) || rankConfigByStarLevel == null)
			{
				rankConfigByStarLevel = (m_rankConfigByLeagueAndStarLevel[record.LeagueId] = new Map<int, LeagueRankDbfRecord>());
			}
			rankConfigByStarLevel[record.StarLevel] = record;
		}
		BnetRegion currentRegion = BattleNet.GetCurrentRegion();
		m_bgPublicRatingEquiv = new Map<int, Map<LeagueBgPublicRatingEquiv.FormatType, List<LeagueBgPublicRatingEquivDbfRecord>>>();
		foreach (LeagueBgPublicRatingEquivDbfRecord record2 in GameDbf.LeagueBgPublicRatingEquiv.GetRecords())
		{
			if (record2.Region == LeagueBgPublicRatingEquiv.Region.REGION_UNKNOWN || record2.Region == (LeagueBgPublicRatingEquiv.Region)currentRegion)
			{
				if (!m_bgPublicRatingEquiv.TryGetValue(record2.LeagueId, out var recordsForLeague))
				{
					recordsForLeague = new Map<LeagueBgPublicRatingEquiv.FormatType, List<LeagueBgPublicRatingEquivDbfRecord>>();
					m_bgPublicRatingEquiv[record2.LeagueId] = recordsForLeague;
				}
				if (!recordsForLeague.TryGetValue(record2.FormatType, out var recordsForFormat))
				{
					recordsForFormat = new List<LeagueBgPublicRatingEquivDbfRecord>();
					recordsForLeague[record2.FormatType] = recordsForFormat;
				}
				recordsForFormat.Add(record2);
			}
		}
		foreach (Map<LeagueBgPublicRatingEquiv.FormatType, List<LeagueBgPublicRatingEquivDbfRecord>> value in m_bgPublicRatingEquiv.Values)
		{
			foreach (List<LeagueBgPublicRatingEquivDbfRecord> value2 in value.Values)
			{
				value2.Sort((LeagueBgPublicRatingEquivDbfRecord a, LeagueBgPublicRatingEquivDbfRecord b) => (a.StarLevel != b.StarLevel) ? a.StarLevel.CompareTo(b.StarLevel) : b.LegendRank.CompareTo(a.LegendRank));
			}
		}
	}

	public bool UseLegacyRankedPlay(int leagueId)
	{
		LeagueDbfRecord leagueRecord = GetLeagueRecord(leagueId);
		if (leagueRecord != null)
		{
			switch (leagueRecord.LeagueType)
			{
			case League.LeagueType.NORMAL:
				return leagueRecord.LeagueVersion <= 2;
			case League.LeagueType.NEW_PLAYER:
				return leagueRecord.LeagueVersion <= 2;
			}
		}
		return false;
	}

	public MedalInfoTranslator GetLocalPlayerMedalInfo()
	{
		NetCache.NetCacheMedalInfo cachedMedalInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMedalInfo>();
		if (cachedMedalInfo == null)
		{
			Log.All.PrintError("NetCacheMedalInfo not yet available!");
			return new MedalInfoTranslator();
		}
		if (m_cachedMedalInfo != cachedMedalInfo)
		{
			m_cachedMedalInfo = cachedMedalInfo;
			m_medalInfoTranslator = new MedalInfoTranslator(cachedMedalInfo, cachedMedalInfo.PreviousMedalInfo);
			return m_medalInfoTranslator;
		}
		return m_medalInfoTranslator;
	}

	public bool WildCardsAllowedInCurrentLeague()
	{
		return GameUtils.HasCompletedApprentice();
	}

	public BnetGameType GetBnetGameTypeForLeague(bool inRankedMode, FormatType format)
	{
		if (!inRankedMode && !GameUtils.HasCompletedApprentice())
		{
			if (format == FormatType.FT_STANDARD)
			{
				return BnetGameType.BGT_CASUAL_STANDARD_APPRENTICE;
			}
			Log.All.PrintError("Apprentice game type expected standard format but was something else!");
		}
		return (BnetGameType)(GetLocalPlayerLeagueConfig(format).LeagueGameType.FirstOrDefault((LeagueGameTypeDbfRecord x) => x.FormatType == (LeagueGameType.FormatType)format && x.IsRankedPlay == inRankedMode)?.BnetGameType ?? LeagueGameType.BnetGameType.BGT_UNKNOWN);
	}

	public bool IsFormatAllowedInLeague(FormatType format)
	{
		if (GetBnetGameTypeForLeague(inRankedMode: false, format) == BnetGameType.BGT_UNKNOWN)
		{
			return GetBnetGameTypeForLeague(inRankedMode: true, format) != BnetGameType.BGT_UNKNOWN;
		}
		return true;
	}

	public bool IsLegendRank(FormatType formatType)
	{
		return GetLocalPlayerMedalInfo().GetCurrentMedal(formatType).IsLegendRank();
	}

	public bool IsNewPlayer()
	{
		if (!Network.ShouldBeConnectedToAurora())
		{
			return false;
		}
		if (GameUtils.HasCompletedApprentice())
		{
			return false;
		}
		return true;
	}

	public void SetRankPresenceField()
	{
		GamePresenceRank gamePresenceRank = CalculateGamePresenceRank();
		BnetPresenceMgr.Get().SetGameFieldBlob(18u, gamePresenceRank);
	}

	private GamePresenceRank CalculateGamePresenceRank()
	{
		GamePresenceRank gamePresenceRank = new GamePresenceRank();
		NetCache netCache = NetCache.Get();
		if (netCache == null)
		{
			return gamePresenceRank;
		}
		SceneMgr sceneMgr = SceneMgr.Get();
		GameMgr gameMgr = GameMgr.Get();
		NetCache.NetCacheFeatures netCacheFeatures = netCache.GetNetObject<NetCache.NetCacheFeatures>();
		bool battlegroundsMedalFriendListDisplayFeatureDisabled = netCacheFeatures == null || !netCacheFeatures.BattlegroundsMedalFriendListDisplayEnabled;
		bool forceShowRanked = battlegroundsMedalFriendListDisplayFeatureDisabled || (sceneMgr != null && sceneMgr.GetMode() == SceneMgr.Mode.TOURNAMENT) || (sceneMgr != null && sceneMgr.GetMode() == SceneMgr.Mode.GAMEPLAY && gameMgr != null && gameMgr.IsRankedPlay());
		bool forceShowBg = !forceShowRanked && ((sceneMgr != null && sceneMgr.GetMode() == SceneMgr.Mode.BACON) || (sceneMgr != null && sceneMgr.GetMode() == SceneMgr.Mode.GAMEPLAY && gameMgr != null && gameMgr.IsBattlegrounds()));
		bool playingBGDuos = sceneMgr != null && sceneMgr.GetMode() == SceneMgr.Mode.GAMEPLAY && gameMgr != null && gameMgr.IsBattlegroundDuoGame();
		MedalInfoData selectedMedalInfoData = null;
		int bgPublicRatingEquiv = 0;
		if (forceShowRanked)
		{
			FormatType formatType = ((!battlegroundsMedalFriendListDisplayFeatureDisabled) ? Options.GetFormatType() : GetLocalPlayerMedalInfo().GetBestCurrentRankFormatType());
			selectedMedalInfoData = netCache.GetNetObject<NetCache.NetCacheMedalInfo>()?.GetMedalInfoData(formatType);
		}
		if (selectedMedalInfoData == null && !forceShowBg)
		{
			foreach (FormatType formatType2 in Enum.GetValues(typeof(FormatType)))
			{
				if (formatType2 != 0)
				{
					MedalInfoData medalInfoData2 = netCache.GetNetObject<NetCache.NetCacheMedalInfo>()?.GetMedalInfoData(formatType2);
					if (medalInfoData2 != null && TryCalculateBgPublicRatingEquiv(medalInfoData2, out var tempBgPublicRatingEquiv2) && tempBgPublicRatingEquiv2 >= bgPublicRatingEquiv)
					{
						selectedMedalInfoData = medalInfoData2;
						bgPublicRatingEquiv = tempBgPublicRatingEquiv2;
					}
				}
			}
		}
		int bgPublicRating = 0;
		GameType bgGameType = GameType.GT_BATTLEGROUNDS;
		NetCache.NetCacheBaconRatingInfo ratingInfo = netCache.GetNetObject<NetCache.NetCacheBaconRatingInfo>();
		if (ratingInfo != null)
		{
			bgPublicRating = ratingInfo.Rating;
			if (ratingInfo.DuosRating > ratingInfo.Rating || playingBGDuos)
			{
				bgPublicRating = ratingInfo.DuosRating;
				bgGameType = GameType.GT_BATTLEGROUNDS_DUO;
			}
		}
		if (!forceShowRanked && !forceShowBg && bgPublicRatingEquiv == 0 && bgPublicRating == 0)
		{
			return gamePresenceRank;
		}
		GamePresenceRankData rankData = new GamePresenceRankData();
		if (forceShowRanked || (!forceShowBg && bgPublicRatingEquiv >= bgPublicRating))
		{
			if (selectedMedalInfoData == null)
			{
				return gamePresenceRank;
			}
			rankData = new GamePresenceRankData
			{
				FormatType = selectedMedalInfoData.FormatType,
				LeagueId = selectedMedalInfoData.LeagueId,
				StarLevel = selectedMedalInfoData.StarLevel,
				LegendRank = selectedMedalInfoData.LegendRank,
				GameType = GameType.GT_RANKED,
				Rating = -1
			};
		}
		else if (forceShowBg || bgPublicRatingEquiv < bgPublicRating)
		{
			rankData = new GamePresenceRankData
			{
				FormatType = FormatType.FT_UNKNOWN,
				LeagueId = 0,
				StarLevel = 0,
				LegendRank = 0,
				GameType = bgGameType,
				Rating = bgPublicRating
			};
		}
		gamePresenceRank.Values.Add(rankData);
		return gamePresenceRank;
		bool TryCalculateBgPublicRatingEquiv(MedalInfoData medalInfoData, out int tempBgPublicRatingEquiv)
		{
			tempBgPublicRatingEquiv = 0;
			if (GetLeagueRecord(medalInfoData.LeagueId) == null)
			{
				return false;
			}
			if (!m_bgPublicRatingEquiv.TryGetValue(medalInfoData.LeagueId, out var recordsForLeague))
			{
				Debug.LogError($"No LEAGUE_BG_PUBLIC_RATING_EQUIV record found for League={medalInfoData.LeagueId}");
				return false;
			}
			if (!recordsForLeague.TryGetValue((LeagueBgPublicRatingEquiv.FormatType)medalInfoData.FormatType, out var recordsForFormat) && !recordsForLeague.TryGetValue(LeagueBgPublicRatingEquiv.FormatType.FT_UNKNOWN, out recordsForFormat))
			{
				Debug.LogError("No LEAGUE_BG_PUBLIC_RATING_EQUIV record found for " + $"League={medalInfoData.LeagueId} Format={medalInfoData.FormatType}");
				return false;
			}
			if (recordsForFormat.Count == 0 || recordsForFormat[0].StarLevel != 1)
			{
				Debug.LogError("No LEAGUE_BG_PUBLIC_RATING_EQUIV record found for StarLevel 1 for" + $"League={medalInfoData.LeagueId} Format={medalInfoData.FormatType}");
				return false;
			}
			bool wasValueFound = false;
			LeagueBgPublicRatingEquivDbfRecord prevRecord = null;
			foreach (LeagueBgPublicRatingEquivDbfRecord currRecord in recordsForFormat)
			{
				if (currRecord.StarLevel >= medalInfoData.StarLevel && (currRecord.StarLevel != medalInfoData.StarLevel || currRecord.LegendRank <= medalInfoData.LegendRank))
				{
					if (currRecord.StarLevel == medalInfoData.StarLevel && currRecord.LegendRank == medalInfoData.LegendRank)
					{
						tempBgPublicRatingEquiv = currRecord.BgPublicRatingEquiv;
						wasValueFound = true;
					}
					else
					{
						LeagueBgPublicRatingEquivDbfRecord lowerBound = prevRecord;
						LeagueBgPublicRatingEquivDbfRecord upperBound = currRecord;
						tempBgPublicRatingEquiv = (int)Mathf.Lerp(t: (currRecord.StarLevel <= medalInfoData.StarLevel) ? ((float)(medalInfoData.LegendRank - lowerBound.LegendRank) / (float)(upperBound.LegendRank - lowerBound.LegendRank)) : ((float)(medalInfoData.StarLevel - lowerBound.StarLevel) / (float)(upperBound.StarLevel - lowerBound.StarLevel)), a: lowerBound.BgPublicRatingEquiv, b: upperBound.BgPublicRatingEquiv);
						wasValueFound = true;
					}
					break;
				}
				prevRecord = currRecord;
			}
			if (!wasValueFound && prevRecord != null)
			{
				tempBgPublicRatingEquiv = prevRecord.BgPublicRatingEquiv;
			}
			return true;
		}
	}

	public MedalInfoTranslator GetRankedMedalFromRankPresenceField(BnetPlayer player)
	{
		if (player == null)
		{
			return null;
		}
		return GetRankedMedalFromRankPresenceField(player.GetHearthstoneGameAccount());
	}

	public MedalInfoTranslator GetRankedMedalFromRankPresenceField(BnetGameAccount gameAccount)
	{
		if (gameAccount != null && gameAccount.TryGetGameFieldBytes(18u, out var blob))
		{
			try
			{
				return MedalInfoTranslator.CreateMedalInfoForGamePresenceRank(ProtobufUtil.ParseFrom<GamePresenceRank>(blob));
			}
			catch (Exception ex)
			{
				Log.Presence.PrintInfo(ex.ToString());
			}
		}
		return new MedalInfoTranslator();
	}

	public bool GetBattlegroundsMedalFromRankPresenceField(BnetGameAccount gameAccount, out int bgRating, out GameType gameType)
	{
		gameType = GameType.GT_UNKNOWN;
		if (gameAccount != null && gameAccount.TryGetGameFieldBytes(18u, out var blob))
		{
			try
			{
				GamePresenceRankData rankData = ProtobufUtil.ParseFrom<GamePresenceRank>(blob).Values.Where((GamePresenceRankData x) => x.GameType == GameType.GT_BATTLEGROUNDS || x.GameType == GameType.GT_BATTLEGROUNDS_DUO).FirstOrDefault();
				if (rankData != null)
				{
					bgRating = rankData.Rating;
					gameType = rankData.GameType;
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Presence.PrintInfo(ex.ToString());
			}
		}
		bgRating = -1;
		return false;
	}

	public LeagueDbfRecord GetLeagueRecord(int leagueId)
	{
		List<LeagueDbfRecord> leagueDbfRecords = GameDbf.League.GetRecords();
		LeagueDbfRecord record = null;
		int i = 0;
		for (int iMax = leagueDbfRecords.Count; i < iMax; i++)
		{
			LeagueDbfRecord tempRecord = leagueDbfRecords[i];
			if (tempRecord.ID == leagueId)
			{
				record = tempRecord;
				break;
			}
		}
		if (record == null)
		{
			Log.All.PrintError("No record for leagueId={0}", leagueId);
			return new LeagueDbfRecord();
		}
		return record;
	}

	public LeagueRankDbfRecord GetLeagueRankRecord(int leagueId, int starLevel)
	{
		if (m_rankConfigByLeagueAndStarLevel.TryGetValue(leagueId, out var rankConfigByStarLevel) && rankConfigByStarLevel != null && rankConfigByStarLevel.TryGetValue(starLevel, out var record) && record != null)
		{
			return record;
		}
		Log.All.PrintError("No record for leagueId={0} starLevel={1}", leagueId, starLevel);
		return new LeagueRankDbfRecord();
	}

	public LeagueDbfRecord GetLeagueRecordForType(League.LeagueType leagueType, int seasonId)
	{
		LeagueDbfRecord foundRecord = null;
		int highestVersionFound = 0;
		foreach (LeagueDbfRecord record in GameDbf.League.GetRecords())
		{
			if (record.LeagueType == leagueType && record.InitialSeasonId <= seasonId && record.LeagueVersion > highestVersionFound)
			{
				highestVersionFound = record.LeagueVersion;
				foundRecord = record;
			}
		}
		if (foundRecord == null)
		{
			Log.All.PrintError("No record for leagueType={0}", leagueType);
		}
		return foundRecord;
	}

	public LeagueRankDbfRecord GetLeagueRankRecordByCheatName(string cheatName)
	{
		LeagueRankDbfRecord foundRecord = null;
		int highestVersionFound = 0;
		foreach (LeagueRankDbfRecord rankRec in GameDbf.LeagueRank.GetRecords())
		{
			if (rankRec.CheatName == cheatName)
			{
				LeagueDbfRecord leagueRec = GetLeagueRecord(rankRec.LeagueId);
				if (leagueRec.LeagueVersion > highestVersionFound)
				{
					highestVersionFound = leagueRec.LeagueVersion;
					foundRecord = rankRec;
				}
			}
		}
		if (foundRecord == null)
		{
			Log.All.PrintError("No record for cheatName={0}", cheatName);
		}
		return foundRecord;
	}

	public LeagueDbfRecord GetLocalPlayerStandardLeagueConfig()
	{
		return GetLocalPlayerLeagueConfig(FormatType.FT_STANDARD);
	}

	public LeagueDbfRecord GetLocalPlayerLeagueConfig(FormatType format)
	{
		return GetLocalPlayerMedalInfo().GetCurrentMedal(format).LeagueConfig;
	}

	public int GetMaxStarLevel(int leagueId)
	{
		if (!m_maxStarLevelByLeagueId.TryGetValue(leagueId, out var maxStarLevel))
		{
			return 0;
		}
		return maxStarLevel;
	}

	public int GetMaxRewardChestVisualIndex()
	{
		return m_maxChestVisualIndex;
	}

	public bool IsCardLockedInCurrentLeague(EntityDef entityDef)
	{
		if (!GameUtils.IsCardGameplayEventEverActive(entityDef))
		{
			return false;
		}
		if (!GameUtils.HasCompletedApprentice() && (GameUtils.IsWildCard(entityDef) || GameUtils.IsClassicCard(entityDef)))
		{
			return true;
		}
		if (IsCardBannedInCurrentLeague(entityDef))
		{
			return true;
		}
		return false;
	}

	public HashSet<string> GetBannedCardsInCurrentLeague()
	{
		int leagueDeckRulesetId = GetLocalPlayerStandardLeagueConfig().LockCardsFromSubsetId;
		return GameDbf.GetIndex().GetSubsetById(leagueDeckRulesetId);
	}

	public bool IsCardBannedInCurrentLeague(EntityDef entityDef)
	{
		if (GetBannedCardsInCurrentLeague().Contains(entityDef.GetCardId()))
		{
			return true;
		}
		return false;
	}

	public int GetRankedRewardBoosterIdForSeasonId(int seasonId)
	{
		List<BoosterDbfRecord> records = GameDbf.Booster.GetRecords((BoosterDbfRecord r) => r.RankedRewardInitialSeason > 0);
		records.Sort((BoosterDbfRecord a, BoosterDbfRecord b) => b.RankedRewardInitialSeason - a.RankedRewardInitialSeason);
		foreach (BoosterDbfRecord record in records)
		{
			if (seasonId >= record.RankedRewardInitialSeason)
			{
				return record.ID;
			}
		}
		return 1;
	}

	public int GetRankedCardBackIdForSeasonId(int seasonId)
	{
		int cardBackId = 0;
		foreach (CardBackDbfRecord record in GameDbf.CardBack.GetRecords())
		{
			if (record.Source == Assets.CardBack.Source.SEASON && record.Data1 == seasonId)
			{
				cardBackId = record.ID;
				break;
			}
		}
		return cardBackId;
	}

	public static int GetCurrentTwistSeasonId()
	{
		int result = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().TwistSeasonOverride;
		if (result > 0)
		{
			return result;
		}
		NetCache netCache = NetCache.Get();
		if (netCache == null)
		{
			return 0;
		}
		NetCache.NetCacheMedalInfo netCacheMedalInfo = netCache.GetNetObject<NetCache.NetCacheMedalInfo>();
		if (netCacheMedalInfo == null)
		{
			return 0;
		}
		return netCacheMedalInfo.GetMedalInfoData(FormatType.FT_TWIST)?.SeasonId ?? 0;
	}

	public RankedPlaySeason GetCurrentTwistSeason()
	{
		int seasonId = GetCurrentTwistSeasonId();
		if (m_twistRankedSeason != null && seasonId != m_twistRankedSeason.Season)
		{
			m_twistRankedSeason = null;
		}
		if (m_twistRankedSeason == null)
		{
			RankedPlaySeasonDbfRecord twistSeasonRecord = GameDbf.RankedPlaySeason.GetRecord((RankedPlaySeasonDbfRecord record) => record.Season == seasonId);
			if (twistSeasonRecord != null)
			{
				m_twistRankedSeason = new RankedPlaySeason(twistSeasonRecord);
			}
		}
		return m_twistRankedSeason;
	}

	public static int GetSecondsRemainingInCurrentTwistSeason()
	{
		NetCache netCache = NetCache.Get();
		if (netCache == null)
		{
			Debug.LogWarning("RankMgr.GetSecondsRemainingInCurrentSeason: NetCache is null.");
			return 0;
		}
		NetCache.NetCacheMedalInfo netCacheMedalInfo = netCache.GetNetObject<NetCache.NetCacheMedalInfo>();
		if (netCacheMedalInfo == null)
		{
			Debug.LogWarning("RankMgr.GetSecondsRemainingInCurrentSeason: No NetCacheMedalInfo found.");
			return 0;
		}
		MedalInfoData twistMedalInfo = netCacheMedalInfo.GetMedalInfoData(FormatType.FT_TWIST);
		if (twistMedalInfo == null)
		{
			Debug.LogWarning($"RankMgr.GetSecondsRemainingInCurrentSeason: No medal data found for {FormatType.FT_TWIST}");
			return 0;
		}
		return twistMedalInfo.SecondsUntilSeasonEnd;
	}

	public static TimeSpan GetTimeLeftInCurrentSeason()
	{
		int secondsLeftInTwistSeason = GetSecondsRemainingInCurrentTwistSeason();
		TimeSpan timeLeft = DateTime.Now.AddSeconds(secondsLeftInTwistSeason) - DateTime.Now;
		if (timeLeft.TotalSeconds < 0.0)
		{
			timeLeft = TimeSpan.Zero;
		}
		return timeLeft;
	}

	public static bool IsNextSeasonValid()
	{
		int nextSeasonId = GetCurrentTwistSeasonId() + 1;
		return GameDbf.RankedPlaySeason.GetRecord((RankedPlaySeasonDbfRecord record) => record.Season == nextSeasonId) != null;
	}

	public static bool IsCurrentTwistSeasonActive()
	{
		if (Get()?.GetCurrentTwistSeason() == null)
		{
			return false;
		}
		return true;
	}

	public static bool IsCurrentTwistSeasonUsingHeroicDecks()
	{
		return Get()?.GetCurrentTwistSeason()?.UsesPrebuiltDecks == true;
	}

	public static bool IsClassLockedForTwist(List<TAG_CLASS> deckClasses)
	{
		RankedPlaySeason twistSeason = Get()?.GetCurrentTwistSeason();
		if (twistSeason == null)
		{
			return false;
		}
		ScenarioDbfRecord scenarioRecord = twistSeason.GetScenario();
		if (scenarioRecord != null)
		{
			foreach (ClassExclusionsDbfRecord classExclusion in scenarioRecord.ClassExclusions)
			{
				TAG_CLASS excludedClass = (TAG_CLASS)classExclusion.ClassId;
				foreach (TAG_CLASS deckClass in deckClasses)
				{
					if (deckClass == excludedClass)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static bool IsClassLockedForTwist(TAG_CLASS deckClass)
	{
		RankedPlaySeason twistSeason = Get()?.GetCurrentTwistSeason();
		if (twistSeason == null)
		{
			return false;
		}
		ScenarioDbfRecord scenarioRecord = twistSeason.GetScenario();
		if (scenarioRecord != null)
		{
			foreach (ClassExclusionsDbfRecord classExclusion in scenarioRecord.ClassExclusions)
			{
				TAG_CLASS excludedClass = (TAG_CLASS)classExclusion.ClassId;
				if (deckClass == excludedClass)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static List<TAG_CLASS> GetExcludedClassesForFormat(FormatType formatType)
	{
		List<TAG_CLASS> excludedClasses = new List<TAG_CLASS>();
		if (formatType == FormatType.FT_TWIST)
		{
			RankedPlaySeason twistSeason = Get()?.GetCurrentTwistSeason();
			if (twistSeason != null)
			{
				ScenarioDbfRecord scenarioRecord = twistSeason.GetScenario();
				if (scenarioRecord != null)
				{
					foreach (ClassExclusionsDbfRecord classExclusion in scenarioRecord.ClassExclusions)
					{
						excludedClasses.Add((TAG_CLASS)classExclusion.ClassId);
					}
				}
			}
		}
		return excludedClasses;
	}

	public static bool IsTwistDeckWithNoSeason(CollectionDeck deck)
	{
		if (deck != null && deck.FormatType == FormatType.FT_TWIST)
		{
			return !IsCurrentTwistSeasonActive();
		}
		return false;
	}

	public static bool IsTwistDeckValidForSeason(CollectionDeck deck)
	{
		if (deck == null || deck.FormatType != FormatType.FT_TWIST)
		{
			return false;
		}
		RankedPlaySeason season = Get().GetCurrentTwistSeason();
		if (season == null)
		{
			return false;
		}
		if (!season.UsesPrebuiltDecks)
		{
			return true;
		}
		if (!deck.IsDeckTemplate)
		{
			return false;
		}
		return season.GetDeck(deck.DeckTemplateId) != null;
	}

	public void RequestSkipApprentice()
	{
		bool num = RewardTrackManager.Get()?.HasAnyUnclaimedApprenticeRewards() ?? true;
		bool hasUnclaimedTavernGuideRewards = TavernGuideManager.Get()?.HasUnclaimedSetReward() ?? true;
		if (num || hasUnclaimedTavernGuideRewards)
		{
			ShowUnclaimedRewardsSkipError();
			return;
		}
		DidSkipApprenticeThisSession = true;
		IsWaitingForApprenticeComplete = true;
		m_receivedSkipApprenticeResponse = false;
		m_receivedPostApprenticeQuests = false;
		bool hasAlreadyReceivedRewardTrack = (m_receivedPostApprenticeRewardTrack = RewardTrackManager.Get().GetRewardTrack(Global.RewardTrackType.GLOBAL) != null);
		Network network = Network.Get();
		if (network == null)
		{
			return;
		}
		network.RegisterNetHandler(SkipApprenticeResponse.PacketID.ID, OnSkipApprenticeResponse);
		network.RegisterNetHandler(PlayerQuestStateUpdate.PacketID.ID, OnPostApprenticeQuestUpdate);
		if (!hasAlreadyReceivedRewardTrack)
		{
			RewardTrackManager.Get().OnRewardTracksReceived += OnPostApprenticeRewardTrackUpdate;
		}
		GameSaveDataManager.Get().OnGameSaveDataUpdate += OnGameSaveDataUpdate;
		PopupDisplayManager.Get().ResetReadyToShowPopups();
		network.RequestSkipApprentice();
		Box box = Box.Get();
		if (box != null)
		{
			box.ToggleSkipApprenticeLoading(isLoading: true);
			JournalButton journalButton = box.GetJournalButton();
			if (JournalPopup.s_isShowing && journalButton != null)
			{
				journalButton.CloseJournal();
			}
		}
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.HUB)
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
		}
	}

	private void OnSkipApprenticeResponse()
	{
		SkipApprenticeResponse response = Network.Get().GetSkipApprenticeResponse();
		if (response.ErrorCode != 0)
		{
			DidSkipApprenticeThisSession = false;
			IsWaitingForApprenticeComplete = false;
			Box box = Box.Get();
			if (box != null)
			{
				box.ToggleSkipApprenticeLoading(isLoading: false);
			}
			Log.All.PrintError("Player not able to Skip Apprentice. Player={0}, Error={1}", BnetPresenceMgr.Get().GetMyPlayer().GetAccountId(), response.ErrorCode);
			if (response.ErrorCode == ErrorCode.ERROR_APPRENTICE_TRACK_SKIP_WITH_UNCLAIMED_REWARDS)
			{
				ShowUnclaimedRewardsSkipError();
			}
			PopupDisplayManager.Get().ReadyToShowPopups();
		}
		else
		{
			m_receivedSkipApprenticeResponse = true;
			AttemptApprenticeEndedTransition();
		}
	}

	private void OnPostApprenticeQuestUpdate()
	{
		m_receivedPostApprenticeQuests = true;
		AttemptApprenticeEndedTransition();
	}

	private void OnPostApprenticeRewardTrackUpdate()
	{
		m_receivedPostApprenticeRewardTrack = true;
		AttemptApprenticeEndedTransition();
	}

	private void OnGameSaveDataUpdate(GameSaveKeyId key)
	{
		if (key == GameSaveKeyId.PLAYER_FLAGS && GameUtils.HasCompletedApprentice())
		{
			m_receivedApprenticeCompletedFlag = true;
			GameSaveDataManager.Get().OnGameSaveDataUpdate -= OnGameSaveDataUpdate;
			AttemptApprenticeEndedTransition();
		}
	}

	private void AttemptApprenticeEndedTransition()
	{
		bool hasQuestData = m_receivedPostApprenticeQuests || !TavernGuideManager.Get().IsTavernGuideActive();
		if (IsWaitingForApprenticeComplete && hasQuestData && m_receivedSkipApprenticeResponse && m_receivedPostApprenticeRewardTrack && m_receivedApprenticeCompletedFlag)
		{
			HandleApprenticeEnded();
		}
	}

	private void HandleApprenticeEnded()
	{
		if (!IsWaitingForApprenticeComplete)
		{
			return;
		}
		IsWaitingForApprenticeComplete = false;
		DidSkipApprenticeThisSession = true;
		Options.SetInRankedPlayMode(inRankedPlayMode: true);
		Network.Get().RemoveNetHandler(SkipApprenticeResponse.PacketID.ID, OnSkipApprenticeResponse);
		Network.Get().RemoveNetHandler(PlayerQuestStateUpdate.PacketID.ID, OnPostApprenticeQuestUpdate);
		RewardTrackManager.Get().OnRewardTracksReceived -= OnPostApprenticeRewardTrackUpdate;
		BnetBar bnetBar = BnetBar.Get();
		if (bnetBar != null)
		{
			bnetBar.RefreshCurrency();
		}
		Box box = Box.Get();
		if (box != null)
		{
			box.GetRailroadManager().ToggleBoxTutorials(setEnabled: false);
		}
		if (SetRotationManager.Get().ShouldShowSetRotationIntro())
		{
			if (SceneMgr.Get().GetMode() != SceneMgr.Mode.HUB)
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
			}
			else if (box != null)
			{
				box.TryToStartSetRotationFromHub();
			}
		}
		else if (box != null)
		{
			if (box.GetRailroadManager() != null)
			{
				box.GetRailroadManager().UpdateRailroadState();
			}
			box.UpdateUI();
			box.ToggleSkipApprenticeLoading(isLoading: false);
		}
		PopupDisplayManager.Get().ReadyToShowPopups();
	}

	private void ShowUnclaimedRewardsSkipError()
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_LEAGUE_PROMOTE_SELF_UNCLAIMED_REWARDS_ERROR_TITLE"),
			m_text = GameStrings.Get("GLUE_LEAGUE_PROMOTE_SELF_UNCLAIMED_REWARDS_ERROR_DESCRIPTION"),
			m_showAlertIcon = true,
			m_responseDisplay = AlertPopup.ResponseDisplay.OK
		};
		DialogManager.Get().ShowPopup(info);
	}
}
