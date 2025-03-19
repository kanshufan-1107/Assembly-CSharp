using System;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class EventEndedPopup : MonoBehaviour
{
	[SerializeField]
	private UberText m_messageBody;

	public static readonly AssetReference EVENT_ENDED_POPUP_PREFAB = new AssetReference("EventEndedPopup.prefab:2e21ebc3432a3044294370e100cbf81a");

	private const string CODE_DISMISS = "CODE_DISMISS";

	private Widget m_widget;

	private GameObject m_owner;

	private Action m_callback;

	private SpecialEventDataModel m_specialEventDataModel;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "CODE_DISMISS")
			{
				Hide();
			}
		});
		m_owner = base.gameObject;
		if (base.transform.parent != null && base.transform.parent.GetComponent<WidgetInstance>() != null)
		{
			m_owner = base.transform.parent.gameObject;
		}
	}

	private void OnDestroy()
	{
		UIContext.GetRoot().DismissPopup(m_owner);
		m_callback?.Invoke();
	}

	public void Initialize(Action callback, SpecialEventDataModel specialEventDataModel)
	{
		m_callback = callback;
		if (specialEventDataModel == null)
		{
			Debug.LogError("EventEndedPopup initialized without an Event.");
			return;
		}
		m_specialEventDataModel = specialEventDataModel;
		m_widget.BindDataModel(m_specialEventDataModel);
	}

	public void Show()
	{
		if (m_messageBody != null && m_specialEventDataModel != null)
		{
			m_messageBody.Text = GameStrings.Format("GLUE_PROGRESSION_EVENT_TAB_POPUP_EXPIRED_BODY", m_specialEventDataModel.Name);
		}
		OverlayUI.Get().AddGameObject(base.transform.parent.gameObject);
		m_widget.RegisterDoneChangingStatesListener(delegate
		{
			UIContext.GetRoot().ShowPopup(base.gameObject);
		}, null, callImmediatelyIfSet: true, doOnce: true);
	}

	public void Hide()
	{
		UIContext.GetRoot().DismissPopup(base.gameObject);
		m_widget.Hide();
		UnityEngine.Object.Destroy(m_owner);
	}
}
