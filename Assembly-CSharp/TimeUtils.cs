using System;
using System.Text;
using System.Text.RegularExpressions;
using PegasusShared;
using UnityEngine;

public class TimeUtils
{
	public enum ElapsedTimeType
	{
		SECONDS,
		MINUTES,
		HOURS,
		YESTERDAY,
		DAYS,
		WEEKS,
		MONTH_AGO
	}

	public class ElapsedStringSet
	{
		public string m_seconds;

		public string m_minutes;

		public string m_hours;

		public string m_yesterday;

		public string m_days;

		public string m_weeks;

		public string m_monthAgo;
	}

	public static readonly DateTime EPOCH_TIME = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static readonly ElapsedStringSet SPLASHSCREEN_DATETIME_STRINGSET = new ElapsedStringSet
	{
		m_seconds = "GLOBAL_DATETIME_SPLASHSCREEN_SECONDS",
		m_minutes = "GLOBAL_DATETIME_SPLASHSCREEN_MINUTES",
		m_hours = "GLOBAL_DATETIME_SPLASHSCREEN_HOURS",
		m_yesterday = "GLOBAL_DATETIME_SPLASHSCREEN_DAY",
		m_days = "GLOBAL_DATETIME_SPLASHSCREEN_DAYS",
		m_weeks = "GLOBAL_DATETIME_SPLASHSCREEN_WEEKS",
		m_monthAgo = "GLOBAL_DATETIME_SPLASHSCREEN_MONTH"
	};

	public static long UnixTimestampMilliseconds => (long)GetElapsedTimeSinceEpoch(null).TotalMilliseconds;

	public static long BinaryStamp()
	{
		return DateTime.UtcNow.ToBinary();
	}

	public static DateTime ConvertEpochMicrosecToDateTime(long microsec)
	{
		DateTime ePOCH_TIME = EPOCH_TIME;
		return ePOCH_TIME.AddMilliseconds((double)microsec / 1000.0);
	}

	public static TimeSpan GetElapsedTimeSinceEpoch(DateTime? endDateTime = null)
	{
		return (endDateTime.HasValue ? endDateTime.Value : DateTime.UtcNow) - EPOCH_TIME;
	}

	public static string GetElapsedTimeStringFromEpochMicrosec(long microsec, ElapsedStringSet stringSet)
	{
		DateTime timestampDate = ConvertEpochMicrosecToDateTime(microsec);
		return GetElapsedTimeString((int)(DateTime.UtcNow - timestampDate).TotalSeconds, stringSet);
	}

	public static ulong DateTimeToUnixTimeStamp(DateTime time)
	{
		return (ulong)(time.ToUniversalTime() - EPOCH_TIME).TotalSeconds;
	}

	public static ulong DateTimeToUnixTimeStampMilliseconds(DateTime time)
	{
		return (ulong)(time.ToUniversalTime() - EPOCH_TIME).TotalMilliseconds;
	}

	public static DateTime UnixTimeStampToDateTimeUtc(long secondsSinceEpoch)
	{
		DateTime ePOCH_TIME = EPOCH_TIME;
		return ePOCH_TIME.AddSeconds(secondsSinceEpoch);
	}

	public static DateTime UnixTimeStampMillisecondsToDateTimeUtc(long millisecondsSinceEpoch)
	{
		DateTime ePOCH_TIME = EPOCH_TIME;
		return ePOCH_TIME.AddMilliseconds(millisecondsSinceEpoch);
	}

	public static DateTime UnixTimeStampToDateTimeLocal(long secondsSinceEpoch)
	{
		DateTime ePOCH_TIME = EPOCH_TIME;
		return ePOCH_TIME.AddSeconds(secondsSinceEpoch).ToLocalTime();
	}

