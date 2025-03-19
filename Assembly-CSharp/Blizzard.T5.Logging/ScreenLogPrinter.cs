using Blizzard.T5.Services;

namespace Blizzard.T5.Logging;

public class ScreenLogPrinter : ILogPrinter
{
	public string PrinterTargetName => "SCREEN";

	public void LogToPrinter(string logName, string logLine, LogLevel level, string timeOfDay)
	{
		if (ServiceManager.TryGet<SceneDebugger>(out var sceneDebugger))
		{
			string formatMessage = $"[{logName}] {logLine}";
			sceneDebugger.AddMessage(level, formatMessage);
		}
	}
}
