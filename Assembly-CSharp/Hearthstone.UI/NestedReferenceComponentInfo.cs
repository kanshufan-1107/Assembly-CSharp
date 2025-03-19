using UnityEngine;

namespace Hearthstone.UI;

public struct NestedReferenceComponentInfo
{
	public Component FoundComponent;

	public bool CheckedAllComponents;

	public NestedReferenceComponentInfo(Component comp, bool componentsChecked = true)
	{
		FoundComponent = comp;
		CheckedAllComponents = componentsChecked;
	}
}
