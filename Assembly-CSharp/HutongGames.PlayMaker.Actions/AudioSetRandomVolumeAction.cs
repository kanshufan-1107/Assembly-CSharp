using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Randomly sets the volume of an AudioSource on a Game Object.")]
[ActionCategory("Pegasus Audio")]
public class AudioSetRandomVolumeAction : FsmStateAction
{
	[CheckForComponent(typeof(AudioSource))]
	[RequiredField]
	public FsmOwnerDefault m_GameObject;

	[HasFloatSlider(0f, 1f)]
	public FsmFloat m_MinVolume;

	[HasFloatSlider(0f, 1f)]
	public FsmFloat m_MaxVolume;

	public bool m_EveryFrame;

	private float m_volume;

	public override void Reset()
	{
		m_GameObject = null;
		m_MinVolume = 1f;
		m_MaxVolume = 1f;
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

	private void ChooseVolume()
	{
		float minVolume = (m_MinVolume.IsNone ? 1f : m_MinVolume.Value);
		float maxVolume = (m_MaxVolume.IsNone ? 1f : m_MaxVolume.Value);
		m_volume = Random.Range(minVolume, maxVolume);
	}

	private void UpdateVolume()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (!(go == null))
		{
			AudioSource source = go.GetComponent<AudioSource>();
			if (!(source == null))
			{
				SoundManager.Get().SetVolume(source, m_volume);
			}
		}
	}
}
