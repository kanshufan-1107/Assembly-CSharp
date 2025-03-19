using UnityEngine;

namespace Hearthstone;

[AddComponentMenu("Hearthstone/Match Angle to Movement")]
[DisallowMultipleComponent]
public class MatchAngleToMovement : MonoBehaviour
{
	private Vector3 m_previousPosition;

	[SerializeField]
	private Vector3 m_offset;

	private void Reset()
	{
		m_offset = Vector3.zero;
	}

	private void Start()
	{
		m_previousPosition = base.transform.position;
	}

	private void LateUpdate()
	{
		if (!Approximately(base.transform.position, m_previousPosition))
		{
			Vector3 direction = (base.transform.position - m_previousPosition).normalized;
			base.transform.LookAt(base.transform.position + direction, Vector3.up);
			base.transform.localEulerAngles += m_offset;
			m_previousPosition = base.transform.position;
		}
	}

	private static bool Approximately(Vector3 a, Vector3 b)
	{
		if (!Mathf.Approximately(a.x, b.x))
		{
			return false;
		}
		if (!Mathf.Approximately(a.y, b.y))
		{
			return false;
		}
		if (!Mathf.Approximately(a.z, b.z))
		{
			return false;
		}
		return true;
	}
}
