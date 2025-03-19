using System.Collections.Generic;
using UnityEngine;

public abstract class MouseOverLinkSpell : Spell
{
	public Spell m_targetSpellFX;

	private List<Spell> m_spells;

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		StartCoroutine("SetupTargetsAndPlay");
	}

	protected override void OnNone(SpellStateType prevStateType)
	{
		if (m_spells != null)
		{
			foreach (Spell spell in m_spells)
			{
				spell.Deactivate();
				SpellManager.Get().ReleaseSpell(spell);
			}
			m_spells.Clear();
			m_spells = null;
		}
		base.OnNone(prevStateType);
	}

	protected abstract void GetAllTargets(Entity source, List<GameObject> targets);

	private void SetupTargetsAndPlay()
	{
		SetupTargets();
		if (m_targets.Count == 0)
		{
			OnSpellFinished();
		}
		else
		{
			PlaySpells();
		}
	}

	private void SetupTargets()
	{
		m_targets.Clear();
		Card source = GetSourceCard();
		if (!(source == null))
		{
			Entity entity = source.GetEntity();
			if (entity != null)
			{
				GetAllTargets(entity, m_targets);
			}
		}
	}

	private void PlaySpells()
	{
		m_spells = new List<Spell>();
		foreach (GameObject target in m_targets)
		{
			Card card = target.GetComponent<Card>();
			Spell spell = SpellManager.Get().GetSpell(m_targetSpellFX);
			SpellUtils.SetCustomSpellParent(spell, card.GetActor());
			spell.SetSource(card.gameObject);
			spell.Activate();
			m_spells.Add(spell);
		}
	}
}
