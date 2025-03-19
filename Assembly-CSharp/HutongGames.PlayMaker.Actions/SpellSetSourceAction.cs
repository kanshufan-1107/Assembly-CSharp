using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Sets the source for a Spell.")]
[ActionCategory("Pegasus")]
public class SpellSetSourceAction : SpellAction
{
	public FsmOwnerDefault m_SpellObject;

	public FsmGameObject m_SourceObject;

	protected override GameObject GetSpellOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_SpellObject);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (!(m_spell == null))
		{
			GameObject go = m_SourceObject.Value;
			m_spell.SetSource(go);
			Finish();
		}
	}
}
