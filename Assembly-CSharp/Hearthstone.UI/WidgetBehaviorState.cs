using System;
using System.Collections.Generic;
using System.Diagnostics;
using Hearthstone.UI.Logging;
using Hearthstone.UI.Scripting;
using UnityEngine;

namespace Hearthstone.UI;

[Serializable]
public class WidgetBehaviorState : IWidgetState
{
	public delegate void ActivateCallbackDelegate(AsyncOperationResult result, object userData);

	[SerializeField]
	private string m_name;

	[SerializeField]
	private List<ScriptString> m_triggers;

	[SerializeField]
	private List<StateAction> m_actions;

	[SerializeField]
	private bool m_consumeEvent;

	private int m_currentIndex;

	private bool m_actionEnqueued;

	private IDataModelProvider m_dataModelProvider;

	private IWidgetStateCollection m_stateCollectionContext;

	private ScriptContext m_triggerContext;

	private HashSet<int> m_dataModelIDs;

	private string m_lastTriggerScriptString;

	private Dictionary<int, Dictionary<int, int>> m_dataModelToPropertyHash = new Dictionary<int, Dictionary<int, int>>();

	private ActivateCallbackDelegate m_callback;

	private object m_callbackUserData;

	public string Name => m_name;

	public bool ConsumeEvent => m_consumeEvent;

	public bool HasActionsToRun
	{
		get
		{
			if (m_actions != null && m_currentIndex >= 0)
			{
				return m_currentIndex < m_actions.Count;
			}
			return false;
		}
	}

	public IEnumerable<ScriptString> Triggers => m_triggers;

	public IEnumerable<StateAction> Actions => m_actions;

	public bool HasTriggers
	{
		get
		{
			if (m_triggers != null && m_triggers.Count > 0)
			{
				return !string.IsNullOrEmpty(m_triggers[0].Script);
			}
			return false;
		}
	}

	public void Activate(IDataModelProvider dataModelProvider, IWidgetStateCollection stateCollection, ActivateCallbackDelegate callback, object userData = null)
	{
		Abort();
		m_dataModelProvider = dataModelProvider;
		m_stateCollectionContext = stateCollection;
		m_callback = callback;
		m_callbackUserData = userData;
		m_currentIndex = -1;
		m_actionEnqueued = false;
		EnqueueNextAction(AsyncOperationResult.Success);
	}

	private void EnqueueNextAction(AsyncOperationResult result)
	{
		switch (result)
		{
		case AsyncOperationResult.Aborted:
			m_currentIndex = -1;
			m_actionEnqueued = false;
			return;
		case AsyncOperationResult.Wait:
			m_actionEnqueued = false;
			InvokeCallback(result, m_callbackUserData);
			return;
		}
		if (m_currentIndex == m_actions.Count - 1)
		{
			m_currentIndex = -1;
			m_actionEnqueued = false;
			InvokeCallback(result, m_callbackUserData);
		}
		else
		{
			m_actionEnqueued = true;
			m_currentIndex++;
		}
	}

	public void Update(bool loadSynchronously)
	{
		while (m_actionEnqueued && m_currentIndex < m_actions.Count)
		{
			m_actionEnqueued = false;
			if (!m_actions[m_currentIndex].TryRun(EnqueueNextAction, m_dataModelProvider, m_stateCollectionContext, loadSynchronously))
			{
				EnqueueNextAction(AsyncOperationResult.Success);
			}
		}
		if (HasActionsToRun)
		{
			m_actions[m_currentIndex].Update();
		}
	}

	public void Abort()
	{
		m_actionEnqueued = false;
		if (HasActionsToRun)
		{
			StateAction stateAction = m_actions[m_currentIndex];
			m_currentIndex = -1;
			stateAction.Abort();
		}
		InvokeCallback(AsyncOperationResult.Aborted, m_callbackUserData);
	}

	public void CancelAsyncOperations()
	{
		if (m_actions == null)
		{
			return;
		}
		foreach (StateAction action in m_actions)
		{
			action.CancelAsyncOperations();
		}
	}

	private void InvokeCallback(AsyncOperationResult result, object userData)
	{
		if (m_callback != null)
		{
			if (result == AsyncOperationResult.Wait)
			{
				m_callback?.Invoke(result, userData);
				return;
			}
			ActivateCallbackDelegate callback = m_callback;
			m_callback = null;
			callback?.Invoke(result, userData);
		}
	}

	public List<GameObject> GetGameObjectsTargetedByShowActions()
	{
		List<GameObject> gameObjects = new List<GameObject>();
		foreach (StateAction action in m_actions)
		{
			if (action.Type == StateAction.ActionType.ShowGameObject)
			{
				GameObject go = action.TargetGameObject;
				if (go != null)
				{
					gameObjects.Add(go);
				}
			}
		}
		return gameObjects;
	}

