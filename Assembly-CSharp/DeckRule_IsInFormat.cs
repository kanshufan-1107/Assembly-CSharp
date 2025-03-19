using System.Collections.Generic;
using PegasusShared;

public class DeckRule_IsInFormat : DeckRule
{
	public DeckRule_IsInFormat(DeckRulesetRuleDbfRecord record)
		: base(RuleType.IS_IN_FORMAT, record)
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
		return GetResult(CardBelongsInFormat(cardId));
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
				bool cardValidResult = CardBelongsInFormat(cardId);
				if (!GetResult(cardValidResult))
				{
					countInvalid += slot.Count;
					deckResult = false;
				}
			}
		}
		if (!deckResult)
		{
			reason = new RuleInvalidReason(GameStrings.Format("GLUE_COLLECTION_DECK_RULE_NOT_IN_FORMAT", countInvalid), countInvalid);
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
		bool cardValidResult = CardBelongsInFormat(cardId);
		cardValidResult = GetResult(cardValidResult);
		if (!cardValidResult)
		{
			reason = new RuleInvalidReason(GameStrings.Get("GLUE_COLLECTION_LOCK_CARD_NOT_IN_FORMAT"));
		}
		return cardValidResult;
	}

	private bool CardBelongsInFormat(string cardId)
	{
		return GameUtils.IsCardValidForFormat((FormatType)m_minValue, cardId);
	}
}
