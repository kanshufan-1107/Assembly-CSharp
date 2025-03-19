using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Triggers an event on the FSM of the Legendary Hero object attached to an actor")]
public class TriggerLegendaryHeroAnimEvent : FsmStateAction
{
	[CheckForComponent(typeof(Actor))]
	[Tooltip("Actors game object")]
	[RequiredField]
	public FsmOwnerDefault m_GameObject;

	[Tooltip("The animation to play.")]
	public LegendaryHeroAnimations anim;

	public override void OnEnter()
	{
		base.OnEnter();
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (!go)
		{
			Finish();
			return;
		}
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - FAILED to find actor on game object {1}", this, m_GameObject);
			Finish();
			return;
		}
		ILegendaryHeroPortrait portrait = actor.LegendaryHeroPortrait;
		if (portrait == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - FAILED to find legendary portrait on actor {1}", this, m_GameObject);
			Finish();
		}
		else
		{
			portrait.RaiseAnimationEvent(anim);
			Finish();
		}
	}
}
