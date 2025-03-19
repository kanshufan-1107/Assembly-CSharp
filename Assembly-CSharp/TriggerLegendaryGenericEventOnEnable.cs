using UnityEngine;

public class TriggerLegendaryGenericEventOnEnable : MonoBehaviour
{
	public string m_OnEnableEventName;

	public string m_OnDisableEventName;

	public Actor m_Actor;

	private void OnEnable()
	{
		if (string.IsNullOrEmpty(m_OnEnableEventName))
		{
			return;
		}
		if (m_Actor == null)
		{
			m_Actor = LegendaryUtil.FindLegendaryHeroActor(base.gameObject);
			if (m_Actor == null)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
		}
		m_Actor.LegendaryHeroPortrait?.RaiseGenericEvent(m_OnEnableEventName, m_Actor);
	}

	private void OnDisable()
	{
		if (!string.IsNullOrEmpty(m_OnDisableEventName) && !(m_Actor == null))
		{
			m_Actor.LegendaryHeroPortrait?.RaiseGenericEvent(m_OnDisableEventName, m_Actor);
		}
	}
}
