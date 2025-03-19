using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.Telemetry.WTCG.Client;
using HearthstoneTelemetry;

namespace Hearthstone.Networking.BattleNet;

public class BattleNetLogTelemetryReporter
{
	private readonly ITelemetryClient m_client;

	public BattleNetLogTelemetryReporter(ITelemetryClient client)
	{
		m_client = client;
	}

	public void OnBnetLog(object sender, BnetLogEventArgs args)
	{
		ReportLog(args.Level, args.Message, args.Source);
	}

	private void ReportLog(LogLevel level, string message, string source)
	{
		switch (level)
		{
		case LogLevel.Fatal:
			SendFatalNetworkErrorTelemetry(message, source);
			break;
		case LogLevel.Exception:
			SendLiveIssueTelemetry(message, source);
			break;
		}
	}

	private void SendFatalNetworkErrorTelemetry(string message, string sourceName)
	{
		string formattedMessage = ((!string.IsNullOrEmpty(sourceName)) ? ("Source: " + sourceName + "  Message: " + message) : message);
		m_client?.SendNetworkError(NetworkError.ErrorType.FATAL_LOG, formattedMessage, 0);
	}

	private void SendLiveIssueTelemetry(string message, string sourceName)
	{
		string telemetryMessage = FormatLiveTelemetryMessage(message, sourceName);
		m_client.SendLiveIssue("Battle.net Exception", telemetryMessage);
	}

	private static string FormatLiveTelemetryMessage(string message, string sourceName)
	{
		if (string.IsNullOrEmpty(sourceName))
		{
			return "Exception: " + message;
		}
		return "Source: " + sourceName + "  Message: " + message;
	}
}
