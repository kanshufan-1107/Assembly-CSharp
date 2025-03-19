using System;
using UnityEngine;

namespace Hearthstone.Timeline;

[Serializable]
public class EventEmitterEntryCollection
{
	[SerializeField]
	private EventEmitterEntry[] m_array = new EventEmitterEntry[0];

	public int Count => m_array.Length;

	public EventEmitterEntry Get(int i)
	{
		if (i < 0 || i >= Count)
		{
			return null;
		}
		return m_array[i];
	}
}
