using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Serializable]
[Obsolete("Use CameraShaker2 instead.", false)]
public class CameraShakeBehaviour : PlayableBehaviour
{
	public Vector3 m_Amount;

	public AnimationCurve m_IntensityCurve;

	public Vector3 m_cameraOrigin;

	private bool m_needsReset;

	private Camera m_camera;

	public override void OnGraphStart(Playable playable)
	{
		m_needsReset = true;
		if (Application.isPlaying)
		{
			m_camera = Camera.main;
		}
	}

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (!Application.isPlaying)
		{
			m_camera = playerData as Camera;
		}
		if (m_camera != null && m_IntensityCurve != null)
		{
			if (m_needsReset)
			{
				m_needsReset = false;
				m_cameraOrigin = m_camera.transform.position;
			}
			float time = (float)playable.GetTime() / (float)playable.GetDuration();
			float intensity = m_IntensityCurve.Evaluate(time);
			Vector3 shake = default(Vector3);
			shake.x = UnityEngine.Random.Range((0f - m_Amount.x) * intensity, m_Amount.x * intensity);
			shake.y = UnityEngine.Random.Range((0f - m_Amount.y) * intensity, m_Amount.y * intensity);
			shake.z = UnityEngine.Random.Range((0f - m_Amount.z) * intensity, m_Amount.z * intensity);
			m_camera.transform.position = m_cameraOrigin + shake;
		}
	}
}
