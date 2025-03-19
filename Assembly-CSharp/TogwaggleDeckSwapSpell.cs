using System.Collections;
using System.Collections.Generic;

public class TogwaggleDeckSwapSpell : SpawnToHandSpell
{
	protected override void OnAction(SpellStateType prevStateType)
	{
		StartCoroutine(DoActionWithTiming(prevStateType));
	}

	private IEnumerator DoActionWithTiming(SpellStateType prevStateType)
	{
		int friendlyDeckSize = 0;
		Player friendlyPlayer = GameState.Get().GetFriendlySidePlayer();
		if (friendlyPlayer != null)
		{
			ZoneDeck deck = friendlyPlayer.GetDeckZone();
			if (deck != null)
			{
				friendlyDeckSize = deck.GetCardCount();
			}
		}
		int opponentDeckSize = 0;
		Player opponentPlayer = GameState.Get().GetOpposingSidePlayer();
		if (opponentPlayer != null)
		{
			ZoneDeck deck2 = opponentPlayer.GetDeckZone();
			if (deck2 != null)
			{
				opponentDeckSize = deck2.GetCardCount();
			}
		}
		foreach (Zone item in SpellUtils.FindZonesFromTag(SpellZoneTag.DECK))
		{
			item.AddLayoutBlocker();
		}
		int lastDeckZoneChangeTaskIndex = -1;
		List<PowerTask> tasks = m_taskList.GetTaskList();
		for (int taskIndex = 0; taskIndex < tasks.Count; taskIndex++)
		{
			if (tasks[taskIndex].GetPower() is Network.HistTagChange tagChange)
			{
				bool relevantChange = false;
				if (tagChange.Tag == 49 && tagChange.Value == 2)
				{
					relevantChange = true;
				}
				if (tagChange.Tag == 50)
				{
					relevantChange = true;
				}
				if (relevantChange)
				{
					lastDeckZoneChangeTaskIndex = taskIndex;
				}
			}
		}
		if (lastDeckZoneChangeTaskIndex >= 0)
		{
			bool complete = false;
			m_taskList.DoTasks(0, lastDeckZoneChangeTaskIndex + 1, delegate
			{
				complete = true;
			});
			while (!complete)
			{
				yield return null;
			}
		}
		base.OnBeforeActivateAreaEffectSpell = delegate(Spell spell)
		{
			spell.AddFinishedCallback(OnAEFinished);
			PlayMakerFSM component = spell.GetComponent<PlayMakerFSM>();
			if (component != null)
			{
				component.FsmVariables.GetFsmInt("FriendlyDeckSize").Value = friendlyDeckSize;
				component.FsmVariables.GetFsmInt("OpponentDeckSize").Value = opponentDeckSize;
			}
		};
		base.OnAction(prevStateType);
	}

	private void OnAEFinished(Spell spell, object userData)
	{
		if (spell != m_activeAreaEffectSpell)
		{
			return;
		}
		foreach (Zone item in SpellUtils.FindZonesFromTag(SpellZoneTag.DECK))
		{
			ZoneDeck deckZone = item as ZoneDeck;
			if (deckZone != null)
			{
				deckZone.RemoveLayoutBlocker();
				deckZone.SetSuppressEmotes(suppress: true);
				deckZone.SetVisibility(visible: true);
				deckZone.UpdateLayout();
				deckZone.SetSuppressEmotes(suppress: false);
			}
		}
	}
}
