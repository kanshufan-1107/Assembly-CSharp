using Hearthstone;

[CustomEditClass]
public class LettuceCollectionScene : BasicScene
{
	public override void Unload()
	{
		if (CollectionManager.Get().GetCollectibleDisplay() != null)
		{
			CollectionManager.Get().GetCollectibleDisplay().Unload();
		}
		Network.Get().SendAckCardsSeen();
		base.Unload();
	}

	private void OnDestroy()
	{
		HearthstoneApplication.Get()?.UnloadUnusedAssets();
	}
}
