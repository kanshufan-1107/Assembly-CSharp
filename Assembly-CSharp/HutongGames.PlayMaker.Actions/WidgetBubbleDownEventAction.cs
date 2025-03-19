using Hearthstone.UI;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Send an event down the UI hierarchy")]
[ActionCategory("Pegasus")]
public class WidgetBubbleDownEventAction : FsmStateAction
{
	[RequiredField]
	[Tooltip("Name of the event we're sending down.")]
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
