using System.Collections.Generic;

public class DeckRule_IsNotBannedInLeague : DeckRule
{
	public DeckRule_IsNotBannedInLeague(DeckRulesetRuleDbfRecord record)
		: base(RuleType.IS_NOT_BANNED_IN_LEAGUE, record)
	{
	}

	public override bool Filter(EntityDef def, CollectionDeck deck)
	{
		return !RankMgr.Get().IsCardBannedInCurrentLeague(def);
	}

	public override bool IsDeckValid(CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		List<CollectionDeckSlot> slots = deck.GetSlots();
		int bannedCardCount = 0;
		foreach (CollectionDeckSlot slot in slots)
		{
			EntityDef def = DefLoader.Get().GetEntityDef(slot.CardID);
			if (AppliesTo(slot.CardID) && GameUtils.IsBanned(deck, def))
			{
				bannedCardCount++;
			}
		}
		bool isValid = GetResult(bannedCardCount == 0);
		if (!isValid)
		{
			if (RankMgr.Get().IsNewPlayer())
			{
				reason = new RuleInvalidReason(GameStrings.Format("GLUE_COLLECTION_DECK_RULE_INVALID_CARDS_NPR", bannedCardCount), bannedCardCount);
			}
			else
			{
				reason = new RuleInvalidReason(GameStrings.Format("GLUE_COLLECTION_DECK_RULE_INVALID_CARDS", bannedCardCount), bannedCardCount);
			}
		}
		return isValid;
	}

	public override bool CanAddToDeck(EntityDef def, TAG_PREMIUM premium, CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		string cardId = def.GetCardId();
		if (!AppliesTo(cardId))
		{
			return true;
		}
		bool isNotOnDenyList = !GameUtils.IsBannedByDuelsDenylist(deck, cardId) && !GameUtils.IsBannedByConstructedDenylist(deck, cardId) && !GameUtils.IsBannedByTwistDenylist(deck, cardId) && !GameUtils.IsBannedByStandardDenylist(deck, cardId) && !GameUtils.IsBannedByWildDenylist(deck, cardId) && !GameUtils.IsBannedByTavernBrawlDenylist(deck, cardId);
		bool cardValidResult = GetResult(isNotOnDenyList);
		if (!cardValidResult)
		{
			reason = new RuleInvalidReason(GameStrings.Format("GLUE_COLLECTION_DECK_RULE_INVALID_CARDS", 1), 1);
		}
		return cardValidResult;
	}
}
