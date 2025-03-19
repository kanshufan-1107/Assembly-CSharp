namespace Hearthstone.Game.AssetProfiler.Events;

public class AssetBundleUnloadedEvent : AssetBundleChangedEvent
{
	public override string Name => "AssetBundleUnloadedEvent";

	public AssetBundleUnloadedEvent(string assetBundleName)
		: base(assetBundleName)
	{
	}
}
