using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class StaticWidgetCacheLender : MonoBehaviour
{
	[SerializeField]
	private GameObject m_handlerObject;

	[SerializeField]
	private GameLayer m_layerOverride;

	public const string REQUEST_WIDGET = "REQUEST_WIDGET";

	public const string RETURN_WIDGETS = "RETURN_WIDGETS";

	private Widget m_widget;

	private IStaticWidgetCacheOwner m_cacheOwner;

	private string m_dataModelUniqueId;

	private StaticWidgetCacheBase Cache => m_cacheOwner.Cache;

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		if (!(m_widget == null))
		{
			m_cacheOwner = GetComponentInParent<IStaticWidgetCacheOwner>();
			if (m_cacheOwner != null)
			{
				m_widget.RegisterEventListener(HandleEvent);
			}
		}
	}

	private void OnDestroy()
	{
		ReturnWidgets();
	}

	private void RequestWidget(IDataModel dataModel)
	{
		if (m_cacheOwner == null || dataModel == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(m_dataModelUniqueId))
		{
			if (m_dataModelUniqueId == Cache.GetUniqueIdentifier(dataModel))
			{
				return;
			}
			ReturnWidgets();
		}
		Cache.RequestWidget(this, dataModel, m_handlerObject, m_layerOverride);
		m_dataModelUniqueId = Cache.GetUniqueIdentifier(dataModel);
	}

	private void ReturnWidgets()
	{
		if (m_cacheOwner != null && !string.IsNullOrEmpty(m_dataModelUniqueId))
		{
			Cache.ReturnWidgets(this);
			m_dataModelUniqueId = null;
		}
	}

	private void HandleEvent(string eventName)
	{
		if (eventName == "REQUEST_WIDGET")
		{
			EventDataModel eventDataModel = m_widget.GetDataModel<EventDataModel>();
			if (eventDataModel != null && eventDataModel.Payload != null && eventDataModel.Payload is IDataModel payload)
			{
				RequestWidget(payload);
			}
		}
		else if (eventName == "RETURN_WIDGETS")
		{
			ReturnWidgets();
		}
	}
}
