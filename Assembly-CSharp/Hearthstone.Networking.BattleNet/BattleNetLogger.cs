using System.Collections.Generic;
using System.Text;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using UnityEngine;

namespace Hearthstone.Networking.BattleNet;

public class BattleNetLogger
{
	private readonly Blizzard.T5.Core.ILogger m_logger;

	private readonly Dictionary<Blizzard.T5.Core.LogLevel, bool> m_consoleLogLevelsAllowed;

	public BattleNetLogger(Blizzard.T5.Core.ILogger logger, Dictionary<Blizzard.T5.Core.LogLevel, bool> consoleLogLevelsAllowed)
	{
		m_logger = logger;
		m_consoleLogLevelsAllowed = consoleLogLevelsAllowed;
	}

	public void OnBnetLog(object sender, BnetLogEventArgs args)
	{
		LogFromSource(args.Level, args.Message, args.Source);
	}

	public bool IsConsoleLoggingAllowed(Blizzard.T5.Core.LogLevel level)
	{
		if (m_consoleLogLevelsAllowed != null && m_consoleLogLevelsAllowed.TryGetValue(level, out var isAllowed))
		{
			return isAllowed;
		}
		return false;
	}

	private void LogToUnityConsoleIfAllowed(Blizzard.GameService.SDK.Client.Integration.LogLevel logLevel, string str)
	{
		Blizzard.T5.Core.LogLevel level = ConvertBGSLogToILoggerLevel(logLevel);
		if (IsConsoleLoggingAllowed(level))
		{
			string formatMessage = "[BattleNet] " + str;
			if (level == Blizzard.T5.Core.LogLevel.Warning)
			{
				Debug.LogWarning(formatMessage);
			}
			else
			{
				Debug.LogError(formatMessage);
			}
		}
	}

	private static Blizzard.T5.Core.LogLevel ConvertBGSLogToILoggerLevel(Blizzard.GameService.SDK.Client.Integration.LogLevel logLevel)
	{
		switch (logLevel)
		{
		case Blizzard.GameService.SDK.Client.Integration.LogLevel.Error:
		case Blizzard.GameService.SDK.Client.Integration.LogLevel.Exception:
		case Blizzard.GameService.SDK.Client.Integration.LogLevel.Fatal:
			return Blizzard.T5.Core.LogLevel.Error;
		case Blizzard.GameService.SDK.Client.Integration.LogLevel.Warning:
			return Blizzard.T5.Core.LogLevel.Warning;
		case Blizzard.GameService.SDK.Client.Integration.LogLevel.Info:
			return Blizzard.T5.Core.LogLevel.Information;
		case Blizzard.GameService.SDK.Client.Integration.LogLevel.None:
		case Blizzard.GameService.SDK.Client.Integration.LogLevel.Debug:
			return Blizzard.T5.Core.LogLevel.Debug;
		default:
			return Blizzard.T5.Core.LogLevel.Error;
		}
	}

	private void LogFromSource(Blizzard.GameService.SDK.Client.Integration.LogLevel logLevel, string message, string sourceName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		stringBuilder.Append(sourceName);
		stringBuilder.Append("] ");
		stringBuilder.Append(message);
		Log(logLevel, stringBuilder.ToString());
	}

	private void Log(Blizzard.GameService.SDK.Client.Integration.LogLevel logLevel, string str)
	{
		Blizzard.T5.Core.LogLevel level = ConvertBGSLogToILoggerLevel(logLevel);
		m_logger.Log(level, str);
		LogToUnityConsoleIfAllowed(logLevel, str);
	}
}
