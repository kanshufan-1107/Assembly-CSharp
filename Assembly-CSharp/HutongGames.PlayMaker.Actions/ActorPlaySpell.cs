using Blizzard.T5.Core.Utils;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Play a spell on the given Actor")]
[ActionCategory("Pegasus")]
public class ActorPlaySpell : FsmStateAction
{
	public SpellType m_Spell;

	public FsmGameObject m_actorObject;

	public override void Reset()
	{
		m_Spell = SpellType.NONE;
		m_actorObject = null;
	}

	public override void OnEnter()
	{
		if (!m_actorObject.IsNone)
		{
			Actor actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(m_actorObject.Value);
			if (actor != null && m_Spell != 0)
			{
				actor.ActivateSpellBirthState(m_Spell);
			}
		}
		Finish();
	}

	public override void OnUpdate()
	{
	}
}
