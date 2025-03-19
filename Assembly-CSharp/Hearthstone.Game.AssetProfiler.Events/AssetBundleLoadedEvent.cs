namespace Hearthstone.Game.AssetProfiler.Events;

public class AssetBundleLoadedEvent : AssetBundleChangedEvent
{
	public override string Name => "AssetBundleLoadedEvent";

	public AssetBundleLoadedEvent(string assetBundleName)
		: base(assetBundleName)
	{
	}
}
