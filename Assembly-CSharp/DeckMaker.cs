using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core.Utils;
using UnityEngine;

public class DeckMaker
{
	public class DeckChoiceFill
	{
		public EntityDef m_removeTemplate;

		public List<EntityDef> m_addChoices = new List<EntityDef>();

		public string m_reason;

		public DeckChoiceFill(EntityDef remove, params EntityDef[] addChoices)
		{
			m_removeTemplate = remove;
			if (addChoices != null && addChoices.Length != 0)
			{
				m_addChoices = new List<EntityDef>(addChoices);
			}
		}

		public DeckFill GetDeckFillChoice(int idx)
		{
			if (idx >= m_addChoices.Count)
			{
				return null;
			}
			return new DeckFill
			{
				m_removeTemplate = m_removeTemplate,
				m_addCard = m_addChoices[idx],
				m_reason = m_reason
			};
		}
	}

	public class DeckFill
	{
		public EntityDef m_removeTemplate;

		public EntityDef m_addCard;

		public string m_reason;
	}

	public delegate bool CardRequirementsCondition(EntityDef entityDef);

	private class CardRequirements
	{
		public int m_requiredCount;

		public CardRequirementsCondition m_condition;

		private string m_reason;

		public CardRequirements(int requiredCount, CardRequirementsCondition condition, string reason = "")
		{
			m_requiredCount = requiredCount;
			m_condition = condition;
			m_reason = reason;
		}

		public string GetRequirementReason()
		{
			if (string.IsNullOrEmpty(m_reason))
			{
				return "No reason!";
			}
			return GameStrings.Get(m_reason);
		}
	}

	private class SortableEntityDef
	{
		public EntityDef m_entityDef;

		public int m_suggestWeight;
	}

	private static readonly CardRequirements[] s_OrderedCardRequirements = new CardRequirements[6]
	{
		new CardRequirements(8, (EntityDef e) => IsMinion(e) && HasMinCost(e, 1) && HasMaxCost(e, 2), "GLUE_RDM_LOW_COST"),
		new CardRequirements(5, (EntityDef e) => IsMinion(e) && HasMinCost(e, 3) && HasMaxCost(e, 4), "GLUE_RDM_MEDIUM_COST"),
		new CardRequirements(4, (EntityDef e) => IsMinion(e) && HasMinCost(e, 5), "GLUE_RDM_HIGH_COST"),
		new CardRequirements(7, (EntityDef e) => IsSpell(e), "GLUE_RDM_MORE_SPELLS"),
		new CardRequirements(2, (EntityDef e) => IsWeapon(e), "GLUE_RDM_MORE_WEAPONS"),
		new CardRequirements(int.MaxValue, (EntityDef e) => IsMinion(e), "GLUE_RDM_NO_SPECIFICS")
	};

	private static readonly DeckRule.RuleType[] s_ignoreOwned = new DeckRule.RuleType[1] { DeckRule.RuleType.PLAYER_OWNS_EACH_COPY };

	private static bool IsMinion(EntityDef e)
	{
		return e.GetCardType() == TAG_CARDTYPE.MINION;
	}

	private static bool IsSpell(EntityDef e)
	{
		return e.GetCardType() == TAG_CARDTYPE.SPELL;
	}

	private static bool IsWeapon(EntityDef e)
	{
		return e.GetCardType() == TAG_CARDTYPE.WEAPON;
	}

	private static bool HasMinCost(EntityDef e, int minCost)
	{
		return e.GetCost() >= minCost;
	}

	private static bool HasMaxCost(EntityDef e, int maxCost)
	{
		return e.GetCost() <= maxCost;
	}

