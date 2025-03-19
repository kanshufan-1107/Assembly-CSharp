public class DeckRule_TouristLimit : DeckRule
{
	public int MaxTourists = 1;

	public DeckRule_TouristLimit(DeckRulesetRuleDbfRecord record)
		: base(RuleType.TOURIST_LIMIT, record)
	{
		MaxTourists = record.MaxValue;
	}

	public override bool CanAddToDeck(EntityDef def, TAG_PREMIUM premium, CollectionDeck deck, out RuleInvalidReason reason)
	{
		if (!def.HasTag(GAME_TAG.TOURIST))
		{
			reason = null;
			return true;
		}
		int touristCount = 1;
		foreach (string cardId in deck.GetCardsWithCardID())
		{
			if (DefLoader.Get().GetEntityDef(cardId).HasTag(GAME_TAG.TOURIST))
			{
				touristCount++;
			}
		}
		if (touristCount > MaxTourists)
		{
			reason = new RuleInvalidReason(GameStrings.Get("GLUE_COLLECTION_DECK_RULE_TOO_MANY_TOURIST"));
			return false;
		}
		reason = null;
		return true;
	}

	public override bool IsDeckValid(CollectionDeck deck, out RuleInvalidReason reason)
	{
		int touristCount = 0;
		foreach (string cardId in deck.GetCardsWithCardID())
		{
			if (DefLoader.Get().GetEntityDef(cardId).HasTag(GAME_TAG.TOURIST))
			{
				touristCount++;
			}
		}
		if (touristCount > MaxTourists)
		{
			reason = new RuleInvalidReason(GameStrings.Get("GLUE_COLLECTION_DECK_RULE_TOO_MANY_TOURIST"));
			return false;
		}
		reason = null;
		return true;
	}

	public override bool Filter(EntityDef def, CollectionDeck deck)
	{
		return true;
	}
}
