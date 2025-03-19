using UnityEngine;

public class DoomedMinionSpell : SuperSpell
{
	public SpellType m_SpellType;

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_effectsPendingFinish++;
		base.OnAction(prevStateType);
		foreach (GameObject target in GetVisualTargets())
		{
			if (!(target == null))
			{
				target.GetComponent<Card>().ActivateActorSpell(m_SpellType);
			}
		}
		m_effectsPendingFinish--;
		FinishIfPossible();
	}
}
