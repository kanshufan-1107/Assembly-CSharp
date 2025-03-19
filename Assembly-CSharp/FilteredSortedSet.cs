using System;
using System.Collections.Generic;

public class FilteredSortedSet<T> : FilteredSet<T, SortedSet<T>> where T : IComparable
{
	public FilteredSortedSet()
	{
	}

	public FilteredSortedSet(IComparer<T> comparer)
	{
		m_filtersToExcludedItems = new DictionaryOfHashSets<Filter<T>, T>();
		m_itemsToExcludingFilters = new DictionaryOfHashSets<T, Filter<T>>();
		m_itemsFilteredOut = new SortedSet<T>(comparer);
		m_itemsRemaining = new SortedSet<T>(comparer);
	}
}