	public static string GetCountdownTimerString(TimeSpan timeRemaining, bool getFinalSeconds = false)
	{
		if (timeRemaining.Days > 0)
		{
			return GameStrings.Format("GLOBAL_DATETIME_TIMER_DAYS_HOURS", timeRemaining.Days, timeRemaining.Hours);
		}
		if (timeRemaining.Hours > 0)
		{
			return GameStrings.Format("GLOBAL_DATETIME_TIMER_HOURS_MINUTES", timeRemaining.Hours, timeRemaining.Minutes);
		}
		if (timeRemaining.Minutes > 0 || !getFinalSeconds)
		{
			return GameStrings.Format((!getFinalSeconds && timeRemaining.Minutes == 0) ? "GLOBAL_DATETIME_TIMER_LESS_THAN_X_MINUTES" : "GLOBAL_DATETIME_TIMER_MINUTES", Math.Max(timeRemaining.Minutes, 1));
		}
		return GameStrings.Format("GLOBAL_DATETIME_TIMER_SECONDS", timeRemaining.Seconds);
	}

	public static string GetElapsedTimeString(long seconds, ElapsedStringSet stringSet, bool roundUp = false)
	{
		ElapsedTimeType timeType;
		long time;
		if (roundUp)
		{
			GetElapsedTimeRoundedUp(seconds, out timeType, out time);
		}
		else
		{
			GetElapsedTimeRoundedDown(seconds, out timeType, out time);
		}
		return GetElapsedTimeString(timeType, time, stringSet);
	}

	public static string GetElapsedTimeString(int seconds, ElapsedStringSet stringSet, bool roundUp = false)
	{
		return GetElapsedTimeString((long)seconds, stringSet, roundUp);
	}

	public static string GetElapsedTimeString(ElapsedTimeType timeType, int time, ElapsedStringSet stringSet)
	{
		return GetElapsedTimeString(timeType, (long)time, stringSet);
	}

	public static string GetElapsedTimeString(ElapsedTimeType timeType, long time, ElapsedStringSet stringSet)
	{
		switch (timeType)
		{
		case ElapsedTimeType.SECONDS:
			if (stringSet.m_seconds == null)
			{
				time = 1L;
				goto case ElapsedTimeType.MINUTES;
			}
			return GameStrings.Format(stringSet.m_seconds, time);
		case ElapsedTimeType.MINUTES:
			if (stringSet.m_minutes == null)
			{
				time = 1L;
				goto case ElapsedTimeType.HOURS;
			}
			return GameStrings.Format(stringSet.m_minutes, time);
		case ElapsedTimeType.HOURS:
			if (stringSet.m_hours == null)
			{
				time = 1L;
				goto case ElapsedTimeType.YESTERDAY;
			}
			return GameStrings.Format(stringSet.m_hours, time);
		case ElapsedTimeType.YESTERDAY:
			if (stringSet.m_yesterday == null)
			{
				time = 1L;
				goto case ElapsedTimeType.DAYS;
			}
			return GameStrings.Get(stringSet.m_yesterday);
		case ElapsedTimeType.DAYS:
			if (stringSet.m_days == null)
			{
				time = 1L;
				goto case ElapsedTimeType.WEEKS;
			}
			return GameStrings.Format(stringSet.m_days, time);
		case ElapsedTimeType.WEEKS:
			if (stringSet.m_weeks == null)
			{
				time = 1L;
				break;
			}
			return GameStrings.Format(stringSet.m_weeks, time);
		}
		return GameStrings.Format(stringSet.m_monthAgo, time);
	}

	public static void GetElapsedTime(long seconds, out ElapsedTimeType timeType, out int time, bool roundUp = false)
	{
		long longTime;
		if (roundUp)
		{
			GetElapsedTimeRoundedUp(seconds, out timeType, out longTime);
		}
		else
		{
			GetElapsedTimeRoundedDown(seconds, out timeType, out longTime);
		}
		time = (int)longTime;
	}

