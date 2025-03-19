using UnityEngine;

public class Hero_08as_WeaponLocator : MonoBehaviour
{
	public Camera m_rttCamera;

	public Vector3 GetWeaponLocatorPos()
	{
		return m_rttCamera.WorldToViewportPoint(base.transform.position);
	}
}
