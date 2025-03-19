using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone;

public class BlurOnTopOfPopup : MonoBehaviour
{
	public Widget WidgetToListenOn;

	public GameObject TargetToBlurBehind;

	private const string BLUR = "BLUR_BACKGROUND";

	private const string CLEAR = "CLEAR_BACKGROUND";

	private void Awake()
	{
		WidgetToListenOn.RegisterEventListener(delegate(string eventName)
		{
			if (!(eventName == "BLUR_BACKGROUND"))
			{
				if (eventName == "CLEAR_BACKGROUND")
				{
					ClearBackground();
				}
			}
			else
			{
				BlurBackground();
			}
		});
	}

	private void BlurBackground()
	{
		UIContext.GetRoot().ShowPopup(TargetToBlurBehind);
	}

	private void ClearBackground()
	{
		UIContext.GetRoot().DismissPopup(TargetToBlurBehind);
	}
}
