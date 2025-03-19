using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Used to Show and Hide card Highlights")]
public class HighlightShowHide : FsmStateAction
{
	[RequiredField]
	[Tooltip("GameObject to send highlight states to")]
	public FsmOwnerDefault m_gameObj;

	[RequiredField]
	[Tooltip("Show or Hide")]
	public FsmBool m_Show = true;

	private DelayedEvent delayedEvent;

	public override void Reset()
	{
		m_gameObj = null;
		m_Show = true;
	}

	public override void OnEnter()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_gameObj);
		if (go == null)
		{
			Finish();
			return;
		}
		HighlightState[] hlStates = go.GetComponentsInChildren<HighlightState>();
		if (hlStates == null)
		{
			Finish();
			return;
		}
		HighlightState[] array = hlStates;
		foreach (HighlightState hls in array)
		{
			if (m_Show.Value)
			{
				hls.Show();
			}
			else
			{
				hls.Hide();
			}
		}
		Finish();
	}
}
