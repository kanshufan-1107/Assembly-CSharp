using System.Collections;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Plays the Audio Clip on a Game Object or plays a one shot clip. Waits for the audio to finish.")]
[ActionCategory("Pegasus Audio")]
public class AudioPlayAction : FsmStateAction
{
	[Tooltip("The GameObject with the AudioSource component.")]
	[CheckForComponent(typeof(AudioSource))]
	[RequiredField]
	public FsmOwnerDefault m_GameObject;

	[HasFloatSlider(0f, 1f)]
	[Tooltip("Scales the volume of the AudioSource just for this Play call.")]
	public FsmFloat m_VolumeScale;

	public float m_Delay;

	[ObjectType(typeof(SoundDef))]
	[Tooltip("Optionally play a one shot AudioClip.")]
	public FsmObject m_OneShotSound;

	[Tooltip("Event to send when the AudioSource finishes playing.")]
	public FsmEvent m_FinishedEvent;

	private float m_DelayTime;

	public override void Reset()
	{
		m_GameObject = null;
		m_VolumeScale = 1f;
		m_Delay = 0f;
		m_DelayTime = 0f;
		m_OneShotSound = null;
	}

	public override void OnEnter()
	{
		m_DelayTime = m_Delay;
		StartCoroutine(Delay());
	}

	private AudioSource GetSource()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (go == null)
		{
			return null;
		}
		return go.GetComponent<AudioSource>();
	}

	private void Play()
	{
		AudioSource source = GetSource();
		if (source == null)
		{
			base.Fsm.Event(m_FinishedEvent);
			Finish();
			return;
		}
		SoundDef soundDef = m_OneShotSound.Value as SoundDef;
		if (soundDef == null)
		{
			if (!m_VolumeScale.IsNone)
			{
				SoundManager.Get().SetVolume(source, m_VolumeScale.Value);
			}
			SoundManager.Get().Play(source);
		}
		else
		{
			SoundPlayClipArgs args = new SoundPlayClipArgs();
			args.m_templateSource = source;
			args.m_def = soundDef;
			if (!m_VolumeScale.IsNone)
			{
				args.m_volume = m_VolumeScale.Value;
			}
			args.m_parentObject = source.gameObject;
			if (SoundManager.Get() != null)
			{
				SoundManager.Get().PlayClip(args);
			}
		}
		if (!SoundManager.Get().IsActive(source))
		{
			base.Fsm.Event(m_FinishedEvent);
			Finish();
		}
	}

	private IEnumerator Delay()
	{
		while (m_DelayTime > 0f)
		{
			m_DelayTime -= Time.deltaTime;
			yield return null;
		}
		Play();
		AudioSource source = GetSource();
		while (SoundManager.Get().IsActive(source))
		{
			yield return null;
		}
		base.Fsm.Event(m_FinishedEvent);
		Finish();
	}
}
