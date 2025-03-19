using Hearthstone.UI;
using UnityEngine;

public class MulliganButton : MonoBehaviour
{
	public UberText uberText;

	public GameObject buttonContainer;

	public void SetText(string text)
	{
		uberText.Text = text;
		uberText.UpdateText();
	}

	public void SetEnabled(bool active)
	{
		VisualController vc = buttonContainer.GetComponent<VisualController>();
		if (active)
		{
			vc.SetState("Active");
		}
		else
		{
			vc.SetState("Inactive");
		}
	}

	public virtual bool AddEventListener(UIEventType type, UIEvent.Handler handler)
	{
		return buttonContainer.GetComponent<Clickable>().AddEventListener(type, handler);
	}
}
