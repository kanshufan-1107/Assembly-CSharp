namespace Hearthstone.UI;

public static class SpawnableLibraryItemParametersFactory
{
	public static SpawnableLibraryItemParameters CreateParameters(SpawnableLibrary.ItemType itemType)
	{
		if (itemType == SpawnableLibrary.ItemType.Sprite)
		{
			return new SpriteItemParameters();
		}
		return null;
	}
}
