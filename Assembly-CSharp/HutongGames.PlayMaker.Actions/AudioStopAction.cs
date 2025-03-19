using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus Audio")]
[Tooltip("Stops an Audio Source on a Game Object.")]
public class AudioStopAction : FsmStateAction
{
	[RequiredField]
	[CheckForComponent(typeof(AudioSource))]
	public FsmOwnerDefault m_GameObject;

	public override void Reset()
	{
		m_GameObject = null;
	}

	public override void OnEnter()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (go != null)
		{
			AudioSource source = go.GetComponent<AudioSource>();
			if (source != null)
			{
				SoundManager.Get().Stop(source);
			}
		}
		Finish();
	}
}
