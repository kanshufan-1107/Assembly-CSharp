using Blizzard.T5.Core.Utils;

namespace Assets;

public static class TavernGuideQuest
{
	public enum UnlockableGameMode
	{
		INVALID,
		TAVERN_BRAWL,
		ARENA,
		BATTLEGROUNDS,
		SOLO_ADVENTURES,
		DUELS,
		MERCENARIES
	}

	public static UnlockableGameMode ParseUnlockableGameModeValue(string value)
	{
		EnumUtils.TryGetEnum<UnlockableGameMode>(value, out var e);
		return e;
	}
}
