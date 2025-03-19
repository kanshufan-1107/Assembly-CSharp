using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class AchievementPin : MonoBehaviour
{
	private Widget m_widget;

	private const string SHOW_NOTIF = "SHOW_NOTIF";

	private const string HIDE_NOTIF = "HIDE_NOTIF";

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
	}

	private void OnEnable()
	{
		AchievementCategoryDataModel category = m_widget.GetDataModel<AchievementCategoryDataModel>();
		if (category != null)
		{
			if (category.Stats.Unclaimed > 0)
			{
				m_widget.TriggerEvent("SHOW_NOTIF");
			}
			else
			{
				m_widget.TriggerEvent("HIDE_NOTIF");
			}
		}
	}
}
