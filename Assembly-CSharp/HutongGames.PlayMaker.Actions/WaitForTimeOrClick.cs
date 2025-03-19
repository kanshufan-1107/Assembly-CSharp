using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Delays a state from finishing by the specified time, unless a mouse press interrupts it. NOTE: Other actions continue, but FINISHED can't happen before Time.")]
[ActionCategory(ActionCategory.Time)]
public class WaitForTimeOrClick : FsmStateAction
{
	[Tooltip("The maximum time this will delay the FINISHED event, unless interrupted by a click.")]
	[RequiredField]
	public FsmFloat time;

	[RequiredField]
	[Tooltip("The mouse button to listen for.")]
	public MouseButton button;

	public FsmEvent finishEvent;

	private float startTime;

	private float timer;

	public override void Reset()
	{
		time = 1f;
		button = MouseButton.Left;
	}

	public override void OnEnter()
	{
		if (time.Value <= 0f)
		{
			base.Fsm.Event(finishEvent);
			Finish();
		}
		else
		{
			startTime = FsmTime.RealtimeSinceStartup;
			timer = 0f;
		}
	}

	public override void OnUpdate()
	{
		timer += Time.deltaTime;
		bool num = timer >= time.Value;
		bool buttonClicked = InputCollection.GetMouseButton((int)button);
		if (num || buttonClicked)
		{
			Finish();
			if (finishEvent != null)
			{
				base.Fsm.Event(finishEvent);
			}
		}
	}
}
