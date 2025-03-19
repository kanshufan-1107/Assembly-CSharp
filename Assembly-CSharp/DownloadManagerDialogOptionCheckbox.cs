using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class DownloadManagerDialogOptionCheckbox : MonoBehaviour, IWidgetEventListener
{
	private const string ON_CHECKBOX_SELECTED_EVENT = "ON_CHECKBOX_SELECTED";

	private WidgetTemplate m_rootWidget;

	public WidgetTemplate OwningWidget => m_rootWidget;

	private void Awake()
	{
		m_rootWidget = GetComponent<WidgetTemplate>();
	}

	public WidgetEventListenerResponse EventReceived(string eventName, TriggerEventParameters eventParams)
	{
		bool consumed = false;
		if (eventName == "ON_CHECKBOX_SELECTED")
		{
			DownloadManagerDialogOptionCheckboxDataModel optionDataModel = m_rootWidget.GetDataModel<DownloadManagerDialogOptionCheckboxDataModel>();
			SendEventUpwardStateAction.SendEventUpward(base.gameObject, "OPTION_SELECT_EVENT", new EventDataModel
			{
				Payload = optionDataModel.ID
			});
			consumed = true;
		}
		WidgetEventListenerResponse result = default(WidgetEventListenerResponse);
		result.Consumed = consumed;
		return result;
	}
}
