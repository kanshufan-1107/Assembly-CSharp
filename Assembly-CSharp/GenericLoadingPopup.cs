using System;
using Hearthstone.UI;
using UnityEngine;

public class GenericLoadingPopup : MonoBehaviour, IWidgetEventListener
{
	private const string BUTTON_ON_CLICK_EVENT = "ON_BUTTON_CLICKED";

	private const string LOADING_STATE = "LOADING";

	private const string COMPLETED_STATE = "COMPLETED";

	private const string FAILED_STATE = "FAILED";

	private WidgetTemplate m_widgetTemplate;

	[SerializeField]
	private UberText m_messageText;

	private Action m_buttonCallback;

	public WidgetTemplate OwningWidget => m_widgetTemplate;

	private void Awake()
	{
		m_widgetTemplate = GetComponent<WidgetTemplate>();
	}

	public void SetLoading(string message)
	{
		m_buttonCallback = null;
		UpdateMessage(message);
		SendEventDownwardStateAction.SendEventDownward(base.gameObject, "LOADING", SendEventDownwardStateAction.BubbleDownEventDepth.AllDescendants);
	}

	public void SetCompleted(string message, Action buttonCallback)
	{
		m_buttonCallback = buttonCallback;
		UpdateMessage(message);
		SendEventDownwardStateAction.SendEventDownward(base.gameObject, "COMPLETED", SendEventDownwardStateAction.BubbleDownEventDepth.AllDescendants);
	}

	public void SetFailed(string message, Action buttonCallback)
	{
		m_buttonCallback = buttonCallback;
		UpdateMessage(message);
		SendEventDownwardStateAction.SendEventDownward(base.gameObject, "FAILED", SendEventDownwardStateAction.BubbleDownEventDepth.AllDescendants);
	}

	public void UpdateMessage(string message)
	{
		m_messageText.Text = message;
	}

	public WidgetEventListenerResponse EventReceived(string eventName, TriggerEventParameters eventParams)
	{
		bool consumed = false;
		if (eventName == "ON_BUTTON_CLICKED")
		{
			m_buttonCallback?.Invoke();
			consumed = true;
		}
		WidgetEventListenerResponse result = default(WidgetEventListenerResponse);
		result.Consumed = consumed;
		return result;
	}
}
