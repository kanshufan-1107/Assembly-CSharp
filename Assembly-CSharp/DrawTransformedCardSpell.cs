using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawTransformedCardSpell : SuperSpell
{
	public float m_OldCardHoldTime;

	public float m_NewCardHoldTime;

	public bool m_FriendlyOnly;

	private int m_transformTaskIndex;

	public override bool AddPowerTargets()
	{
		base.AddPowerTargets();
		return FindTransformTask();
	}

	private bool FindTransformTask()
	{
		List<PowerTask> tasks = m_taskList.GetTaskList();
		for (int i = 0; i < tasks.Count; i++)
		{
			Network.PowerHistory power = tasks[i].GetPower();
			if (power.Type != Network.PowerType.CHANGE_ENTITY)
			{
				continue;
			}
			Network.HistChangeEntity changeEntity = (Network.HistChangeEntity)power;
			Entity changedEntity = GameState.Get().GetEntity(changeEntity.Entity.ID);
			if (changedEntity != null)
			{
				Card changedCard = changedEntity.GetCard();
				if (!(changedCard == null) && (!m_FriendlyOnly || changedCard.GetEntity().IsControlledByFriendlySidePlayer()))
				{
					m_transformTaskIndex = i;
					AddTarget(changedCard.gameObject);
					return true;
				}
			}
		}
		return false;
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		StartCoroutine(DoTasksBeforeTransform());
		StartCoroutine(DoEffectWithTiming());
	}

	private IEnumerator DoTasksBeforeTransform()
	{
		bool complete = false;
		m_taskList.DoTasks(0, m_transformTaskIndex, delegate
		{
			complete = true;
		});
		while (!complete)
		{
			yield return null;
		}
	}

	private IEnumerator DoEffectWithTiming()
	{
		yield return new WaitForSeconds(m_OldCardHoldTime);
		bool complete = false;
		m_taskList.DoTasks(m_transformTaskIndex, 1, delegate
		{
			complete = true;
		});
		while (!complete)
		{
			yield return null;
		}
		PowerTask transformTask = m_taskList.GetTaskList()[m_transformTaskIndex];
		transformTask.SetCompleted(complete: false);
		yield return new WaitForSeconds(m_NewCardHoldTime);
		transformTask.SetCompleted(complete: true);
		OnSpellFinished();
	}
}
