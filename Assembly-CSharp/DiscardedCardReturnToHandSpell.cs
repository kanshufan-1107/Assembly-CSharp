using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardedCardReturnToHandSpell : Spell
{
	[SerializeField]
	private Spell m_TargetSpell;

	private Entity m_entityDiscarded;

	private List<Spell> m_activeTargetSpells = new List<Spell>();

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_entityDiscarded = m_taskList.GetSourceEntity(warnIfNull: false);
		base.OnAction(prevStateType);
		StartCoroutine(DoActionWithTiming());
	}

	private IEnumerator DoActionWithTiming()
	{
		ProcessShowEntityForTargets();
		yield return StartCoroutine(WaitAssetLoad());
		yield return StartCoroutine(PlayTargetSpells());
	}

	private void ProcessShowEntityForTargets()
	{
		foreach (PowerTask task in GetPowerTaskList().GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type != Network.PowerType.SHOW_ENTITY)
			{
				continue;
			}
			Network.Entity netEnt = (power as Network.HistShowEntity).Entity;
			Entity target = FindTargetEntity(netEnt.ID);
			if (target == null)
			{
				continue;
			}
			foreach (Network.Entity.Tag netTag in netEnt.Tags)
			{
				target.SetTag(netTag.Name, netTag.Value);
			}
		}
	}

	private Entity FindTargetEntity(int entityID)
	{
		foreach (GameObject target in m_targets)
		{
			Card card = target.GetComponent<Card>();
			if (!(card == null))
			{
				Entity entity = card.GetEntity();
				if (entity != null && entity.GetEntityId() == entityID)
				{
					return entity;
				}
			}
		}
		return null;
	}

	private IEnumerator WaitAssetLoad()
	{
		foreach (GameObject gameObject in m_targets)
		{
			Card card = gameObject.GetComponent<Card>();
			if (!(card == null))
			{
				string newCardID = m_entityDiscarded.GetCardId();
				EntityDef entityDef = DefLoader.Get().GetEntityDef(newCardID);
				card.GetEntity().LoadCard(newCardID);
				card.UpdateActor(forceIfNullZone: true, ActorNames.GetHandActor(entityDef, m_entityDiscarded.GetPremiumType()));
				while (card.IsActorLoading())
				{
					yield return null;
				}
				TransformUtil.CopyWorld(card, m_entityDiscarded.GetCard().transform);
				card.HideCard();
			}
		}
	}

	private IEnumerator PlayTargetSpells()
	{
		if (m_TargetSpell == null)
		{
			yield break;
		}
		foreach (GameObject target in m_targets)
		{
			Spell targetSpell = SpellManager.Get().GetSpell(m_TargetSpell);
			if (!(targetSpell == null))
			{
				m_activeTargetSpells.Add(targetSpell);
				TransformUtil.AttachAndPreserveLocalTransform(targetSpell.transform, target.transform);
				targetSpell.SetSource(target);
				targetSpell.AddFinishedCallback(OnSelectedSpellFinished);
				targetSpell.AddStateFinishedCallback(OnSelectedSpellStateFinished);
				targetSpell.Activate();
			}
		}
	}

	private void OnSelectedSpellFinished(Spell spell, object userData)
	{
		if (m_activeTargetSpells.Count == 0)
		{
			return;
		}
		foreach (Spell activeTargetSpell in m_activeTargetSpells)
		{
			if (!activeTargetSpell.IsFinished())
			{
				return;
			}
		}
		OnSpellFinished();
	}

	private void OnSelectedSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (m_activeTargetSpells.Count == 0)
		{
			return;
		}
		foreach (Spell activeTargetSpell in m_activeTargetSpells)
		{
			_ = activeTargetSpell;
			if (spell.GetActiveState() != 0)
			{
				return;
			}
		}
		foreach (Spell activeTargetSpell2 in m_activeTargetSpells)
		{
			Object.Destroy(activeTargetSpell2);
		}
		m_activeTargetSpells.Clear();
		OnStateFinished();
	}
}
