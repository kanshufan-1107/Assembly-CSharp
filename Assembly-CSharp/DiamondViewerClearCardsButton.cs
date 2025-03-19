using UnityEngine;

public class DiamondViewerClearCardsButton : MonoBehaviour
{
	public void OnButtonPress()
	{
		Actor[] array = Object.FindObjectsOfType<Actor>();
		foreach (Actor a in array)
		{
			Debug.Log("Deleting : " + a.name);
			a.Destroy();
		}
	}
}
