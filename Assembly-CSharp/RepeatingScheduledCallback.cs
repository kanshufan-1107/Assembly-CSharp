using System;
using Hearthstone.Core;
using UnityEngine;

public class RepeatingScheduledCallback
{
	private Func<bool> m_callback;

	private float m_initialDelaySecs;

	private float m_baseIntervalSecs;

	private float m_currIntervalSecs;

	private float m_backoffFactor;

	private float m_jitterSecs;

	public bool IsRunning { get; private set; }

	public int CallbackCount { get; private set; }

	public DateTime NextCallbackTime { get; private set; }

	public void Start(Func<bool> callback, float initialDelaySecs, float intervalSecs, float backoffFactor = 1f, float jitterSecs = 0f)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		Stop();
		m_callback = callback;
		m_initialDelaySecs = initialDelaySecs;
		m_baseIntervalSecs = intervalSecs;
		m_backoffFactor = backoffFactor;
		m_jitterSecs = jitterSecs;
		ScheduleNextCallback();
	}

	public void Stop()
	{
		Processor.CancelScheduledCallback(InternalScheduledCallback);
		IsRunning = false;
		CallbackCount = 0;
		NextCallbackTime = DateTime.MinValue;
	}

	private void ScheduleNextCallback()
	{
		if (CallbackCount == 0)
		{
			m_currIntervalSecs = m_initialDelaySecs;
		}
		else if (CallbackCount == 1)
		{
			m_currIntervalSecs = m_baseIntervalSecs;
		}
		else
		{
			m_baseIntervalSecs *= m_backoffFactor;
			m_currIntervalSecs = m_baseIntervalSecs;
		}
		m_currIntervalSecs += UnityEngine.Random.Range(0f, m_jitterSecs);
		NextCallbackTime = DateTime.Now.AddSeconds(m_currIntervalSecs);
		IsRunning = true;
		Processor.ScheduleCallback(m_currIntervalSecs, realTime: true, InternalScheduledCallback);
	}

	private void InternalScheduledCallback(object userData)
	{
		if (m_callback())
		{
			CallbackCount++;
			ScheduleNextCallback();
		}
	}
}
