using System;
using System.Collections.Generic;
using Assets;
using PegasusUtil;

namespace Hearthstone.Progression;

public class AchievementStats
{
	private class Memoizer<TKey, TValue>
	{
		private readonly Dictionary<TKey, TValue> m_values = new Dictionary<TKey, TValue>();

		private readonly Func<TKey, TValue> m_compute;

		public Memoizer(Func<TKey, TValue> compute)
		{
			m_compute = compute;
		}

		public TValue GetValue(TKey key)
		{
			if (m_values.TryGetValue(key, out var value))
			{
				return value;
			}
			value = m_compute(key);
			m_values[key] = value;
			return value;
		}

		public void Invalidate(TKey key)
		{
			m_values.Remove(key);
		}

		public void InvalidateAll()
		{
			m_values.Clear();
		}
	}

	private readonly Memoizer<AchievementCategoryDbfRecord, int> m_categoryPoints;

	private readonly Memoizer<AchievementCategoryDbfRecord, int> m_categoryUnclaimed;

	private readonly Memoizer<AchievementCategoryDbfRecord, int> m_categoryCompleted;

	private readonly Memoizer<AchievementSubcategoryDbfRecord, int> m_subcategoryPoints;

	private readonly Memoizer<AchievementSubcategoryDbfRecord, int> m_subcategoryUnclaimed;

	private readonly Memoizer<AchievementSubcategoryDbfRecord, int> m_subcategoryCompleted;

	private readonly Memoizer<AchievementSectionDbfRecord, int> m_sectionPoints;

	private readonly Memoizer<AchievementSectionDbfRecord, int> m_sectionUnclaimed;

	private readonly Memoizer<AchievementSectionDbfRecord, int> m_sectionCompleted;

	public AchievementStats(Func<int, PlayerAchievementState> playerState)
	{
		m_categoryPoints = new Memoizer<AchievementCategoryDbfRecord, int>(CountCategoryPoints);
		m_categoryUnclaimed = new Memoizer<AchievementCategoryDbfRecord, int>(CountCategoryUnclaimed);
		m_categoryCompleted = new Memoizer<AchievementCategoryDbfRecord, int>(CountCategoryCompleted);
		m_subcategoryPoints = new Memoizer<AchievementSubcategoryDbfRecord, int>(CountSubcategoryPoints);
		m_subcategoryUnclaimed = new Memoizer<AchievementSubcategoryDbfRecord, int>(CountSubcategoryUnclaimed);
		m_subcategoryCompleted = new Memoizer<AchievementSubcategoryDbfRecord, int>(CountSubcategoryCompleted);
		m_sectionPoints = new Memoizer<AchievementSectionDbfRecord, int>((AchievementSectionDbfRecord section) => CountSectionPoints(section, playerState));
		m_sectionUnclaimed = new Memoizer<AchievementSectionDbfRecord, int>((AchievementSectionDbfRecord section) => CountSectionUnclaimed(section, playerState));
		m_sectionCompleted = new Memoizer<AchievementSectionDbfRecord, int>((AchievementSectionDbfRecord section) => CountSectionCompleted(section, playerState));
	}

