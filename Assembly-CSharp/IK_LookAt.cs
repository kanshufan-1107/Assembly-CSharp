using System;
using UnityEngine;

[Serializable]
public class IK_LookAt : IK_Chain
{
	public Vector2 decay;

	protected override void InitializeIK()
	{
		base.InitializeIK();
		for (int i = 0; i < m_bones.Count; i++)
		{
			m_startRotationBone.Add(m_bones[i].localRotation);
		}
		m_initialized = true;
	}

	protected override void ResolveIK()
	{
		base.ResolveIK();
		for (int i = m_bones.Count - 1; i >= 0; i--)
		{
			Quaternion lookRotation = Quaternion.LookRotation(chainTarget.position - m_bones[i].position);
			if (i > 0)
			{
				float t = Mathf.Clamp(Mathf.Lerp(decay.x, decay.y, Mathf.InverseLerp(chainLength, 0f, i)) * chainWeight * globalWeight, 0f, 1f);
				m_bones[i].rotation = Quaternion.Lerp(m_bones[i].rotation, lookRotation * m_startRotationBone[i], t);
			}
			else
			{
				float t = Mathf.Clamp(leafWeight * chainWeight * globalWeight, 0f, 1f);
				m_bones[i].rotation = Quaternion.Lerp(m_bones[i].rotation, lookRotation * m_startRotationBone[i], t);
			}
		}
	}
}
