public class DeckRule_PlayerOwnsDeckTemplate : DeckRule
{
	public DeckRule_PlayerOwnsDeckTemplate(DeckRulesetRuleDbfRecord record)
		: base(RuleType.PLAYER_OWNS_DECK_TEMPLATE, record)
	{
	}

	public override bool CanAddToDeck(EntityDef def, TAG_PREMIUM premium, CollectionDeck deck, out RuleInvalidReason reason)
	{
		return DefaultYes(out reason);
	}

	public override bool IsDeckValid(CollectionDeck deck, out RuleInvalidReason reason)
	{
		return DefaultYes(out reason);
	}
}
