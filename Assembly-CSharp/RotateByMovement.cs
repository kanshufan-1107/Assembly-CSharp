using System;
using UnityEngine;

public class RotateByMovement : MonoBehaviour
{
	private Vector3 m_previousPos;

	public GameObject mParent;

	private void Update()
	{
		Transform parent = mParent.transform;
		if (!(m_previousPos == parent.localPosition))
		{
			if (m_previousPos == Vector3.zero)
			{
				m_previousPos = parent.localPosition;
				return;
			}
			Vector3 currentPos = parent.localPosition;
			float yDiff = currentPos.z - m_previousPos.z;
			float hyp = Mathf.Sqrt(Mathf.Pow(currentPos.x - m_previousPos.x, 2f) + Mathf.Pow(yDiff, 2f));
			float angle = Mathf.Asin(yDiff / hyp) * 180f / (float)Math.PI;
			angle -= 90f;
			base.transform.localEulerAngles = new Vector3(90f, angle, 0f);
			m_previousPos = currentPos;
		}
	}
}
