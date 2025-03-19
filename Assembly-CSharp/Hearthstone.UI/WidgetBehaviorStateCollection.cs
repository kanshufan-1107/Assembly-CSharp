using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hearthstone.UI;

[Serializable]
public class WidgetBehaviorStateCollection : IWidgetStateCollection
{
	public delegate void ChangingStatesDelegate();

	public static bool m_autoReevaluateStateActions = true;

	[SerializeField]
	private List<WidgetBehaviorState> m_states;

	[SerializeField]
	private bool m_isRadioGroup;

	private WidgetBehaviorState m_activeState;

	private WidgetBehaviorState m_requestedState;

	private IDataModelProvider m_dataModelProvider;

	private bool m_isChangingStates;

	private bool m_transitioning;

	private bool m_isInWaitOperation;

	private int m_lastDataVersion;

	private Queue<WidgetBehaviorState> m_enqueuedStates;

	private List<WidgetBehaviorState> m_requestedStates;

	private HashSet<int> m_dataModelIDs;

	public WidgetBehaviorState.ActivateCallbackDelegate OnStateActivated;

	public ChangingStatesDelegate OnStartChangingStates;

	public ChangingStatesDelegate OnDoneChangingStates;

	public bool WillLoadSynchronously { get; set; }

	public bool IndependentStates { get; set; }

	public IList<WidgetBehaviorState> States => m_states;

	public IDataModelProvider DataModelProvider => m_dataModelProvider;

	public int DataVersion
	{
		get
		{
			if (DataModelProvider == null)
			{
				return 0;
			}
			return DataModelProvider.GetLocalDataVersion();
		}
	}

	public string ActiveStateName
	{
		get
		{
			if (m_activeState != null)
			{
				return m_activeState.Name;
			}
			return null;
		}
	}

	public string RequestedStateName
	{
		get
		{
			if (m_requestedState != null)
			{
				return m_requestedState.Name;
			}
			return null;
		}
	}

	public bool IsChangingStates
	{
		get
		{
			if (!m_isChangingStates)
			{
				return m_lastDataVersion != DataVersion;
			}
			return true;
		}
	}

	public bool HasPendingActions
	{
		get
		{
			if (!m_transitioning)
			{
				return m_lastDataVersion != DataVersion;
			}
			return true;
		}
	}

	public IWidgetState ActiveState => m_activeState;

	public IWidgetState RequestedState => m_requestedState;

	public IList<IWidgetState> GetStateList()
	{
		return m_states.Cast<IWidgetState>().ToList();
	}

	public void ActivateFirstState()
	{
		if (m_states.Count > 0)
		{
			ActivateState(m_states[0]);
		}
	}

	public bool DoesStateExist(string name)
	{
		if (m_states == null)
		{
			return false;
		}
		foreach (WidgetBehaviorState state in m_states)
		{
			if (state.Name == name)
			{
				return true;
			}
		}
		return false;
	}

	public bool ActivateState(IDataModelProvider dataModelProvider, string name, bool updateImmediately = true, bool mustExist = false)
	{
		m_dataModelProvider = dataModelProvider;
		WidgetBehaviorState state = FindStateByName(name, mustExist);
		if (state != null)
		{
			ActivateState(state, updateImmediately);
			return true;
		}
		return false;
	}

	public bool ActivateState(IDataModelProvider dataModelProvider, int stateIndex, bool updateImmediately = true, bool mustExist = false)
	{
		m_dataModelProvider = dataModelProvider;
		WidgetBehaviorState state = FindStateByIndex(stateIndex, mustExist);
		if (state != null)
		{
			ActivateState(state, updateImmediately);
			return true;
		}
		return false;
	}

	public void AbortState(string name)
	{
		FindStateByName(name, logErrorIfNotFound: false)?.Abort();
	}

	public bool EnqueueState(IDataModelProvider dataModelProvider, string name, bool updateImmediately = true, bool mustExist = false)
	{
		m_dataModelProvider = dataModelProvider;
		WidgetBehaviorState state = FindStateByName(name, mustExist);
		if (state != null)
		{
			EnqueueState(state, updateImmediately);
			return true;
		}
		return false;
	}

	public HashSet<int> GetDataModelIDsFromTriggers()
	{
		if (m_dataModelIDs == null)
		{
			m_dataModelIDs = new HashSet<int>();
		}
		else
		{
			m_dataModelIDs.Clear();
		}
		foreach (WidgetBehaviorState state in m_states)
		{
			HashSet<int> triggerIds = state.GetDataModelIDsFromTrigger();
			if (triggerIds != null)
			{
				m_dataModelIDs.UnionWith(triggerIds);
			}
		}
		return m_dataModelIDs;
	}

	public bool DoesAnyTriggerContainDataModelID(int dataModelId)
	{
		foreach (WidgetBehaviorState state in m_states)
		{
			HashSet<int> triggerIds = state.GetDataModelIDsFromTrigger();
			if (triggerIds != null && triggerIds.Contains(dataModelId))
			{
				return true;
			}
		}
		return false;
	}

	private WidgetBehaviorState FindStateByName(string stateName, bool logErrorIfNotFound)
	{
		if (m_states != null)
		{
			foreach (WidgetBehaviorState state in m_states)
			{
				if (state.Name == stateName)
				{
					return state;
				}
			}
		}
		if (logErrorIfNotFound)
		{
			Debug.LogErrorFormat("State '{0}' does not exist", stateName);
		}
		return null;
	}

