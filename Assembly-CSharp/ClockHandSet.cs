using System;
using UnityEngine;

public class ClockHandSet : MonoBehaviour
{
	public GameObject m_MinuteHand;

	public GameObject m_HourHand;

	private int m_prevMinute;

	private int m_prevHour;

	private void Update()
	{
		DateTime currentTime = DateTime.Now;
		int minute = currentTime.Minute;
		if (minute != m_prevMinute)
		{
			float num = ComputeMinuteRotation(minute);
			float prevMinuteRotation = ComputeMinuteRotation(m_prevMinute);
			float minuteDelta = num - prevMinuteRotation;
			m_MinuteHand.transform.Rotate(Vector3.up, minuteDelta);
			m_prevMinute = minute;
		}
		int hour = currentTime.Hour % 12;
		if (hour != m_prevHour)
		{
			float num2 = ComputeHourRotation(hour);
			float prevHourRotation = ComputeHourRotation(m_prevHour);
			float hourDelta = num2 - prevHourRotation;
			m_HourHand.transform.Rotate(Vector3.up, hourDelta);
			m_prevHour = hour;
		}
	}

	private float ComputeMinuteRotation(int minute)
	{
		return (float)minute * 6f;
	}

	private float ComputeHourRotation(int hour)
	{
		return (float)hour * 30f;
	}
}
