using Blizzard.GameService.SDK.Client.Integration;

namespace Hearthstone.Networking.BattleNet;

public struct BnetLogEventArgs
{
	public LogLevel Level { get; set; }

	public string Message { get; set; }

	public string Source { get; set; }

	public BnetLogEventArgs(LogLevel level, string message, string source)
	{
		Level = level;
		Message = message;
		Source = source;
	}
}
