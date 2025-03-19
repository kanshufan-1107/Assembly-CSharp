using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PegasusShared;

public abstract class CollectibleCardFilter : CollectibleFilteredSet<CollectibleCard>
{
	[Flags]
	public enum FilterMask
	{
		NONE = 0,
		PREMIUM_NORMAL = 2,
		PREMIUM_GOLDEN = 4,
		PREMIUM_DIAMOND = 8,
		PREMIUM_SIGNATURE = 0x10,
		PREMIUM_ALL = 0x1E,
		OWNED = 0x20,
		UNOWNED = 0x40,
		ALL = -1
	}

	protected string m_extraToken = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_EXTRA");

	protected string m_favoriteToken = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_FAVORITE");

	private TAG_CARD_SET[] m_filterCardSets;

	protected TAG_CLASS[] m_filterClasses;

	private TAG_CARDTYPE[] m_filterCardTypes;

	protected TAG_ROLE[] m_filterRoles;

	private int? m_filterManaCost;

	private int? m_filterOwnedMinimum = 1;

	private List<FilterMask> m_filterMasks;

	private bool? m_craftableFilterValue;

	protected string m_filterText;

	private bool m_filterIsHero;

	private DeckRuleset m_deckRuleset;

	private HashSet<string> m_leagueBannedCardsSubset;

	private List<int> m_specificCards;

	private bool? m_filterCounterpartCards = true;

	private List<HashSet<string>> m_filterSubsets;

	public CollectionManager.FindCardsResult FindCardsResult { get; protected set; }

	public bool IsManaCostFilterActive => m_filterManaCost.HasValue;

	public bool IsSingleSetFilterActive
	{
		get
		{
			if (m_filterCardSets == null)
			{
				return true;
			}
			return m_filterCardSets.Length == 1;
		}
	}

	public static FilterMask FilterMaskFromPremiumType(TAG_PREMIUM premiumType)
	{
		FilterMask mask = FilterMask.NONE;
		return premiumType switch
		{
			TAG_PREMIUM.GOLDEN => mask | FilterMask.PREMIUM_GOLDEN, 
			TAG_PREMIUM.DIAMOND => mask | FilterMask.PREMIUM_DIAMOND, 
			TAG_PREMIUM.SIGNATURE => mask | FilterMask.PREMIUM_SIGNATURE, 
			_ => mask | FilterMask.PREMIUM_NORMAL, 
		};
	}

	public abstract void UpdateResults();

	public abstract int GetTotalNumPages();

	public abstract List<CollectibleCard> GetFirstNonEmptyPage(out int collectionPage);

	public void SetDeckRuleset(DeckRuleset deckRuleset)
	{
		m_deckRuleset = deckRuleset;
	}

	public void FilterTheseCardSets(params TAG_CARD_SET[] cardSets)
	{
		m_filterCardSets = null;
		if (cardSets != null && cardSets.Length != 0)
		{
			m_filterCardSets = cardSets;
		}
	}

	public void FilterTheseSubsets(List<HashSet<string>> subsets)
	{
		m_filterSubsets = null;
		if (subsets != null && subsets.Count > 0)
		{
			m_filterSubsets = subsets;
		}
	}

