using UnityEngine;

namespace Hearthstone.UI;

public interface INestedReferenceResolver : IAsyncInitializationBehavior
{
	NestedReferenceComponentInfo GetComponentInfoById(long id);

	bool GetComponentId(Component component, out long id);

	string GetPathToObject();
}
