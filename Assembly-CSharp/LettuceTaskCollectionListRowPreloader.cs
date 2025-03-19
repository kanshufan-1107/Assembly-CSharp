using Hearthstone.UI;
using UnityEngine;

public class LettuceTaskCollectionListRowPreloader : MonoBehaviour
{
	private Widget m_widget;

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		m_widget.RegisterEventListener(HandleEvent);
	}

	private void HandleEvent(string eventName)
	{
		if (eventName == "POST_SHOW_TASK_ITEM")
		{
			m_widget.TriggerEvent("SHOW_TASK_ITEM");
		}
		else if (eventName == "POST_HIDE_TASK_ITEM")
		{
			m_widget.TriggerEvent("HIDE_TASK_ITEM");
		}
	}
}
