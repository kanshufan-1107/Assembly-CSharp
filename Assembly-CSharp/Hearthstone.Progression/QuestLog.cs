using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class QuestLog : MonoBehaviour
{
	[SerializeField]
	private Widget m_dailyQuestsListWidget;

	[SerializeField]
	private Widget m_weeklyQuestsListWidget;

	[SerializeField]
	private Widget m_eventQuestsListWidget;

	[SerializeField]
	private int m_maxQuestsPerRow;

	[SerializeField]
	private int m_maxQuestsPerRowDuringEvent;

	private Widget m_widget;

	private JournalMetaDataModel m_journalMeta;

	private bool m_showingEventLayout;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterReadyListener(HandleReady);
		m_widget.RegisterActivatedListener(HandleActivate);
		m_widget.RegisterDeactivatedListener(HandleDeactivate);
	}

	private void HandleReady(object unused)
	{
		m_journalMeta = m_widget.GetDataModel<JournalMetaDataModel>();
		m_showingEventLayout = ShouldShowEventLayout();
		UpdateDailyWeeklyQuestLists();
		UpdateEventQuestList();
		if (PopupDisplayManager.Get().QuestPopups.ShouldShowAdjustedQuestsPopupOnJournal())
		{
			m_widget.TriggerEvent("QUESTS_CHANGED");
			PopupDisplayManager.Get().QuestPopups.OnAdjustedQuestsPopupShownOnJournal();
		}
	}

	private void HandleActivate(object unused)
	{
		if (m_journalMeta != null)
		{
			bool num = ShouldShowEventLayout() != m_showingEventLayout;
			m_showingEventLayout = ShouldShowEventLayout();
			QuestManager.Get().OnQuestStateUpdate -= UpdateDailyWeeklyQuestLists;
			QuestManager.Get().OnQuestStateUpdate += UpdateDailyWeeklyQuestLists;
			if (num)
			{
				UpdateDailyWeeklyQuestLists();
			}
			UpdateEventQuestList();
			if (PopupDisplayManager.Get().QuestPopups.ShouldShowAdjustedQuestsPopupOnJournal())
			{
				m_widget.TriggerEvent("QUESTS_CHANGED");
				PopupDisplayManager.Get().QuestPopups.OnAdjustedQuestsPopupShownOnJournal();
			}
		}
	}

	private void HandleDeactivate(object unused)
	{
		QuestManager questManager = QuestManager.Get();
		if (questManager != null)
		{
			questManager.OnQuestStateUpdate -= UpdateDailyWeeklyQuestLists;
		}
	}

	private bool ShouldShowEventLayout()
	{
		if (m_journalMeta != null && m_journalMeta.EventActive && !m_journalMeta.EventCompleted)
		{
			return !m_journalMeta.EventIsNew;
		}
		return false;
	}

	private void UpdateDailyWeeklyQuestLists()
	{
		QuestListDataModel specialAndDailyQuests = new QuestListDataModel();
		QuestListDataModel questListDataModel = QuestManager.Get().CreateActiveQuestsDataModel(QuestPool.QuestPoolType.NONE, QuestPool.RewardTrackType.GLOBAL, appendTimeUntilNextQuest: true);
		int maxQuestsPerRow = (m_showingEventLayout ? m_maxQuestsPerRowDuringEvent : m_maxQuestsPerRow);
		foreach (QuestDataModel questDataModel in questListDataModel.Quests)
		{
			if (specialAndDailyQuests.Quests.Count < maxQuestsPerRow)
			{
				specialAndDailyQuests.Quests.Add(questDataModel);
				continue;
			}
			break;
		}
		foreach (QuestDataModel questDataModel2 in QuestManager.Get().CreateActiveQuestsDataModel(QuestPool.QuestPoolType.DAILY, QuestPool.RewardTrackType.GLOBAL, appendTimeUntilNextQuest: true).Quests)
		{
			if (specialAndDailyQuests.Quests.Count < maxQuestsPerRow)
			{
				specialAndDailyQuests.Quests.Add(questDataModel2);
				continue;
			}
			break;
		}
		m_dailyQuestsListWidget.BindDataModel(specialAndDailyQuests);
		QuestListDataModel weeklyQuests = QuestManager.Get().CreateActiveQuestsDataModel(QuestPool.QuestPoolType.WEEKLY, QuestPool.RewardTrackType.GLOBAL, appendTimeUntilNextQuest: true);
		m_weeklyQuestsListWidget.BindDataModel(weeklyQuests);
	}

	private void UpdateEventQuestList()
	{
		if (m_showingEventLayout)
		{
			RewardTrack eventTrack = RewardTrackManager.Get().GetCurrentEventRewardTrack();
			if (eventTrack != null && eventTrack.IsValid)
			{
				QuestListDataModel eventQuests = QuestManager.Get().CreateActiveQuestsDataModel(QuestPool.QuestPoolType.EVENT, (QuestPool.RewardTrackType)eventTrack.TrackDataModel.RewardTrackType, eventTrack.TrackDataModel.Level < eventTrack.TrackDataModel.LevelHardCap);
				eventQuests.Quests.Sort(QuestManager.SortChainQuestsToFront);
				m_eventQuestsListWidget.BindDataModel(eventQuests);
			}
		}
	}
}
