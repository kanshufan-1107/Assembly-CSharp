using System.Collections.Generic;

public class FilteredSet<T, TSet> where TSet : ISet<T>, new()
{
	protected DictionaryOfHashSets<Filter<T>, T> m_filtersToExcludedItems;

	protected DictionaryOfHashSets<T, Filter<T>> m_itemsToExcludingFilters;

	protected TSet m_itemsFilteredOut;

	protected TSet m_itemsRemaining;

	public FilteredSet()
	{
		m_filtersToExcludedItems = new DictionaryOfHashSets<Filter<T>, T>();
		m_itemsToExcludingFilters = new DictionaryOfHashSets<T, Filter<T>>();
		m_itemsFilteredOut = new TSet();
		m_itemsRemaining = new TSet();
	}

	public bool AddFilter(Filter<T> filter)
	{
		if (!m_filtersToExcludedItems.AddKey(filter))
		{
			return false;
		}
		foreach (T item in m_itemsToExcludingFilters.Keys)
		{
			if (!filter.PassesFilter(item))
			{
				m_filtersToExcludedItems.Add(filter, item);
				m_itemsToExcludingFilters.Add(item, filter);
				m_itemsFilteredOut.Add(item);
				m_itemsRemaining.Remove(item);
			}
		}
		return true;
	}

	public bool AddItem(T item)
	{
		if (!m_itemsToExcludingFilters.AddKey(item))
		{
			return false;
		}
		bool passesAllFilters = true;
		foreach (Filter<T> filter in m_filtersToExcludedItems.Keys)
		{
			if (!filter.PassesFilter(item))
			{
				m_filtersToExcludedItems.Add(filter, item);
				m_itemsToExcludingFilters.Add(item, filter);
				if (passesAllFilters)
				{
					m_itemsFilteredOut.Add(item);
					passesAllFilters = false;
				}
			}
		}
		if (m_itemsRemaining.Contains(item))
		{
			Log.CollectionManager.PrintError("Duplicate key detected. Check item's comparison function and make sure it cannot have collisions for different items.", item.ToString());
			return false;
		}
		if (passesAllFilters)
		{
			m_itemsRemaining.Add(item);
		}
		return true;
	}

	public int AddItems(IEnumerable<T> items)
	{
		int itemsAdded = 0;
		foreach (T item in items)
		{
			if (AddItem(item))
			{
				itemsAdded++;
			}
		}
		return itemsAdded;
	}

	public bool RemoveFilter(Filter<T> filter)
	{
		if (m_filtersToExcludedItems.TryGetValues(filter, out var itemsForFilter))
		{
			foreach (T item in itemsForFilter)
			{
				m_itemsToExcludingFilters.Remove(item, filter, removeKeyIfSetBecomesEmpty: false);
				m_itemsToExcludingFilters.TryGetValues(item, out var remainingFiltersForItem);
				if (remainingFiltersForItem.Count == 0)
				{
					m_itemsFilteredOut.Remove(item);
					m_itemsRemaining.Add(item);
				}
			}
		}
		return m_filtersToExcludedItems.RemoveKey(filter);
	}

	public HashSet<Filter<T>> GetAllFilters()
	{
		return new HashSet<Filter<T>>(m_filtersToExcludedItems.Keys);
	}
}
