using Hearthstone.UI;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Send an event down the UI hierarchy")]
public class WidgetBubbleDownEventAction : FsmStateAction
{
	[Tooltip("Name of the event we're sending down.")]
	[RequiredField]
	public FsmString eventName;

	[Tooltip("How far down should the event be propagated")]
	public SendEventDownwardStateAction.BubbleDownEventDepth DepthLimit;

	public override void Reset()
	{
		eventName = null;
		DepthLimit = SendEventDownwardStateAction.BubbleDownEventDepth.DirectChildrenOnly;
	}

	public override void OnEnter()
	{
		SendEventDownward();
		Finish();
	}

	private void SendEventDownward()
	{
		if (eventName == null || eventName.Value == null)
		{
			Debug.LogError("WidgetBubbleDownEventAction.SendEventDownward() - Event Name is null.");
		}
		else
		{
			SendEventDownwardStateAction.SendEventDownward(base.Owner, eventName.Value, DepthLimit);
		}
	}
}
