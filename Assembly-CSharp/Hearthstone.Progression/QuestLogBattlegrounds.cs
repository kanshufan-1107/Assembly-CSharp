using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class QuestLogBattlegrounds : MonoBehaviour
{
	public Widget m_weeklyQuestsListWidget;

	public int m_maxQuestsPerRow;

	private Widget m_widget;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "INFOPOPUP_show")
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
				{
					m_headerText = GameStrings.Get("GLUE_PROGRESSION_BANKED_QUEST_INFO_HEADER"),
					m_text = GameStrings.Get("GLUE_PROGRESSION_BANKED_QUEST_INFO_BODY"),
					m_responseDisplay = AlertPopup.ResponseDisplay.OK,
					m_showAlertIcon = false
				};
				DialogManager.Get().ShowPopup(info);
			}
		});
		UpdateQuestLists();
	}

	private void UpdateQuestLists()
	{
		QuestListDataModel weeklyQuests = QuestManager.Get().CreateActiveQuestsDataModel(QuestPool.QuestPoolType.WEEKLY, QuestPool.RewardTrackType.BATTLEGROUNDS, appendTimeUntilNextQuest: true);
		GetComponent<Widget>().BindDataModel(weeklyQuests);
	}
}
