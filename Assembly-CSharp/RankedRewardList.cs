using System.Collections.Generic;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class RankedRewardList : MonoBehaviour
{
	public UberText m_cardBackProgressText;

	public RewardListAutoScroller m_autoScroller;

	public Widget m_nextApprenticeRewardRow;

	public Widget m_leagueGraduationRow;

	[SerializeField]
	private List<Widget> m_boosterRewards;

	private Widget m_widget;

	private int m_currentRankSectionIndex;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
	}

	public void Initialize(MedalInfoTranslator mit)
	{
		if (mit == null)
		{
			return;
		}
		bool isTooltipEnabled = false;
		bool hasEarnedCardBack = mit.HasEarnedSeasonCardBack();
		FormatType currentFormat = mit.GetBestCurrentRankFormatType(mustBeThisSeason: true);
		RankedPlayDataModel rankedPlayDataModel = mit.CreateDataModel(currentFormat, RankedMedal.DisplayMode.Default, isTooltipEnabled, hasEarnedCardBack);
		m_widget.BindDataModel(rankedPlayDataModel);
		m_cardBackProgressText.Text = GameStrings.Format("GLUE_RANKED_REWARD_LIST_CARDBACK_PROGRESS", mit.GetSeasonCardBackWinsRemaining());
		int seasonId = mit.GetCurrentSeasonId();
		CardBackDataModel cardBackDataModel = new CardBackDataModel
		{
			CardBackId = RankMgr.Get().GetRankedCardBackIdForSeasonId(seasonId)
		};
		m_widget.BindDataModel(cardBackDataModel);
		PackDataModel packDataModel = new PackDataModel
		{
			Type = (BoosterDbId)RankMgr.Get().GetRankedRewardBoosterIdForSeasonId(seasonId),
			Quantity = 1
		};
		foreach (Widget boosterReward in m_boosterRewards)
		{
			boosterReward.BindDataModel(packDataModel);
		}
		if (rankedPlayDataModel.IsNewPlayer)
		{
			TranslatedMedalInfo currentMedalInfo = mit.GetCurrentMedal(currentFormat);
			int currentStars = rankedPlayDataModel.StarLevel;
			List<AchieveDbfRecord> records = GameDbf.Achieve.GetRecords();
			int nextRewardStars = 0;
			foreach (AchieveDbfRecord achieve in records)
			{
				if (achieve.LeagueType == Achieve.LeagueType.NEW_PLAYER && achieve.StarLevel > currentStars && (nextRewardStars == 0 || achieve.StarLevel < nextRewardStars))
				{
					nextRewardStars = achieve.StarLevel;
				}
			}
			if (nextRewardStars != 0)
			{
				TranslatedMedalInfo nextMedalInfo = MedalInfoTranslator.CreateTranslatedMedalInfo(currentFormat, currentMedalInfo.leagueId, nextRewardStars, -1);
				RankedPlayDataModel nextRewardRank = new RankedPlayDataModel();
				nextRewardRank.IsNewPlayer = true;
				nextRewardRank.MedalText = nextMedalInfo.GetMedalText();
				nextRewardRank.RankName = nextMedalInfo.GetRankName();
				m_nextApprenticeRewardRow.BindDataModel(nextRewardRank);
			}
			TranslatedMedalInfo lastMedalInfo = MedalInfoTranslator.CreateTranslatedMedalInfo(currentFormat, currentMedalInfo.leagueId, currentMedalInfo.GetMaxStarLevel(), -1);
			RankedPlayDataModel lastRank = new RankedPlayDataModel();
			lastRank.IsNewPlayer = true;
			lastRank.MedalText = lastMedalInfo.GetMedalText();
			lastRank.RankName = lastMedalInfo.GetRankName();
			m_leagueGraduationRow.BindDataModel(lastRank);
			return;
		}
		TranslatedMedalInfo currMedalInfo = mit.GetCurrentMedal(mit.GetBestCurrentRankFormatType(mustBeThisSeason: true));
		List<LeagueRankDbfRecord> ranks = currMedalInfo.LeagueConfig.Ranks;
		ranks.Sort((LeagueRankDbfRecord a, LeagueRankDbfRecord b) => a.StarLevel - b.StarLevel);
		m_currentRankSectionIndex = -1;
		RankedPlayListDataModel rankedPlayListDataModel = new RankedPlayListDataModel();
		foreach (LeagueRankDbfRecord rankRecord in ranks)
		{
			if (rankRecord.RewardBagId != 0)
			{
				RankedPlayDataModel dm = MedalInfoTranslator.CreateTranslatedMedalInfo(currMedalInfo.format, currMedalInfo.leagueId, rankRecord.StarLevel, 0).CreateDataModel(RankedMedal.DisplayMode.Chest);
				rankedPlayListDataModel.Items.Add(dm);
				if (rankedPlayDataModel.StarLevel >= rankRecord.StarLevel && currMedalInfo.seasonGames > 0)
				{
					m_currentRankSectionIndex++;
				}
			}
		}
		m_widget.BindDataModel(rankedPlayListDataModel);
		m_autoScroller.Init(m_widget, m_currentRankSectionIndex);
	}
}
