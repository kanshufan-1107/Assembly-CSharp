using UnityEngine;

public class DeckRule_DeckSize : DeckRule
{
	public DeckRule_DeckSize(DeckRulesetRuleDbfRecord record)
		: base(RuleType.DECK_SIZE, record)
	{
		if (m_ruleIsNot)
		{
			Debug.LogError("DECK_SIZE rules do not support \"is not\".");
		}
		if (m_appliesToSubset != null)
		{
			Debug.LogError("DECK_SIZE rules do not support \"applies to subset\".");
		}
	}

	public override bool CanAddToDeck(EntityDef def, TAG_PREMIUM premium, CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		if (deck == null)
		{
			return false;
		}
		if (!(deck is SideboardDeck sideboard))
		{
			return DefaultYes(out reason);
		}
		if (sideboard.GetTotalCardCount() == sideboard.DataModel.MaxCards)
		{
			reason = new RuleInvalidReason("GLUE_COLLECTION_DECK_RULE_TOO_MANY_CARDS", 1);
			return false;
		}
		return DefaultYes(out reason);
	}

	public override bool IsDeckValid(CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		int count = deck.GetTotalCardCount();
		int countInvalid = 0;
		bool isMinimum = false;
		int minDeckSize = m_minValue;
		int maxDeckSize = m_maxValue;
		foreach (CollectionDeckSlot slot in deck.GetSlots())
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(slot.CardID);
			if (entityDef.HasTag(GAME_TAG.DECK_RULE_MOD_DECK_SIZE))
			{
				maxDeckSize = entityDef.GetTag(GAME_TAG.DECK_RULE_MOD_DECK_SIZE);
				minDeckSize = maxDeckSize;
				break;
			}
		}
		bool isWithinBounds = true;
		if (count < minDeckSize)
		{
			isWithinBounds = false;
			countInvalid = minDeckSize - count;
			isMinimum = true;
		}
		else if (count > maxDeckSize)
		{
			isWithinBounds = false;
			countInvalid = count - maxDeckSize;
		}
		bool result = GetResult(isWithinBounds);
		if (!result)
		{
			string displayError = ((count >= minDeckSize) ? GameStrings.Format("GLUE_COLLECTION_DECK_RULE_TOO_MANY_CARDS", countInvalid) : GameStrings.Format("GLUE_COLLECTION_DECK_RULE_MISSING_CARDS", countInvalid));
			reason = new RuleInvalidReason(displayError, countInvalid, isMinimum);
		}
		return result;
	}

	public int GetMaximumDeckSize(CollectionDeck deck = null)
	{
		if (deck == null)
		{
			return GetDefaultDeckSize();
		}
		if (CardInDeckModifiesDeckSize(deck, out var modifiedSize))
		{
			return modifiedSize;
		}
		return m_maxValue;
	}

	public int GetMinimumDeckSize(CollectionDeck deck = null)
	{
		if (deck == null)
		{
			return GetDefaultDeckSize();
		}
		if (CardInDeckModifiesDeckSize(deck, out var modifiedSize))
		{
			return modifiedSize;
		}
		return m_minValue;
	}

	private bool CardInDeckModifiesDeckSize(CollectionDeck deck, out int modifiedDeckSize)
	{
		foreach (CollectionDeckSlot slot in deck.GetSlots())
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(slot.CardID);
			if (entityDef.HasTag(GAME_TAG.DECK_RULE_MOD_DECK_SIZE))
			{
				modifiedDeckSize = entityDef.GetTag(GAME_TAG.DECK_RULE_MOD_DECK_SIZE);
				return true;
			}
			if (entityDef.HasTag(GAME_TAG.IGNORE_DECK_RULESET))
			{
				modifiedDeckSize = int.MaxValue;
				return true;
			}
		}
		modifiedDeckSize = 0;
		return false;
	}

	private int GetDefaultDeckSize()
	{
		return m_maxValue;
	}
}
