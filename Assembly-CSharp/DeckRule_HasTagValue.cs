using System.Collections.Generic;

public class DeckRule_HasTagValue : DeckRule
{
	public DeckRule_HasTagValue(DeckRulesetRuleDbfRecord record)
		: base(RuleType.HAS_TAG_VALUE, record)
	{
	}

	public override bool Filter(EntityDef def, CollectionDeck deck)
	{
		if (!AppliesTo(def.GetCardId()))
		{
			return true;
		}
		int tagValue = def.GetTag(m_tag);
		return GetResult(CardHasTagValue(tagValue, m_tagMaxValue, m_tagMinValue));
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
				bool cardValidResult = CardHasTagValue(DefLoader.Get().GetEntityDef(cardId).GetTag(m_tag), m_tagMaxValue, m_tagMinValue);
				if (!GetResult(cardValidResult))
				{
					countInvalid += slot.Count;
					deckResult = false;
				}
			}
		}
		if (!deckResult)
		{
			reason = new RuleInvalidReason(m_errorString, countInvalid);
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
		reason = new RuleInvalidReason(GameStrings.Get("GLUE_COLLECTION_LOCK_CARD_BANNED"));
		int tagValue = def.GetTag(m_tag);
		return GetResult(CardHasTagValue(tagValue, m_tagMaxValue, m_tagMinValue));
	}

	private static bool CardHasTagValue(int tagValue, int max, int min)
	{
		if (tagValue >= max)
		{
			return tagValue <= min;
		}
		return false;
	}
}
