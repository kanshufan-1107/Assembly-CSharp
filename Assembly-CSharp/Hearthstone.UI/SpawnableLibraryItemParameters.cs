using System;

namespace Hearthstone.UI;

[Serializable]
public abstract class SpawnableLibraryItemParameters
{
	public abstract SpawnableLibrary.ItemType ItemType { get; }
}
