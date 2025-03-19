using System.Collections;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Plays an audio clip dependent on a Spell's Card's ID.")]
[ActionCategory("Pegasus Audio")]
public class SpellCardIdAudioPlayAction : SpellCardIdAudioAction
{
	public FsmOwnerDefault m_SpellObject;

	[Tooltip("Which Card to check on the Spell.")]
	public Which m_WhichCard;

	[Tooltip("The GameObject with the AudioSource component.")]
	[CheckForComponent(typeof(AudioSource))]
	public FsmOwnerDefault m_AudioSourceObject;

	[CompoundArray("Sounds", "Card Id", "Sound")]
	[RequiredField]
	public string[] m_CardIds;

	public SoundDef[] m_Sounds;

	public SoundDef m_DefaultSound;

	[HasFloatSlider(0f, 1f)]
	[Tooltip("Scales the volume of the AudioSource just for this Play call.")]
	public FsmFloat m_VolumeScale;

	public float m_Delay;

	[Tooltip("Wait for the Audio Source to finish playing before moving on.")]
	public FsmBool m_WaitForFinish;

	private float m_DelayTime;

	protected override GameObject GetSpellOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_SpellObject);
	}

	public override void Reset()
	{
		m_SpellObject = null;
		m_WhichCard = Which.SOURCE;
		m_AudioSourceObject = null;
		m_CardIds = new string[2];
		m_Sounds = new SoundDef[2];
		m_DefaultSound = null;
		m_VolumeScale = 1f;
		m_WaitForFinish = false;
		m_Delay = 0f;
		m_DelayTime = 0f;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_DelayTime = m_Delay;
		StartCoroutine(Delay());
	}

	private void Play()
	{
		AudioSource source = GetAudioSource(m_AudioSourceObject);
		if (source == null)
		{
			Finish();
			return;
		}
		SoundDef sound = GetClipMatchingCardId(m_WhichCard, m_CardIds, m_Sounds, m_DefaultSound);
		if (sound == null)
		{
			Finish();
			return;
		}
		if (m_VolumeScale.IsNone)
		{
			SoundManager.Get().PlayOneShot(source, sound);
		}
		else
		{
			SoundManager.Get().PlayOneShot(source, sound, m_VolumeScale.Value);
		}
		if (!m_WaitForFinish.Value || !SoundManager.Get().IsActive(source))
		{
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
		AudioSource source = GetAudioSource(m_AudioSourceObject);
		while (m_WaitForFinish.Value && SoundManager.Get().IsActive(source))
		{
			yield return null;
		}
		Finish();
	}
}
