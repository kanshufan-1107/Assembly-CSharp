using UnityEngine;

public class MatchingQueueTab : MonoBehaviour
{
	public GameObject m_root;

	public UberText m_waitTime;

	public UberText m_queueTime;

	private TimeUtils.ElapsedStringSet m_timeStringSet;

	private float m_timeInQueue;

	private const string TIME_RANGE_STRING = "GLOBAL_APPROXIMATE_DATETIME_RANGE";

	private const string TIME_STRING = "GLOBAL_APPROXIMATE_DATETIME";

	private const int SUPPRESS_TIME = 30;

	private void Update()
	{
		InitTimeStringSet();
		m_timeInQueue += Time.deltaTime;
		m_waitTime.Text = TimeUtils.GetElapsedTimeString(Mathf.RoundToInt(m_timeInQueue), m_timeStringSet);
	}

	public void Show()
	{
		m_root.SetActive(value: true);
	}

	public void Hide()
	{
		m_root.SetActive(value: false);
	}

	public void ResetTimer()
	{
		m_timeInQueue = 0f;
		UpdateDisplay(0, 0);
	}

	public void UpdateDisplay(int minSeconds, int maxSeconds)
	{
		InitTimeStringSet();
		int timeInQueue = Mathf.RoundToInt(m_timeInQueue);
		maxSeconds += timeInQueue;
		if (maxSeconds <= 30)
		{
			Hide();
			return;
		}
		m_queueTime.Text = GetElapsedTimeString(minSeconds + timeInQueue, maxSeconds);
		Show();
	}

	private void InitTimeStringSet()
	{
		if (m_timeStringSet == null)
		{
			m_timeStringSet = new TimeUtils.ElapsedStringSet
			{
				m_seconds = "GLOBAL_DATETIME_SPINNER_SECONDS",
				m_minutes = "GLOBAL_DATETIME_SPINNER_MINUTES",
				m_hours = "GLOBAL_DATETIME_SPINNER_HOURS",
				m_yesterday = "GLOBAL_DATETIME_SPINNER_DAY",
				m_days = "GLOBAL_DATETIME_SPINNER_DAYS",
				m_weeks = "GLOBAL_DATETIME_SPINNER_WEEKS",
				m_monthAgo = "GLOBAL_DATETIME_SPINNER_MONTH"
			};
		}
	}

	private string GetElapsedTimeString(int minSeconds, int maxSeconds)
	{
		TimeUtils.GetElapsedTime(minSeconds, out var minTimeType, out var minTime);
		if (minSeconds == maxSeconds)
		{
			return GameStrings.Format("GLOBAL_APPROXIMATE_DATETIME", TimeUtils.GetElapsedTimeString(minSeconds, m_timeStringSet));
		}
		TimeUtils.GetElapsedTime(maxSeconds, out var maxTimeType, out var maxTime);
		if (minTimeType == maxTimeType)
		{
			return minTimeType switch
			{
				TimeUtils.ElapsedTimeType.SECONDS => GameStrings.Format("GLOBAL_APPROXIMATE_DATETIME_RANGE", minTime, GameStrings.Format(m_timeStringSet.m_seconds, maxTime)), 
				TimeUtils.ElapsedTimeType.MINUTES => GameStrings.Format("GLOBAL_APPROXIMATE_DATETIME_RANGE", minTime, GameStrings.Format(m_timeStringSet.m_minutes, maxTime)), 
				TimeUtils.ElapsedTimeType.HOURS => GameStrings.Format("GLOBAL_APPROXIMATE_DATETIME_RANGE", minTime, GameStrings.Format(m_timeStringSet.m_hours, maxTime)), 
				TimeUtils.ElapsedTimeType.YESTERDAY => GameStrings.Get(m_timeStringSet.m_yesterday), 
				TimeUtils.ElapsedTimeType.DAYS => GameStrings.Format("GLOBAL_APPROXIMATE_DATETIME_RANGE", minTime, GameStrings.Format(m_timeStringSet.m_days, maxTime)), 
				TimeUtils.ElapsedTimeType.WEEKS => GameStrings.Format(m_timeStringSet.m_weeks, minTime, maxTime), 
				_ => GameStrings.Get(m_timeStringSet.m_monthAgo), 
			};
		}
		string elapsedTimeStringMin = TimeUtils.GetElapsedTimeString(minTimeType, minTime, m_timeStringSet);
		string elapsedTimeStringMax = TimeUtils.GetElapsedTimeString(maxTimeType, maxTime, m_timeStringSet);
		return GameStrings.Format("GLOBAL_APPROXIMATE_DATETIME_RANGE", elapsedTimeStringMin, elapsedTimeStringMax);
	}
}
