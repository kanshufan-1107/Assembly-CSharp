using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HypnotizeMoveHandToDeckSpell : SuperSpell
{
	public float m_MoveUpTime;

	public float m_MoveUpOffsetZ;

	public float m_MoveUpScale;

	public float m_MoveToDeckInterval;

	private List<Actor> m_friendlyActors = new List<Actor>();

	private List<Actor> m_opponentActors = new List<Actor>();

	public override bool AddPowerTargets()
	{
		m_visualToTargetIndexMap.Clear();
		m_targetToMetaDataMap.Clear();
		m_targets.Clear();
		List<PowerTask> tasks = m_taskList.GetTaskList();
		for (int i = 0; i < tasks.Count; i++)
		{
			PowerTask task = tasks[i];
			Card card = GetTargetCardFromPowerTask(i, task);
			if (!(card == null) && IsValidSpellTarget(card.GetEntity()))
			{
				AddTarget(card.gameObject);
			}
		}
		return true;
	}

	protected override Card GetTargetCardFromPowerTask(int index, PowerTask task)
	{
		Network.PowerHistory power = task.GetPower();
		if (power.Type != Network.PowerType.TAG_CHANGE)
		{
			return null;
		}
		Network.HistTagChange tagChange = power as Network.HistTagChange;
		if (tagChange.Tag != 49)
		{
			return null;
		}
		if (tagChange.Value != 2)
		{
			return null;
		}
		Entity entity = GameState.Get().GetEntity(tagChange.Entity);
		if (entity == null)
		{
			Debug.LogWarningFormat("{0}.GetTargetCardFromPowerTask() - WARNING trying to target entity with id {1} but there is no entity with that id", this, tagChange.Entity);
			return null;
		}
		if (entity.GetZone() != TAG_ZONE.HAND)
		{
			return null;
		}
		return entity.GetCard();
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_effectsPendingFinish++;
		SetActors();
		base.OnAction(prevStateType);
		StartCoroutine(DoActionWithTiming());
	}

	private void SetActors()
	{
		m_friendlyActors.Clear();
		m_opponentActors.Clear();
		InputManager.Get().DisableInput();
		for (int i = 0; i < m_targets.Count; i++)
		{
			Card component = m_targets[i].GetComponent<Card>();
			Entity entity = component.GetEntity();
			Actor actor = component.GetActor();
			if (entity.IsControlledByFriendlySidePlayer())
			{
				m_friendlyActors.Add(actor);
			}
			else
			{
				m_opponentActors.Add(actor);
			}
		}
	}

	private int FindTaskCountToRun()
	{
		int taskCount = 0;
		foreach (PowerTask task in m_taskList.GetTaskList())
		{
			if (task.GetPower().Type == Network.PowerType.SHOW_ENTITY)
			{
				return taskCount;
			}
			taskCount++;
		}
		return 0;
	}

	private IEnumerator DoActionWithTiming()
	{
		yield return StartCoroutine(DoMoveEffects());
		yield return StartCoroutine(CompleteTasksUntilDraw());
	}

	private IEnumerator DoMoveEffects()
	{
		if (m_friendlyActors.Count > 0)
		{
			while ((bool)GameState.Get().GetFriendlyCardBeingDrawn())
			{
				yield return null;
			}
			while (GameState.Get().GetFriendlySidePlayer().GetHandZone()
				.IsUpdatingLayout())
			{
				yield return null;
			}
			AnimateSpread(m_friendlyActors);
		}
		if (m_opponentActors.Count > 0)
		{
			while ((bool)GameState.Get().GetOpponentCardBeingDrawn())
			{
				yield return null;
			}
			while (GameState.Get().GetOpposingSidePlayer().GetHandZone()
				.IsUpdatingLayout())
			{
				yield return null;
			}
			AnimateSpread(m_opponentActors);
		}
		while (m_friendlyActors.Count > 0 || m_opponentActors.Count > 0)
		{
			yield return null;
		}
		InputManager.Get().EnableInput();
	}

	private void AnimateSpread(List<Actor> actors)
	{
		for (int i = 0; i < actors.Count; i++)
		{
			float waitSec = (float)(actors.Count - i - 1) * m_MoveToDeckInterval;
			StartCoroutine(AnimateActor(actors, actors[i], waitSec));
		}
	}

	private IEnumerator AnimateActor(List<Actor> actors, Actor actor, float waitSec)
	{
		Card card = actor.GetCard();
		Player player = card.GetController();
		ZoneDeck deck = player.GetDeckZone();
		actor.Show();
		float slideAmount = (player.IsFriendlySide() ? m_MoveUpOffsetZ : (0f - m_MoveUpOffsetZ));
		iTween.MoveTo(position: new Vector3(card.transform.position.x, card.transform.position.y, card.transform.position.z + slideAmount), target: card.gameObject, time: m_MoveUpTime);
		iTween.ScaleTo(card.gameObject, card.transform.localScale * m_MoveUpScale, m_MoveUpTime);
		yield return new WaitForSeconds(m_MoveUpTime + waitSec);
		bool hiddenActor = !player.IsFriendlySide();
		yield return StartCoroutine(actor.GetCard().AnimatePlayToDeck(actor.gameObject, deck, hiddenActor));
		actors.Remove(actor);
	}

	private IEnumerator CompleteTasksUntilDraw()
	{
		int taskCount = FindTaskCountToRun();
		if (taskCount <= 0)
		{
			m_effectsPendingFinish--;
			FinishIfPossible();
			yield break;
		}
		bool complete = false;
		m_taskList.DoTasks(0, taskCount, delegate
		{
			complete = true;
		});
		while (!complete)
		{
			yield return null;
		}
		m_effectsPendingFinish--;
		FinishIfPossible();
	}
}
