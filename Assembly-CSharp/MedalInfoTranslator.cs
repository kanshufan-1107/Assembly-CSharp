using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using Hearthstone.DataModels;
using PegasusClient;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class MedalInfoTranslator
{
	private Map<FormatType, TranslatedMedalInfo> m_currMedalInfo = new Map<FormatType, TranslatedMedalInfo>();

	private Map<FormatType, TranslatedMedalInfo> m_prevMedalInfo = new Map<FormatType, TranslatedMedalInfo>();

	public int TotalRankedWins => m_currMedalInfo.Sum((KeyValuePair<FormatType, TranslatedMedalInfo> x) => x.Value.seasonWins);

	public int TotalRankedWinsPrevious => m_prevMedalInfo.Sum((KeyValuePair<FormatType, TranslatedMedalInfo> x) => x.Value.seasonWins);

	public bool IsDisplayable()
	{
		return m_currMedalInfo.Any((KeyValuePair<FormatType, TranslatedMedalInfo> x) => x.Value.IsValid() && GameDbf.League.HasRecord(x.Value.leagueId));
	}

	public MedalInfoTranslator()
	{
		foreach (FormatType enumValue in Enum.GetValues(typeof(FormatType)))
		{
			if (enumValue != 0)
			{
				m_currMedalInfo.Add(enumValue, CreateTranslatedMedalInfo(enumValue, 0, 0, 0));
				m_prevMedalInfo.Add(enumValue, CreateTranslatedMedalInfo(enumValue, 0, 0, 0));
			}
		}
	}

	public static MedalInfoTranslator CreateMedalInfoForLeagueId(int leagueId, int starLevel, int legendIndex)
	{
		MedalInfoTranslator medalInfoTranslator = new MedalInfoTranslator();
		foreach (FormatType formatType in Enum.GetValues(typeof(FormatType)))
		{
			if (formatType != 0)
			{
				medalInfoTranslator.m_currMedalInfo[formatType] = CreateTranslatedMedalInfo(formatType, leagueId, starLevel, legendIndex);
				medalInfoTranslator.m_prevMedalInfo[formatType] = medalInfoTranslator.m_currMedalInfo[formatType].ShallowCopy();
			}
		}
		return medalInfoTranslator;
	}

	public static MedalInfoTranslator CreateMedalInfoForGamePresenceRank(GamePresenceRank gamePresenceRank)
	{
		MedalInfoTranslator medalInfoTranslator = new MedalInfoTranslator();
		foreach (FormatType formatType in Enum.GetValues(typeof(FormatType)))
		{
			if (formatType != 0)
			{
				GamePresenceRankData rankData = gamePresenceRank.Values.Where((GamePresenceRankData x) => x.FormatType == formatType).FirstOrDefault();
				if (rankData != null)
				{
					medalInfoTranslator.m_currMedalInfo[formatType] = CreateTranslatedMedalInfo(formatType, rankData.LeagueId, rankData.StarLevel, rankData.LegendRank);
				}
				else
				{
					medalInfoTranslator.m_currMedalInfo[formatType] = CreateTranslatedMedalInfo(formatType, 0, 0, 0);
				}
				medalInfoTranslator.m_prevMedalInfo[formatType] = medalInfoTranslator.m_currMedalInfo[formatType].ShallowCopy();
			}
		}
		return medalInfoTranslator;
	}

	public static TranslatedMedalInfo CreateTranslatedMedalInfo(FormatType format, int leagueId, int starLevel, int legendIndex)
	{
		return new TranslatedMedalInfo
		{
			format = format,
			leagueId = leagueId,
			starLevel = starLevel,
			legendIndex = legendIndex
		};
	}

	public MedalInfoTranslator(NetCache.NetCacheMedalInfo currMedalInfo, NetCache.NetCacheMedalInfo prevMedalInfo = null)
	{
		if (currMedalInfo == null)
		{
			return;
		}
		Map<FormatType, MedalInfoData> curMedalMap = currMedalInfo.MedalData;
		foreach (KeyValuePair<FormatType, MedalInfoData> item in curMedalMap)
		{
			MedalInfoData medalInfoData = item.Value;
			FormatType formatType = medalInfoData.FormatType;
			m_currMedalInfo[formatType] = Translate(formatType, medalInfoData);
		}
		if (prevMedalInfo != null)
		{
			foreach (KeyValuePair<FormatType, MedalInfoData> medalDatum in prevMedalInfo.MedalData)
			{
				MedalInfoData medalInfoData2 = medalDatum.Value;
				FormatType formatType2 = medalInfoData2.FormatType;
				m_prevMedalInfo[formatType2] = Translate(formatType2, medalInfoData2);
			}
			return;
		}
		foreach (KeyValuePair<FormatType, MedalInfoData> item2 in curMedalMap)
		{
			FormatType formatType3 = item2.Value.FormatType;
			m_prevMedalInfo[formatType3] = m_currMedalInfo[formatType3].ShallowCopy();
		}
	}

	private TranslatedMedalInfo Translate(FormatType format, MedalInfoData medalInfoData)
	{
		if (medalInfoData == null)
		{
			return CreateTranslatedMedalInfo(format, 0, 0, 0);
		}
		TranslatedMedalInfo translatedMedalInfo = CreateTranslatedMedalInfo(format, medalInfoData.LeagueId, medalInfoData.StarLevel, medalInfoData.HasLegendRank ? medalInfoData.LegendRank : 0);
		translatedMedalInfo.bestStarLevel = medalInfoData.BestStarLevel;
		translatedMedalInfo.earnedStars = medalInfoData.Stars;
		translatedMedalInfo.winStreak = medalInfoData.Streak;
		translatedMedalInfo.seasonId = medalInfoData.SeasonId;
		translatedMedalInfo.seasonWins = medalInfoData.SeasonWins;
		translatedMedalInfo.seasonGames = medalInfoData.SeasonGames;
		translatedMedalInfo.starsPerWin = medalInfoData.StarsPerWin;
		return translatedMedalInfo;
	}

	public static MedalInfoTranslator DebugCreateMedalInfo(int leagueId, int starLevel, int stars, int starsPerWin, FormatType formatType, bool isWinStreak, bool showWin)
	{
		MedalInfoTranslator medalInfoTranslator = CreateMedalInfoForLeagueId(leagueId, starLevel, 1337);
		TranslatedMedalInfo prevMedal = medalInfoTranslator.GetPreviousMedal(formatType);
		TranslatedMedalInfo currMedal = medalInfoTranslator.GetCurrentMedal(formatType);
		prevMedal.earnedStars = stars;
		prevMedal.starsPerWin = starsPerWin;
		currMedal.earnedStars = stars;
		currMedal.starsPerWin = starsPerWin;
		currMedal.seasonGames++;
		if (showWin)
		{
			currMedal.seasonWins++;
			int starsToAdd = starsPerWin;
			if (isWinStreak)
			{
				prevMedal.winStreak = prevMedal.RankConfig.WinStreakThreshold;
				currMedal.winStreak = prevMedal.RankConfig.WinStreakThreshold;
				starsToAdd *= 2;
			}
			while (starsToAdd > 0 && currMedal.RankConfig.Stars > 0)
			{
				int starsLeftToEarnAtRank = Mathf.Max(currMedal.RankConfig.Stars - currMedal.earnedStars, 0);
				if (starsToAdd <= starsLeftToEarnAtRank)
				{
					currMedal.earnedStars += starsToAdd;
					starsToAdd = 0;
					continue;
				}
				currMedal.earnedStars += starsLeftToEarnAtRank;
				starsToAdd -= starsLeftToEarnAtRank;
				currMedal.starLevel++;
				currMedal.earnedStars = 0;
			}
			currMedal.legendIndex++;
		}
		else
		{
			if (currMedal.RankConfig.CanLoseStars)
			{
				if (currMedal.earnedStars > 0)
				{
					currMedal.earnedStars--;
				}
				else if (currMedal.starLevel > 1 && currMedal.RankConfig.CanLoseLevel)
				{
					currMedal.earnedStars = currMedal.GetMaxStarsAtRank() - 1;
					currMedal.starLevel--;
				}
			}
			currMedal.legendIndex--;
		}
		return medalInfoTranslator;
	}

	public TranslatedMedalInfo GetCurrentMedal(FormatType formatType)
	{
		if (!m_currMedalInfo.TryGetValue(formatType, out var result))
		{
			Debug.LogError("MedalInfoTranslator.GetCurrentMedal called for unsupported format type " + formatType.ToString() + ". Returning default TranslatedMedalInfo");
			return new TranslatedMedalInfo();
		}
		return result;
	}

	public TranslatedMedalInfo GetCurrentMedalForCurrentFormatType()
	{
		return GetCurrentMedal(Options.GetFormatType());
	}

	public TranslatedMedalInfo GetPreviousMedal(FormatType formatType)
	{
		if (!m_prevMedalInfo.TryGetValue(formatType, out var result))
		{
			Debug.LogError("MedalInfoTranslator.GetPreviousMedal called for unsupported format type " + formatType.ToString() + ". Returning default TranslatedMedalInfo");
			return new TranslatedMedalInfo();
		}
		return result;
	}

	public FormatType GetBestCurrentRankFormatType(bool mustBeThisSeason = false)
	{
		if (m_currMedalInfo == null || m_currMedalInfo.Count == 0)
		{
			Debug.LogError("MedalInfoTranslator.GetBestCurrentRankFormatType had a null or empty m_currMedalInfo. Returning FT_STANDARD. Was this called before the ctor?");
			return FormatType.FT_STANDARD;
		}
		List<KeyValuePair<FormatType, TranslatedMedalInfo>> medalInfoList = null;
		medalInfoList = ((!mustBeThisSeason) ? m_currMedalInfo.ToList() : m_currMedalInfo.Where((KeyValuePair<FormatType, TranslatedMedalInfo> keyValuePair) => keyValuePair.Value.seasonGames > 0).ToList());
		if (medalInfoList.Count == 0)
		{
			return FormatType.FT_STANDARD;
		}
		medalInfoList.Sort(delegate(KeyValuePair<FormatType, TranslatedMedalInfo> f1, KeyValuePair<FormatType, TranslatedMedalInfo> f2)
		{
			if (!f1.Value.IsValid() || !f2.Value.IsValid())
			{
				return f1.Value.starLevel.CompareTo(f2.Value.starLevel);
			}
			int num = f1.Value.LeagueConfig.LeagueLevel.CompareTo(f2.Value.LeagueConfig.LeagueLevel);
			if (num != 0)
			{
				return num;
			}
			if (f1.Value.IsLegendRank() && f2.Value.IsLegendRank())
			{
				num = f1.Value.legendIndex.CompareTo(f2.Value.legendIndex);
				if (num != 0)
				{
					return -num;
				}
			}
			num = f1.Value.starLevel.CompareTo(f2.Value.starLevel);
			if (num != 0)
			{
				return num;
			}
			num = f1.Value.earnedStars.CompareTo(f2.Value.earnedStars);
			return (num != 0) ? num : CompareFormatTypes(f1.Value.format, f2.Value.format);
		});
		return medalInfoList.Last().Key;
	}

	private int CompareFormatTypes(FormatType f1, FormatType f2)
	{
		List<FormatType> obj = new List<FormatType>
		{
			FormatType.FT_CLASSIC,
			FormatType.FT_WILD,
			FormatType.FT_STANDARD
		};
		int index1 = obj.IndexOf(f1);
		int index2 = obj.IndexOf(f2);
		return index1.CompareTo(index2);
	}

	public int GetCurrentSeasonId()
	{
		return GetCurrentMedal(FormatType.FT_STANDARD).seasonId;
	}

	public int GetSeasonCardBackMinWins()
	{
		int result = GetPreviousMedal(FormatType.FT_WILD).LeagueConfig.SeasonCardBackMinWins;
		foreach (FormatType enumValue in Enum.GetValues(typeof(FormatType)))
		{
			if (enumValue != 0 && enumValue != FormatType.FT_WILD)
			{
				result = Mathf.Min(result, GetPreviousMedal(enumValue).LeagueConfig.SeasonCardBackMinWins);
			}
		}
		return result;
	}

	public int GetSeasonCardBackWinsRemaining()
	{
		return Mathf.Max(0, GetSeasonCardBackMinWins() - TotalRankedWins);
	}

	public bool HasEarnedSeasonCardBack()
	{
		return GetSeasonCardBackWinsRemaining() == 0;
	}

	public bool ShouldShowCardBackProgress()
	{
		if (TotalRankedWins > TotalRankedWinsPrevious)
		{
			return TotalRankedWinsPrevious < GetSeasonCardBackMinWins();
		}
		return false;
	}

	public bool GetRankedRewardsEarned(FormatType formatType, ref List<List<RewardData>> rewardsEarned)
	{
		TranslatedMedalInfo tPrevMedal = GetPreviousMedal(formatType);
		TranslatedMedalInfo tCurrMedal = GetCurrentMedal(formatType);
		if (tPrevMedal == null || tCurrMedal == null)
		{
			return false;
		}
		int bestStarLevelInAnyOtherFormat = 0;
		foreach (FormatType otherFormatType in Enum.GetValues(typeof(FormatType)))
		{
			if (otherFormatType != 0 && otherFormatType != formatType)
			{
				bestStarLevelInAnyOtherFormat = Math.Max(bestStarLevelInAnyOtherFormat, GetCurrentMedal(formatType).bestStarLevel);
			}
		}
		bool num = tPrevMedal.bestStarLevel >= tCurrMedal.bestStarLevel;
		bool alreadyReachedRankThisSeasonInOtherFormat = bestStarLevelInAnyOtherFormat > tCurrMedal.bestStarLevel;
		if (num || alreadyReachedRankThisSeasonInOtherFormat)
		{
			return false;
		}
		rewardsEarned.Clear();
		int starLevel = tPrevMedal.starLevel;
		while (starLevel < tCurrMedal.starLevel)
		{
			starLevel++;
			LeagueRankDbfRecord leagueRankRecord = RankMgr.Get().GetLeagueRankRecord(tPrevMedal.leagueId, starLevel);
			List<RewardData> rewardsEarnedForRank = new List<RewardData>();
			RewardUtils.AddRewardDataStubForBag(leagueRankRecord.RewardBagId, tCurrMedal.seasonId, ref rewardsEarnedForRank);
			if (rewardsEarnedForRank.Count > 0)
			{
				rewardsEarned.Add(rewardsEarnedForRank);
			}
		}
		return true;
	}

	public RankChangeType GetChangeType(FormatType formatType)
	{
		TranslatedMedalInfo tPrevMedal = GetPreviousMedal(formatType);
		TranslatedMedalInfo tCurrMedal = GetCurrentMedal(formatType);
		if (tPrevMedal == null || tCurrMedal == null)
		{
			return RankChangeType.UNKNOWN;
		}
		if (tCurrMedal.seasonId == tPrevMedal.seasonId && tCurrMedal.seasonGames == tPrevMedal.seasonGames)
		{
			return RankChangeType.NO_GAME_PLAYED;
		}
		if (tCurrMedal.LeagueConfig.LeagueLevel < tPrevMedal.LeagueConfig.LeagueLevel)
		{
			return RankChangeType.RANK_DOWN;
		}
		if (tCurrMedal.LeagueConfig.LeagueLevel > tPrevMedal.LeagueConfig.LeagueLevel)
		{
			return RankChangeType.RANK_UP;
		}
		if (tCurrMedal.starLevel < tPrevMedal.starLevel)
		{
			return RankChangeType.RANK_DOWN;
		}
		if (tCurrMedal.starLevel > tPrevMedal.starLevel)
		{
			return RankChangeType.RANK_UP;
		}
		return RankChangeType.RANK_SAME;
	}

	public bool IsOnWinStreak(FormatType formatType)
	{
		TranslatedMedalInfo tPrevMedal = GetPreviousMedal(formatType);
		TranslatedMedalInfo tCurrMedal = GetCurrentMedal(formatType);
		if (tPrevMedal == null || tCurrMedal == null)
		{
			return false;
		}
		if (tPrevMedal.RankConfig.WinStreakThreshold > 0)
		{
			return tCurrMedal.winStreak >= tPrevMedal.RankConfig.WinStreakThreshold;
		}
		return false;
	}

	public RankedPlayDataModel CreateDataModel(FormatType formatType, RankedMedal.DisplayMode mode, bool isTooltipEnabled = false, bool hasEarnedCardBack = false)
	{
		return GetCurrentMedal(formatType).CreateDataModel(mode, isTooltipEnabled, hasEarnedCardBack);
	}

	public void CreateOrUpdateDataModel(FormatType formatType, ref RankedPlayDataModel dataModel, RankedMedal.DisplayMode mode, bool isTooltipEnabled = false, bool hasEarnedCardBack = false)
	{
		GetCurrentMedal(formatType).CreateOrUpdateDataModel(ref dataModel, mode, isTooltipEnabled, hasEarnedCardBack);
	}
}
