using System.Collections;
using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Move an object's actor.  Used for spells that are dynamically loaded.")]
public class iTweenMoveActorTo : iTweenFsmAction
{
	[RequiredField]
	public FsmOwnerDefault gameObject;

	[Tooltip("iTween ID. If set you can use iTween Stop action to stop it by its id.")]
	public FsmString id;

	[Tooltip("Position the GameObject will animate to.")]
	public FsmVector3 vectorPosition;

	[Tooltip("The time in seconds the animation will take to complete.")]
	public FsmFloat time;

	[Tooltip("The time in seconds the animation will wait before beginning.")]
	public FsmFloat delay;

	[Tooltip("The shape of the easing curve applied to the animation.")]
	public iTween.EaseType easeType = iTween.EaseType.linear;

	[Tooltip("The type of loop to apply once the animation has completed.")]
	public iTween.LoopType loopType;

	public override void Reset()
	{
		base.Reset();
		id = new FsmString
		{
			UseVariable = true
		};
		vectorPosition = new FsmVector3
		{
			UseVariable = true
		};
		time = 1f;
		delay = 0f;
		easeType = iTween.EaseType.linear;
		loopType = iTween.LoopType.none;
	}

	public override void OnEnter()
	{
		OnEnteriTween(gameObject, loopType != iTween.LoopType.none);
		DoiTween();
	}

	public override void OnExit()
	{
		OnExitiTween(gameObject);
	}

	private void DoiTween()
	{
		GameObject owner = base.Fsm.GetOwnerDefaultTarget(gameObject);
		if (owner == null)
		{
			return;
		}
		Actor actor = GameObjectUtils.FindComponentInParents<Actor>(owner);
		if (actor == null)
		{
			return;
		}
		GameObject actorObject = actor.gameObject;
		if (!(actorObject == null))
		{
			itweenType = "move";
			Hashtable argTable = iTweenManager.Get().GetTweenHashTable();
			argTable.Add("position", vectorPosition);
			argTable.Add("name", id.IsNone ? "" : id.Value);
			argTable.Add("delay", delay.IsNone ? 0f : delay.Value);
			argTable.Add("easetype", easeType);
			argTable.Add("looptype", loopType);
			argTable.Add("ignoretimescale", !realTime.IsNone && realTime.Value);
			if (time.Value <= 0f)
			{
				argTable.Add("time", 0f);
				iTween.FadeUpdate(actorObject, argTable);
				base.Fsm.Event(startEvent);
				base.Fsm.Event(finishEvent);
				Finish();
			}
			else
			{
				argTable["time"] = (time.IsNone ? 1f : time.Value);
				argTable.Add("oncomplete", "iTweenOnComplete");
				argTable.Add("oncompleteparams", itweenID);
				argTable.Add("onstart", "iTweenOnStart");
				argTable.Add("onstartparams", itweenID);
				argTable.Add("oncompletetarget", owner);
				iTween.MoveTo(actorObject, argTable);
			}
		}
	}
}
