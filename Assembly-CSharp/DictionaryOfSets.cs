using System.Collections.Generic;

public class DictionaryOfSets<TKey, TValue, TSet, TDictionary> where TSet : ISet<TValue>, new() where TDictionary : IDictionary<TKey, TSet>, new()
{
	protected TDictionary m_inner;

	public ICollection<TKey> Keys => m_inner.Keys;

	public DictionaryOfSets()
	{
		m_inner = new TDictionary();
	}

	public bool AddKey(TKey key)
	{
		if (m_inner.ContainsKey(key))
		{
			return false;
		}
		m_inner.Add(key, new TSet());
		return true;
	}

	public bool Add(TKey key, TValue value)
	{
		if (!m_inner.TryGetValue(key, out var hashSet))
		{
			hashSet = new TSet();
			m_inner.Add(key, hashSet);
		}
		return hashSet.Add(value);
	}

	public bool Remove(TKey key, TValue value, bool removeKeyIfSetBecomesEmpty)
	{
		if (!m_inner.TryGetValue(key, out var hashSet))
		{
			return false;
		}
		if (hashSet == null)
		{
			RemoveKey(key);
			return false;
		}
		bool result = hashSet.Remove(value);
		if (removeKeyIfSetBecomesEmpty && hashSet.Count == 0)
		{
			RemoveKey(key);
		}
		return result;
	}

	public bool RemoveKey(TKey key)
	{
		return m_inner.Remove(key);
	}

	public bool TryGetValues(TKey key, out TSet values)
	{
		return m_inner.TryGetValue(key, out values);
	}
}
