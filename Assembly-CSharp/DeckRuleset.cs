using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using PegasusShared;
using UnityEngine;

public class DeckRuleset
{
	private int m_id;

	private List<DeckRule> m_rules;

	private static Map<FormatType, DeckRuleset> s_FormatRulesets = new Map<FormatType, DeckRuleset>();

	private static DeckRuleset s_PVPDRRuleset;

	private static DeckRuleset s_PVPDRDisplayRuleset;

	public List<DeckRule> Rules => m_rules;

	public static DeckRuleset GetDeckRuleset(int id)
	{
		if (GetDeckRulesetLookup().TryGetValue(id, out var formatType))
		{
			return GetRuleset(formatType);
		}
		return GetDeckRulesetFromDBF(id);
	}

	private static Dictionary<int, FormatType> GetDeckRulesetLookup()
	{
		Dictionary<int, FormatType> deckRulesetLookup = new Dictionary<int, FormatType>
		{
			{
				1,
				FormatType.FT_WILD
			},
			{
				2,
				FormatType.FT_STANDARD
			},
			{
				482,
				FormatType.FT_CLASSIC
			}
		};
		RankedPlaySeason twistSeason = RankMgr.Get()?.GetCurrentTwistSeason();
		if (twistSeason != null)
		{
			ScenarioDbfRecord scenarioRecord = twistSeason.GetScenario();
			if (scenarioRecord != null)
			{
				deckRulesetLookup.Add(scenarioRecord.DeckRulesetId, FormatType.FT_TWIST);
			}
		}
		return deckRulesetLookup;
	}

	private static Dictionary<FormatType, int> GetReverseDeckRulesetLookup()
	{
		Dictionary<FormatType, int> reverseDeckRulesetLookup = new Dictionary<FormatType, int>
		{
			{
				FormatType.FT_WILD,
				1
			},
			{
				FormatType.FT_STANDARD,
				2
			},
			{
				FormatType.FT_CLASSIC,
				482
			}
		};
		RankedPlaySeason twistSeason = RankMgr.Get()?.GetCurrentTwistSeason();
		if (twistSeason != null)
		{
			ScenarioDbfRecord scenarioRecord = twistSeason.GetScenario();
			if (scenarioRecord != null)
			{
				reverseDeckRulesetLookup.Add(FormatType.FT_TWIST, scenarioRecord.DeckRulesetId);
			}
		}
		return reverseDeckRulesetLookup;
	}

	private static DeckRuleset GetDeckRulesetFromDBF(int id)
	{
		if (id <= 0)
		{
			return null;
		}
		if (!GameDbf.DeckRuleset.HasRecord(id))
		{
			Debug.LogErrorFormat("DeckRuleset not found for id {0}", id);
			return null;
		}
		DeckRuleset result = new DeckRuleset();
		result.m_id = id;
		result.m_rules = new List<DeckRule>();
		DeckRulesetRuleDbfRecord[] rulesForDeckRuleset = GameDbf.GetIndex().GetRulesForDeckRuleset(id);
		for (int i = 0; i < rulesForDeckRuleset.Length; i++)
		{
			DeckRule rule = DeckRule.CreateFromDBF(rulesForDeckRuleset[i]);
			result.m_rules.Add(rule);
		}
		result.m_rules.Sort(DeckRuleViolation.SortComparison_Rule);
		return result;
	}

	public static DeckRuleset GetRuleset(FormatType formatType)
	{
		if (!s_FormatRulesets.TryGetValue(formatType, out var result))
		{
			if (!GetReverseDeckRulesetLookup().TryGetValue(formatType, out var deckRulesetId))
			{
				if (formatType == FormatType.FT_TWIST)
				{
					return null;
				}
				Debug.LogError("DeckRuleset.GetRuleset called with invalid format type " + formatType);
				return null;
			}
			if (!GameDbf.DeckRuleset.HasRecord(deckRulesetId))
			{
				Debug.LogError("Error generating ruleset for id " + deckRulesetId + ", could not find ruleset DBF");
				return null;
			}
			result = GetDeckRulesetFromDBF(deckRulesetId);
			s_FormatRulesets.Add(formatType, result);
		}
		return result;
	}

