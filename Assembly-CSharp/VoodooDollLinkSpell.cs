using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoodooDollLinkSpell : Spell
{
	public Spell m_VooDooFX;

	private List<Spell> m_voodooSpells;

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		StartCoroutine(SetupTargetsAndPlay());
	}

	protected override void OnNone(SpellStateType prevStateType)
	{
		if (m_voodooSpells != null)
		{
			foreach (Spell spell in m_voodooSpells)
			{
				spell.Deactivate();
				SpellManager.Get().ReleaseSpell(spell);
			}
			m_voodooSpells.Clear();
			m_voodooSpells = null;
		}
		base.OnNone(prevStateType);
	}

	private IEnumerator SetupTargetsAndPlay()
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
		yield break;
	}

	private void SetupTargets()
	{
		m_targets.Clear();
		Card source = GetSourceCard();
		if (source == null)
		{
			return;
		}
		Entity entity = source.GetEntity();
		if (entity.HasTag(GAME_TAG.VOODOO_LINK))
		{
			Entity target = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.VOODOO_LINK));
			if (target != null && target.GetCard() != null)
			{
				m_targets.Add(source.gameObject);
				m_targets.Add(target.GetCard().gameObject);
			}
			return;
		}
		foreach (Entity enchant in entity.GetAttachments())
		{
			if (enchant.HasTag(GAME_TAG.VOODOO_LINK))
			{
				Entity voodooDoll = GameState.Get().GetEntity(enchant.GetTag(GAME_TAG.VOODOO_LINK));
				if (voodooDoll != null && voodooDoll.GetCard() != null)
				{
					m_targets.Add(voodooDoll.GetCard().gameObject);
				}
			}
		}
		if (m_targets.Count > 0)
		{
			m_targets.Add(source.gameObject);
		}
	}

	private void PlaySpells()
	{
		m_voodooSpells = new List<Spell>();
		foreach (GameObject target in m_targets)
		{
			Card card = target.GetComponent<Card>();
			Spell voodooSpell = SpellManager.Get().GetSpell(m_VooDooFX);
			SpellUtils.SetCustomSpellParent(voodooSpell, card.GetActor());
			voodooSpell.SetSource(card.gameObject);
			voodooSpell.Activate();
			m_voodooSpells.Add(voodooSpell);
		}
	}
}
