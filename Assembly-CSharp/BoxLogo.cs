using System.Collections;
using UnityEngine;

public class BoxLogo : MonoBehaviour
{
	public enum State
	{
		SHOWN,
		HIDDEN
	}

	private Box m_parent;

	private BoxLogoStateInfo m_info;

	private State m_state;

	public Box GetParent()
	{
		return m_parent;
	}

	public void SetParent(Box parent)
	{
		m_parent = parent;
	}

	public BoxLogoStateInfo GetInfo()
	{
		return m_info;
	}

	public void SetInfo(BoxLogoStateInfo info)
	{
		m_info = info;
	}

	public bool ChangeState(State state)
	{
		if (m_state == state)
		{
			return false;
		}
		if (m_parent == null)
		{
			return false;
		}
		m_state = state;
		switch (state)
		{
		case State.SHOWN:
		{
			m_parent.OnAnimStarted();
			Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
			args2.Add("amount", m_info.m_ShownAlpha);
			args2.Add("delay", m_info.m_ShownDelaySec);
			args2.Add("time", m_info.m_ShownFadeSec);
			args2.Add("easetype", m_info.m_ShownFadeEaseType);
			args2.Add("oncomplete", "OnAnimFinished");
			args2.Add("oncompletetarget", m_parent.gameObject);
			iTween.FadeTo(base.gameObject, args2);
			break;
		}
		case State.HIDDEN:
		{
			m_parent.OnAnimStarted();
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("amount", m_info.m_HiddenAlpha);
			args.Add("delay", m_info.m_HiddenDelaySec);
			args.Add("time", m_info.m_HiddenFadeSec);
			args.Add("easetype", m_info.m_HiddenFadeEaseType);
			args.Add("oncomplete", "OnAnimFinished");
			args.Add("oncompletetarget", m_parent.gameObject);
			iTween.FadeTo(base.gameObject, args);
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
		case State.SHOWN:
			RenderUtils.SetAlpha(base.gameObject, m_info.m_ShownAlpha);
			break;
		case State.HIDDEN:
			RenderUtils.SetAlpha(base.gameObject, m_info.m_HiddenAlpha);
			break;
		}
	}
}
