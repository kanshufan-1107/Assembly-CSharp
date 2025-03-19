using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus Audio")]
[Tooltip("Use this to manually play death VO for an actor. Created for custom death override effects.")]
public class AudioPlayActorDeathVOAction : SpellAction
{
	public FsmOwnerDefault m_SpellObject;

	public Which m_WhichActor;

	protected override GameObject GetSpellOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_SpellObject);
	}

	public override void Reset()
	{
		m_SpellObject = null;
		m_WhichActor = Which.SOURCE;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Actor actor = GetActor(m_WhichActor);
		if (actor == null)
		{
			Error.AddDevFatal("AudioPlayHeroDeathVOAction.OnEnter() - Actor not found!");
			Finish();
			return;
		}
		Card card = actor.GetCard();
		if (card == null)
		{
			Error.AddDevFatal("AudioPlayHeroDeathVOAction.OnEnter() - Card not found!");
			Finish();
		}
		else
		{
			card.ForceActivateDeathSoundSpells();
			Finish();
		}
	}
}
