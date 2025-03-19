using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using UnityEngine;

public class CollectibleCardClassFilter : CollectibleCardFilter
{
	private int m_cardsPerPage = 8;

	private Map<TAG_CLASS, List<CollectibleCard>> m_currentResultsByClass = new Map<TAG_CLASS, List<CollectibleCard>>();

	private List<CollectibleCard> m_unfilteredDeathKnightCards = new List<CollectibleCard>();

	private List<CollectibleCard> m_hiddenDeathKnightCards = new List<CollectibleCard>();

	private static CollectionTabInfo[] m_orderedCollectionTabInfos = new CollectionTabInfo[12]
	{
		new CollectionTabInfo
		{
			tagClass = TAG_CLASS.DEATHKNIGHT
		},
		new CollectionTabInfo
		{
			tagClass = TAG_CLASS.DEMONHUNTER
		},
		new CollectionTabInfo
		{
			tagClass = TAG_CLASS.DRUID
		},
		new CollectionTabInfo
		{
			tagClass = TAG_CLASS.HUNTER
		},
		new CollectionTabInfo
		{
			tagClass = TAG_CLASS.MAGE
		},
		new CollectionTabInfo
		{
			tagClass = TAG_CLASS.PALADIN
		},
		new CollectionTabInfo
		{
			tagClass = TAG_CLASS.PRIEST
		},
		new CollectionTabInfo
		{
			tagClass = TAG_CLASS.ROGUE
		},
		new CollectionTabInfo
		{
			tagClass = TAG_CLASS.SHAMAN
		},
		new CollectionTabInfo
		{
			tagClass = TAG_CLASS.WARLOCK
		},
		new CollectionTabInfo
		{
			tagClass = TAG_CLASS.WARRIOR
		},
		new CollectionTabInfo
		{
			tagClass = TAG_CLASS.NEUTRAL
		}
	};

	public bool HasHiddenDeathKnightCards => m_hiddenDeathKnightCards.Count > 0;

	public void Init(int cardsPerPage)
	{
		m_cardsPerPage = cardsPerPage;
		CollectionTabInfo[] orderedCollectionTabInfos = m_orderedCollectionTabInfos;
		for (int i = 0; i < orderedCollectionTabInfos.Length; i++)
		{
			CollectionTabInfo tabOrder = orderedCollectionTabInfos[i];
			if (!m_currentResultsByClass.ContainsKey(tabOrder.tagClass))
			{
				m_currentResultsByClass[tabOrder.tagClass] = new List<CollectibleCard>();
			}
		}
	}

	public override void UpdateResults()
	{
		base.FindCardsResult = GenerateResults();
		List<CollectibleCard> filteredCards = base.FindCardsResult.m_cards;
		foreach (KeyValuePair<TAG_CLASS, List<CollectibleCard>> item in m_currentResultsByClass)
		{
			item.Value.Clear();
		}
		m_unfilteredDeathKnightCards.Clear();
		m_hiddenDeathKnightCards.Clear();
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (RankMgr.IsTwistDeckWithNoSeason(deck))
		{
			return;
		}
		List<TAG_CLASS> classes = new List<TAG_CLASS>();
		foreach (CollectibleCard card in filteredCards)
		{
			card.GetEntityDef().GetClasses(classes);
			foreach (TAG_CLASS cardClass in classes)
			{
				if (m_filterClasses != null && !m_filterClasses.Contains(cardClass))
				{
					continue;
				}
				if (!m_currentResultsByClass.ContainsKey(cardClass))
				{
					Error.AddDevFatal("Card: {0} ({1}) has an invalid class: {2}. Cannot render page.", card.Name, card.CardId, card.Class);
					return;
				}
				if (deck != null)
				{
					List<TAG_CLASS> heroClasses = deck.GetHeroClasses();
					if (classes.Count >= 2 && !heroClasses.Contains(cardClass))
					{
						continue;
					}
				}
				if (cardClass != TAG_CLASS.DEATHKNIGHT)
				{
					m_currentResultsByClass[cardClass].Add(card);
					continue;
				}
				m_unfilteredDeathKnightCards.Add(card);
				EntityDef entityDef = card.GetEntityDef();
				if (entityDef.HasRuneCost)
				{
					RunePattern runePattern = default(RunePattern);
					runePattern.SetCostsFromEntity(entityDef);
					if (!CollectionPageManager.IsShowingLockedRuneCards && deck != null && !deck.CanAddRunes(runePattern, DeckRule_DeathKnightRuneLimit.MaxRuneSlots))
					{
						m_hiddenDeathKnightCards.Add(card);
						continue;
					}
				}
				m_currentResultsByClass[cardClass].Add(card);
			}
		}
	}

