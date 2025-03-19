using UnityEngine;

public class BackBehavior : MonoBehaviour
{
	public void Awake()
	{
		PegUIElement element = base.gameObject.GetComponent<PegUIElement>();
		if (element != null)
		{
			element.AddEventListener(UIEventType.RELEASE, delegate
			{
				OnRelease();
			});
		}
	}

	public void OnRelease()
	{
		Navigation.GoBack();
	}
}
