using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Stores the value of an Entity's tag in passed int.")]
[ActionCategory("Pegasus")]
public class GetEntityTagValueAction : SpellAction
{
	public FsmOwnerDefault m_spellObject;

	public Which m_whichEntity;

	public GAME_TAG m_tagToCheck;

	[RequiredField]
	[UIHint(UIHint.Variable)]
	[Tooltip("Output variable.")]
	public FsmInt m_TagValue;

	protected override GameObject GetSpellOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_spellObject);
	}

	public override void Reset()
	{
		m_TagValue = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_TagValue == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - No variable hooked up to store tag value!", this);
			Finish();
			return;
		}
		Entity ent = GetEntity(m_whichEntity);
		if (ent == null)
		{
			global::Log.All.PrintError("{0}.OnEnter() - FAILED to find relevant entity: \"{1}\"", this, m_whichEntity);
			Finish();
		}
		else if (!PlayMakerUtils.CanPlaymakerGetTag(m_tagToCheck))
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - Playmaker should not check value of tag {1}", this, m_tagToCheck);
			Finish();
		}
		else
		{
			m_TagValue.Value = ent.GetTag(m_tagToCheck);
			Finish();
		}
	}
}
