using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.Tendril;

public class TendrilInverseKinematic : TendrilBase
{
	public float snapBack = 0.5f;

	public float delta = 0.1f;

	public int iterations = 10;

	public bool relativeRotation = true;

	private List<Vector3> m_positions = new List<Vector3>();

	private List<Vector3> m_directions = new List<Vector3>();

	private List<Quaternion> m_rotations = new List<Quaternion>();

	private List<float> m_bonesLength = new List<float>();

	private Quaternion m_startRotationTarget;

	private float m_completeLength;

	protected override bool InitializeConditions()
	{
		return target != null;
	}

	protected override void InitializeTendril()
	{
		m_startRotationTarget = GetRotationRootSpace(target.rotation);
		for (int i = 0; i < bones.Count; i++)
		{
			m_positions.Add(Vector3.zero);
			m_rotations.Add(GetRotationRootSpace(bones[i].rotation));
			if (i == bones.Count - 1)
			{
				m_directions.Add(GetPositionRootSpace(target.position) - GetPositionRootSpace(bones[i].position));
				continue;
			}
			m_directions.Add(GetPositionRootSpace(bones[i + 1].position) - GetPositionRootSpace(bones[i].position));
			m_bonesLength.Add(m_directions[i].magnitude);
			m_completeLength += 1f;
		}
		base.InitializeTendril();
	}

	protected override void ResolveTendril()
	{
		for (int i = 0; i < bones.Count; i++)
		{
			m_positions[i] = GetPositionRootSpace(bones[i].position);
			m_rotations[i] = GetRotationRootSpace(bones[i].rotation);
			if (i == bones.Count - 1)
			{
				m_directions[i] = GetPositionRootSpace(target.position) - GetPositionRootSpace(bones[i].position);
				continue;
			}
			m_directions[i] = GetPositionRootSpace(bones[i + 1].position) - GetPositionRootSpace(bones[i].position);
			m_bonesLength[i] = m_directions[i].magnitude;
		}
		Vector3 targetPosition = GetPositionRootSpace(target.position);
		Quaternion targetRotation = GetRotationRootSpace(target.rotation);
		for (int j = 0; j < m_positions.Count - 1; j++)
		{
			m_positions[j + 1] = Vector3.Lerp(m_positions[j + 1], m_positions[j] + m_directions[j], snapBack);
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
		ResolvePole();
		SetPositionRotation(targetRotation);
	}

	private bool OutOfReach(Vector3 targetPosition)
	{
		return (targetPosition - GetPositionRootSpace(bones[0].position)).sqrMagnitude >= m_completeLength * m_completeLength;
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
		if (pole != null)
		{
			Vector3 polePosition = GetPositionRootSpace(pole.position);
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
		return Quaternion.Inverse(m_rootBone.rotation) * (current - m_rootBone.position);
	}

	private Quaternion GetRotationRootSpace(Quaternion current)
	{
		return Quaternion.Inverse(current) * m_rootBone.rotation;
	}

	private void SetPositionRotation(Quaternion targetRotation)
	{
		for (int i = 0; i < m_positions.Count; i++)
		{
			float combinedWeight = ((i == m_positions.Count - 1) ? (leafWeight * weight * controlWeight) : (weight * controlWeight));
			SetRotationRootSpace(bones[i], TargetRotation(targetRotation, i), combinedWeight);
		}
	}

	private void SetPositionRootSpace(Transform current, Vector3 position, float weight)
	{
		Vector3 targetPos = m_rootBone.rotation * position + m_rootBone.position;
		current.position = Vector3.Lerp(current.position, targetPos, weight);
	}

	private void SetRotationRootSpace(Transform current, Quaternion rotation, float weight)
	{
		Quaternion targetRot = m_rootBone.rotation * rotation;
		current.rotation = Quaternion.Lerp(current.rotation, targetRot, weight);
	}

	private Quaternion TargetRotation(Quaternion targetRotation, int index)
	{
		if (index == m_positions.Count - 1)
		{
			if (relativeRotation)
			{
				return Quaternion.Inverse(targetRotation) * m_startRotationTarget * Quaternion.Inverse(m_rotations[index]);
			}
			return Quaternion.Inverse(targetRotation) * Quaternion.Inverse(m_rotations[index]);
		}
		return Quaternion.FromToRotation(m_directions[index], m_positions[index + 1] - m_positions[index]) * Quaternion.Inverse(m_rotations[index]);
	}

	protected override void TendrilGizmos()
	{
		for (int i = 0; i < m_positions.Count; i++)
		{
			Vector3 scale = base.transform.lossyScale;
			scale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
			Vector3 source = m_rootBone.TransformPoint(Vector3.Scale(m_positions[i], scale));
			Gizmos.DrawWireSphere(source, 0.25f);
			if (i != m_positions.Count - 1)
			{
				Vector3 target = m_rootBone.TransformPoint(Vector3.Scale(m_positions[i + 1], scale));
				Gizmos.DrawLine(source, target);
			}
		}
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.target.position, 0.25f);
		if (pole != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(pole.position, 0.25f);
		}
		base.TendrilGizmos();
	}
}
