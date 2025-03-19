using System.Collections.Generic;

public class TagListMap
{
	private Dictionary<int, List<int>> m_values;

	public TagListMap()
	{
		m_values = new Dictionary<int, List<int>>();
	}

	public bool TryGetValue(int tag, out List<int> value)
	{
		return m_values.TryGetValue(tag, out value);
	}

	public void SetValues(int tag, List<int> values)
	{
		m_values.Remove(tag);
		m_values.Add(tag, values);
	}
}
