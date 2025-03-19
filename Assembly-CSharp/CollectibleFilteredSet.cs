using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blizzard.T5.Core.Utils;
using Hearthstone.Extensions;

public abstract class CollectibleFilteredSet<T> : FilteredSortedSet<T> where T : ICollectible
{
	protected int m_remainingItemCount;

	protected string m_ownedTag = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_OWNED");

	protected string m_newToken = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_NEW");

	protected string m_hasTag = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_HAS");

	protected string m_evenTag = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_EVEN_CARDS").ToLower();

	protected string m_oddTag = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_ODD_CARDS").ToLower();

	protected string m_missingToken = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MISSING");

	public static readonly char[] SearchTagColons = new char[2] { ':', '：' };

	public static readonly char[] SearchTokenDelimiters = new char[2] { ' ', '\t' };

	private List<T> m_itemsRemaining_memoized;

	public int TotalPages { get; protected set; }

	public int ItemsPerPage { get; set; }

	public string SearchString { get; set; }

	protected virtual ICollection<Filter<T>> CreateValuelessFilters(string token)
	{
		List<Filter<T>> filters = new List<Filter<T>>();
		if (string.Equals(token, m_newToken, StringComparison.OrdinalIgnoreCase))
		{
			Filter<T> filter = new Filter<T>((T c) => c.IsNewCollectible);
			filters.Add(filter);
			return filters;
		}
		if (string.Equals(token, m_ownedTag, StringComparison.OrdinalIgnoreCase) && !SearchTagColons.Any(token.Contains))
		{
			Filter<T> filter2 = new Filter<T>((T c) => c.OwnedCount > 0);
			filters.Add(filter2);
			return filters;
		}
		if (string.Equals(token, m_missingToken, StringComparison.OrdinalIgnoreCase))
		{
			Filter<T> filter3 = new Filter<T>((T c) => c.OwnedCount <= 0);
			filters.Add(filter3);
			return filters;
		}
		return filters;
	}

	protected virtual ICollection<Filter<T>> CreateNumericFilters(string tag, int minVal, int maxVal)
	{
		List<Filter<T>> filters = new List<Filter<T>>();
		if (string.Equals(tag, m_ownedTag, StringComparison.OrdinalIgnoreCase))
		{
			Filter<T> filter = new Filter<T>((T c) => minVal <= c.OwnedCount && c.OwnedCount <= maxVal);
			filters.Add(filter);
			return filters;
		}
		return filters;
	}

	protected virtual ICollection<Filter<T>> CreateTagValueFilters(string tag, string value)
	{
		List<Filter<T>> filters = new List<Filter<T>>();
		if (string.Equals(tag, m_ownedTag, StringComparison.OrdinalIgnoreCase))
		{
			if (TryCreateOddEvenParityFilter((T c) => c.OwnedCount, value, out var evenOddFilter))
			{
				filters.Add(evenOddFilter);
			}
			return filters;
		}
		if (string.Equals(tag, m_hasTag, StringComparison.OrdinalIgnoreCase))
		{
			Filter<T> hasFilter = new Filter<T>((T c) => CollectionUtils.FindTextInCollectible(c, value));
			filters.Add(hasFilter);
			return filters;
		}
		return filters;
	}

	protected virtual bool ShouldAppendToRegularSearchTokens(string token, ICollection<Filter<T>> generatedFilters)
	{
		return !generatedFilters.Any();
	}

	protected bool TryCreateOddEvenParityFilter(Func<T, int> fieldGetter, string value, out Filter<T> filter)
	{
		string lowerValue = value.ToLower();
		if (lowerValue == m_evenTag)
		{
			filter = new Filter<T>((T c) => fieldGetter(c) % 2 == 0);
			return true;
		}
		if (lowerValue == m_oddTag)
		{
			filter = new Filter<T>((T c) => fieldGetter(c) % 2 == 1);
			return true;
		}
		filter = null;
		return false;
	}

	protected Filter<T> CreateMinMaxFilter(Func<T, int> fieldGetter, int minVal, int maxVal)
	{
		return new Filter<T>((T c) => fieldGetter(c) >= minVal && fieldGetter(c) <= maxVal);
	}

