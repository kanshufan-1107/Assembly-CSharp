using UnityEngine;

public class SpellAreaEffect : Spell
{
	public Spell m_ImpactSpellPrefab;

	public override bool AddPowerTargets()
	{
		if (!CanAddPowerTargets())
		{
			return false;
		}
		if (!AddMultiplePowerTargets())
		{
			return false;
		}
		return GetTargets().Count > 0;
	}

	protected override void OnDeath(SpellStateType prevStateType)
	{
		base.OnDeath(prevStateType);
		if (!(m_ImpactSpellPrefab == null))
		{
			for (int i = 0; i < m_targets.Count; i++)
			{
				SpawnImpactSpell(m_targets[i]);
			}
		}
	}

	private void SpawnImpactSpell(GameObject targetObject)
	{
		Spell spell = SpellManager.Get().GetSpell(m_ImpactSpellPrefab);
		spell.transform.position = targetObject.transform.position;
		spell.AddStateFinishedCallback(OnImpactSpellStateFinished);
		spell.Activate();
	}

	private void OnImpactSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			Object.Destroy(spell.gameObject);
		}
	}
}
