using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForActorReadySpell : Spell
{
	public float m_timeoutSeconds;

	public bool m_useTimeout;

	public float m_secondsDelayAfterActorTransition;

	private bool m_isContinueFired;

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		StartCoroutine(ContinueAfterActorReady());
		if (m_useTimeout)
		{
			StartCoroutine(ContinueAfterTimeOut());
		}
	}

	private IEnumerator ContinueAfterTimeOut()
	{
		yield return new WaitForSeconds(m_timeoutSeconds);
		Continue();
	}

	private IEnumerator ContinueAfterActorReady()
	{
		List<Card> cardsToWait = new List<Card>();
		Card sourceCard = Source.GetComponent<Card>();
		if (sourceCard != null)
		{
			cardsToWait.Add(sourceCard);
		}
		foreach (GameObject target in m_targets)
		{
			Card targetCard = target.GetComponent<Card>();
			if (targetCard != null)
			{
				cardsToWait.Add(targetCard);
			}
		}
		bool hasActorTransition = false;
		if (!AreActorsReady(cardsToWait))
		{
			hasActorTransition = true;
			yield return null;
		}
		if (hasActorTransition && m_secondsDelayAfterActorTransition > 0f)
		{
			yield return new WaitForSeconds(m_secondsDelayAfterActorTransition);
		}
		Continue();
	}

	private bool AreActorsReady(List<Card> cards)
	{
		foreach (Card card in cards)
		{
			if (!card.IsActorReady() || !card.IsTransitioningZones())
			{
				return false;
			}
		}
		return true;
	}

	private void Continue()
	{
		if (!m_isContinueFired)
		{
			m_isContinueFired = true;
			OnStateFinished();
		}
	}
}
