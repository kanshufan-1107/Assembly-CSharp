using UnityEngine;

namespace Hearthstone.Tendril;

public class TendrilTarget : MonoBehaviour
{
	[SerializeField]
	private Transform m_anchor;

	[SerializeField]
	protected string m_trigger1;

	[SerializeField]
	protected string m_trigger2;

	[SerializeField]
	private float m_maxDistance;

	[SerializeField]
	private float m_smooth;

	[SerializeField]
	private float m_startweight;

	protected TendrilBase m_tendril;

	protected Animator m_animator;

	private Vector3 m_targetPosition;

	private Vector3 m_targetPositionRef;

	public bool interactive;

	public virtual void InitializeTarget(TendrilBase tendril)
	{
		m_tendril = tendril;
		m_tendril.controlWeight = m_startweight;
		m_animator = m_tendril.GetComponentInParent<Animator>();
	}

	public virtual void Selected(Vector3 target)
	{
		interactive = true;
		base.transform.position = target;
		m_tendril.AnimateWeight(1f, 0.5f);
	}

	public virtual void Deselected()
	{
		m_tendril.AnimateWeight(0f, 0.25f);
		if (m_animator != null)
		{
			m_animator.SetTrigger(m_trigger2);
		}
	}

	public virtual void UpdatePosition(Vector3 target)
	{
		m_targetPosition = Vector3.SmoothDamp(base.transform.position, ConstrainTarget(target), ref m_targetPositionRef, m_smooth);
		base.transform.position = m_targetPosition;
	}

	protected void InteractCondition()
	{
		interactive = false;
		m_tendril.AnimateWeight(0f, 0.25f);
		if (m_animator != null)
		{
			m_animator.SetTrigger(m_trigger1);
		}
	}

	protected Vector3 ConstrainTarget(Vector3 target)
	{
		if (m_anchor != null && Vector3.Distance(target, m_anchor.position) > m_maxDistance)
		{
			Vector3 vector = (target - m_anchor.position).normalized * m_maxDistance;
			InteractCondition();
			return vector + m_anchor.position;
		}
		return target;
	}
}
