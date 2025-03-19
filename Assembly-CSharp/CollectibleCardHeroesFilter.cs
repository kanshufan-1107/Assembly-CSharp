using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using UnityEngine;

public class CollectibleCardHeroesFilter : CollectibleCardFilter
{
	private static readonly Map<int, int> s_forcedPairs = new Map<int, int>
	{
		{ 7, 57751 },
		{ 1066, 57753 },
		{ 930, 57755 },
		{ 671, 57757 },
		{ 31, 57759 },
		{ 274, 57761 },
		{ 893, 57763 },
		{ 637, 57765 },
		{ 813, 57767 },
		{ 56550, 60238 }
	};

	private static readonly Map<TAG_CLASS, int> s_classOrder = new Map<TAG_CLASS, int>
	{
		{
			TAG_CLASS.DEATHKNIGHT,
			0
		},
		{
			TAG_CLASS.DEMONHUNTER,
			100
		},
		{
			TAG_CLASS.DRUID,
			200
		},
		{
			TAG_CLASS.HUNTER,
			300
		},
		{
			TAG_CLASS.MAGE,
			400
		},
		{
			TAG_CLASS.PALADIN,
			500
		},
		{
			TAG_CLASS.PRIEST,
			600
		},
		{
			TAG_CLASS.ROGUE,
			700
		},
		{
			TAG_CLASS.SHAMAN,
			800
		},
		{
			TAG_CLASS.WARLOCK,
			900
		},
		{
			TAG_CLASS.WARRIOR,
			1000
		},
		{
			TAG_CLASS.NEUTRAL,
			1100
		}
	};

	private static Comparison<CollectibleCard> SortHeroResults = delegate(CollectibleCard a, CollectibleCard b)
	{
		int num = b.OwnedCount.CompareTo(a.OwnedCount);
		if (num == 0)
		{
			int @class = (int)a.Class;
			int class2 = (int)b.Class;
			num = @class.CompareTo(class2);
			if (num == 0)
			{
				num = string.Compare(a.Name, b.Name, ignoreCase: false, Localization.GetCultureInfo());
			}
		}
		return num;
	};

	private const int UNLOCKABLE_SORT_VALUE = 1;

	private const int UNFAVORITED_SORT_VALUE = 1200;

	private const int UNOWNED_PURCHASABLE_SORT_VALUE = 10000;

	private const int UNOWNED_UNPURCHASABLE_SORT_VALUE = 20000;

	private TAG_CLASS[] m_classTabOrder;

	private int m_heroesPerPage = 6;

	private List<CollectibleCard> m_results = new List<CollectibleCard>();

	private List<CollectibleCard> m_unfilteredResults = new List<CollectibleCard>();

	private Map<TAG_CLASS, List<CollectibleCard>> m_currentResultsByClass = new Map<TAG_CLASS, List<CollectibleCard>>();

	public void Init(int heroesPerPage)
	{
		m_heroesPerPage = heroesPerPage;
		FilterHero(isHero: true);
		FilterOnlyOwned(owned: false);
	}

	public override void UpdateResults()
	{
		m_unfilteredResults = GenerateUnOrderedResults().m_cards;
		FilterGoldenHeroes();
		SortResults();
		FilterHeroesByActiveClass();
	}

	public void SortResults()
	{
		m_unfilteredResults.Sort(SortHeroResults);
		m_unfilteredResults = m_unfilteredResults.OrderBy(HeroSkinSortValue).ToList();
		EnforcePairingPositions();
	}

	public void FilterHeroesByActiveClass()
	{
		m_results = new List<CollectibleCard>(m_unfilteredResults);
		CollectionDeck editedDeck = CollectionManager.Get()?.GetEditedDeck();
		if (editedDeck != null)
		{
			TAG_CLASS editedClass = editedDeck.GetClass();
			for (int i = m_results.Count - 1; i > -1; i--)
			{
				if (m_results[i].Class != editedClass)
				{
					m_results.RemoveAt(i);
				}
			}
			return;
		}
		TAG_CLASS? selectedHeroSkinClass = (CollectionManager.Get()?.GetCollectibleDisplay() as CollectionManagerDisplay)?.GetHeroSkinClass();
		if (!selectedHeroSkinClass.HasValue || m_filterText != null)
		{
			return;
		}
		for (int i2 = m_results.Count - 1; i2 > -1; i2--)
		{
			if (m_results[i2].Class != selectedHeroSkinClass)
			{
				m_results.RemoveAt(i2);
			}
		}
	}

