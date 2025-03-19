using System.Collections.Generic;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;

[CustomEditClass]
public class CompletedQuestsUpdatedPopup : DialogBase
{
	public class Info
	{
		public List<int> m_quests;

		public Dictionary<int, List<QuestChangeDbfRecord>> m_changes;

		public HideCallback m_callbackOnHide;
	}

	[CustomEditField(Sections = "Object Links")]
	public UIBButton m_okayButton;

	[CustomEditField(Sections = "Object Links")]
	public Clickable m_leftButton;

	[CustomEditField(Sections = "Object Links")]
	public Clickable m_rightButton;

	private const int MAX_QUESTS_PER_PAGE = 3;

	private int m_currentPage;

	private int m_maxPages = 1;

	private const string ON_LEFT_BUTTON_PRESSED = "left_arrow_clicked";

	private const string ON_RIGHT_BUTTON_PRESSED = "right_arrow_clicked";

	private Info m_info = new Info();

	protected override void Awake()
	{
		base.Awake();
		m_okayButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			Hide();
		});
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (UniversalInputManager.Get() != null)
		{
			UniversalInputManager.Get().SetSystemDialogActive(active: false);
		}
	}

	public void SetInfo(Info info)
	{
		m_info = info;
		if (m_info.m_callbackOnHide != null)
		{
			AddHideListener(m_info.m_callbackOnHide);
		}
		m_maxPages = m_info.m_quests.Count / 3 + 1;
		ShowPage(0);
	}

	public override void Show()
	{
		base.Show();
		DialogBase.DoBlur();
		UniversalInputManager.Get().SetSystemDialogActive(active: true);
		Widget widget = base.gameObject.GetComponent<Widget>();
		if (widget != null)
		{
			widget.RegisterEventListener(HandleEvent);
		}
		UpdateArrowButtonVisibility();
	}

	public override void Hide()
	{
		base.Hide();
		DialogBase.EndBlur();
		Widget widget = base.gameObject.GetComponent<Widget>();
		if (widget != null)
		{
			widget.RemoveEventListener(HandleEvent);
		}
	}

	private void ShowPage(int pageIndex)
	{
		if (pageIndex < 0 || pageIndex >= 3)
		{
			return;
		}
		m_currentPage = pageIndex;
		Widget widgetComponent = base.gameObject.GetComponent<Widget>();
		if (widgetComponent != null)
		{
			QuestListDataModel questListDataModel = new QuestListDataModel();
			for (int currentQuestIndex = pageIndex * 3; currentQuestIndex < m_info.m_quests.Count; currentQuestIndex++)
			{
				if (questListDataModel.Quests.Count >= 3)
				{
					break;
				}
				int questId = m_info.m_quests[currentQuestIndex];
				QuestDataModel currentQuestDataModel = QuestManager.Get().CreateQuestDataModelById(questId);
				currentQuestDataModel.DisplayMode = QuestManager.QuestDisplayMode.Notification;
				if (!m_info.m_changes.TryGetValue(questId, out var currentQuestChanges))
				{
					continue;
				}
				foreach (QuestChangeDbfRecord questChangeDbfRecord in currentQuestChanges)
				{
					switch (questChangeDbfRecord.ChangeAttribute)
					{
					case QuestChange.ChangeAttribute.QUOTA:
						currentQuestDataModel.QuotaChangeStatus = QuestManager.TranslateQuestChangeState(questChangeDbfRecord.ChangeType);
						break;
					case QuestChange.ChangeAttribute.TRIGGER:
						currentQuestDataModel.TriggerChangeStatus = QuestManager.TranslateQuestChangeState(questChangeDbfRecord.ChangeType);
						break;
					case QuestChange.ChangeAttribute.XP:
						currentQuestDataModel.XPChangeStatus = QuestManager.TranslateQuestChangeState(questChangeDbfRecord.ChangeType);
						break;
					}
				}
				questListDataModel.Quests.Add(currentQuestDataModel);
			}
			widgetComponent.BindDataModel(questListDataModel);
		}
		UpdateArrowButtonVisibility();
	}

	private void UpdateArrowButtonVisibility()
	{
		if (m_leftButton != null)
		{
			m_leftButton.gameObject.SetActive(m_currentPage != 0);
		}
		if (m_rightButton != null)
		{
			m_rightButton.gameObject.SetActive(m_currentPage < m_maxPages - 1);
		}
	}

	private void HandleEvent(string eventName)
	{
		if (!(eventName == "left_arrow_clicked"))
		{
			if (eventName == "right_arrow_clicked")
			{
				ShowPage(m_currentPage + 1);
			}
		}
		else
		{
			ShowPage(m_currentPage - 1);
		}
	}
}
