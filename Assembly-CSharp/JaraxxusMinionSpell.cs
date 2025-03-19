using System.Collections;
using UnityEngine;

public class JaraxxusMinionSpell : Spell
{
	public float m_MoveToLocationDelay;

	public float m_MoveToLocationDuration = 1.5f;

	public iTween.EaseType m_MoveToLocationEaseType = iTween.EaseType.linear;

	public float m_MoveToHeroSpotDelay = 3.5f;

	public float m_MoveToHeroSpotDuration = 0.3f;

	public iTween.EaseType m_MoveToHeroSpotEaseType = iTween.EaseType.linear;

	public override bool AddPowerTargets()
	{
		foreach (PowerTask task in m_taskList.GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type == Network.PowerType.FULL_ENTITY)
			{
				int entityId = (power as Network.HistFullEntity).Entity.ID;
				Entity entity = GameState.Get().GetEntity(entityId);
				if (entity == null)
				{
					Debug.LogWarning($"{this}.AddPowerTargets() - WARNING encountered HistFullEntity where entity id={entityId} but there is no entity with that id");
					return false;
				}
				if (!entity.IsHero())
				{
					Debug.LogWarning($"{this}.AddPowerTargets() - WARNING HistFullEntity where entity id={entityId} is not a hero");
					return false;
				}
				AddTarget(entity.GetCard().gameObject);
				return true;
			}
		}
		return false;
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		StartCoroutine(SetupHero());
	}

	private IEnumerator SetupHero()
	{
		Card minionCard = GetSourceCard();
		Card heroCard = GetTargetCard();
		Entity heroEntity = heroCard.GetEntity();
		minionCard.SuppressDeathEffects(suppress: true);
		minionCard.GetActor().TurnOffCollider();
		FindFullEntityTask().DoTask();
		while (heroEntity.IsLoadingAssets())
		{
			yield return null;
		}
		heroCard.HideCard();
		Zone heroZone = ZoneMgr.Get().FindZoneForEntity(heroEntity);
		heroCard.TransitionToZone(heroZone);
		while (heroCard.IsActorLoading())
		{
			yield return null;
		}
		heroCard.GetActor().TurnOffCollider();
		StartCoroutine(PlaySummoningSpells(minionCard, heroCard));
	}

	private PowerTask FindFullEntityTask()
	{
		foreach (PowerTask task in m_taskList.GetTaskList())
		{
			if (task.GetPower().Type == Network.PowerType.FULL_ENTITY)
			{
				return task;
			}
		}
		return null;
	}

	private void Finish()
	{
		GetSourceCard().GetActor().TurnOnCollider();
		Card targetCard = GetTargetCard();
		targetCard.GetActor().TurnOnCollider();
		targetCard.ActivateStateSpells();
		targetCard.ShowCard();
		OnSpellFinished();
	}

	private IEnumerator PlaySummoningSpells(Card minionCard, Card heroCard)
	{
		heroCard.transform.position = minionCard.transform.position;
		minionCard.ActivateActorSpell(SpellType.SUMMON_JARAXXUS);
		heroCard.ActivateActorSpell(SpellType.SUMMON_JARAXXUS);
		yield return new WaitForSeconds(m_MoveToLocationDelay);
		MoveToSpellLocation(minionCard, heroCard);
		yield return new WaitForSeconds(m_MoveToHeroSpotDelay);
		MoveToHeroSpot(minionCard, heroCard);
	}

	private void MoveToSpellLocation(Card minionCard, Card heroCard)
	{
		Hashtable minionMoveArgTable = iTweenManager.Get().GetTweenHashTable();
		minionMoveArgTable.Add("position", base.transform.position);
		minionMoveArgTable.Add("time", m_MoveToLocationDuration);
		minionMoveArgTable.Add("easetype", m_MoveToLocationEaseType);
		iTween.MoveTo(minionCard.gameObject, minionMoveArgTable);
		Hashtable heroMoveArgTable = iTweenManager.Get().GetTweenHashTable();
		heroMoveArgTable.Add("position", base.transform.position);
		heroMoveArgTable.Add("time", m_MoveToLocationDuration);
		heroMoveArgTable.Add("easetype", m_MoveToLocationEaseType);
		iTween.MoveTo(heroCard.gameObject, heroMoveArgTable);
	}

	private void MoveToHeroSpot(Card minionCard, Card heroCard)
	{
		ZoneHero heroZone = heroCard.GetController().GetHeroZone();
		Hashtable minionMoveArgTable = iTweenManager.Get().GetTweenHashTable();
		minionMoveArgTable.Add("position", heroZone.transform.position);
		minionMoveArgTable.Add("time", m_MoveToHeroSpotDuration);
		minionMoveArgTable.Add("easetype", m_MoveToHeroSpotEaseType);
		iTween.MoveTo(minionCard.gameObject, minionMoveArgTable);
		Hashtable heroMoveArgTable = iTweenManager.Get().GetTweenHashTable();
		heroMoveArgTable.Add("position", heroZone.transform.position);
		heroMoveArgTable.Add("time", m_MoveToHeroSpotDuration);
		heroMoveArgTable.Add("easetype", m_MoveToHeroSpotEaseType);
		heroMoveArgTable.Add("oncomplete", "OnMoveToHeroSpotComplete");
		heroMoveArgTable.Add("oncompletetarget", base.gameObject);
		iTween.MoveTo(heroCard.gameObject, heroMoveArgTable);
	}

	private void OnMoveToHeroSpotComplete()
	{
		Finish();
	}
}
