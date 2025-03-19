using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Returns if this spell object has an entity.")]
public class CheckHasEntityAction : SpellAction
{
	public FsmOwnerDefault m_spellObject;

	public Which m_whichEntity;

	[Tooltip("Output variable.")]
	[UIHint(UIHint.Variable)]
	[RequiredField]
	public FsmBool m_HasEntity;

	protected override GameObject GetSpellOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_spellObject);
	}

	public override void Reset()
	{
		m_HasEntity = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Entity ent = GetEntity(m_whichEntity);
		m_HasEntity.Value = ent != null;
		Finish();
	}
}
