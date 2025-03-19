using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus Audio")]
[Tooltip("Stops the ducking started by AudioStartDuckingAction on this object.")]
public class AudioStopDuckingAction : FsmStateAction
{
	[Tooltip("Game Object whose ducking we want to stop.")]
	[RequiredField]
	public FsmOwnerDefault m_GameObject;

	public override void Reset()
	{
	}

	public override void OnEnter()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (go != null)
		{
			SoundDucker ducker = null;
			ducker = go.GetComponent<SoundDucker>();
			if (ducker != null)
			{
				ducker.StopDucking();
				Object.Destroy(ducker);
			}
		}
		Finish();
	}
}
