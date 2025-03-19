using UnityEngine;

public class OverrideCustomDeathSpell : SuperSpell
{
	public Spell m_CustomDeathSpell;

	public bool m_SuppressKeywordDeaths = true;

	public float m_KeywordDeathDelay = 0.3f;

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_effectsPendingFinish++;
		base.OnAction(prevStateType);
		foreach (GameObject target in GetVisualTargets())
		{
			if (!(target == null))
			{
				Card component = target.GetComponent<Card>();
				component.OverrideCustomDeathSpell(SpellManager.Get().GetSpell(m_CustomDeathSpell));
				component.SuppressKeywordDeaths(m_SuppressKeywordDeaths);
				component.SetKeywordDeathDelaySec(m_KeywordDeathDelay);
			}
		}
		m_effectsPendingFinish--;
		FinishIfPossible();
	}
}
