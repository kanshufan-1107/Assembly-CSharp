using System.Collections.Generic;
using Hearthstone.DataModels;

namespace Hearthstone.Progression;

public static class Subcategory
{
	public static AchievementCategoryDbfRecord GetAchievementCategory(this AchievementSubcategoryDbfRecord source)
	{
		return GameDbf.AchievementCategory.GetRecord(source.AchievementCategoryId);
	}

	public static int CountAvailablePoints(this AchievementSubcategoryDbfRecord source)
	{
		int sum = 0;
		List<AchievementSectionItemDbfRecord> records = source.Sections;
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			sum += records[i].AchievementSectionRecord.CountAvailablePoints();
		}
		return sum;
	}

	public static int CountAchievements(this AchievementSubcategoryDbfRecord source)
	{
		int sum = 0;
		List<AchievementSectionItemDbfRecord> records = source.Sections;
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			sum += records[i].AchievementSectionRecord.CountAchievements();
		}
		return sum;
	}

	public static void UpdateIsLocked(this AchievementSubcategoryDataModel source)
	{
		source.IsLocked = AchievementManager.Get()?.IsAchievementSubcategoryLocked(source.ID) ?? false;
	}
}
