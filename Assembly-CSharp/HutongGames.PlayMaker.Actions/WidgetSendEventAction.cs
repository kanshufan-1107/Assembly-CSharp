using Hearthstone.UI;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Send an event to the chosen game object. (No payload)")]
[ActionCategory("Pegasus")]
public class WidgetSendEventAction : FsmStateAction
{
	[Tooltip("Specify which game object to send the event.")]
	[RequiredField]
	public FsmGameObject gameObject;

	[Tooltip("Name of the event we're sending to the widget.")]
	[RequiredField]
	public FsmString eventName;

	public override void Reset()
	{
		gameObject = null;
		eventName = null;
	}

	public override void OnEnter()
	{
		SendEvent();
		Finish();
	}

	private void SendEvent()
	{
		if (gameObject == null || gameObject.Value == null)
		{
			Debug.LogError("WidgetSendEventAction.SendEvent() - Game Object is null.");
		}
		else if (eventName == null || eventName.Value == null)
		{
			Debug.LogError("WidgetSendEventAction.SendEvent() - Event Name is null.");
		}
		else if (!EventFunctions.TriggerEvent(gameObject.Value.transform, eventName.Value, new TriggerEventParameters($"Playmaker {gameObject}: {base.State.Name}", null, noDownwardPropagation: true, ignorePlaymaker: true)))
		{
			Debug.LogError($"WidgetSendEventAction.SendEvent() - Sending event '{eventName}' to '{gameObject}' but no receivers were found");
		}
	}
}
