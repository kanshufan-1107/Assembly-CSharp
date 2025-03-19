using UnityEngine;

public class MercenariesClassFilterButton : PegUIElement
{
	public GameObject m_newCardCount;

	public UberText m_newCardCountText;

	[SerializeField]
	private TAG_ROLE m_role;

	public TAG_ROLE Role => m_role;

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
		(CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as LettuceCollectionPageManager).SelectRole(m_role);
		GetComponentInParent<SlidingTray>().HideTray();
	}

	public void SetNewCardCount(int count)
	{
		if (m_newCardCount != null)
		{
			m_newCardCount.SetActive(count > 0);
		}
		if (count > 0 && m_newCardCountText != null)
		{
			m_newCardCountText.Text = GameStrings.Format("GLUE_COLLECTION_NEW_CARD_CALLOUT", count);
		}
	}
}
