using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

[CustomEditClass]
public class StateEventTable : MonoBehaviour
{
	[Serializable]
	public class StateEvent
	{
		public string m_Name;

		public Spell m_Event;
	}

	protected class QueueStateEvent
	{
		public StateEvent m_StateEvent;

		public string m_NameOverride;

		public bool m_SaveAsLastState = true;

		public string GetEventName()
		{
			if (!string.IsNullOrEmpty(m_NameOverride))
			{
				return m_NameOverride;
			}
			return m_StateEvent.m_Name;
		}
	}

	public delegate void StateEventTrigger(Spell evt);

	[CustomEditField(Sections = "Event Table", ListTable = true)]
	public List<StateEvent> m_Events = new List<StateEvent>();

	private Map<string, List<StateEventTrigger>> m_StateEventStartListeners = new Map<string, List<StateEventTrigger>>();

	private Map<string, List<StateEventTrigger>> m_StateEventEndListeners = new Map<string, List<StateEventTrigger>>();

	private Map<string, List<StateEventTrigger>> m_StateEventStartOnceListeners = new Map<string, List<StateEventTrigger>>();

	private Map<string, List<StateEventTrigger>> m_StateEventEndOnceListeners = new Map<string, List<StateEventTrigger>>();

	private QueueList<QueueStateEvent> m_QueuedEvents = new QueueList<QueueStateEvent>();

	private string m_LastState;

	public void TriggerState(string eventName, bool saveLastState = true, string nameOverride = null)
	{
		StateEvent spellEvt = GetStateEvent(eventName);
		if (spellEvt == null)
		{
			Debug.LogError($"{eventName} not defined in event table.", base.gameObject);
			return;
		}
		m_QueuedEvents.Enqueue(new QueueStateEvent
		{
			m_StateEvent = spellEvt,
			m_NameOverride = nameOverride,
			m_SaveAsLastState = saveLastState
		});
		Log.EventTable.Print("Enqueuing event {0}", eventName);
		if (m_QueuedEvents.Count == 1)
		{
			StartNextQueuedState(null);
			return;
		}
		Log.EventTable.Print("Event {0} will not start yet, currently waiting on event {1}.", eventName, m_QueuedEvents.Peek().m_StateEvent.m_Name);
	}

	public bool HasState(string eventName)
	{
		return m_Events.Find((StateEvent e) => e.m_Name == eventName) != null;
	}

	public void CancelQueuedStates()
	{
		m_QueuedEvents.Clear();
	}

	public Spell GetSpellEvent(string eventName)
	{
		return GetStateEvent(eventName)?.m_Event;
	}

	public string GetLastState()
	{
		return m_LastState;
	}

	public void AddStateEventStartListener(string eventName, StateEventTrigger dlg, bool once = false)
	{
		AddStateEventListener(once ? m_StateEventStartOnceListeners : m_StateEventStartListeners, eventName, dlg);
	}

	public void RemoveStateEventStartListener(string eventName, StateEventTrigger dlg)
	{
		RemoveStateEventListener(m_StateEventStartListeners, eventName, dlg);
	}

	public void AddStateEventEndListener(string eventName, StateEventTrigger dlg, bool once = false)
	{
		AddStateEventListener(once ? m_StateEventEndOnceListeners : m_StateEventEndListeners, eventName, dlg);
	}

	public void RemoveStateEventEndListener(string eventName, StateEventTrigger dlg)
	{
		RemoveStateEventListener(m_StateEventEndListeners, eventName, dlg);
	}

	public PlayMakerFSM GetFSMFromEvent(string evtName)
	{
		Spell spell = GetSpellEvent(evtName);
		if (spell != null)
		{
			return spell.GetComponent<PlayMakerFSM>();
		}
		return null;
	}

	public void SetFloatVar(string eventName, string varName, float value)
	{
		PlayMakerFSM fsm = GetFSMFromEvent(eventName);
		if (!(fsm == null))
		{
			fsm.FsmVariables.GetFsmFloat(varName).Value = value;
		}
	}

	public void SetIntVar(string eventName, string varName, int value)
	{
		PlayMakerFSM fsm = GetFSMFromEvent(eventName);
		if (!(fsm == null))
		{
			fsm.FsmVariables.GetFsmInt(varName).Value = value;
		}
	}

