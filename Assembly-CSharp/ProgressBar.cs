using System;
using System.Collections;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI.Core;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
	public TextMesh m_label;

	public UberText m_uberLabel;

	public float m_increaseAnimTime = 2f;

	public float m_decreaseAnimTime = 1f;

	public float m_coolDownAnimTime = 1f;

	public float m_barIntensity = 1.2f;

	public float m_barIntensityIncreaseMax = 3f;

	public float m_audioFadeInOut = 0.2f;

	public float m_increasePitchStart = 1f;

	public float m_increasePitchEnd = 1.2f;

	public float m_decreasePitchStart = 1f;

	public float m_decreasePitchEnd = 0.8f;

	private Material m_barMaterial;

	private float m_prevVal;

	private float m_currVal;

	private float m_factor;

	private float m_maxIntensity;

	private float m_Uadd;

	private float m_animationTime;

	private float m_progress;

	private float m_animationValueLastFrame;

	[Overridable]
	public float Progress
	{
		get
		{
			return m_progress;
		}
		set
		{
			SetProgressBar(value);
		}
	}

	public event Action OnProgressBarFilled;

	public void Awake()
	{
		m_barMaterial = GetComponent<Renderer>().GetMaterial();
	}

	public void OnDestroy()
	{
		UnityEngine.Object.Destroy(m_barMaterial);
	}

	public void SetMaterial(Material material)
	{
		m_barMaterial = material;
	}

	public void ResetMaterialReference()
	{
		m_barMaterial = GetComponent<Renderer>().GetMaterial();
	}

	public void AnimateProgress(float prevVal, float currVal, iTween.EaseType easeType = iTween.EaseType.easeOutQuad)
	{
		m_prevVal = prevVal;
		m_currVal = currVal;
		if (m_currVal > m_prevVal)
		{
			m_factor = m_currVal - m_prevVal;
		}
		else
		{
			m_factor = m_prevVal - m_currVal;
		}
		m_factor = Mathf.Abs(m_factor);
		m_animationValueLastFrame = prevVal;
		if (m_currVal > m_prevVal)
		{
			IncreaseProgress(m_currVal, m_prevVal, easeType);
		}
		else
		{
			DecreaseProgress(m_currVal, m_prevVal);
		}
	}

	public void SetProgressBar(float progress)
	{
		m_progress = Mathf.Repeat(progress, 1f);
		if (progress % 1f == 0f && progress != 0f)
		{
			m_progress = 1f;
		}
		if (m_barMaterial == null)
		{
			m_barMaterial = GetComponent<Renderer>().GetMaterial();
		}
		m_barMaterial.SetFloat("_Intensity", m_barIntensity);
		m_barMaterial.SetFloat("_Percent", m_progress);
	}

	public float GetAnimationTime()
	{
		return m_animationTime;
	}

	public void SetLabel(string text)
	{
		if (m_uberLabel != null)
		{
			m_uberLabel.Text = text;
		}
		if (m_label != null)
		{
			m_label.text = text;
		}
	}

	public void SetBarTexture(Texture texture)
	{
		if (m_barMaterial == null)
		{
			m_barMaterial = GetComponent<Renderer>().GetMaterial();
		}
		m_barMaterial.SetTexture("_NoiseTex", texture);
	}

	private void IncreaseProgress(float currProgress, float prevProgress, iTween.EaseType easeType)
	{
		float animationTime = (m_animationTime = m_increaseAnimTime * m_factor);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("from", prevProgress);
		args.Add("to", currProgress);
		args.Add("time", animationTime);
		args.Add("easetype", easeType);
		args.Add("onupdate", "Progress_OnUpdate");
		args.Add("onupdatetarget", base.gameObject);
		args.Add("name", "IncreaseProgress");
		iTween.StopByName(base.gameObject, "IncreaseProgress");
		iTween.ValueTo(base.gameObject, args);
		Hashtable speedArgs = iTweenManager.Get().GetTweenHashTable();
		speedArgs.Add("from", 0f);
		speedArgs.Add("to", 0.005f);
		speedArgs.Add("time", animationTime);
		speedArgs.Add("easetype", iTween.EaseType.easeOutQuad);
		speedArgs.Add("onupdate", "ScrollSpeed_OnUpdate");
		speedArgs.Add("onupdatetarget", base.gameObject);
		speedArgs.Add("name", "UVSpeed");
		iTween.StopByName(base.gameObject, "UVSpeed");
		iTween.ValueTo(base.gameObject, speedArgs);
		m_maxIntensity = m_barIntensity + (m_barIntensityIncreaseMax - m_barIntensity) * Mathf.Clamp01(m_factor);
		Hashtable intensityArgs = iTweenManager.Get().GetTweenHashTable();
		intensityArgs.Add("from", m_barIntensity);
		intensityArgs.Add("to", m_maxIntensity);
		intensityArgs.Add("time", animationTime);
		intensityArgs.Add("easetype", easeType);
		intensityArgs.Add("onupdate", "Intensity_OnUpdate");
		intensityArgs.Add("onupdatetarget", base.gameObject);
		intensityArgs.Add("name", "Intensity");
		intensityArgs.Add("oncomplete", "Intensity_OnComplete");
		intensityArgs.Add("oncompletetarget", base.gameObject);
		iTween.StopByName(base.gameObject, "Intensity");
		iTween.ValueTo(base.gameObject, intensityArgs);
		if (TryGetComponent<AudioSource>(out var audioSource))
		{
			SoundManager.Get().SetVolume(audioSource, 0f);
			SoundManager.Get().SetPitch(audioSource, m_increasePitchStart);
			SoundManager.Get().Play(audioSource);
		}
		Hashtable barVolumeStartArgs = iTweenManager.Get().GetTweenHashTable();
		barVolumeStartArgs.Add("from", 0f);
		barVolumeStartArgs.Add("to", 1f);
		barVolumeStartArgs.Add("time", animationTime * m_audioFadeInOut);
		barVolumeStartArgs.Add("delay", 0f);
		barVolumeStartArgs.Add("easetype", easeType);
		barVolumeStartArgs.Add("onupdate", "AudioVolume_OnUpdate");
		barVolumeStartArgs.Add("onupdatetarget", base.gameObject);
		barVolumeStartArgs.Add("name", "barVolumeStart");
		iTween.StopByName(base.gameObject, "barVolumeStart");
		iTween.ValueTo(base.gameObject, barVolumeStartArgs);
		Hashtable barVolumeEndArgs = iTweenManager.Get().GetTweenHashTable();
		barVolumeEndArgs.Add("from", 1f);
		barVolumeEndArgs.Add("to", 0f);
		barVolumeEndArgs.Add("time", animationTime * m_audioFadeInOut);
		barVolumeEndArgs.Add("delay", animationTime * (1f - m_audioFadeInOut));
		barVolumeEndArgs.Add("easetype", easeType);
		barVolumeEndArgs.Add("onupdate", "AudioVolume_OnUpdate");
		barVolumeEndArgs.Add("onupdatetarget", base.gameObject);
		barVolumeEndArgs.Add("oncomplete", "AudioVolume_OnComplete");
		barVolumeEndArgs.Add("name", "barVolumeEnd");
		iTween.StopByName(base.gameObject, "barVolumeEnd");
		iTween.ValueTo(base.gameObject, barVolumeEndArgs);
		Hashtable barPitchArgs = iTweenManager.Get().GetTweenHashTable();
		barPitchArgs.Add("from", m_increasePitchStart);
		barPitchArgs.Add("to", m_increasePitchEnd);
		barPitchArgs.Add("time", animationTime);
		barPitchArgs.Add("delay", 0f);
		barPitchArgs.Add("easetype", easeType);
		barPitchArgs.Add("onupdate", "AudioPitch_OnUpdate");
		barPitchArgs.Add("onupdatetarget", base.gameObject);
		barPitchArgs.Add("name", "barPitch");
		iTween.StopByName(base.gameObject, "barPitch");
		iTween.ValueTo(base.gameObject, barPitchArgs);
	}

	private void Progress_OnUpdate(float val)
	{
		if (m_barMaterial == null)
		{
			m_barMaterial = GetComponent<Renderer>().GetMaterial();
		}
		float wrappedValue = Mathf.Repeat(val, 1f);
		if (val % 1f == 0f && val != 0f)
		{
			wrappedValue = 1f;
		}
		m_barMaterial.SetFloat("_Percent", wrappedValue);
		if ((m_animationValueLastFrame > wrappedValue && m_animationValueLastFrame != 1f) || wrappedValue == 1f)
		{
			this.OnProgressBarFilled?.Invoke();
		}
		m_animationValueLastFrame = wrappedValue;
	}

	private void Intensity_OnComplete()
	{
		iTween.StopByName(base.gameObject, "Increase");
		iTween.StopByName(base.gameObject, "Intensity");
		iTween.StopByName(base.gameObject, "UVSpeed");
		Hashtable intensityArgs = iTweenManager.Get().GetTweenHashTable();
		intensityArgs.Add("from", m_maxIntensity);
		intensityArgs.Add("to", m_barIntensity);
		intensityArgs.Add("time", m_coolDownAnimTime);
		intensityArgs.Add("easetype", iTween.EaseType.easeOutQuad);
		intensityArgs.Add("onupdate", "Intensity_OnUpdate");
		intensityArgs.Add("onupdatetarget", base.gameObject);
		intensityArgs.Add("name", "Intensity");
		iTween.ValueTo(base.gameObject, intensityArgs);
		Hashtable speedArgs = iTweenManager.Get().GetTweenHashTable();
		speedArgs.Add("from", 0.005f);
		speedArgs.Add("to", 0f);
		speedArgs.Add("time", m_coolDownAnimTime);
		speedArgs.Add("easetype", iTween.EaseType.easeOutQuad);
		speedArgs.Add("onupdate", "ScrollSpeed_OnUpdate");
		speedArgs.Add("onupdatetarget", base.gameObject);
		speedArgs.Add("name", "UVSpeed");
		iTween.ValueTo(base.gameObject, speedArgs);
	}

	private void Intensity_OnUpdate(float val)
	{
		if (m_barMaterial == null)
		{
			m_barMaterial = GetComponent<Renderer>().GetMaterial();
		}
		m_barMaterial.SetFloat("_Intensity", val);
	}

	private void ScrollSpeed_OnUpdate(float val)
	{
		if (m_barMaterial == null)
		{
			m_barMaterial = GetComponent<Renderer>().GetMaterial();
		}
		m_Uadd += val;
		m_barMaterial.SetFloat("_Uadd", m_Uadd);
	}

	private void AudioVolume_OnUpdate(float val)
	{
		if (TryGetComponent<AudioSource>(out var audioSource))
		{
			SoundManager.Get().SetVolume(audioSource, val);
		}
	}

	private void AudioVolume_OnComplete()
	{
		if (TryGetComponent<AudioSource>(out var audioSource))
		{
			SoundManager.Get().Stop(audioSource);
		}
	}

	private void AudioPitch_OnUpdate(float val)
	{
		if (TryGetComponent<AudioSource>(out var audioSource))
		{
			SoundManager.Get().SetPitch(audioSource, val);
		}
	}

	private void DecreaseProgress(float currProgress, float prevProgress)
	{
		float animationTime = (m_animationTime = m_decreaseAnimTime * m_factor);
		iTween.EaseType easeType = iTween.EaseType.easeInOutCubic;
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("from", prevProgress);
		args.Add("to", currProgress);
		args.Add("time", animationTime);
		args.Add("easetype", easeType);
		args.Add("onupdate", "Progress_OnUpdate");
		args.Add("onupdatetarget", base.gameObject);
		args.Add("name", "Decrease");
		iTween.StopByName(base.gameObject, "Decrease");
		iTween.ValueTo(base.gameObject, args);
		if (TryGetComponent<AudioSource>(out var audioSource))
		{
			SoundManager.Get().SetVolume(audioSource, 0f);
			SoundManager.Get().SetPitch(audioSource, m_decreasePitchStart);
			SoundManager.Get().Play(audioSource);
		}
		Hashtable barVolumeStartArgs = iTweenManager.Get().GetTweenHashTable();
		barVolumeStartArgs.Add("from", 0f);
		barVolumeStartArgs.Add("to", 1f);
		barVolumeStartArgs.Add("time", animationTime * m_audioFadeInOut);
		barVolumeStartArgs.Add("delay", 0f);
		barVolumeStartArgs.Add("easetype", easeType);
		barVolumeStartArgs.Add("onupdate", "AudioVolume_OnUpdate");
		barVolumeStartArgs.Add("onupdatetarget", base.gameObject);
		barVolumeStartArgs.Add("name", "barVolumeStart");
		iTween.StopByName(base.gameObject, "barVolumeStart");
		iTween.ValueTo(base.gameObject, barVolumeStartArgs);
		Hashtable barVolumeEndArgs = iTweenManager.Get().GetTweenHashTable();
		barVolumeEndArgs.Add("from", 1f);
		barVolumeEndArgs.Add("to", 0f);
		barVolumeEndArgs.Add("time", animationTime * m_audioFadeInOut);
		barVolumeEndArgs.Add("delay", animationTime * (1f - m_audioFadeInOut));
		barVolumeEndArgs.Add("easetype", easeType);
		barVolumeEndArgs.Add("onupdate", "AudioVolume_OnUpdate");
		barVolumeEndArgs.Add("onupdatetarget", base.gameObject);
		barVolumeEndArgs.Add("oncomplete", "AudioVolume_OnComplete");
		barVolumeEndArgs.Add("name", "barVolumeEnd");
		iTween.StopByName(base.gameObject, "barVolumeEnd");
		iTween.ValueTo(base.gameObject, barVolumeEndArgs);
		Hashtable barPitchArgs = iTweenManager.Get().GetTweenHashTable();
		barPitchArgs.Add("from", m_decreasePitchStart);
		barPitchArgs.Add("to", m_decreasePitchEnd);
		barPitchArgs.Add("time", animationTime);
		barPitchArgs.Add("delay", 0f);
		barPitchArgs.Add("easetype", easeType);
		barPitchArgs.Add("onupdate", "AudioPitch_OnUpdate");
		barPitchArgs.Add("onupdatetarget", base.gameObject);
		barPitchArgs.Add("name", "barPitch");
		iTween.StopByName(base.gameObject, "barPitch");
		iTween.ValueTo(base.gameObject, barPitchArgs);
	}
}
