using Blizzard.T5.Core;
using UnityEngine;

public class ParticleSystemScaler : MonoBehaviour
{
	public float ParticleSystemScale = 1f;

	public GameObject ObjectToInherit;

	private float m_unitMagnitude;

	private Map<ParticleSystem, ParticleSystemSizes> m_initialValues = new Map<ParticleSystem, ParticleSystemSizes>();

	private void Awake()
	{
		m_unitMagnitude = Vector3.one.magnitude;
	}

	private void Update()
	{
		if (ObjectToInherit != null)
		{
			ParticleSystemScale = ObjectToInherit.transform.lossyScale.magnitude / m_unitMagnitude;
		}
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem system in componentsInChildren)
		{
			ParticleSystem.MainModule main = system.main;
			if (!m_initialValues.ContainsKey(system))
			{
				m_initialValues.Add(system, new ParticleSystemSizes());
				m_initialValues[system].startSpeed = main.startSpeed.constant;
				m_initialValues[system].startSize = main.startSize.constant;
				m_initialValues[system].gravityModifier = main.gravityModifier.constant;
			}
			main.startSize = m_initialValues[system].startSize * ParticleSystemScale;
			main.startSpeed = m_initialValues[system].startSpeed * ParticleSystemScale;
			main.gravityModifier = m_initialValues[system].gravityModifier * ParticleSystemScale;
		}
	}

	private void ScaleParticleSystems(float scaleFactor)
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			ParticleSystem.MainModule main = componentsInChildren[i].main;
			main.startSpeed = main.startSpeed.constant * scaleFactor;
			main.startSize = main.startSize.constant * scaleFactor;
			main.gravityModifier = main.gravityModifier.constant * scaleFactor;
		}
	}
}
