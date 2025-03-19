using UnityEngine;
using UnityEngine.Audio;

public class AudioSourceSettings
{
	public AudioMixerGroup m_mixerOutput;

	public bool m_bypassEffects;

	public bool m_loop;

	public int m_priority;

	public float m_volume;

	public float m_pitch;

	public float m_stereoPan;

	public float m_spatialBlend;

	public float m_reverbZoneMix;

	public AudioRolloffMode m_rolloffMode;

	public float m_dopplerLevel;

	public float m_minDistance;

	public float m_maxDistance;

	public float m_spread;

	public bool m_changeMixerOutput = true;

	public bool m_changeBypassEffects = true;

	public bool m_changeLoop = true;

	public bool m_changePriority = true;

	public bool m_changeVolume = true;

	public bool m_changePitch = true;

	public bool m_changeStereoPan = true;

	public bool m_changeSpatialBlend = true;

	public bool m_changeReverbZoneMix = true;

	public bool m_changeRolloffMode = true;

	public bool m_changeDopplerLevel = true;

	public bool m_changeMinDistance = true;

	public bool m_changeMaxDistance = true;

	public bool m_changeSpread = true;

	public AudioSourceSettings()
	{
		LoadDefaults();
	}

	public void LoadDefaults()
	{
		m_mixerOutput = null;
		m_bypassEffects = false;
		m_loop = false;
		m_priority = 128;
		m_volume = 1f;
		m_pitch = 1f;
		m_stereoPan = 0f;
		m_spatialBlend = 0f;
		m_reverbZoneMix = 1f;
		m_rolloffMode = AudioRolloffMode.Linear;
		m_dopplerLevel = 1f;
		m_minDistance = 100f;
		m_maxDistance = 500f;
		m_spread = 0f;
	}
}
