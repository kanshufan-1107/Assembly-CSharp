using System.Collections.Generic;
using System.Linq;

namespace Hearthstone.Extensions;

public static class FunctionalExtension
{
	public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
	{
		return source.Select((T item, int index) => (item: item, index: index));
	}

	public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
	{
		foreach (T item in items)
		{
			collection.Add(item);
		}
	}
}
