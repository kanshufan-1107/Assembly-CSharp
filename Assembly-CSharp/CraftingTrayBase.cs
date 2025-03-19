using UnityEngine;

public abstract class CraftingTrayBase : MonoBehaviour
{
	public abstract void Show(bool? firstOption = null, bool? secondOption = null, bool? thirdOption = null, bool? fourthOption = null, bool? fifthOption = null, bool updatePage = true);

	public abstract void Hide();

	public abstract bool IsShown();
}
