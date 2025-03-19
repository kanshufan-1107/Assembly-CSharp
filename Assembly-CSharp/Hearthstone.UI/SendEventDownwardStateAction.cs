using Hearthstone.DataModels;
using Hearthstone.UI.Scripting;
using UnityEngine;

namespace Hearthstone.UI;

public class SendEventDownwardStateAction : StateActionImplementation
{
	public enum BubbleDownEventDepth
	{
		DirectChildrenOnly,
		DirectChildrenAndGrandchildrenOnly,
		AllDescendants
	}

	public static readonly int s_SendEventDownwardDepthLimitIndex;

	private const int DirectChildrenOnlyDepthValue = 0;

	private const int DirectChildrenAndGrandchildrenDepthValue = 1;

	private const int AllDecendantsDepthValue = 25;

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
			BubbleDownEventDepth depthLimit = (BubbleDownEventDepth)GetIntValueAtIndex(s_SendEventDownwardDepthLimitIndex);
			SendEventDownward(target.gameObject, eventName, depthLimit, eventData);
		}
		Complete(success: true);
	}

	public static void SendEventDownward(GameObject baseObject, string eventName, BubbleDownEventDepth depthLimit, EventDataModel eventData = null)
	{
		WidgetTemplate currentTemplate = null;
		if (eventData != null)
		{
			IWidgetEventListener listener = baseObject.GetComponent<IWidgetEventListener>();
			if (listener != null)
			{
				currentTemplate = listener.OwningWidget;
				if (currentTemplate != null)
				{
					currentTemplate.BindDataModel(eventData);
				}
			}
		}
		bool consumed = false;
		int currentDepth = 0;
		int depthLimitValue = GetDepthValue(depthLimit);
		ProcessObjectListeners(baseObject.transform, ref consumed, eventName, eventData);
		RecurseAndProcessObjects(baseObject.transform, ref consumed, ref currentDepth, eventName, eventData, ref depthLimitValue);
		if (currentTemplate != null && eventData != null)
		{
			currentTemplate.UnbindDataModel(eventData.DataModelId);
		}
	}

	private static void RecurseAndProcessObjects(Transform item, ref bool consumed, ref int depth, string eventName, EventDataModel eventData, ref int maxDepthValue)
	{
		if (((depth > maxDepthValue) | consumed) || item.childCount == 0)
		{
			return;
		}
		for (int i = 0; i < item.childCount; i++)
		{
			ProcessObjectListeners(item.GetChild(i), ref consumed, eventName, eventData);
			if (consumed)
			{
				return;
			}
		}
		depth++;
		for (int j = 0; j < item.childCount; j++)
		{
			RecurseAndProcessObjects(item.GetChild(j), ref consumed, ref depth, eventName, eventData, ref maxDepthValue);
			if (consumed)
			{
				depth--;
				return;
			}
		}
		depth--;
	}

	private static void ProcessObjectListeners(Transform item, ref bool consumed, string eventName, EventDataModel eventData)
	{
		IWidgetEventListener[] components = item.GetComponents<IWidgetEventListener>();
		for (int i = 0; i < components.Length; i++)
		{
			consumed |= components[i].EventReceived(eventName, new TriggerEventParameters(eventData?.SourceName ?? eventName, eventData?.Payload, noDownwardPropagation: true, ignorePlaymaker: true)).Consumed;
		}
	}

	private static int GetDepthValue(BubbleDownEventDepth depth)
	{
		return depth switch
		{
			BubbleDownEventDepth.DirectChildrenOnly => 0, 
			BubbleDownEventDepth.DirectChildrenAndGrandchildrenOnly => 1, 
			BubbleDownEventDepth.AllDescendants => 25, 
			_ => 0, 
		};
	}
}
