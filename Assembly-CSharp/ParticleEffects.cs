using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ParticleEffects : MonoBehaviour
{
	public List<ParticleSystem> m_ParticleSystems;

	public bool m_WorldSpace;

	public ParticleEffectsOrientation m_ParticleOrientation;

	public List<ParticleEffectsAttractor> m_ParticleAttractors;

	public List<ParticleEffectsRepulser> m_ParticleRepulsers;

	private void Update()
	{
		if (m_ParticleSystems == null)
		{
			return;
		}
		if (m_ParticleSystems.Count == 0)
		{
			ParticleSystem particleSystem = GetComponent<ParticleSystem>();
			if (particleSystem == null)
			{
				base.enabled = false;
			}
			m_ParticleSystems.Add(particleSystem);
		}
		for (int i = 0; i < m_ParticleSystems.Count; i++)
		{
			ParticleSystem particleSystem2 = m_ParticleSystems[i];
			if (!(particleSystem2 == null))
			{
				int particleCount = particleSystem2.particleCount;
				if (particleCount == 0)
				{
					break;
				}
				ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleCount];
				particleSystem2.GetParticles(particles);
				if (m_ParticleAttractors != null)
				{
					ParticleAttractor(particleSystem2, particles, particleCount);
				}
				if (m_ParticleRepulsers != null)
				{
					ParticleRepulser(particleSystem2, particles, particleCount);
				}
				if (m_ParticleOrientation != null && m_ParticleOrientation.m_OrientToDirection)
				{
					OrientParticlesToDirection(particleSystem2, particles, particleCount);
				}
				particleSystem2.SetParticles(particles, particleCount);
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (m_ParticleAttractors != null)
		{
			foreach (ParticleEffectsAttractor attractor in m_ParticleAttractors)
			{
				if (!(attractor.m_Transform == null))
				{
					Gizmos.color = Color.green;
					float radius = attractor.m_Radius * ((attractor.m_Transform.lossyScale.x + attractor.m_Transform.lossyScale.y + attractor.m_Transform.lossyScale.z) * 0.333f);
					Gizmos.DrawWireSphere(attractor.m_Transform.position, radius);
				}
			}
		}
		if (m_ParticleRepulsers == null)
		{
			return;
		}
		foreach (ParticleEffectsRepulser repulser in m_ParticleRepulsers)
		{
			if (!(repulser.m_Transform == null))
			{
				Gizmos.color = Color.red;
				float radius2 = repulser.m_Radius * ((repulser.m_Transform.lossyScale.x + repulser.m_Transform.lossyScale.y + repulser.m_Transform.lossyScale.z) * 0.333f);
				Gizmos.DrawWireSphere(repulser.m_Transform.position, radius2);
			}
		}
	}

	private void OrientParticlesToDirection(ParticleSystem particleSystem, ParticleSystem.Particle[] particles, int particleCount)
	{
		for (int idx = 0; idx < particleCount; idx++)
		{
			particles[idx].angularVelocity = 0f;
			Vector3 velocity = particles[idx].velocity;
			if (!m_WorldSpace)
			{
				velocity = particleSystem.transform.TransformDirection(particles[idx].velocity);
			}
			if (m_ParticleOrientation.m_UpVector == ParticleEffectsOrientUpVectors.Horizontal)
			{
				particles[idx].rotation = VectorAngle(Vector3.forward, velocity, Vector3.up);
			}
			else if (m_ParticleOrientation.m_UpVector == ParticleEffectsOrientUpVectors.Vertical)
			{
				particles[idx].rotation = VectorAngle(Vector3.up, velocity, Vector3.forward);
			}
		}
	}

	private void ParticleAttractor(ParticleSystem particleSystem, ParticleSystem.Particle[] particles, int particleCount)
	{
		for (int idx = 0; idx < particleCount; idx++)
		{
			foreach (ParticleEffectsAttractor attractor in m_ParticleAttractors)
			{
				if (!(attractor.m_Transform == null) && !(attractor.m_Radius <= 0f) && !(attractor.m_Power <= 0f))
				{
					Vector3 pos = particles[idx].position;
					if (!m_WorldSpace)
					{
						pos = particleSystem.transform.TransformPoint(particles[idx].position);
					}
					Vector3 dir = attractor.m_Transform.position - pos;
					float radius = attractor.m_Radius * ((attractor.m_Transform.lossyScale.x + attractor.m_Transform.lossyScale.y + attractor.m_Transform.lossyScale.z) * 0.333f);
					float amount = (1f - dir.magnitude / radius) * attractor.m_Power;
					Vector3 newDir = dir * particles[idx].velocity.magnitude;
					if (!m_WorldSpace)
					{
						newDir = particleSystem.transform.InverseTransformDirection(dir * particles[idx].velocity.magnitude);
					}
					Vector3 newVelocity = Vector3.Lerp(particles[idx].velocity, newDir, amount * Time.deltaTime).normalized * particles[idx].velocity.magnitude;
					particles[idx].velocity = newVelocity;
				}
			}
		}
	}

	private void ParticleRepulser(ParticleSystem particleSystem, ParticleSystem.Particle[] particles, int particleCount)
	{
		for (int idx = 0; idx < particleCount; idx++)
		{
			foreach (ParticleEffectsRepulser repulser in m_ParticleRepulsers)
			{
				if (!(repulser.m_Transform == null) && !(repulser.m_Radius <= 0f) && !(repulser.m_Power <= 0f))
				{
					Vector3 pos = particles[idx].position;
					if (!m_WorldSpace)
					{
						pos = particleSystem.transform.TransformPoint(particles[idx].position);
					}
					Vector3 dir = repulser.m_Transform.position - pos;
					float radius = repulser.m_Radius * ((repulser.m_Transform.lossyScale.x + repulser.m_Transform.lossyScale.y + repulser.m_Transform.lossyScale.z) * 0.333f);
					float amount = (1f - dir.magnitude / radius) * repulser.m_Power + repulser.m_Power;
					Vector3 newDir = -dir * particles[idx].velocity.magnitude;
					if (!m_WorldSpace)
					{
						newDir = particleSystem.transform.InverseTransformDirection(-dir * particles[idx].velocity.magnitude);
					}
					Vector3 newVelocity = Vector3.Lerp(particles[idx].velocity, newDir, amount * Time.deltaTime).normalized * particles[idx].velocity.magnitude;
					particles[idx].velocity = newVelocity;
				}
			}
		}
	}

	private static float VectorAngle(Vector3 forwardVector, Vector3 targetVector, Vector3 upVector)
	{
		float angle = Vector3.Angle(forwardVector, targetVector);
		if (Vector3.Dot(Vector3.Cross(forwardVector, targetVector), upVector) < 0f)
		{
			return 360f - angle;
		}
		return angle;
	}
}
