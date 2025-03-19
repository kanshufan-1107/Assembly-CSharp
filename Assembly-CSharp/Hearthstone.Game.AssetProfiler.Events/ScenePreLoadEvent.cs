namespace Hearthstone.Game.AssetProfiler.Events;

public class ScenePreLoadEvent : ModeChangedEvent
{
	public override string Name => "ScenePreLoadEvent";

	public ScenePreLoadEvent(string previousMode, string nextMode)
		: base(previousMode, nextMode)
	{
	}
}
