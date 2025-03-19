using Blizzard.GameService.SDK.Client.Integration;

namespace Hearthstone.Networking.BattleNet;

public class BattleNetLogFatalErrorHandler
{
	private readonly IBnetErrorReporter m_bnetErrorReporter;

	public BattleNetLogFatalErrorHandler(IBnetErrorReporter bnetErrorReporter)
	{
		m_bnetErrorReporter = bnetErrorReporter;
	}

	public void OnBnetLog(object sender, BnetLogEventArgs args)
	{
		HandleLog(args.Level, args.Message, args.Source);
	}

	private void HandleLog(LogLevel logLevel, string message, string sourceName)
	{
		if (logLevel == LogLevel.Fatal)
		{
			SendFatalEvent(message);
		}
	}

	private void SendFatalEvent(string message)
	{
		if (HearthstoneApplication.IsInternal())
		{
			string locStr = GameStrings.Get(message);
			m_bnetErrorReporter.ReportDevFatal(locStr);
		}
		else
		{
			m_bnetErrorReporter.ReportFatal(FatalErrorReason.BNET_FATAL, "GLOBAL_ERROR_NETWORK_UNAVAILABLE_UNKNOWN");
		}
	}
}
