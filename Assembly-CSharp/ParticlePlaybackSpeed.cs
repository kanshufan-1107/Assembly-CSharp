using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

[ExecuteAlways]
public class ParticlePlaybackSpeed : MonoBehaviour
{
	public float m_ParticlePlaybackSpeed = 1f;

	public bool m_RestoreSpeedOnDisable = true;

	private float m_PreviousPlaybackSpeed = 1f;

	private Map<ParticleSystem, float> m_OrgPlaybackSpeed;

	private List<ParticleSystem> m_ParticleSystems;

	private void Start()
	{
		Init();
	}

	private void Update()
	{
		if (m_ParticlePlaybackSpeed == m_PreviousPlaybackSpeed)
		{
			return;
		}
		m_PreviousPlaybackSpeed = m_ParticlePlaybackSpeed;
		int i = 0;
		while (i < m_ParticleSystems.Count)
		{
			ParticleSystem particleSystem = m_ParticleSystems[i];
			if ((bool)particleSystem)
			{
				ParticleSystem.MainModule particleMain = particleSystem.main;
				particleMain.simulationSpeed = m_ParticlePlaybackSpeed;
				i++;
			}
			else
			{
				m_OrgPlaybackSpeed.Remove(particleSystem);
				m_ParticleSystems.RemoveAt(i);
			}
		}
	}

	private void OnDisable()
	{
		if (m_RestoreSpeedOnDisable)
		{
			foreach (KeyValuePair<ParticleSystem, float> pair in m_OrgPlaybackSpeed)
			{
				ParticleSystem particleSystem = pair.Key;
				float speed = pair.Value;
				if ((bool)particleSystem)
				{
					ParticleSystem.MainModule particleMain = particleSystem.main;
					particleMain.simulationSpeed = speed;
				}
			}
		}
		m_PreviousPlaybackSpeed = -10000000f;
		m_ParticleSystems.Clear();
		m_OrgPlaybackSpeed.Clear();
	}

	private void OnEnable()
	{
		Init();
	}

	private void Init()
	{
		if (m_ParticleSystems == null)
		{
			m_ParticleSystems = new List<ParticleSystem>();
		}
		else
		{
			m_ParticleSystems.Clear();
		}
		if (m_OrgPlaybackSpeed == null)
		{
			m_OrgPlaybackSpeed = new Map<ParticleSystem, float>();
		}
		else
		{
			m_OrgPlaybackSpeed.Clear();
		}
		ParticleSystem[] componentsInChildren = base.gameObject.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			m_OrgPlaybackSpeed.Add(particleSystem, particleSystem.main.simulationSpeed);
			m_ParticleSystems.Add(particleSystem);
		}
	}
}
