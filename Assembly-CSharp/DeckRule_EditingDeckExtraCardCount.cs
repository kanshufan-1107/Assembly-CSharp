using UnityEngine;

public class DeckRule_EditingDeckExtraCardCount : DeckRule
{
	public DeckRule_EditingDeckExtraCardCount(DeckRulesetRuleDbfRecord record)
		: base(RuleType.EDITING_DECK_EXTRA_CARD_COUNT, record)
	{
		if (m_ruleIsNot)
		{
			Debug.LogError("EDITING_DECK_EXTRA_CARD_COUNT rules do not support \"is not\".");
		}
		if (m_appliesToSubset != null)
		{
			Debug.LogError("EDITING_DECK_EXTRA_CARD_COUNT rules do not support \"applies to subset\".");
		}
	}

	public override bool IsDeckValid(CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		return true;
	}

	public int GetEditingDeckExtraCardCount()
	{
		return m_maxValue;
	}
}
