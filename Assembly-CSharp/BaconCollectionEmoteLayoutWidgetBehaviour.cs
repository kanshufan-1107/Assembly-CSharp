using Hearthstone.UI;
using UnityEngine;
using UnityEngine.Rendering;

public class BaconCollectionEmoteLayoutWidgetBehaviour : MonoBehaviour
{
	[SerializeField]
	private AsyncReference m_imageWidgetRef;

	private SortingGroup m_sortingGroup;

	private BoxCollider m_dragCollider;

	public bool m_flipBubble;

	[SerializeField]
	private bool m_disableClickable;

	private void Start()
	{
		m_imageWidgetRef.RegisterReadyListener(delegate(Transform t)
		{
			LayerUtils.SetLayer(base.gameObject, GameLayer.IgnoreFullScreenEffects);
			m_sortingGroup = GetComponentInChildren<SortingGroup>();
			BoxCollider[] componentsInChildren = t.GetComponentsInChildren<BoxCollider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			if (m_disableClickable)
			{
				base.gameObject.GetComponentInChildren<Clickable>().enabled = false;
			}
		});
	}

	public void IncreaseSpriteSortOrder(int amount)
	{
		m_sortingGroup.sortingOrder += amount;
	}

	public BoxCollider GetDragCollider()
	{
		if (m_dragCollider == null)
		{
			m_dragCollider = GetComponentInChildren<BoxCollider>();
		}
		return m_dragCollider;
	}
}
