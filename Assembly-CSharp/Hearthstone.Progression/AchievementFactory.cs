using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusUtil;

namespace Hearthstone.Progression;

public static class AchievementFactory
{
	public static AchievementCategoryListDataModel CreateAchievementListDataModel(AchievementStats stats)
	{
		AchievementCategoryListDataModel obj = new AchievementCategoryListDataModel
		{
			Categories = CreateAchievementCategoryDataModelList(stats),
			Stats = new AchievementStatsDataModel
			{
				Points = stats.GetTotalPoints(),
				AvailablePoints = CategoryList.CountAvailablePoints(),
				Unclaimed = stats.GetTotalUnclaimed(),
				CompletedAchievements = stats.GetTotalCompleted(),
				TotalAchievements = CategoryList.CountAchievements()
			}
		};
		obj.Stats.UpdateCompletionPercentage();
		return obj;
	}

	public static DataModelList<AchievementCategoryDataModel> CreateAchievementCategoryDataModelList(AchievementStats stats)
	{
		return (from record in GameDbf.AchievementCategory.GetRecords()
			orderby record.SortOrder
			select CreateAchievementCategoryDataModel(record, stats)).ToDataModelList();
	}

	public static AchievementCategoryDataModel CreateAchievementCategoryDataModel(AchievementCategoryDbfRecord record, AchievementStats stats)
	{
		AchievementCategoryDataModel obj = new AchievementCategoryDataModel
		{
			Name = (record.Name?.GetString() ?? string.Empty),
			Icon = record.Icon,
			Subcategories = new AchievementSubcategoryListDataModel
			{
				Subcategories = CreateAchievementSubCategoryDataModelList(record.Subcategories, stats)
			},
			Stats = new AchievementStatsDataModel
			{
				Points = stats.GetCategoryPoints(record),
				AvailablePoints = record.CountAvailablePoints(),
				Unclaimed = stats.GetCategoryUnclaimed(record),
				CompletedAchievements = stats.GetCategoryCompleted(record),
				TotalAchievements = record.CountAchievements()
			},
			ID = record.ID
		};
		obj.Stats.UpdateCompletionPercentage();
		return obj;
	}

	public static DataModelList<AchievementSubcategoryDataModel> CreateAchievementSubCategoryDataModelList(List<AchievementSubcategoryDbfRecord> records, AchievementStats stats)
	{
		return (from record in records
			orderby record.SortOrder
			select CreateAchievementSubcategoryDataModel(record, stats)).ToDataModelList();
	}

	public static AchievementSubcategoryDataModel CreateAchievementSubcategoryDataModel(AchievementSubcategoryDbfRecord record, AchievementStats stats)
	{
		DataModelList<AchievementSectionDataModel> sections = CreateAchievementSectionDataModelList(record.Sections);
		AchievementSubcategoryDataModel obj = new AchievementSubcategoryDataModel
		{
			Name = (record.Name?.GetString() ?? string.Empty),
			FullName = ProgressUtils.FormatAchievementSubcategoryFullName(record.GetAchievementCategory(), record),
			Icon = record.Icon,
			Sections = new AchievementSectionListDataModel
			{
				Sections = sections
			},
			Stats = new AchievementStatsDataModel
			{
				Points = stats.GetSubcategoryPoints(record),
				AvailablePoints = record.CountAvailablePoints(),
				Unclaimed = stats.GetSubcategoryUnclaimed(record),
				CompletedAchievements = stats.GetSubcategoryCompleted(record),
				TotalAchievements = record.CountAchievements()
			},
			ID = record.ID
		};
		obj.Stats.UpdateCompletionPercentage();
		return obj;
	}

	public static DataModelList<AchievementSectionDataModel> CreateAchievementSectionDataModelList(List<AchievementSectionItemDbfRecord> records)
	{
		return (from record in records
			orderby record.SortOrder, record.AchievementSectionRecord.Name?.GetString() ?? string.Empty
			select record.AchievementSectionRecord into record
			select CreateAchievementSectionDataModel(record)).ToDataModelList();
	}

	public static AchievementSectionDataModel CreateAchievementSectionDataModel(AchievementSectionDbfRecord record)
	{
		return new AchievementSectionDataModel
		{
			Name = (record.Name?.GetString() ?? string.Empty),
			Achievements = new AchievementListDataModel
			{
				Achievements = new DataModelList<AchievementDataModel>()
			},
			ID = record.ID
		};
	}

	public static DataModelList<AchievementDataModel> CreateAchievementDataModelList(int sectionId, Func<int, PlayerAchievementState> playerState)
	{
		bool showHiddenAchievements = ProgressUtils.ShowHiddenAchievements;
		DataModelList<AchievementDataModel> achievements = new DataModelList<AchievementDataModel>();
		List<AchievementDbfRecord> records = GameDbf.Achievement.GetRecords();
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			AchievementDbfRecord record = records[i];
			if (record.AchievementSection == sectionId && record.Enabled && (showHiddenAchievements || record.AchievementVisibility == Assets.Achievement.AchievementVisibility.VISIBLE))
			{
				achievements.Add(CreateAchievementDataModel(record, playerState(record.ID)));
			}
		}
		achievements.UpdateTiers();
		return achievements;
	}

	public static AchievementDataModel CreateAchievementDataModel(AchievementDbfRecord record, PlayerAchievementState playerState)
	{
		AchievementManager.AchievementStatus status = (AchievementManager.AchievementStatus)playerState.Status;
		int progress = playerState.Progress;
		AchievementDataModel obj = new AchievementDataModel
		{
			Name = (record.Name?.GetString() ?? string.Empty),
			Description = ProgressUtils.FormatDescription(record.Description, record.Quota),
			Progress = progress,
			Quota = record.Quota,
			Points = record.Points,
			Status = status,
			CompletionDate = ((playerState.CompletedDate != 0L) ? ProgressUtils.FormatAchievementCompletionDate(playerState.CompletedDate) : string.Empty),
			ID = record.ID,
			NextTierID = record.NextTier,
			SortOrder = record.SortOrder,
			AllowExceedQuota = record.AllowExceedQuota
		};
		obj.UpdateProgress();
		return obj;
	}

	public static RewardScrollDataModel CreateRewardScrollDataModel(int achievementId, int chooseOneRewardItemId = 0, List<RewardItemOutput> rewardItemOutput = null)
	{
		AchievementDbfRecord achievementRecord = GameDbf.Achievement.GetRecord(achievementId);
		if (achievementRecord == null)
		{
			return null;
		}
		return new RewardScrollDataModel
		{
			DisplayName = achievementRecord.Name,
			Description = ProgressUtils.FormatDescription(achievementRecord.Description, achievementRecord.Quota),
			RewardList = RewardUtils.CreateRewardListDataModelFromRewardListId(achievementRecord.RewardList, chooseOneRewardItemId, rewardItemOutput)
		};
	}
}
