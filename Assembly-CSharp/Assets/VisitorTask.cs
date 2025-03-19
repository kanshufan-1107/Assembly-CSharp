using Blizzard.T5.Core.Utils;

namespace Assets;

public static class VisitorTask
{
	public enum VillageTutorialServerEvent
	{
		NONE,
		GRANT_TUTORIAL_MERCENARY,
		ADD_MERCENARY_TO_TEAM,
		ADD_TUTORIAL_VISITOR,
		ADD_VISITORS_AFTER_TUTORIAL,
		GRANT_STARTER_QUEST_CHAIN
	}

	public static VillageTutorialServerEvent ParseVillageTutorialServerEventValue(string value)
	{
		EnumUtils.TryGetEnum<VillageTutorialServerEvent>(value, out var e);
		return e;
	}
}
