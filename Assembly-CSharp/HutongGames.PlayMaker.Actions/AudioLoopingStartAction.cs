using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus Audio")]
[Tooltip("Plays the Audio Clip on a Game Object and sets it to loop.")]
public class AudioLoopingStartAction : FsmStateAction
{
	[CheckForComponent(typeof(SoundDef))]
	[RequiredField]
	[CheckForComponent(typeof(AudioSource))]
	public FsmOwnerDefault m_GameObject;

	public FsmFloat m_FadeInTime;

	public FsmFloat m_TargetVolume;

	public FsmBool m_randomizeStartPoint;

	[Tooltip("If this FX doesn't get destroyed on death, then it will persist through multiple Births/Deaths. In that case, check this box to keep the audio clip around as well.")]
	public FsmBool m_dontDestroySource;

	private SoundDef m_soundDef;

	private AudioSource m_audioSource;

	private float m_startVolume;

	private float m_startTime;

	private float m_currentTime;

	private float m_endTime;

	private float m_progress;

	public override void Reset()
	{
		m_GameObject = null;
		m_FadeInTime = 0f;
		m_TargetVolume = 0f;
		m_randomizeStartPoint = false;
		m_dontDestroySource = false;
	}

	public override void OnEnter()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (go == null)
		{
			Finish();
			return;
		}
		m_soundDef = go.GetComponent<SoundDef>();
		if (m_soundDef == null)
		{
			Finish();
			return;
		}
		SoundManager.Get();
		m_audioSource = go.GetComponent<AudioSource>();
		m_audioSource.loop = true;
		m_audioSource.enabled = true;
		m_startVolume = 0f;
		m_startTime = FsmTime.RealtimeSinceStartup;
		m_currentTime = m_startTime;
		m_endTime = m_startTime + m_FadeInTime.Value;
		SoundPlayClipArgs args = new SoundPlayClipArgs();
		args.m_def = m_soundDef;
		args.m_templateSource = m_audioSource;
		SoundManager.SoundOptions soundOptions = new SoundManager.SoundOptions();
		soundOptions.RandomStartTime = m_randomizeStartPoint.Value;
		if (m_dontDestroySource.Value)
		{
			SoundManager.Get().Play(m_audioSource, args.m_def, null, soundOptions);
		}
		else
		{
			SoundManager.Get().PlayClip(args, createNewSource: false, soundOptions);
		}
	}

	public override void OnUpdate()
	{
		m_currentTime += Time.deltaTime;
		if (m_currentTime < m_endTime)
		{
			float lerpControl = (m_currentTime - m_startTime) / m_FadeInTime.Value;
			float volume = Mathf.Lerp(m_startVolume, m_TargetVolume.Value, lerpControl);
			m_progress = lerpControl;
			SoundManager.Get().SetVolume(m_audioSource, volume);
		}
		else
		{
			SoundManager.Get().SetVolume(m_audioSource, m_TargetVolume.Value);
			Finish();
		}
	}

	public override float GetProgress()
	{
		return m_progress;
	}
}
