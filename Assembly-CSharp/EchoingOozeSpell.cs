using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoingOozeSpell : Spell
{
	public Spell m_CustomSpawnSpell;

	public float m_PostSpawnDelayMin;

	public float m_PostSpawnDelayMax;

	protected override Card GetTargetCardFromPowerTask(int index, PowerTask task)
	{
		if (!(task.GetPower() is Network.HistFullEntity { Entity: var netEnt }))
		{
			return null;
		}
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
		base.OnAction(prevStateType);
		Card targetCard = GetTargetCard();
		if (targetCard == null)
		{
			OnStateFinished();
		}
		else
		{
			DoEffect(targetCard);
		}
	}

	private void DoEffect(Card targetCard)
	{
		Spell customSpawnSpell = SpellManager.Get().GetSpell(m_CustomSpawnSpell);
		targetCard.OverrideCustomSpawnSpell(customSpawnSpell);
		DoTasksUntilSpawn(targetCard);
		StartCoroutine(WaitThenFinish());
	}

	private void DoTasksUntilSpawn(Card targetCard)
	{
		int targetEntityId = targetCard.GetEntity().GetEntityId();
		List<PowerTask> taskList = m_taskList.GetTaskList();
		int spawnTaskIndex = 0;
		for (int i = 0; i < taskList.Count; i++)
		{
			if (taskList[i].GetPower() is Network.HistFullEntity fullEntity && fullEntity.Entity.ID == targetEntityId)
			{
				spawnTaskIndex = i;
				break;
			}
		}
		m_taskList.DoTasks(0, spawnTaskIndex + 1);
	}

	private IEnumerator WaitThenFinish()
	{
		float delaySec = Random.Range(m_PostSpawnDelayMin, m_PostSpawnDelayMax);
		if (!Mathf.Approximately(delaySec, 0f))
		{
			yield return new WaitForSeconds(delaySec);
		}
		OnStateFinished();
	}
}
