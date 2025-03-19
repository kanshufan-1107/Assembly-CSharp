using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroPowerSwapSpell : Spell
{
	public Spell m_swapFX;

	public override bool AddPowerTargets()
	{
		if (!CanAddPowerTargets())
		{
			return false;
		}
		int yourHeroPowerEntityId = GameState.Get().GetFriendlySidePlayer().GetHeroPower()
			.GetEntityId();
		int theirHeroPowerEntityId = GameState.Get().GetOpposingSidePlayer().GetHeroPower()
			.GetEntityId();
		int yourHeroPowerControllerChangeIndex = -1;
		int theirHeroPowerControllerChangeIndex = -1;
		List<PowerTask> tasks = m_taskList.GetTaskList();
		for (int taskIndex = 0; taskIndex < tasks.Count; taskIndex++)
		{
			if (tasks[taskIndex].GetPower() is Network.HistTagChange { Tag: 50 } tagChange)
			{
				if (tagChange.Entity == yourHeroPowerEntityId)
				{
					yourHeroPowerControllerChangeIndex = taskIndex;
				}
				else if (tagChange.Entity == theirHeroPowerEntityId)
				{
					theirHeroPowerControllerChangeIndex = taskIndex;
				}
			}
		}
		if (yourHeroPowerControllerChangeIndex < 0)
		{
			return false;
		}
		if (theirHeroPowerControllerChangeIndex < 0)
		{
			return false;
		}
		return true;
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		StartCoroutine(DoActionWithTiming(prevStateType));
	}

	private IEnumerator DoActionWithTiming(SpellStateType prevStateType)
	{
		Card yourHeroPowerCard = GameState.Get().GetFriendlySidePlayer().GetHeroPowerCard();
		Card theirHeroPowerCard = GameState.Get().GetOpposingSidePlayer().GetHeroPowerCard();
		Animation yourAnim = yourHeroPowerCard.GetActor().GetComponent<Animation>();
		Animation theirAnim = theirHeroPowerCard.GetActor().GetComponent<Animation>();
		while (yourAnim.isPlaying || theirAnim.isPlaying)
		{
			yield return null;
		}
		if (m_swapFX == null)
		{
			OnSpellFinished();
			OnStateFinished();
			yield break;
		}
		Spell spell2 = SpellManager.Get().GetSpell(m_swapFX);
		SpellUtils.SetCustomSpellParent(spell2, this);
		spell2.SetSource(yourHeroPowerCard.gameObject);
		spell2.AddTarget(theirHeroPowerCard.gameObject);
		spell2.AddFinishedCallback(delegate
		{
			OnSpellFinished();
		});
		spell2.AddStateFinishedCallback(delegate
		{
			OnStateFinished();
		});
		spell2.Activate();
	}
}