	public CollectibleCard GetFirstRuneCard()
	{
		if (m_unfilteredDeathKnightCards.Count <= 0)
		{
			return null;
		}
		CollectibleCard startingCard = m_unfilteredDeathKnightCards[0];
		if (startingCard.Runes.HasRunes)
		{
			return startingCard;
		}
		return GetNextValidDeathKnightCardRight(startingCard, mustHaveRunes: true);
	}

	public List<CollectibleCard> GetCardsForTab(CollectionTabInfo tabInfo)
	{
		if (tabInfo.tagClass == TAG_CLASS.INVALID)
		{
			return null;
		}
		if (!m_currentResultsByClass.TryGetValue(tabInfo.tagClass, out var cards))
		{
			return null;
		}
		return cards;
	}

	public int GetNumPagesForTab(CollectionTabInfo tabInfo)
	{
		List<CollectibleCard> results = GetCardsForTab(tabInfo);
		if (results == null)
		{
			return 0;
		}
		int numPagesForContents = results.Count / m_cardsPerPage + ((results.Count % m_cardsPerPage > 0) ? 1 : 0);
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck != null && deck.GetTouristClasses().Contains(tabInfo.tagClass) && GetDoesAnyCardExistIgnoreOwnership(tabInfo.tagClass))
		{
			return Math.Max(numPagesForContents, 1);
		}
		return numPagesForContents;
	}

	public int GetNumNewCardsForTab(CollectionTabInfo tabInfo)
	{
		return GetCardsForTab(tabInfo)?.Where((CollectibleCard c) => c.IsNewCard).Count() ?? 0;
	}

	public override int GetTotalNumPages()
	{
		int numPages = 0;
		CollectionTabInfo[] orderedCollectionTabInfos = m_orderedCollectionTabInfos;
		foreach (CollectionTabInfo tabInfo in orderedCollectionTabInfos)
		{
			numPages += GetNumPagesForTab(tabInfo);
		}
		return numPages;
	}

	public override List<CollectibleCard> GetPageContents(int page)
	{
		if (page < 0 || page > GetTotalNumPages())
		{
			return new List<CollectibleCard>();
		}
		int maxPageNum = 0;
		CollectionTabInfo[] orderedCollectionTabInfos = m_orderedCollectionTabInfos;
		foreach (CollectionTabInfo tabInfo in orderedCollectionTabInfos)
		{
			int lastClassEndPageIdx = maxPageNum;
			maxPageNum += GetNumPagesForTab(tabInfo);
			if (page <= maxPageNum)
			{
				int pageWithinClass = page - lastClassEndPageIdx;
				int collectionPage;
				return GetPageContentsForTab(tabInfo, pageWithinClass, calculateCollectionPage: false, out collectionPage);
			}
		}
		return new List<CollectibleCard>();
	}

	public CollectionTabInfo GetCurrentTabInfoFromPage(int page)
	{
		if (page < 0 || page > GetTotalNumPages())
		{
			return default(CollectionTabInfo);
		}
		int maxPageNum = 0;
		CollectionTabInfo[] orderedCollectionTabInfos = m_orderedCollectionTabInfos;
		foreach (CollectionTabInfo tabInfo in orderedCollectionTabInfos)
		{
			maxPageNum += GetNumPagesForTab(tabInfo);
			if (page <= maxPageNum)
			{
				return tabInfo;
			}
		}
		return default(CollectionTabInfo);
	}

	public override List<CollectibleCard> GetFirstNonEmptyPage(out int collectionPage)
	{
		collectionPage = 0;
		CollectionTabInfo collectionTabInfo = default(CollectionTabInfo);
		collectionTabInfo.tagClass = TAG_CLASS.NEUTRAL;
		CollectionTabInfo pageTabInfo = collectionTabInfo;
		for (int i = 0; i < m_orderedCollectionTabInfos.Length; i++)
		{
			CollectionTabInfo tabInfo = m_orderedCollectionTabInfos[i];
			if (m_currentResultsByClass[tabInfo.tagClass].Count > 0)
			{
				pageTabInfo = m_orderedCollectionTabInfos[i];
				break;
			}
		}
		return GetPageContentsForTab(pageTabInfo, 1, calculateCollectionPage: true, out collectionPage);
	}

	public List<CollectibleCard> GetPageContentsForTab(CollectionTabInfo pageTabInfo, int pageWithinClass, bool calculateCollectionPage, out int collectionPage)
	{
		collectionPage = 0;
		if (pageWithinClass <= 0 || pageWithinClass > GetNumPagesForTab(pageTabInfo))
		{
			return new List<CollectibleCard>();
		}
		if (calculateCollectionPage)
		{
			for (int i = 0; i < m_orderedCollectionTabInfos.Length; i++)
			{
				CollectionTabInfo tabInfo = m_orderedCollectionTabInfos[i];
				if (tabInfo.tagClass == pageTabInfo.tagClass)
				{
					break;
				}
				collectionPage += GetNumPagesForTab(tabInfo);
			}
			collectionPage += pageWithinClass;
		}
		List<CollectibleCard> classCards = GetCardsForTab(pageTabInfo);
		if (classCards == null)
		{
			return new List<CollectibleCard>();
		}
		return classCards.Skip(m_cardsPerPage * (pageWithinClass - 1)).Take(m_cardsPerPage).ToList();
	}

	public List<CollectibleCard> GetPageContentsForCard(string cardID, TAG_PREMIUM premiumType, out int collectionPage, CollectionTabInfo tabInfoContext)
	{
		collectionPage = 0;
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardID);
		List<TAG_CLASS> cardClasses = new List<TAG_CLASS>();
		entityDef.GetClasses(cardClasses);
		CollectionTabInfo collectionTabInfo = default(CollectionTabInfo);
		collectionTabInfo.tagClass = TAG_CLASS.NEUTRAL;
		CollectionTabInfo pageTabInfo = collectionTabInfo;
		if (cardClasses.Count() == 1)
		{
			pageTabInfo.tagClass = cardClasses.ElementAt(0);
		}
		else if (tabInfoContext.tagClass != 0 && cardClasses.Contains(tabInfoContext.tagClass))
		{
			pageTabInfo = tabInfoContext;
		}
		else
		{
			Debug.LogWarning("CollectibleCardClassFilter.GetPageContentsForCard() - The specified card class mismatches its class context.");
		}
		int cardIdx = GetCardsForTab(pageTabInfo).FindIndex((CollectibleCard obj) => obj.CardId == cardID && obj.PremiumType == premiumType);
		if (cardIdx < 0)
		{
			return new List<CollectibleCard>();
		}
		int cardNum = cardIdx + 1;
		int pageWithinClass = cardNum / m_cardsPerPage + ((cardNum % m_cardsPerPage > 0) ? 1 : 0);
		return GetPageContentsForTab(pageTabInfo, pageWithinClass, calculateCollectionPage: true, out collectionPage);
	}

	public CollectibleCard GetNextValidDeathKnightCardLeft(CollectibleCard startingCard, bool mustHaveRunes = false)
	{
		if (startingCard == null)
		{
			return null;
		}
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck == null)
		{
			return null;
		}
		int startingIndex = m_unfilteredDeathKnightCards.FindIndex((CollectibleCard card) => card.CardId == startingCard.CardId);
		if (startingIndex < 0)
		{
			return null;
		}
		RunePattern deckRunes = deck.Runes;
		for (int i = startingIndex - 1; i >= 0; i--)
		{
			CollectibleCard result = m_unfilteredDeathKnightCards[i];
			RunePattern cardRunes = result.GetEntityDef().GetRuneCost();
			if ((!mustHaveRunes || cardRunes.HasRunes) && deckRunes.CanAddRunes(result.GetEntityDef().GetRuneCost(), DeckRule_DeathKnightRuneLimit.MaxRuneSlots))
			{
				return result;
			}
		}
		return null;
	}

	public CollectibleCard GetNextValidDeathKnightCardRight(CollectibleCard startingCard, bool mustHaveRunes = false)
	{
		if (startingCard == null)
		{
			return null;
		}
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (deck == null)
		{
			return null;
		}
		int startingIndex = m_unfilteredDeathKnightCards.FindIndex((CollectibleCard card) => card.CardId == startingCard.CardId);
		if (startingIndex < 0)
		{
			return null;
		}
		RunePattern deckRunes = deck.Runes;
		for (int i = startingIndex + 1; i < m_unfilteredDeathKnightCards.Count; i++)
		{
			CollectibleCard result = m_unfilteredDeathKnightCards[i];
			RunePattern cardRunes = result.GetEntityDef().GetRuneCost();
			if ((!mustHaveRunes || cardRunes.HasRunes) && deckRunes.CanAddRunes(cardRunes, DeckRule_DeathKnightRuneLimit.MaxRuneSlots))
			{
				return result;
			}
		}
		return null;
	}

	public int GetPageNumberForCard(CollectibleCard card, CollectionTabInfo classContext)
	{
		GetPageContentsForCard(card.CardId, card.PremiumType, out var page, classContext);
		return page;
	}

	public int GetFirstPageForTab(CollectionTabInfo tabInfo)
	{
		List<CollectibleCard> cards = GetCardsForTab(tabInfo);
		if (cards == null)
		{
			return 0;
		}
		CollectibleCard lastCard = cards[0];
		GetPageContentsForCard(lastCard.CardId, lastCard.PremiumType, out var page, tabInfo);
		return page;
	}

	public int GetLastPageForTab(CollectionTabInfo tabInfo)
	{
		List<CollectibleCard> cards = GetCardsForTab(tabInfo);
		if (cards == null)
		{
			return 0;
		}
		CollectibleCard lastCard = cards[cards.Count - 1];
		GetPageContentsForCard(lastCard.CardId, lastCard.PremiumType, out var page, tabInfo);
		return page;
	}
}
