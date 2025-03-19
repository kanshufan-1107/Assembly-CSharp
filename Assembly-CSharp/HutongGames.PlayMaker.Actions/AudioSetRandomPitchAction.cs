using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Randomly sets the pitch of an AudioSource on a Game Object.")]
[ActionCategory("Pegasus Audio")]
public class AudioSetRandomPitchAction : FsmStateAction
{
	[RequiredField]
	[CheckForComponent(typeof(AudioSource))]
	public FsmOwnerDefault m_GameObject;

	[HasFloatSlider(-3f, 3f)]
	public FsmFloat m_MinPitch;

	[HasFloatSlider(-3f, 3f)]
	public FsmFloat m_MaxPitch;

	public bool m_EveryFrame;

	private float m_pitch;

	public override void Reset()
	{
		m_GameObject = null;
		m_MinPitch = 1f;
		m_MaxPitch = 1f;
		m_EveryFrame = false;
	}

	public override void OnEnter()
	{
		ChoosePitch();
		UpdatePitch();
		if (!m_EveryFrame)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		UpdatePitch();
	}

	private void ChoosePitch()
	{
		float minPitch = (m_MinPitch.IsNone ? 1f : m_MinPitch.Value);
		float maxPitch = (m_MaxPitch.IsNone ? 1f : m_MaxPitch.Value);
		m_pitch = Random.Range(minPitch, maxPitch);
	}

	private void UpdatePitch()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (!(go == null))
		{
			AudioSource source = go.GetComponent<AudioSource>();
			if (!(source == null))
			{
				SoundManager.Get().SetPitch(source, m_pitch);
			}
		}
	}
}