	protected ICollection<Filter<T>> CreateFiltersFromToken(string token)
	{
		ICollection<Filter<T>> filters = CreateValuelessFilters(token);
		if (filters.Any())
		{
			return filters;
		}
		if (SearchTagColons.Any(token.Contains))
		{
			string[] subTokens = token.Split(SearchTagColons);
			if (subTokens.Length == 2)
			{
				string tag = subTokens[0].Trim();
				string value = subTokens[1].Trim();
				GeneralUtils.ParseNumericRange(value, out var isNumericValue, out var minVal, out var maxVal);
				if (isNumericValue)
				{
					ICollection<Filter<T>> numericFilters = CreateNumericFilters(tag, minVal, maxVal);
					if (numericFilters.Any())
					{
						filters.AddRange(numericFilters);
						return filters;
					}
				}
				ICollection<Filter<T>> tagValueFilters = CreateTagValueFilters(tag, value);
				if (tagValueFilters.Any())
				{
					filters.AddRange(tagValueFilters);
					return filters;
				}
			}
		}
		return filters;
	}

	public CollectibleFilteredSet()
	{
	}

	public CollectibleFilteredSet(IComparer<T> comparer)
		: base(comparer)
	{
	}

	public void UpdateFilters()
	{
		HashSet<Filter<T>> previousFilters = GetAllFilters();
		ISet<Filter<T>> currentFilters = CreateAllFilters();
		IEnumerable<Filter<T>> enumerable = previousFilters.Except(currentFilters);
		IEnumerable<Filter<T>> filtersToAdd = currentFilters.Except(previousFilters);
		bool filtersChanged = enumerable.Any() || filtersToAdd.Any();
		foreach (Filter<T> filterToRemove in enumerable)
		{
			RemoveFilter(filterToRemove);
		}
		foreach (Filter<T> filterToAdd in filtersToAdd)
		{
			AddFilter(filterToAdd);
		}
		if (filtersChanged || m_itemsRemaining_memoized == null)
		{
			UpdateMemoizedFields();
		}
	}

	public virtual List<T> GetPageContents(int currentPageNumber)
	{
		int indexOfFirstItem = GetIndexOfFirstItemOnPage(currentPageNumber);
		int itemsOnPage = Math.Min(ItemsPerPage, m_itemsRemaining_memoized.Count - indexOfFirstItem);
		return m_itemsRemaining_memoized.GetRange(indexOfFirstItem, itemsOnPage);
	}

	protected ISet<Filter<T>> CreateFiltersFromSearchString(string searchString)
	{
		HashSet<Filter<T>> returnValue = new HashSet<Filter<T>>();
		if (string.IsNullOrWhiteSpace(searchString))
		{
			return returnValue;
		}
		string[] array = searchString.ToLower().Split(SearchTokenDelimiters, StringSplitOptions.RemoveEmptyEntries);
		StringBuilder regularTokens = new StringBuilder();
		string[] array2 = array;
		foreach (string token in array2)
		{
			ICollection<Filter<T>> filters = CreateFiltersFromToken(token);
			returnValue.UnionWith(filters);
			if (ShouldAppendToRegularSearchTokens(token, filters))
			{
				if (regularTokens.Length > 0)
				{
					regularTokens.Append(" ");
				}
				regularTokens.Append(token);
			}
		}
		string regularTokensString = regularTokens.ToString();
		if (!string.IsNullOrWhiteSpace(regularTokensString))
		{
			Filter<T> regularSearchFilter = new Filter<T>((T collectible) => CollectionUtils.FindTextInCollectible(collectible, regularTokensString));
			returnValue.Add(regularSearchFilter);
		}
		return returnValue;
	}

	private ISet<Filter<T>> CreateAllFilters()
	{
		return CreateFiltersFromSearchString(SearchString);
	}

	private void UpdateMemoizedFields()
	{
		m_itemsRemaining_memoized = m_itemsRemaining.ToList();
		m_remainingItemCount = m_itemsRemaining_memoized.Count();
		TotalPages = m_remainingItemCount / ItemsPerPage + ((m_remainingItemCount % ItemsPerPage > 0) ? 1 : 0);
	}

	private int GetIndexOfFirstItemOnPage(int currentPageNumber)
	{
		return (currentPageNumber - 1) * ItemsPerPage;
	}
}
