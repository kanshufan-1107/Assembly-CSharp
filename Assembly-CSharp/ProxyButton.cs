using UnityEngine;

public class ProxyButton : PegUIElement
{
	[SerializeField]
	private GameObject m_target;

	private PegUIElement m_targetUIElem;

	private void TryInitTargetElement()
	{
		if (!m_targetUIElem && (bool)m_target)
		{
			m_targetUIElem = m_target.GetComponentInChildren<PegUIElement>();
		}
	}

	protected override void OnPress()
	{
		TryInitTargetElement();
		if ((bool)m_targetUIElem)
		{
			m_targetUIElem.TriggerPress();
		}
	}

	protected override void OnRelease()
	{
		TryInitTargetElement();
		if ((bool)m_targetUIElem)
		{
			m_targetUIElem.TriggerRelease();
		}
	}

	protected override void OnReleaseAll(bool mouseIsOver)
	{
		TryInitTargetElement();
		if ((bool)m_targetUIElem)
		{
			m_targetUIElem.TriggerReleaseAll(mouseIsOver);
		}
	}

	protected override void OnOver(InteractionState oldState)
	{
		TryInitTargetElement();
		if ((bool)m_targetUIElem)
		{
			m_targetUIElem.TriggerOver();
		}
	}

	protected override void OnOut(InteractionState oldState)
	{
		TryInitTargetElement();
		if ((bool)m_targetUIElem)
		{
			m_targetUIElem.TriggerOut();
		}
	}

	protected override void OnHold()
	{
		TryInitTargetElement();
		if ((bool)m_targetUIElem)
		{
			m_targetUIElem.TriggerHold();
		}
	}

	protected override void OnRightClick()
	{
		TryInitTargetElement();
		if ((bool)m_targetUIElem)
		{
			m_targetUIElem.TriggerRightClick();
		}
	}

	protected override void OnDoubleClick()
	{
		TryInitTargetElement();
		if ((bool)m_targetUIElem)
		{
			m_targetUIElem.TriggerDoubleClick();
		}
	}

	protected override void OnTap()
	{
		TryInitTargetElement();
		if ((bool)m_targetUIElem)
		{
			m_targetUIElem.TriggerTap();
		}
	}

	protected override void OnDrag()
	{
		TryInitTargetElement();
		if ((bool)m_targetUIElem)
		{
			m_targetUIElem.TriggerDrag();
		}
	}
}
