using UnityEngine;

public class BaconClassFilterButton : PegUIElement
{
	public GameObject m_newItemCount;

	public UberText m_newItemCountText;

	public CollectionUtils.ViewMode m_tabViewMode = CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS;

	protected int m_numNewItems;

	protected override void Awake()
	{
		AddEventListener(UIEventType.RELEASE, delegate
		{
			HandleRelease();
		});
		base.Awake();
	}

	public void HandleRelease()
	{
		CollectionManager.Get().GetCollectibleDisplay().SetViewMode(m_tabViewMode);
		GetComponentInParent<SlidingTray>().HideTray();
	}

	public void UpdateNewItemCount(int numNewItems)
	{
		m_numNewItems = numNewItems;
		UpdateNewItemCountVisuals();
	}

	private void UpdateNewItemCountVisuals()
	{
		if (m_newItemCountText != null)
		{
			m_newItemCountText.Text = GameStrings.Format("GLUE_COLLECTION_NEW_CARD_CALLOUT", m_numNewItems);
		}
		if (m_newItemCount != null)
		{
			m_newItemCount.SetActive(m_numNewItems > 0);
		}
	}
}
