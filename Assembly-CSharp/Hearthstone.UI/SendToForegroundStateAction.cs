using UnityEngine;

namespace Hearthstone.UI;

public class SendToForegroundStateAction : StateActionImplementation
{
	public static readonly int s_SendToOrthoBoolIndex;

	public override void Run(bool loadSynchronously = false)
	{
		RunOnInstanceOrTargetGameObject(GetOverride(0).NestedReference, enableInstance: true, delegate(WidgetInstance instance)
		{
			SendToForeground(instance.Widget.gameObject);
		}, SendToForeground);
	}

	private void SendToForeground(GameObject gameObject)
	{
		if (gameObject != null)
		{
			gameObject.SetActive(value: true);
		}
		if (GetBoolValueAtIndex(s_SendToOrthoBoolIndex))
		{
			OverlayUI.Get().AddGameObject(gameObject);
		}
		UIContext.GetRoot().ShowPopup(gameObject, UIContext.BlurType.Legacy);
		Complete(success: true);
	}
}
