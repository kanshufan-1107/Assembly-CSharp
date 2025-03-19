using UnityEngine;

public class CameraShaker : MonoBehaviour
{
	public Vector3 m_Amount;

	public AnimationCurve m_IntensityCurve;

	public bool m_Hold;

	public float m_HoldAtSec;

	public bool m_IsOverride;

	public void StartShake()
	{
		float? holdAtTime = null;
		if (m_Hold)
		{
			holdAtTime = m_HoldAtSec;
		}
		CameraShakeMgr.Shake(Camera.main, m_Amount, m_IntensityCurve, holdAtTime, m_IsOverride);
	}
}
