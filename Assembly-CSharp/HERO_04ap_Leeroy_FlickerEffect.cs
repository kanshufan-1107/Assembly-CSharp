using UnityEngine;

public class HERO_04ap_Leeroy_FlickerEffect : MonoBehaviour
{
	public LegendarySkin m_legendarySkin;

	public float m_flickerSpeed = 1f;

	public float m_baseIntensity;

	public float m_intensityRange = 0.2f;

	private void Awake()
	{
		if (m_legendarySkin == null)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		float flickerValue = Mathf.PerlinNoise(Time.time * m_flickerSpeed, 0f);
		float intensity = Mathf.Clamp(m_baseIntensity + flickerValue * m_intensityRange, 0f, 1f);
		m_legendarySkin.m_RimLightColorMult = intensity;
	}
}
