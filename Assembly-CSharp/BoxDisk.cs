using System.Collections;
using UnityEngine;

public class BoxDisk : MonoBehaviour
{
	public enum State
	{
		LOADING,
		MAINMENU
	}

	private Box m_parent;

	private BoxDiskStateInfo m_info;

	private State m_state;

	public void SetParent(Box parent)
	{
		m_parent = parent;
	}

	public Box GetParent()
	{
		return m_parent;
	}

	public BoxDiskStateInfo GetInfo()
	{
		return m_info;
	}

	public void SetInfo(BoxDiskStateInfo info)
	{
		m_info = info;
	}

	public bool ChangeState(State state)
	{
		if (m_state == state)
		{
			return false;
		}
		m_state = state;
		switch (state)
		{
		case State.LOADING:
		{
			m_parent.OnAnimStarted();
			Vector3 angleDeltas2 = m_info.m_LoadingRotation - base.transform.localRotation.eulerAngles;
			Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
			args2.Add("amount", angleDeltas2);
			args2.Add("delay", m_info.m_LoadingDelaySec);
			args2.Add("time", m_info.m_LoadingRotateSec);
			args2.Add("easetype", m_info.m_LoadingRotateEaseType);
			args2.Add("space", Space.Self);
			args2.Add("oncomplete", "OnAnimFinished");
			args2.Add("oncompletetarget", m_parent.gameObject);
			iTween.RotateAdd(base.gameObject, args2);
			m_parent.GetEventSpell(BoxEventType.DISK_LOADING).ActivateState(SpellStateType.BIRTH);
			break;
		}
		case State.MAINMENU:
		{
			m_parent.OnAnimStarted();
			Vector3 angleDeltas = m_info.m_MainMenuRotation - base.transform.localRotation.eulerAngles;
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("amount", angleDeltas);
			args.Add("delay", m_info.m_MainMenuDelaySec);
			args.Add("time", m_info.m_MainMenuRotateSec);
			args.Add("easetype", m_info.m_MainMenuRotateEaseType);
			args.Add("space", Space.Self);
			args.Add("oncomplete", "OnAnimFinished");
			args.Add("oncompletetarget", m_parent.gameObject);
			iTween.RotateAdd(base.gameObject, args);
			m_parent.GetEventSpell(BoxEventType.DISK_MAIN_MENU).ActivateState(SpellStateType.BIRTH);
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
		case State.LOADING:
			base.transform.localRotation = Quaternion.Euler(m_info.m_LoadingRotation);
			m_parent.GetEventSpell(BoxEventType.DISK_LOADING).ActivateState(SpellStateType.ACTION);
			break;
		case State.MAINMENU:
			base.transform.localRotation = Quaternion.Euler(m_info.m_MainMenuRotation);
			m_parent.GetEventSpell(BoxEventType.DISK_MAIN_MENU).ActivateState(SpellStateType.ACTION);
			break;
		}
	}
}
