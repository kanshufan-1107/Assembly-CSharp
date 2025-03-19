using System.Collections;
using System.Collections.Generic;
using PegasusGame;
using UnityEngine;

public class HoldDrawnCardsSpell : SuperSpell
{
	public float m_PreEffectHoldTime;

	public float m_PostEffectHoldTime;

	public Spell m_DrawnCardSpell;

	private SortedList<int, Card> m_drawCardData = new SortedList<int, Card>();

	private List<Spell> m_drawnCardSpellInstances = new List<Spell>();

	public override bool AttachPowerTaskList(PowerTaskList taskList)
	{
		if (!base.AttachPowerTaskList(taskList))
		{
			return false;
		}
		FindHoldDrawnCardMetaDataTasks();
		return true;
	}

	private void FindHoldDrawnCardMetaDataTasks()
	{
		m_drawCardData.Clear();
		if (m_taskList == null)
		{
			return;
		}
		List<PowerTask> tasks = m_taskList.GetTaskList();
		for (int taskIndex = 0; taskIndex < tasks.Count; taskIndex++)
		{
			if (!(tasks[taskIndex].GetPower() is Network.HistMetaData { MetaType: HistoryMeta.Type.HOLD_DRAWN_CARD } metaData) || metaData.Info.Count != 1)
			{
				continue;
			}
			Entity entity = GameState.Get().GetEntity(metaData.Info[0]);
			if (entity != null)
			{
				Card card = entity.GetCard();
				if (!(card == null))
				{
					m_drawCardData.Add(taskIndex, card);
				}
			}
		}
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_effectsPendingFinish++;
		base.OnAction(prevStateType);
		StartCoroutine(DrawCardsWithEffects());
	}

	private IEnumerator DrawCardsWithEffects()
	{
		int drawnCardIndex = 0;
		while (drawnCardIndex < m_drawCardData.Count)
		{
			int holdDrawMetaDataTaskIndex = m_drawCardData.Keys[drawnCardIndex];
			Card drawnCard = m_drawCardData.Values[drawnCardIndex];
			if (TurnStartManager.Get().IsCardDrawHandled(drawnCard))
			{
				TurnStartManager.Get().DrawCardImmediately(drawnCard);
			}
			bool complete = false;
			m_taskList.DoTasks(0, holdDrawMetaDataTaskIndex + 1, delegate
			{
				complete = true;
			});
			while (!complete)
			{
				yield return null;
			}
			m_taskList.GetTaskList()[holdDrawMetaDataTaskIndex].SetCompleted(complete: false);
			while (!drawnCard.IsActorReady())
			{
				yield return null;
			}
			yield return new WaitForSeconds(m_PreEffectHoldTime);
			if (m_DrawnCardSpell != null)
			{
				Spell spell = SpellManager.Get().GetSpell(m_DrawnCardSpell);
				SpellUtils.SetCustomSpellParent(spell, this);
				spell.SetSource(GetSource());
				spell.AddTarget(drawnCard.gameObject);
				spell.Activate();
				m_drawnCardSpellInstances.Add(spell);
			}
			int numTasksToComplete = m_taskList.GetTaskList().Count;
			if (drawnCardIndex + 1 < m_drawCardData.Count)
			{
				numTasksToComplete = m_drawCardData.Keys[drawnCardIndex + 1] - holdDrawMetaDataTaskIndex - 1;
			}
			m_taskList.DoTasks(holdDrawMetaDataTaskIndex + 1, numTasksToComplete);
			yield return new WaitForSeconds(m_PostEffectHoldTime);
			m_taskList.GetTaskList()[holdDrawMetaDataTaskIndex].SetCompleted(complete: true);
			int num = drawnCardIndex + 1;
			drawnCardIndex = num;
		}
		foreach (Spell spell2 in m_drawnCardSpellInstances)
		{
			if (!(spell2 == null))
			{
				while (!spell2.CanPurge())
				{
					yield return null;
				}
				SpellUtils.PurgeSpell(spell2);
			}
		}
		m_drawnCardSpellInstances.Clear();
		m_effectsPendingFinish--;
		FinishIfPossible();
	}
}