	private WidgetBehaviorState FindStateByIndex(int index, bool logErrorIfNotFound)
	{
		if (m_states == null)
		{
			if (logErrorIfNotFound)
			{
				Debug.LogErrorFormat("Unexpected error finding state! State list is empty.", index);
			}
			return null;
		}
		if (index < 0 || index >= m_states.Count)
		{
			if (logErrorIfNotFound)
			{
				Debug.LogErrorFormat("State index '{0}' is out of bounds", index);
			}
			return null;
		}
		return m_states[index];
	}

	public bool ObservesDataModelWithId(int id)
	{
		int i = 0;
		for (int iMax = m_states.Count; i < iMax; i++)
		{
			if (m_states[i].ObservesDataModelWithId(id))
			{
				return true;
			}
		}
		return false;
	}

	public void Update(IDataModelProvider dataModelProvider)
	{
		m_dataModelProvider = dataModelProvider;
		if (m_dataModelProvider != null)
		{
			UpdateStateTriggers();
		}
		if (m_requestedState != null)
		{
			m_requestedState.Update(WillLoadSynchronously);
		}
		if (m_requestedStates != null)
		{
			for (int i = 0; i < m_requestedStates.Count; i++)
			{
				m_requestedStates[i].Update(WillLoadSynchronously);
			}
		}
		if (m_isChangingStates && (!m_transitioning || m_isInWaitOperation))
		{
			m_isChangingStates = false;
			OnDoneChangingStates?.Invoke();
		}
	}

	public bool CanConsumeEvent(string eventName)
	{
		return FindStateByName(eventName, logErrorIfNotFound: false)?.ConsumeEvent ?? false;
	}

	private void UpdateStateTriggers()
	{
		int dataVersion = DataVersion;
		if (m_states == null || m_lastDataVersion == dataVersion)
		{
			return;
		}
		m_lastDataVersion = dataVersion;
		WidgetBehaviorState stateToTrigger = null;
		bool anyEventsRaised = false;
		foreach (WidgetBehaviorState state in m_states)
		{
			if (state.EvaluateTriggers(m_dataModelProvider, this, out var eventsRaised) && stateToTrigger == null)
			{
				anyEventsRaised = anyEventsRaised || eventsRaised;
				stateToTrigger = state;
			}
		}
		if (m_autoReevaluateStateActions && m_states.Count != 0)
		{
			WidgetBehaviorState stateToReevaluate = ((m_activeState != null) ? m_activeState : m_states[0]);
			if (stateToReevaluate != null && (stateToReevaluate == stateToTrigger || stateToTrigger == null) && stateToReevaluate.ReevaluateState(m_dataModelProvider))
			{
				anyEventsRaised = true;
				stateToTrigger = stateToReevaluate;
			}
		}
		if (stateToTrigger != null && (stateToTrigger != m_activeState || anyEventsRaised))
		{
			ActivateState(stateToTrigger);
		}
	}

	private void EnqueueState(WidgetBehaviorState state, bool updateImmediately = true)
	{
		if (!m_transitioning && !m_isInWaitOperation)
		{
			ActivateState(state, updateImmediately);
			return;
		}
		if (m_enqueuedStates == null)
		{
			m_enqueuedStates = new Queue<WidgetBehaviorState>();
		}
		m_enqueuedStates.Enqueue(state);
	}

	public WidgetBehaviorState GetNextEnqueuedState()
	{
		if (m_enqueuedStates != null && m_enqueuedStates.Count > 0)
		{
			return m_enqueuedStates.Peek();
		}
		return null;
	}

	private void ActivateState(WidgetBehaviorState state, bool updateImmediately = true)
	{
		if (!m_isChangingStates)
		{
			m_isChangingStates = true;
			OnStartChangingStates?.Invoke();
		}
		if (m_requestedState != null && m_requestedState != state)
		{
			m_requestedState.CancelAsyncOperations();
		}
		m_transitioning = true;
		m_requestedState = state;
		HideIfInRadioGroup(m_requestedState);
		if (IndependentStates)
		{
			if (m_requestedStates == null)
			{
				m_requestedStates = new List<WidgetBehaviorState>();
			}
			if (!m_requestedStates.Contains(state))
			{
				m_requestedStates.Add(state);
			}
		}
		state.Activate(m_dataModelProvider, this, HandleStateActivationResult, state);
		if (updateImmediately)
		{
			state.Update(WillLoadSynchronously);
		}
	}

	private void HandleStateActivationResult(AsyncOperationResult result, object callbackObject)
	{
		WidgetBehaviorState state = (WidgetBehaviorState)callbackObject;
		m_transitioning = (m_requestedStates != null && m_requestedStates.Count > 0) || state.HasActionsToRun;
		if (!m_isChangingStates && m_transitioning && m_isInWaitOperation && result != AsyncOperationResult.Wait)
		{
			m_isChangingStates = true;
			OnStartChangingStates?.Invoke();
		}
		m_isInWaitOperation = result == AsyncOperationResult.Wait;
		if (!m_isInWaitOperation)
		{
			if (m_requestedStates != null)
			{
				m_requestedStates.Remove(state);
			}
			if (result != AsyncOperationResult.Aborted)
			{
				m_activeState = m_requestedState;
			}
			OnStateActivated?.Invoke(result, null);
			if (m_enqueuedStates != null && m_enqueuedStates.Count > 0)
			{
				ActivateState(m_enqueuedStates.Dequeue());
			}
		}
	}

	private void HideIfInRadioGroup(WidgetBehaviorState ignoredState = null)
	{
		if (!m_isRadioGroup || m_states == null)
		{
			return;
		}
		foreach (WidgetBehaviorState state in m_states)
		{
			if (state == ignoredState)
			{
				continue;
			}
			foreach (GameObject gameObjectsTargetedByShowAction in state.GetGameObjectsTargetedByShowActions())
			{
				gameObjectsTargetedByShowAction.SetActive(value: false);
			}
		}
	}
}
