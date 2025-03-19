public class DeckRule_SideboardHasTagValue : DeckRule
{
	public DeckRule_SideboardHasTagValue(DeckRulesetRuleDbfRecord record)
		: base(RuleType.SIDEBOARD_HAS_TAG_VALUE, record)
	{
	}

	public override bool IsDeckValid(CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		return true;
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
		if (m_tag == 2936)
		{
			CollectionDeckTray deckTray = CollectionDeckTray.Get();
			if (deckTray != null && !deckTray.IsSideboardOpen)
			{
				return true;
			}
		}
		return !CardHasTagValue(def.GetTag(m_tag), m_tagMaxValue, m_tagMinValue) && !GameUtils.IsBannedBySideBoardDenylist(deck, cardId);
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
