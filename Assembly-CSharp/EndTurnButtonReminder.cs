using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButtonReminder : MonoBehaviour
{
	public float m_MaxDelaySec = 0.3f;

	private List<Card> m_cardsWaitingToRemind = new List<Card>();

	public bool ShowFriendlySidePlayerTurnReminder()
	{
		GameState state = GameState.Get();
		if (GameMgr.Get().IsBattlegrounds())
		{
			return false;
		}
		if (state.IsMulliganManagerActive())
		{
			return false;
		}
		Player player = state.GetFriendlySidePlayer();
		if (player == null)
		{
			return false;
		}
		if (!player.IsCurrentPlayer())
		{
			return false;
		}
		ZoneMgr zoneMgr = ZoneMgr.Get();
		if (zoneMgr == null)
		{
			return false;
		}
		ZonePlay zonePlay = zoneMgr.FindZoneOfType<ZonePlay>(Player.Side.FRIENDLY);
		if (zonePlay == null)
		{
			return false;
		}
		List<Card> cards = GenerateCardsToRemindList(state, zonePlay.GetCards());
		if (cards.Count == 0)
		{
			return true;
		}
		PlayReminders(cards);
		return true;
	}

	private List<Card> GenerateCardsToRemindList(GameState state, List<Card> originalList)
	{
		List<Card> newList = new List<Card>();
		GameEntity gameEntity = GameState.Get()?.GetGameEntity();
		foreach (Card card in originalList)
		{
			if (gameEntity != null && gameEntity.OverwriteEndTurnReminder(card.GetEntity(), out var showReminder))
			{
				if (showReminder)
				{
					newList.Add(card);
				}
			}
			else if (state.HasResponse(card.GetEntity(), null))
			{
				newList.Add(card);
			}
		}
		return newList;
	}

	private void PlayReminders(List<Card> cards)
	{
		int cardThatStartsNowIndex;
		Card cardThatStartsNow;
		do
		{
			cardThatStartsNowIndex = Random.Range(0, cards.Count);
			cardThatStartsNow = cards[cardThatStartsNowIndex];
		}
		while (m_cardsWaitingToRemind.Contains(cardThatStartsNow));
		for (int i = 0; i < cards.Count; i++)
		{
			Card card = cards[i];
			Spell reminderSpell = card.GetActorSpell(SpellType.WIGGLE);
			if (reminderSpell == null || reminderSpell.GetActiveState() != 0 || m_cardsWaitingToRemind.Contains(card))
			{
				continue;
			}
			if (i == cardThatStartsNowIndex)
			{
				reminderSpell.Activate();
				continue;
			}
			float delay = Random.Range(0f, m_MaxDelaySec);
			if (Mathf.Approximately(delay, 0f))
			{
				reminderSpell.Activate();
				continue;
			}
			m_cardsWaitingToRemind.Add(card);
			StartCoroutine(WaitAndPlayReminder(card, reminderSpell, delay));
		}
	}

	private IEnumerator WaitAndPlayReminder(Card card, Spell reminderSpell, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (GameState.Get().IsFriendlySidePlayerTurn() && card.GetZone() is ZonePlay)
		{
			reminderSpell.Activate();
			m_cardsWaitingToRemind.Remove(card);
		}
	}
}
