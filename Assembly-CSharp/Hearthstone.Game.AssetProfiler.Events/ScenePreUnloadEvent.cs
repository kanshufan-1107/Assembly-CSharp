namespace Hearthstone.Game.AssetProfiler.Events;

public class ScenePreUnloadEvent : SceneChangedEvent
{
	public override string Name => "ScenePreUnloadEvent";

	public ScenePreUnloadEvent(string scene, string mode)
		: base(scene, mode)
	{
	}
}
