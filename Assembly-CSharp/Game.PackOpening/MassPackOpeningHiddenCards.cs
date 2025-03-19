using System.Collections.Generic;
using System.Linq;

namespace Game.PackOpening;

public class MassPackOpeningHiddenCards : HiddenCards
{
	public void InitializeCards(List<PackOpeningCard> cards)
	{
		m_cards = cards;
		SetUpCards();
	}

	public void ResetCards()
	{
		foreach (PackOpeningCard card in m_cards)
		{
			card.gameObject.SetActive(value: false);
			card.Destroy();
		}
		m_cards.Clear();
	}

	public void AttachMassPackOpeningHighlightCards(List<NetCache.BoosterCard> items)
	{
		foreach (var (card2, item2) in m_cards.Zip(items, (PackOpeningCard card, NetCache.BoosterCard item) => (card: card, item: item)))
		{
			card2.AttachBoosterCard(item2, isMassPackOpening: true);
		}
	}

	public bool AreAllCardsRevealed()
	{
		foreach (PackOpeningCard card in m_cards)
		{
			if (!card.IsRevealed())
			{
				return false;
			}
		}
		return true;
	}
}
