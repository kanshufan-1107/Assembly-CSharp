using System.Collections.Generic;

namespace Hearthstone.TrailMaker;

public class MutableArray<T>
{
	private readonly List<T> m_list = new List<T>();

	private int m_startIndex;

	private int m_length;

	public int Count
	{
		get
		{
			return m_length;
		}
		set
		{
			while (m_length < value)
			{
				AddToEnd(default(T));
			}
			m_length = value;
		}
	}

	private int LastIndex => m_startIndex + m_length - 1;

	public void Clear()
	{
		m_startIndex = 0;
		m_length = 0;
	}

	public void Clone(MutableArray<T> source)
	{
		m_startIndex = 0;
		m_length = 0;
		for (int i = source.m_startIndex; i <= source.LastIndex; i++)
		{
			AddToEnd(source.m_list[i]);
		}
	}

	public T Get(int index)
	{
		if (index < 0 || index >= m_length)
		{
			return default(T);
		}
		return m_list[m_startIndex + index];
	}

	public T GetFirst()
	{
		return Get(0);
	}

	public T GetLast()
	{
		return Get(m_length - 1);
	}

	public void Set(int index, T value)
	{
		if (index >= 0 && index < m_length)
		{
			m_list[m_startIndex + index] = value;
		}
	}

	public void AddToEnd(T item)
	{
		if (m_startIndex + m_length < m_list.Count)
		{
			m_list[m_startIndex + m_length] = item;
		}
		else
		{
			m_list.Add(item);
		}
		m_length++;
	}

	public void RemoveFromStart()
	{
		if (m_length != 0)
		{
			m_startIndex++;
			m_length--;
		}
	}

	public void RemoveFromEnd()
	{
		if (m_length != 0)
		{
			m_length--;
		}
	}

	public void RemoveAt(int index)
	{
		if (index >= 0 && index < m_length)
		{
			if (index == 0)
			{
				RemoveFromStart();
				return;
			}
			if (index == m_length - 1)
			{
				RemoveFromEnd();
				return;
			}
			m_list.RemoveAt(m_startIndex + index);
			m_length--;
		}
	}
}
