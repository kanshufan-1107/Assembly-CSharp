using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Checks if the actor is set to skip armor animations")]
public class DoesActorWantToSkipArmorAnimation : FsmStateAction
{
	public FsmOwnerDefault m_GameObject;

	[Tooltip("Output variable.")]
	[RequiredField]
	[UIHint(UIHint.Variable)]
	public FsmBool m_Result;

	public override void Reset()
	{
		m_GameObject = null;
		m_Result = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_Result.Value = false;
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (go != null)
		{
			Actor actor = go.GetComponent<Actor>();
			if (actor != null)
			{
				m_Result.Value = actor.IsSkipArmorAnimationActive();
			}
		}
		Finish();
	}
}