	public bool Filter(EntityDef entity, CollectionDeck deck, out DeckRule brokenRule, DeckRule.RuleType[] ignoreRules = null)
	{
		brokenRule = null;
		if (EntityIgnoresRuleset(entity) || EntityInDeckIgnoresRuleset(deck))
		{
			return true;
		}
		foreach (DeckRule rule in m_rules)
		{
			if ((ignoreRules == null || !ignoreRules.Contains(rule.Type)) && !rule.Filter(entity, deck))
			{
				brokenRule = rule;
				return false;
			}
		}
		return true;
	}

	public bool Filter(EntityDef entity, CollectionDeck deck, DeckRule.RuleType[] ignoreRules = null)
	{
		DeckRule brokenRule;
		return Filter(entity, deck, out brokenRule, ignoreRules);
	}

	public bool CanAddToDeck(EntityDef def, TAG_PREMIUM premium, CollectionDeck deck, params DeckRule.RuleType[] ignoreRules)
	{
		RuleInvalidReason reason;
		DeckRule rule;
		return CanAddToDeck(def, premium, deck, out reason, out rule, ignoreRules);
	}

	public bool CanAddToDeck(EntityDef def, TAG_PREMIUM premium, CollectionDeck deck, out RuleInvalidReason reason, out DeckRule brokenRule, params DeckRule.RuleType[] ignoreRules)
	{
		reason = null;
		brokenRule = null;
		if (EntityIgnoresRuleset(def) || EntityInDeckIgnoresRuleset(deck))
		{
			return true;
		}
		foreach (DeckRule rule in m_rules)
		{
			if ((ignoreRules == null || !ignoreRules.Contains(rule.Type)) && !rule.CanAddToDeck(def, premium, deck, out reason))
			{
				brokenRule = rule;
				return false;
			}
		}
		return true;
	}

	public bool CanAddToDeck(EntityDef def, TAG_PREMIUM premium, CollectionDeck deck, out List<RuleInvalidReason> reasons, out List<DeckRule> brokenRules, params DeckRule.RuleType[] ignoreRules)
	{
		if (EntityIgnoresRuleset(def) || EntityInDeckIgnoresRuleset(deck))
		{
			reasons = null;
			brokenRules = null;
			return true;
		}
		reasons = new List<RuleInvalidReason>();
		brokenRules = new List<DeckRule>();
		foreach (DeckRule rule in m_rules)
		{
			if ((ignoreRules == null || !ignoreRules.Contains(rule.Type)) && !rule.CanAddToDeck(def, premium, deck, out var reason))
			{
				reasons.Add(reason);
				brokenRules.Add(rule);
			}
		}
		if (brokenRules.Count == 0)
		{
			return true;
		}
		return false;
	}

	public bool IsDeckValid(CollectionDeck deck, params DeckRule.RuleType[] ignoreRules)
	{
		IList<DeckRuleViolation> violations;
		return IsDeckValid(deck, out violations, ignoreRules);
	}

	public bool IsDeckValid(CollectionDeck deck, out IList<DeckRuleViolation> violations, params DeckRule.RuleType[] ignoreRules)
	{
		List<DeckRuleViolation> brokenRules = (List<DeckRuleViolation>)(violations = new List<DeckRuleViolation>());
		List<RuleInvalidReason> reasons = new List<RuleInvalidReason>();
		if (EntityInDeckIgnoresRuleset(deck))
		{
			return true;
		}
		bool success = true;
		foreach (DeckRule rule in m_rules)
		{
			if (ignoreRules == null || !ignoreRules.Contains(rule.Type))
			{
				RuleInvalidReason reason;
				bool valid = rule.IsDeckValid(deck, out reason);
				if (!valid)
				{
					reasons.Add(reason);
					DeckRuleViolation violation = new DeckRuleViolation(rule, reason.DisplayError);
					violations.Add(violation);
					success = false;
				}
				Log.DeckRuleset.Print("validating rule={0} deck={1} result={2} reason={3}", rule, deck, valid, reason);
			}
		}
		brokenRules.Sort(DeckRuleViolation.SortComparison_Violation);
		CollapseSpecialBrokenRules(violations, reasons);
		return success;
	}

