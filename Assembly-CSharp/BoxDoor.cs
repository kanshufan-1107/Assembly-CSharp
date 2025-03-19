using System.Collections;
using UnityEngine;

public class BoxDoor : MonoBehaviour
{
	public enum State
	{
		CLOSED,
		OPENED
	}

	private const float BOX_SLIDE_PERCENTAGE_PHONE = 1.038f;

	private Box m_parent;

	private BoxDoorStateInfo m_info;

	private State m_state;

	private bool m_main;

	private Vector3 m_startingPosition;

	private void Awake()
	{
		m_startingPosition = base.gameObject.transform.localPosition;
	}

	public Box GetParent()
	{
		return m_parent;
	}

	public void SetParent(Box parent)
	{
		m_parent = parent;
	}

	public BoxDoorStateInfo GetInfo()
	{
		return m_info;
	}

	public void SetInfo(BoxDoorStateInfo info)
	{
		m_info = info;
	}

	public void EnableMain(bool enable)
	{
		m_main = enable;
	}

	private bool IsMain()
	{
		return m_main;
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
		case State.CLOSED:
		{
			m_parent.OnAnimStarted();
			Vector3 angleDeltas2 = m_info.m_ClosedRotation - m_info.m_OpenedRotation;
			Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
			args2.Add("amount", angleDeltas2);
			args2.Add("delay", m_info.m_ClosedDelaySec);
			args2.Add("time", m_info.m_ClosedRotateSec);
			args2.Add("easetype", m_info.m_ClosedRotateEaseType);
			args2.Add("space", Space.Self);
			args2.Add("oncomplete", "OnAnimFinished");
			args2.Add("oncompletetarget", m_parent.gameObject);
			iTween.RotateAdd(base.gameObject, args2);
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				Hashtable phoneArgs2 = iTweenManager.Get().GetTweenHashTable();
				phoneArgs2.Add("position", m_startingPosition);
				phoneArgs2.Add("islocal", true);
				phoneArgs2.Add("delay", m_info.m_ClosedDelaySec);
				phoneArgs2.Add("time", m_info.m_ClosedRotateSec);
				phoneArgs2.Add("easetype", m_info.m_ClosedRotateEaseType);
				iTween.MoveTo(base.gameObject, phoneArgs2);
			}
			if (IsMain())
			{
				m_parent.GetEventSpell(BoxEventType.DOORS_CLOSE).Activate();
				m_parent.GetEventSpell(BoxEventType.SHADOW_FADE_IN).ActivateState(SpellStateType.BIRTH);
			}
			break;
		}
		case State.OPENED:
		{
			m_parent.OnAnimStarted();
			Vector3 angleDeltas = m_info.m_OpenedRotation - m_info.m_ClosedRotation;
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("amount", angleDeltas);
			args.Add("delay", m_info.m_OpenedDelaySec);
			args.Add("time", m_info.m_OpenedRotateSec);
			args.Add("easetype", m_info.m_OpenedRotateEaseType);
			args.Add("space", Space.Self);
			args.Add("oncomplete", "OnAnimFinished");
			args.Add("oncompletetarget", m_parent.gameObject);
			iTween.RotateAdd(base.gameObject, args);
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				Vector3 position = m_startingPosition;
				position.x *= 1.038f;
				Hashtable phoneArgs = iTweenManager.Get().GetTweenHashTable();
				phoneArgs.Add("position", position);
				phoneArgs.Add("islocal", true);
				phoneArgs.Add("delay", m_info.m_ClosedDelaySec);
				phoneArgs.Add("time", m_info.m_ClosedRotateSec);
				phoneArgs.Add("easetype", m_info.m_ClosedRotateEaseType);
				iTween.MoveTo(base.gameObject, phoneArgs);
			}
			if (IsMain())
			{
				m_parent.GetEventSpell(BoxEventType.DOORS_OPEN).Activate();
				m_parent.GetEventSpell(BoxEventType.SHADOW_FADE_OUT).ActivateState(SpellStateType.BIRTH);
			}
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
			base.transform.localRotation = Quaternion.Euler(m_info.m_ClosedRotation);
			m_parent.GetEventSpell(BoxEventType.SHADOW_FADE_IN).ActivateState(SpellStateType.ACTION);
			break;
		case State.OPENED:
			base.transform.localRotation = Quaternion.Euler(m_info.m_OpenedRotation);
			m_parent.GetEventSpell(BoxEventType.SHADOW_FADE_OUT).ActivateState(SpellStateType.ACTION);
			break;
		}
	}
}
