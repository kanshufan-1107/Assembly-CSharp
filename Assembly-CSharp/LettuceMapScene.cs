using Hearthstone;

[CustomEditClass]
public class LettuceMapScene : BasicScene
{
	private void OnDestroy()
	{
		HearthstoneApplication app = HearthstoneApplication.Get();
		if (app != null)
		{
			app.UnloadUnusedAssets();
		}
	}
}
