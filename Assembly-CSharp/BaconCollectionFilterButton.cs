using UnityEngine;

public class BaconCollectionFilterButton : MonoBehaviour
{
	public UIBButton m_activeFilterButton;

	public UIBButton m_inactiveFilterButton;

	protected void Awake()
	{
		if (m_inactiveFilterButton != null)
		{
			m_inactiveFilterButton.AddEventListener(UIEventType.RELEASE, delegate
			{
				ToggleFilters();
			});
		}
		if (m_activeFilterButton != null)
		{
			m_activeFilterButton.AddEventListener(UIEventType.RELEASE, delegate
			{
				ToggleFilters();
			});
		}
	}

	public void SetActive(bool active)
	{
		if (m_activeFilterButton != null && m_activeFilterButton.IsEnabled() != active)
		{
			m_activeFilterButton.SetEnabled(active);
			m_activeFilterButton.Flip(active);
		}
		if (m_inactiveFilterButton != null && m_inactiveFilterButton.IsEnabled() != active)
		{
			m_inactiveFilterButton.SetEnabled(active);
			m_inactiveFilterButton.Flip(active);
		}
	}

	private void ToggleFilters()
	{
		(CollectionManager.Get().GetCollectibleDisplay() as BaconCollectionDisplay).ToggleHeroSkinFilterMode();
		FilterUpdated();
	}

	public void FilterUpdated()
	{
		if ((CollectionManager.Get().GetCollectibleDisplay() as BaconCollectionDisplay).GetHeroSkinFilterMode() == CollectionUtils.BattlegroundsHeroSkinFilterMode.DEFAULT)
		{
			m_activeFilterButton.gameObject.SetActive(value: false);
			m_inactiveFilterButton.gameObject.SetActive(value: true);
		}
		else
		{
			m_activeFilterButton.gameObject.SetActive(value: true);
			m_inactiveFilterButton.gameObject.SetActive(value: false);
		}
	}
}
