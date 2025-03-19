using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.Tendril;

public class TendrilSpring : TendrilBase
{
	private List<TendrilPoint> m_points = new List<TendrilPoint>();

	protected override void InitializeTendril()
	{
		for (int i = 0; i < bones.Count; i++)
		{
			Vector3 target = TendrilUtilities.Target(bones, i);
			m_points.Add(new TendrilPoint
			{
				currentPosition = target,
				previousPosition = target
			});
		}
		base.InitializeTendril();
	}

	protected override void ResolveTendril()
	{
		for (int i = 0; i < m_points.Count; i++)
		{
			m_points[i].previousPosition = m_points[i].currentPosition;
			float lerp = speed * Time.deltaTime;
			m_points[i].currentPosition = Vector3.Lerp(m_points[i].previousPosition, TendrilUtilities.Target(bones, i), lerp);
			Vector3 targetDirection = bones[i].InverseTransformDirection((m_points[i].currentPosition - bones[i].position).normalized);
			Quaternion targetRotation = Quaternion.FromToRotation(bones[i].InverseTransformDirection(TendrilUtilities.Direction(bones, i)), targetDirection);
			bones[i].localRotation = Quaternion.Lerp(bones[i].localRotation, bones[i].localRotation * targetRotation, weight * controlWeight);
		}
		base.ResolveTendril();
	}

	protected override void TendrilGizmos()
	{
		for (int i = 0; i < m_points.Count; i++)
		{
			Gizmos.DrawLine(bones[i].position, m_points[i].currentPosition);
			Gizmos.DrawWireSphere(m_points[i].currentPosition, 0.25f);
		}
		base.TendrilGizmos();
	}
}
