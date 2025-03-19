using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RafaamStaffOfOriginationSpell : Spell
{
	public Spell m_CustomSpawnSpell;

	private int m_spawnTaskIndex;

	public override bool AddPowerTargets()
	{
		if (!m_taskList.DoesBlockHaveMetaDataTasks())
		{
			return false;
		}
		m_spawnTaskIndex = -1;
		bool foundElectricChargeLevel = false;
		List<PowerTask> tasks = m_taskList.GetTaskList();
		for (int i = 0; i < tasks.Count; i++)
		{
			Network.PowerHistory power = tasks[i].GetPower();
			if (power is Network.HistTagChange { Tag: 420 })
			{
				foundElectricChargeLevel = true;
			}
			else if (power is Network.HistFullEntity fullEntity && foundElectricChargeLevel)
			{
				Card card = GameState.Get().GetEntity(fullEntity.Entity.ID).GetCard();
				if (!(card == null))
				{
					m_targets.Add(card.gameObject);
					m_spawnTaskIndex = i;
					break;
				}
			}
		}
		if (m_spawnTaskIndex < 0)
		{
			return false;
		}
		return true;
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		ApplyCustomSpawnOverride();
		DoTasksUntilSpawn();
	}

	private void ApplyCustomSpawnOverride()
	{
		foreach (GameObject target in m_targets)
		{
			Card component = target.GetComponent<Card>();
			Spell customSpawnSpell = SpellManager.Get().GetSpell(m_CustomSpawnSpell);
			component.OverrideCustomSpawnSpell(customSpawnSpell);
		}
	}

	private void DoTasksUntilSpawn()
	{
		PowerTaskList.CompleteCallback callback = delegate
		{
			StartCoroutine(WaitThenFinish());
		};
		m_taskList.DoTasks(0, m_spawnTaskIndex, callback);
	}

	private IEnumerator WaitThenFinish()
	{
		Network.HistFullEntity fullEntity = (Network.HistFullEntity)m_taskList.GetTaskList()[m_spawnTaskIndex].GetPower();
		Card heroPowerCard = GameState.Get().GetEntity(fullEntity.Entity.ID).GetHeroPowerCard();
		Spell electricSpell = heroPowerCard.GetActorSpell(SpellType.ELECTRIC_CHARGE_LEVEL_LARGE);
		while (!electricSpell.IsFinished())
		{
			yield return null;
		}
		OnStateFinished();
	}
}
