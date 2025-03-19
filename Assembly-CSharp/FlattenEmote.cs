using Hearthstone.UI;
using UnityEngine;

public class FlattenEmote : MonoBehaviour
{
	public WidgetInstance target;

	public int TargetSortingOrder;

	private void Start()
	{
		target.RegisterReadyListener(FlattenListener);
	}

	private void FlattenListener(object o)
	{
		Flatten();
	}

	public void Flatten()
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sortingOrder = TargetSortingOrder;
		}
	}
}
