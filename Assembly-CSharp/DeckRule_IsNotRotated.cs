using System.Linq;

public class DeckRule_IsNotRotated : DeckRule
{
	public DeckRule_IsNotRotated(DeckRulesetRuleDbfRecord record)
		: base(RuleType.IS_NOT_ROTATED, record)
	{
	}

	public override bool Filter(EntityDef def, CollectionDeck deck)
	{
		return !GameUtils.IsCardRotated(def);
	}

	public override bool IsDeckValid(CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		int countRotatedCards = deck.GetSlots().Sum((CollectionDeckSlot s) => (AppliesTo(s.CardID) && GameUtils.IsCardRotated(s.CardID)) ? s.Count : 0);
		bool isValid = GetResult(countRotatedCards <= 0);
		if (!isValid)
		{
			if (RankMgr.Get().IsNewPlayer())
			{
				reason = new RuleInvalidReason(GameStrings.Format("GLUE_COLLECTION_DECK_RULE_INVALID_CARDS_NPR", countRotatedCards), countRotatedCards);
			}
			else
			{
				reason = new RuleInvalidReason(GameStrings.Format("GLUE_COLLECTION_DECK_RULE_INVALID_CARDS", countRotatedCards), countRotatedCards);
			}
		}
		return isValid;
	}
}
