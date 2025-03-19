using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Used to control the state of the Pegasus Highlight system")]
[ActionCategory("Pegasus")]
public class HighlightUpdateAction : FsmStateAction
{
	[Tooltip("GameObject to send highlight states to")]
	[RequiredField]
	public FsmOwnerDefault m_gameObj;

	private DelayedEvent delayedEvent;

	public override void Reset()
	{
		m_gameObj = null;
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
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ForceUpdate();
		}
		Finish();
	}
}
