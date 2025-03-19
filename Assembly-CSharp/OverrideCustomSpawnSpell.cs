using Blizzard.T5.Game.Spells;
using Blizzard.T5.Game.Spells.SuperSpells;
using UnityEngine;

public class OverrideCustomSpawnSpell : SuperSpell, ISuperSpell, ISpell
{
	[SerializeField]
	private Spell m_CustomSpawnSpell;

	public ISpell CustomSpawnSpell => m_CustomSpawnSpell;

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_effectsPendingFinish++;
		base.OnAction(prevStateType);
		if (m_CustomSpawnSpell == null)
		{
			Debug.LogError("OverrideCustomSpawnSpell.OverrideCustomSpawnSpell in null!");
			m_effectsPendingFinish--;
			FinishIfPossible();
			return;
		}
		foreach (GameObject target in GetVisualTargets())
		{
			if (!(target == null))
			{
				target.GetComponent<Card>().OverrideCustomSpawnSpell(SpellManager.Get().GetSpell(m_CustomSpawnSpell));
			}
		}
		m_effectsPendingFinish--;
		FinishIfPossible();
	}
}
