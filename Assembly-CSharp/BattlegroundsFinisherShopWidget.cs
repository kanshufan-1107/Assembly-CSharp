using UnityEngine;

public class BattlegroundsFinisherShopWidget : MonoBehaviour
{
	public Transform m_finisherWidgetTransform;

	private void Start()
	{
		if (base.transform.rotation.eulerAngles.y != 0f)
		{
			m_finisherWidgetTransform.localRotation = Quaternion.identity;
		}
	}
}
