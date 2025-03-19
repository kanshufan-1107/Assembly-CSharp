using System;
using UnityEngine;

public class LegendaryHeroGenericEventHandler : MonoBehaviour
{
	public virtual void HandleEvent(string eventName, object eventData)
	{
		throw new NotImplementedException();
	}
}
