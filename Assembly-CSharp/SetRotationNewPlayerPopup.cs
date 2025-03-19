using Hearthstone.UI;
using UnityEngine;

public class SetRotationNewPlayerPopup : BasicPopup
{
	private const string HIDE_FINISHED_EVENT_NAME = "CODE_HIDE_FINISHED";

	private WidgetTemplate m_widget;

	protected override void Awake()
	{
		base.Awake();
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "Button_Framed_Clicked")
			{
				Hide();
			}
			if (eventName == "CODE_HIDE_FINISHED")
			{
				Object.Destroy(base.gameObject);
			}
		});
		m_widget.RegisterReadyListener(delegate
		{
			OnWidgetReady();
		});
	}

	protected override void OnDestroy()
	{
		GameObject go = base.transform.parent.gameObject;
		if (go != null && go.GetComponent<WidgetInstance>() != null)
		{
			Object.Destroy(base.transform.parent.gameObject);
		}
		base.OnDestroy();
	}

	private void OnWidgetReady()
	{
		if (m_headerText != null)
		{
			m_headerText.Text = GameStrings.Format("GLUE_NEW_PLAYER_SET_ROTATION_POPUP_HEADER", SetRotationManager.Get().GetActiveSetRotationYearLocalizedString());
		}
	}
}
