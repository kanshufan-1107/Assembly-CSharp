using Hearthstone.UI;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Send an event up the UI hierarchy")]
[ActionCategory("Pegasus")]
public class WidgetBubbleUpEventAction : FsmStateAction
{
	[RequiredField]
	[Tooltip("Name of the event we're sending up.")]
	public FsmString eventName;

	public override void Reset()
	{
		eventName = null;
	}

	public override void OnEnter()
	{
		SendEventUpward();
		Finish();
	}

	private void SendEventUpward()
	{
		if (eventName == null || eventName.Value == null)
		{
			Debug.LogError("WidgetBubbleUpEventAction.SendEventUpward() - Event Name is null.");
		}
		else
		{
			SendEventUpwardStateAction.SendEventUpward(base.Owner, eventName.Value);
		}
	}
}