	private void CollapseSpecialBrokenRules(IList<DeckRuleViolation> violations, List<RuleInvalidReason> reasons)
	{
		if (reasons.Count <= 1)
		{
			return;
		}
		DeckRule firstRuleCollapsed = null;
		int sumTotalCollapsedCounts = 0;
		List<int> ruleIndexesToCollapse = null;
		for (int i = 0; i < violations.Count; i++)
		{
			DeckRule r = violations[i].Rule;
			if (r.Type == DeckRule.RuleType.PLAYER_OWNS_EACH_COPY && !r.RuleIsNot)
			{
				if (ruleIndexesToCollapse == null)
				{
					ruleIndexesToCollapse = new List<int>();
				}
				if (firstRuleCollapsed == null)
				{
					firstRuleCollapsed = r;
				}
				ruleIndexesToCollapse.Add(i);
				sumTotalCollapsedCounts += reasons[i].CountParam;
			}
			else if (r.Type == DeckRule.RuleType.DECK_SIZE && reasons[i].IsMinimum)
			{
				if (ruleIndexesToCollapse == null)
				{
					ruleIndexesToCollapse = new List<int>();
				}
				if (firstRuleCollapsed == null)
				{
					firstRuleCollapsed = r;
				}
				ruleIndexesToCollapse.Add(i);
				sumTotalCollapsedCounts += reasons[i].CountParam;
			}
		}
		if (ruleIndexesToCollapse != null && ruleIndexesToCollapse.Count > 1)
		{
			for (int i2 = ((ruleIndexesToCollapse == null) ? (-1) : (ruleIndexesToCollapse.Count - 1)); i2 >= 0; i2--)
			{
				int origIndex = ruleIndexesToCollapse[i2];
				violations.RemoveAt(origIndex);
				reasons.RemoveAt(origIndex);
			}
			string displayError = GameStrings.Format("GLUE_COLLECTION_DECK_RULE_MISSING_CARDS", sumTotalCollapsedCounts);
			RuleInvalidReason reason = new RuleInvalidReason(displayError, sumTotalCollapsedCounts);
			reasons.Add(reason);
			DeckRuleViolation violation = new DeckRuleViolation(firstRuleCollapsed, displayError);
			violations.Add(violation);
		}
	}

	private DeckRule_DeckSize GetDeckSizeRule(CollectionDeck deck)
	{
		DeckRule rule = ((m_rules == null) ? null : m_rules.FirstOrDefault((DeckRule r) => r is DeckRule_DeckSize));
		if (rule != null)
		{
			return rule as DeckRule_DeckSize;
		}
		return null;
	}

	public int GetDeckSize(CollectionDeck deck)
	{
		return GetDeckSizeRule(deck)?.GetMaximumDeckSize(deck) ?? 30;
	}

	private DeckRule_EditingDeckExtraCardCount GetEditingDeckExtraCardCountRule()
	{
		DeckRule rule = m_rules?.FirstOrDefault((DeckRule r) => r is DeckRule_EditingDeckExtraCardCount);
		if (rule != null)
		{
			return rule as DeckRule_EditingDeckExtraCardCount;
		}
		return null;
	}

	public int GetDeckSizeWhileEditing(CollectionDeck deck, EntityDef cardBeingAdded = null)
	{
		int deckSize = GetDeckSize(deck);
		if (deckSize < 30)
		{
			return deckSize;
		}
		if (cardBeingAdded != null && cardBeingAdded.HasTag(GAME_TAG.DECK_RULE_MOD_DECK_SIZE))
		{
			deckSize = cardBeingAdded.GetTag(GAME_TAG.DECK_RULE_MOD_DECK_SIZE);
		}
		if (IsOvercappedDecksEnabled())
		{
			DeckRule_EditingDeckExtraCardCount editingDeckExtraCardCountRule = GetEditingDeckExtraCardCountRule();
			deckSize = ((editingDeckExtraCardCountRule == null) ? deckSize : (deckSize + editingDeckExtraCardCountRule.GetEditingDeckExtraCardCount()));
		}
		return deckSize;
	}

	public int GetMinimumAllowedDeckSize(CollectionDeck deck)
	{
		return GetDeckSizeRule(deck)?.GetMinimumDeckSize(deck) ?? 30;
	}

	public bool HasOwnershipOrRotatedRule()
	{
		return ((m_rules == null) ? null : m_rules.FirstOrDefault((DeckRule r) => r.Type == DeckRule.RuleType.IS_NOT_ROTATED || (r.Type == DeckRule.RuleType.PLAYER_OWNS_EACH_COPY && !r.RuleIsNot))) != null;
	}

