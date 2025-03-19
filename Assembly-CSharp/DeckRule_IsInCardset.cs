using System.Collections.Generic;

public class DeckRule_IsInCardset : DeckRule
{
	public DeckRule_IsInCardset(DeckRulesetRuleDbfRecord record)
		: base(RuleType.IS_IN_CARDSET, record)
	{
	}

	public override bool Filter(EntityDef def, CollectionDeck deck)
	{
		string cardId = def.GetCardId();
		if (!AppliesTo(cardId))
		{
			return true;
		}
		if (deck == null)
		{
			return true;
		}
		return GetResult(CardBelongsInSet(cardId));
	}

	public override bool IsDeckValid(CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		bool deckResult = true;
		List<CollectionDeckSlot> slots = deck.GetSlots();
		int countInvalid = 0;
		foreach (CollectionDeckSlot slot in slots)
		{
			string cardId = slot.CardID;
			if (AppliesTo(cardId))
			{
				bool cardValidResult = CardBelongsInSet(cardId);
				if (!GetResult(cardValidResult))
				{
					countInvalid += slot.Count;
					deckResult = false;
				}
			}
		}
		if (!deckResult)
		{
			reason = new RuleInvalidReason(GameStrings.Format("GLUE_COLLECTION_DECK_RULE_NOT_IN_CARDSET", countInvalid), countInvalid);
		}
		return deckResult;
	}

	public override bool CanAddToDeck(EntityDef def, TAG_PREMIUM premium, CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		string cardId = def.GetCardId();
		if (!AppliesTo(cardId))
		{
			return true;
		}
		bool cardValidResult = CardBelongsInSet(cardId);
		cardValidResult = GetResult(cardValidResult);
		if (!cardValidResult)
		{
			reason = new RuleInvalidReason(GameStrings.Get("GLUE_COLLECTION_LOCK_CARD_NOT_IN_CARDSET"));
		}
		return cardValidResult;
	}

	private bool CardBelongsInSet(string cardId)
	{
		return GameUtils.GetCardSetFromCardID(cardId) == (TAG_CARD_SET)m_minValue;
	}
}
