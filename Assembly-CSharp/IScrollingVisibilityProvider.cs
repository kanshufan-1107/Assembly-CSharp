using System;
using UnityEngine;

public interface IScrollingVisibilityProvider
{
	void AddFastVisibleAffectedObject(GameObject topObj, Vector3 extents, bool visible, float buffer, Action<int, int, bool> callback = null);

	void RemoveFastVisibleAffectedObject(GameObject index);

	void ChangeExtentsOnFastVisibleObject(GameObject topObj, Vector3 extents);

	void UpdateVisibility(GameObject gameObject);
}