	public static IEnumerable<DeckFill> GetFillCards(CollectionDeck deck, DeckRuleset deckRuleset)
	{
		bool replaceInvalid = true;
		InitFromDeck(deck, deckRuleset, out var currentDeckCards, out var currentInvalidCards, out var cardsICanAddToDeck);
		int maxDeckSize = deckRuleset?.GetDeckSize(deck) ?? CollectionManager.Get().GetDeckSize();
		int remainingCardsToFill = maxDeckSize - currentDeckCards.Count;
		if (remainingCardsToFill <= 0)
		{
			yield break;
		}
		if (replaceInvalid)
		{
			foreach (DeckFill replace in GetInvalidFillCards(cardsICanAddToDeck, currentDeckCards, currentInvalidCards))
			{
				remainingCardsToFill--;
				yield return replace;
			}
		}
		int i = 0;
		while (i < s_OrderedCardRequirements.Length)
		{
			if (remainingCardsToFill <= 0)
			{
				break;
			}
			CardRequirements cardReq = s_OrderedCardRequirements[i];
			CardRequirementsCondition condition = cardReq.m_condition;
			int cardsInDeckThatMatchReq = currentDeckCards.FindAll((EntityDef e) => condition(e)).Count;
			int cardsToAddFromSet = Mathf.Min(cardReq.m_requiredCount - cardsInDeckThatMatchReq, remainingCardsToFill);
			int num;
			if (cardsToAddFromSet > 0)
			{
				List<EntityDef> filteredCardList = cardsICanAddToDeck.FindAll((EntityDef e) => condition(e));
				foreach (EntityDef addCard in filteredCardList)
				{
					if (cardsToAddFromSet <= 0)
					{
						break;
					}
					cardsICanAddToDeck.Remove(addCard);
					currentDeckCards.Add(addCard);
					if (addCard.HasTag(GAME_TAG.DECK_RULE_MOD_DECK_SIZE))
					{
						int newDeckSize = addCard.GetTag(GAME_TAG.DECK_RULE_MOD_DECK_SIZE);
						int deltaDeckSize = newDeckSize - maxDeckSize;
						remainingCardsToFill += deltaDeckSize;
						maxDeckSize = newDeckSize;
					}
					num = cardsToAddFromSet - 1;
					cardsToAddFromSet = num;
					num = remainingCardsToFill - 1;
					remainingCardsToFill = num;
					yield return new DeckFill
					{
						m_removeTemplate = null,
						m_addCard = addCard,
						m_reason = cardReq.GetRequirementReason()
					};
				}
			}
			num = i + 1;
			i = num;
		}
		i = 0;
		while (i < cardsICanAddToDeck.Count)
		{
			EntityDef addCard2 = cardsICanAddToDeck[i];
			if (addCard2 != null)
			{
				currentDeckCards.Add(addCard2);
				cardsICanAddToDeck[i] = null;
				yield return new DeckFill
				{
					m_removeTemplate = null,
					m_addCard = addCard2,
					m_reason = null
				};
			}
			int num = i + 1;
			i = num;
		}
	}

	public static DeckChoiceFill GetFillCardChoices(CollectionDeck deck, EntityDef referenceCard, int choices, DeckRuleset deckRuleset = null)
	{
		if (deckRuleset == null)
		{
			deckRuleset = deck.GetRuleset(null);
		}
		InitFromDeck(deck, deckRuleset, out var currentDeckCards, out var currentInvalidCards, out var cardsICanAddToDeck);
		return GetFillCard(referenceCard, cardsICanAddToDeck, currentDeckCards, currentInvalidCards, choices);
	}

