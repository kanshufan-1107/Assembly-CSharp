using System.Collections.Generic;
using UnityEngine;

public class DeckRule_IsClassCardOrNeutral : DeckRule
{
	public DeckRule_IsClassCardOrNeutral(DeckRulesetRuleDbfRecord record)
		: base(RuleType.IS_CLASS_CARD_OR_NEUTRAL, record)
	{
		if (m_ruleIsNot)
		{
			Debug.LogError("IS_CLASS_CARD_OR_NEUTRAL rules do not support \"is not\".");
		}
	}

	public override bool Filter(EntityDef def, CollectionDeck deck)
	{
		if (!AppliesTo(def.GetCardId()))
		{
			return true;
		}
		if (deck == null)
		{
			return true;
		}
		List<TAG_CLASS> deckClasses = deck.GetClasses();
		List<TAG_CLASS> heroClasses = deck.GetHeroClasses();
		return GetResult(CardIsClassCardOrNeutral(def, deckClasses, heroClasses));
	}

	public override bool IsDeckValid(CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		bool deckResult = true;
		List<CollectionDeckSlot> slots = deck.GetSlots();
		int countInvalid = 0;
		List<TAG_CLASS> deckClasses = deck.GetClasses();
		List<TAG_CLASS> heroClasses = deck.GetHeroClasses();
		foreach (CollectionDeckSlot slot in slots)
		{
			string cardId = slot.CardID;
			if (AppliesTo(cardId))
			{
				bool cardValidResult = CardIsClassCardOrNeutral(DefLoader.Get().GetEntityDef(cardId), deckClasses, heroClasses);
				if (!GetResult(cardValidResult))
				{
					countInvalid += slot.Count;
					deckResult = false;
				}
			}
		}
		if (!deckResult)
		{
			reason = new RuleInvalidReason(GameStrings.Format("GLUE_COLLECTION_DECK_RULE_INVALID_CLASS_CARD", countInvalid), countInvalid);
		}
		return deckResult;
	}

	public override bool CanAddToDeck(EntityDef def, TAG_PREMIUM premium, CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		if (!AppliesTo(def.GetCardId()))
		{
			return true;
		}
		List<TAG_CLASS> deckClasses = deck.GetClasses();
		List<TAG_CLASS> heroClasses = deck.GetHeroClasses();
		bool cardValidResult = CardIsClassCardOrNeutral(def, deckClasses, heroClasses);
		cardValidResult = GetResult(cardValidResult);
		if (!cardValidResult)
		{
			reason = new RuleInvalidReason(GameStrings.Get("GLUE_COLLECTION_LOCK_CARD_INVALID_CLASS"));
		}
		return cardValidResult;
	}

	private static bool CardIsClassCardOrNeutral(EntityBase def, List<TAG_CLASS> deckClasses, List<TAG_CLASS> heroClasses)
	{
		List<TAG_CLASS> cardClasses = new List<TAG_CLASS>();
		def.GetClasses(cardClasses);
		foreach (TAG_CLASS cardClass in cardClasses)
		{
			if (cardClass == TAG_CLASS.NEUTRAL)
			{
				return true;
			}
			foreach (TAG_CLASS heroClass in heroClasses)
			{
				if (heroClass == cardClass)
				{
					return true;
				}
			}
		}
		if (def.GetCardSet() == TAG_CARD_SET.ISLAND_VACATION)
		{
			foreach (TAG_CLASS cardClass2 in cardClasses)
			{
				foreach (TAG_CLASS deckClass in deckClasses)
				{
					if (deckClass == cardClass2)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
