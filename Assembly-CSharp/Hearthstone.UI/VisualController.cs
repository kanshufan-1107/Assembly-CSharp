using System;
using System.Collections.Generic;
using System.Diagnostics;
using Hearthstone.UI.Core;
using Hearthstone.UI.Logging;
using Unity.Profiling;
using UnityEngine;

namespace Hearthstone.UI;

[HelpURL("https://confluence.blizzard.com/x/LaSZJ")]
[AddComponentMenu("")]
[ExecuteAlways]
public class VisualController : WidgetBehavior, IWidgetEventListener
{
	public delegate void OnStateChangedDelegate(VisualController controller);

	[SerializeField]
	[HideInInspector]
	private string m_name;

	[SerializeField]
	private WidgetBehaviorStateCollection m_stateCollection;

	private bool m_isPendingDefaultState;

	private static ProfilerMarker s_onUpdateProfilerMarker = new ProfilerMarker("VisualController.OnUpdate");

	public string Label => m_name;

	[Overridable]
	public string State
	{
		get
		{
			if (m_stateCollection == null)
			{
				return null;
			}
			return m_stateCollection.ActiveStateName;
		}
		set
		{
			SetState(value);
		}
	}

	[Overridable]
	public int StateIndex
	{
		get
		{
			if (m_stateCollection == null)
			{
				return 0;
			}
			int index = m_stateCollection.States.IndexOf((WidgetBehaviorState)m_stateCollection.ActiveState);
			if (index < 0)
			{
				return 0;
			}
			return index;
		}
		set
		{
			SetState(value);
		}
	}

	public string RequestedState
	{
		get
		{
			if (m_stateCollection == null)
			{
				return null;
			}
			return m_stateCollection.RequestedStateName;
		}
	}

	public bool HasPendingActions
	{
		get
		{
			if (base.IsActive)
			{
				if (m_stateCollection != null)
				{
					return m_stateCollection.HasPendingActions;
				}
				return false;
			}
			return false;
		}
	}

	public override bool IsChangingStates
	{
		get
		{
			if (base.IsActive)
			{
				if (m_stateCollection != null)
				{
					return m_stateCollection.IsChangingStates;
				}
				return true;
			}
			return false;
		}
	}

	public WidgetTemplate OwningWidget => base.Owner;

	private event OnStateChangedDelegate m_onStateChanged;

	public override bool GetIsChangingStates(Func<GameObject, bool> includeGameObject)
	{
		if (!includeGameObject(base.gameObject))
		{
			return false;
		}
		return IsChangingStates;
	}

	public bool HasState(string stateName)
	{
		if (m_stateCollection != null)
		{
			return m_stateCollection.DoesStateExist(stateName);
		}
		return false;
	}

	protected override void OnInitialize()
	{
		if (m_stateCollection != null)
		{
			m_stateCollection.OnStateActivated = HandleStateChanged;
			m_stateCollection.OnStartChangingStates = base.HandleStartChangingStates;
			m_stateCollection.OnDoneChangingStates = base.HandleDoneChangingStates;
			m_stateCollection.WillLoadSynchronously = base.WillLoadSynchronously;
		}
		m_isPendingDefaultState = true;
	}

	public bool SetState(string eventName)
	{
		if (m_stateCollection == null || !m_stateCollection.DoesStateExist(eventName))
		{
			return false;
		}
		m_stateCollection.ActivateState(this, eventName, base.CanTick);
		return true;
	}

	public bool SetState(int stateIndex)
	{
		if (m_stateCollection == null)
		{
			return false;
		}
		if (stateIndex < 0 || stateIndex >= m_stateCollection.States.Count)
		{
			Log.UIFramework.PrintError($"Attempted to set state index ({stateIndex}) on VisualController {base.gameObject.name} ({GetInstanceID()}) but was out of bounds!");
			return false;
		}
		m_stateCollection.ActivateState(this, stateIndex, base.CanTick);
		return true;
	}

	private void HandleStateChanged(AsyncOperationResult result, object userData)
	{
		if (result != AsyncOperationResult.Aborted)
		{
			this.m_onStateChanged?.Invoke(this);
		}
	}

	public override void OnUpdate()
	{
		if (m_stateCollection == null)
		{
			return;
		}
		m_stateCollection.Update(this);
		if (m_isPendingDefaultState)
		{
			if (m_stateCollection.States != null && m_stateCollection.States.Count > 0 && m_stateCollection.RequestedState == null)
			{
				SetState(m_stateCollection.States[0].Name);
			}
			m_isPendingDefaultState = false;
		}
	}

	public override bool TryIncrementDataVersion(int id)
	{
		HashSet<int> dataModelIds = null;
		if (m_stateCollection != null)
		{
			dataModelIds = m_stateCollection.GetDataModelIDsFromTriggers();
		}
		bool num = dataModelIds?.Contains(id) ?? false;
		bool observesThisDataModel = m_stateCollection != null && m_stateCollection.ObservesDataModelWithId(id);
		if (num || observesThisDataModel)
		{
			IncrementLocalDataVersion();
			return true;
		}
		return false;
	}

	public void RegisterStateChangedListener(OnStateChangedDelegate listener)
	{
		m_onStateChanged -= listener;
		m_onStateChanged += listener;
	}

	public WidgetEventListenerResponse EventReceived(string eventName, TriggerEventParameters parameters)
	{
		WidgetEventListenerResponse response = default(WidgetEventListenerResponse);
		if (m_stateCollection != null)
		{
			response.Consumed = m_stateCollection.CanConsumeEvent(eventName);
			WidgetBehaviorState nextState = m_stateCollection.GetNextEnqueuedState();
			if (nextState != null && nextState.Name == eventName)
			{
				return response;
			}
			m_stateCollection.OnStartChangingStates = base.HandleStartChangingStates;
			m_stateCollection.OnDoneChangingStates = base.HandleDoneChangingStates;
			m_stateCollection.EnqueueState(this, eventName, base.CanTick);
		}
		return response;
	}

	[Conditional("UNITY_EDITOR")]
	private void DebugLog(string message, string type)
	{
		Hearthstone.UI.Logging.Log.Get().AddMessage(message, this, LogLevel.Info, type);
	}
}
