using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class AchievementSubcategory : MonoBehaviour
{
	private const int NUM_SECTIONS_TO_STAGGER = 2;

	public Widget m_sectionListWidget;

	private readonly AchievementSectionListDataModel m_sections = new AchievementSectionListDataModel();

	private void Awake()
	{
		m_sectionListWidget.BindDataModel(m_sections);
		AchievementManager.Get().OnSelectedSubcategoryChanged += OnSubcategoryChanged;
	}

	private void OnDestroy()
	{
		AchievementManager achievementMan = AchievementManager.Get();
		if (achievementMan != null)
		{
			achievementMan.OnSelectedSubcategoryChanged -= OnSubcategoryChanged;
		}
	}

	private void OnSubcategoryChanged(AchievementSubcategoryDataModel subcategory)
	{
		m_sections.Sections.OverwriteDataModels(subcategory.Sections.Sections.Where((AchievementSectionDataModel x) => x.Achievements.Achievements.Count > 0).ToDataModelList());
		SetAchievementDisplayDelay();
	}

	private void SetAchievementDisplayDelay()
	{
		float displayDelay = AchievementCell.ACHIEVEMENT_SHOW_DELAY;
		int count = m_sections.Sections.Count;
		int numSectionsToStagger = Mathf.Min(2, count);
		for (int i = 0; i < numSectionsToStagger; i++)
		{
			AchievementSectionDataModel section = m_sections.Sections[i];
			section.DisplayDelay = displayDelay;
			displayDelay += AchievementCell.ACHIEVEMENT_SHOW_DELAY;
			if (section.DisplayedAchievements.Count == 0)
			{
				section.SetDisplayedAchievements();
			}
			int achievementCount = section.DisplayedAchievements.Count;
			for (int j = 0; j < achievementCount; j++)
			{
				section.DisplayedAchievements[j].DisplayDelay = displayDelay;
				displayDelay += AchievementCell.ACHIEVEMENT_SHOW_DELAY;
			}
		}
		for (int k = numSectionsToStagger; k < count; k++)
		{
			AchievementSectionDataModel section2 = m_sections.Sections[k];
			section2.DisplayDelay = 0f;
			int achievementCount2 = section2.DisplayedAchievements.Count;
			for (int l = 0; l < achievementCount2; l++)
			{
				section2.DisplayedAchievements[l].DisplayDelay = 0f;
			}
		}
	}
}
