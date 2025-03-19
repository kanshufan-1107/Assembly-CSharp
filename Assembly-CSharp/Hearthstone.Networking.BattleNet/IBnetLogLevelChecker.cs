using Blizzard.GameService.SDK.Client.Integration;

namespace Hearthstone.Networking.BattleNet;

public interface IBnetLogLevelChecker
{
	bool IsEnabled(LogLevel level);
}