	private static void GetElapsedTimeRoundedDown(long seconds, out ElapsedTimeType timeType, out long time)
	{
		time = 0L;
		if (seconds < 60)
		{
			timeType = ElapsedTimeType.SECONDS;
			time = seconds;
			return;
		}
		if (seconds < 3600)
		{
			timeType = ElapsedTimeType.MINUTES;
			time = seconds / 60;
			return;
		}
		long days = seconds / 86400;
		switch (days)
		{
		case 0L:
			timeType = ElapsedTimeType.HOURS;
			time = seconds / 3600;
			return;
		case 1L:
			timeType = ElapsedTimeType.YESTERDAY;
			return;
		}
		long weeks = seconds / 604800;
		if (weeks == 0L)
		{
			timeType = ElapsedTimeType.DAYS;
			time = days;
			return;
		}
		long months = seconds / 2592000;
		if (months == 0L)
		{
			timeType = ElapsedTimeType.WEEKS;
			time = weeks;
		}
		else
		{
			timeType = ElapsedTimeType.MONTH_AGO;
			time = months;
		}
	}

	private static void GetElapsedTimeRoundedUp(long seconds, out ElapsedTimeType timeType, out long time)
	{
		time = 0L;
		long minutes = seconds / 60;
		long hours = seconds / 3600;
		long days = seconds / 86400;
		long weeks = seconds / 604800;
		long months = seconds / 2592000;
		if (months > 0)
		{
			timeType = ElapsedTimeType.MONTH_AGO;
			time = months + 1;
		}
		else if ((double)days <= 14.5)
		{
			timeType = ElapsedTimeType.DAYS;
			time = days;
		}
		else if (weeks > 0)
		{
			timeType = ElapsedTimeType.WEEKS;
			time = weeks + 1;
		}
		else if (days > 0)
		{
			timeType = ElapsedTimeType.DAYS;
			time = days + 1;
		}
		else if (hours > 0)
		{
			timeType = ElapsedTimeType.HOURS;
			time = hours + 1;
		}
		else if (minutes > 0)
		{
			timeType = ElapsedTimeType.MINUTES;
			time = minutes + 1;
		}
		else
		{
			timeType = ElapsedTimeType.SECONDS;
			time = seconds;
		}
	}

	public static string GetDevElapsedTimeString(TimeSpan span)
	{
		return GetDevElapsedTimeString((long)span.TotalMilliseconds);
	}

	public static string GetDevElapsedTimeString(long ms)
	{
		StringBuilder builder = new StringBuilder();
		int unitCount = 0;
		if (ms >= 3600000)
		{
			AppendDevTimeUnitsString("{0}h", 3600000, builder, ref ms, ref unitCount);
		}
		if (ms >= 60000)
		{
			AppendDevTimeUnitsString("{0}m", 60000, builder, ref ms, ref unitCount);
		}
		if (ms >= 1000)
		{
			AppendDevTimeUnitsString("{0}s", 1000, builder, ref ms, ref unitCount);
		}
		if (unitCount <= 1)
		{
			if (unitCount > 0)
			{
				builder.Append(' ');
			}
			builder.AppendFormat("{0}ms", ms);
		}
		return builder.ToString();
	}

	public static string GetDevElapsedTimeString(float sec)
	{
		StringBuilder builder = new StringBuilder();
		int unitCount = 0;
		if (sec >= 3600f)
		{
			AppendDevTimeUnitsString("{0}h", 3600f, builder, ref sec, ref unitCount);
		}
		if (sec >= 60f)
		{
			AppendDevTimeUnitsString("{0}m", 60f, builder, ref sec, ref unitCount);
		}
		if (sec >= 1f)
		{
			AppendDevTimeUnitsString("{0}s", 1f, builder, ref sec, ref unitCount);
		}
		if (unitCount <= 1)
		{
			if (unitCount > 0)
			{
				builder.Append(' ');
			}
			float ms = sec * 1000f;
			if (ms > 0f)
			{
				builder.AppendFormat("{0:f0}ms", ms);
			}
			else
			{
				builder.AppendFormat("{0}ms", ms);
			}
		}
		return builder.ToString();
	}

