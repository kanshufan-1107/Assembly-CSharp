using UnityEngine;

public class MobileActorMinion : MonoBehaviour
{
	public Vector3 m_minionScaleFactor = Vector3.one;

	private void Awake()
	{
		if (PlatformSettings.IsMobile() && (bool)UniversalInputManager.UsePhoneUI)
		{
			Vector3 currentScale = base.gameObject.transform.localScale;
			currentScale.x *= m_minionScaleFactor.x;
			currentScale.y *= m_minionScaleFactor.y;
			currentScale.z *= m_minionScaleFactor.z;
			base.gameObject.transform.localScale = currentScale;
		}
	}
}
