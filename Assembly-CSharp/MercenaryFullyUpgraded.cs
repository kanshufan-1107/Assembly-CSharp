using System;
using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(VisualController), typeof(Widget))]
public class MercenaryFullyUpgraded : MonoBehaviour
{
	private List<Action> m_doneCallbacks;

	public Widget Widget { get; private set; }

	public VisualController VisualController { get; private set; }

	private void Awake()
	{
		VisualController = GetComponent<VisualController>();
		Widget = GetComponent<Widget>();
		m_doneCallbacks = new List<Action>();
		Widget.RegisterEventListener(WidgetEventListener);
	}

	public void RegisterDoneCallback(Action action)
	{
		if (action != null)
		{
			m_doneCallbacks.Add(action);
		}
	}

	private void WidgetEventListener(string eventName)
	{
		if (!(eventName == "HIDE_COMPLETE_code"))
		{
			return;
		}
		foreach (Action doneCallback in m_doneCallbacks)
		{
			doneCallback();
		}
		m_doneCallbacks.Clear();
	}
}
