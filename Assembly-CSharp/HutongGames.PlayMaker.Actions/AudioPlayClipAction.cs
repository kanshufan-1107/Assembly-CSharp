using System.Collections;
using Assets;
using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus Audio")]
[Tooltip("Generates an AudioSource based on a template, then plays that source.")]
public class AudioPlayClipAction : FsmStateAction
{
	[Tooltip("Optional. If specified, the generated Audio Source will be placed at the same location as this object.")]
	public FsmOwnerDefault m_ParentObject;

	[ObjectType(typeof(SoundDef))]
	[RequiredField]
	public FsmObject m_Sound;

	[HasFloatSlider(0f, 1f)]
	public FsmFloat m_Volume;

	[HasFloatSlider(-3f, 3f)]
	public FsmFloat m_Pitch;

	[HasFloatSlider(0f, 1f)]
	public FsmFloat m_SpatialBlend;

	public Global.SoundCategory m_Category;

	public float m_Delay;

	[Tooltip("If specified, this Audio Source will be used as a template for the generated Audio Source, otherwise the one in the SoundConfig will be the template.")]
	public AudioSource m_TemplateSource;

	[Tooltip("If true, this audio clip will be suppressed by SUPPRESS_ALL_SUMMON_VO")]
	public bool m_IsMinionSummonVO;

	[Tooltip("In some game modes we want the keyword sound(like from taunt) to still play even if summon vo is suppressed")]
	public bool m_IsKeywordVO;

	[Tooltip("Should the sound play even if the entity is supressing sounds.")]
	public bool m_SkipSuppressEntitySoundCheck;

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
		m_ParentObject = null;
		m_Sound = null;
		m_Volume = 1f;
		m_Pitch = 1f;
		m_SpatialBlend = 0f;
		m_Category = Global.SoundCategory.FX;
		m_TemplateSource = null;
		m_Delay = 0f;
		m_DelayTime = 0f;
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

	private void Play()
	{
		if (m_Sound == null)
		{
			Finish();
			return;
		}
		bool skipSuppressVO = m_IsKeywordVO && GameState.Get() != null && GameState.Get().GetGameEntity().HasTag(GAME_TAG.DONT_SUPPRESS_KEYWORD_VO);
		Actor parentActor = GetActor();
		if (!m_SkipSuppressEntitySoundCheck && parentActor != null && parentActor.GetEntity() != null && parentActor.GetEntity().HasTag(GAME_TAG.SUPPRES_ALL_SOUNDS_FOR_ENTITY))
		{
			Finish();
			return;
		}
		if (m_IsMinionSummonVO && !skipSuppressVO && (GameState.Get() == null || (parentActor != null && parentActor.GetEntity() != null && parentActor.GetEntity().HasTag(GAME_TAG.SUPPRESS_ALL_SUMMON_VO))))
		{
			Finish();
			return;
		}
		SoundPlayClipArgs args = new SoundPlayClipArgs();
		args.m_templateSource = m_TemplateSource;
		args.m_def = m_Sound.Value as SoundDef;
		if (!m_Volume.IsNone)
		{
			args.m_volume = m_Volume.Value;
		}
		if (!m_Pitch.IsNone)
		{
			args.m_pitch = m_Pitch.Value;
		}
		if (!m_SpatialBlend.IsNone)
		{
			args.m_spatialBlend = m_SpatialBlend.Value;
		}
		if (m_Category != 0)
		{
			args.m_category = m_Category;
		}
		args.m_parentObject = base.Fsm.GetOwnerDefaultTarget(m_ParentObject);
		SoundManager.SoundOptions additionalOptions = new SoundManager.SoundOptions
		{
			InstanceLimited = m_InstanceLimited,
			InstanceTimeLimit = m_InstanceLimitedDuration,
			MaxInstancesOfThisSound = m_InstanceLimitMaximum,
			LimitMaxingOutOption = m_LimitMaxOutOption
		};
		SoundManager.Get().PlayClip(args, createNewSource: true, additionalOptions);
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