	public List<CollectibleCard> GetAllResults()
	{
		return m_results;
	}

	public override List<CollectibleCard> GetPageContents(int page)
	{
		return GetHeroesContents(page);
	}

	public List<CollectibleCard> GetHeroesContents(int currentPage)
	{
		currentPage = Mathf.Min(currentPage, GetTotalNumPages());
		return m_results.Skip(m_heroesPerPage * (currentPage - 1)).Take(m_heroesPerPage).ToList();
	}

	public override List<CollectibleCard> GetFirstNonEmptyPage(out int collectionPage)
	{
		collectionPage = 0;
		for (int i = 0; i < GetTotalNumPages(); i++)
		{
			List<CollectibleCard> cardList = GetHeroesContents(i);
			if (cardList.Count > 0)
			{
				collectionPage = i;
				return cardList;
			}
		}
		return new List<CollectibleCard>();
	}

	public override int GetTotalNumPages()
	{
		int heroCount = m_results.Count;
		return heroCount / m_heroesPerPage + ((heroCount % m_heroesPerPage > 0) ? 1 : 0);
	}

	private int HeroSkinSortValue(CollectibleCard card)
	{
		int sortValue = 0;
		TAG_CLASS cardClass = card.Class;
		int classOrder = s_classOrder[TAG_CLASS.NEUTRAL];
		if (s_classOrder.ContainsKey(cardClass))
		{
			classOrder = s_classOrder[cardClass];
		}
		sortValue += classOrder;
		if (CardBackManager.Get().MultipleFavoriteCardBacksEnabled())
		{
			if (!GameUtils.IsVanillaHero(card.CardId))
			{
				sortValue++;
			}
			if (!CollectionManager.Get().IsFavoriteHero(card.CardId))
			{
				sortValue += 1200;
			}
		}
		if (card.OwnedCount == 0)
		{
			sortValue += (HeroSkinUtils.CanBuyHeroSkinFromCollectionManager(card.CardId) ? 10000 : 20000);
		}
		return sortValue;
	}

	private void FilterGoldenHeroes()
	{
		for (int i = m_unfilteredResults.Count - 1; i > -1; i--)
		{
			CollectibleCard card = m_unfilteredResults[i];
			if (card.PremiumType == TAG_PREMIUM.GOLDEN)
			{
				if (card.OwnedCount == 0)
				{
					m_unfilteredResults.RemoveAt(i);
				}
			}
			else
			{
				CollectibleCard goldenCard = CollectionManager.Get()?.GetCard(card.CardId, TAG_PREMIUM.GOLDEN);
				if (goldenCard != null && goldenCard.OwnedCount > 0)
				{
					m_unfilteredResults.RemoveAt(i);
				}
			}
		}
	}

	private void EnforcePairingPositions()
	{
		foreach (KeyValuePair<int, int> pair in s_forcedPairs)
		{
			int firstIndex = -1;
			int secondIndex = -1;
			int i = 0;
			for (int iMax = m_unfilteredResults.Count; i < iMax; i++)
			{
				if (pair.Key == m_unfilteredResults[i].CardDbId)
				{
					firstIndex = i;
				}
				if (pair.Value == m_unfilteredResults[i].CardDbId)
				{
					secondIndex = i;
				}
				if (firstIndex != -1 && secondIndex != -1)
				{
					break;
				}
			}
			if (firstIndex == -1 || secondIndex == -1)
			{
				continue;
			}
			CollectibleCard first = m_unfilteredResults[firstIndex];
			CollectibleCard second = m_unfilteredResults[secondIndex];
			if ((first.OwnedCount != 0 || second.OwnedCount != 0) && (first.OwnedCount <= 0 || second.OwnedCount <= 0))
			{
				continue;
			}
			CollectionManager collectionManager = CollectionManager.Get();
			if (collectionManager != null)
			{
				bool num = collectionManager.IsFavoriteHero(first.CardId);
				bool secondIsFavorite = collectionManager.IsFavoriteHero(second.CardId);
				if (num != secondIsFavorite)
				{
					continue;
				}
			}
			m_unfilteredResults.RemoveAt(secondIndex);
			m_unfilteredResults.Insert(firstIndex + ((firstIndex <= secondIndex) ? 1 : 0), second);
		}
	}
}
