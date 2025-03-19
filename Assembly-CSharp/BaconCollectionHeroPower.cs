using UnityEngine;

public class BaconCollectionHeroPower : MonoBehaviour
{
	public GameObject m_shadow;

	public void HideItemsForGhostView()
	{
		m_shadow?.SetActive(value: false);
	}
}
