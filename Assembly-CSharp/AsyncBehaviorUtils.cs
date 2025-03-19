using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

public static class AsyncBehaviorUtils
{
	public static List<IAsyncInitializationBehavior> GetAsyncBehaviors(Component component)
	{
		Component[] components = component.GetComponents<Component>();
		List<IAsyncInitializationBehavior> asyncBehaviors = new List<IAsyncInitializationBehavior>();
		Component[] array = components;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is IAsyncInitializationBehavior asyncBehavior)
			{
				asyncBehaviors.Add(asyncBehavior);
			}
		}
		return asyncBehaviors;
	}
}
