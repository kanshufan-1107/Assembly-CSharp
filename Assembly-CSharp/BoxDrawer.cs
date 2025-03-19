using System.Collections;
using UnityEngine;

public class BoxDrawer : MonoBehaviour
{
	public enum State
	{
		CLOSED,
		CLOSED_BOX_OPENED,
		OPENED
	}

	private Box m_parent;

	private BoxDrawerStateInfo m_info;

	private State m_state;

	private void Awake()
	{
		if (m_state == State.CLOSED || m_state == State.CLOSED_BOX_OPENED)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public Box GetParent()
	{
		return m_parent;
	}

	public void SetParent(Box parent)
	{
		m_parent = parent;
	}

	public BoxDrawerStateInfo GetInfo()
	{
		return m_info;
	}

	public void SetInfo(BoxDrawerStateInfo info)
	{
		m_info = info;
	}

	public bool ChangeState(State state)
	{
		if (DemoMgr.Get().GetMode() == DemoMode.PAX_EAST_2013)
		{
			return true;
		}
		if (DemoMgr.Get().GetMode() == DemoMode.BLIZZCON_2013)
		{
			return true;
		}
		if (m_state == state)
		{
			return false;
		}
		State prevState = m_state;
		m_state = state;
		if (IsInactiveState(prevState) && IsInactiveState(m_state))
		{
			return true;
		}
		base.gameObject.SetActive(value: true);
		switch (state)
		{
		case State.CLOSED:
		{
			m_parent.OnAnimStarted();
			Hashtable args3 = iTweenManager.Get().GetTweenHashTable();
			args3.Add("position", m_info.m_ClosedBone.transform.position);
			args3.Add("delay", m_info.m_ClosedDelaySec);
			args3.Add("time", m_info.m_ClosedMoveSec);
			args3.Add("easetype", m_info.m_ClosedMoveEaseType);
			args3.Add("oncomplete", "OnClosedAnimFinished");
			args3.Add("oncompletetarget", base.gameObject);
			iTween.MoveTo(base.gameObject, args3);
			m_parent.GetEventSpell(BoxEventType.DRAWER_CLOSE).Activate();
			break;
		}
		case State.CLOSED_BOX_OPENED:
		{
			m_parent.OnAnimStarted();
			Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
			args2.Add("position", m_info.m_ClosedBoxOpenedBone.transform.position);
			args2.Add("delay", m_info.m_ClosedBoxOpenedDelaySec);
			args2.Add("time", m_info.m_ClosedBoxOpenedMoveSec);
			args2.Add("easetype", m_info.m_ClosedBoxOpenedMoveEaseType);
			args2.Add("oncomplete", "OnClosedBoxOpenedAnimFinished");
			args2.Add("oncompletetarget", base.gameObject);
			iTween.MoveTo(base.gameObject, args2);
			m_parent.GetEventSpell(BoxEventType.DRAWER_CLOSE).Activate();
			break;
		}
		case State.OPENED:
		{
			m_parent.OnAnimStarted();
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("position", m_info.m_OpenedBone.transform.position);
			args.Add("delay", m_info.m_OpenedDelaySec);
			args.Add("time", m_info.m_OpenedMoveSec);
			args.Add("easetype", m_info.m_OpenedMoveEaseType);
			args.Add("oncomplete", "OnOpenedAnimFinished");
			args.Add("oncompletetarget", base.gameObject);
			iTween.MoveTo(base.gameObject, args);
			m_parent.GetEventSpell(BoxEventType.DRAWER_OPEN).Activate();
			break;
		}
		}
		return true;
	}

	public void UpdateState(State state)
	{
		m_state = state;
		switch (state)
		{
		case State.CLOSED:
			base.transform.position = m_info.m_ClosedBone.transform.position;
			base.gameObject.SetActive(value: false);
			break;
		case State.CLOSED_BOX_OPENED:
			base.transform.position = m_info.m_ClosedBoxOpenedBone.transform.position;
			base.gameObject.SetActive(value: false);
			break;
		case State.OPENED:
			base.transform.position = m_info.m_OpenedBone.transform.position;
			base.gameObject.SetActive(value: true);
			break;
		}
	}

	private bool IsInactiveState(State state)
	{
		if (state != 0)
		{
			return state == State.CLOSED_BOX_OPENED;
		}
		return true;
	}

	private void OnClosedAnimFinished()
	{
		base.gameObject.SetActive(value: false);
		m_parent.OnAnimFinished();
	}

	private void OnClosedBoxOpenedAnimFinished()
	{
		base.gameObject.SetActive(value: false);
		m_parent.OnAnimFinished();
	}

	private void OnOpenedAnimFinished()
	{
		base.gameObject.SetActive(value: true);
		m_parent.OnAnimFinished();
	}
}
