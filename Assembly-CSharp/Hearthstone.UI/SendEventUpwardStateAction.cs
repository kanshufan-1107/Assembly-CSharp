using Hearthstone.DataModels;
using Hearthstone.UI.Scripting;
using UnityEngine;

namespace Hearthstone.UI;

public class SendEventUpwardStateAction : StateActionImplementation
{
	public override void Run(bool loadSynchronously = false)
	{
		GetOverride(0).RegisterReadyListener(HandleReady);
	}

	private void HandleReady(object unused)
	{
		GetOverride(0).RemoveReadyOrInactiveListener(HandleReady);
		Override @override = GetOverride(0);
		string eventName = GetString(0);
		if (@override.Resolve(out var target))
		{
			ScriptString script = GetValueScript(0);
			EventDataModel eventData = null;
			if (!string.IsNullOrEmpty(script.Script))
			{
				string stateName = GetStateName();
				ScriptContext.EvaluationResults evaluationResults = new ScriptContext().Evaluate(script.Script, base.DataContext, base.StateCollectionContext);
				eventData = new EventDataModel();
				eventData.Payload = evaluationResults.Value;
				eventData.SourceName = stateName;
			}
			SendEventUpward(target.gameObject, eventName, eventData);
		}
		Complete(success: true);
	}

	public static void SendEventUpward(GameObject baseObject, string eventName, EventDataModel eventData = null)
	{
		Transform current = baseObject.transform;
		WidgetTemplate lastTemplate = null;
		while (current != null)
		{
			IWidgetEventListener[] eventListeners = current.GetComponents<IWidgetEventListener>();
			bool consumed = false;
			if (eventListeners.Length != 0 && eventListeners[0] != null)
			{
				WidgetTemplate currentTemplate = eventListeners[0].OwningWidget;
				if (eventData != null && currentTemplate != null)
				{
					if (lastTemplate != null)
					{
						lastTemplate.UnbindDataModel(120);
					}
					currentTemplate.BindDataModel(eventData);
					lastTemplate = currentTemplate;
				}
			}
			IWidgetEventListener[] array = eventListeners;
			for (int i = 0; i < array.Length; i++)
			{
				consumed |= array[i].EventReceived(eventName, new TriggerEventParameters(eventData?.SourceName ?? eventName, eventData?.Payload, noDownwardPropagation: true, ignorePlaymaker: true)).Consumed;
			}
			if (!consumed)
			{
				current = current.parent;
				continue;
			}
			break;
		}
	}
}