	public bool FilterFailsOnShowInvalidRule(EntityDef entity, CollectionDeck deck)
	{
		bool failsOnShowInvalidRule = false;
		foreach (DeckRule rule in m_rules)
		{
			if (!rule.Filter(entity, deck))
			{
				if (!rule.ShowInvalidCards)
				{
					failsOnShowInvalidRule = false;
					break;
				}
				failsOnShowInvalidRule = true;
			}
		}
		return failsOnShowInvalidRule;
	}

	public bool HasIsPlayableRule()
	{
		foreach (DeckRule rule in m_rules)
		{
			if (rule.Type == DeckRule.RuleType.IS_CARD_PLAYABLE && !rule.RuleIsNot)
			{
				return true;
			}
		}
		return false;
	}

	public int GetMaxCopiesOfCardAllowed(EntityDef entity)
	{
		int lowestMaxCopies = int.MaxValue;
		foreach (DeckRule rule in m_rules)
		{
			if (rule is DeckRule_CountCopiesOfEachCard && ((DeckRule_CountCopiesOfEachCard)rule).GetMaxCopies(entity, out var ruleMaxCopies))
			{
				lowestMaxCopies = Mathf.Min(lowestMaxCopies, ruleMaxCopies);
			}
		}
		return lowestMaxCopies;
	}

	public HashSet<TAG_CARD_SET> GetAllowedCardSets()
	{
		HashSet<TAG_CARD_SET> cardSets = new HashSet<TAG_CARD_SET>();
		foreach (DeckRule rule in m_rules)
		{
			if (!(rule is DeckRule_IsInAnySubset))
			{
				continue;
			}
			foreach (int id in GameDbf.GetIndex().GetCardSetIdsForSubsetRule(rule.GetID()))
			{
				cardSets.Add((TAG_CARD_SET)id);
			}
			foreach (int subsetId in GameDbf.GetIndex().GetSubsetDbfRecordsForRule(rule.GetID()))
			{
				SubsetDbfRecord subsetDbf = GameDbf.Subset.GetRecord(subsetId);
				if (subsetDbf == null || !subsetDbf.IncludeAllCounterpartCards)
				{
					continue;
				}
				foreach (string subsetCardId in GameDbf.GetIndex().GetSubsetById(subsetId))
				{
					if (GameUtils.IsCardCollectible(subsetCardId))
					{
						TAG_CARD_SET cardSetToAdd = GameUtils.GetCardSetFromCardID(subsetCardId);
						if (cardSetToAdd != TAG_CARD_SET.CORE_HIDDEN && cardSetToAdd != TAG_CARD_SET.VANILLA)
						{
							cardSets.Add(cardSetToAdd);
						}
					}
				}
			}
		}
		return cardSets;
	}

	public List<HashSet<string>> GetCardIdsFromAnySubsetRules()
	{
		List<HashSet<string>> result = new List<HashSet<string>>();
		List<HashSet<string>> disallowedSubsets = new List<HashSet<string>>();
		foreach (DeckRule rule in m_rules)
		{
			if (rule is DeckRule_IsInAnySubset anySubsetRule)
			{
				if (anySubsetRule.RuleIsNot)
				{
					disallowedSubsets.AddRange(anySubsetRule.GetSubsets());
				}
				else
				{
					result.AddRange(anySubsetRule.GetSubsets());
				}
			}
		}
		foreach (HashSet<string> subset in result)
		{
			foreach (HashSet<string> disallowedSubset in disallowedSubsets)
			{
				subset.ExceptWith(disallowedSubset);
			}
		}
		return result;
	}

	public bool EntityIgnoresRuleset(EntityDef def)
	{
		return def.HasTag(GAME_TAG.IGNORE_DECK_RULESET);
	}

	public bool EntityInDeckIgnoresRuleset(CollectionDeck deck)
	{
		DefLoader defLoader = DefLoader.Get();
		List<CollectionDeckSlot> deckSlots = deck.GetSlots();
		int i = 0;
		for (int iMax = deckSlots.Count; i < iMax; i++)
		{
			EntityDef entityDef = defLoader.GetEntityDef(deckSlots[i].CardID);
			if (EntityIgnoresRuleset(entityDef))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsOvercappedDecksEnabled()
	{
		return (NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>())?.OvercappedDecksEnabled ?? false;
	}

	public bool HasMaxTouristsRule(out int maxTourists)
	{
		foreach (DeckRule rule in m_rules)
		{
			if (rule is DeckRule_TouristLimit touristRule)
			{
				maxTourists = touristRule.MaxTourists;
				return true;
			}
		}
		maxTourists = 0;
		return false;
	}
}