	private static void InitFromDeck(CollectionDeck deck, DeckRuleset deckRuleset, out List<EntityDef> currentDeckCards, out List<EntityDef> currentInvalidCards, out List<EntityDef> distinctCardsICanAddToDeck)
	{
		CollectionManager cm = CollectionManager.Get();
		List<SortableEntityDef> entitiesICanAdd = new List<SortableEntityDef>();
		currentDeckCards = new List<EntityDef>();
		currentInvalidCards = new List<EntityDef>();
		bool isEvenDeck = false;
		bool isOddDeck = false;
		foreach (CollectionDeckSlot slot in deck.GetSlots())
		{
			foreach (TAG_PREMIUM premium in Enum.GetValues(typeof(TAG_PREMIUM)))
			{
				int slotCount = slot.GetCount(premium);
				if (slotCount <= 0)
				{
					continue;
				}
				CollectibleCard card = CollectionManager.Get().GetCard(slot.CardID, premium);
				if (card == null)
				{
					continue;
				}
				EntityDef entityDef = card.GetEntityDef();
				for (int i = 0; i < slotCount; i++)
				{
					if (deck.IsValidSlot(slot, ignoreOwnership: false, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: false, null))
					{
						currentDeckCards.Add(entityDef);
					}
					else
					{
						currentInvalidCards.Add(entityDef);
					}
				}
				if (entityDef.IsCollectionManagerFilterManaCostByEven)
				{
					isEvenDeck = true;
				}
				if (entityDef.IsCollectionManagerFilterManaCostByOdd)
				{
					isOddDeck = true;
				}
			}
		}
		foreach (KeyValuePair<string, SideboardDeck> allSideboard in deck.GetAllSideboards())
		{
			foreach (CollectionDeckSlot slot2 in allSideboard.Value.GetSlots())
			{
				foreach (TAG_PREMIUM premium2 in Enum.GetValues(typeof(TAG_PREMIUM)))
				{
					int slotCount2 = slot2.GetCount(premium2);
					if (slotCount2 <= 0)
					{
						continue;
					}
					CollectibleCard card2 = CollectionManager.Get().GetCard(slot2.CardID, premium2);
					if (card2 == null)
					{
						continue;
					}
					EntityDef entityDef2 = card2.GetEntityDef();
					for (int j = 0; j < slotCount2; j++)
					{
						if (deck.IsValidSlot(slot2, ignoreOwnership: false, ignoreGameplayEvent: false, enforceRemainingDeckRuleset: false, null))
						{
							currentDeckCards.Add(entityDef2);
						}
						else
						{
							currentInvalidCards.Add(entityDef2);
						}
					}
				}
			}
		}
		if (isEvenDeck && isOddDeck)
		{
			isEvenDeck = false;
			isOddDeck = false;
		}
		foreach (KeyValuePair<string, EntityDef> kvpair in DefLoader.Get().GetAllEntityDefs())
		{
			CollectibleCard normal = cm.GetCard(kvpair.Key, TAG_PREMIUM.NORMAL);
			if (normal == null || normal.IsHeroSkin || (!normal.GetEntityDef().HasClass(deck.GetClass()) && normal.Class != TAG_CLASS.NEUTRAL) || (deckRuleset != null && !deckRuleset.Filter(kvpair.Value, deck)) || (kvpair.Value.HasRuneCost && deckRuleset != null && !deckRuleset.CanAddToDeck(kvpair.Value, TAG_PREMIUM.NORMAL, deck, s_ignoreOwned)) || (normal.Set != TAG_CARD_SET.CORE && CollectionManager.Get().HasCoreOrWondersCounterpart(normal)) || RankMgr.Get().IsCardLockedInCurrentLeague(normal.GetEntityDef()) || (isEvenDeck && normal.ManaCost % 2 != 0) || (isOddDeck && normal.ManaCost % 2 == 0) || !GameUtils.IsCardSetFilterEventActive(normal.CardId))
			{
				continue;
			}
			int maxCopiesToSuggestToAdd = 2;
			if (deckRuleset != null)
			{
				maxCopiesToSuggestToAdd = Mathf.Min(2, deckRuleset.GetMaxCopiesOfCardAllowed(kvpair.Value));
			}
			int totalOwnedCount = cm.GetTotalOwnedCount(kvpair.Key);
			int copiesInDeck = currentDeckCards.FindAll((EntityDef e) => e == kvpair.Value).Count;
			int copiesAlreadySuggested = 0;
			List<string> counterpartCardIds = GameUtils.GetCounterpartCardIds(kvpair.Value.GetCardId());
			if (counterpartCardIds != null)
			{
				foreach (SortableEntityDef entitiyICanAdd in entitiesICanAdd)
				{
					foreach (string counterpartCardId in counterpartCardIds)
					{
						if (entitiyICanAdd.m_entityDef.GetCardId() == counterpartCardId)
						{
							copiesAlreadySuggested++;
							break;
						}
					}
				}
			}
			int addCopies = Mathf.Min(maxCopiesToSuggestToAdd, totalOwnedCount) - (copiesInDeck + copiesAlreadySuggested);
			for (int k = 0; k < addCopies; k++)
			{
				entitiesICanAdd.Add(new SortableEntityDef
				{
					m_entityDef = kvpair.Value,
					m_suggestWeight = normal.SuggestWeight
				});
			}
		}
		int randomizer = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		entitiesICanAdd.Sort(delegate(SortableEntityDef lhs, SortableEntityDef rhs)
		{
			int num = rhs.m_suggestWeight - lhs.m_suggestWeight;
			return (num != 0) ? num : ((lhs.GetHashCode() ^ randomizer) - (rhs.GetHashCode() ^ randomizer));
		});
		distinctCardsICanAddToDeck = new List<EntityDef>();
		foreach (SortableEntityDef sortEntityDef in entitiesICanAdd)
		{
			distinctCardsICanAddToDeck.Add(sortEntityDef.m_entityDef);
		}
	}

	private static IEnumerable<DeckFill> GetInvalidFillCards(List<EntityDef> cardsICanAddToDeck, List<EntityDef> currentDeckCards, List<EntityDef> currentInvalidCards)
	{
		EntityDef[] tplCards = currentInvalidCards.ToArray();
		int i = 0;
		while (i < tplCards.Length)
		{
			DeckFill choice = GetFillCard(tplCards[i], cardsICanAddToDeck, null, currentInvalidCards, 1).GetDeckFillChoice(0);
			if (ReplaceInvalidCard(choice, cardsICanAddToDeck, currentDeckCards, currentInvalidCards))
			{
				yield return choice;
			}
			int num = i + 1;
			i = num;
		}
	}

