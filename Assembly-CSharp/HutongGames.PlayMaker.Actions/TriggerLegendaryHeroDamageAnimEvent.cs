using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Triggers an event on the FSM of the Legendary Hero object attached to an actor")]
[ActionCategory("Pegasus")]
public class TriggerLegendaryHeroDamageAnimEvent : FsmStateAction
{
	[Tooltip("Spells game object")]
	public FsmOwnerDefault m_SpellObject;

	[CheckForComponent(typeof(Actor))]
	[RequiredField]
	[Tooltip("Actors game object")]
	public FsmOwnerDefault m_HeroObject;

	[Tooltip("The animation to play.")]
	public LegendaryHeroAnimations damageAnim;

	[Tooltip("The animation to play.")]
	public LegendaryHeroAnimations healAnim;

	public override void OnEnter()
	{
		base.OnEnter();
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_HeroObject);
		if (!go)
		{
			Finish();
			return;
		}
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - FAILED to find actor on game object {1}", this, m_HeroObject);
			Finish();
			return;
		}
		Entity entity = actor.GetEntity();
		if (entity == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - FAILED to find entity on game object {1}", this, m_HeroObject);
			Finish();
			return;
		}
		ILegendaryHeroPortrait portrait = actor.LegendaryHeroPortrait;
		if (portrait == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - FAILED to find legendary portrait on actor {1}", this, m_HeroObject);
			Finish();
			return;
		}
		float damage = entity.GetDamage();
		GameObject spellObject = base.Fsm.GetOwnerDefaultTarget(m_SpellObject);
		if (spellObject != null)
		{
			DamageSplatSpell splatSpell = spellObject.GetComponent<DamageSplatSpell>();
			if (splatSpell != null)
			{
				damage = splatSpell.GetDamage();
			}
		}
		if (damage > 0f)
		{
			if (damageAnim != 0)
			{
				portrait.RaiseAnimationEvent(damageAnim);
			}
		}
		else if (damage < 0f && healAnim != 0)
		{
			portrait.RaiseAnimationEvent(healAnim);
		}
		Finish();
	}
}
