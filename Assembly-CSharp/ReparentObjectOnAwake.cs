using UnityEngine;

public class ReparentObjectOnAwake : MonoBehaviour
{
	public Transform NewParent;

	public bool WorldPositionStays = true;

	private void Awake()
	{
		if (NewParent != null)
		{
			base.transform.SetParent(NewParent, WorldPositionStays);
		}
	}
}
