using UnityEngine;

public class OverrideCustomDiscardSpell : SuperSpell
{
	public Spell m_CustomDiscardSpell;

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_effectsPendingFinish++;
		base.OnAction(prevStateType);
		foreach (GameObject target in GetVisualTargets())
		{
			if (!(target == null))
			{
				target.GetComponent<Card>().OverrideCustomDiscardSpell(SpellManager.Get().GetSpell(m_CustomDiscardSpell));
			}
		}
		m_effectsPendingFinish--;
		FinishIfPossible();
	}
}
