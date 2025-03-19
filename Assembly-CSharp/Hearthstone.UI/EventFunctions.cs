using Blizzard.T5.Core.Utils;
using Hearthstone.DataModels;
using UnityEngine;

namespace Hearthstone.UI;

public static class EventFunctions
{
	public static bool TriggerEvent(Transform target, string eventName, TriggerEventParameters parameters = default(TriggerEventParameters))
	{
		bool foundListeners = false;
		if (parameters.Payload != null)
		{
			EventDataModel eventData = new EventDataModel();
			eventData.SourceName = parameters.SourceName ?? "UNKNOWN";
			eventData.Payload = parameters.Payload;
			WidgetTemplate databindingOwner = target.GetComponentInParent<WidgetTemplate>(includeInactive: true);
			if (databindingOwner != null)
			{
				databindingOwner.BindDataModel(eventData, target.gameObject);
			}
		}
		GameObjectUtils.WalkSelfAndChildren(target, delegate(Transform child)
		{
			Component[] components = child.GetComponents<Component>();
			bool flag = false;
			bool flag2 = false;
			Component[] array = components;
			foreach (Component component in array)
			{
				if (!(null == component))
				{
					if (component is IWidgetEventListener widgetEventListener)
					{
						flag |= widgetEventListener.EventReceived(eventName, parameters).Consumed;
						foundListeners = true;
					}
					if (!parameters.IgnorePlaymaker)
					{
						PlayMakerFSM playMakerFSM = component as PlayMakerFSM;
						if (playMakerFSM != null)
						{
							playMakerFSM.SendEvent(eventName);
							foundListeners = true;
						}
					}
					if (!flag)
					{
						WidgetInstance widgetInstance = component as WidgetInstance;
						if (widgetInstance != null)
						{
							flag2 = true;
							widgetInstance.TriggerEvent(eventName, parameters);
							foundListeners = true;
						}
					}
				}
			}
			return !flag2 && !flag && !parameters.NoDownwardPropagation;
		});
		return foundListeners;
	}
}