	public void SetBoolVar(string eventName, string varName, bool value)
	{
		PlayMakerFSM fsm = GetFSMFromEvent(eventName);
		if (!(fsm == null))
		{
			fsm.FsmVariables.GetFsmBool(varName).Value = value;
		}
	}

	public void SetGameObjectVar(string eventName, string varName, GameObject value)
	{
		PlayMakerFSM fsm = GetFSMFromEvent(eventName);
		if (!(fsm == null))
		{
			fsm.FsmVariables.GetFsmGameObject(varName).Value = value;
		}
	}

	public void SetGameObjectVar(string eventName, string varName, Component value)
	{
		PlayMakerFSM fsm = GetFSMFromEvent(eventName);
		if (!(fsm == null))
		{
			fsm.FsmVariables.GetFsmGameObject(varName).Value = value.gameObject;
		}
	}

	public void SetVector3Var(string eventName, string varName, Vector3 value)
	{
		PlayMakerFSM fsm = GetFSMFromEvent(eventName);
		if (!(fsm == null))
		{
			fsm.FsmVariables.GetFsmVector3(varName).Value = value;
		}
	}

	public void SetVar(string eventName, string varName, object value)
	{
		Action action;
		if (value is GameObject)
		{
			SetGameObjectVar(eventName, varName, (GameObject)value);
		}
		else if (value is Component)
		{
			SetGameObjectVar(eventName, varName, (Component)value);
		}
		else if (new Map<Type, Action>
		{
			{
				typeof(float),
				delegate
				{
					SetFloatVar(eventName, varName, (float)value);
				}
			},
			{
				typeof(int),
				delegate
				{
					SetIntVar(eventName, varName, (int)value);
				}
			},
			{
				typeof(bool),
				delegate
				{
					SetBoolVar(eventName, varName, (bool)value);
				}
			}
		}.TryGetValue(value.GetType(), out action))
		{
			action();
		}
		else
		{
			Debug.LogError($"Set var type ({value.GetType()}) not supported.");
		}
	}

	protected StateEvent GetStateEvent(string eventName)
	{
		return m_Events.Find((StateEvent e) => e.m_Name == eventName);
	}

	private void StartNextQueuedState(QueueStateEvent lastEvt)
	{
		if (m_QueuedEvents.Count == 0)
		{
			if (lastEvt != null)
			{
				FireStateEventFinishedEvent(m_StateEventEndListeners, lastEvt);
				FireStateEventFinishedEvent(m_StateEventEndOnceListeners, lastEvt, clear: true);
			}
			return;
		}
		QueueStateEvent nextEvt = m_QueuedEvents.Peek();
		StateEvent stateEvent = nextEvt.m_StateEvent;
		if (nextEvt.m_SaveAsLastState)
		{
			m_LastState = nextEvt.GetEventName();
		}
		stateEvent.m_Event.AddStateFinishedCallback(QueueNextState, nextEvt);
		FireStateEventFinishedEvent(m_StateEventStartListeners, nextEvt);
		FireStateEventFinishedEvent(m_StateEventStartOnceListeners, nextEvt, clear: true);
		stateEvent.m_Event.Activate();
	}

	private void QueueNextState(Spell spell, SpellStateType prevStateType, object thisStateEvent)
	{
		if (m_QueuedEvents.Count != 0)
		{
			m_QueuedEvents.Dequeue();
			StartNextQueuedState((QueueStateEvent)thisStateEvent);
		}
	}

	private void AddStateEventListener(Map<string, List<StateEventTrigger>> listenerDict, string eventName, StateEventTrigger dlg)
	{
		if (!listenerDict.TryGetValue(eventName, out var listeners))
		{
			listeners = (listenerDict[eventName] = new List<StateEventTrigger>());
		}
		listeners.Add(dlg);
	}

	private void RemoveStateEventListener(Map<string, List<StateEventTrigger>> listenerDict, string eventName, StateEventTrigger dlg)
	{
		if (listenerDict.TryGetValue(eventName, out var listeners))
		{
			listeners.Remove(dlg);
		}
	}

	private void FireStateEventFinishedEvent(Map<string, List<StateEventTrigger>> listenerDict, QueueStateEvent stateEvt, bool clear = false)
	{
		if (listenerDict.TryGetValue(stateEvt.GetEventName(), out var listeners))
		{
			StateEventTrigger[] array = listeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i](stateEvt.m_StateEvent.m_Event);
			}
			if (clear)
			{
				listeners.Clear();
			}
		}
	}
}
