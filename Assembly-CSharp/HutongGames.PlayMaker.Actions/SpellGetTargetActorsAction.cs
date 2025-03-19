using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Get the GameObjects for each Card added as Target for a Spell.")]
[ActionCategory("Pegasus")]
public class SpellGetTargetActorsAction : SpellAction
{
	public FsmOwnerDefault m_SpellObject;

	[ArrayEditor(VariableType.GameObject, "", 0, 0, 65536)]
	public FsmArray m_CardActorGameObjects;

	protected override GameObject GetSpellOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_SpellObject);
	}

	public override void Reset()
	{
		m_SpellObject = null;
		m_CardActorGameObjects = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Spell spell = GetSpell();
		if (spell == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter(): Unable to find Spell.", this);
			Finish();
		}
		else
		{
			FsmArray cardActorGameObjects = m_CardActorGameObjects;
			object[] values = spell.GetVisualTargets().ToArray();
			cardActorGameObjects.Values = values;
			Finish();
		}
	}
}
