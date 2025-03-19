using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus Audio")]
[Tooltip("Pauses the Audio Source of a Game Object.")]
public class AudioPauseAction : FsmStateAction
{
	[CheckForComponent(typeof(AudioSource))]
	[RequiredField]
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
				SoundManager.Get().Pause(source);
			}
		}
		Finish();
	}
}