	public static bool TryParseDevSecFromElapsedTimeString(string timeStr, out float sec)
	{
		sec = 0f;
		MatchCollection matches = Regex.Matches(timeStr, "(?<number>(?:[0-9]+,)*[0-9]+)\\s*(?<units>[a-zA-Z]+)");
		if (matches.Count == 0)
		{
			return false;
		}
		Match match = matches[0];
		if (!match.Groups[0].Success)
		{
			return false;
		}
		Group numberGroup = match.Groups["number"];
		Group unitsGroup = match.Groups["units"];
		if (!numberGroup.Success || !unitsGroup.Success)
		{
			return false;
		}
		string value = numberGroup.Value;
		string unitsStr = unitsGroup.Value;
		if (!float.TryParse(value, out sec))
		{
			return false;
		}
		unitsStr = ParseTimeUnitsStr(unitsStr);
		if (unitsStr == "min")
		{
			sec *= 60f;
		}
		else if (unitsStr == "hour")
		{
			sec *= 3600f;
		}
		return true;
	}

	public static long PegDateToFileTimeUtc(Date date)
	{
		return new DateTime(date.Year, date.Month, date.Day, date.Hours, date.Min, date.Sec).ToFileTimeUtc();
	}

	public static Date FileTimeUtcToPegDate(long fileTimeUtc)
	{
		DateTime date = DateTime.FromFileTimeUtc(fileTimeUtc);
		return new Date
		{
			Year = date.Year,
			Month = date.Month,
			Day = date.Day,
			Hours = date.Hour,
			Min = date.Minute,
			Sec = date.Second
		};
	}

	public static string GetComingSoonText(EventTimingType comingSoonEvent)
	{
		TimeSpan? timeUntilAdventureUnlock = EventTimingManager.Get().GetEventEndTimeUtc(comingSoonEvent) - DateTime.UtcNow;
		if (!timeUntilAdventureUnlock.HasValue)
		{
			return GameStrings.Get("GLOBAL_DATETIME_COMING_SOON");
		}
		ElapsedStringSet stringSet = new ElapsedStringSet
		{
			m_minutes = "GLOBAL_DATETIME_COMING_SOON_MINUTES",
			m_hours = "GLOBAL_DATETIME_COMING_SOON_HOURS",
			m_days = "GLOBAL_DATETIME_COMING_SOON_DAYS",
			m_weeks = "GLOBAL_DATETIME_COMING_SOON_WEEKS",
			m_monthAgo = "GLOBAL_DATETIME_COMING_SOON"
		};
		return GetElapsedTimeString((long)timeUntilAdventureUnlock.Value.TotalSeconds, stringSet, roundUp: true);
	}

	private static void AppendDevTimeUnitsString(string formatString, int msPerUnit, StringBuilder builder, ref long ms, ref int unitCount)
	{
		long units = ms / msPerUnit;
		if (units > 0)
		{
			if (unitCount > 0)
			{
				builder.Append(' ');
			}
			builder.AppendFormat(formatString, units);
			unitCount++;
		}
		ms -= units * msPerUnit;
	}

	private static void AppendDevTimeUnitsString(string formatString, float secPerUnit, StringBuilder builder, ref float sec, ref int unitCount)
	{
		float units = Mathf.Floor(sec / secPerUnit);
		if (units > 0f)
		{
			if (unitCount > 0)
			{
				builder.Append(' ');
			}
			builder.AppendFormat(formatString, units);
			unitCount++;
		}
		sec -= units * secPerUnit;
	}

	private static string ParseTimeUnitsStr(string unitsStr)
	{
		if (unitsStr == null)
		{
			return "sec";
		}
		unitsStr = unitsStr.ToLowerInvariant();
		switch (unitsStr)
		{
		case "s":
		case "sec":
		case "secs":
		case "second":
		case "seconds":
			return "sec";
		case "m":
		case "min":
		case "mins":
		case "minute":
		case "minutes":
			return "min";
		case "h":
		case "hour":
		case "hours":
			return "hour";
		default:
			return "sec";
		}
	}
}
