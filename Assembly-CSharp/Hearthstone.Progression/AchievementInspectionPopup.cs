using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

public class AchievementInspectionPopup : MonoBehaviour
{
	public const string NEXT_ACHIEVEMENT = "CODE_SELECT_NEXT_ACHIEVEMENT";

	public const string PREV_ACHIEVEMENT = "CODE_SELECT_PREVIOUS_ACHIEVEMENT";

	public const string UPDATE_ACHIEVEMENT = "CODE_UPDATE_ACHIEVEMENT";

	private Widget m_widget;

	private AchievementDataModel m_achievementDataModel;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		if (m_widget != null)
		{
			m_widget.RegisterEventListener(WidgetEventListener);
		}
	}

	private void OnDisable()
	{
		m_achievementDataModel = null;
		m_widget.UnbindDataModel(222);
	}

	private void WidgetEventListener(string eventName)
	{
		switch (eventName)
		{
		case "CODE_SELECT_PREVIOUS_ACHIEVEMENT":
			SelectPreviousAchievement();
			break;
		case "CODE_SELECT_NEXT_ACHIEVEMENT":
			SelectNextAchievement();
			break;
		case "CODE_UPDATE_ACHIEVEMENT":
			HandleUpdateAchievement();
			break;
		}
	}

	private void HandleUpdateAchievement()
	{
		EventDataModel eventDataModel = m_widget.GetDataModel<EventDataModel>();
		UpdateAchievement(eventDataModel.Payload as AchievementDataModel);
	}

	private void UpdateAchievement(AchievementDataModel achievement)
	{
		if (achievement != null)
		{
			AchievementManager.Get().LoadRewards(new DataModelList<AchievementDataModel> { achievement });
			m_achievementDataModel = achievement.CloneDataModel();
			m_achievementDataModel.DisplayMode = AchievementManager.AchievementDisplayMode.Inspection;
			m_widget.BindDataModel(m_achievementDataModel);
		}
	}

	private void SelectPreviousAchievement()
	{
		AchievementSectionDataModel section = AchievementManager.Get().GetAchievementSectionDataModelFromAchievement(m_achievementDataModel);
		AchievementDataModel achievement = m_achievementDataModel.FindPreviousAchievement(section.Achievements.Achievements);
		UpdateAchievement(achievement);
	}

	private void SelectNextAchievement()
	{
		AchievementSectionDataModel section = AchievementManager.Get().GetAchievementSectionDataModelFromAchievement(m_achievementDataModel);
		AchievementDataModel achievement = m_achievementDataModel.FindNextAchievement(section.Achievements.Achievements);
		UpdateAchievement(achievement);
	}
}
