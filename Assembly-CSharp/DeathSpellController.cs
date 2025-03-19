using System.Collections.Generic;
using UnityEngine;

public class DeathSpellController : SpellController
{
	protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
	{
		AddDeadCardsToTargetList(taskList);
		if (m_targets.Count == 0)
		{
			return false;
		}
		return true;
	}

	protected override void OnProcessTaskList()
	{
		int deathSoundCardIndex = PickDeathSoundCardIndex();
		for (int i = 0; i < m_targets.Count; i++)
		{
			Card targetCard = m_targets[i];
			if (i != deathSoundCardIndex)
			{
				targetCard.SuppressDeathSounds(suppress: true);
			}
			targetCard.ActivateCharacterDeathEffects();
		}
		base.OnProcessTaskList();
	}

	private void AddDeadCardsToTargetList(PowerTaskList taskList)
	{
		List<PowerTask> tasks = m_taskList.GetTaskList();
		for (int i = 0; i < tasks.Count; i++)
		{
			Network.PowerHistory power = tasks[i].GetPower();
			if (power.Type != Network.PowerType.TAG_CHANGE)
			{
				continue;
			}
			Network.HistTagChange tagChange = power as Network.HistTagChange;
			if (GameUtils.IsCharacterDeathTagChange(tagChange))
			{
				Entity entity = GameState.Get().GetEntity(tagChange.Entity);
				Card card = entity.GetCard();
				if (CanAddTarget(entity, card))
				{
					AddTarget(card);
				}
			}
		}
	}

	private bool CanAddTarget(Entity entity, Card card)
	{
		if (card.WillSuppressDeathEffects())
		{
			return false;
		}
		return true;
	}

	private int PickDeathSoundCardIndex()
	{
		if (m_targets.Count == 1)
		{
			Entity entity = m_targets[0].GetEntity();
			if (CanPlayDeathSound(entity))
			{
				return 0;
			}
			return -1;
		}
		if (m_targets.Count == 2)
		{
			Card card0 = m_targets[0];
			Card card1 = m_targets[1];
			Entity entity2 = card0.GetEntity();
			Entity entity3 = card1.GetEntity();
			if (WasAttackedBy(entity2, entity3))
			{
				if (CanPlayDeathSound(entity2))
				{
					return 0;
				}
				return 1;
			}
			if (WasAttackedBy(entity3, entity2))
			{
				if (CanPlayDeathSound(entity3))
				{
					return 1;
				}
				return 0;
			}
		}
		return PickRandomDeathSoundCardIndex();
	}

	private bool WasAttackedBy(Entity defender, Entity attacker)
	{
		if (!attacker.HasTag(GAME_TAG.ATTACKING))
		{
			return false;
		}
		if (!defender.HasTag(GAME_TAG.DEFENDING))
		{
			return false;
		}
		if (defender.GetTag(GAME_TAG.LAST_AFFECTED_BY) != attacker.GetEntityId())
		{
			return false;
		}
		return true;
	}

	private int PickRandomDeathSoundCardIndex()
	{
		List<int> potentialCardIndexes = new List<int>();
		for (int i = 0; i < m_targets.Count; i++)
		{
			Entity targetEntity = m_targets[i].GetEntity();
			if (CanPlayDeathSound(targetEntity))
			{
				potentialCardIndexes.Add(i);
			}
		}
		if (potentialCardIndexes.Count == 0)
		{
			return -1;
		}
		return potentialCardIndexes[Random.Range(0, potentialCardIndexes.Count)];
	}

	private bool CanPlayDeathSound(Entity entity)
	{
		if (entity.HasTag(GAME_TAG.DEATHRATTLE_RETURN_ZONE))
		{
			return false;
		}
		if (entity.HasTag(GAME_TAG.SUPPRESS_DEATH_SOUND))
		{
			return false;
		}
		return true;
	}
}
