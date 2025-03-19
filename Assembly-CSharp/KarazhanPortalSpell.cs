using System.Collections.Generic;
using UnityEngine;

public class KarazhanPortalSpell : IdleSuperSpell
{
	public Spell m_customSpawnSpell;

	private bool m_waitForSpawnSpell;

	private Spell m_spawnSpellInstance;

	private Card m_spawnedMinion;

	private bool m_willSummonAMinion;

	public KarazhanPortalSpell()
	{
		m_playIdleSpellWithoutTargets = true;
	}

	protected override void DoActionPreTasks()
	{
		m_willSummonAMinion = false;
		if (m_spawnedMinion == null)
		{
			m_spawnedMinion = GetSpawnedMinion();
			if (m_spawnedMinion != null)
			{
				m_waitForSpawnSpell = true;
				m_spawnSpellInstance = SpellManager.Get().GetSpell(m_customSpawnSpell);
				m_spawnSpellInstance.AddSpellEventCallback(OnSpawnSpellEvent);
				m_spawnedMinion.OverrideCustomSpawnSpell(m_spawnSpellInstance);
				m_willSummonAMinion = true;
			}
		}
	}

	protected override void DoActionPostTasks()
	{
		if (m_willSummonAMinion)
		{
			SuppressDeathSoundsOnKilledTargets();
		}
	}

	protected override bool HasPendingTasks()
	{
		return m_waitForSpawnSpell;
	}

	private void SuppressDeathSoundsOnKilledTargets()
	{
		List<Entity> entityTargetList = new List<Entity>();
		foreach (GameObject target in GetVisualTargets())
		{
			if (!(target == null))
			{
				Card targetCard = target.GetComponent<Card>();
				entityTargetList.Add(targetCard.GetEntity());
			}
		}
		foreach (Entity entitiesKilledBySourceAmongstTarget in GameUtils.GetEntitiesKilledBySourceAmongstTargets(GetSourceCard().GetEntity().GetEntityId(), entityTargetList))
		{
			entitiesKilledBySourceAmongstTarget.GetCard().SuppressDeathSounds(suppress: true);
		}
	}

	public void OnSpawnSpellEvent(string eventName, object eventData, object userData)
	{
		if (eventName == "ClosePortal")
		{
			m_waitForSpawnSpell = false;
			OnSpellFinished();
		}
	}

	private Card GetSpawnedMinion()
	{
		for (int i = 0; i < m_taskList.GetTaskList().Count; i++)
		{
			Network.PowerHistory power = m_taskList.GetTaskList()[i].GetPower();
			if (power.Type != Network.PowerType.FULL_ENTITY)
			{
				continue;
			}
			int entID = (power as Network.HistFullEntity).Entity.ID;
			Entity entity = GameState.Get().GetEntity(entID);
			if (entity.GetTag(GAME_TAG.ZONE) != 6 && entity != null)
			{
				Card card = entity.GetCard();
				if (!(card == null))
				{
					return card;
				}
			}
		}
		return null;
	}
}
