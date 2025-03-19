using UnityEngine;

public class DeckRule_CountCardsInDeck : DeckRule
{
	public DeckRule_CountCardsInDeck(DeckRulesetRuleDbfRecord record)
		: base(RuleType.COUNT_CARDS_IN_DECK, record)
	{
		if (m_appliesToSubset == null)
		{
			Debug.LogError("COUNT_CARDS_IN_DECK only supports rules with a defined \"applies to\" subset");
		}
	}

	public override bool IsDeckValid(CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		int count = deck.GetCardCountInSet(m_appliesToSubset, m_appliesToIsNot);
		int countInvalid = 0;
		bool isMinimum = false;
		bool isWithinBounds = true;
		if (count < m_minValue)
		{
			isWithinBounds = false;
			countInvalid = m_minValue - count;
			isMinimum = true;
		}
		else if (count > m_maxValue)
		{
			isWithinBounds = false;
			countInvalid = count - m_maxValue;
		}
		bool result = GetResult(isWithinBounds);
		if (!result)
		{
			reason = new RuleInvalidReason(m_errorString, countInvalid, isMinimum);
		}
		return result;
	}
}
