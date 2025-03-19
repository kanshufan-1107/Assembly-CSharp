using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Logging;
using HearthstoneTelemetry;

namespace Hearthstone.Networking.BattleNet;

public static class BattleNetLoggerBuilder
{
	private class LogLevelChecker : IBnetLogLevelChecker
	{
		private readonly Logger m_logger;

		public LogLevelChecker(Logger logger)
		{
			m_logger = logger;
		}

		public bool IsEnabled(Blizzard.GameService.SDK.Client.Integration.LogLevel level)
		{
			Blizzard.T5.Logging.LogLevel loggerLogLevel = ConvertBGSLogToLogLevel(level);
			return m_logger.CanPrint(loggerLogLevel, null);
		}

		private static Blizzard.T5.Logging.LogLevel ConvertBGSLogToLogLevel(Blizzard.GameService.SDK.Client.Integration.LogLevel logLevel)
		{
			switch (logLevel)
			{
			case Blizzard.GameService.SDK.Client.Integration.LogLevel.Error:
			case Blizzard.GameService.SDK.Client.Integration.LogLevel.Exception:
			case Blizzard.GameService.SDK.Client.Integration.LogLevel.Fatal:
				return Blizzard.T5.Logging.LogLevel.Error;
			case Blizzard.GameService.SDK.Client.Integration.LogLevel.Warning:
				return Blizzard.T5.Logging.LogLevel.Warning;
			case Blizzard.GameService.SDK.Client.Integration.LogLevel.Info:
				return Blizzard.T5.Logging.LogLevel.Info;
			case Blizzard.GameService.SDK.Client.Integration.LogLevel.Debug:
				return Blizzard.T5.Logging.LogLevel.Debug;
			case Blizzard.GameService.SDK.Client.Integration.LogLevel.None:
				return Blizzard.T5.Logging.LogLevel.None;
			default:
				return Blizzard.T5.Logging.LogLevel.Error;
			}
		}
	}

	public static LoggerInterface BuildLoggerInterface(ITelemetryClient client, Logger logger, IBnetErrorReporter bnetErrorReporter)
	{
		BattleNetLogEventEmitter battleNetLogEventEmitter = new BattleNetLogEventEmitter(new LogLevelChecker(logger));
		BattleNetLogger bnetLogger = BuildBattleNetLogger(logger);
		battleNetLogEventEmitter.OnBnetLog += bnetLogger.OnBnetLog;
		BattleNetLogFatalErrorHandler fatalErrorHandler = new BattleNetLogFatalErrorHandler(bnetErrorReporter);
		battleNetLogEventEmitter.OnBnetLog += fatalErrorHandler.OnBnetLog;
		BattleNetLogTelemetryReporter logTelemetryReporter = new BattleNetLogTelemetryReporter(client);
		battleNetLogEventEmitter.OnBnetLog += logTelemetryReporter.OnBnetLog;
		return battleNetLogEventEmitter;
	}

	public static BattleNetLogger BuildBattleNetLogger(Logger logger)
	{
		Dictionary<Blizzard.T5.Core.LogLevel, bool> consoleLogsAllowed = GetLoggerInterfaceConsoleLogsAllowed(logger);
		return new BattleNetLogger(logger, consoleLogsAllowed);
	}

	private static Dictionary<Blizzard.T5.Core.LogLevel, bool> GetLoggerInterfaceConsoleLogsAllowed(Logger logger)
	{
		return new Dictionary<Blizzard.T5.Core.LogLevel, bool>
		{
			{
				Blizzard.T5.Core.LogLevel.Warning,
				ShouldLoggerLogToConsoleForLevel(Blizzard.T5.Logging.LogLevel.Warning, logger)
			},
			{
				Blizzard.T5.Core.LogLevel.Error,
				ShouldLoggerLogToConsoleForLevel(Blizzard.T5.Logging.LogLevel.Error, logger)
			}
		};
	}

	private static bool ShouldLoggerLogToConsoleForLevel(Blizzard.T5.Logging.LogLevel logLevel, Logger logger)
	{
		return !logger.CanPrint(LogTarget.CONSOLE, logLevel, verbose: false);
	}
}
