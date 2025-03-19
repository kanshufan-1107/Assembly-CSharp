using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Basic setup for a custom spawn effect.\n\nSwitches Actor from being Owner's parent to being Owner's grandchild.\n\nNote: Only use this if you're animating the token. Otherwise, just use SpellInitActorVariables.")]
public class InitializeCustomSpawn : FsmStateAction
{
	[Tooltip("Token root.")]
	[RequiredField]
	public FsmGameObject tokenRoot;

	[Tooltip("Whether or not to reset Actor's local position to 0,0,0 after parenting.")]
	[UIHint(UIHint.FsmBool)]
	public FsmBool resetActorPosition;

	[Tooltip("Whether or not to reset Actor's local rotation to 0,0,0 after parenting.")]
	[UIHint(UIHint.FsmBool)]
	public FsmBool resetActorRotation;

	[Tooltip("Whether or not to reset Actor's local scale to 1,1,1 after parenting.")]
	[UIHint(UIHint.FsmBool)]
	public FsmBool resetActorScale;

	[Tooltip("Optional: Store the actor in this variable.")]
	[UIHint(UIHint.Variable)]
	public FsmGameObject actorOutput;

	[Tooltip("Optional: Store the actorParent in this variable.")]
	[UIHint(UIHint.Variable)]
	public FsmGameObject actorParentOutput;

	public override void Reset()
	{
		tokenRoot = null;
		resetActorPosition = new FsmBool
		{
			Value = false
		};
		resetActorRotation = new FsmBool
		{
			Value = false
		};
		resetActorScale = new FsmBool
		{
			Value = false
		};
		actorOutput = new FsmGameObject
		{
			UseVariable = true
		};
		actorParentOutput = new FsmGameObject
		{
			UseVariable = true
		};
	}

	public override void OnEnter()
	{
		DoAction();
		Finish();
	}

	private void DoAction()
	{
		if (tokenRoot == null || tokenRoot.IsNone || tokenRoot.Value == null)
		{
			Debug.LogError(GetType().Name + " is missing required field.");
			return;
		}
		GameObject owner = GameObjectUtils.FindComponentInThisOrParents<Transform>(base.Owner).gameObject;
		GameObject actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(base.Owner).gameObject;
		GameObject actorParent = actor.transform.parent.gameObject;
		if (owner == null || actor == null || actorParent == null)
		{
			Debug.LogError(GetType().Name + " is not properly constructed.");
			return;
		}
		owner.transform.SetParent(actorParent.transform, worldPositionStays: true);
		actor.transform.SetParent(tokenRoot.Value.transform, worldPositionStays: true);
		if (resetActorPosition.Value)
		{
			actor.transform.localPosition = Vector3.zero;
		}
		if (resetActorRotation.Value)
		{
			actor.transform.localEulerAngles = Vector3.zero;
		}
		if (resetActorScale.Value)
		{
			actor.transform.localScale = Vector3.one;
		}
		if (!actorOutput.IsNone && actorOutput.UseVariable)
		{
			actorOutput.Value = actor;
		}
		if (!actorParentOutput.IsNone && actorParentOutput.UseVariable)
		{
			actorParentOutput.Value = actorParent;
		}
	}
}
