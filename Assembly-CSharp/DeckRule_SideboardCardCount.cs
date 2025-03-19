using System.Collections.Generic;
using UnityEngine;

public class DeckRule_SideboardCardCount : DeckRule
{
	public DeckRule_SideboardCardCount(DeckRulesetRuleDbfRecord record)
		: base(RuleType.SIDEBOARD_CARD_COUNT_LIMIT, record)
	{
		if (m_ruleIsNot)
		{
			Debug.LogError("SIDEBOARD_CARD_COUNT_LIMIT rules do not support \"is not\".");
		}
		if (m_appliesToSubset != null)
		{
			Debug.LogError("SIDEBOARD_CARD_COUNT_LIMIT rules do not support \"applies to subset\".");
		}
	}

	public override bool IsDeckValid(CollectionDeck deck, out RuleInvalidReason reason)
	{
		foreach (KeyValuePair<string, SideboardDeck> allSideboard in deck.GetAllSideboards())
		{
			SideboardDeck value = allSideboard.Value;
			value.GetCardCountRange(out var minCards, out var maxCards);
			int totalSideboardCards = value.GetTotalCardCount();
			if (totalSideboardCards < minCards)
			{
				reason = new RuleInvalidReason(GameStrings.Get("GLUE_COLLECTION_DECK_RULE_TOO_FEW_SIDEBOARD_CARDS"));
				return false;
			}
			if (totalSideboardCards > maxCards)
			{
				reason = new RuleInvalidReason(GameStrings.Get("GLUE_COLLECTION_DECK_RULE_TOO_MANY_SIDEBOARD_CARDS"));
				return false;
			}
		}
		reason = null;
		return true;
	}
}
