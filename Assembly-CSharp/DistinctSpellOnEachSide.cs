using UnityEngine;

public class DistinctSpellOnEachSide : Spell
{
	public Spell m_FriendlySideSpell;

	public Spell m_OpponentSideSpell;

	private Spell m_oneSideSpell;

	private bool InitSpell()
	{
		Card card = GetSourceCard();
		if (card == null)
		{
			return false;
		}
		Spell spell = ((card.GetControllerSide() == Player.Side.FRIENDLY) ? m_FriendlySideSpell : m_OpponentSideSpell);
		m_oneSideSpell = SpellManager.Get().GetSpell(spell);
		m_oneSideSpell.SetSource(card.gameObject);
		return true;
	}

	public override bool AttachPowerTaskList(PowerTaskList taskList)
	{
		if (!InitSpell())
		{
			return false;
		}
		if (!base.AttachPowerTaskList(taskList))
		{
			return false;
		}
		return m_oneSideSpell.AttachPowerTaskList(taskList);
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		m_oneSideSpell.AddFinishedCallback(OnOneSideSpellFinished);
		m_oneSideSpell.AddStateFinishedCallback(OnOneSideSpellStateFinished);
		m_oneSideSpell.ActivateState(SpellStateType.ACTION);
	}

	private void OnOneSideSpellFinished(Spell spell, object userData)
	{
		OnSpellFinished();
	}

	private void OnOneSideSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		Object.Destroy(m_oneSideSpell.gameObject);
		m_oneSideSpell = null;
		Deactivate();
	}
}
