using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class TavernGuideTabDisplay : MonoBehaviour
{
	private enum PagePriority
	{
		CLAIM_READY,
		NEW_QUEST,
		ANY_QUEST_AVAILABLE,
		DEFAULT
	}

	private const string CATEGORY_PRESSED = "CODE_CATEGORY_PRESSED";

	private const string NEXT_PAGE_PRESSED = "CODE_NEXT_PAGE_PRESSED";

	private const string PREV_PAGE_PRESSED = "CODE_PREV_PAGE_PRESSED";

	[SerializeField]
	private Widget m_tabCategory1;

	[SerializeField]
	private Widget m_tabCategory2;

	[SerializeField]
	private Widget m_tabCategory3;

	private Widget m_widget;

	private TavernGuideDataModel m_tavernGuideDatamodel;

	private Dictionary<int, Tuple<int, PagePriority>> m_categoryPagePriority;

	protected virtual void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_tavernGuideDatamodel = TavernGuideManager.Get().GetTavernGuideDataModel();
		m_categoryPagePriority = new Dictionary<int, Tuple<int, PagePriority>>();
		UpdateStartingTavernGuideQuestPage(m_tavernGuideDatamodel);
		m_widget.RegisterEventListener(HandleEvent);
		m_widget.BindDataModel(m_tavernGuideDatamodel);
		TavernGuideManager.Get().OnQuestSetsChanged += UpdateTavernGuide;
		TavernGuideManager.Get().OnInnerQuestStatusChanged += UpdateTavernGuideInnerQuest;
	}

	private void UpdateTavernGuide()
	{
		if (!(m_widget == null))
		{
			TavernGuideDataModel updatedTavernGuideDataModel = TavernGuideManager.Get().GetTavernGuideDataModel();
			if (m_tavernGuideDatamodel != null)
			{
				updatedTavernGuideDataModel.SelectedCategoryIndex = m_tavernGuideDatamodel.SelectedCategoryIndex;
				updatedTavernGuideDataModel.SelectedQuestSetIndex = m_tavernGuideDatamodel.SelectedQuestSetIndex;
			}
			m_tavernGuideDatamodel = updatedTavernGuideDataModel;
			SkipIntroQuestSetIfNeeded();
			m_widget.BindDataModel(m_tavernGuideDatamodel);
		}
	}

	private void UpdateTavernGuideInnerQuest(int tavernGuideQuestId, QuestManager.QuestStatus status)
	{
		if (m_tavernGuideDatamodel == null)
		{
			return;
		}
		foreach (TavernGuideQuestSetCategoryDataModel category in m_tavernGuideDatamodel.TavernGuideQuestSetCategories)
		{
			bool categoryHasNewQuest = false;
			foreach (TavernGuideQuestSetDataModel questSet in category.TavernGuideQuestSets)
			{
				bool questSetHasNewQuest = false;
				foreach (TavernGuideQuestDataModel tavernGuideQuest in questSet.Quests)
				{
					if (tavernGuideQuest.ID == tavernGuideQuestId)
					{
						tavernGuideQuest.Quest.Status = status;
					}
					if (tavernGuideQuest.Quest.Status == QuestManager.QuestStatus.NEW)
					{
						questSetHasNewQuest = true;
					}
				}
				questSet.HasNewQuest = questSetHasNewQuest;
				if (questSet.HasNewQuest)
				{
					categoryHasNewQuest = true;
				}
			}
			category.HasNewQuest = categoryHasNewQuest;
		}
	}

	private void HandleEvent(string eventName)
	{
		switch (eventName)
		{
		case "CODE_CATEGORY_PRESSED":
			if (m_widget.GetDataModel<EventDataModel>().Payload is double selectedIndex)
			{
				int index = Mathf.Clamp((int)selectedIndex, 0, m_tavernGuideDatamodel.TavernGuideQuestSetCategories.Count - 1);
				if ((double)index != selectedIndex)
				{
					Log.All.PrintError("Selected Tavern Guide category out of bounds!");
				}
				m_tavernGuideDatamodel.SelectedCategoryIndex = index;
				m_tavernGuideDatamodel.SelectedQuestSetIndex = 0;
				if (m_categoryPagePriority.ContainsKey(index))
				{
					m_tavernGuideDatamodel.SelectedQuestSetIndex = index;
					m_tavernGuideDatamodel.SelectedQuestSetIndex = m_categoryPagePriority[index].Item1;
				}
				SkipIntroQuestSetIfNeeded();
				Options.Get().SetInt(Option.TAVERN_GUIDE_LAST_VISITED_QUEST_SET_CATEGORY, index);
				m_tavernGuideDatamodel.IsOpening = false;
			}
			break;
		case "CODE_NEXT_PAGE_PRESSED":
			GoToNextQuestSet();
			break;
		case "CODE_PREV_PAGE_PRESSED":
			GoToPreviousQuestSet();
			break;
		}
	}

	private void UpdateStartingTavernGuideQuestPage(TavernGuideDataModel dataModel)
	{
		if (dataModel?.TavernGuideQuestSetCategories == null || dataModel.TavernGuideQuestSetCategories.Count == 0)
		{
			return;
		}
		m_categoryPagePriority = new Dictionary<int, Tuple<int, PagePriority>>();
		for (int categoryIndex = 0; categoryIndex < dataModel.TavernGuideQuestSetCategories.Count; categoryIndex++)
		{
			TavernGuideQuestSetCategoryDataModel category = dataModel.TavernGuideQuestSetCategories[categoryIndex];
			m_categoryPagePriority[categoryIndex] = new Tuple<int, PagePriority>(0, PagePriority.DEFAULT);
			if (!category.Enabled || category.TavernGuideQuestSets.Count == 0)
			{
				continue;
			}
			int questSetWithNewQuestIndex = -1;
			int questSetWithActiveQuestIndex = -1;
			bool foundReadyToClaimPage = false;
			for (int setIndex = 0; setIndex < category.TavernGuideQuestSets.Count; setIndex++)
			{
				TavernGuideQuestSetDataModel questSet = category.TavernGuideQuestSets[setIndex];
				if (questSet.CompletionAchievement != null && questSet.CompletionAchievement.Status != 0 && questSet.CompletionAchievement.Status < AchievementManager.AchievementStatus.REWARD_GRANTED)
				{
					if (questSet.CompletionAchievement.Status == AchievementManager.AchievementStatus.COMPLETED)
					{
						m_categoryPagePriority[categoryIndex] = new Tuple<int, PagePriority>(setIndex, PagePriority.CLAIM_READY);
						foundReadyToClaimPage = true;
						break;
					}
					if (questSet.Quests.Any((TavernGuideQuestDataModel quest) => quest.Quest.Status == QuestManager.QuestStatus.NEW) && questSetWithNewQuestIndex == -1)
					{
						questSetWithNewQuestIndex = setIndex;
					}
					if (questSet.Quests.Any((TavernGuideQuestDataModel quest) => quest.Quest.Status == QuestManager.QuestStatus.ACTIVE) && questSetWithActiveQuestIndex == -1)
					{
						questSetWithActiveQuestIndex = setIndex;
					}
				}
			}
			if (!foundReadyToClaimPage)
			{
				if (questSetWithNewQuestIndex != -1)
				{
					m_categoryPagePriority[categoryIndex] = new Tuple<int, PagePriority>(questSetWithNewQuestIndex, PagePriority.NEW_QUEST);
				}
				else if (questSetWithActiveQuestIndex != -1)
				{
					m_categoryPagePriority[categoryIndex] = new Tuple<int, PagePriority>(questSetWithActiveQuestIndex, PagePriority.ANY_QUEST_AVAILABLE);
				}
				else
				{
					m_categoryPagePriority[categoryIndex] = new Tuple<int, PagePriority>(0, PagePriority.DEFAULT);
				}
			}
		}
		int recommendedCategoryIndex = 0;
		int recommendedQuestSetIndex = 0;
		PagePriority highestPriority = PagePriority.DEFAULT;
		foreach (KeyValuePair<int, Tuple<int, PagePriority>> item in m_categoryPagePriority)
		{
			item.Deconstruct(out var key, out var value);
			int category2 = key;
			Tuple<int, PagePriority> tuple = value;
			int questSet2 = tuple.Item1;
			PagePriority pagePriority = tuple.Item2;
			if (pagePriority < highestPriority)
			{
				recommendedCategoryIndex = category2;
				recommendedQuestSetIndex = questSet2;
				highestPriority = pagePriority;
			}
		}
		int lastVisitedCategoryIndex = Options.Get().GetInt(Option.TAVERN_GUIDE_LAST_VISITED_QUEST_SET_CATEGORY, 0);
		if (highestPriority == PagePriority.DEFAULT || highestPriority == PagePriority.ANY_QUEST_AVAILABLE)
		{
			dataModel.SelectedCategoryIndex = lastVisitedCategoryIndex;
			if (m_categoryPagePriority.ContainsKey(lastVisitedCategoryIndex))
			{
				Tuple<int, PagePriority> tuple2 = m_categoryPagePriority[lastVisitedCategoryIndex];
				if (tuple2 == null || tuple2.Item1 != -1)
				{
					dataModel.SelectedQuestSetIndex = m_categoryPagePriority[lastVisitedCategoryIndex].Item1;
					goto IL_02bb;
				}
			}
			dataModel.SelectedQuestSetIndex = 0;
		}
		else
		{
			dataModel.SelectedCategoryIndex = recommendedCategoryIndex;
			dataModel.SelectedQuestSetIndex = recommendedQuestSetIndex;
		}
		goto IL_02bb;
		IL_02bb:
		SkipIntroQuestSetIfNeeded();
		dataModel.IsOpening = true;
	}

	private void GoToNextQuestSet()
	{
		if (m_tavernGuideDatamodel == null)
		{
			return;
		}
		TavernGuideQuestSetCategoryDataModel currCategory = GetCurrentQuestCategory();
		if (currCategory != null)
		{
			int next = m_tavernGuideDatamodel.SelectedQuestSetIndex + 1;
			if (TryGetSafeQuestSetIndex(currCategory, next, out var newIndex))
			{
				m_tavernGuideDatamodel.SelectedQuestSetIndex = newIndex;
			}
			m_tavernGuideDatamodel.IsOpening = false;
		}
	}

	private void GoToPreviousQuestSet()
	{
		if (m_tavernGuideDatamodel == null)
		{
			return;
		}
		TavernGuideQuestSetCategoryDataModel currCategory = GetCurrentQuestCategory();
		if (currCategory != null)
		{
			int prev = m_tavernGuideDatamodel.SelectedQuestSetIndex - 1;
			if (TryGetSafeQuestSetIndex(currCategory, prev, out var newIndex))
			{
				m_tavernGuideDatamodel.SelectedQuestSetIndex = newIndex;
			}
			m_tavernGuideDatamodel.IsOpening = false;
		}
	}

	private TavernGuideQuestSetCategoryDataModel GetCurrentQuestCategory()
	{
		if (!TryGetSafeCategoryIndex(m_tavernGuideDatamodel.SelectedCategoryIndex, out var catIndex))
		{
			return null;
		}
		return m_tavernGuideDatamodel.TavernGuideQuestSetCategories[catIndex];
	}

	private TavernGuideQuestSetDataModel GetCurrentQuestSet()
	{
		TavernGuideQuestSetCategoryDataModel currCategory = GetCurrentQuestCategory();
		if (currCategory == null)
		{
			return null;
		}
		if (!TryGetSafeQuestSetIndex(currCategory, m_tavernGuideDatamodel.SelectedQuestSetIndex, out var setIndex))
		{
			return null;
		}
		return currCategory.TavernGuideQuestSets[setIndex];
	}

	private bool TryGetSafeCategoryIndex(int index, out int safeIndex)
	{
		safeIndex = -1;
		if (m_tavernGuideDatamodel == null || m_tavernGuideDatamodel.TavernGuideQuestSetCategories.Count == 0)
		{
			return false;
		}
		safeIndex = Mathf.Clamp(index, 0, m_tavernGuideDatamodel.TavernGuideQuestSetCategories.Count - 1);
		return true;
	}

	private bool TryGetSafeQuestSetIndex(TavernGuideQuestSetCategoryDataModel category, int index, out int safeIndex)
	{
		safeIndex = -1;
		TavernGuideQuestSetCategoryDataModel currCategory = GetCurrentQuestCategory();
		if (currCategory == null || currCategory.TavernGuideQuestSets.Count == 0)
		{
			return false;
		}
		safeIndex = Mathf.Clamp(index, 0, currCategory.TavernGuideQuestSets.Count - 1);
		return true;
	}

	private void SkipIntroQuestSetIfNeeded()
	{
		if (TavernGuideManager.Get().CanShowAllQuestSets() && m_tavernGuideDatamodel.SelectedCategoryIndex == 0 && m_tavernGuideDatamodel.SelectedQuestSetIndex == 0)
		{
			GoToNextQuestSet();
		}
	}
}
