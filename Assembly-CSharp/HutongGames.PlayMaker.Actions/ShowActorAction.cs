using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Calls Show() on an attatched Actor")]
public class ShowActorAction : FsmStateAction
{
	[RequiredField]
	public FsmOwnerDefault gameObject;

	public override void Reset()
	{
		gameObject = null;
	}

	public override void OnEnter()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(gameObject);
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter(): GameObject {1} does not have an Actor component to show.", this, go);
		}
		else
		{
			actor.Show();
		}
	}
}
