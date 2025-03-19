using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class ActorStateMgr : MonoBehaviour
{
	public GameObject m_ObjectContainer;

	private Map<ActorStateType, List<ActorState>> m_actorStateMap = new Map<ActorStateType, List<ActorState>>();

	private ActorStateType m_activeStateType;

	private bool m_shown = true;

	private HighlightState m_HighlightState;

	private int m_initialHighlightRenderQueue;

	private void Start()
	{
		m_HighlightState = FindHightlightObject();
		if (m_HighlightState != null)
		{
			m_initialHighlightRenderQueue = m_HighlightState.m_RenderQueue;
		}
		BuildStateMap();
		if (m_activeStateType == ActorStateType.NONE)
		{
			HideImpl();
		}
		else if (m_shown)
		{
			ShowImpl();
		}
		else
		{
			HideImpl();
		}
	}

	public Map<ActorStateType, List<ActorState>> GetStateMap()
	{
		return m_actorStateMap;
	}

	public ActorStateType GetActiveStateType()
	{
		return m_activeStateType;
	}

	public List<ActorState> GetActiveStateList()
	{
		List<ActorState> stateList = null;
		if (!m_actorStateMap.TryGetValue(m_activeStateType, out stateList))
		{
			return null;
		}
		return stateList;
	}

	public float GetMaximumAnimationTimeOfActiveStates()
	{
		if (GetActiveStateList() == null)
		{
			return 0f;
		}
		float maxTime = 0f;
		foreach (ActorState activeState in GetActiveStateList())
		{
			maxTime = Mathf.Max(activeState.GetAnimationDuration(), maxTime);
		}
		return maxTime;
	}

	public bool ChangeState(ActorStateType stateType)
	{
		if (ChangeState_NewState(stateType))
		{
			return true;
		}
		if (ChangeState_LegacyState(stateType))
		{
			return true;
		}
		return false;
	}

	public bool ChangeState_NewState(ActorStateType stateType)
	{
		if (!m_HighlightState)
		{
			return false;
		}
		ActorStateType activeStateType = m_activeStateType;
		m_activeStateType = stateType;
		if (activeStateType != stateType)
		{
			return m_HighlightState.ChangeState(stateType);
		}
		return true;
	}

	public bool ChangeState_LegacyState(ActorStateType stateType)
	{
		List<ActorState> actorStateList = null;
		m_actorStateMap.TryGetValue(stateType, out actorStateList);
		ActorStateType prevStateType = m_activeStateType;
		m_activeStateType = stateType;
		if (prevStateType != 0)
		{
			if (m_actorStateMap.TryGetValue(prevStateType, out var prevActorStateList))
			{
				foreach (ActorState item in prevActorStateList)
				{
					item.Stop(actorStateList);
				}
			}
		}
		else if (stateType != 0 && m_ObjectContainer != null)
		{
			m_ObjectContainer.SetActive(value: true);
		}
		if (stateType == ActorStateType.NONE)
		{
			if (prevStateType != 0 && m_ObjectContainer != null)
			{
				m_ObjectContainer.SetActive(value: false);
			}
			return true;
		}
		if (actorStateList != null)
		{
			foreach (ActorState item2 in actorStateList)
			{
				item2.Play();
			}
			return true;
		}
		return false;
	}

	public void ShowStateMgr()
	{
		if (!m_shown)
		{
			m_shown = true;
			ShowImpl();
		}
	}

	public void HideStateMgr()
	{
		if (m_shown)
		{
			m_shown = false;
			HideImpl();
		}
	}

	public void RefreshStateMgr()
	{
		if ((bool)m_HighlightState)
		{
			m_HighlightState.SetDirty();
		}
	}

	public bool SetStateRenderQueue(bool reset, int renderQueue)
	{
		if (m_HighlightState == null)
		{
			return false;
		}
		m_HighlightState.m_RenderQueue = (reset ? m_initialHighlightRenderQueue : renderQueue);
		return true;
	}

	private HighlightState FindHightlightObject()
	{
		foreach (Transform item in base.transform)
		{
			HighlightState hlState = item.gameObject.GetComponent<HighlightState>();
			if ((bool)hlState)
			{
				return hlState;
			}
		}
		return null;
	}

	private void BuildStateMap()
	{
		foreach (Transform item in base.transform)
		{
			ActorState state = item.gameObject.GetComponent<ActorState>();
			if (state == null)
			{
				continue;
			}
			ActorStateType stateType = state.m_StateType;
			if (stateType != 0)
			{
				if (!m_actorStateMap.TryGetValue(stateType, out var stateList))
				{
					stateList = new List<ActorState>();
					m_actorStateMap.Add(stateType, stateList);
				}
				stateList.Add(state);
			}
		}
	}

	private void ShowImpl()
	{
		if ((bool)m_HighlightState)
		{
			m_HighlightState.ChangeState(m_activeStateType);
		}
		if (m_activeStateType != 0 && m_ObjectContainer != null)
		{
			m_ObjectContainer.SetActive(value: true);
		}
		List<ActorState> activeStateList = GetActiveStateList();
		if (activeStateList == null)
		{
			return;
		}
		foreach (ActorState item in activeStateList)
		{
			item.ShowState();
		}
	}

	private void HideImpl()
	{
		if ((bool)m_HighlightState)
		{
			m_HighlightState.ChangeState(ActorStateType.NONE);
		}
		List<ActorState> activeStateList = GetActiveStateList();
		if (activeStateList != null)
		{
			foreach (ActorState item in activeStateList)
			{
				item.HideState();
			}
		}
		if (m_activeStateType != 0 && m_ObjectContainer != null)
		{
			m_ObjectContainer.SetActive(value: false);
		}
	}
}
