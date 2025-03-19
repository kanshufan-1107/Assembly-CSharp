using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(Widget))]
public class LettuceShowMercHoverCard : MonoBehaviour
{
	[CustomEditField(Sections = "Bones")]
	public Transform m_hoverCardTopBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_hoverCardBottomBone;

	public AsyncReference m_mercHoverReference;

	private Widget m_mercHoverCard;

	private Widget m_widget;

	private void Start()
	{
		m_widget = GetComponent<Widget>();
		m_widget.RegisterEventListener(EventListener);
		m_mercHoverReference.RegisterReadyListener<Widget>(OnMercHoverCardWidgetReady);
	}

	private void OnDestroy()
	{
		m_mercHoverCard.UnbindDataModel(216);
		m_widget.RemoveEventListener(EventListener);
	}

	private void OnMercHoverCardWidgetReady(Widget widget)
	{
		m_mercHoverCard = widget;
		m_mercHoverCard.SetLayerOverride(GameLayer.IgnoreFullScreenEffects);
		HideMercHoverCard();
	}

	private void EventListener(string eventName)
	{
		if (!(eventName == "MERC_OVER_code"))
		{
			if (eventName == "MERC_OUT_code")
			{
				HideMercHoverCard();
			}
		}
		else if (m_widget.GetDataModel<EventDataModel>().Payload is LettuceMercenaryDataModel mercDataModel)
		{
			ShowHoverCard(mercDataModel);
		}
	}

	private void ShowHoverCard(IDataModel dataModel)
	{
		m_mercHoverCard.BindDataModel(dataModel);
		float z = PegUI.Get().GetMousedOverElement().transform.position.z;
		float top = ((m_hoverCardTopBone != null) ? m_hoverCardTopBone.position.z : base.transform.position.z);
		float bottom = ((m_hoverCardBottomBone != null) ? m_hoverCardBottomBone.position.z : base.transform.position.z);
		z = Mathf.Clamp(z, bottom, top);
		TransformUtil.SetPosZ(m_mercHoverCard.transform, z);
		m_mercHoverCard.gameObject.SetActive(value: true);
	}

	private void HideMercHoverCard()
	{
		m_mercHoverCard.gameObject.SetActive(value: false);
	}
}
