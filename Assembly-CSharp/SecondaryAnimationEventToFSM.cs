using UnityEngine;

public class SecondaryAnimationEventToFSM : MonoBehaviour
{
	public PlayMakerFSM PlayMaker;

	private SecondaryAnimationEventListener m_eventListener;

	private void Awake()
	{
		m_eventListener = GetComponentInChildren<SecondaryAnimationEventListener>();
	}

	private void OnEnable()
	{
		if ((bool)m_eventListener)
		{
			m_eventListener.OnAnimationEvent += ForwardAnimationEvent;
		}
	}

	private void OnDisable()
	{
		if ((bool)m_eventListener)
		{
			m_eventListener.OnAnimationEvent -= ForwardAnimationEvent;
		}
	}

	private void ForwardAnimationEvent(SecondaryAnimationEvent animEvent)
	{
		if (PlayMaker != null && animEvent != 0)
		{
			PlayMaker.SendEvent($"On{animEvent}");
		}
	}
}
