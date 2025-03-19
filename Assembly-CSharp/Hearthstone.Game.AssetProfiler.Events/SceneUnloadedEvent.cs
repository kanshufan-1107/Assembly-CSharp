namespace Hearthstone.Game.AssetProfiler.Events;

public class SceneUnloadedEvent : SceneChangedEvent
{
	public override string Name => "SceneUnloadedEvent";

	public SceneUnloadedEvent(string scene, string mode)
		: base(scene, mode)
	{
	}
}
