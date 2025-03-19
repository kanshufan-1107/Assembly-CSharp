using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;

public class CollectibleCardRoleFilter : CollectibleCardFilter
{
	public struct SearchTerms
	{
		public bool Missing;

		public bool Owned;
	}

	private int m_cardsPerPage = 8;

	private TAG_ROLE[] m_roleTabOrder;

	private Map<TAG_ROLE, List<LettuceMercenary>> m_currentResultsByRole = new Map<TAG_ROLE, List<LettuceMercenary>>();

	private bool? m_filterOwned = true;

	private bool? m_filterOnlyUpgradeable;

	public bool? m_filterOnlyFulIyUpgraded { get; private set; }

	public CollectionManager.FindMercenariesResult FindMercenariesResult { get; protected set; }

	public void Init(TAG_ROLE[] roleTabOrder, int cardsPerPage)
	{
		m_roleTabOrder = roleTabOrder;
		m_cardsPerPage = cardsPerPage;
		for (int i = 0; i < roleTabOrder.Length; i++)
		{
			m_currentResultsByRole[roleTabOrder[i]] = new List<LettuceMercenary>();
		}
	}

	public CollectionManager.FindMercenariesResult GenerateMercenariesResults()
	{
		CollectionManager collectionManager = CollectionManager.Get();
		string filterText = m_filterText;
		bool? filterOwned = m_filterOwned;
		bool? filterOnlyUpgradeable = m_filterOnlyUpgradeable;
		bool? filterOnlyFulIyUpgraded = m_filterOnlyFulIyUpgraded;
		return collectionManager.FindMercenaries(filterText, filterOwned, filterOnlyUpgradeable, null, null, ordered: true, filterOnlyFulIyUpgraded);
	}

	public override void UpdateResults()
	{
		FindMercenariesResult = GenerateMercenariesResults();
		List<LettuceMercenary> filteredMercenaries = FindMercenariesResult.m_mercenaries;
		foreach (KeyValuePair<TAG_ROLE, List<LettuceMercenary>> item in m_currentResultsByRole)
		{
			item.Value.Clear();
		}
		foreach (LettuceMercenary mercenary in filteredMercenaries)
		{
			if (m_filterRoles == null || m_filterRoles.Contains(mercenary.m_role))
			{
				if (!m_currentResultsByRole.ContainsKey(mercenary.m_role))
				{
					Error.AddDevFatal("Mercenary: {0} ({1}) has an invalid role: {2}. Cannot render page.", mercenary.m_mercName, mercenary.ID, mercenary.m_role);
					break;
				}
				m_currentResultsByRole[mercenary.m_role].Add(mercenary);
			}
		}
	}

	public override void FilterOnlyOwned(bool owned)
	{
		base.FilterOnlyOwned(owned);
		m_filterOwned = null;
		if (owned)
		{
			m_filterOwned = owned;
		}
	}

	public void FilterOnlyUpgradeableMercs(bool onlyUpgradeable)
	{
		m_filterOnlyUpgradeable = null;
		if (onlyUpgradeable)
		{
			m_filterOnlyUpgradeable = onlyUpgradeable;
		}
	}

	public void FilterOnlyFullyUpgraded(bool fullyUpgraded)
	{
		m_filterOnlyFulIyUpgraded = null;
		if (fullyUpgraded)
		{
			m_filterOnlyFulIyUpgraded = fullyUpgraded;
		}
	}

	public int GetNumPagesForRole(TAG_ROLE cardRole)
	{
		if (!m_currentResultsByRole.TryGetValue(cardRole, out var results))
		{
			return 0;
		}
		int roleCards = results.Count;
		return roleCards / m_cardsPerPage + ((roleCards % m_cardsPerPage > 0) ? 1 : 0);
	}

	public int GetNumNewCardsForRole(TAG_ROLE cardRole)
	{
		return CollectionManager.Get().GetNumMercenariesToAcknowledgeForRole(cardRole);
	}

	public override int GetTotalNumPages()
	{
		int numPages = 0;
		TAG_ROLE[] roleTabOrder = m_roleTabOrder;
		foreach (TAG_ROLE cardRole in roleTabOrder)
		{
			numPages += GetNumPagesForRole(cardRole);
		}
		return numPages;
	}

	public List<LettuceMercenary> GetMercenariesPageContents(int page)
	{
		if (page < 0 || page > GetTotalNumPages())
		{
			return new List<LettuceMercenary>();
		}
		int maxPageNum = 0;
		for (int i = 0; i < m_roleTabOrder.Length; i++)
		{
			int lastRoleEndPageIdx = maxPageNum;
			TAG_ROLE cardRole = m_roleTabOrder[i];
			maxPageNum += GetNumPagesForRole(cardRole);
			if (page <= maxPageNum)
			{
				int pageWithinClass = page - lastRoleEndPageIdx;
				int collectionPage;
				return GetPageContentsForRole(cardRole, pageWithinClass, calculateCollectionPage: false, out collectionPage);
			}
		}
		return new List<LettuceMercenary>();
	}

