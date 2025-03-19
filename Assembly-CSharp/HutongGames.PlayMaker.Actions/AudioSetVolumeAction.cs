using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Sets the volume of an AudioSource on a Game Object.")]
[ActionCategory("Pegasus Audio")]
public class AudioSetVolumeAction : FsmStateAction
{
	[RequiredField]
	[CheckForComponent(typeof(AudioSource))]
	public FsmOwnerDefault m_GameObject;

	[HasFloatSlider(0f, 1f)]
	public FsmFloat m_Volume;

	public bool m_EveryFrame;

	public override void Reset()
	{
		m_GameObject = null;
		m_Volume = 1f;
		m_EveryFrame = false;
	}

	public override void OnEnter()
	{
		UpdateVolume();
		if (!m_EveryFrame)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		UpdateVolume();
	}

	private void UpdateVolume()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (!(go == null))
		{
			AudioSource source = go.GetComponent<AudioSource>();
			if (!(source == null) && !m_Volume.IsNone)
			{
				SoundManager.Get().SetVolume(source, m_Volume.Value);
			}
		}
	}
}
