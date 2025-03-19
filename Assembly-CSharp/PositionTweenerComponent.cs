using UnityEngine;

public class PositionTweenerComponent : MonoBehaviour
{
	[SerializeField]
	private Transform m_transformComponent;

	[SerializeField]
	private PositionTweener m_settings = new PositionTweener();

	private void Reset()
	{
		if (m_transformComponent == null)
		{
			m_transformComponent = base.transform;
		}
		SetCurrentAsInitialPosition();
		SetCurrentAsFinalPosition();
	}

	[ContextMenu("Play Forward")]
	public void PlayForward()
	{
		m_settings.Play(base.gameObject, forward: true);
	}

	[ContextMenu("Play Reverse")]
	public void PlayReverse()
	{
		m_settings.Play(base.gameObject, forward: false);
	}

	[ContextMenu("Set Current As Initial Position")]
	public void SetCurrentAsInitialPosition()
	{
		m_settings.SetInitialPosition(m_settings.IsLocal ? m_transformComponent.localPosition : m_transformComponent.position);
	}

	[ContextMenu("Set Current As Final Position")]
	public void SetCurrentAsFinalPosition()
	{
		m_settings.SetFinalPosition(m_settings.IsLocal ? m_transformComponent.localPosition : m_transformComponent.position);
	}

	[ContextMenu("Reset To Beginning")]
	public void ResetToBeginning()
	{
		if (m_settings.IsLocal)
		{
			m_transformComponent.localPosition = m_settings.InitialPosition;
		}
		else
		{
			m_transformComponent.position = m_settings.InitialPosition;
		}
	}

	[ContextMenu("Set To End")]
	public void SetToEnd()
	{
		if (m_settings.IsLocal)
		{
			m_transformComponent.localPosition = m_settings.FinalPosition;
		}
		else
		{
			m_transformComponent.position = m_settings.FinalPosition;
		}
	}
}
