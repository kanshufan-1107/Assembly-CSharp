using Blizzard.T5.Game.Spells;
using UnityEngine;

public class FourHorsemenSpell : SuperSpell
{
	public SuperSpell m_MissileSpell;

	public Spell m_DeathSpell;

	private int m_missilesWAitingToFinish;

	protected override void OnAction(SpellStateType prevStateType)
	{
		if (m_targets.Count <= 0)
		{
			OnSpellFinished();
			OnStateFinished();
		}
		else
		{
			m_effectsPendingFinish++;
			base.OnAction(prevStateType);
			FireMissileVolley();
		}
	}

	private void FireMissileVolley()
	{
		if (m_MissileSpell != null)
		{
			for (int i = 0; i < m_visualTargets.Count; i++)
			{
				m_missilesWAitingToFinish++;
				FireSingleMissile(i);
			}
		}
	}

	private void FireSingleMissile(int targetIndex)
	{
		m_effectsPendingFinish++;
		SuperSpell spell = (SuperSpell)CloneSpell(m_MissileSpell, null);
		GameObject sourceObject = m_visualTargets[targetIndex];
		GameObject targetObject = SpellUtils.GetSpellLocationObject(this, SpellLocation.OPPONENT_HERO);
		spell.SetSource(sourceObject);
		spell.AddTarget(targetObject);
		if (targetIndex > 0)
		{
			spell.RemoveImpactInfo();
		}
		spell.AddFinishedCallback(OnMissileFinished);
		spell.ActivateState(SpellStateType.ACTION);
	}

	private void OnMissileFinished(Spell spell, object userData)
	{
		m_missilesWAitingToFinish--;
		DoFinalImpactIfPossible();
	}

	protected void DoFinalImpactIfPossible()
	{
		if (m_missilesWAitingToFinish <= 0)
		{
			ISpell spell = CloneSpell(m_DeathSpell, null);
			GameObject sourceObject = SpellUtils.GetSpellLocationObject(this, SpellLocation.OPPONENT_HERO);
			spell.SetSource(sourceObject);
			if (spell is Spell s)
			{
				s.AddFinishedCallback(OnDeathFinished);
			}
			spell.Activate();
		}
	}

	private void OnDeathFinished(Spell spell, object userData)
	{
		m_effectsPendingFinish--;
		FinishIfPossible();
	}
}
