using System;
using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using UnityEngine;

namespace Hearthstone.Networking.BattleNet;

public class BattleNetLogEventEmitter : LoggerBase
{
	private readonly IBnetLogLevelChecker m_logLevelChecker;

	private readonly SafeEventHandlerInvoker<BnetLogEventArgs> m_safeEventHandlerInvoker = new SafeEventHandlerInvoker<BnetLogEventArgs>();

	public event EventHandler<BnetLogEventArgs> OnBnetLog
	{
		add
		{
			m_safeEventHandlerInvoker.AddHandler(value);
		}
		remove
		{
			m_safeEventHandlerInvoker.RemoveHandler(value);
		}
	}

	public BattleNetLogEventEmitter(IBnetLogLevelChecker logLevelChecker)
	{
		m_logLevelChecker = logLevelChecker;
	}

	public override bool IsEnabled(Blizzard.GameService.SDK.Client.Integration.LogLevel logLevel)
	{
		return m_logLevelChecker?.IsEnabled(logLevel) ?? false;
	}

	public override void Log(Blizzard.GameService.SDK.Client.Integration.LogLevel logLevel, string str, string sourceName = "")
	{
		BnetLogEventArgs eventArgs = new BnetLogEventArgs(logLevel, str, sourceName);
		BroadcastLogEvent(eventArgs);
	}

	private void BroadcastLogEvent(BnetLogEventArgs eventArgs)
	{
		HandleAnyExceptions(m_safeEventHandlerInvoker.Invoke(this, eventArgs));
	}

	private static void HandleAnyExceptions(IList<Exception> exceptions)
	{
		if (exceptions == null)
		{
			return;
		}
		foreach (Exception exception in exceptions)
		{
			Debug.LogException(exception);
		}
	}

	public override void LogDebug(string message, string sourceName = "")
	{
		Log(Blizzard.GameService.SDK.Client.Integration.LogLevel.Debug, message, sourceName);
	}

	public override void LogInfo(string message, string sourceName = "")
	{
		Log(Blizzard.GameService.SDK.Client.Integration.LogLevel.Info, message, sourceName);
	}

	public override void LogWarning(string message, string sourceName = "")
	{
		Log(Blizzard.GameService.SDK.Client.Integration.LogLevel.Warning, message, sourceName);
	}

	public override void LogError(string message, string sourceName = "")
	{
		Log(Blizzard.GameService.SDK.Client.Integration.LogLevel.Error, message, sourceName);
	}

	public override void LogException(string message, string sourceName = "")
	{
		Log(Blizzard.GameService.SDK.Client.Integration.LogLevel.Exception, message, sourceName);
	}

	public override void LogFatal(string message, string sourceName = "")
	{
		Log(Blizzard.GameService.SDK.Client.Integration.LogLevel.Fatal, message, sourceName);
	}
}
