using System;
using System.Collections.Generic;
using UnityEngine;

public class UberCurve : MonoBehaviour
{
	[Serializable]
	public class UberCurveControlPoint
	{
		public Vector3 position = Vector3.zero;
	}

	public bool m_Looping;

	public bool m_Reverse;

	public float m_HandleSize = 0.3f;

	public List<UberCurveControlPoint> m_controlPoints;

	private int m_gizmoSteps = 10;

	private bool m_renderControlPoints;

	private bool m_renderStepPoints;

	private bool m_renderingGizmo;

	private void Awake()
	{
		CatmullRomInit();
	}

	private void OnDrawGizmos()
	{
		CatmullRomGizmo();
	}

	public Vector3 CatmullRomEvaluateWorldPosition(float position)
	{
		Vector3 localPosition = CatmullRomEvaluateLocalPosition(position);
		return base.transform.TransformPoint(localPosition);
	}

	public Vector3 CatmullRomEvaluateLocalPosition(float position)
	{
		if (m_controlPoints == null)
		{
			return Vector3.zero;
		}
		int controlPointCount = m_controlPoints.Count;
		if (!m_Looping)
		{
			controlPointCount = m_controlPoints.Count - 1;
		}
		if (m_Reverse && !m_renderingGizmo)
		{
			position = 1f - position;
		}
		position = Mathf.Clamp01(position);
		int idx = Mathf.FloorToInt(position * (float)controlPointCount);
		float p = position * (float)controlPointCount - (float)idx;
		Vector3 pointA = m_controlPoints[ClampIndexCatmullRom(idx - 1)].position;
		Vector3 pointB = m_controlPoints[idx].position;
		Vector3 pointC = m_controlPoints[ClampIndexCatmullRom(idx + 1)].position;
		Vector3 pointD = m_controlPoints[ClampIndexCatmullRom(idx + 2)].position;
		return CatmullRomCalc(p, pointA, pointB, pointC, pointD);
	}

	public Vector3 CatmullRomEvaluateDirection(float position)
	{
		if (m_controlPoints == null)
		{
			return Vector3.zero;
		}
		Vector3 pointA = CatmullRomEvaluateLocalPosition(position);
		return Vector3.Normalize(CatmullRomEvaluateLocalPosition(position + 0.01f) - pointA);
	}

	private void CatmullRomInit()
	{
		if (m_controlPoints == null)
		{
			m_controlPoints = new List<UberCurveControlPoint>();
			for (int i = 0; i < 4; i++)
			{
				UberCurveControlPoint cp = new UberCurveControlPoint();
				cp.position = new Vector3(0f, 0f, (float)i * 4f);
				m_controlPoints.Add(cp);
			}
		}
	}

	[ContextMenu("Show Curve Steps")]
	private void ShowRenderSteps()
	{
		m_renderStepPoints = !m_renderStepPoints;
	}

	private void CatmullRomGizmo()
	{
		if (m_gizmoSteps < 1)
		{
			m_gizmoSteps = 1;
		}
		if (m_controlPoints == null)
		{
			CatmullRomInit();
		}
		if (m_controlPoints.Count < 4)
		{
			return;
		}
		m_renderingGizmo = true;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		float step = 0f;
		step = ((!m_Looping) ? (1f / (float)(m_gizmoSteps * (m_controlPoints.Count - 1))) : (1f / (float)(m_gizmoSteps * m_controlPoints.Count)));
		Gizmos.color = Color.cyan;
		Vector3 lastPosition = m_controlPoints[0].position;
		for (float p = 0f; p <= 1f; p += step)
		{
			Vector3 newPosition = CatmullRomEvaluateLocalPosition(p);
			Gizmos.DrawLine(lastPosition, newPosition);
			lastPosition = newPosition;
		}
		if (m_renderStepPoints)
		{
			Gizmos.color = new Color(0.2f, 0.2f, 0.9f, 0.75f);
			for (float p2 = 0f; p2 <= 1f; p2 += step)
			{
				Gizmos.DrawSphere(CatmullRomEvaluateLocalPosition(p2), m_HandleSize * 0.15f);
			}
		}
		if (m_renderControlPoints)
		{
			Gizmos.color = new Color(0.3f, 0.3f, 1f, 1f);
			for (int i = 0; i < m_controlPoints.Count; i++)
			{
				Gizmos.DrawSphere(m_controlPoints[i].position, 0.25f);
			}
		}
		m_renderingGizmo = false;
	}

	private Vector3 CatmullRomCalc(float i, Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD)
	{
		Vector3 vector = 0.5f * (2f * pointB);
		Vector3 v2 = 0.5f * (pointC - pointA);
		Vector3 v3 = 0.5f * (2f * pointA - 5f * pointB + 4f * pointC - pointD);
		Vector3 v4 = 0.5f * (-pointA + 3f * pointB - 3f * pointC + pointD);
		return vector + v2 * i + v3 * i * i + v4 * i * i * i;
	}

	private int ClampIndexCatmullRom(int pos)
	{
		if (m_Looping)
		{
			if (pos < 0)
			{
				pos = m_controlPoints.Count - 1;
			}
			if (pos > m_controlPoints.Count)
			{
				pos = 1;
			}
			else if (pos > m_controlPoints.Count - 1)
			{
				pos = 0;
			}
		}
		else
		{
			if (pos < 0)
			{
				pos = 0;
			}
			if (pos > m_controlPoints.Count - 1)
			{
				pos = m_controlPoints.Count - 1;
			}
		}
		return pos;
	}
}
