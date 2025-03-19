using UnityEngine;

public class RaycastCache : MonoBehaviour
{
	private void Update()
	{
		UniversalInputManager.Get()?.UpdateCachedValues();
	}
}
