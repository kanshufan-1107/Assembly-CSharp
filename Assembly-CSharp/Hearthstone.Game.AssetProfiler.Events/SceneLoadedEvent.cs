namespace Hearthstone.Game.AssetProfiler.Events;

public class SceneLoadedEvent : SceneChangedEvent
{
	public override string Name => "SceneLoadedEvent";

	public SceneLoadedEvent(string scene, string mode)
		: base(scene, mode)
	{
	}
}
