using System.Collections;
using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Plays the Audio Clip on a Game Object or plays a one shot clip. Does not wait for the audio to finish.")]
[ActionCategory("Pegasus Audio")]
public class AudioPlaythroughAction : FsmStateAction
{
	[CheckForComponent(typeof(AudioSource))]
	[RequiredField]
	[Tooltip("The GameObject with the AudioSource component.")]
	public FsmOwnerDefault m_GameObject;

	[HasFloatSlider(0f, 1f)]
	[Tooltip("Scales the volume of the AudioSource just for this Play call.")]
	public FsmFloat m_VolumeScale;

	[ObjectType(typeof(AudioClip))]
	[Tooltip("Optionally play a One Shot AudioClip.")]
	public FsmObject m_OneShotClip;

	public float m_Delay;

	[Tooltip("Optionally play a one shot AudioClip.")]
	[ObjectType(typeof(SoundDef))]
	public FsmObject m_OneShotSound;

	[Tooltip("Event to send when the AudioSource finishes playing.")]
	public FsmEvent m_FinishedEvent;

	[Tooltip("If true, this audio clip will be suppressed by SUPPRESS_ALL_SUMMON_VO")]
	public bool m_IsMinionSummonVO;

	[Tooltip("In some game modes we want the keyword sound(like from taunt) to still play even if summon vo is suppressed")]
	public bool m_IsKeywordVO;

	[Tooltip("If true, there will be a limit to the number instances of of this sound that can play at once.")]
	public bool m_InstanceLimited;

	[Tooltip("If instance limited, this defines the duration that each clip will prevent another from playing.  If zero, then it will measure the length of the audio file.")]
	public float m_InstanceLimitedDuration;

	[Tooltip("If instance limited, this defines the maximum number of instances of the sound that can be playing at once.")]
	public int m_InstanceLimitMaximum = 1;

	[Tooltip("If instance limited, this defines the action to take when we're about to play a sound that already hit maximum number of instances")]
	public SoundManager.LimitMaxOutOption m_LimitMaxOutOption;

	private float m_DelayTime;

	public override void Reset()
	{
		m_GameObject = null;
		m_VolumeScale = 1f;
		m_OneShotClip = null;
		m_Delay = 0f;
		m_DelayTime = 0f;
		m_OneShotSound = null;
	}

	public override void OnEnter()
	{
		if (m_Delay > 0f)
		{
			m_DelayTime = m_Delay;
			StartCoroutine(Delay());
		}
		else
		{
			Play();
		}
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
		bool skipSuppressVO = m_IsKeywordVO && GameState.Get() != null && GameState.Get().GetGameEntity().HasTag(GAME_TAG.DONT_SUPPRESS_KEYWORD_VO);
		if (m_IsMinionSummonVO && !skipSuppressVO)
		{
			Actor parentActor = GetActor();
			if (parentActor != null && parentActor.GetEntity() != null && parentActor.GetEntity().HasTag(GAME_TAG.SUPPRESS_ALL_SUMMON_VO))
			{
				base.Fsm.Event(m_FinishedEvent);
				Finish();
				return;
			}
		}
		if (TeammateBoardViewer.Get() != null)
		{
			Actor parentActor2 = GetActor();
			TeammateGameObject teammateObject = GameObjectUtils.FindComponentInThisOrParents<TeammateGameObject>(base.Owner);
			if (!TeammateBoardViewer.Get().IsViewingTeammate() && ((parentActor2 != null && parentActor2.IsTeammateActor()) || teammateObject != null))
			{
				base.Fsm.Event(m_FinishedEvent);
				Finish();
				return;
			}
		}
		SoundManager.SoundOptions additionalOptions = new SoundManager.SoundOptions
		{
			InstanceLimited = m_InstanceLimited,
			InstanceTimeLimit = m_InstanceLimitedDuration,
			MaxInstancesOfThisSound = m_InstanceLimitMaximum,
			LimitMaxingOutOption = m_LimitMaxOutOption
		};
		SoundDef soundDef = m_OneShotSound.Value as SoundDef;
		if (soundDef == null)
		{
			if (!m_VolumeScale.IsNone)
			{
				SoundManager.Get().SetVolume(source, m_VolumeScale.Value);
			}
			SoundManager.Get().Play(source, null, null, additionalOptions);
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
				SoundManager.Get().PlayClip(args, createNewSource: true, additionalOptions);
			}
		}
		base.Fsm.Event(m_FinishedEvent);
		Finish();
	}

	private IEnumerator Delay()
	{
		while (m_DelayTime > 0f)
		{
			m_DelayTime -= Time.deltaTime;
			yield return null;
		}
		Play();
	}

	protected Actor GetActor()
	{
		Actor actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(base.Owner);
		if (actor == null)
		{
			Card card = GameObjectUtils.FindComponentInThisOrParents<Card>(base.Owner);
			if (card != null)
			{
				actor = card.GetActor();
			}
		}
		return actor;
	}
}
