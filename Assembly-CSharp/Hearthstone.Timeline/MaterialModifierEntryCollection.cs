using System;
using UnityEngine;

namespace Hearthstone.Timeline;

[Serializable]
public class MaterialModifierEntryCollection
{
	[SerializeField]
	private MaterialModifierEntry[] m_array = new MaterialModifierEntry[0];

	public int Count => m_array.Length;

	public MaterialModifierEntry Get(int i)
	{
		if (i < 0 || i >= Count)
		{
			return null;
		}
		return m_array[i];
	}
}
