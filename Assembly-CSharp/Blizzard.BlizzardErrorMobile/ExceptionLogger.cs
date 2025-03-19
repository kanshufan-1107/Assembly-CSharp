using System;
using UnityEngine;

namespace Blizzard.BlizzardErrorMobile;

internal static class ExceptionLogger
{
	private static IExceptionLogger s_logger = null;

	private static string s_indicatorLogger = "ExceptionLogger: ";

	public static void SetLogger(IExceptionLogger logger)
	{
		s_logger = logger;
	}

	public static bool IsExceptionLoggerError(string message)
	{
		return message.Contains(s_indicatorLogger);
	}

	public static void LogDebug(string format, params object[] args)
	{
		if (s_logger != null)
		{
			string msg = ((args.Length != 0) ? string.Format(format, args) : format);
			try
			{
				s_logger.LogDebug(msg);
			}
			catch (Exception ex)
			{
				Debug.LogFormat("Failed to record LogDebug: {0}, Debug {1}", ex.Message, msg);
			}
		}
	}

	public static void LogInfo(string format, params object[] args)
	{
		if (s_logger != null)
		{
			string msg = ((args.Length != 0) ? string.Format(format, args) : format);
			try
			{
				s_logger.LogInfo(msg);
			}
			catch (Exception ex)
			{
				Debug.LogFormat("Failed to record LogInfo: {0}\n Info: {1}", ex.Message, msg);
			}
		}
	}

	public static void LogWarning(string format, params object[] args)
	{
		if (s_logger != null)
		{
			string msg = ((args.Length != 0) ? string.Format(format, args) : format);
			try
			{
				s_logger.LogWarning(msg);
			}
			catch (Exception ex)
			{
				Debug.LogFormat("Failed to record LogWarning: {0}\n Warning: {1}", ex.Message, msg);
			}
		}
	}

	public static void LogError(string format, params object[] args)
	{
		if (s_logger != null)
		{
			if (ExceptionReporter.Get().SendErrors)
			{
				format = s_indicatorLogger + format;
			}
			string msg = ((args.Length != 0) ? string.Format(format, args) : format);
			try
			{
				s_logger.LogError(msg);
			}
			catch (Exception ex)
			{
				Debug.LogFormat("Failed to record LogError: {0}\n Error: {1}", ex.Message, msg);
			}
		}
	}
}
