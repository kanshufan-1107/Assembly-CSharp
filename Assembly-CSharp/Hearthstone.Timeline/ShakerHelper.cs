using System;
using UnityEngine;

namespace Hearthstone.Timeline;

public class ShakerHelper : TimelineEffectHelper
{
	private AnimationCurve m_falloff;

	private Vector3 m_amount;

	private float m_interval;

	private Vector3 m_originalPosition;

	private Vector3[] m_positions = new Vector3[0];

	protected override void Initialize(params object[] values)
	{
		if (values.Length >= 3)
		{
			Vector3 amountVector = (Vector3)values[0];
			m_amount = new Vector3(Mathf.Abs(amountVector.x), Mathf.Abs(amountVector.y), Mathf.Abs(amountVector.z));
			m_falloff = (AnimationCurve)values[1];
			m_interval = Convert.ToSingle(values[2]);
			if (!base.ReceivedOriginalValues)
			{
				m_originalPosition = base.transform.position;
			}
			m_positions = new Vector3[30];
			float[] angles = new float[30];
			for (int i = 0; i < m_positions.Length; i++)
			{
				angles[i] = ((i == 0) ? UnityEngine.Random.Range(0f, 6.283f) : UnityEngine.Random.Range(angles[i - 1] + 1.745f, angles[i - 1] + 4.538f));
				float x = Mathf.Cos(angles[i]) * m_amount.x;
				float y = Mathf.Sin(angles[i]) * m_amount.y;
				m_positions[i] = new Vector3
				{
					x = x,
					y = y,
					z = ((i == 0 || m_positions[i - 1].z < 0f) ? UnityEngine.Random.Range(0f, m_amount.z) : UnityEngine.Random.Range(0f - m_amount.z, 0f))
				};
			}
		}
	}

	protected override void CopyOriginalValuesFrom<T>(T other)
	{
		ShakerHelper shakerHelper = other as ShakerHelper;
		m_originalPosition = shakerHelper.m_originalPosition;
	}

	protected override void OnKill(TimelineEffectKillCause _)
	{
	}

	protected override void ResetTarget(TimelineEffectResetCause _)
	{
		base.transform.position = m_originalPosition;
	}

	protected override void UpdateTarget(float normalizedTime)
	{
		if (Options.Get().GetBool(Option.SCREEN_SHAKE_ENABLED))
		{
			float m_durationDividedByInterval = ((m_interval > 0f) ? (base.Duration / m_interval) : 500f);
			float num = normalizedTime * m_durationDividedByInterval;
			int flooredPosition = Mathf.FloorToInt(num);
			float t = Mathf.Cos((num - (float)flooredPosition) * (float)Math.PI);
			t *= 0.5f;
			t += 0.5f;
			t = 1f - t;
			t = Mathf.Cos(t * (float)Math.PI);
			t *= 0.5f;
			t += 0.5f;
			t = 1f - t;
			Vector3 shake = Vector3.Lerp(m_positions[flooredPosition % m_positions.Length], m_positions[(flooredPosition + 1) % m_positions.Length], t);
			shake *= m_falloff.Evaluate(normalizedTime);
			base.transform.position = m_originalPosition + base.transform.right * shake.x + base.transform.up * shake.y + base.transform.forward * shake.z;
		}
	}
}
