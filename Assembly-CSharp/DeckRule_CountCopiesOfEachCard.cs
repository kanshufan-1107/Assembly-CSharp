using System.Collections.Generic;

public class DeckRule_CountCopiesOfEachCard : DeckRule
{
	public DeckRule_CountCopiesOfEachCard(DeckRulesetRuleDbfRecord record)
		: base(RuleType.COUNT_COPIES_OF_EACH_CARD, record)
	{
	}

	public override bool CanAddToDeck(EntityDef def, TAG_PREMIUM premium, CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		if (!AppliesTo(def.GetCardId()))
		{
			return true;
		}
		string cardId = def.GetCardId();
		int cardCount = deck.GetCardIdCount(cardId);
		List<CounterpartCardsDbfRecord> counterpartCards = GameUtils.GetCounterpartCards(cardId);
		if (counterpartCards != null)
		{
			foreach (CounterpartCardsDbfRecord counterpartCard in counterpartCards)
			{
				cardCount += deck.GetCardIdCount(GameUtils.TranslateDbIdToCardId(counterpartCard.DeckEquivalentCardId));
			}
		}
		bool atMaximum = cardCount >= m_maxValue;
		if (atMaximum)
		{
			reason = new RuleInvalidReason(GameStrings.Format("GLUE_COLLECTION_LOCK_MAX_DECK_COPIES", m_maxValue), m_maxValue);
		}
		return GetResult(!atMaximum);
	}

	public bool GetMaxCopies(EntityDef def, out int maxCopies)
	{
		maxCopies = int.MaxValue;
		if (!AppliesTo(def.GetCardId()))
		{
			return false;
		}
		maxCopies = m_maxValue;
		return true;
	}

	public override bool IsDeckValid(CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		bool result = true;
		List<CollectionDeckSlot> slots = deck.GetSlots();
		int countInvalid = 0;
		bool isMinimum = false;
		foreach (CollectionDeckSlot item in slots)
		{
			string cardId = item.CardID;
			if (AppliesTo(cardId))
			{
				int count = deck.GetCardIdCount(cardId);
				if (count < m_minValue)
				{
					result = false;
					countInvalid = m_minValue - count;
					isMinimum = true;
					break;
				}
				if (count > m_maxValue)
				{
					result = false;
					countInvalid = (count = m_maxValue);
					break;
				}
			}
		}
		result = GetResult(result);
		if (!result)
		{
			reason = new RuleInvalidReason(m_errorString, countInvalid, isMinimum);
		}
		return result;
	}
}
