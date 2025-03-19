using Hearthstone;

[CustomEditClass]
public class LettuceBountyBoardScene : BasicScene
{
	private void OnDestroy()
	{
		if (HearthstoneApplication.Get() != null)
		{
			HearthstoneApplication.Get().UnloadUnusedAssets();
		}
	}
}
