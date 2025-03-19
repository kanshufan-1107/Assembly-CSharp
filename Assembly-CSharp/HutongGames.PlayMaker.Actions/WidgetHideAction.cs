using Hearthstone.UI;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Hides a widget on a game object.")]
[ActionCategory("Pegasus")]
public class WidgetHideAction : FsmStateAction
{
	[Tooltip("Game object containing the widget to hide.")]
	[RequiredField]
	public FsmGameObject widgetObject;

	public override void Reset()
	{
		widgetObject = null;
	}

	public override void OnEnter()
	{
		Hide();
		Finish();
	}

	private void Hide()
	{
		if (widgetObject == null || widgetObject.Value == null)
		{
			Debug.LogError("WidgetHideAction.Hide() - Widget Object is null.");
			return;
		}
		Widget widget = widgetObject.Value.GetComponent<Widget>();
		if (widget == null)
		{
			Debug.LogError($"WidgetHideAction.Hide() - Game Object {widgetObject} does not have a Widget component.");
		}
		else
		{
			widget.Hide();
		}
	}
}
