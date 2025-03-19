using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPickPlaymakerEvent : MonoBehaviour
{
	[Serializable]
	public class PickEvent
	{
		public PlayMakerFSM m_FSM;

		public string m_StartEvent;

		public string m_EndEvent;

		[HideInInspector]
		public int m_CurrentItemIndex;
	}

	public int m_AwakeStateIndex = -1;

	public bool m_AllowNoneState = true;

	public List<PickEvent> m_State;

	public List<PickEvent> m_AlternateState;

	private bool m_StateActive;

	private PickEvent m_CurrentState;

	private Collider m_Collider;

	private bool m_AlternateEventState;

	private int m_LastEventIndex;

	private int m_LastAlternateIndex;

	private bool m_StartAnimationFinished = true;

	private bool m_EndAnimationFinished = true;

	private void Awake()
	{
		m_Collider = GetComponent<Collider>();
		if (m_AwakeStateIndex > -1)
		{
			m_CurrentState = m_State[m_AwakeStateIndex];
			m_LastEventIndex = m_AwakeStateIndex;
			m_StateActive = true;
		}
	}

	public void RandomPickEvent()
	{
		if (!m_StartAnimationFinished || !m_EndAnimationFinished)
		{
			return;
		}
		if (m_StateActive && m_CurrentState.m_EndEvent != string.Empty && m_CurrentState.m_FSM != null)
		{
			m_CurrentState.m_FSM.SendEvent(m_CurrentState.m_EndEvent);
			m_EndAnimationFinished = false;
			m_StateActive = false;
			StartCoroutine(WaitForEndAnimation());
		}
		else if (m_AlternateState.Count > 0)
		{
			if (m_AlternateEventState)
			{
				SendRandomEvent();
			}
			else
			{
				SendAlternateRandomEvent();
			}
		}
		else
		{
			SendRandomEvent();
		}
	}

	public void StartAnimationFinished()
	{
		m_StartAnimationFinished = true;
	}

	public void EndAnimationFinished()
	{
		m_EndAnimationFinished = true;
	}

	private void SendRandomEvent()
	{
		m_StateActive = true;
		m_AlternateEventState = false;
		List<int> possibleItems = new List<int>();
		if (m_State.Count == 1)
		{
			possibleItems.Add(0);
		}
		else
		{
			for (int idx = 0; idx < m_State.Count; idx++)
			{
				if (idx != m_LastEventIndex)
				{
					possibleItems.Add(idx);
				}
			}
		}
		int randomIndex = UnityEngine.Random.Range(0, possibleItems.Count);
		PickEvent fsmEvent = (m_CurrentState = m_State[possibleItems[randomIndex]]);
		m_LastEventIndex = possibleItems[randomIndex];
		m_StartAnimationFinished = false;
		StartCoroutine(WaitForStartAnimation());
		fsmEvent.m_FSM.SendEvent(fsmEvent.m_StartEvent);
	}

	private void SendAlternateRandomEvent()
	{
		m_StateActive = true;
		m_AlternateEventState = true;
		List<int> possibleItems = new List<int>();
		if (m_AlternateState.Count == 1)
		{
			possibleItems.Add(0);
		}
		else
		{
			for (int idx = 0; idx < m_AlternateState.Count; idx++)
			{
				if (idx != m_LastAlternateIndex)
				{
					possibleItems.Add(idx);
				}
			}
		}
		int randomIndex = UnityEngine.Random.Range(0, possibleItems.Count);
		PickEvent fsmEvent = (m_CurrentState = m_AlternateState[possibleItems[randomIndex]]);
		m_LastAlternateIndex = possibleItems[randomIndex];
		m_StartAnimationFinished = false;
		StartCoroutine(WaitForStartAnimation());
		fsmEvent.m_FSM.SendEvent(fsmEvent.m_StartEvent);
	}

	private IEnumerator WaitForStartAnimation()
	{
		while (!m_StartAnimationFinished)
		{
			yield return null;
		}
	}

	private IEnumerator WaitForEndAnimation()
	{
		while (!m_EndAnimationFinished)
		{
			yield return null;
		}
		m_CurrentState = null;
		if (!m_AllowNoneState)
		{
			while (!m_StartAnimationFinished)
			{
				yield return null;
			}
			RandomPickEvent();
		}
	}

	private void EnableCollider()
	{
		if (m_Collider != null)
		{
			m_Collider.enabled = true;
		}
	}

	private void DisableCollider()
	{
		if (m_Collider != null)
		{
			m_Collider.enabled = false;
		}
	}
}
