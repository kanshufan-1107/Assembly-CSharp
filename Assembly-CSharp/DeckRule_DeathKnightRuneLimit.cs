using System.Collections.Generic;

public class DeckRule_DeathKnightRuneLimit : DeckRule
{
	public static int MaxRuneSlots = 3;

	public DeckRule_DeathKnightRuneLimit(DeckRulesetRuleDbfRecord record)
		: base(RuleType.DEATHKNIGHT_RUNE_LIMIT, record)
	{
		MaxRuneSlots = record.MaxValue;
	}

	public override bool CanAddToDeck(EntityDef def, TAG_PREMIUM premium, CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		List<TAG_CLASS> classes = deck.GetClasses();
		bool isDeathKnightDeck = false;
		foreach (TAG_CLASS item in classes)
		{
			if (item == TAG_CLASS.DEATHKNIGHT)
			{
				isDeathKnightDeck = true;
				break;
			}
		}
		if (!isDeathKnightDeck)
		{
			return true;
		}
		return ValidateRunes(new RunePattern(def), deck.Runes, out reason);
	}

	public override bool IsDeckValid(CollectionDeck deck, out RuleInvalidReason reason)
	{
		reason = null;
		if (!deck.HasClass(TAG_CLASS.DEATHKNIGHT))
		{
			return true;
		}
		int invalidCount = deck.GetTotalInvalidRuneCardCount();
		foreach (KeyValuePair<string, SideboardDeck> allSideboard in deck.GetAllSideboards())
		{
			invalidCount += allSideboard.Value.GetTotalInvalidRuneCardCount();
		}
		if (invalidCount <= 0)
		{
			return true;
		}
		reason = new RuleInvalidReason(GameStrings.Get("GLUE_COLLECTION_INCOMPATIBLE_RUNES_HEADER"), invalidCount);
		return false;
	}

	public override bool Filter(EntityDef def, CollectionDeck deck)
	{
		CollectionManager cm = CollectionManager.Get();
		if (cm != null && cm.IsEditingDeathKnightDeck())
		{
			return true;
		}
		if (deck.HasClass(TAG_CLASS.DEATHKNIGHT))
		{
			return deck.CanAddRunes(def.GetRuneCost(), deck.Runes.CombinedValue);
		}
		return true;
	}

	private bool ValidateRunes(RunePattern runesToAdd, RunePattern validRunes, out RuleInvalidReason reason)
	{
		reason = null;
		if (!runesToAdd.HasRunes)
		{
			return true;
		}
		if (!validRunes.CanAddRunes(runesToAdd, m_maxValue))
		{
			reason = new RuleInvalidReason("GLUE_COLLECTION_INCOMPATIBLE_RUNES");
			return false;
		}
		return true;
	}
}
