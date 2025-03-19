using System.Collections;
using UnityEngine;

public class JaraxxusHeroSpell : Spell
{
	private PowerTask m_heroPowerTask;

	private PowerTask m_weaponTask;

	public override bool AddPowerTargets()
	{
		foreach (PowerTask task in m_taskList.GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type != Network.PowerType.FULL_ENTITY)
			{
				continue;
			}
			int entityId = (power as Network.HistFullEntity).Entity.ID;
			Entity entity = GameState.Get().GetEntity(entityId);
			if (entity == null)
			{
				Debug.LogWarning($"{this}.AddPowerTargets() - WARNING encountered HistFullEntity where entity id={entityId} but there is no entity with that id");
				return false;
			}
			if (entity.IsHeroPower())
			{
				m_heroPowerTask = task;
				AddTarget(entity.GetCard().gameObject);
				if (m_weaponTask != null)
				{
					return true;
				}
			}
			else if (entity.IsWeapon())
			{
				m_weaponTask = task;
				AddTarget(entity.GetCard().gameObject);
				if (m_heroPowerTask != null)
				{
					return true;
				}
			}
		}
		Reset();
		return false;
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		StartCoroutine(SetupCards());
	}

	private IEnumerator SetupCards()
	{
		Entity heroPower = LoadCardFromTask(m_heroPowerTask);
		Entity weapon = LoadCardFromTask(m_weaponTask);
		while (heroPower.IsLoadingAssets() || weapon.IsLoadingAssets())
		{
			yield return null;
		}
		Card heroPowerCard = heroPower.GetCard();
		heroPowerCard.HideCard();
		Zone heroPowerZone = ZoneMgr.Get().FindZoneForEntity(heroPower);
		heroPowerCard.TransitionToZone(heroPowerZone);
		Card weaponCard = weapon.GetCard();
		weaponCard.HideCard();
		Zone weaponZone = ZoneMgr.Get().FindZoneForEntity(weapon);
		weaponCard.TransitionToZone(weaponZone);
		while (heroPowerCard.IsActorLoading() || weaponCard.IsActorLoading())
		{
			yield return null;
		}
		PlayCardSpells(heroPowerCard, weaponCard);
	}

	private Entity LoadCardFromTask(PowerTask task)
	{
		Network.Entity netEnt = (task.GetPower() as Network.HistFullEntity).Entity;
		int entityId = netEnt.ID;
		Entity entity = GameState.Get().GetEntity(entityId);
		entity.LoadCard(netEnt.CardID);
		return entity;
	}

	private Card GetCardFromTask(PowerTask task)
	{
		int entityId = (task.GetPower() as Network.HistFullEntity).Entity.ID;
		return GameState.Get().GetEntity(entityId).GetCard();
	}

	private void Reset()
	{
		m_heroPowerTask = null;
		m_weaponTask = null;
	}

	private void Finish()
	{
		Reset();
		OnSpellFinished();
	}

	private void PlayCardSpells(Card heroPowerCard, Card weaponCard)
	{
		heroPowerCard.ShowCard();
		heroPowerCard.ActivateStateSpells();
		heroPowerCard.ActivateActorSpell(SpellType.SUMMON_JARAXXUS, OnSpellFinished_HeroPower);
		weaponCard.ActivateActorSpell(SpellType.SUMMON_JARAXXUS, OnSpellFinished_Weapon);
	}

	private void OnSpellFinished_HeroPower(Spell spell, object userData)
	{
		m_heroPowerTask.SetCompleted(complete: true);
		if (m_weaponTask.IsCompleted())
		{
			Finish();
		}
	}

	private void OnSpellFinished_Weapon(Spell spell, object userData)
	{
		Card cardFromTask = GetCardFromTask(m_weaponTask);
		cardFromTask.ShowCard();
		cardFromTask.ActivateStateSpells();
		m_weaponTask.SetCompleted(complete: true);
		if (m_heroPowerTask.IsCompleted())
		{
			Finish();
		}
	}
}