	public int GetTotalPoints()
	{
		int sum = 0;
		List<AchievementCategoryDbfRecord> records = GameDbf.AchievementCategory.GetRecords();
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			sum += GetCategoryPoints(records[i]);
		}
		return sum;
	}

	public int GetTotalUnclaimed()
	{
		int sum = 0;
		List<AchievementCategoryDbfRecord> records = GameDbf.AchievementCategory.GetRecords();
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			sum += GetCategoryUnclaimed(records[i]);
		}
		return sum;
	}

	public int GetTotalCompleted()
	{
		int sum = 0;
		List<AchievementCategoryDbfRecord> records = GameDbf.AchievementCategory.GetRecords();
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			sum += GetCategoryCompleted(records[i]);
		}
		return sum;
	}

	public void InvalidateAll()
	{
		m_categoryUnclaimed.InvalidateAll();
		m_categoryCompleted.InvalidateAll();
		m_subcategoryUnclaimed.InvalidateAll();
		m_subcategoryCompleted.InvalidateAll();
		m_sectionUnclaimed.InvalidateAll();
		m_sectionCompleted.InvalidateAll();
	}

	public int GetCategoryPoints(AchievementCategoryDbfRecord category)
	{
		return m_categoryPoints.GetValue(category);
	}

	public int GetCategoryUnclaimed(AchievementCategoryDbfRecord category)
	{
		return m_categoryUnclaimed.GetValue(category);
	}

	public int GetCategoryCompleted(AchievementCategoryDbfRecord category)
	{
		return m_categoryCompleted.GetValue(category);
	}

	public void InvalidateCategory(AchievementCategoryDbfRecord category)
	{
		m_categoryPoints.Invalidate(category);
		m_categoryUnclaimed.Invalidate(category);
		m_categoryCompleted.Invalidate(category);
	}

	public int GetSubcategoryPoints(AchievementSubcategoryDbfRecord subcategory)
	{
		return m_subcategoryPoints.GetValue(subcategory);
	}

	public int GetSubcategoryUnclaimed(AchievementSubcategoryDbfRecord subcategory)
	{
		return m_subcategoryUnclaimed.GetValue(subcategory);
	}

	public int GetSubcategoryCompleted(AchievementSubcategoryDbfRecord subcategory)
	{
		return m_subcategoryCompleted.GetValue(subcategory);
	}

	public void InvalidSubcategory(AchievementSubcategoryDbfRecord subcategory)
	{
		m_subcategoryPoints.Invalidate(subcategory);
		m_subcategoryUnclaimed.Invalidate(subcategory);
		m_subcategoryCompleted.Invalidate(subcategory);
	}

	public void InvalidateSection(AchievementSectionDbfRecord section)
	{
		m_sectionPoints.Invalidate(section);
		m_sectionUnclaimed.Invalidate(section);
		m_sectionCompleted.Invalidate(section);
	}

	private int CountCategoryPoints(AchievementCategoryDbfRecord category)
	{
		int sum = 0;
		List<AchievementSubcategoryDbfRecord> subcategories = category.Subcategories;
		int i = 0;
		for (int iMax = subcategories.Count; i < iMax; i++)
		{
			sum += m_subcategoryPoints.GetValue(subcategories[i]);
		}
		return sum;
	}

	private int CountCategoryUnclaimed(AchievementCategoryDbfRecord category)
	{
		int sum = 0;
		List<AchievementSubcategoryDbfRecord> subcategories = category.Subcategories;
		int i = 0;
		for (int iMax = subcategories.Count; i < iMax; i++)
		{
			sum += m_subcategoryUnclaimed.GetValue(subcategories[i]);
		}
		return sum;
	}

	private int CountCategoryCompleted(AchievementCategoryDbfRecord category)
	{
		int sum = 0;
		List<AchievementSubcategoryDbfRecord> subcategories = category.Subcategories;
		int i = 0;
		for (int iMax = subcategories.Count; i < iMax; i++)
		{
			sum += m_subcategoryCompleted.GetValue(subcategories[i]);
		}
		return sum;
	}

	private int CountSubcategoryPoints(AchievementSubcategoryDbfRecord subcategory)
	{
		int sum = 0;
		List<AchievementSectionItemDbfRecord> sections = subcategory.Sections;
		int i = 0;
		for (int iMax = sections.Count; i < iMax; i++)
		{
			sum += m_sectionPoints.GetValue(sections[i].AchievementSectionRecord);
		}
		return sum;
	}

	private int CountSubcategoryUnclaimed(AchievementSubcategoryDbfRecord subcategory)
	{
		int sum = 0;
		List<AchievementSectionItemDbfRecord> sections = subcategory.Sections;
		int i = 0;
		for (int iMax = sections.Count; i < iMax; i++)
		{
			sum += m_sectionUnclaimed.GetValue(sections[i].AchievementSectionRecord);
		}
		return sum;
	}

	private int CountSubcategoryCompleted(AchievementSubcategoryDbfRecord subcategory)
	{
		int sum = 0;
		List<AchievementSectionItemDbfRecord> sections = subcategory.Sections;
		int i = 0;
		for (int iMax = sections.Count; i < iMax; i++)
		{
			sum += m_sectionCompleted.GetValue(sections[i].AchievementSectionRecord);
		}
		return sum;
	}

	private static int CountSectionPoints(AchievementSectionDbfRecord section, Func<int, PlayerAchievementState> playerState)
	{
		int sum = 0;
		int sourceId = section.ID;
		List<AchievementDbfRecord> records = GameDbf.Achievement.GetRecords();
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			AchievementDbfRecord record = records[i];
			if (record.AchievementVisibility != Assets.Achievement.AchievementVisibility.HIDDEN && Section.IsMatchingAchievement(record, sourceId))
			{
				sum += records[i].CountPoints(playerState);
			}
		}
		return sum;
	}

	private static int CountSectionUnclaimed(AchievementSectionDbfRecord section, Func<int, PlayerAchievementState> playerState)
	{
		int count = 0;
		int sourceId = section.ID;
		List<AchievementDbfRecord> records = GameDbf.Achievement.GetRecords();
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			AchievementDbfRecord record = records[i];
			if (record.AchievementVisibility != Assets.Achievement.AchievementVisibility.HIDDEN && Section.IsMatchingAchievement(record, sourceId) && records[i].IsUnclaimed(playerState))
			{
				count++;
			}
		}
		return count;
	}

	private static int CountSectionCompleted(AchievementSectionDbfRecord section, Func<int, PlayerAchievementState> playerState)
	{
		int count = 0;
		int sourceId = section.ID;
		List<AchievementDbfRecord> records = GameDbf.Achievement.GetRecords();
		int i = 0;
		for (int iMax = records.Count; i < iMax; i++)
		{
			AchievementDbfRecord record = records[i];
			if (record.AchievementVisibility != Assets.Achievement.AchievementVisibility.HIDDEN && Section.IsMatchingAchievement(record, sourceId) && records[i].IsCompleted(playerState))
			{
				count++;
			}
		}
		return count;
	}
}