	public override List<CollectibleCard> GetPageContents(int page)
	{
		return new List<CollectibleCard>();
	}

	public TAG_ROLE GetCurrentRoleFromPage(int page)
	{
		if (page < 0 || page > GetTotalNumPages())
		{
			return TAG_ROLE.INVALID;
		}
		int maxPageNum = 0;
		for (int i = 0; i < m_roleTabOrder.Length; i++)
		{
			TAG_ROLE cardRole = m_roleTabOrder[i];
			maxPageNum += GetNumPagesForRole(cardRole);
			if (page <= maxPageNum)
			{
				return cardRole;
			}
		}
		return TAG_ROLE.INVALID;
	}

	public List<LettuceMercenary> GetFirstNonEmptyMercenaryPage(out int collectionPage)
	{
		collectionPage = 0;
		TAG_ROLE pageRole = TAG_ROLE.FIGHTER;
		for (int i = 0; i < m_roleTabOrder.Length; i++)
		{
			if (m_currentResultsByRole[m_roleTabOrder[i]].Count > 0)
			{
				pageRole = m_roleTabOrder[i];
				break;
			}
		}
		return GetPageContentsForRole(pageRole, 1, calculateCollectionPage: true, out collectionPage);
	}

	public override List<CollectibleCard> GetFirstNonEmptyPage(out int collectionPage)
	{
		collectionPage = 0;
		return new List<CollectibleCard>();
	}

	public List<LettuceMercenary> GetPageContentsForRole(TAG_ROLE pageRole, int pageWithinRole, bool calculateCollectionPage, out int collectionPage)
	{
		collectionPage = 0;
		if (pageWithinRole <= 0 || pageWithinRole > GetNumPagesForRole(pageRole))
		{
			return new List<LettuceMercenary>();
		}
		if (calculateCollectionPage)
		{
			for (int i = 0; i < m_roleTabOrder.Length; i++)
			{
				TAG_ROLE cardRole = m_roleTabOrder[i];
				if (cardRole == pageRole)
				{
					break;
				}
				collectionPage += GetNumPagesForRole(cardRole);
			}
			collectionPage += pageWithinRole;
		}
		List<LettuceMercenary> roleCards = m_currentResultsByRole[pageRole];
		if (roleCards == null)
		{
			return new List<LettuceMercenary>();
		}
		return roleCards.Skip(m_cardsPerPage * (pageWithinRole - 1)).Take(m_cardsPerPage).ToList();
	}

	public List<LettuceMercenary> GetPageContentsForMercenary(LettuceMercenary merc, out int collectionPage)
	{
		collectionPage = 0;
		TAG_ROLE pageRole = merc.m_role;
		int mercIdx = m_currentResultsByRole[pageRole].FindIndex((LettuceMercenary m) => m.ID == merc.ID);
		if (mercIdx < 0)
		{
			return new List<LettuceMercenary>();
		}
		int mercNum = mercIdx + 1;
		int pageWithinRole = mercNum / m_cardsPerPage + ((mercNum % m_cardsPerPage > 0) ? 1 : 0);
		return GetPageContentsForRole(pageRole, pageWithinRole, calculateCollectionPage: true, out collectionPage);
	}

	public List<LettuceMercenary> GetAllRoleResults()
	{
		List<LettuceMercenary> results = new List<LettuceMercenary>();
		foreach (KeyValuePair<TAG_ROLE, List<LettuceMercenary>> item in m_currentResultsByRole)
		{
			results.AddRange(item.Value);
		}
		return results;
	}

