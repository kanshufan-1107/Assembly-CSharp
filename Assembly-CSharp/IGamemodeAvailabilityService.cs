using Blizzard.T5.Services;

public interface IGamemodeAvailabilityService : IService
{
	public enum Gamemode
	{
		NONE,
		HEARTHSTONE,
		BATTLEGROUNDS,
		MERCENARIES,
		TAVERN_BRAWL,
		ARENA,
		DUEL,
		SOLO_ADVENTURE
	}

	public enum Status
	{
		NONE = 0,
		UNINITIALIZED = 1,
		NOT_DOWNLOADED = 10,
		WAITING_FOR_TUTORIAL = 20,
		TUTORIAL_INCOMPLETE = 21,
		READY = 100
	}

	public delegate void OnGamemodeStateChanged(Gamemode mode, Status newStatus, Status oldStatus);

	Status GetGamemodeStatus(Gamemode mode);
}
