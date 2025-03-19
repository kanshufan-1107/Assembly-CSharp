using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusUtil;

namespace Hearthstone.Progression;

public static class Section
{
	public static DataModelList<AchievementDataModel> SetDisplayedAchievements(this AchievementSectionDataModel source)
	{
		source.DisplayedAchievements = source.Achievements.Achievements.Where((AchievementDataModel achievement) => achievement.IsCurrentTierInList(source.Achievements.Achievements)).Sorted().ToDataModelList();
		return source.DisplayedAchievements;
	}

	public static void LoadAchievements(this AchievementSectionDataModel source, Func<int, PlayerAchievementState> playerState)
	{
		source.Achievements.Achievements = AchievementFactory.CreateAchievementDataModelList(source.ID, playerState);
	}

	public static bool IsMatchingAchievement(AchievementDbfRecord record, int sourceId)
	{
		if (record != null && record.AchievementSection == sourceId)
		{
			return record.Enabled;
		}
		return false;
	}

	public static int CountAvailablePoints(this AchievementSectionDbfRecord source)
	{
		int sum = 0;
		int sourceId = source.ID;
		List<AchievementDbfRecord> records = GameDbf.Achievement.GetRecords();
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			AchievementDbfRecord record = records[i];
			if (IsMatchingAchievement(record, sourceId))
			{
				sum += record.Points;
			}
		}
		return sum;
	}

	public static int CountAchievements(this AchievementSectionDbfRecord source)
	{
		int count = 0;
		int sourceId = source.ID;
		List<AchievementDbfRecord> records = GameDbf.Achievement.GetRecords();
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			if (IsMatchingAchievement(records[i], sourceId))
			{
				count++;
			}
		}
		return count;
	}
}
