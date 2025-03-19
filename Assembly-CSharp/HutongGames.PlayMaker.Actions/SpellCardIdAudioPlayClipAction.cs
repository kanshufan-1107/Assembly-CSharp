using System.Collections;
using Assets;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus Audio")]
[Tooltip("Generates an AudioSource based on a template, then plays an audio clip dependent on a Spell's Card's ID.")]
public class SpellCardIdAudioPlayClipAction : SpellCardIdAudioAction
{
	public FsmOwnerDefault m_SpellObject;

	[Tooltip("Which Card to check on the Spell.")]
	public Which m_WhichCard;

	[Tooltip("If specified, this Audio Source will be used as a template for the generated Audio Source, otherwise the one in the SoundConfig will be the template.")]
	public AudioSource m_TemplateSource;

	[CompoundArray("Sounds", "Card Id", "Sound")]
	public string[] m_CardIds;

	public SoundDef[] m_Sounds;

	public SoundDef m_DefaultSound;

	[HasFloatSlider(0f, 1f)]
	public FsmFloat m_Volume;

	[HasFloatSlider(-3f, 3f)]
	public FsmFloat m_Pitch;

	[Tooltip("If you want the template Category the Category, change this so that it's not NONE.")]
	public Global.SoundCategory m_Category;

	public float m_Delay;

	private float m_DelayTime;

	protected override GameObject GetSpellOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_SpellObject);
	}

	public override void Reset()
	{
		m_SpellObject = null;
		m_WhichCard = Which.SOURCE;
		m_TemplateSource = null;
		m_CardIds = new string[2];
		m_Sounds = new SoundDef[2];
		m_DefaultSound = null;
		m_Volume = 1f;
		m_Pitch = 1f;
		m_Category = Global.SoundCategory.FX;
		m_Delay = 0f;
		m_DelayTime = 0f;
	}

	public override void OnEnter()
	{
		base.OnEnter();
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
		SoundDef def = GetClipMatchingCardId(m_WhichCard, m_CardIds, m_Sounds, m_DefaultSound);
		if (def == null)
		{
			Finish();
			return;
		}
		SoundPlayClipArgs args = new SoundPlayClipArgs();
		args.m_templateSource = m_TemplateSource;
		args.m_def = def;
		if (!m_Volume.IsNone)
		{
			args.m_volume = m_Volume.Value;
		}
		if (!m_Pitch.IsNone)
		{
			args.m_pitch = m_Pitch.Value;
		}
		if (m_Category != 0)
		{
			args.m_category = m_Category;
		}
		args.m_parentObject = base.Owner;
		SoundManager.Get().PlayClip(args);
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
}
