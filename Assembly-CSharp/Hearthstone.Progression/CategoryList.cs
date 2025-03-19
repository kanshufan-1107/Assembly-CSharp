using System.Collections.Generic;

namespace Hearthstone.Progression;

public static class CategoryList
{
	public static int CountAvailablePoints()
	{
		int sum = 0;
		List<AchievementCategoryDbfRecord> records = GameDbf.AchievementCategory.GetRecords();
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			sum += records[i].CountAvailablePoints();
		}
		return sum;
	}

	public static int CountAchievements()
	{
		int sum = 0;
		List<AchievementCategoryDbfRecord> records = GameDbf.AchievementCategory.GetRecords();
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			sum += records[i].CountAchievements();
		}
		return sum;
	}
}
