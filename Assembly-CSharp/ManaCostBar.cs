using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class ManaCostBar : MonoBehaviour
{
	public GameObject m_manaCostBarObject;

	public GameObject m_ParticleObject;

	public GameObject m_ParticleStart;

	public GameObject m_ParticleEnd;

	public GameObject m_ParticleImpact;

	public float m_maxValue = 10f;

	public float m_BarIntensity = 1.6f;

	public float m_maxIntensity = 2f;

	public float m_increaseAnimTime = 2f;

	public float m_coolDownAnimTime = 1f;

	private float m_previousVal;

	private float m_currentVal;

	private float m_factor;

	private Vector3 m_particleStartPoint = Vector3.zero;

	private Vector3 m_particleEndPoint = Vector3.zero;

	private Material m_barMaterial;

	private ParticleSystem m_particleImpactSystem;

	private List<ParticleSystem> m_allParticleSystems;

	private void Start()
	{
		if (m_manaCostBarObject == null)
		{
			base.enabled = false;
		}
		if (m_ParticleStart != null)
		{
			m_particleStartPoint = m_ParticleStart.transform.localPosition;
		}
		if (m_ParticleEnd != null)
		{
			m_particleEndPoint = m_ParticleEnd.transform.localPosition;
		}
		m_barMaterial = m_manaCostBarObject.GetComponent<Renderer>().GetMaterial();
		m_barMaterial.SetFloat("_Seed", Random.Range(0f, 1f));
		if (m_ParticleImpact != null)
		{
			m_particleImpactSystem = m_ParticleImpact.GetComponent<ParticleSystem>();
		}
	}

	public void SetBar(float newValue)
	{
		m_currentVal = newValue / m_maxValue;
		SetBarValue(m_currentVal);
		m_previousVal = m_currentVal;
	}

	public void AnimateBar(float newValue)
	{
		if (newValue == 0f)
		{
			SetBarValue(0f);
			return;
		}
		m_currentVal = newValue / m_maxValue;
		if (!(m_manaCostBarObject == null) && m_currentVal != m_previousVal)
		{
			if (m_currentVal > m_previousVal)
			{
				m_factor = m_currentVal - m_previousVal;
			}
			else
			{
				m_factor = m_previousVal - m_currentVal;
			}
			m_factor = Mathf.Abs(m_factor);
			if (m_currentVal > m_previousVal)
			{
				IncreaseBar(m_currentVal, m_previousVal);
			}
			else
			{
				DecreaseBar(m_currentVal, m_previousVal);
			}
			m_previousVal = m_currentVal;
		}
	}

	private void SetBarValue(float val)
	{
		m_currentVal = val / m_maxValue;
		if (!(m_manaCostBarObject == null) && m_currentVal != m_previousVal)
		{
			BarPercent_OnUpdate(val);
			ParticlePosition_OnUpdate(val);
			if (val == 0f)
			{
				PlayParticles(state: false);
			}
			m_previousVal = m_currentVal;
		}
	}

	private void IncreaseBar(float newVal, float prevVal)
	{
		float animationTime = m_increaseAnimTime * m_factor;
		PlayParticles(state: true);
		iTween.EaseType easeType = iTween.EaseType.easeInQuad;
		Hashtable barPercentArgs = iTweenManager.Get().GetTweenHashTable();
		barPercentArgs.Add("from", prevVal);
		barPercentArgs.Add("to", newVal);
		barPercentArgs.Add("time", animationTime);
		barPercentArgs.Add("easetype", easeType);
		barPercentArgs.Add("onupdate", "BarPercent_OnUpdate");
		barPercentArgs.Add("oncomplete", "Increase_OnComplete");
		barPercentArgs.Add("oncompletetarget", base.gameObject);
		barPercentArgs.Add("onupdatetarget", base.gameObject);
		barPercentArgs.Add("name", "IncreaseBarPercent");
		iTween.StopByName(m_manaCostBarObject.gameObject, "IncreaseBarPercent");
		iTween.ValueTo(m_manaCostBarObject.gameObject, barPercentArgs);
		Hashtable ParticlePosArgs = iTweenManager.Get().GetTweenHashTable();
		ParticlePosArgs.Add("from", prevVal);
		ParticlePosArgs.Add("to", newVal);
		ParticlePosArgs.Add("time", animationTime);
		ParticlePosArgs.Add("easetype", easeType);
		ParticlePosArgs.Add("onupdate", "ParticlePosition_OnUpdate");
		ParticlePosArgs.Add("onupdatetarget", base.gameObject);
		ParticlePosArgs.Add("name", "ParticlePos");
		iTween.StopByName(m_manaCostBarObject.gameObject, "ParticlePos");
		iTween.ValueTo(m_manaCostBarObject.gameObject, ParticlePosArgs);
		Hashtable IntArgs = iTweenManager.Get().GetTweenHashTable();
		IntArgs.Add("from", m_BarIntensity);
		IntArgs.Add("to", m_maxIntensity);
		IntArgs.Add("time", animationTime);
		IntArgs.Add("easetype", easeType);
		IntArgs.Add("onupdate", "Intensity_OnUpdate");
		IntArgs.Add("onupdatetarget", base.gameObject);
		IntArgs.Add("name", "Intensity");
		iTween.StopByName(m_manaCostBarObject.gameObject, "Intensity");
		iTween.ValueTo(m_manaCostBarObject.gameObject, IntArgs);
	}

	private void DecreaseBar(float newVal, float prevVal)
	{
		float animationTime = m_increaseAnimTime * m_factor;
		PlayParticles(state: true);
		iTween.EaseType easeType = iTween.EaseType.easeOutQuad;
		Hashtable barPercentArgs = iTweenManager.Get().GetTweenHashTable();
		barPercentArgs.Add("from", prevVal);
		barPercentArgs.Add("to", newVal);
		barPercentArgs.Add("time", animationTime);
		barPercentArgs.Add("easetype", easeType);
		barPercentArgs.Add("onupdate", "BarPercent_OnUpdate");
		barPercentArgs.Add("oncomplete", "Decrease_OnComplete");
		barPercentArgs.Add("onupdatetarget", base.gameObject);
		barPercentArgs.Add("name", "IncreaseBarPercent");
		iTween.StopByName(m_manaCostBarObject.gameObject, "IncreaseBarPercent");
		iTween.ValueTo(m_manaCostBarObject.gameObject, barPercentArgs);
		Hashtable ParticlePosArgs = iTweenManager.Get().GetTweenHashTable();
		ParticlePosArgs.Add("from", prevVal);
		ParticlePosArgs.Add("to", newVal);
		ParticlePosArgs.Add("time", animationTime);
		ParticlePosArgs.Add("easetype", easeType);
		ParticlePosArgs.Add("onupdate", "ParticlePosition_OnUpdate");
		ParticlePosArgs.Add("onupdatetarget", base.gameObject);
		ParticlePosArgs.Add("name", "ParticlePos");
		iTween.StopByName(m_manaCostBarObject.gameObject, "ParticlePos");
		iTween.ValueTo(m_manaCostBarObject.gameObject, ParticlePosArgs);
	}

	private void BarPercent_OnUpdate(float val)
	{
		m_barMaterial.SetFloat("_Percent", val);
	}

	private void ParticlePosition_OnUpdate(float val)
	{
		m_ParticleObject.transform.localPosition = Vector3.Lerp(m_particleStartPoint, m_particleEndPoint, val);
	}

	private void Intensity_OnUpdate(float val)
	{
		m_barMaterial.SetFloat("_Intensity", val);
	}

	private void Increase_OnComplete()
	{
		if (m_ParticleImpact != null)
		{
			m_ParticleImpact.GetComponent<ParticleSystem>().Play();
		}
		CoolDown();
	}

	private void CoolDown()
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("from", m_maxIntensity);
		args.Add("to", m_BarIntensity);
		args.Add("time", m_coolDownAnimTime);
		args.Add("easetype", iTween.EaseType.easeOutQuad);
		args.Add("onupdate", "Intensity_OnUpdate");
		args.Add("onupdatetarget", base.gameObject);
		args.Add("name", "CoolDownIntensity");
		args.Add("oncomplete", "CoolDown_OnComplete");
		args.Add("oncompletetarget", base.gameObject);
		iTween.StopByName(m_manaCostBarObject.gameObject, "CoolDownIntensity");
		iTween.ValueTo(m_manaCostBarObject.gameObject, args);
	}

	private void CoolDown_OnComplete()
	{
		iTween.StopByName(m_manaCostBarObject.gameObject, "CoolDownIntensity");
	}

	private void PlayParticles(bool state)
	{
		if (m_allParticleSystems == null)
		{
			m_allParticleSystems = new List<ParticleSystem>();
			m_ParticleObject.GetComponentsInChildren(m_allParticleSystems);
		}
		foreach (ParticleSystem ps in m_allParticleSystems)
		{
			if (state && ps != m_particleImpactSystem)
			{
				ps.Play();
			}
			else
			{
				ps.Stop();
			}
			ParticleSystem.EmissionModule em = ps.emission;
			em.enabled = state;
		}
	}

	private void OnDestroy()
	{
		m_allParticleSystems = null;
	}
}
