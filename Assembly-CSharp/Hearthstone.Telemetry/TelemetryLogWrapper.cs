using Blizzard.Telemetry;

namespace Hearthstone.Telemetry;

public class TelemetryLogWrapper : ILogger
{
	public void LogInfo(string message)
	{
		Log.Telemetry.PrintInfo("[SDK] {0}", message);
	}

	public void LogError(string message)
	{
		Log.Telemetry.PrintError("[SDK] {0}", message);
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		return true;
	}
}