	private static bool ReplaceInvalidCard(DeckFill choice, List<EntityDef> cardsICanAddToDeck, List<EntityDef> currentDeckCards, List<EntityDef> currentInvalidCards)
	{
		if (choice == null)
		{
			return false;
		}
		if (!currentInvalidCards.Remove(choice.m_removeTemplate))
		{
			return false;
		}
		cardsICanAddToDeck.Remove(choice.m_addCard);
		currentDeckCards.Add(choice.m_addCard);
		return true;
	}

	private static DeckChoiceFill GetFillCard(EntityDef referenceCard, List<EntityDef> cardsICanAddToDeck, List<EntityDef> currentDeckCards, List<EntityDef> currentInvalidCards, int totalNumChoices = 3)
	{
		if (referenceCard == null && currentInvalidCards != null && currentInvalidCards.Count > 0)
		{
			referenceCard = currentInvalidCards.First();
		}
		int cardRequirementsStartIndex = GetCardRequirementsStartIndex(referenceCard, currentDeckCards);
		DeckChoiceFill result = new DeckChoiceFill(referenceCard);
		CollectionManager collectionManager = CollectionManager.Get();
		for (int i = cardRequirementsStartIndex; i < s_OrderedCardRequirements.Length; i++)
		{
			if (totalNumChoices <= 0)
			{
				break;
			}
			CardRequirements cardReq = s_OrderedCardRequirements[i];
			CardRequirementsCondition condition = cardReq.m_condition;
			List<EntityDef> filteredCardList = cardsICanAddToDeck.FindAll((EntityDef e) => condition(e));
			if (filteredCardList.Count <= 0)
			{
				continue;
			}
			int numChoiceCount = 8;
			List<EntityDef> priorityRandomizedChoices = new List<EntityDef>();
			List<EntityDef> otherRandomizedChoices = new List<EntityDef>();
			int bestWeight = int.MinValue;
			foreach (EntityDef addCard in filteredCardList.Distinct())
			{
				TAG_PREMIUM premium = collectionManager.GetBestCardPremium(addCard.GetCardId());
				CollectibleCard card = collectionManager.GetCard(addCard.GetCardId(), premium);
				bestWeight = Mathf.Max(bestWeight, card.SuggestWeight);
			}
			foreach (EntityDef addCard2 in filteredCardList.Distinct())
			{
				if (numChoiceCount <= 0)
				{
					break;
				}
				TAG_PREMIUM premium2 = collectionManager.GetBestCardPremium(addCard2.GetCardId());
				CollectibleCard card2 = collectionManager.GetCard(addCard2.GetCardId(), premium2);
				if (bestWeight - card2.SuggestWeight > 100)
				{
					otherRandomizedChoices.Add(addCard2);
				}
				else
				{
					priorityRandomizedChoices.Add(addCard2);
				}
				numChoiceCount--;
			}
			GeneralUtils.Shuffle(priorityRandomizedChoices);
			GeneralUtils.Shuffle(otherRandomizedChoices);
			int prioritySuggestions = Mathf.Min(priorityRandomizedChoices.Count, totalNumChoices);
			int otherSuggestions = Mathf.Min(otherRandomizedChoices.Count, totalNumChoices - prioritySuggestions);
			if (prioritySuggestions > 0)
			{
				result.m_addChoices.AddRange(priorityRandomizedChoices.GetRange(0, prioritySuggestions));
			}
			if (otherSuggestions > 0)
			{
				result.m_addChoices.AddRange(otherRandomizedChoices.GetRange(0, otherSuggestions));
			}
			totalNumChoices -= prioritySuggestions + otherSuggestions;
			result.m_reason = ((referenceCard == null) ? cardReq.GetRequirementReason() : GameStrings.Format("GLUE_RDM_TEMPLATE_REPLACE", referenceCard.GetName()));
		}
		return result;
	}

	private static int GetCardRequirementsStartIndex(EntityDef referenceCard, List<EntityDef> currentDeckCards)
	{
		if (referenceCard != null)
		{
			for (int i = 0; i < s_OrderedCardRequirements.Length; i++)
			{
				if (s_OrderedCardRequirements[i].m_condition(referenceCard))
				{
					return i;
				}
			}
		}
		else if (currentDeckCards != null)
		{
			for (int j = 0; j < s_OrderedCardRequirements.Length; j++)
			{
				CardRequirements cardReq = s_OrderedCardRequirements[j];
				CardRequirementsCondition condition = cardReq.m_condition;
				if (currentDeckCards.FindAll((EntityDef e) => condition(e)).Count < cardReq.m_requiredCount)
				{
					return j;
				}
			}
		}
		return 0;
	}
}