	public static List<CollectionManager.MercenaryFilterFunc> FilterMercsFromSearchString(string searchString, ref SearchTerms setSearchTerms)
	{
		List<CollectionManager.MercenaryFilterFunc> filterFuncs = new List<CollectionManager.MercenaryFilterFunc>();
		string artist = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_ARTIST");
		string health = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_HEALTH");
		string attack = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_ATTACK");
		string owned = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_OWNED");
		string type = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_TYPE");
		string missing = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MISSING");
		string newcards = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_NEW");
		string has = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_HAS");
		string maxLevel = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MAX_LEVEL");
		string maxLevel_alt = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MAX_LEVEL_ALT");
		string golden = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_GOLDEN");
		string diamond = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_DIAMOND");
		string craftable = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_CRAFTABLE");
		string upgradable = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_UPGRADABLE");
		string[] lowerTokens = searchString.ToLower().Split(CollectibleFilteredSet<ICollectible>.SearchTokenDelimiters, StringSplitOptions.RemoveEmptyEntries);
		StringBuilder regularTokens = new StringBuilder();
		for (int i = 0; i < lowerTokens.Length; i++)
		{
			if (lowerTokens[i] == missing)
			{
				setSearchTerms.Missing = true;
				continue;
			}
			if (lowerTokens[i] == owned)
			{
				setSearchTerms.Owned = true;
				continue;
			}
			if (lowerTokens[i] == golden)
			{
				filterFuncs.Add((LettuceMercenary card) => card.GetEquippedArtVariation().m_premium == TAG_PREMIUM.GOLDEN);
				continue;
			}
			if (lowerTokens[i] == diamond)
			{
				filterFuncs.Add((LettuceMercenary card) => card.GetEquippedArtVariation().m_premium == TAG_PREMIUM.DIAMOND);
				continue;
			}
			if (lowerTokens[i] == maxLevel || lowerTokens[i] == maxLevel_alt)
			{
				filterFuncs.Add((LettuceMercenary card) => card.IsMaxLevel());
				continue;
			}
			if (lowerTokens[i].Contains(craftable))
			{
				filterFuncs.Add((LettuceMercenary card) => card.IsReadyForCrafting());
				continue;
			}
			if (lowerTokens[i].Contains(upgradable))
			{
				filterFuncs.Add((LettuceMercenary card) => card.CanAnyCardBeUpgraded());
				continue;
			}
			if (lowerTokens[i] == newcards)
			{
				filterFuncs.Add((LettuceMercenary card) => CollectionManager.Get().DoesMercenaryNeedToBeAcknowledged(card));
				continue;
			}
			bool didTagMatch = false;
			if (CollectibleFilteredSet<ICollectible>.SearchTagColons.Any(lowerTokens[i].Contains))
			{
				string[] tagTokens = lowerTokens[i].Split(CollectibleFilteredSet<ICollectible>.SearchTagColons);
				if (tagTokens.Length == 2)
				{
					string tag = tagTokens[0].Trim();
					string val = tagTokens[1].Trim();
					GeneralUtils.ParseNumericRange(val, out var isNumericalValue, out var minVal, out var maxVal);
					if (isNumericalValue)
					{
						if (tag == attack)
						{
							filterFuncs.Add((LettuceMercenary card) => card.m_attack >= minVal && card.m_attack <= maxVal);
							didTagMatch = true;
						}
						if (tag == health)
						{
							filterFuncs.Add((LettuceMercenary card) => card.m_health >= minVal && card.m_health <= maxVal);
							didTagMatch = true;
						}
					}
					else
					{
						if (tag == artist)
						{
							filterFuncs.Add((LettuceMercenary card) => SearchableString.SearchInternationalText(val, card.GetCollectibleCard().ArtistName));
							filterFuncs.Add((LettuceMercenary card) => SearchableString.SearchInternationalText(val, card.GetCollectibleCard().SignatureArtistName));
							didTagMatch = true;
						}
						if (tag == type)
						{
							filterFuncs.Add(delegate(LettuceMercenary card)
							{
								string cardTypeName = GameStrings.GetCardTypeName(card.GetCollectibleCard().CardType);
								return cardTypeName != null && SearchableString.SearchInternationalText(val, cardTypeName);
							});
							didTagMatch = true;
						}
						if (tag == has)
						{
							filterFuncs.Add((LettuceMercenary card) => card.FindTextInCard(val));
							didTagMatch = true;
						}
						if (tag == attack)
						{
							string even = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_EVEN_ATTACK").ToLower();
							string odd = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_ODD_ATTACK").ToLower();
							string lowerVal = val.ToLower();
							if (lowerVal == even)
							{
								filterFuncs.Add((LettuceMercenary card) => card.m_attack % 2 == 0);
								didTagMatch = true;
							}
							else if (lowerVal == odd)
							{
								filterFuncs.Add((LettuceMercenary card) => card.m_attack % 2 == 1);
								didTagMatch = true;
							}
						}
						if (tag == health)
						{
							string even2 = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_EVEN_HEALTH").ToLower();
							string odd2 = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_ODD_HEALTH").ToLower();
							string lowerVal2 = val.ToLower();
							if (lowerVal2 == even2)
							{
								filterFuncs.Add((LettuceMercenary card) => card.m_health % 2 == 0);
								didTagMatch = true;
							}
							else if (lowerVal2 == odd2)
							{
								filterFuncs.Add((LettuceMercenary card) => card.m_health % 2 == 1);
								didTagMatch = true;
							}
						}
					}
				}
			}
			if (!didTagMatch)
			{
				regularTokens.Append(lowerTokens[i]);
				regularTokens.Append(" ");
			}
		}
		filterFuncs.Add((LettuceMercenary card) => card.FindTextInCard(regularTokens.ToString()));
		return filterFuncs;
	}
}
