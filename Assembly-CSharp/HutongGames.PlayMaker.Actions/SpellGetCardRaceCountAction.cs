using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Stores the number of races for a card in passed int.")]
[ActionCategory("Pegasus")]
public class SpellGetCardRaceCountAction : SpellAction
{
	public FsmOwnerDefault m_SpellObject;

	public Which m_WhichCard;

	[UIHint(UIHint.Variable)]
	[Tooltip("Output variable.")]
	[RequiredField]
	public FsmInt m_RaceCount;

	protected override GameObject GetSpellOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_SpellObject);
	}

	public override void Reset()
	{
		m_SpellObject = null;
		m_WhichCard = Which.SOURCE;
		m_RaceCount = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Card card = GetCard(m_WhichCard);
		if (card == null)
		{
			Error.AddDevFatal("SpellGetCardRaceCountAction.OnEnter() - Card not found!");
			Finish();
			return;
		}
		Entity entity = card.GetEntity();
		if (card == null)
		{
			Error.AddDevFatal("SpellGetCardRaceCountAction.OnEnter() - Entity not found!");
			Finish();
		}
		else
		{
			m_RaceCount.Value = entity.GetRaceCount();
			Finish();
		}
	}
}
