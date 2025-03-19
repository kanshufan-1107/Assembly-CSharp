using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Basic cleanup for a custom spawn effect.\n\nMakes Actor a sibling of Owner. (Switches Actor back to its original parent.).\n\nNote: Only use this if you also used InitializeCustomSpawn.")]
public class FinalizeCustomSpawn : FsmStateAction
{
	[RequiredField]
	[Tooltip("Actor.")]
	public FsmGameObject actor;

	[Tooltip("Whether or not to reset Actor's local position to 0,0,0 after parenting.")]
	[UIHint(UIHint.FsmBool)]
	public FsmBool resetActorPosition;

	[UIHint(UIHint.FsmBool)]
	[Tooltip("Whether or not to reset Actor's local rotation to 0,0,0 after parenting.")]
	public FsmBool resetActorRotation;

	[UIHint(UIHint.FsmBool)]
	[Tooltip("Whether or not to reset Actor's local scale to 1,1,1 after parenting.")]
	public FsmBool resetActorScale;

	public override void Reset()
	{
		actor = null;
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
	}

	public override void OnEnter()
	{
		DoAction();
		Finish();
	}

	private void DoAction()
	{
		if (actor == null || actor.IsNone || actor.Value == null)
		{
			Debug.LogError(GetType().Name + " is missing required field.");
			return;
		}
		if (actor.Value.GetComponent<Actor>() == null)
		{
			Debug.LogWarning($"{GetType().Name} expected Actor component on {actor.Value}.");
		}
		GameObject actorParent = GameObjectUtils.FindComponentInThisOrParents<Transform>(base.Owner).gameObject.transform.parent.gameObject;
		if (actorParent == null)
		{
			Debug.LogError(GetType().Name + " is not properly constructed.");
			return;
		}
		actor.Value.transform.SetParent(actorParent.transform, worldPositionStays: true);
		if (resetActorPosition.Value)
		{
			actor.Value.transform.localPosition = Vector3.zero;
		}
		if (resetActorRotation.Value)
		{
			actor.Value.transform.localEulerAngles = Vector3.zero;
		}
		if (resetActorScale.Value)
		{
			actor.Value.transform.localScale = Vector3.one;
		}
	}
}
