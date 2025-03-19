using System;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class RatingsPopupControl : MonoBehaviour
{
	public bool WaitForUserToStart;

	public string m_startPressedEvent = "USER_START_PRESSED";

	public string m_inputModeClickEvent = "INPUT_MODE_CLICK";

	public string m_inputModeTouchEvent = "INPUT_MODE_TOUCH";

	private Widget m_widget;

	public event Action OnUserStartPressed;

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		m_widget.RegisterEventListener(OnEventMessage);
		m_widget.RegisterReadyListener(OnWidgetReady);
	}

	private void OnEventMessage(string eventName)
	{
		if (eventName.Equals(m_startPressedEvent))
		{
			this.OnUserStartPressed?.Invoke();
		}
	}

	private void OnWidgetReady(object obj)
	{
		switch (PlatformSettings.Input)
		{
		case InputCategory.Mouse:
			m_widget.TriggerEvent(m_inputModeClickEvent);
			break;
		case InputCategory.Touch:
			m_widget.TriggerEvent(m_inputModeTouchEvent);
			break;
		}
	}
}
