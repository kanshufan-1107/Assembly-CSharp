using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IK_Limb : IK_Chain
{
	public float snapBack;

	public float delta;

	public int iterations;

	public bool pole;

	private List<Vector3> m_startDirection = new List<Vector3>();

	private List<Vector3> m_positions = new List<Vector3>();

	private Quaternion m_startRotationTarget;

	private List<float> m_bonesLength;

	private float m_completeLength;

	protected override void InitializeIK()
	{
		base.InitializeIK();
		ResetVariables();
		m_startRotationTarget = GetRotationRootSpace(chainTarget.rotation);
		Transform current = leafBone;
		for (int i = 0; i < m_bones.Count; i++)
		{
			m_positions.Add(Vector3.zero);
			m_startRotationBone.Add(GetRotationRootSpace(current.rotation));
			if (i == 0)
			{
				m_startDirection.Add(GetPositionRootSpace(chainTarget.position) - GetPositionRootSpace(current.position));
				current = current.parent;
				continue;
			}
			m_startDirection.Add(GetPositionRootSpace(m_bones[i - 1].position) - GetPositionRootSpace(current.position));
			m_bonesLength.Add(m_startDirection[i].magnitude);
			m_completeLength += 1f;
			current = current.parent;
		}
		m_bones.Reverse();
		m_startRotationBone.Reverse();
		m_startDirection.Reverse();
		m_bonesLength.Reverse();
		m_initialized = true;
	}

	private void ResetVariables()
	{
		m_startDirection = new List<Vector3>();
		m_positions = new List<Vector3>();
		m_startRotationBone = new List<Quaternion>();
		m_bonesLength = new List<float>();
		m_completeLength = 0f;
	}

	protected override void ResolveIK()
	{
		base.ResolveIK();
		FABRIK();
	}

	private void FABRIK()
	{
		for (int i = 0; i < m_bones.Count; i++)
		{
			m_positions[i] = GetPositionRootSpace(m_bones[i].position);
		}
		Vector3 targetPosition = GetPositionRootSpace(chainTarget.position);
		Quaternion targetRotation = GetRotationRootSpace(chainTarget.rotation);
		if (OutOfReach(targetPosition))
		{
			Stretch(targetPosition);
		}
		else
		{
			for (int j = 0; j < m_positions.Count - 1; j++)
			{
				m_positions[j + 1] = Vector3.Lerp(m_positions[j + 1], m_positions[j] + m_startDirection[j], snapBack);
			}
			for (int k = 0; k < iterations; k++)
			{
				IterateBackward(targetPosition);
				IterateForward();
				if ((m_positions[m_positions.Count - 1] - targetPosition).sqrMagnitude < delta * delta)
				{
					break;
				}
			}
		}
		ResolvePole();
		SetPositionRotation(targetRotation);
	}

	private bool OutOfReach(Vector3 targetPosition)
	{
		return (targetPosition - GetPositionRootSpace(m_bones[0].position)).sqrMagnitude >= m_completeLength * m_completeLength;
	}

	private void Stretch(Vector3 targetPosition)
	{
		Vector3 direction = (targetPosition - m_positions[0]).normalized;
		for (int i = 1; i < m_positions.Count; i++)
		{
			m_positions[i] = m_positions[i - 1] + direction * m_bonesLength[i - 1];
		}
	}

	private void IterateBackward(Vector3 targetPosition)
	{
		for (int i = m_positions.Count - 1; i > 0; i--)
		{
			if (i == m_positions.Count - 1)
			{
				m_positions[i] = targetPosition;
			}
			else
			{
				m_positions[i] = m_positions[i + 1] + (m_positions[i] - m_positions[i + 1]).normalized * m_bonesLength[i];
			}
		}
	}

	private void IterateForward()
	{
		for (int i = 1; i < m_positions.Count; i++)
		{
			m_positions[i] = m_positions[i - 1] + (m_positions[i] - m_positions[i - 1]).normalized * m_bonesLength[i - 1];
		}
	}

	private void ResolvePole()
	{
		if (pole)
		{
			Vector3 polePosition = GetPositionRootSpace(chainPole.position);
			for (int i = 1; i < m_positions.Count - 1; i++)
			{
				Plane plane = new Plane(m_positions[i + 1] - m_positions[i - 1], m_positions[i - 1]);
				Vector3 projectedPole = plane.ClosestPointOnPlane(polePosition);
				float angle = Vector3.SignedAngle(plane.ClosestPointOnPlane(m_positions[i]) - m_positions[i - 1], projectedPole - m_positions[i - 1], plane.normal);
				m_positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (m_positions[i] - m_positions[i - 1]) + m_positions[i - 1];
			}
		}
	}

	private Vector3 GetPositionRootSpace(Vector3 current)
	{
		return Quaternion.Inverse(m_root.rotation) * (current - m_root.position);
	}

	private Quaternion GetRotationRootSpace(Quaternion current)
	{
		return Quaternion.Inverse(current) * m_root.rotation;
	}

	private void SetPositionRotation(Quaternion targetRotation)
	{
		for (int i = 0; i < m_positions.Count; i++)
		{
			float weight = ((i == m_positions.Count - 1) ? (leafWeight * chainWeight * globalWeight) : (chainWeight * globalWeight));
			SetRotationRootSpace(m_bones[i], TargetRotation(targetRotation, i), weight);
		}
	}

	private void SetRotationRootSpace(Transform current, Quaternion rotation, float weight)
	{
		Quaternion targetRot = m_root.rotation * rotation;
		current.rotation = Quaternion.Lerp(current.rotation, targetRot, weight);
	}

	private Quaternion TargetRotation(Quaternion targetRotation, int index)
	{
		if (index == m_positions.Count - 1)
		{
			return Quaternion.Inverse(targetRotation) * m_startRotationTarget * Quaternion.Inverse(m_startRotationBone[index]);
		}
		return Quaternion.FromToRotation(m_startDirection[index], m_positions[index + 1] - m_positions[index]) * Quaternion.Inverse(m_startRotationBone[index]);
	}
}
