using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shared.UI.Scripts.Carousel;

public class ItemCollection
{
	private readonly List<Carousel.Item> m_items = new List<Carousel.Item>();

	public void SetItems(IEnumerable<Carousel.Item> items)
	{
		Clear();
		m_items.AddRange(items);
	}

	public void ForEach(Action<Carousel.Item, int> consumer)
	{
		foreach (var (item2, index2) in m_items.Select((Carousel.Item item, int index) => (item: item, index: index)))
		{
			consumer?.Invoke(item2, index2);
		}
	}

	public Carousel.Item GetClampedItemAtIndex(int index)
	{
		return m_items[Mathf.Clamp(index, 0, m_items.Count - 1)];
	}

	public int MouseHit(out Carousel.Item itemHit)
	{
		foreach (var (index2, item2, obj) in m_items.Select((Carousel.Item item, int index) => (index: index, item: item, item.GetGameObject())))
		{
			if (UniversalInputManager.Get().InputIsOver(obj, out var _))
			{
				itemHit = item2;
				return index2;
			}
		}
		itemHit = null;
		return -1;
	}

	private void Clear()
	{
		foreach (Carousel.Item item in m_items)
		{
			item.Clear();
		}
		m_items.Clear();
	}
}
