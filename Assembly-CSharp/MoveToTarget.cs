using UnityEngine;

public class MoveToTarget : MonoBehaviour
{
	public enum MoveType
	{
		MoveByTime,
		MoveBySpeed
	}

	public Transform m_StartPosition;

	public Transform m_TargetObject;

	public MoveType m_MoveType;

	public float m_Time = 1f;

	public float m_Speed = 1f;

	public float m_SnapDistance = 0.1f;

	public bool m_OrientToPath;

	public bool m_AnimateOnStart;

	private bool m_Animate;

	private bool m_isDone;

	private Vector3 m_LastTargetPosition;

	private float m_LerpPosition;

	private void Start()
	{
		if (m_AnimateOnStart)
		{
			StartAnimation();
		}
	}

	private void Update()
	{
		if (m_MoveType == MoveType.MoveByTime)
		{
			MoveTime();
		}
		else
		{
			MoveSpeed();
		}
	}

	private void MoveTime()
	{
		if (m_isDone)
		{
			base.transform.position = m_TargetObject.position;
		}
		if (m_Animate)
		{
			Vector3 currentTargetPosition = m_TargetObject.position;
			float rate = 1f / m_Time;
			m_LerpPosition += rate * Time.deltaTime;
			if (m_LerpPosition > 1f)
			{
				m_isDone = true;
				base.transform.position = m_TargetObject.position;
			}
			else
			{
				Vector3 position = Vector3.Lerp(m_StartPosition.position, currentTargetPosition, m_LerpPosition);
				base.transform.position = position;
			}
		}
	}

	private void MoveSpeed()
	{
		if (m_isDone)
		{
			base.transform.position = m_TargetObject.position;
		}
		if (m_Animate)
		{
			if (Vector3.Distance(base.transform.position, m_TargetObject.position) < m_SnapDistance)
			{
				m_isDone = true;
				base.transform.position = m_TargetObject.position;
				return;
			}
			Vector3 delta = m_TargetObject.position - base.transform.position;
			delta.Normalize();
			float moveSpeed = m_Speed * Time.deltaTime;
			base.transform.position = base.transform.position + delta * moveSpeed;
		}
	}

	private void StartAnimation()
	{
		if ((bool)m_StartPosition)
		{
			base.transform.position = m_StartPosition.position;
		}
		m_Animate = true;
		m_LerpPosition = 0f;
	}
}
