using UnityEngine;

public class CameraShakeMgr : MonoBehaviour
{
	private Vector3 m_amount;

	private AnimationCurve m_intensityCurve;

	private float? m_holdAtSec;

	private bool m_isOverride;

	private bool m_started;

	private Vector3 m_initialPos;

	private float m_progressSec;

	private float m_durationSec;

	public bool IsCameraShaking => m_started;

	private void Update()
	{
		if (m_started)
		{
			if (m_progressSec >= m_durationSec && !IsHolding())
			{
				StopShake();
			}
			else
			{
				UpdateShake();
			}
		}
	}

	public static void Shake(Camera camera, Vector3 amount, AnimationCurve intensityCurve, float? holdAtTime = null, bool isOverride = false)
	{
		if (!camera || !Options.Get().GetBool(Option.SCREEN_SHAKE_ENABLED))
		{
			return;
		}
		if (camera.TryGetComponent<CameraShakeMgr>(out var mgr))
		{
			if (DoesCurveHaveZeroTime(intensityCurve))
			{
				mgr.StopShake();
				return;
			}
		}
		else
		{
			if (DoesCurveHaveZeroTime(intensityCurve))
			{
				return;
			}
			mgr = camera.gameObject.AddComponent<CameraShakeMgr>();
		}
		mgr.StartShake(amount, intensityCurve, holdAtTime, isOverride);
	}

	public static void Shake(Camera camera, Vector3 amount, float time)
	{
		AnimationCurve intensityCurve = AnimationCurve.Linear(0f, 1f, time, 0f);
		Shake(camera, amount, intensityCurve, null);
	}

	public static void Stop(Camera camera, float time = 0f)
	{
		if ((bool)camera && Options.Get().GetBool(Option.SCREEN_SHAKE_ENABLED) && camera.TryGetComponent<CameraShakeMgr>(out var mgr))
		{
			if (time <= 0f)
			{
				mgr.StopShake();
				return;
			}
			float currIntensity = mgr.ComputeIntensity();
			AnimationCurve intensityCurve = AnimationCurve.Linear(0f, currIntensity, time, 0f);
			mgr.StartShake(mgr.m_amount, intensityCurve, null);
		}
	}

	public static bool IsShaking(Camera camera)
	{
		if (!camera)
		{
			return false;
		}
		if (!camera.TryGetComponent<CameraShakeMgr>(out var mgr))
		{
			return false;
		}
		return mgr.IsCameraShaking;
	}

	private static bool DoesCurveHaveZeroTime(AnimationCurve intensityCurve)
	{
		if (intensityCurve == null)
		{
			return true;
		}
		if (intensityCurve.length == 0)
		{
			return true;
		}
		if (intensityCurve[intensityCurve.length - 1].time <= 0f)
		{
			return true;
		}
		return false;
	}

	private void StartShake(Vector3 amount, AnimationCurve intensityCurve, float? holdAtSec = null, bool isOverride = false)
	{
		if ((isOverride || !(amount.sqrMagnitude < m_amount.sqrMagnitude)) && !m_isOverride)
		{
			m_amount = amount;
			m_intensityCurve = intensityCurve;
			m_holdAtSec = holdAtSec;
			m_isOverride = isOverride;
			if (!m_started)
			{
				m_started = true;
				m_initialPos = base.transform.position;
			}
			m_progressSec = 0f;
			m_durationSec = intensityCurve[intensityCurve.length - 1].time;
		}
	}

	private void StopShake()
	{
		if (IsCameraShaking)
		{
			base.transform.position = m_initialPos;
		}
		m_amount = Vector3.zero;
		m_intensityCurve = null;
		m_holdAtSec = null;
		m_isOverride = false;
		m_started = false;
	}

	private void UpdateShake()
	{
		float intensity = ComputeIntensity();
		Vector3 shake = default(Vector3);
		shake.x = Random.Range((0f - m_amount.x) * intensity, m_amount.x * intensity);
		shake.y = Random.Range((0f - m_amount.y) * intensity, m_amount.y * intensity);
		shake.z = Random.Range((0f - m_amount.z) * intensity, m_amount.z * intensity);
		base.transform.position = m_initialPos + shake;
		if (!IsHolding())
		{
			m_progressSec = Mathf.Min(m_progressSec + Time.deltaTime, m_durationSec);
		}
	}

	private float ComputeIntensity()
	{
		if (m_intensityCurve != null)
		{
			return m_intensityCurve.Evaluate(m_progressSec);
		}
		return 0f;
	}

	private bool IsHolding()
	{
		if (!m_holdAtSec.HasValue)
		{
			return false;
		}
		return m_progressSec >= m_holdAtSec;
	}
}
