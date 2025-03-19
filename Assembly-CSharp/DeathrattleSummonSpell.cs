using UnityEngine;

public class DeathrattleSummonSpell : Spell
{
	protected override Card GetTargetCardFromPowerTask(int index, PowerTask task)
	{
		Network.PowerHistory power = task.GetPower();
		if (power.Type != Network.PowerType.FULL_ENTITY)
		{
			return null;
		}
		Network.Entity netEnt = ((Network.HistFullEntity)power).Entity;
		Entity entity = GameState.Get().GetEntity(netEnt.ID);
		if (entity == null)
		{
			Debug.LogWarning($"{this}.GetTargetCardFromPowerTask() - WARNING trying to target entity with id {netEnt.ID} but there is no entity with that id");
			return null;
		}
		return entity.GetCard();
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		Card sourceCard = GetSourceCard();
		foreach (GameObject target in m_targets)
		{
			Card component = target.GetComponent<Card>();
			component.transform.position = sourceCard.transform.position;
			float START_SCALE = 0.2f;
			component.transform.localScale = new Vector3(START_SCALE, START_SCALE, START_SCALE);
			component.SetTransitionStyle(ZoneTransitionStyle.VERY_SLOW);
			component.SetDoNotWarpToNewZone(on: true);
		}
		base.OnBirth(prevStateType);
		OnSpellFinished();
	}
}