	public bool CardSetFilterIncludesWild()
	{
		if (m_filterCardSets == null && m_specificCards == null)
		{
			return true;
		}
		if (m_filterCardSets != null)
		{
			TAG_CARD_SET[] filterCardSets = m_filterCardSets;
			for (int i = 0; i < filterCardSets.Length; i++)
			{
				if (GameUtils.IsWildCardSet(filterCardSets[i]))
				{
					return true;
				}
			}
		}
		if (m_specificCards != null)
		{
			foreach (int specificCard in m_specificCards)
			{
				if (GameUtils.IsWildCard(specificCard))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CardSetFilterIsAllStandardSets()
	{
		if (m_filterCardSets == null)
		{
			return false;
		}
		List<TAG_CARD_SET> standardSets = new List<TAG_CARD_SET>(GameUtils.GetStandardSets());
		return new HashSet<TAG_CARD_SET>(m_filterCardSets).SetEquals(standardSets);
	}

	public bool CardSetFilterIsClassicSet()
	{
		if (m_filterCardSets == null)
		{
			return false;
		}
		List<TAG_CARD_SET> classicSet = new List<TAG_CARD_SET> { TAG_CARD_SET.VANILLA };
		return new HashSet<TAG_CARD_SET>(m_filterCardSets).SetEquals(classicSet);
	}

	public bool CardSetFilterIsTwistSet()
	{
		if (m_filterCardSets == null)
		{
			return false;
		}
		List<TAG_CARD_SET> twistSet = GameUtils.GetTwistSets().ToList();
		return new HashSet<TAG_CARD_SET>(m_filterCardSets).SetEquals(twistSet);
	}

	public void FilterTheseClasses(params TAG_CLASS[] classTypes)
	{
		m_filterClasses = null;
		if (classTypes != null && classTypes.Length != 0)
		{
			m_filterClasses = classTypes;
		}
	}

	public void FilterManaCost(int? manaCost)
	{
		m_filterManaCost = manaCost;
	}

	public virtual void FilterOnlyOwned(bool owned)
	{
		m_filterOwnedMinimum = null;
		if (owned)
		{
			m_filterOwnedMinimum = 1;
		}
	}

	public void FilterByMask(List<FilterMask> filterMasks)
	{
		if (filterMasks == null)
		{
			filterMasks = new List<FilterMask> { FilterMask.ALL };
		}
		m_filterMasks = filterMasks;
	}

	public void FilterByCraftability(bool? isCraftable)
	{
		m_craftableFilterValue = isCraftable;
	}

	public void FilterLeagueBannedCardsSubset(HashSet<string> leagueBannedCardsSubset)
	{
		m_leagueBannedCardsSubset = leagueBannedCardsSubset;
	}

	public void FilterSearchText(string searchText)
	{
		m_filterText = searchText;
	}

	public bool HasSearchText()
	{
		return !string.IsNullOrEmpty(m_filterText);
	}

	public void FilterHero(bool isHero)
	{
		m_filterIsHero = isHero;
	}

	public void FilterSpecificCards(List<int> specificCards)
	{
		m_specificCards = specificCards.Where((int x) => GameUtils.IsCardCollectible(GameUtils.TranslateDbIdToCardId(x))).ToList();
	}

	public CollectionManager.FindCardsResult GenerateResults()
	{
		CollectionManager collectionManager = CollectionManager.Get();
		string filterText = m_filterText;
		int? filterManaCost = m_filterManaCost;
		List<FilterMask> filterMasks = m_filterMasks;
		TAG_CARD_SET[] filterCardSets = m_filterCardSets;
		TAG_CLASS[] filterClasses = m_filterClasses;
		TAG_CARDTYPE[] filterCardTypes = m_filterCardTypes;
		TAG_ROLE[] filterRoles = m_filterRoles;
		bool? isHero = m_filterIsHero;
		int? filterOwnedMinimum = m_filterOwnedMinimum;
		bool? craftableFilterValue = m_craftableFilterValue;
		DeckRuleset deckRuleset = m_deckRuleset;
		HashSet<string> leagueBannedCardsSubset = m_leagueBannedCardsSubset;
		List<int> specificCards = m_specificCards;
		bool? filterCounterpartCards = m_filterCounterpartCards;
		List<HashSet<string>> filterSubsets = m_filterSubsets;
		return collectionManager.FindOrderedCards(filterText, filterMasks, filterManaCost, filterCardSets, filterClasses, filterCardTypes, filterRoles, null, null, isHero, filterOwnedMinimum, null, craftableFilterValue, null, deckRuleset, returnAfterFirstResult: false, leagueBannedCardsSubset, specificCards, filterCounterpartCards, filterSubsets);
	}

	public CollectionManager.FindCardsResult GenerateUnOrderedResults()
	{
		CollectionManager collectionManager = CollectionManager.Get();
		string filterText = m_filterText;
		int? filterManaCost = m_filterManaCost;
		List<FilterMask> filterMasks = m_filterMasks;
		TAG_CARD_SET[] filterCardSets = m_filterCardSets;
		TAG_CLASS[] filterClasses = m_filterClasses;
		TAG_CARDTYPE[] filterCardTypes = m_filterCardTypes;
		TAG_ROLE[] filterRoles = m_filterRoles;
		bool? isHero = m_filterIsHero;
		int? filterOwnedMinimum = m_filterOwnedMinimum;
		bool? craftableFilterValue = m_craftableFilterValue;
		DeckRuleset deckRuleset = m_deckRuleset;
		HashSet<string> leagueBannedCardsSubset = m_leagueBannedCardsSubset;
		List<int> specificCards = m_specificCards;
		bool? filterCounterpartCards = m_filterCounterpartCards;
		return collectionManager.FindCards(filterText, filterMasks, filterManaCost, filterCardSets, filterClasses, filterCardTypes, filterRoles, null, null, isHero, filterOwnedMinimum, null, craftableFilterValue, null, deckRuleset, returnAfterFirstResult: false, leagueBannedCardsSubset, specificCards, filterCounterpartCards);
	}

	public bool GetDoesAnyCardExistIgnoreOwnership(TAG_CLASS tagClass)
	{
		CollectionManager collectionManager = CollectionManager.Get();
		string filterText = m_filterText;
		int? filterManaCost = m_filterManaCost;
		List<FilterMask> filterMasks = m_filterMasks;
		TAG_CARD_SET[] filterCardSets = m_filterCardSets;
		TAG_CLASS[] theseClassTypes = new TAG_CLASS[1] { tagClass };
		TAG_CARDTYPE[] filterCardTypes = m_filterCardTypes;
		TAG_ROLE[] filterRoles = m_filterRoles;
		bool? isHero = m_filterIsHero;
		int? minOwned = 0;
		bool? craftableFilterValue = m_craftableFilterValue;
		DeckRuleset deckRuleset = m_deckRuleset;
		HashSet<string> leagueBannedCardsSubset = m_leagueBannedCardsSubset;
		List<int> specificCards = m_specificCards;
		bool? filterCounterpartCards = m_filterCounterpartCards;
		return collectionManager.FindCards(filterText, filterMasks, filterManaCost, filterCardSets, theseClassTypes, filterCardTypes, filterRoles, null, null, isHero, minOwned, null, craftableFilterValue, null, deckRuleset, returnAfterFirstResult: true, leagueBannedCardsSubset, specificCards, filterCounterpartCards).m_cards.Count > 0;
	}

	private static void AddSearchableTokensToSet(string str, HashSet<string> addToList, bool split = true)
	{
		bool addOriginalString = !split;
		if (split)
		{
			int numTokensAdded = 0;
			ReadOnlySpanExtensions.SplitEnumerator enumerator = str.AsSpan().Split(CollectibleFilteredSet<ICollectible>.SearchTokenDelimiters).GetEnumerator();
			while (enumerator.MoveNext())
			{
				ReadOnlySpan<char> token = enumerator.Current;
				if (token.Length > 0)
				{
					AddSingleSearchableTokenToSet(token, addToList);
					numTokensAdded++;
				}
			}
			if (numTokensAdded > 1)
			{
				addOriginalString = true;
			}
		}
		if (addOriginalString)
		{
			AddSingleSearchableTokenToSet(str, addToList);
		}
	}

	public static void AddSearchableTokensToSet<T>(T structType, Func<T, bool> hasTypeString, Func<T, string> getTypeString, HashSet<string> addToList) where T : struct
	{
		if (hasTypeString(structType))
		{
			AddSearchableTokensToSet(getTypeString(structType), addToList);
		}
	}

	public static void AddSingleSearchableTokenToSet(ReadOnlySpan<char> token, HashSet<string> addToList)
	{
		Span<char> toLowerSpan = stackalloc char[token.Length];
		token.ToLower(toLowerSpan, CultureInfo.CurrentCulture);
		string lowerToken = toLowerSpan.ToString();
		addToList.Add(lowerToken);
		if (SearchableString.HasEuropeanCharacters(lowerToken))
		{
			string nonEuroToken = SearchableString.ConvertEuropeanCharacters(lowerToken);
			addToList.Add(nonEuroToken);
		}
		if (SearchableString.NeedsDiacriticsRemoved(lowerToken))
		{
			string noDiacriticsToken = SearchableString.RemoveDiacritics(lowerToken);
			if (!lowerToken.Equals(noDiacriticsToken))
			{
				addToList.Add(noDiacriticsToken);
			}
		}
	}

	public static void AddSingleSearchableTokenToSet(string token, HashSet<string> addToList)
	{
		string lowerToken = token.ToLower();
		addToList.Add(lowerToken);
		if (SearchableString.HasEuropeanCharacters(lowerToken))
		{
			string nonEuroToken = SearchableString.ConvertEuropeanCharacters(lowerToken);
			addToList.Add(nonEuroToken);
		}
		if (SearchableString.NeedsDiacriticsRemoved(lowerToken))
		{
			string noDiacriticsToken = SearchableString.RemoveDiacritics(lowerToken);
			if (!lowerToken.Equals(noDiacriticsToken))
			{
				addToList.Add(noDiacriticsToken);
			}
		}
	}

	protected override ICollection<Filter<CollectibleCard>> CreateValuelessFilters(string token)
	{
		ICollection<Filter<CollectibleCard>> filters = new List<Filter<CollectibleCard>>();
		if (token == m_extraToken || (token == m_favoriteToken && CardBackManager.Get().MultipleFavoriteCardBacksEnabled()))
		{
			return filters;
		}
		if (token == m_missingToken)
		{
			Filter<CollectibleCard> filter = new Filter<CollectibleCard>((CollectibleCard c) => c.OwnedCount <= 0 || c.IsEverCraftable);
			filters.Add(filter);
			return filters;
		}
		filters = base.CreateValuelessFilters(token);
		if (filters.Any())
		{
			return filters;
		}
		string golden = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_GOLDEN");
		string diamond = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_DIAMOND");
		string signature = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_SIGNATURE");
		string refund = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_REFUND");
		string whelp = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_WHELP");
		string imp = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_IMP");
		string runeBlood = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_RUNE_BLOOD");
		string runeFrost = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_RUNE_FROST");
		string runeUnholy = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_RUNE_UNHOLY");
		if (token == golden)
		{
			Filter<CollectibleCard> filter2 = new Filter<CollectibleCard>((CollectibleCard card) => card.PremiumType == TAG_PREMIUM.GOLDEN);
			filters.Add(filter2);
			return filters;
		}
		if (token == diamond)
		{
			Filter<CollectibleCard> filter3 = new Filter<CollectibleCard>((CollectibleCard card) => card.PremiumType == TAG_PREMIUM.DIAMOND);
			filters.Add(filter3);
			return filters;
		}
		if (token == signature)
		{
			Filter<CollectibleCard> filter4 = new Filter<CollectibleCard>((CollectibleCard card) => card.PremiumType == TAG_PREMIUM.SIGNATURE);
			filters.Add(filter4);
			return filters;
		}
		if (token == refund)
		{
			Filter<CollectibleCard> filter5 = new Filter<CollectibleCard>((CollectibleCard card) => card.IsRefundable);
			filters.Add(filter5);
			return filters;
		}
		if (token == whelp)
		{
			Filter<CollectibleCard> filter6 = new Filter<CollectibleCard>((CollectibleCard card) => card.HasCardTag(GAME_TAG.WHELP) || card.FindTextInCard(whelp));
			filters.Add(filter6);
			return filters;
		}
		if (token == imp)
		{
			Filter<CollectibleCard> filter7 = new Filter<CollectibleCard>((CollectibleCard card) => card.HasCardTag(GAME_TAG.IMP) || card.FindTextInCard(imp));
			filters.Add(filter7);
			return filters;
		}
		if (token == runeBlood)
		{
			Filter<CollectibleCard> filter8 = new Filter<CollectibleCard>((CollectibleCard card) => card.Runes.Blood > 0 || card.FindTextInCard(runeBlood));
			filters.Add(filter8);
			return filters;
		}
		if (token == runeFrost)
		{
			Filter<CollectibleCard> filter9 = new Filter<CollectibleCard>((CollectibleCard card) => card.Runes.Frost > 0 || card.FindTextInCard(runeFrost));
			filters.Add(filter9);
			return filters;
		}
		if (token == runeUnholy)
		{
			Filter<CollectibleCard> filter10 = new Filter<CollectibleCard>((CollectibleCard card) => card.Runes.Unholy > 0 || card.FindTextInCard(runeUnholy));
			filters.Add(filter10);
			return filters;
		}
		return filters;
	}

	protected override ICollection<Filter<CollectibleCard>> CreateNumericFilters(string tag, int minVal, int maxVal)
	{
		ICollection<Filter<CollectibleCard>> filters = base.CreateNumericFilters(tag, minVal, maxVal);
		if (filters.Any())
		{
			return filters;
		}
		string health = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_HEALTH");
		string attack = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_ATTACK");
		string mana = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MANA").ToLower();
		string runeBlood = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_RUNE_BLOOD");
		string runeFrost = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_RUNE_FROST");
		string runeUnholy = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_RUNE_UNHOLY");
		if (tag == attack)
		{
			Filter<CollectibleCard> minMaxFilter = CreateMinMaxFilter((CollectibleCard card) => card.Attack, minVal, maxVal);
			Filter<CollectibleCard> isAttackTypeCard = new Filter<CollectibleCard>((CollectibleCard card) => card.CardType == TAG_CARDTYPE.MINION || card.CardType == TAG_CARDTYPE.WEAPON);
			filters.Add(minMaxFilter);
			filters.Add(isAttackTypeCard);
			return filters;
		}
		if (tag == health)
		{
			Filter<CollectibleCard> minMaxFilter2 = CreateMinMaxFilter((CollectibleCard card) => card.Health, minVal, maxVal);
			Filter<CollectibleCard> isMinionTypeCard = new Filter<CollectibleCard>((CollectibleCard card) => card.CardType == TAG_CARDTYPE.MINION);
			filters.Add(minMaxFilter2);
			filters.Add(isMinionTypeCard);
			return filters;
		}
		if (tag == mana)
		{
			Filter<CollectibleCard> filter = CreateMinMaxFilter((CollectibleCard card) => card.ManaCost, minVal, maxVal);
			filters.Add(filter);
			return filters;
		}
		if (tag == runeBlood)
		{
			Filter<CollectibleCard> filter2 = CreateMinMaxFilter((CollectibleCard card) => card.Runes.Blood, minVal, maxVal);
			filters.Add(filter2);
			return filters;
		}
		if (tag == runeFrost)
		{
			Filter<CollectibleCard> filter3 = CreateMinMaxFilter((CollectibleCard card) => card.Runes.Frost, minVal, maxVal);
			filters.Add(filter3);
			return filters;
		}
		if (tag == runeUnholy)
		{
			Filter<CollectibleCard> filter4 = CreateMinMaxFilter((CollectibleCard card) => card.Runes.Unholy, minVal, maxVal);
			filters.Add(filter4);
			return filters;
		}
		return filters;
	}

	protected override ICollection<Filter<CollectibleCard>> CreateTagValueFilters(string tagKey, string value)
	{
		ICollection<Filter<CollectibleCard>> filters = base.CreateTagValueFilters(tagKey, value);
		if (filters.Any())
		{
			return filters;
		}
		string artist = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_ARTIST");
		string health = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_HEALTH");
		string attack = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_ATTACK");
		string mana = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MANA").ToLower();
		string rarity = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_RARITY");
		string type = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_TYPE");
		string runes = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_RUNES");
		string tag = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_TAG");
		string school = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_SCHOOL");
		if (tagKey == artist)
		{
			Filter<CollectibleCard> filter = new Filter<CollectibleCard>((CollectibleCard card) => SearchableString.SearchInternationalText(card.ArtistName, value));
			filters.Add(filter);
			return filters;
		}
		if (tagKey == rarity)
		{
			Filter<CollectibleCard> filter2 = new Filter<CollectibleCard>((CollectibleCard card) => SearchableString.SearchInternationalText(value, GameStrings.GetRarityText(card.Rarity)));
			filters.Add(filter2);
			return filters;
		}
		if (tagKey == type)
		{
			Filter<CollectibleCard> filter3 = new Filter<CollectibleCard>(delegate(CollectibleCard card)
			{
				string cardTypeName = GameStrings.GetCardTypeName(card.CardType);
				return cardTypeName != null && SearchableString.SearchInternationalText(value, cardTypeName);
			});
			filters.Add(filter3);
			return filters;
		}
		if (tagKey == attack)
		{
			if (TryCreateOddEvenParityFilter((CollectibleCard card) => card.Attack, value, out var attackOddEvenFilter))
			{
				Filter<CollectibleCard> cardHasAttackFilter = new Filter<CollectibleCard>((CollectibleCard card) => card.CardType == TAG_CARDTYPE.MINION || card.CardType == TAG_CARDTYPE.WEAPON);
				filters.Add(attackOddEvenFilter);
				filters.Add(cardHasAttackFilter);
			}
			return filters;
		}
		if (tagKey == health)
		{
			if (TryCreateOddEvenParityFilter((CollectibleCard card) => card.Health, value, out var healthOddEvenFilter))
			{
				Filter<CollectibleCard> cardIsMinionFilter = new Filter<CollectibleCard>((CollectibleCard card) => card.CardType == TAG_CARDTYPE.MINION);
				filters.Add(healthOddEvenFilter);
				filters.Add(cardIsMinionFilter);
			}
			return filters;
		}
		if (tagKey == mana)
		{
			if (TryCreateOddEvenParityFilter((CollectibleCard card) => card.ManaCost, value, out var manaOddEvenFilter))
			{
				filters.Add(manaOddEvenFilter);
			}
			return filters;
		}
		if (tagKey == runes)
		{
			RunePattern runePattern = default(RunePattern);
			char bloodRuneChar = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_RUNE_BLOOD_CHAR")[0];
			char frostRuneChar = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_RUNE_FROST_CHAR")[0];
			char unholyRuneChar = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_RUNE_UNHOLY_CHAR")[0];
			char[] array = value.ToCharArray();
			foreach (char runeChar in array)
			{
				if (runeChar == bloodRuneChar)
				{
					runePattern.AddRunes(RuneType.RT_BLOOD, 1);
				}
				else if (runeChar == frostRuneChar)
				{
					runePattern.AddRunes(RuneType.RT_FROST, 1);
				}
				else if (runeChar == unholyRuneChar)
				{
					runePattern.AddRunes(RuneType.RT_UNHOLY, 1);
				}
			}
			if (runePattern.HasRunes)
			{
				Filter<CollectibleCard> filter4 = new Filter<CollectibleCard>((CollectibleCard card) => card.Runes.Matches(runePattern));
				filters.Add(filter4);
			}
			return filters;
		}
		if (tagKey == tag)
		{
			string whelp = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_WHELP");
			string imp = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_IMP");
			if (value == whelp)
			{
				Filter<CollectibleCard> filter5 = new Filter<CollectibleCard>((CollectibleCard card) => card.HasCardTag(GAME_TAG.WHELP));
				filters.Add(filter5);
			}
			else if (value == imp)
			{
				Filter<CollectibleCard> filter6 = new Filter<CollectibleCard>((CollectibleCard card) => card.HasCardTag(GAME_TAG.IMP));
				filters.Add(filter6);
			}
			return filters;
		}
		if (tagKey == school)
		{
			Dictionary<string, TAG_SPELL_SCHOOL> schoolDictionary = new Dictionary<string, TAG_SPELL_SCHOOL>
			{
				{
					GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_SCHOOL_NONE"),
					TAG_SPELL_SCHOOL.NONE
				},
				{
					GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_SCHOOL_ARCANE"),
					TAG_SPELL_SCHOOL.ARCANE
				},
				{
					GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_SCHOOL_FIRE"),
					TAG_SPELL_SCHOOL.FIRE
				},
				{
					GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_SCHOOL_FROST"),
					TAG_SPELL_SCHOOL.FROST
				},
				{
					GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_SCHOOL_NATURE"),
					TAG_SPELL_SCHOOL.NATURE
				},
				{
					GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_SCHOOL_HOLY"),
					TAG_SPELL_SCHOOL.HOLY
				},
				{
					GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_SCHOOL_SHADOW"),
					TAG_SPELL_SCHOOL.SHADOW
				},
				{
					GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_SCHOOL_FEL"),
					TAG_SPELL_SCHOOL.FEL
				}
			};
			if (schoolDictionary.ContainsKey(value))
			{
				Filter<CollectibleCard> filter7 = new Filter<CollectibleCard>((CollectibleCard card) => card.IsSpell && card.SpellSchool == schoolDictionary[value]);
				filters.Add(filter7);
			}
			return filters;
		}
		return filters;
	}

	protected override bool ShouldAppendToRegularSearchTokens(string token, ICollection<Filter<CollectibleCard>> generatedFilters)
	{
		if (token == m_extraToken)
		{
			return false;
		}
		return base.ShouldAppendToRegularSearchTokens(token, generatedFilters);
	}

	public List<CollectionManager.CollectibleCardFilterFunc> FiltersFromSearchString(string searchString)
	{
		ISet<Filter<CollectibleCard>> set = CreateFiltersFromSearchString(searchString);
		List<CollectionManager.CollectibleCardFilterFunc> filterFuncs = new List<CollectionManager.CollectibleCardFilterFunc>();
		foreach (Filter<CollectibleCard> filter in set)
		{
			CollectionManager.CollectibleCardFilterFunc filterFunc = (CollectibleCard card) => filter.PassesFilter(card);
			filterFuncs.Add(filterFunc);
		}
		return filterFuncs;
	}

	public static string CreateSearchTerm_Mana_OddEven(bool isOdd)
	{
		return string.Format("{0}{1}{2}", GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MANA"), CollectibleFilteredSet<ICollectible>.SearchTagColons.First(), GameStrings.Get(isOdd ? "GLUE_COLLECTION_MANAGER_SEARCH_ODD_MANA" : "GLUE_COLLECTION_MANAGER_SEARCH_EVEN_MANA"));
	}

	public void ClearOutFiltersFromSetFilterDropdown()
	{
		m_specificCards = null;
		m_filterCardSets = null;
		m_filterSubsets = null;
	}
}
