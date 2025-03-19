using System;
using System.IO;
using UnityEngine;

namespace Blizzard.BlizzardErrorMobile;

public class CallbackManager : MonoBehaviour
{
	public static void RegisterExceptionHandler()
	{
	}

	public static long CatchCrashCaptureFromLog(long lastReadTime, string packageName)
	{
		string playerPrevLogPath = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\LocalLow\\Blizzard Entertainment\\Hearthstone\\Player-prev.log");
		try
		{
			if (!File.Exists(playerPrevLogPath))
			{
				return 0L;
			}
			long lastModified = File.GetLastWriteTimeUtc(playerPrevLogPath).Ticks;
			string fullCrashReport = string.Empty;
			string stackTrace = string.Empty;
			using (StreamReader reader = new StreamReader(playerPrevLogPath))
			{
				if (reader.BaseStream.Length > 40000)
				{
					reader.BaseStream.Seek(-40000L, SeekOrigin.End);
				}
				_ = new char[40000];
				string content = reader.ReadToEnd();
				bool foundCrash = false;
				bool foundHearthstoneIndicator = false;
				using (StringReader stringReader = new StringReader(content.Substring(500)))
				{
					string line;
					while ((line = stringReader.ReadLine()) != null)
					{
						if (line.Contains("END OF STACKTRACE"))
						{
							foundCrash = true;
						}
						if (line.Contains("Hearthstone/Crashes"))
						{
							foundHearthstoneIndicator = true;
						}
					}
				}
				if (!foundHearthstoneIndicator)
				{
					return 0L;
				}
				if (foundCrash)
				{
					bool foundCrashInfo = false;
					using StringReader stringReader2 = new StringReader(content);
					string line;
					while ((line = stringReader2.ReadLine()) != null)
					{
						if (foundCrashInfo)
						{
							if (line.Contains("END OF STACKTRACE"))
							{
								break;
							}
							fullCrashReport = fullCrashReport + line + Environment.NewLine;
							if (line.StartsWith("0x"))
							{
								stackTrace = stackTrace + line + Environment.NewLine;
							}
						}
						else if (line.Contains("OUTPUTTING STACK TRACE"))
						{
							foundCrashInfo = true;
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(fullCrashReport) && !string.IsNullOrEmpty(stackTrace))
			{
				ExceptionLogger.LogInfo("Reporting a unity crash report from Player-prev.log.");
				ExceptionReporter.Get().RecordException("Unity Crash Report", stackTrace, recordOnly: true, ExceptionSettings.ReportType.CRASH, happenedBefore: true, null, fullCrashReport);
			}
			return lastModified;
		}
		catch (Exception ex)
		{
			ExceptionLogger.LogWarning("Failed to grab the crash information: " + ex.Message);
			return 0L;
		}
	}
}