	public bool EvaluateTriggers(IDataModelProvider dataModelProvider, IWidgetStateCollection stateCollection, out bool eventsRaised)
	{
		eventsRaised = false;
		if (m_triggers == null || m_triggers.Count == 0)
		{
			return false;
		}
		if (m_triggerContext == null)
		{
			m_triggerContext = new ScriptContext();
		}
		bool triggerMet = false;
		ScriptString trigger = m_triggers[0];
		if (!string.IsNullOrEmpty(trigger.Script))
		{
			ScriptContext.EvaluationResults result = m_triggerContext.Evaluate(trigger.Script, dataModelProvider, stateCollection);
			if (object.Equals(result.Value, true))
			{
				eventsRaised |= result.EventRaised;
				triggerMet = true;
			}
		}
		return triggerMet;
	}

	public HashSet<int> GetDataModelIDsFromTrigger()
	{
		if (m_triggers == null || m_triggers.Count == 0)
		{
			return null;
		}
		ScriptString trigger = m_triggers[0];
		if (m_dataModelIDs == null)
		{
			m_dataModelIDs = new HashSet<int>();
		}
		if (m_lastTriggerScriptString != trigger.Script)
		{
			m_dataModelIDs.Clear();
			trigger.GetDataModelIDs(m_dataModelIDs);
			m_lastTriggerScriptString = trigger.Script;
		}
		return m_dataModelIDs;
	}

	[Conditional("UNITY_EDITOR")]
	private void Log(string message, string type, LogLevel level = LogLevel.Info)
	{
		Hearthstone.UI.Logging.Log.Get().AddMessage(message, this, level, type);
	}

	public bool ReevaluateState(IDataModelProvider dataModelProvider)
	{
		return CheckForDatamodelChanges(dataModelProvider);
	}

	private bool CheckForDatamodelChanges(IDataModelProvider dataModelProvider)
	{
		bool shouldReevaluate = false;
		List<StateAction> actionsToBeReevaluated = new List<StateAction>();
		foreach (StateAction action in m_actions)
		{
			if (ActionCanBeReevaluated(action))
			{
				actionsToBeReevaluated.Add(action);
			}
		}
		if (m_dataModelToPropertyHash.Count == 0 && actionsToBeReevaluated != null)
		{
			foreach (StateAction action2 in actionsToBeReevaluated)
			{
				shouldReevaluate |= BuildPropertiesDictionaryFromScriptString(action2.GetValueScript(0));
			}
		}
		IEnumerator<IDataModel> dataModels = dataModelProvider.GetDataModels().GetEnumerator();
		while (dataModels.MoveNext())
		{
			IDataModel dataModel = dataModels.Current;
			int dataModelId = dataModel.DataModelId;
			if (!m_dataModelToPropertyHash.ContainsKey(dataModelId))
			{
				continue;
			}
			DataModelProperty[] properties = dataModel.Properties;
			for (int i = 0; i < properties.Length; i++)
			{
				int propertyId = properties[i].PropertyId;
				if (!m_dataModelToPropertyHash[dataModelId].ContainsKey(propertyId) || !dataModel.GetPropertyValue(propertyId, out var propertyValue))
				{
					continue;
				}
				if (!(propertyValue is IDataModelProperties dataModelPropertyValue))
				{
					if (propertyValue != null)
					{
						object propertyValueObject = propertyValue;
						shouldReevaluate |= m_dataModelToPropertyHash[dataModelId][propertyId] != propertyValueObject.GetHashCode();
						m_dataModelToPropertyHash[dataModelId][propertyId] = propertyValueObject.GetHashCode();
					}
					else if (propertyValue != null)
					{
						shouldReevaluate |= m_dataModelToPropertyHash[dataModelId][propertyId] != propertyValue.GetHashCode();
						m_dataModelToPropertyHash[dataModelId][propertyId] = propertyValue.GetHashCode();
					}
				}
				else
				{
					shouldReevaluate |= m_dataModelToPropertyHash[dataModelId][propertyId] != dataModelPropertyValue.GetPropertiesHashCode();
					m_dataModelToPropertyHash[dataModelId][propertyId] = dataModelPropertyValue.GetPropertiesHashCode();
				}
			}
		}
		return shouldReevaluate;
	}

	private bool BuildPropertiesDictionaryFromScriptString(ScriptString script)
	{
		if (string.IsNullOrEmpty(script.Script))
		{
			return false;
		}
		bool shouldReevaluate = false;
		Dictionary<int, HashSet<int>> propertyTable = script.GetPropertyIds();
		foreach (int v in propertyTable.Keys)
		{
			if (!m_dataModelToPropertyHash.ContainsKey(v))
			{
				m_dataModelToPropertyHash.Add(v, new Dictionary<int, int>());
				shouldReevaluate = true;
			}
			foreach (int p in propertyTable[v])
			{
				if (!m_dataModelToPropertyHash[v].ContainsKey(p))
				{
					m_dataModelToPropertyHash[v].Add(p, 0);
					shouldReevaluate = true;
				}
			}
		}
		return shouldReevaluate;
	}

	private static bool ActionCanBeReevaluated(StateAction a)
	{
		return a.Type == StateAction.ActionType.Override;
	}

	public bool ObservesDataModelWithId(int id)
	{
		return m_dataModelToPropertyHash.ContainsKey(id);
	}
}
