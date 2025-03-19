using System.Collections.Generic;

namespace Hearthstone.Progression;

public static class Category
{
	public static int CountAvailablePoints(this AchievementCategoryDbfRecord source)
	{
		int sum = 0;
		List<AchievementSubcategoryDbfRecord> records = source.Subcategories;
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			sum += records[i].CountAvailablePoints();
		}
		return sum;
	}

	public static int CountAchievements(this AchievementCategoryDbfRecord source)
	{
		int sum = 0;
		List<AchievementSubcategoryDbfRecord> records = source.Subcategories;
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			sum += records[i].CountAchievements();
		}
		return sum;
	}
}
